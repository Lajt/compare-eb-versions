using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Bot;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.MapBot
{
    public class KillBossTask : ITask
    {
        private const int MaxKillAttempts = 100;

        private static readonly Interval LogInterval = new Interval(1000);
        private static readonly Interval TickInterval = new Interval(200);

        public static bool BossKilled { get; private set; }

        private static readonly List<CachedBoss> CachedBosses = new List<CachedBoss>();

        private static CachedBoss _currentTarget;
        private static int _bossesKilled;

        private static bool _multiPhaseBoss;
        private static bool _teleportingBoss;
        private static string _priorityBossName;
        private static int _bossRange;

        private static Func<Monster, bool> _isMapBoss = DefaultBossSelector;

        public async Task<bool> Run()
        {
            if (BossKilled || MapExplorationTask.MapCompleted)
                return false;

            var area = World.CurrentArea;

            if (!area.IsMap)
                return false;

            if (_currentTarget == null)
            {
                if ((_currentTarget = CachedBosses.ClosestValid(b => !b.IsDead)) == null)
                    return false;
            }

            if (Blacklist.Contains(_currentTarget.Id))
            {
                GlobalLog.Warn("[KillBossTask] Boss is in global blacklist. Now marking it as killed.");
                _currentTarget.IsDead = true;
                _currentTarget = null;
                RegisterDeath();
                return true;
            }

            if (_priorityBossName != null && _currentTarget.Position.Name != _priorityBossName)
            {
                var priorityBoss = CachedBosses.ClosestValid(b => !b.IsDead && b.Position.Name == _priorityBossName);
                if (priorityBoss != null)
                {
                    GlobalLog.Debug($"[KillBossTask] Switching current target to \"{priorityBoss}\".");
                    _currentTarget = priorityBoss;
                    return true;
                }
            }

            if (_currentTarget.IsDead)
            {
                var newBoss = CachedBosses.Valid().FirstOrDefault(b => !b.IsDead);
                if (newBoss != null) _currentTarget = newBoss;
            }

            var pos = _currentTarget.Position;
            if (pos.Distance <= 50 && pos.PathDistance <= 55)
            {
                var bossObj = _currentTarget.Object as Monster;
                if (bossObj == null)
                {
                    if (_teleportingBoss)
                    {
                        CachedBosses.Remove(_currentTarget);
                        _currentTarget = null;
                        return true;
                    }
                    GlobalLog.Debug("[KillBossTask] We are close to last know position of map boss, but boss object does not exist anymore.");
                    GlobalLog.Debug("[KillBossTask] Most likely this boss does not spawn a corpse or was shattered/exploded.");
                    _currentTarget.IsDead = true;
                    _currentTarget = null;
                    RegisterDeath();
                    return true;
                }
            }

            if (pos.Distance > _bossRange)
            {
                if (LogInterval.Elapsed)
                {
                    GlobalLog.Debug($"[KillBossTask] Going to {pos}");
                }
                if (!pos.TryCome())
                {
                    GlobalLog.Error(MapData.Current.Type == MapType.Regular
                        ? $"[KillBossTask] Unexpected error. Fail to move to map boss ({pos.Name}) in a regular map."
                        : $"[KillBossTask] Fail to move to the map boss \"{pos.Name}\". Will try again after area transition.");
                    _currentTarget.Unwalkable = true;
                    _currentTarget = null;
                }
                return true;
            }

            var attempts = ++_currentTarget.InteractionAttempts;

            // Helps to trigger Gorge and Underground River bosses
            if (attempts == MaxKillAttempts / 4)
            {
                GlobalLog.Debug("[KillBossTask] Trying to move around to trigger a boss.");
                var distantPos = WorldPosition.FindPathablePositionAtDistance(40, 70, 5);
                if (distantPos != null)
                {
                    await Move.AtOnce(distantPos, "distant position", 10);
                }
            }
            if (attempts > MaxKillAttempts)
            {
                GlobalLog.Error("[KillBossTask] Boss did not become active. Now ignoring it.");
                _currentTarget.Ignored = true;
                _currentTarget = null;
                RegisterDeath();
                return true;
            }
            await Coroutines.FinishCurrentAction();
            GlobalLog.Debug($"[KillBossTask] Waiting for map boss to become active ({attempts}/{MaxKillAttempts})");
            await Wait.StuckDetectionSleep(200);
            return true;
        }

        public void Tick()
        {
            if (BossKilled || MapExplorationTask.MapCompleted)
                return;

            if (!TickInterval.Elapsed)
                return;

            if (!LokiPoe.IsInGame || !World.CurrentArea.IsMap)
                return;

            foreach (var obj in LokiPoe.ObjectManager.Objects)
            {
                var mob = obj as Monster;

                if (mob == null || !_isMapBoss(mob))
                    continue;

                var id = mob.Id;
                var cached = CachedBosses.Find(b => b.Id == id);

                if (!mob.IsDead)
                {
                    var pos = mob.WalkablePosition(5, 20);
                    if (cached != null)
                    {
                        cached.Position = pos;
                    }
                    else
                    {
                        CachedBosses.Add(new CachedBoss(id, pos, false));
                        GlobalLog.Warn($"[KillBossTask] Registering {pos}");
                    }
                }
                else
                {
                    if (cached == null)
                    {
                        GlobalLog.Warn($"[KillBossTask] Registering dead map boss \"{mob.Name}\".");
                        CachedBosses.Add(new CachedBoss(id, mob.WalkablePosition(), true));
                        RegisterDeath();
                    }
                    else if (!cached.IsDead)
                    {
                        GlobalLog.Warn($"[KillBossTask] Registering death of \"{mob.Name}\".");
                        cached.IsDead = true;
                        if (!_multiPhaseBoss) _currentTarget = null;
                        RegisterDeath();
                    }
                }
            }
        }

        private static void RegisterDeath()
        {
            ++_bossesKilled;
            var total = BossAmountForMap;
            GlobalLog.Warn($"[KillBossTask] Bosses killed: {_bossesKilled} out of {total}.");
            if (_bossesKilled >= BossAmountForMap) BossKilled = true;
        }

        private static int BossAmountForMap
        {
            get
            {
                int bosses;
                if (!BossesPerMap.TryGetValue(World.CurrentArea.Name, out bosses))
                {
                    bosses = 1;
                }
                LokiPoe.LocalData.MapMods.TryGetValue(StatTypeGGG.MapSpawnTwoBosses, out int twoBossesFlag);
                if (twoBossesFlag == 1)
                {
                    return bosses * 2;
                }
                return Math.Max(bosses, CachedBosses.Count);
            }
        }

        private static readonly Dictionary<string, int> BossesPerMap = new Dictionary<string, int>
        {
            [MapNames.Arcade] = 2,
            [MapNames.CrystalOre] = 3,
            [MapNames.VaalPyramid] = 3,
            [MapNames.Canyon] = 2,
            [MapNames.Racecourse] = 3,
            [MapNames.Strand] = 2,
            [MapNames.Arcade] = 2,
            [MapNames.Arena] = 3,
            [MapNames.TropicalIsland] = 3,
            [MapNames.Coves] = 2,
            [MapNames.Promenade] = 2,
            [MapNames.Courtyard] = 3,
            [MapNames.Port] = 5,
            [MapNames.Excavation] = 2,
            [MapNames.Plateau] = 2,
            [MapNames.OvergrownRuin] = 2,
            [MapNames.MineralPools] = 2,
            [MapNames.Palace] = 2,
            [MapNames.Core] = 4,
            [MapNames.CitySquare] = 3,
            [MapNames.Courthouse] = 3,
            [MapNames.Graveyard] = 3,
            [MapNames.Basilica] = 2,

            [MapNames.MaelstromOfChaos] = 2,
            [MapNames.WhakawairuaTuahu] = 2,
            [MapNames.OlmecSanctum] = 5,
        };

        public MessageResult Message(Message message)
        {
            var id = message.Id;
            if (id == MapBot.Messages.NewMapEntered)
            {
                GlobalLog.Info("[KillBossTask] Reset.");
                var areaName = message.GetInput<string>();
                _bossesKilled = 0;
                _currentTarget = null;
                BossKilled = false;
                _multiPhaseBoss = false;
                _teleportingBoss = false;
                CachedBosses.Clear();
                SetBossSelector(areaName);
                SetPriorityBossName(areaName);
                SetBossRange(areaName);

                if (areaName == MapNames.VaultsOfAtziri)
                {
                    BossKilled = true;
                    GlobalLog.Info($"[KillBossTask] BossKilled is set to true ({areaName})");
                    return MessageResult.Processed;
                }

                if (areaName == MapNames.MineralPools ||
                    areaName == MapNames.Palace ||
                    areaName == MapNames.Basilica ||
                    areaName == MapNames.MaelstromOfChaos)
                {
                    _multiPhaseBoss = true;
                    GlobalLog.Info($"[KillBossTask] MultiPhaseBoss is set to true ({areaName})");
                    return MessageResult.Processed;
                }

                if (areaName == MapNames.Pen ||
                    areaName == MapNames.Pier ||
                    areaName == MapNames.Shrine ||
                    areaName == MapNames.DesertSpring ||
                    areaName == MapNames.Summit ||
                    areaName == MapNames.DarkForest ||
                    areaName == MapNames.PutridCloister)
                {
                    _teleportingBoss = true;
                    GlobalLog.Info($"[KillBossTask] TeleportingBoss is set to true ({areaName})");
                    return MessageResult.Processed;
                }
                return MessageResult.Processed;
            }
            if (id == ComplexExplorer.LocalTransitionEnteredMessage)
            {
                GlobalLog.Info("[KillBossTask] Resetting unwalkable flags.");
                foreach (var cachedBoss in CachedBosses)
                {
                    cachedBoss.Unwalkable = false;
                }
                return MessageResult.Processed;
            }
            return MessageResult.Unprocessed;
        }

        private static void SetPriorityBossName(string areaName)
        {
            if (areaName == MapNames.Racecourse)
            {
                _priorityBossName = "Bringer of Blood";
                GlobalLog.Info("[KillBossTask] Priority boss name is set to \"Bringer of Blood\".");
            }
            else if (areaName == MapNames.InfestedValley)
            {
                _priorityBossName = "Gorulis' Nest";
                GlobalLog.Info("[KillBossTask] Priority boss name is set to \"Gorulis' Nest\".");
            }
            else if (areaName == MapNames.Siege)
            {
                _priorityBossName = "Tukohama's Protection";
                GlobalLog.Info("[KillBossTask] Priority boss name is set to \"Tukohama's Protection\".");
            }
            else
            {
                _priorityBossName = null;
            }
        }

        private static void SetBossRange(string areaName)
        {
            if (areaName == MapNames.Tower)
            {
                _bossRange = 35;
            }
            else if (areaName == MapNames.Fields)
            {
                _bossRange = 30;
            }
            else if (areaName == MapNames.Gorge ||
                     areaName == MapNames.UndergroundRiver)
            {
                _bossRange = 7;
            }
            else
            {
                _bossRange = 15;
            }
            GlobalLog.Info($"[KillBossTask] Boss range: {_bossRange}");
        }

        private static void SetBossSelector(string areaName)
        {
            if (areaName == MapNames.Precinct)
            {
                GlobalLog.Info("[KillBossTask] This map has a group of Rogue Exiles as map bosses.");
                _isMapBoss = m => m.Rarity == Rarity.Unique && m.Metadata.Contains("MapBoss");
                return;
            }
            if (areaName == MapNames.Courthouse)
            {
                GlobalLog.Info("[KillBossTask] This map has a group of dark Rogue Exiles as map bosses.");
                _isMapBoss = m => m.Rarity == Rarity.Unique && m.Metadata.EndsWith("Kitava") && m.Metadata.Contains("/Exiles/");
                return;
            }
            if (areaName == MapNames.InfestedValley)
            {
                GlobalLog.Info("[KillBossTask] Nests have to be killed to activate boss on this map.");
                _isMapBoss = m => m.IsMapBoss || m.Name == "Gorulis' Nest";
                return;
            }
            if (areaName == MapNames.Siege)
            {
                GlobalLog.Info("[KillBossTask] Totems must be killed to remove boss immunity on this map.");
                _isMapBoss = m => m.IsMapBoss || m.Name == "Tukohama's Protection";
                return;
            }
            if (areaName == MapNames.WhakawairuaTuahu)
            {
                GlobalLog.Info("[KillBossTask] This map has Shade of a player as one of the map bosses.");
                _isMapBoss = m => m.IsMapBoss || (m.Rarity == Rarity.Unique && m.Metadata.Contains("DarkExile"));
                return;
            }

            if (areaName == MapNames.Shipyard ||
                areaName == MapNames.Lighthouse ||
                areaName == MapNames.Iceberg ||
                areaName == MapNames.AcidLakes)
            {
                GlobalLog.Info("[KillBossTask] This map has a Warband leader as a map boss.");
                _isMapBoss = m => m.Rarity == Rarity.Unique && m.ExplicitAffixes.Any(a => a.InternalName == "MonsterWbLeader");
                return;
            }

            _isMapBoss = DefaultBossSelector;
        }

        private static bool DefaultBossSelector(Monster m)
        {
            return m.IsMapBoss;
        }

        private class CachedBoss : CachedObject
        {
            public bool IsDead { get; set; }

            public CachedBoss(int id, WalkablePosition position, bool isDead) : base(id, position)
            {
                IsDead = isDead;
            }
        }

        #region Unused interface methods

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public string Name => "KillBossTask";
        public string Description => "Task for killing map boss.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}