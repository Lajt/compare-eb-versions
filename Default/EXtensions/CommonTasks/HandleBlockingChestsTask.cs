using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions.Global;
using Loki.Bot;
using Loki.Common;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.EXtensions.CommonTasks
{
    public class HandleBlockingChestsTask : ITask
    {
        private static readonly HashSet<int> Processed = new HashSet<int>();

        public static bool Enabled;

        public async Task<bool> Run()
        {
            if (!Enabled || !World.CurrentArea.IsCombatArea)
                return false;

            var chests = LokiPoe.ObjectManager.Objects
                .Where<Chest>(c => c.Distance <= 10 && !c.IsOpened && c.IsStompable && !Processed.Contains(c.Id))
                .OrderBy(c => c.DistanceSqr)
                .ToList();

            if (chests.Count == 0)
            {
                //run this task only once, by StuckDetection demand
                Enabled = false;
                return false;
            }

            LokiPoe.ProcessHookManager.Reset();
            await Coroutines.CloseBlockingWindows();

            var positions1 = new List<Vector2i> {LokiPoe.MyPosition};
            var positions2 = new List<Vector2> {LokiPoe.MyWorldPosition};

            foreach (var chest in chests)
            {
                Processed.Add(chest.Id);
                positions1.Add(chest.Position);
                positions2.Add(chest.WorldPosition);
            }

            foreach (var position in positions1)
            {
                MouseManager.SetMousePos("EXtensions.CommonTasks.HandleBlockingChestsTask", position);
                await Click();
            }

            foreach (var position in positions2)
            {
                MouseManager.SetMousePos("EXtensions.CommonTasks.HandleBlockingChestsTask", position);
                await Click();
            }
            return true;
        }

        private static async Task Click()
        {
            StuckDetection.Reset();
            await Wait.LatencySleep();
            var target = LokiPoe.InGameState.CurrentTarget;
            if (target != null)
            {
                GlobalLog.Info($"[HandleBlockingChestsTask] \"{target.Name}\" ({target.Id}) is under the cursor. Now clicking on it.");
                LokiPoe.Input.PressLMB();
                await Coroutines.FinishCurrentAction(false);
            }
        }

        public MessageResult Message(Message message)
        {
            if (message.Id == Events.Messages.AreaChanged)
            {
                Processed.Clear();
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

        public void Tick()
        {
        }

        public void Stop()
        {
        }

        public string Name => "HandleBlockingChestsTask";
        public string Description => "This task will handle breaking any blocking chests that interfere with movement.";
        public string Author => "Bossland GmbH";
        public string Version => "0.0.1.1";

        #endregion
    }
}