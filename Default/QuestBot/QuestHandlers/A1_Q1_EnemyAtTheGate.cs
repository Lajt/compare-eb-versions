using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Bot;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A1_Q1_EnemyAtTheGate
    {
        private static Monster Hillock => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Hillock)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static WalkablePosition CachedHillockPos
        {
            get => CombatAreaCache.Current.Storage["HillockPosition"] as WalkablePosition;
            set => CombatAreaCache.Current.Storage["HillockPosition"] = value;
        }

        public static void Tick()
        {
            if (!World.Act1.TwilightStrand.IsCurrentArea)
                return;

            var hillok = Hillock;
            if (hillok != null)
            {
                CachedHillockPos = hillok.IsDead ? null : hillok.WalkablePosition();
            }
        }

        public static async Task<bool> EnterLioneyeWatch()
        {
            if (!World.Act1.TwilightStrand.IsCurrentArea)
                return false;

            if (LokiPoe.InGameState.IsSkipAllTutorialsButtonShowing)
            {
                GlobalLog.Error("[QuestBot] \"Skip all tutorials\" button is active. Please do the tutorial (or press the button).");
                if (!BotManager.IsStopping) BotManager.Stop();
                return true;
            }

            var hillockPos = CachedHillockPos;
            if (hillockPos != null)
            {
                await Helpers.MoveAndWait(hillockPos);
            }
            else
            {
                await Travel.To(World.Act1.LioneyeWatch);
            }
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act1.LioneyeWatch,
                TownNpcs.Tarkleigh,
                "Hillock Reward",
                Quests.EnemyAtTheGate.Id);
        }
    }
}