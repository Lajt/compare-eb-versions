using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A2_Q7_ShadowOfVaal
    {
        private static TriggerableBlockage DarkAltar => LokiPoe.ObjectManager.Objects
            .FirstOrDefault<TriggerableBlockage>(t => t.Metadata == "Metadata/Monsters/IncaShadowBoss/IncaBossSpawner");

        private static Monster VaalOversoul => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Vaal_Oversoul)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static bool _vaalKilled;

        public static void Tick()
        {
            _vaalKilled = World.Act3.CityOfSarn.IsWaypointOpened;
        }

        public static async Task<bool> KillVaal()
        {
            if (_vaalKilled)
            {
                // tp to town to skip lengthy transition opening
                if (World.Act2.ForestEncampment.IsCurrentArea)
                    return false;

                await Travel.To(World.Act2.ForestEncampment);
                return true;
            }

            if (World.Act2.AncientPyramid.IsCurrentArea)
            {
                var darkAltar = DarkAltar;
                if (darkAltar != null)
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.VaalOversoul))
                        return true;

                    if (!darkAltar.IsOpened)
                    {
                        await darkAltar.WalkablePosition().ComeAtOnce();

                        if (!await PlayerAction.Interact(darkAltar, () => darkAltar.Fresh().IsOpened, "Dark Altar opening"))
                            ErrorManager.ReportError();

                        return true;
                    }
                    var vaal = VaalOversoul;
                    if (vaal != null)
                    {
                        await Helpers.MoveToBossOrAnyMob(vaal);
                        return true;
                    }
                    await Helpers.MoveAndWait(darkAltar.WalkablePosition(), "Waiting for Vaal Oversoul");
                    return true;
                }
            }
            await Travel.To(World.Act3.CityOfSarn);
            return true;
        }
    }
}