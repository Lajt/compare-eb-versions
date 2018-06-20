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
    public static class A1_Q4_BreakingSomeEggs
    {
        private static IEnumerable<Chest> RhoaNests => LokiPoe.ObjectManager.Objects
            .Where<Chest>(c => c.Metadata.Contains("QuestChests/RhoaChest"));

        private static NetworkObject GlyphWall => LokiPoe.ObjectManager.Objects
            .Find(o => o.Metadata == "Metadata/QuestObjects/WaterCave/GlyphWall");

        private static List<CachedObject> CachedRhoaNests
        {
            get
            {
                var cases = CombatAreaCache.Current.Storage["RhoaNests"] as List<CachedObject>;
                if (cases == null)
                {
                    cases = new List<CachedObject>(3);
                    CombatAreaCache.Current.Storage["RhoaNests"] = cases;
                }
                return cases;
            }
        }

        private static CachedObject CachedGlyphWall
        {
            get => CombatAreaCache.Current.Storage["GlyphWall"] as CachedObject;
            set => CombatAreaCache.Current.Storage["GlyphWall"] = value;
        }

        public static void Tick()
        {
            if (!World.Act1.MudFlats.IsCurrentArea)
                return;

            if (CachedGlyphWall == null)
            {
                var wall = GlyphWall;
                if (wall != null)
                {
                    CachedGlyphWall = new CachedObject(wall);
                }
            }

            foreach (var nest in RhoaNests)
            {
                var opened = nest.IsOpened;
                var id = nest.Id;
                var cachedNests = CachedRhoaNests;
                var index = cachedNests.FindIndex(c => c.Id == nest.Id);

                if (index >= 0)
                {
                    if (opened)
                    {
                        GlobalLog.Warn($"[BreakingSomeEggs] Removing opened {nest.WalkablePosition()}");
                        cachedNests.RemoveAt(index);
                    }
                }
                else
                {
                    if (!opened)
                    {
                        var pos = nest.WalkablePosition();
                        GlobalLog.Warn($"[BreakingSomeEggs] Registering {pos}");
                        cachedNests.Add(new CachedObject(id, pos));
                    }
                }
            }
        }

        public static async Task<bool> GrabGlyphs()
        {
            if (Helpers.PlayerHasQuestItemAmount(QuestItemMetadata.Glyph, 3))
                return false;

            if (World.Act1.MudFlats.IsCurrentArea)
            {
                if (await Helpers.OpenQuestChest(CachedRhoaNests.FirstOrDefault()))
                    return true;

                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act1.MudFlats);
            return true;
        }

        public static async Task<bool> OpenPassage()
        {
            if (!Helpers.PlayerHasQuestItemAmount(QuestItemMetadata.Glyph, 3))
                return false;

            if (World.Act1.MudFlats.IsCurrentArea)
            {
                if (await Helpers.HandleQuestObject(CachedGlyphWall))
                    return true;

                await Travel.To(World.Act1.SubmergedPassage);
                return true;
            }
            await Travel.To(World.Act1.MudFlats);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            if (World.Act1.MudFlats.IsCurrentArea)
            {
                await Travel.To(World.Act1.SubmergedPassage);
                return true;
            }
            return await Helpers.TakeQuestReward(
                World.Act1.LioneyeWatch,
                TownNpcs.Tarkleigh,
                "Glyph Reward",
                Quests.BreakingSomeEggs.Id);
        }
    }
}