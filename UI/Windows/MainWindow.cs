﻿using AOSharp.Common.GameData.UI;
using AOSharp.Core.Misc;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MalisDamageMeter
{
    public class MainWindow : AOSharpWindow
    {
        public List<MeterView> MeterViews = new List<MeterView>();
        protected Views _views = new Views();
        public SettingsWindow SettingsWindow;
        public Config ViewSettings = new Config();

        public MainWindow(string name, string path, WindowStyle windowStyle = WindowStyle.Popup, WindowFlags flags = WindowFlags.AutoScale | WindowFlags.NoFade) : base(name, path, windowStyle, flags) { }

        protected override void OnWindowCreating() { }

        protected class Views
        {
            public Button ResumePauseButton;
            public Button ResetButton;
            public Button ModeButton;
            public Button LogButton;
            public Button ScopeButton;
            public Button SettingsButton;
            public BitmapView Background;
            public BitmapView Icon;
            public TextView Elapsed;
            public TextView ModeText;
            public View MetersRoot;
        }

        public class Config
        {
            public PlayerPetManager PlayerPetManager = new PlayerPetManager();
            public bool AutoToggleTimer;
            public bool AutoAssignPets;
            public Scope Scope = new Scope();
            public Mode Mode = new Mode();
            public bool IsPaused;
            public double ElapsedTime;
            public AutoResetInterval ResetTimer = new AutoResetInterval(125);
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

        internal static class Textures
        {
            public const int PauseButton = 1430035;
            public const int StartButton = 1430036;
            public const int ResetButton = 1430037;
            public const int ModeButton = 1430038;
            public const int SettingsButton = 1430039;
            public const int Background = 1430040;
            public const int PowerbarBackground = 1430041;
            public const int PowerbarForeground = 1430042;
            public const int SoloScopeButton = 1430043;
            public const int TeamScopeButton = 1430044;
            public const int AllScopeButton = 1430045;
            public const int GreenCircleButton = 1430046;
            public const int RedCircleButton = 1430047;
            public const int Header = 1430048;
            public const int CloseButton = 1430049;
            public const int RedMinusButton = 1430050;
            public const int GreenPlusButton = 1430051;
            public const int SettingsBackground = 1430052;
            public const int HelpBackground = 1430053;
            public const int HelpButton = 1430054;
            public const int LogButton = 1430055;
        }
    }
}