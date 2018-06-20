using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A5_Q2_InServiceToScience
    {
        private static Chest ExperimentalSupplies => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Experimental_Supplies)
            .FirstOrDefault<Chest>();

        private static CachedObject CachedSupplies
        {
            get => CombatAreaCache.Current.Storage["ExperimentalSupplies"] as CachedObject;
            set => CombatAreaCache.Current.Storage["ExperimentalSupplies"] = value;
        }

        public static void Tick()
        {
            if (!World.Act5.ControlBlocks.IsCurrentArea)
                return;

            if (CachedSupplies == null)
            {
                var supplies = ExperimentalSupplies;
                if (supplies != null)
                {
                    CachedSupplies = new CachedObject(supplies);
                }
            }
        }

        public static async Task<bool> GrabMiasmeter()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.Miasmeter))
                return false;

            if (World.Act5.ControlBlocks.IsCurrentArea)
            {
                if (await Helpers.OpenQuestChest(CachedSupplies))
                    return true;

                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act5.ControlBlocks);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act5.OverseerTower,
                TownNpcs.Vilenta,
                "Miasmeter Reward",
                book: QuestItemMetadata.BookMiasmeter);
        }
    }
}