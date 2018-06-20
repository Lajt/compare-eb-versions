using System;
using System.Diagnostics;
using System.Linq;
using log4net;
using Loki.Bot.Pathfinding;
using Loki.Common;
using Loki.Game;
using Loki.Bot;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace Legacy.OldPlayerMover
{
	/// <summary>
	/// TODO: This is old code. It will be rewritten in the future...
	/// </summary>
	internal class OldPlayerMover : IPlayerMover, IUrlProvider
	{
		#region Implementation of IUrlProvider
		public string Url => "https://www.thebuddyforum.com/threads/notice-upcoming-playermover-breaking-changes.418088/";
		#endregion

		private static readonly ILog Log = Logger.GetLoggerInstanceForType();

		private Gui _instance;

		#region Implementation of IAuthored

		/// <summary> The name of the plugin. </summary>
		public string Name => "OldPlayerMover";

		/// <summary>The author of the plugin.</summary>
		public string Author => "Bossland GmbH";

		/// <summary> The description of the plugin. </summary>
		public string Description => "The old legacy player mover for Exilebuddy.";

		/// <summary>The version of the plugin.</summary>
		public string Version => "0.0.1.1";

		#endregion

		private PathfindingCommand _cmd;
		private readonly Stopwatch _sw = new Stopwatch();
		private Vector2i _lastPoint = Vector2i.Zero;

		private LokiPoe.TerrainDataEntry[,] _tgts;
		private uint _tgtSeed;

		private bool _useForceAdjustments;
		private bool _useAct3TownAdjustments;

		private LokiPoe.TerrainDataEntry TgtUnderPlayer
		{
			get
			{
				var myPos = LokiPoe.LocalData.MyPosition;
				return _tgts[myPos.X / 23, myPos.Y / 23];
			}
		}

		#region Implementation of IBase

		/// <summary>Initializes this object.</summary>
		public void Initialize()
		{
		}

		/// <summary>Deinitializes this object. This is called when the object is being unloaded from the bot.</summary>
		public void Deinitialize()
		{
		}

		#endregion

		#region Implementation of ITickEvents / IStartStopEvents

		/// <summary> The mover start callback. Do any initialization here. </summary>
		public void Start()
		{
		}

		/// <summary> The mover tick callback. Do any update logic here. </summary>
		public void Tick()
		{
			if (!LokiPoe.IsInGame)
				return;

			var cwa = LokiPoe.CurrentWorldArea;

			if (cwa.IsCombatArea && OldPlayerMoverSettings.Instance.ForceAdjustCombatAreas || OldPlayerMoverSettings.Instance.ForcedAdjustmentAreas.Any(e => e.Value.Equals(cwa.Name, StringComparison.OrdinalIgnoreCase)))
			{
				_useForceAdjustments = true;
			}
			else
			{
				_useForceAdjustments = false;
			}

			if (cwa.IsTown && cwa.Act == 3)
			{
				_useAct3TownAdjustments = true;
			}
			else
			{
				_useAct3TownAdjustments = false;
			}
		}

		/// <summary> The mover stop callback. Do any pre-dispose cleanup here. </summary>
		public void Stop()
		{
		}

		#endregion

		#region Implementation of IConfigurable

		/// <summary>The settings object. This will be registered in the current configuration.</summary>
		public JsonSettings Settings => OldPlayerMoverSettings.Instance;

		/// <summary> The plugin's settings control. This will be added to the Exilebuddy Settings tab.</summary>
		public UserControl Control => (_instance ?? (_instance = new Gui()));

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

		#region Implementation of IEnableable

		/// <summary> The plugin is being enabled.</summary>
		public void Enable()
		{
		}

		/// <summary> The plugin is being disabled.</summary>
		public void Disable()
		{
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

		#region Override of IPlayerMover

		/// <summary>
		/// Returns the player mover's current PathfindingCommand being used.
		/// </summary>
		public PathfindingCommand CurrentCommand => _cmd;

		/// <summary>
		/// Attempts to move towards a position. This function will perform pathfinding logic and take into consideration move distance
		/// to try and smoothly move towards a point.
		/// </summary>
		/// <param name="position">The position to move towards.</param>
		/// <param name="user">A user object passed.</param>
		/// <returns>true if the position was moved towards, and false if there was a pathfinding error.</returns>
		public bool MoveTowards(Vector2i position, params dynamic[] user)
		{
			var myPosition = LokiPoe.MyPosition;
			if (
				_cmd == null || // No command yet
				_cmd.Path == null ||
				_cmd.EndPoint != position || // Moving to a new position
				LokiPoe.CurrentWorldArea.IsTown || // In town, always generate new paths
				(_sw.IsRunning && _sw.ElapsedMilliseconds > OldPlayerMoverSettings.Instance.PathRefreshRateMs) || // New paths on interval
				_cmd.Path.Count <= 2 || // Not enough points
				_cmd.Path.All(p => myPosition.Distance(p) > 7))
			// Try and find a better path to follow since we're off course
			{
				_cmd = new PathfindingCommand(myPosition, position, 3, OldPlayerMoverSettings.Instance.AvoidWallHugging);
				if (!ExilePather.FindPath(ref _cmd))
				{
					_sw.Restart();
					Log.ErrorFormat("[OldPlayerMover.MoveTowards] ExilePather.FindPath failed from {0} to {1}.",
						myPosition, position);
					return false;
				}
				//Log.InfoFormat("[OldPlayerMover.MoveTowards] Finding new path.");
				_sw.Restart();
				//_originalPath = new IndexedList<Vector2i>(_cmd.Path);
			}

			// Eliminate points until we find one within a good moving range.
			while (_cmd.Path.Count > 1)
			{
				if (_cmd.Path[0].Distance(myPosition) < OldPlayerMoverSettings.Instance.MoveRange)
				{
					_cmd.Path.RemoveAt(0);
				}
				else
				{
					break;
				}
			}

			var point = _cmd.Path[0];
			point += new Vector2i(LokiPoe.Random.Next(-2, 3), LokiPoe.Random.Next(-2, 3));

			if (_useForceAdjustments)
			{
				var negX = 0;
				var posX = 0;

				var tmp1 = point;
				var tmp2 = point;

				for (var i = 0; i < 10; i++)
				{
					tmp1.X--;
					if (!ExilePather.IsWalkable(tmp1))
					{
						negX++;
					}

					tmp2.X++;
					if (!ExilePather.IsWalkable(tmp2))
					{
						posX++;
					}
				}

				if (negX > 5 && posX == 0)
				{
					point.X += 10;
					if (OldPlayerMoverSettings.Instance.DebugAdjustments)
					{
						Log.WarnFormat("[OldPlayerMover.MoveTowards] X-Adjustments being made!");
					}
					_cmd.Path[0] = point;
				}
				else if (posX > 5 && negX == 0)
				{
					point.X -= 10;
					if (OldPlayerMoverSettings.Instance.DebugAdjustments)
					{
						Log.WarnFormat("[OldPlayerMover.MoveTowards] X-Adjustments being made!");
					}
					_cmd.Path[0] = point;
				}

				var negY = 0;
				var posY = 0;

				tmp1 = point;
				tmp2 = point;

				for (var i = 0; i < 10; i++)
				{
					tmp1.Y--;
					if (!ExilePather.IsWalkable(tmp1))
					{
						negY++;
					}

					tmp2.Y++;
					if (!ExilePather.IsWalkable(tmp2))
					{
						posY++;
					}
				}

				if (negY > 5 && posY == 0)
				{
					point.Y += 10;
					if (OldPlayerMoverSettings.Instance.DebugAdjustments)
					{
						Log.WarnFormat("[OldPlayerMover.MoveTowards] Y-Adjustments being made!");
					}
					_cmd.Path[0] = point;
				}
				else if (posY > 5 && negY == 0)
				{
					point.Y -= 10;
					if (OldPlayerMoverSettings.Instance.DebugAdjustments)
					{
						Log.WarnFormat("[OldPlayerMover.MoveTowards] Y-Adjustments being made!");
					}
					_cmd.Path[0] = point;
				}
			}

			// Le sigh...
			if(_useAct3TownAdjustments)
			{
				var seed = LokiPoe.LocalData.AreaHash;
				if (_tgtSeed != seed || _tgts == null)
				{
					Log.InfoFormat("[OldPlayerMover.MoveTowards] Now building TGT info.");
					_tgts = LokiPoe.TerrainData.TgtEntries;
					_tgtSeed = seed;
				}
				if (TgtUnderPlayer.TgtName.Equals("Art/Models/Terrain/Act3Town/Act3_town_01_01_c16r7.tgt"))
				{
					Log.InfoFormat("[OldPlayerMover.MoveTowards] Act 3 Town force adjustment being made!");
					point.Y += 5;
				}
			}

			var move = LokiPoe.InGameState.SkillBarHud.LastBoundMoveSkill;
			if (move == null)
			{
				Log.ErrorFormat("[OldPlayerMover.MoveTowards] Please assign the \"Move\" skill to your skillbar!");
				return false;
			}

			if ((LokiPoe.ProcessHookManager.GetKeyState(move.BoundKeys.Last()) & 0x8000) != 0 &&
				LokiPoe.Me.HasCurrentAction)
			{
				if (myPosition.Distance(position) < OldPlayerMoverSettings.Instance.SingleUseDistance)
				{
					LokiPoe.ProcessHookManager.ClearAllKeyStates();
					LokiPoe.InGameState.SkillBarHud.UseAt(move.Slots.Last(), false, point);
					if (OldPlayerMoverSettings.Instance.DebugInputApi)
					{
						Log.WarnFormat("[SkillBarHud.UseAt] {0}", point);
					}
					_lastPoint = point;
				}
				else
				{
					if (OldPlayerMoverSettings.Instance.UseMouseSmoothing)
					{
						var d = _lastPoint.Distance(point);
						if (d >= OldPlayerMoverSettings.Instance.MouseSmoothDistance)
						{
							MouseManager.SetMousePos("OldPlayerMover.MoveTowards", point, false);
							if (OldPlayerMoverSettings.Instance.DebugInputApi)
							{
								Log.WarnFormat("[MouseManager.SetMousePos] {0} [{1}]", point, d);
							}
							_lastPoint = point;
						}
						else
						{
							if (OldPlayerMoverSettings.Instance.DebugInputApi)
							{
								Log.WarnFormat("[MouseManager.SetMousePos] Skipping moving mouse to {0} because [{1}] < [{2}]", point, d, OldPlayerMoverSettings.Instance.MouseSmoothDistance);
							}
						}
					}
					else
					{
						MouseManager.SetMousePos("OldPlayerMover.MoveTowards", point, false);
					}
				}
			}
			else
			{
				LokiPoe.ProcessHookManager.ClearAllKeyStates();
				if (myPosition.Distance(position) < OldPlayerMoverSettings.Instance.SingleUseDistance)
				{
					LokiPoe.InGameState.SkillBarHud.UseAt(move.Slots.Last(), false, point);
					if (OldPlayerMoverSettings.Instance.DebugInputApi)
					{
						Log.WarnFormat("[SkillBarHud.UseAt] {0}", point);
					}
				}
				else
				{
					LokiPoe.InGameState.SkillBarHud.BeginUseAt(move.Slots.Last(), false, point);
					if (OldPlayerMoverSettings.Instance.DebugInputApi)
					{
						Log.WarnFormat("[BeginUseAt] {0}", point);
					}
				}
			}

			return true;
		}

		#endregion
	}
}