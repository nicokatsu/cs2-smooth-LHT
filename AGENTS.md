# Agent Guide

This workspace contains a Cities: Skylines II mod project. This file is for
future agents working in the repo. Do not assume it is tracked by git.

## Global Workflow

Follow the user's global `$cs2-mod-workflow` for coordination and lane
ownership. This file only records project-specific context for SmoothLHT.

## Project

The main project is `SmoothLHT`, short for Smooth Left-Hand Traffic.

Business goal:

- Make Cities: Skylines II left-hand traffic saves behave better for buildings
  with internal roads, driveways, and transport paths.
- Flip supported building internal networks so entrances and exits align with
  LHT traffic flow.
- Give the player a toolbar toggle to control inversion per asset and per
  upgrade.
- Persist the player's per-prefab choices between loads.

Important user-facing behavior:

- The mod changes prefab-level data, not individual placed building instances.
- Changing a toggle affects all already placed instances of that prefab or
  upgrade.
- Temporary pathfinding conflicts can happen after switching inversion; replacing
  the affected building or upgrade usually rebuilds the paths.
- The mod does not support left-hand and right-hand versions of the same asset
  at the same time.
- The mod is intended for LHT saves. RHT saves should be unaffected.

## Core Business Logic

The mod uses the game's built-in subnet inversion mechanism:

- It edits `ObjectSubNets.m_InvertWhen`.
- Enabled/default inverted state is `NetInvertMode.LefthandTraffic`.
- Disabled/non-inverted state is `NetInvertMode.Never`.
- This is a prefab-wide setting. It is not an instance-level mirror flip.

Supported internal network detection:

- The scanner only considers `BuildingPrefab` and `BuildingExtensionPrefab`.
- A prefab must have `ObjectSubNets`.
- Each subnet's `m_NetPrefab` is inspected when it is a `NetGeometryPrefab`.
- Supported lane types are:
  - active `CarLane` with `RoadTypes.Car`
  - active `CarLane` with `RoadTypes.Bicycle`
  - active `TrackLane` with `TrackTypes.Tram`
- Pure pedestrian paths, train tracks, subway tracks, power lines, and other
  unsupported network types should not qualify by themselves.

Ignored prefab prefixes live in `InvertPrefabLHTSystem`:

- `Aquaculture Area Placeholder -Water`
- `Offshore Oil Industry Placeholder`
- `Openwater Fish Farm Entrance`
- `Openwater Fishing Area Entrance`
- `Pack10-OHSignature02_Ext02`

Default non-inverted prefabs:

- `BusStation01 Extra Platforms`
- `BusStation01 Taxi Stop`
- `BusStation01`

## Data Flow

Startup/load flow:

1. `InvertPrefabLHTSystem.OnWorldReady` and `OnGamePreload` call
   `InvertAllPrefabs`.
2. `InvertPreferenceStore.Load` reads persisted non-inverted prefab names.
3. `InvertiblePrefabScanner.Scan` scans building and extension prefab entities.
4. Scanner returns:
   - `Prefabs`: invertible prefabs to update
   - `InvertibleAssets`: names of prefabs that qualify
   - `BuildingUpgrades`: host building name to qualifying upgrade prefabs
5. `PrefabInvertService.ApplyPreferredInvertModes` applies persisted modes to
   all scanned invertible prefabs.

Manual toggle flow:

1. `InvertPrefabUISystem` receives tool/prefab change events.
2. It asks `InvertPrefabLHTSystem.TryGetInvertMode` whether the selected prefab
   should show the toggle.
3. Toggle visibility is true when the selected prefab is itself invertible or
   when it has invertible upgrades.
4. Toggle state comes from `InvertPreferenceStore.GetDesiredInvertMode` using
   the selected prefab name. Do not infer UI state from an upgrade's current
   `m_InvertWhen`; that caused edge-case complexity.
5. When the user toggles, `InvertPrefabLHTSystem.InvertPrefab` calls
   `PrefabInvertService.InvertPrefabAndUpgrades`.
6. The service applies the mode recursively to the selected prefab and mapped
   upgrades, updates persisted preferences, and saves once.

Upgrade edge case:

- Some host buildings have no qualifying `ObjectSubNets`, but their upgrades do.
- The host building should still show the toggle if `BuildingUpgrades` maps it
  to at least one invertible upgrade.
