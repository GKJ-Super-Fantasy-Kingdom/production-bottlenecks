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
    [BepInPlugin(Id, "Production Bottlenecks", "0.1.2")]
    public sealed class Plugin : BaseUnityPlugin
    {
        private const string Id = "sfk.productionbottlenecks";
        private static Plugin instance;
        private Harmony patches;
        private BuildingCity hovered;
        private string tooltipText;
        private Building nativeWarningOwner;
        private string nativeWarningText;
        private bool gamepad;
        private float nextRefresh;
        private BuildingCity selected;
        private GameObject row;
        private TextMeshProUGUI label;

        private static readonly AccessTools.FieldRef<TooltipManager, TextMeshProUGUI> TooltipLabel =
            AccessTools.FieldRefAccess<TooltipManager, TextMeshProUGUI>("text");

        private void Awake()
        {
            instance = this;
            patches = new Harmony(Id);
            try
            {
                patches.PatchAll(typeof(BuildingWarning));
                patches.PatchAll(typeof(BuildingPanel));
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
                LocalizedString ___stringStorageFull, LocalizedString ___stringBadWeather)
            {
                if (instance == null) return;
                instance.nativeWarningOwner = null;
                instance.nativeWarningText = null;
                var tip = TooltipManager.Instance;
                if (building == null || building.IsOutside() || tip == null || !tip.IsOpen()) return;

                string visible = TooltipLabel(tip).text;
                var missing = building.GetMissingResource();
                var data = missing != ResourceType.None && RaceManager.Instance != null
                    ? RaceManager.Instance.GetResourceData(missing) : null;
                bool matches = (data != null && visible == ___stringMissing.GetLocalizedString(data.GetName())) ||
                    (building.GetWeatherRestriction() != WeatherTypes.None && visible == ___stringBadWeather.GetLocalizedString()) ||
                    (building.IsProducer() && building.GetStorage() != null && visible == ___stringStorageFull.GetLocalizedString());
                if (matches)
                {
                    instance.nativeWarningOwner = building;
                    instance.nativeWarningText = visible;
                }
            }
        }

        [HarmonyPatch(typeof(UIOverlayVariable), nameof(UIOverlayVariable.Generate),
            new Type[] { typeof(Building), typeof(List<UIBuildingOverlayElement>), typeof(string) })]
        private static class BuildingPanel
        {
            private static void Postfix(UIOverlayVariable __instance, Building building)
            {
                if (instance != null && building is BuildingCity city)
                    instance.AttachRow(__instance, city);
            }
        }

        private void AttachRow(UIOverlayVariable panel, BuildingCity city)
        {
            // Generate has just called the game's Clean(), which retires the previous rows.
            var prefab = Resources.Load<GameObject>("UI/OverlayElementText");
            if (prefab == null) return;
            row = Instantiate(prefab, panel.transform);
            label = row.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label == null)
            {
                Destroy(row);
                return;
            }
            foreach (var graphic in row.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = false;
            selected = city;
            UpdateRow();
        }

        private void UpdateRow()
        {
            string reason = selected != null ? ProductionStatus.Read(selected, RowIcon) : null;
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
                hovered = null;
                return;
            }

            var ui = UIManager.Instance;
            var city = CityManager.Instance;
            var target = ui != null && city != null && OverlayManager.Instance != null &&
                PauseMenuManger.Instance != null && ui.IsWorldInteractable() ? city.GetHighlightedBuilding() : null;
            bool usingGamepad = GamepadManager.Instance != null && GamepadManager.Instance.IsGamepadInUse();
            bool changed = target != hovered || gamepad != usingGamepad;
            if (changed || (target == null && tooltipText != null))
            {
                ClearTooltip();
                hovered = target;
                gamepad = usingGamepad;
            }
            if (!changed && Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.2f;

            if (row != null && label != null && row.transform.parent.gameObject.activeInHierarchy)
                UpdateRow();
            if (target != null)
                UpdateTooltip(ProductionStatus.Read(target, TooltipIcon));
        }

        private string RowIcon(Sprite sprite) => ResourceIcon.Tag(label, sprite);

        private string TooltipIcon(Sprite sprite)
        {
            var tip = TooltipManager.Instance;
            return tip != null ? ResourceIcon.Tag(TooltipLabel(tip), sprite) : null;
        }

        private void UpdateTooltip(string reason)
        {
            var tip = TooltipManager.Instance;
            if (tip == null || (reason == null && tooltipText == null && nativeWarningOwner != hovered)) return;
            string visible = tip.IsOpen() ? TooltipLabel(tip).text : null;
            if (visible != tooltipText) tooltipText = null;

            string message = HoverTooltip.NextText(hovered.GetTitle(), reason, visible, tooltipText,
                nativeWarningOwner == hovered ? nativeWarningText : null);
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
        }

        private void ClearTooltip()
        {
            var tip = TooltipManager.Instance;
            if (tooltipText != null && tip != null && tip.IsOpen() && TooltipLabel(tip).text == tooltipText)
                tip.StopFollowMouse();
            tooltipText = null;
        }

        private void OnDestroy()
        {
            ClearTooltip();
            if (row != null) Destroy(row);
            patches?.UnpatchSelf();
            instance = null;
        }
    }
}
