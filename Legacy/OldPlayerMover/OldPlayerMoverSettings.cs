using System.ComponentModel;
using Loki;
using Loki.Common;
using System.Collections.ObjectModel;

namespace Legacy.OldPlayerMover
{
	public class StringWrapper
	{
		public string Value { get; set; }
	}

	/// <summary>Settings for the Dev tab. </summary>
	public class OldPlayerMoverSettings : JsonSettings
	{
		private static OldPlayerMoverSettings _instance;

		/// <summary>The current instance for this class. </summary>
		public static OldPlayerMoverSettings Instance => _instance ?? (_instance = new OldPlayerMoverSettings());

		/// <summary>The default ctor. Will use the settings path "OldPlayerMover".</summary>
		public OldPlayerMoverSettings()
			: base(GetSettingsFilePath(Configuration.Instance.Name, string.Format("{0}.json", "OldPlayerMover")))
		{
			if (_forcedAdjustmentAreas == null)
			{
				_forcedAdjustmentAreas = new ObservableCollection<StringWrapper> {
					new StringWrapper { Value = "The City of Sarn" },
					new StringWrapper { Value = "The Slums" },
					new StringWrapper { Value = "The Quay" },
					new StringWrapper { Value = "The Toxic Conduits" } };
			}
		}

		private bool _useMouseSmoothing;
		private bool _debugInputApi;
		private int _mouseSmoothDistance;
		private bool _avoidWallHugging;
		private int _moveRange;
		private int _singleUseDistance;
		private int _pathRefreshRateMs;
		private ObservableCollection<StringWrapper> _forcedAdjustmentAreas;
		private bool _forceAdjustCombatAreas;
		private bool _debugAdjustments;

		/// <summary>
		/// Should the area adjustments be used for all combat areas and not just ForcedAdjustmentAreas?
		/// </summary>
		[DefaultValue(false)]
		public bool DebugAdjustments
		{
			get { return _debugAdjustments; }
			set
			{
				if (value.Equals(_debugAdjustments))
				{
					return;
				}
				_debugAdjustments = value;
				NotifyPropertyChanged(() => ForceAdjustCombatAreas);
			}
		}

		/// <summary>
		/// Should the area adjustments be used for all combat areas and not just ForcedAdjustmentAreas?
		/// </summary>
		[DefaultValue(false)]
		public bool ForceAdjustCombatAreas
		{
			get { return _forceAdjustCombatAreas; }
			set
			{
				if (value.Equals(_forceAdjustCombatAreas))
				{
					return;
				}
				_forceAdjustCombatAreas = value;
				NotifyPropertyChanged(() => ForceAdjustCombatAreas);
			}
		}

		/// <summary>
		/// The time in ms to refresh a path that was generated.
		/// </summary>
		[DefaultValue(32)]
		public int PathRefreshRateMs
		{
			get { return _pathRefreshRateMs; }
			set
			{
				if (value.Equals(_pathRefreshRateMs))
				{
					return;
				}
				_pathRefreshRateMs = value;
				NotifyPropertyChanged(() => PathRefreshRateMs);
			}
		}

		/// <summary>
		/// A list of areas to force movement adjustments on.
		/// </summary>
		public ObservableCollection<StringWrapper> ForcedAdjustmentAreas
		{
			get
			{
				return _forcedAdjustmentAreas;
			}
			set
			{
				if (value.Equals(_forcedAdjustmentAreas))
				{
					return;
				}
				_forcedAdjustmentAreas = value;
				NotifyPropertyChanged(() => ForcedAdjustmentAreas);
			}
		}

		[DefaultValue(true)]
		public bool UseMouseSmoothing
		{
			get { return _useMouseSmoothing; }
			set
			{
				if (value.Equals(_useMouseSmoothing))
				{
					return;
				}
				_useMouseSmoothing = value;
				NotifyPropertyChanged(() => UseMouseSmoothing);
			}
		}

		[DefaultValue(false)]
		public bool DebugInputApi
		{
			get { return _debugInputApi; }
			set
			{
				if (value.Equals(_debugInputApi))
				{
					return;
				}
				_debugInputApi = value;
				NotifyPropertyChanged(() => DebugInputApi);
			}
		}

		[DefaultValue(10)]
		public int MouseSmoothDistance
		{
			get { return _mouseSmoothDistance; }
			set
			{
				if (value.Equals(_mouseSmoothDistance))
				{
					return;
				}
				_mouseSmoothDistance = value;
				NotifyPropertyChanged(() => MouseSmoothDistance);
			}
		}

		[DefaultValue(true)]
		public bool AvoidWallHugging
		{
			get { return _avoidWallHugging; }
			set
			{
				if (value.Equals(_avoidWallHugging))
				{
					return;
				}
				_avoidWallHugging = value;
				NotifyPropertyChanged(() => AvoidWallHugging);
			}
		}

		[DefaultValue(15)]
		public int MoveRange
		{
			get { return _moveRange; }
			set
			{
				if (value.Equals(_moveRange))
				{
					return;
				}
				_moveRange = value;
				NotifyPropertyChanged(() => MoveRange);
			}
		}

		[DefaultValue(25)]
		public int SingleUseDistance
		{
			get { return _singleUseDistance; }
			set
			{
				if (value.Equals(_singleUseDistance))
				{
					return;
				}
				_singleUseDistance = value;
				NotifyPropertyChanged(() => SingleUseDistance);
			}
		}
	}
}
