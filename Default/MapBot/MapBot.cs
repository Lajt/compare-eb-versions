using System.Collections.Generic;
using System.Threading.Tasks;
using Buddy.Coroutines;
using Default.EXtensions;
using Default.EXtensions.CommonTasks;
using Default.EXtensions.Global;
using log4net;
using Loki.Bot;
using Loki.Bot.Pathfinding;
using Loki.Common;
using Loki.Game;
using Newtonsoft.Json.Linq;
using UserControl = System.Windows.Controls.UserControl;

namespace Default.MapBot
{
    public class MapBot : IBot, ITaskManagerHolder, IUrlProvider
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        private Gui _gui;
        private Coroutine _coroutine;

        private readonly TaskManager _taskManager = new TaskManager();

        internal static bool IsOnRun;

        public void Start()
        {
            ItemEvaluator.Instance = DefaultItemEvaluator.Instance;
            Explorer.CurrentDelegate = user => CombatAreaCache.Current.Explorer.BasicExplorer;

            ComplexExplorer.ResetSettingsProviders();
            ComplexExplorer.AddSettingsProvider("MapBot", MapBotExploration, ProviderPriority.Low);

            // Cache all bound keys.
            LokiPoe.Input.Binding.Update();

            // Reset the default MsBetweenTicks on start.
            Log.Debug($"[Start] MsBetweenTicks: {BotManager.MsBetweenTicks}.");
            Log.Debug($"[Start] NetworkingMode: {LokiPoe.ConfigManager.NetworkingMode}.");
            Log.Debug($"[Start] KeyPickup: {LokiPoe.ConfigManager.KeyPickup}.");
            Log.Debug($"[Start] IsAutoEquipEnabled: {LokiPoe.ConfigManager.IsAutoEquipEnabled}.");

            // Since this bot will be performing client actions, we need to enable the process hook manager.
            LokiPoe.ProcessHookManager.Enable();

            _coroutine = null;

            ExilePather.Reload();

            _taskManager.Reset();

            AddTasks();

            Events.Start();
            PluginManager.Start();
            RoutineManager.Start();
            PlayerMoverManager.Start();
            _taskManager.Start();

            foreach (var plugin in PluginManager.EnabledPlugins)
            {
                Log.Debug($"[Start] The plugin {plugin.Name} is enabled.");
            }

            if (ExilePather.BlockTrialOfAscendancy == FeatureEnum.Unset)
            {
                //no need for this, map trials are in separate areas
                ExilePather.BlockTrialOfAscendancy = FeatureEnum.Disabled;
            }
        }

        public void Tick()
        {
            if (_coroutine == null)
            {
                _coroutine = new Coroutine(() => MainCoroutine());
            }

            ExilePather.Reload();

            Events.Tick();
            CombatAreaCache.Tick();
            _taskManager.Tick();
            PluginManager.Tick();
            RoutineManager.Tick();
            PlayerMoverManager.Tick();
            StuckDetection.Tick();

            Statistics.Instance.Tick();

            // Check to see if the coroutine is finished. If it is, stop the bot.
            if (_coroutine.IsFinished)
            {
                Log.Debug($"The bot coroutine has finished in a state of {_coroutine.Status}");
                BotManager.Stop();
                return;
            }

            try
            {
                _coroutine.Resume();
            }
            catch
            {
                var c = _coroutine;
                _coroutine = null;
                c.Dispose();
                throw;
            }
        }

        public void Stop()
        {
            _taskManager.Stop();
            PluginManager.Stop();
            RoutineManager.Stop();
            PlayerMoverManager.Stop();

            // When the bot is stopped, we want to remove the process hook manager.
            LokiPoe.ProcessHookManager.Disable();

            // Cleanup the coroutine.
            if (_coroutine != null)
            {
                _coroutine.Dispose();
                _coroutine = null;
            }
        }

