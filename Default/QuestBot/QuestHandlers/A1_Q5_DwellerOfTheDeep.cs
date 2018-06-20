using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A1_Q5_DwellerOfTheDeep
    {
        private const int FinishedStateMinimum = 3;
        private static bool _finished;

        private static Monster Dweller => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.The_Deep_Dweller)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique && !m.IsDead);

        private static WalkablePosition CachedDwellerPos
        {
            get => CombatAreaCache.Current.Storage["DwellerPosition"] as WalkablePosition;
            set => CombatAreaCache.Current.Storage["DwellerPosition"] = value;
        }

        public static void Tick()
        {
            _finished = QuestManager.GetStateInaccurate(Quests.DwellerOfTheDeep) <= FinishedStateMinimum;

            if (World.Act1.FloodedDepths.IsCurrentArea)
            {
                var dweller = Dweller;
                if (dweller != null)
                {
                    CachedDwellerPos = dweller.WalkablePosition();
                }
            }
        }

        public static async Task<bool> KillDweller()
        {
            if (_finished)
                return false;

            if (World.Act1.FloodedDepths.IsCurrentArea)
            {
                var dwellerPos = CachedDwellerPos;
                if (dwellerPos != null)
                {
                    await Helpers.MoveAndWait(dwellerPos);
                    return true;
                }
                if (!await CombatAreaCache.Current.Explorer.Execute())
                {
                    if (QuestManager.GetState(Quests.DwellerOfTheDeep) <= FinishedStateMinimum)
                        return false;

                    GlobalLog.Error("[KillDweller] Flooded Depths area is fully explored but Deep Dweller was not killed. Now going to create a new Flooded Depths instance.");

                    Travel.RequestNewInstance(World.Act1.FloodedDepths);

                    if (!await PlayerAction.TpToTown())
                        ErrorManager.ReportError();
                }
                return true;
            }
            await Travel.To(World.Act1.FloodedDepths);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act1.LioneyeWatch,
                TownNpcs.Tarkleigh,
                "Dweller Reward",
                book: QuestItemMetadata.BookDweller);
        }
    }
}