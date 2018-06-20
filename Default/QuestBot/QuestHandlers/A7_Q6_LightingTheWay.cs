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
    public static class A7_Q6_LightingTheWay
    {
        private const int MinHaveAllFirefliesState = 3;
        private static bool _haveAllFireflies;

        private static IEnumerable<Chest> FireflyChest => LokiPoe.ObjectManager.Objects
            .Where<Chest>(c => c.Metadata.Contains("QuestChests/Fireflies/FireflyChest"));

        private static List<CachedObject> CachedFireflyChests
        {
            get
            {
                var chests = CombatAreaCache.Current.Storage["FireflyChests"] as List<CachedObject>;
                if (chests == null)
                {
                    chests = new List<CachedObject>(7);
                    CombatAreaCache.Current.Storage["FireflyChests"] = chests;
                }
                return chests;
            }
        }

        public static void Tick()
        {
            _haveAllFireflies = Helpers.PlayerHasQuestItemAmount(QuestItemMetadata.Firefly, 7) ||
                                QuestManager.GetStateInaccurate(Quests.LightingTheWay) <= MinHaveAllFirefliesState;

            if (World.Act7.DreadThicket.IsCurrentArea)
            {
                foreach (var chest in FireflyChest)
                {
                    var opened = chest.IsOpened;
                    var targetable = chest.IsTargetable;
                    var id = chest.Id;
                    var cachedChests = CachedFireflyChests;
                    var index = cachedChests.FindIndex(c => c.Id == chest.Id);

                    if (index >= 0)
                    {
                        if (opened)
                        {
                            GlobalLog.Warn($"[LightingTheWay] Removing opened {chest.WalkablePosition()}");
                            cachedChests.RemoveAt(index);
                        }
                    }
                    else
                    {
                        if (!opened && targetable)
                        {
                            var pos = chest.WalkablePosition();
                            GlobalLog.Warn($"[LightingTheWay] Registering {pos}");
                            cachedChests.Add(new CachedObject(id, pos));
                        }
                    }
                }
            }
        }

        public static async Task<bool> GrabFireflies()
        {
            if (_haveAllFireflies)
                return false;

            if (World.Act7.DreadThicket.IsCurrentArea)
            {
                if (await Helpers.OpenQuestChest(CachedFireflyChests.FirstOrDefault()))
                    return true;

                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act7.DreadThicket);
            return true;
        }
    }
}