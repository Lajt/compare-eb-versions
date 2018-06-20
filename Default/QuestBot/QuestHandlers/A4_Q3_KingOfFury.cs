using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A4_Q3_KingOfFury
    {
        private static readonly TgtPosition KaomRoomTgt = new TgtPosition("Kaom room", "lava_lake_throne_room_v0?_0?_c1r2.tgt | lava_lake_throne_room_v0?_0?_c3r2.tgt");

        private static Monster Kaom => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.King_Kaom)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        public static void Tick()
        {
        }

        public static async Task<bool> KillKaom()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.EyeOfFury))
                return false;

            if (World.Act4.KaomStronghold.IsCurrentArea)
            {
                var kaom = Kaom;
                if (kaom != null)
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.Kaom))
                        return true;

                    if (kaom.PathExists())
                    {
                        await Helpers.MoveAndWait(kaom);
                        return true;
                    }
                }
                await Helpers.MoveAndTakeLocalTransition(KaomRoomTgt);
                return true;
            }
            await Travel.To(World.Act4.KaomStronghold);
            return true;
        }

        public static async Task<bool> TurnInQuest()
        {
            if (!Helpers.PlayerHasQuestItem(QuestItemMetadata.EyeOfFury))
                return false;

            if (World.Act4.CrystalVeins.IsCurrentArea)
            {
                var dialla = Helpers.LadyDialla;
                if (dialla == null)
                {
                    GlobalLog.Error("[KingOfFury] Fail to detect Lady Dialla in Crystal Veins.");
                    ErrorManager.ReportCriticalError();
                    return true;
                }
                await Helpers.TalkTo(dialla);
                return true;
            }
            await Travel.To(World.Act4.CrystalVeins);
            return true;
        }
    }
}