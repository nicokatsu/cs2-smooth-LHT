using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using SmoothLHT.Systems;
using SmoothLHT.UI;


namespace SmoothLHT
{
    public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger($"{nameof(SmoothLHT)}").SetShowsErrorsInUI(false);
        
        public const string ModID =  "SmoothLHT";

        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");
            
            updateSystem.UpdateAt<InvertPrefabLHTSystem>(SystemUpdatePhase.PrefabUpdate);
            updateSystem.UpdateAt<InvertPrefabUISystem>(SystemUpdatePhase.UIUpdate);
        }

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
        }
    }
}