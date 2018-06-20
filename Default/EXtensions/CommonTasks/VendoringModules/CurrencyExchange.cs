using System.Collections.Generic;
using System.Threading.Tasks;
using Default.EXtensions.CachedObjects;
using Loki.Bot;
using Loki.Game;
using Loki.Game.GameData;
using PurchaseUi = Loki.Game.LokiPoe.InGameState.PurchaseUi;

namespace Default.EXtensions.CommonTasks.VendoringModules
{
    internal class CurrencyExchange : VendoringModule
    {
        private static readonly List<CurrencyInfo> CurrencyToBuy = new List<CurrencyInfo>();

        public override async Task Execute()
        {
            CurrencyToBuy.Sort((c1, c2) => ExchangePriority[c1.Name].CompareTo(ExchangePriority[c2.Name]));

            var currency = CurrencyToBuy[0];

            GlobalLog.Info($"[VendorTask] Now going to buy {currency.Amount} {currency.Name}.");

            if (!CanBuyInCurrentArea(currency.Name))
            {
                var exchangeArea = GetExchangeArea();
                if (exchangeArea == null)
                {
                    GlobalLog.Warn($"[VendorTask] Cannot exchange \"{currency.Name}\" in current area and Act 3 has not been opened yet.");
                    ResetData();
                    return;
                }
                if (!await PlayerAction.TakeWaypoint(exchangeArea))
                {
                    ReportError();
                    return;
                }
            }

            if (!await CurrencyPurchase(currency))
                ReportError();
        }

        public override void OnStashing(CachedItem item)
        {
            if (item.Type.ItemType != ItemTypes.Currency)
                return;

            CheckCurrency(Settings.TransExchange, CurrencyNames.Transmutation, item.Name);
            CheckCurrency(Settings.AugsExchange, CurrencyNames.Augmentation, item.Name);
            CheckCurrency(Settings.AltsExchange, CurrencyNames.Alteration, item.Name);
            CheckCurrency(Settings.JewsExchange, CurrencyNames.Jeweller, item.Name);
            CheckCurrency(Settings.ChancesExchange, CurrencyNames.Chance, item.Name);
            CheckCurrency(Settings.ScoursExchange, CurrencyNames.Scouring, item.Name);
        }

        public override void ResetData()
        {
            CurrencyToBuy.Clear();
        }

        public override bool Enabled => true;
        public override bool ShouldExecute => CurrencyToBuy.Count > 0 && !ErrorLimitReached;

        #region Subroutines

        private static async Task<bool> CurrencyPurchase(CurrencyInfo currency)
        {
            if (!await TownNpcs.GetCurrencyVendor().OpenPurchasePanel())
                return false;

            await Wait.SleepSafe(500);

            var name = currency.Name;

            var item = PurchaseUi.InventoryControl.Inventory.Items.Find(i => i.Name == name);
            if (item == null)
            {
                GlobalLog.Error($"[CurrencyPurchase] Unexpected error. Fail to find \"{name}\" in vendor's inventory.");
                return false;
            }

            var id = item.LocalId;

            using (new InputDelayOverride(10))
            {
                while (currency.Amount > 0)
                {
                    if (BotManager.IsStopping)
                    {
                        GlobalLog.Debug("[CurrencyPurchase] Bot is stopping. Now breaking from purchase loop.");
                        break;
                    }
                    if (!LokiPoe.IsInGame)
                    {
                        GlobalLog.Error("[CurrencyPurchase] Disconnected during currency purchase.");
                        break;
                    }
                    if (!HasInvenotorySpaceForCurrency(name))
                    {
                        GlobalLog.Warn("[CurrencyPurchase] Not enough inventory space.");
                        break;
                    }

                    GlobalLog.Info($"Purchasing \"{name}\" ({currency.Amount})");

                    var moved = PurchaseUi.InventoryControl.FastMove(id);
                    if (moved != FastMoveResult.None)
                    {
                        GlobalLog.Error($"[CurrencyPurchase] Fail to purchase. Error: \"{moved}\".");
                        return false;
                    }

                    --currency.Amount;
                }
            }

            if (currency.Amount == 0)
                CurrencyToBuy.RemoveAt(0);

            return true;
        }

        #endregion

        #region Helper functions

