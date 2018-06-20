using System;

namespace Default.EXtensions
{
    public abstract class ErrorReporter
    {
        protected int MaxErrors = 5;
        protected Action OnErrorLimitReached;
        protected string ErrorLimitMessage;

        public int ErrorCount { get; private set; }
        public bool ErrorLimitReached { get; private set; }

        public void ReportError()
        {
            ++ErrorCount;
            GlobalLog.Error($"[{GetType().Name}] Error count: {ErrorCount}/{MaxErrors}");

            if (ErrorCount == MaxErrors)
            {
                ErrorLimitReached = true;

                if (ErrorLimitMessage != null)
                    GlobalLog.Error(ErrorLimitMessage);

                OnErrorLimitReached?.Invoke();
            }
        }

        public void ResetErrors()
        {
            ErrorCount = 0;
            ErrorLimitReached = false;
        }
    }
}