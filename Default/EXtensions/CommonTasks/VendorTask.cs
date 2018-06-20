using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.CommonTasks.VendoringModules;
using Loki.Bot;

namespace Default.EXtensions.CommonTasks
{
    public class VendorTask : ITask
    {
        private readonly VendoringModule[] _modules =
        {
            new CardExchange(),
            new CurrencyExchange(),
            new GcpRecipe()
        };

        public async Task<bool> Run()
        {
            var area = World.CurrentArea;
            if (!area.IsTown && !area.IsHideoutArea)
                return false;

            var module = _modules.FirstOrDefault(m => m.Enabled && m.ShouldExecute);
            if (module != null)
            {
                await module.Execute();
                return true;
            }

            GlobalLog.Info("[VendorTask] No items to vendor.");
            return false;
        }

        public MessageResult Message(Message message)
        {
            if (message.Id == Events.Messages.ItemStashedEvent)
            {
                var item = message.GetInput<CachedItem>();
                foreach (var m in _modules.Where(m => m.Enabled))
                {
                    m.OnStashing(item);
                }

                return MessageResult.Processed;
            }

            if (message.Id == Events.Messages.CombatAreaChanged)
            {
                foreach (var m in _modules.Where(m => m.Enabled))
                {
                    m.ResetErrors();
                }

                return MessageResult.Processed;
            }

            return MessageResult.Unprocessed;
        }

        #region Unused interface methods

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public void Tick()
        {
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public string Name => "VendorTask";
        public string Description => "Task for exchanging various items with vendors.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}