using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A4_Q1_BreakingSeal
    {
        private static readonly WalkablePosition DeshretSealPos = new WalkablePosition("Deshret Seal location", 330, 620);

        private static Monster Voll => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Voll_Emperor_of_Purity)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static NetworkObject DeshretSeal => LokiPoe.ObjectManager.Objects
            .Find(o => o.Metadata == "Metadata/QuestObjects/Act4/MineEntranceSeal");

        private static WalkablePosition CachedVollPos
        {
            get => CombatAreaCache.Current.Storage["VollPosition"] as WalkablePosition;
            set => CombatAreaCache.Current.Storage["VollPosition"] = value;
        }

        public static void Tick()
        {
            if (!World.Act4.DriedLake.IsCurrentArea)
                return;

            var voll = Voll;
            if (voll != null)
            {
                CachedVollPos = voll.WalkablePosition();
            }
        }

        public static async Task<bool> KillVoll()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.DeshretBanner))
                return false;

            if (World.Act4.DriedLake.IsCurrentArea)
            {
                var vollPos = CachedVollPos;
                if (vollPos != null)
                {
                    await Helpers.MoveAndWait(vollPos);
                    return true;
                }
                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act4.DriedLake);
            return true;
        }

        public static async Task<bool> BreakSeal()
        {
            if (!Helpers.PlayerHasQuestItem(QuestItemMetadata.DeshretBanner))
                return false;

            if (World.Act4.Highgate.IsCurrentArea)
            {
                await DeshretSealPos.ComeAtOnce();

                var seal = DeshretSeal;
                if (seal == null)
                {
                    GlobalLog.Debug("[BreakingSeal] We are near Deshret Seal position but Seal object is null.");
                    await Wait.StuckDetectionSleep(500);
                    return true;
                }
                if (!seal.IsTargetable)
                {
                    GlobalLog.Debug("[BreakingSeal] Deshret Seal is not targetable.");
                    await Wait.StuckDetectionSleep(500);
                    return true;
                }

                if (!await PlayerAction.Interact(seal, () => !seal.Fresh().IsTargetable, "Deshret Seal interaction"))
                    ErrorManager.ReportError();

                return true;
            }
            await Travel.To(World.Act4.Highgate);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act4.Highgate,
                TownNpcs.Oyun,
                "Red Banner Reward",
                Quests.BreakingSeal.Id);
        }
    }
}