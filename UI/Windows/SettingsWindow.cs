using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using static MalisDamageMeter.MainWindow;

namespace MalisDamageMeter
{
    public class SettingsWindow: AOSharpWindow
    {
        private Views _views;
        private List<SimpleChar> _cachedDynels = new List<SimpleChar>();

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
                    _views.AutoPet.SetAllGfx(SetEnabledTexture(Main.Window.ViewSettings.AutoAssignPets));
                    _views.AutoPet.Clicked = AutoAssignPetClick;
                }

                if (Window.FindView("AutoTimer", out _views.AutoTimer))
                {
                    _views.AutoTimer.SetAllGfx(SetEnabledTexture(Main.Window.ViewSettings.AutoToggleTimer));
                    _views.AutoTimer.Clicked = AutoStartTimerClick;
                }

                if (Window.FindView("LogMobs", out _views.LogMobs))
                {
                    _views.LogMobs.SetAllGfx(SetEnabledTexture(Main.Window.ViewSettings.AutoToggleTimer));
                    _views.LogMobs.Clicked = LogMobsClick;
                }

                if (Window.FindView("PetListRoot", out _views.PetListRoot))
                {
                    foreach (PlayerPet pet in Main.Window.ViewSettings.PlayerPetManager.PlayerPet)
                    {
                        PetView petView = new PetView(_views.PetListRoot, pet.PlayerName, pet.PetName);
                    }
                }

                if (Window.FindView("Help", out _views.Help))
                {
                    _views.Help.SetAllGfx(Textures.HelpButton);
                    _views.Help.Clicked = HelpClick;
                }

                if (Window.FindView("Close", out _views.Close))
                {
                    _views.Close.SetAllGfx(Textures.CloseButton);
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
            Main.Window.SettingsWindow = null;
            Window.Close();
            Midi.Play("Click");
        }

        private void AutoStartTimerClick(object sender, ButtonBase e)
        {
            Main.Window.ViewSettings.AutoToggleTimer = !Main.Window.ViewSettings.AutoToggleTimer;
            Main.Settings.Save();
            ((Button)e).SetAllGfx(SetEnabledTexture(Main.Window.ViewSettings.AutoToggleTimer));
            Midi.Play("Click");
        }

        private void AutoAssignPetClick(object sender, ButtonBase e)
        {
            Main.Window.ViewSettings.AutoAssignPets = !Main.Window.ViewSettings.AutoAssignPets;
            Main.Settings.Save();
            ((Button)e).SetAllGfx(SetEnabledTexture(Main.Window.ViewSettings.AutoAssignPets));
            Midi.Play("Click");
        }

        private void LogMobsClick(object sender, ButtonBase e)
        {
            Main.Window.ViewSettings.LogMobs = !Main.Window.ViewSettings.LogMobs;
            Main.Settings.Save();
            ((Button)e).SetAllGfx(SetEnabledTexture(Main.Window.ViewSettings.LogMobs));
            Midi.Play("Click");
        }

        private int SetEnabledTexture(bool enabled) => enabled ? Textures.GreenCircleButton : Textures.RedCircleButton;

        private void AssignPetClick(object sender, ButtonBase e)
        {
            string playerName = _views.PlayerSelectMenu.GetItemLabel(_views.PlayerSelectMenu.GetSelection());
            string petName = _views.PetSelectMenu.GetItemLabel(_views.PetSelectMenu.GetSelection());

            if (playerName == string.Empty || petName == string.Empty || playerName == "Players                " || petName == "Pets                              ")
            {
                Chat.WriteLine("Cannot add an empty entry.");
                return;
            }

            if (Main.Window.ViewSettings.PlayerPetManager.PlayerPet.Any(x => x.PetName == petName))
            {
                Chat.WriteLine("This pet name has already been assigned.");
                return;
            }

            if (Main.Window.ViewSettings.PlayerPetManager.PlayerPet.Any(x => x.PetName == petName && x.PlayerName == playerName))
            {
                Chat.WriteLine("Your pet list already contains this entry.");
                return;
            }

            PetView petView = new PetView(_views.PetListRoot, playerName, petName);
            Main.Window.ViewSettings.PlayerPetManager.PlayerPet.Add(new PlayerPet { PetName = petName, PlayerName = playerName, PlayerId = _cachedDynels.FirstOrDefault(x => x.Name == playerName).Identity.Instance });
            Main.Settings.Save();
            Midi.Play("Click");
        }

        public void Update()
        {
            _cachedDynels = new List<SimpleChar>();
            // foreach (var dynel in DynelManager.Characters.OrderBy(x => x.Name).Distinct())
            foreach (var dynel in DynelManager.Characters.Where(x => x.Identity != DynelManager.LocalPlayer.Identity).OrderBy(x => x.Name).Distinct())
            {
                if (dynel.IsPlayer)
                {
                    _views.PlayerSelectMenu.AppendItem(dynel.Name);
                    _cachedDynels.Add(dynel);
                }
                else if (dynel.IsPet && !DynelManager.LocalPlayer.IsPetOwner(dynel.Identity.Instance)) 
                {
                    if (Main.Window.ViewSettings.PlayerPetManager.PlayerPet.FirstOrDefault(x => x.PetName == dynel.Name) != null)
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
            public Button LogMobs;
            public BitmapView Background;
        }   
    }
}