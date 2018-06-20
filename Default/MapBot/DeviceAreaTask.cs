using System.Threading.Tasks;
using Default.EXtensions;
using Loki.Bot;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;
using Message = Loki.Bot.Message;

namespace Default.MapBot
{
    public class DeviceAreaTask : ITask
    {
        private static bool _toMap;

        public async Task<bool> Run()
        {
            var area = World.CurrentArea;
            if (area.IsHideoutArea)
            {
                await EnterMapPortal();
                return true;
            }
            if (area.IsMapRoom)
            {
                if (_toMap)
                {
                    if (await HandleStairs(true))
                        return true;

                    await EnterMapPortal();
                    return true;
                }

                var portal = ClosestTownPortal;
                if (portal != null && portal.PathExists())
                {
                    if (!await PlayerAction.TakePortal(portal))
                        ErrorManager.ReportError();

                    return true;
                }

                if (await HandleStairs(false))
                    return true;

                if (!await PlayerAction.TakeWaypoint(World.Act11.Oriath))
                    ErrorManager.ReportError();

                return true;
            }
            return false;
        }

        public static async Task<bool> HandleStairs(bool down)
        {
            var wp = LokiPoe.ObjectManager.Objects.Find(o => o is Waypoint);
            if (down)
            {
                if (wp != null && wp.PathExists())
                {
                    if (!await PlayerAction.TakeTransition(ClosestLocalTransition))
                        ErrorManager.ReportError();

                    return true;
                }
            }
            else
            {
                if (wp == null || !wp.PathExists())
                {
                    if (!await PlayerAction.TakeTransition(ClosestLocalTransition))
                        ErrorManager.ReportError();

                    return true;
                }
            }
            return false;
        }

        private static async Task EnterMapPortal()
        {
            var portal = ClosestActiveMapPortal;
            if (portal == null)
            {
                GlobalLog.Error("[DeviceAreaTask] Fail to find any active map portal.");
                MapBot.IsOnRun = false;
                _toMap = false;
                return;
            }
            if (!await PlayerAction.TakePortal(portal))
                ErrorManager.ReportError();
        }

        private static Portal ClosestActiveMapPortal
        {
            get
            {
                var mapPortal = LokiPoe.ObjectManager.Objects.Closest<Portal>(p => p.IsTargetable && p.LeadsTo(a => a.IsMap));

                if (mapPortal != null)
                    return mapPortal;

                // Zana daily quest
                return LokiPoe.ObjectManager.Objects.Closest<Portal>(p => p.IsTargetable && p.LeadsTo(a => a.IsMapRoom));
            }
        }

        private static Portal ClosestTownPortal => LokiPoe.ObjectManager.Objects
            .Closest<Portal>(p => p.IsTargetable && p.LeadsTo(a => a.IsTown || a.IsHideoutArea));

        private static AreaTransition ClosestLocalTransition => LokiPoe.ObjectManager.Objects
            .Closest<AreaTransition>(a => a.IsTargetable && a.TransitionType == TransitionTypes.Local);

        public MessageResult Message(Message message)
        {
            var id = message.Id;
            if (id == Events.Messages.PlayerResurrected)
            {
                _toMap = true;
                return MessageResult.Processed;
            }
            if (id == Events.Messages.AreaChanged)
            {
                var newArea = message.GetInput<DatWorldAreaWrapper>(3);
                if (newArea.IsMap)
                {
                    _toMap = false;
                    return MessageResult.Processed;
                }
                var oldArea = message.GetInput<DatWorldAreaWrapper>(2);
                if (oldArea != null && !oldArea.IsMap && newArea.IsMapRoom)
                {
                    _toMap = true;
                }
                return MessageResult.Processed;
            }
            return MessageResult.Unprocessed;
        }

        #region Unused interface methods

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public void Tick()
        {
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public string Name => "DeviceAreaTask";
        public string Description => "Task for traveling through area containing Map Device.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}