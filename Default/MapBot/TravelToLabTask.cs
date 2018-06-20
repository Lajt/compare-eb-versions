using System.Threading.Tasks;
using Default.EXtensions;
using Loki.Bot;

namespace Default.MapBot
{
    public class TravelToLabTask : ITask
    {
        public async Task<bool> Run()
        {
            if (GeneralSettings.Instance.UseHideout)
                return false;

            var area = World.CurrentArea;
            if (area.IsMapRoom || area.IsMap)
                return false;

            if (area.IsTown || area.IsHideoutArea)
            {
                if (!await PlayerAction.TakeWaypoint(World.Act11.TemplarLaboratory))
                    ErrorManager.ReportError();
            }
            else
            {
                if (!await PlayerAction.TpToTown())
                    ErrorManager.ReportError();
            }
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

        public string Name => "TravelToLabTask";
        public string Description => "Task for traveling to The Eternal Laboratory.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}