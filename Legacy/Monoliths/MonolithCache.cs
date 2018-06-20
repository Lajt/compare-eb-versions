using System.Collections.Generic;
using System.Linq;
using log4net;
using Loki.Bot.Pathfinding;
using Loki.Common;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Legacy.Monoliths
{
	public class MonolithCache
	{
		private static readonly ILog Log = Logger.GetLoggerInstanceForType();

		public static int ValidateDistance = 75;

		public bool IsValid { get; private set; }

		public int Id { get; }

		public Vector2i Position { get; }

		public Vector2i WalkablePosition { get; }

		public string MonsterName { get; }

		public string MonsterMetadata { get; }

		public List<DatBaseItemTypeWrapper> Essences { get; }

		public Monolith NetworkObject => LokiPoe.ObjectManager.GetObjectById<Monolith>(Id);

		public bool? Activate { get; set; }

		public MonolithCache(Monolith monolith)
		{
			Id = monolith.Id;
			Position = monolith.Position;
			WalkablePosition = ExilePather.FastWalkablePositionFor(monolith);
			MonsterName = monolith.Name;
			MonsterMetadata = monolith.MonsterTypeMetadata;
			Essences = monolith.EssenceBaseItemTypes;
			IsValid = true;

			Log.InfoFormat("[MonolithCache] {0} {1} {2} {3} {4}", Id, WalkablePosition, MonsterName, MonsterMetadata,
				string.Join(", ", Essences.Select(e => e.Metadata)));
		}

		public void Update(Monolith monolith)
		{
			// Nothing to do here
		}

		public void Validate()
		{
			if (!IsValid)
				return;

			// If we're in spawning range of the monolith, but it no longer exists or is not a Monolith object, it's no longer valid.
			if (LokiPoe.MyPosition.Distance(Position) <= ValidateDistance)
			{
				var monolith = NetworkObject;
				if (monolith == null)
				{
					Log.InfoFormat("[MonolithCache::Validate] The Monolith [{0}] is no longer valid because it does not exist.", Id);
					IsValid = false;
				}
				else
				{
					if (!monolith.IsTargetable)
					{
						Log.InfoFormat("[MonolithCache::Validate] The Monolith [{0}] is no longer valid because it is not targetable.", Id);
						IsValid = false;
					}
				}
			}
		}
	}
}