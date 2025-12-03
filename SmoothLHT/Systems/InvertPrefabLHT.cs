using System;
using System.Linq;
using Colossal.Logging;
using Colossal.Serialization.Entities;
using Game;
using Game.City;
using Game.Prefabs;
using Unity.Collections;
using Unity.Entities;

namespace SmoothLHT.Systems
{
    public partial class InvertPrefabLHT : GameSystemBase
    {
        private PrefabSystem prefabSystem;
        private EntityQuery allAssets;
        private bool isAllInverted;

        private static string[] ASSETS_PREFIX_NOT_INVERTED =
        {
            "Dome Parking Hall",
            "Aquaculture Area Placeholder -Water",
            "Offshore Oil Industry Placeholder",
            "Openwater Fish Farm Entrance",
            "Openwater Fishing Area Entrance",
            "BusStation01",
            "Pack10-OHSignature02_Ext02"
        };


        private static ILog log = LogManager.GetLogger($"{nameof(SmoothLHT)}").SetShowsErrorsInUI(false);


        protected override void OnCreate()
        {
            base.OnCreate();
            log.Info($"Initializing {nameof(InvertPrefabLHT)}");
            prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            World.GetOrCreateSystemManaged<CityConfigurationSystem>();
            allAssets = SystemAPI.QueryBuilder().WithAllRW<PrefabData>().Build();
        }

        protected override void OnUpdate()
        {
        }

        protected override void OnGamePreload(Purpose purpose, GameMode mode)
        {
            base.OnGamePreload(purpose, mode);
            if (isAllInverted) return;
            var allAssetEntities = allAssets.ToEntityArray(Allocator.Temp);
            log.Info($"Loaded {allAssetEntities.Length} assets");
            var ct = 0;
            foreach (var entity in allAssetEntities)
            {
                try
                {
                    if (!prefabSystem.TryGetPrefab(entity, out PrefabBase prefab) ||
                        prefab is not (BuildingPrefab or BuildingExtensionPrefab) ||
                        ASSETS_PREFIX_NOT_INVERTED.Any(prefab.name.StartsWith) ||
                        !prefab.TryGet(out ObjectSubNets subNets) ||
                        subNets is null ||
                        subNets.m_InvertWhen.Equals(NetInvertMode.LefthandTraffic)
                       ) continue;

                    subNets.m_InvertWhen = NetInvertMode.LefthandTraffic;
                    prefabSystem.UpdatePrefab(prefab);
                    ct++;
                }
                catch (Exception e)
                {
                    log.Error($"Failed to invert prefab {entity}: {e}");
                }
            }
            isAllInverted = true;
            log.Info($"Inverted {ct} assets");
        }
    }
}