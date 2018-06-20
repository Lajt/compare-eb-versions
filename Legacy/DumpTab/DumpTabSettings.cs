using System.ComponentModel;
using Loki;
using Loki.Common;

namespace Legacy.DumpTab
{
	/// <summary>Settings for the Dev tab. </summary>
	public class DumpTabSettings : JsonSettings
	{
		private static DumpTabSettings _instance;

		/// <summary>The current instance for this class. </summary>
		public static DumpTabSettings Instance => _instance ?? (_instance = new DumpTabSettings());

		/// <summary>The default ctor. Will use the settings path "DumpTab".</summary>
		public DumpTabSettings()
			: base(GetSettingsFilePath(Configuration.Instance.Name, string.Format("{0}.json", "DumpTab")))
		{
		}
	}
}
