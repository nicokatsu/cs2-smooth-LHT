using Colossal.Logging;
using Colossal.UI.Binding;
using Game.Prefabs;
using Game.Tools;
using Game.UI;
using SmoothLHT.Systems;

namespace SmoothLHT.UI
{
    public partial class InvertPrefabUISystem : UISystemBase
    {
        private static ILog log = LogManager.GetLogger($"{nameof(SmoothLHT)}").SetShowsErrorsInUI(false);

        private ValueBinding<bool> isShowing;

        private ValueBinding<int> isInverted;


        private InvertPrefabLHTSystem invertPrefabLHTSystem;
        private ToolSystem toolSystem;
        private PrefabBase currentPrefab;


        protected override void OnCreate()
        {
            base.OnCreate();
            invertPrefabLHTSystem = World.GetOrCreateSystemManaged<InvertPrefabLHTSystem>();

            AddBinding(isShowing = new ValueBinding<bool>(Mod.ModID, "IsShowing", false));
            AddBinding(isInverted = new ValueBinding<int>(Mod.ModID, "IsInverted", (int)NetInvertMode.LefthandTraffic));

            AddBinding(new TriggerBinding<int>(Mod.ModID, "ToggleInverted", ToggleInverted));

            toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            toolSystem.EventToolChanged += EventToolChanged;
            toolSystem.EventPrefabChanged += EventPrefabChanged;
        }

        private void ToggleInverted(int val)
        {
            invertPrefabLHTSystem.invertPrefab(currentPrefab, (NetInvertMode)val);
            isInverted.Update(val);
        }

        private void EventToolChanged(ToolBaseSystem obj)
        {
            if (obj is ObjectToolSystem or UpgradeToolSystem && obj.GetPrefab())
            {
                EventPrefabChanged(obj.GetPrefab());
            }
            else
            {
                isShowing.Update(false);
            }
        }

        private void EventPrefabChanged(PrefabBase obj)
        {
            if (obj is BuildingPrefab or BuildingExtensionPrefab &&
                invertPrefabLHTSystem.InvertibleAssets.Contains(obj.name) &&
                obj.TryGet(out ObjectSubNets subNets))
            {
                currentPrefab = obj;
                isShowing.Update(true);
                isInverted.Update((int)subNets.m_InvertWhen);
            }
            else
            {
                isShowing.Update(false);
            }
        }
    }
}