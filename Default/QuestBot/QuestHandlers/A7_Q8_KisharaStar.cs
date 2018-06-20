using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A7_Q8_KisharaStar
    {
        private static Chest KisharaLockbox => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Kisharas_Lockbox)
            .FirstOrDefault<Chest>();

        private static CachedObject CachedKisharaLockbox
        {
            get => CombatAreaCache.Current.Storage["KisharaLockbox"] as CachedObject;
            set => CombatAreaCache.Current.Storage["KisharaLockbox"] = value;
        }

        public static void Tick()
        {
            if (!World.Act7.Causeway.IsCurrentArea)
                return;

            if (CachedKisharaLockbox == null)
            {
                var lockbox = KisharaLockbox;
                if (lockbox != null)
                {
                    CachedKisharaLockbox = new CachedObject(lockbox);
                }
            }
        }

        public static async Task<bool> GrabKisharaStar()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.KisharaStar))
                return false;

            if (World.Act7.Causeway.IsCurrentArea)
            {
                if (await Helpers.OpenQuestChest(CachedKisharaLockbox))
                    return true;

                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act7.Causeway);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            if (World.Act7.Causeway.IsCurrentArea)
            {
                await Travel.To(World.Act7.VaalCity);
                return true;
            }
            return await Helpers.TakeQuestReward(
                World.Act7.BridgeEncampment,
                TownNpcs.WeylamRoth,
                "Kishara's Star Reward",
                book: QuestItemMetadata.BookKishara);
        }
    }
}