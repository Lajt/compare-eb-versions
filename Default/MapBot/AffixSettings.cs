using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Default.EXtensions;
using Loki;
using Newtonsoft.Json;

namespace Default.MapBot
{
    public class AffixSettings
    {
        private static readonly string SettingsPath = Path.Combine(Configuration.Instance.Path, "MapBot", "AffixSettings.json");

        private static AffixSettings _instance;
        public static AffixSettings Instance => _instance ?? (_instance = new AffixSettings());

        private AffixSettings()
        {
            InitList();
            Load();
            InitDict();
            Configuration.OnSaveAll += (sender, args) => { Save(); };

            AffixList = AffixList
                .OrderByDescending(a => a.RerollMagic)
                .ThenByDescending(a => a.RerollRare)
                .ThenBy(a => a.Name)
                .ToList();
        }

        public List<AffixData> AffixList { get; } = new List<AffixData>();
        public Dictionary<string, AffixData> AffixDict { get; } = new Dictionary<string, AffixData>();

        private void InitList()
        {
            var br = Environment.NewLine;

            AffixList.Add(new AffixData("Abhorrent", "Area is inhabited by Abominations"));
            AffixList.Add(new AffixData("Anarchic", "Area is inhabited by 2 additional Rogue Exiles"));
            AffixList.Add(new AffixData("Antagonist's", "Rare Monsters each have a Nemesis Mod" + br + "X% more Rare Monsters"));
            AffixList.Add(new AffixData("Bipedal", "Area is inhabited by Humanoids"));
            AffixList.Add(new AffixData("Capricious", "Area is inhabited by Goatmen"));
            AffixList.Add(new AffixData("Ceremonial", "Area contains many Totems"));
            AffixList.Add(new AffixData("Chaining", "Monsters' skills Chain 2 additional times"));
            AffixList.Add(new AffixData("Conflagrating", "All Monster Damage from Hits always Ignites"));
            AffixList.Add(new AffixData("Demonic", "Area is inhabited by Demons"));
            AffixList.Add(new AffixData("Emanant", "Area is inhabited by ranged monsters"));
            AffixList.Add(new AffixData("Feasting", "Area is inhabited by Cultists of Kitava"));
            AffixList.Add(new AffixData("Feral", "Area is inhabited by Animals"));
            AffixList.Add(new AffixData("Haunting", "Area is inhabited by Ghosts"));
            AffixList.Add(new AffixData("Lunar", "Area is inhabited by Lunaris fanatics"));
            AffixList.Add(new AffixData("Multifarious", "Area has increased monster variety"));
            AffixList.Add(new AffixData("Otherworldly", "Slaying Enemies close together can attract monsters from Beyond"));
            AffixList.Add(new AffixData("Skeletal", "Area is inhabited by Skeletons"));
            AffixList.Add(new AffixData("Slithering", "Area is inhabited by Sea Witches and their Spawn"));
            AffixList.Add(new AffixData("Solar", "Area is inhabited by Solaris fanatics"));
            AffixList.Add(new AffixData("Twinned", "Area contains two Unique Bosses"));
            AffixList.Add(new AffixData("Undead", "Area is inhabited by Undead"));
            AffixList.Add(new AffixData("Unstoppable", "Monsters cannot be slowed below base speed" + br + "Monsters cannot be Taunted"));
            AffixList.Add(new AffixData("Armoured", "+X% Monster Physical Damage Reduction"));
            AffixList.Add(new AffixData("Burning", "Monsters deal X% extra Damage as Fire"));
            AffixList.Add(new AffixData("Fecund", "X% more Monster Life"));
            AffixList.Add(new AffixData("Fleet", "X% increased Monster Movement Speed" + br + "X% increased Monster Attack Speed" + br + "X% increased Monster Cast Speed"));
            AffixList.Add(new AffixData("Freezing", "Monsters deal X% extra Damage as Cold"));
            AffixList.Add(new AffixData("Hexwarded", "X% less effect of Curses on Monsters"));
            AffixList.Add(new AffixData("Hexproof", "Monsters are Hexproof"));
            AffixList.Add(new AffixData("Impervious", "Monsters have a X% chance to avoid Poison, Blind, and Bleed"));
            AffixList.Add(new AffixData("Mirrored", "Monsters reflect X% of Elemental Damage"));
            AffixList.Add(new AffixData("Overlord's", "Unique Boss deals X% increased Damage" + br + "Unique Boss has X% increased Attack and Cast Speed"));
            AffixList.Add(new AffixData("Punishing", "Monsters reflect X% of Physical Damage"));
            AffixList.Add(new AffixData("Resistant", "+X% Monster Chaos Resistance" + br + "+X% Monster Elemental Resistance"));
            AffixList.Add(new AffixData("Savage", "X% increased Monster Damage"));
            AffixList.Add(new AffixData("Shocking", "Monsters deal X% extra Damage as Lightning"));
            AffixList.Add(new AffixData("Splitting", "Monsters fire 2 additional Projectiles"));
            AffixList.Add(new AffixData("Titan's", "Unique Boss has X% increased Life" + br + "Unique Boss has X% increased Area of Effect"));
            AffixList.Add(new AffixData("Unwavering", "Monsters cannot be Stunned" + br + "X% more Monster Life"));
            AffixList.Add(new AffixData("Empowered", "Monsters have a X% chance to cause Elemental Ailments on Hit"));
            AffixList.Add(new AffixData("of Balance", "Players have Elemental Equilibrium"));
            AffixList.Add(new AffixData("of Bloodlines", "Magic Monster Packs each have a Bloodline Mod" + br + "X% more Magic Monsters"));
            AffixList.Add(new AffixData("of Endurance", "Monsters gain an Endurance Charge on Hit"));
            AffixList.Add(new AffixData("of Frenzy", "Monsters gain a Frenzy Charge on Hit"));
            AffixList.Add(new AffixData("of Power", "Monsters gain a Power Charge on Hit"));
            AffixList.Add(new AffixData("of Skirmishing", "Players have Point Blank"));
            AffixList.Add(new AffixData("of Venom", "Monsters Poison on Hit"));
            AffixList.Add(new AffixData("of Deadliness", "Monsters have X% increased Critical Strike Chance" + br + "+X% to Monster Critical Strike Multiplier"));
            AffixList.Add(new AffixData("of Desecration", "Area has patches of desecrated ground"));
            AffixList.Add(new AffixData("of Drought", "Players gain X% reduced Flask Charges"));
            AffixList.Add(new AffixData("of Flames", "Area has patches of burning ground"));
            AffixList.Add(new AffixData("of Giants", "Monsters have X% increased Area of Effect"));
            AffixList.Add(new AffixData("of Ice", "Area has patches of chilled ground"));
            AffixList.Add(new AffixData("of Impotence", "Players have X% less Area of Effect"));
            AffixList.Add(new AffixData("of Insulation", "Monsters have X% chance to Avoid Elemental Status Ailments"));
            AffixList.Add(new AffixData("of Lightning", "Area has patches of shocking ground"));
            AffixList.Add(new AffixData("of Miring", "Player Dodge chance is Unlucky" + br + "Monsters have X% increased Accuracy Rating"));
            AffixList.Add(new AffixData("of Rust", "Players have X% reduced Block Chance" + br + "Players have X% less Armour"));
            AffixList.Add(new AffixData("of Smothering", "Players have X% less Recovery Rate of Life and Energy Shield"));
            AffixList.Add(new AffixData("of Toughness", "Monsters take X% reduced Extra Damage from Critical Strikes"));
            AffixList.Add(new AffixData("of Elemental Weakness", "Players are Cursed with Elemental Weakness"));
            AffixList.Add(new AffixData("of Enfeeblement", "Players are Cursed with Enfeeble"));
            AffixList.Add(new AffixData("of Exposure", "-X% maximum Player Resistances"));
            AffixList.Add(new AffixData("of Stasis", "Players cannot Regenerate Life, Mana or Energy Shield"));
            AffixList.Add(new AffixData("of Temporal Chains", "Players are Cursed with Temporal Chains"));
            AffixList.Add(new AffixData("of Vulnerability", "Players are Cursed with Vulnerability"));
            AffixList.Add(new AffixData("of Congealment", "Cannot Leech Life from Monsters" + br + "Cannot Leech Mana from Monsters"));
        }

