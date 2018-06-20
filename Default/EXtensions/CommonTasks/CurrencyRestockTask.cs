using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loki.Bot;

namespace Default.EXtensions.CommonTasks
{
    public class CurrencyRestockTask : ErrorReporter, ITask
    {
        private static readonly List<string> Unavailable = new List<string>();

        public CurrencyRestockTask()
        {
            ErrorLimitMessage = "[CurrencyRestockTask] Too many errors. Now disabling this task until combat area change.";
        }

        public async Task<bool> Run()
        {
            if (ErrorLimitReached)
                return false;

            var area = World.CurrentArea;

            if (!area.IsTown && !area.IsHideoutArea)
                return false;

            var errors = GetUserErrors();
            if (errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    GlobalLog.Error(error);
                }
                BotManager.Stop();
                return true;
            }

            var currencyToRestock = new List<string>();

            foreach (var currency in Settings.Instance.InventoryCurrencies)
            {
                var name = currency.Name;
                var restock = currency.Restock;

                if (restock < 0)
                    continue;

                GetCurrentAndMaxStackCount(name, out var count, out var maxStackCount);

                if (restock >= maxStackCount)
                {
                    GlobalLog.Error($"[InventoryCurrency] Invalid restock value for \"{name}\". Restock: {restock}. Max stack count: {maxStackCount}.");
                    GlobalLog.Error("[InventoryCurrency] Restock value must be less than item's max stack count. Please correct your settings.");
                    BotManager.Stop();
                    return true;
                }

                if (count > restock)
                    continue;

                if (Unavailable.Contains(name))
                {
                    GlobalLog.Debug($"[CurrencyRestockTask] Skipping \"{name}\" restock because it is marked as unavailable.");
                    continue;
                }

                GlobalLog.Debug($"[CurrencyRestockTask] Restock is needed for \"{name}\". Current count: {count}. Count to restock: {restock}.");
                currencyToRestock.Add(name);
            }

            if (currencyToRestock.Count == 0)
            {
                GlobalLog.Info("[CurrencyRestockTask] No currency to restock.");
                return false;
            }

            foreach (var currency in currencyToRestock)
            {
                GlobalLog.Debug($"[CurrencyRestockTask] Now going to restock \"{currency}\".");

                var result = await Inventories.WithdrawCurrency(currency);

                if (result == WithdrawResult.Error)
                {
                    ReportError();
                    return true;
                }
                if (result == WithdrawResult.Unavailable)
                {
                    GlobalLog.Warn($"[CurrencyRestockTask] There are no \"{currency}\" in all tabs assigned to them. Now marking this currency as unavailable.");
                    Unavailable.Add(currency);
                }
            }
            await Coroutines.CloseBlockingWindows();
            return true;
        }

        private static HashSet<string> GetUserErrors()
        {
            var errors = new HashSet<string>();
            var currencies = Settings.Instance.InventoryCurrencies;

            foreach (var currency in currencies)
            {
                var name = currency.Name;

                if (name == Settings.InventoryCurrency.DefaultName)
                    continue;

                if (currencies.Count(c => c.Name == name) > 1)
                {
                    errors.Add($"[InventoryCurrency] Duplicate name \"{name}\". Please remove all duplicates.");
                }

                var row = currency.Row;
                var column = currency.Column;

                if (row > 5)
                {
                    errors.Add($"[InventoryCurrency] Invalid Row value for \"{name}\". Row cannot be greater than 5.");
                }
                if (column > 12)
                {
                    errors.Add($"[InventoryCurrency] Invalid Column value for \"{name}\". Column cannot be greater than 12.");
                }

                if (row < 1 || column < 1)
                    continue;

                if (currencies.Count(c => c.Row == row && c.Column == column) > 1)
                {
                    errors.Add($"[InventoryCurrency] Duplicate Row and Column combination {row},{column}. Please remove all duplicates.");
                }
            }
            return errors;
        }

        private static void GetCurrentAndMaxStackCount(string name, out int count, out int maxStackCount)
        {
            count = 0;
            maxStackCount = int.MaxValue;

            foreach (var item in Inventories.InventoryItems)
            {
                if (item.Name == name)
                {
                    count += item.StackCount;
                    maxStackCount = item.MaxStackCount;
                }
            }
        }

        public void Start()
        {
            ResetErrors();
            Unavailable.Clear();
        }

        public MessageResult Message(Message message)
        {
            if (message.Id == Events.Messages.CombatAreaChanged)
            {
                ResetErrors();
                Unavailable.Clear();
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

        public void Stop()
        {
        }

        public string Name => "CurrencyRestockTask";
        public string Description => "Task that takes currency from stash if needed.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}