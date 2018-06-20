using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Buddy.Coroutines;
using log4net;
using Loki;
using Loki.Bot;
using Loki.Common;
using Loki.Game;

namespace Legacy.AutoLogin
{
	internal class AutoLogin : IPlugin, ITickEvents
	{
		private static readonly ILog Log = Logger.GetLoggerInstanceForType();

		private Gui _instance;

		public static Crypto Crypto = new Crypto(Encoding.ASCII.GetBytes(Environment.MachineName + "tube7aXuwaph2su5"));

		private bool _needsLoginReset;
		private bool _needsCharReset;
		private DateTime _lastCharacterSelection = DateTime.Now;
		private int _unexpected = 0;

		#region Implementation of IAuthored

		/// <summary> The name of the plugin. </summary>
		public string Name => "AutoLogin";

		/// <summary>The author of the plugin.</summary>
		public string Author => "Bossland GmbH";

		/// <summary> The description of the plugin. </summary>
		public string Description => "A plugin that provides auto login and character select functionality.";

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

		#region Implementation of ITickEvents

		/// <summary> The plugin tick callback. Do any update logic here. </summary>
		public void Tick()
		{
			if (LokiPoe.IsInGame)
			{
				// Once we are in game, clear the unexpected error tracker since now if we get DCed, it'll be from random
				// server issues as opposed to a login loop.
				if (_unexpected != 0)
				{
					Log.InfoFormat("[AutoLogin] Now clearing _unexpected ({0}).", _unexpected);
					_unexpected = 0;
				}
			}

			if (_needsLoginReset && (LokiPoe.IsInGame || LokiPoe.IsInCharacterSelectionScreen))
			{
				AutoLoginSettings.Instance.LoginAttempts = 0;
				AutoLoginSettings.Instance.NextLoginTime = DateTime.Now;

				_needsLoginReset = false;
			}

			if (_needsCharReset && (LokiPoe.IsInGame || LokiPoe.IsInLoginScreen))
			{
				AutoLoginSettings.Instance.SelectCharacterAttempts = 0;

				_needsCharReset = false;
			}
		}
		
		#endregion

		#region Implementation of IConfigurable

		/// <summary>The settings object. This will be registered in the current configuration.</summary>
		public JsonSettings Settings => AutoLoginSettings.Instance;

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
			if (logic.Id == "hook_login_screen")
			{
				if (await login_screen(logic))
				{
					return LogicResult.Provided;
				}
				return LogicResult.Unprovided;
			}

			if (logic.Id == "hook_character_selection")
			{
				if (await character_selection(logic))
				{
					return LogicResult.Provided;
				}
				return LogicResult.Unprovided;
			}
			
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
			// Support other code setting the time to next login with.
			if (message.Id == "SetNextLoginTime")
			{
				AutoLoginSettings.Instance.NextLoginTime = message.GetInput<DateTime>();
				Log.InfoFormat("[AutoLogin] Execute({0}) = {1}.", message.Id, AutoLoginSettings.Instance.NextLoginTime);
				return MessageResult.Processed;
			}

			// Support other code getting the time to next login with.
			if (message.Id == "GetNextLoginTime")
			{
				message.AddOutput(this, AutoLoginSettings.Instance.NextLoginTime);
				return MessageResult.Processed;
			}

			return MessageResult.Unprocessed;
		}

		#endregion

		#region Implementation of IEnableable

