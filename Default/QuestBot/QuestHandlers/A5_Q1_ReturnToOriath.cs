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
    public static class A5_Q1_ReturnToOriath
    {
        private static readonly TgtPosition AscentSummitTgt = new TgtPosition("Portal device location", "ascent_summit_v01_01_c3r2.tgt");

        private static NetworkObject DeviceLever => LokiPoe.ObjectManager.Objects
            .Find(o => o.Metadata == "Metadata/Terrain/Act4/Area7/Objects/PortalDeviceLever");

        private static AreaTransition OriathTransition => LokiPoe.ObjectManager.Objects
            .FirstOrDefault<AreaTransition>(a => a.Metadata == "Metadata/Terrain/Act4/Area7/Objects/PortalDeviceTransition");

        private static Monster OverseerKrow => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Overseer_Krow)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static CachedObject CachedDeviceLever
        {
            get => CombatAreaCache.Current.Storage["DeviceLever"] as CachedObject;
            set => CombatAreaCache.Current.Storage["DeviceLever"] = value;
        }

        private static WalkablePosition CachedOverseerKrowPos
        {
            get => CombatAreaCache.Current.Storage["OverseerKrowPosition"] as WalkablePosition;
            set => CombatAreaCache.Current.Storage["OverseerKrowPosition"] = value;
        }

        public static void Tick()
        {
            if (World.Act4.Ascent.IsCurrentArea)
            {
                if (CachedDeviceLever == null)
                {
                    var lever = DeviceLever;
                    if (lever != null)
                    {
                        CachedDeviceLever = new CachedObject(lever);
                    }
                }
                return;
            }
            if (World.Act5.SlavePens.IsCurrentArea)
            {
                var krow = OverseerKrow;
                if (krow != null)
                {
                    CachedOverseerKrowPos = krow.IsDead ? null : krow.WalkablePosition();
                }
            }
        }

        public static async Task<bool> EnterOverseerTower()
        {
            if (World.Act5.OverseerTower.IsCurrentArea)
            {
                if (World.Act5.OverseerTower.IsWaypointOpened)
                    return false;

                if (!await PlayerAction.OpenWaypoint())
                    ErrorManager.ReportError();

                return true;
            }
            if (World.Act4.Ascent.IsCurrentArea)
            {
                var lever = CachedDeviceLever;
                if (lever != null)
                {
                    var pos = lever.Position;
                    if (pos.IsFar)
                    {
                        pos.Come();
                        return true;
                    }
                    var leverObj = lever.Object;
                    if (leverObj.IsTargetable)
                    {
                        if (!await PlayerAction.Interact(leverObj, () => !leverObj.Fresh().IsTargetable, "Device lever interaction"))
                            ErrorManager.ReportError();

                        return true;
                    }
                    var transition = OriathTransition;
                    if (transition != null && transition.IsTargetable)
                    {
                        if (!await PlayerAction.TakeTransition(transition))
                            ErrorManager.ReportError();

                        return true;
                    }
                    GlobalLog.Debug("Waiting for portal to Oriath");
                    await Wait.StuckDetectionSleep(500);
                    return true;
                }
                AscentSummitTgt.Come();
                return true;
            }
            if (World.Act5.SlavePens.IsCurrentArea)
            {
                var krowPos = CachedOverseerKrowPos;
                if (krowPos != null)
                {
                    await Helpers.MoveAndWait(krowPos);
                    return true;
                }
                await Travel.To(World.Act5.OverseerTower);
                return true;
            }
            if (World.Act5.SlavePens.IsWaypointOpened)
            {
                await Travel.To(World.Act5.SlavePens);
                return true;
            }
            await Travel.To(World.Act4.Ascent);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act5.OverseerTower,
                TownNpcs.Lani,
                "Overseer Reward",
                Quests.ReturnToOriath.Id);
        }
    }
}