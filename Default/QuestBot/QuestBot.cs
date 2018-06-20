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
using settings = Default.QuestBot.Settings;
using UserControl = System.Windows.Controls.UserControl;

namespace Default.QuestBot
{
    public class QuestBot : IBot, ITaskManagerHolder, IUrlProvider
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        private Gui _gui;
        private Coroutine _coroutine;

        private readonly TaskManager _taskManager = new TaskManager();

        public void Start()
        {
            ItemEvaluator.Instance = DefaultItemEvaluator.Instance;
            Explorer.CurrentDelegate = user => CombatAreaCache.Current.Explorer.BasicExplorer;

            ComplexExplorer.ResetSettingsProviders();
            ComplexExplorer.AddSettingsProvider("QuestBot", QuestBotExploration, ProviderPriority.Low);

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
                ExilePather.BlockTrialOfAscendancy = FeatureEnum.Enabled;
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
            else if (id == Events.Messages.IngameBotStart)
            {
                QuestManager.CompletedQuests.Instance.Verify();
                handled = true;
            }
            else if (message.Id == Events.Messages.PlayerDied)
            {
                int deathCount = message.GetInput<int>();
                GrindingHandler.OnPlayerDied(deathCount);
                handled = true;
            }
            else if (message.Id == "QB_get_current_quest")
            {
                var s = settings.Instance;
                message.AddOutputs(this, s.CurrentQuestName, s.CurrentQuestState);
                handled = true;
            }
            else if (message.Id == "QB_finish_grinding")
            {
                GlobalLog.Debug("[QuestBot] Grinding force finish: true");
                GrindingHandler.ForceFinish = true;
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
            _taskManager.Add(new TravelToHideoutTask());
            _taskManager.Add(new HandleBlockingChestsTask());
            _taskManager.Add(new HandleBlockingObjectTask());
            _taskManager.Add(new CombatTask(50));
            _taskManager.Add(new ReturnAfterDeathTask());
            _taskManager.Add(new PostCombatHookTask());
            _taskManager.Add(new LootItemTask());
            _taskManager.Add(new OpenChestTask());
            _taskManager.Add(new CombatTask(-1));
            _taskManager.Add(new IdTask());
            _taskManager.Add(new SellTask());
            _taskManager.Add(new StashTask());
            _taskManager.Add(new CurrencyRestockTask());
            _taskManager.Add(new SortInventoryTask());
            _taskManager.Add(new VendorTask());
            _taskManager.Add(new ReturnAfterTownrunTask());
            _taskManager.Add(new OpenWaypointTask());
            _taskManager.Add(new CorruptedAreaTask());
            _taskManager.Add(new QuestTask());
            _taskManager.Add(new FallbackTask());
        }

        private static ExplorationSettings QuestBotExploration()
        {
            var area = World.CurrentArea;

            if (!area.IsOverworldArea)
                return null;

            var areaId = area.Id;

            if (areaId == World.Act4.GrandArena.Id)
                return new ExplorationSettings(false, true, true, false, tileSeenRadius: 3);

            if (areaId == World.Act7.MaligaroSanctum.Id)
                return new ExplorationSettings(false, true);

            if (MultilevelAreas.Contains(areaId))
                return new ExplorationSettings(false, true, openPortals: false);

            return null;
        }

        private static readonly HashSet<string> MultilevelAreas = new HashSet<string>
        {
            World.Act2.AncientPyramid.Id,
            World.Act3.SceptreOfGod.Id,
            World.Act3.UpperSceptreOfGod.Id,
            World.Act6.PrisonerGate.Id,
            World.Act7.Crypt.Id,
            World.Act7.TempleOfDecay1.Id,
            World.Act7.TempleOfDecay2.Id,
            World.Act9.Descent.Id,
            World.Act9.Oasis.Id,
            World.Act9.RottingCore.Id,
            World.Act10.Ossuary.Id
        };

        public string Name => "QuestBot";
        public string Description => "Bot for doing quests.";
        public string Author => "ExVault";
        public string Version => "2.0.4";
        public JsonSettings Settings => settings.Instance;
        public UserControl Control => _gui ?? (_gui = new Gui());
        public override string ToString() => $"{Name}: {Description}";
        public string Url => "https://www.thebuddyforum.com/threads/questbot-2-0-guide-support-discussion.407087/";
    }
}