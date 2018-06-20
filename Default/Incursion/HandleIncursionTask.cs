using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Loki.Bot;
using Loki.Game;
using Loki.Game.Objects;
using System.Linq;
using Default.EXtensions.Positions;

namespace Default.Incursion
{
    public class HandleIncursionTask : ITask
    {
        private const int MaxArchitectAttempts = 15;
        private const string IconUpgrade = "IncursionArchitectUpgrade";
        private const string IconChange = "IncursionArchitectReplace";

        private static readonly Interval TickInterval = new Interval(200);

        private static IEnumerable<NetworkObject> IncursionDoors => LokiPoe.ObjectManager.Objects
            .Where(o => o.Metadata == "Metadata/Terrain/Leagues/Incursion/Objects/ClosedDoorPast");

        private static IncursionData CachedIncursionData
        {
            get
            {
                var data = CombatAreaCache.Current.Storage["IncursionData"] as IncursionData;
                if (data == null)
                {
                    data = new IncursionData();
                    CombatAreaCache.Current.Storage["IncursionData"] = data;
                }
                return data;
            }
        }

        public async Task<bool> Run()
        {
            var area = World.CurrentArea;

            if (!area.IsMap && !area.IsOverworldArea)
                return false;

            var cache = CombatAreaCache.Current;

            if (!CombatAreaCache.IsInIncursion)
                return false;

            var settings = Incursion.CurrentRoomSettings;
            if (settings != null)
            {
                if (settings.PriorityAction == PriorityAction.Upgrading)
                {
                    if (await KillArchitect(IconUpgrade))
                        return true;
                }
                else if (settings.PriorityAction == PriorityAction.Changing)
                {
                    if (await KillArchitect(IconChange))
                        return true;
                }
            }

            if (await OpenIncursionDoor())
                return true;

            if (await TrackMobLogic.Execute())
                return true;

            if (await cache.Explorer.Execute())
                return true;

            if (await ExitIncursion())
                return true;

            GlobalLog.Debug("[HandleIncursionTask] Incursion is fully explored. Waiting for timer to run out...");
            await Wait.StuckDetectionSleep(200);
            return true;
        }

        public void Tick()
        {
            if (!CombatAreaCache.IsInIncursion)
                return;

            if (!TickInterval.Elapsed)
                return;

            if (!LokiPoe.IsInGame)
                return;

            var data = CachedIncursionData;

            DoorScan(data);
            ArchitectScan(data);
        }

        private static void DoorScan(IncursionData data)
        {
            foreach (var door in IncursionDoors)
            {
                var targetable = door.IsTargetable;
                var id = door.Id;
                var index = data.Doors.FindIndex(d => d.Id == id);

                if (index >= 0)
                {
                    if (!targetable)
                    {
                        GlobalLog.Info($"[HandleIncursionTask] Removing opened {door.WalkablePosition()}");
                        data.Doors.RemoveAt(index);
                    }
                }
                else
                {
                    if (targetable)
                    {
                        var pos = door.WalkablePosition();
                        GlobalLog.Warn($"[HandleIncursionTask] Registering {pos}");
                        data.Doors.Add(new CachedObject(id, pos));
                    }
                }
            }
        }

        private static void ArchitectScan(IncursionData data)
        {
            foreach (var icon in LokiPoe.InstanceInfo.MinimapIcons)
            {
                var iconName = icon.MinimapIcon.Name;

                if (iconName != IconUpgrade && iconName != IconChange)
                    continue;

                var id = icon.ObjectId;

                if (Blacklist.Contains(id))
                    continue;

                var settings = Incursion.CurrentRoomSettings;
                if (settings != null)
                {
                    if (settings.NoUpgrade && iconName == IconUpgrade)
                    {
                        GlobalLog.Warn("[HandleIncursionTask] Blacklisting upgrading architect according to settings.");
                        Blacklist.Add(id, TimeSpan.FromMinutes(10));
                        continue;
                    }
                    if (settings.NoChange && iconName == IconChange)
                    {
                        GlobalLog.Warn("[HandleIncursionTask] Blacklisting changing architect according to settings.");
                        Blacklist.Add(id, TimeSpan.FromMinutes(10));
                        continue;
                    }
                }

                var mob = icon.NetworkObject as Monster;

                if (mob == null)
                    continue;

                var isDead = mob.IsDead;
                var index = data.Architects.FindIndex(a => a.Id == id);

                if (index >= 0)
                {
                    if (isDead)
                    {
                        GlobalLog.Info($"[HandleIncursionTask] Removing dead {mob.WalkablePosition()}");
                        data.Architects.RemoveAt(index);
                    }
                    else
                    {
                        data.Architects[index].Position = mob.WalkablePosition();
                    }
                }
                else
                {
                    if (!isDead)
                    {
                        var pos = mob.WalkablePosition();
                        GlobalLog.Warn($"[HandleIncursionTask] Registering {pos}");
                        data.Architects.Add(new CachedArchitect(id, pos, iconName));
                    }
                }
            }
        }

