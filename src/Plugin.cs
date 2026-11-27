using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using SuperFantasyKingdom;
using SuperFantasyKingdom.Buildings;
using SuperFantasyKingdom.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using SuperFantasyKingdom.Weather;

namespace ProductionBottlenecks
{
    [BepInPlugin(Id, "Production Bottlenecks", "0.2.9")]
    public sealed partial class Plugin : BaseUnityPlugin
    {
        private const string Id = "sfk.productionbottlenecks";
        private static Plugin instance;
        private Harmony patches;
        private BuildingCity hovered;
        private string tooltipText;
        private HoverVisibility hoverVisibility;
        private Building nativeWarningOwner;
        private string nativeWarningText;
        private bool emptyOutputWarning;
        private bool gamepad;
        private BuildingCity workerCountBuilding;
        private string workerCountText;
        private float nextRefresh;
        private BuildingCity selected;
        private GameObject row;
        private TextMeshProUGUI label;

        private static readonly AccessTools.FieldRef<TooltipManager, TextMeshProUGUI> TooltipLabel =
            AccessTools.FieldRefAccess<TooltipManager, TextMeshProUGUI>("text");
        private static readonly AccessTools.FieldRef<TooltipManager, RectTransform> TooltipFrame =
            AccessTools.FieldRefAccess<TooltipManager, RectTransform>("rectTransform");

        private void Awake()
        {
            instance = this;
            patches = new Harmony(Id);
            try
            {
                patches.PatchAll(typeof(BuildingWarning));
                patches.PatchAll(typeof(BuildingPanel));
                patches.PatchAll(typeof(WorldBuildingPanel));
                patches.PatchAll(typeof(WorkerCount));
                Logger.LogInfo("Production Bottlenecks ready.");
            }
            catch
            {
                patches.UnpatchSelf();
                instance = null;
                enabled = false;
                throw;
            }
        }

        [HarmonyPatch(typeof(UIBuildingWarnings), "UIBuildingOnMouseEnter")]
        private static class BuildingWarning
        {
            // Let the native handler and other patches run. Only its known, localized
            // warning text may be replaced when the same building remains highlighted.
            private static void Postfix(Building building, LocalizedString ___stringMissing,
                LocalizedString ___stringStorageFull, LocalizedString ___stringBadWeather, bool __runOriginal)
            {
                if (instance == null) return;
                instance.nativeWarningOwner = null;
                instance.nativeWarningText = null;
                instance.emptyOutputWarning = false;
                var tip = TooltipManager.Instance;
                if (!__runOriginal || building == null || building.IsOutside() || tip == null || !tip.IsOpen()) return;

                string visible = TooltipLabel(tip).text;
                var missing = building.GetMissingResource();
                var data = missing != ResourceType.None && RaceManager.Instance != null
                    ? RaceManager.Instance.GetResourceData(missing) : null;
                bool matches = (data != null && visible == ___stringMissing.GetLocalizedString(data.GetName())) ||
                    (building.GetWeatherRestriction() != WeatherTypes.None && visible == ___stringBadWeather.GetLocalizedString());
                if (!matches && building.IsProducer() && visible == ___stringStorageFull.GetLocalizedString())
                {
                    var storage = building.GetStorage();
                    var spaces = storage != null ? storage.GetStorageSpaces() : null;
                    var output = building.GetResource();
                    if (spaces != null && (uint)output < (uint)spaces.Length && storage.SpaceForResource(output, real: false) <= 0)
                    {
                        matches = true;
                        instance.emptyOutputWarning = output == ResourceType.None || storage.GetStorageSpace(output) == 0;
                    }
                }
                if (matches)
                {
                    instance.nativeWarningOwner = building;
                    instance.nativeWarningText = visible;
                }
            }
        }

