# Production Bottlenecks compatibility checks

Production Bottlenecks reads the active building's public game state on hover and selection. Other mods can change that state without referencing this plugin. The package depends only on BepInEx and its manager configuration.

## Panel reuse and tooltip dismissal in 0.1.5

The city and world panel generators both release this mod's previous row when the same panel changes to a replacement view. An unrelated panel keeps its own state. Closing a visible tooltip on a blocked building keeps that shortage closed until the blocker changes or focus leaves and returns. This applies to a third-party card that occupied the tooltip while the same shortage was active.

Offline checks loaded the DLL from the local ZIP, resolved both Harmony targets against the installed game and invoked their postfixes. They exercised same-panel and unrelated-panel transitions, including a replacement that skips native cleanup. The tooltip checks exercised dismissal, refresh, changed blockers and fresh focus. Unity activation and destruction were simulated in memory. The earlier 0.1.4 player run reproduced the reopening at both scales. Live rendering and physical controller input for 0.1.5 remain unchecked.

To run the archive-level checks from the checkout, set the installed game's Managed directory and a BepInEx profile's core directory:

```powershell
$managed = 'D:/SteamLibrary/steamapps/common/Super Fantasy Kingdom/Super Fantasy Kingdom_Data/Managed'
$core = "$env:APPDATA/Thunderstore Mod Manager/DataFolder/SuperFantasyKingdom/profiles/Daybook Dev/BepInEx/core"
dotnet run --project checks/UiOwnership -c Release -- dist/ProductionBottlenecks-0.1.5.zip "$managed" "$core"
```

## Controller worker count in 0.1.4

On game 0.8.068, the game alone and the ten-mod controller profile with Production Bottlenecks 0.1.3 both displayed the hammer count at the parked mouse. The focused Lumberyard was at (960, 540), and the mouse was at (1516, 303) in Unity screen coordinates. With the same ten mods and Production Bottlenecks 0.1.4, the top of the count was centered 24 pixels below the building at 1× and 36 pixels below it at 1.5×. Assigning and removing a slot changed the game's count between 8/7 and 7/7 in all three configurations.

The isolated probe used virtual right-stick navigation, invoked the direct assign/remove handlers using test input while in controller mode, and checked the game's E/Q bindings after switching to keyboard and mouse. The legacy pointer was unlocked for the parked-mouse scenario. Moving it left the controller count in place. The Tree shortage returned on mouse hover and cleared when focus left. An open third-party-style tooltip kept its text. On 0.1.4, a programmatic close while the same blocked building stayed focused allowed its shortage to appear again on refresh. Physical controller feel remains unverified.

| UI scale | Ten mods with 0.1.3 | Ten mods with 0.1.4 |
| --- | --- | --- |
| 1× | [Before](screenshots/0.1.4/before-1x.png) | [After](screenshots/0.1.4/after-1x.png) |
| 1.5× | [Before](screenshots/0.1.4/before-1p5x.png) | [After](screenshots/0.1.4/after-1p5x.png) |

## Packaged mod checks

The offline comparison used the installed game's 0.8.067 managed assemblies and invoked patch methods from the versioned mod DLLs directly. Unity object lifetime, localization and sprite rendering were represented by test data. Stock deliveries, the missing-resource flag and worker transitions were supplied to the game objects. The checks compared the status used by the building panel with the text composed for hover. They did not run the job scheduler, Harmony dispatch, the scene renderer or controller input.

| Mod and configuration | Observation |
| --- | --- |
| Production Bottlenecks alone | The installed Human Bakery prefab uses 2 Flour and 1 Water to produce 2 Bread. With one worker slot assigned and Water supplied, Flour stocks of 0, 1 and 2 produced **Needs 2 Flour**, **Needs 1 Flour**, then a clear panel row and the bare **Bakery** hover title when the game warning cleared. |
| ownlyme/BakeryUsesWheat 1.2.0, default behavior | Its packaged initialization prefix changed Flour to Wheat in the recipe and input storage, retaining the quantities. Repeating the same stock sequence displayed **Needs 2 Wheat**, **Needs 1 Wheat**, then cleared. Both strings used the Wheat sprite supplied by the game resource lookup. The prefix also set the Windmill's Flour capacity to 30. |
| sfkbutter/No_Breaks 1.0.2, default behavior | Its packaged postfix extends work through Sunset and Moonrise on ordinary and boss days, and through Evening on ordinary days. Night stays closed. Compared all seven phase values on days 3 and 7. A worker on the extended shift received no false idle warning, and a reported weather restriction remained visible. |
| Pin a Build Cost 0.1.0, local packaged DLL | Its cost calculation continued to report the held-material deficit and then **Ready** alongside the bakery's changing production status. Its upgrade control and the non-interactive production status row also appeared separately in the live building panel. |
| ownlyme/TavernFoodPriorities 1.7.0, package inspection | Its Harmony prefix targets tavern food selection. It can change which stored foods are consumed. It does not patch the building recipe, world tooltip or building-panel generation methods. |

