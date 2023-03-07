using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.Misc;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MalisDamageMeter
{
    public static class Format
    {
        public class Colors
        {
            public const string Title = "b9ff00";
            public const string Accent = "b34c5e";
            public const string Bonus = "b36e4c";
            public const string Info = "498ab6";
            public const string Name = "eec911";
        }

        public static string Time(double seconds) => (TimeSpan.FromSeconds(seconds)).ToString(@"h\:mm\:ss\:f");

        public static string DumpDmgFormatBasic(double elapsedTime)
        {
            string core = "";
            int i = 0;

            var totalDmg = HitRegisters.Characters.Values.Select(x => x.DamageSources.Total).Sum();

            foreach (var charData in HitRegisters.Characters.Values)
                core += BasicCoreDumpGeneral("Damage", elapsedTime, ++i, charData.Name, charData.Profession.ToString(), charData.DamageSources.Total, totalDmg);

            string header = BasicHeaderDumpGeneral("Damage", elapsedTime, totalDmg);

            string fullLog = $@"<a href=""text://" + $"{header + core}" + $"<font color='#{Colors.Info}'>~ Dump provided by Mali's Damage Meter.</font>" + $@""" >Basic Dump (Damage)</a>";

            return fullLog;
        }

        public static string DumpHealingFormatBasic(double elapsedTime)
        {
            string core = "";
            int i = 0;

            var totalDmg = HitRegisters.Characters.Values.Select(x => x.HealSource.Total).Sum();

            foreach (var charData in HitRegisters.Characters.Values)
                core += BasicCoreDumpGeneral("Healing", elapsedTime, ++i, charData.Name, charData.Profession.ToString(), charData.HealSource.Total, totalDmg);

            string header = BasicHeaderDumpGeneral("Healing", elapsedTime, totalDmg);

            string fullLog = $@"<a href=""text://" + $"{header + core}" + $"<font color='#{Colors.Info}'>~ Dump provided by Mali's Damage Meter.</font>" + $@""" >Basic Dump (Healing)</a>";

            return fullLog;
        }

        public static string BasicCoreDumpGeneral(string name, double elapsedTime, int i, string charName, string charProfession, int totalCharSource, int totalSource)
        {
            return $"{++i}. <font color='#{Colors.Name}'>{charName}</font> " +
                    $"<font color='#{Colors.Accent}'>({charProfession})</font> " +
                    $"<font color='#{Colors.Info}'>Total {name}:</font> " +
                    $"{TotalDmgFormat(totalCharSource)} <font color='#{Colors.Accent}'>|</font> " +
                    $"<font color='#{Colors.Info}'>Per Minute:</font> " +
                    $"{DpmFormat(totalCharSource, elapsedTime)} <font color='#{Colors.Accent}'>|</font>" +
                    $" <font color='#{Colors.Info}'>Percent:</font> " +
                    $"{PercentFormat((float)totalCharSource / totalSource)}<br>";
        }

        public static string BasicHeaderDumpGeneral(string name, double elapsedTime, int totalSource)
        {
            return $"<font color='#{Colors.Title}'>Fight Duration:</font> {Time(elapsedTime)} (h:m:s:ds)<br>" +
                    $"<font color='#{Colors.Title}'>Total {name}:</font> {totalSource}<br>" +
                    $"<font color='#{Colors.Title}'>{name} Per Minute:</font> {DpmFormat(totalSource, elapsedTime)}<br><br>";
        }


        public static string DumpDmgFormatAdvanced(SimpleCharData charData, double elapsedTime)
        {
            string coreDump =
                $"{FormatHeader(charData, elapsedTime)}" +
                $"{FormatAllTotal(charData, elapsedTime)}" +
                $"{FormatWeaponInfo(charData)}" +
                $"{FormatAcDamage(charData)}" +
                $"{FormatSpecialDamage(charData)}" +
                $"{FormatOtherDamage(charData)}" +
                $"{FormatHealing(charData)}" +
                $"{FormatHitInfo(charData)}<br><br>" +
                $"<font color='#{Colors.Info}'>~ Dump provided by Mali's Damage Meter.</font>";

            string fullLog = $@"<a href=""text://" + $"{coreDump}" + $@""" >Advanced Dump ({charData.Name})</a>";

            return fullLog;
        }

        private static string FormatHealing(SimpleCharData charData)
        {
            string log = $"{FormatHitSingle("Total", charData.HealSource.Total, charData.HealSource.UserTotal, charData.HealSource.PetTotal)}";
            return $"<font color='#{Colors.Name}'>Healing:</font><br>{log}<br>";
        }

        public static string FormatAllTotal(SimpleCharData charData, double elapsedTime)
        {
            string log =
                $"{FormatTotal(charData.DamageSources.Total,"Damage", elapsedTime)}" +
                $"{FormatTotal(charData.HealSource.Total, "Healing", elapsedTime)}" +
                $"{FormatTotal(charData.AbsorbSource.Total, "Absorbed", elapsedTime)}" +
                $"{FormatTotal(charData.HitSource.Total, "Hits", elapsedTime)}" +
                $"{FormatTotal(charData.HitSource.User.Crit + charData.HitSource.Pet.Crit, "Crits", elapsedTime)}";

            return $"<font color='#{Colors.Name}'>Totals:</font><br>{log}<br>";

        }

        private static string FormatTotal(int totalAmount, string text, double elapsedTime)
        {
            return $" <font color='#{Colors.Info}'>{text}:</font> " +
                $"{TotalDmgFormat(totalAmount)}<font color='#{Colors.Accent}'> - </font>" +
                $"<font color='#{Colors.Info}'>Per Minute:</font> " +
                $"{DpmFormat(totalAmount, elapsedTime)}<br>";
        }

        public static string FormatSpecialDamage(SimpleCharData charData)
        {
            string log = "";

            foreach (var damageAmount in Utils.SetSpecialsStats())
                log += $" <font color='#{Colors.Info}'>{damageAmount.Key}:</font> {charData.DamageSources.Weapon.User.Specials.DamagePerType[damageAmount.Key]}<br>";

            return $"<font color='#{Colors.Name}'>Specials Damage:</font><br>{log}<br>";
        }

        public static string FormatOtherDamage(SimpleCharData charData)
        {
            string log =
                FormatHitSingle("Reflect", charData.DamageSources.DeflectSource.Reflect.Total, charData.DamageSources.DeflectSource.Reflect.UserTotal, charData.DamageSources.DeflectSource.Reflect.PetTotal) +
                FormatHitSingle("Shield", charData.DamageSources.DeflectSource.Shield.Total, charData.DamageSources.DeflectSource.Shield.UserTotal, charData.DamageSources.DeflectSource.Shield.PetTotal);

            return $"<font color='#{Colors.Name}'>Other Damage:</font><br>{log}<br>";
        }

        public static string FormatAcDamage(SimpleCharData charData)
        {
            string log = "";

            foreach (var damageAmount in Utils.SetAcStats())
            {
                var UserAutoAttack= charData.DamageSources.Weapon.User.AutoAttack.DamagePerType[damageAmount.Key];
                var UserNanobots = charData.DamageSources.Nanobots.User.DamagePerType[damageAmount.Key];
                var PetAutoAttack = charData.DamageSources.Weapon.Pet.AutoAttack.DamagePerType[damageAmount.Key];
                var PetNanobots = charData.DamageSources.Nanobots.Pet.DamagePerType[damageAmount.Key];

                int totalAmount = UserAutoAttack + UserNanobots + PetAutoAttack + PetNanobots;

                log += 
                    $" <font color='#{Colors.Info}'>{damageAmount.Key}:</font> " +
                    $"{totalAmount}" +
                    $"{FormatDamageAcSingle("User",UserAutoAttack, UserNanobots)}"+
                    $"{FormatDamageAcSingle("Pet",PetAutoAttack, PetNanobots)}<br>";
            }

            return $"<br><font color='#{Colors.Name}'>AC Damage:</font><br>{log}<br>";
        }

        private static string FormatDamageAcSingle(string text, int autoAttackTotal, int nanobotsTotal)
        {
            return
                 $"<font color='#{Colors.Accent}'> - </font>" +
                 $"<font color='#{Colors.Info}'>{text} (</font>" +
                 $"<font color='#{Colors.Accent}'>W:</font> " +
                 $"{autoAttackTotal}" +
                 $"<font color='#{Colors.Info}'> , </font>" +
                 $"<font color='#{Colors.Accent}'>N:</font> " +
                 $"{nanobotsTotal}" +
                 $"<font color='#{Colors.Info}'>)</font>";
        }

        public static string FormatHealingInfo(SimpleCharData charData, double elapsedTime, ModeEnum currentMode)
        {
            string healingInfo = $"<font color='#{Colors.Info}'>Healing Per Minute:</font> " +
                $"{DpmFormat(charData.HealSource.Total, elapsedTime)}<br>" +
                $"<font color='#{Colors.Info}'>Total Percent:</font> " +
                $"{PercentFormat((float)charData.HealSource.Total / HitRegisters.Sum(currentMode))}<br>" +
                $"<font color='#{Colors.Info}'>Total Healing:</font> " +
                $"{TotalDmgFormat(charData.HealSource.Total)}<br>";

            return healingInfo;
        }

        public static string FormatAbsorbInfo(SimpleCharData charData, double elapsedTime, ModeEnum currentMode)
        {
            return $"<font color='#{Colors.Info}'>Absorb Per Minute:</font> " +
                $"{DpmFormat(charData.AbsorbSource.Total, elapsedTime)}<br>" +
                $"<font color='#{Colors.Info}'>Total Percent:</font> " +
                $"{PercentFormat((float)charData.AbsorbSource.Total / HitRegisters.Sum(currentMode))}<br>" +
                $"<font color='#{Colors.Info}'>Total Absorb:</font> " +
                $"{TotalDmgFormat(charData.AbsorbSource.Total)}<br>";
        }

        public static string FormatWeaponInfo(SimpleCharData charData)
        {
            string weaponInfo = $"<font color='#{Colors.Name}'>Weapons:</font><br>";

            foreach (var weaponIds in charData.WeaponIds)
                weaponInfo += $"<font color='#{Colors.Info}'> {weaponIds.Slot}:</font> " +
                $"<a href='itemref://{weaponIds.DummyItem.LowId}/{weaponIds.DummyItem.HighId}/{weaponIds.DummyItem.Ql}'>{weaponIds.DummyItem.Name}</a><br>";

            return weaponInfo;
        }

        public static string FormatHeader(SimpleCharData charData, double elapsedTime)
        {
            return
                $"<font color='#{Colors.Title}'>Name:</font> {charData.Name}</font><br>" +
                $"<font color='#{Colors.Title}'>Profession:</font> {charData.Profession}<br>" +
                $"<font color='#{Colors.Title}'>Duration:</font> {Time(elapsedTime)}<br><br>";

        }

        public static string FormatHitInfo(SimpleCharData charData)
        {
            string hitInfo = $"<font color='#{Colors.Name}'>Hits:</font><br>";

            string hitTypeInfo =
                $"{FormatHitSingle("Normal", charData.HitSource.User.Normal + charData.HitSource.Pet.Normal, charData.HitSource.User.Normal, charData.HitSource.Pet.Normal)}" +
                $"{FormatHitSingle("Critical", charData.HitSource.User.Crit + charData.HitSource.Pet.Crit, charData.HitSource.User.Crit, charData.HitSource.Pet.Crit)}" +
                $"{FormatHitSingle("Miss", charData.HitSource.User.Miss + charData.HitSource.Pet.Miss, charData.HitSource.User.Miss, charData.HitSource.Pet.Miss)}" +
                $"{FormatHitSingle("Glancing", charData.HitSource.User.Glancing + charData.HitSource.Pet.Glancing, charData.HitSource.User.Glancing, charData.HitSource.Pet.Glancing)}";

            return hitInfo + hitTypeInfo;
        }

        private static string FormatHitSingle(string text, int combinedTotal, int charTotal, int petTotal)
        {
            return
                $"<font color='#{Colors.Info}'> {text}:</font> " +
                $"{combinedTotal}" +
                $"<font color='#{Colors.Accent}'> - </font>" +
                $"<font color='#{Colors.Info}'>User:</font> " +
                $"{charTotal}" +
                $"<font color='#{Colors.Accent}'> - </font>" +
                $"<font color='#{Colors.Info}'>Pet:</font> " +
                $"{petTotal}<br>";
        }

        public static string TotalDmgFormat(int totalDamage)
        {
            if (totalDamage < 100000)
                return totalDamage.ToString();
            else if (totalDamage < 1000000)
                return string.Format("{0:0.00}", (double)totalDamage / 1000) + "K";
            else
                return string.Format("{0:0.00}", (double)totalDamage / 1000000) + "M";
        }

        public static string PercentFormat(float number)
        {
            return string.Format("{0:0.0}", number * 100) + "%";
        }

        public static string DpmFormat(int totalDamage, double elapsedTime)
        {
            var dpm = (float)totalDamage * 60 / elapsedTime;

            if (dpm < 1000)
                return string.Format("{0:0.0}", dpm);
            else
                return string.Format("{0:0.00}", dpm / 1000) + "K";
        }
    }
}