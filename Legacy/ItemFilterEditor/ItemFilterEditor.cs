using System.Threading.Tasks;
using System.Windows.Controls;
using log4net;
using Loki.Bot;
using Loki.Common;
using Loki.Game;

namespace Legacy.ItemFilterEditor
{
	internal class ItemFilterEditor : IPlugin, IStartStopEvents, ITickEvents
	{
		private static readonly ILog Log = Logger.GetLoggerInstanceForType();

		private Gui _instance;

		/// <summary> The name of the plugin. </summary>
		public string Name => "ItemFilterEditor";

		/// <summary> The description of the plugin. </summary>
		public string Description => "A plugin that provides a basic item filter editor.";

		/// <summary>The author of the plugin.</summary>
		public string Author => "Bossland GmbH";

		/// <summary>The version of the plugin.</summary>
		public string Version => "0.0.1.1";

		/// <summary>Initializes this plugin.</summary>
		public void Initialize()
		{
		}

		/// <summary>Deinitializes this object. This is called when the object is being unloaded from the bot.</summary>
		public void Deinitialize()
		{
		}

		/// <summary> The plugin start callback. Do any initialization here. </summary>
		public void Start()
		{
			// Set the new item eval.
			ItemEvaluator.Instance = ConfigurableItemEvaluator.Instance;
		}
		
		/// <summary> The plugin stop callback. Do any pre-dispose cleanup here. </summary>
		public void Stop()
		{
		}

		public void Tick()
		{
			if (!LokiPoe.IsInGame)
				return;

			// If the flag to refresh the item eval is set, refresh and clear it. We have to do this here, like this,
			// since otherwise the event would come from the Gui thread, and we'd need to AcquireFrame, which via settings
			// is not always the cleanest and can lead to unintended side effects.
			if(ItemFilterEditorSettings.Instance.RefreshItemEvaluator)
			{
				ItemFilterEditorSettings.Instance.RefreshItemEvaluator = false;
				ItemEvaluator.Refresh();
			}
		}

		#region Implementation of IEnableable

		/// <summary>Called when the task should be enabled.</summary>
		public void Enable()
		{
			// Set the new item eval. Set it here in case the user wants to mess with it before starting the bot!
			ItemEvaluator.Instance = ConfigurableItemEvaluator.Instance;
		}

		/// <summary>Called when the task should be disabled.</summary>
		public void Disable()
		{
		}

		#endregion

		#region Implementation of IConfigurable

		/// <summary>The settings object. This will be registered in the current configuration.</summary>
		public JsonSettings Settings => ItemFilterEditorSettings.Instance;

		/// <summary> The plugin's settings control. This will be added to the Exilebuddy Settings tab.</summary>
		public UserControl Control => (_instance ?? (_instance = new Gui()));

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