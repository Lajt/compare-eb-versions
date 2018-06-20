using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Default.EXtensions.CommonTasks;
using Loki;
using Loki.Bot;
using Loki.Common;
using Loki.Game.GameData;
using Newtonsoft.Json;

namespace Default.EXtensions
{
    public class Settings : JsonSettings
    {
        private static Settings _instance;
        public static Settings Instance => _instance ?? (_instance = new Settings());

        private Settings()
            : base(GetSettingsFilePath(Configuration.Instance.Name, "EXtensions.json"))
        {
            InitGeneralStashingRules();
            InitCurrencyStashingRules();
            InitChestEntries(ref _chests, GetDefaultChestList);
            InitChestEntries(ref _strongboxes, GetDefaultStrongboxList);
            InitChestEntries(ref _shrines, GetDefaultShrineList);

            if (InventoryCurrencies == null)
            {
                InventoryCurrencies = new ObservableCollection<InventoryCurrency>
                {
                    new InventoryCurrency(CurrencyNames.Wisdom, 5, 12),
                    new InventoryCurrency(CurrencyNames.Portal, 5, 11),
                };
            }
        }

        #region Stashing

        public List<StashingRule> GeneralStashingRules { get; private set; } = new List<StashingRule>();
        public List<TogglableStashingRule> CurrencyStashingRules { get; private set; } = new List<TogglableStashingRule>();

        public readonly List<FullTabInfo> FullTabs = new List<FullTabInfo>();

        public static class StashingCategory
        {
            public const string Currency = "Currency";
            public const string Rare = "Rares";
            public const string Unique = "Uniques";
            public const string Gem = "Gems";
            public const string Card = "Cards";
            public const string Prophecy = "Prophecies";
            public const string Essence = "Essences";
            public const string Jewel = "Jewels";
            public const string Map = "Maps";
            public const string Fragment = "Fragments";
            public const string Leaguestone = "Leaguestones";
            public const string Other = "Other";
        }

        public static class CurrencyGroup
        {
            public const string Sextant = "Sextants";
            public const string Breach = "Breach";
        }

        public List<string> GetTabsForCategory(string categoryName)
        {
            var rule = GeneralStashingRules.Find(r => r.Name == categoryName);
            if (rule == null)
            {
                GlobalLog.Error($"[EXtensions] Stashing rule requested for unknown name: \"{categoryName}\".");
                return GeneralStashingRules.Find(r => r.Name == StashingCategory.Other).TabList;
            }
            return rule.TabList;
        }

        public List<string> GetTabsForCurrency(string currencyName)
        {
            if (currencyName.EndsWith("Sextant"))
                return GetIndividualOrDefault(CurrencyGroup.Sextant);

            if (BreachCurrency.Contains(currencyName))
                return GetIndividualOrDefault(CurrencyGroup.Breach);

            if (CurrencyNames.ShardToCurrencyDict.TryGetValue(currencyName, out var notShard))
                currencyName = notShard;

            return GetIndividualOrDefault(currencyName);
        }

        private List<string> GetIndividualOrDefault(string currencyName)
        {
            var rule = CurrencyStashingRules.Find(r => r.Enabled && r.Name == currencyName);
            if (rule != null) return rule.TabList;
            return GeneralStashingRules.Find(r => r.Name == StashingCategory.Currency).TabList;
        }

        public bool IsTabFull(string tabName, string itemMetadata)
        {
            foreach (var tab in FullTabs)
            {
                if (tab.Name != tabName)
                    continue;

                var metadata = tab.ControlsMetadata;
                return metadata.Count == 0 || metadata.Contains(itemMetadata);
            }
            return false;
        }

        public void MarkTabAsFull(string tabName, string itemMetadata)
        {
            var tab = FullTabs.Find(t => t.Name == tabName);
            if (tab == null)
            {
                FullTabs.Add(new FullTabInfo(tabName, itemMetadata));
                GlobalLog.Debug($"[MarkTabAsFull] New tab added. Name: \"{tabName}\". Metadata: \"{itemMetadata ?? "null"}\".");
            }
            else if (itemMetadata != null)
            {
                tab.ControlsMetadata.Add(itemMetadata);
                GlobalLog.Debug($"[MarkTabAsFull] Existing tab updated. Name: \"{tabName}\". Metadata: \"{itemMetadata}\".");
            }
            else
            {
                GlobalLog.Debug($"[MarkTabAsFull] \"{tabName}\" is already marked as full.");
            }
        }

