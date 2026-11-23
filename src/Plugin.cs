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

namespace ProductionBottlenecks
{
    [BepInPlugin(Id, "Production Bottlenecks", "0.1.0")]
    public sealed class Plugin : BaseUnityPlugin
    {
        private const string Id = "sfk.productionbottlenecks";
        private static Plugin instance;
        private Harmony patches;
        private BuildingCity hovered;
        private string tooltipText;
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
            // The game normally writes up to three messages to the same tooltip here.
            // For town producers, LateUpdate writes one reason for the native highlighted building.
            private static bool Prefix(Building building) =>
                instance == null || !instance.isActiveAndEnabled || !ProductionStatus.Supports(building);
        }

        [HarmonyPatch(typeof(UIOverlayVariable), nameof(UIOverlayVariable.Generate),
            new Type[] { typeof(Building), typeof(List<UIBuildingOverlayElement>), typeof(string) })]
        private static class BuildingPanel
        {
            private static void Postfix(UIOverlayVariable __instance, Building building)
            {
                if (instance != null && ProductionStatus.Supports(building))
                    instance.AttachRow(__instance, (BuildingCity)building);
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
            string reason = selected != null ? ProductionStatus.Read(selected) : null;
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
            if (target != null && ProductionStatus.Supports(target))
                UpdateTooltip(ProductionStatus.Read(target));
        }

        private void UpdateTooltip(string reason)
        {
            var tip = TooltipManager.Instance;
            if (tip == null) return;
            if (reason == null)
            {
                ClearTooltip();
                return;
            }
            // Give other game tools (for example the worker count after assignment) priority.
            if (tip.IsOpen() && (TooltipLabel(tip).text != tooltipText || reason == tooltipText))
                return;

            tip.StopFollowMouse();
            tip.SetAnchor(TooltipManager.AnchorPresets.Top);
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
            tooltipText = reason;
            tip.Display(reason, position, fix: false);
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
