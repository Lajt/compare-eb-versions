using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions.Positions;
using Loki.Bot;
using Loki.Common;
using Loki.Game;
using Loki.Game.GameData;

namespace Default.EXtensions.CommonTasks
{
    public class IdTask : ITask
    {
        public async Task<bool> Run()
        {
            var area = World.CurrentArea;

            if (!area.IsTown && !area.IsHideoutArea)
                return false;

            var itemsToId = new List<Vector2i>();
            var itemFilter = ItemEvaluator.Instance;

            foreach (var item in Inventories.InventoryItems)
            {
                if (item.IsIdentified || item.IsCorrupted || item.IsMirrored)
                    continue;

                if (!itemFilter.Match(item, EvaluationType.Id))
                    continue;

                itemsToId.Add(item.LocationTopLeft);
            }

            if (itemsToId.Count == 0)
            {
                GlobalLog.Info("[IdTask] No items to identify.");
                return false;
            }

            GlobalLog.Info($"[IdTask] {itemsToId.Count} items to id.");

            int scrollsAmount = LokiPoe.InstanceInfo.GetPlayerInventoryBySlot(InventorySlot.Main).ItemAmount(CurrencyNames.Wisdom);
            if (scrollsAmount == 0 && Inventories.AvailableInventorySquares == 0)
            {
                GlobalLog.Error("[IdTask] No id scrolls and no free space in inventory. Now stopping the bot because it cannot continue.");
                BotManager.Stop();
                return true;
            }

            GlobalLog.Info($"[IdTask] {scrollsAmount} id scrolls.");

            if (scrollsAmount < itemsToId.Count)
            {
                GlobalLog.Warn("[IdTask] Not enough id scrolls to identify all items. Now going to take them from stash.");

                var result = await Inventories.WithdrawCurrency(CurrencyNames.Wisdom);

                if (result == WithdrawResult.Error)
                {
                    ErrorManager.ReportError();
                    return true;
                }
                if (result == WithdrawResult.Unavailable)
                {
                    GlobalLog.Error("[IdTask] There are no id scrolls in all tabs assigned to them. Now stopping the bot because it cannot continue.");
                    BotManager.Stop();
                    return true;
                }
            }

            if (!await Inventories.OpenInventory())
            {
                ErrorManager.ReportError();
                return true;
            }

            itemsToId.Sort(Position.Comparer.Instance);

            foreach (var pos in itemsToId)
            {
                if (!await Identify(pos))
                {
                    ErrorManager.ReportError();
                    return true;
                }
            }
            await Coroutines.CloseBlockingWindows();
            return true;
        }

        private static async Task<bool> Identify(Vector2i itemPos)
        {
            var item = LokiPoe.InGameState.InventoryUi.InventoryControl_Main.Inventory.FindItemByPos(itemPos);
            if (item == null)
            {
                GlobalLog.Error($"[Identify] Fail to find item at {itemPos} in player's inventory.");
                return false;
            }

            var scroll = Inventories.InventoryItems
                .Where(s => s.Name == CurrencyNames.Wisdom)
                .OrderBy(s => s.StackCount)
                .FirstOrDefault();

            if (scroll == null)
            {
                GlobalLog.Error("[Identify] No id scrolls.");
                return false;
            }

            var name = item.Name;

            GlobalLog.Debug($"[Identify] Now using id scroll on \"{name}\".");

            if (!await LokiPoe.InGameState.InventoryUi.InventoryControl_Main.PickItemToCursor(scroll.LocationTopLeft, true))
                return false;

            if (!await LokiPoe.InGameState.InventoryUi.InventoryControl_Main.PlaceItemFromCursor(itemPos))
                return false;

            if (!await Wait.For(() => IsIdentified(itemPos), "item identification"))
                return false;

            GlobalLog.Debug($"[Identify] \"{name}\" has been successfully identified.");
            return true;
        }

        private static bool IsIdentified(Vector2i pos)
        {
            var item = LokiPoe.InGameState.InventoryUi.InventoryControl_Main.Inventory.FindItemByPos(pos);
            if (item == null)
            {
                GlobalLog.Error("[Identify] Unexpected error. Item became null while waiting for identification.");
                ErrorManager.ReportError();
                return true;
            }
            return item.IsIdentified;
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

        public void Tick()
        {
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public string Name => "IdTask";
        public string Description => "Task that handles item identification.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}