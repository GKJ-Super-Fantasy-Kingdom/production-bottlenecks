# 0.2.3

- Open **Blocked Buildings** from a city building’s panel to see each blocked building and its current reason. The list scrolls with the mouse wheel or controller focus and updates as warnings clear.
- Identify same-name buildings by their plot coordinates. If a selected warning disappears, focus moves to the nearest surviving row.

- Controller visits use the building’s click collider. When nearby targets make map focus ambiguous, the list opens the chosen building’s native inspection panel.

# 0.2.2 (local test build)

- Same-name buildings show their map coordinates. Select a row to visit that plot.
- When several warnings clear together, focus follows the closest surviving row.

# 0.2.1 (local test build)

- Blocked Buildings expands to fit available space. Longer lists scroll with the mouse wheel or as keyboard and controller focus moves between rows.

# 0.2.0 (local test build)

- Open **Blocked Buildings** from a city building’s inspection panel to browse buildings that currently report a production warning.
- Select a row to visit that building. The list refreshes as warnings change and offers additional pages for longer lists.

# 0.1.5

This update includes the recipe, tooltip layout and controller improvements tested in the 0.1.3 and 0.1.4 local builds.

- Remove the previous city's shortage row when a mod reuses the panel for a world building, including when it replaces the native panel generator.
- Keep a dismissed shortage closed while the same building and blocker remain focused. A new blocker or a fresh hover can show a warning again.

# 0.1.4 (local test build)

- Assigning or removing workers with a controller places the game's hammer and worker count beside the focused building. This also applies when changing assignments through navigation.

# 0.1.3 (local test build)

- Follow the active recipe when a started task's queue changes. A warning that disagrees with the recipe or sufficient local stock is identified as a reported shortage.
- Read effective storage capacity and available jobs for custom producers. Keep explicit weather and signless building shortages visible.
- Respect other mods that replace native warning and panel handlers. Resolve same-name resource icons using the actual sprite.
- Keep mouse shortage tooltips inside the screen at both UI scales. Near the bottom edge, the tooltip moves above the pointer.

# 0.1.2

Fixed missing warnings when a staffed building displays its out-of-work sign. Added support for processors, outside producers and buildings that expose a native work warning. Missing inputs now show the game's icon and, when the selected recipe and local stock establish it, the remaining quantity. The tooltip reads live requirements and allows native and third-party warning handlers to run.

# 0.1.1

Fixed blocked town producers showing only their building name on hover. The tooltip now adds the reason below the name and restores the name when the blocker clears.

# 0.1.0

First local test build. Town producers show one reason in the native tooltip and building panel. The line follows the focused building and updates when its worker assignment, reported shortage, or output storage changes.
