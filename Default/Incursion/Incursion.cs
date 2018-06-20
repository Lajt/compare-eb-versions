using System.Threading.Tasks;
using System.Windows.Controls;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Loki.Bot;
using Loki.Bot.Pathfinding;
using Loki.Common;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;
using settings = Default.Incursion.Settings;

namespace Default.Incursion
{
    public class Incursion : IPlugin, IStartStopEvents, ITickEvents
    {
        private static readonly Interval TickInterval = new Interval(200);
        private Gui _gui;

        public static RoomEntry CurrentRoomSettings;

        public static Npc Alva => LokiPoe.ObjectManager.Objects
            .FirstOrDefault<Npc>(n => n.Metadata == "Metadata/NPC/League/TreasureHunter");

        public static Monster Omnitect => LokiPoe.ObjectManager.Objects
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique && m.Metadata == "Metadata/Monsters/LeagueIncursion/VaalSaucerBoss");

        public static CachedObject CachedAlva
        {
            get => CombatAreaCache.Current.Storage["AlvaValai"] as CachedObject;
            set => CombatAreaCache.Current.Storage["AlvaValai"] = value;
        }

        public static CachedObject CachedOmnitect
        {
            get => CombatAreaCache.Current.Storage["VaalOmnitect"] as CachedObject;
            set => CombatAreaCache.Current.Storage["VaalOmnitect"] = value;
        }

        public void Tick()
        {
            if (!TickInterval.Elapsed)
                return;

            if (!LokiPoe.IsInGame)
                return;

            var area = World.CurrentArea;

            if (area.IsMap || area.IsOverworldArea)
            {
                if (CachedAlva == null)
                {
                    var alva = Alva;
                    if (alva != null)
                    {
                        CachedAlva = new CachedObject(alva);
                    }
                }
                return;
            }
            if (area.IsTempleOfAtzoatl && !settings.Instance.IgnoreBossroom)
            {
                var cachedOmnitect = CachedOmnitect;
                if (cachedOmnitect == null)
                {
                    var omnitect = Omnitect;
                    if (omnitect != null && !omnitect.IsDead)
                    {
                        CachedOmnitect = new CachedObject(omnitect);
                    }
                }
                else
                {
                    var omnitect = cachedOmnitect.Object as Monster;
                    if (omnitect != null && omnitect.IsDead)
                    {
                        GlobalLog.Warn("[HandleTempleTask] Registering death of Vaal Omnitect.");
                        CachedOmnitect = null;
                    }
                }
            }
        }

        public void Start()
        {
            ExilePather.BlockLockedTempleDoors = FeatureEnum.Enabled;

            ComplexExplorer.AddSettingsProvider("IncursionPlugin", IncursionExploration, ProviderPriority.High);

            var taskManager = BotStructure.TaskManager;
            taskManager.AddBefore(new EnterIncursionTask(), "CombatTask (Leash -1)");
            taskManager.AddAfter(new HandleIncursionTask(), "CombatTask (Leash -1)");
            taskManager.AddAfter(new EnterTempleTask(), "HandleIncursionTask");
            taskManager.AddAfter(new HandleTempleTask(), "EnterTempleTask");
        }

        public MessageResult Message(Message message)
        {
            if (message.Id == ComplexExplorer.LocalTransitionEnteredMessage)
            {
                var storage = CombatAreaCache.Current.Storage;

                var alva = storage["AlvaValai"] as CachedObject;
                if (alva != null) alva.Unwalkable = false;

                var omnitect = storage["VaalOmnitect"] as CachedObject;
                if (omnitect != null) omnitect.Unwalkable = false;

                return MessageResult.Processed;
            }
            return MessageResult.Unprocessed;
        }

        private static ExplorationSettings IncursionExploration()
        {
            if (CombatAreaCache.IsInIncursion)
                return new ExplorationSettings(tileSeenRadius: 5);

            if (World.CurrentArea.IsTempleOfAtzoatl)
                return new ExplorationSettings(settings.Instance.IgnoreBossroom, tileSeenRadius: 5);

            return null;
        }

        #region Unused interface methods

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

        public void Deinitialize()
        {
        }

        public void Enable()
        {
        }

        public void Disable()
        {
        }

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public string Name => "Incursion";
        public string Description => "Plugin that handles incursions.";
        public string Author => "ExVault";
        public string Version => "1.0";
        public UserControl Control => _gui ?? (_gui = new Gui());
        public JsonSettings Settings => settings.Instance;

        #endregion
    }
}