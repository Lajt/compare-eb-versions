using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using log4net;
using Loki.Bot;
using Loki.Common;
using System;

namespace Legacy.PythonExample
{
	internal class PythonExample : IPlugin
	{
		private static readonly ILog Log = Logger.GetLoggerInstanceForType();

		private Gui _instance;

		private readonly ScriptManager _scriptManager = new ScriptManager();

		internal ScriptManager ScriptManager => _scriptManager;

		#region Implementation of IAuthored

		/// <summary> The name of the plugin. </summary>
		public string Name => "PythonExample";

		/// <summary>The author of the plugin.</summary>
		public string Author => "Bossland GmbH";

		/// <summary> The description of the plugin. </summary>
		public string Description => "An example plugin to show Python integration.";

		/// <summary>The version of the plugin.</summary>
		public string Version => "0.0.1.1";

		#endregion

		#region Implementation of IBase

		/// <summary>Initializes this plugin.</summary>
		public void Initialize()
		{
			// This logs to the plugin's logger.
			_scriptManager.Initialize(null, new List<string>
			{
				"Loki.Game",
				"Loki.Game.GameData",
				"Loki.Game.Objects",
				"Loki.Game.Objects.Components",
				"Loki.Bot",
				"Loki.Bot.Pathfinding",
				"Loki.Common",
				"Loki",
				"Legacy.PythonExample"
			});

			Hotkeys.Register("PythonExample.RunCode", System.Windows.Forms.Keys.F,
				ModifierKeys.Alt | ModifierKeys.Shift,
				h =>
				{
					if (!PluginManager.IsEnabled(this))
						return;

					if (_instance != null)
					{
						_instance.Dispatcher.BeginInvoke(new Action(() => _instance.ExecutePythonButton_Click(null, null)));
					}
				});

		}

		/// <summary>Deinitializes this object. This is called when the object is being unloaded from the bot.</summary>
		public void Deinitialize()
		{
			_scriptManager.Deinitialize();
		}

		#endregion
		
		#region Implementation of IConfigurable

		/// <summary>The settings object. This will be registered in the current configuration.</summary>
		public JsonSettings Settings => PythonExampleSettings.Instance;

		/// <summary> The plugin's settings control. This will be added to the Exilebuddy Settings tab.</summary>
		public UserControl Control => (_instance ?? (_instance = new Gui(this)));

		#endregion

		#region Implementation of ILogicHandler

		/// <summary>
		/// Implements the ability to handle a logic passed through the system.
		/// </summary>
		/// <param name="logic">The logic to be processed.</param>
		/// <returns>A LogicResult that describes the result..</returns>
		public async Task<LogicResult> Logic(Logic logic)
		{
			return LogicResult.Unprovided;
		}
		
		#endregion

		#region Implementation of IMessageHandler

		/// <summary>
		/// Implements logic to handle a message passed through the system.
		/// </summary>
		/// <param name="message">The message to be processed.</param>
		/// <returns>A tuple of a MessageResult and object.</returns>
		public MessageResult Message(Message message)
		{
			return MessageResult.Unprocessed;
		}

		#endregion

		#region Implementation of IEnableable

		/// <summary> The plugin is being enabled.</summary>
		public void Enable()
		{
		}

		/// <summary> The plugin is being disabled.</summary>
		public void Disable()
		{
		}

		#endregion

		#region Override of Object

		/// <summary>Returns a string that represents the current object.</summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString()
		{
			return Name + ": " + Description;
		}

		#endregion
	}
}