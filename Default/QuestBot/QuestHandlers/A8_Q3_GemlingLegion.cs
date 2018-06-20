using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A8_Q3_GemlingLegion
    {
        private const int FinishedStateMinimum = 2;
        private static bool _finished;

        private static IEnumerable<Monster> Gemlings => LokiPoe.ObjectManager.Objects
            .Where<Monster>(m => m.Rarity == Rarity.Unique && m.Metadata.Contains("GemlingLegionnaire"));

        private static List<CachedObject> CachedGemlings
        {
            get
            {
                var gemlings = CombatAreaCache.Current.Storage["Gemlings"] as List<CachedObject>;
                if (gemlings == null)
                {
                    gemlings = new List<CachedObject>();
                    CombatAreaCache.Current.Storage["Gemlings"] = gemlings;
                }
                return gemlings;
            }
        }

        public static void Tick()
        {
            _finished = QuestManager.GetStateInaccurate(Quests.GemlingLegion) <= FinishedStateMinimum;

            if (World.Act8.GrainGate.IsCurrentArea)
            {
                foreach (var gemling in Gemlings)
                {
                    var isDead = gemling.IsDead;
                    var id = gemling.Id;
                    var cachedGemlings = CachedGemlings;
                    var index = cachedGemlings.FindIndex(c => c.Id == gemling.Id);

                    if (index >= 0)
                    {
                        if (isDead)
                        {
                            GlobalLog.Warn($"[GemlingLegion] Removing dead {gemling.WalkablePosition()}");
                            cachedGemlings.RemoveAt(index);
                        }
                        else
                        {
                            cachedGemlings[index].Position = gemling.WalkablePosition();
                        }
                    }
                    else
                    {
                        if (!isDead)
                        {
                            var pos = gemling.WalkablePosition();
                            GlobalLog.Warn($"[GemlingLegion] Registering {pos}");
                            cachedGemlings.Add(new CachedObject(id, pos));
                        }
                    }
                }
            }
        }

        public static async Task<bool> KillGemlings()
        {
            if (_finished)
                return false;

            if (World.Act8.GrainGate.IsCurrentArea)
            {
                var gemling = CachedGemlings.FirstOrDefault();
                if (gemling != null)
                {
                    var pos = gemling.Position;
                    if (pos.Distance > 30)
                    {
                        pos.Come();
                    }
                    else
                    {
                        var gemlingObj = gemling.Object;
                        if (gemlingObj == null)
                        {
                            GlobalLog.Warn($"[GemlingLegion] Gemling with id {gemling.Id} no longer exist.");
                            CachedGemlings.RemoveAt(0);
                        }
                    }
                    return true;
                }
                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act8.GrainGate);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act8.SarnEncampment,
                TownNpcs.Maramoa_A8,
                "Gemling Legion Reward",
                book: QuestItemMetadata.BookGemlingLegion);
        }
    }
}