using System;
using ProductionBottlenecks;
using SuperFantasyKingdom;
using SuperFantasyKingdom.Buildings;
using SuperFantasyKingdom.Weather;

int count = 0;
void Check(string name, bool passed)
{
    if (!passed) throw new Exception(name);
    count++;
}
void Expect(BuildingCity building, string expected, string scenario) =>
    Check(scenario, ProductionStatus.Read(building) == expected);

var bakery = new BuildingCrafting { Output = ResourceType.None, Possible = 0, Missing = ResourceType.Flour };
bakery.Recipes.Add(new CraftingRecipe { product = ResourceType.Bread });
int bread = (int)ResourceType.Bread;
bakery.Storage.Capacity[bread] = 3;
Check("producer in town", ProductionStatus.Supports(bakery));
Check("outside", !ProductionStatus.Supports(new BuildingCity { Outside = true }));
Check("non-producer", !ProductionStatus.Supports(new BuildingCity { Producer = false }));
Expect(bakery, "No worker assigned", "disabled worker slot");
bakery.Possible = 1;
Expect(bakery, "Needs Flour", "assignment exposes second blocker");
bakery.Storage.Stored[bread] = 3;
Expect(bakery, "Output storage full", "selected recipe determines output");
bakery.Working = 1;
Expect(bakery, null, "active worker suppresses stale warnings");
bakery.Working = 0;
bakery.Storage.Stored[bread] = 2;
bakery.Storage.Announced[bread] = 1;
Expect(bakery, null, "worker left but batch is still running");
bakery.Storage.Announced[bread] = 0;
Expect(bakery, "Needs Flour", "storage cleared");
bakery.Missing = ResourceType.None;
Expect(bakery, null, "production can resume");
bakery.Sign.gameObject.activeInHierarchy = true;
Expect(bakery, "Idle", "visible native sign with no known cause");
bakery.Sign.enabled = false;
Expect(bakery, null, "disabled native sign");
bakery.Weather = WeatherTypes.Storm;
Expect(bakery, "Waiting for better weather", "weather restriction");
bakery.Weather = WeatherTypes.None;
bakery.CurrentRecipe = -1;
Expect(bakery, null, "no recipe");
bakery.Size = BuildingSize.Small;
bakery.Specialist = true;
bakery.EnabledSlot = false;
Expect(bakery, "No worker assigned", "small building uses specialist toggle");
bakery.EnabledSlot = true;
bakery.Possible = 0;
Expect(bakery, null, "active specialist independent of ordinary slot count");
Expect(new BuildingGeneratingg { Possible = 0 }, null, "automatic production");
Expect(new BuildingDemand { Possible = 0 }, null, "empty production queue");
Console.WriteLine($"{count} production-state fixture checks passed.");
