using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.Misc;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace MalisDamageMeter
{
    public class DamageMeterWindow : MainWindow
    {
        private DisplayConfig _displayConfig;

        public DamageMeterWindow(string name, string path, int textureStartId, WindowStyle windowStyle = WindowStyle.Popup, WindowFlags flags = WindowFlags.AutoScale | WindowFlags.NoFade) : base(name, path, windowStyle, flags)
        {
            Utils.LoadCustomTextures($"{Main.PluginDir}\\UI\\Textures\\", textureStartId);

            if (Main.Settings.ShowTutorial)
            {
                HelpWindow _helpWindow = new HelpWindow();
                _helpWindow.Window.MoveToCenter();
                _helpWindow.Window.Show(true);
            }
        }

        protected override void OnWindowCreating()
        {
            try
            {
                LoadSettings();

                if (Window.FindView("Background", out _views.Background))
                {
                    _views.Background.SetBitmap("Background");
                }

                if (Window.FindView("Icon", out _views.Icon))
                {
                    _views.Icon.SetBitmap("HeaderIcon");
                }

                if (Window.FindView("ResumePause", out _views.ResumePauseButton))
                {
                    _views.ResumePauseButton.SetAllGfx(Textures.StartButton);
                    _views.ResumePauseButton.Clicked = PauseClick;
                }

                if (Window.FindView("Reset", out _views.ResetButton))
                {
                    _views.ResetButton.SetAllGfx(Textures.ResetButton);
                    _views.ResetButton.Clicked = ResetClick;
                }

                if (Window.FindView("Mode", out _views.ModeButton))
                {
                    _views.ModeButton.SetAllGfx(Textures.ModeButton);
                    _views.ModeButton.Clicked = ModeClick;
                }

                if (Window.FindView("Log", out _views.LogButton))
                {
                    _views.LogButton.SetAllGfx(Textures.LogButton);
                    _views.LogButton.Clicked = LogClick;
                }


                if (Window.FindView("Scope", out _views.ScopeButton))
                {
                    int modeGfx = ViewSettings.Scope.SetIcon();
                    _views.ScopeButton.SetAllGfx(modeGfx);
                    _views.ScopeButton.Clicked = ScopeClick;
                }

                if (Window.FindView("Settings", out _views.SettingsButton))
                {
                    _views.SettingsButton.SetAllGfx(Textures.SettingsButton);
                    _views.SettingsButton.Clicked = SettingsClick;
                }

                if (Window.FindView("ModeText", out _views.ModeText))
                {
                    _views.ModeText.Text = "Damage";
                }

                if (Window.FindView("Elapsed", out _views.Elapsed)) { }

                if (Window.FindView("Meters", out _views.MetersRoot)) { }

                SetMeterDefaults();
            }
            catch (Exception e)
            {
                Chat.WriteLine(e);
            }
        }

        private void LogClick(object sender, ButtonBase e)
        {
            SaveAllDumps();
        }

        public void Update(object sender, float deltaTime)
        {
            if (Main.Settings.AutoToggleTimer && ViewSettings.ToggleDelayTimer.Elapsed)
            {
                if (ViewSettings.IsPaused && ViewSettings.Scope.IsInCombat || !ViewSettings.IsPaused && ViewSettings.Scope.IsNotInCombat)
                {
                    PauseAction();
                }
            }

            if (!ViewSettings.IsPaused)
            {
                ViewSettings.ElapsedTime += deltaTime;
                UpdateMainWindow();
            }
        }

        private void UpdateMainWindow()
        {
            if (ViewSettings.ResetTimer.Elapsed)
            {
                _views.Elapsed.Text = Format.Time(ViewSettings.ElapsedTime);
                UpdateMeterViews();
            }
        }

        public void UpdateMeterCount()
        {
            MeterView meterView = new MeterView();
            _views.MetersRoot.AddChild(meterView.Root, true);
            MeterViews.Add(meterView);
            _views.MetersRoot.FitToContents();
        }

        public void UpdateMeterViews()
        {
            if (HitRegisters.Characters.Count == 0)
                return;

            _displayConfig = HitRegisters.GetDisplayConfig(ViewSettings.Mode);

            if (MeterViews.Count != _displayConfig.SimpleCharMeterData.Count)
                MeterViews.Redraw(_views.MetersRoot, _displayConfig.SimpleCharMeterData.Count);

            if (_displayConfig.SimpleCharMeterData.Count == 0)
                return;

            float highestValue = _displayConfig.SimpleCharMeterData[0].Total;

            for (int i = 0; i < MeterViews.Count; i++)
            {
                var simpleCharMeterData = _displayConfig.SimpleCharMeterData[i];
                MeterViews[i].InitMeterData(simpleCharMeterData, highestValue);

                MeterViews[i].LeftTextView.Text = $"{i + 1}. {simpleCharMeterData.SimpleCharData.Name}";
                MeterViews[i].RightTextView.Text = $"{Format.TotalDmgFormat(simpleCharMeterData.Total)} " +
                $"({Format.DpmFormat(simpleCharMeterData.Total, ViewSettings.ElapsedTime)}, " +
                $" {Format.PercentFormat((float)simpleCharMeterData.Total / _displayConfig.TotalAmount)})";
            }
        }

        private void PauseClick(object sender, ButtonBase e)
        {
            PauseAction();
            Midi.Play("Click");
        }

        private void ResetClick(object sender, ButtonBase e)
        {
            SetMeterDefaults();
            Midi.Play("Click");
        }

        private void ScopeClick(object sender, ButtonBase e)
        {
            ScopeAction();
            Midi.Play("Click");
        }

        private void ModeClick(object sender, ButtonBase e)
        {
            var modeView = ViewSettings.Mode.GetNext();
            ViewSettings.Mode.Current = modeView.Key;
            _views.ModeText.Text = modeView.Value;

            UpdateMeterViews();

            foreach (MeterView s in MeterViews)
                s.ResetMeter();

            Midi.Play("Click");
        }

        private void SettingsClick(object sender, ButtonBase e)
        {
            if (SettingsWindow != null && SettingsWindow.Window.IsVisible)
                return;

            SettingsWindow = new SettingsWindow("MdmSettings", $"{Main.PluginDir}\\UI\\Windows\\SettingsWindow.xml");
            SettingsWindow.Show();
            SettingsWindow.Update();
            Midi.Play("Click");
        }

        private void PauseAction()
        {
            int texId = ViewSettings.IsPaused ? Textures.PauseButton : Textures.StartButton;
            _views.ResumePauseButton.SetAllGfx(texId);
            ViewSettings.IsPaused = !ViewSettings.IsPaused;
            UpdateMainWindow();
        }

        private void SetMeterDefaults()
        {
            HitRegisters.Characters.Clear();
            HitRegisters.Pets.Clear();

            _views.Elapsed.Text = "0:00:00:0";
            ViewSettings.ElapsedTime = 0;

            foreach (var s in MeterViews)
                _views.MetersRoot.RemoveChild(s.Root);

            MeterViews.Clear();
            _views.MetersRoot.FitToContents();
        }

        private void ScopeAction()
        {
            ViewSettings.Scope.Next();
            _views.ScopeButton.SetAllGfx(ViewSettings.Scope.SetIcon());
            Main.Settings.Scope = ViewSettings.Scope.Current;
            Main.Settings.Save();
            Chat.WriteLine($"Current Scope set to: {ViewSettings.Scope.Current}");
        }

        public new void Show()
        {
            base.Show();
            if (Main.Settings.Frame.X == 0 && Main.Settings.Frame.Y == 0)
                Window.MoveToCenter();
            else
                Window.MoveTo(Main.Settings.Frame.X, Main.Settings.Frame.Y);
        }

        public void Dump()
        {
            if (_displayConfig == null)
                return;

            string dumpText = Format.DumpDmgFormatBasic(ViewSettings.ElapsedTime);

            if (dumpText != "")
                Chat.SendVicinityMessage(dumpText, VicinityMessageType.Shout);
        }

        private void SaveAllDumps()
        {
            if (HitRegisters.Characters.Count == 0)
            {
                Chat.WriteLine("Error: No registered characters. Log not saved.", ChatColor.Red);
                return;
            }

            if (_displayConfig == null)
                return;

            Chat.WriteLine("Preview Logs:");

            var basicDmgDump = Format.DumpDmgFormatBasic(ViewSettings.ElapsedTime);

            Chat.WriteLine(basicDmgDump);
            File.WriteAllText($"{Utils.FindScriptFolder()}\\mdmb_d", basicDmgDump);

            var basicHealDump = Format.DumpHealingFormatBasic(ViewSettings.ElapsedTime);
           
            Chat.WriteLine(basicHealDump);
            File.WriteAllText($"{Utils.FindScriptFolder()}\\mdmb_h", basicHealDump);

            foreach (var simpleCharData in HitRegisters.Characters.Values.OrderBy(x => x.Name))
            {
                if (simpleCharData.DamageSources.Total == 0 && simpleCharData.HealSource.Total == 0)
                    continue;

                var advDump = Format.DumpDmgFormatAdvanced(simpleCharData, ViewSettings.ElapsedTime);
                Chat.WriteLine(advDump);
                File.WriteAllText($"{Utils.FindScriptFolder()}\\mdma_{simpleCharData.Name.ToLower()}", advDump);
            }

            Chat.WriteLine("Logs saved to script folder.", ChatColor.Green);
        }

        private void LoadSettings()
        {
            ViewSettings.IsPaused = true;
            ViewSettings.Mode.Current = ModeEnum.Damage;
            ViewSettings.Scope.Current = Main.Settings.Scope;
            ViewSettings.PlayerPetManager.PlayerPet = Main.Settings.PetList;
            ViewSettings.AutoAssignPets = Main.Settings.AutoAssignPets;
            ViewSettings.AutoToggleTimer = Main.Settings.AutoToggleTimer;
            ViewSettings.Scope.Current = Main.Settings.Scope;
            ViewSettings.PlayerPetManager.PlayerPet = Main.Settings.PetList;
        }
    }
}