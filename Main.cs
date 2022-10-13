using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using Newtonsoft.Json;
using SmokeLounge.AOtomation.Messaging.GameData;
using SmokeLounge.AOtomation.Messaging.Messages;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MalisDamageMeter
{
    public class Main : AOPluginEntry
    {
        public static string PluginDir;
        public static UI UI;
        public static HitRegisters HitRegisters;
        public static Settings Settings;

        public unsafe override void Run(string pluginDir)
        {
            Chat.WriteLine("- Mali's Damage Meter -", ChatColor.Gold);

            PluginDir = pluginDir;
            Settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText($"{pluginDir}\\JSON\\Settings.json"));

            UI = new UI("MalisDmgMeter", $"{pluginDir}\\UI\\Windows\\MainWindow.xml");
            UI.Show();
            UI.MoveWindow();

            if (Settings.ShowTutorial)
            {
                HelpWindow _helpWindow = new HelpWindow();

                _helpWindow.Window.MoveToCenter();
                _helpWindow.Window.Show(true);
            }

            HitRegisters = new HitRegisters();
            Midi.Play("Alert");

            Game.OnUpdate += Update;
            Network.N3MessageReceived += N3MessageReceived;
        }

        private void N3MessageReceived(object sender, N3Message n3Msg)
        {
            HitRegisters.N3MessageCallback(n3Msg);
        }

        private void Update(object sender, float e)
        {
            UI.Update(e);
        }

        public override void Teardown()
        {
            Midi.TearDown();
            Settings.Save();
        }
    }
}