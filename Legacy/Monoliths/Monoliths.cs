using System.Threading.Tasks;
using System.Windows.Controls;
using log4net;
using Loki.Bot;
using Loki.Common;

namespace Legacy.Monoliths
{
	internal class Monoliths : IPlugin, IStartStopEvents, ITickEvents
	{
		private static readonly ILog Log = Logger.GetLoggerInstanceForType();

		private readonly TaskManager _taskManager = new TaskManager();

		private Gui _instance;

		#region Implementation of IAuthored

		/// <summary> The name of the plugin. </summary>
		public string Name => "Monoliths";

		/// <summary>The author of the plugin.</summary>
		public string Author => "Bossland GmbH";

		/// <summary> The description of the plugin. </summary>
		public string Description => "A plugin that provides basic Monolith interaction.";

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
		}

		#endregion

		#region Implementation of ITickEvents / IStartStopEvents

		/// <summary> The plugin start callback. Do any initialization here. </summary>
		public void Start()
		{
			_taskManager.Reset();
			_taskManager.Add(new HandleMonolithsTask());
			_taskManager.Freeze();

			_taskManager.Start();
		}

		/// <summary> The plugin tick callback. Do any update logic here. </summary>
		public void Tick()
		{
			_taskManager.Tick(); // TaskManager will check IsInGame
		}

		/// <summary> The plugin stop callback. Do any pre-dispose cleanup here. </summary>
		public void Stop()
		{
			_taskManager.Stop();
		}

		#endregion

		#region Implementation of IConfigurable

		/// <summary>The settings object. This will be registered in the current configuration.</summary>
		public JsonSettings Settings => MonolithsSettings.Instance;

		/// <summary> The plugin's settings control. This will be added to the Exilebuddy Settings tab.</summary>
		public UserControl Control => (_instance ?? (_instance = new Gui()));

		#endregion

		#region Implementation of ILogicProvider

		/// <summary>
		/// Implements the ability to handle a logic passed through the system.
		/// </summary>
		/// <param name="logic">The logic to be processed.</param>
		/// <returns>A LogicResult that describes the result..</returns>
		public async Task<LogicResult> Logic(Logic logic)
		{
			if (logic.Id == "hook_post_combat")
			{
				return await _taskManager.Run(TaskGroup.Enabled, RunBehavior.UntilHandled) == RunTasksResult.TasksRan
					? LogicResult.Provided
					: LogicResult.Unprovided;
			}

			return await _taskManager.ProvideLogic(TaskGroup.Enabled, RunBehavior.UntilHandled, logic);
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
			return _taskManager.SendMessage(TaskGroup.Enabled, message);
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