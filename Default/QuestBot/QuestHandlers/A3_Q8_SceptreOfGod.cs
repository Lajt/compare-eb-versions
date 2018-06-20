using System;
using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Loki.Bot;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A3_Q8_SceptreOfGod
    {
        private static NetworkObject _dominusRoomObj;
        private static Monster _dominus;
        private static Monster _dominus2;
        private static Monster _anyActiveUniqueMob;
        private static bool _dominusKilled;

        public static void Tick()
        {
            _dominusKilled = World.Act4.Aqueduct.IsWaypointOpened;
        }

        public static async Task<bool> KillDominus()
        {
            if (_dominusKilled)
                return false;

            if (World.Act3.UpperSceptreOfGod.IsCurrentArea)
            {
                UpdateDominusFightObjects();

                if (_dominus2 != null)
                {
                    await Helpers.MoveAndWait(_dominus2.WalkablePosition());
                    return true;
                }
                if (_dominus != null)
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.Dominus))
                        return true;

                    if (!_dominus.IsTargetable && _anyActiveUniqueMob != null)
                    {
                        await Helpers.MoveAndWait(_anyActiveUniqueMob.WalkablePosition());
                        return true;
                    }
                    if (_dominus.Health == 1)
                    {
                        var id = _dominus.Id;
                        if (!Blacklist.Contains(id))
                        {
                            Blacklist.Add(id, TimeSpan.FromDays(1), "Dominus first form waiting for death");
                        }
                    }
                    await Helpers.MoveAndWait(_dominus.WalkablePosition());
                    return true;
                }
                if (_dominusRoomObj != null)
                {
                    await Helpers.MoveAndWait(_dominusRoomObj.WalkablePosition(), "Waiting for any Dominus fight object");
                    return true;
                }
            }
            await Travel.To(World.Act4.Aqueduct);
            return true;
        }

        public static async Task<bool> EnterHighgate()
        {
            if (World.Act4.Highgate.IsCurrentArea)
            {
                if (World.Act4.Highgate.IsWaypointOpened)
                    return false;

                if (!await PlayerAction.OpenWaypoint())
                    ErrorManager.ReportError();

                return true;
            }
            await Travel.To(World.Act4.Highgate);
            return true;
        }

        private static void UpdateDominusFightObjects()
        {
            _dominusRoomObj = null;
            _dominus = null;
            _dominus2 = null;
            _anyActiveUniqueMob = null;

            foreach (var obj in LokiPoe.ObjectManager.Objects)
            {
                var mob = obj as Monster;
                if (mob != null && mob.Rarity == Rarity.Unique)
                {
                    if (mob.Metadata == "Metadata/Monsters/Pope/Pope")
                    {
                        _dominus = mob;
                        continue;
                    }
                    if (mob.Metadata == "Metadata/Monsters/Dominusdemon/Dominusdemon")
                    {
                        _dominus2 = mob;
                        continue;
                    }
                    if (!mob.IsDead && mob.IsTargetable)
                    {
                        _anyActiveUniqueMob = mob;
                    }
                    continue;
                }
                if (obj.Metadata == "Metadata/Monsters/Demonmodular/TowerSpawners/TowerSpawnerBoss2")
                {
                    _dominusRoomObj = obj;
                }
            }
        }
    }
}