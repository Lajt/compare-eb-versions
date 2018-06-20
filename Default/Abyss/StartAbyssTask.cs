using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Loki.Bot;

namespace Default.Abyss
{
    public class StartAbyssTask : ITask
    {
        private const int MaxAttempts = 15;

        internal static CachedObject StartNode;

        public async Task<bool> Run()
        {
            if (!World.CurrentArea.IsCombatArea)
                return false;

            if (StartNode == null)
            {
                if ((StartNode = Abyss.CachedData.StartNodes.ClosestValid()) == null)
                    return false;
            }

            var pos = StartNode.Position;
            if (pos.Distance > 10 || pos.PathDistance > 10)
            {
                if (!pos.TryCome())
                {
                    GlobalLog.Error($"[StartAbyssTask] Fail to move to {pos}. Abyss start node is unwalkable.");
                    StartNode.Unwalkable = true;
                    StartNode = null;
                }
                return true;
            }

            var attempts = ++StartNode.InteractionAttempts;
            if (attempts > MaxAttempts)
            {
                GlobalLog.Error("[StartAbyssTask] Abyss start node activation timeout. Now ignoring it.");
                StartNode.Ignored = true;
                StartNode = null;
                return true;
            }
            GlobalLog.Debug($"[StartAbyssTask] Waiting for Abyss start node activation ({attempts}/{MaxAttempts})");
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

        public void Tick()
        {
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public string Name => "StartAbyssTask";
        public string Description => "Task that starts abyss crack";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}