        private void InitDict()
        {
            foreach (var data in AffixList)
            {
                AffixDict.Add(data.Name, data);
            }
        }

        private void Load()
        {
            if (!File.Exists(SettingsPath))
                return;

            var json = File.ReadAllText(SettingsPath);
            if (string.IsNullOrWhiteSpace(json))
            {
                GlobalLog.Error("[MapBot] Fail to load \"AffixSettings.json\". File is empty.");
                return;
            }
            var parts = JsonConvert.DeserializeObject<Dictionary<string, EditablePart>>(json);
            if (parts == null)
            {
                GlobalLog.Error("[MapBot] Fail to load \"AffixSettings.json\". Json deserealizer returned null.");
                return;
            }
            foreach (var data in AffixList)
            {
                if (parts.TryGetValue(data.Name, out EditablePart part))
                {
                    data.RerollMagic = part.RerollMagic;
                    data.RerollRare = part.RerollRare;
                }
            }
        }

        private void Save()
        {
            var parts = new Dictionary<string, EditablePart>(AffixList.Count);

            foreach (var data in AffixList)
            {
                var part = new EditablePart
                {
                    RerollMagic = data.RerollMagic,
                    RerollRare = data.RerollRare
                };
                parts.Add(data.Name, part);
            }
            var json = JsonConvert.SerializeObject(parts, Formatting.Indented);
            File.WriteAllText(SettingsPath, json);
        }

        private class EditablePart
        {
            public bool RerollMagic;
            public bool RerollRare;
        }
    }
}