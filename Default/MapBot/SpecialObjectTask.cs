using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Loki.Bot;
using Loki.Game;
using Loki.Game.GameData;

namespace Default.MapBot
{
    public class SpecialObjectTask : ITask
    {
        private const int MaxInteractionAttempts = 5;

        private static readonly Interval TickInterval = new Interval(200);

        private static readonly List<CachedObject> Objects = new List<CachedObject>();

        private static bool _enabled;
        private static CachedObject _current;
        private static Func<Task> _postInteraction;

        public async Task<bool> Run()
        {
            if (!_enabled || MapExplorationTask.MapCompleted || !World.CurrentArea.IsMap)
                return false;

            if (_current == null)
            {
                if ((_current = Objects.ClosestValid()) == null)
                    return false;
            }

            var pos = _current.Position;
            if (pos.IsFar || pos.IsFarByPath)
            {
                if (!pos.TryCome())
                {
                    GlobalLog.Error($"[SpecialObjectTask] Fail to move to {pos}. Marking this special object as unwalkable.");
                    _current.Unwalkable = true;
                    _current = null;
                }
                return true;
            }
            var obj = _current.Object;
            if (obj == null || !obj.IsTargetable)
            {
                Objects.Remove(_current);
                _current = null;
                return true;
            }

            var name = _current.Position.Name;

            var attempts = ++_current.InteractionAttempts;
            if (attempts > MaxInteractionAttempts)
            {
                GlobalLog.Error($"[SpecialObjectTask] All attempts to interact with \"{name}\" have been spent. Now ignoring it.");
                _current.Ignored = true;
                _current = null;
                return true;
            }
            if (await PlayerAction.Interact(obj, () => !obj.Fresh().IsTargetable, $"\"{name}\" interaction", 1000))
            {
                if (_postInteraction != null)
                    await _postInteraction();
            }
            else
            {
                await Wait.SleepSafe(500);
            }
            return true;
        }

        public void Tick()
        {
            if (!_enabled || MapExplorationTask.MapCompleted)
                return;

            if (!TickInterval.Elapsed)
                return;

            if (!LokiPoe.IsInGame || !World.CurrentArea.IsMap)
                return;

            foreach (var obj in LokiPoe.ObjectManager.Objects)
            {
                if (!SpecialObjectMetadata.Contains(obj.Metadata))
                    continue;

                var id = obj.Id;
                var cached = Objects.Find(s => s.Id == id);

                if (obj.IsTargetable)
                {
                    if (cached == null)
                    {
                        var pos = obj.WalkablePosition(5, 20);
                        Objects.Add(new CachedObject(obj.Id, pos));
                        GlobalLog.Debug($"[SpecialObjectTask] Registering {pos}");
                    }
                }
                else
                {
                    if (cached != null)
                    {
                        if (cached == _current) _current = null;
                        Objects.Remove(cached);
                    }
                }
            }
        }

        private static void Reset(string areaName)
        {
            _enabled = false;
            _current = null;
            _postInteraction = null;
            Objects.Clear();

            if (LokiPoe.LocalData.MapMods.ContainsKey(StatTypeGGG.MapZanaSubareaMission))
            {
                GlobalLog.Info("[SpecialObjectTask] Zana map detected.");
                _enabled = true;
                return;
            }
            if (areaName == MapNames.OlmecSanctum)
            {
                _enabled = true;
                _postInteraction = OlmecPostInteraction;
                return;
            }
            if (areaName == MapNames.MaoKun)
            {
                _enabled = true;
                _postInteraction = MaoKunPostInteraction;
                return;
            }
            if (GeneralSettings.Instance.BreakNests &&
                (areaName == MapNames.AridLake ||
                 areaName == MapNames.Bog))
            {
                _enabled = true;
                return;
            }
            if (areaName == MapNames.Desert ||
                areaName == MapNames.WastePool ||
                areaName == MapNames.WhakawairuaTuahu)
            {
                _enabled = true;
            }
        }

        private static readonly HashSet<string> SpecialObjectMetadata = new HashSet<string>
        {
            // Zana quest objects
            "Metadata/Effects/Environment/artifacts/Gaius/ObjectiveTablet",
            "Metadata/Effects/Environment/artifacts/Gaius/TimeTablet",

            // Desert - Storm-Weathered Chest
            "Metadata/Terrain/EndGame/MapDesert/Objects/MummyEventChest",

            // Waste Pool - Valve
            "Metadata/Monsters/Doedre/DoedreSewer/DoedreCauldronValve",

            // Whakawairua Tuahu - Ancient Seal
            "Metadata/Terrain/EndGame/MapShipGraveyardCagan/Objects/IncaReleaseBall",

            // Olmec's Sanctum - Silver Monkey body parts
            "Metadata/Terrain/EndGame/MapIncaUniqueLegends/Objects/LegendsGlyph1",
            "Metadata/Terrain/EndGame/MapIncaUniqueLegends/Objects/LegendsGlyph2",
            "Metadata/Terrain/EndGame/MapIncaUniqueLegends/Objects/LegendsGlyph3",
            "Metadata/Terrain/EndGame/MapIncaUniqueLegends/Objects/LegendsGlyph4",
            "Metadata/Terrain/EndGame/MapIncaUniqueLegends/Objects/LegendsGlyphMain",

            // Mao Kun - Fairgraves
            "Metadata/Terrain/EndGame/MapTreasureIsland/Objects/FairgravesTreasureIsland",
        };

        internal static void ToggleRhoaNests(bool enable)
        {
            if (enable)
            {
                SpecialObjectMetadata.Add("Metadata/Terrain/EndGame/MapSaltFlats/Objects/AngeredBird");
                SpecialObjectMetadata.Add("Metadata/Terrain/EndGame/MapSwampFetid/Objects/AngeredBird");
            }
            else
            {
                SpecialObjectMetadata.Remove("Metadata/Terrain/EndGame/MapSaltFlats/Objects/AngeredBird");
                SpecialObjectMetadata.Remove("Metadata/Terrain/EndGame/MapSwampFetid/Objects/AngeredBird");
            }
        }

        private static async Task OlmecPostInteraction()
        {
            await Wait.For(() => CombatAreaCache.Current.AreaTransitions
                .Any(a => a.Position.Name == MapNames.OlmecSanctum && a.Position.Distance < 200), "area transition activation");
        }

        private static async Task MaoKunPostInteraction()
        {
            GlobalLog.Warn("[SpecialObjectTask] Fairgraves has been interacted. Now resetting the Explorer.");
            CombatAreaCache.Current.Explorer.BasicExplorer.Reset();
        }

        public MessageResult Message(Message message)
        {
            var id = message.Id;
            if (id == MapBot.Messages.NewMapEntered)
            {
                GlobalLog.Info("[SpecialObjectTask] Reset.");

                Reset(message.GetInput<string>());

                if (_enabled)
                    GlobalLog.Info("[SpecialObjectTask] Enabled.");

                return MessageResult.Processed;
            }
            if (id == ComplexExplorer.LocalTransitionEnteredMessage)
            {
                GlobalLog.Info("[SpecialObjectTask] Resetting unwalkable flags.");
                foreach (var speacialObj in Objects)
                {
                    speacialObj.Unwalkable = false;
                }
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

        public void Stop()
        {
        }

        public string Name => "SpecialObjectTask";
        public string Description => "Task that handles objects specific to certain maps.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}