        private static List<StashingRule> GetDefaultGeneralStashingRules()
        {
            return new List<StashingRule>
            {
                new StashingRule(StashingCategory.Currency, "1"),
                new StashingRule(StashingCategory.Rare, "2"),
                new StashingRule(StashingCategory.Unique, "2"),
                new StashingRule(StashingCategory.Gem, "3"),
                new StashingRule(StashingCategory.Card, "3"),
                new StashingRule(StashingCategory.Prophecy, "3"),
                new StashingRule(StashingCategory.Essence, "3"),
                new StashingRule(StashingCategory.Jewel, "3"),
                new StashingRule(StashingCategory.Map, "4"),
                new StashingRule(StashingCategory.Fragment, "4"),
                new StashingRule(StashingCategory.Leaguestone, "4"),
                new StashingRule(StashingCategory.Other, "4"),
            };
        }

        private static List<TogglableStashingRule> GetDefaultCurrencyStashingRules()
        {
            return new List<TogglableStashingRule>
            {
                new TogglableStashingRule(CurrencyGroup.Breach, "1"),
                new TogglableStashingRule(CurrencyGroup.Sextant, "1"),
                new TogglableStashingRule(CurrencyNames.SilverCoin, "1"),
                new TogglableStashingRule(CurrencyNames.PerandusCoin, "1"),
                new TogglableStashingRule(CurrencyNames.Wisdom, "1"),
                new TogglableStashingRule(CurrencyNames.Portal, "1"),
                new TogglableStashingRule(CurrencyNames.Transmutation, "1"),
                new TogglableStashingRule(CurrencyNames.Augmentation, "1"),
                new TogglableStashingRule(CurrencyNames.Alteration, "1"),
                new TogglableStashingRule(CurrencyNames.Scrap, "1"),
                new TogglableStashingRule(CurrencyNames.Whetstone, "1"),
                new TogglableStashingRule(CurrencyNames.Glassblower, "1"),
                new TogglableStashingRule(CurrencyNames.Chisel, "1"),
                new TogglableStashingRule(CurrencyNames.Chromatic, "1"),
                new TogglableStashingRule(CurrencyNames.Chance, "1"),
                new TogglableStashingRule(CurrencyNames.Alchemy, "1"),
                new TogglableStashingRule(CurrencyNames.Jeweller, "1"),
                new TogglableStashingRule(CurrencyNames.Scouring, "1"),
                new TogglableStashingRule(CurrencyNames.Fusing, "1"),
                new TogglableStashingRule(CurrencyNames.Blessed, "1"),
                new TogglableStashingRule(CurrencyNames.Regal, "1"),
                new TogglableStashingRule(CurrencyNames.Chaos, "1"),
                new TogglableStashingRule(CurrencyNames.Vaal, "1"),
                new TogglableStashingRule(CurrencyNames.Regret, "1"),
                new TogglableStashingRule(CurrencyNames.Gemcutter, "1"),
                new TogglableStashingRule(CurrencyNames.Divine, "1"),
                new TogglableStashingRule(CurrencyNames.Exalted, "1"),
                new TogglableStashingRule(CurrencyNames.Eternal, "1"),
                new TogglableStashingRule(CurrencyNames.Mirror, "1"),
                new TogglableStashingRule(CurrencyNames.Annulment, "1"),
                new TogglableStashingRule(CurrencyNames.Binding, "1"),
                new TogglableStashingRule(CurrencyNames.Horizon, "1"),
                new TogglableStashingRule(CurrencyNames.Harbinger, "1"),
                new TogglableStashingRule(CurrencyNames.Engineer, "1"),
                new TogglableStashingRule(CurrencyNames.Ancient, "1"),
            };
        }

