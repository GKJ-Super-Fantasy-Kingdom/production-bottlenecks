using System.Collections.Generic;
using HarmonyLib;
using SuperFantasyKingdom;
using SuperFantasyKingdom.Buildings;
using SuperFantasyKingdom.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProductionBottlenecks
{
    internal sealed class BlockedBuildingsList : MonoBehaviour
    {
        private const float RowHeight = 74f;
        private const float Gap = 6f;
        private const float BackHeight = 42f;
        // This is the same device-aware Back prompt the game displays in its calendar.
        private static readonly AccessTools.FieldRef<OverlayManager, GameObject> CancelPrompt =
            AccessTools.FieldRefAccess<OverlayManager, GameObject>("cancelButtonPrompt");
        private sealed class Entry
        {
            public BuildingCity Building;
            public Button Button;
            public TextMeshProUGUI Text;
            public Image Icon;
            public bool Present;
            public string Title;
            public string Reason;
        }

        private readonly Dictionary<BuildingCity, Entry> entries = new Dictionary<BuildingCity, Entry>();
        private readonly Dictionary<string, int> titleCounts = new Dictionary<string, int>();
        private readonly HashSet<BuildingCity> seen = new HashSet<BuildingCity>();
        private List<Entry> rows = new List<Entry>();
        private List<Entry> nextRows = new List<Entry>();
        private ScrollRect scroll;
        private RectTransform content, viewport;
        private LayoutElement windowSize;
        private Transform resourceBar;
        private RectTransform panel, frame, title;
        private float lowerEdge, upperEdge, appliedShift;
        private Camera uiCamera;
        private readonly List<Graphic> hudGraphics = new List<Graphic>();
        private readonly Vector3[] corners = new Vector3[4];
        private Button back;
        private TextMeshProUGUI visitHint;
        private GameObject empty;
        private TextMeshProUGUI promptStyle;
        private GameObject lastSelection;
        private GameObject focusMark;
        private float nextRefresh;

        private static Button AddButton(Transform parent, string title, float height, UnityAction action)
        {
            var prefab = Resources.Load<GameObject>("UI/OverlayElementButton");
            var style = prefab.GetComponentInChildren<Button>(true);
            var source = style.GetComponentInChildren<TextMeshProUGUI>(true);
            var obj = new GameObject(title, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            obj.transform.SetParent(parent, false);
            var image = obj.GetComponent<Image>();
            var face = (Image)style.targetGraphic;
            image.sprite = face.sprite;
            image.type = face.type;
            image.color = face.color;
            image.pixelsPerUnitMultiplier = face.pixelsPerUnitMultiplier;
            var button = obj.GetComponent<Button>();
            button.targetGraphic = image;
            button.colors = style.colors;
            button.transition = style.transition;
            button.spriteState = style.spriteState;

            var label = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            var caption = label.GetComponent<TextMeshProUGUI>();
            var rt = caption.rectTransform;
            rt.SetParent(obj.transform, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(8, 0);
            rt.offsetMax = new Vector2(-8, 0);
            caption.font = source.font;
            caption.fontSharedMaterial = source.fontSharedMaterial;
            caption.spriteAsset = source.spriteAsset;
            caption.color = source.color;
            caption.alignment = TextAlignmentOptions.Center;
            caption.fontSize = 28;
            caption.text = title;
            caption.raycastTarget = false;
            var layout = obj.GetComponent<LayoutElement>();
            layout.minHeight = layout.preferredHeight = height;
            layout.preferredWidth = 300;
            layout.flexibleWidth = 1;
            ((RectTransform)obj.transform).sizeDelta = new Vector2(300, height);
            button.onClick.AddListener(action);
            var navigation = button.navigation;
            navigation.mode = Navigation.Mode.Automatic;
            button.navigation = navigation;
            button.gameObject.SetActive(true);
            return button;
        }

        internal static GameObject AddLauncher(Transform panel, UnityAction open) =>
            AddButton(panel, "Blocked Buildings", 42, open).gameObject;

        internal static void Open(Transform panel)
        {
            var root = new GameObject("Blocked buildings list", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement), typeof(ContentSizeFitter));
            root.transform.SetParent(panel, false);
            var group = root.GetComponent<VerticalLayoutGroup>();
            group.childControlWidth = group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;
            group.spacing = 6;
            var layout = root.GetComponent<LayoutElement>();
            layout.minWidth = layout.preferredWidth = 440;
            var rect = (RectTransform)root.transform;
            rect.sizeDelta = new Vector2(440, 0);
            root.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            root.AddComponent<BlockedBuildingsList>().Build();
        }

        private void Build()
        {
            var window = new GameObject("Building rows", typeof(RectTransform), typeof(ScrollRect), typeof(LayoutElement));
            window.transform.SetParent(transform, false);
            windowSize = window.GetComponent<LayoutElement>();
            scroll = window.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.inertia = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = RowHeight + Gap;

            viewport = NewRect("Viewport", window.transform);
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = viewport.offsetMax = Vector2.zero;
            viewport.gameObject.AddComponent<Image>().color = Color.clear;
            viewport.gameObject.AddComponent<RectMask2D>();
            scroll.viewport = viewport;
            content = NewRect("Content", viewport);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = content.pivot = Vector2.one;
            content.sizeDelta = content.anchoredPosition = Vector2.zero;
            var group = content.gameObject.AddComponent<VerticalLayoutGroup>();
            group.spacing = Gap;
            group.childControlWidth = group.childControlHeight = group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;
            scroll.content = content;
            foreach (var template in OverlayManager.Instance.GetComponentsInChildren<Scrollbar>(true))
            {
                if (template.direction != Scrollbar.Direction.BottomToTop && template.direction != Scrollbar.Direction.TopToBottom) continue;
                var bar = Instantiate(template, window.transform);
                bar.name = "Scroll bar";
                bar.onValueChanged = new Scrollbar.ScrollEvent();
                bar.navigation = new Navigation { mode = Navigation.Mode.None };
                bar.direction = Scrollbar.Direction.BottomToTop;
                var rt = (RectTransform)bar.transform;
                rt.anchorMin = new Vector2(1, 0);
                rt.anchorMax = Vector2.one;
                rt.pivot = new Vector2(1, .5f);
                rt.sizeDelta = new Vector2(20, 0);
                rt.anchoredPosition = Vector2.zero;
                scroll.verticalScrollbar = bar;
                break;
            }

            empty = Instantiate(Resources.Load<GameObject>("UI/OverlayElementText"), transform);
            var notice = empty.GetComponentInChildren<TextMeshProUGUI>(true);
            notice.text = "No blocked buildings.";
            notice.raycastTarget = false;
            var footer = NewRect("Footer", transform);
            var footerLayout = footer.gameObject.AddComponent<HorizontalLayoutGroup>();
            footerLayout.childControlWidth = footerLayout.childControlHeight = true;
            footerLayout.childForceExpandWidth = false;
            footerLayout.spacing = Gap;
            back = AddButton(footer, "Back", BackHeight, () => OverlayManager.Instance.Cancel());
            var backSize = back.GetComponent<LayoutElement>();
            backSize.minWidth = backSize.preferredWidth = 96;
            backSize.flexibleWidth = 0;
            promptStyle = back.GetComponentInChildren<TextMeshProUGUI>(true);
            visitHint = Instantiate(promptStyle, footer);
            visitHint.name = "Visit hint";
            visitHint.transform.SetAsFirstSibling();
            visitHint.alignment = TextAlignmentOptions.MidlineLeft;
            visitHint.margin = new Vector4(8, 0, 0, 0);
            visitHint.fontSize = 24;
            visitHint.text = "Select a building to visit.";
            visitHint.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            resourceBar = FindObjectOfType<UIHeader>()?.transform.Find("PanelResources");
            panel = (RectTransform)transform.parent;
            while (panel != null && panel.name != "Panel") panel = panel.parent as RectTransform;
            frame = panel?.parent as RectTransform;
            title = frame?.Find("HeaderContainer/Header") as RectTransform;
            CancelPrompt(OverlayManager.Instance)?.SetActive(true);
            Refresh(true);
        }

        private static RectTransform NewRect(string name, Transform parent)
        {
            var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            return rect;
        }

        private Entry AddRow(BuildingCity building)
        {
            var slot = new Entry { Building = building };
            slot.Button = AddButton(content, "Building " + building.GetInstanceID(), RowHeight, () => Jump(building));
            slot.Text = slot.Button.GetComponentInChildren<TextMeshProUGUI>(true);
            slot.Text.alignment = TextAlignmentOptions.MidlineLeft;
            slot.Text.enableAutoSizing = true;
            slot.Text.fontSizeMin = 15;
            slot.Text.fontSizeMax = 30;
            slot.Text.rectTransform.anchorMin = Vector2.zero;
            slot.Text.rectTransform.anchorMax = Vector2.one;
            slot.Text.rectTransform.offsetMin = new Vector2(68, 3);
            slot.Text.rectTransform.offsetMax = new Vector2(-10, -3);
            var icon = new GameObject("Building icon", typeof(RectTransform), typeof(Image));
            slot.Icon = icon.GetComponent<Image>();
            var rt = slot.Icon.rectTransform;
            rt.SetParent(slot.Button.transform, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0, .5f);
            rt.anchoredPosition = new Vector2(35, 0);
            rt.sizeDelta = new Vector2(50, 54);
            slot.Icon.raycastTarget = false;
            slot.Icon.preserveAspect = true;
            var mark = NewRect("Focus mark", slot.Button.transform);
            mark.anchorMin = new Vector2(0, .15f);
            mark.anchorMax = new Vector2(0, .85f);
            mark.pivot = new Vector2(0, .5f);
            mark.sizeDelta = new Vector2(4, 0);
            mark.anchoredPosition = new Vector2(5, 0);
            var ink = mark.gameObject.AddComponent<Image>();
            ink.color = new Color32(255, 224, 170, 255);
            ink.raycastTarget = false;
            mark.gameObject.SetActive(false);
            return slot;
        }

        private void Update()
        {
            if (Time.unscaledTime >= nextRefresh) Refresh(false);
            var current = EventSystem.current?.currentSelectedGameObject;
            if (current != lastSelection)
                RevealSelection(current);
        }

        private void RevealSelection(GameObject current)
        {
            lastSelection = current;
            if (focusMark != null) focusMark.SetActive(false);
            focusMark = current != null && current.transform.parent == content
                ? current.transform.Find("Focus mark")?.gameObject : null;
            if (focusMark != null) focusMark.SetActive(true);
            if (!scroll.vertical || current == null || current.transform.parent != content) return;
            int index = current.transform.GetSiblingIndex();
            float top = index * (RowHeight + Gap);
            float offset = content.anchoredPosition.y;
            float visibleHeight = windowSize.preferredHeight;
            float desired = Mathf.Clamp(offset, top + RowHeight - visibleHeight, top);
            if (!Mathf.Approximately(desired, offset))
                SetOffset(desired);
        }

        private void SetOffset(float offset)
        {
            float range = content.rect.height - windowSize.preferredHeight;
            scroll.verticalNormalizedPosition = range > 0 ? 1f - Mathf.Clamp01(offset / range) : 1f;
        }

        private void Refresh(bool opening)
        {
            nextRefresh = Time.unscaledTime + .2f;
            var current = EventSystem.current?.currentSelectedGameObject;
            int oldIndex = current != null && current.transform.parent == content ? current.transform.GetSiblingIndex() : -1;
            foreach (var oldRow in rows) oldRow.Present = false;
            nextRows.Clear();
            seen.Clear();
            titleCounts.Clear();
            var city = CityManager.Instance;
            if (city != null)
            {
                Collect(city.GetBuildings());
                Collect(city.GetOutposts());
            }
            Button replacement = null;
            if (oldIndex >= 0 && oldIndex < rows.Count && !rows[oldIndex].Present)
            {
                // Follow the nearest surviving neighbor in the original order, even
                // when several warnings disappear together. Prefer the next on a tie.
                for (int d = 1; d < rows.Count && replacement == null; d++)
                {
                    if (oldIndex + d < rows.Count && rows[oldIndex + d].Present)
                        replacement = rows[oldIndex + d].Button;
                    else if (oldIndex >= d && rows[oldIndex - d].Present)
                        replacement = rows[oldIndex - d].Button;
                }
            }
            foreach (var row in nextRows)
            {
                var heading = row.Title;
                if (titleCounts[heading] > 1)
                {
                    var plot = row.Building.transform.position;
                    heading += $"  <color=#FFE0AA>({plot.x:0.##}, {plot.y:0.##})</color>";
                }
                var text = heading + "\n" + row.Reason;
                if (row.Text.text != text) row.Text.text = text;
            }
            // Detach retiring rows now; Unity destroys them after the frame.
            foreach (var row in rows)
            {
                if (row.Present) continue;
                entries.Remove(row.Building);
                row.Button.gameObject.SetActive(false);
                row.Button.transform.SetParent(null, false);
                Destroy(row.Button.gameObject);
            }
            (rows, nextRows) = (nextRows, rows);
            for (int i = 0; i < rows.Count; i++)
                Link(rows[i].Button, i > 0 ? rows[i - 1].Button : null, i + 1 < rows.Count ? rows[i + 1].Button : back);
            Link(back, rows.Count > 0 ? rows[rows.Count - 1].Button : null, null);

            float fullHeight = Mathf.Max(0, rows.Count * (RowHeight + Gap) - Gap);
            float visible = Mathf.Min(fullHeight, AvailableHeight());
            bool resized = !Mathf.Approximately(visible, windowSize.preferredHeight) ||
                !Mathf.Approximately(fullHeight, content.rect.height);
            var bar = scroll.verticalScrollbar;
            scroll.vertical = fullHeight > visible + .5f;
            if (bar != null)
            {
                bar.gameObject.SetActive(scroll.vertical);
                viewport.offsetMax = new Vector2(scroll.vertical ? -26 : 0, 0);
            }
            empty.SetActive(rows.Count == 0);
            visitHint.enabled = rows.Count > 0;
            scroll.gameObject.SetActive(rows.Count > 0);
            if (resized)
            {
                float offset = content.anchoredPosition.y;
                windowSize.minHeight = windowSize.preferredHeight = visible;
                content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, fullHeight);
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
                SetOffset(offset);
            }
            if (opening || (oldIndex >= 0 && (current == null || !current.activeInHierarchy)))
            {
                var focus = replacement ?? (rows.Count == 0 ? back : rows[Mathf.Clamp(oldIndex, 0, rows.Count - 1)].Button);
                GamepadManager.Instance.SetGameObjectToSelect(focus.gameObject, true);
                EventSystem.current?.SetSelectedGameObject(focus.gameObject);
                RevealSelection(focus.gameObject);
            }
            else if (oldIndex >= 0 && (current.transform.GetSiblingIndex() != oldIndex ||
                (resized && GamepadManager.Instance.IsGamepadInUse())))
                RevealSelection(current);
        }

        private float AvailableHeight()
        {
            var canvas = scroll.GetComponentInParent<Canvas>().rootCanvas;
            var cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            var origin = RectTransformUtility.WorldToScreenPoint(cam, transform.position);
            float scale = Mathf.Max(.01f, RectTransformUtility.WorldToScreenPoint(cam, transform.TransformPoint(Vector3.up)).y - origin.y);
            var safe = Screen.safeArea;
            float upper = safe.yMax;
            if (resourceBar != null)
            {
                resourceBar.GetComponentsInChildren(false, hudGraphics);
                float halfWidth = (((RectTransform)transform).rect.width / 2 + 50f) * scale;
                foreach (var graphic in hudGraphics)
                {
                    if (!graphic.isActiveAndEnabled || graphic.color.a <= 0) continue;
                    graphic.rectTransform.GetWorldCorners(corners);
                    var low = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
                    var high = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);
                    if (low.y > Screen.height * .5f && high.x > origin.x - halfWidth && low.x < origin.x + halfWidth)
                        upper = Mathf.Min(upper, low.y);
                }
            }
            uiCamera = cam;
            lowerEdge = safe.yMin + 14 * scale;
            upperEdge = upper - 14 * scale;
            // Reserve the title ribbon, panel borders and fixed Back row.
            return Mathf.Max(RowHeight, Mathf.Floor((upperEdge - lowerEdge) / scale - 90 - BackHeight - Gap));
        }

        private void LateUpdate()
        {
            if (title == null || !title.gameObject.activeInHierarchy) return;
            // Keep short lists at the native center. Long lists can use the extra
            // space below the HUD without moving the HUD or any other overlay.
            var parent = (RectTransform)frame.parent;
            var origin = RectTransformUtility.WorldToScreenPoint(uiCamera, parent.position);
            float scale = RectTransformUtility.WorldToScreenPoint(uiCamera, parent.TransformPoint(Vector3.up)).y - origin.y;
            if (scale <= 0) return;
            title.GetWorldCorners(corners);
            float top = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[2]).y - appliedShift * scale;
            panel.GetWorldCorners(corners);
            float bottom = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[0]).y - appliedShift * scale;
            float shift = Mathf.Clamp(0, lowerEdge - bottom, upperEdge - top) / scale;
            if (Mathf.Abs(shift - appliedShift) > .25f)
            {
                frame.anchoredPosition += Vector2.up * (shift - appliedShift);
                appliedShift = shift;
            }
        }

        private static void Link(Button button, Button up, Button down)
        {
            button.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnUp = up, selectOnDown = down };
        }

        private void Collect(List<BuildingCity> buildings)
        {
            foreach (var building in buildings)
            {
                if (building == null || !building.isActiveAndEnabled || building.ShouldRemove() || !seen.Add(building) ||
                    building.GetComponent<UIBuildingOverlay>() == null) continue;
                var name = building.GetTitle() ?? string.Empty;
                titleCounts.TryGetValue(name, out int count);
                titleCounts[name] = count + 1;
                var reason = ProductionStatus.Read(building, sprite => ResourceIcon.Tag(promptStyle, sprite));
                if (reason == null) continue;
                if (!entries.TryGetValue(building, out var row))
                    entries.Add(building, row = AddRow(building));
                if (row.Present) continue;
                row.Present = true;
                if (row.Button.transform.GetSiblingIndex() != nextRows.Count)
                    row.Button.transform.SetSiblingIndex(nextRows.Count);
                nextRows.Add(row);
                var sprite = building.icon ?? building.GetSpriteRenderer()?.sprite;
                row.Icon.sprite = sprite;
                row.Icon.enabled = sprite != null;
                row.Title = name;
                row.Reason = reason;
            }
        }

        private void Jump(BuildingCity building)
        {
            var city = CityManager.Instance;
            if (building == null || !building.isActiveAndEnabled || building.ShouldRemove() || city == null ||
                !(city.GetBuildings().Contains(building) || city.GetOutposts().Contains(building)) ||
                ProductionStatus.Read(building, null) == null)
            {
                Refresh(false);
                return;
            }
            var view = building.GetComponent<UIBuildingOverlay>();
            var camera = GameCamera.Instance;
            var overlays = OverlayManager.Instance;
            if (view == null || camera == null || !overlays.CloseOverlay() || overlays.IsOpen()) return;
            bool controller = GamepadManager.Instance.IsGamepadInUse() && EventSystem.current != null;
            var rootHit = view.GetComponent<Collider2D>();
            Vector3 point = rootHit != null ? rootHit.bounds.center : building.transform.position;
            Collider2D chosen = null;
            if (controller)
            {
                foreach (var hit in view.GetComponentsInChildren<Collider2D>())
                {
                    if (!hit.enabled || (ShaderParameters.LayerMaskUI & (1 << hit.gameObject.layer)) == 0 ||
                        ExecuteEvents.GetEventHandler<IPointerEnterHandler>(hit.gameObject) != view.gameObject) continue;
                    var bounds = hit.bounds;
                    var pivot = hit.transform.position;
                    var aim = VisitPoint.Choose((bounds.center.x, bounds.center.y), (pivot.x, pivot.y),
                        bounds.extents.x, bounds.extents.y, hit.isTrigger,
                        (x, y) => IsClearVisitPoint(hit, new Vector2(x, y)));
                    if (!aim.HasValue) continue;
                    point = new Vector3(aim.Value.X, aim.Value.Y, bounds.center.z);
                    chosen = hit;
                    break;
                }
            }
            camera.SetCurrentNavigationVelocity(Vector2.zero);
            camera.SetPosition(point);
            camera.SetTarget(camera.transform.position, 100f);
            if (controller)
            {
                // SetPosition clamps to the map bounds. Check the attainable point
                // before offering world actions on a possibly different building.
                if (chosen == null || !IsClearVisitPoint(chosen, camera.transform.position))
                {
                    if (view.IsInteractable()) view.Open(false);
                    return;
                }
                var pointer = new PointerEventData(EventSystem.current) { position = GamepadManager.Instance.GetCursorPosition() };
                var old = city.GetHighlightedBuilding();
                if (old != null && old != building) old.GetComponent<UIBuildingOverlay>()?.OnPointerExit(pointer);
                if (city.GetHighlightedBuilding() != building) view.OnPointerEnter(pointer);
            }
        }

        private static bool IsClearVisitPoint(Collider2D target, Vector2 point)
        {
            if (!target.OverlapPoint(point)) return false;
            var hits = Physics2D.OverlapCircleAll(point, VisitPoint.Clearance, ShaderParameters.LayerMaskUI);
            return hits.Length == 1 && hits[0] == target;
        }

        private void OnDisable()
        {
            if (frame != null) frame.anchoredPosition -= Vector2.up * appliedShift;
            appliedShift = 0;
            // Panel objects are pooled by the game; retire this list on any native close.
            Destroy(gameObject);
        }
    }
}