        private static async Task<bool> OpenIncursionDoor()
        {
            if (!HasKey)
                return false;

            var door = CachedIncursionData.Doors.Find(d => !d.Unwalkable && !d.Ignored);

            if (door == null)
                return false;

            var pos = door.Position;
            if (pos.IsFar || pos.IsFarByPath)
            {
                if (!pos.TryCome())
                {
                    GlobalLog.Error($"[HandleIncursionTask] Fail to move to {pos}.");
                    door.Unwalkable = true;
                }
                return true;
            }

            var doorObj = door.Object;
            if (doorObj == null)
            {
                GlobalLog.Error("[HandleIncursionTask] Unexpected error. We are near cached incursion door, but actual object is null.");
                door.Ignored = true;
                return true;
            }

            var name = doorObj.Name;

            var attempts = ++door.InteractionAttempts;
            if (attempts > 3)
            {
                GlobalLog.Error($"[HandleIncursionTask] All attempts to open {name} have been spent.");
                door.Ignored = true;
                return true;
            }

            if (!doorObj.IsTargetable)
            {
                CachedIncursionData.Doors.Remove(door);
                return true;
            }

            if (await PlayerAction.Interact(doorObj, () => !doorObj.Fresh().IsTargetable, $"{name} interaction"))
                CachedIncursionData.Doors.Remove(door);

            return true;
        }

        private static async Task<bool> KillArchitect(string type)
        {
            var arch = CachedIncursionData.Architects.Find(a => !a.Unwalkable && !a.Ignored && a.Type == type);

            if (arch == null)
                return false;

            var pos = arch.Position;
            if (pos.IsFar)
            {
                if (!pos.TryCome())
                {
                    GlobalLog.Error($"[HandleIncursionTask] Fail to move to {pos}. Now marking it as unwalkable.");
                    arch.Unwalkable = true;
                }
                return true;
            }
            var attempts = ++arch.InteractionAttempts;
            if (attempts > MaxArchitectAttempts)
            {
                GlobalLog.Error($"[HandleIncursionTask] {pos.Name} was not killed. Now ignoring it.");
                arch.Ignored = true;
                return true;
            }
            await Coroutines.FinishCurrentAction();
            GlobalLog.Debug($"[HandleIncursionTask] Waiting for combat routine to kill the architect ({attempts}/{MaxArchitectAttempts})");
            await Wait.StuckDetectionSleep(200);
            return true;
        }

        private static async Task<bool> ExitIncursion()
        {
            var cachedPortal = CombatAreaCache.Current.AreaTransitions
                .Find(a => a.Type == TransitionType.Incursion && !a.Ignored && !a.Unwalkable);

            if (cachedPortal == null)
                return false;

            var pos = cachedPortal.Position;
            if (pos.IsFar || pos.IsFarByPath)
            {
                if (!pos.TryCome())
                {
                    GlobalLog.Error($"[HandleIncursionTask] Fail to move to {pos}.");
                    cachedPortal.Unwalkable = true;
                }
                return true;
            }

            var portalObj = cachedPortal.Object;
            if (portalObj == null)
            {
                GlobalLog.Error("[HandleIncursionTask] Unexpected error. We are near cached Time Portal, but actual object is null.");
                cachedPortal.Ignored = true;
                return true;
            }
            if (!portalObj.IsTargetable)
            {
                GlobalLog.Error("[HandleIncursionTask] Cannot exit incursion. Time Portal is not active.");
                cachedPortal.Ignored = true;
                return true;
            }
            var attempts = ++cachedPortal.InteractionAttempts;
            if (attempts > 5)
            {
                GlobalLog.Error("[HandleIncursionTask] All attempts to interact with Time Portal have been spent.");
                cachedPortal.Ignored = true;
                return true;
            }
            await PlayerAction.TakeTransition(portalObj);
            return true;
        }

        private static bool HasKey => Inventories.InventoryItems.Exists(i => i.Metadata == "Metadata/Items/Incursion/IncursionKey");

        private class IncursionData
        {
            public readonly List<CachedObject> Doors = new List<CachedObject>();
            public readonly List<CachedArchitect> Architects = new List<CachedArchitect>();
        }

        private class CachedArchitect : CachedObject
        {
            public string Type { get; }

            public CachedArchitect(int id, WalkablePosition pos, string type) : base(id, pos)
            {
                Type = type;
            }
        }

        #region Unused interface methods

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public MessageResult Message(Message message)
        {
            return MessageResult.Unprocessed;
        }

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public string Name => "HandleIncursionTask";
        public string Description => "Task that handles incursion areas.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}