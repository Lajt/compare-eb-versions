using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Loki.Bot;
using Loki.Game;
using StashUi = Loki.Game.LokiPoe.InGameState.StashUi;

namespace Default.MapBot
{
    public class SextantTask : ErrorReporter, ITask
    {
        private bool _hasWhiteSextants = true;
        private bool _hasYellowSextants = true;
        private bool _hasRedSextants = true;

        private static int? _maxSextants;

        public async Task<bool> Run()
        {
            if (ErrorLimitReached || MapBot.IsOnRun)
                return false;

            var area = World.CurrentArea;
            if (!area.IsTown && !area.IsHideoutArea)
                return false;

            var forSextanting = MapSettings.Instance.MapList.FindAll(m => m.Sextant);

            if (forSextanting.Count == 0)
                return false;

            if (_maxSextants == null)
            {
                if (!await InitializeMaxSextants())
                {
                    ReportError();
                    return true;
                }
            }

            var maps = GetMapInfo(forSextanting);

            if (maps.Count == 0)
                return false;

            FilterMapsBySextantAvailability(maps);

            if (maps.Count == 0)
                return false;

            while (true)
            {
                var map = maps.OrderBy(m => m.Tier).ThenBy(m => MapSettings.Instance.MapDict[m.Name].Priority).FirstOrDefault();

                if (map == null)
                    return false;

                var sextant = GetSextantByTier(map.Tier);

                if (!await Inventories.FindTabWithCurrency(sextant))
                {
                    GlobalLog.Warn($"[SextantTask] We are out of \"{sextant}\". Now marking them as unavailable.");
                    SetSextantUnavailable(map.Tier);
                    FilterMapsBySextantAvailability(maps);
                    continue;
                }

                if (!await ApplySextant(sextant, map.Id))
                {
                    ReportError();
                    return true;
                }

                maps.Remove(map);

                if (BotManager.IsStopping || !LokiPoe.IsInGame)
                    return true;
            }
        }

        private static List<MapInfo> GetMapInfo(List<MapData> mapData)
        {
            var completedMaps = LokiPoe.InstanceInfo.Atlas.CompletedAreas;
            var shapedMaps = LokiPoe.InstanceInfo.Atlas.ShapedAreas;
            var sextantedMaps = LokiPoe.InstanceInfo.Sextants;

            var result = new List<MapInfo>();

            if (sextantedMaps.Count >= _maxSextants)
                return result;

            foreach (var mapName in mapData.Select(m => m.Name))
            {
                var mapArea = completedMaps.Find(m => m.Name == mapName);

                if (mapArea == null)
                    continue;

                var id = mapArea.Id;

                if (sextantedMaps.Exists(s => s.WorldArea.Id == id))
                    continue;

                var tier = mapArea.MonsterLevel - 67;

                if (shapedMaps.Exists(m => m.Name == mapName))
                    tier += 5;

                result.Add(new MapInfo(mapName, id, tier));

                if (result.Count >= (_maxSextants - sextantedMaps.Count))
                    break;
            }

            return result;
        }

        private void FilterMapsBySextantAvailability(List<MapInfo> maps)
        {
            if (!_hasWhiteSextants)
            {
                maps.RemoveAll(m => m.Tier <= 5);
            }
            if (!_hasYellowSextants)
            {
                maps.RemoveAll(m => m.Tier >= 6 && m.Tier <= 10);
            }
            if (!_hasRedSextants)
            {
                maps.RemoveAll(m => m.Tier >= 11);
            }
        }

        private static async Task<bool> ApplySextant(string sextantName, string mapId)
        {
            GlobalLog.Debug($"[ApplySextant] Now going to apply \"{sextantName}\" to \"{mapId}\".");

            if (StashUi.StashTabInfo.IsPremiumCurrency)
            {
                var control = Inventories.GetControlWithCurrency(sextantName);
                if (!await control.PickItemToCursor(true))
                    return false;
            }
            else
            {
                var sextant = Inventories.StashTabItems.Find(i => i.Name == sextantName);
                if (!await StashUi.InventoryControl.PickItemToCursor(sextant.LocationTopLeft, true))
                    return false;
            }

            if (!await OpenAtlasUi())
                return false;

            _maxSextants = LokiPoe.InGameState.AtlasUi.MaxSextants;

            var err = LokiPoe.InGameState.AtlasUi.ApplyCursorTo(mapId);
            if (err != LokiPoe.InGameState.ApplyCursorToAtlasResult.None)
            {
                GlobalLog.Error($"[ApplySextant] Fail to apply \"{sextantName}\" to \"{mapId}\". Error: \"{err}\".");
                return false;
            }

            if (!await Wait.For(() => LokiPoe.InstanceInfo.Sextants.Exists(s => s.WorldArea.Id == mapId), "sextant applying"))
                return false;

            if (!await CloseAtlasUi())
                return false;

            GlobalLog.Debug($"[ApplySextant] \"{sextantName}\" has been successfully applied to \"{mapId}\".");
            return true;
        }