- Applying the toggle to that host may not update the host itself if it has no
  `ObjectSubNets`, but it should update and persist the qualifying upgrades.

## Important Files

Backend:

- `SmoothLHT/Mod.cs`: mod load/unload and UI module registration entry point.
- `SmoothLHT/Systems/InvertPrefabLHTSystem.cs`: orchestrates scanning,
  preference loading, applying modes, and UI-facing state queries.
- `SmoothLHT/Services/InvertiblePrefabScanner.cs`: finds invertible building
  and extension prefabs and maps host buildings to upgrades.
- `SmoothLHT/Services/PrefabInvertService.cs`: applies `NetInvertMode` to
  prefabs and mapped upgrades, then saves preferences.
- `SmoothLHT/Services/InvertPreferenceStore.cs`: reads/writes
  `non_inverted_assets.json` under user data.
- `SmoothLHT/Tools/InvertPrefabUISystem.cs`: C# UI bindings exposed to the
  frontend.

Frontend:

- `SmoothLHT/UI/SmoothLHT/src/index.tsx`: UI extension registration.
- `SmoothLHT/UI/SmoothLHT/src/mods/InvertLHTTool.tsx`: injects the LHT invert
  tool section into the game's tool options UI.
- `SmoothLHT/UI/SmoothLHT/imgs/button.svg`: toolbar button image.

Publishing:

- `SmoothLHT/Properties/PublishConfiguration.xml`: Paradox Mods metadata,
  version, changelog, screenshots, tags, and external links.
- `SmoothLHT/Properties/Thumbnail.png`: primary thumbnail.
- `SmoothLHT/Properties/Screenshots/*`: publish screenshots.
- `README.md`: public project README.

Build/package:

- `SmoothLHT.sln`: normal solution build entry.
- `SmoothLHT/SmoothLHT.csproj`: CS2 mod project file and references.
- `SmoothLHT/UI/SmoothLHT/package.json`: frontend build scripts.
- `SmoothLHT/UI/SmoothLHT/webpack.config.js`: frontend bundle config.

## UI Notes

Frontend bindings:

- `IsShowing`: boolean, controls whether the tool section is injected.
- `IsInverted`: int, current `NetInvertMode`.
- `ToggleInverted`: trigger with the selected next mode.

Frontend mode constants currently used:

- `LEFT_HAND_TRAFFIC_MODE = 1`
- `DEFAULT_MODE = 0`

The frontend appends a `Section` and `ToolButton` to the vanilla
`MouseToolOptions` UI. Be careful with focus keys; earlier UI logs showed
duplicate focus-key warnings when injected controls are mishandled.

As of 2026-06-03, the frontend resolves vanilla `Section` and `ToolButton`
directly through the official `ModuleRegistry.get`/`extend` APIs during
registration. Do not reintroduce a feature-local vanilla component resolver or
local SCSS for the toggle button unless the game's vanilla `ToolButton` can no
longer render the icon/selected state correctly.

## Persistence

Persisted file:

- `ModsData/SmoothLHT/non_inverted_assets.json` under the CS2 user data path.

Model:

- The store records names of prefabs that should not be inverted.
- Missing from the set means desired mode is `NetInvertMode.LefthandTraffic`.
- Present in the set means desired mode is `NetInvertMode.Never`.
- If loading/parsing fails, the store falls back to the default non-inverted set
  and logs the error.

Key risk:

- Preferences are keyed by `prefab.name`. If an asset author renames a prefab,
  the stored preference may no longer apply.

## Publishing Notes

When updating publish metadata:

- Keep `ModVersion`, `ChangeLog`, README, screenshots, and public description
  consistent.
- Do not change `GameVersion` unless the user explicitly asks for that field.
- The project previously hit a Paradox/Skyve warning for bundling
  `Colossal.PSI.Common.dll`. Game assemblies must not be shipped with the mod.
  Check references in `SmoothLHT.csproj` and use `<Private>false</Private>` for
  game assemblies.

Current known release line:

- Recent release moved to `0.5.0`.
- Recent changelog covered game version `1.5.7f1` compatibility and the
  upgrade-only internal network toggle fix.

## Logs And Troubleshooting

Useful log locations:

