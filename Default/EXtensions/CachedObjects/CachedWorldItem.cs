using Default.EXtensions.Positions;
using Loki.Common;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.EXtensions.CachedObjects
{
    public class CachedWorldItem : CachedObject
    {
        public Vector2i Size { get; }
        public Rarity Rarity { get; }

        public CachedWorldItem(int id, WalkablePosition position, Vector2i size, Rarity rarity)
            : base(id, position)
        {
            Size = size;
            Rarity = rarity;
        }

        public new WorldItem Object => GetObject() as WorldItem;
    }
}