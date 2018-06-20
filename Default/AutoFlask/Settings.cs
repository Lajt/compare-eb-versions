using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Loki;
using Loki.Common;
using Loki.Game.GameData;
using Newtonsoft.Json;

namespace Default.AutoFlask
{
    public class Settings : JsonSettings
    {
        private static Settings _instance;
        public static Settings Instance => _instance ?? (_instance = new Settings());

        private Settings()
            : base(GetSettingsFilePath(Configuration.Instance.Name, "AutoFlask.json"))
        {
            InitFlaskList(ref _utilityFlasks, GetDefaultUtilityFlaskList);
            InitFlaskList(ref _uniqueFlasks, GetDefaultUniqueFlaskList);
        }

        public int HpPercent { get; set; } = 75;
        public int HpPercentInstant { get; set; } = 50;
        public int MpPercent { get; set; } = 30;
        public int QsilverRange { get; set; } = 80;

        public bool RemoveFreeze { get; set; }
        public bool RemoveShock { get; set; }
        public bool RemoveIgnite { get; set; }
        public bool RemoveSilence { get; set; }
        public bool RemoveBleed { get; set; }
        public bool RemoveCblood { get; set; }
        public int MinCbloodStacks { get; set; } = 1;
        public bool RemovePoison { get; set; }
        public int MinPoisonStacks { get; set; } = 1;

        private readonly List<FlaskEntry> _utilityFlasks = new List<FlaskEntry>();
        private readonly List<FlaskEntry> _uniqueFlasks = new List<FlaskEntry>();

        public List<FlaskEntry> UtilityFlasks => _utilityFlasks;
        public List<FlaskEntry> UniqueFlasks => _uniqueFlasks;

        public List<FlaskTrigger> GetFlaskTriggers(string name)
        {
            foreach (var flask in _utilityFlasks)
            {
                if (flask.Name == name)
                    return flask.Triggers.ToList();
            }
            foreach (var flask in _uniqueFlasks)
            {
                if (flask.Name == name)
                    return flask.Triggers.ToList();
            }
            return null;
        }

        private static List<FlaskEntry> GetDefaultUtilityFlaskList()
        {
            return new List<FlaskEntry>
            {
                new FlaskEntry(FlaskNames.Amethyst),
                new FlaskEntry(FlaskNames.Aquamarine),
                new FlaskEntry(FlaskNames.Basalt),
                new FlaskEntry(FlaskNames.Bismuth),
                new FlaskEntry(FlaskNames.Diamond),
                new FlaskEntry(FlaskNames.Granite),
                new FlaskEntry(FlaskNames.Jade),
                new FlaskEntry(FlaskNames.Quartz),
                new FlaskEntry(FlaskNames.Ruby),
                new FlaskEntry(FlaskNames.Sapphire),
                new FlaskEntry(FlaskNames.Silver),
                new FlaskEntry(FlaskNames.Stibnite),
                new FlaskEntry(FlaskNames.Sulphur),
                new FlaskEntry(FlaskNames.Topaz),
            };
        }

        private static List<FlaskEntry> GetDefaultUniqueFlaskList()
        {
            return new List<FlaskEntry>
            {
                new FlaskEntry(FlaskNames.AtziriPromise),
                new FlaskEntry(FlaskNames.CoralitoSignature),
                new FlaskEntry(FlaskNames.CoruscatingElixir),
                new FlaskEntry(FlaskNames.DivinationDistillate),
                new FlaskEntry(FlaskNames.DyingSun),
                new FlaskEntry(FlaskNames.ForbiddenTaste),
                new FlaskEntry(FlaskNames.KiaraDetermination),
                new FlaskEntry(FlaskNames.LionRoar),
                new FlaskEntry(FlaskNames.OverflowingChalice),
                new FlaskEntry(FlaskNames.RumiConcoction),
                new FlaskEntry(FlaskNames.SinRebirth),
                new FlaskEntry(FlaskNames.SorrowOfDivine),
                new FlaskEntry(FlaskNames.TasteOfHate),
                new FlaskEntry(FlaskNames.VesselOfVinktar),
                new FlaskEntry(FlaskNames.WiseOak),
                new FlaskEntry(FlaskNames.WitchfireBrew),
            };
        }

        private static void InitFlaskList(ref List<FlaskEntry> jsonList, Func<List<FlaskEntry>> getDefaulList)
        {
            if (jsonList.Count == 0)
            {
                jsonList = getDefaulList();
            }
            else
            {
                var defaultList = getDefaulList();
                foreach (var defaultEntry in defaultList)
                {
                    var jsonEntry = jsonList.Find(f => f.Name == defaultEntry.Name);
                    if (jsonEntry != null) defaultEntry.Triggers = jsonEntry.Triggers;
                }
                jsonList = defaultList;
            }
        }

        public class FlaskEntry
        {
            public string Name { get; }
            public ObservableCollection<FlaskTrigger> Triggers { get; set; }

            public FlaskEntry(string name)
            {
                Name = name;
                Triggers = new ObservableCollection<FlaskTrigger>();
            }
        }

        [JsonIgnore]
        public static readonly TriggerType[] TriggerTypes = {TriggerType.Hp, TriggerType.Es, TriggerType.Mobs, TriggerType.Attack};

        [JsonIgnore]
        public static readonly Rarity[] Rarities = {Rarity.Normal, Rarity.Magic, Rarity.Rare, Rarity.Unique};
    }
}