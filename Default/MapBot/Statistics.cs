using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Default.EXtensions;
using JetBrains.Annotations;
using Loki.Common;
using Loki.Game;

namespace Default.MapBot
{
    public class Statistics : INotifyPropertyChanged
    {
        private static Statistics _instance;
        public static Statistics Instance => _instance ?? (_instance = new Statistics());

        private Statistics()
        {
            Events.AreaChanged += OnAreaChanged;
            LokiPoe.OnGuiTick += (sender, args) =>
            {
                OnPropertyChanged(nameof(TotalTimeSpent));
                OnPropertyChanged(nameof(CurrentTimeSpent));
            };
        }

        private int _totalEntered;
        private int _totalFinished;
        private int _totalFound;
        private int _averageTierEntered;
        private int _averageTierFound;
        private string _averageTimeSpent = "0";

        private readonly List<int> _mapTiersEntered = new List<int>();
        private readonly List<int> _mapTiersFound = new List<int>();
        private readonly List<int> _mapTimings = new List<int>();

        private readonly Stopwatch _uptimeTimer = Stopwatch.StartNew();
        private readonly Stopwatch _mapTimer = new Stopwatch();

        private readonly Interval _tickInterval = new Interval(500);
        private readonly HashSet<Vector2i> _inventoryMapPositions = new HashSet<Vector2i>();

        #region Properties

        public int TotalEntered
        {
            get => _totalEntered;
            set
            {
                if (value == _totalEntered) return;
                _totalEntered = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MapIncome));
            }
        }

        public int TotalFinished
        {
            get => _totalFinished;
            set
            {
                if (value == _totalFinished) return;
                _totalFinished = value;
                OnPropertyChanged();
            }
        }

        public int TotalFound
        {
            get => _totalFound;
            set
            {
                if (value == _totalFound) return;
                _totalFound = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MapIncome));
            }
        }

        public int AverageTierEntered
        {
            get => _averageTierEntered;
            set
            {
                if (value == _averageTierEntered) return;
                _averageTierEntered = value;
                OnPropertyChanged();
            }
        }

        public int AverageTierFound
        {
            get => _averageTierFound;
            set
            {
                if (value == _averageTierFound) return;
                _averageTierFound = value;
                OnPropertyChanged();
            }
        }

        public string AverageTimeSpent
        {
            get => _averageTimeSpent;
            set
            {
                if (value == _averageTimeSpent) return;
                _averageTimeSpent = value;
                OnPropertyChanged();
            }
        }

        public string MapIncome => (TotalFound - TotalEntered).ToString("+#;-#;0");
        public string TotalTimeSpent => _uptimeTimer.Elapsed.ToString("hh\\:mm\\:ss");
        public string CurrentTimeSpent => _mapTimer.Elapsed.ToString("hh\\:mm\\:ss");

        #endregion

        private void OnAreaChanged(object sender, AreaChangedArgs args)
        {
            var area = args.NewArea;
            if (area.IsMap)
            {
                _mapTimer.Start();
            }
            else
            {
                _mapTimer.Stop();
            }
            if (area.IsTown || area.IsHideoutArea)
            {
                GlobalLog.Info("[Statistics] Clearing inventory map positions.");
                _inventoryMapPositions.Clear();
            }
        }

        public void OnNewMapEnter()
        {
            ++TotalEntered;
            _mapTiersEntered.Add(World.CurrentArea.MonsterLevel - 67);
            AverageTierEntered = Round(_mapTiersEntered.Average());
            _mapTimer.Restart();
        }

        public void OnMapFinish()
        {
            ++TotalFinished;
            _mapTimer.Stop();
            _mapTimings.Add(Round(_mapTimer.Elapsed.TotalSeconds));
            AverageTimeSpent = TimeSpan.FromSeconds(_mapTimings.Average()).ToString("hh\\:mm\\:ss");
        }

        public void Tick()
        {
            if (!_tickInterval.Elapsed)
                return;

            if (!LokiPoe.IsInGame)
            {
                _mapTimer.Stop();
                return;
            }

            if (!World.CurrentArea.IsMap)
                return;

            var maps = Inventories.InventoryItems
                .Where(i => i.IsMap() && !_inventoryMapPositions.Contains(i.LocationTopLeft))
                .ToList();

            if (maps.Count == 0)
                return;

            foreach (var map in maps)
            {
                GlobalLog.Info($"[Statistics] Found \"{map.Name}\".");
                _inventoryMapPositions.Add(map.LocationTopLeft);
                _mapTiersFound.Add(map.MapTier);
            }

            TotalFound = _mapTiersFound.Count;
            AverageTierFound = Round(_mapTiersFound.Average());
        }

        private static int Round(double d)
        {
            return (int) Math.Round(d);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}