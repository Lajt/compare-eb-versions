using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Loki.Bot;
using Loki.Game;

namespace Default.MapBot
{
    public class MapExplorationTask : ITask
    {
        private static readonly Interval TickInterval = new Interval(100);

        private static bool _mapCompletionPointReached;
        private static bool _mapCompleted;
        private static bool _bossInTheEnd;

        public static bool MapCompleted
        {
            get => _mapCompleted;
            private set
            {
                _mapCompleted = value;
                if (value) TrackMobTask.RestrictRange();
            }
        }

        public async Task<bool> Run()
        {
            if (MapCompleted || !World.CurrentArea.IsMap)
                return false;

            return await CombatAreaCache.Current.Explorer.Execute();
        }

        public void Tick()
        {
            if (MapCompleted || !TickInterval.Elapsed)
                return;

            if (!LokiPoe.IsInGame || !World.CurrentArea.IsMap)
                return;

            var mapData = MapData.Current;
            var type = mapData.Type;

            if (KillBossTask.BossKilled)
            {
                if (_mapCompletionPointReached)
                {
                    GlobalLog.Debug("[MapExplorationTask] Boss is killed and map completion point is reached. Map is complete.");
                    MapCompleted = true;
                    return;
                }
                if (type == MapType.Bossroom)
                {
                    GlobalLog.Debug("[MapExplorationTask] Boss is killed and map type is bossroom. Map is complete.");
                    MapCompleted = true;
                    return;
                }
                if (type == MapType.Multilevel)
                {
                    GlobalLog.Debug("[MapExplorationTask] Boss is killed and map type is multilevel. Map is complete.");
                    MapCompleted = true;
                    return;
                }
                if (_bossInTheEnd)
                {
                    GlobalLog.Debug("[MapExplorationTask] Boss is killed and BossInTheEnd flag is true. Map is complete.");
                    MapCompleted = true;
                    return;
                }
            }

            if (!_mapCompletionPointReached)
            {
                if (LokiPoe.InstanceInfo.MonstersRemaining <= mapData.MobRemaining)
                {
                    GlobalLog.Warn($"[MapExplorationTask] Monster remaining limit has been reached ({mapData.MobRemaining})");
                    if (mapData.StrictMobRemaining)
                    {
                        GlobalLog.Debug("[MapExplorationTask] Strict monster remaining is true. Map is complete.");
                        MapCompleted = true;
                        return;
                    }
                    if (type == MapType.Bossroom)
                    {
                        if (mapData.IgnoredBossroom)
                        {
                            GlobalLog.Debug("[MapExplorationTask] Bossroom is ignored. Map is complete.");
                            MapCompleted = true;
                            return;
                        }
                        TrackMobTask.RestrictRange();
                        CombatAreaCache.Current.Explorer.Settings.FastTransition = true;
                    }
                    _mapCompletionPointReached = true;
                    return;
                }
                if (type == MapType.Regular || type == MapType.Bossroom)
                {
                    if (CombatAreaCache.Current.Explorer.BasicExplorer.PercentComplete >= mapData.ExplorationPercent)
                    {
                        GlobalLog.Warn($"[MapExplorationTask] Exploration limit has been reached ({mapData.ExplorationPercent}%)");
                        if (mapData.StrictExplorationPercent)
                        {
                            GlobalLog.Debug("[MapExplorationTask] Strict exploration percent is true. Map is complete.");
                            MapCompleted = true;
                            return;
                        }
                        if (type == MapType.Bossroom)
                        {
                            if (mapData.IgnoredBossroom)
                            {
                                GlobalLog.Debug("[MapExplorationTask] Bossroom is ignored. Map is complete.");
                                MapCompleted = true;
                                return;
                            }
                            TrackMobTask.RestrictRange();
                            CombatAreaCache.Current.Explorer.Settings.FastTransition = true;
                        }
                        _mapCompletionPointReached = true;
                    }
                }
            }
        }

        private static void Reset(string areaName)
        {
            MapCompleted = false;
            _mapCompletionPointReached = false;
            _bossInTheEnd = false;

            if (areaName == MapNames.Excavation || areaName == MapNames.Arena)
            {
                _bossInTheEnd = true;
                GlobalLog.Info("[MapExplorationTask] BossInTheEnd is set to true.");
                return;
            }
            if (areaName == MapNames.VaultsOfAtziri)
            {
                MapData.Current.MobRemaining = -1;
                GlobalLog.Info("[MapExplorationTask] Monster remaining is set to -1.");
            }
        }

        private static void SpecificTweaksOnLocalTransition()
        {
            var areaName = World.CurrentArea.Name;
            if (areaName == MapNames.JungleValley || areaName == MapNames.ArachnidNest || areaName == MapNames.DarkForest)
            {
                GlobalLog.Info("[MapExplorationTask] Setting TileSeenRadius to 1 for this bossroom.");
                CombatAreaCache.Current.Explorer.BasicExplorer.TileSeenRadius = 1;
                return;
            }
            if (areaName == MapNames.Ramparts)
            {
                var backTransition = CombatAreaCache.Current.AreaTransitions
                    .Where(t => t.Type == TransitionType.Local && !t.LeadsBack && !t.Visited)
                    .OrderByDescending(t => t.Position.DistanceSqr)
                    .FirstOrDefault();

                if (backTransition != null)
                {
                    GlobalLog.Info($"[MapExplorationTask] Marking {backTransition.Position} as back transition.");
                    backTransition.LeadsBack = true;
                }
            }
        }

        public MessageResult Message(Message message)
        {
            var id = message.Id;
            if (id == MapBot.Messages.NewMapEntered)
            {
                GlobalLog.Info("[MapExplorationTask] Reset.");
                Reset(message.GetInput<string>());
                return MessageResult.Processed;
            }
            if (id == ComplexExplorer.LocalTransitionEnteredMessage)
            {
                SpecificTweaksOnLocalTransition();
                return MessageResult.Processed;
            }
            return MessageResult.Unprocessed;
        }

        #region Unused interface methods

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public string Name => "MapExplorationTask";
        public string Description => "Task that handles map exploration.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}