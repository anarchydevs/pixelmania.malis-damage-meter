using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Common.Helpers;
using AOSharp.Common.Unmanaged.Imports;
using AOSharp.Common.Unmanaged.Interfaces;
using AOSharp.Core;
using AOSharp.Core.UI;
using SmokeLounge.AOtomation.Messaging.GameData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MalisDamageMeter
{
    public class HelpWindow
    {
        public Window Window;

        public HelpWindow()
        {
            Window = Window.CreateFromXml("MalisDmgMeterHelp", $"{Main.PluginDir}\\UI\\Windows\\HelpWindow.xml", 
                WindowStyle.Popup, WindowFlags.AutoScale | WindowFlags.NoFade);

            if (Window.FindView("Start", out Button Start))
            {
                Start.SetAllGfx(1430036);
            }

            if (Window.FindView("Pause", out Button Pause))
            {
                Pause.SetAllGfx(1430035);

            }

            if (Window.FindView("Reset", out Button Reset))
            {
                Reset.SetAllGfx(1430037);
            }

            if (Window.FindView("Solo", out Button Solo))
            {
                Solo.SetAllGfx(1430043);
            }

            if (Window.FindView("Group", out Button Group))
            {
                Group.SetAllGfx(1430044);
            }

            if (Window.FindView("All", out Button All))
            {
                All.SetAllGfx(1430045);
            }

            if (Window.FindView("Mode", out Button Mode))
            {
                Mode.SetAllGfx(1430038);
            }

            if (Window.FindView("Settings", out Button Settings))
            {
                Settings.SetAllGfx(1430039);
            }

            if (Window.FindView("Text", out TextView textView))
            {
                textView.Text = $"\n\n " +
                $"- /mdmb - basic dump\n " +
                $"  For other data, change 'Display Mode'\n " +
                $"  change the 'Display Mode' beforehand\n " +
                $"  \n " +
                $"- /mdma name - advanced dump\n " +
                $"  Dumps advanced logs of one player,\n " +
                $"  \n " +
                $"- Open 'Settings' to register player pets \n " +
                $"  (don't need to register your own pets) \n " +
                $"  or to reopen this window again. \n ";
            }

            if (Window.FindView("Text2", out TextView textView2))
            {
                textView2.Text =$"\n" +
                $"- For bugs / glitches / requests:\n " +
                $"  Discord:  Pixelmania#0349\n\n\n " +
                $"       ~ Made with AOSharp SDK";
            }

            if (Window.FindView("Close", out Button _closeHelp))
            {
                _closeHelp.SetAllGfx(1430049);
                _closeHelp.Clicked = CloseClick;
            }

            if (Window.FindView("Logo", out BitmapView _logo))
            {
                _logo.SetBitmap("BigLogo");
            }
        }

        private void CloseClick(object sender, ButtonBase e)
        {
            Midi.Play("Click");
            Window.Close();
        }
    }
}