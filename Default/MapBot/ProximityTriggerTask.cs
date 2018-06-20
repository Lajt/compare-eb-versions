using System;
using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Loki.Bot;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.MapBot
{
    public class ProximityTriggerTask : ITask
    {
        private static readonly Interval TickInterval = new Interval(200);

        private static string _triggerMetadata;
        private static CachedObject _trigger;
        private static Func<Task> _waitFunc;

        public async Task<bool> Run()
        {
            if (_triggerMetadata == null || MapExplorationTask.MapCompleted)
                return false;

            if (_trigger == null || _trigger.Ignored || _trigger.Unwalkable)
                return false;

            if (!World.CurrentArea.IsMap)
                return false;

            var pos = _trigger.Position;
            if (pos.Distance > 10 || pos.PathDistance > 12)
            {
                if (!pos.TryCome())
                {
                    GlobalLog.Error($"[ProximityTriggerTask] Fail to move to {pos}. Marking this trigger object as unwalkable.");
                    _trigger.Unwalkable = true;
                }
                return true;
            }

            await Coroutines.FinishCurrentAction();

            if (_waitFunc != null)
                await _waitFunc();

            _triggerMetadata = null;
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
                if (obj.Metadata == _triggerMetadata)
                {
                    var pos = obj.WalkablePosition();
                    GlobalLog.Warn($"[ProximityTriggerTask] Registering {pos}");
                    _trigger = new CachedObject(obj.Id, pos);
                    return;
                }
            }
        }

        private static void Reset(string areaName)
        {
            _triggerMetadata = null;
            _trigger = null;
            _waitFunc = null;

            if (areaName == MapNames.Mausoleum)
            {
                _triggerMetadata = "Metadata/Terrain/EndGame/MapMausoleum/Objects/AnkhOfEternityMap";
                _waitFunc = MausoleumWait;
                return;
            }
            if (areaName == MapNames.MaoKun)
            {
                _triggerMetadata = "Metadata/Terrain/EndGame/MapTreasureIsland/Objects/FairgravesTreasureIsland";
                _waitFunc = MaoKunWait;
            }
        }

        private static async Task MausoleumWait()
        {
            await Wait.For(() => LokiPoe.ObjectManager.Objects
                .Any<Monster>(m => m.Distance < 70 && m.IsActive), "any active monster", 500, 10000);
        }

        private static async Task MaoKunWait()
        {
            await Wait.For(() =>
            {
                var fairgraves = LokiPoe.ObjectManager.Objects
                    .Find(o => o.Metadata == "Metadata/Terrain/EndGame/MapTreasureIsland/Objects/FairgravesTreasureIsland");
                return fairgraves != null && fairgraves.IsTargetable;
            }, "Fairgraves activation", 500, 7000);
        }

        public MessageResult Message(Message message)
        {
            var id = message.Id;
            if (id == MapBot.Messages.NewMapEntered)
            {
                GlobalLog.Info("[ProximityTriggerTask] Reset.");

                Reset(message.GetInput<string>());

                if (_triggerMetadata != null)
                    GlobalLog.Info("[ProximityTriggerTask] Enabled.");

                return MessageResult.Processed;
            }
            if (id == ComplexExplorer.LocalTransitionEnteredMessage)
            {
                if (_trigger != null)
                {
                    GlobalLog.Info("[ProximityTriggerTask] Resetting unwalkable flag.");
                    _trigger.Unwalkable = false;
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

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public string Name => "ProximityTriggerTask";
        public string Description => "Task that comes to certain objects to trigger an event.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}