        private void InitGeneralStashingRules()
        {
            if (GeneralStashingRules.Count == 0)
            {
                GeneralStashingRules = GetDefaultGeneralStashingRules();
            }
            else
            {
                var defaultRules = GetDefaultGeneralStashingRules();
                foreach (var defaultRule in defaultRules)
                {
                    var jsonRule = GeneralStashingRules.Find(c => c.Name == defaultRule.Name);
                    if (jsonRule != null) defaultRule.CopyContents(jsonRule);
                }
                GeneralStashingRules = defaultRules;
            }
            foreach (var rule in GeneralStashingRules)
            {
                rule.FillTabList();
            }
        }

        private void InitCurrencyStashingRules()
        {
            if (CurrencyStashingRules.Count == 0)
            {
                CurrencyStashingRules = GetDefaultCurrencyStashingRules();
            }
            else
            {
                var defaultRules = GetDefaultCurrencyStashingRules();
                foreach (var defaultRule in defaultRules)
                {
                    var jsonRule = CurrencyStashingRules.Find(c => c.Name == defaultRule.Name);
                    if (jsonRule != null) defaultRule.CopyContents(jsonRule);
                }
                CurrencyStashingRules = defaultRules;
            }
            foreach (var rule in CurrencyStashingRules)
            {
                rule.FillTabList();
            }
        }

        public class StashingRule
        {
            public string Name { get; }

            // ReSharper disable once InconsistentNaming
            protected string _tabs;

            public string Tabs
            {
                get => _tabs;
                set
                {
                    if (value == _tabs) return;
                    _tabs = value;
                    FillTabList();
                    StashTask.RequestInvalidTabCheck();
                }
            }

            [JsonIgnore]
            public List<string> TabList { get; set; }

            public StashingRule(string name, string tabs)
            {
                Name = name;
                _tabs = tabs;
                TabList = new List<string>();
            }

            public void FillTabList()
            {
                try
                {
                    Parse(_tabs, TabList);
                }
                catch (Exception ex)
                {
                    if (BotManager.IsRunning)
                    {
                        GlobalLog.Error($"Parsing error in \"{_tabs}\".");
                        GlobalLog.Error(ex.Message);
                        BotManager.Stop();
                    }
                    else
                    {
                        MessageBoxes.Error($"Parsing error in \"{_tabs}\".\n{ex.Message}");
                    }
                }
            }

            private static void Parse(string str, ICollection<string> list)
            {
                if (str == string.Empty)
                    throw new Exception("Stashing setting cannot be empty.");

                list.Clear();

                var commaParams = str.Split(',');
                foreach (var param in commaParams)
                {
                    var trimmed = param.Trim();
                    if (trimmed == string.Empty)
                        throw new Exception("Remove double commas and/or commas from the start/end of the string.");

                    if (!ParseRange(trimmed, list))
                    {
                        list.Add(trimmed);
                    }
                }
            }

            private static bool ParseRange(string str, ICollection<string> list)
            {
                var hyphenParams = str.Split('-');
                if (hyphenParams.Length == 2)
                {
                    var start = hyphenParams[0].Trim();
                    var end = hyphenParams[1].Trim();

                    if (!int.TryParse(start, out var first))
                        throw new Exception($"Invalid parameter \"{start}\". Only numeric values are supported with range delimiter.");

                    if (!int.TryParse(end, out var last))
                        throw new Exception($"Invalid parameter \"{end}\". Only numeric values are supported with range delimiter.");

                    list.Add(start);

                    for (int i = first + 1; i < last; ++i)
                    {
                        list.Add(i.ToString());
                    }
                    list.Add(end);
                    return true;
                }
                if (hyphenParams.Length == 1) return false;
                throw new Exception($"Invalid range string: \"{str}\". Supported format: \"X-Y\".");
            }

            internal void CopyContents(StashingRule other)
            {
                _tabs = other._tabs;
            }
        }

        public class TogglableStashingRule : StashingRule
        {
            public bool Enabled { get; set; }

            public TogglableStashingRule(string name, string tabs, bool enabled = false)
                : base(name, tabs)
            {
                Enabled = enabled;
            }

