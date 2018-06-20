using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A7_Q1_SilverLocket
    {
        private static Chest DirtyLockbox => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Dirty_Lockbox)
            .FirstOrDefault<Chest>();

        private static CachedObject CachedDirtyLockbox
        {
            get => CombatAreaCache.Current.Storage["DirtyLockbox"] as CachedObject;
            set => CombatAreaCache.Current.Storage["DirtyLockbox"] = value;
        }

        public static void Tick()
        {
            if (!World.Act7.BrokenBridge.IsCurrentArea)
                return;

            if (CachedDirtyLockbox == null)
            {
                var lockbox = DirtyLockbox;
                if (lockbox != null)
                {
                    CachedDirtyLockbox = new CachedObject(lockbox);
                }
            }
        }

        public static async Task<bool> GrabSilverLocket()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.SilverLocket))
                return false;

            if (World.Act7.BrokenBridge.IsCurrentArea)
            {
                if (await Helpers.OpenQuestChest(CachedDirtyLockbox))
                    return true;

                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act7.BrokenBridge);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act7.BridgeEncampment,
                TownNpcs.WeylamRoth,
                "Silver Locket Reward",
                Quests.SilverLocket.Id);
        }
    }
}