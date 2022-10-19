using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using SmokeLounge.AOtomation.Messaging.Messages;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MalisDamageMeter
{
    public class HitRegisters
    {
        public static TotalDamage TotalDamage = new TotalDamage();
        public static Dictionary<int, CharData> CharData = new Dictionary<int, CharData>();
        public static Dictionary<int, PetData> PetData = new Dictionary<int, PetData>();
        public static Dictionary<N3MessageType, Action<N3Message>> n3MsgCallbacks = new Dictionary<N3MessageType, Action<N3Message>>
        {            
            { N3MessageType.SpecialAttackInfo, SpecialAttackDamage },
            { N3MessageType.ReflectAttack, ReflectDamage },
            { N3MessageType.ShieldAttack, ShieldAttackDamage },
            { N3MessageType.Absorb, AbsorbDamage },
            { N3MessageType.AttackInfo, AttackDamage },
            { N3MessageType.HealthDamage, NanoDamage },
            { N3MessageType.MissedAttackInfo, MissedAutoAttack },
        };

        internal void N3MessageCallback(N3Message n3Msg)
        {
            if (!n3MsgCallbacks.ContainsKey(n3Msg.N3MessageType))
                return;

            if (Main.UI.IsPaused)
                return;

            n3MsgCallbacks[n3Msg.N3MessageType].Invoke(n3Msg);
        }

        public void ResetData()
        {
            TotalDamage = new TotalDamage();
            CharData.Clear();
            PetData.Clear();
        }

        private static void MissedAutoAttack(N3Message n3Msg)
        {
            MissedAttackInfoMessage infoMsg = (MissedAttackInfoMessage)n3Msg;

            DynelManager.Find(infoMsg.Attacker, out SimpleChar simpleChar);


            if (!ScopeCheck(infoMsg.Defender))
                return;

            AddCharDataEntry(simpleChar);

            CharData.TryGetValue(infoMsg.Attacker.Instance, out CharData charData);
            charData.TotalHits.Misses += 1;
            charData.TotalHits.AllHits += 1;
        }

        public static void SpecialAttackDamage(N3Message n3Msg)
        {
            SpecialAttackInfoMessage infoMsg = (SpecialAttackInfoMessage)n3Msg;

            DynelManager.Find(infoMsg.Identity, out SimpleChar simpleChar);

            if (!ScopeCheck(infoMsg.Identity))
                return;

            AddCharDataEntry(simpleChar);

            CharData.TryGetValue(infoMsg.Identity.Instance, out CharData charData);
            charData.TotalDamage.AllDamage += infoMsg.Amount;
            charData.WeaponDamage[infoMsg.Stat] += infoMsg.Amount;
            TotalDamage.AllDamage += infoMsg.Amount;
            TotalDamage.Weapon += infoMsg.Amount;
        }

        public static void ReflectDamage(N3Message n3Msg)
        {
            ReflectAttackMessage infoMsg = (ReflectAttackMessage)n3Msg;

            DynelManager.Find(infoMsg.Target, out SimpleChar simpleChar);
            ProcessShieldAndReflect(infoMsg.Target, infoMsg.Stat, infoMsg.Amount);
        }

        private static void ShieldAttackDamage(N3Message n3Msg)
        {
            ShieldAttackMessage infoMsg = (ShieldAttackMessage)n3Msg;

            DynelManager.Find(infoMsg.Target, out SimpleChar simpleChar);
            ProcessShieldAndReflect(infoMsg.Target, infoMsg.Stat, infoMsg.Amount);
        }

        public static void AbsorbDamage(N3Message n3Msg)
        {
            AbsorbMessage infoMsg = (AbsorbMessage)n3Msg;

            DynelManager.Find(infoMsg.Identity, out SimpleChar simpleChar);


            if (!ScopeCheck(infoMsg.Identity))
                return;

            AddCharDataEntry(simpleChar);

            CharData.TryGetValue(infoMsg.Identity.Instance, out CharData charData);

            charData.TotalDamage.Absorb += infoMsg.Amount;
            TotalDamage.Absorb += infoMsg.Amount;
        }

        public static void AttackDamage(N3Message n3Msg)
        {
            AttackInfoMessage infoMsg = (AttackInfoMessage)n3Msg;

            DynelManager.Find(infoMsg.Identity, out SimpleChar simpleChar);

            var weaponInfo = infoMsg.GetWeaponDamageType();

            if (simpleChar.IsPet)
            {
                AddPetDataEntry(simpleChar);

                PetData.TryGetValue(infoMsg.Identity.Instance, out PetData petData);
                petData.PetDamage[weaponInfo.DamageType] += infoMsg.Amount;

                CharPetProcess(petData, infoMsg.Identity.Instance, infoMsg.Amount);
            }
            else if (simpleChar.IsPlayer)
            {
                if (!ScopeCheck(infoMsg.Identity))
                    return;

                AddCharDataEntry(simpleChar);

                CharData.TryGetValue(infoMsg.Identity.Instance, out CharData charData);

                if (!charData.WeaponIds.Any(x => x.WeaponStat.LowId == weaponInfo.WeaponStat.LowId && x.Slot == weaponInfo.Slot))
                    charData.WeaponIds.Add(new WeaponInfo { WeaponStat = weaponInfo.WeaponStat, Slot = weaponInfo.Slot });

                charData.WeaponDamage[weaponInfo.DamageType] += infoMsg.Amount;

                charData.TotalDamage.Weapon += infoMsg.Amount;
                TotalDamage.Weapon += infoMsg.Amount;

                charData.TotalDamage.AllDamage += infoMsg.Amount;
                TotalDamage.AllDamage += infoMsg.Amount;

                if (infoMsg.HitType == HitType.Normal)
                    charData.TotalHits.Normal += 1;
                else if (infoMsg.HitType == HitType.Critical)
                    charData.TotalHits.Crits += 1;
                else if (infoMsg.HitType == HitType.Glancing)
                    charData.TotalHits.Glancing += 1;

                charData.TotalHits.AllHits += 1;
            }
        }

        public static void NanoDamage(N3Message n3Msg)
        {
            HealthDamageMessage infoMsg = (HealthDamageMessage)n3Msg;

            DynelManager.Find(infoMsg.Target, out SimpleChar simpleChar);

            if (simpleChar.IsPet)
            {
                AddPetDataEntry(simpleChar);
                var petData = PetData[infoMsg.Target.Instance];

                if (infoMsg.Amount == 0)
                    return;

                if (infoMsg.Amount < 0)
                    petData.PetDamage[infoMsg.Stat] += Math.Abs(infoMsg.Amount);
                else if (infoMsg.Amount > 0)
                    petData.Healing += infoMsg.Amount;

                CharPetProcess(petData, infoMsg.Target.Instance, infoMsg.Amount);
            }
            else if (simpleChar.IsPlayer)
            {
                if (!ScopeCheck(infoMsg.Target))
                    return;

                if (infoMsg.Amount == 0)
                    return;

                AddCharDataEntry(simpleChar);

                CharData.TryGetValue(infoMsg.Target.Instance, out CharData charData);

                if (infoMsg.Amount < 0)
                {
                    charData.NanoDamage[infoMsg.Stat] += Math.Abs(infoMsg.Amount);

                    charData.TotalDamage.Nano += Math.Abs(infoMsg.Amount);
                    TotalDamage.Nano += Math.Abs(infoMsg.Amount);

                    charData.TotalDamage.AllDamage += Math.Abs(infoMsg.Amount);
                    TotalDamage.AllDamage += Math.Abs(infoMsg.Amount);
                }
                else if (infoMsg.Amount > 0)
                {
                    charData.TotalDamage.Healing += infoMsg.Amount;
                    TotalDamage.Healing += infoMsg.Amount;
                }
            }
        }

        private static void ProcessShieldAndReflect(Identity infoMsgIdentity, Stat dmgType, int amount)
        {
            DynelManager.Find(infoMsgIdentity, out SimpleChar simpleChar);

            if (simpleChar.IsPet)
            {
                AddPetDataEntry(simpleChar);

                PetData.TryGetValue(infoMsgIdentity.Instance, out PetData petData);
                petData.PetDamage[dmgType] += amount;

                CharPetProcess(petData, infoMsgIdentity.Instance, -amount);
            }
            else if (simpleChar.IsPlayer)
            {
                if (!ScopeCheck(infoMsgIdentity))
                    return;

                AddCharDataEntry(simpleChar);

                CharData.TryGetValue(infoMsgIdentity.Instance, out CharData charData);

                charData.TotalDamage.Reflect += amount;
                TotalDamage.Reflect += amount;

                charData.TotalDamage.AllDamage += amount;
                TotalDamage.AllDamage += amount;
            }
        }

        private static void CharPetProcess(PetData petData, int identityInstance, int dmgAmount)
        {
            var charPet = CharData.FirstOrDefault(x => x.Value.PetIds.Contains(identityInstance));

            if (charPet.Value == null)
                return;

            charPet.Value.PetDamage = PetData[identityInstance].PetDamage;

            CharData.TryGetValue(charPet.Key, out CharData charData);

            if (!petData.IsRegistered)
            {
                int damageCounter = 0;

                foreach (var b in petData.PetDamage)
                    damageCounter += b.Value;

                charData.TotalDamage.AllDamage += damageCounter;
                TotalDamage.AllDamage += damageCounter;

                charData.TotalDamage.Healing += petData.Healing;
                TotalDamage.Healing += petData.Healing;

                charData.TotalDamage.Pet += damageCounter;
                TotalDamage.Pet += damageCounter;

                PetData[identityInstance].IsRegistered = true;
            }
            else
            {
                if (dmgAmount > 0)
                {
                    charData.TotalDamage.AllDamage += dmgAmount;
                    TotalDamage.AllDamage += dmgAmount;

                    charData.TotalDamage.Pet += dmgAmount;
                    TotalDamage.Pet += dmgAmount;
                }
                else
                {
                    charData.TotalDamage.Healing += dmgAmount;
                    TotalDamage.Healing += dmgAmount;
                }
            }

        }

        private static void AddPetDataEntry(SimpleChar simpleChar)
        {
            if (!PetData.ContainsKey(simpleChar.Identity.Instance))
            {
                PetData.Add(simpleChar.Identity.Instance,
                    new PetData
                    {
                        Name = simpleChar.Name,
                        IsRegistered = false,
                        PetDamage = new Dictionary<Stat, int>().SetStats(),
                        Healing = 0
                    });
            }


            var petList = Main.Settings.PetList.FirstOrDefault(x => x.PetName == simpleChar.Name);
            SimpleChar target = null;

            if (petList == null)
            {
                if (Main.Settings.AutoAssignPets)
                    target = DynelManager.Players.FirstOrDefault(x => x.Name == simpleChar.Name);

                if (DynelManager.LocalPlayer.Pets.Length > 0 &&
                    DynelManager.LocalPlayer.Pets.FirstOrDefault(x => x.Identity == simpleChar.Identity) != null)
                    target = DynelManager.LocalPlayer;
            }
            else
            {
                target = DynelManager.Players.FirstOrDefault(x => x.Identity.Instance == petList.PlayerId);
            }

            if (target != null)
            {
                if (!ScopeCheck(target.Identity))
                    return;

                AddCharDataEntry(target);
            }

            if (CharData[target.Identity.Instance].PetIds.Contains(simpleChar.Identity.Instance))
                return;

            CharData[target.Identity.Instance].PetIds.Add(simpleChar.Identity.Instance);
        }

        private static void AddCharDataEntry(SimpleChar simpleChar)
        {
            if (!CharData.ContainsKey(simpleChar.Identity.Instance))
            {
                Utils.InfoPacket(simpleChar.Identity);

                CharData.Add(simpleChar.Identity.Instance,
                    new CharData
                    {
                        Name = simpleChar.Name,
                        Profession = simpleChar.Profession,
                        TotalDamage = new TotalDamage(),
                        WeaponDamage = new Dictionary<Stat, int>().SetStats(),
                        NanoDamage = new Dictionary<Stat, int>().SetStats(),
                        PetDamage = new Dictionary<Stat, int>().SetStats(),
                        PetIds = new List<int>(),
                        WeaponIds = new List<WeaponInfo>(),
                        TotalHits = new TotalHits(),
                    });
            }
            else if (CharData.TryGetValue(simpleChar.Identity.Instance, out CharData data) && data.Profession == (Profession)4294967295)
            {
                CharData[simpleChar.Identity.Instance].Profession = simpleChar.Profession;
                UI.MeterViews[simpleChar.Identity.Instance].SetIcon(simpleChar.Profession);
            }
        }

        private static bool ScopeCheck(Identity identity)
        {
            if (Main.UI.CurrentScope == Scope.Solo && identity != DynelManager.LocalPlayer.Identity)
                return false;

            if (Main.UI.CurrentScope == Scope.Team && !Team.Members.Any(x => x.Character.Identity == identity))
                return false;

            return true;
        }
    }
}
public class WeaponInfo
{
    public WeaponStat WeaponStat;
    public WeaponSlots Slot;
    public Stat DamageType;
}

public class WeaponStat
{
    public string Name;
    public int Ql;
    public int LowId;
    public int HighId;
}

public class CharData
{
    public string Name;
    public Profession Profession;
    public TotalDamage TotalDamage;
    public TotalHits TotalHits;
    public List<WeaponInfo> WeaponIds;
    public List<int> PetIds;
    public Dictionary<Stat, int> WeaponDamage;
    public Dictionary<Stat, int> NanoDamage;
    public Dictionary<Stat, int> PetDamage;
}

public class PetData
{
    public bool IsRegistered = false;
    public string Name;
    public Dictionary<Stat, int> PetDamage;
    public int Healing;
}

public enum WeaponSlots
{
    Fist = 0x0,
    PetFist1 = 0x1,
    PetFist2 = 0x2,
    PetFist3 = 0x3,
    PetFist4 = 0x4,
    PetFist5 = 0x5,
    MainHand = 0x6,
    Offhand = 0x8,
}

public class TotalHits
{
    public int AllHits;
    public int Normal;
    public int Crits;
    public int Misses;
    public int Glancing;
}

public class TotalDamage
{
    public int AllDamage;
    public int Weapon;
    public int Nano;
    public int Healing;
    public int Pet;
    public int Absorb;
    public int Reflect;
}