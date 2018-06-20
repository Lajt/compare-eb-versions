using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A10_Q3_VilentaVengeance
    {
        private static readonly TgtPosition VilentaRoomTgt = new TgtPosition("Vilenta room", "slave_ledge_doubledoor_transition_v01_01_c1r2.tgt");

        private const int FinishedStateMinimum = 2;
        private static bool _finished;

        private static Monster Vilenta => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Vilenta)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        public static void Tick()
        {
            _finished = QuestManager.GetStateInaccurate(Quests.VilentaVengeance) <= FinishedStateMinimum;
        }

        public static async Task<bool> KillVilenta()
        {
            if (_finished)
                return false;

            if (World.Act10.ControlBlocks.IsCurrentArea)
            {
                var vilenta = Vilenta;
                if (vilenta != null && vilenta.PathExists())
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.Vilenta))
                        return true;

                    await Helpers.MoveAndWait(vilenta);
                    return true;
                }
                await Helpers.MoveAndTakeLocalTransition(VilentaRoomTgt);
                return true;
            }
            await Travel.To(World.Act10.ControlBlocks);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act10.OriathDocks,
                TownNpcs.Lani_A10,
                "Vilenta Reward",
                book: QuestItemMetadata.BookVilenta);
        }
    }
}