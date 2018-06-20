using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions.Positions;
using Loki.Bot;
using Loki.Common;
using Loki.Game;
using Loki.Game.Objects;
using InventoryUi = Loki.Game.LokiPoe.InGameState.InventoryUi;

namespace Default.EXtensions.CommonTasks
{
    public class SortInventoryTask : ITask
    {
        public async Task<bool> Run()
        {
            var area = World.CurrentArea;

            if (!area.IsTown && !area.IsHideoutArea)
                return false;

            if (Inventories.AvailableInventorySquares == 0)
            {
                GlobalLog.Error("[SortInventoryTask] Unexpected state. Main inventory is full. Nothing will be sorted.");
                return false;
            }

            while (true)
            {
                var cursorItem = LokiPoe.InGameState.CursorItemOverlay.Item;
                if (cursorItem != null)
                {
                    var pos = GetMovePosForCursorItem(cursorItem);

                    GlobalLog.Debug($"[SortInventoryTask] Now moving \"{cursorItem.Name}\" from cursor to {pos}");

                    if (!await MoveCursorItem(pos))
                    {
                        ErrorManager.ReportError();
                        return true;
                    }
                    continue;
                }

                var item = GetItemToMove();

                if (item == null)
                    return false;

                GlobalLog.Debug($"[SortInventoryTask] Now moving \"{item.Name}\" from {item.From} to {item.To}");

                if (!await MoveInventoryItem(item.From, item.To))
                {
                    ErrorManager.ReportError();
                    return true;
                }
            }
        }

        private static ItemToMove GetItemToMove()
        {
            foreach (var item in Inventories.InventoryItems)
            {
                var pos = GetMovePosForInventoryItem(item);
                if (pos != null)
                {
                    return new ItemToMove(item.Name, item.LocationTopLeft, pos);
                }
            }
            return null;
        }

        private static Position GetMovePosForInventoryItem(Item item)
        {
            var itemName = item.Name;

            var currency = Settings.Instance.InventoryCurrencies.FirstOrDefault(i => i.Name == itemName);
            if (currency != null && HasFixedPosition(currency))
            {
                var destination = new Position(currency.Column - 1, currency.Row - 1);
                if (destination != item.LocationTopLeft)
                {
                    if (OccupiedBySameItem(itemName, destination))
                    {
                        GlobalLog.Error($"[SortInventoryTask] Unexpected error. \"{itemName}\" will not be sorted correctly because destination position is already occupied by the same item.");
                        return GetMovePosLessThanCurrent(item);
                    }
                    return destination;
                }
                return null;
            }
            return GetMovePosLessThanCurrent(item);
        }

        private static Position GetMovePosForCursorItem(Item item)
        {
            var itemName = item.Name;

            var currency = Settings.Instance.InventoryCurrencies.FirstOrDefault(i => i.Name == itemName);
            if (currency != null && HasFixedPosition(currency))
            {
                var destination = new Position(currency.Column - 1, currency.Row - 1);
                if (OccupiedBySameItem(itemName, destination))
                {
                    GlobalLog.Error($"[SortInventoryTask] Unexpected error. \"{itemName}\" will not be sorted correctly because destination position is already occupied by the same item.");
                    return GetMovePosFirstAvailable(item);
                }
                return destination;
            }
            return GetMovePosFirstAvailable(item);
        }

        private static Position GetMovePosLessThanCurrent(Item item)
        {
            var pos = GetMovePosFirstAvailable(item);
            return Position.Comparer.Instance.Compare(pos, item.LocationTopLeft) < 0 ? pos : null;
        }

        private static Position GetMovePosFirstAvailable(Item item)
        {
            if (!InventoryUi.InventoryControl_Main.Inventory.CanFitItem(item.Size, out int x, out int y))
            {
                GlobalLog.Error("[SortInventoryTask] Unexpected error. Cannot fit item anywhere in main inventory.");
                ErrorManager.ReportCriticalError();
            }
            return new Position(x, y);
        }

        private static bool HasFixedPosition(Settings.InventoryCurrency item)
        {
            return item.Row > 0 && item.Column > 0;
        }

        private static bool OccupiedBySameItem(string name, Vector2i pos)
        {
            var item = Inventories.InventoryItems.Find(i => i.LocationTopLeft == pos);
            return item != null && item.Name == name;
        }

        private static async Task<bool> MoveCursorItem(Vector2i to)
        {
            if (!await Inventories.OpenInventory())
                return false;

            if (!await InventoryUi.InventoryControl_Main.PlaceItemFromCursor(to))
                return false;

            return true;
        }

        private static async Task<bool> MoveInventoryItem(Vector2i from, Vector2i to)
        {
            if (!await Inventories.OpenInventory())
                return false;

            if (!await InventoryUi.InventoryControl_Main.PickItemToCursor(from))
                return false;

            if (!await InventoryUi.InventoryControl_Main.PlaceItemFromCursor(to))
                return false;

            return true;
        }

        private class ItemToMove
        {
            public readonly string Name;
            public readonly Vector2i From;
            public readonly Vector2i To;

            public ItemToMove(string name, Vector2i from, Vector2i to)
            {
                Name = name;
                From = from;
                To = to;
            }
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

        public string Name => "SortInventoryTask";
        public string Author => "ExVault";
        public string Description => "Task for organizing items in inventory";
        public string Version => "1.0";

        #endregion
    }
}