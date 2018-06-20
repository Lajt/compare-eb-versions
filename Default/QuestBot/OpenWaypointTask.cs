using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Bot;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.QuestBot
{
    public class OpenWaypointTask : ITask
    {
        private static readonly Interval ScanInterval = new Interval(500);

        private static WalkablePosition _waypointTgtPos;
        private static bool _sceptreSpecial;
        private static bool _enabled;

        private static WalkablePosition CachedWaypointPos
        {
            get => CombatAreaCache.Current.Storage["WaypointPosition"] as WalkablePosition;
            set => CombatAreaCache.Current.Storage["WaypointPosition"] = value;
        }

        public async Task<bool> Run()
        {
            if (!_enabled || !World.CurrentArea.IsOverworldArea)
                return false;

            var wpPos = CachedWaypointPos;
            if (wpPos != null)
            {
                if (wpPos.IsFar)
                {
                    wpPos.Come();
                    return true;
                }
                if (!await PlayerAction.OpenWaypoint())
                {
                    ErrorManager.ReportError();
                    return true;
                }
                _enabled = false;
                await Coroutines.CloseBlockingWindows();
                return true;
            }
            if (_waypointTgtPos == null)
            {
                var pos = Tgt.FindWaypoint();
                if (pos == null)
                {
                    GlobalLog.Error($"[OpenWaypointTask] Fail to find any walkable waypoint tgt. Skipping this task for \"{World.CurrentArea.Name}\".");
                    _enabled = false;
                    return true;
                }
                _waypointTgtPos = new WalkablePosition("Waypoint location", pos);
            }
            _waypointTgtPos.Come();
            return true;
        }

        public void Tick()
        {
            if (!_enabled && !_sceptreSpecial)
                return;

            if (!ScanInterval.Elapsed || !LokiPoe.IsInGame || !World.CurrentArea.IsOverworldArea)
                return;

            if (CachedWaypointPos != null)
                return;

            var waypoint = LokiPoe.ObjectManager.Objects.Find(o => o is Waypoint);
            if (waypoint != null)
            {
                CachedWaypointPos = waypoint.WalkablePosition();
            }

            if (_sceptreSpecial)
            {
                if (waypoint != null)
                {
                    GlobalLog.Warn("[OpenWaypointTask] Enabled (waypoint object detected)");
                    _enabled = true;
                    _sceptreSpecial = false;
                    return;
                }
                if (LokiPoe.ObjectManager.Objects.Exists(o => o is AreaTransition && o.Name == World.Act3.UpperSceptreOfGod.Name))
                {
                    GlobalLog.Warn("[OpenWaypointTask] Enabled (Upper Sceptre of God transition detected)");
                    _enabled = true;
                    _sceptreSpecial = false;
                }
            }
        }

        public MessageResult Message(Message message)
        {
            if (message.Id == Events.Messages.CombatAreaChanged)
            {
                _waypointTgtPos = null;
                _enabled = false;
                _sceptreSpecial = false;

                var area = World.CurrentArea;
                var areaId = area.Id;
                if (area.IsOverworldArea && area.HasWaypoint && !World.IsWaypointOpened(areaId))
                {
                    if (areaId == World.Act3.SceptreOfGod.Id)
                    {
                        _sceptreSpecial = true;
                    }
                    else if (!BlockedByBoss(areaId))
                    {
                        GlobalLog.Warn("[OpenWaypointTask] Enabled.");
                        _enabled = true;
                    }
                }
                return MessageResult.Processed;
            }
            return MessageResult.Unprocessed;
        }

        private static bool BlockedByBoss(string areaId)
        {
            return areaId == World.Act5.CathedralRooftop.Id ||
                   areaId == World.Act8.DoedreCesspool.Id;
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

        public string Name => "OpenWaypointTask";
        public string Description => "Task that handles waypoint opening.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}