            internal void CopyContents(TogglableStashingRule other)
            {
                _tabs = other._tabs;
                Enabled = other.Enabled;
            }
        }

        public class FullTabInfo
        {
            public readonly string Name;
            public readonly List<string> ControlsMetadata = new List<string>();

            public FullTabInfo(string name, string metadata)
            {
                Name = name;
                if (metadata != null)
                    ControlsMetadata.Add(metadata);
            }
        }

        #endregion

        #region Vendoring

        public bool CardsEnabled { get; set; }
        public int MinCardSets { get; set; } = 5;
        public int MaxCardSets { get; set; } = 15;

        public bool GcpEnabled { get; set; }
        public int GcpMaxQ { get; set; } = 19;
        public int GcpMaxLvl { get; set; } = 19;

        public string CurrencyExchangeAct { get; set; } = "5";
        public ExchangeSettings TransExchange { get; set; } = new ExchangeSettings(160, 5);
        public ExchangeSettings AugsExchange { get; set; } = new ExchangeSettings(120, 10);
        public ExchangeSettings AltsExchange { get; set; } = new ExchangeSettings(100, 10);
        public ExchangeSettings JewsExchange { get; set; } = new ExchangeSettings(100, 0);
        public ExchangeSettings ChancesExchange { get; set; } = new ExchangeSettings(100, 0);
        public ExchangeSettings ScoursExchange { get; set; } = new ExchangeSettings(30, 5);

        public class ExchangeSettings
        {
            public bool Enabled { get; set; }
            public int Min { get; set; }
            public int Save { get; set; }

            public ExchangeSettings(int min, int save)
            {
                Min = min;
                Save = save;
            }
        }

        #endregion

        #region Chests

        public int ChestOpenRange { get; set; } = 50;
        public int StrongboxOpenRange { get; set; } = -1;
        public int ShrineOpenRange { get; set; } = -1;
        public Rarity MaxStrongboxRarity { get; set; } = Rarity.Unique;

        private readonly List<ChestEntry> _chests = new List<ChestEntry>();
        private readonly List<ChestEntry> _strongboxes = new List<ChestEntry>();
        private readonly List<ChestEntry> _shrines = new List<ChestEntry>();

        public List<ChestEntry> Chests => _chests;
        public List<ChestEntry> Strongboxes => _strongboxes;
        public List<ChestEntry> Shrines => _shrines;

        private static List<ChestEntry> GetDefaultChestList()
        {
            // !OpensOnDamage only
            return new List<ChestEntry>
            {
                new ChestEntry("Chest"),
                new ChestEntry("Bone Chest"),
                new ChestEntry("Golden Chest"),
                new ChestEntry("Tribal Chest"),
                new ChestEntry("Sarcophagus"),
                new ChestEntry("Boulder"),
                new ChestEntry("Trunk"),
                new ChestEntry("Cocoon"),
                new ChestEntry("Corpse"),
                new ChestEntry("Bound Corpse"),
                new ChestEntry("Crucified Corpse"),
                new ChestEntry("Impaled Corpse"),
                new ChestEntry("Armour Rack"),
                new ChestEntry("Weapon Rack"),
                new ChestEntry("Scribe's Rack")
            };
        }

        private static List<ChestEntry> GetDefaultStrongboxList()
        {
            return new List<ChestEntry>
            {
                new ChestEntry("Arcanist's Strongbox"),
                new ChestEntry("Armourer's Strongbox"),
                new ChestEntry("Artisan's Strongbox"),
                new ChestEntry("Blacksmith's Strongbox"),
                new ChestEntry("Cartographer's Strongbox"),
                new ChestEntry("Diviner's Strongbox"),
                new ChestEntry("Gemcutter's Strongbox"),
                new ChestEntry("Jeweller's Strongbox"),
                new ChestEntry("Large Strongbox"),
                new ChestEntry("Ornate Strongbox"),
                new ChestEntry("Strongbox")
            };
        }

