using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dalamud.Game;
using RadarPlugin.GameObjects;

namespace RadarPlugin.Managers {
    public static class GameObjectManager {
        public const uint EmptyObjectId = 0xE000_0000;
        public static readonly IntPtr BaseAddress;
        private static IntPtr _playerPtr;
        private static GameObject _player;
        private static readonly Dictionary<uint, GameObject> _cachedEntities = new Dictionary<uint, GameObject>();

        public static IEnumerable<GameObject> GameObjects => _cachedEntities.Values;

        public static GameObject LocalPlayer {
            get {
                var ptr = Core.Memory.Read<IntPtr>(BaseAddress);
                if (ptr == IntPtr.Zero)
                    return null;
                if (_player == null || _playerPtr != ptr) {
                    _playerPtr = ptr;
                    _player = new GameObject(ptr);
                }

                return _player;
            }
        }

        static GameObjectManager() {
            using (var scanner = new SigScanner(Process.GetCurrentProcess().MainModule)) {
                BaseAddress = scanner.GetStaticAddressFromSig("48 8B 42 08 48 C1 E8 03 3D A7 01 00 00 77 ?? 8B C0 48 8D 0D", 17);
            }
        }

        public static void Update() {
            if (BaseAddress == IntPtr.Zero) return;
            foreach (var gameObject in GameObjects)
                gameObject.UpdatePointer(IntPtr.Zero);
            foreach (var gameObject in GetRawEntities()) {
                if (gameObject == null || gameObject.Pointer == IntPtr.Zero) continue;
                if (_cachedEntities.TryGetValue(gameObject.ObjectId, out var oldObj))
                    oldObj.UpdatePointer(gameObject.Pointer);
                else
                    _cachedEntities.Add(gameObject.ObjectId, gameObject);
            }

            foreach (var invalidId in _cachedEntities.Where(kv => !kv.Value.IsValid).Select(kv => kv.Key).ToList())
                _cachedEntities.Remove(invalidId);
        }

        public static void Clear() {
            _cachedEntities.Clear();
        }

        public static IEnumerable<GameObject> GetRawEntities() {
            var hashSet = new HashSet<GameObject>();
            if (BaseAddress == IntPtr.Zero) return hashSet;
            var ptrList = Core.Memory.Read<IntPtr>(BaseAddress, 424);
            foreach (var ptr in ptrList) {
                if (ptr == IntPtr.Zero) continue;
                hashSet.Add(new GameObject(ptr));
            }

            return hashSet;
        }
    }
}