using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;

namespace MalisDamageMeter
{
    public class Main : AOPluginEntry
    {
        public static string PluginDir;
        public static Settings Settings;
        public static DamageMeterWindow Window;
        public static N3MessageCallbacks N3MessageCallbacks;

        public override void Run(string pluginDir)
        {
            Chat.WriteLine("- Mali's Damage Meter -", ChatColor.Gold);

            PluginDir = pluginDir;

            Settings = Settings.Load($"{PluginDir}\\JSON\\Settings.json");

            Window = new DamageMeterWindow("MalisDmgMeter", $"{pluginDir}\\UI\\Windows\\MainWindow.xml", 1430035);
            Window.Show();

            N3MessageCallbacks = new N3MessageCallbacks();

            Game.OnUpdate += Window.Update;
            Network.N3MessageReceived += N3MessageCallbacks.N3MessageCallback;

            Utils.SetScriptMaxFileSize(16384);
        }

        public override void Teardown()
        {
            Midi.TearDown();
            Utils.SetScriptMaxFileSize(4096);
            Settings.Save();
        }
    }
}