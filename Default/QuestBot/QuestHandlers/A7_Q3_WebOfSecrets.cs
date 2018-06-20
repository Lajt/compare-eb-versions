using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A7_Q3_WebOfSecrets
    {
        private static Npc Silk => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Silk)
            .FirstOrDefault<Npc>();

        private static CachedObject CachedSilk
        {
            get => CombatAreaCache.Current.Storage["Silk"] as CachedObject;
            set => CombatAreaCache.Current.Storage["Silk"] = value;
        }

        public static void Tick()
        {
            if (World.Act7.ChamberOfSins1.IsCurrentArea)
            {
                if (CachedSilk == null)
                {
                    var silk = Silk;
                    if (silk != null)
                    {
                        CachedSilk = new CachedObject(silk);
                    }
                }
            }
        }

        public static async Task<bool> TakeObsidianKey()
        {
            if (World.Act7.ChamberOfSins1.IsCurrentArea)
            {
                if (CachedSilk != null)
                {
                    var pos = CachedSilk.Position;
                    if (pos.IsFar)
                    {
                        pos.Come();
                        return true;
                    }

                    if (!await CachedSilk.Object.AsTownNpc().TakeReward(null, "Black Death Reward"))
                        ErrorManager.ReportError();

                    return false;
                }
                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act7.ChamberOfSins1);
            return true;
        }
    }
}