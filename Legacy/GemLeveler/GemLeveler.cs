using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;
using log4net;
using Loki.Bot;
using Loki.Common;
using Loki.Game;
using Loki.Game.Objects;

namespace Legacy.GemLeveler
{
	internal class GemLeveler : IPlugin, ITickEvents
	{
		private static readonly ILog Log = Logger.GetLoggerInstanceForType();

		private Gui _instance;

		private bool _ranOnce;

		/// <summary> The name of the plugin. </summary>
		public string Name => "GemLeveler";

		/// <summary> The description of the plugin. </summary>
		public string Description => "A plugin to automatically level skill gems.";

		/// <summary>The author of the plugin.</summary>
		public string Author => "Bossland GmbH";

		/// <summary>The version of the plugin.</summary>
		public string Version => "0.0.1.1";

		/// <summary>Initializes this plugin.</summary>
		public void Initialize()
		{
			BotManager.PostStart += BotManagerOnPostStart;
		}

		private void BotManagerOnPostStart(IBot bot)
		{
			Reset();
		}

		/// <summary>Deinitializes this object. This is called when the object is being unloaded from the bot.</summary>
		public void Deinitialize()
		{
			BotManager.PostStart -= BotManagerOnPostStart;
		}

		/// <summary> The plugin tick callback. Do any update logic here. </summary>
		public void Tick()
		{
			// Don't update while we are not in the game.
			if (!LokiPoe.IsInGame)
				return;

			// Update the current skillgems once per major event.
			if (!_ranOnce)
			{
				// This can trigger gui code from a non-gui thread, so we need to run it on a gui thread.
				LokiPoe.BeginDispatchIfNecessary(new Action(() => GemLevelerSettings.Instance.RefreshSkillGemsList()));
				_ranOnce = true;
				return;
			}
		}

		private void Reset()
		{
			Log.DebugFormat("[GemLeveler] Now resetting task state.");
			_ranOnce = false;
		}

		#region Implementation of IEnableable

		/// <summary>Called when the task should be enabled.</summary>
		public void Enable()
		{
		}

		/// <summary>Called when the task should be disabled.</summary>
		public void Disable()
		{
		}

		#endregion

		#region Implementation of IConfigurable

		/// <summary>The settings object. This will be registered in the current configuration.</summary>
		public JsonSettings Settings => GemLevelerSettings.Instance;

		/// <summary> The plugin's settings control. This will be added to the Exilebuddy Settings tab.</summary>
		public UserControl Control => (_instance ?? (_instance = new Gui()));

		#endregion

		#region Implementation of ILogicProvider

