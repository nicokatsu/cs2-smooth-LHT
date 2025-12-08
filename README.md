# Smooth Left-Hand Traffic (Experimental)

**[Paradox Mod](https://mods.paradoxplaza.com/mods/123929/Windows)**

This mod adjusts building prefabs to better support **Left-Hand Traffic (LHT)** cities.
It **flips each building’s internal roads** and driveways when loading LHT saves so that **entrances and exits match the traffic direction** — preventing vehicle crossovers and twisted internal paths.

Works with **all building assets**, including mod ones.
Some mods already include separate LHT and RHT versions (for example, [Overground Parking by Dome](https://mods.paradoxplaza.com/mods/81913/Windows));
those will keep their original layout to avoid visual conflicts.

### **Update: Per-Building Invert Toggle**

With the release of the Asset Editor, many new assets will come — and not all of them behave well when their networks are inverted.
This update adds a **new toolbar button** that lets you **toggle “Invert Networks” for each individual service building** (enabled by default).

**What the toggle does:**
- Changes the **inversion setting for each building and its in-place upgrade**.
- Useful if a specific asset breaks when inverted or simply doesn’t need inversion.
- The invert toggle setting for each building **is saved** and will persist.

**Important note:**
- Changing the toggle will **affect all already-placed instances** of this building (and its upgrade).
- Currently **does not support having both inverted and non-inverted versions** of the same building placed at the same time.
- Switching the toggle may temporarily cause **pathfinding errors**. If that happens, **replacing the affected building or it's upgrade** will fix the issue.

## Experimental Notice
This mod is still in testing.
Some assets may have:
- unreachable internal paths,
- or incorrectly flipped models that cause placement conflicts.
- or ground arrows that display incorrectly after flipping (this does **not affect gameplay** and you can manually fix decals).

Known problematic assets are already excluded, but if you encounter more, please leave feedback, so I can update the list.

**Note:** Buildings **already placed** in your city will need to be **replaced** for the changes to take effect.



## Tips
- The mod only affects **Left-Hand Traffic saves** — Right-Hand saves are **unaffected**.
- You can **freely switch** between LHT and RHT saves **without restarting or disabling** the mod.


### Credits
- Implementation inspired by **StarQ**’s mod example.
- UI implementation references ideas from **Kemorno’s _Advanced Road Tools_**.
- Uses **yenyang’s _VanillaComponentResolver_** for component resolution.


