using Default.EXtensions.Positions;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.EXtensions.CachedObjects
{
    public class CachedStrongbox : CachedObject
    {
        public Rarity Rarity { get; }

        public CachedStrongbox(int id, WalkablePosition position, Rarity rarity)
            : base(id, position)
        {
            Rarity = rarity;
        }

        public new Chest Object => GetObject() as Chest;
    }
}