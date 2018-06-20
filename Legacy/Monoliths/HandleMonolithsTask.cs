using System;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Loki.Bot;
using Loki.Common;
using Loki.Game;

namespace Legacy.Monoliths
{
	public class HandleMonolithsTask : ITask
	{
		private static readonly ILog Log = Logger.GetLoggerInstanceForType();

		private bool _skip;

		// This needs to be static so we have persistent data to use. Alternatively, we could host it elsewhere and pass it to the task, but
		// for this plugin, we'll do the former.
		internal static readonly AreaDataManager<MonolithData> MonolithDataManager =
			new AreaDataManager<MonolithData>(hash => new MonolithData(hash)) {DebugLogging = true};

		private MonolithCache _current;
		private int _moveErrors;
		private int _interactErrors;
		private int _interactAttempts;

		/// <summary>The name of this task.</summary>
		public string Name => "HandleMonolithsTask";

		/// <summary>A description of what this task does.</summary>
		public string Description => "This task will handle interacting with Monoliths.";

		/// <summary>The author of this task.</summary>
		public string Author => "Bossland GmbH";

		/// <summary>The version of this task.</summary>
		public string Version => "0.0.1.1";

		bool ShouldActivate(MonolithCache monolith)
		{
			// Cache and reuse this for performance reasons.
			if (monolith.Activate != null)
			{
				return monolith.Activate.Value;
			}

			var count = monolith.Essences.Count;

			if (MonolithsSettings.Instance.MinEssences != -1)
			{
				if (count < MonolithsSettings.Instance.MinEssences)
				{
					Log.DebugFormat(
						"[HandleMonolithsTask::ShouldActivate] The Monolith [{0}] will not be activated because it's essence count [{1}] is too small [{2}].",
						monolith.Id, count, MonolithsSettings.Instance.MinEssences);
					monolith.Activate = false;
					return false;
				}
			}

			if (MonolithsSettings.Instance.MaxEssences != -1)
			{
				if (count > MonolithsSettings.Instance.MaxEssences)
				{
					Log.DebugFormat(
						"[HandleMonolithsTask::ShouldActivate] The Monolith [{0}] will not be activated because it's essence count [{1}] is too large [{2}].",
						monolith.Id, count, MonolithsSettings.Instance.MaxEssences);
					monolith.Activate = false;
					return false;
				}
			}

			// If this monolith has a blacklisted essence, don't activate it.
			foreach (var entry in MonolithsSettings.Instance.BlacklistEssenceMetadata)
			{
				if(entry == null || string.IsNullOrEmpty(entry.Value))
					continue;

				var str = entry.Value.ToLowerInvariant();
				foreach (var essence in monolith.Essences)
				{
					if (essence.Metadata.ToLowerInvariant().Contains(str))
					{
						Log.DebugFormat(
							"[HandleMonolithsTask::ShouldActivate] The Monolith [{0}] will not be activated because it's essence [{1}] is blacklisted [{2}].",
							monolith.Id, essence.Metadata, entry.Value);
						monolith.Activate = false;
						return false;
					}
				}
			}

			// If this monolith has a blacklisted monster, don't activate it.
			foreach (var entry in MonolithsSettings.Instance.BlacklistMonsterMetadata)
			{
				if (entry == null || string.IsNullOrEmpty(entry.Value))
					continue;

				var str = entry.Value.ToLowerInvariant();
				if (monolith.MonsterMetadata.ToLowerInvariant().Contains(str))
				{
					Log.DebugFormat(
						"[HandleMonolithsTask] The Monolith [{0}] will not be activated because it's monster [{1}] is blacklisted [{2}].",
						monolith.Id, monolith.MonsterMetadata, entry.Value);
					monolith.Activate = false;
					return false;
				}
			}

			// If this monolith has a whitelisted essence, activate it.
			foreach (var entry in MonolithsSettings.Instance.WhitelistEssenceMetadata)
			{
				if (entry == null || string.IsNullOrEmpty(entry.Value))
					continue;

				var str = entry.Value.ToLowerInvariant();
				foreach (var essence in monolith.Essences)
				{
					if (essence.Metadata.ToLowerInvariant().Contains(str))
					{
						Log.DebugFormat(
							"[HandleMonolithsTask] The Monolith [{0}] will be activated because it's essence [{1}] is whitelisted [{2}].",
							monolith.Id, essence.Metadata, entry.Value);
						monolith.Activate = true;
						return true;
					}
				}
			}

			// If this monolith has a whitelisted monster, activate it.
			foreach (var entry in MonolithsSettings.Instance.WhitelistMonsterMetadata)
			{
				if (entry == null || string.IsNullOrEmpty(entry.Value))
					continue;

				var str = entry.Value.ToLowerInvariant();
				if (monolith.MonsterMetadata.ToLowerInvariant().Contains(str))
				{
					Log.DebugFormat(
						"[HandleMonolithsTask] The Monolith [{0}] will be activated because it's monster [{1}] is whitelisted [{2}].",
						monolith.Id, monolith.MonsterMetadata, entry.Value);
					monolith.Activate = true;
					return true;
				}
			}

			Log.DebugFormat(
				"[HandleMonolithsTask] The Monolith [{0}] will not be activated because it matches no blacklist or whitelist entry.",
				monolith.Id);

			// Doesn't match anything, so don't activate it.
			monolith.Activate = false;
			return false;
		}

