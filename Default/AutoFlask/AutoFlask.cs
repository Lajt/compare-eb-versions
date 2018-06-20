using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using Default.EXtensions;
using log4net;
using Loki.Bot;
using Loki.Common;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;
using AutoFlaskSettings = Default.AutoFlask.Settings;
using FlaskHud = Loki.Game.LokiPoe.InGameState.QuickFlaskHud;

namespace Default.AutoFlask
{
    public class AutoFlask : IPlugin, ITickEvents, IUrlProvider
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();
        private Gui _gui;

        private static readonly Func<Rarity, int, bool> FlaskHookDelegate = FlaskHook;

        private static readonly LatencyInterval DebuffInterval = new LatencyInterval(100);
        private static readonly LatencyInterval LifeInterval = new LatencyInterval(150);
        private static readonly LatencyInterval ManaInterval = new LatencyInterval(300);
        private static readonly LatencyInterval QsilverInterval = new LatencyInterval(250);
        private static readonly LatencyInterval TriggerInterval = new LatencyInterval(200);

        private static readonly DataCache CachedData = new DataCache();

        private static FlaskInfo _info = new FlaskInfo();

        public void Tick()
        {
            if (!LokiPoe.IsInGame || !World.CurrentArea.IsCombatArea || LokiPoe.Me.IsDead)
                return;

            CachedData.Clear();

            var settings = AutoFlaskSettings.Instance;

            // Debuff check
            if (DebuffInterval.Elapsed)
            {
                Item flask;

                // Freeze
                if (_info.HasAntiFreeze && settings.RemoveFreeze && CachedData.HasAura(Constants.FreezeEffect))
                {
                    flask = Flasks.KiaraDetermination;
                    if (flask != null)
                    {
                        Use(flask, "we are frozen");
                        return;
                    }
                    flask = Flasks.ByStat(Constants.AntiFreezeStat);
                    if (flask != null)
                    {
                        Use(flask, "we are frozen");
                        return;
                    }
                }

                // Silence
                if (_info.HasAntiCurse && settings.RemoveSilence && CachedData.HasAura(Constants.SilenceEffect))
                {
                    flask = Flasks.KiaraDetermination;
                    if (flask != null)
                    {
                        Use(flask, "we are cursed with Silence");
                        return;
                    }
                    flask = Flasks.ByStat(Constants.AntiCurseStat);
                    if (flask != null)
                    {
                        Use(flask, "we are cursed with Silence");
                        return;
                    }
                }

                // Bleed
                if (_info.HasAntiBleed)
                {
                    // Bleed during movement
                    if (settings.RemoveBleed && CachedData.HasAura(Constants.BleedMovementEffect))
                    {
                        flask = Flasks.ByStat(Constants.AntiBleedStat);
                        if (flask != null)
                        {
                            Use(flask, "we are bleeding during movement");
                            return;
                        }
                    }

                    // Corrupted Blood
                    if (settings.RemoveCblood)
                    {
                        var stacks = Helpers.MyCbloodStacks;
                        if (stacks >= settings.MinCbloodStacks)
                        {
                            flask = Flasks.ByStat(Constants.AntiBleedStat);
                            if (flask != null)
                            {
                                Use(flask, $"we have {stacks} Corrupted Blood stacks");
                                return;
                            }
                        }
                    }
                }

                // Poison
                if (_info.HasAntiPoison && settings.RemovePoison)
                {
                    var stacks = CachedData.PoisonStacks;
                    if (stacks >= settings.MinPoisonStacks)
                    {
                        flask = Flasks.ByStat(Constants.AntiPoisonStat);
                        if (flask != null)
                        {
                            Use(flask, $"we have {stacks} Poison stacks");
                            return;
                        }
                    }
                }

                // Shock
                if (_info.HasAntiShock && settings.RemoveShock && CachedData.HasAura(Constants.ShockEffect))
                {
                    flask = Flasks.ByStat(Constants.AntiShockStat);
                    if (flask != null)
                    {
                        Use(flask, "we are shocked");
                        return;
                    }
                }

                // Ignite
                if (_info.HasAntiIgnite && settings.RemoveIgnite && CachedData.HasAura(Constants.IgniteEffect))
                {
                    flask = Flasks.ByStat(Constants.AntiIgniteStat);
                    if (flask != null)
                    {
                        Use(flask, "we are ignited");
                        return;
                    }
                }
            }

            // Life flask
            if (_info.HasLifeFlask && LifeInterval.Elapsed)
            {
                Item flask;
                bool overTimeUsed = false;

                var hpPercent = (int) LokiPoe.Me.HealthPercent;
                if (hpPercent < settings.HpPercent && !CachedData.HasAura(Constants.LifeFlaskEffect))
                {
                    if ((flask = Flasks.LifeFlask) != null)
                    {
                        Use(flask, $"we are at {hpPercent}% HP");
                        overTimeUsed = true;
                    }
                    else if ((flask = Flasks.HybridFlask) != null)
                    {
                        Use(flask, $"we are at {hpPercent}% HP");
                        overTimeUsed = true;
                    }
                }
                if (!overTimeUsed && hpPercent < settings.HpPercentInstant)
                {
                    if ((flask = Flasks.InstantLifeFlask) != null)
                    {
                        Use(flask, $"we are at {hpPercent}% HP");
                    }
                    else if ((flask = Flasks.InstantHybridFlask) != null)
                    {
                        Use(flask, $"we are at {hpPercent}% HP");
                    }
                }
            }

            // Mana flask
            if (_info.HasManaFlask && ManaInterval.Elapsed)
            {
                var mpPercent = (int) LokiPoe.Me.ManaPercent;
                if (mpPercent < settings.MpPercent)
                {
                    var lastAction = LokiPoe.InstanceInfo.LastAction;
                    if (lastAction != null && !lastAction.IsManaReserving)
                    {
                        Item flask;
                        bool overTimeUsed = false;

                        if (!CachedData.HasAura(Constants.ManaFlaskEffect))
                        {
                            if ((flask = Flasks.ManaFlask) != null)
                            {
                                Use(flask, $"we are at {mpPercent}% MP");
                                overTimeUsed = true;
                            }
                            else if ((flask = Flasks.HybridFlask) != null)
                            {
                                Use(flask, $"we are at {mpPercent}% MP");
                                overTimeUsed = true;
                            }
                        }
                        if (!overTimeUsed)
                        {
                            if ((flask = Flasks.InstantManaFlask) != null)
                            {
                                Use(flask, $"we are at {mpPercent}% MP");
                            }
                            else if ((flask = Flasks.InstantHybridFlask) != null)
                            {
                                Use(flask, $"we are at {mpPercent}% MP");
                            }
                        }
                    }
                }
            }

            // Quicksilver flask
            if (_info.HasQsilverFlask && QsilverInterval.Elapsed)
            {
                if (!CachedData.HasAura(Constants.QsilverEffect))
                {
                    if (settings.QsilverRange == 0)
                    {
                        var flask = Flasks.QuicksilverFlask;
                        if (flask != null)
                        {
                            Use(flask, "quicksilver flask is in \"fire at will\" mode");
                        }
                    }
                    else if (LokiPoe.InstanceInfo.LastActionId == Constants.MoveSkillId &&
                             Helpers.NoMobsInRange(settings.QsilverRange, out var dist))
                    {
                        var flask = Flasks.QuicksilverFlask;
                        if (flask != null)
                        {
                            var reason = dist == -1 ? "there are no monsters nearby" : $"closest monster is {dist} away";
                            Use(flask, reason);
                        }
                    }
                }
            }

            // Trigger flasks (HP/ES percent and mobs nearby)
            if (_info.HasTriggerFlask && TriggerInterval.Elapsed)
            {
                foreach (var info in _info.TriggerFlasks)
                {
                    if (!CachedData.HasAura(info.Effect) && Helpers.ShouldTrigger(info.Triggers, CachedData, out var reason))
                    {
                        var flask = Flasks.ByProperName(info.Name);
                        if (flask != null)
                        {
                            Use(flask, reason);
                        }
                    }
                }
            }
        }

