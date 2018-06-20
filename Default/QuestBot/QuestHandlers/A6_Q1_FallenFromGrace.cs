using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;

namespace Default.QuestBot.QuestHandlers
{
    public static class A6_Q1_FallenFromGrace
    {
        private const int FinishedStateMinimum = 2;
        private static bool _finished;

        private static TownNpc _townLilly = new TownNpc(new WalkablePosition("Lilly Roth", 229, 158));

        public static void Tick()
        {
            _finished = QuestManager.GetStateInaccurate(Quests.FallenFromGrace) <= FinishedStateMinimum;
        }

        public static async Task<bool> ClearStrand()
        {
            if (_finished)
                return false;

            if (World.Act6.TwilightStrand.IsCurrentArea)
            {
                if (await TrackMobLogic.Execute())
                    return true;

                if (!await CombatAreaCache.Current.Explorer.Execute())
                {
                    if (QuestManager.GetState(Quests.FallenFromGrace) <= FinishedStateMinimum)
                        return false;

                    GlobalLog.Error("[ClearTwilightStrand] Twilight Strand is fully explored but not all monsters were killed. Now going to create a new Twilight Strand instance.");

                    Travel.RequestNewInstance(World.Act6.TwilightStrand);

                    if (!await PlayerAction.TpToTown())
                        ErrorManager.ReportError();
                }
                return true;
            }
            await Travel.To(World.Act6.TwilightStrand);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            if (World.Act6.LioneyeWatch.IsCurrentArea)
            {
                await _townLilly.Position.ComeAtOnce();

                if (_townLilly.NpcObject == null)
                {
                    if (_townLilly == TownNpcs.LillyRoth)
                    {
                        GlobalLog.Error("[FallenFromGrace] Unknown error. There is no Lilly Roth.");
                        ErrorManager.ReportCriticalError();
                        return true;
                    }
                    GlobalLog.Warn("[FallenFromGrace] There is no Lilly Roth near ship. Now going to her default position.");
                    _townLilly = TownNpcs.LillyRoth;
                    return true;
                }

                return await Helpers.TakeQuestReward(
                    World.Act6.LioneyeWatch,
                    _townLilly,
                    "Twilight Strand Reward",
                    book: QuestItemMetadata.BookFallenFromGrace);
            }
            await Travel.To(World.Act6.LioneyeWatch);
            return true;
        }
    }
}