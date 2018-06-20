using System.Collections.Generic;
using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Common;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A7_Q9_MotherOfSpiders
    {
        private static bool _arakaaliKilled;

        private static Monster Arakaali => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Arakaali_Spinner_of_Shadows)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static NetworkObject ArakaaliRoomObj => LokiPoe.ObjectManager.Objects
            .Find(o => o.Metadata == "Metadata/Terrain/Act7/Area12Level2/Objects/ArakaaliArenaMiddle");

        private static Dictionary<Vector2i, WalkablePosition> CachedArakaaliPositions
        {
            get
            {
                var pos = CombatAreaCache.Current.Storage["ArakaaliPositions"] as Dictionary<Vector2i, WalkablePosition>;
                if (pos == null)
                {
                    pos = new Dictionary<Vector2i, WalkablePosition>();
                    CombatAreaCache.Current.Storage["ArakaaliPositions"] = pos;
                }
                return pos;
            }
        }

        public static void Tick()
        {
            _arakaaliKilled = World.Act8.SarnRamparts.IsWaypointOpened;
        }

        public static async Task<bool> KillArakaali()
        {
            if (_arakaaliKilled)
                return false;

            if (World.Act7.TempleOfDecay2.IsCurrentArea)
            {
                var roomObj = ArakaaliRoomObj;
                if (roomObj != null)
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.Arakaali))
                        return true;

                    var arakaali = Arakaali;
                    if (arakaali != null)
                    {
                        var pos = GetCachedWalkable(arakaali.Position);
                        if (pos != null)
                        {
                            await Helpers.MoveAndWait(pos, "Waiting for Arakaali", 10);
                        }
                        else
                        {
                            await Wait.StuckDetectionSleep(500);
                        }
                        return true;
                    }
                    await Helpers.MoveAndWait(roomObj.WalkablePosition(), "Waiting for any Arakaali fight object");
                    return true;
                }
            }
            await Travel.To(World.Act8.SarnRamparts);
            return true;
        }

        public static async Task<bool> EnterSarnEncampment()
        {
            if (World.Act8.SarnEncampment.IsCurrentArea)
            {
                if (World.Act8.SarnEncampment.IsWaypointOpened)
                    return false;

                if (!await PlayerAction.OpenWaypoint())
                    ErrorManager.ReportError();

                return true;
            }
            await Travel.To(World.Act8.SarnEncampment);
            return true;
        }

        public static WalkablePosition GetCachedWalkable(Vector2i pos)
        {
            if (CachedArakaaliPositions.TryGetValue(pos, out WalkablePosition walkable))
                return walkable;

            walkable = new WalkablePosition("walkable Arakaali position", pos, 5);
            if (!walkable.Initialize())
            {
                GlobalLog.Error($"[MotherOfSpiders] Cannot find walkable position for current Arakaali position {pos}.");
                return null;
            }
            GlobalLog.Warn($"[MotherOfSpiders] Registering walkable Arakaali position {walkable.AsVector}.");
            CachedArakaaliPositions.Add(pos, walkable);
            return walkable;
        }
    }
}