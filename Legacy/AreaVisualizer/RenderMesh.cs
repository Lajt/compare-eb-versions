using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using Loki.Bot.Pathfinding;
using Loki.Game;
using Tripper.RecastManaged.Detour;
using System.Diagnostics;

namespace Legacy.AreaVisualizer
{
	public class RenderMesh : RenderGroup
	{
		private uint _initialSeed;
		private AreaVisualizerData _curData;

		public RenderMesh(HelixViewport3D viewport) : base(viewport)
		{
			Visual = new MeshVisual3D();
		}

		private void CreateNavMeshOverlay()
		{
			var vertices = new List<Point3D>();
			var indices = new List<int>();

			NavMesh navMesh = ExilePather.PolyPathfinder.NavMesh;

			int tcount = navMesh.GetMaxTiles();
			for (int i = 0; i < tcount; i++)
			{
				MeshTile tile = navMesh.GetTile(i);
				if (tile.Header == null)
				{
					continue;
				}

				AddMeshTile(tile, vertices, indices);
			}

			var b = new MeshBuilder();
			for (int i = 0; i < indices.Count - 3; i += 3)
			{
				b.AddTriangle(vertices[indices[i + 2]], vertices[indices[i + 1]], vertices[indices[i]]);
			}

			_initialSeed = _curData.Seed;

			LokiPoe.BeginDispatchIfNecessary(View.Dispatcher, () =>
					(Visual as MeshVisual3D).Content =
						new GeometryModel3D(b.ToMesh(true), MaterialHelper.CreateMaterial(Colors.DeepSkyBlue, 0.35)));
		}

		private static void AddMeshTile(MeshTile tile, List<Point3D> vertices, List<int> indices)
		{
			byte[] detailTris = tile.GetAllDetailIndices();
			var verts = tile.GetAllVertices();
			var detailVerts = tile.GetAllDetailVertices();

			var vertMap = new Dictionary<uint, int>();

			for (int i = 0; i < tile.Header.PolyCount; i++)
			{
				Poly p = tile.GetPoly(i);
				if (p.Type == 1) // DT_POLYTYPE_OFFMESH_CONNECTION
				{
					continue;
				}

				PolyDetail pd = tile.GetPolyDetail(i);

				for (int j = 0; j < pd.TriCount; j++)
				{
					for (int k = 0; k < 3; k++)
					{
						int index = detailTris[(pd.TriBase + j)*4 + k];
						uint vertIndex;
						if (index < p.VertCount)
						{
							vertIndex = p.GetVert(index);
						}
						else
						{
							var val = verts.Length + pd.VertBase + index - p.VertCount;
							Debug.Assert(val <= uint.MaxValue);
							vertIndex = (uint)val;
						}

						if (!vertMap.ContainsKey(vertIndex))
						{
							var pos = vertIndex >= verts.Length ? detailVerts[vertIndex - verts.Length] : verts[vertIndex];
							pos.Y += 0.03f;
							vertices.Add(new Point3D(pos.X, pos.Z, pos.Y + 0.5));
							vertMap[vertIndex] = vertices.Count - 1;
						}

						indices.Add(vertMap[vertIndex]);
					}
				}
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
			if (polyPathfinder.AreaGenerated)
			{
				if (polyPathfinder.GeneratedAreaHash == _curData.Seed || _curData.ForceReload)
				{
					if ((Visual as MeshVisual3D).Content == null || _initialSeed != _curData.Seed || _curData.ForceReload)
					{
						_curData.ForceReload = false;
						CreateNavMeshOverlay();
					}
				}
			}
		}

		#endregion
	}
}