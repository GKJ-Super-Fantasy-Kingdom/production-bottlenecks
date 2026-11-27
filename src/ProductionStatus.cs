using System;
using UnityEngine;
using SuperFantasyKingdom;
using SuperFantasyKingdom.Buildings;
using SuperFantasyKingdom.Weather;

namespace ProductionBottlenecks
{
    internal static class ProductionStatus
    {
        internal static string Read(BuildingCity city, Func<Sprite, string> iconTag)
            => ReadStyled(city, iconTag, null, false);

        internal static string ReadStyled(BuildingCity city, Func<Sprite, string> iconTag, Func<string, string> glyphTag,
            bool leadingIcon)
        {
            // The native warning handler displays weather last, even during other jobs.
            if (city.GetWeatherRestriction() != WeatherTypes.None)
                return "Waiting for better weather";

            var sign = city.GetOutOfWorkRenderer();
            bool signVisible = sign != null && sign.enabled && sign.gameObject.activeInHierarchy;
            bool busy = city.GetCurrentlyWorking() > 0 || city.IsUpgradeWorking() || city.GetCurrentlyInside() > 0;
            var missing = city.GetMissingResource();
            if (busy && !signVisible && (sign != null || missing == ResourceType.None)) return null;

            var recipes = city.GetCraftingRecipes();
            bool production = city.IsProducer() || city.IsProcessor() || recipes?.Count > 0;
            if (!production && !signVisible && missing == ResourceType.None)
                return null;

            int index = city.GetCurrentlyCrafting();
            if (city is BuildingDemand demand)
            {
                if (demand.IsCrafting() && !signVisible) return null;
                // Once started, the task panel and job use GetCurrentlyCrafting().
                if (!demand.IsStarted() && !demand.IsCrafting())
                {
                    var queue = demand.GetQueue();
                    if (queue == null || queue.Count == 0) return signVisible ? "Queue a task" : null;
                    index = queue[0];
                }
            }
            CraftingRecipe? recipe = recipes != null && (uint)index < (uint)recipes.Count ? recipes[index] : (CraftingRecipe?)null;
            var output = recipes?.Count > 0 || city is BuildingCrafting ? recipe?.product ?? ResourceType.None : city.GetResource();
            if (output != ResourceType.None && city is BuildingCrafting craft && !craft.IsProductReal(output))
                output = ResourceType.None;

            var storage = city.GetStorage();
            var spaces = storage != null ? storage.GetStorageSpaces() : null;
            bool storesOutput = output != ResourceType.None && spaces != null &&
                (uint)output < (uint)spaces.Length && storage.GetStorageSpace(output) > 0;

            if (!signVisible && storesOutput && city is BuildingCrafting && storage.GetAnnouncedResourceQuantity(output) > 0)
                return null;

            bool automatic = city.IsOutside() || city is BuildingGeneratingg || city is BuildingVeinGenerate || city is BuildingVeinGenerateMorning;
            if (!automatic && (production || signVisible))
            {
                bool specialist = city.GetBuildingSize() == BuildingSize.Small && city.CanUpgradeBeActivated();
                if (specialist ? !city.IsUpgradeActive() : city.GetMaxWorkers() > 0 && city.GetPossibleWorkers() == 0 && city.GetPossibleJobs() == 0)
                    return WithIcon(glyphTag?.Invoke("IconWorking"), "No worker assigned");
            }

            if (storesOutput && storage.SpaceForResource(output, real: false) <= 0)
                return WithIcon(Icon(RaceManager.Instance?.GetResourceIcon(output), glyphTag != null ? iconTag : null), "Output storage full");

            if (missing != ResourceType.None)
                return FormatShortage(city, missing, city is BuildingDemand active && active.IsCrafting() ? null : recipe, iconTag, leadingIcon);

            if (!signVisible) return null;
            return busy ? "Work blocked" : "Idle";
        }

        private static string Icon(Sprite sprite, Func<Sprite, string> tag) => sprite != null ? tag?.Invoke(sprite) : null;

        private static string WithIcon(string icon, string text) => icon == null ? text : icon + " " + text;

        private static string FormatShortage(BuildingCity city, ResourceType missing, CraftingRecipe? recipe, Func<Sprite, string> iconTag, bool leadingIcon)
        {
            var race = RaceManager.Instance;
            var data = race != null ? race.GetResourceData(missing) : null;
            var sprite = race != null ? race.GetResourceIcon(missing) : null;
            var name = data != null ? data.GetName() : null;
            string icon = Icon(sprite, iconTag);
            if (string.IsNullOrEmpty(name) && icon == null) return "Missing resource";
            string resource = (leadingIcon ? name : WithIcon(icon, name))?.TrimEnd();
            string Message(string text) => leadingIcon ? WithIcon(icon, text.TrimEnd()) : text;
            var ingredients = recipe?.ingredients;
            var storage = city.GetStorage();
            var stock = storage != null ? storage.GetAllResourceQuantity() : null;
            if (ingredients != null && stock != null && (uint)missing < (uint)stock.Length)
            {
                int required = 0;
                foreach (var input in ingredients)
                {
                    if (input.type != missing) continue;
                    // A custom recipe can encode alternatives or repeated inputs.
                    if (required > 0 || input.amount <= 0) return Message("Needs " + resource);
                    required = input.amount;
                }
                // Selection and the job's last shortage can change on different ticks.
                if (required == 0 && ingredients.Length > 0) return Message("Reported shortage of " + resource);
                int held = city.GetResourceQuantity(missing);
                if (held >= 0 && required > held)
                    return Message("Needs " + (required - held) + " " + resource);
                if (required > 0 && held >= required) return Message("Reported shortage of " + resource);
            }
            return Message("Needs " + resource);
        }
    }
}
