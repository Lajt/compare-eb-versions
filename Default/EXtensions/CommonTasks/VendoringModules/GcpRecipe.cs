using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions.CachedObjects;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.EXtensions.CommonTasks.VendoringModules
{
    internal class GcpRecipe : VendoringModule
    {
        private static string _tabWithGcpSet;
        private static GemSet _gcpSet;

        public override async Task Execute()
        {
            GlobalLog.Info("[VendorTask] Now going to sell quality skill gems for Gemcutter's Prism.");

            if (!await TakeGcpSets())
            {
                ReportError();
                return;
            }
            if (Settings.GcpMaxQ >= 20)
            {
                if (!await TakeQ20Gems())
                {
                    ReportError();
                    return;
                }
            }
            if (!await SellGems())
                ReportError();
        }

        public override void OnStashing(CachedItem item)
        {
            if (_tabWithGcpSet != null || item.Type.ItemType != ItemTypes.Gem || item.Quality < 1)
                return;

            var qualities = GemQualitiesInCurrentTab;

            if (qualities.Count == 0)
                return;

            var finder = new GemSetFinder(qualities);
            _gcpSet = finder.BestSet;

            if (_gcpSet != null)
            {
                GlobalLog.Info($"[OnQGemStash] Found gem set for gcp recipe {_gcpSet}");
                _tabWithGcpSet = LokiPoe.InGameState.StashUi.TabControl.CurrentTabName;
            }
            else
            {
                GlobalLog.Info("[OnQGemStash] Gem set for gcp recipe was not found.");
            }
        }

        public override void ResetData()
        {
            _tabWithGcpSet = null;
            _gcpSet = null;
        }

        public override bool Enabled => Settings.GcpEnabled;
        public override bool ShouldExecute => _tabWithGcpSet != null && !ErrorLimitReached;

        #region Subroutines

        private static async Task<bool> TakeGcpSets()
        {
            if (!await Inventories.OpenStashTab(_tabWithGcpSet))
                return false;

            while (true)
            {
                if (_gcpSet == null)
                {
                    var qualities = GemQualitiesInCurrentTab;
                    if (qualities.Count == 0)
                    {
                        GlobalLog.Info($"[TakeGcpSets] No quality gems were found in \"{_tabWithGcpSet}\" tab.");
                        _tabWithGcpSet = null;
                        return true;
                    }

                    var finder = new GemSetFinder(qualities);
                    _gcpSet = finder.BestSet;

                    if (_gcpSet == null)
                    {
                        GlobalLog.Info($"[TakeGcpSets] No more gem sets for gcp recipe were found in \"{_tabWithGcpSet}\" tab.");
                        _tabWithGcpSet = null;
                        return true;
                    }
                }

                if (!_gcpSet.CanFit)
                {
                    GlobalLog.Warn("[TakeGcpSets] Not enough inventory space for current gcp set.");
                    _gcpSet = null;
                    return true;
                }

                GlobalLog.Warn($"[TakeGcpSets] Now taking gcp set {_gcpSet}");

                foreach (int q in _gcpSet.Qualities)
                {
                    var gem = StashGems.FirstOrDefault(g => QGemFitsForSelling(g) && g.Quality == q);
                    if (gem == null)
                    {
                        GlobalLog.Error($"[TakeGcpSets] Unexpected error. Fail to find gem with quality {q} as a part of {_gcpSet} gcp set.");
                        _tabWithGcpSet = null;
                        _gcpSet = null;
                        return false;
                    }

                    if (!await Inventories.FastMoveFromStashTab(gem.LocationTopLeft))
                        return false;
                }
                _gcpSet = null;
            }
        }

        private static async Task<bool> TakeQ20Gems()
        {
            if (!LokiPoe.InGameState.StashUi.IsOpened)
            {
                GlobalLog.Error("[TakeQ20Gems] Unexpected error. Stash is closed.");
                return false;
            }

            while (true)
            {
                var gem = StashGems.FirstOrDefault(Q20GemFitsForSelling);

                if (gem == null)
                    return true;

                if (Inventories.AvailableInventorySquares == 0)
                {
                    GlobalLog.Warn("[TakeQ20Gems] Inventory is full.");
                    return true;
                }

                GlobalLog.Warn($"[TakeQ20Gems] Now taking \"{gem.Name}\".");

                if (!await Inventories.FastMoveFromStashTab(gem.LocationTopLeft))
                    return false;
            }
        }

        private static async Task<bool> SellGems()
        {
            var inventoryGems = Inventories.InventoryItems.Where(i => i.Components.SkillGemComponent != null).Select(i => i.LocationTopLeft).ToList();
            if (inventoryGems.Count == 0)
            {
                GlobalLog.Error("[SellGems] Unexpected error. Fail to find any skill gem in inventory.");
                return false;
            }
            return await TownNpcs.SellItems(inventoryGems);
        }

        #endregion

        #region Helper functions

        private static bool QGemFitsForSelling(Item gem)
        {
            var q = gem.Quality;
            return q > 0 && q <= Math.Min(19, Settings.GcpMaxQ) &&
                   gem.SkillGemLevel <= Settings.GcpMaxLvl && !IsRareGem(gem);
        }

        private static bool Q20GemFitsForSelling(Item gem)
        {
            var q = gem.Quality;
            return q >= 20 && q <= Settings.GcpMaxQ &&
                   gem.SkillGemLevel <= Settings.GcpMaxLvl && !IsRareGem(gem);
        }

        private static bool IsRareGem(Item gem)
        {
            var n = gem.Name;
            return n == "Enlighten Support" || n == "Empower Support" || n == "Enhance Support";
        }

        private static IEnumerable<Item> StashGems => Inventories.StashTabItems.Where(i => i.Components.SkillGemComponent != null);
        private static List<int> GemQualitiesInCurrentTab => StashGems.Where(QGemFitsForSelling).Select(g => g.Quality).ToList();

        #endregion

        #region Helper classes

        public class GemSet : IComparable<GemSet>
        {
            public readonly List<int> Qualities;
            public readonly int TotalQuality;

            public bool CanFit => Qualities.Count <= Inventories.AvailableInventorySquares;

            public GemSet(List<int> qualities, int totalQuality)
            {
                Qualities = qualities;
                TotalQuality = totalQuality;
            }

            public int CompareTo(GemSet other)
            {
                int diff = TotalQuality - other.TotalQuality;
                if (diff == 0) return Qualities.Count - other.Qualities.Count;
                return diff;
            }

            public override string ToString()
            {
                return $"({TotalQuality})-[{string.Join("+", Qualities)}]";
            }
        }

        public class GemSetFinder
        {
            private readonly List<int> _numbers;
            private readonly List<GemSet> _sets;
            private GemSet _perfectSet;

            public GemSet BestSet
            {
                get
                {
                    if (_perfectSet != null)
                        return _perfectSet;

                    if (_sets.Count > 0)
                    {
                        _sets.Sort();
                        return _sets[0];
                    }
                    return null;
                }
            }

            public GemSetFinder(List<int> numbers)
            {
                _numbers = numbers;
                _sets = new List<GemSet>();
                FindSets(new bool[numbers.Count], 0, 0);
            }

            // http://algorithms.tutorialhorizon.com/dynamic-programming-subset-sum-problem/
            private void FindSets(bool[] solution, int currentSum, int index)
            {
                if (currentSum == 40)
                {
                    _perfectSet = CreateGemSet(solution, currentSum);
                    return;
                }
                if (currentSum > 40)
                {
                    if (currentSum <= 45)
                    {
                        _sets.Add(CreateGemSet(solution, currentSum));
                    }
                    return;
                }

                if (_perfectSet != null || index == _numbers.Count)
                    return;

                solution[index] = true;
                currentSum += _numbers[index];
                FindSets(solution, currentSum, index + 1);

                solution[index] = false;
                currentSum -= _numbers[index];
                FindSets(solution, currentSum, index + 1);
            }

            private GemSet CreateGemSet(bool[] solution, int sum)
            {
                var list = new List<int>();
                for (int i = 0; i < solution.Length; ++i)
                {
                    if (solution[i])
                    {
                        list.Add(_numbers[i]);
                    }
                }
                return new GemSet(list, sum);
            }
        }

        #endregion
    }
}