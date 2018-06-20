using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Bot;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A1_Q7_MaroonedMariner
    {
        private static readonly TgtPosition AllflameTgt = new TgtPosition("Slave Girl location", "boat_small_damaged_allflame_v01_01_c1r1.tgt");
        private static readonly TgtPosition FairgravesTgt = new TgtPosition("Captain Fairgraves location", "shipwreck_quest_v01_01_c8r7.tgt");

        private static Chest SlaveGirl => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Slave_Girl)
            .FirstOrDefault<Chest>();

        private static Npc CaptainFairgraves => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Captain_Fairgraves)
            .FirstOrDefault<Npc>(n => n.Reaction == Reaction.Npc);

        private static Monster SkeleFairgraves => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Captain_Fairgraves)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique && m.Reaction == Reaction.Enemy);

        private static CachedObject CachedSlaveGirl
        {
            get => CombatAreaCache.Current.Storage["SlaveGirl"] as CachedObject;
            set => CombatAreaCache.Current.Storage["SlaveGirl"] = value;
        }

        private static WalkablePosition CachedFairgravesPos
        {
            get => CombatAreaCache.Current.Storage["FairgravesPosition"] as WalkablePosition;
            set => CombatAreaCache.Current.Storage["FairgravesPosition"] = value;
        }

        public static void Tick()
        {
            if (World.Act1.ShipGraveyardCave.IsCurrentArea)
            {
                if (CachedSlaveGirl == null)
                {
                    var girl = SlaveGirl;
                    if (girl != null)
                    {
                        CachedSlaveGirl = new CachedObject(girl);
                    }
                }
                return;
            }
            if (World.Act1.ShipGraveyard.IsCurrentArea)
            {
                if (CachedFairgravesPos == null)
                {
                    var fairgraves = CaptainFairgraves;
                    if (fairgraves != null)
                    {
                        CachedFairgravesPos = fairgraves.WalkablePosition();
                    }
                }
            }
        }

        public static async Task<bool> GrabAllflame()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.Allflame))
                return false;

            if (World.Act1.ShipGraveyardCave.IsCurrentArea)
            {
                if (await Helpers.OpenQuestChest(CachedSlaveGirl))
                    return true;

                AllflameTgt.Come();
                return true;
            }
            await Travel.To(World.Act1.ShipGraveyardCave);
            return true;
        }

        public static async Task<bool> KillFairgraves()
        {
            if (World.Act1.ShipGraveyard.IsCurrentArea)
            {
                var skeleFairgraves = SkeleFairgraves;
                if (skeleFairgraves != null)
                {
                    if (skeleFairgraves.IsDead)
                        return false;

                    await Helpers.MoveAndWait(skeleFairgraves.WalkablePosition());
                    return true;
                }
                var fairgravesPos = CachedFairgravesPos;
                if (fairgravesPos != null)
                {
                    if (fairgravesPos.IsFar)
                    {
                        fairgravesPos.Come();
                        return true;
                    }
                    var fairgraves = CaptainFairgraves;
                    if (fairgraves == null)
                    {
                        GlobalLog.Debug("[MaroonedMariner] We are near Fairgraves position but neither NPC nor Monster Fairgraves exists.");
                        await Wait.StuckDetectionSleep(500);
                        return true;
                    }
                    if (!fairgraves.IsTargetable)
                    {
                        GlobalLog.Debug("[MaroonedMariner] We are near Fairgraves position but NPC Fairgraves is not targetable and Monster does not exist.");
                        await Wait.StuckDetectionSleep(500);
                        return true;
                    }
                    if (!await PlayerAction.Interact(fairgraves))
                    {
                        ErrorManager.ReportError();
                        return true;
                    }
                    await Coroutines.CloseBlockingWindows();
                    return true;
                }
                FairgravesTgt.Come();
                return true;
            }

            if (World.Act1.ShipGraveyardCave.IsCurrentArea)
            {
                var transition = LokiPoe.ObjectManager.Objects.FirstOrDefault<AreaTransition>(a => a.LeadsTo(World.Act1.ShipGraveyard));
                if (transition != null)
                {
                    if (!await PlayerAction.TakeTransition(transition))
                        ErrorManager.ReportError();
                }
                else
                {
                    if (!await PlayerAction.TpToTown())
                        ErrorManager.ReportError();
                }
                return true;
            }
            await Travel.To(World.Act1.ShipGraveyard);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            if (World.Act1.ShipGraveyard.IsCurrentArea)
            {
                await Travel.To(World.Act1.CavernOfWrath);
                return true;
            }
            return await Helpers.TakeQuestReward(
                World.Act1.LioneyeWatch,
                TownNpcs.Bestel,
                "Fairgraves Reward",
                book: QuestItemMetadata.BookFairgraves);
        }
    }
}