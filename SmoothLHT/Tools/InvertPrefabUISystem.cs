using Colossal.UI.Binding;
using Game.Prefabs;
using Game.Tools;
using Game.UI;
using SmoothLHT.Systems;

namespace SmoothLHT.UI
{
    public partial class InvertPrefabUISystem : UISystemBase
    {
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
            if (currentPrefab is null)
            {
                return;
            }

            invertPrefabLHTSystem.InvertPrefab(currentPrefab, (NetInvertMode)val);
            isInverted.Update(val);
        }

        private void EventToolChanged(ToolBaseSystem obj)
        {
            if (obj is ObjectToolSystem or UpgradeToolSystem && obj.GetPrefab())
            {
                EventPrefabChanged(obj.GetPrefab());
                return;
            }

            Hide();
        }

        private void EventPrefabChanged(PrefabBase prefab)
        {
            if (!ShouldShowForPrefab(prefab, out var invertMode))
            {
                Hide();
                return;
            }

            currentPrefab = prefab;
            isShowing.Update(true);
            isInverted.Update((int)invertMode);
        }

        private bool ShouldShowForPrefab(PrefabBase prefab, out NetInvertMode invertMode)
        {
            return invertPrefabLHTSystem.TryGetInvertMode(prefab, out invertMode);
        }

        private void Hide()
        {
            currentPrefab = null;
            isShowing.Update(false);
        }
    }
}
