using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Positions;
using Loki.Bot;
using Loki.Common;
using Loki.Game;
using Loki.Game.Objects;
using DialogUi = Loki.Game.LokiPoe.InGameState.NpcDialogUi;
using InventoryUi = Loki.Game.LokiPoe.InGameState.InventoryUi;
using RewardUi = Loki.Game.LokiPoe.InGameState.RewardUi;
using SellUi = Loki.Game.LokiPoe.InGameState.SellUi;

namespace Default.EXtensions
{
    public static class TownNpcs
    {
        public static readonly TownNpc Nessa = new TownNpc(new WalkablePosition("Nessa", 340, 261));
        public static readonly TownNpc Bestel = new TownNpc(new WalkablePosition("Bestel", 354, 245));
        public static readonly TownNpc Tarkleigh = new TownNpc(new WalkablePosition("Tarkleigh", 381, 189));

        public static readonly TownNpc Greust = new TownNpc(new WalkablePosition("Greust", 195, 171));
        public static readonly TownNpc Eramir = new TownNpc(new WalkablePosition("Eramir", 166, 178));
        public static readonly TownNpc Yeena = new TownNpc(new WalkablePosition("Yeena", 162, 245));
        public static readonly TownNpc Silk = new TownNpc(new WalkablePosition("Silk", 172, 238));
        public static readonly TownNpc Helena = new TownNpc(new WalkablePosition("Helena", 167, 247));

        public static readonly TownNpc Clarissa = new TownNpc(new WalkablePosition("Clarissa", 156, 332));
        public static readonly TownNpc Hargan = new TownNpc(new WalkablePosition("Hargan", 281, 357));
        public static readonly TownNpc Maramoa = new TownNpc(new WalkablePosition("Maramoa", 272, 299));
        public static readonly TownNpc Grigor = new TownNpc(new WalkablePosition("Grigor", 120, 380));

        public static readonly TownNpc Kira = new TownNpc(new WalkablePosition("Kira", 169, 503));
        public static readonly TownNpc PetarusAndVanja = new TownNpc(new WalkablePosition("Petarus and Vanja", 204, 546));
        public static readonly TownNpc Tasuni = new TownNpc(new WalkablePosition("Tasuni", 398, 449));
        public static readonly TownNpc Dialla = new TownNpc(new WalkablePosition("Dialla", 544, 502));
        public static readonly TownNpc Oyun = new TownNpc(new WalkablePosition("Oyun", 557, 496));

        public static readonly TownNpc Utula = new TownNpc(new WalkablePosition("Utula", 350, 310));
        public static readonly TownNpc Lani = new TownNpc(new WalkablePosition("Lani", 364, 305));
        public static readonly TownNpc Vilenta = new TownNpc(new WalkablePosition("Vilenta", 404, 316));
        public static readonly TownNpc Bannon = new TownNpc(new WalkablePosition("Bannon", 411, 341));

        public static readonly TownNpc Bestel_A6 = new TownNpc(new WalkablePosition("Bestel", 354, 383));
        public static readonly TownNpc Tarkleigh_A6 = new TownNpc(new WalkablePosition("Tarkleigh", 385, 327));
        public static readonly TownNpc LillyRoth = new TownNpc(new WalkablePosition("Lilly Roth", 336, 395));

        public static readonly TownNpc Yeena_A7 = new TownNpc(new WalkablePosition("Yeena", 332, 615));
        public static readonly TownNpc Eramir_A7 = new TownNpc(new WalkablePosition("Eramir", 334, 573));
        public static readonly TownNpc Helena_A7 = new TownNpc(new WalkablePosition("Helena", 345, 614));
        public static readonly TownNpc WeylamRoth = new TownNpc(new WalkablePosition("Weylam Roth", 311, 415));

        public static readonly TownNpc Clarissa_A8 = new TownNpc(new WalkablePosition("Clarissa", 146, 338));
        public static readonly TownNpc Hargan_A8 = Hargan;
        public static readonly TownNpc Maramoa_A8 = Maramoa;

        public static readonly TownNpc Tasuni_A9 = Tasuni;
        public static readonly TownNpc PetarusAndVanja_A9 = new TownNpc(new WalkablePosition("Petarus and Vanja", 160, 527));
        public static readonly TownNpc Irasha = new TownNpc(new WalkablePosition("Irasha", 182, 558));

        public static readonly TownNpc Lani_A10 = new TownNpc(new WalkablePosition("Lani", 458, 274));
        public static readonly TownNpc LillyRoth_A10 = new TownNpc(new WalkablePosition("Lilly Roth", 451, 358));
        public static readonly TownNpc WeylamRoth_A10 = new TownNpc(new WalkablePosition("Weylam Roth", 451, 358));

        public static readonly TownNpc Lani_A11 = new TownNpc(new WalkablePosition("Lani", 770, 1056));

