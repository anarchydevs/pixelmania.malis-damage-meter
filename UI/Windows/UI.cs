using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.Misc;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace MalisDamageMeter
{
    public class UI: AOSharpWindow
    {
        public static List<MeterView> MeterViews = new List<MeterView>();
        public Scope CurrentScope;
        public Mode CurrentMode;
        public bool IsPaused;
        private static Views _views;
        private static double _elapsedTime;
        private AutoResetInterval _resetTimer;
        public SettingsWindow SettingsWindow;
        public class Colors
        {
            public const string Title = "b9ff00";
            public const string Accent = "b34c5e";
            public const string Bonus = "b36e4c";
            public const string Info = "498ab6";
            public const string Name = "eec911";
        }

        public UI(string name, string path, WindowStyle windowStyle = WindowStyle.Popup, WindowFlags flags = WindowFlags.AutoScale | WindowFlags.NoFade) : base(name, path, windowStyle, flags)
        {
            _views = new Views();
            _resetTimer = new AutoResetInterval(100);
            Utils.LoadCustomTextures($"{Main.PluginDir}\\UI\\Textures\\", 1430035);

            Chat.RegisterCommand("mdmb", (string command, string[] param, ChatWindow chatWindow) =>
            {
                string dumpText = DumpDmgFormatBasic();

                if (dumpText != "")
                    Chat.SendVicinityMessage(dumpText);
            });

            Chat.RegisterCommand("mdma", (string command, string[] param, ChatWindow chatWindow) =>
            {
                if (Targeting.TargetChar == null)
                {
                    Chat.WriteLine("Please select a target.");
                    return;
                }

                string dumpText = DumpDmgFormatAdvanced();

                if (dumpText != "")
                    Chat.SendVicinityMessage(dumpText);
            });
        }

        protected override void OnWindowCreating()
        {
            try
            {
                if (Window.FindView("Background", out _views.Background))
                {
                    _views.Background.SetBitmap("Background");
                }

                if (Window.FindView("Icon", out _views.Icon))
                {
                    _views.Icon.SetBitmap("HeaderIcon");
                }

                if (Window.FindView("Pause", out _views.Pause))
                {
                    _views.Pause.SetAllGfx(1430036);
                    IsPaused = true;
                    _views.Pause.Clicked = PauseClick;
                }

                if (Window.FindView("Reset", out _views.Reset))
                {
                    _views.Reset.SetAllGfx(1430037);
                    _views.Reset.Clicked = ResetClick;
                }

                if (Window.FindView("Mode", out _views.Mode))
                {
                    _views.Mode.SetAllGfx(1430038);
                    CurrentMode = Mode.Damage;
                    _views.Mode.Clicked = ModeClick;
                }

                if (Window.FindView("Scope", out _views.Scope))
                {
                    CurrentScope = Main.Settings.Scope;
                    int modeGfx = CurrentScope == Scope.Solo ? 1430043 : CurrentScope == Scope.Team ? 1430044 : 1430045;
                    _views.Scope.SetAllGfx(modeGfx);
                    _views.Scope.Clicked = ScopeClick;
                }

                if (Window.FindView("Settings", out _views.Settings))
                {
                    _views.Settings.SetAllGfx(1430039);
                    _views.Settings.Clicked = SettingsClick;
                }

                if (Window.FindView("ModeText", out _views.ModeText))
                {
                    _views.ModeText.Text = "All Damage";
                }

                if (Window.FindView("Elapsed", out _views.Elapsed))
                {
                }

                if (Window.FindView("Meters", out _views.MetersRoot))
                {
                }
            }
            catch (Exception e)
            {
                Chat.WriteLine(e);
            }
        }

        public void Update(float deltaTime)
        {
            if (Main.Settings.AutoToggleTimer)
            {
                if (IsPaused)
                {
                    if (CurrentScope == Scope.Solo)
                    {
                        if (DynelManager.LocalPlayer.IsAttacking)
                            PauseToggle();
                    }
                    else if (CurrentScope == Scope.Team)
                    {
                        if (Team.Members.FirstOrDefault(x => x.Character.IsAttacking) != null)
                            PauseToggle();
                    }
                    else if (CurrentScope == Scope.All)
                    {
                        if (DynelManager.Players.FirstOrDefault(x => x.IsAttacking) != null)
                            PauseToggle();
                    }

                }
                else
                {
                    if (CurrentScope == Scope.Solo)
                    {
                        if (!DynelManager.LocalPlayer.IsAttacking)
                            PauseToggle();
                    }
                    else if (CurrentScope == Scope.Team)
                    {
                        if (Team.Members.FirstOrDefault(x => x.Character.IsAttacking) == null)
                            PauseToggle();
                    }
                    else if (CurrentScope == Scope.All)
                    {
                        if (DynelManager.Players.FirstOrDefault(x => x.IsAttacking) == null)
                            PauseToggle();
                    }
                }

            }

            if (IsPaused)
                return;

            _elapsedTime += deltaTime;

            if (!_resetTimer.Elapsed)
                return;

            _views.Elapsed.Text = string.Format("{0:0.0}", _elapsedTime);

            UpdateMeterViews();
        }

        private static string TotalDmgFormat(int totalDamage)
        {
            if (totalDamage < 100000)
                return totalDamage.ToString();
            else if (totalDamage < 1000000)
                return string.Format("{0:0.00}", (double)totalDamage / 1000) + "K";
            else
                return string.Format("{0:0.00}", (double)totalDamage / 1000000) + "M";
        }

        public string DumpDmgFormatBasic()
        {
            string dump = "";
            int counter = 0;
            var displayConfig = CurrentDisplayConfig();

            foreach (var charData in displayConfig.OrderedCharData)
            {
                var dmgDisplayData = displayConfig.DamageDisplayData.Skip(counter++).FirstOrDefault();

                dump += $@"{counter}.) <font color='#{Colors.Name}'>{charData.Value.Name}</font> <font color='#{Colors.Accent}'>({charData.Value.Profession})</font> <font color='#{Colors.Info}'>Total {CurrentMode}:</font> {TotalDmgFormat(dmgDisplayData)} <font color='#{Colors.Accent}'>|</font> <font color='#{Colors.Info}'>{CurrentMode} Per Minute:</font> {DpmFormat(dmgDisplayData)} <font color='#{Colors.Accent}'>|</font> <font color='#{Colors.Info}'>Total Percent:</font> {PercentFormat((float)dmgDisplayData / displayConfig.TotalAmount)}<br>";
            }

            string fullLog = $@"<a href=""text://<font color='#{Colors.Title}'>Total {CurrentMode}:</font> {displayConfig.TotalAmount}<br><font color='#{Colors.Title}'>Duration:</font> {string.Format("{0:0.0}", _elapsedTime)}s<br><br>" + $"{dump}" + $@""" >{CurrentMode} Dump Basic ({CurrentScope})</a>";

            return fullLog;
        }


        public string DumpDmgFormatAdvanced()
        {
            if (Targeting.TargetChar == null || !HitRegisters.CharData.ContainsKey(Targeting.TargetChar.Identity.Instance))
                return "";

            var charData = HitRegisters.CharData[Targeting.TargetChar.Identity.Instance];
            string coreDump = $@"{FormatHeader(charData)}<br>{FormatWeaponInfo(charData)}<br>{FormatDamageInfo(charData)}<br>{FormatHealingInfo(charData)}<br>{FormatAbsorbInfo(charData)}<br>{FormatHitInfo(charData)}";
            string fullLog = $@"<a href=""text://<font color='#{Colors.Title}'>Total Damage:</font> {HitRegisters.TotalDamage.AllDamage}<br><font color='#{Colors.Title}'>Total Healing:</font> {HitRegisters.TotalDamage.Healing}<br><font color='#{Colors.Title}'>Total Absorbed:</font> {HitRegisters.TotalDamage.Absorb}<br><font color='#{Colors.Title}'>Duration:</font> {string.Format("{0:0.0}", _elapsedTime)}s</font><br><br>" + $"{coreDump}" + $@""" >{Targeting.TargetChar.Name} Dump (Advanced)</a>";

            return fullLog;
        }


        private string FormatDamageInfo(CharData charData)
        {
            string damageInfo = $"<font color='#{Colors.Info}'>Damage Per Minute:</font> {DpmFormat(charData.TotalDamage.AllDamage)}<br><font color='#{Colors.Info}'>Total Percent:</font> {PercentFormat((float)charData.TotalDamage.AllDamage / HitRegisters.TotalDamage.AllDamage)}<br><font color='#{Colors.Info}'>Total Damage:</font> {TotalDmgFormat(charData.TotalDamage.AllDamage)}<br>";
            string damagePerType = "";

            foreach (var damageAmount in new Dictionary<Stat, int>().TotalDamagePerStat(charData))
                damagePerType += $"<font color='#{Colors.Info}'> {damageAmount.Key}:</font> {damageAmount.Value}<font color='#{Colors.Accent}'> - W:</font> <font color='#{Colors.Bonus}'>{charData.WeaponDamage[damageAmount.Key]}</font> <font color='#{Colors.Accent}'>N:</font> <font color='#{Colors.Bonus}'>{charData.NanoDamage[damageAmount.Key]}</font> <font color='#{Colors.Accent}'>P:</font> <font color='#{Colors.Bonus}'>{charData.PetDamage[damageAmount.Key]}</font> <br>";

            damagePerType += $"<font color='#{Colors.Info}'> Reflect/Shield:</font> {charData.TotalDamage.Reflect}<br>";
            damageInfo += damagePerType;

            return damageInfo;
        }

        private string FormatHealingInfo(CharData charData)
        {
            string healingInfo = $"<font color='#{Colors.Info}'>Healing Per Minute:</font> {DpmFormat(charData.TotalDamage.Healing)}<br><font color='#{Colors.Info}'>Total Percent:</font> {PercentFormat((float)charData.TotalDamage.Healing / HitRegisters.TotalDamage.Healing)}<br><font color='#{Colors.Info}'>Total Healing:</font> {TotalDmgFormat(charData.TotalDamage.Healing)}<br>";
            return healingInfo;
        }

        private string FormatWeaponInfo(CharData charData)
        {
            string weaponInfo = $"<font color='#{Colors.Info}'>Weapons:</font><br>";

            foreach (var weaponIds in charData.WeaponIds)
                weaponInfo += $"<font color='#{Colors.Info}'> {weaponIds.Slot}:</font> <a href='itemref://{weaponIds.WeaponStat.LowId}/{weaponIds.WeaponStat.HighId}/{weaponIds.WeaponStat.Ql}'>{weaponIds.WeaponStat.Name}</a><br>";

            return weaponInfo;
        }

        private string FormatHeader(CharData charData)
        {
            return  $"<font color='#{Colors.Name}'>{charData.Name}</font><br><font color='#{Colors.Info}'>Profession:</font> {charData.Profession}<br>";
        }

        private string FormatHitInfo(CharData charData)
        {
            return $"<font color='#{Colors.Info}'>Weapon Hits:</font><br><font color='#{Colors.Info}'> Normal:</font> {charData.TotalHits.Normal}<br><font color='#{Colors.Info}'> Critical:</font> {charData.TotalHits.Crits}<br><font color='#{Colors.Info}'> Miss:</font> {charData.TotalHits.Misses}<br><font color='#{Colors.Info}'> Glancing:</font> {charData.TotalHits.Glancing}<br><font color='#{Colors.Info}'> Total:</font> {charData.TotalHits.AllHits}<br><br>";
        }

        private string FormatAbsorbInfo(CharData charData)
        {
            return $"<font color='#{Colors.Info}'>Absorb Per Minute:</font> {DpmFormat(charData.TotalDamage.Absorb)}<br><font color='#{Colors.Info}'>Total Percent:</font> {PercentFormat((float)charData.TotalDamage.Absorb / HitRegisters.TotalDamage.Absorb)}<br><font color='#{Colors.Info}'>Total Absorb:</font> {TotalDmgFormat(charData.TotalDamage.Absorb)}<br>";
        }

        private static string PercentFormat(float number)
        {
            return string.Format("{0:0.0}", number * 100) + "%";
        }

        private static string DpmFormat(int totalDamage)
        {
            return string.Format("{0:0.00}", ((double)totalDamage / 1000) * 60 / _elapsedTime) + "K";
        }

        public void UpdateMeterCount()
        {
            MeterView meterView = new MeterView();
            _views.MetersRoot.AddChild(meterView.Root, true);
            MeterViews.Add(meterView);
            _views.MetersRoot.FitToContents();
        }

        public DisplayConfig CurrentDisplayConfig()
        {
            IOrderedEnumerable<KeyValuePair<int, CharData>> orderedData;

            switch (CurrentMode)
            {
                case Mode.Damage:
                    orderedData = HitRegisters.CharData.Where(x => x.Value.TotalDamage.AllDamage > 0).OrderByDescending(x => x.Value.TotalDamage.AllDamage);
                    return new DisplayConfig
                    {
                        OrderedCharData = orderedData,
                        DamageDisplayData = orderedData.Select(x => x.Value.TotalDamage.AllDamage),
                        TotalAmount = HitRegisters.TotalDamage.AllDamage,
                        BarColorCode = ColorCode.Damage
                    };
                case Mode.Weapons:
                    orderedData = HitRegisters.CharData.Where(x => x.Value.TotalDamage.Weapon > 0).OrderByDescending(x => x.Value.TotalDamage.Weapon);
                    return new DisplayConfig 
                    { 
                        OrderedCharData = orderedData, 
                        DamageDisplayData = orderedData.Select(x => x.Value.TotalDamage.Weapon), 
                        TotalAmount = HitRegisters.TotalDamage.Weapon, 
                        BarColorCode = ColorCode.Weapon
                    };
                case Mode.NanosPerks:
                    orderedData = HitRegisters.CharData.Where(x => x.Value.TotalDamage.Nano > 0).OrderByDescending(x => x.Value.TotalDamage.Nano);
                    return new DisplayConfig 
                    { 
                        OrderedCharData = orderedData, 
                        DamageDisplayData = orderedData.Select(x => x.Value.TotalDamage.Nano), 
                        TotalAmount = HitRegisters.TotalDamage.Nano, 
                        BarColorCode = ColorCode.Nano
                    };
                case Mode.Healing:
                    orderedData = HitRegisters.CharData.Where(x => x.Value.TotalDamage.Healing > 0).OrderByDescending(x => x.Value.TotalDamage.Healing);
                    return new DisplayConfig
                    {
                        OrderedCharData = orderedData,
                        DamageDisplayData = orderedData.Select(x => x.Value.TotalDamage.Healing),
                        TotalAmount = HitRegisters.TotalDamage.Healing,
                        BarColorCode = ColorCode.Healing
                    };
                case Mode.Pets:
                    orderedData = HitRegisters.CharData.Where(x => x.Value.TotalDamage.Pet > 0).OrderByDescending(x => x.Value.TotalDamage.Pet);
                    return new DisplayConfig 
                    { 
                        OrderedCharData = orderedData, 
                        DamageDisplayData = orderedData.Select(x => x.Value.TotalDamage.Pet), 
                        TotalAmount = HitRegisters.TotalDamage.Pet, 
                        BarColorCode = ColorCode.Pet
                    };
                case Mode.Absorb:
                    orderedData = HitRegisters.CharData.Where(x => x.Value.TotalDamage.Absorb > 0).OrderByDescending(x => x.Value.TotalDamage.Absorb);
                    return new DisplayConfig 
                    { 
                        OrderedCharData = orderedData, 
                        DamageDisplayData = orderedData.Select(x => x.Value.TotalDamage.Absorb), 
                        TotalAmount = HitRegisters.TotalDamage.Absorb, 
                        BarColorCode = ColorCode.Absorb
                    };
                case Mode.ShieldReflect:
                    orderedData = HitRegisters.CharData.Where(x => x.Value.TotalDamage.Reflect > 0).OrderByDescending(x => x.Value.TotalDamage.Reflect);
                    return new DisplayConfig 
                    { 
                        OrderedCharData = orderedData, 
                        DamageDisplayData = orderedData.Select(x => x.Value.TotalDamage.Reflect), 
                        TotalAmount = HitRegisters.TotalDamage.Reflect, 
                        BarColorCode = ColorCode.Reflect
                    };
            }
            return null;
        }

        public void UpdateMeterViews()
        {
            int i = 0;

            DisplayConfig displayConfig = CurrentDisplayConfig();

            if (MeterViews.Count != displayConfig.DamageDisplayData.Count())
                MeterViews.Redraw(_views.MetersRoot, displayConfig.DamageDisplayData.Count());

            var highestValue = displayConfig.DamageDisplayData.FirstOrDefault();

            foreach (var meterView in MeterViews)
            {
                var disData = displayConfig.DamageDisplayData.Skip(i).FirstOrDefault();
                var charData = displayConfig.OrderedCharData.Skip(i++).FirstOrDefault();
                meterView.SetIcon(charData.Value.Profession);
                meterView.Meter.Value = (float)disData / highestValue;
                meterView.SetColor(displayConfig.BarColorCode);
                meterView.Meter.SetLabels(charData.Value.Name,
                     $"{TotalDmgFormat(disData)} " +
                     $"({DpmFormat(disData)}, " +
                     $"{PercentFormat((float)disData / displayConfig.TotalAmount)})");
            }
        }

        private void ScopeClick(object sender, ButtonBase e)
        {
            int modeGfx = CurrentScope == Scope.Solo ? 1430044 : CurrentScope == Scope.Team ? 1430045 : 1430043;
            _views.Scope.SetAllGfx(modeGfx);
            CurrentScope = CurrentScope == Scope.Solo ? Scope.Team : CurrentScope == Scope.Team ? Scope.All : Scope.Solo;
            Main.Settings.Scope = CurrentScope;
            Main.Settings.Save();
            Chat.WriteLine($"Scope switched to: {CurrentScope}");
            Midi.Play("Click");
        }

        private void PauseClick(object sender, ButtonBase e)
        {
            PauseToggle();
            Midi.Play("Click");
        }

        private void PauseToggle()
        {
            int texId = IsPaused ? 1430035 : 1430036;
            _views.Pause.SetAllGfx(texId);
            IsPaused = !IsPaused;
        }

        private void ResetClick(object sender, ButtonBase e)
        {
            Main.HitRegisters.ResetData();

            _views.Elapsed.Text = "0";
            _elapsedTime = 0;

            foreach (var s in MeterViews)
                _views.MetersRoot.RemoveChild(s.Root);

            MeterViews.Clear();
            _views.MetersRoot.FitToContents();
            Midi.Play("Click");
        }

        private void ModeClick(object sender, ButtonBase e)
        {
            ModeView modeView = CurrentMode.Next();
            CurrentMode = modeView.Mode;
            _views.ModeText.Text = modeView.Text;

            UpdateMeterViews();
            Midi.Play("Click");
        }

        public void MoveWindow()
        {
            if (Main.Settings.Frame.X == 0 && Main.Settings.Frame.Y == 0)
                Window.MoveToCenter();
            else
                Window.MoveTo(Main.Settings.Frame.X, Main.Settings.Frame.Y);
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

        internal class Views
        {
            public Button Pause;
            public Button Reset;
            public Button Mode;
            public Button Scope;
            public Button Settings;
            public BitmapView Background;
            public BitmapView Icon;
            public TextView Elapsed;
            public TextView ModeText;
            public View MetersRoot;
        }

        internal static class ColorCode
        {
            public const uint Damage = 0x27677A;
            public const uint Weapon = 0x7a4627;
            public const uint Nano = 0x27317a;
            public const uint Healing = 0x7a2727;
            public const uint Pet = 0x7a276d;
            public const uint Absorb = 0x277a6d;
            public const uint Reflect = 0x7a7327;
        }

        public class DisplayConfig
        {
            public IOrderedEnumerable<KeyValuePair<int, CharData>> OrderedCharData;
            public IEnumerable<int> DamageDisplayData;
            public int TotalAmount;
            public uint BarColorCode;
        }
    }
}