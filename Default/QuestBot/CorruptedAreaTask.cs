using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Loki.Bot;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot
{
    public class CorruptedAreaTask : ITask
    {
        private static readonly Interval ScanInterval = new Interval(200);

        private static CachedTransition _lastEnteredTransition;

        private static bool ExitExists => LokiPoe.ObjectManager.Objects
            .Any<AreaTransition>(a => a.IsTargetable && a.Metadata == "Metadata/MiscellaneousObjects/VaalSideAreaReturnPortal");

        private static NetworkObject TransitionBlockage => LokiPoe.ObjectManager.Objects
            .Find(o => o.IsTargetable && o.Metadata == "Metadata/QuestObjects/Sewers/SewersGrate");

        public static CorruptedAreaData CachedData
        {
            get
            {
                var data = CombatAreaCache.Current.Storage["CorruptedAreaData"] as CorruptedAreaData;
                if (data == null)
                {
                    data = new CorruptedAreaData();
                    CombatAreaCache.Current.Storage["CorruptedAreaData"] = data;
                }
                return data;
            }
        }

        public async Task<bool> Run()
        {
            if (!Settings.Instance.EnterCorruptedAreas)
                return false;

            var area = World.CurrentArea;
            if (area.IsOverworldArea)
            {
                var transition = CombatAreaCache.Current.AreaTransitions.Find(ValidCorruptedAreaTransition);
                if (transition != null)
                {
                    await EnterCorruptedArea(transition);
                    return true;
                }
            }
            else if (area.IsCorruptedArea)
            {
                await HandleCorruptedArea();
                return true;
            }
            return false;
        }

        public void Tick()
        {
            if (!World.CurrentArea.IsCorruptedArea)
                return;

            if (!ScanInterval.Elapsed)
                return;

            var cachedData = CachedData;

            if (cachedData.IsAreaFinished)
                return;

            foreach (var obj in LokiPoe.ObjectManager.Objects)
            {
                if (obj is Chest c && obj.Metadata == "Metadata/Chests/SideArea/SideAreaChest")
                {
                    if (c.IsOpened)
                    {
                        GlobalLog.Warn("[CorruptedAreaTask] Vaal Vessel has been opened. Area is finished.");
                        cachedData.IsAreaFinished = true;
                    }
                    else if (cachedData.Vessel == null)
                    {
                        cachedData.Vessel = new CachedObject(obj);
                        GlobalLog.Warn($"[CorruptedAreaTask] Registering {cachedData.Vessel}");
                    }
                    continue;
                }

                if (!cachedData.IsBossKilled &&
                    obj is Monster m &&
                    m.Rarity == Rarity.Unique &&
                    m.ExplicitAffixes.Any(a => a.Category == "MonsterSideAreaBoss"))
                {
                    if (Blacklist.Contains(obj.Id))
                    {
                        GlobalLog.Warn("[CorruptedAreaTask] Boss was blacklisted from outside. We will not be able to finish this area.");
                        cachedData.IsAreaFinished = true;
                        continue;
                    }
                    if (m.IsDead)
                    {
                        GlobalLog.Warn("[CorruptedAreaTask] Corrupted area boss has been killed.");
                        cachedData.IsBossKilled = true;
                        continue;
                    }
                    if (cachedData.Boss == null)
                    {
                        cachedData.Boss = new CachedObject(obj);
                        GlobalLog.Warn($"[CorruptedAreaTask] Registering {cachedData.Boss}");
                    }
                    else
                    {
                        cachedData.Boss.Position = obj.WalkablePosition();
                    }
                }
            }
        }

        private static async Task EnterCorruptedArea(CachedTransition transition)
        {
            var pos = transition.Position;
            if (pos.IsFar)
            {
                if (!pos.TryCome())
                {
                    GlobalLog.Debug($"[CorruptedAreaTask] Fail to move to {pos}. Marking this transition as unwalkable.");
                    transition.Unwalkable = true;
                }
                return;
            }
            var transitionObj = transition.Object;
            if (transitionObj == null)
            {
                GlobalLog.Error("[CorruptedAreaTask] Unexpected error. Transition object is null.");
                transition.Ignored = true;
                return;
            }
            var attempts = ++transition.InteractionAttempts;
            if (attempts > 5)
            {
                GlobalLog.Error("[CorruptedAreaTask] All attempts to enter corrupted area transition have been spent. Now ignoring it.");
                transition.Ignored = true;
                return;
            }
            if (!transitionObj.IsTargetable)
            {
                var blockage = TransitionBlockage;
                if (blockage != null)
                {
                    await PlayerAction.Interact(blockage, () => transitionObj.Fresh().IsTargetable, "transition become targetable");
                }
                else
                {
                    GlobalLog.Error("[CorruptedAreaTask] Unexpected error. Transition object is untargetable.");
                    transition.Ignored = true;
                }
                return;
            }

            _lastEnteredTransition = transition;

            if (!await PlayerAction.TakeTransition(transitionObj))
                await Wait.SleepSafe(500);
        }

        private static async Task HandleCorruptedArea()
        {
            var cachedData = CachedData;

            if (cachedData.IsAreaFinished)
            {
                DisableEntrance();

                var exit = LokiPoe.ObjectManager.Objects.Closest<AreaTransition>(a => a.IsTargetable);

                if (exit != null && await PlayerAction.TakeTransition(exit))
                    return;

                await PlayerAction.TpToTown();
                return;
            }

            var vessel = cachedData.Vessel;
            if (vessel != null && cachedData.IsBossKilled)
            {
                var pos = vessel.Position;
                if (pos.IsFar || pos.IsFarByPath)
                {
                    if (!pos.TryCome())
                    {
                        GlobalLog.Error("[CorruptedAreaTask] Unexpected error. Vaal Vessel position is unwalkable.");
                        cachedData.IsAreaFinished = true;
                    }
                    return;
                }
                var vesselObj = vessel.Object as Chest;
                if (vesselObj == null)
                {
                    GlobalLog.Error("[CorruptedAreaTask] Unexpected error. Vaal Vessel object is null.");
                    cachedData.IsAreaFinished = true;
                    return;
                }
                var attempts = ++vessel.InteractionAttempts;
                if (attempts > 5)
                {
                    GlobalLog.Error("[CorruptedAreaTask] All attempts to open Vaal Vessel have been spent.");
                    cachedData.IsAreaFinished = true;
                    return;
                }
                if (await PlayerAction.Interact(vesselObj, () => vesselObj.Fresh().IsOpened, "Vaal Vessel opening"))
                {
                    await Wait.For(() => ExitExists, "corrupted area exit spawning", 500, 5000);
                }
                else
                {
                    await Wait.SleepSafe(500);
                }
                return;
            }

            var boss = cachedData.Boss;
            if (boss != null)
            {
                var pos = boss.Position;
                if (pos.IsFar || pos.IsFarByPath)
                {
                    if (!pos.TryCome())
                    {
                        GlobalLog.Error("[CorruptedAreaTask] Unexpected error. Corrupted area boss is unwalkable.");
                        cachedData.IsAreaFinished = true;
                    }
                    return;
                }
                var bossObj = boss.Object;
                if (bossObj == null)
                {
                    GlobalLog.Warn("[CorruptedAreaTask] There is no boss object near cached position. Marking it as dead.");
                    cachedData.IsBossKilled = true;
                    return;
                }
                GlobalLog.Debug("[CorruptedAreaTask] Waiting for combat routine to kill the boss.");
                await Wait.StuckDetectionSleep(500);
                return;
            }

            if (!await CombatAreaCache.Current.Explorer.Execute())
            {
                GlobalLog.Warn("[CorruptedAreaTask] Now leaving the corrupted area because it is fully explored.");
                cachedData.IsAreaFinished = true;
            }
        }

        private static void DisableEntrance()
        {
            if (_lastEnteredTransition != null)
            {
                GlobalLog.Warn($"[CorruptedAreaTask] Marking \"{_lastEnteredTransition.Position.Name}\" transition as visited.");
                _lastEnteredTransition.Visited = true;
                _lastEnteredTransition = null;
            }
        }

        private static bool ValidCorruptedAreaTransition(CachedTransition t)
        {
            return t.Type == TransitionType.Vaal && !t.Visited && !t.Ignored && !t.Unwalkable;
        }

        public class CorruptedAreaData
        {
            public bool IsAreaFinished;
            public bool IsBossKilled;
            public CachedObject Boss;
            public CachedObject Vessel;
        }

        #region Unused interface methods

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public MessageResult Message(Message message)
        {
            return MessageResult.Unprocessed;
        }

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public string Name => "CorruptedAreaTask";
        public string Description => "Task that handles corrupted side areas.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}