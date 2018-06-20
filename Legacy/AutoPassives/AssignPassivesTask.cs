using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Buddy.Coroutines;
using log4net;
using Loki.Bot;
using Loki.Common;
using Loki.Game;
using Loki.Game.Objects;
using Message = Loki.Bot.Message;

namespace Legacy.AutoPassives
{
	public class AssignPassivesTask : ITask
	{
		private static readonly ILog Log = Logger.GetLoggerInstanceForType();
		private readonly WaitTimer _levelWait = WaitTimer.FiveSeconds;
		private bool _skip;

		/// <summary>The name of this task.</summary>
		public string Name => "AssignPassivesTask";

		/// <summary>A description of what this task does.</summary>
		public string Description => "This task will assign passives as points are available.";

		/// <summary>The author of this task.</summary>
		public string Author => "Bossland GmbH";

		/// <summary>The version of this task.</summary>
		public string Version => "0.0.1.1";

		/// <summary>
		/// Opens the skills panel and will NOT close the skill reset dialog if present.
		/// </summary>
		/// <returns>True on success and false on failure.</returns>
		public static async Task<bool> OpenSkillsUi(int timeout = 5000)
		{
			Log.DebugFormat("[OpenSkillsUi]");

			var sw = Stopwatch.StartNew();

			// Make sure we close all blocking windows so we can actually open the inventory.
			if (!LokiPoe.InGameState.SkillsUi.IsOpened)
			{
				await Coroutines.CloseBlockingWindows();
			}

			// Open the passive skills panel like a player would.
			while (!LokiPoe.InGameState.SkillsUi.IsOpened)
			{
				Log.DebugFormat("[OpenSkillsUi] The SkillsPanel is not opened. Now opening it.");

				if (sw.ElapsedMilliseconds > timeout)
				{
					Log.ErrorFormat("[OpenSkillsUi] Timeout.");
					return false;
				}

				if (LokiPoe.Me.IsDead)
				{
					Log.ErrorFormat("[OpenSkillsUi] We are now dead.");
					return false;
				}

				LokiPoe.Input.SimulateKeyEvent(LokiPoe.Input.Binding.open_passive_skills_panel, true, false, false);

				await Coroutines.ReactionWait();
			}

			return true;
		}

