using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A6_Q3_FatherOfWar
    {
        private static readonly TgtPosition TukohamaRoomTgt = new TgtPosition("Tukohama's Keep", "swamp_longbridge_v01_01_c1r2.tgt", true);

        private const int FinishedStateMinimum = 2;
        private static bool _finished;

        private static Monster DishonouredQueen => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.The_Dishonoured_Queen)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static Monster Tukohama => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Tukohama_Karui_God_of_War)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static Monster ClosestTukohamaTotem => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Tukohamas_Protection)
            .Closest<Monster>(m => m.IsTargetable && !m.IsDead);

        private static NetworkObject TukohamaRoomObj => LokiPoe.ObjectManager.Objects
            .Find(o => o.Metadata == "Metadata/MiscellaneousObjects/ArenaMiddle");

        private static AreaTransition TukohamaRoomNorthernExit => LokiPoe.ObjectManager.Objects
            .Where<AreaTransition>(t => t.Name == World.Act6.KaruiFortress.Name)
            .OrderByDescending(t => t.Position.Y)
            .FirstOrDefault();

        private static WalkablePosition CachedQueenPos
        {
            get => CombatAreaCache.Current.Storage["DishonouredQueenPosition"] as WalkablePosition;
            set => CombatAreaCache.Current.Storage["DishonouredQueenPosition"] = value;
        }

        public static void Tick()
        {
            _finished = QuestManager.GetStateInaccurate(Quests.FatherOfWar) <= FinishedStateMinimum;

            if (World.Act6.MudFlats.IsCurrentArea)
            {
                var queen = DishonouredQueen;
                if (queen != null)
                {
                    CachedQueenPos = queen.IsDead ? null : queen.WalkablePosition();
                }
            }
        }

        public static async Task<bool> GrabEyeOfConquest()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.EyeOfConquest))
                return false;

            if (World.Act6.MudFlats.IsCurrentArea)
            {
                var queenPos = CachedQueenPos;
                if (queenPos != null)
                {
                    await Helpers.MoveAndWait(queenPos);
                    return true;
                }
                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act6.MudFlats);
            return true;
        }

        public static async Task<bool> KillTukohama()
        {
            if (_finished)
                return false;

            if (World.Act6.KaruiFortress.IsCurrentArea)
            {
                var roomObj = TukohamaRoomObj;
                if (roomObj != null)
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.Tukohama))
                        return true;

                    var totem = ClosestTukohamaTotem;
                    if (totem != null)
                    {
                        totem.WalkablePosition().Come();
                        return true;
                    }
                    var tukohama = Tukohama;
                    if (tukohama != null)
                    {
                        await Helpers.MoveAndWait(tukohama.WalkablePosition());
                    }
                    return true;
                }
                await Helpers.MoveAndTakeLocalTransition(TukohamaRoomTgt);
                return true;
            }
            await Travel.To(World.Act6.KaruiFortress);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            if (World.Act6.KaruiFortress.IsCurrentArea)
            {
                var roomObj = TukohamaRoomObj;
                if (roomObj != null)
                {
                    if (!await PlayerAction.TakeTransition(TukohamaRoomNorthernExit))
                        ErrorManager.ReportError();
                }
                else
                {
                    await Travel.To(World.Act6.Ridge);
                }
                return true;
            }
            return await Helpers.TakeQuestReward(
                World.Act6.LioneyeWatch,
                TownNpcs.Tarkleigh_A6,
                "Tukohama Reward",
                book: QuestItemMetadata.BookTukohama);
        }
    }
}