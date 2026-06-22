using Colossal.Logging;
using Game;
using System;
using System.Diagnostics;
using Game.Modding;
using Game.SceneFlow;
using SmoothLHT.Systems;
using SmoothLHT.UI;


namespace SmoothLHT
{
    public class Mod : IMod
    {
        public static readonly ILog log = LogManager.GetLogger($"{nameof(SmoothLHT)}").SetShowsErrorsInUI(false);

        public const string ModID = "SmoothLHT";

        public void OnLoad(UpdateSystem updateSystem)
        {
            LogEssential(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                LogEssential($"Current mod asset at {asset.path}");
            
            updateSystem.UpdateAt<InvertPrefabLHTSystem>(SystemUpdatePhase.PrefabUpdate);
            updateSystem.UpdateAt<InvertPrefabUISystem>(SystemUpdatePhase.UIUpdate);
        }

        public void OnDispose()
        {
            LogEssential(nameof(OnDispose));
        }

        internal static void LogEssential(string message)
        {
            log.Info(message);
        }

        [Conditional("DEBUG")]
        internal static void LogDiagnostic(string message)
        {
            log.Info(message);
        }

        internal static void LogException(Exception exception, string message)
        {
            log.Info(exception, $"[ERROR] {message}");
        }
    }
}
