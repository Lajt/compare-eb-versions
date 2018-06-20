using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Loki.Bot;
using Loki.Game;
using Message = Loki.Bot.Message;
using Skillbar = Loki.Game.LokiPoe.InGameState.SkillBarHud;

namespace Default.EXtensions.CommonTasks
{
    public class AssignMoveSkillTask : ITask
    {
        private bool _shouldAssign;

        public async Task<bool> Run()
        {
            if (!_shouldAssign)
                return false;

            GlobalLog.Debug("[AssignMoveSkillTask] Now going to assign the Move skill to the skillbar. It must be bound to anything except left mouse button.");

            var emptySlot = FirstEmptySkillSlot;
            if (emptySlot == -1)
            {
                GlobalLog.Error("[AssignMoveSkillTask] Cannot assign the Move skill. There are no free slots on the skillbar.");
                BotManager.Stop();
                return true;
            }

            var moveSkill = Skillbar.Skills.FirstOrDefault(s => s != null && s.InternalName == "Move");
            if (moveSkill == null)
            {
                GlobalLog.Error("[AssignMoveSkillTask] Unknown error. Cannot find the Move skill on the skillbar.");
                BotManager.Stop();
                return true;
            }

            var err = Skillbar.SetSlot(emptySlot, moveSkill);
            if (err != LokiPoe.InGameState.SetSlotResult.None)
            {
                GlobalLog.Error($"[AssignMoveSkillTask] Fail to assign the Move skill to slot {emptySlot}. Error: \"{err}\".");
                ErrorManager.ReportError();
                await Wait.SleepSafe(500);
                return true;
            }

            if (!await Wait.For(() => Skillbar.Slot(emptySlot)?.InternalName == "Move", "skill slot changing"))
            {
                ErrorManager.ReportError();
                return true;
            }

            GlobalLog.Debug($"[AssignMoveSkillTask] Move skill has been successfully assigned to slot {emptySlot}.");
            _shouldAssign = false;
            return false;
        }

        public MessageResult Message(Message message)
        {
            if (message.Id == Events.Messages.IngameBotStart)
            {
                _shouldAssign = !IsMoveSkillAssigned;
                return MessageResult.Processed;
            }
            return MessageResult.Unprocessed;
        }

        private static bool IsMoveSkillAssigned
        {
            get
            {
                var skill = Skillbar.LastBoundMoveSkill;
                return skill != null && skill.BoundKeys.Last() != Keys.LButton;
            }
        }

        private static int FirstEmptySkillSlot
        {
            get
            {
                for (int i = 1; i < 8; ++i)
                {
                    if (LokiPoe.InstanceInfo.SkillBarIds[i] == 0)
                        return i + 1;
                }
                return -1;
            }
        }

        #region Unused interface methods

        public void Tick()
        {
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public string Name => "AssignMoveSkillTask";
        public string Description => "Task that assigns the Move skill to the skillbar.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}
