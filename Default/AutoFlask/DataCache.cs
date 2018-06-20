using System.Collections.Generic;
using System.Linq;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.AutoFlask
{
    public class DataCache
    {
        private int _hpPercent;
        private int _esPercent;
        private List<string> _auras;
        private List<MobData> _mobs;

        private List<string> Auras => _auras ?? (_auras = LokiPoe.Me.Auras.Select(a => a.InternalName).ToList());

        public int PoisonStacks => Auras.Count(a => a == Constants.PoisonEffect);

        public int HpPercent
        {
            get
            {
                if (_hpPercent == -1)
                    _hpPercent = (int) LokiPoe.Me.HealthPercent;

                return _hpPercent;
            }
        }

        public int EsPercent
        {
            get
            {
                if (_esPercent == -1)
                    _esPercent = (int) LokiPoe.Me.EnergyShieldPercent;

                return _esPercent;
            }
        }

        public bool HasAura(string name)
        {
            return Auras.Exists(a => a == name);
        }

        public int MobCount(Rarity rarity, int range)
        {
            if (_mobs == null)
            {
                _mobs = new List<MobData>();
                foreach (var obj in LokiPoe.ObjectManager.Objects)
                {
                    var mob = obj as Monster;
                    if (mob != null && mob.IsActive)
                        _mobs.Add(new MobData(mob));
                }
            }
            return _mobs.Count(mob => mob.Rarity == rarity && mob.Distance <= range);
        }

        public void Clear()
        {
            _hpPercent = -1;
            _esPercent = -1;
            _auras = null;
            _mobs = null;
        }

        private struct MobData
        {
            public readonly Rarity Rarity;
            public readonly float Distance;

            public MobData(Monster mob)
            {
                Rarity = mob.Rarity;
                Distance = mob.Distance;
            }
        }
    }
}