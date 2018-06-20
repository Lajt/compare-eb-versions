using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Default.EXtensions.CachedObjects;
using Loki.Bot;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.EXtensions.Global
{
    public class CombatAreaCache
    {
        private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(15);
        private static readonly Interval ScanInterval = new Interval(200);
        private static readonly Interval ItemScanInterval = new Interval(25);

        private static readonly List<PickupEvalHolder> PickupEvaluators = new List<PickupEvalHolder>();

        private static readonly Dictionary<uint, CombatAreaCache> Caches = new Dictionary<uint, CombatAreaCache>();

        private CombatAreaCache _incursionSubCache;
        public static bool IsInIncursion { get; set; }

        public static CombatAreaCache Current
        {
            get
            {
                var hash = LokiPoe.LocalData.AreaHash;
                if (Caches.TryGetValue(hash, out var cache))
                {
                    cache._lastAccessTime.Restart();

                    if (IsInIncursion)
                        return cache._incursionSubCache ?? (cache._incursionSubCache = new CombatAreaCache(hash));

                    return cache;
                }
                RemoveOldCaches();
                var newCache = new CombatAreaCache(hash);
                Caches.Add(hash, newCache);
                return newCache;
            }
        }

        public uint Hash { get; }
        public DatWorldAreaWrapper WorldArea { get; }
        public ComplexExplorer Explorer { get; }
        public int DeathCount { get; internal set; }
        public int StuckCount { get; internal set; }

        public readonly List<CachedWorldItem> Items = new List<CachedWorldItem>();
        public readonly List<CachedObject> Chests = new List<CachedObject>();
        public readonly List<CachedObject> SpecialChests = new List<CachedObject>();
        public readonly List<CachedStrongbox> Strongboxes = new List<CachedStrongbox>();
        public readonly List<CachedObject> Shrines = new List<CachedObject>();
        public readonly List<CachedObject> Monsters = new List<CachedObject>();
        public readonly List<CachedTransition> AreaTransitions = new List<CachedTransition>();
        public readonly ObjectDictionary Storage = new ObjectDictionary();

        private readonly HashSet<int> _processedObjects = new HashSet<int>();

        //keep this in a separate collection to reset on ItemEvaluatorRefresh
        private readonly HashSet<int> _processedItems = new HashSet<int>();

        private readonly Stopwatch _lastAccessTime;

        static CombatAreaCache()
        {
            ItemEvaluator.OnRefreshed += OnItemEvaluatorRefresh;
            ComplexExplorer.LocalTransitionEntered += OnLocalTransitionEntered;
            BotManager.OnBotChanged += (sender, args) => Caches.Clear();
        }

        private CombatAreaCache(uint hash)
        {
            GlobalLog.Info($"[CombatAreaCache] Creating cache for \"{World.CurrentArea.Name}\" (hash: {hash})");
            Hash = hash;
            WorldArea = World.CurrentArea;
            Explorer = new ComplexExplorer();
            _lastAccessTime = Stopwatch.StartNew();
        }

        private static void RemoveOldCaches()
        {
            var toRemove = Caches.Where(c => c.Value._lastAccessTime.Elapsed > Lifetime).Select(c => c.Value).ToList();
            foreach (var cache in toRemove)
            {
                GlobalLog.Info($"[CombatAreaCache] Removing cache for \"{cache.WorldArea.Name}\" (hash: {cache.Hash}). Last accessed {(int) cache._lastAccessTime.Elapsed.TotalMinutes} minutes ago.");
                Caches.Remove(cache.Hash);
            }
        }

        public static bool AddPickupItemEvaluator(string id, Func<Item, bool> evaluator)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            if (evaluator == null)
                throw new ArgumentNullException(nameof(evaluator));

            if (PickupEvaluators.Exists(e => e.Id == id))
                return false;

            PickupEvaluators.Add(new PickupEvalHolder(id, evaluator));
            return true;
        }

        public static bool RemovePickupItemEvaluator(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var index = PickupEvaluators.FindIndex(e => e.Id == id);

            if (index < 0)
                return false;

            PickupEvaluators.RemoveAt(index);
            return true;
        }

        public static void Tick()
        {
            if (!LokiPoe.IsInGame || LokiPoe.Me.IsDead || !World.CurrentArea.IsCombatArea)
                return;

            Current.OnTick();
        }

        private void OnTick()
        {
            Current.Explorer.Tick();

            if (ItemScanInterval.Elapsed)
            {
                WorldItemScan();
            }

            if (ScanInterval.Elapsed)
            {
                foreach (var obj in LokiPoe.ObjectManager.Objects)
                {
                    var mob = obj as Monster;
                    if (mob != null)
                    {
                        ProcessMonster(mob);
                        continue;
                    }

                    var chest = obj as Chest;
                    if (chest != null)
                    {
                        if (IsSpecialChest(chest))
                        {
                            ProcessSpeacialChest(chest);
                            continue;
                        }
                        if (chest.IsStrongBox)
                        {
                            ProcessStrongbox(chest);
                            continue;
                        }
                        ProcessChest(chest);
                        continue;
                    }

                    var shrine = obj as Shrine;
                    if (shrine != null)
                    {
                        ProcessShrine(shrine);
                        continue;
                    }

                    var transition = obj as AreaTransition;
                    if (transition != null)
                    {
                        ProcessTransition(transition);
                    }
                }
                UpdateMonsters();
            }
        }

        private void WorldItemScan()
        {
            foreach (var obj in LokiPoe.ObjectManager.Objects)
            {
                var worldItem = obj as WorldItem;

                if (worldItem == null)
                    continue;

                if (worldItem.IsAllocatedToOther && DateTime.Now < worldItem.PublicTime)
                    continue;

                var id = worldItem.Id;

                if (_processedItems.Contains(id))
                    continue;

                var item = worldItem.Item;

                if ((Settings.Instance.LootVisibleItems && worldItem.HasVisibleHighlightLabel) ||
                    ItemEvaluator.Match(item, EvaluationType.PickUp) ||
                    PickupEvaluators.Exists(e => e.Eval(item)))
                {
                    var pos = worldItem.WalkablePosition();
                    pos.Initialized = true; //disable walkable position searching for items
                    Items.Add(new CachedWorldItem(id, pos, item.Size, item.Rarity));
                }
                _processedItems.Add(id);
            }
        }

        private void ProcessMonster(Monster m)
        {
            if (m.IsDead || m.CannotDie || m.Reaction != Reaction.Enemy)
                return;

            if ((!m.IsTargetable || m.GetStat(StatTypeGGG.CannotBeDamaged) == 1) && !IsEmerging(m))
                return;

            if (!IsInIncursion && m.ExplicitAffixes.Any(a => a.InternalName.StartsWith("MonsterIncursion")))
                return;

            var id = m.Id;
            if (_processedObjects.Contains(id))
                return;

            if (HasImmunityAura(m) || SkipThisMob(m))
            {
                _processedObjects.Add(id);
                return;
            }

            var pos = m.WalkablePosition();
            pos.Initialized = true; //disable walkable position searching for monsters
            Monsters.Add(new CachedObject(id, pos));
            _processedObjects.Add(id);
        }

        private void ProcessChest(Chest c)
        {
            if (c.IsOpened || c.IsLocked || c.OpensOnDamage || !c.IsTargetable)
                return;

            var id = c.Id;
            if (_processedObjects.Contains(id))
                return;

            Chests.Add(new CachedObject(id, c.WalkablePosition(5, 20)));
            _processedObjects.Add(id);
        }

        private void ProcessSpeacialChest(Chest c)
        {
            if (c.IsOpened || !c.IsTargetable)
                return;

            // Perandus chests are always locked for some reason
            if (c.IsLocked && !c.Metadata.Contains("/PerandusChests/"))
                return;

            var id = c.Id;
            if (_processedObjects.Contains(id))
                return;

            var pos = c.WalkablePosition();
            SpecialChests.Add(new CachedObject(id, pos));
            _processedObjects.Add(id);
            GlobalLog.Warn($"[CombatAreaCache] Registering {pos}");
        }

        private void ProcessStrongbox(Chest box)
        {
            if (box.IsOpened || box.IsLocked || !box.IsTargetable)
                return;

            var id = box.Id;
            if (_processedObjects.Contains(id))
                return;

            var pos = box.WalkablePosition();
            Strongboxes.Add(new CachedStrongbox(id, pos, box.Rarity));
            _processedObjects.Add(id);
            GlobalLog.Warn($"[CombatAreaCache] Registering {pos}");
        }

        private void ProcessShrine(Shrine s)
        {
            if (s.IsDeactivated || !s.IsTargetable)
                return;

            var id = s.Id;
            if (_processedObjects.Contains(id))
                return;

            var pos = s.WalkablePosition();
            Shrines.Add(new CachedObject(id, pos));
            _processedObjects.Add(id);
            GlobalLog.Warn($"[CombatAreaCache] Registering {pos}");
        }

        private void ProcessTransition(AreaTransition t)
        {
            var id = t.Id;
            if (_processedObjects.Contains(id))
                return;

            if (SkipThisTransition(t))
            {
                _processedObjects.Add(id);
                return;
            }

            TransitionType type;

            if (t.Metadata.Contains("LabyrinthTrial"))
            {
                type = TransitionType.Trial;
            }
            else if (t.Metadata.Contains("IncursionPortal"))
            {
                type = TransitionType.Incursion;
            }
            else if (t.ExplicitAffixes.Any(a => a.Category == "MapMissionMods"))
            {
                type = TransitionType.Master;
            }
            else if (t.ExplicitAffixes.Any(a => a.InternalName.Contains("CorruptedSideArea")))
            {
                type = TransitionType.Vaal;
            }
            else if (t.TransitionType == TransitionTypes.Local)
            {
                type = TransitionType.Local;
            }
            else
            {
                type = TransitionType.Regular;
            }

            var pos = t.WalkablePosition(10, 20);
            var dest = t.Destination ?? Dat.LookupWorldArea(1);
            var cachedTrans = new CachedTransition(id, pos, type, dest);
            AreaTransitions.Add(cachedTrans);
            _processedObjects.Add(id);
            GlobalLog.Debug($"[CombatAreaCache] Registering {pos} (Type: {type})");
            TweakTransition(cachedTrans);
        }

        private void UpdateMonsters()
        {
            var toRemove = new List<CachedObject>();
            foreach (var cachedMob in Monsters)
            {
                var m = cachedMob.Object as Monster;
                if (m != null)
                {
                    if (m.IsDead || m.CannotDie || m.Reaction != Reaction.Enemy || HasImmunityAura(m))
                    {
                        toRemove.Add(cachedMob);
                        if (cachedMob == TrackMobLogic.CurrentTarget)
                            TrackMobLogic.CurrentTarget = null;
                    }
                    else
                    {
                        var pos = m.WalkablePosition();
                        pos.Initialized = true;
                        cachedMob.Position = pos;
                    }
                }
                else
                {
                    //remove monsters that were close to us, but now are null (corpse exploded, shattered etc)
                    //optimal distance is debatable, but its not recommended to be higher than 100
                    if (cachedMob.Position.Distance <= 80)
                    {
                        toRemove.Add(cachedMob);
                        if (cachedMob == TrackMobLogic.CurrentTarget)
                            TrackMobLogic.CurrentTarget = null;
                    }
                }
            }
            foreach (var m in toRemove)
            {
                Monsters.Remove(m);
            }
        }

        private static bool HasImmunityAura(Monster mob)
        {
            foreach (var aura in mob.Auras)
            {
                var name = aura.InternalName;
                if (name == "cannot_be_damaged" ||
                    name == "bloodlines_necrovigil" ||
                    name == "god_mode" ||
                    name == "shrine_godmode")
                    return true;
            }
            return false;
        }

        private static bool SkipThisMob(Monster mob)
        {
            var m = mob.Metadata;
            return m == "Metadata/Monsters/LeagueIncursion/VaalSaucerBoss" ||
                   m.Contains("DoedreStonePillar");
        }

        private static bool IsEmerging(Monster mob)
        {
            if (mob.GetStat(StatTypeGGG.IsHiddenMonster) != 1)
                return false;

            var m = mob.Metadata;
            return m.Contains("/SandSpitterEmerge/") ||
                   m.Contains("/WaterElemental/") ||
                   m.Contains("/RootSpiders/") ||
                   m.Contains("ZombieMiredGraspEmerge") ||
                   m.Contains("ReliquaryMonsterEmerge");
        }


        private bool SkipThisTransition(AreaTransition t)
        {
            var name = t.Name;

            if (name == "Area Transition" && t.Destination == null)
            {
                GlobalLog.Debug($"[CombatAreaCache] Skipping dummy area transition (id: {t.Id})");
                return true;
            }

            if (t.TransitionType == TransitionTypes.Local && !t.Metadata.Contains("IncursionPortal"))
            {
                if (WorldArea.Name == MapNames.Caldera && name != "Caldera of The King")
                {
                    GlobalLog.Debug($"[CombatAreaCache] Skipping \"{name}\" area transition because it leads to the same level.");
                    return true;
                }
                if (WorldArea.Id == World.Act9.RottingCore.Id)
                {
                    var metadata = t.Metadata;
                    if (metadata == "Metadata/QuestObjects/Act9/HarvestFinalBossTransition")
                    {
                        GlobalLog.Debug($"[CombatAreaCache] Skipping \"{name}\" area transition because it is unlocked by a quest.");
                        return true;
                    }
                    if (metadata.Contains("BellyArenaTransition"))
                    {
                        GlobalLog.Debug($"[CombatAreaCache] Skipping \"{name}\" area transition because it is not a pathfinding obstacle.");
                        return true;
                    }
                }
            }
            return false;
        }

        private void TweakTransition(CachedTransition t)
        {
            var name = t.Position.Name;
            var areaName = WorldArea.Name;
            if (areaName == MapNames.Villa && (name == MapNames.Villa || name == "Arena"))
            {
                GlobalLog.Debug("[CombatAreaCache] Marking this area transition as unwalkable (Villa tweak)");
                t.Unwalkable = true;
                return;
            }
            if (areaName == MapNames.Summit && name == MapNames.Summit)
            {
                GlobalLog.Debug("[CombatAreaCache] Marking this area transition as back transition (Summit tweak)");
                t.LeadsBack = true;
            }
        }

        private static bool IsSpecialChest(Chest chest)
        {
            var m = chest.Metadata;

            if (SpecialChestMetadada.Contains(m))
                return true;

            if (m.Contains("/Breach/"))
                return true;

            if (m.Contains("/PerandusChests/"))
                return true;

            if (m.Contains("IncursionChest"))
                return true;

            return false;
        }

        private static readonly HashSet<string> SpecialChestMetadada = new HashSet<string>
        {
            "Metadata/Chests/BootyChest",
            "Metadata/Chests/NotSoBootyChest",
            "Metadata/Chests/VaultTreasurePile",
            "Metadata/Chests/GhostPirateBootyChest",
            "Metadata/Chests/StatueMakersTools",
            "Metadata/Chests/StrongBoxes/VaultsOfAtziriUniqueChest",
            "Metadata/Chests/CopperChestEpic3",
            "Metadata/Chests/TutorialSupportGemChest"
        };

        private static void OnLocalTransitionEntered()
        {
            GlobalLog.Info("[CombatAreaCache] Resetting unwalkable flags on all cached objects.");

            var cache = Current;

            foreach (var item in cache.Items)
            {
                item.Unwalkable = false;
            }
            foreach (var monster in cache.Monsters)
            {
                monster.Unwalkable = false;
            }
            foreach (var chest in cache.Chests)
            {
                chest.Unwalkable = false;
            }
            foreach (var specialChest in cache.SpecialChests)
            {
                specialChest.Unwalkable = false;
            }
            foreach (var strongbox in cache.Strongboxes)
            {
                strongbox.Unwalkable = false;
            }
            foreach (var shrine in cache.Shrines)
            {
                shrine.Unwalkable = false;
            }
            foreach (var transition in cache.AreaTransitions)
            {
                transition.Unwalkable = false;
            }
        }

        private static void OnItemEvaluatorRefresh(object sender, ItemEvaluatorRefreshedEventArgs args)
        {
            if (Caches.TryGetValue(LokiPoe.LocalData.AreaHash, out var cache))
            {
                GlobalLog.Info("[CombatAreaCache] Clearing processed items.");
                cache._processedItems.Clear();
            }
        }

        public class ObjectDictionary
        {
            private readonly Dictionary<string, object> _dict = new Dictionary<string, object>();

            public object this[string key]
            {
                get
                {
                    _dict.TryGetValue(key, out object obj);
                    return obj;
                }
                set
                {
                    if (!_dict.ContainsKey(key))
                    {
                        GlobalLog.Debug($"[Storage] Registering [{key}] = [{value ?? "null"}]");
                        _dict.Add(key, value);
                    }
                    else
                    {
                        _dict[key] = value;
                    }
                }
            }

            public bool Contains(string key)
            {
                return _dict.ContainsKey(key);
            }
        }

        private class PickupEvalHolder
        {
            public readonly string Id;
            public readonly Func<Item, bool> Eval;

            public PickupEvalHolder(string id, Func<Item, bool> eval)
            {
                Id = id;
                Eval = eval;
            }
        }
    }
}