using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Positions;
using Loki.Common;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;
using StashUi = Loki.Game.LokiPoe.InGameState.StashUi;
using ExchangeUi = Loki.Game.LokiPoe.InGameState.CardTradeUi;

namespace Default.EXtensions.CommonTasks.VendoringModules
{
    internal class CardExchange : VendoringModule
    {
        private static string _tabWithCardSet;
        private static bool _itemLeftInExchangeUi;

        public override async Task Execute()
        {
            var area = GetCardExchangeArea();
            if (area == null)
            {
                GlobalLog.Warn("[VendorTask] Divination card exchange is not possible because this character has no access to Highgate and we are not in hideout.");
                ResetData();
                return;
            }

            if (_itemLeftInExchangeUi)
            {
                GlobalLog.Warn("[VendorTask] Item was left in ExchangeUi. Now going to take it.");

                if (!area.IsCurrentArea)
                {
                    if (!await PlayerAction.TakeWaypoint(area))
                    {
                        ReportError();
                        return;
                    }
                }

                if (!await TakeItemFromExchangeUi())
                    ReportError();

                return;
            }

            GlobalLog.Info("[VendorTask] Now going to exchange divination cards.");

            if (!await TakeCards())
            {
                ReportError();
                return;
            }

            if (!area.IsCurrentArea)
            {
                if (!await PlayerAction.TakeWaypoint(area))
                {
                    ReportError();
                    return;
                }
            }

            if (!await ExchangeCards())
                ReportError();
        }

        public override void OnStashing(CachedItem item)
        {
            if (_tabWithCardSet != null || item.Type.ItemType != ItemTypes.DivinationCard)
                return;

            var cardSets = StashUi.StashTabInfo.IsPremiumDivination
                ? StashUi.DivinationTab.All.Sum(CardSetsInControl)
                : StashUi.InventoryControl.Inventory.Items.Count(ItemIsCardSet);

            var tabName = StashUi.TabControl.CurrentTabName;

            GlobalLog.Info($"[OnCardStash] Found {cardSets} complete divination card sets in \"{tabName}\" tab.");

            if (cardSets >= Settings.MinCardSets)
            {
                _tabWithCardSet = tabName;
            }
        }

        public override void ResetData()
        {
            _tabWithCardSet = null;
            _itemLeftInExchangeUi = false;
        }

        public override bool Enabled => Settings.CardsEnabled;
        public override bool ShouldExecute => (_tabWithCardSet != null || _itemLeftInExchangeUi) && !ErrorLimitReached;

        #region Subroutines

        private static async Task<bool> TakeCards()
        {
            if (!await Inventories.OpenStashTab(_tabWithCardSet))
                return false;

            if (StashUi.StashTabInfo.IsPremiumDivination)
            {
                using (new InputDelayOverride(10))
                {
                    while (true)
                    {
                        var cardCount = CardCountInInventory;
                        if (cardCount >= Settings.MaxCardSets)
                        {
                            GlobalLog.Warn("[TakeCards] Max card sets per run has been reached.");
                            return true;
                        }

                        var control = StashUi.DivinationTab.Ordered.FirstOrDefault(c => CardSetsInControl(c) > 0);

                        if (control == null)
                        {
                            _tabWithCardSet = null;
                            return true;
                        }

                        var cardName = control.CustomTabItem.Name;
                        GlobalLog.Info($"[TakeCards] Now taking \"{cardName}\".");

                        var moved = StashUi.DivinationTab.Withdraw((i, u) => i.Name == cardName);
                        if (moved != FastMoveResult.None)
                        {
                            GlobalLog.Error($"[TakeCards] Fail to withdraw a card set from Divination Stash Tab. Error: \"{moved}\".");
                            return false;
                        }

                        if (!await Wait.For(() => CardCountInInventory > cardCount, "cards appear in inventory"))
                            return false;

                        if (Settings.ArtificialDelays)
                            await Wait.ArtificialDelay();
                    }
                }
            }

            while (true)
            {
                var cardCount = CardCountInInventory;
                if (cardCount >= Settings.MaxCardSets)
                {
                    GlobalLog.Warn($"[TakeCards] Max card sets for exchange has been reached ({Settings.MaxCardSets})");
                    return true;
                }

                var card = Inventories.StashTabItems
                    .Where(ItemIsCardSet)
                    .OrderBy(i => i.LocationTopLeft, Position.Comparer.Instance)
                    .FirstOrDefault();

                if (card == null)
                {
                    _tabWithCardSet = null;
                    return true;
                }

                GlobalLog.Info($"[TakeCards] Now taking \"{card.Name}\".");

                if (!await Inventories.FastMoveFromStashTab(card.LocationTopLeft))
                    return false;

                if (!await Wait.For(() => CardCountInInventory > cardCount, "cards appear in inventory"))
                    return false;
            }
        }

