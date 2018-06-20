using System.Threading.Tasks;
using System.Windows.Controls;
using log4net;
using Loki.Bot;
using Loki.Common;
using Loki.Game;

//!CompilerOption|AddRef|HelixToolkit.dll
//!CompilerOption|AddRef|HelixToolkit.Wpf.dll

namespace Legacy.AreaVisualizer
{
	internal class AreaVisualizer : IPlugin, ITickEvents
	{
		private static readonly ILog Log = Logger.GetLoggerInstanceForType();

		private Gui _instance;
		private AreaVisualizerData _data = new AreaVisualizerData();

		#region Implementation of IAuthored

		/// <summary> The name of the plugin. </summary>
		public string Name => "AreaVisualizer";

		/// <summary>The author of the plugin.</summary>
		public string Author => "Bossland GmbH";

		/// <summary> The description of the plugin. </summary>
		public string Description => "A plugin that provides basic area visualization.";

		/// <summary>The version of the plugin.</summary>
		public string Version => "0.0.1.1";

		#endregion

		#region Implementation of IBase

		/// <summary>Initializes this plugin.</summary>
		public void Initialize()
		{
		}

		/// <summary>Deinitializes this object. This is called when the object is being unloaded from the bot.</summary>
		public void Deinitialize()
		{
			_instance?.OnDeinitialize();
		}

		#endregion

		#region Implementation of ITickEvents
		
		/// <summary> The plugin tick callback. Do any update logic here. </summary>
		public void Tick()
		{
			_data.Update();
		}
		
		#endregion

		#region Implementation of IConfigurable

		/// <summary>The settings object. This will be registered in the current configuration.</summary>
		public JsonSettings Settings => AreaVisualizerSettings.Instance;

		/// <summary> The plugin's settings control. This will be added to the Exilebuddy Settings tab.</summary>
		public UserControl Control => (_instance ?? (_instance = new Gui(() => PluginManager.IsEnabled(this), () => _data)));

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
			LokiPoe.OnGuiTick += LokiPoeOnOnGuiTick;
		}

		/// <summary> The plugin is being disabled.</summary>
		public void Disable()
		{
			LokiPoe.OnGuiTick -= LokiPoeOnOnGuiTick;
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

		private void LokiPoeOnOnGuiTick(object sender, GuiTickEventArgs guiTickEventArgs)
		{
			if (!BotManager.IsRunning)
			{
				using (LokiPoe.AcquireFrame())
				{
					_data.Update();
				}
			}
		}
	}
}