        public static async Task<bool> InitializeMaxSextants()
        {
            if (!await OpenAtlasUi())
                return false;

            _maxSextants = LokiPoe.InGameState.AtlasUi.MaxSextants;
            GlobalLog.Info($"[SextantTask] MaxSextants: {_maxSextants}");

            if (!await CloseAtlasUi())
                return false;

            return true;
        }

        public static async Task<bool> OpenAtlasUi()
        {
            if (LokiPoe.InGameState.AtlasUi.IsOpened)
                return true;

            LokiPoe.Input.SimulateKeyEvent(LokiPoe.Input.Binding.open_atlas_screen, true, false, false);

            if (!await Wait.For(() => LokiPoe.InGameState.AtlasUi.IsOpened, "atlas ui opening"))
                return false;

            return true;
        }

        public static async Task<bool> CloseAtlasUi()
        {
            if (!LokiPoe.InGameState.AtlasUi.IsOpened)
                return true;

            LokiPoe.Input.SimulateKeyEvent(Keys.Escape, true, false, false);

            if (!await Wait.For(() => !LokiPoe.InGameState.AtlasUi.IsOpened, "atlas ui closing"))
                return false;

            return true;
        }

        private static string GetSextantByTier(int tier)
        {
            if (tier <= 5)
                return CurrencyNames.SextantApprentice;
            if (tier <= 10)
                return CurrencyNames.SextantJourneyman;

            return CurrencyNames.SextantMaster;
        }

        private void SetSextantUnavailable(int tier)
        {
            if (tier <= 5)
                _hasWhiteSextants = false;
            else if (tier <= 10)
                _hasYellowSextants = false;
            else
                _hasRedSextants = false;
        }

        public MessageResult Message(Loki.Bot.Message message)
        {
            var id = message.Id;
            if (id == Events.Messages.ItemStashedEvent)
            {
                var itemName = message.GetInput<CachedItem>()?.Name;

                if (!_hasWhiteSextants && itemName == CurrencyNames.SextantApprentice)
                {
                    GlobalLog.Info("[SextantTask] Apprentice Sextant has been stashed. Now marking them as available.");
                    _hasWhiteSextants = true;
                    return MessageResult.Processed;
                }
                if (!_hasYellowSextants && itemName == CurrencyNames.SextantJourneyman)
                {
                    GlobalLog.Info("[SextantTask] Journeyman Sextant has been stashed. Now marking them as available.");
                    _hasYellowSextants = true;
                    return MessageResult.Processed;
                }
                if (!_hasRedSextants && itemName == CurrencyNames.SextantMaster)
                {
                    GlobalLog.Info("[SextantTask] Master Sextant has been stashed. Now marking them as available.");
                    _hasRedSextants = true;
                    return MessageResult.Processed;
                }
                return MessageResult.Unprocessed;
            }
            if (id == Events.Messages.CombatAreaChanged)
            {
                ResetErrors();
                return MessageResult.Processed;
            }
            return MessageResult.Unprocessed;
        }

        public SextantTask()
        {
            ErrorLimitMessage = "[SextantTask] Too many errors. This task will be disabled until combat area change.";
        }

        private class MapInfo
        {
            public readonly string Name;
            public readonly string Id;
            public readonly int Tier;

            public MapInfo(string name, string id, int tier)
            {
                Name = name;
                Id = id;
                Tier = tier;
            }
        }

        #region Unused interface methods

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

        public string Name => "SextantTask";
        public string Description => "Task that applies Cartographer's Sextants to maps on the Atlas.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}