using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A4_Q2_IndomitableSpirit
    {
        private const int FinishedStateMinimum = 2;
        private static bool _finished;

        private static NetworkObject DeshretSpirit => LokiPoe.ObjectManager.Objects
            .Find(o => o.Metadata == "Metadata/QuestObjects/Act4/DeshretSpirit");

        private static CachedObject CachedDeshretSpirit
        {
            get => CombatAreaCache.Current.Storage["DeshretSpirit"] as CachedObject;
            set => CombatAreaCache.Current.Storage["DeshretSpirit"] = value;
        }

        public static void Tick()
        {
            _finished = QuestManager.GetStateInaccurate(Quests.IndomitableSpirit) <= FinishedStateMinimum;

            if (World.Act4.Mines2.IsCurrentArea)
            {
                if (CachedDeshretSpirit == null)
                {
                    var spirit = DeshretSpirit;
                    if (spirit != null)
                    {
                        CachedDeshretSpirit = new CachedObject(spirit);
                    }
                }
            }
        }

        public static async Task<bool> FreeDeshret()
        {
            if (_finished)
                return false;

            if (World.Act4.Mines2.IsCurrentArea)
            {
                if (await Helpers.HandleQuestObject(CachedDeshretSpirit))
                    return true;

                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act4.Mines2);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            if (World.Act4.Mines2.IsCurrentArea)
            {
                await Travel.To(World.Act4.CrystalVeins);
                return true;
            }
            return await Helpers.TakeQuestReward(
                World.Act4.Highgate,
                TownNpcs.Tasuni,
                "Deshret Reward",
                book: QuestItemMetadata.BookDeshretSpirit);
        }
    }
}