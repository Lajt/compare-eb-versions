using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Loki.Bot;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A7_Q4_MasterOfMillionFaces
    {
        private static bool _ralakeshKilled;

        private static NetworkObject RalakeshRoomObj => LokiPoe.ObjectManager.Objects
            .Find(o => o.Metadata == "Metadata/MiscellaneousObjects/ArenaMiddle");

        public static void Tick()
        {
            _ralakeshKilled = World.Act7.NorthernForest.IsWaypointOpened;
        }

        public static async Task<bool> KillRalakesh()
        {
            if (_ralakeshKilled)
                return false;

            if (World.Act7.AshenFields.IsCurrentArea)
            {
                var roomObj = RalakeshRoomObj;
                if (roomObj != null)
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.Ralakesh))
                        return true;

                    if (roomObj.PathExists())
                    {
                        var mob = Helpers.ClosestActiveMob;
                        if (mob != null && mob.PathExists())
                        {
                            PlayerMoverManager.MoveTowards(mob.Position);
                            return true;
                        }
                        await Helpers.MoveAndWait(roomObj, "Waiting for any Ralakesh fight object");
                        return true;
                    }
                }
            }
            await Travel.To(World.Act7.NorthernForest);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act7.BridgeEncampment,
                TownNpcs.Eramir_A7,
                "Ralakesh Reward",
                book: QuestItemMetadata.BookRalakesh);
        }
    }
}