using System.Diagnostics.CodeAnalysis;
using Default.EXtensions;

namespace Default.MapBot
{
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    public class MapData
    {
        public string Name { get; }
        public int Tier { get; }
        public MapType Type { get; }

        private int _priority;
        private bool _ignored;
        private bool _ignoredBossroom;
        private bool _sextant;
        private int _zanaMod;
        private int _mobRemaining = -1;
        private bool _strictMobRemaining;
        private int _explorationPercent = -1;
        private bool _strictExplorationPercent;
        private bool? _trackMob;
        private bool? _fastTransition;

        public int Priority
        {
            get => _priority;
            set => _priority = value;
        }

        public bool Ignored
        {
            get => Unsupported || _ignored;
            set => _ignored = value;
        }

        public bool IgnoredBossroom
        {
            get => Type == MapType.Bossroom && (UnsupportedBossroom || _ignoredBossroom);
            set => _ignoredBossroom = value;
        }

        public bool Sextant
        {
            get => _sextant;
            set => _sextant = value;
        }

        public int ZanaMod
        {
            get => _zanaMod;
            set => _zanaMod = value;
        }

        public int MobRemaining
        {
            get => _mobRemaining;
            set => _mobRemaining = value;
        }

        public bool StrictMobRemaining
        {
            get => _strictMobRemaining;
            set => _strictMobRemaining = value;
        }

        public int ExplorationPercent
        {
            get => Type == MapType.Regular || Type == MapType.Bossroom ? _explorationPercent : -1;
            set => _explorationPercent = value;
        }

        public bool StrictExplorationPercent
        {
            get => _strictExplorationPercent;
            set => _strictExplorationPercent = value;
        }

        public bool? TrackMob
        {
            get => _trackMob;
            set => _trackMob = value;
        }

        public bool? FastTransition
        {
            get => Type == MapType.Multilevel || Type == MapType.Complex ? _fastTransition : false;
            set => _fastTransition = value;
        }

        public bool Unsupported { get; internal set; }
        public bool UnsupportedBossroom { get; internal set; }

        public MapData(string name, int tier, MapType type)
        {
            Name = name;
            Tier = tier;
            Type = type;
        }

        public MapData(MapData other)
        {
            Name = other.Name;
            Tier = other.Tier;
            Type = other.Type;
            Priority = other.Priority;
            Ignored = other.Ignored;
            IgnoredBossroom = other.IgnoredBossroom;
            MobRemaining = other.MobRemaining;
            StrictMobRemaining = other.StrictMobRemaining;
            ExplorationPercent = other.ExplorationPercent;
            StrictExplorationPercent = other.StrictExplorationPercent;
            TrackMob = other.TrackMob;
            FastTransition = other.FastTransition;
            Unsupported = other.Unsupported;
            UnsupportedBossroom = other.UnsupportedBossroom;
        }

        public static MapData Current { get; private set; }

        internal static void ResetCurrent()
        {
            var areaName = World.CurrentArea.Name;
            if (!MapSettings.Instance.MapDict.TryGetValue(areaName, out var settingsData))
            {
                GlobalLog.Error($"[MapData] Unknown map name \"{areaName}\". MapBot will use global settings for this area.");
                Current = CreateFromGlobal(areaName);
                return;
            }
            var data = new MapData(settingsData);
            var global = GeneralSettings.Instance;
            if (data.MobRemaining == -1)
            {
                data.MobRemaining = global.MobRemaining;
                data.StrictMobRemaining = global.StrictMobRemaining;
            }
            if (data.ExplorationPercent == -1)
            {
                data.ExplorationPercent = global.ExplorationPercent;
                data.StrictExplorationPercent = global.StrictExplorationPercent;
            }
            if (data.TrackMob == null)
            {
                data.TrackMob = global.TrackMob;
            }
            if (data.FastTransition == null)
            {
                data.FastTransition = global.FastTransition;
            }

            var type = data.Type;

            GlobalLog.Info($"[MapData] Name: {data.Name}");
            GlobalLog.Info($"[MapData] Tier: {data.Tier}");
            GlobalLog.Info($"[MapData] Type: {type}");
            GlobalLog.Info($"[MapData] Monster remaining: {data.MobRemaining}");
            if (type == MapType.Regular || type == MapType.Bossroom)
            {
                GlobalLog.Info($"[MapData] Exploration percent: {data.ExplorationPercent}");
            }
            else
            {
                GlobalLog.Info("[MapData] Exploration percent: not used");
            }

            GlobalLog.Info($"[MapData] Monster tracking: {data.TrackMob}");

            if (type == MapType.Multilevel || type == MapType.Complex)
            {
                GlobalLog.Info($"[MapData] Fast transition: {data.FastTransition}");
            }
            else
            {
                GlobalLog.Info("[MapData] Fast transition: not used");
            }
            GlobalLog.Info($"[MapData] Strict monster remaining: {data.StrictMobRemaining}");
            GlobalLog.Info($"[MapData] Strict exploration percent: {data.StrictExplorationPercent}");
            Current = data;
        }

        private static MapData CreateFromGlobal(string areaName)
        {
            var global = GeneralSettings.Instance;
            return new MapData(areaName, 1, MapType.Regular)
            {
                MobRemaining = global.MobRemaining,
                StrictMobRemaining = global.StrictMobRemaining,
                ExplorationPercent = global.ExplorationPercent,
                StrictExplorationPercent = global.StrictExplorationPercent,
                TrackMob = global.TrackMob,
                FastTransition = global.FastTransition
            };
        }
    }

    public enum MapType
    {
        Regular,
        Bossroom,
        Multilevel,
        Complex
    }
}