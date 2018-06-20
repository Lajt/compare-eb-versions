using System.ComponentModel;
using Loki;
using Loki.Common;

namespace Legacy.PythonExample
{
	/// <summary>Settings for the Dev tab. </summary>
	public class PythonExampleSettings : JsonSettings
	{
		private static PythonExampleSettings _instance;

		/// <summary>The current instance for this class. </summary>
		public static PythonExampleSettings Instance => _instance ?? (_instance = new PythonExampleSettings());

		private string _code;

		/// <summary>The default ctor. Will use the settings path "PythonExample".</summary>
		public PythonExampleSettings()
			: base(GetSettingsFilePath(Configuration.Instance.Name, string.Format("{0}.json", "PythonExample")))
		{
		}

		/// <summary>The data in the Text control.</summary>
		[DefaultValue(
			"import sys\r\nsys.stdout=ioproxy\r\n\r\ndef Execute():\r\n\tprint LokiPoe.Me.Name"
			)]
		public string Code
		{
			get
			{
				return _code;
			}
			set
			{
				if (value.Equals(_code))
				{
					return;
				}
				_code = value;
				NotifyPropertyChanged(() => Code);
			}
		}
	}
}
