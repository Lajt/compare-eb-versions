using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A5_Q3_KeyToFreedom
    {
        private static Monster Casticus => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Justicar_Casticus)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static WalkablePosition CachedCasticusPos
        {
            get => CombatAreaCache.Current.Storage["CasticusPosition"] as WalkablePosition;
            set => CombatAreaCache.Current.Storage["CasticusPosition"] = value;
        }

        public static void Tick()
        {
            if (!World.Act5.ControlBlocks.IsCurrentArea)
                return;

            var casticus = Casticus;
            if (casticus != null)
            {
                CachedCasticusPos = casticus.WalkablePosition();
            }
        }

        public static async Task<bool> KillCasticus()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.EyesOfZeal))
                return false;

            if (World.Act5.ControlBlocks.IsCurrentArea)
            {
                var casticusPos = CachedCasticusPos;
                if (casticusPos != null)
                {
                    await Helpers.MoveAndWait(casticusPos);
                    return true;
                }
            }
            await Travel.To(World.Act5.OriathSquare);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            if (World.Act5.ControlBlocks.IsCurrentArea)
            {
                await Travel.To(World.Act5.OriathSquare);
                return true;
            }
            return await Helpers.TakeQuestReward(
                World.Act5.OverseerTower,
                TownNpcs.Lani,
                "Casticus Reward",
                Quests.KeyToFreedom.Id);
        }
    }
}