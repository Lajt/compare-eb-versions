using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A10_Q2_NoLoveForOldGhosts
    {
        private static Chest TemplarStash => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Sealed_Chest)
            .FirstOrDefault<Chest>();

        private static CachedObject CachedTemplarStash
        {
            get => CombatAreaCache.Current.Storage["TemplarStash"] as CachedObject;
            set => CombatAreaCache.Current.Storage["TemplarStash"] = value;
        }

        public static void Tick()
        {
            if (!World.Act10.Ossuary.IsCurrentArea)
                return;

            if (CachedTemplarStash == null)
            {
                var stash = TemplarStash;
                if (stash != null)
                {
                    CachedTemplarStash = new CachedObject(stash);
                }
            }
        }

        public static async Task<bool> GrabElixir()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.ElixirOfAllure))
                return false;

            if (World.Act10.Ossuary.IsCurrentArea)
            {
                if (await Helpers.OpenQuestChest(CachedTemplarStash))
                    return true;

                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act10.Ossuary);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act10.OriathDocks,
                TownNpcs.WeylamRoth_A10,
                "Elixir of Allure Reward",
                book: QuestItemMetadata.BookOldGhost);
        }
    }
}