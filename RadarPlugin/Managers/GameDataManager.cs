using System.Collections.Generic;
using System.Globalization;
using Dalamud;
using Dalamud.Plugin;
using Lumina.Excel.GeneratedSheets;

namespace RadarPlugin.Managers {
    public static class GameDataManager {
        private static readonly Dictionary<uint, string> _eventNpcNames = new Dictionary<uint, string>();
        private static readonly Dictionary<uint, string> _battleNpcNames = new Dictionary<uint, string>();
        private static TextInfo _textInfo = new CultureInfo("en-US").TextInfo;

        public static void Init(DalamudPluginInterface plugin) {
            switch (plugin.ClientState.ClientLanguage) {
                case ClientLanguage.English:
                    _textInfo = new CultureInfo("en-US").TextInfo;
                    break;
                case ClientLanguage.German:
                    _textInfo = new CultureInfo("de-DE").TextInfo;
                    break;
                case ClientLanguage.French:
                    _textInfo = new CultureInfo("fr-FR").TextInfo;
                    break;
                default:
                    _textInfo = null;
                    break;
            }
            InitEventNpcData(plugin);
            InitBattleNpcData(plugin);
        }
        
        private static void InitEventNpcData(DalamudPluginInterface plugin) {
            _eventNpcNames.Clear();
            var sheet = plugin.Data.GetExcelSheet<ENpcResident>();
            foreach (var npc in sheet) {
                var name = npc.Singular.ToString();
                if (string.IsNullOrEmpty(name)) continue;
                _eventNpcNames[npc.RowId] = ToTitleCase(name);
            }
        }

        private static void InitBattleNpcData(DalamudPluginInterface plugin) {
            _battleNpcNames.Clear();
            var sheet = plugin.Data.GetExcelSheet<BNpcName>();
            foreach (var npc in sheet) {
                var name = npc.Singular.ToString();
                if (string.IsNullOrEmpty(name)) continue;
                _battleNpcNames[npc.RowId] = ToTitleCase(npc.Singular.ToString());
            }
        }

        private static string ToTitleCase(string text) {
            return _textInfo == null || string.IsNullOrEmpty(text) ? text : _textInfo.ToTitleCase(text);
        }
        
        public static string GetEventNpcName(uint id) {
            return _eventNpcNames.ContainsKey(id) ? _eventNpcNames[id] : string.Empty;
        }

        public static string GetBattleNpcName(uint id) {
            return _battleNpcNames.ContainsKey(id) ? _battleNpcNames[id] : string.Empty;
        }
    }
}