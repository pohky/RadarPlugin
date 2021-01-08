using System;
using System.Collections.Generic;
using System.Linq;
using RadarPlugin.GameObjects;

namespace RadarPlugin.Managers {
    public class GameObjectManager {
        public const uint EmptyObjectId = 0xE000_0000;
        public readonly IntPtr BaseAddress;
        private IntPtr _playerPtr;
        private GameObject _player;
        private readonly Dictionary<uint, GameObject> _cachedEntities = new Dictionary<uint, GameObject>();

        public IEnumerable<GameObject> GameObjects => _cachedEntities.Values;

        public GameObject LocalPlayer {
            get {
                var ptr = Memory.Read<IntPtr>(BaseAddress);
                if (ptr == IntPtr.Zero)
                    return null;
                if (_player == null || _playerPtr != ptr) {
                    _playerPtr = ptr;
                    _player = new GameObject(ptr);
                }
                return _player;
            }
        }

        public GameObjectManager(XivRadarPlugin plugin) {
            BaseAddress = plugin.Interface.TargetModuleScanner.GetStaticAddressFromSig("48 8B 42 08 48 C1 E8 03 3D A7 01 00 00 77 ?? 8B C0 48 8D 0D", 17);
        }
        
        public void Update() {
            if(BaseAddress == IntPtr.Zero) return;
            foreach (var gameObject in GameObjects)
                gameObject.UpdatePointer(IntPtr.Zero);
            foreach (var gameObject in GetRawEntities()) {
                if(gameObject == null || gameObject.Pointer == IntPtr.Zero) continue;
                if (_cachedEntities.TryGetValue(gameObject.ObjectId, out var oldObj)) {
                    oldObj.UpdatePointer(gameObject.Pointer);
                } else {
                    _cachedEntities.Add(gameObject.ObjectId, gameObject);
                }
            }
            RemoveInvalidEntries(_cachedEntities);
        }

        public void Clear() {
            _cachedEntities.Clear();
        }

        public IEnumerable<GameObject> GetRawEntities() {
            var hashSet = new HashSet<GameObject>();
            if (BaseAddress == IntPtr.Zero) return hashSet;
            var ptrList = Memory.Read<IntPtr>(BaseAddress, 424);
            foreach (var ptr in ptrList) {
                if(ptr == IntPtr.Zero) continue;
                hashSet.Add(new GameObject(ptr));
            }
            return hashSet;
        }

        private static void RemoveInvalidEntries(Dictionary<uint, GameObject> list) {
            foreach (var invalidId in list.Where(kv => !kv.Value.IsValid).Select(kv => kv.Key).ToList())
                list.Remove(invalidId);
        }
    }
}