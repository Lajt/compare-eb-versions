using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A10_Q1_SafePassage
    {
        private static readonly TgtPosition BannonRoomTgt = new TgtPosition("Bannon room", "cathedralroof_boss_area_v02_01_c5r10.tgt");

        private const int FinishedStateMinimum = 4;
        private static bool _finished;

        private static Npc Bannon => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Bannon)
            .FirstOrDefault<Npc>();

        public static void Tick()
        {
            _finished = QuestManager.GetStateInaccurate(Quests.SafePassage) <= FinishedStateMinimum;
        }

        public static async Task<bool> SaveBannon()
        {
            if (_finished)
                return false;

            if (World.Act10.CathedralRooftop.IsCurrentArea)
            {
                var bannon = Bannon;
                if (bannon != null && bannon.PathExists())
                {
                    var mob = Helpers.ClosestActiveMob;
                    if (mob != null && mob.PathExists())
                    {
                        await Helpers.MoveAndWait(mob);
                        return true;
                    }
                    await Helpers.MoveAndWait(bannon, "Waiting for any active monster");
                    return true;
                }
                await Helpers.MoveAndTakeLocalTransition(BannonRoomTgt);
                return true;
            }
            await Travel.To(World.Act10.CathedralRooftop);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act10.OriathDocks,
                TownNpcs.Lani_A10,
                "Bannon Reward",
                Quests.SafePassage.Id);
        }
    }
}