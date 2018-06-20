using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A10_Q4_MapToTsoatha
    {
        private static Chest PearlCase => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Pearl_Case)
            .FirstOrDefault<Chest>();

        private static CachedObject CachedPearlCase
        {
            get => CombatAreaCache.Current.Storage["PearlCase"] as CachedObject;
            set => CombatAreaCache.Current.Storage["PearlCase"] = value;
        }

        public static void Tick()
        {
            if (!World.Act10.Reliquary.IsCurrentArea)
                return;

            if (CachedPearlCase == null)
            {
                var pearlCase = PearlCase;
                if (pearlCase != null)
                {
                    CachedPearlCase = new CachedObject(pearlCase);
                }
            }
        }

        public static async Task<bool> GrabTeardrop()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.Teardrop))
                return false;

            if (World.Act10.Reliquary.IsCurrentArea)
            {
                if (await Helpers.OpenQuestChest(CachedPearlCase))
                    return true;

                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act10.Reliquary);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act10.OriathDocks,
                TownNpcs.LillyRoth_A10,
                "Tsoatha Reward",
                Quests.MapToTsoatha.Id);
        }
    }
}