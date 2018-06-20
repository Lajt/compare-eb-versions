using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A4_Q4_KingOfDesire
    {
        private static Monster Daresso => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Daresso_King_of_Swords)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static Monster AnyUniqueMob => LokiPoe.ObjectManager.Objects
            .Closest<Monster>(m => m.Rarity == Rarity.Unique && m.IsActive);

        public static void Tick()
        {
        }

        public static async Task<bool> KillDaresso()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.EyeOfDesire))
                return false;

            if (World.Act4.GrandArena.IsCurrentArea)
            {
                var daresso = Daresso;
                if (daresso != null && daresso.PathExists())
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.Daresso))
                        return true;

                    await Helpers.MoveAndWait(daresso);
                    return true;
                }
                // Gladiators
                var mob = AnyUniqueMob;
                if (mob != null && mob.PathExists())
                {
                    await Helpers.MoveAndWait(mob);
                    return true;
                }
                await Helpers.Explore();
                return true;
            }
            if (World.Act4.DaressoDream.IsCurrentArea)
            {
                if (await TrackMobLogic.Execute(100))
                    return true;

                await Travel.To(World.Act4.GrandArena);
                return true;
            }
            await Travel.To(World.Act4.GrandArena);
            return true;
        }

        public static async Task<bool> TurnInQuest()
        {
            if (!Helpers.PlayerHasQuestItem(QuestItemMetadata.EyeOfDesire))
                return false;

            if (World.Act4.CrystalVeins.IsCurrentArea)
            {
                var dialla = Helpers.LadyDialla;
                if (dialla == null)
                {
                    GlobalLog.Error("[KingOfDesire] Fail to detect Lady Dialla in Crystal Veins.");
                    ErrorManager.ReportCriticalError();
                    return true;
                }
                await Helpers.TalkTo(dialla);
                return true;
            }
            await Travel.To(World.Act4.CrystalVeins);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act4.Highgate,
                TownNpcs.Dialla,
                "Rapture Reward",
                Quests.EternalNightmare.Id);
        }
    }
}