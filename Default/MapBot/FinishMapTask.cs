using System.Threading.Tasks;
using Default.EXtensions;
using Loki.Bot;

namespace Default.MapBot
{
    public class FinishMapTask : ITask
    {
        private static int _pulse;

        public async Task<bool> Run()
        {
            if (!World.CurrentArea.IsMap)
                return false;

            await Coroutines.FinishCurrentAction();

            var maxPulses = MaxPulses;
            if (_pulse < maxPulses)
            {
                ++_pulse;
                GlobalLog.Info($"[FinishMapTask] Final pulse {_pulse}/{maxPulses}");
                await Wait.SleepSafe(500);
                return true;
            }

            GlobalLog.Warn("[FinishMapTask] Now leaving current map.");

            if (!await PlayerAction.TpToTown())
            {
                ErrorManager.ReportError();
                return true;
            }

            MapBot.IsOnRun = false;
            Statistics.Instance.OnMapFinish();
            GlobalLog.Info("[MapBot] MapFinished event.");
            Utility.BroadcastMessage(this, MapBot.Messages.MapFinished);
            return true;
        }

        private static int MaxPulses
        {
            get
            {
                if (!KillBossTask.BossKilled && !MapData.Current.IgnoredBossroom)
                {
                    var areaName = World.CurrentArea.Name;

                    if (areaName == MapNames.JungleValley || areaName == MapNames.Mausoleum)
                        return 10;

                    if (areaName == MapNames.ArachnidNest || areaName == MapNames.Lookout)
                        return 8;
                }
                return 3;
            }
        }

        public MessageResult Message(Message message)
        {
            if (message.Id == MapBot.Messages.NewMapEntered)
            {
                _pulse = 0;
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

        public string Name => "FinishMapTask";
        public string Description => "Task for leaving current map.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}