        // Trigger flasks (before attacking)
        private static bool FlaskHook(Rarity rarity, int hpPercent)
        {
            bool used = false;
            foreach (var info in _info.TriggerFlasks)
            {
                foreach (var t in info.Triggers)
                {
                    if (t.Type != TriggerType.Attack)
                        continue;

                    if (rarity != t.MobRarity || hpPercent < t.MobHpPercent)
                        continue;

                    // 1 sec should be ok, shortest flask in the game has 2 sec duration
                    if (info.PostUseDelay.ElapsedMilliseconds < 1000)
                        continue;

                    if (CachedData.HasAura(info.Effect))
                        continue;

                    var flask = Flasks.ByProperName(info.Name);

                    if (flask == null)
                        continue;

                    info.PostUseDelay.Restart();
                    Use(flask, $"we are attacking {rarity} monster with {hpPercent}% HP");
                    used = true;
                }
            }
            return used;
        }

        private static void Use(Item flask, string reason)
        {
            var slot = flask.LocationTopLeft.X + 1;

            Log.Info($"[Autoflask] Using {flask.ProperName()} (slot {slot}) because {reason}.");

            if (!FlaskHud.UseFlaskInSlot(slot))
                Log.Error($"[Autoflask] UseFlaskInSlot returned false for slot {slot}.");
        }

