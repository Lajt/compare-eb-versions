using System.Collections.Generic;
using System.IO;
using System.Linq;
using Default.EXtensions;
using Loki;
using Newtonsoft.Json;

namespace Default.MapBot
{
    public class MapSettings
    {
        private static readonly string SettingsPath = Path.Combine(Configuration.Instance.Path, "MapBot", "MapSettings.json");

        private static MapSettings _instance;
        public static MapSettings Instance => _instance ?? (_instance = new MapSettings());

        private MapSettings()
        {
            InitList();
            Load();
            InitDict();
            Configuration.OnSaveAll += (sender, args) => { Save(); };

            MapList = MapList.OrderByDescending(m => m.Priority).ToList();
        }

        public List<MapData> MapList { get; } = new List<MapData>();
        public Dictionary<string, MapData> MapDict { get; } = new Dictionary<string, MapData>();

        private void InitList()
        {
            MapList.Add(new MapData(MapNames.Lookout, 1, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Beach, 1, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Graveyard, 1, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Dungeon, 1, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Alleyways, 2, MapType.Regular));
            MapList.Add(new MapData(MapNames.Pen, 2, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Desert, 2, MapType.Regular));
            MapList.Add(new MapData(MapNames.AridLake, 2, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.FloodedMine, 2, MapType.Regular));
            MapList.Add(new MapData(MapNames.Marshes, 2, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Iceberg, 3, MapType.Regular));
            MapList.Add(new MapData(MapNames.Cage, 3, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Springs, 3, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Excavation, 3, MapType.Complex));
            MapList.Add(new MapData(MapNames.Leyline, 3, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Peninsula, 3, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Port, 3, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.BurialChambers, 3, MapType.Multilevel));
            MapList.Add(new MapData(MapNames.Cells, 3, MapType.Regular));
            MapList.Add(new MapData(MapNames.Arcade, 3, MapType.Regular));
            MapList.Add(new MapData(MapNames.CitySquare, 4, MapType.Regular));
            MapList.Add(new MapData(MapNames.RelicChambers, 4, MapType.Regular));
            MapList.Add(new MapData(MapNames.Courthouse, 4, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Strand, 4, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Chateau, 4, MapType.Regular));
            MapList.Add(new MapData(MapNames.Grotto, 4, MapType.Regular));
            MapList.Add(new MapData(MapNames.Gorge, 4, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Volcano, 4, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Lighthouse, 4, MapType.Regular));
            MapList.Add(new MapData(MapNames.Canyon, 4, MapType.Regular));
            MapList.Add(new MapData(MapNames.Conservatory, 5, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.SulphurVents, 5, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.HauntedMansion, 5, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Maze, 5, MapType.Regular));
            MapList.Add(new MapData(MapNames.Channel, 5, MapType.Regular));
            MapList.Add(new MapData(MapNames.ToxicSewer, 5, MapType.Regular));
            MapList.Add(new MapData(MapNames.AncientCity, 5, MapType.Regular));
            MapList.Add(new MapData(MapNames.IvoryTemple, 5, MapType.Complex));
            MapList.Add(new MapData(MapNames.SpiderLair, 5, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Barrows, 5, MapType.Complex));
            MapList.Add(new MapData(MapNames.Mausoleum, 6, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Fields, 6, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.JungleValley, 6, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Phantasmagoria, 6, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Academy, 6, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Thicket, 6, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Wharf, 6, MapType.Regular));
            MapList.Add(new MapData(MapNames.AshenWood, 6, MapType.Regular));
            MapList.Add(new MapData(MapNames.Atoll, 6, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Cemetery, 6, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.UndergroundSea, 6, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Tribunal, 7, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.CoralRuins, 7, MapType.Regular));
            MapList.Add(new MapData(MapNames.LavaChamber, 7, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Residence, 7, MapType.Multilevel));
            MapList.Add(new MapData(MapNames.Ramparts, 7, MapType.Multilevel));
            MapList.Add(new MapData(MapNames.Dunes, 7, MapType.Regular));
            MapList.Add(new MapData(MapNames.BoneCrypt, 7, MapType.Regular));
            MapList.Add(new MapData(MapNames.UndergroundRiver, 7, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Gardens, 7, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.ArachnidNest, 7, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Bazaar, 7, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Laboratory, 8, MapType.Bossroom) {UnsupportedBossroom = true});
            MapList.Add(new MapData(MapNames.InfestedValley, 8, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.OvergrownRuin, 8, MapType.Multilevel));
            MapList.Add(new MapData(MapNames.VaalPyramid, 8, MapType.Multilevel));
            MapList.Add(new MapData(MapNames.Geode, 8, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Armoury, 8, MapType.Regular));
            MapList.Add(new MapData(MapNames.Courtyard, 8, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.MudGeyser, 8, MapType.Regular));
            MapList.Add(new MapData(MapNames.Shore, 8, MapType.Regular));
            MapList.Add(new MapData(MapNames.TropicalIsland, 8, MapType.Multilevel));
            MapList.Add(new MapData(MapNames.MineralPools, 8, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.MoonTemple, 9, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Sepulchre, 9, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Tower, 9, MapType.Multilevel));
            MapList.Add(new MapData(MapNames.WastePool, 9, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Plateau, 9, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Estuary, 9, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Vault, 9, MapType.Bossroom) {Unsupported = true});
            MapList.Add(new MapData(MapNames.Temple, 9, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Arena, 9, MapType.Complex));
            MapList.Add(new MapData(MapNames.Museum, 9, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Scriptorium, 9, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Siege, 10, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Shipyard, 10, MapType.Regular));
            MapList.Add(new MapData(MapNames.Belfry, 10, MapType.Bossroom) {UnsupportedBossroom = true});
            MapList.Add(new MapData(MapNames.ArachnidTomb, 10, MapType.Multilevel));
            MapList.Add(new MapData(MapNames.Wasteland, 10, MapType.Regular));
            MapList.Add(new MapData(MapNames.Precinct, 10, MapType.Regular));
            MapList.Add(new MapData(MapNames.Bog, 10, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Pier, 10, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.CursedCrypt, 10, MapType.Regular));
            MapList.Add(new MapData(MapNames.Orchard, 10, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Promenade, 11, MapType.Regular));
            MapList.Add(new MapData(MapNames.Lair, 11, MapType.Multilevel));
            MapList.Add(new MapData(MapNames.Colonnade, 11, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.PrimordialPool, 11, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.SpiderForest, 11, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Coves, 11, MapType.Regular));
            MapList.Add(new MapData(MapNames.Waterways, 11, MapType.Regular));
            MapList.Add(new MapData(MapNames.Factory, 11, MapType.Regular));
            MapList.Add(new MapData(MapNames.Mesa, 11, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Pit, 11, MapType.Regular));
            MapList.Add(new MapData(MapNames.DefiledCathedral, 12, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Summit, 12, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.OvergrownShrine, 12, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.CastleRuins, 12, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.CrystalOre, 12, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Villa, 12, MapType.Multilevel));
            MapList.Add(new MapData(MapNames.TortureChamber, 12, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Necropolis, 12, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Racecourse, 12, MapType.Multilevel));
            MapList.Add(new MapData(MapNames.Caldera, 13, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Ghetto, 13, MapType.Regular));
            MapList.Add(new MapData(MapNames.Park, 13, MapType.Regular));
            MapList.Add(new MapData(MapNames.Malformation, 13, MapType.Multilevel));
            MapList.Add(new MapData(MapNames.Terrace, 13, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Shrine, 13, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Arsenal, 13, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.DesertSpring, 13, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Core, 13, MapType.Bossroom) {UnsupportedBossroom = true});
            MapList.Add(new MapData(MapNames.Colosseum, 14, MapType.Multilevel));
            MapList.Add(new MapData(MapNames.AcidLakes, 14, MapType.Regular));
            MapList.Add(new MapData(MapNames.DarkForest, 14, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.CrimsonTemple, 14, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Plaza, 14, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Dig, 14, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Palace, 14, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.LavaLake, 15, MapType.Bossroom) {UnsupportedBossroom = true});
            MapList.Add(new MapData(MapNames.Basilica, 15, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.SunkenCity, 15, MapType.Bossroom) {UnsupportedBossroom = true});
            MapList.Add(new MapData(MapNames.Reef, 15, MapType.Bossroom) {UnsupportedBossroom = true});
            MapList.Add(new MapData(MapNames.Carcass, 15, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.VaalTemple, 16, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.ForgeOfPhoenix, 16, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.LairOfHydra, 16, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.MazeOfMinotaur, 16, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.PitOfChimera, 16, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.VaultsOfAtziri, 3, MapType.Regular) {Ignored = true});
            MapList.Add(new MapData(MapNames.WhakawairuaTuahu, 6, MapType.Multilevel) {Ignored = true});
            MapList.Add(new MapData(MapNames.OlmecSanctum, 7, MapType.Complex) {Ignored = true});
            MapList.Add(new MapData(MapNames.MaelstromOfChaos, 8, MapType.Bossroom) {Ignored = true});
            MapList.Add(new MapData(MapNames.MaoKun, 9, MapType.Regular) {Ignored = true});
            MapList.Add(new MapData(MapNames.PoorjoyAsylum, 9, MapType.Regular) {Ignored = true});
            MapList.Add(new MapData(MapNames.PutridCloister, 9, MapType.Multilevel) {Ignored = true});
            MapList.Add(new MapData(MapNames.CaerBlaiddWolfpackDen, 10, MapType.Bossroom) {Ignored = true});
            MapList.Add(new MapData(MapNames.Beachhead, 15, MapType.Bossroom) {Ignored = true});
        }

