using System.Collections.ObjectModel;
using System.ComponentModel;
using Loki;
using Loki.Common;

namespace Legacy.Monoliths
{
	public class StringWrapper
	{
		public string Value { get; set; }
	}

	/// <summary>Settings for the Dev tab. </summary>
	public class MonolithsSettings : JsonSettings
	{
		private static MonolithsSettings _instance;

		/// <summary>The current instance for this class. </summary>
		public static MonolithsSettings Instance => _instance ?? (_instance = new MonolithsSettings());

		/// <summary>The default ctor. Will use the settings path "Monoliths".</summary>
		public MonolithsSettings()
			: base(GetSettingsFilePath(Configuration.Instance.Name, string.Format("{0}.json", "Monoliths")))
		{
			// Whitelist all essence by default.
			if (_whitelistEssenceMetadata == null)
			{
				_whitelistEssenceMetadata = new ObservableCollection<StringWrapper>
				{
					new StringWrapper {Value = "Metadata/Items/Currency/"}
				};
			}
			else
			{
				foreach (var entry in _whitelistEssenceMetadata)
				{
					if (entry != null && entry.Value == null)
					{
						entry.Value = "";
					}
				}
			}

			// Whitelist all monsters by default.
			if (_whitelistMonsterMetadata == null)
			{
				_whitelistMonsterMetadata = new ObservableCollection<StringWrapper>
				{
					new StringWrapper {Value = "Metadata/Monsters/"}
				};
			}
			else
			{
				foreach (var entry in _whitelistMonsterMetadata)
				{
					if (entry != null && entry.Value == null)
					{
						entry.Value = "";
					}
				}
			}

			// Blacklist nothing by default.
			if (_blacklistEssenceMetadata == null)
			{
				_blacklistEssenceMetadata = new ObservableCollection<StringWrapper>();
			}
			else
			{
				foreach (var entry in _blacklistEssenceMetadata)
				{
					if (entry != null && entry.Value == null)
					{
						entry.Value = "";
					}
				}
			}

			// Blacklist nothing by default.
			if (_blacklistMonsterMetadata == null)
			{
				_blacklistMonsterMetadata = new ObservableCollection<StringWrapper>();
			}
			else
			{
				foreach (var entry in _blacklistMonsterMetadata)
				{
					if (entry != null && entry.Value == null)
					{
						entry.Value = "";
					}
				}
			}
		}

		private bool _enabled;
		private bool _open;
		private int _minEssences;
		private int _maxEssences;
		private ObservableCollection<StringWrapper> _whitelistEssenceMetadata;
		private ObservableCollection<StringWrapper> _whitelistMonsterMetadata;
		private ObservableCollection<StringWrapper> _blacklistEssenceMetadata;
		private ObservableCollection<StringWrapper> _blacklistMonsterMetadata;

		/// <summary>Should Monolith logic run? If false, Monoliths will be skipped over.</summary>
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

		/// <summary>Should Monoliths be interacted with? If false, the bot will stand near them instead.</summary>
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

		/// <summary>How many essences the monolith should have at minimal (or -1 to ignore).</summary>
		[DefaultValue(-1)]
		public int MinEssences
		{
			get { return _minEssences; }
			set
			{
				if (value.Equals(_minEssences))
				{
					return;
				}
				_minEssences = value;
				NotifyPropertyChanged(() => MinEssences);
				Save();
			}
		}

		/// <summary>How many essences the monolith should have at maximum (or -1 to ignore).</summary>
		[DefaultValue(-1)]
		public int MaxEssences
		{
			get { return _maxEssences; }
			set
			{
				if (value.Equals(_maxEssences))
				{
					return;
				}
				_maxEssences = value;
				NotifyPropertyChanged(() => MaxEssences);
				Save();
			}
		}

		/// <summary>
		/// The essence metadata whitelist.
		/// </summary>
		public ObservableCollection<StringWrapper> WhitelistEssenceMetadata
		{
			get { return _whitelistEssenceMetadata; }
			set
			{
				if (value.Equals(_whitelistEssenceMetadata))
				{
					return;
				}
				_whitelistEssenceMetadata = value;
				NotifyPropertyChanged(() => WhitelistEssenceMetadata);
				Save();
			}
		}

		/// <summary>
		/// The monster metadata whitelist.
		/// </summary>
		public ObservableCollection<StringWrapper> WhitelistMonsterMetadata
		{
			get { return _whitelistMonsterMetadata; }
			set
			{
				if (value.Equals(_whitelistMonsterMetadata))
				{
					return;
				}
				_whitelistMonsterMetadata = value;
				NotifyPropertyChanged(() => WhitelistMonsterMetadata);
				Save();
			}
		}

		/// <summary>
		/// The essence metadata blacklist.
		/// </summary>
		public ObservableCollection<StringWrapper> BlacklistEssenceMetadata
		{
			get { return _blacklistEssenceMetadata; }
			set
			{
				if (value.Equals(_blacklistEssenceMetadata))
				{
					return;
				}
				_blacklistEssenceMetadata = value;
				NotifyPropertyChanged(() => BlacklistEssenceMetadata);
				Save();
			}
		}

		/// <summary>
		/// The monster metadata blacklist.
		/// </summary>
		public ObservableCollection<StringWrapper> BlacklistMonsterMetadata
		{
			get { return _blacklistMonsterMetadata; }
			set
			{
				if (value.Equals(_blacklistMonsterMetadata))
				{
					return;
				}
				_blacklistMonsterMetadata = value;
				NotifyPropertyChanged(() => BlacklistMonsterMetadata);
				Save();
			}
		}
	}
}