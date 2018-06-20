using System.Threading.Tasks;
using Default.EXtensions.CachedObjects;

namespace Default.EXtensions.CommonTasks.VendoringModules
{
    internal abstract class VendoringModule : ErrorReporter
    {
        protected static readonly Settings Settings = Settings.Instance;

        public abstract Task Execute();
        public abstract void OnStashing(CachedItem item);
        public abstract void ResetData();

        public abstract bool Enabled { get; }
        public abstract bool ShouldExecute { get; }

        protected VendoringModule()
        {
            ErrorLimitMessage = "Too many errors. Now disabling this vendoring module until combat area change.";
            OnErrorLimitReached = ResetData;
        }
    }
}