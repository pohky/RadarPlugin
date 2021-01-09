using RadarPlugin.GameObjects;
using RadarPlugin.Managers;
using RadarPlugin.Memory;

namespace RadarPlugin {
    public static class Core {
        public static readonly ProcessMemory Memory = new ProcessMemory();
        public static GameObject Me => GameObjectManager.LocalPlayer;
        public static GameObject Player => GameObjectManager.LocalPlayer;
    }
}