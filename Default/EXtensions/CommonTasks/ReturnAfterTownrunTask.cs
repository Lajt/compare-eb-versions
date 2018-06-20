using System.Threading.Tasks;
using Default.EXtensions.Positions;
using Loki.Bot;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.EXtensions.CommonTasks
{
    public class ReturnAfterTownrunTask : ITask
    {
        public static bool Enabled;

        public async Task<bool> Run()
        {
            if (!Enabled || !World.CurrentArea.IsTown)
                return false;

            await StaticPositions.GetCommonPortalSpotByAct().ComeAtOnce();

            var portalObj = LokiPoe.LocalData.TownPortals.Find(p => p.NetworkObject.IsTargetable && p.OwnerName == LokiPoe.Me.Name);
            if (portalObj == null)
            {
                GlobalLog.Error("[ReturnAfterTownrunTask] There is no portal to enter.");
                Enabled = false;
                return true;
            }

            var portal = portalObj.NetworkObject as Portal;
            await portal.WalkablePosition().ComeAtOnce();

            if (!await PlayerAction.TakePortal(portal))
            {
                ErrorManager.ReportError();
                return true;
            }
            Enabled = false;
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

        public void Stop()
        {
        }

        public void Tick()
        {
        }

        public string Name => "ReturnAfterTownrunTask";

        public string Description => "Task for returning to overworld area after townrun.";

        public string Author => "ExVault";

        public string Version => "1.0";

        #endregion
    }
}