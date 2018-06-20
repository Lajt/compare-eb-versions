using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A7_Q7_QueenOfDespair
    {
        private static readonly TgtPosition GruthkulRoomTgt = new TgtPosition("Gruthkul room", "forestcave_entrance_hole_v01_01_c2r2.tgt");

        private const int FinishedStateMinimum = 2;
        private static bool _finished;

        private static Monster Gruthkul => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Gruthkul_Mother_of_Despair)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        public static void Tick()
        {
            _finished = QuestManager.GetStateInaccurate(Quests.QueenOfDespair) <= FinishedStateMinimum;
        }

        public static async Task<bool> KillGruthkul()
        {
            if (_finished)
                return false;

            if (World.Act7.DreadThicket.IsCurrentArea)
            {
                var gruthkul = Gruthkul;
                if (gruthkul != null && gruthkul.PathExists())
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.Gruthkul))
                        return true;

                    await Helpers.MoveAndWait(gruthkul);
                    return true;
                }
                await Helpers.MoveAndTakeLocalTransition(GruthkulRoomTgt);
                return true;
            }
            await Travel.To(World.Act7.DreadThicket);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act7.BridgeEncampment,
                TownNpcs.Eramir_A7,
                "Gruthkul Reward",
                book: QuestItemMetadata.BookGruthkul);
        }
    }
}