        [HarmonyPatch(typeof(GameInputManager), "DisplayWorkerAmount")]
        private static class WorkerCount
        {
            [HarmonyPriority(Priority.Last)]
            private static void Prefix(BuildingCity __0, bool __runOriginal, out bool __state)
            {
                var tip = TooltipManager.Instance;
                __state = __runOriginal && instance != null && __0 != null && tip != null &&
                    GamepadManager.Instance != null && GamepadManager.Instance.IsGamepadInUse();
                if (!__state) return;

                // Navigation enables legacy mouse tracking. The native count writer
                // supplies no position, so start with a stationary, centered anchor.
                tip.StopFollowMouse();
                tip.SetAnchor(TooltipManager.AnchorPresets.Top);
            }

            private static void Postfix(BuildingCity __0, bool __state, bool __runOriginal)
            {
                if (!__state || !__runOriginal) return;
                var tip = TooltipManager.Instance;
                if (!tip.IsOpen()) return;
                instance.workerCountBuilding = __0;
                instance.workerCountText = TooltipLabel(tip).text;
                instance.PlaceWorkerCount(tip);
            }
        }

        private void PlaceWorkerCount(TooltipManager tip)
        {
            var focus = GamepadManager.Instance.GetCursorPosition();
            var camera = GameCamera.Instance != null ? GameCamera.Instance.GetCamera() : null;
            if (camera != null)
            {
                var point = camera.WorldToScreenPoint(workerCountBuilding.transform.position);
                if (point.z > 0) focus = point;
            }
            var label = TooltipLabel(tip);
            var scale = TooltipFrame(tip).lossyScale;
            // TooltipManager sizes its frame from the preferred text size plus 20 x 10.
            var size = ((label.preferredWidth + 20f) * scale.x, (label.preferredHeight + 10f) * scale.y);
            var safe = Screen.safeArea;
            var fitted = HoverTooltip.FitBelowFocus((focus.x, focus.y - 24f * SettingsManager.Instance.GetUIScaling()),
                size, (safe.xMin, safe.yMin, safe.xMax, safe.yMax), focus.y);
            tip.transform.position = new Vector3(fitted.X, fitted.Y, tip.transform.position.z);
        }

        private void RefreshWorkerCount(BuildingCity target, bool usingGamepad)
        {
            if (workerCountText == null) return;
            var tip = TooltipManager.Instance;
            if (tip != null && tip.IsOpen() && TooltipLabel(tip).text == workerCountText)
            {
                if (target != null && target == workerCountBuilding)
                {
                    if (usingGamepad)
                    {
                        PlaceWorkerCount(tip);
                        return;
                    }
                    tip.FollowMouse();
                    tip.SetAnchor(TooltipManager.AnchorPresets.TopLeft);
                    tip.Display(workerCountText, Vector3.zero);
                }
                else tip.StopFollowMouse();
            }
            workerCountText = null;
            workerCountBuilding = null;
        }

        private void AttachRow(UIOverlayVariable panel, BuildingCity city)
        {
            // Only one inspected city owns a status row at a time.
            ReleaseRow();
            var prefab = Resources.Load<GameObject>("UI/OverlayElementText");
            if (prefab == null) return;
            row = Instantiate(prefab, panel.transform);
            label = row.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label == null)
            {
                ReleaseRow();
                return;
            }
            foreach (var graphic in row.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = false;
            launcher = BlockedBuildingsList.AddLauncher(panel.transform, () => OpenBlockedList(city));
            rowPanel = panel;
            selected = city;
            UpdateRow();
        }

        private void UpdateRow()
        {
            string reason = selected != null ? ProductionStatus.ReadStyled(selected, RowIcon, name => ResourceIcon.Glyph(label, name), true) : null;
            if (reason != null && label.text != reason)
                label.text = reason;
            if (row.activeSelf != (reason != null))
                row.SetActive(reason != null);
        }

        private void LateUpdate()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsGameLoaded() || GameManager.Instance.IsUnloading())
            {
                ClearTooltip();
                RefreshWorkerCount(null, false);
                hovered = null;
                hoverVisibility = default;
                return;
            }

