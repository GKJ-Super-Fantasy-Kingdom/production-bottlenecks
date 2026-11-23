// Small managed fixtures for state transitions. The plugin project references the installed game.
namespace SuperFantasyKingdom.Weather
{
    public enum WeatherTypes { None, Storm }
}

namespace SuperFantasyKingdom
{
    public enum ResourceType { None, Bread, Flour }
    public sealed class ResourceData { public string GetName() => "Flour"; }
    public sealed class RaceManager
    {
        public static RaceManager Instance = new RaceManager();
        public ResourceData GetResourceData(ResourceType type) => new ResourceData();
    }
    public sealed class ResourceStorage
    {
        public readonly int[] Capacity = new int[3], Stored = new int[3], Announced = new int[3];
        public int[] GetStorageSpaces() => Capacity;
        public int GetAnnouncedResourceQuantity(ResourceType type) => Announced[(int)type];
        public int SpaceForResource(ResourceType type, bool real) =>
            Capacity[(int)type] - Stored[(int)type] - (real ? Announced[(int)type] : 0);
    }
}

namespace SuperFantasyKingdom.Buildings
{
    using System.Collections.Generic;
    using SuperFantasyKingdom.Weather;

    public enum BuildingSize { Big, Small }
    public class Building
    {
        public bool Outside, Producer = true;
        public bool IsOutside() => Outside;
        public bool IsProducer() => Producer;
    }
    public class Sign
    {
        public bool enabled = true;
        public readonly SignObject gameObject = new SignObject();
    }
    public class SignObject { public bool activeInHierarchy; }
    public class BuildingCity : Building
    {
        public int Working, Possible = 1, Maximum = 2, Inside;
        public bool Specialist, EnabledSlot = true, UpgradeWorking;
        public BuildingSize Size;
        public ResourceType Missing, Output = ResourceType.Bread;
        public WeatherTypes Weather;
        public readonly ResourceStorage Storage = new ResourceStorage();
        public readonly Sign Sign = new Sign();
        public int GetCurrentlyWorking() => Working;
        public int GetCurrentlyInside() => Inside;
        public bool IsUpgradeWorking() => UpgradeWorking;
        public int GetPossibleWorkers() => Possible;
        public int GetMaxWorkers() => Maximum;
        public bool CanUpgradeBeActivated() => Specialist;
        public BuildingSize GetBuildingSize() => Size;
        public bool IsUpgradeActive() => EnabledSlot;
        public ResourceStorage GetStorage() => Storage;
        public ResourceType GetResource() => Output;
        public ResourceType GetMissingResource() => Missing;
        public WeatherTypes GetWeatherRestriction() => Weather;
        public Sign GetOutOfWorkRenderer() => Sign;
    }
    public class BuildingGeneratingg : BuildingCity { }
    public class BuildingVeinGenerate : BuildingCity { }
    public class BuildingVeinGenerateMorning : BuildingCity { }
    public struct CraftingRecipe { public ResourceType product; }
    public class BuildingCrafting : BuildingCity
    {
        public readonly List<CraftingRecipe> Recipes = new List<CraftingRecipe>();
        public int CurrentRecipe;
        public bool Real = true;
        public List<CraftingRecipe> GetCraftingRecipes() => Recipes;
        public int GetCurrentlyCrafting() => CurrentRecipe;
        public bool IsProductReal(ResourceType type) => Real;
    }
    public class BuildingDemand : BuildingCrafting
    {
        public bool Crafting;
        public readonly List<int> Queue = new List<int>();
        public bool IsCrafting() => Crafting;
        public List<int> GetQueue() => Queue;
    }
}
