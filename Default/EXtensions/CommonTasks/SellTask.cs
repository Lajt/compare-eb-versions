using System.Collections.Generic;
using System.Threading.Tasks;
using Loki.Bot;
using Loki.Common;
using Loki.Game.GameData;

namespace Default.EXtensions.CommonTasks
{
    public class SellTask : ITask
    {
        public async Task<bool> Run()
        {
            var area = World.CurrentArea;
            if (!area.IsTown && !area.IsHideoutArea)
                return false;

            var itemsToSell = new List<Vector2i>();
            var itemFilter = ItemEvaluator.Instance;

            foreach (var item in Inventories.InventoryItems)
            {
                var c = item.Class;

                if (c == ItemClasses.QuestItem || c == ItemClasses.PantheonSoul)
                    continue;

                if (item.HasMicrotransitionAttachment || item.HasSkillGemsEquipped)
                    continue;

                if (!itemFilter.Match(item, EvaluationType.Sell))
                    continue;

                if (itemFilter.Match(item, EvaluationType.Save))
                    continue;

                itemsToSell.Add(item.LocationTopLeft);
            }

            if (Settings.Instance.SellExcessPortals)
            {
                foreach (var portal in Inventories.GetExcessCurrency(CurrencyNames.Portal))
                {
                    itemsToSell.Add(portal.LocationTopLeft);
                }
            }

            if (itemsToSell.Count == 0)
            {
                GlobalLog.Info("[SellTask] No items to sell.");
                return false;
            }

            GlobalLog.Info($"[SellTask] {itemsToSell.Count} items to sell.");

            if (!await TownNpcs.SellItems(itemsToSell))
                ErrorManager.ReportError();

            return true;
        }

        #region Unused interface methods

        public MessageResult Message(Message message)
        {
            return MessageResult.Unprocessed;
        }

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Tick()
        {
        }

        public string Name => "SellTask";

        public string Description => "Task that handles item selling.";

        public string Author => "ExVault";

        public string Version => "1.0";

        #endregion
    }
}