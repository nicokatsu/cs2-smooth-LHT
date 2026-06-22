using System;
using System.Collections.Generic;
using Colossal.Serialization.Entities;
using Game;
using Game.Prefabs;
using SmoothLHT.Services;
using Unity.Collections;
using Unity.Entities;

namespace SmoothLHT.Systems
{
    public partial class InvertPrefabLHTSystem : GameSystemBase
    {
        private static readonly string[] AssetsPrefixShouldNotBeInverted =
        {
            "Aquaculture Area Placeholder -Water",
            "Offshore Oil Industry Placeholder",
            "Openwater Fish Farm Entrance",
            "Openwater Fishing Area Entrance",
            "Pack10-OHSignature02_Ext02"
        };

        private static readonly string[] DefaultNonInvertedAssets =
        {
            "BusStation01 Extra Platforms",
            "BusStation01 Taxi Stop",
            "BusStation01"
        };

        private PrefabSystem prefabSystem;
        private EntityQuery allAssets;
        private InvertPreferenceStore preferenceStore;
        private InvertiblePrefabScanner prefabScanner;
        private PrefabInvertService prefabInvertService;
        private Dictionary<string, List<PrefabBase>> buildingUpgrades = new();

        public HashSet<string> InvertibleAssets { get; } = new HashSet<string>();

        protected override void OnCreate()
        {
            base.OnCreate();
            Mod.LogDiagnostic($"Initializing {nameof(InvertPrefabLHTSystem)}");
            prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            preferenceStore = new InvertPreferenceStore(DefaultNonInvertedAssets);
            prefabScanner = new InvertiblePrefabScanner(prefabSystem, AssetsPrefixShouldNotBeInverted);
            prefabInvertService = new PrefabInvertService(prefabSystem);
        }

        protected override void OnUpdate()
        {
        }

        protected override void OnWorldReady()
        {
            base.OnWorldReady();
            Mod.LogEssential("[Lifecycle] OnWorldReady: refreshing prefab invert modes");
            InvertAllPrefabs();
        }

        protected override void OnGamePreload(Purpose purpose, GameMode mode)
        {
            base.OnGamePreload(purpose, mode);
            Mod.LogEssential($"[Lifecycle] OnGamePreload purpose={purpose} mode={mode}: refreshing prefab invert modes");
            InvertAllPrefabs();
        }

        public void InvertPrefab(PrefabBase prefab, NetInvertMode invertMode)
        {
            if (prefab is null)
            {
                Mod.LogDiagnostic("[Toggle] Skipped invert request because prefab is null");
                return;
            }

            if (!InvertModePolicy.IsSupported(invertMode))
            {
                Mod.LogEssential($"[ERROR] Ignoring unsupported invert mode {invertMode} for prefab={prefab.name}");
                return;
            }

            var result = prefabInvertService.InvertPrefabAndUpgrades(prefab, invertMode, buildingUpgrades, preferenceStore);
            Mod.LogEssential($"[Toggle] Applied invert mode prefab={prefab.name} mode={invertMode} visited={result.VisitedPrefabCount} updated={result.UpdatedPrefabCount} nonInvertedPreferences={result.SavedNonInvertedPreferenceCount}");
        }

        public bool TryGetInvertMode(PrefabBase prefab, out NetInvertMode invertMode)
        {
            invertMode = NetInvertMode.LefthandTraffic;

            if (prefab is null || prefab is not BuildingPrefab and not BuildingExtensionPrefab)
            {
                return false;
            }

            if (InvertibleAssets.Contains(prefab.name) || HasInvertibleUpgrades(prefab))
            {
                invertMode = preferenceStore.GetDesiredInvertMode(prefab.name);
                return true;
            }

            return false;
        }

        private bool HasInvertibleUpgrades(PrefabBase prefab)
        {
            if (!buildingUpgrades.TryGetValue(prefab.name, out var upgrades))
            {
                return false;
            }

            foreach (var upgrade in upgrades)
            {
                if (InvertibleAssets.Contains(upgrade.name))
                {
                    return true;
                }
            }

            return false;
        }

        private void InvertAllPrefabs()
        {
            allAssets = SystemAPI.QueryBuilder()
                .WithAll<PrefabData>()
                .WithAny<BuildingData, BuildingExtensionData>()
                .Build();
            using var allAssetEntities = allAssets.ToEntityArray(Allocator.Temp);

            Mod.LogDiagnostic($"[Scan] Starting prefab scan candidateEntities={allAssetEntities.Length}");
            preferenceStore.Load();
            var scanResult = prefabScanner.Scan(allAssetEntities);

            InvertibleAssets.Clear();
            InvertibleAssets.UnionWith(scanResult.InvertibleAssets);
            buildingUpgrades = scanResult.BuildingUpgrades;

            Mod.LogDiagnostic($"[Scan] Completed scanned={scanResult.ScannedEntityCount} invertible={scanResult.Prefabs.Count} upgradeHosts={scanResult.BuildingUpgrades.Count} upgradeLinks={scanResult.MappedUpgradeCount} skipped={FormatSkipCounts(scanResult.SkippedPrefabs)}");

            try
            {
                var updatedCount = prefabInvertService.ApplyPreferredInvertModes(scanResult.Prefabs, preferenceStore);
                Mod.LogDiagnostic($"[Apply] Applied preferred invert modes candidates={scanResult.Prefabs.Count} updated={updatedCount} nonInvertedPreferences={preferenceStore.NonInvertedAssets.Count}");
            }
            catch (Exception e)
            {
                Mod.LogException(e, "Failed to apply invert preferences.");
            }
        }

        private static string FormatSkipCounts(Dictionary<PrefabSkipReason, int> skippedPrefabs)
        {
            if (skippedPrefabs.Count == 0)
            {
                return "none";
            }

            var parts = new List<string>();
            foreach (var skippedPrefab in skippedPrefabs)
            {
                parts.Add($"{skippedPrefab.Key}={skippedPrefab.Value}");
            }

            return string.Join(", ", parts);
        }
    }
}