        private async Task MainCoroutine()
        {
            while (true)
            {
                if (LokiPoe.IsInLoginScreen)
                {
                    // Offload auto login logic to a plugin.
                    var logic = new Logic("hook_login_screen", this);
                    foreach (var plugin in PluginManager.EnabledPlugins)
                    {
                        if (await plugin.Logic(logic) == LogicResult.Provided)
                            break;
                    }
                }
                else if (LokiPoe.IsInCharacterSelectionScreen)
                {
                    // Offload character selection logic to a plugin.
                    var logic = new Logic("hook_character_selection", this);
                    foreach (var plugin in PluginManager.EnabledPlugins)
                    {
                        if (await plugin.Logic(logic) == LogicResult.Provided)
                            break;
                    }
                }
                else if (LokiPoe.IsInGame)
                {
                    // To make things consistent, we once again allow user coroutine logic to preempt the bot base coroutine logic.
                    // This was supported to a degree in 2.6, and in general with our bot bases. Technically, this probably should
                    // be at the top of the while loop, but since the bot bases offload two sets of logic to plugins this way, this
                    // hook is being placed here.
                    var hooked = false;
                    var logic = new Logic("hook_ingame", this);
                    foreach (var plugin in PluginManager.EnabledPlugins)
                    {
                        if (await plugin.Logic(logic) == LogicResult.Provided)
                        {
                            hooked = true;
                            break;
                        }
                    }
                    if (!hooked)
                    {
                        // Wait for game pause
                        if (LokiPoe.InstanceInfo.IsGamePaused)
                        {
                            Log.Debug("Waiting for game pause");
                            await Wait.StuckDetectionSleep(200);
                        }
                        // Resurrect character if it is dead
                        else if (LokiPoe.Me.IsDead)
                        {
                            await ResurrectionLogic.Execute();
                        }
                        // What the bot does now is up to the registered tasks.
                        else
                        {
                            await _taskManager.Run(TaskGroup.Enabled, RunBehavior.UntilHandled);
                        }
                    }
                }
                else
                {
                    // Most likely in a loading screen, which will cause us to block on the executor, 
                    // but just in case we hit something else that would cause us to execute...
                    await Coroutine.Sleep(1000);
                    continue;
                }

                // End of the tick.
                await Coroutine.Yield();
            }
            // ReSharper disable once FunctionNeverReturns
        }

        public MessageResult Message(Message message)
        {
            var handled = false;
            var id = message.Id;

            if (id == BotStructure.GetTaskManagerMessage)
            {
                message.AddOutput(this, _taskManager);
                handled = true;
            }
            else if (id == Messages.GetIsOnRun)
            {
                message.AddOutput(this, IsOnRun);
                handled = true;
            }
            else if (id == Messages.SetIsOnRun)
            {
                var value = message.GetInput<bool>();
                GlobalLog.Info($"[MapBot] SetIsOnRun: {value}");
                IsOnRun = value;
                handled = true;
            }
            else if (id == Messages.GetMapSettings)
            {
                message.AddOutput(this, JObject.FromObject(MapSettings.Instance.MapDict));
                handled = true;
            }

            Events.FireEventsFromMessage(message);

            var res = _taskManager.SendMessage(TaskGroup.Enabled, message);
            if (res == MessageResult.Processed)
                handled = true;

            return handled ? MessageResult.Processed : MessageResult.Unprocessed;
        }

        public async Task<LogicResult> Logic(Logic logic)
        {
            return await _taskManager.ProvideLogic(TaskGroup.Enabled, RunBehavior.UntilHandled, logic);
        }

        public TaskManager GetTaskManager()
        {
            return _taskManager;
        }

        public void Initialize()
        {
            BotManager.OnBotChanged += BotManagerOnOnBotChanged;
        }

        public void Deinitialize()
        {
            BotManager.OnBotChanged -= BotManagerOnOnBotChanged;
        }

        private void BotManagerOnOnBotChanged(object sender, BotChangedEventArgs botChangedEventArgs)
        {
            if (botChangedEventArgs.New == this)
            {
                ItemEvaluator.Instance = DefaultItemEvaluator.Instance;
            }
        }

