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
    public static class Extensions
    {
        public static WeaponInfo GetWeaponDamageType(this AttackInfoMessage attckInfoMsg)
        {
            if (!DynelManager.Find(attckInfoMsg.Identity, out SimpleChar infoChar))
                return null;

            bool isEquippableWeapon = ((WeaponSlots)attckInfoMsg.WeaponSlot).IsEquippableWeapon();

            WeaponInfo weaponInfo = new WeaponInfo
            {
                Slot = (WeaponSlots)attckInfoMsg.WeaponSlot,
                DummyItem = new WeaponStat { Name = "", LowId = 0, HighId = 0, Ql = 0 },
            };

            if (infoChar.Weapons.Count() != 0 && isEquippableWeapon)
            {
                weaponInfo.DummyItem.Name = infoChar.Weapons[(EquipSlot)attckInfoMsg.WeaponSlot].Name;
                weaponInfo.DummyItem.LowId = infoChar.Weapons[(EquipSlot)attckInfoMsg.WeaponSlot].GetStat(Stat.ACGItemTemplateID);
                weaponInfo.DummyItem.HighId = infoChar.Weapons[(EquipSlot)attckInfoMsg.WeaponSlot].GetStat(Stat.ACGItemTemplateID2);
                weaponInfo.DummyItem.Ql = infoChar.Weapons[(EquipSlot)attckInfoMsg.WeaponSlot].GetStat(Stat.ACGItemLevel);
            }

            if (infoChar.GetStat(Stat.DamageType1) != 0)
            {
                weaponInfo.DamageType = (Stat)infoChar.GetStat(Stat.DamageType1);
                return weaponInfo;
            }
            else if (isEquippableWeapon)
            {
                weaponInfo.DamageType = (Stat)infoChar.Weapons[(EquipSlot)attckInfoMsg.WeaponSlot].GetStat(Stat.DamageType2);
                return weaponInfo;
            }
            else
            {
                weaponInfo.DamageType = Stat.MeleeAC;
                return weaponInfo;
            }
        }

        public static bool IsEquippableWeapon(this WeaponSlots weaponSlot)
        {
            if (weaponSlot == WeaponSlots.MainHand ||
                weaponSlot == WeaponSlots.Offhand)
                return true;

            return false;
        }
        public static Dictionary<Stat, int> SetStats(this Dictionary<Stat, int> dict)
        {
            return new Dictionary<Stat, int>
            {                
                { Stat.ChemicalAC , 0 },
                { Stat.ColdAC , 0 },
                { Stat.EnergyAC , 0 },
                { Stat.FireAC , 0 },
                { Stat.MeleeAC , 0 },
                { Stat.PoisonAC , 0 },
                { Stat.ProjectileAC , 0 },
                { Stat.RadiationAC , 0 },
                { Stat.FlingShot , 0 },
                { Stat.Burst , 0 },
                { Stat.FullAuto , 0 },
                { Stat.AimedShot , 0 },
                { Stat.Brawl , 0 },
                { Stat.Dimach , 0 },
                { Stat.Backstab , 0 },
                { Stat.FastAttack , 0 },
                { Stat.SneakAttack , 0 }
            };
        }

        public static Dictionary<Stat, int> TotalDamagePerStat(this Dictionary<Stat, int> dict, CharData charData)
        {
            Dictionary<Stat, int> totalDamagePerStat = new Dictionary<Stat, int>().SetStats();

            foreach (var dmg in charData.WeaponDamage)
                totalDamagePerStat[dmg.Key] += dmg.Value;

            foreach (var dmg in charData.NanoDamage)
                totalDamagePerStat[dmg.Key] += dmg.Value;

            foreach (var dmg in charData.PetDamage)
                totalDamagePerStat[dmg.Key] += dmg.Value;

            return totalDamagePerStat;
        }

        public static ModeView Next(this Mode modeEnum)
        {
            switch (modeEnum)
            {
                case Mode.Damage:
                    return new ModeView { Mode = Mode.Weapons, Text = "Weapons" };
                case Mode.Weapons:
                    return new ModeView { Mode = Mode.NanosPerks, Text = "Nano / Perk" };
                case Mode.NanosPerks:
                    return new ModeView { Mode = Mode.Healing, Text = "Healing" };
                case Mode.Healing:
                    return new ModeView { Mode = Mode.Pets, Text = "Pets" };
                case Mode.Pets:
                    return new ModeView { Mode = Mode.Absorb, Text = "Absorb" };
                case Mode.Absorb:
                    return new ModeView { Mode = Mode.ShieldReflect, Text = "Shield / Reflect" };
                case Mode.ShieldReflect:
                    return new ModeView { Mode = Mode.Damage, Text = "All Damage" };
                default:
                    return null;
            }
        }

        public static void Redraw(this List<MeterView> meterViews, View meterRoot, int count)
        {
            foreach (var meterView in meterViews)
                meterRoot.RemoveChild(meterView.Root);

            meterViews.Clear();

            for (int i = 0; i < count; i++)
            {
                MeterView meterView = new MeterView();
                meterRoot.AddChild(meterView.Root, true);
                meterViews.Add(meterView);
            }
            meterRoot.FitToContents();
        }

        public static void SetAllGfx(this Button button, int gfxId)
        {
            button.SetGfx(ButtonState.Raised, gfxId);
            button.SetGfx(ButtonState.Hover, gfxId);
            button.SetGfx(ButtonState.Pressed, gfxId);
        }
    }
}

public enum Scope
{
    Solo,
    Team,
    All
}

public enum Mode
{
    Damage,
    Weapons,
    NanosPerks,
    Healing,
    Pets,
    Absorb,
    ShieldReflect
}

public class ModeView
{
    public Mode Mode;
    public string Text;
}


public class PlayerPet
{
    public string PlayerName;
    public int PlayerId;
    public string PetName;
}