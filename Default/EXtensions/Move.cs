using System.Threading.Tasks;
using Loki.Bot;
using Loki.Common;
using Loki.Game;

namespace Default.EXtensions
{
    public static class Move
    {
        private static readonly Interval LogInterval = new Interval(1000);

        public static bool Towards(Vector2i pos, string destination)
        {
            if (LogInterval.Elapsed)
                GlobalLog.Debug($"[MoveTowards] Moving towards {destination} at {pos} (distance: {LokiPoe.MyPosition.Distance(pos)})");

            if (!PlayerMoverManager.MoveTowards(pos))
            {
                GlobalLog.Error($"[MoveTowards] Fail to move towards {destination} at {pos}");
                return false;
            }
            return true;
        }

        public static void TowardsWalkable(Vector2i pos, string destination)
        {
            if (!Towards(pos, destination))
            {
                GlobalLog.Error($"[MoveTowardsWalkable] Unexpected error. Fail to move towards {destination} at {pos}");
                ErrorManager.ReportError();
            }
        }

        public static async Task AtOnce(Vector2i pos, string destination, int minDistance = 20)
        {
            if (LokiPoe.MyPosition.Distance(pos) <= minDistance)
                return;

            while (LokiPoe.MyPosition.Distance(pos) > minDistance)
            {
                if (LogInterval.Elapsed)
                {
                    await Coroutines.CloseBlockingWindows();
                    GlobalLog.Debug($"[MoveAtOnce] Moving to {destination} at {pos} (distance: {LokiPoe.MyPosition.Distance(pos)})");
                }

                if (!LokiPoe.IsInGame || LokiPoe.Me.IsDead || BotManager.IsStopping)
                    return;

                TowardsWalkable(pos, destination);
                await Wait.Sleep(50);
            }
            await Coroutines.FinishCurrentAction();
        }
    }
}