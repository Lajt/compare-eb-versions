using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Default.EXtensions;
using Loki;
using Loki.Common;
using Loki.Game.GameData;
using Newtonsoft.Json;

namespace Default.QuestBot
{
    public class Settings : JsonSettings
    {
        private static Settings _instance;
        public static Settings Instance => _instance ?? (_instance = new Settings());

        private Settings()
            : base(GetSettingsFilePath(Configuration.Instance.Name, "QuestBot.json"))
        {
            InitBosses();
            InitOptionalQuests();
            InitRewardQuests();
        }

        #region Current quest

        private string _currentQuestName;
        private string _currentQuestState;

        [JsonIgnore]
        public string CurrentQuestName
        {
            get => _currentQuestName;
            set
            {
                if (value == _currentQuestName) return;
                _currentQuestName = value;
                NotifyPropertyChanged(() => CurrentQuestName);
            }
        }

        [JsonIgnore]
        public string CurrentQuestState
        {
            get => _currentQuestState;
            set
            {
                if (value == _currentQuestState) return;
                _currentQuestState = value;
                NotifyPropertyChanged(() => CurrentQuestState);
            }
        }

        #endregion

        #region Grinding

        public int ExplorationPercent { get; set; } = 85;
        public int MaxDeaths { get; set; } = 7;
        public bool TrackMob { get; set; }

        public ObservableCollection<GrindingRule> GrindingRules { get; set; } = new ObservableCollection<GrindingRule>();

        [JsonIgnore]
        public static List<Quest> QuestList
        {
            get
            {
                var list = new List<Quest>();
                foreach (var quest in Quests.All)
                {
                    if (quest == Quests.RibbonSpool || //completes along with Fiery Dust
                        quest == Quests.SwigOfHope || //early decanter messes this up
                        quest == Quests.EndToHunger) //epilogue one should be used
                        continue;

                    list.Add(quest);
                }
                return list;
            }
        }

        [JsonIgnore]
        public static List<Area> AreaList
        {
            get
            {
                var list = new List<Area>();
                foreach (var act in typeof(World).GetNestedTypes())
                {
                    foreach (var field in act.GetFields())
                    {
                        var area = field.GetValue(field) as AreaInfo;

                        if (area == null)
                            continue;

                        var id = area.Id;
                        if (id == World.Act1.TwilightStrand.Id ||
                            id == World.Act7.MaligaroSanctum.Id ||
                            id == World.Act11.TemplarLaboratory.Id ||
                            area.Id.Contains("town"))
                            continue;

                        list.Add(new Area(area));
                    }
                }
                return list;
            }
        }

        public class GrindingRule
        {
            public Quest Quest { get; set; } = Quests.EnemyAtTheGate;
            public int LevelCap { get; set; } = 100;
            public ObservableCollection<GrindingArea> Areas { get; set; } = new ObservableCollection<GrindingArea>();
        }

        public class GrindingArea
        {
            public Area Area { get; set; } = new Area(World.Act1.Coast);
            public int Pool { get; set; } = 1;
        }

        public class Area : IEquatable<Area>
        {
            public string Id { get; set; }
            public string Name { get; set; }

            public Area()
            {
            }

            public Area(AreaInfo area)
            {
                Id = area.Id;
                Name = area.Name;
            }

            public bool Equals(Area other)
            {
                return this == other;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as Area);
            }

            public static bool operator ==(Area left, Area right)
            {
                if (ReferenceEquals(left, right))
                    return true;

                if (((object) left == null) || ((object) right == null))
                    return false;

                return left.Id == right.Id;
            }

            public static bool operator !=(Area left, Area right)
            {
                return !(left == right);
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }

        #endregion

        #region Manual bosses

        public bool NotifyBoss { get; set; }

        public List<Boss> Bosses { get; set; } = new List<Boss>();

        [JsonIgnore]
        public List<ActGroup<Boss>> BossesByAct => GroupByAct(Bosses, NextBossAct);

        public bool StopBeforeBoss(string bossName)
        {
            return Bosses.Exists(b => b.Enabled && b.Name == bossName);
        }