        private static async Task<bool> ExchangeCards()
        {
            var cardPositions = Inventories.InventoryItems
                .Where(ItemIsCardSet)
                .Select(c => c.LocationTopLeft)
                .ToList();

            if (cardPositions.Count == 0)
            {
                GlobalLog.Error("[ExchangeCards] Fail to find any complete divination set in inventory.");
                return false;
            }

            cardPositions.Sort(Position.Comparer.Instance);

            GlobalLog.Info($"[ExchangeCards] Now going to exchange {cardPositions.Count} divination card sets.");

            if (!ExchangeUi.IsOpened)
            {
                if (!await OpenExchangeUi())
                    return false;
            }
            if (ExchangeUiItem != null)
            {
                if (!await TakeItemFromExchangeUi())
                    return false;
            }
            foreach (var cardPos in cardPositions)
            {
                if (_itemLeftInExchangeUi)
                    break;

                if (!await ExchangeCard(cardPos))
                    return false;

                if (!await TakeItemFromExchangeUi())
                    return false;
            }
            return true;
        }

        private static async Task<bool> ExchangeCard(Vector2i cardPos)
        {
            if (!ExchangeUi.IsOpened)
            {
                if (!await OpenExchangeUi())
                    return false;
            }

            var card = LokiPoe.InGameState.InventoryUi.InventoryControl_Main.Inventory.FindItemByPos(cardPos);
            if (card == null)
            {
                GlobalLog.Error($"[ExchangeCard] Fail to find item at {cardPos}.");
                return false;
            }

            var cardName = card.Name;
            GlobalLog.Info($"[ExchangeCard] Now exchanging \"{cardName}\".");

            if (!await Inventories.FastMoveFromInventory(cardPos))
                return false;

            if (!await Wait.For(() => ExchangeUiItem != null, "divination card appear in ExchangeUi"))
                return false;

            // similar to Map Device UI, sometimes Activate button needs extra time to become clickable.
            await Wait.SleepSafe(200);

            var id = ExchangeUiItem.LocalId;

            var activated = ExchangeUi.Activate();
            if (activated != LokiPoe.InGameState.ActivateResult.None)
            {
                GlobalLog.Error($"[ExchangeCard] Fail to activate the ExchangeUi. Error: \"{activated}\".");
                return false;
            }

            if (!await Wait.For(() =>
            {
                var item = ExchangeUiItem;
                return item != null && item.LocalId != id;
            }, "divination card exchanging"))
                return false;

            WriteToLog(cardName, ExchangeUiItem);
            return true;
        }

