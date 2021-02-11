using System;
using System.Numerics;
using ImGuiNET;
using RadarPlugin.GameObjects;

namespace RadarPlugin.PluginGui {
    public class DrawObject {
        public IntPtr Address { get; }
        public string Name { get; }
        public Vector3 ObjectLocation { get; }
        public GameObjectType Type { get; }

        public uint Color { get; }
        public int Radius { get; set; } = 4;

        public DrawObject(GameObject obj) {
            Address = obj.Pointer;
            Name = obj.Name;
            ObjectLocation = obj.Location;
            Type = obj.Type;

            switch (Type) {
                case GameObjectType.Mount:
                case GameObjectType.Minion:
                case GameObjectType.Retainer:
                    Color = 0xFFFFFFFF;
                    break;
                case GameObjectType.Treasure:
                case GameObjectType.AetheryteObject:
                case GameObjectType.EventNpc:
                case GameObjectType.EventObject:
                case GameObjectType.GatheringPoint:
                case GameObjectType.HousingEventObject:
                    Color = 0xFF42a1f5;
                    break;
                case GameObjectType.BattleNpc:
                    Color = 0xFFf54242;
                    break;
                case GameObjectType.Pc:
                    //Name = "<censored>";
                    Color = 0xFF42f54b;
                    break;
                default:
                    Color = 0xFF000000;
                    break;
            }
        }

        public void Draw(ImDrawListPtr g, Vector3 origin, Vector3 playerLocation, float playerHeading, float scale) {
            //fixed north
            //var loc = playerLocation.Subtract(ObjectLocation).Scale(scale);
            //var drawLoc = new Vector3(-loc.X, 0, -loc.Z).Add(origin);

            //rotate with player
            var drawLoc = playerLocation.Subtract(ObjectLocation).Rotate2D(playerHeading).Scale(scale).Add(origin);
            
            g.AddCircleFilled(drawLoc.ToVector2(), Radius, Color, 16);
            
            if (ImGui.IsMouseHoveringRect(new Vector2(drawLoc.X - Radius, drawLoc.Z - Radius), new Vector2(drawLoc.X + Radius, drawLoc.Z + Radius))) {
                ImGui.SetTooltip($"[{Type}] {Name} 0x{Address.ToInt64():X8}");
            }

            var textLoc = drawLoc.Subtract(new Vector3(0, 0, -(Radius + 1)));
            g.AddText(ImGui.GetFont(), 14, textLoc.ToVector2(), 0xFFFFFFFF, $"{Name}");
        }
    }
}