using System.Collections.Generic;
using Default.EXtensions;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.AutoFlask
{
    public static class Helpers
    {
        public static int MyCbloodStacks
        {
            get
            {
                // "corrupted_blood", "corrupted_blood_rain"
                var aura = LokiPoe.Me.Auras.Find(a => a.InternalName.StartsWith("corrupted_blood"));
                return aura != null ? aura.Charges : 0;
            }
        }

        public static bool NoMobsInRange(int range, out int closestDistance)
        {
            closestDistance = -1;
            var closest = LokiPoe.ObjectManager.Objects.Closest<Monster>(m => m.IsActive);

            if (closest == null)
                return true;

            var dist = (int) closest.Distance;
            if (dist > range)
            {
                closestDistance = dist;
                return true;
            }
            return false;
        }

        public static bool ShouldTrigger(List<FlaskTrigger> triggers, DataCache cachedData, out string reason)
        {
            reason = string.Empty;

            foreach (var t in triggers)
            {
                var type = t.Type;
                if (type == TriggerType.Hp)
                {
                    var hpPercent = cachedData.HpPercent;
                    if (hpPercent < t.MyHpPercent)
                    {
                        reason = $"we are at {hpPercent}% HP";
                        return true;
                    }
                }
                else if (type == TriggerType.Es)
                {
                    var esPercent = cachedData.EsPercent;
                    if (esPercent < t.MyEsPercent)
                    {
                        reason = $"we are at {esPercent}% ES";
                        return true;
                    }
                }
                else if (type == TriggerType.Mobs)
                {
                    // Do not waste flasks if we are under grace period effect
                    if (cachedData.HasAura(Constants.GracePeriod))
                        return false;

                    var count = cachedData.MobCount(t.MobRarity, t.MobRange);
                    if (count >= t.MobCount)
                    {
                        reason = count == 1
                            ? $"there is 1 {t.MobRarity} monster in range of {t.MobRange}"
                            : $"there are {count} {t.MobRarity} monsters in range of {t.MobRange}";
                        return true;
                    }
                }
            }
            return false;
        }
    }
}