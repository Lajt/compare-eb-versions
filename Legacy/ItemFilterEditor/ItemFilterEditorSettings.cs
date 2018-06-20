using Loki;
using Loki.Bot;
using Loki.Common;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Legacy.ItemFilterEditor
{
	/// <summary>Settings for the ItemFilterEditor plugin. </summary>
	public class ItemFilterEditorSettings : JsonSettings
	{
		private static ItemFilterEditorSettings _instance;

		/// <summary>The current instance for this class. </summary>
		public static ItemFilterEditorSettings Instance => _instance ?? (_instance = new ItemFilterEditorSettings());

		/// <summary>The default ctor. Will use the settings path "ItemFilterEditor".</summary>
		public ItemFilterEditorSettings()
			: base(GetSettingsFilePath(Configuration.Instance.Name, string.Format("{0}.json", "ItemFilterEditor")))
		{
		}

		/// <summary>
		/// Flag to refresh the item eval after switching looting of a currency on or off.
		/// </summary>
		public bool RefreshItemEvaluator { get; set; }

		private bool _debugPickupLimit;

		[DefaultValue(true)]
		public bool DebugPickupLimit
		{
			get { return _debugPickupLimit; }
			set
			{
				if (value.Equals(_debugPickupLimit))
				{
					return;
				}
				_debugPickupLimit = value;
				NotifyPropertyChanged(() => DebugPickupLimit);
				Save();
			}
		}

		private bool _limitWisdomPickup;

		[DefaultValue(false)]
		public bool LimitWisdomPickup
		{
			get { return _limitWisdomPickup; }
			set
			{
				if (value.Equals(_limitWisdomPickup))
				{
					return;
				}
				_limitWisdomPickup = value;
				NotifyPropertyChanged(() => LimitWisdomPickup);
				Save();
				RefreshItemEvaluator = true;
			}
		}

		private bool _limitPortalPickup;

		[DefaultValue(false)]
		public bool LimitPortalPickup
		{
			get { return _limitPortalPickup; }
			set
			{
				if (value.Equals(_limitPortalPickup))
				{
					return;
				}
				_limitPortalPickup = value;
				NotifyPropertyChanged(() => LimitPortalPickup);
				Save();
				RefreshItemEvaluator = true;
			}
		}

		private int _wisdomPickupLimit;

		[DefaultValue(40)]
		public int WisdomPickupLimit
		{
			get { return _wisdomPickupLimit; }
			set
			{
				if (value.Equals(_wisdomPickupLimit))
				{
					return;
				}
				_wisdomPickupLimit = value;
				NotifyPropertyChanged(() => WisdomPickupLimit);
				Save();
			}
		}

		private int _portalPickupLimit;

		[DefaultValue(40)]
		public int PortalPickupLimit
		{
			get { return _portalPickupLimit; }
			set
			{
				if (value.Equals(_portalPickupLimit))
				{
					return;
				}
				_portalPickupLimit = value;
				NotifyPropertyChanged(() => PortalPickupLimit);
				Save();
			}
		}
	}
}