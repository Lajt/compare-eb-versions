using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A3_Q4_SeverRightHand
    {
        private static readonly TgtPosition GraviciusTgt = new TgtPosition("General Gravicius location", "temple_carpet_oneside_01_01.tgt");

        private const int FinishedStateMinimum = 1;
        private static bool _finished;

        private static Monster Gravicius => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.General_Gravicius)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static WalkablePosition CachedGraviciusPos
        {
            get => CombatAreaCache.Current.Storage["GraviciusPosition"] as WalkablePosition;
            set => CombatAreaCache.Current.Storage["GraviciusPosition"] = value;
        }

        public static void Tick()
        {
            _finished = QuestManager.GetStateInaccurate(Quests.SeverRightHand) <= FinishedStateMinimum;

            if (World.Act3.EbonyBarracks.IsCurrentArea)
            {
                var gravicius = Gravicius;
                if (gravicius != null)
                {
                    CachedGraviciusPos = gravicius.WalkablePosition();
                }
            }
        }

        public static async Task<bool> KillGravicius()
        {
            if (_finished)
                return false;

            if (World.Act3.EbonyBarracks.IsCurrentArea)
            {
                var graviciusPos = CachedGraviciusPos;
                if (graviciusPos != null)
                {
                    await Helpers.MoveAndWait(graviciusPos);
                    return true;
                }
                GraviciusTgt.Come();
                return true;
            }
            await Travel.To(World.Act3.EbonyBarracks);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            if (World.Act3.EbonyBarracks.IsCurrentArea)
            {
                await Travel.To(World.Act3.LunarisTemple1);
                return true;
            }
            return await Helpers.TakeQuestReward(
                World.Act3.SarnEncampment,
                TownNpcs.Maramoa,
                "Gravicius Reward",
                Quests.SeverRightHand.Id);
        }
    }
}