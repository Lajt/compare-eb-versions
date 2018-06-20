using System.Collections.Generic;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using Loki.Bot.Pathfinding;
using Loki.Bot.Pathfinding.RD;
using Loki.Game;

namespace Legacy.AreaVisualizer
{
	public class RenderNavGrid : RenderGroup
	{
		private uint _initialSeed;
		private AreaVisualizerData _curData;

		public RenderNavGrid(HelixViewport3D viewport) : base(viewport)
		{
			Visual = new MeshVisual3D();
		}

		private void CreateVisual()
		{
			_initialSeed = _curData.Seed;

			if (_curData.IsInGame)
			{
				var builder = new MeshBuilder(true, false);

				List<Tripper.Tools.Math.Vector3> verts;
				List<int> indices;

				if(!RDPathfinder.UseNewGetTris)
				{
					RDPathfinder.GetTris(_curData.CachedTerrainData, 2, out verts, out indices); // 2 is the walkable value used for pathfinding
				}
				else
				{
					RDPathfinder.GetTris2(_curData.CachedTerrainData, 2, out verts, out indices, out var rects); // 2 is the walkaloe value used for pathfinding
				}

				for (int i = 0; i < indices.Count - 3; i += 3)
				{
					var p0 = verts[indices[i + 2]];
					var p1 = verts[indices[i + 1]];
					var p2 = verts[indices[i + 0]];

					// We need to swap Y/Z for display purposes.
					builder.AddTriangle(new Point3D(p0.X, p0.Z, p0.Y), new Point3D(p1.X, p1.Z, p1.Y), new Point3D(p2.X, p2.Z, p2.Y));
				}

				LokiPoe.BeginDispatchIfNecessary(View.Dispatcher,
					() => (Visual as MeshVisual3D).Content = new GeometryModel3D(builder.ToMesh(true), Materials.White));
			}
			else
			{
				LokiPoe.BeginDispatchIfNecessary(View.Dispatcher, () => (Visual as MeshVisual3D).Content = null);
			}
		}

		#region Overrides of RenderGroup

		public override void Render(AreaVisualizerData data)
		{
			var polyPathfinder = ExilePather.PolyPathfinder;
			if (polyPathfinder == null)
				return;

			_curData = data;

			// Detect mesh updates by checking if the mesh is generated, and the seed it was generated on.
			if (polyPathfinder.AreaGenerated && polyPathfinder.GeneratedAreaHash == _curData.Seed)
			{
				if ((Visual as MeshVisual3D).Content == null || _initialSeed != _curData.Seed)
				{
					CreateVisual();
					//AddChild(Visual);
				}
			}
		}

		#endregion
	}
}