using System;
using System.Collections.Generic;
using System.IO;
using Colossal.Json;
using Colossal.PSI.Environment;
using Game.Prefabs;

namespace SmoothLHT.Services
{
    public class InvertPreferenceStore
    {
        private const string NonInvertedAssetsFileName = "non_inverted_assets.json";
        private readonly HashSet<string> defaultNonInvertedAssets;
        private readonly string storageFolder;

        public InvertPreferenceStore(IEnumerable<string> defaultNonInvertedAssets)
        {
            this.defaultNonInvertedAssets = new HashSet<string>(defaultNonInvertedAssets);
            storageFolder = Path.Combine(EnvPath.kUserDataPath, "ModsData", "SmoothLHT");
            Directory.CreateDirectory(storageFolder);
        }

        public HashSet<string> NonInvertedAssets { get; private set; } = new HashSet<string>();

        public void Load()
        {
            var path = GetStoragePath();
            if (!File.Exists(path))
            {
                NonInvertedAssets = new HashSet<string>(defaultNonInvertedAssets);
                Mod.LogDiagnostic($"[Preferences] No persisted preferences at {path}; using defaults count={NonInvertedAssets.Count}");
                return;
            }

            try
            {
                NonInvertedAssets = JSON.MakeInto<HashSet<string>>(JSON.Load(File.ReadAllText(path))) ??
                                    new HashSet<string>(defaultNonInvertedAssets);
                Mod.LogDiagnostic($"[Preferences] Loaded non-inverted assets count={NonInvertedAssets.Count} from {path}");
            }
            catch (IOException e)
            {
                Mod.LogException(e, $"Failed to read non-inverted assets from {path}; using defaults count={defaultNonInvertedAssets.Count}.");
                NonInvertedAssets = new HashSet<string>(defaultNonInvertedAssets);
            }
            catch (Exception e)
            {
                Mod.LogException(e, $"Failed to parse non-inverted assets from {path}; using defaults count={defaultNonInvertedAssets.Count}.");
                NonInvertedAssets = new HashSet<string>(defaultNonInvertedAssets);
            }
        }

        public void Save()
        {
            var path = GetStoragePath();
            try
            {
                File.WriteAllText(path, JSON.Dump(NonInvertedAssets));
                Mod.LogDiagnostic($"[Preferences] Saved non-inverted assets count={NonInvertedAssets.Count} to {path}");
            }
            catch (Exception e)
            {
                Mod.LogException(e, $"Failed to save non-inverted assets count={NonInvertedAssets.Count} to {path}.");
            }
        }

        public NetInvertMode GetDesiredInvertMode(string prefabName)
        {
            return NonInvertedAssets.Contains(prefabName)
                ? NetInvertMode.Never
                : NetInvertMode.LefthandTraffic;
        }

        public void SetInvertMode(string prefabName, NetInvertMode invertMode)
        {
            if (invertMode == NetInvertMode.Never)
            {
                NonInvertedAssets.Add(prefabName);
            }
            else
            {
                NonInvertedAssets.Remove(prefabName);
            }
        }

        private string GetStoragePath()
        {
            return Path.Combine(storageFolder, NonInvertedAssetsFileName);
        }
    }
}
