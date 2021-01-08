using System;
using System.Numerics;
using RadarPlugin.Managers;

namespace RadarPlugin.GameObjects {
    public class GameObject : RemoteObject, IEquatable<GameObject> {
        private string _name;
        public override bool IsValid => base.IsValid && ObjectId == GetObjectId();
        public string Name => GetName();
        public uint ObjectId { get; }
        public uint IdLocation { get; private set; }

        public GameObjectType Type => (GameObjectType)Memory.Read<byte>(Pointer + Offsets.Object.Type);
        public Vector3 Location => Memory.Read<Vector3>(Pointer + Offsets.Object.Location);
        public float Heading => Memory.Read<float>(Pointer + Offsets.Object.Heading);

        public virtual uint NpcId => Memory.Read<uint>(Pointer + Offsets.Object.NpcId);

        public GameObject(IntPtr pointer) : base(pointer) {
            ObjectId = GetObjectId();
        }

        private uint GetObjectId() {
            uint objId;
            var id1 = Memory.Read<uint>(Pointer + Offsets.Object.ObjectId);
            if (id1 != GameObjectManager.EmptyObjectId) {
                objId = id1;
                IdLocation = 0;
            } else {
                var id2 = Memory.Read<uint>(Pointer + Offsets.Object.ObjectId2);
                var id3 = Memory.Read<ushort>(Pointer + Offsets.Object.ObjectId3);
                if (id2 != 0 && id3 - 200 > 43) {
                    objId = id2;
                    IdLocation = 1;
                } else {
                    objId = id3;
                    IdLocation = 2;
                }
            }
            return objId;
        }

        private string GetName() {
            if (!string.IsNullOrEmpty(_name)) return _name;
            
            var type = Type;
            var npcId = NpcId;
            string localizedName = null;
            if (type == GameObjectType.BattleNpc && npcId > 0) {
                localizedName = GameDataManager.GetBattleNpcName(npcId);
            } else if (type == GameObjectType.EventNpc) {
                localizedName = GameDataManager.GetEventNpcName(npcId);
            } else if (type == GameObjectType.GatheringPoint) {
                return _name = Memory.ReadString(Pointer + Offsets.Object.Name);
            }
            if (!string.IsNullOrEmpty(localizedName))
                return _name = localizedName;
            return _name = Memory.ReadString(Pointer + Offsets.Object.Name);
        }
        
        public override string ToString() => $"{Name} {Pointer.ToInt64():X8}";

        #region IEquatable

        public bool Equals(GameObject other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ObjectId == other.ObjectId;
        }

        public override bool Equals(object obj) {
            return ReferenceEquals(this, obj) || obj is GameObject other && Equals(other);
        }

        public override int GetHashCode() {
            return (int)ObjectId;
        }

        public static bool operator ==(GameObject left, GameObject right) {
            return Equals(left, right);
        }

        public static bool operator !=(GameObject left, GameObject right) {
            return !Equals(left, right);
        }

        #endregion
    }
}