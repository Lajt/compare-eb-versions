using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A3_Q3_GemlingQueen
    {
        private static readonly TgtPosition RibbonTgt = new TgtPosition("Ribbon Spool location", "act3_brokenbridge_v01_01_c16r5.tgt");
        private static readonly TgtPosition SulphiteTgt = new TgtPosition("Thaumetic Sulphite location", "templeruinforest_questcart.tgt");
        private static readonly TgtPosition DiallaTgt = new TgtPosition("Lady Dialla location", "gemling_queen_throne_v01_01_c3r3.tgt");

        private static Chest BlackguardChest => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Blackguard_Chest)
            .FirstOrDefault<Chest>();

        private static Chest SupplyContainer => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Supply_Container)
            .FirstOrDefault<Chest>();

        private static CachedObject CachedBlackguardChest
        {
            get => CombatAreaCache.Current.Storage["BlackguardChest"] as CachedObject;
            set => CombatAreaCache.Current.Storage["BlackguardChest"] = value;
        }

        private static CachedObject CachedSupplyContainer
        {
            get => CombatAreaCache.Current.Storage["SupplyContainer"] as CachedObject;
            set => CombatAreaCache.Current.Storage["SupplyContainer"] = value;
        }

        public static void Tick()
        {
            if (World.Act3.Battlefront.IsCurrentArea)
            {
                if (CachedBlackguardChest == null)
                {
                    var chest = BlackguardChest;
                    if (chest != null)
                    {
                        CachedBlackguardChest = new CachedObject(chest);
                    }
                }
                return;
            }
            if (World.Act3.Docks.IsCurrentArea)
            {
                if (CachedSupplyContainer == null)
                {
                    var container = SupplyContainer;
                    if (container != null)
                    {
                        CachedSupplyContainer = new CachedObject(container);
                    }
                }
            }
        }

        public static async Task<bool> GrabRibbon()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.RibbonSpool))
                return false;

            if (World.Act3.Battlefront.IsCurrentArea)
            {
                if (await Helpers.OpenQuestChest(CachedBlackguardChest))
                    return true;

                RibbonTgt.Come();
                return true;
            }
            await Travel.To(World.Act3.Battlefront);
            return true;
        }

        public static async Task<bool> GrabSulphite()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.ThaumeticSulphite))
                return false;

            if (World.Act3.Docks.IsCurrentArea)
            {
                if (await Helpers.OpenQuestChest(CachedSupplyContainer))
                    return true;

                SulphiteTgt.Come();
                return true;
            }
            await Travel.To(World.Act3.Docks);
            return true;
        }

        public static async Task<bool> TakeRibbonReward()
        {
            return await TakeDiallaReward(false);
        }

        public static async Task<bool> TakeTalcReward()
        {
            return await TakeDiallaReward(true);
        }

        private static async Task<bool> TakeDiallaReward(bool talc)
        {
            if (World.Act3.SolarisTemple2.IsCurrentArea)
            {
                var dialla = Helpers.LadyDialla;
                if (dialla != null)
                {
                    var pos = dialla.WalkablePosition();
                    if (pos.IsFar)
                    {
                        pos.Come();
                        return true;
                    }
                    if (talc)
                    {
                        if (!await dialla.AsTownNpc().TakeReward(null, "Take Infernal Talc"))
                            ErrorManager.ReportError();
                    }
                    else
                    {
                        var reward = Settings.Instance.GetRewardForQuest(Quests.RibbonSpool.Id);

                        if (!await dialla.AsTownNpc().TakeReward(reward, "Ribbon Spool Reward"))
                            ErrorManager.ReportError();
                    }
                    return false;
                }
                if (DiallaTgt.IsFar)
                {
                    DiallaTgt.Come();
                    return true;
                }
                GlobalLog.Debug("[GemlingQueen] We are near Dialla tgt but NPC object is null.");
                await Wait.StuckDetectionSleep(500);
                return true;
            }
            await Travel.To(World.Act3.SolarisTemple2);
            return true;
        }
    }
}