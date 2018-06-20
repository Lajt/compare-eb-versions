using Loki;
using Loki.Common;
using Newtonsoft.Json;

namespace Default.MapBot
{
    public class GeneralSettings : JsonSettings
    {
        private static GeneralSettings _instance;
        public static GeneralSettings Instance => _instance ?? (_instance = new GeneralSettings());

        private GeneralSettings()
            : base(GetSettingsFilePath(Configuration.Instance.Name, "MapBot", "GeneralSettings.json"))
        {
            SpecialObjectTask.ToggleRhoaNests(BreakNests);
        }

        public bool UseHideout { get; set; }
        public int MaxMapTier { get; set; } = 10;
        public int MobRemaining { get; set; } = 20;
        public bool StrictMobRemaining { get; set; }
        public int ExplorationPercent { get; set; } = 85;
        public bool StrictExplorationPercent { get; set; }
        public bool TrackMob { get; set; } = true;
        public bool FastTransition { get; set; } = true;
        public bool RunUnId { get; set; }
        public bool IgnoreHiddenAuras { get; set; }
        public Trials Trials { get; set; } = new Trials();

        public bool SellEnabled { get; set; }
        public bool SellIgnoredMaps { get; set; } = true;
        public int MaxSellTier { get; set; } = 10;
        public int MaxSellPriority { get; set; }
        public int MinMapAmount { get; set; } = 7;

        public int ShapedPriority { get; set; }
        public int ShaperInfluencePriority { get; set; }
        public int ElderInfluencePriority { get; set; }
        public bool AtlasExplorationEnabled { get; set; }

        public Upgrade MagicUpgrade { get; set; } = new Upgrade {TierEnabled = true};
        public Upgrade RareUpgrade { get; set; } = new Upgrade();
        public Upgrade ChiselUpgrade { get; set; } = new Upgrade();
        public Upgrade VaalUpgrade { get; set; } = new Upgrade();
        public Upgrade FragmentUpgrade { get; set; } = new Upgrade();
        public Upgrade MagicRareUpgrade { get; set; } = new Upgrade();
        public RareReroll RerollMethod { get; set; }
        public ExistingRares ExistingRares { get; set; }

        private bool _breakNests;
        private bool _stopRequested;

        public bool BreakNests
        {
            get => _breakNests;
            set
            {
                _breakNests = value;
                SpecialObjectTask.ToggleRhoaNests(value);
            }
        }

        [JsonIgnore]
        public bool StopRequested
        {
            get => _stopRequested;
            set
            {
                if (value == _stopRequested) return;
                _stopRequested = value;
                NotifyPropertyChanged(() => StopRequested);
            }
        }

        public bool TrialEnabled(string trialName)
        {
            if (trialName.Contains("Piercing"))
                return Trials.PiercingTruth;

            if (trialName.Contains("Swirling"))
                return Trials.SwirlingFear;

            if (trialName.Contains("Crippling"))
                return Trials.CripplingGrief;

            if (trialName.Contains("Burning"))
                return Trials.BurningRage;

            if (trialName.Contains("Lingering"))
                return Trials.LingeringPain;

            if (trialName.Contains("Stinging"))
                return Trials.StingingDoubt;

            return false;
        }

        // Not in gui. You must edit this in /Settings/profile_name/MapBot/GeneralSettings.json if you know what you are doing.
        public bool OpenPortals { get; set; } = true;
    }

    public class Upgrade
    {
        public bool TierEnabled { get; set; }
        public int Tier { get; set; } = 1;
        public bool PriorityEnabled { get; set; }
        public int Priority { get; set; }
    }

    public class Trials
    {
        public bool PiercingTruth { get; set; }
        public bool SwirlingFear { get; set; }
        public bool CripplingGrief { get; set; }
        public bool BurningRage { get; set; }
        public bool LingeringPain { get; set; }
        public bool StingingDoubt { get; set; }
    }

    public enum RareReroll
    {
        ScourAlch,
        Chaos
    }

    public enum ExistingRares
    {
        Run,
        NoRun,
        NoReroll,
        Downgrade
    }
}