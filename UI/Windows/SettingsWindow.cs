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

                if (Window.FindView("AutoTimer", out _views.AutoTimer))
                {
                    _views.AutoTimer.SetAllGfx(SetEnabledTexture(Main.Window.ViewSettings.AutoToggleTimer));
                    _views.AutoTimer.Clicked = AutoStartTimerClick;
                }

                if (Window.FindView("LogMobs", out _views.LogMobs))
                {
                    _views.LogMobs.SetAllGfx(SetEnabledTexture(Main.Window.ViewSettings.LogMobs));
                    _views.LogMobs.Clicked = LogMobsClick;
                }

                if (Window.FindView("TotalValues", out _views.TotalValues))
                {
                    _views.TotalValues.SetAllGfx(SetEnabledTexture(Main.Window.ViewSettings.TotalValues));
                    _views.TotalValues.Clicked = TotalValuesClick;
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
            if (Main.Window.HelpWindow != null && Main.Window.HelpWindow.Window.IsValid && Main.Window.HelpWindow.Window.IsVisible)
                return;

            Main.Window.HelpWindow = new HelpWindow();
            Main.Window.HelpWindow.Window.Show(true);
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

        private void LogMobsClick(object sender, ButtonBase e)
        {
            Main.Window.ViewSettings.LogMobs = !Main.Window.ViewSettings.LogMobs;
            Main.Settings.Save();
            ((Button)e).SetAllGfx(SetEnabledTexture(Main.Window.ViewSettings.LogMobs));
            Midi.Play("Click");
        }

        private void TotalValuesClick(object sender, ButtonBase e)
        {
            Main.Window.ViewSettings.TotalValues = !Main.Window.ViewSettings.TotalValues;
            Main.Window.ViewCache.TotalDisplayView.Hide();
            Main.Settings.Save();
            ((Button)e).SetAllGfx(SetEnabledTexture(Main.Window.ViewSettings.TotalValues));
            Midi.Play("Click");
        }

        private int SetEnabledTexture(bool enabled) => enabled ? Textures.GreenCircleButton : Textures.RedCircleButton;

        internal class Views
        {
            public Button Help;
            public Button Close;
            public Button AutoTimer;
            public Button LogMobs;
            public Button TotalValues;
            public BitmapView Background;
        }   
    }
}