using System;
using Loki.Game;

namespace Default.EXtensions
{
    public class InputDelayOverride : IDisposable
    {
        private readonly int _oldDelay;

        public InputDelayOverride(int newDelay)
        {
            _oldDelay = LokiPoe.Input.InputEventDelayMs;
            GlobalLog.Info($"[InputDelayOverride] Setting input delay to {newDelay} ms.");
            LokiPoe.Input.InputEventDelayMs = newDelay;
        }

        public void Dispose()
        {
            GlobalLog.Info($"[InputDelayOverride] Restoring original input delay {_oldDelay} ms.");
            LokiPoe.Input.InputEventDelayMs = _oldDelay;
        }
    }
}