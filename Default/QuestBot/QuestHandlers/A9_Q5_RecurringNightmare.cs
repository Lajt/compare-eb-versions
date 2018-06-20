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
    public static class A9_Q5_RecurringNightmare
    {     
        private static Npc _sin;
        private static Npc _lilly;
        private static Monster _uniqueMob;
        private static AreaTransition _blackHeart;
        private static bool _trioIsDead;

        private static Monster Basilisk => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.The_Basilisk)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static Monster Adus => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.General_Adus)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static Chest Theurgic => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Theurgic_Precipitate_Machine)
            .FirstOrDefault<Chest>();

        private static bool IsFightEnded
        {
            get
            {
                var lilly = LokiPoe.ObjectManager.Objects
                    .FirstOrDefault<Npc>(n => n.Metadata == "Metadata/NPC/Act9/Lilly");

                return lilly != null && lilly.PathExists();
            }
        }

        private static WalkablePosition CachedBasiliskPos
        {
            get => CombatAreaCache.Current.Storage["BasiliskPosition"] as WalkablePosition;
            set => CombatAreaCache.Current.Storage["BasiliskPosition"] = value;
        }

        private static WalkablePosition CachedAdusPos
        {
            get => CombatAreaCache.Current.Storage["AdusPosition"] as WalkablePosition;
            set => CombatAreaCache.Current.Storage["AdusPosition"] = value;
        }

        private static CachedObject CachedTheurgic
        {
            get => CombatAreaCache.Current.Storage["Theurgic"] as CachedObject;
            set => CombatAreaCache.Current.Storage["Theurgic"] = value;
        }

        public static bool HaveAnyIngredient => Inventories.InventoryItems
            .Exists(i => i.Class == ItemClasses.QuestItem && i.Metadata.Contains("Ingredient"));

        public static void Tick()
        {
            if (World.Act9.BoilingLake.IsCurrentArea)
            {
                var basilisk = Basilisk;
                if (basilisk != null)
                {
                    CachedBasiliskPos = basilisk.IsDead ? null : basilisk.WalkablePosition();
                }
            }
            if (World.Act9.Refinery.IsCurrentArea)
            {
                if (CachedTheurgic == null)
                {
                    var theurgic = Theurgic;
                    if (theurgic != null)
                    {
                        CachedTheurgic = new CachedObject(theurgic);
                    }
                }
                var adus = Adus;
                if (adus != null)
                {
                    CachedAdusPos = adus.IsDead ? null : adus.WalkablePosition();
                }
            }
        }

        public static async Task<bool> GrabAcid()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.BasiliskAcid))
                return false;

            if (World.Act9.BoilingLake.IsCurrentArea)
            {
                var basiliskPos = CachedBasiliskPos;
                if (basiliskPos != null)
                {
                    basiliskPos.Come();
                    return true;
                }
                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act9.BoilingLake);
            return true;
        }

        public static async Task<bool> GrabPowder()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.TrarthanPowder))
                return false;

            if (World.Act9.Refinery.IsCurrentArea)
            {
                var adusPos = CachedAdusPos;
                if (adusPos != null)
                {
                    adusPos.Come();
                    return true;
                }

                if (await Helpers.OpenQuestChest(CachedTheurgic))
                    return true;

                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act9.Refinery);
            return true;
        }

        public static async Task<bool> TurnInIngredient()
        {
            if (!HaveAnyIngredient)
                return false;

            if (World.Act9.Highgate.IsCurrentArea)
            {
                if (await Helpers.Sin_A9.Talk())
                {
                    await Coroutines.CloseBlockingWindows();
                }
                else
                {
                    ErrorManager.ReportError();
                }
                return true;
            }
            await Travel.To(World.Act9.Highgate);
            return true;
        }

        public static async Task<bool> KillTrinity()
        {
            if (World.Act9.RottingCore.IsCurrentArea)
            {
                UpdateTrinityObjects();

                if (_lilly != null && _lilly.PathExists())
                {
                    var hash = LokiPoe.LocalData.AreaHash;
                    if (!await _lilly.AsTownNpc().Converse("Sail to Oriath"))
                    {
                        ErrorManager.ReportError();
                        return true;
                    }
                    await Coroutines.CloseBlockingWindows();
                    await Wait.ForAreaChange(hash);
                    return true;
                }
                if (_trioIsDead)
                {
                    if (!await Wait.For(() => IsFightEnded, "Waiting for Depraved Trinity fight ending", 500, 7000))
                    {
                        ErrorManager.ReportError();

                        if (World.Act10.OriathDocks.IsWaypointOpened)
                            return false;
                    }
                    return true;
                }
                if (_blackHeart != null)
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.DepravedTrinity))
                        return true;

                    if (_blackHeart.IsTargetable)
                    {
                        if (!await PlayerAction.TakeTransition(_blackHeart))
                            ErrorManager.ReportError();

                        return true;
                    }
                }
                if (_sin != null && _sin.HasNpcFloatingIcon)
                {
                    if (!await _sin.AsTownNpc().Talk())
                    {
                        ErrorManager.ReportError();
                        return true;
                    }
                    await Coroutines.CloseBlockingWindows();
                    await Wait.SleepSafe(500);

                    var t = LokiPoe.ObjectManager.Objects.Closest<AreaTransition>(a => a.Metadata.Contains("UnholyTrioSubBossPortal"));
                    if (t != null && !t.IsTargetable)
                    {
                        await Wait.For(() => _sin.Fresh().HasNpcFloatingIcon, "Next Sin's dialogue", 500, 7000);
                    }
                    else
                    {
                        var state = QuestManager.GetStateInaccurate(Quests.RecurringNightmare);
                        if (state == 4 || state == 5)
                            await Wait.For(() => _blackHeart.Fresh().IsTargetable, "Black Heart activation", 500, 10000);
                    }
                    return true;
                }
                if (_uniqueMob != null && _uniqueMob.PathExists())
                {
                    await Helpers.MoveAndWait(_uniqueMob);
                    return true;
                }
                await Helpers.Explore();
                return true;
            }
            if (World.Act10.OriathDocks.IsCurrentArea)
            {
                if (World.Act10.OriathDocks.IsWaypointOpened)
                    return false;

                if (!await PlayerAction.OpenWaypoint())
                    ErrorManager.ReportError();

                return true;
            }
            await Travel.To(World.Act9.RottingCore);
            return true;
        }

        private static void UpdateTrinityObjects()
        {
            _sin = null;
            _lilly = null;
            _uniqueMob = null;
            _blackHeart = null;
            _trioIsDead = false;

            foreach (var obj in LokiPoe.ObjectManager.Objects)
            {
                var metadata = obj.Metadata;

                var mob = obj as Monster;
                if (mob != null && mob.Rarity == Rarity.Unique && mob.Reaction != Reaction.Friendly)
                {
                    if (mob.IsDead)
                    {
                        if (metadata == "Metadata/Monsters/UnholyTrio/UnholyTrio")                   
                            _trioIsDead = true;                   
                    }
                    else
                    {
                        // Doedre goes hidden instead of dying
                        if (mob.IsHidden && metadata.Contains("Doedre/DoedreSoul"))
                            continue;

                        _uniqueMob = mob;
                    }
                    continue;
                }
                var transition = obj as AreaTransition;
                if (transition != null && metadata == "Metadata/QuestObjects/Act9/HarvestFinalBossTransition")
                {
                    _blackHeart = transition;
                    continue;
                }
                var npc = obj as Npc;
                if (npc != null)
                {
                    if (metadata == "Metadata/NPC/Act9/SinCore")
                    {
                        _sin = npc;
                    }
                    else if (metadata == "Metadata/NPC/Act9/Lilly")
                    {
                        _lilly = npc;
                    }
                }
            }
        }
    }
}