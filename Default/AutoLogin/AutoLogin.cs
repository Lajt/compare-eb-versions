using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using Buddy.Coroutines;
using Default.EXtensions;
using log4net;
using Loki.Bot;
using Loki.Common;
using Loki.Game;
using AutoLoginSettings = Default.AutoLogin.Settings;

namespace Default.AutoLogin
{
    public class AutoLogin : IPlugin, IStartStopEvents, ITickEvents
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();
        private Gui _gui;

        private float _currentLoginDelay;
        private int _currentCharSelectAttempt;

        private bool _needsLoginReset;
        private bool _needsCharReset;

        public void Start()
        {
            _currentLoginDelay = AutoLoginSettings.Instance.LoginDelayInitial;
            _currentCharSelectAttempt = 0;
            _needsLoginReset = false;
            _needsCharReset = false;
        }

        public void Tick()
        {
            if (_needsLoginReset && (LokiPoe.IsInGame || LokiPoe.IsInCharacterSelectionScreen))
            {
                _currentLoginDelay = AutoLoginSettings.Instance.LoginDelayInitial;
                _needsLoginReset = false;
            }
            if (_needsCharReset && (LokiPoe.IsInGame || LokiPoe.IsInLoginScreen))
            {
                _currentCharSelectAttempt = 0;
                _needsCharReset = false;
            }
        }

        public async Task<LogicResult> Logic(Logic logic)
        {
            var id = logic.Id;

            if (id == "hook_login_screen")
                return await Login() ? LogicResult.Provided : LogicResult.Unprovided;

            if (id == "hook_character_selection")
                return await CharacterSelection() ? LogicResult.Provided : LogicResult.Unprovided;

            return LogicResult.Unprovided;
        }

        private async Task<bool> Login()
        {
            // Sanity check, to make sure the coroutine is in the expected state.
            if (!LokiPoe.IsInLoginScreen)
            {
                Log.Error("[AutoLogin] Login coroutine has been called, but we are not on login screen.");
                return false;
            }

            // We should reset as soon as we're no longer in the login screen as we most likely have executed logic to login.
            _needsLoginReset = true;

            var settings = AutoLoginSettings.Instance;

            // Do the login delay.
            var delay = Randomize((int) (_currentLoginDelay * 1000), settings.LoginDelayRandPct);
            var nextDelay = _currentLoginDelay + settings.LoginDelayStep;
            _currentLoginDelay = Math.Min(nextDelay, settings.LoginDelayFinal);
            Log.Debug($"[AutoLogin] Doing login delay: {Math.Round(delay / 1000f, 1)} sec");
            await Coroutine.Sleep(delay);

            // Close any blocking error windows now.
            if (LokiPoe.PreGameState.IsMessageBoxActive)
            {
                var title = LokiPoe.PreGameState.MessageBoxTitle;
                var text = LokiPoe.PreGameState.MessageBoxText;

                Log.Info($"[AutoLogin] Login screen message box is present. Title: \"{title}\". Text: \"{text}\".");

                LokiPoe.PreGameState.ConfirmMessageBox();

                if (text.ContainsIgnorecase("has ended"))
                {
                    Log.Warn("[AutoLogin] The league has ended. You cannot create characters in it anymore.");
                    BotManager.Stop();
                    return true;
                }
                await Coroutine.Sleep(1000);
            }

            // Perform the login, according to settings and current version.
            LokiPoe.LoginState.LoginError err;
            var clientVersion = LokiPoe.ClientVersion;

            if (clientVersion == LokiPoe.PoeVersion.Official)
            {
                if (settings.LoginUsingUserCredentials)
                {
                    err = settings.LoginUsingGateway
                        ? LokiPoe.LoginState.Login(settings.Email, settings.Password, settings.Gateway)
                        : LokiPoe.LoginState.Login(settings.Email, settings.Password);
                }
                else
                {
                    err = settings.LoginUsingGateway ? LokiPoe.LoginState.Login(settings.Gateway) : LokiPoe.LoginState.Login();
                }
            }
            else if (clientVersion == LokiPoe.PoeVersion.OfficialSteam)
            {
                err = settings.LoginUsingGateway ? LokiPoe.LoginState.Login(settings.Gateway) : LokiPoe.LoginState.Login();
            }
            else
            {
                Log.Error($"[AutoLogin] Unknown client version: \"{clientVersion}\".");
                BotManager.Stop();
                return true;
            }

            // If we didn't click the login button, handle the error.
            if (err != LokiPoe.LoginState.LoginError.None)
            {
                Log.Warn($"[AutoLogin] Login function returned: \"{err}\".");

                // No reason to continue if any of those errors are present
                if (err == LokiPoe.LoginState.LoginError.ProcessHookManagerNotEnabled ||
                    err == LokiPoe.LoginState.LoginError.UnlockCodeRequired ||
                    err == LokiPoe.LoginState.LoginError.NoCredentials ||
                    err == LokiPoe.LoginState.LoginError.TermsOfUsePresent)
                {
                    BotManager.Stop();
                }
                return true;
            }

            // Wait while we connect, this should fix the longer delay after login on realms with higher latency.
            while (LokiPoe.LoginState.IsConnecting)
            {
                Log.Info("[AutoLogin] Connecting...");
                await Coroutine.Sleep(100);
            }

            // Let all messages exist for a little time before processing them.
            await Coroutine.Sleep(1000);

            var logEntries = LokiPoe.PreGameState.LogEntries;
            foreach (var entry in logEntries)
            {
                Log.Info($"[AutoLogin] {entry}");
            }

            // Perform a dry-run login attempt to assess the current state of things.
            err = LokiPoe.LoginState.Login(true);

            // We logged in if we're no longer on the login screen.
            if (err == LokiPoe.LoginState.LoginError.NotOnLoginScreen)
                return true;

            if (err == LokiPoe.LoginState.LoginError.LoginErrorWindowPresent)
            {
                var title = LokiPoe.PreGameState.MessageBoxTitle;
                var text = LokiPoe.PreGameState.MessageBoxText;

                Log.Info($"[AutoLogin] Login screen message box is present. Title: \"{title}\". Text: \"{text}\".");

                if (text.ContainsIgnorecase("banned") || // Your Account has been Banned by Administrator.
                    text.ContainsIgnorecase("maintenance") || // The realm is currently down for maintenance. Try again later.
                    text.ContainsIgnorecase("patch")) // There has been a patch that you need to update to. Please restart Path of Exile.
                {
                    BotManager.Stop();
                    return true;
                }
            }
            else if (err == LokiPoe.LoginState.LoginError.TermsOfUsePresent)
            {
                Log.Error("[AutoLogin] TermsOfUsePresent - https://www.thebuddyforum.com/threads/exiledbuddy-3-2-0-downtime-status-thread-part-2.418364/#post-2553386");
                BotManager.Stop();
                return true;
            }
            return true;
        }

