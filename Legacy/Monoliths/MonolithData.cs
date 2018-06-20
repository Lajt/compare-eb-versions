using System.Collections.Generic;
using System.Linq;
using Loki.Bot;
using Loki.Game;
using Loki.Game.Objects;

namespace Legacy.Monoliths
{
	public class MonolithData : AreaData
	{
		private readonly Dictionary<int, MonolithCache> _monoliths = new Dictionary<int, MonolithCache>();

		public MonolithData(uint hash)
			: base(hash)
		{
		}

		public override void Start(bool isActive)
		{
			// Reset the cached settings check, in case the user updated settings.
			// We run this whether or not we're in the area the data exists.
			foreach (var kvp in _monoliths)
			{
				kvp.Value.Activate = null;
			}

			// If we're not in the current area, don't do anything else for now.
			if (!isActive)
				return;

			// TODO: Any Start logic
		}

		public override void Tick(bool isActive)
		{
			// If we're not in the current area, don't do anything for now.
			if (!isActive)
				return;

			// Update currently seen monoliths, but not the mini ones.
			var monoliths = LokiPoe.ObjectManager.GetObjectsByType<Monolith>().Where(m => !m.IsMini).ToList();
			foreach (var monolith in monoliths)
			{
				MonolithCache cache;
				if (!_monoliths.TryGetValue(monolith.Id, out cache))
				{
					cache = new MonolithCache(monolith);
					_monoliths.Add(monolith.Id, cache);
				}
				cache.Update(monolith);
			}

			// Validate all cached monoliths.
			foreach (var kvp in _monoliths)
			{
				kvp.Value.Validate();
			}
		}

		public override void Stop(bool isActive)
		{
			// If we're not in the current area, don't do anything for now.
			if (!isActive)
				return;

			// TODO: Any Stop logic
		}

		/// <summary>
		/// Returns a list of currently cached monoliths.
		/// </summary>
		public IEnumerable<MonolithCache> Monoliths => _monoliths.Select(kvp => kvp.Value).ToList();
	}
}