See the published packages for [Bakery Uses Wheat](https://thunderstore.io/c/super-fantasy-kingdom/p/ownlyme/BakeryUsesWheat/versions/), [No Breaks](https://thunderstore.io/c/super-fantasy-kingdom/p/sfkbutter/No_Breaks/versions/), [Tavern Food Priorities](https://thunderstore.io/c/super-fantasy-kingdom/p/ownlyme/TavernFoodPriorities/versions/).

## Custom rules and UI ownership

- Started demand tasks use the same current recipe index as the game's task panel. An unstarted task uses the head of its queue. Reordering or clearing a queue leaves an already-started task's calculation attached to its active recipe.
- A resource reported outside the selected recipe, or still reported after enough stock arrives, is identified as a **reported shortage**. The tooltip gives a number only for an unambiguous positive local deficit. With only a visible work sign, **Idle** or **Work blocked** is the fallback. A completely private rule with no public signal yields no inferred cause.
- Custom producers with empty recipe lists can still expose a direct output. Effective output capacity, available jobs and weather restrictions are read from the game. A custom building can report a missing resource even when it has no sign renderer.
- Where the text atlas identifies a different sprite under the same name, the plugin uses the resource's own sprite if it can draw it. An unsupported sprite falls back to the resource name. The existing Ore and Tree examples remain in the [screenshot gallery](screenshots/0.1.2).
- The world tooltip yields to unrelated open text. A skipped native panel generator relinquishes the mod's own row. The plugin does not register a new input action, change the pin row or consume its clicks. Visitor Calendar uses the shared tooltip for its calendar cells; coexistence with its open text was checked by source and ownership fixtures.

## Live player checks

The Windows 0.8.067 player loaded Production Bottlenecks 0.1.3, Pin a Build Cost 0.1.0 and Daybook 0.4.0 together in a separate save directory. A probe exercised the game's input actions using virtual mouse, keyboard and gamepad devices. Captures used 1920 × 1080 borderless fullscreen at UI scales 1 and 1.5.

- **Staffed Lumberyard.** With two worker slots assigned, one worker active and a Tree shortage set through the building's warning API, hover and selection displayed the Tree sprite and name. Clearing the warning removed the row. Tab pinned a native upgrade. Holding Left Alt or the gamepad's right trigger expanded the ledger. The tooltip, ledger and pin had separate bounds. The status row used the native font, allowed clicks through and cleared the native action buttons. [Ledger at 1×](screenshots/0.1.3/lumberyard-ledger-1x.png)
- **Screen edges.** At 1.5× the mouse tooltip originally extended 98 pixels beyond the left edge and 101 pixels below the bottom. The repaired tooltip fit at the left, right and bottom edges at both scales. At the bottom it moved above the pointer. [Bottom edge at 1.5×](screenshots/0.1.3/lumberyard-bottom-1p5x.png)
- **BakeryUsesWheat 1.2.0.** The same three mods ran with ownlyme's packaged DLL in its default configuration. The Human Bakery's live recipe used 2 Wheat and 1 Water for 2 Bread. With two worker slots assigned, Water stocked and available Wheat exhausted, the native crafting job reported a Wheat shortage. The tooltip and selected panel showed **Needs 2 Wheat**, then **Needs 1 Wheat** after one delivery. With the second Wheat the job found its input, the panel row disappeared and the hover title returned to **Bakery**. The probe inserted real resource packets and invoked the job's resource search with simulation time held steady. [Bakery panel at 1.5×](screenshots/0.1.3/bakery-panel-1p5x.png) · [One Wheat delivered](screenshots/0.1.3/bakery-one-wheat.png)

The [0.1.2 Goldmine and Lumberyard captures](screenshots/0.1.2) remain the player-supplied evidence for those original cases. Physical controller feel, a complete unattended worker delivery and bake cycle, No Breaks shifts in a live scene, and a third-party pack introducing a new building and resource remain open for playtesting.
