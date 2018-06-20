using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A3_Q6_SwigOfHope
    {
        private static readonly TgtPosition DecanterTgt = new TgtPosition("Decanter Spiritus location", "market_place_straight_v01_01_unique1_c3r2.tgt");

        private static readonly TgtPosition PlumTgt = new TgtPosition("Plum tree location", "fruittree_c2r2.tgt");

        private static Chest OrnateChest => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Ornate_Chest)
            .FirstOrDefault<Chest>();

        private static Chest PlumTree => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Plum)
            .FirstOrDefault<Chest>();

        private static Npc Fairgraves => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Captain_Fairgraves)
            .FirstOrDefault<Npc>();

        private static CachedObject CachedOrnateChest
        {
            get => CombatAreaCache.Current.Storage["OrnateChest"] as CachedObject;
            set => CombatAreaCache.Current.Storage["OrnateChest"] = value;
        }

        private static CachedObject CachedPlumTree
        {
            get => CombatAreaCache.Current.Storage["PlumTree"] as CachedObject;
            set => CombatAreaCache.Current.Storage["PlumTree"] = value;
        }

        private static CachedObject CachedFairgraves
        {
            get => CombatAreaCache.Current.Storage["CaptainFairgraves"] as CachedObject;
            set => CombatAreaCache.Current.Storage["CaptainFairgraves"] = value;
        }

        public static void Tick()
        {
            var id = World.CurrentArea.Id;
            if (id == World.Act3.Marketplace.Id)
            {
                if (CachedOrnateChest == null)
                {
                    var chest = OrnateChest;
                    if (chest != null)
                    {
                        CachedOrnateChest = new CachedObject(chest);
                    }
                }
                return;
            }
            if (id == World.Act3.ImperialGardens.Id)
            {
                if (CachedPlumTree == null)
                {
                    var tree = PlumTree;
                    if (tree != null)
                    {
                        CachedPlumTree = new CachedObject(tree);
                    }
                }
                return;
            }
            if (id == World.Act3.Docks.Id)
            {
                if (CachedFairgraves == null)
                {
                    var fairgraves = Fairgraves;
                    if (fairgraves != null)
                    {
                        CachedFairgraves = new CachedObject(fairgraves);
                    }
                }
            }
        }

        public static async Task<bool> GrabDecanter()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.DecanterSpiritus))
                return false;

            if (World.Act3.Marketplace.IsCurrentArea)
            {
                if (await Helpers.OpenQuestChest(CachedOrnateChest))
                    return true;

                DecanterTgt.Come();
                return true;
            }
            await Travel.To(World.Act3.Marketplace);
            return true;
        }

        public static async Task<bool> GrabPlum()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.ChitusPlum))
                return false;

            if (World.Act3.ImperialGardens.IsCurrentArea)
            {
                if (await Helpers.OpenQuestChest(CachedPlumTree))
                    return true;

                PlumTgt.Come();
                return true;
            }
            await Travel.To(World.Act3.ImperialGardens);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            if (World.Act3.Docks.IsCurrentArea)
            {
                var fairgraves = CachedFairgraves;
                if (fairgraves != null)
                {
                    var pos = fairgraves.Position;
                    if (pos.IsFar)
                    {
                        pos.Come();
                        return true;
                    }
                    var reward = Settings.Instance.GetRewardForQuest(Quests.SwigOfHope.Id);

                    if (!await fairgraves.Object.AsTownNpc().TakeReward(reward, "Swig of Hope Reward"))
                        ErrorManager.ReportError();

                    return false;
                }
                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act3.Docks);
            return true;
        }
    }
}