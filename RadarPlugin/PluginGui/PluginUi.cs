using System;
using System.Numerics;
using ImGuiNET;
using RadarPlugin.GameObjects;
using RadarPlugin.Managers;

namespace RadarPlugin.PluginGui {
    public class PluginUi : IDisposable {
        private readonly XivRadarPlugin _plugin;

        public bool RadarVisible = true;
        public bool SettingsVisible;
        public float Scale = 75f;
        public bool ShowPc = true;
        public bool ShowNpc;
        public bool ShowBattleNpc;

        public PluginUi(XivRadarPlugin plugin) {
            _plugin = plugin;
            _plugin.Interface.UiBuilder.OnBuildUi += Draw;
        }

        public void Draw() {
            if (SettingsVisible) DrawSettings();
            DrawRadar();
        }

        private void DrawRadar() {
            var windowFlags = ImGuiWindowFlags.NoBackground;
            if (!RadarVisible) {
                windowFlags |= ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar;
            }
            ImGui.SetNextWindowSizeConstraints(new Vector2(100), new Vector2(float.PositiveInfinity));
            if (ImGui.Begin("Radar", windowFlags)) {
                ImGui.SetNextItemWidth(200f);
                ImGui.SliderFloat("Scale", ref Scale, 25f, 500f, "%.0f", ImGuiSliderFlags.ClampOnInput);
                ImGui.SameLine(); ImGui.Checkbox("PC", ref ShowPc);
                ImGui.SameLine(); ImGui.Checkbox("NPC", ref ShowNpc);
                ImGui.SameLine(); ImGui.Checkbox("BNPC", ref ShowBattleNpc);
                DrawRadarObjects();
                ImGui.End();
            }
        }

        private void DrawRadarObjects() {
            var player = GameObjectManager.LocalPlayer;
            if(player == null) return;
            var width = ImGui.GetContentRegionMax().X;
            var height = ImGui.GetContentRegionMax().Y;
            var center = new Vector2(width / 2 + ImGui.GetWindowPos().X, height / 2 + ImGui.GetWindowPos().Y);
            var playerLocation = player.Location;
            var playerHeading = player.Heading;
            var origin = new Vector3(center.X, 0, center.Y);

            var g = ImGui.GetWindowDrawList();
            foreach (var gameObject in GameObjectManager.GameObjects) {
                if (gameObject == player) {
                    g.AddCircleFilled(center, 4, 0xFF0000FF);
                    continue;
                }
                switch (gameObject.Type) {
                    case GameObjectType.Mount:
                    case GameObjectType.Minion:
                    case GameObjectType.Retainer:
                        continue;
                    case GameObjectType.Treasure:
                    case GameObjectType.AetheryteObject:
                    case GameObjectType.EventNpc:
                    case GameObjectType.EventObject:
                    case GameObjectType.GatheringPoint:
                    case GameObjectType.HousingEventObject:
                        if(!ShowNpc) continue;
                        break;
                    case GameObjectType.BattleNpc:
                        if(!ShowBattleNpc) continue;
                        break;
                    case GameObjectType.Pc:
                        if(!ShowPc) continue;
                        break;
                }
                var obj = new DrawObject(gameObject);
                obj.Draw(g, origin, playerLocation, playerHeading, height / Scale);
            }
        }

        private void DrawSettings() {
            if (ImGui.Begin("XivRadar Settings", ref SettingsVisible)) {
                ImGui.TextUnformatted($"ObjectList: {GameObjectManager.BaseAddress.ToInt64():X8}");
                ImGui.End();
            }
        }

        public void Dispose() {
            _plugin.Interface.UiBuilder.OnBuildUi -= Draw;
        }
    }
}