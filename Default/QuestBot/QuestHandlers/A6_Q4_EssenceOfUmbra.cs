using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Loki.Bot;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A6_Q4_EssenceOfUmbra
    {
        private static NetworkObject _shavronneRoomObj;
        private static Monster _shavronne;
        private static Monster _brutus;

        private static bool _shavronneKilled;

        public static void Tick()
        {
            _shavronneKilled = World.Act6.PrisonerGate.IsWaypointOpened;
        }

        public static async Task<bool> KillShavronne()
        {
            if (_shavronneKilled)
                return false;

            if (World.Act6.ShavronneTower.IsCurrentArea)
            {
                UpdateShavronneFightObjects();

                if (_shavronneRoomObj != null)
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.Shavronne))
                        return true;

                    if (_brutus != null && _brutus.IsActive)
                    {
                        await Helpers.MoveAndWait(_brutus.WalkablePosition());
                        return true;
                    }
                    if (_shavronne != null)
                    {
                        int distance = _shavronne.IsActive ? 20 : 35;
                        var pos = _shavronne.WalkablePosition();
                        if (pos.Distance > distance)
                        {
                            pos.Come();
                            return true;
                        }
                        GlobalLog.Debug($"Waiting for {pos.Name}");
                        await Coroutines.FinishCurrentAction();
                        await Wait.StuckDetectionSleep(200);
                        return true;
                    }
                    await Helpers.MoveAndWait(_shavronneRoomObj.WalkablePosition(), "Waiting for any Shavronne fight object");
                    return true;
                }
            }
            await Travel.To(World.Act6.PrisonerGate);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act6.LioneyeWatch,
                TownNpcs.Tarkleigh_A6,
                "Shavronne Reward",
                Quests.EssenceOfUmbra.Id);
        }

        private static void UpdateShavronneFightObjects()
        {
            _shavronneRoomObj = null;
            _shavronne = null;
            _brutus = null;

            foreach (var obj in LokiPoe.ObjectManager.Objects)
            {
                var metadata = obj.Metadata;

                var mob = obj as Monster;
                if (mob != null && mob.Rarity == Rarity.Unique)
                {
                    if (metadata == "Metadata/Monsters/Shavronne/ShavronneTower")
                    {
                        _shavronne = mob;
                    }
                    else if (metadata == "Metadata/Monsters/Brute/BruteTower")
                    {
                        _brutus = mob;
                    }
                    continue;
                }
                if (metadata == "Metadata/Monsters/Shavronne/ShavronneArenaMiddle")
                {
                    _shavronneRoomObj = obj;
                }
            }
        }
    }
}