        public async Task<bool> CharacterSelection()
        {
            // Sanity check, to make sure the coroutine is in the expected state.
            if (!LokiPoe.IsInCharacterSelectionScreen)
            {
                Log.Error("[AutoLogin] Character selection coroutine has been called, but we are not on character selection screen.");
                return false;
            }

            // We should reset as soon as we're no longer in the character select screen as we most likely have executed logic to select a character.
            _needsCharReset = true;

            // This won't be configurable, because if there's an issue that is not letting us select a character, we should stop the bot.
            if (_currentCharSelectAttempt >= 5)
            {
                Log.Error("[AutoLogin] We have tried 5 times to select a character and was unsuccessful.");
                BotManager.Stop();
                return true;
            }

            var settings = AutoLoginSettings.Instance;

            // Do the character selection delay
            Log.Debug($"[AutoLogin] Doing character selection delay: {settings.CharSelectDelay} sec");
            await Coroutine.Sleep((int) (settings.CharSelectDelay * 1000));

            // Just a little soft delay to let characters load.
            var idx = 0;
            while (LokiPoe.IsInCharacterSelectionScreen && !LokiPoe.SelectCharacterState.IsCharacterListLoaded)
            {
                Log.Debug("[AutoLogin] Waiting for the character list to load.");
                await Coroutine.Sleep(1000);
                if (++idx > 15) break;
            }

            // Be mindful of this, as we can change state in the middle of the coroutine executing.
            if (!LokiPoe.IsInCharacterSelectionScreen)
            {
                Log.Debug("[AutoLogin] !IsInCharacterSelectionScreen");
                return true;
            }

            if (!LokiPoe.SelectCharacterState.IsCharacterListLoaded)
            {
                Log.Debug("[AutoLogin] !IsCharacterListLoaded");
                return true;
            }

            // Close any blocking error windows now.
            if (LokiPoe.PreGameState.IsMessageBoxActive)
            {
                var title = LokiPoe.PreGameState.MessageBoxTitle;
                var text = LokiPoe.PreGameState.MessageBoxText;
                Log.Info($"[AutoLogin] Character selection screen message box is present. Title: \"{title}\". Text: \"{text}\".");

                LokiPoe.PreGameState.ConfirmMessageBox();

                await Coroutine.Sleep(1000);
                return true;
            }

            // Attempt to select the character.
            var err = LokiPoe.SelectCharacterState.SelectCharacter(settings.Character);

            if (err != LokiPoe.SelectCharacterState.SelectCharacterError.None)
            {
                Log.Warn($"[AutoLogin] Select character function returned: \"{err}\".");

                if (err == LokiPoe.SelectCharacterState.SelectCharacterError.NotOnCharacterSelectionScreen)
                    return true;

                BotManager.Stop();
                return true;
            }

            ++_currentCharSelectAttempt;

            await Coroutine.Sleep(100);

            // This logic will try and detect when the character selection screen is left as the client transitions into the game.
            // Before, we just waited a fixed amount and sometimes the character selection logic could run again, but would be in an invalid
            // state, since the client was still connecting.
            idx = 0;
            while (LokiPoe.StateManager.IsWaitingStateActive)
            {
                Log.Debug("[AutoLogin] The WaitingState is active...");
                await Coroutine.Sleep(100);
                if (++idx > 150) break;
            }
            return true;
        }

        private static int Randomize(int value, int pct)
        {
            if (pct <= 0)
                return value;

            var range = value * pct / 100;
            var mod = LokiPoe.Random.Next(-range, range);
            return value + mod;
        }

        #region Unused interface methods

        public MessageResult Message(Message message)
        {
            return MessageResult.Unprocessed;
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

        public void Deinitialize()
        {
        }

        public void Enable()
        {
        }

        public void Disable()
        {
        }

        public string Name => "AutoLoginEx";
        public string Description => "Plugin that provides auto login and character selection functionality.";
        public string Author => "Bossland GmbH";
        public string Version => "1.0";
        public UserControl Control => _gui ?? (_gui = new Gui());
        public JsonSettings Settings => AutoLoginSettings.Instance;

        #endregion
    }
}