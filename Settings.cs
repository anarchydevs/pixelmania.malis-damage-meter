using AOSharp.Common.GameData;
using AOSharp.Core.UI;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MalisDamageMeter
{
    public class Settings
    {
        public Vector2 Frame;
        public bool AutoToggleTimer;
        public bool LogMobs;
        public bool TotalValues;
        public bool ShowTutorial;
        public ScopeEnum Scope;

        public void Save()
        {
            AutoToggleTimer = Main.Window.ViewSettings.AutoToggleTimer;
            LogMobs = Main.Window.ViewSettings.LogMobs;
            TotalValues = Main.Window.ViewSettings.TotalValues;
            Frame.X = Main.Window.Window.GetFrame().MinX;
            Frame.Y = Main.Window.Window.GetFrame().MinY;

            if (ShowTutorial)
                ShowTutorial = false;

            File.WriteAllText($"{Main.PluginDir}\\JSON\\Settings.json", JsonConvert.SerializeObject(this));
        }

        public static Settings Load(string path)
        {
            try
            {
                return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(path));
            }
            catch
            {
                Chat.WriteLine($"Config file can't be loaded.");
                return null;
            }
        }
    }
}