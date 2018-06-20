using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CommonTasks;
using Default.EXtensions.Global;
using Loki.Bot;
using Loki.Game;

namespace Default.QuestBot
{
    public static class GrindingHandler
    {
        public const string Name = "Grinding";
        public const int MaxPulses = 3;

        public static bool ForceFinish;

        private static Settings.GrindingRule _grindingRule;
        private static ShuffledBag<AreaInfo> _areaSequence;
        private static AreaInfo _area;

        private static int GrindingPulses
        {
            get => (int?) CombatAreaCache.Current.Storage["GrindingPulses"] ?? 0;
            set => CombatAreaCache.Current.Storage["GrindingPulses"] = value;
        }

        public static async Task<bool> Execute()
        {
            if (World.CurrentArea.IsTown)
            {
                if (LokiPoe.Me.Level >= _grindingRule.LevelCap)
                {
                    GlobalLog.Warn($"[{Name}] Level cap has been reached for \"{_grindingRule.Quest.Name}\" ({_grindingRule.LevelCap})");
                    _grindingRule = null;
                    return false;
                }
            }
            if (_area.IsCurrentArea)
            {
                if (ForceFinish)
                {
                    if (await FinishExploration())
                        return true;

                    ForceFinish = false;
                }

                var settings = Settings.Instance;

                if (settings.TrackMob && await TrackMobLogic.Execute())
                    return true;

                var explorer = CombatAreaCache.Current.Explorer;
                if (explorer.BasicExplorer.PercentComplete >= settings.ExplorationPercent || !await explorer.Execute())
                {
                    // Finish off surrounding monsters before leaving
                    if (await TrackMobLogic.Execute(80))
                        return true;

                    if (await FinishExploration())
                        return true;
                }
                return true;
            }
            Travel.RequestNewInstance(_area);
            await Travel.To(_area);
            return true;
        }

        private static async Task<bool> FinishExploration()
        {
            if (GrindingPulses < MaxPulses)
            {
                await Coroutines.FinishCurrentAction();

                ++GrindingPulses;
                GlobalLog.Info($"[{Name}] Final pulse {GrindingPulses}/{MaxPulses}");
                await Wait.SleepSafe(500);
                return true;
            }
            if (!await PlayerAction.TpToTown())
            {
                ErrorManager.ReportError();
                return true;
            }
            _area = _areaSequence.NextItem;
            QuestManager.UpdateGuiAndLog(Name, _area.Name);
            return false;
        }

        internal static void OnPlayerDied(int deathCount)
        {
            var settings = Settings.Instance;

            if (settings.MaxDeaths <= 0 || LeaveAreaTask.IsActive || settings.CurrentQuestName != Name)
                return;

            if (deathCount >= settings.MaxDeaths)
            {
                GlobalLog.Error($"[{Name}] Too many deaths in current area ({World.CurrentArea.Name}). Now leaving it.");
                LeaveAreaTask.IsActive = true;
            }
        }

        internal static void SetGrindingRule(Settings.GrindingRule rule)
        {
            if (_grindingRule == rule)
            {
                _areaSequence = CreateAreaSequence();
                if (_grindingRule.Areas.All(a => a.Area.Id != _area.Id))
                {
                    _area = _areaSequence.NextItem;
                }
            }
            else
            {
                _grindingRule = rule;
                _areaSequence = CreateAreaSequence();
                _area = _areaSequence.NextItem;
            }
            QuestManager.UpdateGuiAndLog(Name, _area.Name);
        }

        private static ShuffledBag<AreaInfo> CreateAreaSequence()
        {
            var areas = _grindingRule.Areas;
            var list = new List<AreaInfo>();
            foreach (var area in areas)
            {
                for (int i = 0; i < area.Pool; ++i)
                {
                    list.Add(new AreaInfo(area.Area.Id, area.Area.Name));
                }
            }
            return new ShuffledBag<AreaInfo>(list);
        }

        public class ShuffledBag<T>
        {
            private readonly T[] _items;
            private int _index;

            public ShuffledBag(IEnumerable<T> collection)
            {
                _items = collection.ToArray();
                Shuffle(_items);
            }

            public T NextItem
            {
                get
                {
                    if (_index == _items.Length)
                    {
                        Shuffle(_items);
                        _index = 0;
                    }
                    var item = _items[_index];
                    ++_index;
                    return item;
                }
            }

            // https://stackoverflow.com/questions/273313/randomize-a-listt/1262619#1262619
            private static void Shuffle(T[] array)
            {
                int n = array.Length;
                while (n > 1)
                {
                    n--;
                    int k = LokiPoe.Random.Next(n + 1);
                    var value = array[k];
                    array[k] = array[n];
                    array[n] = value;
                }
            }
        }
    }
}