using AOSharp.Common.GameData;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MalisDamageMeter
{
    public class PetView
    {
        public View Root;
        public string PetName;
        public string PlayerName;
        private TextView _petView;
        private TextView _playerView;
        private Button _remove;
        private View _petListRoot;

        public PetView(View petListRoot, string playerName, string petName)
        {
            Root = View.CreateFromXml($"{Main.PluginDir}\\UI\\Views\\PetView.xml");
            _petListRoot = petListRoot;

            if (Root.FindChild("PlayerName", out _playerView))
            {
                PlayerName = playerName;
                _playerView.Text = PlayerName;
            }

            if (Root.FindChild("PetName", out _petView))
            {
                PetName = petName;
                _petView.Text = PetName;
            }

            if (Root.FindChild("Remove", out _remove))
            {
                _remove.SetAllGfx(1430050);
                _remove.Clicked = RemoveClick;
            }

            petListRoot.AddChild(Root, true);
            petListRoot.FitToContents();
        }

        private void RemoveClick(object sender, ButtonBase e)
        {
            _petListRoot.RemoveChild(Root);
            _petListRoot.FitToContents();
            Main.Settings.PetList.Remove(Main.Settings.PetList.FirstOrDefault(x => x.PlayerName == PlayerName && x.PetName == PetName));
            Main.Settings.Save();
            Midi.Play("Click");
        }
    }
}