        private static async Task<bool> TakeItemFromExchangeUi()
        {
            if (!ExchangeUi.IsOpened)
            {
                if (!await OpenExchangeUi())
                    return false;
            }

            var item = ExchangeUiItem;

            if (item == null)
            {
                GlobalLog.Error("[TakeItemFromExchangeUi] ExchangeUi is empty.");
                return true;
            }

            var name = item.FullName;
            int id = item.LocalId;

            GlobalLog.Debug($"[TakeItemFromExchangeUi] Now taking \"{name}\".");

            if (!LokiPoe.InGameState.InventoryUi.InventoryControl_Main.Inventory.CanFitItem(item.Size))
            {
                GlobalLog.Warn($"[TakeItemFromExchangeUi] Cannot take \"{name}\". Not enough inventory space.");
                _itemLeftInExchangeUi = true;
                return true;
            }

            var moved = ExchangeUi.InventoryControl.FastMove(id);
            if (moved != FastMoveResult.None)
            {
                GlobalLog.Error($"[TakeItemFromExchangeUi] Fast move error: \"{moved}\".");
                return false;
            }

            if (!await Wait.For(() => ExchangeUiItem == null, "item moving from ExchangeUi"))
                return false;

            _itemLeftInExchangeUi = false;
            return true;
        }

        private static async Task<bool> OpenExchangeUi()
        {
            var npc = GetCardExchangeNpc();

            if (npc == null)
                return false;

            if (!await npc.OpenDialogPanel())
                return false;

            var conversed = LokiPoe.InGameState.NpcDialogUi.Converse("Trade Divination Cards");
            if (conversed != LokiPoe.InGameState.ConverseResult.None)
            {
                GlobalLog.Error($"[OpenExchangeUi] Fail to converse \"Trade Divination Cards\". Error: \"{conversed}\".");
                return false;
            }
            if (await Wait.For(() => ExchangeUi.IsOpened, "ExchangeUi opening"))
            {
                GlobalLog.Debug("[OpenExchangeUi] ExchangeUi has been successfully opened.");
                return true;
            }
            return false;
        }

        #endregion

        #region Helper functions

        private static int CardSetsInControl(InventoryControlWrapper control)
        {
            var item = control.CustomTabItem;
            return item == null ? 0 : item.StackCount / item.MaxStackCount;
        }

        private static bool ItemIsCardSet(Item item)
        {
            return item.Class == ItemClasses.DivinationCard && item.StackCount >= item.MaxStackCount;
        }

        private static AreaInfo GetCardExchangeArea()
        {
            var area = World.CurrentArea;

            if (area.IsHideoutArea && Navali != null)
                return area;

            var act = World.LastOpenedAct.Act;

            if (act >= 9)
                return World.Act9.Highgate;

            if (act >= 4)
                return World.Act4.Highgate;

            return null;
        }

        private static TownNpc GetCardExchangeNpc()
        {
            var area = World.CurrentArea;

            if (area.IsHideoutArea)
            {
                var navali = Navali;
                if (navali == null)
                {
                    GlobalLog.Error("[GetCardExchangeNpc] Unexpected error. Fail to find Navali in hideout.");
                    return null;
                }
                return navali.AsTownNpc();
            }

            if (area == World.Act4.Highgate)
                return TownNpcs.Tasuni;

            if (area == World.Act9.Highgate)
                return TownNpcs.Tasuni_A9;

            GlobalLog.Error($"[GetCardExchangeNpc] Unexpected area: {(AreaInfo) area}");
            return null;
        }

        private static Item ExchangeUiItem => ExchangeUi.InventoryControl.Inventory.Items.FirstOrDefault();
        private static int CardCountInInventory => Inventories.InventoryItems.Count(i => i.Class == ItemClasses.DivinationCard);
        private static Npc Navali => LokiPoe.ObjectManager.Objects.FirstOrDefault<Npc>(n => n.Name == "Navali" && n.IsTargetable);

        #endregion

        #region Logging

        private static readonly string PathToLog = Path.Combine(BotStructure.PathToLogs, "DivinationCardExchange.txt");

        private static void WriteToLog(string cardName, Item result)
        {
            var sb = new StringBuilder();
            sb.Append($"[{DateTime.Now}] \"{cardName}\" exchanged to \"{result.FullName}\" ({result.Class}");

            var mods = result.Components.ModsComponent;
            if (mods != null)
            {
                sb.Append($", {mods.Rarity})\n");
            }
            else
            {
                sb.Append(")\n");
            }
            File.AppendAllText(PathToLog, sb.ToString());
        }

        #endregion
    }
}