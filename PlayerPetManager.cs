using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using MalisDamageMeter;
using SmokeLounge.AOtomation.Messaging.Messages;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MalisDamageMeter
{
    public class PlayerPetManager
    {
        public List<PlayerPet> PlayerPet = new List<PlayerPet>();

        public bool Contains(string petName, out PlayerPet playerPet)
        {
            playerPet = null;
            var pet = PlayerPet.FirstOrDefault(x => x.PetName == petName);

            if (pet != null)
            {
                playerPet = pet;
                return true;
            }

            return false;
        }
    }

    public class PlayerPet
    {
        public string PlayerName;
        public int PlayerId;
        public string PetName;
    }
}