        public static TownNpc GetSellVendor()
        {
            var area = World.CurrentArea;

            if (area.IsHideoutArea)
            {
                var npc = LokiPoe.ObjectManager.Objects.Closest<Npc>(n => n.IsTargetable && n.Name != "Alva Valai");
                if (npc == null)
                {
                    GlobalLog.Error("[GetSellVendor] Fail to find any targetable NPC in hideout.");
                    return null;
                }
                return new TownNpc(npc.WalkablePosition());
            }
            if (area.IsTown)
            {
                switch (area.Act)
                {
                    case 11: return Lani_A11;
                    case 10: return Lani_A10;
                    case 9: return PetarusAndVanja_A9;
                    case 8: return Clarissa_A8;
                    case 7: return Yeena_A7;
                    case 6: return Bestel_A6;
                    case 5: return Lani;
                    case 4: return Kira;
                    case 3: return Clarissa;
                    case 2: return Greust;
                    case 1: return Nessa;
                }
            }
            GlobalLog.Error($"[GetSellVendor] Unsupported area for sell vendor \"{area.Name}\".");
            return null;
        }

        public static TownNpc GetCurrencyVendor()
        {
            var area = World.CurrentArea;

            if (area.IsHideoutArea)
            {
                GlobalLog.Error("[GetCurrencyVendor] Cannot buy currency in hideout.");
                return null;
            }
            if (area.IsTown)
            {
                switch (area.Act)
                {
                    case 11: return Lani_A11;
                    case 10: return Lani_A10;
                    case 9: return PetarusAndVanja_A9;
                    case 8: return Clarissa_A8;
                    case 7: return Yeena_A7;
                    case 6: return Bestel_A6;
                    case 5: return Lani;
                    case 4: return PetarusAndVanja;
                    case 3: return Clarissa;
                    case 2: return Yeena;
                    case 1: return Nessa;
                }
            }
            GlobalLog.Error($"[GetCurrencyVendor] Unsupported area for currency vendor \"{area.Name}\".");
            return null;
        }

        public static async Task<bool> SellItems(List<Vector2i> itemPositions)
        {
            if (itemPositions == null)
            {
                GlobalLog.Error("[SellItems] Item list for sell is null.");
                return false;
            }
            if (itemPositions.Count == 0)
            {
                GlobalLog.Error("[SellItems] Item list for sell is empty.");
                return false;
            }

            var vendor = GetSellVendor();

            if (vendor == null)
                return false;

            if (!await vendor.OpenSellPanel())
                return false;

            itemPositions.Sort(Position.Comparer.Instance);

            var soldItems = new List<CachedItem>(itemPositions.Count);

            foreach (var itemPos in itemPositions)
            {
                var item = InventoryUi.InventoryControl_Main.Inventory.FindItemByPos(itemPos);
                if (item == null)
                {
                    GlobalLog.Error($"[SellItems] Fail to find item at {itemPos}. Skipping it.");
                    continue;
                }

                soldItems.Add(new CachedItem(item));

                if (!SellUi.TradeControl.InventoryControl_YourOffer.Inventory.CanFitItem(item.Size))
                    break;

                if (!await Inventories.FastMoveToVendor(itemPos))
                    return false;
            }

            var gainedItems = new List<CachedItem>();

            foreach (var item in SellUi.TradeControl.InventoryControl_OtherOffer.Inventory.Items)
            {
                gainedItems.Add(new CachedItem(item));
            }

            var accepted = SellUi.TradeControl.Accept();
            if (accepted != TradeResult.None)
            {
                GlobalLog.Error($"[SellItems] Fail to accept sell. Error: \"{accepted}\".");
                return false;
            }
            if (!await Wait.For(() => !SellUi.IsOpened, "sell panel closing"))
            {
                await Coroutines.CloseBlockingWindows();
                return false;
            }

            // game needs some time do despawn sold items from inventory
            await Wait.SleepSafe(200);

            GlobalLog.Info($"[Events] Items sold ({soldItems.Count})");
            Utility.BroadcastMessage(null, Events.Messages.ItemsSoldEvent, soldItems, gainedItems);
            return true;
        }
    }

    public class TownNpc
    {
        private readonly NetworkObject _obj;

        public readonly WalkablePosition Position;

        public NetworkObject NpcObject
        {
            get => _obj ?? LokiPoe.ObjectManager.Objects.Find(o => o is Npc && o.Name == Position.Name);
        }

        public TownNpc(WalkablePosition position)
        {
            Position = position;
        }

        // Use NetworkObject.AsTownNpc()
        internal TownNpc(NetworkObject obj)
        {
            _obj = obj;
            Position = obj.WalkablePosition();
        }

        public async Task<bool> Talk()
        {
            await Position.ComeAtOnce();

            var npcObj = NpcObject;
            if (npcObj == null)
            {
                GlobalLog.Error($"[Talk] Fail to find NPC with name \"{Position.Name}\".");
                return false;
            }
            return await PlayerAction.Interact(npcObj, () => DialogUi.IsOpened || RewardUi.IsOpened, "dialog panel opening");
        }

