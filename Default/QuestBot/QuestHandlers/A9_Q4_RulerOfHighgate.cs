using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Bot;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A9_Q4_RulerOfHighgate
    {
        private static readonly TgtPosition GarukhanRoomTgt = new TgtPosition("Garukhan room", "GarukhanArena_entrance_v01_01_c2r1.tgt");

        private static Monster Garukhan => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Garukhan_Queen_of_the_Winds)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        public static void Tick()
        {
        }

        public static async Task<bool> KillGarukhan()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.SekhemaFeather))
                return false;

            if (World.Act9.Quarry.IsCurrentArea)
            {
                var garukhan = Garukhan;
                if (garukhan != null)
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.Garukhan))
                        return true;

                    if (!garukhan.IsActive)
                    {
                        var mob = Helpers.ClosestActiveMob;
                        if (mob != null && mob.PathExists())
                        {
                            PlayerMoverManager.MoveTowards(mob.Position);
                            return true;
                        }
                    }
                    await Helpers.MoveAndWait(garukhan.WalkablePosition());
                    return true;
                }
                await Helpers.MoveAndTakeLocalTransition(GarukhanRoomTgt);
                return true;
            }
            await Travel.To(World.Act9.Quarry);
            return true;
        }

        public static async Task<bool> TakeIrashaReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act9.Highgate,
                TownNpcs.Irasha,
                "Feather Reward",
                book: QuestItemMetadata.BookGarukhan);
        }

        public static async Task<bool> TakeTasuniReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act9.Highgate,
                TownNpcs.Tasuni_A9,
                "Feather Reward",
                book: QuestItemMetadata.BookGarukhan);
        }
    }
}