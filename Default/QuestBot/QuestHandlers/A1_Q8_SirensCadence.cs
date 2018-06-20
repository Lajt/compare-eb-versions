using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A1_Q8_SirensCadence
    {
        private static bool _merveilKilled;
        private static Monster _merveil1;
        private static Monster _merveil2;

        public static void Tick()
        {
            _merveilKilled = World.Act2.SouthernForest.IsWaypointOpened;
        }

        public static async Task<bool> KillMerveil()
        {
            if (_merveilKilled)
                return false;

            if (World.Act1.CavernOfAnger.IsCurrentArea)
            {
                UpdateMerveilFightObjects();

                if (_merveil2 != null)
                {
                    await Helpers.MoveAndWait(_merveil2.WalkablePosition());
                    return true;
                }
                if (_merveil1 != null)
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.Merveil))
                        return true;

                    await Helpers.MoveAndWait(_merveil1.WalkablePosition(10, 50));
                    return true;
                }
            }
            await Travel.To(World.Act2.SouthernForest);
            return true;
        }

        public static async Task<bool> EnterForestEncampment()
        {
            if (World.Act2.ForestEncampment.IsCurrentArea)
            {
                if (World.Act2.ForestEncampment.IsWaypointOpened)
                    return false;

                if (!await PlayerAction.OpenWaypoint())
                    ErrorManager.ReportError();

                return true;
            }

            await Travel.To(World.Act2.ForestEncampment);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act1.LioneyeWatch,
                TownNpcs.Nessa,
                "Merveil Reward",
                Quests.SirensCadence.Id);
        }

        private static void UpdateMerveilFightObjects()
        {
            _merveil1 = null;
            _merveil2 = null;

            foreach (var obj in LokiPoe.ObjectManager.Objects)
            {
                var mob = obj as Monster;
                if (mob != null && mob.Rarity == Rarity.Unique)
                {
                    if (mob.Metadata.EqualsIgnorecase("Metadata/Monsters/seawitchillusion/BossMerveil"))
                    {
                        _merveil1 = mob;
                        continue;
                    }
                    if (mob.Metadata == "Metadata/Monsters/Seawitch/BossMerveil2")
                    {
                        _merveil2 = mob;
                    }
                }
            }
        }
    }
}