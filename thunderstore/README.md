**Production Bottlenecks** shows why a building has stopped producing in **Super Fantasy Kingdom**. Hover over the building to see the reason beneath its name. Select it to see the same reason in its panel.

## What the tooltip tells you

- **Missing resource.** The tooltip shows the resource's name and game icon when available. If a recipe needs 4 Flour and the building already holds 1, it shows **Needs 3 Flour**.
- **Missing deposit.** A Goldmine can report **Needs Ore** and a Lumberyard can report **Needs Tree**, even with workers assigned. Gatherers report the deposit type without a quantity.
- **Other blockers.** The tooltip can tell you to assign a worker, free space in output storage, queue a task, or wait for better weather. If the building raises its work sign without reporting a specific cause, it shows **Idle** or **Work blocked**.

## Controls

Point at a building with the mouse or the controller's world cursor. The reason appears automatically. Use the game's usual select and worker-details actions to inspect the building or change its assignments. When you adjust jobs with a controller, the game's hammer icon and worker count appear beneath the focused building.

## Mod compatibility

The tooltip reads the current recipe, ingredient amounts, local stock and reported shortage as you play. Changes made through those game systems appear on refresh. A custom requirement can be named when its mod reports the missing resource to the game. If the report disagrees with the current recipe or enough stock has already arrived, the tooltip labels it as a reported shortage.

Closing a tooltip while inspecting a blocked building keeps that shortage closed. Move focus away and back to see it again. A change in the blocker can also show a new warning.

## Installation

Choose **Install with App** in Thunderstore, select your **Super Fantasy Kingdom** profile, and launch with **Start modded**.

## In-game examples

With BakeryUsesWheat 1.2.0 enabled, delivering one of the two required Wheat leaves **Needs 1 Wheat** beneath the Bakery name. Captured in 0.1.3.

![Bakery tooltip showing Needs 1 Wheat after one delivery](https://raw.githubusercontent.com/GKJ-Super-Fantasy-Kingdom/production-bottlenecks/0a0c2d39f18174bf7d7945c0c24e443247ca2b80/docs/screenshots/0.1.3/bakery-one-wheat.png)

Before that delivery, the selected Bakery shows **Needs 2 Wheat** below the worker control. Its storage and recipe are visible in the same panel. Captured at UI scale 1.5 in 0.1.3.

![Bakery panel with no Wheat in storage and a two-Wheat shortage](https://raw.githubusercontent.com/GKJ-Super-Fantasy-Kingdom/production-bottlenecks/0a0c2d39f18174bf7d7945c0c24e443247ca2b80/docs/screenshots/0.1.3/bakery-panel-1p5x.png)

After a worker assignment, the hammer and **8/7** count appear beside the focused Lumberyard. Captured at UI scale 1.5 with simulated controller input in 0.1.4.

![Native hammer and worker count beneath the focused Lumberyard](https://raw.githubusercontent.com/GKJ-Super-Fantasy-Kingdom/production-bottlenecks/0a0c2d39f18174bf7d7945c0c24e443247ca2b80/docs/screenshots/0.1.4/after-1p5x.png)
