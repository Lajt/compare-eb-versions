using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A8_Q1_EssenceOfHag
    {
        private static bool _doedreKilled;

        private static Monster Doedre => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Doedre_the_Vile)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static NetworkObject Valve => LokiPoe.ObjectManager.Objects
            .Find(o => o.Metadata == "Metadata/Monsters/Doedre/DoedreSewer/DoedreCauldronValve");

        public static void Tick()
        {
            _doedreKilled = World.Act8.DoedreCesspool.IsWaypointOpened;
        }

        public static async Task<bool> KillDoedre()
        {
            if (_doedreKilled)
                return false;

            if (World.Act8.DoedreCesspool.IsCurrentArea)
            {
                var valve = Valve;
                if (valve != null)
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.Doedre))
                        return true;

                    if (valve.IsTargetable)
                    {
                        await valve.WalkablePosition().ComeAtOnce();

                        if (!await PlayerAction.Interact(valve, () => !valve.Fresh().IsTargetable, "Valve interaction"))
                            ErrorManager.ReportError();

                        return true;
                    }
                    var doedre = Doedre;
                    if (doedre != null && doedre.PathExists())
                    {
                        await Helpers.MoveAndWait(doedre);
                        return true;
                    }
                    await Helpers.MoveAndWait(valve.WalkablePosition(), "Waiting for any Doedre fight object");
                    return true;
                }
            }
            await Travel.To(World.Act8.GrandPromenade);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act8.SarnEncampment,
                TownNpcs.Hargan_A8,
                "Doedre Reward",
                Quests.EssenceOfHag.Id);
        }
    }
}