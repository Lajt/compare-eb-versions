using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;

namespace Default.QuestBot.QuestHandlers
{
    public static class A1_Q3_DirtyJob
    {
        private const int FinishedStateMinimum = 3;
        private static bool _finished;

        public static void Tick()
        {
            _finished = QuestManager.GetStateInaccurate(Quests.DirtyJob) <= FinishedStateMinimum;
        }

        public static async Task<bool> ClearFetidPool()
        {
            if (_finished)
                return false;

            if (World.Act1.FetidPool.IsCurrentArea)
            {
                if (await TrackMobLogic.Execute())
                    return true;

                if (!await CombatAreaCache.Current.Explorer.Execute())
                {
                    if (QuestManager.GetState(Quests.DirtyJob) <= FinishedStateMinimum)
                        return false;

                    GlobalLog.Error("[ClearFetidPool] Fetid Pool is fully explored but not all monsters were killed. Now going to create a new Fetid Pool instance.");

                    Travel.RequestNewInstance(World.Act1.FetidPool);

                    if (!await PlayerAction.TpToTown())
                        ErrorManager.ReportError();
                }
                return true;
            }
            await Travel.To(World.Act1.FetidPool);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act1.LioneyeWatch,
                TownNpcs.Tarkleigh,
                "Necromancer Reward",
                book: QuestItemMetadata.BookDirtyJob);
        }
    }
}