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
        public bool AutoAssignPets;
        public bool AutoToggleTimer;
        public bool ShowTutorial;
        public Scope Scope;

        public List<PlayerPet> PetList = new List<PlayerPet>();

        public Settings()
        {
        }

        public void Save()
        {
            Frame.X = Main.UI.Window.GetFrame().MinX;
            Frame.Y = Main.UI.Window.GetFrame().MinY;

            if (ShowTutorial)
                ShowTutorial = false;

            File.WriteAllText($"{Main.PluginDir}\\JSON\\Settings.json", JsonConvert.SerializeObject(this));
        }
    }
}