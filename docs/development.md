# Development and Playtesting

## Local import

1. In Thunderstore Mod Manager, choose **Super Fantasy Kingdom** and open your test mod profile. Install the two BepInEx dependencies listed in the manifest if the profile needs them.
2. Open **Settings**, find **Import local mod**, and select `ProductionBottlenecks-0.2.3.zip` intact.
3. Choose **Start modded**. In the profile's `BepInEx/LogOutput.log`, look for `Production Bottlenecks ready.` This confirms the plugin registered its game hooks.

## Controller playtest

The [Goldmine](screenshots/0.1.2/goldmine-ore.png) and [Lumberyard](screenshots/0.1.2/lumberyard-tree.png) also show the game's Ore and Tree icons when a deposit is missing.

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

The checks exercise production states and tooltip placement against managed fixtures. The package script verifies the archived files against the source listing and Release binary, then writes the ZIP under `dist/`. See the [compatibility checks](compatibility.md) for live results at both UI scales and the remaining physical controller checks.

## Contributing and license

Bug reports and pull requests are welcome. See the [contribution guide](../CONTRIBUTING.md) for local testing and reporting instructions.

The original mod code is available under the [MIT License](../LICENSE). Super Fantasy Kingdom, its game assets, and third-party dependencies retain their respective licenses and ownership.
