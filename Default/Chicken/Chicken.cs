using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Default.EXtensions;
using Default.EXtensions.CommonTasks;
using Default.EXtensions.Global;
using log4net;
using Loki.Bot;
using Loki.Common;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;
using static Default.EXtensions.EXtensions;
using ChickenSettings = Default.Chicken.Settings;

namespace Default.Chicken
{
    public class Chicken : IPlugin, ITickEvents, IUrlProvider
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();
        private Gui _gui;

        private static readonly Interval MobScanInterval = new Interval(500);

        public void Tick()
        {
            if (!LokiPoe.IsInGame || !World.CurrentArea.IsCombatArea)
                return;

            var me = LokiPoe.Me;

            if (me.IsDead)
                return;

            var settings = ChickenSettings.Instance;

            if (settings.HpEnabled)
            {
                var hpPercent = me.HealthPercent;
                var hpThreshold = settings.HpThreshold;
                if (hpPercent <= hpThreshold)
                {
                    Log.Warn($"[Chicken] Now chickening because our HP ({hpPercent}%) is below threshold ({hpThreshold}%)");
                    Logout();
                    return;
                }
            }
            if (settings.EsEnabled)
            {
                var esPercent = me.EnergyShieldPercent;
                var esThreshold = settings.EsThreshold;
                if (esPercent <= esThreshold)
                {
                    Log.Warn($"[Chicken] Now chickening because our ES ({esPercent}%) is below threshold ({esThreshold}%)");
                    Logout();
                    return;
                }
            }
            if (settings.OnSightEnabled && !LeaveAreaTask.IsActive && MobScanInterval.Elapsed)
            {
                foreach (var obj in LokiPoe.ObjectManager.Objects)
                {
                    var mob = obj as Monster;
                    if (mob == null || mob.Rarity != Rarity.Unique || mob.IsDead)
                        continue;

                    var name = mob.Name;

                    var mobEntry = settings.Monsters.FirstOrDefault(m => m.Name == name);

                    if (mobEntry == null)
                        continue;

                    var distance = mob.Distance;

                    if (distance > mobEntry.Range)
                        continue;

                    var id = mob.Id;

                    if (mobEntry.Action == OnSightAction.Chicken)
                    {
                        Log.Warn($"[Chicken] Detected \"{name}\" at a distance of {distance}. Now leaving this area.");
                        AbandonCurrentArea();
                        LeaveAreaTask.IsActive = true;
                        return;
                    }
                    if (!Blacklist.Contains(id))
                    {
                        Log.Warn($"[Chicken] Detected \"{name}\" at a distance of {distance}. Now ignoring it.");
                        Ignore(id);
                    }
                }
            }
        }

        private static void Logout()
        {
            var err = LokiPoe.EscapeState.LogoutToTitleScreen();
            if (err != LokiPoe.EscapeState.LogoutError.None)
            {
                Log.Error($"[Chicken] Fail to logout. Error: \"{err}\".");
            }
        }

        private static void Ignore(int id)
        {
            Blacklist.Add(id, TimeSpan.MaxValue, "Chicken on sight");
            var cached = CombatAreaCache.Current.Monsters.Find(m => m.Id == id);
            if (cached != null) cached.Ignored = true;
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

        public void Initialize()
        {
        }

        public void Deinitialize()
        {
        }

        public void Enable()
        {
        }

        public void Disable()
        {
        }

        public string Name => "Chicken";
        public string Description => "Plugin that provides chicken functionality.";
        public string Author => "ExVault";
        public string Version => "1.0";
        public JsonSettings Settings => ChickenSettings.Instance;
        public UserControl Control => _gui ?? (_gui = new Gui());
        public string Url => "https://www.thebuddyforum.com/threads/chicken-settings-description.416050/";

        #endregion
    }
}