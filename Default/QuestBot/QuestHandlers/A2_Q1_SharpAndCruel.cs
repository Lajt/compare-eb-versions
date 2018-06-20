using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A2_Q1_SharpAndCruel
    {
        private static readonly TgtPosition WeaverRoomTgt = new TgtPosition("Weaver room", "spidergrove_exit_v01_01_c1r1.tgt");

        private static Monster Weaver => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.The_Weaver)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        public static void Tick()
        {
        }

        public static async Task<bool> KillWeaver()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.MaligaroSpike))
                return false;

            if (World.Act2.WeaverChambers.IsCurrentArea)
            {
                var weaver = Weaver;
                if (weaver != null && weaver.PathExists())
                {
                    await Helpers.MoveAndWait(weaver);
                    return true;
                }
                await Helpers.MoveAndTakeLocalTransition(WeaverRoomTgt);
                return true;
            }
            await Travel.To(World.Act2.WeaverChambers);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act2.ForestEncampment,
                TownNpcs.Silk,
                "Maligaro's Spike Reward",
                Quests.SharpAndCruel.Id);
        }
    }
}