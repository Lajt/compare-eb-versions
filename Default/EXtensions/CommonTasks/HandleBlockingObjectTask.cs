using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loki.Bot;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.EXtensions.CommonTasks
{
    public class HandleBlockingObjectTask : ITask
    {
        private const int MaxAttempts = 10;

        private static int _lastId;
        private static int _attempts;

        public async Task<bool> Run()
        {
            if (!World.CurrentArea.IsCombatArea)
                return false;

            Func<NetworkObject> getObj;
            if (AreaSpecificObjects.TryGetValue(World.CurrentArea.Id, out getObj))
            {
                var obj = getObj();
                if (obj != null)
                {
                    var name = obj.Name;
                    if (AttemptLimitReached(obj.Id, name))
                    {
                        await LeaveArea();
                        return true;
                    }
                    if (await PlayerAction.Interact(obj))
                    {
                        await Wait.LatencySleep();
                        await Wait.For(() => getObj() == null, "object interaction", 200, 2000);
                    }
                    else
                    {
                        await Wait.SleepSafe(500);
                    }
                    return true;
                }
            }

            var door = LokiPoe.ObjectManager.Objects.Closest<TriggerableBlockage>(IsClosedDoor);

            if (door == null)
                return false;

            if (AttemptLimitReached(door.Id, door.Name))
            {
                await LeaveArea();
                return true;
            }
            if (await PlayerAction.Interact(door))
            {
                await Wait.LatencySleep();
                await Wait.For(() => !door.IsTargetable || door.IsOpened, "door opening", 50, 300);
                return true;
            }
            await Wait.SleepSafe(300);
            return true;
        }

        private static async Task LeaveArea()
        {
            GlobalLog.Error("[HandleBlockingObjectTask] Fail to remove a blocking object. Now requesting a new instance.");

            EXtensions.AbandonCurrentArea();

            if (!await PlayerAction.TpToTown())
                ErrorManager.ReportError();
        }

        private static bool AttemptLimitReached(int id, string name)
        {
            if (_lastId == id)
            {
                ++_attempts;
                if (_attempts > MaxAttempts)
                {
                    return true;
                }
                if (_attempts >= 2)
                {
                    GlobalLog.Error($"[HandleBlockingObjectTask] {_attempts}/{MaxAttempts} attempt to interact with \"{name}\" (id: {id})");
                }
            }
            else
            {
                _lastId = id;
                _attempts = 0;
            }
            return false;
        }

        private static bool IsClosedDoor(TriggerableBlockage d)
        {
            return d.IsTargetable && !d.IsOpened && d.Distance <= 15 &&
                   (d.Name == "Door" || d.Metadata == "Metadata/MiscellaneousObjects/Smashable" || d.Metadata.Contains("LabyrinthSmashableDoor"));
        }

        private static NetworkObject PitGate()
        {
            return LokiPoe.ObjectManager.Objects
                .Find(o => o.IsTargetable && o.Distance <= 15 && o.Metadata.Contains("PitGateTransition"));
        }

        private static NetworkObject BellyGate()
        {
            return LokiPoe.ObjectManager.Objects
                .Find(o => o.IsTargetable && o.Distance <= 15 && o.Metadata.Contains("BellyArenaTransition"));
        }

        private static NetworkObject TreeRoots()
        {
            var obj = LokiPoe.ObjectManager.Objects
                .Find(o => o.IsTargetable && o.Distance <= 30 && o.Metadata == "Metadata/QuestObjects/Inca/PoisonTree");

            if (obj != null &&
                PlayerHasQuestItem("Metadata/Items/QuestItems/PoisonSpear") &&
                PlayerHasQuestItem("Metadata/Items/QuestItems/PoisonSkillGem"))
                return obj;

            return null;
        }

        private static NetworkObject AncientSeal()
        {
            return LokiPoe.ObjectManager.Objects
                .Find(o => o.IsTargetable && o.Distance <= 20 && o.Metadata == "Metadata/QuestObjects/Inca/IncaDarknessRelease");
        }

        private static NetworkObject SewerGrating()
        {
            var obj = LokiPoe.ObjectManager.Objects
                .Find(o => o.IsTargetable && o.Distance <= 30 && o.Metadata == "Metadata/QuestObjects/Sewers/SewersLockedDoor");

            if (obj != null && PlayerHasQuestItem("Metadata/Items/QuestItems/SewerKeys"))
                return obj;

            return null;
        }

        private static NetworkObject UndyingBlockage()
        {
            var obj = LokiPoe.ObjectManager.Objects
                .Find(o => o.IsTargetable && o.Distance <= 20 && o.Metadata == "Metadata/QuestObjects/Sewers/BioWall");

            if (obj != null && PlayerHasQuestItem("Metadata/Items/QuestItems/InfernalTalc"))
                return obj;

            return null;
        }

        private static NetworkObject TowerDoor()
        {
            var obj = LokiPoe.ObjectManager.Objects
                .Find(o => o.IsTargetable && o.Distance <= 30 && o.Metadata == "Metadata/QuestObjects/EpicDoor/EpicDoorLock");

            if (obj != null && PlayerHasQuestItem("Metadata/Items/QuestItems/TowerKey"))
                return obj;

            return null;
        }

        private static NetworkObject OriathSquareDoor()
        {
            return LokiPoe.ObjectManager.Objects
                .Find(o => o.IsTargetable && o.Distance <= 50 && o.Metadata == "Metadata/Terrain/Act5/Area2/Objects/SlavePenSecurityDoor");
        }

        private static NetworkObject TemplarCourtsDoor()
        {
            var obj = LokiPoe.ObjectManager.Objects
                .Find(o => o.IsTargetable && o.Distance <= 50 && o.Metadata == "Metadata/QuestObjects/Act5/TemplarCourtsDoor");

            if (obj != null && PlayerHasQuestItem("Metadata/Items/QuestItems/Act5/TemplarCourtKey"))
                return obj;

            return null;
        }

        private static NetworkObject KaruiFortressGate()
        {
            var obj = LokiPoe.ObjectManager.Objects
                .Find(o => o.IsTargetable && o.Distance <= 30 && o.Metadata == "Metadata/QuestObjects/Act6/MudFlatsKaruiDoor");

            if (obj != null && PlayerHasQuestItem("Metadata/Items/QuestItems/Act6/KaruiEye"))
                return obj;

            return null;
        }

        private static NetworkObject LooseGrate()
        {
            return LokiPoe.ObjectManager.Objects
                .Find(o => o.IsTargetable && o.Distance <= 30 && o.Metadata == "Metadata/QuestObjects/Sewers/SewersGrate");
        }

        private static NetworkObject SecretPassage()
        {
            var obj = LokiPoe.ObjectManager.Objects
                .Find(o => o.IsTargetable && o.Distance <= 30 && o.Metadata == "Metadata/QuestObjects/Act7/MaligaroPassageCover");

            if (obj != null && PlayerHasQuestItem("Metadata/Items/QuestItems/Act7/ObsidianKey"))
                return obj;

            return null;
        }

        private static NetworkObject VoltaicWorkshop()
        {
            return LokiPoe.ObjectManager.Objects
                .Find(o => o is AreaTransition && o.IsTargetable && o.Distance <= 15 && o.Name == "Voltaic Workshop");
        }

        private static NetworkObject PlazaLever()
        {
            return LokiPoe.ObjectManager.Objects
                .Find(o => o.IsTargetable && o.Distance <= 30 && o.Metadata == "Metadata/Terrain/Labyrinth/Objects/Puzzle_Parts/Switch_Once");
        }

        private static bool PlayerHasQuestItem(string metadata)
        {
            return Inventories.InventoryItems.Exists(i => i.Class == ItemClasses.QuestItem && i.Metadata == metadata);
        }

        private static readonly Dictionary<string, Func<NetworkObject>> AreaSpecificObjects = new Dictionary<string, Func<NetworkObject>>
        {
            [World.Act2.Wetlands.Id] = TreeRoots,
            [World.Act2.VaalRuins.Id] = AncientSeal,
            [World.Act3.Slums.Id] = SewerGrating,
            [World.Act3.Sewers.Id] = UndyingBlockage,
            [World.Act3.ImperialGardens.Id] = TowerDoor,
            [World.Act4.DaressoDream.Id] = PitGate,
            [World.Act4.BellyOfBeast2.Id] = BellyGate,
            [World.Act4.Harvest.Id] = BellyGate,
            [World.Act5.ControlBlocks.Id] = OriathSquareDoor,
            [World.Act5.OriathSquare.Id] = TemplarCourtsDoor,
            [World.Act6.MudFlats.Id] = KaruiFortressGate,
            [World.Act7.ChamberOfSins2.Id] = SecretPassage,
            [World.Act8.DoedreCesspool.Id] = LooseGrate,
            [World.Act9.Refinery.Id] = VoltaicWorkshop,
            [World.Act9.RottingCore.Id] = BellyGate,
            ["MapWorldsPit"] = PitGate,
            ["MapWorldsMalformation"] = BellyGate,
            ["MapWorldsCore"] = BellyGate,
            ["MapWorldsTribunal"] = BellyGate,
            ["MapWorldsSepulchre"] = BellyGate,
            ["MapWorldsOvergrownShrine"] = BellyGate,
            ["MapWorldsFactory"] = VoltaicWorkshop,
            ["MapWorldsPlaza"] = PlazaLever,

            // Keep legacy variants for a while
            ["MapAtlasPit"] = PitGate,
            ["MapAtlasMalformation"] = BellyGate,
            ["MapAtlasCore"] = BellyGate,
            ["MapAtlasOvergrownShrine"] = BellyGate,
            ["MapAtlasFactory"] = VoltaicWorkshop,
            ["MapAtlasPlaza"] = PlazaLever,
        };


        public MessageResult Message(Message message)
        {
            if (message.Id == Events.Messages.AreaChanged)
            {
                _lastId = 0;
                _attempts = 0;
                return MessageResult.Processed;
            }
            return MessageResult.Unprocessed;
        }

        #region Unused interface methods

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public void Start()
        {
        }

        public void Tick()
        {
        }

        public void Stop()
        {
        }

        public string Name => "HandleBlockingObjectTask";
        public string Description => "Task that handles various blocking objects.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}