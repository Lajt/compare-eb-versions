using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A1_Q6_CagedBrute
    {
        private static bool _brutusKilled;

        private static Npc Navali => LokiPoe.ObjectManager.Objects
            .FirstOrDefault<Npc>(n => n.Metadata == "Metadata/NPC/League/NavaliWild");

        private static TriggerableBlockage NavaliCage => LokiPoe.ObjectManager.Objects
            .FirstOrDefault<TriggerableBlockage>(t => t.Metadata == "Metadata/NPC/League/NavaliCage");

        private static Monster Brutus => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Brutus_Lord_Incarcerator)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        public static void Tick()
        {
            _brutusKilled = World.Act1.PrisonerGate.IsWaypointOpened;
        }

        public static async Task<bool> EnterPrison()
        {
            if (World.Act1.Climb.IsCurrentArea)
            {
                var navali = Navali;
                if (navali != null)
                {
                    if (!navali.IsTargetable)
                    {
                        var navaliPos = navali.WalkablePosition();
                        if (navaliPos.IsFar)
                        {
                            navaliPos.Come();
                            return true;
                        }
                        var cage = NavaliCage;
                        if (cage == null)
                        {
                            GlobalLog.Debug("We are near Navali but Cage object is null.");
                            await Wait.StuckDetectionSleep(500);
                            return true;
                        }
                        if (cage.IsTargetable)
                        {
                            if (!await PlayerAction.Interact(cage, () => !cage.Fresh().IsTargetable, "Navali Cage opening", 5000))
                                ErrorManager.ReportError();
                        }
                        GlobalLog.Debug("Waiting for Navali");
                        await Wait.StuckDetectionSleep(200);
                        return true;
                    }
                    if (navali.HasNpcFloatingIcon)
                    {
                        var navaliPos = navali.WalkablePosition();
                        if (navaliPos.IsFar)
                        {
                            navaliPos.Come();
                        }
                        else
                        {
                            await Helpers.TalkTo(navali);
                        }
                        return true;
                    }
                }
            }

            if (World.Act1.LowerPrison.IsCurrentArea)
                return false;

            await Travel.To(World.Act1.LowerPrison);
            return true;
        }

        public static async Task<bool> KillBrutus()
        {
            if (_brutusKilled)
                return false;

            if (World.Act1.UpperPrison.IsCurrentArea)
            {
                var brutus = Brutus;
                if (brutus != null && brutus.PathExists())
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.Brutus))
                        return true;

                    await Helpers.MoveAndWait(brutus);
                    return true;
                }
            }
            await Travel.To(World.Act1.PrisonerGate);
            return true;
        }

        public static async Task<bool> TakeNessaReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act1.LioneyeWatch,
                TownNpcs.Nessa,
                "Prison Reward",
                Quests.CagedBrute.Id + "b");
        }

        public static async Task<bool> TakeTarkleighReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act1.LioneyeWatch,
                TownNpcs.Tarkleigh,
                "Brutus Reward",
                Quests.CagedBrute.Id);
        }
    }
}