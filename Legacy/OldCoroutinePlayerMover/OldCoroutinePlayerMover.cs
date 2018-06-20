using System.Diagnostics;
using System.Linq;
using log4net;
using Loki.Bot.Pathfinding;
using Loki.Common;
using Loki.Game;
using Loki.Bot;
using System.Windows.Controls;
using System.Threading.Tasks;
using Buddy.Coroutines;
using System.Threading;

/// <summary>
/// This a developer example of using coroutines inside the PlayerMover.
/// 
/// DO NOT USE THIS PLAYER MOVER AS IT IS NOT INTENDED FOR NORMAL USE ANYMORE.
/// </summary>
/// 
namespace Legacy.OldCoroutinePlayerMover
{
	/// <summary>
	/// TODO: This is old code. It will be rewritten in the future...
	/// </summary>
	internal class OldCoroutinePlayerMover : IPlayerMover, IUrlProvider
	{
		#region Implementation of IUrlProvider
		public string Url => "https://www.thebuddyforum.com/threads/notice-upcoming-playermover-breaking-changes.418088/";
		#endregion

		private static readonly ILog Log = Logger.GetLoggerInstanceForType();

		private Gui _instance;

		#region Implementation of IAuthored

		/// <summary> The name of the plugin. </summary>
		public string Name => "OldCoroutinePlayerMover";

		/// <summary>The author of the plugin.</summary>
		public string Author => "Bossland GmbH";

		/// <summary> The description of the plugin. </summary>
		public string Description => "DO NOT USE. The old legacy player mover for Exilebuddy implemented with Coroutine support.";

		/// <summary>The version of the plugin.</summary>
		public string Version => "0.0.1.1";

		#endregion

		private Coroutine _coroutine;

		private PathfindingCommand _cmd;
		private readonly Stopwatch _sw = new Stopwatch();
		private LokiPoe.ConfigManager.NetworkingType _networkingMode = LokiPoe.ConfigManager.NetworkingType.Unknown;
		private int _pathRefreshRate = 1000;
		private Vector2i _lastPoint = Vector2i.Zero;

		/// <summary>
		/// These are areas that always have issues with stock pathfinding, so adjustments will be made.
		/// </summary>
		private readonly string[] _forcedAdjustmentAreas = new[]
		{
			"The City of Sarn",
			"The Slums",
		};

		private LokiPoe.TerrainDataEntry[,] _tgts;
		private uint _tgtSeed;

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
			_networkingMode = LokiPoe.ConfigManager.NetworkingMode; // Now this can be done cleanly!
			if (_networkingMode == LokiPoe.ConfigManager.NetworkingType.Predictive)
			{
				// Generate new paths in predictive more frequently to avoid back and forth issues from the new movement model
				_pathRefreshRate = 16;
			}
			else
			{
				_pathRefreshRate = 1000;
			}

			_coroutine = null;

			_coroutineSleepValue = BotManager.MsBetweenTicks;

			BotManager.Stop(new StopReasonData("OldCoroutinePlayerMover_DoNotUse", "Do not use this IPlayerMover implementation as it's a code example. Use OldPlayerMover instead!"));
		}

		/// <summary> The mover tick callback. Do any update logic here. </summary>
		public void Tick()
		{
		}

		/// <summary> The mover stop callback. Do any pre-dispose cleanup here. </summary>
		public void Stop()
		{
			// Cleanup the coroutine.
			if (_coroutine != null)
			{
				_coroutine.Dispose();
				_coroutine = null;
			}
		}

		#endregion

		#region Implementation of IConfigurable

		/// <summary>The settings object. This will be registered in the current configuration.</summary>
		public JsonSettings Settings => OldCoroutinePlayerMoverSettings.Instance;

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

		private Vector2i _coroutinePosition;
		private dynamic[] _coroutineUser;
		private int _coroutineSleepValue = 15;

