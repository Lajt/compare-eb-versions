using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Bot;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;
using InventoryUi = Loki.Game.LokiPoe.InGameState.InventoryUi;

namespace Default.QuestBot
{
    public static class Helpers
    {
        public static Npc LadyDialla => LokiPoe.ObjectManager
            .GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Dialla, LokiPoe.ObjectManager.PoeObjectEnum.Lady_Dialla)
            .FirstOrDefault<Npc>();

        public static readonly TownNpc Sin_A9 = new TownNpc(new WalkablePosition("Sin", 184, 461));

        public static Monster ClosestActiveMob => LokiPoe.ObjectManager.Objects.Closest<Monster>(m => m.IsActive && !Blacklist.Contains(m.Id));

        public static bool PlayerHasQuestItem(string metadata)
        {
            return Inventories.InventoryItems.Exists(i => i.Class == ItemClasses.QuestItem && i.Metadata.ContainsIgnorecase(metadata));
        }

        public static bool PlayerHasQuestItemAmount(string metadata, int amount)
        {
            var count = Inventories.InventoryItems.Count(item => item.Class == ItemClasses.QuestItem && item.Metadata.ContainsIgnorecase(metadata));
            return count == amount;
        }

        public static async Task TalkTo(NetworkObject npc)
        {
            if (await npc.AsTownNpc().Talk())
            {
                await Coroutines.CloseBlockingWindows();
            }
            else
            {
                ErrorManager.ReportError();
            }
        }

        public static async Task MoveAndTakeLocalTransition(TgtPosition tgtPos)
        {
            if (tgtPos.IsFar)
            {
                tgtPos.Come();
                return;
            }
            var transition = LokiPoe.ObjectManager.Objects.Closest<AreaTransition>();
            if (transition == null)
            {
                GlobalLog.Warn("[MoveAndTakeLocalTransition] There is no area transition near tgt position.");
                tgtPos.ProceedToNext();
                return;
            }
            if (transition.TransitionType != TransitionTypes.Local)
            {
                GlobalLog.Warn("[MoveAndTakeLocalTransition] Area transition is not local.");
                tgtPos.ProceedToNext();
                return;
            }

            if (!await PlayerAction.TakeTransition(transition))
                ErrorManager.ReportError();
        }

        public static async Task MoveAndWait(WalkablePosition pos, string log = null, int distance = 20)
        {
            if (pos.Distance > distance)
            {
                pos.Come();
            }
            else
            {
                GlobalLog.Debug(log ?? $"Waiting for {pos.Name}");
                await Wait.StuckDetectionSleep(200);
            }
        }

        public static async Task MoveAndWait(NetworkObject obj, string log = null, int distance = 20)
        {
            var pos = obj.Position;
            if (LokiPoe.MyPosition.Distance(pos) > distance)
            {
                PlayerMoverManager.MoveTowards(pos);
            }
            else
            {
                GlobalLog.Debug(log ?? $"Waiting for {obj.Name}");
                await Wait.StuckDetectionSleep(200);
            }
        }

        public static async Task MoveToBossOrAnyMob(Monster boss)
        {
            if (!boss.IsActive)
            {
                var mob = LokiPoe.ObjectManager.Objects.Closest<Monster>(m => m.IsActive && !Blacklist.Contains(m.Id));
                if (mob != null)
                {
                    GlobalLog.Debug($"\"{boss.Name}\" is not targetable. Now going to the closest active monster.");
                    if (!PlayerMoverManager.MoveTowards(mob.Position))
                    {
                        GlobalLog.Error($"Fail to move towards \"{mob.Name}\" at {mob.Position}. Now blacklisting it.");
                        Blacklist.Add(mob.Id, TimeSpan.FromSeconds(10), "fail to move to");
                    }
                    return;
                }
            }
            await MoveAndWait(boss.WalkablePosition());
        }

        public static async Task<bool> OpenQuestChest(CachedObject chest)
        {
            if (chest == null)
                return false;

            var chestPos = chest.Position;
            if (chestPos.IsFar || chestPos.IsFarByPath)
            {
                chestPos.Come();
                return true;
            }

            var chestObj = (Chest) chest.Object;
            var name = chestObj.Name;

            if (!chestObj.IsOpened)
            {
                if (chestObj.IsTargetable)
                {
                    if (!await PlayerAction.Interact(chestObj, () => chestObj.Fresh().IsOpened, $"{name} opening"))
                        ErrorManager.ReportError();
                }
                else
                {
                    GlobalLog.Debug($"Waiting for {name}");
                    await Wait.StuckDetectionSleep(200);
                }
                return true;
            }
            GlobalLog.Debug($"{name} is opened. Waiting for quest progress.");
            await Wait.StuckDetectionSleep(500);
            return true;
        }