        public static class BossNames
        {
            public const string Brutus = "Brutus";
            public const string Merveil = "Merveil";
            public const string Alira = BanditHelper.AliraName;
            public const string Kraityn = BanditHelper.KraitynName;
            public const string Oak = BanditHelper.OakName;
            public const string VaalOversoul = "Vaal Oversoul";
            public const string Piety = "Piety";
            public const string Dominus = "Dominus";
            public const string Kaom = "Kaom";
            public const string Daresso = "Daresso";
            public const string PietyAbomination = "Piety Abomination";
            public const string Malachai = "Malachai";
            public const string Avarius = "Avarius";
            public const string Kitava1 = "Kitava 1";
            public const string Tukohama = "Tukohama";
            public const string Shavronne = "Shavronne";
            public const string Abberath = "Abberath";
            public const string Ryslatha = "Ryslatha";
            public const string BrineKing = "Brine King";
            public const string Maligaro = "Maligaro";
            public const string Ralakesh = "Ralakesh";
            public const string Gruthkul = "Gruthkul";
            public const string Arakaali = "Arakaali";
            public const string Doedre = "Doedre";
            public const string Yugul = "Yugul";
            public const string Dusk = "Dusk";
            public const string Dawn = "Dawn";
            public const string SolarisLunaris = "Solaris and Lunaris";
            public const string Shakari = "Shakari";
            public const string Garukhan = "Garukhan";
            public const string DepravedTrinity = "Depraved Trinity";
            public const string Vilenta = "Vilenta";
            public const string AvariusReassembled = "Avarius Reassembled";
            public const string Kitava2 = "Kitava 2";
        }

        private static int NextBossAct(Boss boss)
        {
            var n = boss.Name;

            if (n == BossNames.Merveil)
                return 2;

            if (n == BossNames.VaalOversoul)
                return 3;

            if (n == BossNames.Dominus)
                return 4;

            if (n == BossNames.Malachai)
                return 5;

            if (n == BossNames.Kitava1)
                return 6;

            if (n == BossNames.BrineKing)
                return 7;

            if (n == BossNames.Arakaali)
                return 8;

            if (n == BossNames.SolarisLunaris)
                return 9;

            if (n == BossNames.DepravedTrinity)
                return 10;

            return 0;
        }

        private static List<Boss> GetDefaultBossList()
        {
            var list = new List<Boss>();
            foreach (var field in typeof(BossNames).GetFields())
            {
                list.Add(new Boss(field.GetValue(field).ToString()));
            }
            return list;
        }

        private void InitBosses()
        {
            if (Bosses.Count == 0)
            {
                Bosses = GetDefaultBossList();
            }
            else
            {
                var defaultBosses = GetDefaultBossList();
                foreach (var boss in defaultBosses)
                {
                    var jsonBoss = Bosses.Find(b => b.Name == boss.Name);
                    if (jsonBoss != null)
                        boss.Enabled = jsonBoss.Enabled;
                }
                Bosses = defaultBosses;
            }
        }

        public class Boss
        {
            public string Name { get; }

            public bool Enabled { get; set; }

            public Boss(string name)
            {
                Name = name;
            }
        }

        #endregion

        #region Optional quests

        public List<OptionalQuest> OptionalQuests { get; set; } = new List<OptionalQuest>();

        [JsonIgnore]
        public List<ActGroup<OptionalQuest>> OptionalQuestsByAct => GroupByAct(OptionalQuests, NextOptionalQuestAct);

        public bool IsQuestEnabled(DatQuestWrapper quest)
        {
            var id = quest.Id;
            return OptionalQuests.Exists(q => q.Enabled && q.Id == id);
        }

        private static int NextOptionalQuestAct(OptionalQuest quest)
        {
            var n = quest.Name;

            if (n == Quests.MaroonedMariner.Name)
                return 2;

            if (n == Quests.ThroughSacredGround.Name)
                return 3;

            if (n == Quests.FixtureOfFate.Name)
                return 4;

            if (n == Quests.IndomitableSpirit.Name)
                return 5;

            if (n == Quests.KitavaTorments.Name)
                return 6;

            if (n == Quests.PuppetMistress.Name)
                return 7;

            if (n == Quests.KisharaStar.Name)
                return 8;

            if (n == Quests.ReflectionOfTerror.Name)
                return 9;

            if (n == Quests.RulerOfHighgate.Name)
                return 10;

            return 0;
        }

