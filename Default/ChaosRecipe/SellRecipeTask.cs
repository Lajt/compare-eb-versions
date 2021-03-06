﻿using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions;
using Loki.Bot;

namespace Default.ChaosRecipe
{
    public class SellRecipeTask : ErrorReporter, ITask
    {
        public async Task<bool> Run()
        {
            if (ErrorLimitReached)
                return false;

            var area = World.CurrentArea;
            if (!area.IsTown && !area.IsHideoutArea)
                return false;

            if (!ChaosRecipe.StashData.HasCompleteSet())
            {
                GlobalLog.Info("[SellRecipeTask] No chaos recipe set to sell.");
                return false;
            }

            GlobalLog.Debug("[SellRecipeTask] Now going to take and sell a set of rare items for chaos recipe.");

            if (!await Inventories.OpenStashTab(Settings.Instance.StashTab))
            {
                ReportError();
                return true;
            }

            ChaosRecipe.StashData.SyncWithStashTab();

            if (!ChaosRecipe.StashData.HasCompleteSet())
            {
                GlobalLog.Error("[SellRecipeTask] Saved stash data does not match actual stash data.");
                await Coroutines.CloseBlockingWindows();
                return false;
            }

            var takenItemData = new RecipeData();

            int lowestItemType = RecipeItemType.None;
            var lowestItem = Inventories.StashTabItems
                .OrderBy(i => i.ItemLevel)
                .FirstOrDefault(i => RecipeData.IsItemForChaosRecipe(i, out lowestItemType));

            if (lowestItem == null)
            {
                GlobalLog.Error("[SellRecipeTask] Unknown error. Fail to find actual item for chaos recipe.");
                ReportError();
                return true;
            }

            GlobalLog.Debug($"[SellRecipeTask] Now taking \"{lowestItem.Name}\" (iLvl: {lowestItem.ItemLevel})");

            if (!await Inventories.FastMoveFromStashTab(lowestItem.LocationTopLeft))
            {
                ReportError();
                return true;
            }

            takenItemData.IncreaseItemCount(lowestItemType);

            while (true)
            {
                int type = RecipeItemType.None;

                for (int i = 0; i < RecipeItemType.TotalTypeCount; i++)
                {
                    var count = takenItemData.GetItemCount(i);
                    if (count == 0 || (count == 1 && i == RecipeItemType.Ring))
                    {
                        type = i;
                        break;
                    }
                }

                if (type == RecipeItemType.None)
                    break;

                var item = Inventories.StashTabItems
                    .Where(i => RecipeData.IsItemForChaosRecipe(i, out var t) && t == type)
                    .OrderByDescending(i => i.ItemLevel)
                    .FirstOrDefault();

                if (item == null)
                {
                    GlobalLog.Error("[SellRecipeTask] Unknown error. Fail to find actual item for chaos recipe.");
                    ReportError();
                    return true;
                }

                GlobalLog.Debug($"[SellRecipeTask] Now taking \"{item.Name}\" (iLvl: {item.ItemLevel})");

                if (!await Inventories.FastMoveFromStashTab(item.LocationTopLeft))
                {
                    ReportError();
                    return true;
                }

                takenItemData.IncreaseItemCount(type);
            }

            await Wait.SleepSafe(300);

            ChaosRecipe.StashData.SyncWithStashTab();

            var recipeItems = Inventories.InventoryItems
                .Where(i => RecipeData.IsItemForChaosRecipe(i, out var t))
                .Select(i => i.LocationTopLeft)
                .ToList();

            if (recipeItems.Count == 0)
            {
                GlobalLog.Error("[SellRecipeTask] Unknown error. There are no items in player's inventory after taking them from stash.");
                ReportError();
                return true;
            }

            if (!await TownNpcs.SellItems(recipeItems))
                ReportError();

            return true;
        }

        public SellRecipeTask()
        {
            ErrorLimitMessage = "[SellRecipeTask] Too many errors. This task will be disabled until combat area change.";
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

        #region Unused interface methods

        public void Tick()
        {
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public string Name => "SellRecipeTask";
        public string Description => "Task that sells items for chaos recipe";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}