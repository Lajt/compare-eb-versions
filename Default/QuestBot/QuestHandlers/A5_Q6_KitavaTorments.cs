using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public class A5_Q6_KitavaTorments
    {
        private static IEnumerable<Chest> RelicCases => LokiPoe.ObjectManager.Objects
            .Where<Chest>(c => c.Metadata.Contains("QuestChests/Reliquary/RelicCase"));

        private static List<CachedObject> CachedRelicCases
        {
            get
            {
                var cases = CombatAreaCache.Current.Storage["RelicCases"] as List<CachedObject>;
                if (cases == null)
                {
                    cases = new List<CachedObject>(3);
                    CombatAreaCache.Current.Storage["RelicCases"] = cases;
                }
                return cases;
            }
        }

        public static void Tick()
        {
            if (World.Act5.Reliquary.IsCurrentArea)
            {
                foreach (var relicCase in RelicCases)
                {
                    var opened = relicCase.IsOpened;
                    var id = relicCase.Id;
                    var cachedCases = CachedRelicCases;
                    var index = cachedCases.FindIndex(c => c.Id == relicCase.Id);

                    if (index >= 0)
                    {
                        if (opened)
                        {
                            GlobalLog.Warn($"[KitavaTorments] Removing opened {relicCase.WalkablePosition()}");
                            cachedCases.RemoveAt(index);
                        }
                    }
                    else
                    {
                        if (!opened)
                        {
                            var pos = relicCase.WalkablePosition();
                            GlobalLog.Warn($"[KitavaTorments] Registering {pos}");
                            cachedCases.Add(new CachedObject(id, pos));
                        }
                    }
                }
            }
        }

        public static async Task<bool> GrabTorments()
        {
            if (Helpers.PlayerHasQuestItemAmount(QuestItemMetadata.KitavaTorment, 3))
                return false;

            if (World.Act5.Reliquary.IsCurrentArea)
            {
                if (await Helpers.OpenQuestChest(CachedRelicCases.FirstOrDefault()))
                    return true;

                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act5.Reliquary);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act5.OverseerTower,
                TownNpcs.Lani,
                "Torments Reward",
                book: QuestItemMetadata.BookTorments);
        }
    }
}