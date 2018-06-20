using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions;
using Loki.Bot;
using Loki.Game.GameData;
using Loki.Game.Objects;
using ExSettings = Default.EXtensions.Settings;

namespace Default.MapBot
{
    public class SellMapTask : ITask
    {
        private static readonly GeneralSettings Settings = GeneralSettings.Instance;

        public async Task<bool> Run()
        {
            if (MapBot.IsOnRun)
                return false;

            var area = World.CurrentArea;

            if (!area.IsTown && !area.IsHideoutArea)
                return false;

            MapExtensions.AtlasData.Update();

            if (!Settings.SellEnabled)
                return false;

            if (Inventories.AvailableInventorySquares < 3)
            {
                GlobalLog.Error("[SellMapTask] Not enough inventory space.");
                return false;
            }

            var firstMapTab = ExSettings.Instance.GetTabsForCategory(ExSettings.StashingCategory.Map).First();

            if (!await Inventories.OpenStashTab(firstMapTab))
            {
                GlobalLog.Error($"[SellMapTask] Fail to open stash tab \"{firstMapTab}\".");
                return false;
            }

            var maps = Inventories.StashTabItems
                .Where(m => m.IsMap() && m.ShouldSell())
                .OrderBy(m => m.Priority())
                .ThenBy(m => m.MapTier)
                .ToList();

            if (maps.Count == 0)
                return false;

            var mapGroups = new List<Item[]>();

            foreach (var mapGroup in maps.GroupBy(m => m.Name))
            {
                var groupList = mapGroup.ToList();
                for (int i = 3; i <= groupList.Count; i += 3)
                {
                    var group = new Item[3];
                    group[0] = groupList[i - 3];
                    group[1] = groupList[i - 2];
                    group[2] = groupList[i - 1];
                    mapGroups.Add(group);
                }
            }
            if (mapGroups.Count == 0)
            {
                GlobalLog.Info("[SellMapTask] No map group for sale was found.");
                return false;
            }

            GlobalLog.Info($"[SellMapTask] Map groups for sale: {mapGroups.Count}");

            foreach (var mapGroup in mapGroups)
            {
                if (Inventories.AvailableInventorySquares < 3)
                {
                    GlobalLog.Error("[SellMapTask] Not enough inventory space.");
                    break;
                }

                //exclude ignored maps from min map amount check, if sell ignored maps is enabled
                if (!Settings.SellIgnoredMaps || !mapGroup[0].Ignored())
                {
                    int mapAmount = Inventories.StashTabItems.Count(i => i.IsMap() && i.Rarity != Rarity.Unique);
                    if ((mapAmount - 3) < Settings.MinMapAmount)
                    {
                        GlobalLog.Warn($"[SellMapTask] Min map amount is reached {mapAmount}(-3) from required {Settings.MinMapAmount}");
                        break;
                    }
                }

                for (int i = 0; i < 3; i++)
                {
                    var map = mapGroup[i];
                    GlobalLog.Info($"[SellMapTask] Now getting {i + 1}/{3} \"{map.Name}\".");
                    if (!await Inventories.FastMoveFromStashTab(map.LocationTopLeft))
                    {
                        ErrorManager.ReportError();
                        return true;
                    }
                }
            }

            await Wait.SleepSafe(200);

            var forSell = Inventories.InventoryItems.Where(i => i.IsMap()).Select(m => m.LocationTopLeft).ToList();

            if (forSell.Count == 0)
                return false;

            if (!await TownNpcs.SellItems(forSell))
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

        public void Tick()
        {
        }

        public void Stop()
        {
        }

        public string Name => "SellMapTask";
        public string Description => "Task for selling 3 maps of the same kind";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}