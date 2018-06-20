using System.ComponentModel;
using Loki.Game.GameData;

namespace Default.AutoFlask
{
    public class FlaskTrigger
    {
        public TriggerType Type { get; set; } = TriggerType.Hp;
        public int MyHpPercent { get; set; } = 75;
        public int MyEsPercent { get; set; } = 75;
        public Rarity MobRarity { get; set; } = Rarity.Normal;
        public int MobCount { get; set; } = 1;
        public int MobRange { get; set; } = 40;
        public int MobHpPercent { get; set; } = 0;

        public override string ToString()
        {
            if (Type == TriggerType.Hp)
                return $"{MyHpPercent}% HP";

            if (Type == TriggerType.Es)
                return $"{MyEsPercent}% ES";

            if (Type == TriggerType.Mobs)
                return $"{MobCount} {MobRarity} mob{(MobCount == 1 ? "" : "s")} in {MobRange} range";

            if (Type == TriggerType.Attack)
                return $"{MobRarity} mob {MobHpPercent}% HP";

            return $"Incorrect type: {Type}";
        }
    }

    public enum TriggerType
    {
        [Description("HP percent")]
        Hp,

        [Description("ES percent")]
        Es,

        [Description("Monsters nearby")]
        Mobs,

        [Description("Before attacking")]
        Attack,
    }
}