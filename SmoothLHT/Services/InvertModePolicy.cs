using Game.Prefabs;

namespace SmoothLHT.Services
{
    public static class InvertModePolicy
    {
        public static bool IsSupported(NetInvertMode invertMode)
        {
            return invertMode == NetInvertMode.Never ||
                   invertMode == NetInvertMode.LefthandTraffic;
        }
    }
}
