using System;
using System.Collections.Generic;
using HarmonyLib;
using SuperFantasyKingdom.Buildings;
using SuperFantasyKingdom.Clickable;
using SuperFantasyKingdom.UI;

namespace ProductionBottlenecks
{
    public sealed partial class Plugin
    {
        private UIOverlayVariable rowPanel;

        [HarmonyPatch(typeof(UIOverlayVariable), nameof(UIOverlayVariable.Generate),
            new Type[] { typeof(Building), typeof(List<UIBuildingOverlayElement>), typeof(string) })]
        private static class BuildingPanel
        {
            private static void Postfix(UIOverlayVariable __instance, Building building, bool __runOriginal)
            {
                if (instance == null) return;
                if (__runOriginal && building is BuildingCity city)
                    instance.AttachRow(__instance, city);
                else
                    instance.ReleasePanelRow(__instance);
            }
        }

        [HarmonyPatch(typeof(UIOverlayVariable), nameof(UIOverlayVariable.Generate),
            new Type[] { typeof(BuildingWorld), typeof(List<UIBuildingOverlayElement>), typeof(string) })]
        private static class WorldBuildingPanel
        {
            private static void Postfix(UIOverlayVariable __instance)
            {
                // A replacement can skip Generate and the game's Clean() entirely.
                if (instance != null) instance.ReleasePanelRow(__instance);
            }
        }

        private void ReleasePanelRow(UIOverlayVariable panel)
        {
            if (rowPanel == panel) ReleaseRow();
        }

        private void ReleaseRow()
        {
            if (row != null)
            {
                row.SetActive(false);
                Destroy(row);
            }
            rowPanel = null;
            row = null;
            label = null;
            selected = null;
        }
    }
}
