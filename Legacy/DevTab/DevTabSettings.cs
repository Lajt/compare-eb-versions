using System.ComponentModel;
using Loki;
using Loki.Common;

namespace Legacy.DevTab
{
	/// <summary>Settings for the Dev tab. </summary>
	public class DevTabSettings : JsonSettings
	{
		private static DevTabSettings _instance;

		/// <summary>The current instance for this class. </summary>
		public static DevTabSettings Instance => _instance ?? (_instance = new DevTabSettings());

		/// <summary>The default ctor. Will use the settings path "DevTab".</summary>
		public DevTabSettings()
			: base(GetSettingsFilePath(Configuration.Instance.Name, string.Format("{0}.json", "DevTab")))
		{
			// Setup defaults here if needed for properties that don't support DefaultValue.
		}

		private string _fileName;
		private string _assemblies;
		private string _className;
		private string _code;

		/// <summary>The data in the File control.</summary>
		[DefaultValue("")]
		public string FileName
		{
			get
			{
				return _fileName;
			}
			set
			{
				if (value.Equals(_fileName))
				{
					return;
				}
				_fileName = value;
				NotifyPropertyChanged(() => FileName);
				Save();
			}
		}

		/// <summary>The data in the Assemblies control.</summary>
		[DefaultValue("")]
		public string Assemblies
		{
			get
			{
				return _assemblies;
			}
			set
			{
				if (value.Equals(_assemblies))
				{
					return;
				}
				_assemblies = value;
				NotifyPropertyChanged(() => Assemblies);
				Save();
			}
		}

		/// <summary>The data in the Class control.</summary>
		[DefaultValue("RuntimeCode")]
		public string ClassName
		{
			get
			{
				return _className;
			}
			set
			{
				if (value.Equals(_className))
				{
					return;
				}
				_className = value;
				NotifyPropertyChanged(() => ClassName);
				Save();
			}
		}

		/// <summary>The data in the Text control.</summary>
		[DefaultValue(
			"using System;\r\nusing System.Linq;\r\nusing System.Text;\r\nusing System.Reflection;\r\nusing System.Collections.Generic;\r\nusing System.Windows.Forms;\r\nusing System.Threading.Tasks;\r\nusing System.IO;\r\nusing Buddy.Coroutines;\r\nusing Loki;\r\nusing Loki.Common;\r\nusing Loki.Game;\r\nusing Loki.Game.Objects;\r\nusing Loki.Game.GameData;\r\nusing Loki.Bot;\r\nusing Loki.Bot.Pathfinding;\r\nusing log4net;\r\n\r\npublic class RuntimeCode\r\n{\r\n\tprivate static readonly ILog Log = Logger.GetLoggerInstanceForType();\r\n\r\n\tprivate Coroutine _coroutine;\r\n\r\n\tpublic void Execute()\r\n\t{\r\n\t\tusing(LokiPoe.AcquireFrame())\r\n\t\t{\r\n\t\t\tExilePather.Reload();\r\n\t\t}\r\n\r\n\t\t// Create the coroutine\r\n\t\t_coroutine = new Coroutine(() => MainCoroutine());\r\n\r\n\t\t// Run the coroutine while it's not finished.\r\n\t\twhile(!_coroutine.IsFinished)\r\n\t\t{\r\n\t\t\ttry\r\n\t\t\t{\r\n\t\t\t\tusing(LokiPoe.AcquireFrame())\r\n\t\t\t\t{\r\n\t\t\t\t\t_coroutine.Resume();\r\n\t\t\t\t}\r\n\t\t\t}\r\n\t\t\tcatch (Exception ex)\r\n\t\t\t{\r\n\t\t\t\tLog.ErrorFormat(\"[Execute] {0}.\", ex);\r\n\t\t\t\tbreak;\r\n\t\t\t}\r\n\t\t}\r\n\r\n\t\t// Cleanup the coroutine if it already exists\r\n\t\tif (_coroutine != null)\r\n\t\t{\r\n\t\t\t_coroutine.Dispose();\r\n\t\t\t_coroutine = null;\r\n\t\t}\r\n\t}\r\n\r\n\tprivate async Task<bool> UserCoroutine()\r\n\t{\r\n\t\tLog.InfoFormat(\"UserCoroutine \");\r\n\r\n\t\treturn false;\r\n\t}\r\n\r\n\tprivate async Task MainCoroutine()\r\n\t{\r\n\t\twhile (true)\r\n\t\t{\r\n\t\t\tif (LokiPoe.IsInLoginScreen)\r\n\t\t\t{\r\n\t\t\t\t// Offload auto login logic to a plugin.\r\n\t\t\t\tforeach (var plugin in PluginManager.EnabledPlugins)\r\n\t\t\t\t{\r\n\t\t\t\t\tawait plugin.Logic(new Logic(\"login_screen\"));\r\n\t\t\t\t}\r\n\t\t\t}\r\n\t\t\telse if (LokiPoe.IsInCharacterSelectionScreen)\r\n\t\t\t{\r\n\t\t\t\t// Offload character selection logic to a plugin.\r\n\t\t\t\tforeach (var plugin in PluginManager.EnabledPlugins)\r\n\t\t\t\t{\r\n\t\t\t\t\tawait plugin.Logic(new Logic(\"character_selection\"));\r\n\t\t\t\t}\r\n\t\t\t}\r\n\t\t\telse if (LokiPoe.IsInGame)\r\n\t\t\t{\r\n\t\t\t\t// Execute user logic until false is returned.\r\n\t\t\t\tif(!await UserCoroutine())\r\n\t\t\t\t\tbreak;\r\n\t\t\t}\r\n\t\t\telse\r\n\t\t\t{\r\n\t\t\t\t// Most likely in a loading screen, which will cause us to block on the executor, \r\n\t\t\t\t// but just in case we hit something else that would cause us to execute...\r\n\t\t\t\tawait Coroutine.Sleep(1000);\r\n\t\t\t\tcontinue;\r\n\t\t\t}\r\n\r\n\t\t\t// End of the tick.\r\n\t\t\tawait Coroutine.Yield();\r\n\t\t}\r\n\t}\r\n}"
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
				Save();
			}
		}
	}
}
