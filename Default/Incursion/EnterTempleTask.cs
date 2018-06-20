using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Loki.Bot;
using Loki.Game;
using Loki.Game.Objects;
using Message = Loki.Bot.Message;

namespace Default.Incursion
{
    public class EnterTempleTask : ITask
    {
        private static Portal ActiveTemplePortal => LokiPoe.ObjectManager.Objects
            .Closest<Portal>(p => p.IsTargetable && p.LeadsTo(a => a.IsTempleOfAtzoatl));

        public async Task<bool> Run()
        {
            var area = World.CurrentArea;

            if (!area.IsMap && !area.IsOverworldArea)
                return false;

            if (LokiPoe.InstanceInfo.Incursion.IncursionsRemaining != 0)
                return false;

            var cache = CombatAreaCache.Current;

            if (cache.Storage["IsTempleCompleted"] != null)
                return false;

            var alva = Incursion.CachedAlva;

            if (alva == null || alva.Unwalkable || alva.Ignored)
                return false;

            var pos = alva.Position;
            if (pos.Distance > 20 || pos.PathDistance > 20)
            {
                if (!pos.TryCome())
                {
                    GlobalLog.Error($"[EnterTempleTask] Fail to move to {pos}. Alva is unwalkable.");
                    alva.Unwalkable = true;
                }
                return true;
            }

            var alvaObj = alva.Object;
            if (alvaObj == null)
            {
                GlobalLog.Error("[EnterTempleTask] Unexpected error. We are near cached Alva, but actual object is null.");
                alva.Ignored = true;
                return true;
            }

            var portal = ActiveTemplePortal;
            if (portal == null)
            {
                var attempts = ++alva.InteractionAttempts;
                if (attempts > 7)
                {
                    GlobalLog.Error("[EnterTempleTask] All attempts to interact with Alva have been spent.");
                    alva.Ignored = true;
                    return true;
                }
                if (alvaObj.HasNpcFloatingIcon)
                {
                    if (await PlayerAction.Interact(alvaObj))
                    {
                        await Wait.Sleep(200);
                        await Coroutines.CloseBlockingWindows();
                    }
                    return true;
                }
                if (await alvaObj.AsTownNpc().Converse("Enter Temple"))
                {
                    await Coroutines.CloseBlockingWindows();
                    await Wait.For(() => ActiveTemplePortal != null, "Temple portals activation", 500, 10000);
                }
                return true;
            }

            if (Settings.Instance.SkipTemple)
            {
                cache.Storage["IsTempleCompleted"] = true;
                return true;
            }

            if (ErrorManager.GetErrorCount("EnterTemple") >= 5)
            {
                GlobalLog.Error("[EnterTempleTask] Failed to enter Temple portal 5 times.");
                alva.Ignored = true;
                return true;
            }
            if (!await PlayerAction.TakePortal(portal))
            {
                ErrorManager.ReportError("EnterTemple");
                await Wait.SleepSafe(500);
            }
            return true;
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

        public string Name => "EnterTempleTask";
        public string Description => "Task that enters The Temple of Atzoatl.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}