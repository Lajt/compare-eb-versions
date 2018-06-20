using System.ComponentModel;
using Loki;
using Loki.Common;

namespace Legacy.OldCoroutinePlayerMover
{
	/// <summary>Settings for the Dev tab. </summary>
	public class OldCoroutinePlayerMoverSettings : JsonSettings
	{
		private static OldCoroutinePlayerMoverSettings _instance;

		/// <summary>The current instance for this class. </summary>
		public static OldCoroutinePlayerMoverSettings Instance => _instance ?? (_instance = new OldCoroutinePlayerMoverSettings());

		/// <summary>The default ctor. Will use the settings path "OldCoroutinePlayerMover".</summary>
		public OldCoroutinePlayerMoverSettings()
			: base(GetSettingsFilePath(Configuration.Instance.Name, string.Format("{0}.json", "OldCoroutinePlayerMover")))
		{
		}

		private bool _useMouseSmoothing;
		private bool _debugInputApi;
		private int _mouseSmoothDistance;
		private bool _avoidWallHugging;
		private int _moveRange;
		private int _singleUseDistance;

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
