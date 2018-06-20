using System.Collections.ObjectModel;
using System.ComponentModel;
using log4net;
using Loki;
using Loki.Common;

namespace Legacy.AutoPassives
{
	public class Passive
	{
		public ushort Id { get; set; }
	}

	/// <summary>Settings for the Dev tab. </summary>
	public class AutoPassivesSettings : JsonSettings
	{
		private static readonly ILog Log = Logger.GetLoggerInstanceForType();
		private static AutoPassivesSettings _instance;

		/// <summary>The current instance for this class. </summary>
		public static AutoPassivesSettings Instance => _instance ?? (_instance = new AutoPassivesSettings());

		/// <summary>The default ctor. Will use the settings path "AutoPassives".</summary>
		public AutoPassivesSettings()
			: base(GetSettingsFilePath(Configuration.Instance.Name, string.Format("{0}.json", "AutoPassives")))
		{
			if (_passives == null)
			{
				_passives = new ObservableCollection<Passive>();
			}
		}

		private ObservableCollection<Passive> _passives;
		private bool _onlyAllocateInTown;

		/// <summary>
		/// Should passives only be allocated in town?
		/// </summary>
		[DefaultValue(true)]
		public bool OnlyAllocateInTown
		{
			get
			{
				return _onlyAllocateInTown;
			}
			set
			{
				if (value.Equals(_onlyAllocateInTown))
				{
					return;
				}
				_onlyAllocateInTown = value;
				NotifyPropertyChanged(() => OnlyAllocateInTown);
				Save();
				Log.InfoFormat("[AutoPassivesSettings] OnlyAllocateInTown: {0}", _onlyAllocateInTown);
			}
		}

		/// <summary>
		/// A list of skill ids to level.
		/// </summary>
		public ObservableCollection<Passive> Passives
		{
			get
			{
				return _passives;
			}
			set
			{
				if (value.Equals(_passives))
				{
					return;
				}
				_passives = value;
				NotifyPropertyChanged(() => Passives);
				Save();
			}
		}
	}
}