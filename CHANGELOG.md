# 0.2.9

- Match the native stepped outline on the Blocked Buildings launcher, rows, and Back button during hover, keyboard or controller focus, and held press.
- Keep the wider Back button below the scrolling list. Back and Cancel return focus to the building that opened the list.
- Show native staffing and resource icons beside production warnings, including in the building tooltip.

# 0.2.8 (local review)

- Restore the wider Back button in Blocked Buildings while retaining the Pin action style.

# 0.2.7 (local review)

- Match Blocked Buildings’ Back button to the native dialog style used for Pin, Unpin, and Replace Pin.

# 0.2.6 (local review)

- Use the outlined native Back button in Blocked Buildings, with distinct hover, controller focus and pressed states.

# 0.2.5 (local review)

- Blocked Buildings uses separate columns for building art and status icons, with a shared hover and controller outline and a compact Back footer.
- Building tooltips lead with the matching resource or staffing icon when available.

# 0.2.4

Clarify the mod’s description and compatibility for players.

# 0.2.3

- Open **Blocked Buildings** from a city building’s panel to see each blocked building and its current reason. The list scrolls with the mouse wheel or controller focus and updates as warnings clear.
- Identify same-name buildings by their plot coordinates. If a selected warning disappears, focus moves to the nearest surviving row.

- Controller visits use the building’s click collider. When nearby targets make map focus ambiguous, the list opens the chosen building’s native inspection panel.

# 0.2.2

- Same-name buildings show their map coordinates. Select a row to visit that plot.
- When several warnings clear together, focus follows the closest surviving row.

# 0.2.1

- Blocked Buildings expands to fit available space. Longer lists scroll with the mouse wheel or as keyboard and controller focus moves between rows.

# 0.2.0

- Open **Blocked Buildings** from a city building’s inspection panel to browse buildings that currently report a production warning.
- Select a row to visit that building. The list refreshes as warnings change and offers additional pages for longer lists.

# 0.1.5

Improves recipe quantities, tooltip layout, and controller focus.

- Remove the previous city's shortage row when a mod reuses the panel for a world building, including when it replaces the native panel generator.
- Keep a dismissed shortage closed while the same building and blocker remain focused. A new blocker or a fresh hover can show a warning again.

# 0.1.4

- Assigning or removing workers with a controller places the game's hammer and worker count beside the focused building. This also applies when changing assignments through navigation.

# 0.1.3

- Follow the active recipe when a started task's queue changes. A warning that disagrees with the recipe or sufficient local stock is identified as a reported shortage.
- Read effective storage capacity and available jobs for custom producers. Keep explicit weather and signless building shortages visible.
- Respect other mods that replace native warning and panel handlers. Resolve same-name resource icons using the actual sprite.
- Keep mouse shortage tooltips inside the screen at both UI scales. Near the bottom edge, the tooltip moves above the pointer.

# 0.1.2

Fixed missing warnings when a staffed building displays its out-of-work sign. Added support for processors, outside producers and buildings that expose a native work warning. Missing inputs now show the game's icon and, when the selected recipe and local stock establish it, the remaining quantity. The tooltip reads live requirements and allows native and third-party warning handlers to run.

# 0.1.1

Fixed blocked town producers showing only their building name on hover. The tooltip now adds the reason below the name and restores the name when the blocker clears.

# 0.1.0

Town producers show one reason in the native tooltip and building panel. The line follows the focused building and updates when its worker assignment, reported shortage, or output storage changes.
