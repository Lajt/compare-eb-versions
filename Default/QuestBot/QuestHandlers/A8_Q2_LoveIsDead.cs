using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A8_Q2_LoveIsDead
    {
        private static readonly TgtPosition TolmanRoomTgt = new TgtPosition("Tolman room", "market_transition_warehouse_v01_01_c1r3.tgt", true);

        private const int FinishedStateMinimum = 3;
        private static bool _finished;

        private static Chest SealedCasket => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Sealed_Casket)
            .FirstOrDefault<Chest>();

        private static Npc Clarissa => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Clarissa)
            .FirstOrDefault<Npc>();

        private static Monster Tolman => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Tolman)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static CachedObject CachedSealedCasket
        {
            get => CombatAreaCache.Current.Storage["SealedCasket"] as CachedObject;
            set => CombatAreaCache.Current.Storage["SealedCasket"] = value;
        }

        public static void Tick()
        {
            _finished = QuestManager.GetStateInaccurate(Quests.LoveIsDead) <= FinishedStateMinimum;

            if (World.Act8.Quay.IsCurrentArea)
            {
                if (CachedSealedCasket == null)
                {
                    var casket = SealedCasket;
                    if (casket != null)
                    {
                        CachedSealedCasket = new CachedObject(casket);
                    }
                }
            }
        }

        public static async Task<bool> GrabAnkh()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.AnkhOfEternity))
                return false;

            if (World.Act8.Quay.IsCurrentArea)
            {
                if (await Helpers.OpenQuestChest(CachedSealedCasket))
                    return true;

                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act8.Quay);
            return true;
        }

        public static async Task<bool> KillTolman()
        {
            if (_finished)
                return false;

            if (World.Act8.Quay.IsCurrentArea)
            {
                var clarissa = Clarissa;
                if (clarissa != null)
                {
                    if (clarissa.IsTargetable && clarissa.HasNpcFloatingIcon)
                    {
                        await Helpers.TalkTo(clarissa);
                        return true;
                    }
                    var tolman = Tolman;
                    if (tolman != null)
                    {
                        await Helpers.MoveToBossOrAnyMob(Tolman);
                        return true;
                    }
                    await Helpers.MoveAndWait(clarissa.WalkablePosition(), "Waiting for any Tolman fight object");
                    return true;
                }
                await Helpers.MoveAndTakeLocalTransition(TolmanRoomTgt);
                return true;
            }
            await Travel.To(World.Act8.Quay);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act8.SarnEncampment,
                TownNpcs.Clarissa_A8,
                "Tolman Reward",
                book: QuestItemMetadata.BookTolman);
        }
    }
}