		/// <summary>
		/// Implements the ability to handle a logic passed through the system.
		/// </summary>
		/// <param name="logic">The logic to be processed.</param>
		/// <returns>A LogicResult that describes the result..</returns>
		public async Task<LogicResult> Logic(Logic logic)
		{
			if (logic.Id == "hook_post_combat")
			{
				// Don't update while we are not in the game.
				if (!LokiPoe.IsInGame)
					return LogicResult.Unprovided;

				// When the game is paused, don't try to run.
				if (LokiPoe.InstanceInfo.IsGamePaused)
					return LogicResult.Unprovided;

				// Don't try to do anything when the escape state is active.
				if (LokiPoe.StateManager.IsEscapeStateActive)
					return LogicResult.Unprovided;

				// Don't level skill gems if we're dead.
				if (LokiPoe.Me.IsDead)
					return LogicResult.Unprovided;

				// Can't level skill gems under this scenario either.
				if (LokiPoe.InGameState.IsTopMostOverlayActive)
					return LogicResult.Unprovided;

				// Can't level skill gems under this scenario either.
				if (LokiPoe.InGameState.SkillsUi.IsOpened)
					return LogicResult.Unprovided;

				// Only check for skillgem leveling at a fixed interval.
				if (!_needsToUpdate && !_levelWait.IsFinished)
					return LogicResult.Unprovided;

				Func<Inventory, Item, Item, bool> eval = (inv, holder, gem) =>
				{
					// Ignore any "globally ignored" gems. This just lets the user move gems around
					// equipment, without having to worry about where or what it is.
					if (ContainsHelper(gem.Name))
					{
						if (GemLevelerSettings.Instance.DebugStatements)
						{
							Log.DebugFormat("[LevelSkillGemTask] {0} => {1}.", gem.Name, "GlobalNameIgnoreList");
						}
						return false;
					}

					// Now look though the list of skillgem strings to level, and see if the current gem matches any of them.
					var ss = string.Format("{0} [{1}: {2}]", gem.Name, inv.PageSlot, holder.GetSocketIndexOfGem(gem));
					foreach (var str in GemLevelerSettings.Instance.SkillGemsToLevelList)
					{
						if (str.Equals(ss, StringComparison.OrdinalIgnoreCase))
						{
							if (GemLevelerSettings.Instance.DebugStatements)
							{
								Log.DebugFormat("[LevelSkillGemTask] {0} => {1}.", gem.Name, str);
							}
							return true;
						}
					}

					// No match, we shouldn't level this gem.
					return false;
				};

				// If we have icons on the hud to process.
				if (LokiPoe.InGameState.SkillGemHud.AreIconsDisplayed)
				{
					// If the InventoryUi is already opened, skip this logic and let the next set run.
					if (!LokiPoe.InGameState.InventoryUi.IsOpened)
					{
						// We need to close blocking windows.
						await Coroutines.CloseBlockingWindows();

						// We need to let skills finish casting, because of 2.6 changes.
						await Coroutines.FinishCurrentAction();
						await Coroutines.LatencyWait();
						await Coroutines.ReactionWait();

						var res = LokiPoe.InGameState.SkillGemHud.HandlePendingLevelUps(eval);

						Log.InfoFormat("[LevelSkillGemTask] SkillGemHud.HandlePendingLevelUps returned {0}.", res);

						if (res == LokiPoe.InGameState.HandlePendingLevelUpResult.GemDismissed ||
							res == LokiPoe.InGameState.HandlePendingLevelUpResult.GemLeveled)
						{
							await Coroutines.LatencyWait();

							await Coroutines.ReactionWait();

							return LogicResult.Provided;
						}
					}
				}

				if (_needsToUpdate || LokiPoe.InGameState.InventoryUi.IsOpened)
				{
					if (LokiPoe.InGameState.InventoryUi.IsOpened)
					{
						_needsToCloseInventory = false;
					}
					else
					{
						_needsToCloseInventory = true;
					}

					// We need the inventory panel open.
					if (!await OpenInventoryPanel())
					{
						Log.ErrorFormat("[LevelSkillGemTask] OpenInventoryPanel failed.");
						return LogicResult.Provided;
					}

					// If we have icons on the inventory ui to process.
					// This is only valid when the inventory panel is opened.
					if (LokiPoe.InGameState.InventoryUi.AreIconsDisplayed)
					{
						var res = LokiPoe.InGameState.InventoryUi.HandlePendingLevelUps(eval);

						Log.InfoFormat("[LevelSkillGemTask] InventoryUi.HandlePendingLevelUps returned {0}.", res);

						if (res == LokiPoe.InGameState.HandlePendingLevelUpResult.GemDismissed ||
							res == LokiPoe.InGameState.HandlePendingLevelUpResult.GemLeveled)
						{
							await Coroutines.LatencyWait();

							await Coroutines.ReactionWait();

							return LogicResult.Provided;
						}
					}
				}

				// Just wait 5-10s between checks.
				_levelWait.Reset(TimeSpan.FromMilliseconds(LokiPoe.Random.Next(5000, 10000)));

				//if (_needsToCloseInventory)
				{
					await Coroutines.CloseBlockingWindows();
					_needsToCloseInventory = false;
				}

				_needsToUpdate = false;
			}

			return LogicResult.Unprovided;
		}

		#endregion

		#region Implementation of IMessageHandler

		/// <summary>
		/// Implements logic to handle a message passed through the system.
		/// </summary>
		/// <param name="message">The message to be processed.</param>
		/// <returns>A tuple of a MessageResult and object.</returns>
		public MessageResult Message(Message message)
		{
			var handled = false;
			if (message.Id == "area_changed_event") // Event ids based on EXtensions now
			{
				Reset();
				handled = true;
			}
			else if (message.Id == "player_leveled_event") // Event ids based on EXtensions now
			{
				_needsToUpdate = true;
				handled = true;
			}

			return handled ? MessageResult.Processed : MessageResult.Unprocessed;
		}

		#endregion

		#region Override of Object

		/// <summary>Returns a string that represents the current object.</summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString()
		{
			return Name + ": " + Description;
		}

		#endregion

		#region Old Task Code

		/// <summary>
		/// Opens the inventory panel.
		/// </summary>
		/// <returns></returns>
		public static async Task<bool> OpenInventoryPanel(int timeout = 5000)
		{
			Log.DebugFormat("[OpenInventoryPanel]");

			var sw = Stopwatch.StartNew();

			// Make sure we close all blocking windows so we can actually open the inventory.
			if (!LokiPoe.InGameState.InventoryUi.IsOpened)
			{
				await Coroutines.CloseBlockingWindows();
			}

			// Open the inventory panel like a player would.
			while (!LokiPoe.InGameState.InventoryUi.IsOpened)
			{
				Log.DebugFormat("[OpenInventoryPanel] The InventoryUi is not opened. Now opening it.");

				if (sw.ElapsedMilliseconds > timeout)
				{
					Log.ErrorFormat("[OpenInventoryPanel] Timeout.");
					return false;
				}

				if (LokiPoe.Me.IsDead)
				{
					Log.ErrorFormat("[OpenInventoryPanel] We are now dead.");
					return false;
				}

				LokiPoe.Input.SimulateKeyEvent(LokiPoe.Input.Binding.open_inventory_panel, true, false, false);

				await Coroutines.ReactionWait();
			}

			return true;
		}

		private readonly WaitTimer _levelWait = WaitTimer.FiveSeconds;
		private bool _needsToUpdate = true;
		private bool _needsToCloseInventory;

		private bool ContainsHelper(string name)
		{
			foreach (var entry in GemLevelerSettings.Instance.GlobalNameIgnoreList)
			{
				if (entry.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		#endregion
	}
}