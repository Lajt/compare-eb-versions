using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using Buddy.Coroutines;
using log4net;
using Loki.Bot;
using Loki.Bot.Pathfinding;
using Loki.Common;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Legacy.OldRoutine
{
	/// <summary> </summary>
	public class OldRoutine : IRoutine
	{
		#region Temp Compatibility 

		/// <summary>
		/// 
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="distanceFromPoint"></param>
		/// <param name="dontLeaveFrame">Should the current frame not be left?</param>
		/// <returns></returns>
		public static int NumberOfMobsBetween(NetworkObject start, NetworkObject end, int distanceFromPoint = 5,
			bool dontLeaveFrame = false)
		{
			// More lightweight check to just get an idea of what is around us, rather than the heavy IsActive.
			var mobs =
				LokiPoe.ObjectManager.GetObjectsByType<Monster>().Where(d => d.IsAliveHostile).ToList();
			if (!mobs.Any())
				return 0;

			var path = ExilePather.GetPointsOnSegment(start.Position, end.Position, dontLeaveFrame);

			var count = 0;
			for (var i = 0; i < path.Count; i += 10)
			{
				foreach (var mob in mobs)
				{
					if (mob.Position.Distance(path[i]) <= distanceFromPoint)
					{
						++count;
					}
				}
			}

			return count;
		}

		/// <summary>
		/// Checks for a closed door between start and end.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="distanceFromPoint">How far to check around each point for a door object.</param>
		/// <param name="stride">The distance between points to check in the path.</param>
		/// <param name="dontLeaveFrame">Should the current frame not be left?</param>
		/// <returns>true if there's a closed door and false otherwise.</returns>
		public static bool ClosedDoorBetween(NetworkObject start, NetworkObject end, int distanceFromPoint = 10,
			int stride = 10, bool dontLeaveFrame = false)
		{
			return ClosedDoorBetween(start.Position, end.Position, distanceFromPoint, stride, dontLeaveFrame);
		}

		/// <summary>
		/// Checks for a closed door between start and end.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="distanceFromPoint">How far to check around each point for a door object.</param>
		/// <param name="stride">The distance between points to check in the path.</param>
		/// <param name="dontLeaveFrame">Should the current frame not be left?</param>
		/// <returns>true if there's a closed door and false otherwise.</returns>
		public static bool ClosedDoorBetween(NetworkObject start, Vector2i end, int distanceFromPoint = 10,
			int stride = 10, bool dontLeaveFrame = false)
		{
			return ClosedDoorBetween(start.Position, end, distanceFromPoint, stride, dontLeaveFrame);
		}

		/// <summary>
		/// Checks for a closed door between start and end.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="distanceFromPoint">How far to check around each point for a door object.</param>
		/// <param name="stride">The distance between points to check in the path.</param>
		/// <param name="dontLeaveFrame">Should the current frame not be left?</param>
		/// <returns>true if there's a closed door and false otherwise.</returns>
		public static bool ClosedDoorBetween(Vector2i start, NetworkObject end, int distanceFromPoint = 10,
			int stride = 10, bool dontLeaveFrame = false)
		{
			return ClosedDoorBetween(start, end.Position, distanceFromPoint, stride, dontLeaveFrame);
		}

		/// <summary>
		/// Checks for a closed door between start and end.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="distanceFromPoint">How far to check around each point for a door object.</param>
		/// <param name="stride">The distance between points to check in the path.</param>
		/// <param name="dontLeaveFrame">Should the current frame not be left?</param>
		/// <returns>true if there's a closed door and false otherwise.</returns>
		public static bool ClosedDoorBetween(Vector2i start, Vector2i end, int distanceFromPoint = 10, int stride = 10,
			bool dontLeaveFrame = false)
		{
			var doors = LokiPoe.ObjectManager.AnyDoors.Where(d => !d.IsOpened).ToList();
			if (!doors.Any())
				return false;

			var path = ExilePather.GetPointsOnSegment(start, end, dontLeaveFrame);

			for (var i = 0; i < path.Count; i += stride)
			{
				foreach (var door in doors)
				{
					if (door.Position.Distance(path[i]) <= distanceFromPoint)
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Returns the number of mobs near a target.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="distance"></param>
		/// <param name="dead"></param>
		/// <returns></returns>
		public static int NumberOfMobsNear(NetworkObject target, float distance, bool dead = false)
		{
			var mpos = target.Position;

			var curCount = 0;

			foreach (var mob in LokiPoe.ObjectManager.Objects.OfType<Monster>())
			{
				if (mob.Id == target.Id)
				{
					continue;
				}

				// If we're only checking for dead mobs... then... yeah...
				if (dead)
				{
					if (!mob.IsDead)
					{
						continue;
					}
				}
				else if (!mob.IsAliveHostile)
				{
					continue;
				}

				if (mob.Position.Distance(mpos) < distance)
				{
					curCount++;
				}
			}

			return curCount;
		}

		#endregion

		private static readonly ILog Log = Logger.GetLoggerInstanceForType();

		private int _totalCursesAllowed;

		// Auto-set, you do not have to change these.
		private int _sgSlot;

		private int _summonChaosGolemSlot = -1;
		private int _summonIceGolemSlot = -1;
		private int _summonFlameGolemSlot = -1;
		private int _summonStoneGolemSlot = -1;
		private int _summonLightningGolemSlot = -1;
		private int _raiseZombieSlot = -1;
		private int _raiseSpectreSlot = -1;
		private int _animateWeaponSlot = -1;
		private int _animateGuardianSlot = -1;
		private int _flameblastSlot = -1;
		private int _enduringCrySlot = -1;
		private int _moltenShellSlot = -1;
		private int _bloodRageSlot = -1;
		private int _rfSlot = -1;
		private readonly List<int> _curseSlots = new List<int>();
		private int _auraSlot = -1;
		private int _totemSlot = -1;
		private int _trapSlot = -1;
		private int _mineSlot = -1;
		private int _summonSkeletonsSlot = -1;
		private int _summonRagingSpiritSlot = -1;
		private int _coldSnapSlot = -1;
		private int _fleshOfferingSlot = -1;
		private int _boneOfferingSlot = -1;
		private int _convocationSlot = -1;
		private int _orbOfStormsSlot = -1;
		private int _frostBombSlot = -1;

		private int _contagionSlot = -1;
		//private int _witherSlot = -1;

		private bool _isCasting;
		private int _castingSlot;

		private int _currentLeashRange = -1;

		private readonly Stopwatch _trapStopwatch = Stopwatch.StartNew();
		private readonly Stopwatch _totemStopwatch = Stopwatch.StartNew();
		private readonly Stopwatch _mineStopwatch = Stopwatch.StartNew();
		private readonly Stopwatch _animateWeaponStopwatch = Stopwatch.StartNew();
		private readonly Stopwatch _animateGuardianStopwatch = Stopwatch.StartNew();
		private readonly Stopwatch _moltenShellStopwatch = Stopwatch.StartNew();
		private readonly List<int> _ignoreAnimatedItems = new List<int>();
		private readonly Stopwatch _vaalStopwatch = Stopwatch.StartNew();
		private readonly Stopwatch _fleshOfferingStopwatch = Stopwatch.StartNew();
		private readonly Stopwatch _boneOfferingStopwatch = Stopwatch.StartNew();
		private readonly Stopwatch _convocationStopwatch = Stopwatch.StartNew();
		private readonly Stopwatch _orbOfStormsStopwatch = Stopwatch.StartNew();
		private readonly Stopwatch _frostBombStopwatch = Stopwatch.StartNew();

		private readonly Stopwatch _fuelCartStopWatch = Stopwatch.StartNew();

		private DateTime _nextExecuteTime = DateTime.Now;

		private int _summonSkeletonCount;
		private readonly Stopwatch _summonSkeletonsStopwatch = Stopwatch.StartNew();

		private readonly Stopwatch _summonGolemStopwatch = Stopwatch.StartNew();

		private int _summonRagingSpiritCount;
		private readonly Stopwatch _summonRagingSpiritStopwatch = Stopwatch.StartNew();

		private bool _castingFlameblast;
		private int _lastFlameblastCharges;
		private bool _needsUpdate;

		private readonly Targeting _combatTargeting = new Targeting();

		private Dictionary<string, Func<Tuple<object, string>[], object>> _exposedSettings;

		private Func<Rarity, int, bool> _flaskHook;
		
		// http://stackoverflow.com/a/824854
		private void RegisterExposedSettings()
		{
			if (_exposedSettings != null)
				return;

			_exposedSettings = new Dictionary<string, Func<Tuple<object, string>[], object>>();

			// Not a part of settings, so do it manually
			_exposedSettings.Add("SetLeash", param =>
			{
				_currentLeashRange = (int) param[0].Item1;
				return null;
			});

			_exposedSettings.Add("GetLeash", param =>
			{
				return _currentLeashRange;
			});

			// Automatically handle all settings

			PropertyInfo[] properties = typeof(OldRoutineSettings).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			foreach (PropertyInfo p in properties)
			{
				// Only work with ints
				if (p.PropertyType != typeof(int) && p.PropertyType != typeof(bool))
				{
					continue;
				}

				// If not writable then cannot null it; if not readable then cannot check it's value
				if (!p.CanWrite || !p.CanRead)
				{
					continue;
				}

				MethodInfo mget = p.GetGetMethod(false);
				MethodInfo mset = p.GetSetMethod(false);

				// Get and set methods have to be public
				if (mget == null)
				{
					continue;
				}
				if (mset == null)
				{
					continue;
				}

				Log.InfoFormat("Name: {0} ({1})", p.Name, p.PropertyType);

				_exposedSettings.Add("Set" + p.Name, param =>
				{
					p.SetValue(OldRoutineSettings.Instance, param[0]);
					return null;
				});

				_exposedSettings.Add("Get" + p.Name, param =>
				{
					return p.GetValue(OldRoutineSettings.Instance);
				});
			}
		}

		public Targeting CombatTargeting => _combatTargeting;

		// Do not implement a ctor and do stuff in it.

		#region Targeting

		private void CombatTargetingOnWeightCalculation(NetworkObject entity, ref float weight)
		{
			weight -= entity.Distance / 2;

			var m = entity as Monster;
			if (m == null)
				return;

			// If the monster is the source of Allies Cannot Die, we really want to kill it fast.
			if (m.HasAura("monster_aura_cannot_die"))
				weight += 50;

			/*if (m.IsTargetingMe)
			{
				weight += 20;
			}*/

			if (m.Rarity == Rarity.Magic)
			{
				weight += 5;
			}
			else if (m.Rarity == Rarity.Rare)
			{
				weight += 10;
			}
			else if (m.Rarity == Rarity.Unique)
			{
				weight += 15;
			}

			// Minions that get in the way.
			switch (m.Name)
			{
				case "Summoned Skeleton":
					weight -= 15;
					break;

				case "Raised Zombie":
					weight -= 15;
					break;
			}

			if (m.Rarity == Rarity.Normal && m.Type.Contains("/Totems/"))
			{
				weight -= 15;
			}

			// Necros
			if (m.ExplicitAffixes.Any(a => a.InternalName.Contains("RaisesUndead")) ||
				m.ImplicitAffixes.Any(a => a.InternalName.Contains("RaisesUndead")))
			{
				weight += 45;
			}

			// Ignore these mostly, as they just respawn.
			if (m.Type.Contains("TaniwhaTail"))
			{
				weight -= 30;
			}

			// Ignore mobs that expire and die
			if (m.Components.DiesAfterTimeComponent != null)
			{
				weight -= 15;
			}

			// Make sure hearts are targeted with highest priority.
			if (m.Type.Contains("/BeastHeart"))
			{
				weight += 75;
			}

			if (m.Metadata == "Metadata/Monsters/Tukohama/TukohamaShieldTotem")
			{
				weight += 75;
			}

			if (m.IsStrongboxMinion || m.IsHarbingerMinion)
			{
				weight += 30;
			}
		}

		private readonly string[] _aurasToIgnore = new[]
		{
			"shrine_godmode", // Ignore any mob near Divine Shrine
			"bloodlines_invulnerable", // Ignore Phylacteral Link
			"god_mode", // Ignore Animated Guardian
			"bloodlines_necrovigil",
		};

		private bool CombatTargetingOnInclusionCalcuation(NetworkObject entity)
		{
			try
			{
				var m = entity as Monster;
				if (m == null)
					return false;

				if (Blacklist.Contains(m))
					return false;

				// Do not consider inactive/dead mobs.
				if (!m.IsActive)
					return false;

				// Ignore any mob that cannot die.
				if (m.CannotDie)
					return false;

				// Ignore mobs that are too far to care about.
				if (m.Distance > (_currentLeashRange != -1 ? _currentLeashRange : OldRoutineSettings.Instance.CombatRange))
					return false;

				// Ignore mobs with special aura/buffs
				if (m.HasAura(_aurasToIgnore))
					return false;
				
				// Ignore these mobs when trying to transition in the dom fight.
				// Flag1 has been seen at 5 or 6 at times, so need to work out something more reliable.
				if (m.Name == "Miscreation")
				{
					var dom = LokiPoe.ObjectManager.GetObjectByName<Monster>("Dominus, High Templar");
					if (dom != null && !dom.IsDead &&
						(dom.Components.TransitionableComponent.Flag1 == 6 || dom.Components.TransitionableComponent.Flag1 == 5))
					{
						Blacklist.Add(m.Id, TimeSpan.FromHours(1), "Miscreation");
						return false;
					}
				}

				// Ignore Piety's portals.
				if (m.Name == "Chilling Portal" || m.Name == "Burning Portal")
				{
					Blacklist.Add(m.Id, TimeSpan.FromHours(1), "Piety portal");
					return false;
				}

				// Required for QB
				if (m.Metadata.Contains("DoedreStonePillar"))
				{
					Blacklist.Add(m.Id, TimeSpan.FromHours(1), "Doedre Pillar");
					return false;
				}
			}
			catch (Exception ex)
			{
				Log.Error("[CombatOnInclusionCalcuation]", ex);
				return false;
			}
			return true;
		}

		#endregion

		#region Implementation of IBase

		/// <summary>Initializes this routine.</summary>
		public void Initialize()
		{
			_combatTargeting.InclusionCalcuation += CombatTargetingOnInclusionCalcuation;
			_combatTargeting.WeightCalculation += CombatTargetingOnWeightCalculation;

			RegisterExposedSettings();
		}

		/// <summary> </summary>
		public void Deinitialize()
		{
		}

		#endregion

		#region Implementation of IAuthored

		/// <summary>The name of the routine.</summary>
		public string Name => "OldRoutine";

		/// <summary>The description of the routine.</summary>
		public string Description => "An example routine for Exilebuddy.";

		/// <summary>
		/// The author of this object.
		/// </summary>
		public string Author => "Bossland GmbH";

		/// <summary>
		/// The version of this routone.
		/// </summary>
		public string Version => "0.0.1.1";

		#endregion

		#region Implementation of ITickEvents / IStartStopEvents

		/// <summary> The routine start callback. Do any initialization here. </summary>
		public void Start()
		{
			_needsUpdate = true;

			if (OldRoutineSettings.Instance.SingleTargetMeleeSlot == -1 &&
				OldRoutineSettings.Instance.SingleTargetRangedSlot == -1 &&
				OldRoutineSettings.Instance.AoeMeleeSlot == -1 &&
				OldRoutineSettings.Instance.AoeRangedSlot == -1 &&
				OldRoutineSettings.Instance.FallbackSlot == -1
			)
			{
				Log.ErrorFormat(
					"[Start] Please configure the OldRoutine settings (Settings -> OldRoutine) before starting!");
				BotManager.Stop();
			}
		}

		private bool IsCastableHelper(Skill skill)
		{
			return skill != null && skill.IsCastable && !skill.IsTotem && !skill.IsTrap && !skill.IsMine;
		}

		private bool IsAuraName(string name)
		{
			// This makes sure auras on items don't get used, since they don't have skill gems, and won't have an Aura tag.
			if (!OldRoutineSettings.Instance.EnableAurasFromItems)
			{
				return false;
			}

			var auraNames = new string[]
			{
				"Anger", "Clarity", "Determination", "Discipline", "Grace", "Haste", "Hatred", "Purity of Elements",
				"Purity of Fire", "Purity of Ice", "Purity of Lightning", "Vitality", "Wrath", "Envy",
				"Aspect of the Crab", "Aspect of the Cat", "Aspect of the Avian", "Aspect of the Spider"
			};

			return auraNames.Contains(name);
		}

		private IEnumerable<Skill> NonBlacklistedSkills()
		{
			foreach(var skill in LokiPoe.InGameState.SkillBarHud.Skills)
			{
				if (!SkillBlacklist.IsBlacklisted(skill))
					yield return skill;
			}
		}

		/// <summary> The routine tick callback. Do any update logic here. </summary>
		public void Tick()
		{
			if (!LokiPoe.IsInGame)
				return;

			if (_needsUpdate)
			{
				_sgSlot = -1;
				_summonChaosGolemSlot = -1;
				_summonFlameGolemSlot = -1;
				_summonIceGolemSlot = -1;
				_summonStoneGolemSlot = -1;
				_summonLightningGolemSlot = -1;
				_raiseZombieSlot = -1;
				_raiseSpectreSlot = -1;
				_animateWeaponSlot = -1;
				_animateGuardianSlot = -1;
				_flameblastSlot = -1;
				_enduringCrySlot = -1;
				_moltenShellSlot = -1;
				_auraSlot = -1;
				_totemSlot = -1;
				_trapSlot = -1;
				_coldSnapSlot = -1;
				_contagionSlot = -1;
				//_witherSlot = -1;
				_bloodRageSlot = -1;
				_rfSlot = -1;
				_summonSkeletonsSlot = -1;
				_summonRagingSpiritSlot = -1;
				_summonSkeletonCount = 0;
				_summonRagingSpiritCount = 0;
				_mineSlot = -1;
				_fleshOfferingSlot = -1;
				_boneOfferingSlot = -1;
				_convocationSlot = -1;
				_orbOfStormsSlot = -1;
				_frostBombSlot = -1;
				_curseSlots.Clear();
				_totalCursesAllowed = LokiPoe.Me.TotalCursesAllowed;

				// All the logic below uses the skill slot, so let's only process skills on the bar itself.
				var skills = LokiPoe.InGameState.SkillBarHud.SkillBarSkills.Where(s => s != null).ToList();

				// Register stuff.
				foreach (var skill in skills)
				{
					var tags = skill.SkillTags;
					var name = skill.Name;

					if (tags.Contains("curse"))
					{
						var slot = skill.Slot;
						if (slot != -1 && skill.IsCastable && !skill.IsAurifiedCurse)
						{
							_curseSlots.Add(slot);
						}
					}

					if (_auraSlot == -1 &&
						((tags.Contains("aura") && !skill.IsVaalSkill) || IsAuraName(name) || skill.IsAurifiedCurse ||
						skill.IsConsideredAura))
					{
						if (!skill.IsTotem)
						{
							_auraSlot = skill.Slot;
						}
					}

					// Totem slot has to be a pure totem, and not a trapped or mined totem.
					if (skill.IsTotem && !skill.IsTrap && !skill.IsMine && _totemSlot == -1)
					{
						_totemSlot = skill.Slot;
					}

					if (skill.IsTrap && _trapSlot == -1)
					{
						_trapSlot = skill.Slot;
					}

					if (skill.IsMine && _mineSlot == -1)
					{
						_mineSlot = skill.Slot;
					}
				}
				
				var oos = skills.FirstOrDefault(s => s.InternalId == "orb_of_storms");
				if (IsCastableHelper(oos))
				{
					_orbOfStormsSlot = oos.Slot;
				}

				var fb = skills.FirstOrDefault(s => s.InternalId == "frost_bomb");
				if (IsCastableHelper(fb))
				{
					_frostBombSlot = fb.Slot;
				}

				var conv = skills.FirstOrDefault(s => s.InternalId == "convocation");
				if (IsCastableHelper(conv))
				{
					_convocationSlot = conv.Slot;
				}

				var fo = skills.FirstOrDefault(s => s.InternalId == "flesh_offering");
				if (IsCastableHelper(fo))
				{
					_fleshOfferingSlot = fo.Slot;
				}

				var bo = skills.FirstOrDefault(s => s.InternalId == "bone_offering");
				if (IsCastableHelper(bo))
				{
					_boneOfferingSlot = bo.Slot;
				}

				var cs = skills.FirstOrDefault(s => s.Name == "Cold Snap");
				if (IsCastableHelper(cs))
				{
					_coldSnapSlot = cs.Slot;
				}

				var con = skills.FirstOrDefault(s => s.Name == "Contagion");
				if (IsCastableHelper(con))
				{
					_contagionSlot = con.Slot;
				}

				/*var wither = skills.FirstOrDefault(s => s.Name == "Wither");
				if (IsCastableHelper(wither))
				{
					_witherSlot = wither.Slot;
				}*/

				var ss = skills.FirstOrDefault(s => s.InternalId == "summon_skeletons"); // Name changed 3.0, InternalId should be used for all skills.
				if (IsCastableHelper(ss))
				{
					_summonSkeletonsSlot = ss.Slot;
				}

				var srs = skills.FirstOrDefault(s => s.Name == "Summon Raging Spirit");
				if (IsCastableHelper(srs))
				{
					_summonRagingSpiritSlot = srs.Slot;
				}

				var rf = skills.FirstOrDefault(s => s.Name == "Righteous Fire");
				if (IsCastableHelper(rf))
				{
					_rfSlot = rf.Slot;
				}

				var br = skills.FirstOrDefault(s => s.Name == "Blood Rage");
				if (IsCastableHelper(br))
				{
					_bloodRageSlot = br.Slot;
				}

				var mc = skills.FirstOrDefault(s => s.Name == "Molten Shell");
				if (IsCastableHelper(mc))
				{
					_moltenShellSlot = mc.Slot;
				}

				var ec = skills.FirstOrDefault(s => s.Name == "Enduring Cry");
				if (IsCastableHelper(ec))
				{
					_enduringCrySlot = ec.Slot;
				}

				var scg = skills.FirstOrDefault(s => s.Name == "Summon Chaos Golem");
				if (IsCastableHelper(scg))
				{
					_summonChaosGolemSlot = scg.Slot;
					_sgSlot = _summonChaosGolemSlot;
				}

				var sig = skills.FirstOrDefault(s => s.Name == "Summon Ice Golem");
				if (IsCastableHelper(sig))
				{
					_summonIceGolemSlot = sig.Slot;
					_sgSlot = _summonIceGolemSlot;
				}

				var sfg = skills.FirstOrDefault(s => s.Name == "Summon Flame Golem");
				if (IsCastableHelper(sfg))
				{
					_summonFlameGolemSlot = sfg.Slot;
					_sgSlot = _summonFlameGolemSlot;
				}

				var ssg = skills.FirstOrDefault(s => s.Name == "Summon Stone Golem");
				if (IsCastableHelper(ssg))
				{
					_summonStoneGolemSlot = ssg.Slot;
					_sgSlot = _summonStoneGolemSlot;
				}

				var slg = skills.FirstOrDefault(s => s.Name == "Summon Lightning Golem");
				if (IsCastableHelper(slg))
				{
					_summonLightningGolemSlot = slg.Slot;
					_sgSlot = _summonLightningGolemSlot;
				}

				var rz = skills.FirstOrDefault(s => s.Name == "Raise Zombie");
				if (IsCastableHelper(rz))
				{
					_raiseZombieSlot = rz.Slot;
				}

				var rs = skills.FirstOrDefault(s => s.Name == "Raise Spectre");
				if (IsCastableHelper(rs))
				{
					_raiseSpectreSlot = rs.Slot;
				}

				fb = skills.FirstOrDefault(s => s.Name == "Flameblast");
				if (IsCastableHelper(fb))
				{
					_flameblastSlot = fb.Slot;
				}

				var ag = skills.FirstOrDefault(s => s.Name == "Animate Guardian");
				if (IsCastableHelper(ag))
				{
					_animateGuardianSlot = ag.Slot;
				}

				var aw = skills.FirstOrDefault(s => s.Name == "Animate Weapon");
				if (IsCastableHelper(aw))
				{
					_animateWeaponSlot = aw.Slot;
				}

				_needsUpdate = false;
			}
		}

		/// <summary> The routine stop callback. Do any pre-dispose cleanup here. </summary>
		public void Stop()
		{
		}

		#endregion

		#region Implementation of IConfigurable

		/// <summary> The bot's settings control. This will be added to the Exilebuddy Settings tab.</summary>
		public UserControl Control
		{
			get
			{

				using (
					var fs = new FileStream(
						Path.Combine(ThirdPartyLoader.GetInstance("Legacy").ContentPath, "OldRoutine", "SettingsGui.xaml"),
						FileMode.Open))
				{
					var root = (UserControl) XamlReader.Load(fs);

					// Your settings binding here.

					if (
						!Wpf.SetupCheckBoxBinding(root, "SkipShrinesCheckBox", "SkipShrines",
							BindingMode.TwoWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat(
							"[SettingsControl] SetupCheckBoxBinding failed for 'SkipShrinesCheckBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (
						!Wpf.SetupCheckBoxBinding(root, "LeaveFrameCheckBox", "LeaveFrame",
							BindingMode.TwoWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat(
							"[SettingsControl] SetupCheckBoxBinding failed for 'LeaveFrameCheckBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (
						!Wpf.SetupCheckBoxBinding(root, "EnableAurasFromItemsCheckBox", "EnableAurasFromItems",
							BindingMode.TwoWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat(
							"[SettingsControl] SetupCheckBoxBinding failed for 'EnableAurasFromItemsCheckBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (
						!Wpf.SetupCheckBoxBinding(root, "AlwaysAttackInPlaceCheckBox", "AlwaysAttackInPlace",
							BindingMode.TwoWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat(
							"[SettingsControl] SetupCheckBoxBinding failed for 'AlwaysAttackInPlaceCheckBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (
						!Wpf.SetupCheckBoxBinding(root, "DebugAurasCheckBox", "DebugAuras",
							BindingMode.TwoWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat(
							"[SettingsControl] SetupCheckBoxBinding failed for 'DebugAurasCheckBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (
						!Wpf.SetupCheckBoxBinding(root, "AutoCastVaalSkillsCheckBox", "AutoCastVaalSkills",
							BindingMode.TwoWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat(
							"[SettingsControl] SetupCheckBoxBinding failed for 'AutoCastVaalSkillsCheckBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (
						!Wpf.SetupComboBoxItemsBinding(root, "SingleTargetMeleeSlotComboBox", "AllSkillSlots",
							BindingMode.OneWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat(
							"[SettingsControl] SetupComboBoxItemsBinding failed for 'SingleTargetMeleeSlotComboBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (
						!Wpf.SetupComboBoxSelectedItemBinding(root, "SingleTargetMeleeSlotComboBox",
							"SingleTargetMeleeSlot", BindingMode.TwoWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat(
							"[SettingsControl] SetupComboBoxSelectedItemBinding failed for 'SingleTargetMeleeSlotComboBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (
						!Wpf.SetupComboBoxItemsBinding(root, "SingleTargetRangedSlotComboBox", "AllSkillSlots",
							BindingMode.OneWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat(
							"[SettingsControl] SetupComboBoxItemsBinding failed for 'SingleTargetRangedSlotComboBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (
						!Wpf.SetupComboBoxSelectedItemBinding(root, "SingleTargetRangedSlotComboBox",
							"SingleTargetRangedSlot", BindingMode.TwoWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat(
							"[SettingsControl] SetupComboBoxSelectedItemBinding failed for 'SingleTargetRangedSlotComboBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (
						!Wpf.SetupComboBoxItemsBinding(root, "AoeMeleeSlotComboBox", "AllSkillSlots",
							BindingMode.OneWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat(
							"[SettingsControl] SetupComboBoxItemsBinding failed for 'AoeMeleeSlotComboBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (
						!Wpf.SetupComboBoxSelectedItemBinding(root, "AoeMeleeSlotComboBox",
							"AoeMeleeSlot", BindingMode.TwoWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat(
							"[SettingsControl] SetupComboBoxSelectedItemBinding failed for 'AoeMeleeSlotComboBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (
						!Wpf.SetupComboBoxItemsBinding(root, "AoeRangedSlotComboBox", "AllSkillSlots",
							BindingMode.OneWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat(
							"[SettingsControl] SetupComboBoxItemsBinding failed for 'AoeRangedSlotComboBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (
						!Wpf.SetupComboBoxSelectedItemBinding(root, "AoeRangedSlotComboBox",
							"AoeRangedSlot", BindingMode.TwoWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat(
							"[SettingsControl] SetupComboBoxSelectedItemBinding failed for 'AoeRangedSlotComboBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (
						!Wpf.SetupComboBoxItemsBinding(root, "FallbackSlotComboBox", "AllSkillSlots",
							BindingMode.OneWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat(
							"[SettingsControl] SetupComboBoxItemsBinding failed for 'FallbackSlotComboBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (
						!Wpf.SetupComboBoxSelectedItemBinding(root, "FallbackSlotComboBox",
							"FallbackSlot", BindingMode.TwoWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat(
							"[SettingsControl] SetupComboBoxSelectedItemBinding failed for 'FallbackSlotComboBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (!Wpf.SetupTextBoxBinding(root, "CombatRangeTextBox", "CombatRange",
						BindingMode.TwoWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat("[SettingsControl] SetupTextBoxBinding failed for 'CombatRangeTextBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (!Wpf.SetupTextBoxBinding(root, "MaxMeleeRangeTextBox", "MaxMeleeRange",
						BindingMode.TwoWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat("[SettingsControl] SetupTextBoxBinding failed for 'MaxMeleeRangeTextBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (!Wpf.SetupTextBoxBinding(root, "MaxRangeRangeTextBox", "MaxRangeRange",
						BindingMode.TwoWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat("[SettingsControl] SetupTextBoxBinding failed for 'MaxRangeRangeTextBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (!Wpf.SetupTextBoxBinding(root, "MaxFlameBlastChargesTextBox", "MaxFlameBlastCharges",
						BindingMode.TwoWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat(
							"[SettingsControl] SetupTextBoxBinding failed for 'MaxFlameBlastChargesTextBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (!Wpf.SetupTextBoxBinding(root, "MoltenShellDelayMsTextBox", "MoltenShellDelayMs",
						BindingMode.TwoWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat("[SettingsControl] SetupTextBoxBinding failed for 'MoltenShellDelayMsTextBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (!Wpf.SetupTextBoxBinding(root, "TotemDelayMsTextBox", "TotemDelayMs",
						BindingMode.TwoWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat(
							"[SettingsControl] SetupTextBoxBinding failed for 'TotemDelayMsTextBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (!Wpf.SetupTextBoxBinding(root, "TrapDelayMsTextBox", "TrapDelayMs",
						BindingMode.TwoWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat(
							"[SettingsControl] SetupTextBoxBinding failed for 'TrapDelayMsTextBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (
						!Wpf.SetupTextBoxBinding(root, "SummonRagingSpiritCountPerDelayTextBox",
							"SummonRagingSpiritCountPerDelay",
							BindingMode.TwoWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat(
							"[SettingsControl] SetupTextBoxBinding failed for 'SummonRagingSpiritCountPerDelayTextBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (!Wpf.SetupTextBoxBinding(root, "SummonRagingSpiritDelayMsTextBox", "SummonRagingSpiritDelayMs",
						BindingMode.TwoWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat(
							"[SettingsControl] SetupTextBoxBinding failed for 'SummonRagingSpiritDelayMsTextBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (
						!Wpf.SetupTextBoxBinding(root, "SummonSkeletonCountPerDelayTextBox",
							"SummonSkeletonCountPerDelay",
							BindingMode.TwoWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat(
							"[SettingsControl] SetupTextBoxBinding failed for 'SummonSkeletonCountPerDelayTextBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (!Wpf.SetupTextBoxBinding(root, "SummonSkeletonDelayMsTextBox", "SummonSkeletonDelayMs",
						BindingMode.TwoWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat(
							"[SettingsControl] SetupTextBoxBinding failed for 'SummonSkeletonDelayMsTextBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					if (!Wpf.SetupTextBoxBinding(root, "MineDelayMsTextBox", "MineDelayMs",
						BindingMode.TwoWay, OldRoutineSettings.Instance))
					{
						Log.DebugFormat(
							"[SettingsControl] SetupTextBoxBinding failed for 'MineDelayMsTextBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					// Your settings event handlers here.

					return root;
				}
			}
		}

		/// <summary>The settings object. This will be registered in the current configuration.</summary>
		public JsonSettings Settings => OldRoutineSettings.Instance;

		#endregion

		#region Implementation of ILogicProvider

		public async Task<bool> TryUseAura(Skill skill)
		{
			var doCast = true;

			while (skill.Slot == -1)
			{
				Log.InfoFormat("[TryUseAura] Now assigning {0}|{1} to the skillbar at slot {2}.", skill.Name, skill.Id, _auraSlot);

				var sserr = LokiPoe.InGameState.SkillBarHud.SetSlot(_auraSlot, skill);

				if (sserr != LokiPoe.InGameState.SetSlotResult.None)
				{
					Log.ErrorFormat("[TryUseAura] SetSlot returned {0}.", sserr);

					doCast = false;

					break;
				}

				await Coroutines.LatencyWait();

				await Coroutine.Sleep(1000);
			}

			if (!doCast)
			{
				return false;
			}

			await Coroutines.FinishCurrentAction();

			await Coroutines.LatencyWait();

			var err1 = LokiPoe.InGameState.SkillBarHud.Use(skill.Slot, false);
			if (err1 == LokiPoe.InGameState.UseResult.None)
			{
				await Coroutines.LatencyWait();

				await Coroutines.FinishCurrentAction(false);

				await Coroutines.LatencyWait();

				return true;
			}

			Log.ErrorFormat("[TryUseAura] Use returned {0} for {1}.", err1, skill.Name);

			return false;
		}

		private TimeSpan EnduranceChargesTimeLeft
		{
			get
			{
				Aura aura = LokiPoe.Me.EnduranceChargeAura;
				if (aura != null)
				{
					return aura.TimeLeft;
				}

				return TimeSpan.Zero;
			}
		}

		/// <summary>
		/// Implements the ability to handle a logic passed through the system.
		/// </summary>
		/// <param name="logic">The logic to be processed.</param>
		/// <returns>A LogicResult that describes the result..</returns>
		public async Task<LogicResult> Logic(Logic logic)
		{
			if (logic.Id == "hook_combat")
			{
				// We need to not execute any combat logic for a time to prevent getting stuck on infinite spawning mobs for a quest.
				// This is a temp hack to address a design issue with the new 3.1.0 game change.
				if (DateTime.Now < _nextExecuteTime)
				{
					return LogicResult.Unprovided;
				}

				/*if (LokiPoe.Me.IsAbilityCooldownActive)
				{
					Log.Info("IsAbilityCooldownActive");
					return LogicResult.Provided;
				}*/

				// Player is immobalized by a lab spike trap.
				// spike_trap_immunity

				// Update targeting.
				//using (new PerformanceTimer("CombatTargeting.Update", 1))
				{
					CombatTargeting.Update();
				}

				// We now signal always highlight needs to be disabled, but won't actually do it until we cast something.
				if (
					LokiPoe.ObjectManager.GetObjectsByType<Chest>()
						.Any(c => c.Distance < 70 && !c.IsOpened && c.IsStrongBox))
				{
					_needsToDisableAlwaysHighlight = true;
				}
				else
				{
					_needsToDisableAlwaysHighlight = false;
				}

				var myPos = LokiPoe.MyPosition;

				// If we have flameblast, we need to use special logic for it.
				if (_flameblastSlot != -1)
				{
					if (_castingFlameblast)
					{
						var c = LokiPoe.Me.FlameblastCharges;

						// Handle being cast interrupted.
						if (c < _lastFlameblastCharges)
						{
							_castingFlameblast = false;
							return LogicResult.Provided;
						}
						_lastFlameblastCharges = c;

						if (c >= OldRoutineSettings.Instance.MaxFlameBlastCharges)
						{
							// Stop using the skill, so it's cast.
							await Coroutines.FinishCurrentAction();

							_castingFlameblast = false;
						}
						else
						{
							await DisableAlwaysHiglight();

							// Keep using the skill to build up charges.
							var buaerr = LokiPoe.InGameState.SkillBarHud.UseAt(_flameblastSlot, false,
								myPos);
							if (buaerr != LokiPoe.InGameState.UseResult.None)
							{
								Log.ErrorFormat("[Logic] UseAt returned {0} for {1}.", buaerr, "Flameblast");
							}
						}

						return LogicResult.Provided;
					}
				}

				// Limit this logic once per second, because it can get expensive and slow things down if run too fast.
				if (_animateGuardianSlot != -1 && _animateGuardianStopwatch.ElapsedMilliseconds > 1000)
				{
					// See if we can use the skill.
					var skill = LokiPoe.InGameState.SkillBarHud.Slot(_animateGuardianSlot);
					if (skill.CanUse())
					{
						// Check for a target near us.
						var target = BestAnimateGuardianTarget(skill.DeployedObjects.FirstOrDefault() as Monster,
							skill.GetStat(StatTypeGGG.AnimateItemMaximumLevelRequirement));
						if (target != null)
						{
							var skillName = skill.Name;

							Log.InfoFormat("[Logic] Using {0} on {1}.", skillName, target.Name);

							await DisableAlwaysHiglight();

							await Coroutines.FinishCurrentAction();

							var uaerr = LokiPoe.InGameState.SkillBarHud.UseOn(_animateGuardianSlot, true, target);
							if (uaerr == LokiPoe.InGameState.UseResult.None)
							{
								await Coroutines.LatencyWait();

								await Coroutines.FinishCurrentAction(false);

								await Coroutines.LatencyWait();

								// We need to remove the item highlighting.
								LokiPoe.ProcessHookManager.ClearAllKeyStates();

								return LogicResult.Provided;
							}

							Log.ErrorFormat("[Logic] UseOn returned {0} for {1}.", uaerr, skillName);
						}

						_animateGuardianStopwatch.Restart();
					}
				}

				// Limit this logic once per second, because it can get expensive and slow things down if run too fast.
				if (_animateWeaponSlot != -1 && _animateWeaponStopwatch.ElapsedMilliseconds > 1000)
				{
					// See if we can use the skill.
					var skill = LokiPoe.InGameState.SkillBarHud.Slot(_animateWeaponSlot);
					if (skill.CanUse())
					{
						// Check for a target near us.
						var target = BestAnimateWeaponTarget(skill.GetStat(StatTypeGGG.AnimateItemMaximumLevelRequirement));
						if (target != null)
						{
							var skillName = skill.Name;

							Log.InfoFormat("[Logic] Using {0} on {1}.", skillName, target.Name);

							await DisableAlwaysHiglight();

							await Coroutines.FinishCurrentAction();

							var uaerr = LokiPoe.InGameState.SkillBarHud.UseOn(_animateWeaponSlot, true, target);
							if (uaerr == LokiPoe.InGameState.UseResult.None)
							{
								await Coroutines.LatencyWait();

								await Coroutines.FinishCurrentAction(false);

								await Coroutines.LatencyWait();

								// We need to remove the item highlighting.
								LokiPoe.ProcessHookManager.ClearAllKeyStates();

								_animateWeaponStopwatch.Restart();

								return LogicResult.Provided;
							}

							Log.ErrorFormat("[Logic] UseOn returned {0} for {1}.", uaerr, skillName);
						}

						_animateWeaponStopwatch.Restart();
					}
				}

				// If we have Raise Spectre, we can look for dead bodies to use for our army as we move around.
				if (_raiseSpectreSlot != -1)
				{
					// See if we can use the skill.
					var skill = LokiPoe.InGameState.SkillBarHud.Slot(_raiseSpectreSlot);
					if (skill.CanUse())
					{
						var max = skill.GetStat(StatTypeGGG.NumberOfSpectresAllowed);
						if (skill.NumberDeployed < max)
						{
							// Check for a target near us.
							var target = BestDeadTarget;
							if (target != null)
							{
								var skillName = skill.Name;

								Log.InfoFormat("[Logic] Using {0} on {1}.", skillName, target.Name);

								await DisableAlwaysHiglight();

								await Coroutines.FinishCurrentAction();

								var uaerr = LokiPoe.InGameState.SkillBarHud.UseAt(_raiseSpectreSlot, false,
									target.Position);
								if (uaerr == LokiPoe.InGameState.UseResult.None)
								{
									await Coroutines.LatencyWait();

									await Coroutines.FinishCurrentAction(false);

									await Coroutines.LatencyWait();

									return LogicResult.Provided;
								}

								Log.ErrorFormat("[Logic] UseAt returned {0} for {1}.", uaerr, skillName);
							}
						}
					}
				}

				// If we have a Summon Golem skill, we can cast it if we havn't cast it recently.
				if (_sgSlot != -1 &&
					_summonGolemStopwatch.ElapsedMilliseconds >
					10000)
				{
					// See if we can use the skill.
					var skill = LokiPoe.InGameState.SkillBarHud.Slot(_sgSlot);
					if (skill.CanUse())
					{
						var max = skill.GetStat(StatTypeGGG.NumberOfGolemsAllowed);
						if (skill.NumberDeployed < max)
						{
							var skillName = skill.Name;
							
							await DisableAlwaysHiglight();

							await Coroutines.FinishCurrentAction();

							var err1 = LokiPoe.InGameState.SkillBarHud.UseAt(_sgSlot, true, myPos);
							if (err1 == LokiPoe.InGameState.UseResult.None)
							{
								_summonGolemStopwatch.Restart();

								await Coroutines.FinishCurrentAction(false);

								return LogicResult.Provided;
							}

							Log.ErrorFormat("[Logic] UseAt returned {0} for {1}.", err1, skillName);
						}
					}
				}

				// If we have Raise Zombie, we can look for dead bodies to use for our army as we move around.
				if (_raiseZombieSlot != -1)
				{
					// See if we can use the skill.
					var skill = LokiPoe.InGameState.SkillBarHud.Slot(_raiseZombieSlot);
					if (skill.CanUse())
					{
						var max = skill.GetStat(StatTypeGGG.NumberOfZombiesAllowed);
						if (skill.NumberDeployed < max)
						{
							// Check for a target near us.
							var target = BestDeadTarget;
							if (target != null)
							{
								var skillName = skill.Name;
								var targetName = target.Name;
								var pos = target.Position;

								await DisableAlwaysHiglight();

								await Coroutines.FinishCurrentAction();

								Log.InfoFormat("[Logic] Using {0} on {1}.", skillName, targetName);

								var uaerr = LokiPoe.InGameState.SkillBarHud.UseAt(_raiseZombieSlot, false,
									pos);
								if (uaerr == LokiPoe.InGameState.UseResult.None)
								{
									await Coroutines.LatencyWait();

									await Coroutines.FinishCurrentAction(false);

									await Coroutines.LatencyWait();

									return LogicResult.Provided;
								}

								Log.ErrorFormat("[Logic] UseAt returned {0} for {1}.", uaerr, skillName);
							}
						}
					}
				}

				if (_convocationSlot != -1)
				{
					// See if we can use the skill.
					var skill = LokiPoe.InGameState.SkillBarHud.Slot(_convocationSlot);
					if (skill.CanUse())
					{
						if (_convocationStopwatch.ElapsedMilliseconds > 10000)
						{
							var skillName = skill.Name;

							Log.InfoFormat("[Logic] Using {0}.", skill.Name);

							await DisableAlwaysHiglight();

							await Coroutines.FinishCurrentAction();
							
							var uaerr = LokiPoe.InGameState.SkillBarHud.Use(_convocationSlot, true);
							if (uaerr == LokiPoe.InGameState.UseResult.None)
							{
								await Coroutines.LatencyWait();

								await Coroutines.FinishCurrentAction(false);

								await Coroutines.LatencyWait();

								_convocationStopwatch.Restart();

								return LogicResult.Provided;
							}

							Log.ErrorFormat("[Logic] Use returned {0} for {1}.", uaerr, skillName);
						}
					}
				}

				if (_fleshOfferingSlot != -1)
				{
					// See if we can use the skill.
					var skill = LokiPoe.InGameState.SkillBarHud.Slot(_fleshOfferingSlot);
					if (skill.CanUse())
					{
						if (_fleshOfferingStopwatch.ElapsedMilliseconds > 5000)
						{
							// Check for a target near us.
							var target = BestDeadTarget;
							if (target != null)
							{
								var skillName = skill.Name;

								Log.InfoFormat("[Logic] Using {0} on {1}.", skillName, target.Name);

								await DisableAlwaysHiglight();

								await Coroutines.FinishCurrentAction();
								
								var uaerr = LokiPoe.InGameState.SkillBarHud.UseAt(_fleshOfferingSlot, false,
									target.Position);
								if (uaerr == LokiPoe.InGameState.UseResult.None)
								{
									await Coroutines.LatencyWait();

									await Coroutines.FinishCurrentAction(false);

									await Coroutines.LatencyWait();

									_fleshOfferingStopwatch.Restart();

									return LogicResult.Provided;
								}

								Log.ErrorFormat("[Logic] UseAt returned {0} for {1}.", uaerr, skillName);
							}
						}
					}
				}

				if (_boneOfferingSlot != -1)
				{
					// See if we can use the skill.
					var skill = LokiPoe.InGameState.SkillBarHud.Slot(_boneOfferingSlot);
					if (skill.CanUse())
					{
						if (_boneOfferingStopwatch.ElapsedMilliseconds > 5000)
						{
							// Check for a target near us.
							var target = BestDeadTarget;
							if (target != null)
							{
								var skillName = skill.Name;

								Log.InfoFormat("[Logic] Using {0} on {1}.", skillName, target.Name);

								await DisableAlwaysHiglight();

								await Coroutines.FinishCurrentAction();

								var uaerr = LokiPoe.InGameState.SkillBarHud.UseAt(_boneOfferingSlot, false,
									target.Position);
								if (uaerr == LokiPoe.InGameState.UseResult.None)
								{
									await Coroutines.LatencyWait();

									await Coroutines.FinishCurrentAction(false);

									await Coroutines.LatencyWait();

									_boneOfferingStopwatch.Restart();

									return LogicResult.Provided;
								}

								Log.ErrorFormat("[Logic] UseAt returned {0} for {1}.", uaerr, skillName);
							}
						}
					}
				}

				// Since EC has a cooldown, we can just cast it when mobs are in range to keep our endurance charges refreshed.
				if (_enduringCrySlot != -1)
				{
					// See if we can use the skill.
					var skill = LokiPoe.InGameState.SkillBarHud.Slot(_enduringCrySlot);
					if (skill.CanUse())
					{
						if (EnduranceChargesTimeLeft.TotalSeconds < 5 && NumberOfMobsNear(LokiPoe.Me, 30) > 0)
						{
							var skillName = skill.Name;
							
							await Coroutines.FinishCurrentAction();

							var err1 = LokiPoe.InGameState.SkillBarHud.Use(_enduringCrySlot, true);
							if (err1 == LokiPoe.InGameState.UseResult.None)
							{
								await Coroutines.LatencyWait();

								await Coroutines.FinishCurrentAction(false);

								await Coroutines.LatencyWait();

								return LogicResult.Provided;
							}

							Log.ErrorFormat("[Logic] Use returned {0} for {1}.", err1, skillName);
						}
					}
				}

				// For Molten Shell, we want to limit cast time, since mobs that break the shield often would cause the CR to cast it over and over.
				if (_moltenShellSlot != -1 &&
					_moltenShellStopwatch.ElapsedMilliseconds >= OldRoutineSettings.Instance.MoltenShellDelayMs)
				{
					// See if we can use the skill.
					var skill = LokiPoe.InGameState.SkillBarHud.Slot(_moltenShellSlot);
					if (!LokiPoe.Me.HasMoltenShellBuff && skill.CanUse())
					{
						var skillName = skill.Name;
						
						if (NumberOfMobsNear(LokiPoe.Me, OldRoutineSettings.Instance.CombatRange) > 0)
						{
							await Coroutines.FinishCurrentAction();

							var err1 = LokiPoe.InGameState.SkillBarHud.Use(_moltenShellSlot, true);

							_moltenShellStopwatch.Restart();

							if (err1 == LokiPoe.InGameState.UseResult.None)
							{
								await Coroutines.LatencyWait();

								await Coroutines.FinishCurrentAction(false);

								await Coroutines.LatencyWait();

								return LogicResult.Provided;
							}

							Log.ErrorFormat("[Logic] Use returned {0} for {1}.", err1, skillName);
						}
					}
				}

				// Handle aura logic, but only if we don't have mobs near us, since we don't want to die.
				if (_auraSlot != -1)
				{
					foreach (var skill in NonBlacklistedSkills().ToList())
					{
						// Don't use totemized auras on the aura slot!
						if (skill.IsTotem)
							continue;

						if (skill.IsAurifiedCurse)
						{
							if (!skill.AmICursingWithThis && skill.CanUse(OldRoutineSettings.Instance.DebugAuras, true))
							{
								if (NumberOfMobsNear(LokiPoe.Me, 60) != 0)
									break;

								if (await TryUseAura(skill))
								{
									return LogicResult.Provided;
								}
							}
						}
						else if (skill.IsConsideredAura)
						{
							if (!skill.AmIUsingConsideredAuraWithThis && skill.CanUse(OldRoutineSettings.Instance.DebugAuras, true))
							{
								if (NumberOfMobsNear(LokiPoe.Me, 60) != 0)
									break;

								if (await TryUseAura(skill))
								{
									return LogicResult.Provided;
								}
							}
						}
						else if ((skill.SkillTags.Contains("aura") && !skill.IsVaalSkill) || IsAuraName(skill.Name))
						{
							if (!LokiPoe.Me.HasAura(skill.Name) && skill.CanUse(OldRoutineSettings.Instance.DebugAuras, true))
							{
								if (NumberOfMobsNear(LokiPoe.Me, 60) != 0)
									break;

								if (await TryUseAura(skill))
								{
									return LogicResult.Provided;
								}
							}
						}
					}
				}

				// Check for a surround to use flameblast, just example logic.
				if (_flameblastSlot != -1)
				{
					if (NumberOfMobsNear(LokiPoe.Me, 15) >= 4)
					{
						_castingFlameblast = true;
						_lastFlameblastCharges = 0;
						return LogicResult.Provided;
					}
				}

				// TODO: _currentLeashRange of -1 means we need to use a cached location system to prevent back and forth issues of mobs despawning.

				// This is pretty important. Otherwise, components can go invalid and exceptions are thrown.
				var bestTarget = CombatTargeting.Targets<Monster>().FirstOrDefault();

				// No monsters, we can execute non-critical combat logic, like buffs, auras, etc...
				// For this example, just going to continue executing bot logic.
				if (bestTarget == null)
				{
					if (await HandleShrines())
					{
						return LogicResult.Provided;
					}

					return await CombatLogicEnd();
				}

				// 3.1.0+
				var canMove = true;

				// The Beacon
				if (LokiPoe.LocalData.WorldArea.Id == "2_6_14")
				{
					// If we're close by a fuel cart that is being moved, we don't want the CR to move away from it.
					// This is an ugly hack to solve a problem introduced in 3.1.0+.
					var fuelCarts = LokiPoe.ObjectManager.GetObjectsByMetadata("Metadata/QuestObjects/Act6/BeaconPayload").ToList();
					foreach (var fuelCart in fuelCarts)
					{
						if (fuelCart.Components.TransitionableComponent.Flag2 == 1 && fuelCart.Distance < 30)
						{
							canMove = false;
							if (_fuelCartStopWatch.ElapsedMilliseconds > 3000) // Allow 3s of combat
							{
								_fuelCartStopWatch.Restart();
								_nextExecuteTime = DateTime.Now + TimeSpan.FromMilliseconds(1000); // Then skip combat for 1s
								return LogicResult.Provided;
							}
							break;
						}
					}
				}

				var cachedPosition = bestTarget.Position;
				var targetPosition = bestTarget.InteractCenterWorld;
				var cachedId = bestTarget.Id;
				var cachedName = bestTarget.Name;
				var cachedRarity = bestTarget.Rarity;
				var cachedDistance = bestTarget.Distance;
				var cachedIsCursable = bestTarget.IsCursable;
				var cachedCurseCount = bestTarget.CurseCount;
				var cachedHasCurseFrom = new Dictionary<string, bool>();
				var cachedNumberOfMobsNear = NumberOfMobsNear(bestTarget, 20);
				var cachedProxShield = bestTarget.HasProximityShield;
				var cachedContagion = bestTarget.HasContagion;
				var cachedWither = bestTarget.HasWither;
				var cachedMobsNearForAoe = NumberOfMobsNear(LokiPoe.Me,
					OldRoutineSettings.Instance.MaxMeleeRange);
				var cachedMobsNearForCurse = NumberOfMobsNear(bestTarget, 20);
                var cachedHpPercent = (int) bestTarget.HealthPercentTotal;
				
				foreach (var curseSlot in _curseSlots)
				{
					var skill = LokiPoe.InGameState.SkillBarHud.Slot(curseSlot);
					cachedHasCurseFrom.Add(skill.Name, bestTarget.HasCurseFrom(skill.Name));
				}

				if (await HandleShrines())
				{
					return LogicResult.Provided;
				}

				var canSee = ExilePather.CanObjectSee(LokiPoe.Me, bestTarget, !OldRoutineSettings.Instance.LeaveFrame);
				var pathDistance = ExilePather.PathDistance(myPos, cachedPosition, false, !OldRoutineSettings.Instance.LeaveFrame);
				var blockedByDoor = ClosedDoorBetween(LokiPoe.Me, bestTarget, 10, 10,
					!OldRoutineSettings.Instance.LeaveFrame);

				var skipPathing = cachedRarity == Rarity.Unique &&
								(bestTarget.Metadata.Contains("KitavaBoss/Kitava") || bestTarget.Metadata.Contains("VaalSpiderGod/Arakaali"));

				if (pathDistance.CompareTo(float.MaxValue) == 0 && !skipPathing)
				{
					Log.ErrorFormat(
						"[Logic] Could not determine the path distance to the best target. Now blacklisting it.");
					Blacklist.Add(cachedId, TimeSpan.FromMinutes(1), "Unable to pathfind to.");
					return LogicResult.Provided;
				}

				// Prevent combat loops from happening by preventing combat outside CombatRange.
				if (pathDistance > OldRoutineSettings.Instance.CombatRange && !skipPathing)
				{
					await EnableAlwaysHiglight();

					return LogicResult.Unprovided;
				}

				if ((!canSee && !skipPathing) || blockedByDoor)
				{
					if (!canMove)
					{
						Log.InfoFormat("[Logic] Not moving towards the target because we should not move currently.");

						return LogicResult.Unprovided;
					}
					else
					{
						Log.InfoFormat(
							"[Logic] Now moving towards the monster {0} because [canSee: {1}][pathDistance: {2}][blockedByDoor: {3}]",
							cachedName, canSee, pathDistance, blockedByDoor);

						if (!PlayerMoverManager.MoveTowards(cachedPosition))
						{
							Log.ErrorFormat("[Logic] MoveTowards failed for {0}.", cachedPosition);
							await Coroutines.FinishCurrentAction();
						}

						return LogicResult.Provided;
					}
				}

				if (_frostBombSlot != -1)
				{
					// See if we can use the skill.
					var skill = LokiPoe.InGameState.SkillBarHud.Slot(_frostBombSlot);
					if (skill.CanUse())
					{
						if (_frostBombStopwatch.ElapsedMilliseconds > 3500)
						{
							var skillName = skill.Name;

							Log.InfoFormat("[Logic] Using {0}.", skill.Name);

							await DisableAlwaysHiglight();

							await Coroutines.FinishCurrentAction();

							var uaerr = LokiPoe.InGameState.SkillBarHud.Use(_frostBombSlot, true);
							if (uaerr == LokiPoe.InGameState.UseResult.None)
							{
								await Coroutines.LatencyWait();

								await Coroutines.FinishCurrentAction(false);

								await Coroutines.LatencyWait();

								_frostBombStopwatch.Restart();

								return LogicResult.Provided;
							}

							Log.ErrorFormat("[Logic] Use returned {0} for {1}.", uaerr, skillName);
						}
					}
				}

				if (_orbOfStormsSlot != -1)
				{
					// See if we can use the skill.
					var skill = LokiPoe.InGameState.SkillBarHud.Slot(_orbOfStormsSlot);
					if (skill.CanUse())
					{
						if (_orbOfStormsStopwatch.ElapsedMilliseconds > 2000)
						{
							var skillName = skill.Name;

							Log.InfoFormat("[Logic] Using {0}.", skill.Name);

							await DisableAlwaysHiglight();

							await Coroutines.FinishCurrentAction();

							var uaerr = LokiPoe.InGameState.SkillBarHud.Use(_orbOfStormsSlot, true);
							if (uaerr == LokiPoe.InGameState.UseResult.None)
							{
								await Coroutines.LatencyWait();

								await Coroutines.FinishCurrentAction(false);

								await Coroutines.LatencyWait();

								_orbOfStormsStopwatch.Restart();

								return LogicResult.Provided;
							}

							Log.ErrorFormat("[Logic] Use returned {0} for {1}.", uaerr, skillName);
						}
					}
				}

				// Handle totem logic.
				if (_totemSlot != -1 &&
					_totemStopwatch.ElapsedMilliseconds > OldRoutineSettings.Instance.TotemDelayMs)
				{
					var skill = LokiPoe.InGameState.SkillBarHud.Slot(_totemSlot);
					if (skill.CanUse() &&
						skill.DeployedObjects.Select(o => o as Monster).Count(t => !t.IsDead && t.Distance < 60) <
						LokiPoe.Me.MaxTotemCount)
					{
						var skillName = skill.Name;
						
						await DisableAlwaysHiglight();

						await Coroutines.FinishCurrentAction();

						var err1 = LokiPoe.InGameState.SkillBarHud.UseAt(_totemSlot, true,
							myPos.GetPointAtDistanceAfterThis(cachedPosition,
								cachedDistance / 2));

						_totemStopwatch.Restart();

						if (err1 == LokiPoe.InGameState.UseResult.None)
							return LogicResult.Provided;

						Log.ErrorFormat("[Logic] UseAt returned {0} for {1}.", err1, skillName);
					}
				}

				// Handle trap logic.
				if (_trapSlot != -1 &&
					_trapStopwatch.ElapsedMilliseconds > OldRoutineSettings.Instance.TrapDelayMs)
				{
					var skill = LokiPoe.InGameState.SkillBarHud.Slot(_trapSlot);
					if (skill.CanUse())
					{
						var skillName = skill.Name;
						
						await DisableAlwaysHiglight();

						await Coroutines.FinishCurrentAction();

						var err1 = LokiPoe.InGameState.SkillBarHud.UseAt(_trapSlot, true,
							myPos.GetPointAtDistanceAfterThis(cachedPosition,
								cachedDistance / 2));

						_trapStopwatch.Restart();

						if (err1 == LokiPoe.InGameState.UseResult.None)
							return LogicResult.Provided;

						Log.ErrorFormat("[Logic] UseAt returned {0} for {1}.", err1, skillName);
					}
				}

				// Handle curse logic - curse magic+ and packs of 4+, but only cast within MaxRangeRange.
				var checkCurses = myPos.Distance(cachedPosition) < OldRoutineSettings.Instance.MaxRangeRange &&
								(cachedRarity >= Rarity.Magic || cachedMobsNearForCurse >= 3);
				if (checkCurses)
				{
					foreach (var curseSlot in _curseSlots)
					{
						var skill = LokiPoe.InGameState.SkillBarHud.Slot(curseSlot);
						if (skill != null && skill.CanUse() &&
							cachedIsCursable &&
							cachedCurseCount < _totalCursesAllowed &&
							!cachedHasCurseFrom[skill.Name])
						{
							var skillName = skill.Name;
							
							await DisableAlwaysHiglight();

							await Coroutines.FinishCurrentAction();

							var err1 = LokiPoe.InGameState.SkillBarHud.UseAt(curseSlot, true, cachedPosition);
							if (err1 == LokiPoe.InGameState.UseResult.None)
								return LogicResult.Provided;

							Log.ErrorFormat("[Logic] Use returned {0} for {1}.", err1, skillName);
						}
					}
				}

				// Simply cast Blood Rage if we have it.
				if (_bloodRageSlot != -1)
				{
					// See if we can use the skill.
					var skill = LokiPoe.InGameState.SkillBarHud.Slot(_bloodRageSlot);
					if (skill.CanUse() && !LokiPoe.Me.HasBloodRageBuff && cachedDistance < OldRoutineSettings.Instance.CombatRange)
					{
						var skillName = skill.Name;
						
						await Coroutines.FinishCurrentAction();

						var err1 = LokiPoe.InGameState.SkillBarHud.Use(_bloodRageSlot, true);
						if (err1 == LokiPoe.InGameState.UseResult.None)
						{
							await Coroutines.LatencyWait();

							await Coroutines.FinishCurrentAction(false);

							await Coroutines.LatencyWait();

							return LogicResult.Provided;
						}

						Log.ErrorFormat("[Logic] Use returned {0} for {1}.", err1, skillName);
					}
				}

				// Simply cast RF if we have it.
				if (_rfSlot != -1)
				{
					// See if we can use the skill.
					var skill = LokiPoe.InGameState.SkillBarHud.Slot(_rfSlot);
					if (skill.CanUse() && !LokiPoe.Me.HasRighteousFireBuff)
					{
						var skillName = skill.Name;
						
						await Coroutines.FinishCurrentAction();

						var err1 = LokiPoe.InGameState.SkillBarHud.Use(_rfSlot, true);
						if (err1 == LokiPoe.InGameState.UseResult.None)
						{
							await Coroutines.LatencyWait();

							await Coroutines.FinishCurrentAction(false);

							await Coroutines.LatencyWait();

							return LogicResult.Provided;
						}

						Log.ErrorFormat("[Logic] Use returned {0} for {1}.", err1, skillName);
					}
				}

				if (_summonRagingSpiritSlot != -1 &&
					_summonRagingSpiritStopwatch.ElapsedMilliseconds >
					OldRoutineSettings.Instance.SummonRagingSpiritDelayMs)
				{
					var skill = LokiPoe.InGameState.SkillBarHud.Slot(_summonRagingSpiritSlot);
					var max = skill.GetStat(StatTypeGGG.NumberOfRagingSpiritsAllowed);
					if (skill.NumberDeployed < max && skill.CanUse())
					{
						var skillName = skill.Name;
						
						++_summonRagingSpiritCount;

						await Coroutines.FinishCurrentAction();

						var err1 = LokiPoe.InGameState.SkillBarHud.UseAt(_summonRagingSpiritSlot, false, targetPosition);

						if (_summonRagingSpiritCount >=
							OldRoutineSettings.Instance.SummonRagingSpiritCountPerDelay)
						{
							_summonRagingSpiritCount = 0;
							_summonRagingSpiritStopwatch.Restart();
						}

						if (err1 == LokiPoe.InGameState.UseResult.None)
						{
							await Coroutines.LatencyWait();

							await Coroutines.FinishCurrentAction(false);

							await Coroutines.LatencyWait();

							return LogicResult.Provided;
						}

						Log.ErrorFormat("[Logic] Use returned {0} for {1}.", err1, skillName);
					}
				}

				if (_summonSkeletonsSlot != -1 &&
					_summonSkeletonsStopwatch.ElapsedMilliseconds >
					OldRoutineSettings.Instance.SummonSkeletonDelayMs)
				{
					var skill = LokiPoe.InGameState.SkillBarHud.Slot(_summonSkeletonsSlot);
					var max = skill.GetStat(StatTypeGGG.NumberOfSkeletonsAllowed);
					if (skill.NumberDeployed < max && skill.CanUse())
					{
						var skillName = skill.Name;
						
						++_summonSkeletonCount;

						await DisableAlwaysHiglight();

						await Coroutines.FinishCurrentAction();

						var err1 = LokiPoe.InGameState.SkillBarHud.UseAt(_summonSkeletonsSlot, true,
							myPos.GetPointAtDistanceAfterThis(cachedPosition,
								cachedDistance / 2));

						if (_summonSkeletonCount >= OldRoutineSettings.Instance.SummonSkeletonCountPerDelay)
						{
							_summonSkeletonCount = 0;
							_summonSkeletonsStopwatch.Restart();
						}

						if (err1 == LokiPoe.InGameState.UseResult.None)
						{
							await Coroutines.FinishCurrentAction(false);

							return LogicResult.Provided;
						}

						Log.ErrorFormat("[Logic] UseAt returned {0} for {1}.", err1, skillName);
					}
				}

				if (_mineSlot != -1 && _mineStopwatch.ElapsedMilliseconds >
					OldRoutineSettings.Instance.MineDelayMs &&
					myPos.Distance(cachedPosition) < OldRoutineSettings.Instance.MaxMeleeRange)
				{
					var skill = LokiPoe.InGameState.SkillBarHud.Slot(_mineSlot);
					var max = skill.GetStat(StatTypeGGG.SkillDisplayNumberOfRemoteMinesAllowed);
					var insta = skill.GetStat(StatTypeGGG.MineDetonationIsInstant) == 1;
					if (skill.NumberDeployed < max && skill.CanUse())
					{
						var skillName = skill.Name;
						
						await DisableAlwaysHiglight();

						await Coroutines.FinishCurrentAction();

						var err1 = LokiPoe.InGameState.SkillBarHud.Use(_mineSlot, true);

						if (err1 == LokiPoe.InGameState.UseResult.None)
						{
							await Coroutines.LatencyWait();

							await Coroutines.FinishCurrentAction(false);

							if (!insta)
							{
								await Coroutines.LatencyWait();
								await Coroutine.Sleep(500);

								LokiPoe.Input.SimulateKeyEvent(LokiPoe.Input.Binding.detonate_mines, true, false, false);
							}

							_mineStopwatch.Restart();

							return LogicResult.Provided;
						}

						_mineStopwatch.Restart();

						Log.ErrorFormat("[Logic] UseAt returned {0} for {1}.", err1, skillName);
					}
				}

				// Handle Wither logic.
				/*if (_witherSlot != -1)
				{
					var skill = LokiPoe.InGameState.SkillBarHud.Slot(_witherSlot);
					if (skill.CanUse(false, false, false) && !cachedWither)
					{
						await DisableAlwaysHiglight();

						await Coroutines.FinishCurrentAction();

						var err1 = LokiPoe.InGameState.SkillBarHud.UseAt(_witherSlot, true, cachedPosition);

						if (err1 == LokiPoe.InGameState.UseResult.None)
						{
							await Coroutines.LatencyWait();

							await Coroutines.FinishCurrentAction(false);

							await Coroutines.LatencyWait();

							return true;
						}

						Log.ErrorFormat("[Logic] UseAt returned {0} for {1}.", err1, skill.Name);
					}
				}*/

				// Handle contagion logic.
				if (_contagionSlot != -1)
				{
					var skill = LokiPoe.InGameState.SkillBarHud.Slot(_contagionSlot);
					if (skill.CanUse(false, false, false) && !cachedContagion)
					{
						var skillName = skill.Name;
						
						await DisableAlwaysHiglight();

						await Coroutines.FinishCurrentAction();

						var err1 = LokiPoe.InGameState.SkillBarHud.UseAt(_contagionSlot, true, cachedPosition);

						if (err1 == LokiPoe.InGameState.UseResult.None)
						{
							await Coroutines.LatencyWait();

							await Coroutines.FinishCurrentAction(false);

							await Coroutines.LatencyWait();

							return LogicResult.Provided;
						}

						Log.ErrorFormat("[Logic] UseAt returned {0} for {1}.", err1, skillName);
					}
				}

				// Handle cold snap logic. Only use when power charges won't be consumed.
				if (_coldSnapSlot != -1)
				{
					var skill = LokiPoe.InGameState.SkillBarHud.Slot(_coldSnapSlot);
					if (skill.CanUse(false, false, false))
					{
						var skillName = skill.Name;
						
						await DisableAlwaysHiglight();

						await Coroutines.FinishCurrentAction();

						var err1 = LokiPoe.InGameState.SkillBarHud.UseAt(_coldSnapSlot, true, cachedPosition);

						if (err1 == LokiPoe.InGameState.UseResult.None)
						{
							await Coroutines.LatencyWait();

							await Coroutines.FinishCurrentAction(false);

							await Coroutines.LatencyWait();

							return LogicResult.Provided;
						}

						Log.ErrorFormat("[Logic] UseAt returned {0} for {1}.", err1, skillName);
					}
				}

				// Auto-cast any vaal skill at the best target as soon as it's usable.
				if (OldRoutineSettings.Instance.AutoCastVaalSkills && _vaalStopwatch.ElapsedMilliseconds > 1000)
				{
					// 3.3: Allow blacklisting of vaal skills
					foreach (var skill in NonBlacklistedSkills().ToList())
					{
						// 3.3: We can no longer use the Vaal tag since non-vaal skills inherit it.
						if (skill.IsVaalSkill)
						{
							var skillName = skill.Name;
							
							if (skill.CanUse())
							{
								await DisableAlwaysHiglight();

								await Coroutines.FinishCurrentAction();

								var err1 = LokiPoe.InGameState.SkillBarHud.UseAt(skill.Slot, false, cachedPosition);
								if (err1 == LokiPoe.InGameState.UseResult.None)
								{
									await Coroutines.LatencyWait();

									await Coroutines.FinishCurrentAction(false);

									await Coroutines.LatencyWait();

									return LogicResult.Provided;
								}

								Log.ErrorFormat("[Logic] Use returned {0} for {1}.", err1, skillName);
							}
						}
					}
					_vaalStopwatch.Restart();
				}

				var aip = false;

				var aoe = false;
				var melee = false;

				int slot = -1;

				// Logic for figuring out if we should use an AoE skill or single target.
				if (cachedNumberOfMobsNear > 2 && cachedRarity < Rarity.Rare)
				{
					aoe = true;
				}

				// Logic for figuring out if we should use an AoE skill instead.
				if (myPos.Distance(cachedPosition) < OldRoutineSettings.Instance.MaxMeleeRange)
				{
					melee = true;
					if (cachedMobsNearForAoe >= 3)
					{
						aoe = true;
					}
					else
					{
						aoe = false;
					}
				}

				// This sillyness is for making sure we always use a skill, and is why generic code is a PITA
				// when it can be configured like so.
				if (aoe)
				{
					if (melee)
					{
						slot = EnsurceCast(OldRoutineSettings.Instance.AoeMeleeSlot);
						if (slot == -1)
						{
							slot = EnsurceCast(OldRoutineSettings.Instance.SingleTargetMeleeSlot);
							if (slot == -1)
							{
								melee = false;
								slot = EnsurceCast(OldRoutineSettings.Instance.AoeRangedSlot);
								if (slot == -1)
								{
									slot = EnsurceCast(OldRoutineSettings.Instance.SingleTargetRangedSlot);
								}
							}
						}
					}
					else
					{
						slot = EnsurceCast(OldRoutineSettings.Instance.AoeRangedSlot);
						if (slot == -1)
						{
							slot = EnsurceCast(OldRoutineSettings.Instance.SingleTargetRangedSlot);
							if (slot == -1)
							{
								melee = true;
								slot = EnsurceCast(OldRoutineSettings.Instance.AoeMeleeSlot);
								if (slot == -1)
								{
									slot = EnsurceCast(OldRoutineSettings.Instance.SingleTargetMeleeSlot);
								}
							}
						}
					}
				}
				else
				{
					if (melee)
					{
						slot = EnsurceCast(OldRoutineSettings.Instance.SingleTargetMeleeSlot);
						if (slot == -1)
						{
							slot = EnsurceCast(OldRoutineSettings.Instance.AoeMeleeSlot);
							if (slot == -1)
							{
								melee = false;
								slot = EnsurceCast(OldRoutineSettings.Instance.SingleTargetRangedSlot);
								if (slot == -1)
								{
									slot = EnsurceCast(OldRoutineSettings.Instance.AoeRangedSlot);
								}
							}
						}
					}
					else
					{
						slot = EnsurceCast(OldRoutineSettings.Instance.SingleTargetRangedSlot);
						if (slot == -1)
						{
							slot = EnsurceCast(OldRoutineSettings.Instance.AoeRangedSlot);
							if (slot == -1)
							{
								melee = true;
								slot = EnsurceCast(OldRoutineSettings.Instance.SingleTargetMeleeSlot);
								if (slot == -1)
								{
									slot = EnsurceCast(OldRoutineSettings.Instance.AoeMeleeSlot);
								}
							}
						}
					}
				}

				if (OldRoutineSettings.Instance.AlwaysAttackInPlace)
					aip = true;

				if (slot == -1)
				{
					slot = OldRoutineSettings.Instance.FallbackSlot;
					melee = true;
				}

				if (slot == -1)
				{
					Log.ErrorFormat("[Logic] There is no slot configured to use.");
					return LogicResult.Provided;
				}

				if (melee || cachedProxShield)
				{
					var dist = LokiPoe.MyPosition.Distance(cachedPosition);
					if (dist > OldRoutineSettings.Instance.MaxMeleeRange)
					{
						if (skipPathing)
						{
							Log.Info($"[Logic] Cannot move towards {cachedName}. We will rely on QuestBot to bring us close to him.");
							return LogicResult.Unprovided;
						}

						if (!canMove)
						{
							Log.InfoFormat("[Logic] Not moving towards the target because we should not move currently.");

							return LogicResult.Unprovided;
						}

						Log.InfoFormat("[Logic] Now moving towards {0} because [dist ({1}) > MaxMeleeRange ({2})]",
							cachedPosition, dist, OldRoutineSettings.Instance.MaxMeleeRange);

						if (!PlayerMoverManager.MoveTowards(cachedPosition))
						{
							Log.ErrorFormat("[Logic] MoveTowards failed for {0}.", cachedPosition);
							await Coroutines.FinishCurrentAction();
						}
						return LogicResult.Provided;
					}
				}
				else
				{
					var dist = LokiPoe.MyPosition.Distance(cachedPosition);
					if (dist > OldRoutineSettings.Instance.MaxRangeRange)
					{
						if (skipPathing)
						{
							Log.Info($"[Logic] Cannot move towards {cachedName}. We will rely on QuestBot to bring us close to him.");
							return LogicResult.Unprovided;
						}

						if (!canMove)
						{
							Log.InfoFormat("[Logic] Not moving towards the target because we should not move currently.");

							return LogicResult.Unprovided;
						}

						Log.InfoFormat("[Logic] Now moving towards {0} because [dist ({1}) > MaxRangeRange ({2})]",
							cachedPosition, dist, OldRoutineSettings.Instance.MaxRangeRange);

						if (!PlayerMoverManager.MoveTowards(cachedPosition))
						{
							Log.ErrorFormat("[Logic] MoveTowards failed for {0}.", cachedPosition);
							await Coroutines.FinishCurrentAction();
						}
						return LogicResult.Provided;
					}
				}

				await DisableAlwaysHiglight();

                if (_flaskHook != null && _flaskHook(cachedRarity, cachedHpPercent))
                    return LogicResult.Provided;
				
				var slotSkill = LokiPoe.InGameState.SkillBarHud.Slot(slot);
				if (slotSkill == null)
				{
					Log.ErrorFormat("[Logic] There is no skill in the slot configured to use.");
					return LogicResult.Provided;
				}

				if (_isCasting && slot == _castingSlot && (LokiPoe.ProcessHookManager.GetKeyState(slotSkill.BoundKey) & 0x8000) !=
					0)
				{
					var ck = LokiPoe.Me.CurrentAction.Skill;
					if (LokiPoe.Me.HasCurrentAction && ck != null &&
						!ck.InternalId.Equals("Interaction") &&
						!ck.InternalId.Equals("Move"))
					{
						MouseManager.SetMousePos("OldRoutine.Logic", cachedPosition, false);

						return LogicResult.Provided;
					}
				}

				await Coroutines.FinishCurrentAction();

				var err = LokiPoe.InGameState.SkillBarHud.BeginUseAt(slot, aip, cachedPosition);
				if (err != LokiPoe.InGameState.UseResult.None)
				{
					Log.ErrorFormat("[Logic] BeginUseAt returned {0}.", err);
				}

				_isCasting = true;
				_castingSlot = slot;

				return LogicResult.Provided;
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
			if (message.Id == "SetCombatRange")
			{
				var range = message.GetInput<int>();
				OldRoutineSettings.Instance.CombatRange = range;
				Log.Info($"[OldRoutine] Combat range is set to {range}");
				return MessageResult.Processed;
			}

			Func<Tuple<object, string>[], object> f;
			if (_exposedSettings.TryGetValue(message.Id, out f))
			{
				message.AddOutput(this, f(message.Inputs.ToArray()));
				return MessageResult.Processed;
			}

			if (message.Id == "GetCombatTargeting")
			{
				message.AddOutput(this, CombatTargeting);
				return MessageResult.Processed;
			}

			if (message.Id == "ResetCombatTargeting")
			{
				_combatTargeting.ResetInclusionCalcuation();
				_combatTargeting.ResetWeightCalculation();
				_combatTargeting.InclusionCalcuation += CombatTargetingOnInclusionCalcuation;
				_combatTargeting.WeightCalculation += CombatTargetingOnWeightCalculation;
				return MessageResult.Processed;
			}

			if (message.Id == "player_leveled_event")
			{
				Log.InfoFormat("[Logic] We are now level {0}!", message.GetInput<int>());
				return MessageResult.Processed;
			}

			if (message.Id == "player_died_event")
			{
				var totalDeathsForInstance = message.GetInput<int>();

				return MessageResult.Processed;
			}

			if (message.Id == "area_changed_event")
			{
				_ignoreAnimatedItems.Clear();
				_shrineTries.Clear();
				return MessageResult.Processed;
			}

			if (message.Id == "SetFlaskHook")
            {
                var func = message.GetInput<Func<Rarity, int, bool>>();
                _flaskHook = func;
                Log.Info("[OldRoutine] Flask hook has been set.");
                return MessageResult.Processed;
            }
			
			return MessageResult.Unprocessed;
		}

		#endregion

		private WorldItem BestAnimateGuardianTarget(Monster monster, int maxLevel)
		{
			var worldItems = LokiPoe.ObjectManager.GetObjectsByType<WorldItem>()
				.Where(wi => !_ignoreAnimatedItems.Contains(wi.Id) && wi.Distance < 30)
				.OrderBy(wi => wi.Distance);
			foreach (var wi in worldItems)
			{
				var item = wi.Item;
				if (item.RequiredLevel <= maxLevel &&
					item.IsIdentified &&
					!item.IsChromatic &&
					item.SocketCount < 5 &&
					item.MaxLinkCount < 5 &&
					item.Rarity <= Rarity.Magic
				)
				{
					if (monster == null || monster.LeftHandWeaponVisual == "")
					{
						if (item.AnyMetadataFlags(MetadataFlags.Claws, MetadataFlags.OneHandAxes, MetadataFlags.OneHandMaces,
							MetadataFlags.OneHandSwords, MetadataFlags.OneHandThrustingSwords, MetadataFlags.TwoHandAxes,
							MetadataFlags.TwoHandMaces, MetadataFlags.TwoHandSwords))
						{
							_ignoreAnimatedItems.Add(wi.Id);
							return wi;
						}
					}

					if (monster == null || monster.ChestVisual == "")
					{
						if (item.HasMetadataFlags(MetadataFlags.BodyArmours))
						{
							_ignoreAnimatedItems.Add(wi.Id);
							return wi;
						}
					}

					if (monster == null || monster.HelmVisual == "")
					{
						if (item.HasMetadataFlags(MetadataFlags.Helmets))
						{
							_ignoreAnimatedItems.Add(wi.Id);
							return wi;
						}
					}

					if (monster == null || monster.BootsVisual == "")
					{
						if (item.HasMetadataFlags(MetadataFlags.Boots))
						{
							_ignoreAnimatedItems.Add(wi.Id);
							return wi;
						}
					}

					if (monster == null || monster.GlovesVisual == "")
					{
						if (item.HasMetadataFlags(MetadataFlags.Gloves))
						{
							_ignoreAnimatedItems.Add(wi.Id);
							return wi;
						}
					}
				}
			}

			return null;
		}

		private WorldItem BestAnimateWeaponTarget(int maxLevel)
		{
			var worldItems = LokiPoe.ObjectManager.GetObjectsByType<WorldItem>()
				.Where(wi => !_ignoreAnimatedItems.Contains(wi.Id) && wi.Distance < 30)
				.OrderBy(wi => wi.Distance);
			foreach (var wi in worldItems)
			{
				var item = wi.Item;
				if (item.IsIdentified &&
					item.RequiredLevel <= maxLevel &&
					!item.IsChromatic &&
					item.SocketCount < 5 &&
					item.MaxLinkCount < 5 &&
					item.Rarity <= Rarity.Magic &&
					item.AnyMetadataFlags(MetadataFlags.Claws, MetadataFlags.OneHandAxes, MetadataFlags.OneHandMaces,
						MetadataFlags.OneHandSwords, MetadataFlags.OneHandThrustingSwords, MetadataFlags.TwoHandAxes,
						MetadataFlags.TwoHandMaces, MetadataFlags.TwoHandSwords, MetadataFlags.Daggers, MetadataFlags.Staves))
				{
					_ignoreAnimatedItems.Add(wi.Id);
					return wi;
				}
			}
			return null;
		}

		private Monster BestDeadTarget
		{
			get
			{
				var myPos = LokiPoe.MyPosition;
				return LokiPoe.ObjectManager.GetObjectsByType<Monster>()
					.Where(
						m =>
							m.Distance < 30 && m.IsActiveDead && m.Rarity != Rarity.Unique && m.CorpseUsable &&
							ExilePather.PathDistance(myPos, m.Position, false, !OldRoutineSettings.Instance.LeaveFrame) < 30)
					.OrderBy(m => m.Distance)
					.FirstOrDefault();
			}
		}

		private async Task<LogicResult> CombatLogicEnd()
		{
			var res = LogicResult.Unprovided;

			if (await EnableAlwaysHiglight())
			{
				res = LogicResult.Provided;
			}

			if (_isCasting)
			{
				LokiPoe.ProcessHookManager.ClearAllKeyStates();
				_isCasting = false;
				_castingSlot = -1;

				res = LogicResult.Provided;
			}

			return res;
		}

		private readonly Dictionary<int, int> _shrineTries = new Dictionary<int, int>();

		private async Task<bool> HandleShrines()
		{
			// If the user wants to avoid shrine logic due to stuck issues, simply return without doing anything.
			if (OldRoutineSettings.Instance.SkipShrines)
				return false;

			// TODO: Shrines need speical CR logic, because it's now the CRs responsibility for handling all combaat situations,
			// and shrines are now considered a combat situation due their nature.

			// Check for any active shrines.
			var shrines =
				LokiPoe.ObjectManager.Objects.OfType<Shrine>()
					.Where(s => !Blacklist.Contains(s.Id) && !s.IsDeactivated && s.Distance < 50)
					.OrderBy(s => s.Distance)
					.ToList();

			if (!shrines.Any())
				return false;

			Log.InfoFormat("[HandleShrines]");

			// For now, just take the first shrine found.

			var shrine = shrines[0];
			int tries;

			if (!_shrineTries.TryGetValue(shrine.Id, out tries))
			{
				tries = 0;
				_shrineTries.Add(shrine.Id, tries);
			}

			if (tries > 10)
			{
				Blacklist.Add(shrine.Id, TimeSpan.FromHours(1), "Could not interact with the shrine.");

				return true;
			}

			// Handle Skeletal Shrine in a special way, or handle priority between multiple shrines at the same time.
			var skellyOverride = shrine.ShrineId == "Skeletons";

			// Try and only move to touch it when we have a somewhat navigable path.
			if ((NumberOfMobsBetween(LokiPoe.Me, shrine, 5, true) < 5 &&
				NumberOfMobsNear(LokiPoe.Me, 20) < 3) || skellyOverride)
			{
				var myPos = LokiPoe.MyPosition;

				var pos = ExilePather.FastWalkablePositionFor(shrine);

				// We need to filter out based on pathfinding, since otherwise, a large gap will lockup the bot.
				var pathDistance = ExilePather.PathDistance(myPos, pos, false, !OldRoutineSettings.Instance.LeaveFrame);

				Log.DebugFormat("[HandleShrines] Now moving towards the Shrine {0} [pathPos: {1} pathDis: {2}].", shrine.Id, pos,
					pathDistance);

				if (pathDistance > 50)
				{
					Log.DebugFormat("[HandleShrines] Not attempting to move towards Shrine [{0}] because the path distance is: {1}.",
						shrine.Id, pathDistance);
					return false;
				}

				//var canSee = ExilePather.CanObjectSee(LokiPoe.Me, pos, !OldRoutineSettings.Instance.LeaveFrame);

				// We're in distance when we're sure we're close to the position, but also that the path we need to take to the position
				// isn't too much further. This prevents problems with things on higher ground when we are on lower, and vice-versa.
				var inDistance = myPos.Distance(pos) < 20 && pathDistance < 25;
				if (inDistance)
				{
					Log.DebugFormat("[HandleShrines] Now attempting to interact with the Shrine {0}.", shrine.Id);

					await Coroutines.FinishCurrentAction();

					await Coroutines.InteractWith(shrine);

					_shrineTries[shrine.Id]++;
				}
				else
				{
					if (!PlayerMoverManager.MoveTowards(pos))
					{
						Log.ErrorFormat("[HandleShrines] MoveTowards failed for {0}.", pos);

						Blacklist.Add(shrine.Id, TimeSpan.FromHours(1), "Could not move towards the shrine.");

						await Coroutines.FinishCurrentAction();
					}
				}

				return true;
			}

			return false;
		}

		private bool _needsToDisableAlwaysHighlight;

		// This logic is now CR specific, because Strongbox gui labels interfere with targeting,
		// but not general movement using Move only.
		private async Task DisableAlwaysHiglight()
		{
			if (_needsToDisableAlwaysHighlight && LokiPoe.ConfigManager.IsAlwaysHighlightEnabled)
			{
				Log.InfoFormat("[DisableAlwaysHiglight] Now disabling Always Highlight to avoid skill use issues.");
				LokiPoe.Input.SimulateKeyEvent(LokiPoe.Input.Binding.highlight_toggle, true, false, false);
				await Coroutine.Sleep(16);
			}
		}

		// This logic is now CR specific, because Strongbox gui labels interfere with targeting,
		// but not general movement using Move only.
		private async Task<bool> EnableAlwaysHiglight()
		{
			if (!LokiPoe.ConfigManager.IsAlwaysHighlightEnabled)
			{
				Log.InfoFormat("[EnableAlwaysHiglight] Now enabling Always Highlight.");
				LokiPoe.Input.SimulateKeyEvent(LokiPoe.Input.Binding.highlight_toggle, true, false, false);
				await Coroutine.Sleep(16);
				return true;
			}
			return false;
		}

		private int EnsurceCast(int slot)
		{
			if (slot == -1)
				return slot;

			var slotSkill = LokiPoe.InGameState.SkillBarHud.Slot(slot);
			if (slotSkill == null || !slotSkill.CanUse())
			{
				return -1;
			}

			return slot;
		}

		#region Override of Object

		/// <summary>Returns a string that represents the current object.</summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString()
		{
			return Name + ": " + Description;
		}

		#endregion
	}
}