        private void AddTasks()
        {
            _taskManager.Add(new ClearCursorTask());
            _taskManager.Add(new AssignMoveSkillTask());
            _taskManager.Add(new LeaveAreaTask());
            _taskManager.Add(new HandleBlockingChestsTask());
            _taskManager.Add(new HandleBlockingObjectTask());
            _taskManager.Add(new CombatTask(50));
            _taskManager.Add(new PostCombatHookTask());
            _taskManager.Add(new LootItemTask());
            _taskManager.Add(new SpecialObjectTask());
            _taskManager.Add(new OpenChestTask());
            _taskManager.Add(new CombatTask(-1));
            _taskManager.Add(new EnterTrialTask());
            _taskManager.Add(new IdTask());
            _taskManager.Add(new SellTask());
            _taskManager.Add(new StashTask());
            _taskManager.Add(new CurrencyRestockTask());
            _taskManager.Add(new SortInventoryTask());
            _taskManager.Add(new VendorTask());
            _taskManager.Add(new TravelToHideoutTask());
            _taskManager.Add(new SextantTask());
            _taskManager.Add(new SellMapTask());
            _taskManager.Add(new TakeMapTask());
            _taskManager.Add(new TravelToLabTask());
            _taskManager.Add(new CastAuraTask());
            _taskManager.Add(new OpenMapTask());
            _taskManager.Add(new DeviceAreaTask());
            _taskManager.Add(new ProximityTriggerTask());
            _taskManager.Add(new KillBossTask());
            _taskManager.Add(new TrackMobTask());
            _taskManager.Add(new TransitionTriggerTask());
            _taskManager.Add(new MapExplorationTask());
            _taskManager.Add(new FinishMapTask());
            _taskManager.Add(new FallbackTask());
        }

        private static ExplorationSettings MapBotExploration()
        {
            if (!World.CurrentArea.IsMap)
                return null;

            OnNewMapEnter();

            var data = MapData.Current;
            var type = data.Type;

            if (type == MapType.Regular || (type == MapType.Bossroom && data.IgnoredBossroom))
                return new ExplorationSettings(tileSeenRadius: TileSeenRadius);

            return new ExplorationSettings
            (
                basicExploration: false,
                openPortals: GeneralSettings.Instance.OpenPortals,
                backtracking: type == MapType.Complex,
                priorityTransition: type == MapType.Bossroom ? "Arena" : null,
                fastTransition: data.FastTransition.Value,
                tileSeenRadius: TileSeenRadius
            );
        }

        private static void OnNewMapEnter()
        {
            var areaName = World.CurrentArea.Name;
            Log.Info($"[MapBot] New map has been entered: {areaName}.");
            IsOnRun = true;
            MapData.ResetCurrent();
            Statistics.Instance.OnNewMapEnter();
            Utility.BroadcastMessage(null, Messages.NewMapEntered, areaName);
        }

        private static int TileSeenRadius
        {
            get
            {
                if (TileSeenDict.TryGetValue(World.CurrentArea.Name, out int radius))
                    return radius;

                return ExplorationSettings.DefaultTileSeenRadius;
            }
        }

        private static readonly Dictionary<string, int> TileSeenDict = new Dictionary<string, int>
        {
            [MapNames.MaoKun] = 3,
            [MapNames.Arena] = 3,
            [MapNames.CastleRuins] = 3,
            [MapNames.UndergroundRiver] = 3,
            [MapNames.TropicalIsland] = 3,

            [MapNames.Beach] = 5,
            [MapNames.Strand] = 5,
            [MapNames.Port] = 5,
            [MapNames.Gorge] = 5,
            [MapNames.Alleyways] = 5,
            [MapNames.AcidLakes] = 5,
            [MapNames.Phantasmagoria] = 5,

            [MapNames.Wharf] = 5,
            [MapNames.Cemetery] = 5,
            [MapNames.MineralPools] = 5,
            [MapNames.Temple] = 5,
            [MapNames.Malformation] = 5,
        };

        public static class Messages
        {
            public const string NewMapEntered = "MB_new_map_entered_event";
            public const string MapFinished = "MB_map_finished_event";
            public const string MapTrialEntered = "MB_map_trial_entered_event";
            public const string GetIsOnRun = "MB_get_is_on_run";
            public const string SetIsOnRun = "MB_set_is_on_run";
            public const string GetMapSettings = "MB_get_map_settings";
        }

        public string Name => "MapBot";
        public string Description => "Bot for running maps.";
        public string Author => "ExVault";
        public string Version => "1.0.3";
        public JsonSettings Settings => GeneralSettings.Instance;
        public UserControl Control => _gui ?? (_gui = new Gui());
        public override string ToString() => $"{Name}: {Description}";
        public string Url => "https://www.thebuddyforum.com/threads/mapbot-guide-support-discussion.295808/";
    }
}