		/// <summary> The plugin is being enabled.</summary>
		public void Enable()
		{
			AutoLoginSettings.Instance.LoginAttempts = 0;
			AutoLoginSettings.Instance.NextLoginTime = DateTime.Now;

			AutoLoginSettings.Instance.SelectCharacterAttempts = 0;
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

		public async Task<bool> login_screen(Logic logic)
		{
			// Sanity check, to make sure the coroutine is in the expected state.
			if (!LokiPoe.IsInLoginScreen)
			{
				Log.WarnFormat("!IsInLoginScreen");
				return false;
			}

			// See if the user wants to auto login or not.
			if (!AutoLoginSettings.Instance.AutoLogin)
			{
				return false;
			}

			// We should reset as soon as we're no longer in the login screen as we most likely have executed logic to login.
			_needsLoginReset = true;

			// NextLoginTime can be set when we chicken or have an unexpected DC, forcing the bot to wait before we login again.
			if (DateTime.Now < AutoLoginSettings.Instance.NextLoginTime)
			{
				var val = (int) ((AutoLoginSettings.Instance.NextLoginTime - DateTime.Now).TotalMilliseconds);
				if (val < 0)
					val = 1;

				Log.DebugFormat("[AutoLogin] NextLoginTime: {0}. Now waiting {1} ms to login again.",
					AutoLoginSettings.Instance.NextLoginTime, val);

				await Coroutine.Sleep(val);

				return true;
			}

			// If the user wants to wait before we attempt to login again, we can sleep here. We don't include time for the
			// NextLoginTime wait, since it's two different mechanics.
			if (AutoLoginSettings.Instance.DelayBeforeLoginAttempt)
			{
				Log.DebugFormat(
					"[AutoLogin] DelayBeforeLoginAttempt | LoginAttemptDelay: {0}.",
					AutoLoginSettings.Instance.LoginAttemptDelay);
				var modifier = LokiPoe.Random.Next(100, 125)/100.0f;
				var sleepTime = (int) (modifier*AutoLoginSettings.Instance.LoginAttemptDelay.TotalMilliseconds);
				if (sleepTime < 1000)
					sleepTime = 1000;
				Log.DebugFormat("[AutoLogin] The bot will wait {0} before logging in.",
					TimeSpan.FromMilliseconds(sleepTime));
				await Coroutine.Sleep(sleepTime);
			}
			else
			{
				const int sleepTime = 1000;
				Log.DebugFormat("[AutoLogin] The bot will wait {0} before logging in.",
					TimeSpan.FromMilliseconds(sleepTime));
				await Coroutine.Sleep(sleepTime);
			}

			// Sanity check, to make sure the coroutine is still in the expected state.
			if (!LokiPoe.IsInLoginScreen)
			{
				Log.DebugFormat("[AutoLogin] !IsInLoginScreen");
				return true;
			}

			// Make sure we don't sit at the login screen spamming failed attempts or something.
			if (AutoLoginSettings.Instance.MaxLoginAttempts > 0 &&
				AutoLoginSettings.Instance.LoginAttempts >= AutoLoginSettings.Instance.MaxLoginAttempts)
			{
				Log.DebugFormat(
					"[AutoLogin] LoginAttempts ({0}) >= MaxLoginAttempts ({1}).",
					AutoLoginSettings.Instance.LoginAttempts, AutoLoginSettings.Instance.MaxLoginAttempts);
				BotManager.Stop(new StopReasonData("autologin_max_login_attempts"));
				return true;
			}

			// Close any blocking error windows now.
			if (LokiPoe.PreGameState.IsMessageBoxActive)
			{
				var title = LokiPoe.PreGameState.MessageBoxTitle.ToLowerInvariant();
				var text = LokiPoe.PreGameState.MessageBoxText.ToLowerInvariant();
				Log.InfoFormat("[AutoLogin][login_screen] {0}: {1}", title, text);

				LokiPoe.PreGameState.ConfirmMessageBox();
				
				if (text.Contains("has ended")) // The league has ended. You cannot create characters in it anymore.
				{
					BotManager.Stop();
					return true;
				}

				// If character selection is throwing errors, then stop the bot.
				if ((DateTime.Now - _lastCharacterSelection).TotalSeconds < 15)
				{
					BotManager.Stop();
					return true;
				}

				await Coroutine.Sleep(1000);
			}
			else
			{
				var exit = false;
				foreach (var entry in LokiPoe.PreGameState.LogEntries)
				{
					Log.InfoFormat("[AutoLogin] {0}", entry);

					// Unable to connect to login server.
					// Also seems to be the message sent from an instance server DC.
					if (entry.Contains("unexpected disconnection"))
					{
						_unexpected++;
						Log.WarnFormat("[AutoLogin] This is the #{0} unexpected error (unexpected disconnection) in a short period of time.", _unexpected);
						if (_unexpected > 5)
						{
							exit = true;
						}
					}
				}
				if (exit)
				{
					BotManager.Stop();
					return true;
				}
			}

			// Perform the login, according to settings and current version.
			LokiPoe.LoginState.LoginError err;
			if (LokiPoe.ClientVersion == LokiPoe.PoeVersion.Official)
			{
				if (AutoLoginSettings.Instance.LoginUsingUserCredentials)
				{
					if (AutoLoginSettings.Instance.LoginUsingGateway)
					{
						err = LokiPoe.LoginState.Login(AutoLoginSettings.Instance.Email, AutoLoginSettings.Instance.Password, AutoLoginSettings.Instance.Gateway);
					}
					else
					{
						err = LokiPoe.LoginState.Login(AutoLoginSettings.Instance.Email, AutoLoginSettings.Instance.Password);
					}
				}
				else
				{
					if (AutoLoginSettings.Instance.LoginUsingGateway)
					{
						err = LokiPoe.LoginState.Login(AutoLoginSettings.Instance.Gateway);
					}
					else
					{
						err = LokiPoe.LoginState.Login();
					}
				}
			}
			else if (LokiPoe.ClientVersion == LokiPoe.PoeVersion.OfficialSteam)
			{
				if (AutoLoginSettings.Instance.LoginUsingGateway)
				{
					err = LokiPoe.LoginState.Login(AutoLoginSettings.Instance.Gateway);
				}
				else
				{
					err = LokiPoe.LoginState.Login();
				}
			}
			else
			{
				Log.ErrorFormat("[AutoLogin] Unknown client version: {0}.", LokiPoe.ClientVersion);
				BotManager.Stop();
				return true;
			}

			// If we didn't click the login button, handle the error.
			if (err != LokiPoe.LoginState.LoginError.None)
			{
				Log.DebugFormat("[AutoLogin] Login returned {0}.", err);

				if (err == LokiPoe.LoginState.LoginError.NotOnLoginScreen)
				{
					return true;
				}

				if (err == LokiPoe.LoginState.LoginError.ProcessHookManagerNotEnabled ||
					err == LokiPoe.LoginState.LoginError.UnlockCodeRequired ||
					err == LokiPoe.LoginState.LoginError.NoCredentials ||
					err == LokiPoe.LoginState.LoginError.TermsOfUsePresent
					)
				{
					BotManager.Stop();
					return true;
				}

				if (err == LokiPoe.LoginState.LoginError.AlreadyConnecting ||
					err == LokiPoe.LoginState.LoginError.InQueue ||
					err == LokiPoe.LoginState.LoginError.ControlNotVisible ||
					err == LokiPoe.LoginState.LoginError.ControlNotEnabled)
				{
					// Wait some before trying to login again under these cases.
					AutoLoginSettings.Instance.NextLoginTime = DateTime.Now +
																TimeSpan.FromMilliseconds(LokiPoe.Random.Next(10000, 15000));

					return true;
				}
			}

			// We clicked the login button, so wait a second for a result, and then figure out what to do.
			AutoLoginSettings.Instance.LoginAttempts++;

			// Wait while we connect, this should fix the longer delay after login on realms with higher latency.
			while (LokiPoe.LoginState.IsConnecting)
			{
				Log.InfoFormat("[AutoLogin] Connecting...");
				await Coroutine.Sleep(100);
			}

			// Let all messages exist for a little time before processing them.
			await Coroutine.Sleep(1000);

			var logEntries = LokiPoe.PreGameState.LogEntries;
			foreach (var entry in logEntries)
			{
				Log.InfoFormat("[AutoLogin] {0}", entry);
			}

			// Perform a dry-run login attempt to assess the current state of things.
			err = LokiPoe.LoginState.Login(true);

			// We logged in if we're no longer on the login screen.
			if (err == LokiPoe.LoginState.LoginError.NotOnLoginScreen)
			{
				return true;
			}

			if (err == LokiPoe.LoginState.LoginError.LoginErrorWindowPresent)
			{
				var text = LokiPoe.PreGameState.MessageBoxText.ToLowerInvariant();
				Log.InfoFormat("[AutoLogin] {0}: {1}", text, LokiPoe.PreGameState.MessageBoxText);

				text = text.ToLowerInvariant();
				if (text.Contains("banned")) // Your Account has been Banned by Administrator
				{
					BotManager.Stop();
				}
				else if (text.Contains("maintenance")) // The realm is currently down for maintenance. Try again later.
				{
					BotManager.Stop();
				}
				else if (text.Contains("patch")) // There has been a patch that you need to update to. PLease restart Path of Exile.
				{
					BotManager.Stop();
				}
			}
			else if (err == LokiPoe.LoginState.LoginError.TermsOfUsePresent)
			{
				Log.Error("[AutoLogin] TermsOfUsePresent - https://www.thebuddyforum.com/threads/exiledbuddy-3-2-0-downtime-status-thread-part-2.418364/#post-2553386");
				BotManager.Stop();
				return true;
			}
			else
			{
				var exit = false;
				foreach (var entry in logEntries)
				{
					// Unable to connect to login server.
					if (entry.Contains("Unable to connect"))
					{
						_unexpected++;
						Log.WarnFormat("[AutoLogin] This is the #{0} unexpected error (unable to connect) in a short period of time.", _unexpected);
						if (_unexpected > 5) // TODO: Make this a setting
						{
							exit = true;
						}
					}
				}
				if (exit)
				{
					BotManager.Stop();
					return true;
				}
			}

			// Wait some for the next login attempt. Most likely a server issue.
			AutoLoginSettings.Instance.NextLoginTime = DateTime.Now +
														TimeSpan.FromMilliseconds(LokiPoe.Random.Next(10000, 15000));

			return true;
		}

		public async Task<bool> character_selection(Logic logic)
		{
			// Sanity check, to make sure the coroutine is in the expected state.
			if (!LokiPoe.IsInCharacterSelectionScreen)
			{
				Log.WarnFormat("!IsInCharacterSelectionScreen");
				return false;
			}

			// See if the user wants to auto login or not.
			if (!AutoLoginSettings.Instance.AutoSelectCharacter)
			{
				return false;
			}

			// We should reset as soon as we're no longer in the character select screen as we most likely have executed logic to select a character.
			_needsCharReset = true;

			// This won't be configurable, because if there's an issue that is not letting us select a character, we
			// should stop the bot.
			if (AutoLoginSettings.Instance.SelectCharacterAttempts > 5)
			{
				Log.DebugFormat("[AutoLogin] We have tried 5 times to select a character and was unsuccessful.");
				BotManager.Stop();
				return true;
			}

			if (AutoLoginSettings.Instance.DelayBeforeSelectingCharacter)
			{
				Log.DebugFormat(
					"[AutoLogin] DelayBeforeSelectingCharacter | SelectCharacterDelay: {0}.",
					AutoLoginSettings.Instance.SelectCharacterDelay);
				var modifier = LokiPoe.Random.Next(100, 125)/100.0f;
				var sleepTime = (int) (modifier*AutoLoginSettings.Instance.SelectCharacterDelay.TotalMilliseconds);
				if (sleepTime < 1000)
					sleepTime = 1000;
				Log.DebugFormat("[AutoLogin] The bot will wait {0} before selecting a character.",
					TimeSpan.FromMilliseconds(sleepTime));
				await Coroutine.Sleep(sleepTime);
			}

			// Just a little soft delay to let characters load.
			var idx = 0;
			while (LokiPoe.IsInCharacterSelectionScreen && !LokiPoe.SelectCharacterState.IsCharacterListLoaded)
			{
				Log.DebugFormat("[AutoLogin] Waiting for the character list to load.");
				await Coroutine.Sleep(1000);
				++idx;
				if (idx > 15)
					break;
			}

			// Be mindful of this, as we can change state in the middle of the coroutine executing.
			if (!LokiPoe.IsInCharacterSelectionScreen)
			{
				Log.DebugFormat("[AutoLogin] !IsInCharacterSelectionScreen");
				return true;
			}

			if (!LokiPoe.SelectCharacterState.IsCharacterListLoaded)
			{
				Log.DebugFormat("[AutoLogin] !IsCharacterListLoaded");
				return true;
			}

			// Close any blocking error windows now.
			if (LokiPoe.PreGameState.IsMessageBoxActive)
			{
				var title = LokiPoe.PreGameState.MessageBoxTitle.ToLowerInvariant();
				var text = LokiPoe.PreGameState.MessageBoxText.ToLowerInvariant();
				Log.InfoFormat("[AutoLogin][character_selection] {0}: {1}", title, text);

				LokiPoe.PreGameState.ConfirmMessageBox();

				await Coroutine.Sleep(1000);

				return true;
			}

			_lastCharacterSelection = DateTime.Now;

			// Attempt to select the character.
			var err = LokiPoe.SelectCharacterState.SelectCharacter(AutoLoginSettings.Instance.Character);

			Log.DebugFormat("[AutoLogin] SelectCharacter returned {0}.", err);

			if (err != LokiPoe.SelectCharacterState.SelectCharacterError.None)
			{
				if (err == LokiPoe.SelectCharacterState.SelectCharacterError.NotOnCharacterSelectionScreen)
				{
					return true;
				}

				BotManager.Stop();
				return true;
			}

			AutoLoginSettings.Instance.SelectCharacterAttempts++;

			await Coroutine.Sleep(100);

			// This logic will try and detect when the character selection screen is left as the client transitions into the game.
			// Before, we just waited a fixed amount and sometimes the character selection logic could run again, but would be in an invalid
			// state, since the client was still connecting.
			idx = 0;
			while (LokiPoe.StateManager.IsWaitingStateActive)
			{
				Log.DebugFormat("[AutoLogin] The WaitingState is active...");
				await Coroutine.Sleep(100);
				++idx;
				if (idx > 10*15)
					break;
			}

			return true;
		}
	}
}