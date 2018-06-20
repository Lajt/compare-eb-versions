using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Loki.Bot;

namespace Default.Incursion
{
    public class HandleTempleTask : ITask
    {
        private const int MaxOmnitectAttempts = 100;

        public async Task<bool> Run()
        {
            if (!World.CurrentArea.IsTempleOfAtzoatl)
                return false;

            var settings = Settings.Instance;

            if (settings.SkipTemple)
            {
                await Leave();
                return true;
            }

            if (settings.TrackMobInTemple && await TrackMobLogic.Execute())
                return true;

            var explorer = CombatAreaCache.Current.Explorer;
            var isExplored = explorer.BasicExplorer.PercentComplete >= settings.ExplorationPercent;

            if (settings.IgnoreBossroom)
            {
                if (isExplored)
                {
                    GlobalLog.Warn($"[HandleTempleTask] Exploration limit has been reached ({settings.ExplorationPercent}%). Now leaving the temple.");
                    await Leave();
                    return true;
                }
            }
            else
            {
                if (isExplored && !explorer.Settings.FastTransition)
                {
                    GlobalLog.Warn($"[HandleTempleTask] Exploration limit has been reached ({settings.ExplorationPercent}%). Now seeking the bossroom.");
                    explorer.Settings.FastTransition = true;
                    return true;
                }

                var boss = Incursion.CachedOmnitect;
                if (boss != null && !boss.Ignored && !boss.Unwalkable)
                {
                    var pos = boss.Position;
                    if (pos.IsFar)
                    {
                        if (!pos.TryCome())
                        {
                            GlobalLog.Error("[HandleTempleTask] Fail to move to Vaal Omnitect. Now marking it as unwalkable.");
                            boss.Unwalkable = true;
                        }
                        return true;
                    }
                    var attempts = ++boss.InteractionAttempts;
                    if (attempts > MaxOmnitectAttempts)
                    {
                        GlobalLog.Error("[HandleTempleTask] Vaal Omnitect did not become active. Now ignoring it.");
                        boss.Ignored = true;
                        return true;
                    }
                    await Coroutines.FinishCurrentAction();
                    GlobalLog.Debug($"[HandleTempleTask] Waiting for Vaal Omnitect to become active ({attempts}/{MaxOmnitectAttempts})");
                    await Wait.StuckDetectionSleep(200);
                    return true;
                }
            }

            if (await explorer.Execute())
                return true;

            GlobalLog.Warn("[HandleTempleTask] Temple is fully explored.");
            await Leave();
            return true;
        }

        private static async Task Leave()
        {
            if (await PlayerAction.TpToTown(repeatUntilInTown: false))
                CombatAreaCache.Current.Storage["IsTempleCompleted"] = true;
        }

        #region Unused interface methods

        public void Tick()
        {
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public MessageResult Message(Message message)
        {
            return MessageResult.Unprocessed;
        }

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public string Name => "HandleTempleTask";
        public string Description => "Task that handles The Temple of Atzoatl.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}