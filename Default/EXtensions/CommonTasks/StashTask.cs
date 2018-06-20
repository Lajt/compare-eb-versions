using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Positions;
using Loki.Bot;
using Loki.Common;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;
using StashUi = Loki.Game.LokiPoe.InGameState.StashUi;

namespace Default.EXtensions.CommonTasks
{
    public class StashTask : ITask
    {
        private static bool _checkInvalidTabs = true;
        private static bool _checkFullTabs = true;

        public async Task<bool> Run()
        {
            var area = World.CurrentArea;

            if (!area.IsTown && !area.IsHideoutArea)
                return false;

            var itemsToStash = new List<StashItem>();
            var inventoryCurrency = Settings.Instance.InventoryCurrencies;

            foreach (var item in Inventories.InventoryItems)
            {
                var c = item.Class;

                if (c == ItemClasses.QuestItem || c == ItemClasses.PantheonSoul)
                    continue;

                if (c == ItemClasses.StackableCurrency && inventoryCurrency.Any(i => i.Name == item.Name))
                    continue;

                itemsToStash.Add(new StashItem(item));
            }

            foreach (var currency in inventoryCurrency)
            {
                foreach (var excess in Inventories.GetExcessCurrency(currency.Name))
                {
                    itemsToStash.Add(new StashItem(excess));
                }
            }

            if (itemsToStash.Count == 0)
            {
                GlobalLog.Info("[StashTask] No items to stash.");
                return false;
            }

            if (_checkInvalidTabs)
            {
                if (!await Inventories.OpenStash())
                {
                    ErrorManager.ReportError();
                    return true;
                }
                var wrongTabs = GetNonexistentTabs();
                if (wrongTabs.Count > 0)
                {
                    GlobalLog.Error("[StashTask] The following tabs are specified in stashing rules but do not exist in stash:");
                    GlobalLog.Error($"{string.Join(", ", wrongTabs)}");
                    GlobalLog.Error("[StashTask] Please provide correct tab names.");
                    BotManager.Stop();
                    return true;
                }
                GlobalLog.Debug("[StashTask] All tabs specified in stashing rules exist in stash.");
                _checkInvalidTabs = false;
            }

            if (_checkFullTabs)
            {
                if (Settings.Instance.FullTabs.Count > 0)
                {
                    if (!await FullTabCheck())
                    {
                        ErrorManager.ReportError();
                        return true;
                    }
                }
                _checkFullTabs = false;
            }

            GlobalLog.Info($"[StashTask] {itemsToStash.Count} items to stash.");

            AssignStashTabs(itemsToStash);

            foreach (var item in itemsToStash.OrderBy(i => i.StashTab).ThenBy(i => i.Position, Position.Comparer.Instance))
            {
                var itemName = item.Name;
                var itemPos = item.Position;
                var tabName = item.StashTab;

                GlobalLog.Debug($"[StashTask] Now going to stash \"{itemName}\" to \"{tabName}\" tab.");

                if (!await Inventories.OpenStashTab(tabName))
                {
                    ErrorManager.ReportError();
                    return true;
                }
                if (StashUi.StashTabInfo.IsPremiumMap)
                {
                    GlobalLog.Error("Map stash tab is unsupported and there are no plans to support it in the future. Please remove it from stashing settings.");
                    BotManager.Stop();
                    return true;
                }
                if (!Inventories.StashTabCanFitItem(itemPos))
                {
                    if (StashUi.StashTabInfo.IsPremiumSpecial)
                    {
                        var metadata = item.Metadata;
                        GlobalLog.Warn($"[StashTask] Cannot fit \"{itemName}\" to \"{tabName}\" tab.");
                        GlobalLog.Warn($"[StashTask] Now marking inventory control for \"{metadata}\" as full.");
                        Settings.Instance.MarkTabAsFull(tabName, metadata);
                    }
                    else
                    {
                        GlobalLog.Warn($"[StashTask] Cannot fit \"{itemName}\" to \"{tabName}\" tab. Now marking this tab as full.");
                        Settings.Instance.MarkTabAsFull(tabName, null);
                    }
                    return true;
                }
                if (!await Inventories.FastMoveFromInventory(itemPos))
                {
                    ErrorManager.ReportError();
                    return true;
                }
                GlobalLog.Info($"[Events] Item stashed ({item.FullName})");
                Utility.BroadcastMessage(this, Events.Messages.ItemStashedEvent, item);
            }
            await Coroutines.CloseBlockingWindows();
            return true;
        }

