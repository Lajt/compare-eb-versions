using System.ComponentModel;
using Loki;
using Loki.Common;

namespace Community.Follower
{
	/// <summary>Settings for the Dev tab. </summary>
	public class FollowerSettings : JsonSettings
	{
		private static FollowerSettings _instance;

		/// <summary>The current instance for this class. </summary>
		public static FollowerSettings Instance => _instance ?? (_instance = new FollowerSettings());

		/// <summary>The default ctor. Will use the settings path "Follower".</summary>
		public FollowerSettings()
			: base(GetSettingsFilePath(Configuration.Instance.Name, string.Format("{0}.json", "Follower")))
		{
		}

		private string _leader;

		/// <summary>The leader to follow.</summary>
		[DefaultValue("")]
		public string Leader
		{
			get
			{
				return _leader;
			}
			set
			{
				if (value.Equals(_leader))
				{
					return;
				}
				_leader = value;
				NotifyPropertyChanged(() => Leader);
				Save();
			}
		}

		private int _followDistance;

		/// <summary>The furthers to follow the leader at .</summary>
		[DefaultValue(20)]
		public int FollowDistance
		{
			get
			{
				return _followDistance;
			}
			set
			{
				if (value.Equals(_followDistance))
				{
					return;
				}
				_followDistance = value;
				NotifyPropertyChanged(() => FollowDistance);
				Save();
			}
		}

		private bool _stopOutsideBossDoor;

		/// <summary>Should the bot stop outside the boss door.</summary>
		[DefaultValue(true)]
		public bool StopOutsideBossDoor
		{
			get
			{
				return _stopOutsideBossDoor;
			}
			set
			{
				if (value.Equals(_followDistance))
				{
					return;
				}
				_stopOutsideBossDoor = value;
				NotifyPropertyChanged(() => StopOutsideBossDoor);
				Save();
			}
		}
	}
}