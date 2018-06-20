using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A5_Q5_KingFeast
    {
        private const int FinishedStateMinimum = 3;
        private static bool _finished;

        private static readonly WalkablePosition UtulaPosition = new WalkablePosition("Utula location", 1369, 1001);

        private static Monster Utula => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Utula_Stone_and_Steel)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static WalkablePosition CachedUtulaPos
        {
            get => CombatAreaCache.Current.Storage["UtulaPosition"] as WalkablePosition;
            set => CombatAreaCache.Current.Storage["UtulaPosition"] = value;
        }

        public static void Tick()
        {
            _finished = QuestManager.GetStateInaccurate(Quests.KingFeast) <= FinishedStateMinimum;

            if (World.Act5.RuinedSquare.IsCurrentArea)
            {
                var utula = Utula;
                if (utula != null)
                {
                    CachedUtulaPos = utula.WalkablePosition();
                }
            }
        }

        public static async Task<bool> KillUtula()
        {
            if (_finished)
                return false;

            if (World.Act5.RuinedSquare.IsCurrentArea)
            {
                var utulaPos = CachedUtulaPos;
                if (utulaPos != null)
                {
                    await Helpers.MoveAndWait(utulaPos);
                    return true;
                }
                await Helpers.MoveAndWait(UtulaPosition);
                return true;
            }
            await Travel.To(World.Act5.RuinedSquare);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act5.OverseerTower,
                TownNpcs.Bannon,
                "Utula Reward",
                Quests.KingFeast.Id);
        }
    }
}