            var ui = UIManager.Instance;
            var city = CityManager.Instance;
            var target = ui != null && city != null && OverlayManager.Instance != null &&
                PauseMenuManger.Instance != null && ui.IsWorldInteractable() ? city.GetHighlightedBuilding() : null;
            bool usingGamepad = GamepadManager.Instance != null && GamepadManager.Instance.IsGamepadInUse();
            RefreshWorkerCount(target, usingGamepad);
            bool changed = target != hovered || gamepad != usingGamepad;
            if (changed || (target == null && tooltipText != null))
            {
                // Moving our own open card to the other input scheme is an internal close.
                bool closedOwned = ClearTooltip();
                if (target != hovered || closedOwned) hoverVisibility = default;
                hovered = target;
                gamepad = usingGamepad;
            }
            if (!gamepad && tooltipText != null)
                FitMouseTooltip();
            if (!changed && Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.2f;

            if (row != null && label != null && row.transform.parent.gameObject.activeInHierarchy)
                UpdateRow();
            if (target != null)
                UpdateTooltip(ProductionStatus.ReadStyled(target, TooltipIcon,
                    TooltipGlyph, true));
        }

        private string RowIcon(Sprite sprite) => ResourceIcon.Tag(label, sprite);

        private void FitMouseTooltip()
        {
            var tip = TooltipManager.Instance;
            if (tip == null || !tip.enabled || !tip.IsOpen() || TooltipLabel(tip).text != tooltipText) return;
            var frame = TooltipFrame(tip);
            if (!frame.gameObject.activeInHierarchy) return;

            // FollowMouse writes screen pixels every Update. Move its anchor before
            // other overlays lay out the rendered frame, keeping their offsets intact.
            var pos = tip.transform.position;
            var size = Vector2.Scale(frame.rect.size, frame.lossyScale);
            var safe = Screen.safeArea;
            var fitted = HoverTooltip.FitBelowFocus((pos.x, pos.y), (size.x, size.y),
                (safe.xMin, safe.yMin, safe.xMax, safe.yMax), GamepadManager.Instance.GetCursorPosition().y);
            if (fitted.X != pos.x || fitted.Y != pos.y)
                tip.transform.position = new Vector3(fitted.X, fitted.Y, pos.z);
        }

        private string TooltipGlyph(string name)
        {
            var tip = TooltipManager.Instance;
            return tip != null ? ResourceIcon.Glyph(TooltipLabel(tip), name) : null;
        }

        private string TooltipIcon(Sprite sprite)
        {
            var tip = TooltipManager.Instance;
            return tip != null ? ResourceIcon.Tag(TooltipLabel(tip), sprite) : null;
        }

        private void UpdateTooltip(string reason)
        {
            var tip = TooltipManager.Instance;
            if (tip == null) return;
            bool open = tip.IsOpen();
            bool dismissed = hoverVisibility.Observe(reason, open);
            if (reason == null && tooltipText == null && nativeWarningOwner != hovered) return;
            string visible = open ? TooltipLabel(tip).text : null;
            if (visible != tooltipText) tooltipText = null;
            if (dismissed) return;

            string message = HoverTooltip.NextText(hovered.GetTitle(), reason, visible, tooltipText,
                nativeWarningOwner == hovered ? nativeWarningText : null, emptyOutputWarning);
            if (message == null) return;

            tip.StopFollowMouse();
            tip.SetAnchor(reason == null ? TooltipManager.AnchorPresets.TopLeft : TooltipManager.AnchorPresets.Top);
            var position = Vector3.zero;
            if (gamepad)
            {
                position = GamepadManager.Instance.GetCursorPosition();
                position.y -= 24f * SettingsManager.Instance.GetUIScaling();
            }
            else
            {
                tip.FollowMouse();
            }
            tooltipText = reason == null ? null : message;
            tip.Display(message, position, fix: false);
            hoverVisibility.Observe(reason, tip.IsOpen());
        }

        private bool ClearTooltip()
        {
            var tip = TooltipManager.Instance;
            bool owned = tooltipText != null && tip != null && tip.IsOpen() && TooltipLabel(tip).text == tooltipText;
            if (owned) tip.StopFollowMouse();
            tooltipText = null;
            return owned;
        }

        private void OnDestroy()
        {
            ClearTooltip();
            ReleaseRow();
            patches?.UnpatchSelf();
            instance = null;
        }
    }
}
