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
    public static class A3_Q7_FixtureOfFate
    {
        private static Npc Siosa => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Siosa)
            .FirstOrDefault<Npc>();

        private static IEnumerable<Chest> BookStands => LokiPoe.ObjectManager.Objects
            .Where<Chest>(c => c.Metadata.Contains("QuestChests/Siosa/GoldenBookStand"));

        private static List<CachedObject> CachedBookStands
        {
            get
            {
                var stands = CombatAreaCache.Current.Storage["BookStands"] as List<CachedObject>;
                if (stands == null)
                {
                    stands = new List<CachedObject>(4);
                    CombatAreaCache.Current.Storage["BookStands"] = stands;
                }
                return stands;
            }
        }

        private static CachedObject CachedSiosa
        {
            get => CombatAreaCache.Current.Storage["Siosa"] as CachedObject;
            set => CombatAreaCache.Current.Storage["Siosa"] = value;
        }

        public static void Tick()
        {
            if (World.Act3.Library.IsCurrentArea)
            {
                if (CachedSiosa == null)
                {
                    var siosa = Siosa;
                    if (siosa != null)
                    {
                        CachedSiosa = new CachedObject(siosa);
                    }
                }
                return;
            }
            if (World.Act3.Archives.IsCurrentArea)
            {
                foreach (var stand in BookStands)
                {
                    var opened = stand.IsOpened;
                    var id = stand.Id;
                    var cachedStands = CachedBookStands;
                    var index = cachedStands.FindIndex(s => s.Id == stand.Id);

                    if (index >= 0)
                    {
                        if (opened)
                        {
                            GlobalLog.Warn($"[FixtureOfFate] Removing opened {stand.WalkablePosition()}");
                            cachedStands.RemoveAt(index);
                        }
                    }
                    else
                    {
                        if (!opened)
                        {
                            var pos = stand.WalkablePosition();
                            GlobalLog.Warn($"[FixtureOfFate] Registering {pos}");
                            cachedStands.Add(new CachedObject(id, pos));
                        }
                    }
                }
            }
        }

        public static async Task<bool> GrabPages()
        {
            if (Helpers.PlayerHasQuestItemAmount(QuestItemMetadata.GoldenPage, 4))
                return false;

            if (World.Act3.Archives.IsCurrentArea)
            {
                if (await Helpers.OpenQuestChest(CachedBookStands.FirstOrDefault()))
                    return true;

                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act3.Archives);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            if (World.Act3.Library.IsCurrentArea)
            {
                var siosa = CachedSiosa;
                if (siosa != null)
                {
                    var pos = siosa.Position;
                    if (pos.IsFar)
                    {
                        pos.Come();
                        return true;
                    }
                    var reward = Settings.Instance.GetRewardForQuest(Quests.FixtureOfFate.Id);

                    if (!await siosa.Object.AsTownNpc().TakeReward(reward, "Golden Pages Reward"))
                        ErrorManager.ReportError();

                    return false;
                }
                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act3.Library);
            return true;
        }
    }
}