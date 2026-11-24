# Production Bottlenecks

Hover over a production building, or move the controller's world cursor onto it. If production is blocked, one line explains why. Select the building to see the same line in its panel. The game's existing worker-details action and device prompts still work while you inspect the building.

The tooltip follows the building's live worker slots, out-of-work sign, selected recipe or queued task, reported resource shortage, weather and local output storage. A Lumberyard can show a blockage while another worker is on a job. Producers, processors and other buildings with a native work warning use the same checks. When the sign has no specific cause, the line reads **Idle** or **Work blocked** if another worker is active.

**Needs 3 Flour** means the selected recipe still needs three units at this building. Goods in transit count once delivered. Gatherers such as the Goldmine usually report only the deposit type, so the tooltip shows its icon and localized name without a quantity. A missing text glyph can be drawn from the resource's game sprite directly. A readable name remains when the sprite is unavailable.

Mods that change the game's selected recipe, ingredient amounts, stock, resource data or warning state are picked up on refresh. A rule that stays private to another mod can only be described as specifically as the information it shares with the game. Custom tooltips keep priority over this mod's hover text.

See the [compatibility audit](docs/compatibility.md) for the tested package versions, bakery shortage results and outstanding in-game checks.

## In-game examples

With BakeryUsesWheat 1.2.0 enabled, delivering one of the two required Wheat leaves **Needs 1 Wheat** beneath the Bakery name. Captured in 0.1.3.

![Bakery tooltip showing Needs 1 Wheat after one delivery](docs/screenshots/0.1.3/bakery-one-wheat.png)

Before that delivery, the selected Bakery shows **Needs 2 Wheat** below the worker control. Its storage and recipe are visible in the same panel. Captured at UI scale 1.5 in 0.1.3.

![Bakery panel with no Wheat in storage and a two-Wheat shortage](docs/screenshots/0.1.3/bakery-panel-1p5x.png)

After a worker assignment, the hammer and **8/7** count appear beside the focused Lumberyard. Captured at UI scale 1.5 with simulated controller input in 0.1.4.

![Native hammer and worker count beneath the focused Lumberyard](docs/screenshots/0.1.4/after-1p5x.png)

The [Goldmine](docs/screenshots/0.1.2/goldmine-ore.png) and [Lumberyard](docs/screenshots/0.1.2/lumberyard-tree.png) also show the game's Ore and Tree icons when a deposit is missing.

## Local import

1. In Thunderstore Mod Manager, choose **Super Fantasy Kingdom** and open your test mod profile. Install the two BepInEx dependencies listed in the manifest if the profile needs them.
2. Open **Settings**, find **Import local mod**, and select `ProductionBottlenecks-0.1.5.zip` intact.
3. Choose **Start modded**. In the profile's `BepInEx/LogOutput.log`, look for `Production Bottlenecks ready.` This confirms the plugin registered its game hooks.

## Controller playtest

Use a test kingdom with a staffed producer during working hours. Repeat at **UI scale 1** and **1.5**.

Park the mouse away from the building. With the controller world cursor over it, add and remove a worker using the game's assignment controls. The hammer count should follow the focused building. Switch back to mouse and keyboard and check the native worker count and shortage tooltip.

1. Set the building's worker assignment to zero. Once its last active job ends, point the world cursor at it. Expect **No worker assigned** before pressing a button. Hold and release the game's worker-details action. The status should stay with the focused building.
2. Select the building using the native confirm action. Expect the same status in its panel. Restore a worker slot using the panel controls. The message should clear or change to the next blocker. Check that the worker buttons remain reachable and the status fits alongside their prompts.
3. At a crafter with an unavailable input, wait for the game to flag the shortage. Expect **Needs** with the resource icon and name. For a recipe costing four Flour with one Flour delivered, expect **3**. Restore supply and watch it clear when work resumes. Fill the selected recipe's local output store, let the last batch finish, and check for **Output storage full**. Make room and recheck.
4. Focus a Goldmine that reports missing Ore, then a Lumberyard with a visible work sign and workers still assigned. Check the Ore sprite and the Lumberyard’s current reason. If a recipe-changing mod is enabled, switch recipes and check the new icon and remaining amount.
5. Close the panel, move focus to a different building, then point at empty ground. Check focus return and that the previous status disappears. Switch to mouse and keyboard, hover, use the existing worker-details binding, and reopen the panel. Verify the glyphs follow the active device and that the text fits at both scales.

## Build

Use a .NET SDK with .NET 8 support, Python 3, the installed Windows game, and a BepInEx 5 mod profile. The project defaults to the D: Steam library and reads BepInEx from the Daybook Dev profile. To use another installation, pass `-p:GameDir="..."` and `-p:ProfileDir="..."` to the build.

```powershell
dotnet build ProductionBottlenecks.csproj -c Release
dotnet run --project tests/StatusChecks.csproj -c Release
python package.py
```

The checks exercise production states and tooltip placement against managed fixtures. The package script verifies the archived files against the source listing and Release binary, then writes the ZIP under `dist/`. See the [compatibility checks](docs/compatibility.md) for live results at both UI scales and the remaining physical controller checks.

## Contributing and license

Bug reports and pull requests are welcome. See the [contribution guide](CONTRIBUTING.md) for local testing and reporting instructions.

The original mod code is available under the [MIT License](LICENSE). Super Fantasy Kingdom, its game assets, and third-party dependencies retain their respective licenses and ownership.
