using Loki;
using Loki.Common;

namespace Legacy.ExternalCommunication
{
	/// <summary>Settings for the Dev tab. </summary>
	public class ExternalCommunicationSettings : JsonSettings
	{
		private static ExternalCommunicationSettings _instance;

		/// <summary>The current instance for this class. </summary>
		public static ExternalCommunicationSettings Instance => _instance ?? (_instance = new ExternalCommunicationSettings());

		/// <summary>The default ctor. Will use the settings path "ExternalCommunication".</summary>
		public ExternalCommunicationSettings()
			: base(GetSettingsFilePath(Configuration.Instance.Name, string.Format("{0}.json", "ExternalCommunication")))
		{
		}
	}
}
