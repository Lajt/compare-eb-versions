using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A2_Q3_GreatWhiteBeast
    {
        private static readonly TgtPosition DenExitTgt = new TgtPosition("Den exit", "forestcaveup_exit_v01_01_c1r2.tgt");

        private const int FinishedStateMinimum = 2;
        private static bool _finished;

        private static Monster GreatWhiteBeast => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.The_Great_White_Beast)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static WalkablePosition CachedWhiteBeastPos
        {
            get => CombatAreaCache.Current.Storage["WhiteBeastPosition"] as WalkablePosition;
            set => CombatAreaCache.Current.Storage["WhiteBeastPosition"] = value;
        }

        public static void Tick()
        {
            _finished = QuestManager.GetStateInaccurate(Quests.GreatWhiteBeast) <= FinishedStateMinimum;

            if (World.Act2.Den.IsCurrentArea)
            {
                var beast = GreatWhiteBeast;
                if (beast != null)
                {
                    CachedWhiteBeastPos = beast.WalkablePosition();
                }
            }
        }

        public static async Task<bool> KillWhiteBeast()
        {
            if (_finished)
                return false;

            if (World.Act2.Den.IsCurrentArea)
            {
                var beastPos = CachedWhiteBeastPos;
                if (beastPos != null)
                {
                    await Helpers.MoveAndWait(beastPos);
                    return true;
                }
                if (DenExitTgt.IsFar)
                {
                    DenExitTgt.Come();
                }
                else
                {
                    DenExitTgt.ProceedToNext();
                }
                return true;
            }
            await Travel.To(World.Act2.Den);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            if (World.Act2.Den.IsCurrentArea)
            {
                var transition = LokiPoe.ObjectManager.Objects.FirstOrDefault<AreaTransition>(a => a.LeadsTo(World.Act2.OldFields));
                if (transition != null)
                {
                    if (!await PlayerAction.TakeTransition(transition))
                        ErrorManager.ReportError();
                }
                else
                {
                    if (!await PlayerAction.TpToTown())
                        ErrorManager.ReportError();
                }
                return true;
            }
            if (World.Act2.OldFields.IsCurrentArea)
            {
                await Travel.To(World.Act2.Crossroads);
                return true;
            }
            return await Helpers.TakeQuestReward(
                World.Act2.ForestEncampment,
                TownNpcs.Yeena,
                "Great White Beast Reward",
                Quests.GreatWhiteBeast.Id);
        }
    }
}