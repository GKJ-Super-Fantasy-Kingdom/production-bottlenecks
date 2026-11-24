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
