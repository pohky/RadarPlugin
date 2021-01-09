using System;
using Dalamud.Game.Command;
using Dalamud.Game.Internal;
using Dalamud.Plugin;
using RadarPlugin.Managers;
using RadarPlugin.PluginGui;

namespace RadarPlugin {
    public class XivRadarPlugin : IDalamudPlugin {
        public string Name => "XivRadar";
        public DalamudPluginInterface Interface { get; private set; }
        public PluginUi Gui { get; private set; }

        public void Initialize(DalamudPluginInterface pluginInterface) {
            Interface = pluginInterface;
            pluginInterface.UiBuilder.DisableAutomaticUiHide = true;
            Interface.CommandManager.AddHandler("/radar", new CommandInfo(CommandHandler));
            GameDataManager.Init(pluginInterface);
            Gui = new PluginUi(this);
            Interface.Framework.OnUpdateEvent += FrameworkOnUpdate;
        }

        private static void FrameworkOnUpdate(Framework framework) {
            GameObjectManager.Update();
        }
        
        private void CommandHandler(string cmd, string args) {
            if (string.IsNullOrEmpty(args.Trim()))
                Gui.SettingsVisible = !Gui.SettingsVisible;
            if (args.Trim().Equals("toggle", StringComparison.OrdinalIgnoreCase))
                Gui.RadarVisible = !Gui.RadarVisible;
        }

        public void Dispose() {
            if(Interface == null) return;
            Interface.CommandManager.RemoveHandler("/radar");
            Interface.Framework.OnUpdateEvent -= FrameworkOnUpdate;
            Interface.Dispose();
            Gui?.Dispose();
        }
    }
}