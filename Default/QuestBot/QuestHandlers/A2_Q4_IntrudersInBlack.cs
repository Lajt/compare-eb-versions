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
    public static class A2_Q4_IntrudersInBlack
    {
        private static readonly TgtPosition StrangeDeviceTgt = new TgtPosition("Strange Device location", "templeruinforest_questcart.tgt");

        private static Monster Fidelitas => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Fidelitas_the_Mourning)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static Chest StrangeDevice => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Strange_Device)
            .FirstOrDefault<Chest>();

        private static WalkablePosition CachedFidelitasPos
        {
            get => CombatAreaCache.Current.Storage["FidelitasPosition"] as WalkablePosition;
            set => CombatAreaCache.Current.Storage["FidelitasPosition"] = value;
        }

        private static CachedObject CachedStrangeDevice
        {
            get => CombatAreaCache.Current.Storage["StrangeDevice"] as CachedObject;
            set => CombatAreaCache.Current.Storage["StrangeDevice"] = value;
        }

        public static void Tick()
        {
            if (!World.Act2.ChamberOfSins2.IsCurrentArea)
                return;

            var fidelitas = Fidelitas;
            if (fidelitas != null)
            {
                CachedFidelitasPos = fidelitas.IsDead ? null : fidelitas.WalkablePosition();
            }

            if (CachedStrangeDevice == null)
            {
                var device = StrangeDevice;
                if (device != null)
                {
                    CachedStrangeDevice = new CachedObject(device);
                }
            }
        }

        public static async Task<bool> GrabBalefulGem()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.BalefulGem))
                return false;

            if (World.Act2.ChamberOfSins2.IsCurrentArea)
            {
                var fidelitasPos = CachedFidelitasPos;
                if (fidelitasPos != null)
                {
                    await Helpers.MoveAndWait(fidelitasPos);
                    return true;
                }

                if (await Helpers.OpenQuestChest(CachedStrangeDevice))
                    return true;

                StrangeDeviceTgt.Come();
                return true;
            }
            await Travel.To(World.Act2.ChamberOfSins2);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act2.ForestEncampment,
                TownNpcs.Greust,
                "Blackguard Reward",
                Quests.IntrudersInBlack.Id);
        }
    }
}