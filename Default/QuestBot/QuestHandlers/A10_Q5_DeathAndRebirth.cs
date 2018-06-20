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
    public static class A10_Q5_DeathAndRebirth
    {
        private static readonly TgtPosition AvariusRoomTgt = new TgtPosition("Avarius room", "transition_chamber_to_boss_shattered_v01_01_c3r4.tgt");

        private static readonly TownNpc TownBannon = new TownNpc(new WalkablePosition("Bannon", 470, 295));

        private static Monster Avarius => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Avarius_Reassembled)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        public static void Tick()
        {
        }

        public static async Task<bool> KillAvarius()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.StaffOfPurity))
                return false;

            if (World.Act10.DesecratedChambers.IsCurrentArea)
            {
                var avarius = Avarius;
                if (avarius != null && avarius.PathExists())
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.AvariusReassembled))
                        return true;

                    await Helpers.MoveAndWait(avarius);
                    return true;
                }
                await Helpers.MoveAndTakeLocalTransition(AvariusRoomTgt);
                return true;
            }
            await Travel.To(World.Act10.DesecratedChambers);
            return true;
        }

        public static async Task<bool> TurnInStaff()
        {
            if (!Helpers.PlayerHasQuestItem(QuestItemMetadata.StaffOfPurity))
                return false;

            if (World.Act10.OriathDocks.IsCurrentArea)
            {
                if (await TownBannon.Talk())
                {
                    await Coroutines.CloseBlockingWindows();
                }
                else
                {
                    ErrorManager.ReportError();
                }
                return true;
            }
            await Travel.To(World.Act10.OriathDocks);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act10.OriathDocks,
                TownNpcs.Lani_A10,
                "Innocence Reward",
                Quests.DeathAndRebirth.Id);
        }
    }
}