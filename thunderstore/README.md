Hover over a blocked production building to see what it needs. If your Bakery is waiting for an ingredient, its tooltip can show the resource icon and how many more units must reach the building.

## Features

- See the reason beneath the building name and in its selection panel. Warnings include **No worker assigned**, **Output storage full**, **Queue a task**, and **Waiting for better weather**.
- See the missing amount when the active recipe and local stock identify it. A gatherer such as a Goldmine can show **Needs Ore** with the deposit icon when the game only reports the deposit type.
- When you change worker assignments with a controller, the game's hammer icon and worker count appear beside the focused building.

## How to Use

1. Point at a building with the mouse or move the controller's world cursor onto it. The shortage appears automatically.
2. Read the line under its name. In the Bakery example below, **Needs 1 Wheat** means one more Wheat must arrive for the current recipe. Left-click the Bakery or use the controller's confirm button to compare the shortage with its storage and recipe.
3. If the line says **No worker assigned**, use the game's assignment controls while focused on the building. With default bindings, **E** adds a worker slot and **Q** removes one. On a controller, move the **right stick up** to add a slot or **down** to remove one.

## Screenshots

![Bakery tooltip with a Wheat icon between “Needs 1” and “Wheat”](https://raw.githubusercontent.com/GKJ-Super-Fantasy-Kingdom/production-bottlenecks/0a0c2d39f18174bf7d7945c0c24e443247ca2b80/docs/screenshots/0.1.3/bakery-one-wheat.png)

One of two Wheat has arrived. Look beneath **Bakery** for the remaining **1 Wheat**. Captured in 0.1.3 with BakeryUsesWheat 1.2.0.

![Selected Bakery panel showing zero Wheat in storage, a two-Wheat recipe, and “Needs 2 Wheat” below the worker control](https://raw.githubusercontent.com/GKJ-Super-Fantasy-Kingdom/production-bottlenecks/0a0c2d39f18174bf7d7945c0c24e443247ca2b80/docs/screenshots/0.1.3/bakery-panel-1p5x.png)

Before the first delivery, the panel shows **Needs 2 Wheat** beneath the baker assignment. Captured in 0.1.3 at 1.5× UI scale with BakeryUsesWheat 1.2.0.

![Hammer icon and 8/7 worker count beneath the focused Lumberyard](https://raw.githubusercontent.com/GKJ-Super-Fantasy-Kingdom/production-bottlenecks/0a0c2d39f18174bf7d7945c0c24e443247ca2b80/docs/screenshots/0.1.4/after-1p5x.png)

After an assignment change, look just below the Lumberyard for the hammer and **8/7** count. Captured in 0.1.4 at 1.5× UI scale with simulated controller input.

## Compatibility

Production Bottlenecks reads the active recipe, delivered stock, and warnings exposed by the game. With **BakeryUsesWheat 1.2.0**, the Bakery tooltip and panel follow the switch from Flour to Wheat. Pin a Build Cost and Daybook were also tested together with Production Bottlenecks.

If another mod keeps its blocking rule private and the game shows only a work warning, the tooltip can show **Idle** or **Work blocked**. Other mods' open tooltips keep priority.

## Installation

Choose **Install with App** on this page and select your **Super Fantasy Kingdom** profile. Install the listed **BepInExPack** and **HideManagerGameObject_Config** dependencies, then launch with **Start modded**.
