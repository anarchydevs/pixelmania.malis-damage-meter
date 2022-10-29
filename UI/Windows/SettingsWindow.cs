using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Misc;
using AOSharp.Core.UI;
using Newtonsoft.Json;
using SmokeLounge.AOtomation.Messaging.GameData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AOSharp.Common.Unmanaged.DataTypes;
using AOSharp.Common.Unmanaged.Imports;

namespace MalisDamageMeter
{
    public class SettingsWindow: AOSharpWindow
    {
        private static Views _views;
        private static List<SimpleChar> _cachedDynels = new List<SimpleChar>();

        public SettingsWindow(string name, string path, WindowStyle windowStyle = WindowStyle.Popup, WindowFlags flags = WindowFlags.AutoScale | WindowFlags.NoFade) : base(name, path, windowStyle, flags)
        {
            _views = new Views();
        }

        protected override void OnWindowCreating()
        {
            try
            {
                if (Window.FindView("Background", out _views.Background))
                {
                    _views.Background.SetBitmap("SettingsBackground");
                }

                if (Window.FindView("PlayerSelectMenu", out _views.PlayerSelectMenu))
                {
                    _views.PlayerSelectMenu.AppendItem("Players                ");
                }

                if (Window.FindView("PetSelectMenu", out _views.PetSelectMenu))
                {
                    _views.PetSelectMenu.AppendItem("Pets                              ");
                }

                if (Window.FindView("AssignPet", out _views.RegisterPet))
                {
                    _views.RegisterPet.SetAllGfx(1430051);
                    _views.RegisterPet.Clicked = AssignPetClick;
                }

                if (Window.FindView("AutoPet", out _views.AutoPet))
                {
                    _views.AutoPet.Tag = Main.Settings.AutoAssignPets;
                    int texId = Main.Settings.AutoAssignPets ? 1430046 : 1430047;
                    _views.AutoPet.SetAllGfx(texId);
                    _views.AutoPet.Clicked = AutoPetClick;
                }

                if (Window.FindView("AutoTimer", out _views.AutoTimer))
                {
                    _views.AutoTimer.Tag = Main.Settings.AutoToggleTimer;
                    int texId = Main.Settings.AutoToggleTimer ? 1430046 : 1430047;
                    _views.AutoTimer.SetAllGfx(texId);
                    _views.AutoTimer.Clicked = AutoTimerClick;
                }

                if (Window.FindView("PetListRoot", out _views.PetListRoot))
                {
                    foreach (PlayerPet pet in Main.Settings.PetList.ToList())
                    {
                        PetView petView = new PetView(_views.PetListRoot, pet.PlayerName, pet.PetName);
                    }
                }

                if (Window.FindView("Help", out _views.Help))
                {
                    _views.Help.SetAllGfx(1430054);
                    _views.Help.Clicked = HelpClick;
                }

                if (Window.FindView("Close", out _views.Close))
                {
                    _views.Close.SetAllGfx(1430049);
                    _views.Close.Clicked = CloseClick;
                }
            }
            catch (Exception e)
            {
                Chat.WriteLine(e);
            }
        }

        private void HelpClick(object sender, ButtonBase e)
        {
            HelpWindow helpWindow = new HelpWindow();
            helpWindow.Window.Show(true);
            Midi.Play("Click");
        }

        private void CloseClick(object sender, ButtonBase e)
        {
            Main.UI.SettingsWindow = null;
            Window.Close();
            Midi.Play("Click");
        }

        private void AutoTimerClick(object sender, ButtonBase e)
        {
            bool buttonToggle = !(bool)e.Tag;
            e.Tag = buttonToggle; 
            Main.Settings.AutoToggleTimer = buttonToggle;
            Main.Settings.Save();

            int texId = buttonToggle ? 1430046 : 1430047;
            ((Button)e).SetAllGfx(texId);
            Midi.Play("Click");
        }

        private void AutoPetClick(object sender, ButtonBase e)
        {
            bool buttonToggle = !(bool)e.Tag;
            e.Tag = buttonToggle;
            Main.Settings.AutoAssignPets = buttonToggle;
            Main.Settings.Save();

            int texId = buttonToggle ? 1430046 : 1430047;
            ((Button)e).SetAllGfx(texId);
            Midi.Play("Click");
        }

        private void AssignPetClick(object sender, ButtonBase e)
        {
            string playerName = _views.PlayerSelectMenu.GetItemLabel(_views.PlayerSelectMenu.GetSelection());
            string petName = _views.PetSelectMenu.GetItemLabel(_views.PetSelectMenu.GetSelection());

            if (playerName == string.Empty || petName == string.Empty || playerName == "Players                " || petName == "Pets                              ")
            {
                Chat.WriteLine("Cannot add an empty entry.");
                return;
            }

            if (Main.Settings.PetList.Any(x => x.PetName == petName))
            {
                Chat.WriteLine("This pet name has already been assigned.");
                return;
            }

            if (Main.Settings.PetList.Any(x => x.PetName == petName && x.PlayerName == playerName))
            {
                Chat.WriteLine("Your pet list already contains this entry.");
                return;
            }

            PetView petView = new PetView(_views.PetListRoot, playerName, petName);
            Main.Settings.PetList.Add(new PlayerPet { PetName = petName, PlayerName = playerName, PlayerId = _cachedDynels.FirstOrDefault(x => x.Name == playerName).Identity.Instance });
            Main.Settings.Save();
            Midi.Play("Click");
        }

        public void Update()
        {
            _cachedDynels = new List<SimpleChar>();

            foreach (var dynel in DynelManager.Characters.Where(x => x.Identity != DynelManager.LocalPlayer.Identity).OrderBy(x => x.Name).Distinct())
            {
                if (dynel.IsPlayer)
                {
                    _views.PlayerSelectMenu.AppendItem(dynel.Name);
                    _cachedDynels.Add(dynel);
                }
                else if (dynel.IsPet)
                {
                    if (Main.Settings.PetList.FirstOrDefault(x => x.PetName == dynel.Name) != null)
                        continue;

                    _views.PetSelectMenu.AppendItem(dynel.Name);
                }
            }
        }

        internal class Views
        {
            public DropdownMenu PlayerSelectMenu;
            public DropdownMenu PetSelectMenu;
            public Button Help;
            public Button Close;
            public View PetListRoot;
            public Button RegisterPet;
            public Button AutoPet;
            public Button AutoTimer;
            public BitmapView Background;
        }   
    }
}