		/// <summary>
		/// Coroutine logic to execute.
		/// </summary>
		/// <returns>true if logic was executed to handle this type and false otherwise.</returns>
		public async Task<bool> Run()
		{
			// NOTE: This task's Run function is triggered from "hook_post_combat" Logic, as it's added via a secondary TaskManager!

			// If this task needs to be disabled due to errors, support doing so.
			if (_skip)
			{
				return false;
			}

			// Don't do anything in these cases.
			if (LokiPoe.Me.IsDead || LokiPoe.Me.IsInHideout || LokiPoe.Me.IsInTown || LokiPoe.Me.IsInMapRoom)
				return false;

			// If we're currently disabled, skip logic.
			if (!MonolithsSettings.Instance.Enabled)
				return false;

			var myPos = LokiPoe.MyPosition;

			var active = MonolithDataManager.Active;

			// Make sure the monolith is still valid and not blacklisted if it's set.
			// We don't re-eval current against settings, because of the performance overhead.
			if (_current != null)
			{
				if (!_current.IsValid || Blacklist.Contains(_current.Id))
				{
					_current = null;
				}
			}

			// Find the next best monolith.
			if (_current == null)
			{
				_current =
					active.Monoliths.Where(m => m.IsValid && !Blacklist.Contains(m.Id) && ShouldActivate(m))
						.OrderBy(m => m.Position.Distance(myPos))
						.FirstOrDefault();
				_moveErrors = 0;
				_interactErrors = 0;
				_interactAttempts = 0;
			}

			// Nothing to do if there's no monolith.
			if (_current == null)
			{
				return false;
			}

			// If we can't move to the monolith, blacklist it.
			if (_moveErrors > 5)
			{
				Blacklist.Add(_current.Id, TimeSpan.FromHours(1),
					string.Format("[HandleMonolithsTask::Logic] Unable to move to the Monolith."));
				_current = null;
				return true;
			}

			// If we are too far away to interact, move towards the object.
			if (myPos.Distance(_current.WalkablePosition) > 30)
			{
				// Make sure nothing is in the way.
				await Coroutines.CloseBlockingWindows();

				// Try to move towards the location.
				if (!PlayerMoverManager.MoveTowards(_current.WalkablePosition))
				{
					Log.ErrorFormat("[HandleMonolithsTask::Logic] PlayerMoverManager.MoveTowards({0}) failed for Monolith [{1}].",
						_current.WalkablePosition, _current.Id);
					_moveErrors++;
					return true;
				}

				_moveErrors = 0;

				return true;
			}

			// Make sure we're not doing anything before we interact.
			await Coroutines.FinishCurrentAction();

			// If the user wants to manually open, or just find them without opening, this task just blocks everything else after it,
			// rather than stopping the bot, to avoid deaths.
			if (!MonolithsSettings.Instance.Open)
			{
				return true;
			}

			// We always want to make sure we never just sit around trying to interact, if the interact has no results.
			if (_interactAttempts > 10)
			{
				Blacklist.Add(_current.Id, TimeSpan.FromHours(1),
					string.Format("[HandleMonolithsTask::Logic] Unexpected Monolith interaction result."));
				_current = null;
				return true;
			}

			// If we can't interact with the monolith, blacklist it.
			if (_interactErrors > 5)
			{
				Blacklist.Add(_current.Id, TimeSpan.FromHours(1),
					string.Format("[HandleMonolithsTask::Logic] Unable to interact with the Monolith."));
				_current = null;
				return true;
			}

			// Now process the object, but make sure it exists.
			var monolith = _current.NetworkObject;
			if (monolith == null)
			{
				Log.ErrorFormat("[HandleMonolithsTask::Logic] The NetworkObject does not exist for the Monolith [{0}] yet.",
					_current.Id);
				_interactErrors++;
				return true;
			}

			Log.InfoFormat("[HandleMonolithsTask::Logic] The Monolith [{0}] is at open phase [{1}].", monolith.Id,
				monolith.OpenPhase);

			++_interactAttempts;

			// Attempt to interact with it, we don't want to sleep afterwards because monsters spawn!
			if (!await Coroutines.InteractWith(monolith))
			{
				Log.ErrorFormat("[HandleMonolithsTask::Logic] Coroutines.InteractWith failed for the Monolith [{0}].", monolith.Id);
				_interactErrors++;
				return true;
			}

			_interactErrors = 0;

			return true;
		}
		
		/// <summary>The bot Start event.</summary>
		public void Start()
		{
			_skip = false;
			_current = null; // Force clear, in case settings changed.

			MonolithDataManager.Start(); // Will check IsInGame as needed
		}

		/// <summary>The bot Tick event.</summary>
		public void Tick()
		{
			MonolithDataManager.Tick(); // Will check IsInGame as needed
		}

		/// <summary>The bot Stop event.</summary>
		public void Stop()
		{
			MonolithDataManager.Stop(); // Will check IsInGame as needed
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