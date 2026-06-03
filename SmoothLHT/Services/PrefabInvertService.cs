using System.Collections.Generic;
using Game.Prefabs;

namespace SmoothLHT.Services
{
    public class PrefabInvertService
    {
        private readonly PrefabSystem prefabSystem;

        public PrefabInvertService(PrefabSystem prefabSystem)
        {
            this.prefabSystem = prefabSystem;
        }

        public int ApplyPreferredInvertModes(IEnumerable<PrefabBase> prefabs, InvertPreferenceStore preferenceStore)
        {
            var updatedCount = 0;

            foreach (var prefab in prefabs)
            {
                updatedCount += UpdatePrefabInvertMode(prefab, preferenceStore.GetDesiredInvertMode(prefab.name));
            }

            return updatedCount;
        }

        public PrefabInvertResult InvertPrefabAndUpgrades(
            PrefabBase prefab,
            NetInvertMode invertMode,
            IReadOnlyDictionary<string, List<PrefabBase>> buildingUpgrades,
            InvertPreferenceStore preferenceStore)
        {
            var result = new PrefabInvertResult();
            ApplyInvertModeRecursively(prefab, invertMode, buildingUpgrades, preferenceStore, new HashSet<string>(), result);
            preferenceStore.Save();
            result.SavedNonInvertedPreferenceCount = preferenceStore.NonInvertedAssets.Count;
            return result;
        }

        private void ApplyInvertModeRecursively(
            PrefabBase prefab,
            NetInvertMode invertMode,
            IReadOnlyDictionary<string, List<PrefabBase>> buildingUpgrades,
            InvertPreferenceStore preferenceStore,
            HashSet<string> visitedPrefabNames,
            PrefabInvertResult result)
        {
            if (prefab is null || !visitedPrefabNames.Add(prefab.name))
            {
                return;
            }

            result.VisitedPrefabCount++;
            result.UpdatedPrefabCount += UpdatePrefabInvertMode(prefab, invertMode);
            preferenceStore.SetInvertMode(prefab.name, invertMode);

            if (!buildingUpgrades.TryGetValue(prefab.name, out var upgrades))
            {
                return;
            }

            foreach (var upgrade in upgrades)
            {
                ApplyInvertModeRecursively(upgrade, invertMode, buildingUpgrades, preferenceStore, visitedPrefabNames, result);
            }
        }

        private int UpdatePrefabInvertMode(PrefabBase prefab, NetInvertMode invertMode)
        {
            if (prefab.TryGet(out ObjectSubNets subNets) && subNets.m_InvertWhen != invertMode)
            {
                subNets.m_InvertWhen = invertMode;
                prefabSystem.UpdatePrefab(prefab);
                return 1;
            }

            return 0;
        }
    }

    public class PrefabInvertResult
    {
        public int VisitedPrefabCount { get; set; }

        public int UpdatedPrefabCount { get; set; }

        public int SavedNonInvertedPreferenceCount { get; set; }
    }
}