        private static void SetFlaskInfo()
        {
            _info = new FlaskInfo();

            foreach (var flask in FlaskHud.InventoryControl.Inventory.Items)
            {
                foreach (var stat in flask.LocalStats.Keys)
                {
                    if (stat == Constants.AntiFreezeStat)
                        _info.HasAntiFreeze = true;

                    else if (stat == Constants.AntiIgniteStat)
                        _info.HasAntiIgnite = true;

                    else if (stat == Constants.AntiShockStat)
                        _info.HasAntiShock = true;

                    else if (stat == Constants.AntiPoisonStat)
                        _info.HasAntiPoison = true;

                    else if (stat == Constants.AntiCurseStat)
                        _info.HasAntiCurse = true;

                    else if (stat == Constants.AntiBleedStat)
                        _info.HasAntiBleed = true;
                }

                if (flask.Name == FlaskNames.Quicksilver)
                {
                    _info.HasQsilverFlask = true;
                    continue;
                }

                var c = flask.Class;
                if (c == ItemClasses.LifeFlask)
                {
                    _info.HasLifeFlask = true;
                    continue;
                }
                if (c == ItemClasses.ManaFlask)
                {
                    _info.HasManaFlask = true;
                    continue;
                }

                var fullName = flask.FullName;
                if (c == ItemClasses.HybridFlask && fullName != FlaskNames.DivinationDistillate)
                {
                    _info.HasLifeFlask = true;
                    _info.HasManaFlask = true;
                    continue;
                }
                if (fullName == FlaskNames.KiaraDetermination)
                {
                    _info.HasAntiFreeze = true;
                    _info.HasAntiCurse = true;
                }

                var name = flask.ProperName();

                var triggers = AutoFlaskSettings.Instance.GetFlaskTriggers(name);
                if (triggers == null)
                {
                    Log.Warn($"[Autoflask] \"{name}\" is unknown and will not be used.");
                    continue;
                }
                if (triggers.Count == 0)
                {
                    Log.Warn($"[Autoflask] \"{name}\" has no assigned triggers and will not be used.");
                    continue;
                }

                var effect = Flasks.GetEffect(name);
                if (effect == null)
                {
                    Log.Warn($"[Autoflask] \"{name}\" has unknown effect and will not be used.");
                    continue;
                }
                _info.AddTriggerFlask(flask.LocationTopLeft.X, name, effect, triggers);
                _info.HasTriggerFlask = true;
            }

            _info.Log();
        }


        private void SetFlaskHook()
        {
            if (_info.TriggerFlasks.Exists(f => f.Triggers.Exists(t => t.Type == TriggerType.Attack)))
            {
                var msg = new Message("SetFlaskHook", this, FlaskHookDelegate);
                var r = RoutineManager.Current;
                if (r.Message(msg) != MessageResult.Processed)
                {
                    Log.Error($"[Autoflask] Cannot use \"Before attacking\" triggers because {r.Name} does not support flask hook.");
                    Log.Error("[Autoflask] Please use routine from latest bot version or remove all \"Before attacking\" triggers.");
                    BotManager.Stop();
                }
            }
        }

        public MessageResult Message(Message message)
        {
            if (message.Id == Events.Messages.IngameBotStart)
            {
                SetFlaskInfo();
                SetFlaskHook();
                return MessageResult.Processed;
            }
            return MessageResult.Unprocessed;
        }

        #region Unused interface methods

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

        public string Name => "AutoFlask";
        public string Description => "Plugin that provides flask usage functionality.";
        public string Author => "ExVault";
        public string Version => "1.0";
        public JsonSettings Settings => AutoFlaskSettings.Instance;
        public UserControl Control => _gui ?? (_gui = new Gui());
        public string Url => "https://www.thebuddyforum.com/threads/autoflask-settings-description.408148/";

        #endregion
    }
}