        public static async Task<bool> HandleQuestObject(CachedObject cachedObj)
        {
            if (cachedObj == null)
                return false;

            var objPos = cachedObj.Position;
            if (objPos.IsFar || objPos.IsFarByPath)
            {
                objPos.Come();
                return true;
            }

            var obj = cachedObj.Object;
            var name = obj.Name;

            if (obj.IsTargetable)
            {
                if (!await PlayerAction.Interact(obj, () => !obj.Fresh().IsTargetable, $"{name} interaction"))
                    ErrorManager.ReportError();
            }
            else
            {
                GlobalLog.Debug($"{name} is not targetable. Waiting for quest progress.");
                await Wait.StuckDetectionSleep(500);
            }
            return true;
        }

        public static async Task Explore()
        {
            if (!await CombatAreaCache.Current.Explorer.Execute())
            {
                var area = World.CurrentArea;
                var settings = Settings.Instance;
                GlobalLog.Error($"{area.Name} is fully explored but quest goal was not achieved ({settings.CurrentQuestName} - {settings.CurrentQuestState})");

                Travel.RequestNewInstance(area);

                if (!await PlayerAction.TpToTown())
                    ErrorManager.ReportError();
            }
        }

        public static async Task<bool> TakeQuestReward(AreaInfo area, TownNpc npc, string dialog, string questId = null, string book = null)
        {
            if (area.IsCurrentArea)
            {
                if (book != null && PlayerHasQuestItem(book))
                {
                    if (!await UseQuestItem(book))
                        ErrorManager.ReportError();
                }
                else
                {
                    var reward = questId != null ? Settings.Instance.GetRewardForQuest(questId) : null;

                    if (!await npc.TakeReward(reward, dialog))
                        ErrorManager.ReportError();
                }
                return false;
            }
            await Travel.To(area);
            return true;
        }

        public static async Task<bool> UseQuestItem(string metadata)
        {
            var item = Inventories.InventoryItems.Find(i => i.Class == ItemClasses.QuestItem && i.Metadata.ContainsIgnorecase(metadata));
            if (item == null)
            {
                GlobalLog.Error($"[UseQuestItem] Fail to find item with metadata \"{metadata}\" in inventory.");
                return false;
            }

            var pos = item.LocationTopLeft;
            var id = item.LocalId;
            var name = item.Name;

            GlobalLog.Debug($"[UseQuestItem] Now going to use \"{name}\".");

            if (!await Inventories.OpenInventory())
            {
                ErrorManager.ReportError();
                return false;
            }

            var err = InventoryUi.InventoryControl_Main.UseItem(id);
            if (err != UseItemResult.None)
            {
                GlobalLog.Error($"[UseQuestItem] Fail to use \"{name}\". Error: \"{err}\".");
                return false;
            }

            if (!await Wait.For(() => InventoryUi.InventoryControl_Main.Inventory.FindItemByPos(pos) == null, "quest item despawn", 100, 2000))
                return false;

            if (LokiPoe.InGameState.CursorItemOverlay.Item != null)
            {
                GlobalLog.Error($"[UseQuestItem] Error. \"{name}\" has been picked to cursor.");
                return false;
            }
            await Wait.SleepSafe(500); //give a quest state time to update
            await Coroutines.CloseBlockingWindows();
            return true;
        }

        public static async Task<bool> StopBeforeBoss(string name)
        {
            if (Settings.Instance.StopBeforeBoss(name))
            {
                NotifyBoss(name);
                await Coroutines.FinishCurrentAction();
                await Wait.StuckDetectionSleep(1000);
                return true;
            }
            return false;
        }

        private static void NotifyBoss(string name)
        {
            if (!NotifyBossTimer.IsRunning || NotifyBossTimer.ElapsedMilliseconds > 60000)
            {
                NotifyBossTimer.Restart();

                GlobalLog.Warn($"Doing nothing because {name} is marked for manual kill");

                if (Settings.Instance.NotifyBoss)
                    LokiPoe.BotWindow.Dispatcher.Invoke(MakeTopmost);
            }
        }

        private static void MakeTopmost()
        {
            var w = LokiPoe.BotWindow;

            w.Show();

            if (w.WindowState == WindowState.Minimized)
                w.WindowState = WindowState.Normal;

            w.Activate();
            w.Topmost = true;
            w.Topmost = false;
            w.Focus();
        }

        private static readonly Stopwatch NotifyBossTimer = new Stopwatch();
    }
}