using System.Threading.Tasks;
using Loki.Bot;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.EXtensions.CommonTasks
{
    public class LeaveAreaTask : ITask
    {
        private static bool _isActive;

        public static bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                GlobalLog.Debug(value ? "[LeaveAreaTask] Activated." : "[LeaveAreaTask] Deactivated.");
            }
        }

        public async Task<bool> Run()
        {
            if (!IsActive || !World.CurrentArea.IsCombatArea)
                return false;

            if (AnyMobsNearby)
            {
                GlobalLog.Warn("[LeaveAreaTask] Now logging out because there are monsters nearby.");
                if (!await PlayerAction.Logout())
                {
                    ErrorManager.ReportError();
                    return true;
                }
            }
            else
            {
                GlobalLog.Debug("[LeaveAreaTask] Now leaving current area.");
                if (!await PlayerAction.TpToTown(true))
                {
                    ErrorManager.ReportError();
                    return true;
                }
            }

            IsActive = false;
            return true;
        }

        private static bool AnyMobsNearby => LokiPoe.ObjectManager.Objects.Any<Monster>(m => m.IsActive && m.Distance <= 50);

        #region Unused interface methods

        public MessageResult Message(Message message)
        {
            return MessageResult.Unprocessed;
        }

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

        public string Name => "LeaveAreaTask";
        public string Description => "Task to prematurely leave an area.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}