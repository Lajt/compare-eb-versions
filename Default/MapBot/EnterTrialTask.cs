using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Loki.Bot;

namespace Default.MapBot
{
    public class EnterTrialTask : ITask
    {
        private const int MaxInteractionAttempts = 10;

        private static CachedTransition _trial;
        private static bool _enabled;

        public async Task<bool> Run()
        {
            if (!_enabled)
                return false;

            var area = World.CurrentArea;
            if (!area.IsMap)
            {
                if (area.IsMapTrialArea)
                {
                    GlobalLog.Warn("[EnterTrialTask] We are inside a map trial. Now stopping the bot.");
                    _enabled = false;
                    GlobalLog.Info("[MapBot] MapTrialEntered event.");
                    Utility.BroadcastMessage(this, MapBot.Messages.MapTrialEntered);
                    BotManager.Stop();
                    return true;
                }
                return false;
            }

            if (_trial == null)
            {
                var trial = CombatAreaCache.Current.AreaTransitions.Find(a => a.Type == TransitionType.Trial);
                if (trial == null) return false;

                var name = trial.Position.Name;
                if (!GeneralSettings.Instance.TrialEnabled(name))
                {
                    GlobalLog.Debug($"[EnterTrialTask] Detected \"{name}\" but is it not enabled in settings. Skipping this task.");
                    _enabled = false;
                    return true;
                }
                GlobalLog.Warn($"[EnterTrialTask] \"{name}\" has been detected. Bot will enter it and stop.");
                _trial = trial;
            }

            var pos = _trial.Position;
            if (pos.IsFar || pos.PathDistance > 20)
            {
                if (!pos.TryCome())
                {
                    GlobalLog.Error($"[EnterTrialTask] Fail to move to {pos}. Trial transition is unwalkable.");
                    _enabled = false;
                }
                return true;
            }
            var trialObj = _trial.Object;
            if (trialObj == null)
            {
                GlobalLog.Error("[EnterTrialTask] Unexpected error. We are near cached trial transition but actual object is null.");
                _enabled = false;
                return true;
            }
            var attempts = ++_trial.InteractionAttempts;
            if (attempts > MaxInteractionAttempts)
            {
                GlobalLog.Error("[EnterTrialTask] All attempts to interact with trial transition have been spent.");
                _enabled = false;
                return true;
            }
            if (!await PlayerAction.TakeTransition(trialObj))
            {
                await Wait.SleepSafe(500);
            }
            return true;
        }

        public MessageResult Message(Message message)
        {
            if (message.Id == MapBot.Messages.NewMapEntered)
            {
                GlobalLog.Info("[EnterTrialTask] Reset.");
                _trial = null;
                _enabled = true;
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

        public string Name => "EnterTrialTask";
        public string Description => "Task for entering map trials.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}