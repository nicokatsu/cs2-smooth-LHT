using System;
using System.Collections.Generic;
using System.IO;
using Colossal.Json;
using Colossal.Logging;
using Colossal.PSI.Environment;
using Game.Prefabs;

namespace SmoothLHT.Services
{
    public class InvertPreferenceStore
    {
        private const string NonInvertedAssetsFileName = "non_inverted_assets.json";
        private static readonly ILog log = LogManager.GetLogger($"{nameof(SmoothLHT)}").SetShowsErrorsInUI(false);
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
                return;
            }

            try
            {
                NonInvertedAssets = JSON.MakeInto<HashSet<string>>(JSON.Load(File.ReadAllText(path)));
            }
            catch (IOException e)
            {
                log.Error($"Failed to read non-inverted assets from {path}: {e}");
                NonInvertedAssets = new HashSet<string>(defaultNonInvertedAssets);
            }
            catch (Exception e)
            {
                log.Error($"Failed to parse non-inverted assets from {path}: {e}");
                NonInvertedAssets = new HashSet<string>(defaultNonInvertedAssets);
            }
        }

        public void Save()
        {
            File.WriteAllText(GetStoragePath(), JSON.Dump(NonInvertedAssets));
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
