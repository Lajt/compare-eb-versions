using System.Collections.Generic;
using System.Linq;
using Loki.Bot;
using Loki.Game;
using Loki.Game.Objects;

namespace Legacy.Breaches
{
	public class BreachData : AreaData
	{
		private readonly Dictionary<int, BreachCache> _breaches = new Dictionary<int, BreachCache>();

		public BreachData(uint hash)
			: base(hash)
		{
		}

		public override void Start(bool isActive)
		{
			// Reset the cached settings check, in case the user updated settings.
			// We run this whether or not we're in the area the data exists.
			foreach (var kvp in _breaches)
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

			// Update currently seen breaches, but not the mini ones.
			var breaches = LokiPoe.ObjectManager.GetObjectsByType<Breach>().ToList();
			foreach (var breach in breaches)
			{
				BreachCache cache;
				if (!_breaches.TryGetValue(breach.Id, out cache))
				{
					cache = new BreachCache(breach);
					_breaches.Add(breach.Id, cache);
				}
				cache.Update(breach);
			}

			// Validate all cached breaches.
			foreach (var kvp in _breaches)
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
		/// Returns a list of currently cached Breachs.
		/// </summary>
		public IEnumerable<BreachCache> Breaches => _breaches.Select(kvp => kvp.Value).ToList();
	}
}