using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A3_Q1_LostInLove
    {
        private static readonly WalkablePosition ClarissaPos = new WalkablePosition("Clarissa position", 560, 1278);
        private static readonly TgtPosition TolmanTgt = new TgtPosition("Tolman location", "quest_marker.tgt");

        private static WalkablePosition _guardCaptainPos;
        private static WalkablePosition _anyClarissaGuardPos;
        private static WalkablePosition _pietyPos;
        private static WalkablePosition _anyTolmanGuardPos;

        private static Npc Clarissa => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Clarissa)
            .FirstOrDefault<Npc>();

        private static Monster GuardCaptain => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Guard_Captain)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique && !m.IsDead);

        private static Monster Piety => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Piety)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static Chest Tolman => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Tolman)
            .FirstOrDefault<Chest>();

        private static Monster TolmanGuard => LokiPoe.ObjectManager.Objects
            .FirstOrDefault<Monster>(m => !m.IsDead && m.Metadata.Contains("GuardTolman"));

        private static Monster ClarissaGuard => LokiPoe.ObjectManager.Objects
            .FirstOrDefault<Monster>(m => !m.IsDead && m.Metadata.Contains("GuardClarissa"));

        private static CachedObject CachedTolman
        {
            get => CombatAreaCache.Current.Storage["Tolman"] as CachedObject;
            set => CombatAreaCache.Current.Storage["Tolman"] = value;
        }

        public static void Tick()
        {
            if (World.Act3.CityOfSarn.IsCurrentArea)
            {
                _guardCaptainPos = GuardCaptain?.WalkablePosition();
                _anyClarissaGuardPos = ClarissaGuard?.WalkablePosition();
                return;
            }
            if (World.Act3.Crematorium.IsCurrentArea)
            {
                _pietyPos = Piety?.WalkablePosition();
                _anyTolmanGuardPos = TolmanGuard?.WalkablePosition();

                if (CachedTolman == null)
                {
                    var tolman = Tolman;
                    if (tolman != null)
                    {
                        CachedTolman = new CachedObject(tolman);
                    }
                }
            }
        }

        public static async Task<bool> EnterSarnEncampment()
        {
            if (World.Act3.SarnEncampment.IsCurrentArea)
            {
                if (World.Act3.SarnEncampment.IsWaypointOpened)
                    return false;

                if (!await PlayerAction.OpenWaypoint())
                    ErrorManager.ReportError();

                return true;
            }
            await Travel.To(World.Act3.SarnEncampment);
            return true;
        }

        public static async Task<bool> FreeClarissa()
        {
            if (World.Act3.CityOfSarn.IsCurrentArea)
            {
                if (_guardCaptainPos != null)
                {
                    _guardCaptainPos.Come();
                    return true;
                }
                if (_anyClarissaGuardPos != null)
                {
                    _anyClarissaGuardPos.Come();
                    return true;
                }

                var clarissa = Clarissa;
                if (clarissa != null)
                {
                    var clarissaPos = clarissa.WalkablePosition();
                    if (clarissaPos.IsFar)
                    {
                        clarissaPos.Come();
                        return true;
                    }
                    if (!clarissa.IsTargetable)
                    {
                        GlobalLog.Debug("Waiting for Clarissa");
                        await Wait.StuckDetectionSleep(200);
                        return true;
                    }
                    if (clarissa.HasNpcFloatingIcon)
                    {
                        await Helpers.TalkTo(clarissa);
                        return true;
                    }

                    if (QuestManager.GetState(Quests.LostInLove) < 10)
                        return false;

                    return true;
                }
                ClarissaPos.Come();
                return true;
            }
            await Travel.To(World.Act3.CityOfSarn);
            return true;
        }

        public static async Task<bool> GrabBracelet()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.TolmanBracelet))
                return false;

            if (World.Act3.Crematorium.IsCurrentArea)
            {
                if (_pietyPos != null)
                {
                    await Helpers.MoveAndWait(_pietyPos);
                    return true;
                }
                if (_anyTolmanGuardPos != null)
                {
                    _anyTolmanGuardPos.Come();
                    return true;
                }

                if (await Helpers.OpenQuestChest(CachedTolman))
                    return true;

                TolmanTgt.Come();
                return true;
            }
            await Travel.To(World.Act3.Crematorium);
            return true;
        }

        public static async Task<bool> TakeClarissaReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act3.SarnEncampment,
                TownNpcs.Clarissa,
                "Take Sewer Keys");
        }

        public static async Task<bool> TakeMaramoaReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act3.SarnEncampment,
                TownNpcs.Maramoa,
                "Clarissa Reward",
                Quests.LostInLove.Id);
        }
    }
}