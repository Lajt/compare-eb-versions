using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A6_Q2_BestelEpic
    {
        private static readonly TgtPosition StorageChestTgt = new TgtPosition("Storage chest location", "kyrenia_boat_medicinequest_v01_01_c3r2.tgt");

        private static Chest StorageChest => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Storage_Chest)
            .FirstOrDefault<Chest>();

        private static CachedObject CachedStorageChest
        {
            get => CombatAreaCache.Current.Storage["StorageChest"] as CachedObject;
            set => CombatAreaCache.Current.Storage["StorageChest"] = value;
        }

        public static void Tick()
        {
            if (!World.Act6.TidalIsland.IsCurrentArea)
                return;

            if (CachedStorageChest == null)
            {
                var chest = StorageChest;
                if (chest != null)
                {
                    CachedStorageChest = new CachedObject(chest);
                }
            }
        }

        public static async Task<bool> GrabManuscript()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.BestelManuscript))
                return false;

            if (World.Act6.TidalIsland.IsCurrentArea)
            {
                if (await Helpers.OpenQuestChest(CachedStorageChest))
                    return true;

                StorageChestTgt.Come();
                return true;
            }
            await Travel.To(World.Act6.TidalIsland);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act6.LioneyeWatch,
                TownNpcs.Bestel_A6,
                "Bestel's Epic Reward",
                Quests.BestelEpic.Id);
        }
    }
}