        public async Task<bool> OpenDialogPanel()
        {
            if (!await Talk())
                return false;

            if (RewardUi.IsOpened || DialogUi.DialogDepth != 1)
            {
                const int attempts = 15;
                for (int i = 1; i <= attempts; ++i)
                {
                    GlobalLog.Debug($"[OpenDialogPanel] Pressing ESC to close the topmost NPC dialog ({i}/{attempts}).");

                    LokiPoe.Input.SimulateKeyEvent(Keys.Escape, true, false, false);
                    await Wait.SleepSafe(200);

                    if (!RewardUi.IsOpened && DialogUi.DialogDepth == 1)
                        return true;
                }
                GlobalLog.Error("[OpenDialogPanel] Fail to bring dialog panel to the top.");
                return false;
            }

            if (Settings.Instance.ArtificialDelays)
                await Wait.ArtificialDelay();

            return true;
        }

        public async Task<bool> Converse(string partialDialogName)
        {
            if (!await OpenDialogPanel())
                return false;

            var dialog = DialogUi.DialogEntries.Find(d => d.Text.ContainsIgnorecase(partialDialogName))?.Text;
            if (dialog == null)
            {
                GlobalLog.Error($"[Converse] Fail to find any dialog with \"{partialDialogName}\" in it.");
                return false;
            }
            var err = DialogUi.Converse(dialog);
            if (err != LokiPoe.InGameState.ConverseResult.None)
            {
                GlobalLog.Error($"[Converse] Fail to converse \"{dialog}\". Error: \"{err}\".");
                return false;
            }

            if (Settings.Instance.ArtificialDelays)
                await Wait.ArtificialDelay();

            return true;
        }

        private async Task<bool> OpenRewardPanel(string partialDialogName = null)
        {
            if (partialDialogName == null)
                partialDialogName = "reward";

            if (!await Converse(partialDialogName))
                return false;

            if (!await Wait.For(() => RewardUi.IsOpened, "reward panel opening"))
                return false;

            if (Settings.Instance.ArtificialDelays)
                await Wait.ArtificialDelay();

            return true;
        }

        public async Task<bool> TakeReward(string reward = null, string dialogName = null)
        {
            GlobalLog.Debug($"[TakeReward] Now going to take \"{reward ?? "Any"}\" from {Position.Name}.");

            if (!await OpenRewardPanel(dialogName))
                return false;

            InventoryControlWrapper rewardControl = null;

            var controls = RewardUi.InventoryControls;
            if (controls.Count == 0)
            {
                GlobalLog.Error("[TakeReward] Unknown error. Reward panel is opened, but controls are not loaded.");
                ErrorManager.ReportCriticalError();
                return false;
            }

            if (controls.Count == 1)
            {
                rewardControl = controls[0];
            }
            else
            {
                if (string.IsNullOrEmpty(reward) || reward.EqualsIgnorecase("any"))
                {
                    rewardControl = controls[LokiPoe.Random.Next(0, controls.Count)];
                }
                else
                {
                    foreach (var control in controls)
                    {
                        var item = control.CustomTabItem;
                        if (item.FullName == reward || item.Name == reward)
                        {
                            rewardControl = control;
                            break;
                        }
                    }
                }
            }

            if (rewardControl == null)
            {
                GlobalLog.Error($"[TakeReward] Fail to find reward control with \"{reward}\" item.");
                ErrorManager.ReportCriticalError();
                return false;
            }

            reward = rewardControl.CustomTabItem.FullName;

            var itemCount = Inventories.InventoryItems.Count;

            var err = rewardControl.FastMove();
            if (err != FastMoveResult.None)
            {
                GlobalLog.Error($"[TakeReward] Fail to take \"{reward}\" from {Position.Name}. Error: \"{err}\".");
                return false;
            }

            if (!await Wait.For(() => Inventories.InventoryItems.Count == itemCount + 1, "quest reward appear in inventory"))
                return false;

            GlobalLog.Debug($"[TakeReward] \"{reward}\" has been successfully taken from {Position.Name}.");
            await Wait.SleepSafe(500); //give a quest state time to update
            await Coroutines.CloseBlockingWindows();
            return true;
        }

        public async Task<bool> OpenSellPanel()
        {
            if (SellUi.IsOpened)
                return true;

            GlobalLog.Debug($"[OpenSellPanel] Now going to open sell dialog with {Position.Name}.");

            if (!await Converse("Sell Items"))
                return false;

            if (!await Wait.For(() => SellUi.IsOpened, "sell panel opening"))
                return false;

            GlobalLog.Debug("[OpenSellPanel] Sell panel has been successfully opened.");

            if (Settings.Instance.ArtificialDelays)
                await Wait.ArtificialDelay();

            return true;
        }

        public async Task<bool> OpenPurchasePanel()
        {
            if (LokiPoe.InGameState.PurchaseUi.IsOpened)
                return true;

            GlobalLog.Debug($"[OpenPurchasePanel] Now going to open purchase dialog with {Position.Name}.");

            if (!await Converse("Purchase Items"))
                return false;

            if (!await Wait.For(() => LokiPoe.InGameState.PurchaseUi.IsOpened, "purchase panel opening"))
                return false;

            GlobalLog.Debug("[OpenPurchasePanel] Purchase panel has been successfully opened.");

            if (Settings.Instance.ArtificialDelays)
                await Wait.ArtificialDelay();

            return true;
        }
    }
}