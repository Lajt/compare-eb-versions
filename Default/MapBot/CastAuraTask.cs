using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions;
using Loki.Bot;
using Loki.Game;
using Loki.Game.Objects;
using SkillBar = Loki.Game.LokiPoe.InGameState.SkillBarHud;

namespace Default.MapBot
{
    public class CastAuraTask : ITask
    {
        private const int MinGolemHpPercent = 80;

        public async Task<bool> Run()
        {
            var area = World.CurrentArea;
            if (!area.IsHideoutArea && !area.IsMapRoom)
                return false;

            await Coroutines.CloseBlockingWindows();

            var golemSkill = SkillBar.Skills.FirstOrDefault(s => s.IsOnSkillBar && s.SkillTags.Contains("golem"));
            if (golemSkill != null)
            {
                var golemObj = golemSkill.DeployedObjects.FirstOrDefault() as Monster;
                if (golemObj == null || golemObj.HealthPercent < MinGolemHpPercent)
                {
                    GlobalLog.Debug($"[CastAuraTask] Now summoning \"{golemSkill.Name}\".");
                    SkillBar.Use(golemSkill.Slot, false);
                    await Wait.SleepSafe(100);
                    await Coroutines.FinishCurrentAction();
                    await Wait.SleepSafe(100);
                }
            }
            var auras = GetAurasForCast();
            if (auras.Count > 0 && AllAuras.Any(a => a.IsOnSkillBar))
            {
                GlobalLog.Info($"[CastAuraTask] Found {auras.Count} aura(s) for casting.");
                await CastAuras(auras);
            }
            return false;
        }

        private static async Task CastAuras(IEnumerable<Skill> auras)
        {
            int slotForHidden = AllAuras.First(a => a.IsOnSkillBar).Slot;
            foreach (var aura in auras.OrderByDescending(a => a.Slot))
            {
                if (SkillBlacklist.IsBlacklisted(aura))
                    continue;

                if (aura.Slot == -1)
                    await SetAuraToSlot(aura, slotForHidden);

                await ApplyAura(aura);
            }
        }

        private static async Task ApplyAura(Skill aura)
        {
            string name = aura.Name;
            GlobalLog.Debug($"[CastAuraTask] Now casting \"{name}\".");
            var used = SkillBar.Use(aura.Slot, false);
            if (used != LokiPoe.InGameState.UseResult.None)
            {
                GlobalLog.Error($"[CastAuraTask] Fail to cast \"{name}\". Error: \"{used}\".");
                return;
            }
            await Wait.For(() => !LokiPoe.Me.HasCurrentAction && PlayerHasAura(name), "aura applying");
            await Wait.SleepSafe(100);
        }

        private static async Task SetAuraToSlot(Skill aura, int slot)
        {
            string name = aura.Name;
            GlobalLog.Debug($"[CastAuraTask] Now setting \"{name}\" to slot {slot}.");
            var isSet = SkillBar.SetSlot(slot, aura);
            if (isSet != LokiPoe.InGameState.SetSlotResult.None)
            {
                GlobalLog.Error($"[CastAuraTask] Fail to set \"{name}\" to slot {slot}. Error: \"{isSet}\".");
                return;
            }
            await Wait.For(() => IsInSlot(slot, name), "aura slot changing");
            await Wait.SleepSafe(100);
        }

        private static bool IsInSlot(int slot, string name)
        {
            var skill = SkillBar.Slot(slot);
            return skill != null && skill.Name == name;
        }

        private static List<Skill> GetAurasForCast()
        {
            var auras = new List<Skill>();
            foreach (var aura in AllAuras)
            {
                if (GeneralSettings.Instance.IgnoreHiddenAuras && !aura.IsOnSkillBar)
                    continue;

                if (PlayerHasAura(aura.Name))
                    continue;

                auras.Add(aura);
            }
            return auras;
        }

        private static IEnumerable<Skill> AllAuras => SkillBar.Skills.Where(skill => AuraNames.Contains(skill.Name) || skill.IsAurifiedCurse);

        private static bool PlayerHasAura(string auraName)
        {
            return LokiPoe.Me.Auras.Any(a => a.Name.EqualsIgnorecase(auraName) || a.Name.EqualsIgnorecase(auraName + " aura"));
        }

        private static readonly HashSet<string> AuraNames = new HashSet<string>
        {
            // auras
            "Anger",
            "Clarity",
            "Determination",
            "Discipline",
            "Grace",
            "Haste",
            "Hatred",
            "Purity of Elements",
            "Purity of Fire",
            "Purity of Ice",
            "Purity of Lightning",
            "Vitality",
            "Wrath",

            // heralds
            "Herald of Ash",
            "Herald of Ice",
            "Herald of Thunder",

            // the rest
            "Arctic Armour"
        };

        #region Unused interface methods

        public MessageResult Message(Message message)
        {
            return MessageResult.Unprocessed;
        }

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public void Tick()
        {
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public string Name => "CastAuraTask";
        public string Description => "Task for casting auras before entering a map.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}