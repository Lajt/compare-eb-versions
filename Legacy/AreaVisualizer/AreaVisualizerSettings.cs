using Loki;
using Loki.Common;

namespace Legacy.AreaVisualizer
{
	/// <summary>Settings for the Dev tab. </summary>
	public class AreaVisualizerSettings : JsonSettings
	{
		private static AreaVisualizerSettings _instance;

		/// <summary>The current instance for this class. </summary>
		public static AreaVisualizerSettings Instance => _instance ?? (_instance = new AreaVisualizerSettings());

		/// <summary>The default ctor. Will use the settings path "AreaVisualizer".</summary>
		public AreaVisualizerSettings()
			: base(GetSettingsFilePath(Configuration.Instance.Name, string.Format("{0}.json", "AreaVisualizer")))
		{
		}
	}
}
