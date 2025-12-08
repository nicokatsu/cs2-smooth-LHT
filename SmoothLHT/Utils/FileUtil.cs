using System.Collections.Generic;
using System.IO;
using Colossal.Json;
using Colossal.PSI.Environment;

namespace SmoothLHT.Utils
{
    public static class FileUtil
    {
        public static string folder { get; }

        static FileUtil()
        {
            folder = Path.Combine(EnvPath.kUserDataPath, "ModsData", "SmoothLHT");
            Directory.CreateDirectory(folder);
        }

        public static void saveAssets(HashSet<string> assets)
        {
            var path = Path.Combine(folder, "non_inverted_assets.json");
            File.WriteAllText(path, JSON.Dump(assets));
        }

        public static HashSet<string> loadAssets()
        {
            var path = Path.Combine(folder, "non_inverted_assets.json");
            if (File.Exists(path))
            {
                return JSON.MakeInto<HashSet<string>>(JSON.Load(File.ReadAllText(path)));
            }

            return new HashSet<string>();
        }
    }
}