        private static void AssignStashTabs(IEnumerable<StashItem> items)
        {
            foreach (var item in items)
            {
                var itemType = item.Type.ItemType;
                var metadata = item.Metadata;

                if (itemType == ItemTypes.Currency)
                {
                    if (metadata.Contains("Essence") || metadata.Contains("CorruptMonolith"))
                    {
                        item.StashTab = GetTabForStashing(Settings.StashingCategory.Essence, metadata);
                        continue;
                    }
                    if (metadata.Contains("ItemisedProphecy"))
                    {
                        item.StashTab = GetTabForStashing(Settings.StashingCategory.Prophecy, metadata);
                        continue;
                    }
                    item.StashTab = GetTabForStashing(item.Name, metadata, true);
                    continue;
                }

                if (itemType == ItemTypes.Gem)
                {
                    item.StashTab = GetTabForStashing(Settings.StashingCategory.Gem, metadata);
                    continue;
                }
                if (itemType == ItemTypes.DivinationCard)
                {
                    item.StashTab = GetTabForStashing(Settings.StashingCategory.Card, metadata);
                    continue;
                }
                if (itemType == ItemTypes.Jewel || itemType == ItemTypes.AbyssJewel)
                {
                    item.StashTab = GetTabForStashing(Settings.StashingCategory.Jewel, metadata);
                    continue;
                }
                if (itemType == ItemTypes.Map)
                {
                    item.StashTab = GetTabForStashing(Settings.StashingCategory.Map, metadata);
                    continue;
                }
                if (itemType == ItemTypes.MapFragment)
                {
                    item.StashTab = GetTabForStashing(Settings.StashingCategory.Fragment, metadata);
                    continue;
                }
                if (itemType == ItemTypes.Leaguestone)
                {
                    item.StashTab = GetTabForStashing(Settings.StashingCategory.Leaguestone, metadata);
                    continue;
                }

                var rarity = item.Rarity;
                if (rarity == Rarity.Rare)
                {
                    item.StashTab = GetTabForStashing(Settings.StashingCategory.Rare, metadata);
                    continue;
                }
                if (rarity == Rarity.Unique)
                {
                    item.StashTab = GetTabForStashing(Settings.StashingCategory.Unique, metadata);
                    continue;
                }

                GlobalLog.Warn($"[StashTask] Cannot determine stash tab for \"{item.FullName}\" ({rarity}). It will be stashed to \"Other\" tabs.");
                item.StashTab = GetTabForOther(metadata);
            }
        }

        private static string GetTabForStashing(string name, string metadata, bool forCurrency = false)
        {
            var settings = Settings.Instance;

            var tabs = forCurrency
                ? settings.GetTabsForCurrency(name)
                : settings.GetTabsForCategory(name);

            var tab = tabs.Find(t => !settings.IsTabFull(t, metadata));
            if (tab == null)
            {
                GlobalLog.Warn($"[StashTask] All tabs for \"{name}\" are full. This item will be stashed to \"Other\" tabs.");
                tabs = settings.GetTabsForCategory(Settings.StashingCategory.Other);
                tab = tabs.Find(t => !settings.IsTabFull(t, metadata));
                if (tab == null)
                {
                    GlobalLog.Error($"[StashTask] All tabs for \"{name}\" are full and all \"Other\" tabs are full. Now stopping the bot because it cannot continue.");
                    GlobalLog.Error("[StashTask] Please clean your tabs.");
                    ErrorManager.ReportCriticalError();
                    return null;
                }
            }
            return tab;
        }

        private static string GetTabForOther(string metadata)
        {
            var settings = Settings.Instance;

            var tabs = settings.GetTabsForCategory(Settings.StashingCategory.Other);

            var tab = tabs.Find(t => !settings.IsTabFull(t, metadata));
            if (tab == null)
            {
                GlobalLog.Error("[StashTask] All tabs for \"Other\" are full. Now stopping the bot because it cannot continue.");
                GlobalLog.Error("[StashTask] Please clean your tabs.");
                ErrorManager.ReportCriticalError();
                return null;
            }
            return tab;
        }

        private static HashSet<string> GetNonexistentTabs()
        {
            var result = new HashSet<string>();
            var actualTabs = StashUi.TabControl.TabNames;

            foreach (var rule in Settings.Instance.GeneralStashingRules)
            {
                foreach (var tab in rule.TabList)
                {
                    if (!actualTabs.Contains(tab))
                    {
                        result.Add(tab);
                    }
                }
            }
            foreach (var rule in Settings.Instance.CurrencyStashingRules.Where(r => r.Enabled))
            {
                foreach (var tab in rule.TabList)
                {
                    if (!actualTabs.Contains(tab))
                    {
                        result.Add(tab);
                    }
                }
            }
            return result;
        }

