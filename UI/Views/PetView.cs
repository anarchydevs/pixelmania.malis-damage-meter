using AOSharp.Common.GameData;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using static MalisDamageMeter.MainWindow;

namespace MalisDamageMeter
{
    public class PetView
    {
        public View Root;
        public string PetName;
        public string PlayerName;
        private View _petListRoot;

        public PetView(View petListRoot, string playerName, string petName)
        {
            Root = View.CreateFromXml($"{Main.PluginDir}\\UI\\Views\\PetView.xml");
            _petListRoot = petListRoot;

            if (Root.FindChild("PlayerName", out TextView playerView))
            {
                PlayerName = playerName;
                playerView.Text = PlayerName;
            }

            if (Root.FindChild("PetName", out TextView petView))
            {
                PetName = petName;
                petView.Text = PetName;
            }

            if (Root.FindChild("Remove", out Button remove))
            {
                remove.SetAllGfx(Textures.RedMinusButton);
                remove.Clicked = RemoveClick;
            }

            petListRoot.AddChild(Root, true);
            petListRoot.FitToContents();
        }

        private void RemoveClick(object sender, ButtonBase e)
        {
            _petListRoot.RemoveChild(Root);
            Root.Dispose();
            _petListRoot.FitToContents();
            Main.Window.ViewSettings.PlayerPetManager.PlayerPet.Remove(Main.Window.ViewSettings.PlayerPetManager.PlayerPet.FirstOrDefault(x => x.PlayerName == PlayerName && x.PetName == PetName));
            Main.Settings.PetList = Main.Window.ViewSettings.PlayerPetManager.PlayerPet;
            Main.Settings.Save();
            Midi.Play("Click");
        }
    }
}