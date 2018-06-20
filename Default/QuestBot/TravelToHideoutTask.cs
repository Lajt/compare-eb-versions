using System.Threading.Tasks;
using Default.EXtensions;
using Loki.Bot;
using Loki.Game.GameData;

namespace Default.QuestBot
{
    public class TravelToHideoutTask : ITask
    {
        internal static bool IsBlocked;

        public async Task<bool> Run()
        {
            if (IsBlocked)
                return false;

            var settings = Settings.Instance;

            if (!settings.UseHideout || settings.CurrentQuestName != GrindingHandler.Name)
                return false;

            if (!World.CurrentArea.IsTown)
                return false;

            if (!await PlayerAction.GoToHideout())
                ErrorManager.ReportError();

            return true;
        }

        public MessageResult Message(Message message)
        {
            if (Settings.Instance.UseHideout && message.Id == Events.Messages.AreaChanged)
            {
                var oldArea = message.GetInput<DatWorldAreaWrapper>(2);

                if (oldArea != null && !oldArea.IsHideoutArea)
                    IsBlocked = false;

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

        public string Name => "TravelToHideoutTask";
        public string Description => "Task for traveling to player's hideout.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}