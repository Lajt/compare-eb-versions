using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Loki.Bot;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot
{
    public class ReturnAfterDeathTask : ITask
    {
        private CachedObject _transition;

        public async Task<bool> Run()
        {
            if (_transition == null)
                return false;

            var pos = _transition.Position;
            if (pos.IsFar)
            {
                if (!pos.TryCome())
                {
                    GlobalLog.Debug("[ReturnAfterDeathTask] Transition is unwalkable. Skipping this task.");
                    _transition = null;
                }
                return true;
            }
            var transitionObj = (AreaTransition) _transition.Object;
            if (!await PlayerAction.TakeTransition(transitionObj))
            {
                ErrorManager.ReportError();
                return true;
            }
            _transition = null;
            return true;
        }

        public MessageResult Message(Message message)
        {
            if (message.Id == Events.Messages.PlayerResurrected)
            {
                _transition = null;

                var area = World.CurrentArea;
                var id = area.Id;

                if (area.IsTown || SkipThisArea(id))
                {
                    GlobalLog.Debug($"[ReturnAfterDeathTask] Skipping this task because area is {area.Name}.");
                    return MessageResult.Processed;
                }

                AreaTransition t;

                if (id == World.Act9.RottingCore.Id)
                {
                    t = LokiPoe.ObjectManager.Objects
                        .Where<AreaTransition>(ValidRottingCoreTransition)
                        .OrderByDescending(a => a.Metadata == "Metadata/QuestObjects/Act9/HarvestFinalBossTransition")
                        .ThenBy(a => a.DistanceSqr)
                        .FirstOrDefault();
                }
                else
                {
                    t = LokiPoe.ObjectManager.Objects.Closest<AreaTransition>(a =>
                        a.TransitionType == TransitionTypes.Local &&
                        !a.Metadata.Contains("IncursionPortal") &&
                        a.Distance <= MaxDistanceByArea(id));
                }

                if (t == null)
                {
                    GlobalLog.Debug("[ReturnAfterDeathTask] There is no local area transition nearby. Skipping this task.");
                    return MessageResult.Processed;
                }

                _transition = new CachedObject(t);
                GlobalLog.Debug($"[ReturnAfterDeathTask] Detected local transition {_transition.Position}");
                return MessageResult.Processed;
            }
            return MessageResult.Unprocessed;
        }

        // HandleBlockingObjectTask is responsible for these
        private static bool SkipThisArea(string id)
        {
            return id == World.Act4.DaressoDream.Id ||
                   id == World.Act4.BellyOfBeast2.Id ||
                   id == World.Act4.Harvest.Id ||
                   id == World.Act9.Refinery.Id;
        }

        private static bool ValidRottingCoreTransition(AreaTransition t)
        {
            return t.TransitionType == TransitionTypes.Local &&
                   t.IsTargetable &&
                   t.Distance <= 50 &&
                   !t.Metadata.Contains("BellyArenaTransition") &&
                   !t.Metadata.Contains("IncursionPortal");
        }

        private static int MaxDistanceByArea(string areaId)
        {
            if (areaId == World.Act9.Descent.Id)
                return 100;

            return 50;
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

        public void Tick()
        {
        }

        public string Name => "ReturnAfterDeathTask";
        public string Description => "Task for taking closest local transition after death.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}