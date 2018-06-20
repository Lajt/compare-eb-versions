using System.Threading.Tasks;
using Loki.Bot;

namespace Default.EXtensions.CommonTasks
{
    public class CombatTask : ITask
    {
        private readonly int _leashRange;

        public CombatTask(int leashRange)
        {
            _leashRange = leashRange;
        }

        public async Task<bool> Run()
        {
            if (!World.CurrentArea.IsCombatArea)
                return false;

            var routine = RoutineManager.Current;

            routine.Message(new Message("SetLeash", this, _leashRange));

            var res = await routine.Logic(new Logic("hook_combat", this));
            return res == LogicResult.Provided;
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

        public string Name => "CombatTask (Leash " + _leashRange + ")";

        public string Description => "This task executes routine logic for combat.";

        public string Author => "Bossland GmbH";

        public string Version => "1.0";

        #endregion
    }
}