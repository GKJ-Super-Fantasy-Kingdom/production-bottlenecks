using System;
using UnityEngine;
using SuperFantasyKingdom;
using SuperFantasyKingdom.Buildings;
using SuperFantasyKingdom.Weather;

namespace ProductionBottlenecks
{
    internal static class ProductionStatus
    {
        private static bool HasWorkSign(BuildingCity city)
        {
            var sign = city.GetOutOfWorkRenderer();
            return sign != null && sign.enabled && sign.gameObject.activeInHierarchy;
        }

        internal static string Read(BuildingCity city, Func<Sprite, string> iconTag)
        {
            bool signVisible = HasWorkSign(city);
            bool busy = city.GetCurrentlyWorking() > 0 || city.IsUpgradeWorking() || city.GetCurrentlyInside() > 0;
            if (busy && !signVisible) return null;

            var recipes = city.GetCraftingRecipes();
            var missing = city.GetMissingResource();
            var weather = city.GetWeatherRestriction();
            bool production = city.IsProducer() || city.IsProcessor() || recipes?.Count > 0;
            if (!production && !signVisible && missing == ResourceType.None && weather == WeatherTypes.None)
                return null;

            int index = city.GetCurrentlyCrafting();
            if (city is BuildingDemand demand)
            {
                var queue = demand.GetQueue();
                if (queue == null || queue.Count == 0)
                    return signVisible && !demand.IsCrafting() ? "Queue a task" : null;
                if (demand.IsCrafting() && !signVisible) return null;
                index = queue[0];
            }
            CraftingRecipe? recipe = recipes != null && (uint)index < (uint)recipes.Count ? recipes[index] : (CraftingRecipe?)null;
            var output = recipes != null || city is BuildingCrafting ? recipe?.product ?? ResourceType.None : city.GetResource();
            if (output != ResourceType.None && city is BuildingCrafting craft && !craft.IsProductReal(output))
                output = ResourceType.None;

            var storage = city.GetStorage();
            var spaces = storage != null ? storage.GetStorageSpaces() : null;
            bool storesOutput = output != ResourceType.None && spaces != null &&
                (uint)output < (uint)spaces.Length && spaces[(int)output] > 0;

            if (!signVisible && storesOutput && city is BuildingCrafting && storage.GetAnnouncedResourceQuantity(output) > 0)
                return null;

            bool automatic = city.IsOutside() || city is BuildingGeneratingg || city is BuildingVeinGenerate || city is BuildingVeinGenerateMorning;
            if (!automatic && (production || signVisible))
            {
                bool specialist = city.GetBuildingSize() == BuildingSize.Small && city.CanUpgradeBeActivated();
                if (specialist ? !city.IsUpgradeActive() : city.GetMaxWorkers() > 0 && city.GetPossibleWorkers() == 0)
                    return "No worker assigned";
            }

            if (storesOutput && storage.SpaceForResource(output, real: false) <= 0)
                return "Output storage full";

            if (missing != ResourceType.None)
                return FormatShortage(city, missing, city is BuildingDemand active && active.IsCrafting() ? null : recipe, iconTag);

            if (weather != WeatherTypes.None)
                return "Waiting for better weather";

            if (!signVisible) return null;
            return busy ? "Work blocked" : "Idle";
        }

        private static string FormatShortage(BuildingCity city, ResourceType missing, CraftingRecipe? recipe, Func<Sprite, string> iconTag)
        {
            var race = RaceManager.Instance;
            var data = race != null ? race.GetResourceData(missing) : null;
            var sprite = race != null ? race.GetResourceIcon(missing) : null;
            var name = data != null ? data.GetName() : null;
            string icon = sprite != null ? iconTag?.Invoke(sprite) : null;
            if (string.IsNullOrEmpty(name) && icon == null) return "Missing resource";
            string resource = icon != null ? (icon + " " + name).TrimEnd() : name;
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
                    if (required > 0 || input.amount <= 0) return "Needs " + resource;
                    required = input.amount;
                }
                int held = city.GetResourceQuantity(missing);
                if (held >= 0 && required > held)
                    return "Needs " + (required - held) + " " + resource;
            }
            return "Needs " + resource;
        }
    }
}
