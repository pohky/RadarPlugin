using System;
using System.Numerics;
using ImGuiNET;
using RadarPlugin.Managers;

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
            DrawRadar();
        }

        private void DrawRadar() {
            var windowFlags = ImGuiWindowFlags.NoBackground;
            if (!RadarVisible) {
                windowFlags |= ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar;
            }
            ImGui.SetNextWindowSizeConstraints(new Vector2(100), new Vector2(float.PositiveInfinity));
            if (ImGui.Begin("Radar", windowFlags)) {
                DrawRadarObjects();
                ImGui.End();
            }
        }

        private void DrawRadarObjects() {
            var player = GameObjectManager.LocalPlayer;
            if(player == null) return;
            var width = ImGui.GetContentRegionMax().X; //ImGui.GetIO().DisplaySize.X;
            var height = ImGui.GetContentRegionMax().Y; //ImGui.GetIO().DisplaySize.Y;
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
                var obj = new DrawObject(gameObject);
                obj.Draw(g, origin, playerLocation, playerHeading, height / 125f);
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