		/// <summary>
		/// Attempts to move towards a position. This function will perform pathfinding logic and take into consideration move distance
		/// to try and smoothly move towards a point.
		/// </summary>
		/// <param name="position">The position to move towards.</param>
		/// <param name="user">A user object passed.</param>
		/// <returns>true if the position was moved towards, and false if there was a pathfinding error.</returns>
		public bool MoveTowards(Vector2i position, params dynamic[] user)
		{
			_coroutinePosition = position;
			_coroutineUser = user;

			if (_coroutine == null || _coroutine.IsFinished)
			{
				_coroutine = new Coroutine(() => MainCoroutine());
			}

			// Variant 1: If you do not want all execution to be blocked inside MoveTowards, and instead let 
			// other logic run between MoveTowards calls when you need to yield execution, then you would simply 
			// only call Resume once per MoveTowards call, and coroutine execution would continue where it left off.
			// However, this might result in unexpected side-effects since it's a new design, so use with care...
			/*
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
			*/

			// Variant 2: If you want all execution to be blocked inside MoveTowards, as it is
			// with the normal player mover, then you would want to loop until the coroutine is finished,
			// and release the frame at the end of the loop. The main coroutine would be re-created after
			// it finishes each time.
			while (!_coroutine.IsFinished)
			{
				try
				{
					_coroutine.Resume();
				}
				catch
				{
					// This block of code gets copied around, but isn't explained. The reason why this is done
					// this way, is in the event Dispose throws an exception, the coroutine object ('_coroutine') would not 
					// get set to null, and execution would be locked up with an invalid coroutine object. By keeping it in
					// a local first, then clearing the class variable, if an exception happens in Dispose, we can
					// recover because '_coroutine' was set to null and will be created again next Tick.
					var c = _coroutine;
					_coroutine = null;
					c.Dispose();
					throw;
				}

				if (!_coroutine.IsFinished)
				{
					// Just like normal player mover, we release the frame after logic is done running, to trigger 
					// the next frame for execution to continue. Execution control stays within this loop though.
					LokiPoe.ReleaseFrameProfiler("OldCoroutinePlayerMover.MoveTowards", () =>
					{
						// How you Sleep is up to the mover and what is actually needed, but here's one example
						// of what you can do (allows changing the sleep inside the coroutine to affect here)/
						Thread.Sleep(_coroutineSleepValue);
					});
				}
			}

			// If the coroutine finished, return what it returned.
			if (_coroutine.IsFinished)
			{
				return (bool)_coroutine.Result;
			}

			// Otherwise, the coroutine is still running, so we're going to return true
			// since the move is in progress (if it wasn't the coroutine would have returned false, 
			// and we'd not reach here.)
			return true;
		}

		#endregion

		private async Task<object> MainCoroutine()
		{
			// Update the default value each time, since this can change at any time.
			_coroutineSleepValue = BotManager.MsBetweenTicks;

			var myPosition = LokiPoe.MyPosition;
			if (
				_cmd == null || // No command yet
				_cmd.Path == null ||
				_cmd.EndPoint != _coroutinePosition || // Moving to a new position
				LokiPoe.CurrentWorldArea.IsTown || // In town, always generate new paths
				(_sw.IsRunning && _sw.ElapsedMilliseconds > _pathRefreshRate) || // New paths on interval
				_cmd.Path.Count <= 2 || // Not enough points
				_cmd.Path.All(p => myPosition.Distance(p) > 7))
			// Try and find a better path to follow since we're off course
			{
				_cmd = new PathfindingCommand(myPosition, _coroutinePosition, 3, OldCoroutinePlayerMoverSettings.Instance.AvoidWallHugging);
				if (!ExilePather.FindPath(ref _cmd))
				{
					_sw.Restart();
					Log.ErrorFormat("[OldCoroutinePlayerMover.MoveTowards] ExilePather.FindPath failed from {0} to {1}.",
						myPosition, _coroutinePosition);

					return false;
				}
				//Log.InfoFormat("[OldCoroutinePlayerMover.MoveTowards] Finding new path.");
				_sw.Restart();
				//_originalPath = new IndexedList<Vector2i>(_cmd.Path);
			}

			// Eliminate points until we find one within a good moving range.
			while (_cmd.Path.Count > 1)
			{
				if (_cmd.Path[0].Distance(myPosition) < OldCoroutinePlayerMoverSettings.Instance.MoveRange)
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

			var cwa = LokiPoe.CurrentWorldArea;
			if (!cwa.IsTown && !cwa.IsHideoutArea && _forcedAdjustmentAreas.Contains(cwa.Name))
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
					point.X += 15;
					Log.InfoFormat("[OldCoroutinePlayerMover.MoveTowards] Adjustments being made!");
					_cmd.Path[0] = point;
				}
				else if (posX > 5 && negX == 0)
				{
					point.X -= 15;
					Log.InfoFormat("[OldCoroutinePlayerMover.MoveTowards] Adjustments being made!");
					_cmd.Path[0] = point;
				}
			}

