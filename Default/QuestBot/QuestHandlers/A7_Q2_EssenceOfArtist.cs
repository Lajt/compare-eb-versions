using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;
using MapDeviceUi = Loki.Game.LokiPoe.InGameState.MapDeviceUi;

namespace Default.QuestBot.QuestHandlers
{
    public static class A7_Q2_EssenceOfArtist
    {
        private static readonly TgtPosition MapDeviceTgt = new TgtPosition("Map Device location", "temple_maporrery4_c1r2.tgt");

        private static Chest ContainerOfSins => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Container_of_Sins)
            .FirstOrDefault<Chest>();

        private static Monster Maligaro => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Maligaro_the_Artist)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static Portal MaligaroPortal => LokiPoe.ObjectManager.Objects
            .Closest<Portal>(p => p.IsTargetable && p.LeadsTo(a => a.Name == World.Act7.MaligaroSanctum.Name));

        private static NetworkObject MapDevice => LokiPoe.ObjectManager.Objects
            .Find(o => o.Metadata == "Metadata/QuestObjects/Act7/MaligaroOrrery");

        private static NetworkObject MaligaroRoomObj => LokiPoe.ObjectManager.Objects
            .Find(o => o.Metadata == "Metadata/MiscellaneousObjects/ArenaMiddle");

        private static CachedObject CachedContainerOfSins
        {
            get => CombatAreaCache.Current.Storage["ContainerOfSins"] as CachedObject;
            set => CombatAreaCache.Current.Storage["ContainerOfSins"] = value;
        }

        private static CachedObject CachedMapDevice
        {
            get => CombatAreaCache.Current.Storage["MapDevice"] as CachedObject;
            set => CombatAreaCache.Current.Storage["MapDevice"] = value;
        }

        public static void Tick()
        {
            if (World.Act7.Crypt.IsCurrentArea)
            {
                if (CachedContainerOfSins == null)
                {
                    var container = ContainerOfSins;
                    if (container != null)
                    {
                        CachedContainerOfSins = new CachedObject(container);
                    }
                }
                return;
            }
            if (World.Act7.ChamberOfSins1.IsCurrentArea)
            {
                if (CachedMapDevice == null)
                {
                    var device = MapDevice;
                    if (device != null)
                    {
                        CachedMapDevice = new CachedObject(device);
                    }
                }
            }
        }

        public static async Task<bool> GrabMaligaroMap()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.MaligaroMap))
                return false;

            if (World.Act7.Crypt.IsCurrentArea)
            {
                if (await Helpers.OpenQuestChest(CachedContainerOfSins))
                    return true;

                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act7.Crypt);
            return true;
        }

        public static async Task<bool> KillMaligaro()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.BlackVenom))
                return false;

            if (World.Act7.ChamberOfSins1.IsCurrentArea)
            {
                var portal = MaligaroPortal;
                if (portal != null)
                {
                    if (!await PlayerAction.TakePortal(portal))
                        ErrorManager.ReportError();

                    return true;
                }
                var device = CachedMapDevice;
                if (device != null)
                {
                    var pos = device.Position;
                    if (pos.IsFar)
                    {
                        pos.Come();
                        return true;
                    }

                    if (!await HandleMapDevice(device.Object))
                        ErrorManager.ReportError();

                    return true;
                }
                MapDeviceTgt.Come();
                return true;
            }
            if (World.Act7.MaligaroSanctum.IsCurrentArea)
            {
                var roomObj = MaligaroRoomObj;
                if (roomObj != null)
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.Maligaro))
                        return true;

                    if (roomObj.PathExists())
                    {
                        var maligaro = Maligaro;
                        if (maligaro != null)
                        {
                            await Helpers.MoveToBossOrAnyMob(maligaro);
                            return true;
                        }
                        await Helpers.MoveAndWait(roomObj, "Waiting for any Maligaro fight object");
                        return true;
                    }
                }
                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act7.ChamberOfSins1);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act7.BridgeEncampment,
                TownNpcs.Helena_A7,
                "Maligaro Reward",
                Quests.EssenceOfArtist.Id);
        }

        public static async Task<bool> HandleMapDevice(NetworkObject device)
        {
            if (!MapDeviceUi.IsOpened)
            {
                if (!await PlayerAction.Interact(device, () => MapDeviceUi.IsOpened, "Map Device opening"))
                    return false;
            }

            if (!MapDeviceUi.InventoryControl.Inventory.Items.Exists(IsMaligaroMap))
            {
                var map = Inventories.InventoryItems.Find(IsMaligaroMap);
                if (map == null)
                {
                    GlobalLog.Error("[EssenceOfArtist] Unexpected error. We must have Maligaro's Map at this point.");
                    ErrorManager.ReportCriticalError();
                    return false;
                }

                var count = MapDeviceUi.InventoryControl.Inventory.Items.Count;

                if (!await Inventories.FastMoveFromInventory(map.LocationTopLeft))
                    return false;

                if (!await Wait.For(() => MapDeviceUi.InventoryControl.Inventory.Items.Count == count + 1, "item count change in Map Device"))
                    return false;
            }

            await Wait.SleepSafe(500);

            var err = MapDeviceUi.Activate();
            if (err != LokiPoe.InGameState.ActivateResult.None)
            {
                GlobalLog.Error($"[EssenceOfArtist] Fail to activate the Map Device. Error: \"{err}\".");
                return false;
            }

            if (!await Wait.For(() => !MapDeviceUi.IsOpened, "Map Device closing"))
                return false;

            if (!await Wait.For(() => MaligaroPortal != null, "portal to Maligaro's Sanctum", 100, 10000))
                return false;

            await Wait.SleepSafe(1000);
            return true;
        }

        private static bool IsMaligaroMap(Item item)
        {
            return item.Class == ItemClasses.QuestItem && item.Metadata.Contains(QuestItemMetadata.MaligaroMap);
        }
    }
}