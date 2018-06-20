using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Loki.Bot;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A9_Q2_QueenOfSands
    {
        private const int FinishedStateMinimum = 2;
        private static bool _finished;

        private static Monster Shakari => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Shakari_Queen_of_the_Sands)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        public static void Tick()
        {
            _finished = QuestManager.GetStateInaccurate(Quests.QueenOfSands) <= FinishedStateMinimum;
        }

        public static async Task<bool> TalkToSin()
        {
            if (World.Act9.Highgate.IsCurrentArea)
            {
                if (!await Helpers.Sin_A9.Talk())
                {
                    ErrorManager.ReportError();
                    return true;
                }
                await Coroutines.CloseBlockingWindows();
                return false;
            }
            await Travel.To(World.Act9.Highgate);
            return true;
        }

        public static async Task<bool> KillShakari()
        {
            if (_finished)
                return false;

            if (World.Act9.Oasis.IsCurrentArea)
            {
                var shakari = Shakari;
                if (shakari != null && shakari.PathExists())
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.Shakari))
                        return true;

                    await Helpers.MoveAndWait(shakari);
                    return true;
                }
                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act9.Oasis);
            return true;
        }

        public static async Task<bool> TakeBottledStorm()
        {
            return await Helpers.TakeQuestReward(
                World.Act9.Highgate,
                TownNpcs.PetarusAndVanja_A9,
                "Take Bottled Storm");
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act9.Highgate,
                TownNpcs.Irasha,
                "Shakari Reward",
                book: QuestItemMetadata.BookQueenOfSands);
        }
    }
}
