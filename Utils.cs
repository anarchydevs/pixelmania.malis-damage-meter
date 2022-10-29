using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Common.Unmanaged.DataTypes;
using AOSharp.Common.Unmanaged.Imports;
using AOSharp.Common.Unmanaged.Interfaces;
using AOSharp.Core;
using AOSharp.Core.UI;
using SmokeLounge.AOtomation.Messaging.GameData;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MalisDamageMeter
{
    public class Utils
    {
        public static void LoadCustomTextures(string path, int startId)
        {
            DirectoryInfo textureDir = new DirectoryInfo(path);

            foreach (var file in textureDir.GetFiles("*.png").OrderBy(x => x.Name))
            {
                GuiResourceManager.CreateGUITexture(file.Name.Replace(".png", "").Remove(0, 4), startId++, file.FullName);
            }
        }

        public static void InfoPacket(Identity identity)
        {
            Network.Send(new CharacterActionMessage()
            {
                Action = CharacterActionType.InfoRequest,
                Identity = DynelManager.LocalPlayer.Identity,
                Target = identity,
            });
        }
    }
}