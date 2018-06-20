using System.Threading.Tasks;
using Loki.Bot;

namespace Default.EXtensions.CommonTasks
{
    public class PostCombatHookTask : ITask
    {
        public const string MessageId = "hook_post_combat";

        public async Task<bool> Run()
        {
            foreach (var plugin in PluginManager.EnabledPlugins)
            {
                if (await plugin.Logic(new Logic(MessageId, this)) == LogicResult.Provided)
                {
                    GlobalLog.Info($"[PostCombatHookTask] \"{plugin.Name}\" returned true.");
                    return true;
                }
            }
            foreach (var content in ContentManager.Contents)
            {
                if (await content.Logic(new Logic(MessageId, this)) == LogicResult.Provided)
                {
                    GlobalLog.Info($"[PostCombatHookTask] \"{content.Name}\" returned true.");
                    return true;
                }
            }
            return false;
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

        public string Name => "PostCombatHookTask";
        public string Description => "This task provides a coroutine hook for executing user logic after combat has completed.";
        public string Author => "Bossland GmbH";
        public string Version => "1.0";

        #endregion
    }
}