        private void InitDict()
        {
            foreach (var data in MapList)
            {
                MapDict.Add(data.Name, data);
            }
        }

        private void Load()
        {
            if (!File.Exists(SettingsPath))
                return;

            var json = File.ReadAllText(SettingsPath);
            if (string.IsNullOrWhiteSpace(json))
            {
                GlobalLog.Error("[MapBot] Fail to load \"MapSettings.json\". File is empty.");
                return;
            }
            var parts = JsonConvert.DeserializeObject<Dictionary<string, EditablePart>>(json);
            if (parts == null)
            {
                GlobalLog.Error("[MapBot] Fail to load \"MapSettings.json\". Json deserealizer returned null.");
                return;
            }
            foreach (var data in MapList)
            {
                if (parts.TryGetValue(data.Name, out var part))
                {
                    data.Priority = part.Priority;
                    data.Ignored = part.Ignore;
                    data.IgnoredBossroom = part.IgnoreBossroom;
                    data.Sextant = part.Sextant;
                    data.ZanaMod = part.ZanaMod;
                    data.MobRemaining = part.MobRemaining;
                    data.StrictMobRemaining = part.StrictMobRemaining;
                    data.ExplorationPercent = part.ExplorationPercent;
                    data.StrictExplorationPercent = part.StrictExplorationPercent;
                    data.TrackMob = part.TrackMob;
                    data.FastTransition = part.FastTransition;
                }
            }
        }

        private void Save()
        {
            var parts = new Dictionary<string, EditablePart>(MapList.Count);

            foreach (var map in MapList)
            {
                var part = new EditablePart
                {
                    Priority = map.Priority,
                    Ignore = map.Ignored,
                    IgnoreBossroom = map.IgnoredBossroom,
                    Sextant = map.Sextant,
                    ZanaMod = map.ZanaMod,
                    MobRemaining = map.MobRemaining,
                    StrictMobRemaining = map.StrictMobRemaining,
                    ExplorationPercent = map.ExplorationPercent,
                    StrictExplorationPercent = map.StrictExplorationPercent,
                    TrackMob = map.TrackMob,
                    FastTransition = map.FastTransition
                };
                parts.Add(map.Name, part);
            }
            var json = JsonConvert.SerializeObject(parts, Formatting.Indented);
            File.WriteAllText(SettingsPath, json);
        }

        private class EditablePart
        {
            public int Priority;
            public bool Ignore;
            public bool IgnoreBossroom;
            public bool Sextant;
            public int ZanaMod;
            public int MobRemaining;
            public bool StrictMobRemaining;
            public int ExplorationPercent;
            public bool StrictExplorationPercent;
            public bool? TrackMob;
            public bool? FastTransition;
        }
    }
}