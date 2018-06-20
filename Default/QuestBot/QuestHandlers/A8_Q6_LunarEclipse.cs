using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A8_Q6_LunarEclipse
    {
        private static readonly TgtPosition SolarisLunarisRoomTgt = new TgtPosition("Solaris and Lunaris room", "bridge_arena_v01_01_c11r7.tgt", true);

        private static readonly TgtPosition DuskRoomTgt = new TgtPosition("Dusk room", "templeclean_prepiety_roundtop_center_01_c1r3.tgt");
        private static readonly TgtPosition DawnRoomTgt = new TgtPosition("Dawn room", "gemling_queen_throne_v01_01_c3r2.tgt");

        private static Monster Dusk => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Dusk_Harbinger_of_Lunaris)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static Monster Dawn => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Dawn_Harbinger_of_Solaris)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static NetworkObject _statue;
        private static Monster _solaris;
        private static Monster _lunaris;

        private static bool _finished;

        public static void Tick()
        {
            _finished = World.Act9.BloodAqueduct.IsWaypointOpened;
        }

        public static async Task<bool> GrabMoonOrb()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.MoonOrb))
                return false;

            if (World.Act8.LunarisTemple2.IsCurrentArea)
            {
                var dusk = Dusk;
                if (dusk != null)
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.Dusk))
                        return true;

                    await Helpers.MoveAndWait(dusk.WalkablePosition());
                    return true;
                }
                await Helpers.MoveAndTakeLocalTransition(DuskRoomTgt);
                return true;
            }
            await Travel.To(World.Act8.LunarisTemple2);
            return true;
        }

        public static async Task<bool> GrabSunOrb()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.SunOrb))
                return false;

            if (World.Act8.SolarisTemple2.IsCurrentArea)
            {
                var dawn = Dawn;
                if (dawn != null)
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.Dawn))
                        return true;

                    await Helpers.MoveAndWait(dawn.WalkablePosition());
                    return true;
                }
                await Helpers.MoveAndTakeLocalTransition(DawnRoomTgt);
                return true;
            }
            await Travel.To(World.Act8.SolarisTemple2);
            return true;
        }

        public static async Task<bool> KillSolarisLunaris()
        {
            if (_finished)
                return false;

            if (World.Act8.HarbourBridge.IsCurrentArea)
            {
                UpdateSolarisLunarisFightObjects();

                if (_statue != null)
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.SolarisLunaris))
                        return true;

                    if (_solaris != null && _solaris.IsActive)
                    {
                        await Helpers.MoveAndWait(_solaris.WalkablePosition());
                        return true;
                    }
                    if (_lunaris != null && _lunaris.IsActive)
                    {
                        await Helpers.MoveAndWait(_lunaris.WalkablePosition());
                        return true;
                    }
                    if (_statue.IsTargetable)
                    {
                        await _statue.WalkablePosition().ComeAtOnce();

                        if (!await PlayerAction.Interact(_statue, () => !_statue.Fresh().IsTargetable, "Statue interaction"))
                            ErrorManager.ReportError();

                        return true;
                    }
                    GlobalLog.Debug("Waiting for any Solaris and Lunaris fight object");
                    await Wait.StuckDetectionSleep(500);
                    return true;
                }
            }
            await Travel.To(World.Act9.BloodAqueduct);
            return true;
        }

        public static async Task<bool> EnterHighgate()
        {
            if (World.Act9.Highgate.IsCurrentArea)
            {
                if (World.Act9.Highgate.IsWaypointOpened)
                    return false;

                if (!await PlayerAction.OpenWaypoint())
                    ErrorManager.ReportError();

                return true;
            }
            await Travel.To(World.Act9.Highgate);
            return true;
        }

        private static void UpdateSolarisLunarisFightObjects()
        {
            _statue = null;
            _solaris = null;
            _lunaris = null;

            foreach (var obj in LokiPoe.ObjectManager.Objects)
            {
                var metadata = obj.Metadata;

                var mob = obj as Monster;
                if (mob != null && mob.Rarity == Rarity.Unique)
                {
                    if (metadata == "Metadata/Monsters/LunarisSolaris/Solaris")
                    {
                        _solaris = mob;
                    }
                    else if (metadata == "Metadata/Monsters/LunarisSolaris/Lunaris")
                    {
                        _lunaris = mob;
                    }
                    continue;
                }
                if (metadata == "Metadata/QuestObjects/Act8/GoddessFightStarter")
                {
                    _statue = obj;
                }
            }
        }
    }
}