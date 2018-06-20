using System.ComponentModel;
using Loki;
using Loki.Common;

namespace Legacy.Breaches
{
	/// <summary>Settings for the Dev tab. </summary>
	public class BreachesSettings : JsonSettings
	{
		private static BreachesSettings _instance;

		/// <summary>The current instance for this class. </summary>
		public static BreachesSettings Instance => _instance ?? (_instance = new BreachesSettings());

		/// <summary>The default ctor. Will use the settings path "Breaches".</summary>
		public BreachesSettings()
			: base(GetSettingsFilePath(Configuration.Instance.Name, string.Format("{0}.json", "Breaches")))
		{
		}

		private bool _enabled;
		private bool _open;

		/// <summary>Should Breaches logic run? If false, Breaches will be skipped when possible.</summary>
		[DefaultValue(true)]
		public bool Enabled
		{
			get { return _enabled; }
			set
			{
				if (value.Equals(_enabled))
				{
					return;
				}
				_enabled = value;
				NotifyPropertyChanged(() => Enabled);
				Save();
			}
		}

		/// <summary>Should Breaches be interacted with? If false, the bot will stand near them instead.</summary>
		[DefaultValue(true)]
		public bool Open
		{
			get { return _open; }
			set
			{
				if (value.Equals(_open))
				{
					return;
				}
				_open = value;
				NotifyPropertyChanged(() => Open);
				Save();
			}
		}
	}
}
