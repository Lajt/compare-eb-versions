using log4net;
using Loki.Bot.Pathfinding;
using Loki.Common;
using Loki.Game;
using Loki.Game.Objects;

namespace Legacy.Breaches
{
	public class BreachCache
	{
		private static readonly ILog Log = Logger.GetLoggerInstanceForType();

		public static int ValidateDistance = 75;

		public bool IsValid { get; private set; }

		public int Id { get; }

		public Vector2i Position { get; }

		public Vector2i WalkablePosition { get; }

		public Breach NetworkObject => LokiPoe.ObjectManager.GetObjectById<Breach>(Id);

		public bool? Activate { get; set; }

		public BreachCache(Breach breach)
		{
			Id = breach.Id;
			Position = breach.Position;
			WalkablePosition = ExilePather.FastWalkablePositionFor(breach);
			IsValid = true;

			Log.InfoFormat("[BreachCache] {0} {1}", Id, WalkablePosition);
		}

		public void Update(Breach breach)
		{
			// Nothing to do here
		}

		public void Validate()
		{
			if (!IsValid)
				return;

			// If we're in spawning range of the breach, but it no longer exists or is not a Breach object, it's no longer valid.
			if (LokiPoe.MyPosition.Distance(Position) <= ValidateDistance)
			{
				var breach = NetworkObject;
				if (breach == null)
				{
					Log.InfoFormat("[BreachCache::Validate] The Breach [{0}] is no longer valid because it does not exist.", Id);
					IsValid = false;
				}
			}
		}
	}
}