using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A9_Q3_FastisFortuna
    {
        private static Monster Boulderback => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Boulderback)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static WalkablePosition CachedBoulderbackPos
        {
            get => CombatAreaCache.Current.Storage["BoulderbackPosition"] as WalkablePosition;
            set => CombatAreaCache.Current.Storage["BoulderbackPosition"] = value;
        }

        public static void Tick()
        {
            if (!World.Act9.Foothills.IsCurrentArea)
                return;

            var boulder = Boulderback;
            if (boulder != null)
            {
                CachedBoulderbackPos = boulder.IsDead ? null : boulder.WalkablePosition();
            }
        }

        public static async Task<bool> GrabCalendar()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.CalendarOfFortune))
                return false;

            if (World.Act9.Foothills.IsCurrentArea)
            {
                var boulderPos = CachedBoulderbackPos;
                if (boulderPos != null)
                {
                    boulderPos.Come();
                    return true;
                }
                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act9.Foothills);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act9.Highgate,
                TownNpcs.PetarusAndVanja_A9,
                "Maraketh Calendar Reward",
                book: QuestItemMetadata.BookFortune);
        }
    }
}