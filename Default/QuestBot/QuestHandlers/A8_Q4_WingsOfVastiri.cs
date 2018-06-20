using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A8_Q4_WingsOfVastiri
    {
        private static Monster Hector => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Hector_Titucius_Eternal_Servant)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static Chest HectorTomb => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Tomb_of_Hector_Titucius)
            .FirstOrDefault<Chest>();

        private static WalkablePosition CachedHectorPos
        {
            get => CombatAreaCache.Current.Storage["HectorPosition"] as WalkablePosition;
            set => CombatAreaCache.Current.Storage["HectorPosition"] = value;
        }

        private static CachedObject CachedHectorTomb
        {
            get => CombatAreaCache.Current.Storage["HectorTomb"] as CachedObject;
            set => CombatAreaCache.Current.Storage["HectorTomb"] = value;
        }

        public static void Tick()
        {
            if (!World.Act8.BathHouse.IsCurrentArea)
                return;

            if (CachedHectorTomb == null)
            {
                var tomb = HectorTomb;
                if (tomb != null)
                {
                    CachedHectorTomb = new CachedObject(tomb);
                }
            }
            var hector = Hector;
            if (hector != null)
            {
                CachedHectorPos = hector.IsDead ? null : hector.WalkablePosition();
            }
        }

        public static async Task<bool> GrabWings()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.WingsOfVastiri))
                return false;

            if (World.Act8.BathHouse.IsCurrentArea)
            {
                var hectorPos = CachedHectorPos;
                if (hectorPos != null)
                {
                    hectorPos.Come();
                    return true;
                }

                if (await Helpers.OpenQuestChest(CachedHectorTomb))
                    return true;

                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act8.BathHouse);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act8.SarnEncampment,
                TownNpcs.Hargan_A8,
                "Wings of Vastiri Reward",
                Quests.WingsOfVastiri.Id);
        }
    }
}