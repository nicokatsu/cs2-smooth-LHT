# Smooth Left-Hand Traffic

**[Paradox Mods page](https://mods.paradoxplaza.com/mods/123929/Windows)**

Smooth Left-Hand Traffic adjusts building internal roads and driveways for **left-hand traffic (LHT)** cities in **Cities: Skylines II**.  
It flips supported building networks so entrances and exits line up better with LHT traffic flow and avoid awkward crossovers.

## What it does

- Scans building assets and flips supported internal road networks for LHT saves.
- Supports vanilla and modded buildings.
- Lets you control inversion for **each asset** and **each upgrade** separately.
- Saves your per-asset preferences so they persist between loads.

## Important behavior

- Changing the inversion setting affects **all already-placed instances** of that asset or upgrade.
- Switching inversion may temporarily cause **pathfinding conflicts**.
- Replacing the affected building or upgrade usually fixes those pathfinding issues.
- The mod currently **does not support having left-hand and right-hand versions of the same asset at the same time**.
- The mod only affects **LHT saves**. RHT saves are unaffected.

## Notes

- Some assets may still have broken internal paths or visual issues after inversion.
- Known problematic assets are excluded where possible, but edge cases can still exist.
- If an asset behaves badly, you can disable inversion for that specific asset or upgrade from the in-game toolbar.

## Credits

- Inspired by **StarQ**'s mod example.
- UI approach references ideas from **Kemorno's Advanced Road Tools**.
- Uses **yenyang's VanillaComponentResolver** for UI component resolution.
