using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MalisDamageMeter
{
    public static class HitRegisters
    {
        public static Dictionary<int, SimpleCharData> Characters = new Dictionary<int, SimpleCharData>();
        public static Dictionary<int, PetData> Pets = new Dictionary<int, PetData>();

        public static void TransferPetData(PetData petData, int playerId)
        {
            petData.OwnerId = playerId;

            foreach (var stat in petData.DamageSources.Weapon.Pet.AutoAttack.DamagePerType)
                Characters[playerId].DamageSources.RegisterAttackInfoMessage(stat.Key, stat.Value, RegisterType.Pet);

            foreach (var stat in petData.DamageSources.Weapon.Pet.Specials.DamagePerType)
                Characters[playerId].DamageSources.RegisterSpecialAttackInfoMessage(stat.Key, stat.Value, RegisterType.Pet);

            foreach (var stat in petData.DamageSources.Nanobots.Pet.DamagePerType)
                Characters[playerId].DamageSources.RegisterHealthDamage(stat.Key, stat.Value, RegisterType.Pet);

            Characters[playerId].DamageSources.RegisterReflect(petData.DamageSources.DeflectSource.Reflect.Total, RegisterType.Pet);

            Characters[playerId].DamageSources.RegisterShield(petData.DamageSources.DeflectSource.Shield.Total, RegisterType.Pet);

            Characters[playerId].AbsorbSource.RegisterTotal(petData.AbsorbSource.Total, RegisterType.Pet);
            Characters[playerId].HealSource.RegisterTotal(petData.HealSource.Total, RegisterType.Pet);

            Characters[playerId].HitSource.Pet.Crit += petData.HitSource.Pet.Crit;
            Characters[playerId].HitSource.Pet.Normal += petData.HitSource.Pet.Normal;
            Characters[playerId].HitSource.Pet.Glancing += petData.HitSource.Pet.Glancing;
            Characters[playerId].HitSource.Pet.Miss += petData.HitSource.Pet.Miss;
            Characters[playerId].HitSource.Pet.Total += petData.HitSource.Pet.Total;
        }

        public static int Sum(ModeEnum mode)
        {
            Func<SimpleCharData, int> damageSelector = null;

            switch (mode)
            {
                case ModeEnum.Damage:
                    damageSelector = x => x.DamageSources.Total;
                    break;
                case ModeEnum.Healing:
                    damageSelector = x => x.HealSource.Total;
                    break;
                default:
                    Chat.WriteLine($"Unsupported mode: {mode}");
                    break;
            }

            return Characters.Values.Select(damageSelector).Sum();
        }


        public static DisplayConfig GetDisplayConfig(Mode mode)
        {
            List<SimpleCharMeterData> simpleCharMeterData = new List<SimpleCharMeterData>();

            int totalAmount = 0;

            switch (mode.Current)
            {
                case ModeEnum.Damage:
                    simpleCharMeterData = Characters
                       .Select(x => new SimpleCharMeterData
                       {
                           SimpleCharData = x.Value,
                           Total = x.Value.DamageSources.Total,
                           MeterViewData = new List<MeterViewData>
                           {
                                new MeterViewData { Total = x.Value.DamageSources.Weapon.User.AutoAttack.Total, Color = MeterViewColors.DamageAutoAttack },
                                new MeterViewData { Total = x.Value.DamageSources.Weapon.User.Specials.Total, Color = MeterViewColors.DamageSpecials },
                                new MeterViewData { Total = x.Value.DamageSources.Nanobots.UserTotal, Color = MeterViewColors.DamageNanobots },
                                new MeterViewData { Total = x.Value.DamageSources.PetTotal, Color = MeterViewColors.DamagePet },
                                new MeterViewData { Total = x.Value.DamageSources.DeflectSource.UserTotal, Color = MeterViewColors.DamageDeflect },
                           }.OrderByDescending(y => y.Total).ToList()
                       })
                       .Where(x => x.SimpleCharData.DamageSources.Total > 0)
                       .OrderByDescending(x => { var total = x.SimpleCharData.DamageSources.Total; totalAmount += total; return total; })
                       .ToList();
                    break;
                case ModeEnum.Healing:
                    simpleCharMeterData = Characters
                       .Select(x => new SimpleCharMeterData
                       {
                           SimpleCharData = x.Value,
                           Total = x.Value.HealSource.Total,
                           MeterViewData = new List<MeterViewData>
                           {
                                new MeterViewData { Total = x.Value.HealSource.UserTotal, Color = MeterViewColors.HealUser },
                                new MeterViewData { Total = x.Value.HealSource.PetTotal, Color = MeterViewColors.HealPet },
                           }.OrderByDescending(y => y.Total).ToList()
                       })
                       .Where(x => x.SimpleCharData.HealSource.Total > 0)
                       .OrderByDescending(x => { var total = x.SimpleCharData.HealSource.Total; totalAmount += total; return total; })
                       .ToList();
                    break;
                default:
                    Chat.WriteLine($"Unsupported mode: {mode}");
                    break;
            }

            return new DisplayConfig
            {
                SimpleCharMeterData = simpleCharMeterData,
                TotalAmount = totalAmount,
            };
        }
    }

    public class DisplayConfig
    {
        public List<SimpleCharMeterData> SimpleCharMeterData;
        public int TotalAmount;
        public uint BarColorCode;
    }

    public class SimpleCharMeterData
    {
        public SimpleCharData SimpleCharData;
        public int Total;
        public List<MeterViewData> MeterViewData = new List<MeterViewData>();
    }
}