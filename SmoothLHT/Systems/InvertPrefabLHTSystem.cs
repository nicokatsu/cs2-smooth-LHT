using System;
using System.Collections.Generic;
using System.Linq;
using Colossal.Logging;
using Colossal.Serialization.Entities;
using Game;
using Game.City;
using Game.Prefabs;
using SmoothLHT.Utils;
using Unity.Collections;
using Unity.Entities;

namespace SmoothLHT.Systems
{
    public partial class InvertPrefabLHTSystem : GameSystemBase
    {
        private PrefabSystem prefabSystem;
        private EntityQuery allAssets;
        private bool isAllInverted;

        private static string[] ASSETS_PREFIX_SHOULD_NOT_INVERTED =
        {
            "Dome Parking Hall",
            "Aquaculture Area Placeholder -Water",
            "Offshore Oil Industry Placeholder",
            "Openwater Fish Farm Entrance",
            "Openwater Fishing Area Entrance",
            "BusStation01",
            "Pack10-OHSignature02_Ext02"
        };
        
        private HashSet<string> nonInvertedAssets;

        public HashSet<string> InvertibleAssets { get; } = new HashSet<string>();

        private Dictionary<string, List<PrefabBase>> buildingUpgrades = new Dictionary<string, List<PrefabBase>>();


        private static ILog log = LogManager.GetLogger($"{nameof(SmoothLHT)}").SetShowsErrorsInUI(false);


        protected override void OnCreate()
        {
            base.OnCreate();
            log.Info($"Initializing {nameof(InvertPrefabLHTSystem)}");
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
            nonInvertedAssets = FileUtil.loadAssets();
            var ct = 0;
            foreach (var entity in allAssetEntities)
            {
                try
                {
                    if (!prefabSystem.TryGetPrefab(entity, out PrefabBase prefab) ||
                        prefab is not (BuildingPrefab or BuildingExtensionPrefab) ||
                        ASSETS_PREFIX_SHOULD_NOT_INVERTED.Any(prefab.name.StartsWith) ||
                        !prefab.TryGet(out ObjectSubNets subNets) ||
                        subNets is null
                       ) continue;
                    InvertibleAssets.Add(prefab.name);
                    subNets.m_InvertWhen = !nonInvertedAssets.Contains(prefab.name)
                        ? NetInvertMode.LefthandTraffic
                        : NetInvertMode.Never;
                    prefabSystem.UpdatePrefab(prefab);
                    if (prefab is BuildingExtensionPrefab && prefab.TryGet(out ServiceUpgrade serviceUpgrade))
                    {
                        var buildings = serviceUpgrade.m_Buildings;
                        foreach (var building in buildings)
                        {
                            if (!buildingUpgrades.TryGetValue(building.name, out List<PrefabBase> upgrades))
                            {
                                upgrades = new List<PrefabBase>();
                                buildingUpgrades[building.name] = upgrades;
                            }

                            upgrades.Add(prefab);
                        }
                    }

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

        public void invertPrefab(PrefabBase prefab, NetInvertMode invertMode)
        {
            if (prefab.TryGet(out ObjectSubNets subNets))
            {
                subNets.m_InvertWhen = invertMode;
                prefabSystem.UpdatePrefab(prefab);
                if (buildingUpgrades.TryGetValue(prefab.name, out List<PrefabBase> upgrades))
                {
                    foreach (var upgrade in upgrades)
                    {
                        invertPrefab(upgrade, invertMode);
                    }
                }

                if (invertMode == NetInvertMode.Never)
                {
                    nonInvertedAssets.Add(prefab.name);
                }
                else
                {
                    nonInvertedAssets.Remove(prefab.name);
                }

                FileUtil.saveAssets(nonInvertedAssets);

                log.Info($"Inverted {prefab.name} {subNets.m_InvertWhen}");
            }
        }
    }
}