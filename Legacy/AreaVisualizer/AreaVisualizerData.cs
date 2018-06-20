using Loki.Common;
using Loki.Game;

namespace Legacy.AreaVisualizer
{
	public class AreaVisualizerData
	{
		public bool IsValid { get; set; }

		public bool IsInGame { get; private set; }

		public uint Seed { get; private set; }

		public Vector2i MyPos { get; private set; }

		public Vector2 MyWorldPos { get; private set; }

		public CachedTerrainData CachedTerrainData { get; private set; }

		public bool ForceReload { get; set; }

		public void Update()
		{
			IsInGame = LokiPoe.IsInGame;

			if (IsInGame)
			{
				Seed = LokiPoe.LocalData.AreaHash;

				MyPos = LokiPoe.MyPosition;
				MyWorldPos = LokiPoe.MyWorldPosition;
				CachedTerrainData = LokiPoe.TerrainData.Cache;
			}
			else
			{
				Seed = 0;
			}

			IsValid = true;

			//ForceReload = false; // If we set this to false, we'll never be able to process it when set
		}
	}
}
