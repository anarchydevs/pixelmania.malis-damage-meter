using AOSharp.Common.GameData;
using AOSharp.Core.UI;

namespace MalisDamageMeter
{
    public class MeterView
    {
        public View Root;
        public BitmapView Icon;
        public PowerBarView Meter;
        private Profession _currentProfession;
        private uint _currentColor;

        public void SetColor(uint color)
        {
            if (_currentColor == color)
                return;

            _currentColor = color;
            Meter.SetBarColor(_currentColor);
        }

        public void SetIcon(Profession profession)
        {
            if (profession == (Profession)4294967295)
                return;

            if (profession == _currentProfession)
                return;

            _currentProfession = profession;
            Icon.SetBitmap($"GFX_GUI_ICON_PROFESSION_{(int)profession}");
        }

        public MeterView()
        {
            Root = View.CreateFromXml($"{Main.PluginDir}\\UI\\Views\\MeterView.xml");

            if (Root.FindChild("Icon", out Icon))

                if (Root.FindChild("Meter", out Meter))
                    Meter.SetBarColor(0x27677A);
        }
    }
}