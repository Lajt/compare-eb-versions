using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Bot;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A5_Q7_RavenousGod
    {
        private static readonly TgtPosition TombTgt = new TgtPosition("Tomb of the First Templar location", "ossuary_quest_v01_01.tgt");
        private static readonly TgtPosition KivataRoomTgt = new TgtPosition("Kitava room", "cathedralroof_boss_area_v02_01_c5r10.tgt");

        private static readonly WalkablePosition KitavaWalkablePos = new WalkablePosition("Walkable position in front of Kitava", 1730, 3120);
        private static readonly WalkablePosition LillyRothPos = new WalkablePosition("Lilly Roth", 704, 2543);

        private static NetworkObject _cradle;
        private static Monster _kitava;
        private static Monster _kitavaHeart;

        private static bool _kitavaKilled;

        private static Npc Bannon => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Bannon)
            .FirstOrDefault<Npc>();

        private static Npc LillyRoth => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Lilly_Roth)
            .FirstOrDefault<Npc>();

        private static Chest Tomb => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Tomb_of_the_First_Templar)
            .FirstOrDefault<Chest>();

        private static CachedObject CachedTomb
        {
            get => CombatAreaCache.Current.Storage["FirstTemplarTomb"] as CachedObject;
            set => CombatAreaCache.Current.Storage["FirstTemplarTomb"] = value;
        }

        public static void Tick()
        {
            _kitavaKilled = World.Act5.CathedralRooftop.IsWaypointOpened;

            if (World.Act5.Ossuary.IsCurrentArea)
            {
                if (CachedTomb == null)
                {
                    var tomb = Tomb;
                    if (tomb != null)
                    {
                        CachedTomb = new CachedObject(tomb);
                    }
                }
            }
        }

        public static async Task<bool> TalkToBannonAndGetToSquare()
        {
            if (World.Act5.RuinedSquare.IsCurrentArea)
                return false;

            if (World.Act5.ChamberOfInnocence.IsCurrentArea)
            {
                var bannon = Bannon;
                if (bannon != null && bannon.HasNpcFloatingIcon)
                {
                    var bannonPos = bannon.WalkablePosition();
                    if (bannonPos.IsFar)
                    {
                        bannonPos.Come();
                        return true;
                    }
                    await Helpers.TalkTo(bannon);
                    return true;
                }
            }
            await Travel.To(World.Act5.RuinedSquare);
            return true;
        }

        public static async Task<bool> GrabSignOfPurity()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.SignOfPurity))
                return false;

            if (World.Act5.Ossuary.IsCurrentArea)
            {
                if (await Helpers.OpenQuestChest(CachedTomb))
                    return true;

                TombTgt.Come();
                return true;
            }
            await Travel.To(World.Act5.Ossuary);
            return true;
        }

        public static async Task<bool> KillKitava()
        {
            if (_kitavaKilled)
                return false;

            if (World.Act5.CathedralRooftop.IsCurrentArea)
            {
                UpdateKitavaFightObjects();

                if (_cradle != null && _cradle.PathExists())
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.Kitava1))
                        return true;

                    if (_cradle.IsTargetable)
                    {
                        await _cradle.WalkablePosition().ComeAtOnce();

                        if (!await PlayerAction.Interact(_cradle, () => !_cradle.Fresh().IsTargetable, "Cradle of Purity interaction"))
                            ErrorManager.ReportError();

                        return true;
                    }
                    if (_kitavaHeart != null && _kitavaHeart.IsTargetable)
                    {
                        await Helpers.MoveAndWait(_kitavaHeart.WalkablePosition());
                        return true;
                    }
                    if (_kitava != null)
                    {
                        if (!_kitava.IsActive)
                        {
                            await Helpers.MoveAndWait(KitavaWalkablePos, "Waiting for Kitava, the Insatiable");
                        }
                        else
                        {
                            KitavaWalkablePos.Come();
                        }
                    }
                    return true;
                }
                await Helpers.MoveAndTakeLocalTransition(KivataRoomTgt);
                return true;
            }
            await Travel.To(World.Act5.CathedralRooftop);
            return true;
        }

        public static async Task<bool> SailToWraeclast()
        {
            if (World.Act6.LioneyeWatch.IsCurrentArea)
            {
                if (World.Act6.LioneyeWatch.IsWaypointOpened)
                    return false;

                if (!await PlayerAction.OpenWaypoint())
                    ErrorManager.ReportError();

                return true;
            }
            if (World.Act5.CathedralRooftop.IsCurrentArea)
            {
                if (!LillyRothPos.PathExists)
                {
                    GlobalLog.Debug("Waiting for Kitava fight ending");
                    await Wait.StuckDetectionSleep(500);
                    return true;
                }

                await LillyRothPos.ComeAtOnce();

                var lilly = LillyRoth;
                if (lilly == null)
                {
                    GlobalLog.Error("[SailToWraeclast] We are near Lilly Roth position but NPC object is null.");
                    await Wait.StuckDetectionSleep(500);
                    return true;
                }
                var hash = LokiPoe.LocalData.AreaHash;
                if (!await lilly.AsTownNpc().Converse("Sail to Wraeclast"))
                {
                    ErrorManager.ReportError();
                    return true;
                }
                await Coroutines.CloseBlockingWindows();
                await Wait.ForAreaChange(hash);
                return true;
            }
            await Travel.To(World.Act5.CathedralRooftop);
            return true;
        }

        private static void UpdateKitavaFightObjects()
        {
            _cradle = null;
            _kitava = null;
            _kitavaHeart = null;

            foreach (var obj in LokiPoe.ObjectManager.Objects)
            {
                var mob = obj as Monster;
                if (mob != null && mob.Rarity == Rarity.Unique)
                {
                    var name = mob.Name;
                    if (name == "Kitava, the Insatiable")
                    {
                        _kitava = mob;
                    }
                    else if (name == "Kitava's Heart")
                    {
                        _kitavaHeart = mob;
                    }
                    continue;
                }
                if (obj.Metadata == "Metadata/Terrain/Act5/Area8/Objects/ArenaSocket")
                {
                    _cradle = obj;
                }
            }
        }
    }
}