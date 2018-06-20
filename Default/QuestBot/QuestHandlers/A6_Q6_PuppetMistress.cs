using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A6_Q6_PuppetMistress
    {
        private static readonly TgtPosition RyslathaRoomTgt = new TgtPosition("Ryslatha room", "forest_caveentrance_v01_01_c2r4.tgt", true);

        private const int FinishedStateMinimum = 2;
        private static bool _finished;

        private static Monster Ryslatha => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Ryslatha_the_Puppet_Mistress)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static Monster ClosestRyslathaNest => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Ryslathas_Nest)
            .Closest<Monster>(m => m.Rarity == Rarity.Unique && !m.IsDead);

        public static void Tick()
        {
            _finished = QuestManager.GetStateInaccurate(Quests.PuppetMistress) <= FinishedStateMinimum;
        }

        public static async Task<bool> KillRyslatha()
        {
            if (_finished)
                return false;

            if (World.Act6.Wetlands.IsCurrentArea)
            {
                var ryslatha = Ryslatha;
                if (ryslatha != null)
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.Ryslatha))
                        return true;

                    var nest = ClosestRyslathaNest;
                    if (nest != null)
                    {
                        await Helpers.MoveAndWait(nest.WalkablePosition());
                        return true;
                    }
                    await Helpers.MoveAndWait(ryslatha.WalkablePosition());
                    return true;
                }
                await Helpers.MoveAndTakeLocalTransition(RyslathaRoomTgt);
                return true;
            }
            await Travel.To(World.Act6.Wetlands);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act6.LioneyeWatch,
                TownNpcs.Tarkleigh_A6,
                "Puppet Mistress Reward",
                book: QuestItemMetadata.BookRyslatha);
        }
    }
}