        private static List<OptionalQuest> GetDefaultOptionalQuestList()
        {
            return new List<OptionalQuest>
            {
                new OptionalQuest(Quests.MercyMission),
                new OptionalQuest(Quests.DirtyJob),
                new OptionalQuest(Quests.DwellerOfTheDeep),
                new OptionalQuest(Quests.MaroonedMariner),
                new OptionalQuest(Quests.WayForward),
                new OptionalQuest(Quests.GreatWhiteBeast),
                new OptionalQuest(Quests.ThroughSacredGround),
                new OptionalQuest(Quests.VictarioSecrets),
                new OptionalQuest(Quests.SwigOfHope),
                new OptionalQuest(Quests.FixtureOfFate),
                new OptionalQuest(Quests.IndomitableSpirit),
                new OptionalQuest(Quests.InServiceToScience),
                new OptionalQuest(Quests.KingFeast),
                new OptionalQuest(Quests.KitavaTorments),
                new OptionalQuest(Quests.FallenFromGrace),
                new OptionalQuest(Quests.BestelEpic),
                new OptionalQuest(Quests.ClovenOne),
                new OptionalQuest(Quests.PuppetMistress),
                new OptionalQuest(Quests.SilverLocket),
                new OptionalQuest(Quests.InMemoryOfGreust),
                new OptionalQuest(Quests.QueenOfDespair),
                new OptionalQuest(Quests.KisharaStar),
                new OptionalQuest(Quests.LoveIsDead),
                new OptionalQuest(Quests.GemlingLegion),
                new OptionalQuest(Quests.WingsOfVastiri),
                new OptionalQuest(Quests.ReflectionOfTerror),
                new OptionalQuest(Quests.QueenOfSands),
                new OptionalQuest(Quests.FastisFortuna),
                new OptionalQuest(Quests.RulerOfHighgate),
                new OptionalQuest(Quests.NoLoveForOldGhosts),
                new OptionalQuest(Quests.VilentaVengeance),
                new OptionalQuest(Quests.MapToTsoatha),
            };
        }

        private void InitOptionalQuests()
        {
            if (OptionalQuests.Count == 0)
            {
                OptionalQuests = GetDefaultOptionalQuestList();
            }
            else
            {
                var defaultQuests = GetDefaultOptionalQuestList();
                foreach (var quest in defaultQuests)
                {
                    var jsonQuest = OptionalQuests.Find(q => q.Id == quest.Id);
                    if (jsonQuest != null)
                        quest.Enabled = jsonQuest.Enabled;
                }
                OptionalQuests = defaultQuests;
            }
        }

        public class OptionalQuest : Quest
        {
            public bool Enabled { get; set; } = true;

            public OptionalQuest()
            {
            }

            public OptionalQuest(string name, string id)
                : base(name, id)
            {
            }

            public OptionalQuest(DatQuestWrapper quest)
                : base(quest)
            {
            }
        }

        #endregion

        #region Rewards

        public const string DefaultRewardName = "Any";
        public const string UnsetClassRewardName = "Select a class";

        public List<RewardQuest> RewardQuests { get; set; } = new List<RewardQuest>();
        public CharacterClass CharacterClass { get; set; } = CharacterClass.None;

        [JsonIgnore]
        public List<ActGroup<RewardQuest>> RewardQuestsByAct => GroupByAct(RewardQuests, NextRewardQuestAct);

        [JsonIgnore]
        public static readonly CharacterClass[] CharacterClasses =
        {
            CharacterClass.None,
            CharacterClass.Witch,
            CharacterClass.Shadow,
            CharacterClass.Ranger,
            CharacterClass.Duelist,
            CharacterClass.Marauder,
            CharacterClass.Templar,
            CharacterClass.Scion
        };

        public string GetRewardForQuest(string questId)
        {
            var quest = RewardQuests.Find(q => q.Id == questId);
            if (quest == null)
            {
                GlobalLog.Error($"[Settings][GetQuestReward] Unsupported quest id: \"{questId}\".");
                ErrorManager.ReportCriticalError();
                return null;
            }

            var reward = quest.SelectedReward;

            if (reward == UnsetClassRewardName)
                reward = DefaultRewardName;

            if (questId == Quests.DealWithBandits.Id && reward == DefaultRewardName)
                reward = BanditHelper.EramirName;

            return reward;
        }

        private static int NextRewardQuestAct(RewardQuest quest)
        {
            var n = quest.Name;

            if (n == Quests.SirensCadence.Name)
                return 2;

            if (n == Quests.DealWithBandits.Name)
                return 3;

            if (n == Quests.FixtureOfFate.Name)
                return 4;

            if (n == Quests.EternalNightmare.Name)
                return 5;

            if (n == Quests.KingFeast.Name)
                return 6;

            if (n == Quests.EssenceOfUmbra.Name)
                return 7;

            if (n == Quests.InMemoryOfGreust.Name)
                return 8;

            if (n == Quests.WingsOfVastiri.Name)
                return 9;

            if (n == Quests.StormBlade.Name)
                return 10;

            return 0;
        }

