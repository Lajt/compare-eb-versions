using System.Threading.Tasks;
using Loki.Bot;

namespace Default.EXtensions.CommonTasks
{
    public class FallbackTask : ITask
    {
        public async Task<bool> Run()
        {
            GlobalLog.Error("[FallbackTask] The Fallback task is executing. The bot does not know what to do.");
            await Wait.Sleep(200);
            return true;
        }

        #region Unused interface methods

        public MessageResult Message(Message message)
        {
            return MessageResult.Unprocessed;
        }

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public void Start()
        {
        }

        public void Tick()
        {
        }

        public void Stop()
        {
        }

        public string Name => "FallbackTask";
        public string Description => "This task is the last task executed. It should not execute.";
        public string Author => "Bossland GmbH";
        public string Version => "1.0";

        #endregion
    }
}