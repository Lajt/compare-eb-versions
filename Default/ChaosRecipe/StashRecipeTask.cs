using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Positions;
using Loki.Bot;
using Loki.Common;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.ChaosRecipe
{
    public class StashRecipeTask : ErrorReporter, ITask
    {
        private static bool _shouldUpdateStashData;

        private bool _stashTabIsFull;

        public async Task<bool> Run()
        {
            if (_stashTabIsFull || ErrorLimitReached)
                return false;

            var area = World.CurrentArea;

            if (!area.IsTown && !area.IsHideoutArea)
                return false;

            if (_shouldUpdateStashData)
            {
                GlobalLog.Debug("[StashRecipeTask] Updating chaos recipe stash data (every Start)");

                if (!await OpenRecipeTab())
                    return true;

                ChaosRecipe.StashData.SyncWithStashTab();

                _shouldUpdateStashData = false;
            }
            else
            {
                if (AnyItemToStash)
                {
                    GlobalLog.Debug("[StashRecipeTask] Updating chaos recipe stash data before actually stashing the items.");

                    if (!await OpenRecipeTab())
                        return true;

                    ChaosRecipe.StashData.SyncWithStashTab();
                }
                else
                {
                    GlobalLog.Info("[StashRecipeTask] No items to stash for chaos recipe.");
                    await Coroutines.CloseBlockingWindows();
                    return false;
                }
            }

            var itemsToStash = ItemsToStash;
            if (itemsToStash.Count == 0)
            {
                GlobalLog.Info("[StashRecipeTask] No items to stash for chaos recipe.");
                await Coroutines.CloseBlockingWindows();
                return false;
            }

            GlobalLog.Info($"[StashRecipeTask] {itemsToStash.Count} items to stash for chaos recipe.");

            foreach (var item in itemsToStash.OrderBy(i => i.Position, Position.Comparer.Instance))
            {
                GlobalLog.Debug($"[StashRecipeTask] Now stashing \"{item.Name}\" for chaos recipe.");

                var itemPos = item.Position;
                if (!Inventories.StashTabCanFitItem(itemPos))
                {
                    GlobalLog.Error("[StashRecipeTask] Stash tab for chaos recipe is full and must be cleaned.");
                    _stashTabIsFull = true;
                    return true;
                }
                if (!await Inventories.FastMoveFromInventory(itemPos))
                {
                    ReportError();
                    return true;
                }
                ChaosRecipe.StashData.IncreaseItemCount(item.ItemType);
                GlobalLog.Info($"[Events] Item stashed ({item.FullName})");
                Utility.BroadcastMessage(this, Events.Messages.ItemStashedEvent, item);
            }

            await Wait.SleepSafe(300);

            ChaosRecipe.StashData.SyncWithStashTab();
            ChaosRecipe.StashData.Log();

            return true;
        }

        public StashRecipeTask()
        {
            ErrorLimitMessage = "[StashRecipeTask] Too many errors. This task will be disabled until combat area change.";
        }

        public void Start()
        {
            if (Settings.Instance.AlwaysUpdateStashData)
                _shouldUpdateStashData = true;
        }

        public MessageResult Message(Message message)
        {
            if (message.Id == Events.Messages.CombatAreaChanged)
            {
                ResetErrors();
                return MessageResult.Processed;
            }
            return MessageResult.Unprocessed;
        }

        private async Task<bool> OpenRecipeTab()
        {
            if (!await Inventories.OpenStashTab(Settings.Instance.StashTab))
            {
                ReportError();
                return false;
            }
            var tabInfo = LokiPoe.InGameState.StashUi.StashTabInfo;
            if (tabInfo.IsPremiumSpecial)
            {
                GlobalLog.Error($"[StashRecipeTask] Invalid stash tab type: {tabInfo.TabType}. This tab cannot be used for chaos recipe.");
                BotManager.Stop();
                return false;
            }
            return true;
        }

        private static bool AnyItemToStash
        {
            get
            {
                foreach (var item in Inventories.InventoryItems)
                {
                    if (ItemEvaluator.Match(item, EvaluationType.ExcludeFromChaosRecipe))
                        continue;

                    if (!RecipeData.IsItemForChaosRecipe(item, out var type))
                        continue;

                    if (ChaosRecipe.StashData.GetItemCount(type) >= Settings.Instance.GetMaxItemCount(type))
                        continue;

                    return true;
                }
                return false;
            }
        }

        private static List<RecipeItem> ItemsToStash
        {
            get
            {
                var result = new List<RecipeItem>();
                var inventoryData = new RecipeData();

                foreach (var item in Inventories.InventoryItems)
                {
                    if (ItemEvaluator.Match(item, EvaluationType.ExcludeFromChaosRecipe))
                        continue;

                    if (!RecipeData.IsItemForChaosRecipe(item, out var type))
                        continue;

                    if (ChaosRecipe.StashData.GetItemCount(type) +
                        inventoryData.GetItemCount(type) >=
                        Settings.Instance.GetMaxItemCount(type))
                        continue;

                    inventoryData.IncreaseItemCount(type);
                    result.Add(new RecipeItem(item, type));
                }
                return result;
            }
        }

        private class RecipeItem : CachedItem
        {
            public Vector2i Position { get; }
            public int ItemType { get; }

            public RecipeItem(Item item, int itemType) : base(item)
            {
                Position = item.LocationTopLeft;
                ItemType = itemType;
            }
        }

        #region Unused interface methods

        public void Tick()
        {
        }

        public void Stop()
        {
        }

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public string Name => "StashRecipeTask";
        public string Description => "Task that stashes items for chaos recipe.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}