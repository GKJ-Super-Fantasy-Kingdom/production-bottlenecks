using SuperFantasyKingdom;
using SuperFantasyKingdom.Buildings;
using SuperFantasyKingdom.Weather;

namespace ProductionBottlenecks
{
    internal static class ProductionStatus
    {
        internal static bool Supports(Building building) =>
            building is BuildingCity && !building.IsOutside() && building.IsProducer();

        internal static string Read(BuildingCity city)
        {
            if (city.GetCurrentlyWorking() > 0 || city.IsUpgradeWorking() || city.GetCurrentlyInside() > 0)
                return null;

            if (city is BuildingDemand demand && (demand.IsCrafting() || demand.GetQueue().Count == 0))
                return null;

            var storage = city.GetStorage();
            var output = city.GetResource();
            if (city is BuildingCrafting craft)
            {
                var recipes = craft.GetCraftingRecipes();
                int index = craft.GetCurrentlyCrafting();
                output = recipes != null && index >= 0 && index < recipes.Count
                    ? recipes[index].product : ResourceType.None;
                if (!craft.IsProductReal(output))
                    output = ResourceType.None;
            }
            var spaces = storage != null ? storage.GetStorageSpaces() : null;
            bool storesOutput = output != ResourceType.None && spaces != null &&
                (uint)output < (uint)spaces.Length && spaces[(int)output] > 0;

            // A crafter can finish a batch after the worker leaves.
            if (storesOutput && city is BuildingCrafting && storage.GetAnnouncedResourceQuantity(output) > 0)
                return null;

            // Timed producers can make goods without any worker slots enabled.
            bool automatic = city is BuildingGeneratingg || city is BuildingVeinGenerate || city is BuildingVeinGenerateMorning;
            if (!automatic)
            {
                bool specialistSlot = city.GetBuildingSize() == BuildingSize.Small && city.CanUpgradeBeActivated();
                if (specialistSlot ? !city.IsUpgradeActive() : city.GetMaxWorkers() > 0 && city.GetPossibleWorkers() == 0)
                    return "No worker assigned";
            }

            if (storesOutput && storage.SpaceForResource(output, real: false) <= 0)
                return "Output storage full";

            var missing = city.GetMissingResource();
            if (missing != ResourceType.None)
            {
                var resource = RaceManager.Instance != null ? RaceManager.Instance.GetResourceData(missing) : null;
                return resource != null ? "Needs " + resource.GetName() : "Missing resource";
            }
            if (city.GetWeatherRestriction() != WeatherTypes.None)
                return "Waiting for better weather";

            // Zero workers on site can also mean the next worker is in transit.
            var sign = city.GetOutOfWorkRenderer();
            return !automatic && sign != null && sign.enabled && sign.gameObject.activeInHierarchy ? "Idle" : null;
        }
    }
}
