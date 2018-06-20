using System.Threading.Tasks;
using Default.EXtensions;
using Loki.Bot;
using Loki.Game;

namespace Default.QuestBot
{
    public class QuestTask : ITask
    {
        private readonly Interval _scanInterval = new Interval(200);
        private QuestHandler _handler;

        public async Task<bool> Run()
        {
            if (_handler == null)
            {
                _handler = QuestManager.GetQuestHandler();
                if (_handler == null)
                {
                    GlobalLog.Error("[QuestTask] QuestManager returned null. Lets wait and see maybe something will change in game memory.");
                    ErrorManager.ReportError();
                    await Wait.SleepSafe(500);
                    return true;
                }
                if (_handler == QuestHandler.QuestAddedToCache)
                {
                    GlobalLog.Debug("[QuestTask] Quest was added to Completed quests cache. Now requesting quest handler again.");
                    _handler = null;
                    return true;
                }
                if (_handler == QuestHandler.AllQuestsDone)
                {
                    GlobalLog.Warn("[QuestTask] It seems like all quests are completed.");
                    _handler = null;
                    BotManager.Stop();
                    return true;
                }
                _handler.Tick?.Invoke();
            }

            if (Settings.Instance.TalkToQuestgivers && World.CurrentArea.IsTown)
            {
                if (TownQuestgiversLogic.ShouldExecute && await TownQuestgiversLogic.Execute())
                    return true;
            }

            if (!await _handler.Execute())
                _handler = null;

            return true;
        }

        public void Tick()
        {
            if (!_scanInterval.Elapsed)
                return;

            if (!LokiPoe.IsInGame || LokiPoe.Me.IsDead)
                return;

            if (Settings.Instance.TalkToQuestgivers && World.CurrentArea.IsTown)
                TownQuestgiversLogic.Tick();

            _handler?.Tick?.Invoke();
        }

        public MessageResult Message(Message message)
        {
            if (message.Id == Events.Messages.AreaChanged)
            {
                TownQuestgiversLogic.Reset();
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

        public string Name => "QuestTask";
        public string Author => "ExVault";
        public string Description => "Task that executes quest logic.";
        public string Version => "1.0";

        #endregion
    }
}