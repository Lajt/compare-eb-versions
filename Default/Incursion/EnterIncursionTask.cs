using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Loki.Bot;
using Loki.Game;
using Loki.Game.Objects;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Positions;
using Message = Loki.Bot.Message;

namespace Default.Incursion
{
    public class EnterIncursionTask : ITask
    {
        private static AreaTransition TimePortal => LokiPoe.ObjectManager.Objects
            .FirstOrDefault<AreaTransition>(a => a.Metadata.Contains("IncursionPortal"));

        private static bool AnyPortalsNearby => LokiPoe.ObjectManager.Objects
            .Any<Portal>(p => p.IsTargetable && p.Distance <= 70 && p.LeadsTo(a => a.IsHideoutArea || a.IsMapRoom));

        public async Task<bool> Run()
        {
            var area = World.CurrentArea;

            if (!area.IsMap && !area.IsOverworldArea)
                return false;

            if (CombatAreaCache.IsInIncursion)
                return false;

            if (LokiPoe.InstanceInfo.Incursion.IncursionsRemaining == 0)
                return false;

            var cache = CombatAreaCache.Current;

            if (cache.Storage["IsIncursionCompleted"] != null)
                return false;

            var alva = cache.Storage["AlvaValai"] as CachedObject;

            if (alva == null || alva.Unwalkable || alva.Ignored)
                return false;

            var pos = alva.Position;
            if (pos.IsFar || pos.IsFarByPath)
            {
                if (!pos.TryCome())
                {
                    GlobalLog.Error($"[EnterIncursionTask] Fail to move to {pos}. Alva is unwalkable.");
                    alva.Unwalkable = true;
                }
                return true;
            }

            var alvaObj = alva.Object;
            if (alvaObj == null)
            {
                GlobalLog.Error("[EnterIncursionTask] Unexpected error. We are near cached Alva, but actual object is null.");
                alva.Ignored = true;
                return true;
            }

            var timePortal = TimePortal;
            if (timePortal == null)
            {
                GlobalLog.Error("[EnterIncursionTask] Unexpected error. There is no Time Portal near Alva.");
                cache.Storage["IsIncursionCompleted"] = true;
                return true;
            }

            if (timePortal.Components.TransitionableComponent.Flag2 == 3)
            {
                GlobalLog.Warn("[EnterIncursionTask] Incursion in this area has been completed.");
                cache.Storage["IsIncursionCompleted"] = true;

                if (Settings.Instance.LeaveAfterIncursion)
                    FinishGridning();

                return true;
            }

            if (!timePortal.IsTargetable)
            {
                var attempts = ++alva.InteractionAttempts;
                if (attempts > 5)
                {
                    GlobalLog.Error("[EnterIncursionTask] All attempts to interact with Alva have been spent.");
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
                if (await alvaObj.AsTownNpc().Converse("Enter Incursion"))
                {
                    await Coroutines.CloseBlockingWindows();
                    await Wait.For(() => timePortal.Fresh().IsTargetable, "Time Portal activation", 200, 5000);
                }
                return true;
            }

            if (Settings.Instance.PortalBeforeIncursion && area.IsMap && !AnyPortalsNearby)
            {
                // Have to do this because portal spawned right near Time Portal has a high chance to overlap labels
                var distantPos = WorldPosition.FindPathablePositionAtDistance(30, 35, 5);
                if (distantPos != null)
                {
                    await Move.AtOnce(distantPos, "away from Time Portal", 10);
                    await PlayerAction.CreateTownPortal();
                }
                else
                {
                    await PlayerAction.CreateTownPortal();
                }
            }

            if (ErrorManager.GetErrorCount("EnterIncursion") >= 5)
            {
                GlobalLog.Error("[EnterIncursionTask] Failed to enter Time Portal 5 times.");
                cache.Storage["IsIncursionCompleted"] = true;
                return true;
            }
            if (await PlayerAction.TakeTransition(timePortal))
            {
                GlobalLog.Warn("[EnterIncursionTask] IsInIncursion: true");
                CombatAreaCache.IsInIncursion = true;
                SetRoomSettings();
            }
            else
            {
                ErrorManager.ReportError("EnterIncursion");
                await Wait.SleepSafe(500);
            }
            return true;
        }

        private static void SetRoomSettings()
        {
            var room = LokiPoe.InstanceInfo.Incursion.CurrentIncursionRoom;
            GlobalLog.Info($"[Incursion] Current room: {room.Name} (Tier {room.Tier})");

            var roomSettings = Settings.Instance.IncursionRooms.Find(r => r.IdEquals(room.AreaId));
            if (roomSettings != null)
            {
                Incursion.CurrentRoomSettings = roomSettings;
                GlobalLog.Info($"[Incursion] Prioritize: {roomSettings.PriorityAction}");
                GlobalLog.Info($"[Incursion] Never change: {roomSettings.NoChange}");
                GlobalLog.Info($"[Incursion] Never upgrade: {roomSettings.NoUpgrade}");
            }
            else
            {
                Incursion.CurrentRoomSettings = null;
            }
        }

        private static void FinishGridning()
        {
            var bot = BotManager.Current;

            if (!bot.Name.Contains("QuestBot"))
                return;

            var msg = new Message("QB_get_current_quest");
            if (bot.Message(msg) != MessageResult.Processed)
            {
                GlobalLog.Debug("[EnterIncursionTask] \"QB_get_current_quest\" message was not processed.");
                return;
            }

            var questName = msg.GetOutput<string>();

            if (questName != "Grinding")
                return;

            msg = new Message("QB_finish_grinding");
            if (bot.Message(msg) != MessageResult.Processed)
            {
                GlobalLog.Debug("[EnterIncursionTask] \"QB_finish_grinding\" message was not processed.");
            }
        }

        public void Tick()
        {
            if (!CombatAreaCache.IsInIncursion)
                return;

            if (!LokiPoe.IsInGame)
                return;

            var area = World.CurrentArea;

            if (!area.IsMap && !area.IsOverworldArea)
                return;

            // No tick interval here. We want to set these flags as fast as possible.

            if (Incursion.Alva != null)
            {
                GlobalLog.Warn("[EnterIncursionTask] IsInIncursion: false");
                CombatAreaCache.IsInIncursion = false;
                CombatAreaCache.Current.Storage["IsIncursionCompleted"] = true;

                if (Settings.Instance.LeaveAfterIncursion)
                    FinishGridning();
            }
        }

        public MessageResult Message(Message message)
        {
            var id = message.Id;
            if (id == Events.Messages.AreaChanged || id == Events.Messages.PlayerResurrected)
            {
                CombatAreaCache.IsInIncursion = false;
                return MessageResult.Processed;
            }
            return MessageResult.Unprocessed;
        }

        #region Unused interface methods

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public string Name => "EnterIncursionTask";
        public string Description => "Task that enters incursions.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}