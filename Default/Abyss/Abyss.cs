using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Loki.Bot;
using Loki.Common;
using Loki.Game;
using Loki.Game.Objects;
using Loki.Game.Objects.Components;

namespace Default.Abyss
{
    public class Abyss : IPlugin, IStartStopEvents, ITickEvents
    {
        private static readonly Interval TickInterval = new Interval(200);

        public static AbyssData CachedData
        {
            get
            {
                var data = CombatAreaCache.Current.Storage["AbyssData"] as AbyssData;
                if (data == null)
                {
                    data = new AbyssData();
                    CombatAreaCache.Current.Storage["AbyssData"] = data;
                }

                return data;
            }
        }

        public void Start()
        {
            var taskManager = BotStructure.TaskManager;
            taskManager.AddAfter(new StartAbyssTask(), "OpenChestTask");
            taskManager.AddBefore(new FollowAbyssTask(), "PostCombatHookTask");
            taskManager.AddBefore(new OpenAbyssChestTask(), "FollowAbyssTask");
        }

        public void Tick()
        {
            if (!TickInterval.Elapsed)
                return;

            if (!LokiPoe.IsInGame)
                return;

            var area = World.CurrentArea;

            if (!area.IsCombatArea || area.IsAbyssArea)
                return;

            var cachedData = CachedData;

            foreach (var obj in LokiPoe.ObjectManager.Objects)
            {
                if (obj is AbyssStartNode)
                {
                    ProcessAbyssStartNode(obj, cachedData.StartNodes);
                    continue;
                }

                if (obj is Chest chest && obj.Metadata.Contains("AbyssFinalChest"))
                {
                    ProcessAbyssChest(chest, cachedData.Chests);
                    continue;
                }

                var icon = obj.Components.MinimapIconComponent;
                if (icon != null && IsAbyssIcon(icon))
                {
                    if (icon.IsVisible)
                    {
                        if (cachedData.MapIconOwner == null || cachedData.MapIconOwner.Id != obj.Id)
                            cachedData.MapIconOwner = new CachedObject(obj);
                    }
                    else
                    {
                        if (cachedData.MapIconOwner?.Id == obj.Id)
                            cachedData.MapIconOwner = null;
                    }
                }
                else
                {
                    if (cachedData.MapIconOwner?.Id == obj.Id)
                        cachedData.MapIconOwner = null;
                }
            }
        }

        private static void ProcessAbyssStartNode(NetworkObject obj, List<CachedObject> list)
        {
            var trans = obj.Components.TransitionableComponent;

            if (trans == null)
                return;

            var id = obj.Id;
            var isIntact = trans.Flag2 == 1;
            var index = list.FindIndex(n => n.Id == id);

            if (index >= 0)
            {
                if (!isIntact)
                {
                    list.RemoveAt(index);

                    if (StartAbyssTask.StartNode?.Id == id)
                        StartAbyssTask.StartNode = null;
                }
            }
            else
            {
                if (isIntact)
                {
                    var pos = obj.WalkablePosition();
                    GlobalLog.Warn($"[Abyss] Registering {pos}");
                    list.Add(new CachedObject(id, pos));
                }
            }
        }

        private static void ProcessAbyssChest(Chest chest, List<CachedObject> list)
        {
            var id = chest.Id;
            var isOpened = chest.IsOpened;
            var index = list.FindIndex(n => n.Id == id);

            if (index >= 0)
            {
                if (isOpened)
                {
                    list.RemoveAt(index);

                    if (OpenAbyssChestTask.AbyssChest?.Id == id)
                        OpenAbyssChestTask.AbyssChest = null;
                }
            }
            else
            {
                if (!isOpened)
                {
                    var pos = chest.WalkablePosition();
                    GlobalLog.Warn($"[Abyss] Registering {pos}");
                    list.Add(new CachedObject(id, pos));
                }
            }
        }

        private static bool IsAbyssIcon(MinimapIconComponent component)
        {
            var name = component.MinimapIcon?.Name;
            return name == "Abyss" || name == "AbyssCrack";
        }

        public class AbyssData
        {
            public CachedObject MapIconOwner;
            public readonly List<CachedObject> StartNodes = new List<CachedObject>();
            public readonly List<CachedObject> Chests = new List<CachedObject>();
        }

        public MessageResult Message(Message message)
        {
            if (message.Id == ComplexExplorer.LocalTransitionEnteredMessage)
            {
                GlobalLog.Info("[Abyss] Resetting unwalkable flags.");

                var cachedData = CachedData;

                foreach (var startNode in cachedData.StartNodes)
                {
                    startNode.Unwalkable = false;
                }

                foreach (var chest in cachedData.Chests)
                {
                    chest.Unwalkable = false;
                }

                return MessageResult.Processed;
            }

            return MessageResult.Unprocessed;
        }

        #region Unused interface methods

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

        public void Deinitialize()
        {
        }

        public void Enable()
        {
        }

        public void Disable()
        {
        }

        public string Name => "Abyss";
        public string Description => "Plugin that handles Abyss cracks.";
        public string Author => "ExVault";
        public string Version => "1.1";
        public JsonSettings Settings => null;
        public UserControl Control => null;

        #endregion
    }
}