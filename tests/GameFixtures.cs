// Small managed fixtures for state transitions. The plugin project references the installed game.
using UnityEngine;

namespace UnityEngine { public sealed class Sprite { public string name; } }

namespace SuperFantasyKingdom.Weather
{
    public enum WeatherTypes { None, Storm }
}

namespace SuperFantasyKingdom
{
    public enum ResourceType { None, Bread, Flour, Tree, Ore, Wheat }

    public sealed class ResourceData
    {
        public string Name;
        public Sprite icon;
        public string GetName() => Name;
    }
    public struct ResourceAmount
    {
        public ResourceType type;
        public int amount;
        public ResourceAmount(ResourceType type, int amount) { this.type = type; this.amount = amount; }
    }
    public sealed class RaceManager
    {
        public static RaceManager Instance = new RaceManager();
        public ResourceData GetResourceData(ResourceType type) => !System.Enum.IsDefined(type) ? null : new ResourceData
        {
            Name = type.ToString(), icon = type is ResourceType.Tree or ResourceType.Ore or ResourceType.Wheat ? new Sprite { name = type + "Sprite" } : null
        };
        public Sprite GetResourceIcon(ResourceType type) => GetResourceData(type)?.icon;
    }
    public sealed class ResourceStorage
    {
        public readonly int[] Capacity = new int[6], Stored = new int[6], Announced = new int[6];
        public int[] GetAllResourceQuantity() => Stored;
        public int? EffectiveCapacity;
        public int GetStorageSpace(ResourceType type) => EffectiveCapacity ?? Capacity[(int)type];
        public int[] GetStorageSpaces() => Capacity;
        public int GetAnnouncedResourceQuantity(ResourceType type) => Announced[(int)type];
        public int SpaceForResource(ResourceType type, bool real) =>
            GetStorageSpace(type) - Stored[(int)type] - (real ? Announced[(int)type] : 0);
    }
}

namespace SuperFantasyKingdom.Buildings
{
    using System.Collections.Generic;
    using SuperFantasyKingdom.Weather;

    public enum BuildingSize { Big, Small }
    public class Building
    {
        public bool Outside, Processor, Producer = true;
        public bool IsProcessor() => Processor;
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
        public Sign Sign = new Sign();
        public int GetCurrentlyWorking() => Working;
        public int GetCurrentlyInside() => Inside;
        public bool IsUpgradeWorking() => UpgradeWorking;
        public int ExtraJobs;
        public int GetPossibleJobs() => System.Math.Min(Possible, Maximum) + ExtraJobs;
        public int GetPossibleWorkers() => Possible;
        public int GetMaxWorkers() => Maximum;
        public bool CanUpgradeBeActivated() => Specialist;
        public BuildingSize GetBuildingSize() => Size;
        public bool IsUpgradeActive() => EnabledSlot;
        public ResourceStorage GetStorage() => Storage;
        public int GetResourceQuantity(ResourceType type) => Storage.Stored[(int)type];
        public List<CraftingRecipe> Recipes;
        public int CurrentRecipe;
        public List<CraftingRecipe> GetCraftingRecipes(bool addQueued = false) => Recipes;
        public int GetCurrentlyCrafting() => CurrentRecipe;
        public ResourceType GetResource() => Output;
        public ResourceType GetMissingResource() => Missing;
        public WeatherTypes GetWeatherRestriction() => Weather;
        public Sign GetOutOfWorkRenderer() => Sign;
    }
    public class BuildingGeneratingg : BuildingCity { }
    public class BuildingVeinGenerate : BuildingCity { }
    public class BuildingVeinGenerateMorning : BuildingCity { }
    public struct CraftingRecipe { public ResourceType product; public ResourceAmount[] ingredients; }
    public class BuildingCrafting : BuildingCity
    {
        public BuildingCrafting() { Recipes = new List<CraftingRecipe>(); }
        public bool Real = true;
        public bool IsProductReal(ResourceType type) => Real;
    }
    public class BuildingDemand : BuildingCrafting
    {
        public bool Crafting, Started;
        public bool IsStarted() => Started;
        public readonly List<int> Queue = new List<int>();
        public bool IsCrafting() => Crafting;
        public List<int> GetQueue() => Queue;
    }
}
