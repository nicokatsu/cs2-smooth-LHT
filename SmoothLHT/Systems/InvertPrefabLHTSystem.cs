using System;
using System.Collections.Generic;
using Colossal.Logging;
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

        private static readonly ILog log = LogManager.GetLogger($"{nameof(SmoothLHT)}").SetShowsErrorsInUI(false);

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
            log.Info($"Initializing {nameof(InvertPrefabLHTSystem)}");
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
            log.Info("on world ready");
            InvertAllPrefabs();
        }

        protected override void OnGamePreload(Purpose purpose, GameMode mode)
        {
            base.OnGamePreload(purpose, mode);
            InvertAllPrefabs();
        }

        public void InvertPrefab(PrefabBase prefab, NetInvertMode invertMode)
        {
            if (prefab is null || !prefab.TryGet(out ObjectSubNets subNets))
            {
                return;
            }

            prefabInvertService.InvertPrefabAndUpgrades(prefab, invertMode, buildingUpgrades, preferenceStore);
            log.Info($"Inverted {prefab.name} {subNets.m_InvertWhen}");
        }

        private void InvertAllPrefabs()
        {
            allAssets = SystemAPI.QueryBuilder()
                .WithAll<PrefabData>()
                .WithAny<BuildingData, BuildingExtensionData>()
                .Build();
            using var allAssetEntities = allAssets.ToEntityArray(Allocator.Temp);

            preferenceStore.Load();
            var scanResult = prefabScanner.Scan(allAssetEntities);

            InvertibleAssets.Clear();
            InvertibleAssets.UnionWith(scanResult.InvertibleAssets);
            buildingUpgrades = scanResult.BuildingUpgrades;

            try
            {
                prefabInvertService.ApplyPreferredInvertModes(scanResult.Prefabs, preferenceStore);
            }
            catch (Exception e)
            {
                log.Error($"Failed to apply invert preferences: {e}");
            }
        }
    }
}
