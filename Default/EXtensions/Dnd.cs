// Not used since patch 3.2.2b

namespace Default.EXtensions
{
    //public class Dnd : ErrorReporter
    //{
    //    public Dnd()
    //    {
    //        MaxErrors = 3;
    //        ErrorLimitMessage = "[Dnd] Too many errors. Now disabling this logic until area change.";
    //    }

    //    public async Task<bool> Enter()
    //    {
    //        if (ErrorLimitReached)
    //            return false;

    //        string command;
    //        string msg = Settings.Instance.DndMessage;

    //        if (string.IsNullOrEmpty(msg))
    //        {
    //            GlobalLog.Debug("[Dnd] Now going to enter DND mode.");
    //            command = "/dnd";
    //        }
    //        else
    //        {
    //            GlobalLog.Debug($"[Dnd] Now going to enter DND mode. Message: \"{msg}\".");
    //            command = $"/dnd {msg}";
    //        }

    //        var err = LokiPoe.InGameState.ChatPanel.Chat(command);
    //        if (err != LokiPoe.InGameState.ChatResult.None)
    //        {
    //            GlobalLog.Error($"[Dnd] Chat error: \"{err}\".");
    //            ReportError();
    //            await Wait.SleepSafe(500);
    //            return true;
    //        }
    //        if (!await Wait.For(() => LokiPoe.InGameState.IsDoNotDisturbedEnabled, "DND mode", 100, 1000))
    //        {
    //            ReportError();
    //            return true;
    //        }
    //        return true;
    //    }
    //}
}