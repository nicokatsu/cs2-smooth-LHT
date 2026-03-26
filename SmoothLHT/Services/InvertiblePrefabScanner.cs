using System.Collections.Generic;
using System.Linq;
using Game.City;
using Game.Prefabs;
using Unity.Collections;
using Unity.Entities;

namespace SmoothLHT.Services
{
    public class InvertiblePrefabScanner
    {
        private readonly PrefabSystem prefabSystem;
        private readonly string[] ignoredPrefabNamePrefixes;

        public InvertiblePrefabScanner(PrefabSystem prefabSystem, string[] ignoredPrefabNamePrefixes)
        {
            this.prefabSystem = prefabSystem;
            this.ignoredPrefabNamePrefixes = ignoredPrefabNamePrefixes;
        }

        public PrefabScanResult Scan(NativeArray<Entity> assetEntities)
        {
            var invertibleAssets = new HashSet<string>();
            var buildingUpgrades = new Dictionary<string, List<PrefabBase>>();
            var prefabs = new List<PrefabBase>();

            foreach (var entity in assetEntities)
            {
                if (!TryGetInvertiblePrefab(entity, out var prefab))
                {
                    continue;
                }

                invertibleAssets.Add(prefab.name);
                prefabs.Add(prefab);
                MapBuildingUpgrades(prefab, buildingUpgrades);
            }

            return new PrefabScanResult(
                prefabs,
                invertibleAssets,
                buildingUpgrades);
        }

        private bool TryGetInvertiblePrefab(Entity entity, out PrefabBase prefab)
        {
            prefab = null;

            return prefabSystem.TryGetPrefab(entity, out prefab) &&
                   prefab is BuildingPrefab or BuildingExtensionPrefab &&
                   !ignoredPrefabNamePrefixes.Any(prefab.name.StartsWith) &&
                   prefab.TryGet(out ObjectSubNets subNets) &&
                   subNets is not null &&
                   HasSupportedTransportKinds(subNets);
        }

        private static bool HasSupportedTransportKinds(ObjectSubNets subNets)
        {
            if (subNets?.m_SubNets is null)
            {
                return false;
            }

            foreach (var subNet in subNets.m_SubNets)
            {
                if (subNet.m_NetPrefab is NetGeometryPrefab geometryPrefab)
                {
                    var transportKinds = GetSupportedTransportKinds(geometryPrefab);
                    if (transportKinds != SupportedTransportKinds.None)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static SupportedTransportKinds GetSupportedTransportKinds(NetGeometryPrefab geometryPrefab)
        {
            var transportKinds = SupportedTransportKinds.None;

            if (geometryPrefab.m_Sections is null)
            {
                return transportKinds;
            }

            foreach (var sectionInfo in geometryPrefab.m_Sections)
            {
                if (sectionInfo.m_Section?.m_Pieces is null)
                {
                    continue;
                }

                foreach (var pieceInfo in sectionInfo.m_Section.m_Pieces)
                {
                    if (pieceInfo.m_Piece is null || !pieceInfo.m_Piece.TryGet(out NetPieceLanes pieceLanes) || pieceLanes.m_Lanes is null)
                    {
                        continue;
                    }

                    foreach (var laneInfo in pieceLanes.m_Lanes)
                    {
                        if (laneInfo.m_Lane is null)
                        {
                            continue;
                        }

                        if (laneInfo.m_Lane.TryGet(out CarLane carLane) &&
                            carLane.active)
                        {
                            if ((carLane.m_RoadType & Game.Net.RoadTypes.Car) != 0)
                            {
                                transportKinds |= SupportedTransportKinds.Car;
                            }

                            if ((carLane.m_RoadType & Game.Net.RoadTypes.Bicycle) != 0)
                            {
                                transportKinds |= SupportedTransportKinds.Bicycle;
                            }
                        }

                        if (laneInfo.m_Lane.TryGet(out TrackLane trackLane) &&
                            trackLane.active &&
                            trackLane.m_TrackType == Game.Net.TrackTypes.Tram)
                        {
                            transportKinds |= SupportedTransportKinds.Tram;
                        }
                    }
                }
            }

            return transportKinds;
        }

        private static void MapBuildingUpgrades(PrefabBase prefab, Dictionary<string, List<PrefabBase>> buildingUpgrades)
        {
            if (prefab is not BuildingExtensionPrefab || !prefab.TryGet(out ServiceUpgrade serviceUpgrade))
            {
                return;
            }

            foreach (var building in serviceUpgrade.m_Buildings)
            {
                if (!buildingUpgrades.TryGetValue(building.name, out var upgrades))
                {
                    upgrades = new List<PrefabBase>();
                    buildingUpgrades[building.name] = upgrades;
                }

                if (!upgrades.Contains(prefab))
                {
                    upgrades.Add(prefab);
                }
            }
        }
    }

    [System.Flags]
    public enum SupportedTransportKinds
    {
        None = 0,
        Car = 1,
        Bicycle = 2,
        Tram = 4
    }

    public class PrefabScanResult
    {
        public PrefabScanResult(
            List<PrefabBase> prefabs,
            HashSet<string> invertibleAssets,
            Dictionary<string, List<PrefabBase>> buildingUpgrades)
        {
            Prefabs = prefabs;
            InvertibleAssets = invertibleAssets;
            BuildingUpgrades = buildingUpgrades;
        }

        public List<PrefabBase> Prefabs { get; }

        public HashSet<string> InvertibleAssets { get; }

        public Dictionary<string, List<PrefabBase>> BuildingUpgrades { get; }
    }
}