        private static List<ChestEntry> GetDefaultShrineList()
        {
            return new List<ChestEntry>
            {
                new ChestEntry("Acceleration Shrine"),
                new ChestEntry("Brutal Shrine"),
                new ChestEntry("Diamond Shrine"),
                new ChestEntry("Divine Shrine", false),
                new ChestEntry("Echoing Shrine"),
                new ChestEntry("Freezing Shrine"),
                new ChestEntry("Impenetrable Shrine"),
                new ChestEntry("Lightning Shrine"),
                new ChestEntry("Massive Shrine"),
                new ChestEntry("Replenishing Shrine"),
                new ChestEntry("Resistance Shrine"),
                new ChestEntry("Shrouded Shrine"),
                new ChestEntry("Skeletal Shrine")
            };
        }

        private static void InitChestEntries(ref List<ChestEntry> jsonList, Func<List<ChestEntry>> getDefaulList)
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
                    var jsonEntry = jsonList.Find(c => c.Name == defaultEntry.Name);
                    if (jsonEntry != null) defaultEntry.Enabled = jsonEntry.Enabled;
                }
                jsonList = defaultList;
            }
        }

        public class ChestEntry
        {
            public string Name { get; }
            public bool Enabled { get; set; }

            public ChestEntry(string name, bool enabled = true)
            {
                Name = name;
                Enabled = enabled;
            }
        }

        #endregion

        #region Stuck detection

        public bool StuckDetectionEnabled { get; set; } = true;
        public int MaxStucksPerInstance { get; set; } = 3;
        public int MaxStuckCountSmall { get; set; } = 8;
        public int MaxStuckCountMedium { get; set; } = 15;
        public int MaxStuckCountLong { get; set; } = 30;

        #endregion

        #region Misc

        public bool SellExcessPortals { get; set; } = true;
        public bool LootVisibleItems { get; set; }
        public bool UseChatForHideout { get; set; }
        public int MinInventorySquares { get; set; } = 0;

        public bool ArtificialDelays { get; set; } = true;
        public int MinArtificialDelay { get; set; } = 200;
        public int MaxArtificialDelay { get; set; } = 300;
        public bool AutoDnd { get; set; }
        public string DndMessage { get; set; }
        public ObservableCollection<InventoryCurrency> InventoryCurrencies { get; set; }

        public class InventoryCurrency
        {
            public const string DefaultName = "CurrencyName";

            private string _name;
            private int _row;
            private int _column;

            public string Name
            {
                get => _name;
                set => _name = string.IsNullOrWhiteSpace(value) ? DefaultName : value.Trim();
            }

            public int Row
            {
                get => _row;
                set
                {
                    if (value == 0)
                    {
                        _row = _row == 1 ? -1 : 1;
                    }
                    else
                    {
                        _row = value;
                    }
                }
            }

            public int Column
            {
                get => _column;
                set
                {
                    if (value == 0)
                    {
                        _column = _column == 1 ? -1 : 1;
                    }
                    else
                    {
                        _column = value;
                    }
                }
            }

            public int Restock { get; set; }

            public InventoryCurrency()
            {
                Name = DefaultName;
                Row = -1;
                Column = -1;
                Restock = -1;
            }

            public InventoryCurrency(string name, int row, int column, int restock = -1)
            {
                Name = name;
                Row = row;
                Column = column;
                Restock = restock;
            }
        }

        #endregion

        [JsonIgnore]
        public static readonly Rarity[] Rarities = {Rarity.Normal, Rarity.Magic, Rarity.Rare, Rarity.Unique};

        [JsonIgnore]
        public static readonly string[] CurrencyExchangeActs = {"3", "4", "5", "6", "7", "8", "9", "10", "11", "Random"};

        [JsonIgnore]
        private static readonly HashSet<string> BreachCurrency = new HashSet<string>
        {
            "Splinter of Xoph",
            "Splinter of Tul",
            "Splinter of Esh",
            "Splinter of Uul-Netol",
            "Splinter of Chayula",
            "Blessing of Xoph",
            "Blessing of Tul",
            "Blessing of Esh",
            "Blessing of Uul-Netol",
            "Blessing of Chayula",
        };
    }
}