        private static void CheckCurrency(Settings.ExchangeSettings es, string name, string actualName)
        {
            if (!es.Enabled || name != actualName)
                return;

            int amount = Inventories.GetCurrencyAmountInStashTab(name);
            if (amount >= es.Min + es.Save)
            {
                var exchange = VendorExchanges[name];
                var amoundToBuy = (amount - es.Save) / exchange.Amount;
                var tabName = LokiPoe.InGameState.StashUi.TabControl.CurrentTabName;
                GlobalLog.Warn($"[VendorTask] {amount}(-{es.Save}) {name} in \"{tabName}\" tab will be exchanged to {amoundToBuy} {exchange.Name}.");

                var currency = CurrencyToBuy.Find(c => c.Name == exchange.Name);
                if (currency != null)
                {
                    currency.Amount = amoundToBuy;
                }
                else
                {
                    CurrencyToBuy.Add(new CurrencyInfo(exchange.Name, amoundToBuy));
                }
            }
        }

        private static bool HasInvenotorySpaceForCurrency(string name)
        {
            var inventory = LokiPoe.InGameState.InventoryUi.InventoryControl_Main.Inventory;
            return inventory.AvailableInventorySquares > 0 ||
                   inventory.Items.Exists(i => i.Name == name && i.StackCount < i.MaxStackCount);
        }

        private static bool CanBuyInCurrentArea(string name)
        {
            var area = World.CurrentArea;

            if (!area.IsTown)
                return false;

            if (area == World.Act1.LioneyeWatch)
                return name == CurrencyNames.Augmentation ||
                       name == CurrencyNames.Alteration;

            if (area == World.Act2.ForestEncampment)
                return name == CurrencyNames.Jeweller ||
                       name == CurrencyNames.Fusing ||
                       name == CurrencyNames.Scouring ||
                       name == CurrencyNames.Regret;

            return true;
        }

        private static AreaInfo GetExchangeArea()
        {
            var lastOpenedAct = World.LastOpenedAct.Act;

            if (lastOpenedAct < 3)
                return null;

            var preferedActStr = Settings.Instance.CurrencyExchangeAct;
            if (preferedActStr == "Random")
            {
                var act = LokiPoe.Random.Next(3, lastOpenedAct + 1);
                return Towns[act];
            }

            var preferedAct = int.Parse(preferedActStr);

            if (preferedAct <= lastOpenedAct)
                return Towns[preferedAct];

            return Towns[lastOpenedAct];
        }

        private static readonly Dictionary<string, int> ExchangePriority = new Dictionary<string, int>
        {
            [CurrencyNames.Augmentation] = 1,
            [CurrencyNames.Alteration] = 2,
            [CurrencyNames.Jeweller] = 3,
            [CurrencyNames.Fusing] = 4,
            [CurrencyNames.Scouring] = 5,
            [CurrencyNames.Regret] = 6,
        };

        private static readonly Dictionary<string, CurrencyInfo> VendorExchanges = new Dictionary<string, CurrencyInfo>
        {
            [CurrencyNames.Transmutation] = new CurrencyInfo(CurrencyNames.Augmentation, 4),
            [CurrencyNames.Augmentation] = new CurrencyInfo(CurrencyNames.Alteration, 4),
            [CurrencyNames.Alteration] = new CurrencyInfo(CurrencyNames.Jeweller, 2),
            [CurrencyNames.Jeweller] = new CurrencyInfo(CurrencyNames.Fusing, 4),
            [CurrencyNames.Chance] = new CurrencyInfo(CurrencyNames.Scouring, 4),
            [CurrencyNames.Scouring] = new CurrencyInfo(CurrencyNames.Regret, 2),
        };

        private static readonly AreaInfo[] Towns =
        {
            null,
            World.Act1.LioneyeWatch,
            World.Act2.ForestEncampment,
            World.Act3.SarnEncampment,
            World.Act4.Highgate,
            World.Act5.OverseerTower,
            World.Act6.LioneyeWatch,
            World.Act7.BridgeEncampment,
            World.Act8.SarnEncampment,
            World.Act9.Highgate,
            World.Act10.OriathDocks,
            World.Act11.Oriath,
        };

        #endregion

        #region Helper classes

        private class CurrencyInfo
        {
            public readonly string Name;
            public int Amount;

            public CurrencyInfo(string name, int amount)
            {
                Name = name;
                Amount = amount;
            }
        }

        #endregion
    }
}