- `C:\Users\glydd\AppData\LocalLow\Colossal Order\Cities Skylines II\Logs\SmoothLHT.log`
- `C:\Users\glydd\AppData\LocalLow\Colossal Order\Cities Skylines II\Player.log`
- `C:\Users\glydd\AppData\LocalLow\Colossal Order\Cities Skylines II\Logs\SceneFlow.log`
- `C:\Users\glydd\AppData\LocalLow\Colossal Order\Cities Skylines II\Logs\UI.log`
- `C:\Users\glydd\AppData\LocalLow\Colossal Order\Cities Skylines II\Logs\Modding.log`

Past observed non-SmoothLHT noise:

- `FindIt` generated duplicate vehicle/prop prefab IDs.
- `RoadBuilder` generated duplicate net piece prefab IDs.
- `SmartTransportation` previously shipped `Newtonsoft.Json.dll`, an in-game
  assembly warning.
- Base-game save/load migration can log unknown statistic prefab IDs.

When diagnosing CTDs:

- First separate native crash from managed exception.
- Look for the final meaningful managed stack before `Native Crash Reporting`.
- Do not blame `SmoothLHT` unless its own log or stack frames point there.
- Check exact timestamps across `Player.log`, `SceneFlow.log`, `Modding.log`,
  `UI.log`, and `SmoothLHT.log`.

## Reference And Research Notes

Reference repos that have been useful:

- `ZessonsDE/TrafficToolEssentials`
- `bruceyboy24804/Cities2-TrafficLightsEnhancement`
- `lucarager/CS2-NetworkTools`
- `lucarager/CS2-Platter`
- `lucarager/CS2-LucaModsCommon`

Local temporary clones may exist:

- `_tmp_tle/`
- `_tmp_tte/`

These are reference-only scratch directories. Do not commit them.

Lessons already taken from references:

- Keep game assemblies non-private in project references.
- Keep publish/docs and compatibility notes explicit.
- Use structured services for scanner, store, and apply logic.
- Prefer original implementation based on observed APIs and behavior, not copied
  source.
- 2026-06-03 UI/backend officialization references:
  - `https://cs2.paradoxwikis.com/Modding_Toolchain` oldid 6258: UI mods use
    the official React project/template, `index.tsx` injection points, webpack
    build, and `--uiDeveloperMode` debugger flow.
  - `https://cs2.paradoxwikis.com/Creating_UI_And_Code_Mods` oldid 5526: UI
    `mod.json` id should match the code mod target, and code/UI builds can be
    wired through the project file.
  - `https://cs2.paradoxwikis.com/Creating_a_Settings_File` oldid 3723 and
    `https://cs2.paradoxwikis.com/Options_UI` oldid 6261: official `.coc`
    settings exist, but SmoothLHT keeps dynamic per-prefab inversion choices in
    the existing `ModsData/SmoothLHT/non_inverted_assets.json` file by design.

## Build And Verification

Default verification:

- Run `dotnet build SmoothLHT.sln` after C# changes.
- The build also runs the frontend webpack build through the project tooling.
- A Sass legacy JS API warning from `sass-loader` is known. It is not usually a
  regression unless new warnings or errors appear.

Useful git checks:

- `git status --short`
- `git diff --stat`
- `git diff -- <path>`

Do not stage or commit unless the user asks.

## Editing Rules

- Prefer `rg` and `rg --files` for search.
- Use `apply_patch` for manual edits.
- Do not revert user changes.
- Avoid broad refactors during release metadata or bug-fix tasks.
- Keep temporary reference repos and generated scratch files out of commits.
- Use ASCII unless the file already requires non-ASCII or user-facing text needs
  it.

## Common User Replies

Broken pathfinding after toggling:

```text
That is usually broken pathfinding. Bulldozing and rebuilding the building or upgrade should fix it.
```

Request for always-available mirror flip:

```text
Not easily, because this mod changes the prefab, not individual building instances. It uses the game's built-in subnet inversion setting (`m_InvertWhen`), which can be tied to left-hand traffic and other game conditions. I may look into expanding this in the future, but it would make the project design quite a bit more complex.
```

Paradox/Skyve game-assembly warning appeal:

```text
Hello,

The warning was caused by an older package that accidentally included `Colossal.PSI.Common.dll`. This has already been fixed in the latest release, and the game assembly is no longer included in the published package.

Could you please re-check the latest version and clear the stability warning?

Thank you.
```
