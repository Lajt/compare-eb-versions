using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using log4net;
using Loki.Bot;
using Loki.Common;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;
using Loki.Bot.Pathfinding;
using Default.EXtensions;
using Default.EXtensions.Global;

namespace Community.Follower
{
	public class LeaderData
	{
		public Vector2i LastKnownPosition { get; set; }

		public int LastAreaTransitionId { get; set; }

		public AreaTransition LastAreaTransition => LokiPoe.ObjectManager.GetObjectById<AreaTransition>(LastAreaTransitionId);

		public int LabyrinthReturnPortalId { get; set; }

		public LabyrinthReturnPortal LabyrinthReturnPortal => LokiPoe.ObjectManager.GetObjectById<LabyrinthReturnPortal>(LabyrinthReturnPortalId);
	}

	public static class StringHelper
	{
		private static HashSet<string> ArenaNames;

		public static ClientStringsEnum[] ArenaNameEnums = new ClientStringsEnum[]
		{
			ClientStringsEnum.MerveilArenaName,
			ClientStringsEnum.BrutusArenaName,
			ClientStringsEnum.OversoulArenaName,
			ClientStringsEnum.WeaverArenaName,
			ClientStringsEnum.KaomArenaName,
			ClientStringsEnum.DaressoArenaName,
			ClientStringsEnum.DoedreArenaName,
			ClientStringsEnum.ShavronneArenaName,
			ClientStringsEnum.MaligaroArenaName,
			ClientStringsEnum.PietyBellyArenaName,
			ClientStringsEnum.MalachaiArenaName1,
			ClientStringsEnum.MalachaiArenaName2,
			ClientStringsEnum.ArenaNameDominus,
			ClientStringsEnum.ArenaNameGruthkul,
			ClientStringsEnum.ArenaNameAvarius,
			ClientStringsEnum.ArenaNameKitava,
			ClientStringsEnum.ArenaNameTukohama,
			ClientStringsEnum.ArenaNameShavronne,
			ClientStringsEnum.ArenaNameAbberath1,
			ClientStringsEnum.ArenaNameAbberath2,
			ClientStringsEnum.ArenaNameAbberath3,
			ClientStringsEnum.ArenaNameRyslatha,
			ClientStringsEnum.ArenaNameBrineKing,
			ClientStringsEnum.ArenaNameMaligaro,
			ClientStringsEnum.ArenaNameRalakesh,
			ClientStringsEnum.ArenaNameArakaali,
			ClientStringsEnum.ArenaNameYugul,
			ClientStringsEnum.ArenaNameDoedre,
			ClientStringsEnum.ArenaNameSolarisLunaris,
			ClientStringsEnum.ArenaNameKitava2,
			ClientStringsEnum.ArenaNameTolmanAnkh,
			ClientStringsEnum.ArenaNameGarukhan,
			ClientStringsEnum.ArenaNameGeneralAdus,
			ClientStringsEnum.ArenaNameShakari,
			ClientStringsEnum.ArenaNameShakari2,
			ClientStringsEnum.ArenaNameShakari3,
			ClientStringsEnum.IncursionBossArenaName,
		};

		public static bool IsArenaTransition(AreaTransition at)
		{
			if (at == null)
				return false;

			if(ArenaNames == null)
			{
				ArenaNames = new HashSet<string>();
				foreach(var @enum in ArenaNameEnums)
				{
					var dat = Dat.LookupClientString(@enum);
					if(dat != null)
					{
						ArenaNames.Add(dat.Value);
					}
				}
			}

			return ArenaNames.Contains(at.Name);
		}
	}

	public class FollowerTask : ITask
	{
		private static readonly ILog Log = Logger.GetLoggerInstanceForType();

		private Follower _parent;
		private LeaderData _leaderData;

		private Player Leader => LokiPoe.ObjectManager.GetObjectByName<Player>(FollowerSettings.Instance.Leader);

		internal FollowerTask(Follower parent)
		{
			_parent = parent;
			_leaderData = new LeaderData();
		}

		#region Implementation of ITask

		public async Task<bool> HandleLabyrinthReturnPortal()
		{
			// Check to see if we have a lab portal to use.
			var portal = _leaderData.LabyrinthReturnPortal;
			if (portal == null)
				return false;

			// Take the area transition
			await PlayerAction.TakePortal(portal);

			// Clear the data so we don't run again.
			_leaderData.LabyrinthReturnPortalId = 0;

			return true;
		}

		public async Task<bool> HandleLastAreaTransition()
		{
			// Check to see if we have an area transition to use.
			var transition = _leaderData.LastAreaTransition;
			if (transition == null)
				return false;

			// See if we should stop outside the boss room.
			if(transition.Metadata.Equals("Metadata/Terrain/Labyrinth/Objects/LabyrinthIzaroArenaTransition") ||
				StringHelper.IsArenaTransition(transition))
			{
				if(FollowerSettings.Instance.StopOutsideBossDoor)
				{
					BotManager.Stop(new StopReasonData("Follower_StopOutsideBossDoor", "The user wishes the bot stop outside the boss room door."));
					return true;
				}
			}

			// Take the area transition
			await PlayerAction.TakeTransition(transition);

			// Clear the data so we don't use it again.
			_leaderData.LastAreaTransitionId = 0;

			return true;
		}

