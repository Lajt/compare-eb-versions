using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Loki.Bot;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.EXtensions.CommonTasks
{
    public class OpenChestTask : ITask
    {
        private const int MaxChestAttempts = 3;
        private const int MaxStrongboxAttempts = 5;
        private const int MaxShrineAttempts = 5;
        private const int MaxSpecialChestAttempts = 10;
        private const float AbandonDistanceMult = 1.25f;

        private static readonly Settings Settings = Settings.Instance;

        private static readonly Interval TickInterval = new Interval(100);

        private static readonly Dictionary<int, Stopwatch> TemporaryIgnoredObjects = new Dictionary<int, Stopwatch>();
        private static readonly TimeSpan IgnoringTime = TimeSpan.FromSeconds(30);

        private static CachedObject _chest;
        private static CachedObject _specialChest;
        private static CachedObject _shrine;
        private static CachedStrongbox _strongbox;

        public async Task<bool> Run()
        {
            if (!World.CurrentArea.IsCombatArea)
                return false;

            var cache = CombatAreaCache.Current;

            if (Settings.ChestOpenRange != 0)
            {
                if (_chest != null)
                {
                    await ProcessChest();
                    return true;
                }
                var closestChest = cache.Chests.ClosestValid();
                if (closestChest != null && ShouldOpen(closestChest, Settings.ChestOpenRange, Settings.Chests))
                {
                    _chest = closestChest;
                    return true;
                }
            }
            if (Settings.ShrineOpenRange != 0)
            {
                if (_shrine != null)
                {
                    await ProcessShrine();
                    return true;
                }
                var closestShrine = cache.Shrines.ClosestValid();
                if (closestShrine != null && ShouldOpen(closestShrine, Settings.ShrineOpenRange, Settings.Shrines))
                {
                    _shrine = closestShrine;
                    return true;
                }
            }
            if (Settings.StrongboxOpenRange != 0)
            {
                if (_strongbox != null)
                {
                    await ProcessStrongbox();
                    return true;
                }
                var closestStrongbox = cache.Strongboxes.ClosestValid();
                if (closestStrongbox != null &&
                    closestStrongbox.Rarity <= Settings.MaxStrongboxRarity &&
                    ShouldOpen(closestStrongbox, Settings.StrongboxOpenRange, Settings.Strongboxes))
                {
                    _strongbox = closestStrongbox;
                    return true;
                }
            }

            if (_specialChest != null)
            {
                await ProcessSpecialChest();
                return true;
            }
            var closestSpecialChest = cache.SpecialChests.ClosestValid();
            if (closestSpecialChest != null)
            {
                _specialChest = closestSpecialChest;
                return true;
            }

            return false;
        }

        public void Tick()
        {
            if (!LokiPoe.IsInGame || !TickInterval.Elapsed)
                return;

            if (_chest != null)
            {
                var chestObj = _chest.Object as Chest;
                if (chestObj != null && chestObj.IsOpened)
                {
                    CombatAreaCache.Current.Chests.Remove(_chest);
                    _chest = null;
                }
            }
            if (_strongbox != null)
            {
                var boxObj = _strongbox.Object;
                if (boxObj != null && (boxObj.IsOpened || boxObj.IsLocked))
                {
                    CombatAreaCache.Current.Strongboxes.Remove(_strongbox);
                    _strongbox = null;
                }
            }
            if (_shrine != null)
            {
                var shrineObj = _shrine.Object as Shrine;
                if (shrineObj != null && shrineObj.IsDeactivated)
                {
                    CombatAreaCache.Current.Shrines.Remove(_shrine);
                    _shrine = null;
                }
            }
            if (_specialChest != null)
            {
                var chestObj = _specialChest.Object as Chest;
                if (chestObj != null && chestObj.IsOpened)
                {
                    CombatAreaCache.Current.SpecialChests.Remove(_specialChest);
                    _specialChest = null;
                }
            }
        }

        private static async Task ProcessChest()
        {
            var pos = _chest.Position;

            if (Settings.ChestOpenRange != -1)
            {
                //stop processing if other logic (combat/looting) moved us too far away from current chest
                //checking path distance is not an option because chest's position could be unwalkable
                if (pos.Distance > Settings.ChestOpenRange * AbandonDistanceMult)
                {
                    GlobalLog.Debug("[OpenChestTask] Abandoning current chest because its too far away.");

                    //we should ignore it for some time to prevent back and forth loop
                    TemporaryIgnore(_chest.Id);
                    _chest = null;
                    return;
                }
            }

            if (pos.IsFar)
            {
                if (!pos.TryCome())
                {
                    GlobalLog.Error($"[OpenChestTask] Fail to move to {pos}. Marking this chest as unwalkable.");
                    _chest.Unwalkable = true;
                    _chest = null;
                }
                return;
            }
            var chestObj = _chest.Object as Chest;
            if (chestObj == null || chestObj.IsOpened)
            {
                CombatAreaCache.Current.Chests.Remove(_chest);
                _chest = null;
                return;
            }
            var attempts = ++_chest.InteractionAttempts;
            if (attempts > MaxChestAttempts)
            {
                GlobalLog.Error("[OpenChestTask] All attempts to open a chest have been spent. Now ignoring it.");
                _chest.Ignored = true;
                _chest = null;
                return;
            }
            if (await PlayerAction.Interact(chestObj))
            {
                await Wait.LatencySleep();
                if (await Wait.For(() => chestObj.IsOpened, "chest opening", 50, 300))
                {
                    CombatAreaCache.Current.Chests.Remove(_chest);
                    _chest = null;
                }
                return;
            }
            await Wait.SleepSafe(300);
        }

        private static async Task ProcessStrongbox()
        {
            var pos = _strongbox.Position;

            if (Settings.StrongboxOpenRange != -1)
            {
                if (pos.Distance > Settings.StrongboxOpenRange * AbandonDistanceMult)
                {
                    GlobalLog.Debug("[OpenChestTask] Abandoning current strongbox because its too far away.");
                    TemporaryIgnore(_strongbox.Id);
                    _strongbox = null;
                    return;
                }
            }

            if (pos.IsFar)
            {
                if (!pos.TryCome())
                {
                    GlobalLog.Error($"[OpenChestTask] Fail to move to {pos}. Marking this strongbox as unwalkable.");
                    _strongbox.Unwalkable = true;
                    _strongbox = null;
                }
                return;
            }
            var boxObj = _strongbox.Object;
            if (boxObj == null || boxObj.IsOpened || boxObj.IsLocked)
            {
                CombatAreaCache.Current.Strongboxes.Remove(_strongbox);
                _strongbox = null;
                return;
            }
            var attempts = ++_strongbox.InteractionAttempts;
            if (attempts > MaxStrongboxAttempts)
            {
                GlobalLog.Error("[OpenChestTask] All attempts to open a strongbox have been spent. Now ignoring it.");
                _strongbox.Ignored = true;
                _strongbox = null;
                return;
            }
            if (await PlayerAction.Interact(boxObj))
            {
                await Wait.LatencySleep();
                if (await Wait.For(() => boxObj.IsLocked, "strongbox opening", 100, 400))
                {
                    CombatAreaCache.Current.Strongboxes.Remove(_strongbox);
                    _strongbox = null;
                }
                return;
            }
            await Wait.SleepSafe(400);
        }

        private static async Task ProcessShrine()
        {
            //check if current shrine was blacklisted by Combat Routine
            if (Blacklist.Contains(_shrine.Id))
            {
                GlobalLog.Error("[OpenChestTask] Current shrine was blacklisted from outside.");
                _shrine.Ignored = true;
                _shrine = null;
                return;
            }

            var pos = _shrine.Position;

            if (Settings.ShrineOpenRange != -1)
            {
                if (pos.Distance > Settings.ShrineOpenRange * AbandonDistanceMult)
                {
                    GlobalLog.Debug("[OpenChestTask] Abandoning current shrine because its too far away.");
                    TemporaryIgnore(_shrine.Id);
                    _shrine = null;
                    return;
                }
            }

            if (pos.IsFar)
            {
                if (!pos.TryCome())
                {
                    GlobalLog.Error($"[OpenChestTask] Fail to move to {pos}. Marking this shrine as unwalkable.");
                    _shrine.Unwalkable = true;
                    _shrine = null;
                }
                return;
            }
            var shrineObj = _shrine.Object as Shrine;
            if (shrineObj == null || shrineObj.IsDeactivated)
            {
                CombatAreaCache.Current.Shrines.Remove(_shrine);
                _shrine = null;
                return;
            }
            var attempts = ++_shrine.InteractionAttempts;
            if (attempts > MaxShrineAttempts)
            {
                GlobalLog.Error("[OpenChestTask] All attempts to take a shrine have been spent. Now ignoring it.");
                _shrine.Ignored = true;
                _shrine = null;
                return;
            }
            if (await PlayerAction.Interact(shrineObj))
            {
                await Wait.LatencySleep();
                if (await Wait.For(() => shrineObj.IsDeactivated, "shrine deactivation", 100, 400))
                {
                    CombatAreaCache.Current.Shrines.Remove(_shrine);
                    _shrine = null;
                }
                return;
            }
            await Wait.SleepSafe(400);
        }

        private static async Task ProcessSpecialChest()
        {
            var pos = _specialChest.Position;

            if (pos.IsFar)
            {
                if (!pos.TryCome())
                {
                    GlobalLog.Error($"[OpenChestTask] Fail to move to {pos}. Marking this special chest as unwalkable.");
                    _specialChest.Unwalkable = true;
                    _specialChest = null;
                }
                return;
            }
            var chestObj = _specialChest.Object as Chest;
            if (chestObj == null || chestObj.IsOpened)
            {
                CombatAreaCache.Current.SpecialChests.Remove(_specialChest);
                _specialChest = null;
                return;
            }
            var attempts = ++_specialChest.InteractionAttempts;
            if (attempts > MaxSpecialChestAttempts)
            {
                GlobalLog.Error($"[OpenChestTask] All attempts to open {pos.Name} have been spent. Now ignoring it.");
                _specialChest.Ignored = true;
                _specialChest = null;
                return;
            }
            if (await PlayerAction.Interact(chestObj))
            {
                await Wait.LatencySleep();
                if (await Wait.For(() => chestObj.IsOpened, $"{pos.Name} opening", 100, 400))
                {
                    CombatAreaCache.Current.SpecialChests.Remove(_specialChest);
                    _specialChest = null;
                }
                return;
            }
            await Wait.SleepSafe(400);
        }

        private static bool ShouldOpen(CachedObject obj, int openRange, List<Settings.ChestEntry> settingsList)
        {
            if (openRange != -1)
            {
                if (obj.Position.Distance > openRange) return false;
            }
            return !IsTemporaryIgnored(obj.Id) && settingsList.Where(c => !c.Enabled).All(c => c.Name != obj.Position.Name);
        }

        private static void TemporaryIgnore(int id)
        {
            TemporaryIgnoredObjects.Add(id, Stopwatch.StartNew());
        }

        private static bool IsTemporaryIgnored(int id)
        {
            if (TemporaryIgnoredObjects.TryGetValue(id, out var sw))
            {
                if (sw.Elapsed >= IgnoringTime)
                {
                    TemporaryIgnoredObjects.Remove(id);
                    return false;
                }
                return true;
            }
            return false;
        }

        public MessageResult Message(Message message)
        {
            if (message.Id == Events.Messages.AreaChanged)
            {
                _chest = null;
                _specialChest = null;
                _strongbox = null;
                _shrine = null;
                TemporaryIgnoredObjects.Clear();
                return MessageResult.Processed;
            }
            return MessageResult.Unprocessed;
        }

        #region Unused interface methods

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public string Name => "OpenChestTask";
        public string Description => "Task for opening chests, strongboxes and shrines.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}