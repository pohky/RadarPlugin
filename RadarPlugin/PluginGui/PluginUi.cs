using System;
using System.Numerics;
using ImGuiNET;

namespace RadarPlugin.PluginGui {
    public class PluginUi : IDisposable {
        private readonly XivRadarPlugin _plugin;

        public bool RadarVisible = true;
        public bool SettingsVisible;

        public PluginUi(XivRadarPlugin plugin) {
            _plugin = plugin;
            _plugin.Interface.UiBuilder.OnBuildUi += Draw;
        }

        public void Draw() {
            if (SettingsVisible) DrawSettings();
            if (RadarVisible) DrawRadar();
        }

        private void DrawRadar() {
            var player = _plugin.GameObjectManager.LocalPlayer;
            if(player == null) return;

            var width = ImGui.GetIO().DisplaySize.X;
            var height = ImGui.GetIO().DisplaySize.Y;
            var center = new Vector2(width / 2, height / 2);
            var playerLocation = player.Location;
            var playerHeading = player.Heading;
            var origin = new Vector3(center.X, 0, center.Y);

            var g = ImGui.GetBackgroundDrawList();
            foreach (var gameObject in _plugin.GameObjectManager.GameObjects) {
                if (gameObject == player) {
                    g.AddCircleFilled(center, 4, 0xFF0000FF);
                    continue;
                }
                var obj = new DrawObject(gameObject);
                obj.Draw(g, origin, playerLocation, playerHeading, height / 125f);
            }
        }

        private void DrawSettings() {
            if (ImGui.Begin("XivRadar Settings", ref SettingsVisible)) {
                ImGui.TextUnformatted($"ObjectList: {_plugin.GameObjectManager.BaseAddress.ToInt64():X8}");
                ImGui.End();
            }
        }

        public void Dispose() {
            _plugin.Interface.UiBuilder.OnBuildUi -= Draw;
        }
    }
}