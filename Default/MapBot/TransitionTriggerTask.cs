using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Loki.Bot;
using Loki.Game;

namespace Default.MapBot
{
    public class TransitionTriggerTask : ITask
    {
        private const int MaxInteractionAttempts = 7;
        private const int MaxTransitionWaits = 50;

        private static readonly Interval TickInterval = new Interval(200);

        private static string _triggerMetadata;
        private static CachedObject _trigger;
        private static int _waitCount;

        public async Task<bool> Run()
        {
            if (_triggerMetadata == null || _trigger == null || MapExplorationTask.MapCompleted)
                return false;

            if (!World.CurrentArea.IsMap)
                return false;

            var pos = _trigger.Position;
            if (pos.IsFar)
            {
                if (!pos.TryCome())
                {
                    GlobalLog.Error($"[TransitionTriggerTask] Fail to move to {pos}. Transition trigger is unwalkable. Bot will not be able to enter a bossroom.");
                    _triggerMetadata = null;
                }
                return true;
            }
            var triggerObj = _trigger.Object;
            if (triggerObj == null || !triggerObj.IsTargetable)
            {
                if (CombatAreaCache.Current.AreaTransitions.Any(a => a.Type == TransitionType.Local && a.Position.Distance <= 50))
                {
                    GlobalLog.Debug("[TransitionTriggerTask] Area transition has been successfully unlocked.");
                    _triggerMetadata = null;
                    return true;
                }
                ++_waitCount;
                if (_waitCount > MaxTransitionWaits)
                {
                    GlobalLog.Error("[TransitionTriggerTask] Unexpected error. Waiting for area transition spawn timeout.");
                    _triggerMetadata = null;
                    return true;
                }
                GlobalLog.Debug($"[TransitionTriggerTask] Waiting for area transition spawn {_waitCount}/{MaxTransitionWaits}");
                await Wait.StuckDetectionSleep(200);
                return true;
            }
            var attempts = ++_trigger.InteractionAttempts;
            if (attempts > MaxInteractionAttempts)
            {
                GlobalLog.Error("[TransitionTriggerTask] All attempts to interact with transition trigger have been spent.");
                _triggerMetadata = null;
                return true;
            }

            if (!await PlayerAction.Interact(triggerObj, () => !triggerObj.Fresh().IsTargetable, "transition trigger interaction", 500))
                await Wait.SleepSafe(500);

            return true;
        }

        public void Tick()
        {
            if (_triggerMetadata == null || _trigger != null || MapExplorationTask.MapCompleted)
                return;

            if (!TickInterval.Elapsed)
                return;

            if (!LokiPoe.IsInGame || !World.CurrentArea.IsMap)
                return;

            foreach (var obj in LokiPoe.ObjectManager.Objects)
            {
                if (obj.IsTargetable && obj.Metadata == _triggerMetadata)
                {
                    var pos = obj.WalkablePosition();
                    GlobalLog.Warn($"[TransitionTriggerTask] Registering {pos}");
                    _trigger = new CachedObject(obj.Id, pos);
                    return;
                }
            }
        }

        private static void Reset(string areaName)
        {
            _triggerMetadata = null;
            _trigger = null;
            _waitCount = 0;

            if (MapData.Current.IgnoredBossroom)
            {
                GlobalLog.Info("[TransitionTriggerTask] Skipping this task because bossroom is ignored.");
                return;
            }

            if (areaName == MapNames.Academy ||
                areaName == MapNames.Museum ||
                areaName == MapNames.Scriptorium)
            {
                _triggerMetadata = "Metadata/QuestObjects/Library/HiddenDoorTrigger";
                return;
            }
            if (areaName == MapNames.Necropolis)
            {
                _triggerMetadata = "Metadata/Chests/Sarcophagi/sarcophagus_door";
                return;
            }
            if (areaName == MapNames.WastePool)
            {
                _triggerMetadata = "Metadata/QuestObjects/Sewers/SewersGrate";
            }
        }

        public MessageResult Message(Message message)
        {
            if (message.Id == MapBot.Messages.NewMapEntered)
            {
                GlobalLog.Info("[TransitionTriggerTask] Reset.");

                Reset(message.GetInput<string>());

                if (_triggerMetadata != null)
                    GlobalLog.Info("[TransitionTriggerTask] Enabled.");

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

        public string Name => "TransitionTriggerTask";
        public string Description => "Task that opens transition triggers.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}