        private static List<RewardQuest> GetDefaultRewardQuestList()
        {
            return new List<RewardQuest>
            {
                new RewardQuest(Quests.EnemyAtTheGate),
                new RewardQuest(Quests.MercyMission.Name + " 1", Quests.MercyMission.Id),
                new RewardQuest(Quests.MercyMission.Name + " 2", Quests.MercyMission.Id + "b"),
                new RewardQuest(Quests.BreakingSomeEggs),
                new RewardQuest(Quests.CagedBrute.Name + " 1", Quests.CagedBrute.Id + "b"),
                new RewardQuest(Quests.CagedBrute.Name + " 2", Quests.CagedBrute.Id),
                new RewardQuest(Quests.SirensCadence),
                new RewardQuest(Quests.SharpAndCruel),
                new RewardQuest(Quests.GreatWhiteBeast),
                new RewardQuest(Quests.IntrudersInBlack),
                new RewardQuest(Quests.ThroughSacredGround.Name, Quests.ThroughSacredGround.Id + "b"),
                new RewardQuest(Quests.DealWithBandits),
                new RewardQuest(Quests.LostInLove),
                new RewardQuest(Quests.RibbonSpool),
                new RewardQuest(Quests.SeverRightHand),
                new RewardQuest(Quests.SwigOfHope),
                new RewardQuest(Quests.FixtureOfFate),
                new RewardQuest(Quests.BreakingSeal),
                new RewardQuest(Quests.EternalNightmare),
                new RewardQuest(Quests.ReturnToOriath),
                new RewardQuest(Quests.KeyToFreedom),
                new RewardQuest(Quests.DeathToPurity),
                new RewardQuest(Quests.KingFeast),
                new RewardQuest(Quests.BestelEpic),
                new RewardQuest(Quests.EssenceOfUmbra),
                new RewardQuest(Quests.SilverLocket),
                new RewardQuest(Quests.EssenceOfArtist),
                new RewardQuest(Quests.InMemoryOfGreust),
                new RewardQuest(Quests.EssenceOfHag),
                new RewardQuest(Quests.WingsOfVastiri),
                new RewardQuest(Quests.StormBlade),
                new RewardQuest(Quests.SafePassage),
                new RewardQuest(Quests.MapToTsoatha),
                new RewardQuest(Quests.DeathAndRebirth),
            };
        }

        private void InitRewardQuests()
        {
            if (RewardQuests.Count == 0)
            {
                RewardQuests = GetDefaultRewardQuestList();
            }
            else
            {
                var defaultQuests = GetDefaultRewardQuestList();
                foreach (var quest in defaultQuests)
                {
                    var jsonQuest = RewardQuests.Find(q => q.Id == quest.Id);
                    if (jsonQuest != null)
                        quest.SelectedReward = jsonQuest.SelectedReward;
                }
                RewardQuests = defaultQuests;
            }
        }

        public class RewardQuest : Quest
        {
            public string SelectedReward { get; set; }

            public RewardQuest()
            {
            }

            public RewardQuest(string name, string id)
                : base(name, id)
            {
            }

            public RewardQuest(DatQuestWrapper quest)
                : base(quest)
            {
            }
        }

        #endregion

        #region Misc

        public bool EnterCorruptedAreas { get; set; } = true;
        public bool TalkToQuestgivers { get; set; }

        #endregion

        #region Helpers

        private static List<ActGroup<T>> GroupByAct<T>(List<T> source, Func<T, int> nextAct)
        {
            int index = 0;
            int act = 1;
            var currentAct = new ActGroup<T>(act);
            var allActs = new List<ActGroup<T>> {currentAct};

            foreach (var element in source)
            {
                allActs[index].Elements.Add(element);
                act = nextAct(element);
                if (act != 0)
                {
                    ++index;
                    currentAct = new ActGroup<T>(act);
                    allActs.Add(currentAct);
                }
            }
            return allActs;
        }

        public class ActGroup<T>
        {
            public int Act { get; }
            public List<T> Elements { get; }

            public ActGroup(int act)
            {
                Act = act;
                Elements = new List<T>();
            }
        }

        public class Quest : IEquatable<Quest>
        {
            public string Name { get; set; }
            public string Id { get; set; }

            public Quest()
            {
            }

            public Quest(string name, string id)
            {
                Name = name;
                Id = id;
            }

            public Quest(DatQuestWrapper quest)
            {
                Name = quest.Name;
                Id = quest.Id;
            }

            public static implicit operator Quest(DatQuestWrapper quest)
            {
                return new Quest(quest);
            }

            public bool Equals(Quest other)
            {
                return this == other;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as Quest);
            }

            public static bool operator ==(Quest left, Quest right)
            {
                if (ReferenceEquals(left, right))
                    return true;

                if (((object) left == null) || ((object) right == null))
                    return false;

                return left.Id == right.Id;
            }

            public static bool operator !=(Quest left, Quest right)
            {
                return !(left == right);
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }

            public override string ToString()
            {
                return $"\"{Name}\" ({Id})";
            }
        }

        // For debugging purposes
        public bool CheckGrindingFirst { get; set; }

        #endregion
    }
}