        public static async Task<bool> FullTabCheck()
        {
            if (!await Inventories.OpenStash())
                return false;

            var actualTabs = StashUi.TabControl.TabNames;
            var cleanedTabs = new List<Settings.FullTabInfo>();

            foreach (var tab in Settings.Instance.FullTabs)
            {
                var tabName = tab.Name;

                if (!actualTabs.Contains(tabName))
                {
                    GlobalLog.Warn($"[FullTabCheck] \"{tabName}\" tab no longer exists. Removing it from full tab list.");
                    cleanedTabs.Add(tab);
                    continue;
                }

                if (!await Inventories.OpenStashTab(tabName))
                    return false;

                // Can happen only if user gave map tab a name that was marked as full previously 
                if (StashUi.StashTabInfo.IsPremiumMap)
                {
                    GlobalLog.Error("[FullTabCheck] Map tab is unsupported. Removing it from full tab list.");
                    cleanedTabs.Add(tab);
                    continue;
                }

                if (StashUi.StashTabInfo.IsPremiumSpecial)
                {
                    var controlsMetadata = tab.ControlsMetadata;
                    var cleanedControls = new List<string>();

                    foreach (var metadata in controlsMetadata)
                    {
                        if (PremiumTabCanFit(metadata))
                        {
                            GlobalLog.Warn($"[FullTabCheck] \"{tabName}\" tab is no longer full for \"{metadata}\".");
                            cleanedControls.Add(metadata);
                        }
                        else
                        {
                            GlobalLog.Warn($"[FullTabCheck] \"{tabName}\" tab is still full for \"{metadata}\".");
                        }
                    }
                    foreach (var cleaned in cleanedControls)
                    {
                        controlsMetadata.Remove(cleaned);
                    }
                    if (controlsMetadata.Count == 0)
                    {
                        GlobalLog.Warn($"[FullTabCheck] \"{tabName}\" tab does not contain any controls metadata. Removing it from full tab list.");
                        cleanedTabs.Add(tab);
                    }
                }
                else
                {
                    if (StashUi.InventoryControl.Inventory.AvailableInventorySquares >= 8)
                    {
                        GlobalLog.Warn($"[FullTabCheck] \"{tabName}\" tab is no longer full.");
                        cleanedTabs.Add(tab);
                    }
                    else
                    {
                        GlobalLog.Warn($"[FullTabCheck] \"{tabName}\" tab is still full.");
                    }
                }
            }
            foreach (var tab in cleanedTabs)
            {
                Settings.Instance.FullTabs.Remove(tab);
            }
            return true;
        }

        public static bool PremiumTabCanFit(string metadata)
        {
            var tabType = StashUi.StashTabInfo.TabType;
            if (tabType == InventoryTabType.Currency)
            {
                var control = StashUi.CurrencyTab.GetInventoryControlForMetadata(metadata);

                if (control != null && ControlCanFit(control, metadata))
                    return true;

                return StashUi.CurrencyTab.NonCurrency.Any(miscControl => ControlCanFit(miscControl, metadata));
            }
            if (tabType == InventoryTabType.Essence)
            {
                var control = StashUi.EssenceTab.GetInventoryControlForMetadata(metadata);

                if (control != null && ControlCanFit(control, metadata))
                    return true;

                return StashUi.EssenceTab.NonEssences.Any(miscControl => ControlCanFit(miscControl, metadata));
            }
            if (tabType == InventoryTabType.Divination)
            {
                var control = StashUi.DivinationTab.GetInventoryControlForMetadata(metadata);
                return control != null && ControlCanFit(control, metadata);
            }
            if (tabType == InventoryTabType.Fragment)
            {
                var control = StashUi.FragmentTab.GetInventoryControlForMetadata(metadata);
                return control != null && ControlCanFit(control, metadata);
            }
            GlobalLog.Error("[StashTask] PremiumTabCanFit was called for unknown premium tab type.");
            return false;
        }

        private static bool ControlCanFit(InventoryControlWrapper control, string metadata)
        {
            var item = control.CustomTabItem;

            if (item == null)
                return true;

            if (metadata == "Metadata/Items/Currency/CurrencyItemisedProphecy")
                return false;

            return item.Metadata == metadata && item.StackCount < 5000;
        }

        public static void RequestInvalidTabCheck()
        {
            _checkInvalidTabs = true;
        }

        public static void RequestFullTabCheck()
        {
            _checkFullTabs = true;
        }

        public void Start()
        {
            RequestFullTabCheck();
        }

        private class StashItem : CachedItem
        {
            public Vector2i Position { get; }
            public string StashTab { get; set; }

            public StashItem(Item item) : base(item)
            {
                Position = item.LocationTopLeft;
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

        public void Stop()
        {
        }

        public string Name => "StashTask";

        public string Description => "Task that handles item stashing.";

        public string Author => "ExVault";

        public string Version => "1.0";

        #endregion
    }
}