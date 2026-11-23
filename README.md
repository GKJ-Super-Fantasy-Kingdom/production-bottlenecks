# Production Bottlenecks

Hover over a town production building, or move the controller's world cursor onto it. If production is blocked, one line explains why. Select the building to see the same line in its panel. The game's existing worker-details action and device prompts still work while you inspect the building.

The first release covers disabled worker slots, missing resources reported by the game, full local output storage, and weather restrictions. **Idle** means the building has raised its out-of-work sign without a more specific cause. Buildings with active jobs and crafters finishing an announced batch normally have no status line. The short messages are in English and resource names come from the game.

## Local import

1. In Thunderstore Mod Manager, choose **Super Fantasy Kingdom** and open your test mod profile. Install the two BepInEx dependencies listed in the manifest if the profile needs them.
2. Open **Settings**, find **Import local mod**, and select `ProductionBottlenecks-0.1.0.zip` intact.
3. Choose **Start modded**. In the profile's `BepInEx/LogOutput.log`, look for `Production Bottlenecks ready.` This confirms the plugin registered its game hooks.

## Controller playtest

Use a test kingdom with a staffed producer during working hours. Repeat at **UI scale 1** and **1.5**.

1. Set the building's worker assignment to zero. Once its last active job ends, point the world cursor at it. Expect **No worker assigned** before pressing a button. Hold and release the game's worker-details action. The status should stay with the focused building.
2. Select the building using the native confirm action. Expect the same status in its panel. Restore a worker slot using the panel controls. The message should clear or change to the next blocker. Check that the worker buttons remain reachable and the status fits alongside their prompts.
3. At a crafter with an unavailable input, wait for the game to flag the shortage. Expect **Needs** followed by the resource name. Restore supply and watch it clear when work resumes. Fill the selected recipe's local output store, let the last batch finish, and check for **Output storage full**. Make room and recheck.
4. Close the panel, move focus to a different building, then point at empty ground. Check focus return and that the previous status disappears. Switch to mouse and keyboard, hover, use the existing worker-details binding, and reopen the panel. Verify the glyphs follow the active device and that the text fits at both scales.

## Build

Use a .NET SDK with .NET 8 support, Python 3, the installed Windows game, and a BepInEx 5 mod profile. The project defaults to the D: Steam library and reads BepInEx from the Daybook Dev profile. To use another installation, pass `-p:GameDir="..."` and `-p:ProfileDir="..."` to the build.

```powershell
dotnet build ProductionBottlenecks.csproj -c Release
dotnet run --project tests/StatusChecks.csproj -c Release
python package.py
```

The checks exercise the production-state reader against managed fixtures. The package script verifies the archived files against the source listing and Release binary, then writes the ZIP under `dist/`. Gameplay, physical controller behavior, and in-game layout require the playtest above.
