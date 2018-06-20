using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Loki.Bot;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A9_Q1_StormBlade
    {
        private static NetworkObject StormChest => LokiPoe.ObjectManager.Objects
            .Find(o => o.Metadata == "Metadata/QuestObjects/Act9/MummyEventChest");

        private static CachedObject CachedStormChest
        {
            get => CombatAreaCache.Current.Storage["StormChest"] as CachedObject;
            set => CombatAreaCache.Current.Storage["StormChest"] = value;
        }

        public static void Tick()
        {
            if (World.Act9.VastiriDesert.IsCurrentArea)
            {
                if (CachedStormChest == null)
                {
                    var chest = StormChest;
                    if (chest != null)
                    {
                        CachedStormChest = new CachedObject(chest);
                    }
                }
            }
        }

        public static async Task<bool> GrabStormBlade()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.StormBlade))
                return false;

            if (World.Act9.VastiriDesert.IsCurrentArea)
            {
                var chest = CachedStormChest;
                if (chest != null)
                {
                    var pos = chest.Position;
                    if (pos.IsFar || pos.IsFarByPath)
                    {
                        pos.Come();
                        return true;
                    }
                    var chestObj = chest.Object;
                    if (chestObj.IsTargetable)
                    {
                        if (!await PlayerAction.Interact(chestObj, () => !chestObj.Fresh().IsTargetable, "Storm Chest interaction"))
                            ErrorManager.ReportError();

                        return true;
                    }
                    var mob = Helpers.ClosestActiveMob;
                    if (mob != null && mob.PathExists())
                    {
                        PlayerMoverManager.MoveTowards(mob.Position);
                        return true;
                    }
                    GlobalLog.Debug("Waiting for any active monster");
                    await Wait.StuckDetectionSleep(500);
                    return true;
                }
                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act9.VastiriDesert);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act9.Highgate,
                TownNpcs.PetarusAndVanja_A9,
                "Storm Blade Reward",
                Quests.StormBlade.Id);
        }
    }
}