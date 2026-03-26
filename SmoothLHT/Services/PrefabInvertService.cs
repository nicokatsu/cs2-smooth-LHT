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

        public void InvertPrefabAndUpgrades(
            PrefabBase prefab,
            NetInvertMode invertMode,
            IReadOnlyDictionary<string, List<PrefabBase>> buildingUpgrades,
            InvertPreferenceStore preferenceStore)
        {
            ApplyInvertModeRecursively(prefab, invertMode, buildingUpgrades, preferenceStore, new HashSet<string>());
            preferenceStore.Save();
        }

        private void ApplyInvertModeRecursively(
            PrefabBase prefab,
            NetInvertMode invertMode,
            IReadOnlyDictionary<string, List<PrefabBase>> buildingUpgrades,
            InvertPreferenceStore preferenceStore,
            HashSet<string> visitedPrefabNames)
        {
            if (prefab is null || !visitedPrefabNames.Add(prefab.name))
            {
                return;
            }

            UpdatePrefabInvertMode(prefab, invertMode);
            preferenceStore.SetInvertMode(prefab.name, invertMode);

            if (!buildingUpgrades.TryGetValue(prefab.name, out var upgrades))
            {
                return;
            }

            foreach (var upgrade in upgrades)
            {
                ApplyInvertModeRecursively(upgrade, invertMode, buildingUpgrades, preferenceStore, visitedPrefabNames);
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
}