		/// <summary>
		/// Coroutine logic to execute.
		/// </summary>
		/// <returns>true if logic was executed to handle this type and false otherwise.</returns>
		public async Task<bool> Run()
		{
			// NOTE: This task's Run function is triggered from "hook_post_combat" Logic, as it's added via a secondary TaskManager!

			if (_skip)
			{
				return false;
			}

			// Don't level passives if we're dead.
			if (LokiPoe.Me.IsDead)
				return false;

			// Don't try to level passives if we only want to allocate in town, and are not currently in town.
			if (AutoPassivesSettings.Instance.OnlyAllocateInTown && !(LokiPoe.Me.IsInTown || LokiPoe.Me.IsInHideout))
				return false;

			// Try to not allocate if there's a monster somewhat close, will avoid lots of issues.
			// More lightweight check to just get an idea of what is around us, rather than the heavy IsActive.
			if (
				LokiPoe.ObjectManager.GetObjectsByType<Monster>()
					.Any(m => m.IsAliveHostile && m.Distance < 100))
				return false;

			// Only check for passive leveling at a fixed interval.
			if (!_levelWait.IsFinished)
				return false;

			if (LokiPoe.InstanceInfo.PassiveSkillPointsAvailable == 0)
			{
				_levelWait.Reset(TimeSpan.FromMilliseconds(LokiPoe.Random.Next(5000, 10000)));

				// Close the window to prevent other issues.
				if (LokiPoe.InGameState.SkillsUi.IsOpened)
				{
					Log.DebugFormat("[AssignPassivesTask] The SkillsPanel is open. Now closing it to avoid issues.");
					await Coroutines.CloseBlockingWindows();
					return true;
				}

				return false;
			}

			var pendingPassives =
				AutoPassivesSettings.Instance.Passives.Where(p => !LokiPoe.InstanceInfo.PassiveSkillIds.Contains(p.Id)).ToList();
			if (!pendingPassives.Any())
			{
				_levelWait.Reset(TimeSpan.FromMilliseconds(LokiPoe.Random.Next(5000, 10000)));

				// Close the window to prevent other issues.
				if (LokiPoe.InGameState.SkillsUi.IsOpened)
				{
					Log.DebugFormat("[AssignPassivesTask] The SkillsPanel is open. Now closing it to avoid issues.");
					await Coroutines.CloseBlockingWindows();
					return true;
				}

				return false;
			}

			if (!await OpenSkillsUi())
			{
				Log.ErrorFormat("[AssignPassivesTask] OpenSkillsUi failed.");
				await Coroutines.CloseBlockingWindows();
				return true;
			}

			// Handle the passive tree reset by closing the dialog.
			if (LokiPoe.InGameState.GlobalWarningDialog.IsPassiveTreeWarningOverlayOpen)
			{
				Log.DebugFormat("[AssignPassivesTask] IsPassiveTreeWarningOverlayOpen. Now attempting to close it.");
				LokiPoe.Input.SimulateKeyEvent(Keys.Escape, true, false, false);
				await Coroutine.Sleep(250);
				return true;
			}

			// Make sure we don't screw up the user's character ;)
			if (LokiPoe.InGameState.SkillsUi.IsResetAllPassivesEnabled && LokiPoe.InGameState.SkillsUi.IsResetAllPassivesVisible)
			{
				Log.ErrorFormat(
					"[AssignPassivesTask] The \"Reset All Passives\" button is available. Please reset your passive tree, or manually allocate a point first.");
				_skip = true;
				LokiPoe.Input.SimulateKeyEvent(Keys.Escape, true, false, false);
				await Coroutine.Sleep(250);
				return true;
			}

			var passive = pendingPassives.First();

			var error = true;

			var ret = LokiPoe.InGameState.SkillsUi.ChoosePassive(passive.Id);
			if (ret == LokiPoe.InGameState.ChoosePassiveError.None)
			{
				var ret2 = LokiPoe.InGameState.SkillsUi.ConfirmOperation();
				if (ret2 == LokiPoe.InGameState.PassiveAllocationActionError.None)
				{
					Log.InfoFormat("[AssignPassivesTask] The passive {0} was assigned.", passive.Id);
					error = false;
				}
				else
				{
					Log.ErrorFormat("[AssignPassivesTask] ConfirmOperation returned {0} for {1}.", ret2, passive.Id);
				}
			}
			else
			{
				Log.ErrorFormat("[AssignPassivesTask] ChoosePassive returned {0} for {1}.", ret, passive.Id);
			}

			// If there was an error, stop trying to allocate passives until user intervention.
			if (error)
			{
				_skip = true;
				return true;
			}

			// If we have no more points, then reset the delay, otherwise, try to keep allocating the next time it executes.
			if (LokiPoe.InstanceInfo.PassiveSkillPointsAvailable == 0)
			{
				_levelWait.Reset(TimeSpan.FromMilliseconds(LokiPoe.Random.Next(5000, 10000)));
				await Coroutines.CloseBlockingWindows();
			}

			await Coroutine.Sleep(LokiPoe.Random.Next(750, 1250));

			return true;
		}

		/// <summary>The bot Start event.</summary>
		public void Start()
		{
			_skip = false;
		}

		/// <summary>The bot Tick event.</summary>
		public void Tick()
		{
		}

		/// <summary>The bot Stop event.</summary>
		public void Stop()
		{
		}

		#region Implementation of IMessageHandler

		/// <summary>
		/// Implements logic to handle a message passed through the system.
		/// </summary>
		/// <param name="message">The message to be processed.</param>
		/// <returns>A tuple of a MessageResult and object.</returns>
		public MessageResult Message(Message message)
		{
			return MessageResult.Unprocessed;
		}

		#endregion

		#region Implementation of ILogicProvider

		/// <summary>
		/// Implements the ability to handle a logic passed through the system.
		/// </summary>
		/// <param name="logic">The logic to be processed.</param>
		/// <returns>A LogicResult that describes the result..</returns>
		public async Task<LogicResult> Logic(Logic logic)
		{
			return LogicResult.Unprovided;
		}

		#endregion
		
	}
}