		/// <summary>
		/// Runs a task.
		/// </summary>
		/// <returns>true if the task ran and false otherwise.</returns>
		public async Task<bool> Run()
		{
			StuckDetection.Reset(); // We don't want StuckDetection logic running really.

			var leader = Leader;
			var me = LokiPoe.Me;

			// If we don't have the leader in view, let's try to guess what they did.
			if (leader == null)
			{
				// If we've at least seen the leader once, first try moving to where they were last seen.
				if (_leaderData.LastKnownPosition != Vector2i.Zero)
				{
					// Move closer to their last known position if we're out of range.
					if (me.Position.Distance(_leaderData.LastKnownPosition) > 20)
					{
						// First, make sure we can actually get to the player's last known position before trying to move there.
						if (ExilePather.PathExistsBetween(me.Position, _leaderData.LastKnownPosition, true))
						{
							// Just move to the location.
							if (!PlayerMoverManager.MoveTowards(_leaderData.LastKnownPosition))
							{
								Log.Error($"[FollowerTask] PlayerMoverManager.MoveTowards failed for {_leaderData.LastKnownPosition}.");
							}
						}
					}

					// Clear the data so we don't run again.
					_leaderData.LastKnownPosition = Vector2i.Zero;
				}

				// We have to decide which we want to use based on distance.
				if (_leaderData.LabyrinthReturnPortal != null && _leaderData.LastAreaTransition != null)
				{
					if(_leaderData.LabyrinthReturnPortal.Distance < _leaderData.LastAreaTransition.Distance)
					{
						// Check to see if we saw the player near an area transition or the exit portal
						if (await HandleLabyrinthReturnPortal())
						{
							return true;
						}
					}
					else
					{
						// Check to see if we saw the player near an area transition or the exit portal
						if (await HandleLastAreaTransition())
						{
							return true;
						}
					}
				}
				else // Order doesn't matter
				{
					// Check to see if we saw the player near an area transition or the exit portal
					if (await HandleLastAreaTransition())
					{
						return true;
					}

					// Check to see if we saw the player near an area transition or the exit portal
					if (await HandleLabyrinthReturnPortal())
					{
						return true;
					}
				}

				Log.Warn("[TODO 1]");

				return true;
			}

			// Check to see if were within range. If so, return, as we have nothing else to do here.
			if (leader.Distance <= FollowerSettings.Instance.FollowDistance)
			{
				return true;
			}

			// Otherwise, first check to see if a path exists between us and the leader, this is mostly to handle transitions
			if (ExilePather.PathExistsBetween(me.Position, leader.Position, true))
			{
				var leaderDistance = leader.Distance;

				// If there's an area transition closer to us than the leader, it's a special boss one,
				// but we don't need to stop the bot really, we can just sit and wait until it opens. Unless
				// we want the character to take it, in which case we can add that logic s well.
				if (LokiPoe.ObjectManager.Objects.OfType<AreaTransition>().Any(at => StringHelper.IsArenaTransition(at) && at.Distance < leaderDistance && at.IsTargetable))
				{
					return true;
				}

				// Just move to towards the player.
				if (!PlayerMoverManager.MoveTowards(leader.Position))
				{
					// We can't do much other than log the error and let the user fix the issue.
					Log.Error($"[FollowerTask] PlayerMoverManager.MoveTowards failed for {leader.Position}.");
				}

				// Nothing else to do.
				return true;
			}

			// Check to see if we saw the player near an area transition.
			if (await HandleLastAreaTransition())
			{
				return true;
			}

			Log.Warn("[TODO 2]");

			// This task takes control over QB/MB, so it should always return true;
			return true;
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
			return MessageResult.Unprocessed;
		}

		#endregion

		#region Implementation of ILogicHandler

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

		#region Implementation of ITickEvents

		/// <summary> The task tick callback. Do any update logic here. </summary>
		public void Tick()
		{
			if (!LokiPoe.IsInGame)
				return;

			// Check to see if the client has the leader object.
			var leader = Leader;
			if (leader == null)
				return;

			// Keep track of where we last saw the leader. Technically, we should always have the object in memory if it's in the same area.
			_leaderData.LastKnownPosition = leader.Position;

			// First, track the closest usable AreaTransition to the leader's position.
			var leaderTransition = LokiPoe.ObjectManager.Objects.OfType<AreaTransition>().OrderBy(at => at.Position.Distance(leader.Position)).FirstOrDefault();
			if (leaderTransition != null)
			{
				if (leaderTransition.Id != _leaderData.LastAreaTransitionId)
				{
					_leaderData.LastAreaTransitionId = leaderTransition.Id;
					Log.WarnFormat($"[FollowerTask] LastAreaTransitionId being updated to [{leaderTransition.Id}] {leaderTransition.Name}: {leaderTransition.Metadata}");
				}
			}

			var labyrinthReturnPortal = LokiPoe.ObjectManager.Objects.OfType<LabyrinthReturnPortal>().FirstOrDefault();
			if (labyrinthReturnPortal != null)
			{
				if (_leaderData.LabyrinthReturnPortalId != labyrinthReturnPortal.Id)
				{
					_leaderData.LabyrinthReturnPortalId = labyrinthReturnPortal.Id;
					Log.WarnFormat($"[FollowerTask] LabyrinthReturnPortalId being updated to [{labyrinthReturnPortal.Id}] {labyrinthReturnPortal.Name}: {labyrinthReturnPortal.Metadata}");
				}
			}
		}

		#endregion

		#region Implementation of IStartStopEvents

		/// <summary> The task start callback. Do any initialization here. </summary>
		public void Start()
		{
		}

		/// <summary> The task stop callback. Do any pre-dispose cleanup here. </summary>
		public void Stop()
		{
		}

		#endregion

		#region Implementation of IAuthored

		/// <summary>
		/// The name of this task.
		/// </summary>
		public string Name => "FollowerTask";

		/// <summary>
		/// The author of this task.
		/// </summary>
		public string Author => "Bossland GmbH";

		/// <summary>
		/// The description of this task.
		/// </summary>
		public string Description => "Task that follows a character.";

		/// <summary>
		/// The version of this task.
		/// </summary>
		public string Version => "0.0.1.1";

		#endregion
	}
}