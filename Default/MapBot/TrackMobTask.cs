using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Loki.Bot;

namespace Default.MapBot
{
    public class TrackMobTask : ITask
    {
        private const int RestrictedRange = 100;

        private static int _range = -1;

        public async Task<bool> Run()
        {
            // ReSharper disable once PossibleInvalidOperationException
            if (!MapData.Current.TrackMob.Value)
                return false;

            if (!World.CurrentArea.IsMap)
                return false;

            return await TrackMobLogic.Execute(_range);
        }

        internal static void RestrictRange()
        {
            GlobalLog.Info($"[TrackMobTask] Restricting monster tracking range to {RestrictedRange}");
            _range = RestrictedRange;
            TrackMobLogic.CurrentTarget = null;
        }

        public MessageResult Message(Message message)
        {
            if (message.Id == MapBot.Messages.NewMapEntered)
            {
                _range = -1;

                var areaName = message.GetInput<string>();
                if (areaName == MapNames.MaoKun)
                {
                    MapData.Current.TrackMob = true;
                    GlobalLog.Info("[MapExplorationTask] Monster Tracking is hard enabled for this map.");
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

        public string Name => "TrackMobTask";
        public string Description => "Task for tracking monsters.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}