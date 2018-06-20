using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A7_Q5_InMemoryOfGreust
    {
        private static readonly TgtPosition AzmeriShrineTgt = new TgtPosition("Azmeri Shrine location", "forest_azmeri_shrine_c1r1.tgt");

        private static NetworkObject AzmeriShrine => LokiPoe.ObjectManager.Objects
            .Find(o => o.Metadata == "Metadata/QuestObjects/Act7/GreustShrine");

        private static CachedObject CachedAzmeriShrine
        {
            get => CombatAreaCache.Current.Storage["AzmeriShrine"] as CachedObject;
            set => CombatAreaCache.Current.Storage["AzmeriShrine"] = value;
        }

        public static void Tick()
        {
            if (World.Act7.NorthernForest.IsCurrentArea)
            {
                if (CachedAzmeriShrine == null)
                {
                    var shrine = AzmeriShrine;
                    if (shrine != null)
                    {
                        CachedAzmeriShrine = new CachedObject(shrine);
                    }
                }
            }
        }

        public static async Task<bool> PlaceNecklace()
        {
            if (!Helpers.PlayerHasQuestItem(QuestItemMetadata.GreustNecklace))
                return false;

            if (World.Act7.NorthernForest.IsCurrentArea)
            {
                if (await Helpers.HandleQuestObject(CachedAzmeriShrine))
                    return true;

                AzmeriShrineTgt.Come();
                return true;
            }
            await Travel.To(World.Act7.NorthernForest);
            return true;
        }

        public static async Task<bool> TakeNecklace()
        {
            return await Helpers.TakeQuestReward(
                World.Act7.BridgeEncampment,
                TownNpcs.Helena_A7,
                "Take Greust's Necklace");
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act7.BridgeEncampment,
                TownNpcs.Helena_A7,
                "Greust's Necklace Reward",
                Quests.InMemoryOfGreust.Id);
        }
    }
}