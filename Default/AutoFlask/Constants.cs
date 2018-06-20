using Loki.Game.GameData;

namespace Default.AutoFlask
{
    public static class Constants
    {
        public const int MoveSkillId = 10505;

        public const string GracePeriod = "grace_period";

        public const string LifeFlaskEffect = "flask_effect_life";
        public const string ManaFlaskEffect = "flask_effect_mana";
        public const string QsilverEffect = "flask_utility_sprint";

        public const string FreezeEffect = "frozen";
        public const string ShockEffect = "shocked";
        public const string IgniteEffect = "ignited";
        public const string PoisonEffect = "poison";
        public const string SilenceEffect = "curse_silence";
        public const string BleedMovementEffect = "bleeding_moving";

        public const StatTypeGGG AntiFreezeStat = StatTypeGGG.LocalFlaskChillAndFreezeImmunityWhileHealing;
        public const StatTypeGGG AntiIgniteStat = StatTypeGGG.LocalFlaskIgniteImmunityWhileHealing;
        public const StatTypeGGG AntiShockStat = StatTypeGGG.LocalFlaskShockImmunityWhileHealing;
        public const StatTypeGGG AntiPoisonStat = StatTypeGGG.LocalFlaskPoisonImmunityDuringFlaskEffect;
        public const StatTypeGGG AntiBleedStat = StatTypeGGG.LocalFlaskBleedingImmunityWhileHealing;
        public const StatTypeGGG AntiCurseStat = StatTypeGGG.LocalFlaskCurseImmunityWhileHealing;
    }
}