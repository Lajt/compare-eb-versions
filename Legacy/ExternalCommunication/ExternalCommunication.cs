using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using log4net;
using Loki.Bot;
using Loki.Common;
using Loki.Game;

namespace Legacy.ExternalCommunication
{
	internal class ExternalCommunication : IPlugin
	{
		private static readonly ILog Log = Logger.GetLoggerInstanceForType();

		private Gui _instance;

		#region Implementation of IAuthored

		/// <summary> The name of the plugin. </summary>
		public string Name => "ExternalCommunication";

		/// <summary>The author of the plugin.</summary>
		public string Author => "Bossland GmbH";

		/// <summary> The description of the plugin. </summary>
		public string Description => "A plugin that shows an example of external communication with the bot.";

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
		
		#region Implementation of IConfigurable

		/// <summary>The settings object. This will be registered in the current configuration.</summary>
		public JsonSettings Settings => ExternalCommunicationSettings.Instance;

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
			if (message.Id == "wndproc_hook")
			{
				var hwnd = message.GetInput<IntPtr>(0);
				var msg = message.GetInput<int>(1);
				var wParam = message.GetInput<IntPtr>(2).ToInt32();
				var lParam = message.GetInput<IntPtr>(3).ToInt32();

				// WM_USER + 1: Allow programs to check various state.
				if (msg == 0x401)
				{
					// Keep track of any messages encountred.
					//Log.InfoFormat("[WndProc] {0}: {1}", msg, wParam);

					// Is the bot fully loaded. This always returns 1, but when users query it, the message will
					// return 0 since it's not processed.
					if (wParam == 0)
					{

						message.AddOutput(this, LokiPoe.IsBotFullyLoaded ? (IntPtr)1 : IntPtr.Zero);
						return MessageResult.Processed;
					}

					// Is the bot running.
					if (wParam == 1)
					{
						message.AddOutput(this, BotManager.IsRunning ? (IntPtr)1 : IntPtr.Zero);
						return MessageResult.Processed;
					}

					// Is the bot stopping.
					if (wParam == 2)
					{
						message.AddOutput(this, BotManager.IsStopping ? (IntPtr)1 : IntPtr.Zero);
						return MessageResult.Processed;
					}

					// Is the client detected as being frozen.
					if (wParam == 3)
					{
						message.AddOutput(this, BotManager.ClientFrozen ? (IntPtr)1 : IntPtr.Zero);
						return MessageResult.Processed;
					}

					// Time since last tick.
					if (wParam == 4)
					{
						if (!BotManager.IsRunning)
							message.AddOutput(this, IntPtr.Zero);
						else
							message.AddOutput(this, (IntPtr) (DateTime.Now - BotManager.TimeOfLastTick).TotalMilliseconds);
						return MessageResult.Processed;
					}

					// PoE client id attached to
					if (wParam == 5)
					{
						message.AddOutput(this, (IntPtr)LokiPoe.Memory.Process.Id);
						return MessageResult.Processed;
					}

					// The actual bot window handle. 
					if (wParam == 6)
					{
						message.AddOutput(this, LokiPoe.BotWindowHandle);
						return MessageResult.Processed;
					}

					// Is the client in game.
					if (wParam == 7)
					{
						message.AddOutput(this, LokiPoe.IsInGame ? (IntPtr)1 : IntPtr.Zero);
						return MessageResult.Processed;
					}

					// Number of exceptions in the last minute of execution.
					if (wParam == 8)
					{
						message.AddOutput(this, (IntPtr)BotManager.ExceptionCount);
						return MessageResult.Processed;
					}

					// Log unhandled stuff instead.
					Log.InfoFormat("[WndProc] {0}: {1}", msg, wParam);

					// Unhandled.
					message.AddOutput(this, IntPtr.Zero);
					return MessageResult.Processed;
				}
			}

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