using Default.EXtensions.Positions;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.EXtensions.CachedObjects
{
    public class CachedTransition : CachedObject
    {
        public TransitionType Type { get; }
        public DatWorldAreaWrapper Destination { get; }
        public bool Visited { get; set; }
        public bool LeadsBack { get; set; }

        public CachedTransition(int id, WalkablePosition position, TransitionType type, DatWorldAreaWrapper destination)
            : base(id, position)
        {
            Type = type;
            Destination = destination;
        }

        public new AreaTransition Object => GetObject() as AreaTransition;
    }
}