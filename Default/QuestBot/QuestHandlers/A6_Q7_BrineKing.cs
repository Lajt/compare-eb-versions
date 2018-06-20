using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Bot;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A6_Q7_BrineKing
    {
        private const int BeaconFueledStateMinimum = 7;
        private const int BrineKingFightCombatRange = 25;

        private static readonly TgtPosition WeylamTgt = new TgtPosition("Weylam Roth location", "weylamwalk_v01_01_c6r3.tgt");
        private static readonly TgtPosition BrineKingRoomTgt = new TgtPosition("Brine King room", "beach_island_transition_v01_02_c10r3.tgt");

        private static bool _beaconFueled;
        private static bool _beaconLighted;

        public static int? OriginalCombatRange;

        private static Chest FlagChest => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Flag_Chest)
            .FirstOrDefault<Chest>();

        private static Monster BrineKing => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Tsoagoth_The_Brine_King)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static Npc BrineKingWeylam => LokiPoe.ObjectManager.Objects
            .FirstOrDefault<Npc>(n => n.Metadata == "Metadata/NPC/Act6/WeylamReef2");

        private static List<CachedObject> CachedFuelCarts
        {
            get
            {
                var carts = CombatAreaCache.Current.Storage["FuelCarts"] as List<CachedObject>;
                if (carts == null)
                {
                    carts = new List<CachedObject>(2);
                    CombatAreaCache.Current.Storage["FuelCarts"] = carts;
                }
                return carts;
            }
        }

        private static CachedObject CachedBeaconSwitch
        {
            get => CombatAreaCache.Current.Storage["BeaconSwitch"] as CachedObject;
            set => CombatAreaCache.Current.Storage["BeaconSwitch"] = value;
        }

        private static CachedObject CachedBeaconLighter
        {
            get => CombatAreaCache.Current.Storage["BeaconLighter"] as CachedObject;
            set => CombatAreaCache.Current.Storage["BeaconLighter"] = value;
        }

        private static CachedObject CachedBeaconWeylam
        {
            get => CombatAreaCache.Current.Storage["BeaconWeylam"] as CachedObject;
            set => CombatAreaCache.Current.Storage["BeaconWeylam"] = value;
        }

        public static void Tick()
        {
            if (!World.Act6.Beacon.IsCurrentArea)
                return;

            var state = QuestManager.GetStateInaccurate(Quests.BrineKing);

            _beaconFueled = state <= BeaconFueledStateMinimum;
            _beaconLighted = state != BeaconFueledStateMinimum;

            foreach (var obj in LokiPoe.ObjectManager.Objects)
            {
                var metadata = obj.Metadata;

                if (metadata == "Metadata/QuestObjects/Act6/BeaconPayload")
                {
                    if (!IsDelivered(obj))
                    {
                        var id = obj.Id;
                        var pos = new WalkablePosition("Fuel cart", obj.Position);

                        var cachedCarts = CachedFuelCarts;
                        var cachedCart = cachedCarts.Find(p => p.Id == id);

                        if (cachedCart == null)
                        {
                            GlobalLog.Warn($"[BrineKing] Registering {pos}");
                            cachedCarts.Add(new CachedObject(id, pos));
                        }
                        else
                        {
                            cachedCart.Position = pos;
                        }
                    }
                    continue;
                }
                if (metadata == "Metadata/QuestObjects/Act6/BlackCrest_BeaconLever")
                {
                    if (CachedBeaconSwitch == null)
                    {
                        CachedBeaconSwitch = new CachedObject(obj);
                    }
                    continue;
                }
                if (metadata == "Metadata/QuestObjects/Act6/BlackCrest_BeaconInteract")
                {
                    if (CachedBeaconLighter == null)
                    {
                        CachedBeaconLighter = new CachedObject(obj);
                    }
                    continue;
                }
                if (metadata == "Metadata/NPC/Act6/WeylamBeacon")
                {
                    if (CachedBeaconWeylam == null)
                    {
                        CachedBeaconWeylam = new CachedObject(obj);
                    }
                }
            }
        }

        public static async Task<bool> GrabBlackFlag()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.BlackFlag))
                return false;

            if (World.Act6.CavernOfAnger.IsCurrentArea)
            {
                var flagChest = FlagChest;

                // In case user started bot in the middle of the Cavern
                if (flagChest == null)
                {
                    GlobalLog.Error("[GrabBlackFlag] Unexpected error. We are inside Cavern of Anger but there is no Flag Chest.");
                    ErrorManager.ReportError();
                    await PlayerAction.TpToTown();
                    return true;
                }
                if (!flagChest.IsOpened)
                {
                    await flagChest.WalkablePosition().ComeAtOnce();

                    if (!await PlayerAction.Interact(flagChest, () => flagChest.Fresh().IsOpened, "Flag Chest opening"))
                        ErrorManager.ReportError();

                    return true;
                }
                GlobalLog.Debug("[GrabBlackFlag] Flag Chest is opened. Waiting for Black Flag pick up.");
                await Wait.StuckDetectionSleep(200);
                return true;
            }
            await Travel.To(World.Act6.CavernOfAnger);
            return true;
        }

        public static async Task<bool> FuelBeacon()
        {
            if (_beaconFueled)
                return false;

            if (World.Act6.Beacon.IsCurrentArea)
            {
                var cachedCarts = CachedFuelCarts;
                var cachedCart = cachedCarts.FirstOrDefault();

                if (cachedCart != null)
                {
                    var pos = cachedCart.Position;
                    if (pos.Distance > 10)
                    {
                        pos.Come();
                        return true;
                    }
                    var cartObj = cachedCart.Object;
                    if (IsDelivered(cartObj))
                    {
                        GlobalLog.Warn($"[FuelBeacon] Fuel cart (id: {cachedCart.Id}) has arrived to it's destination.");
                        cachedCarts.Remove(cachedCart);
                        return true;
                    }
                    GlobalLog.Debug($"[FuelBeacon] Waiting for Fuel cart (id: {cachedCart.Id})");
                    await Wait.StuckDetectionSleep(200);
                    return true;
                }
                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act6.Beacon);
            return true;
        }

        public static async Task<bool> LightBeacon()
        {
            if (_beaconLighted)
                return false;

            if (World.Act6.Beacon.IsCurrentArea)
            {
                var beaconSwitch = CachedBeaconSwitch;
                if (beaconSwitch != null)
                {
                    var switchPos = beaconSwitch.Position;
                    if (switchPos.IsFar)
                    {
                        switchPos.Come();
                        return true;
                    }
                    var switchObj = beaconSwitch.Object;
                    if (switchObj.IsTargetable)
                    {
                        if (!await PlayerAction.Interact(switchObj, () => !switchObj.Fresh().IsTargetable, "Ignition Switch interaction", 5000))
                            ErrorManager.ReportError();

                        return true;
                    }
                    var beaconLighter = CachedBeaconLighter;
                    if (beaconLighter == null)
                    {
                        GlobalLog.Debug("[BrineKing] We are near Ignition Switch but Beacon object is null.");
                        await Wait.StuckDetectionSleep(500);
                        return true;
                    }
                    var lighterObj = beaconLighter.Object;
                    if (lighterObj.IsTargetable)
                    {
                        if (!await PlayerAction.Interact(lighterObj, () => !lighterObj.Fresh().IsTargetable, "Beacon interaction"))
                            ErrorManager.ReportError();

                        return true;
                    }
                    GlobalLog.Debug("Waiting for Weylam Roth ship");
                    await Wait.StuckDetectionSleep(500);
                    return true;
                }
                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act6.Beacon);
            return true;
        }

        public static async Task<bool> SailToReef()
        {
            if (World.Act6.BrineKingReef.IsCurrentArea)
                return false;

            if (World.Act6.Beacon.IsCurrentArea)
            {
                var weylam = CachedBeaconWeylam;
                if (weylam != null)
                {
                    var pos = weylam.Position;
                    if (pos.IsFar)
                    {
                        pos.Come();
                        return true;
                    }
                    var weylamObj = weylam.Object;
                    if (!weylamObj.IsTargetable)
                    {
                        GlobalLog.Debug("Waiting for Weylam Roth");
                        await Wait.StuckDetectionSleep(200);
                        return true;
                    }
                    var hash = LokiPoe.LocalData.AreaHash;
                    if (!await weylamObj.AsTownNpc().Converse("Sail to the Brine King's Reef"))
                    {
                        ErrorManager.ReportError();
                        return true;
                    }
                    await Coroutines.CloseBlockingWindows();
                    await Wait.ForAreaChange(hash);
                    return true;
                }
                WeylamTgt.Come();
                return true;
            }
            await Travel.To(World.Act6.Beacon);
            return true;
        }

        public static async Task<bool> KillBrineKingAndSailToAct7()
        {
            if (World.Act7.BridgeEncampment.IsCurrentArea)
            {
                if (World.Act7.BridgeEncampment.IsWaypointOpened)
                    return false;

                if (!await PlayerAction.OpenWaypoint())
                    ErrorManager.ReportError();

                return true;
            }
            if (World.Act6.BrineKingReef.IsCurrentArea)
            {
                var weylam = BrineKingWeylam;
                if (weylam != null && weylam.IsTargetable)
                {
                    var hash = LokiPoe.LocalData.AreaHash;
                    if (!await weylam.AsTownNpc().Converse("Sail to Act 7"))
                    {
                        ErrorManager.ReportError();
                        return true;
                    }
                    await Coroutines.CloseBlockingWindows();
                    await Wait.ForAreaChange(hash);
                    return true;
                }
                var brineKing = BrineKing;
                if (brineKing != null)
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.BrineKing))
                        return true;

                    if (OriginalCombatRange == null)
                    {
                        ChangeCombatRange();
                    }
                    var pos = brineKing.WalkablePosition();
                    if (pos.Distance > BrineKingFightCombatRange - 5)
                    {
                        pos.Come();
                        return true;
                    }
                    GlobalLog.Debug($"Waiting for {pos.Name}");
                    await Coroutines.FinishCurrentAction();
                    await Wait.StuckDetectionSleep(200);
                    return true;
                }
                await Helpers.MoveAndTakeLocalTransition(BrineKingRoomTgt);
                return true;
            }
            await Travel.To(World.Act6.BrineKingReef);
            return true;
        }

        private static void ChangeCombatRange()
        {
            var cr = RoutineManager.Current;
            var msg = new Message("GetCombatRange");
            if (cr.Message(msg) != MessageResult.Processed)
            {
                GlobalLog.Error($"[BrineKing] {cr.Name} does not support GetCombatRange message.");
                ErrorManager.ReportCriticalError();
            }

            OriginalCombatRange = msg.GetOutput<int>();
            GlobalLog.Warn($"[BrineKing] Saving original combat range {OriginalCombatRange}.");

            msg = new Message("SetCombatRange", null, BrineKingFightCombatRange);
            if (cr.Message(msg) != MessageResult.Processed)
            {
                GlobalLog.Error($"[BrineKing] {cr.Name} does not support SetCombatRange message.");
                ErrorManager.ReportCriticalError();
            }
            GlobalLog.Warn($"[BrineKing] Combat range has been set to {BrineKingFightCombatRange}.");
        }

        public static void RestoreCombatRange()
        {
            var cr = RoutineManager.Current;
            var msg = new Message("SetCombatRange", null, OriginalCombatRange);
            if (cr.Message(msg) != MessageResult.Processed)
            {
                GlobalLog.Error($"[BrineKing] Cannot restore original combat range. {cr.Name} does not support SetCombatRange message.");
            }
            else
            {
                GlobalLog.Warn($"[BrineKing] Combat range has been restored to original {OriginalCombatRange}.");
            }
            OriginalCombatRange = null;
        }

        private static bool IsDelivered(NetworkObject cart)
        {
            return cart.Components.TransitionableComponent?.Flag2 == 2;
        }
    }
}