			// Le sigh...
			if (cwa.IsTown && cwa.Act == 3)
			{
				var seed = LokiPoe.LocalData.AreaHash;
				if (_tgtSeed != seed || _tgts == null)
				{
					Log.InfoFormat("[OldCoroutinePlayerMover.MoveTowards] Now building TGT info.");
					_tgts = LokiPoe.TerrainData.TgtEntries;
					_tgtSeed = seed;
				}
				if (TgtUnderPlayer.TgtName.Equals("Art/Models/Terrain/Act3Town/Act3_town_01_01_c16r7.tgt"))
				{
					Log.InfoFormat("[OldCoroutinePlayerMover.MoveTowards] Act 3 Town force adjustment being made!");
					point.Y += 5;
				}
			}

			var move = LokiPoe.InGameState.SkillBarHud.LastBoundMoveSkill;
			if (move == null)
			{
				Log.ErrorFormat("[OldCoroutinePlayerMover.MoveTowards] Please assign the \"Move\" skill to your skillbar!");
				return false;
			}

			if ((LokiPoe.ProcessHookManager.GetKeyState(move.BoundKeys.Last()) & 0x8000) != 0 &&
				LokiPoe.Me.HasCurrentAction)
			{
				if (myPosition.Distance(_coroutinePosition) < OldCoroutinePlayerMoverSettings.Instance.SingleUseDistance)
				{
					LokiPoe.ProcessHookManager.ClearAllKeyStates();
					LokiPoe.InGameState.SkillBarHud.UseAt(move.Slots.Last(), false, point);
					if (OldCoroutinePlayerMoverSettings.Instance.DebugInputApi)
					{
						Log.WarnFormat("[SkillBarHud.UseAt] {0}", point);
					}
					_lastPoint = point;
				}
				else
				{
					if (OldCoroutinePlayerMoverSettings.Instance.UseMouseSmoothing)
					{
						var d = _lastPoint.Distance(point);
						if (d >= OldCoroutinePlayerMoverSettings.Instance.MouseSmoothDistance)
						{
							MouseManager.SetMousePos("OldCoroutinePlayerMover.MoveTowards", point, false);
							if (OldCoroutinePlayerMoverSettings.Instance.DebugInputApi)
							{
								Log.WarnFormat("[MouseManager.SetMousePos] {0} [{1}]", point, d);
							}
							_lastPoint = point;
						}
						else
						{
							if (OldCoroutinePlayerMoverSettings.Instance.DebugInputApi)
							{
								Log.WarnFormat("[MouseManager.SetMousePos] Skipping moving mouse to {0} because [{1}] < [{2}]", point, d, OldCoroutinePlayerMoverSettings.Instance.MouseSmoothDistance);
							}
						}
					}
					else
					{
						MouseManager.SetMousePos("OldCoroutinePlayerMover.MoveTowards", point, false);
					}
				}
			}
			else
			{
				LokiPoe.ProcessHookManager.ClearAllKeyStates();
				if (myPosition.Distance(_coroutinePosition) < OldCoroutinePlayerMoverSettings.Instance.SingleUseDistance)
				{
					LokiPoe.InGameState.SkillBarHud.UseAt(move.Slots.Last(), false, point);
					if (OldCoroutinePlayerMoverSettings.Instance.DebugInputApi)
					{
						Log.WarnFormat("[SkillBarHud.UseAt] {0}", point);
					}
				}
				else
				{
					LokiPoe.InGameState.SkillBarHud.BeginUseAt(move.Slots.Last(), false, point);
					if (OldCoroutinePlayerMoverSettings.Instance.DebugInputApi)
					{
						Log.WarnFormat("[BeginUseAt] {0}", point);
					}
				}
			}

			return true;
		}
	}
}
