using System.Windows.Controls;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace Legacy.AreaVisualizer
{
	class RenderLocalPlayer : RenderGroup
	{
		private TextBlock _playerLocationText;
		private TextBlock _cameraPositionText;
		private TextBlock _cameraLookDirectionText;
		private AreaVisualizerData _curData;
		private GeometryModel3D _model;
		private bool _lockCamera;

		public RenderLocalPlayer(HelixViewport3D viewport, TextBlock playerLocationText, TextBlock cameraPositionText, TextBlock cameraLookDirectionText) : base(viewport)
		{
			MeshBuilder mb = new MeshBuilder();
			mb.AddSphere(new Point3D(), 1.5);

			_model = new GeometryModel3D(mb.ToMesh(true), Materials.Red);
			Visual = new MeshVisual3D
			{
				Content = _model
			};

			_playerLocationText = playerLocationText;
			_cameraPositionText = cameraPositionText;
			_cameraLookDirectionText = cameraLookDirectionText;
		}

		public bool LockCamera
		{
			get
			{
				return _lockCamera;
			}
			set
			{
				if (_lockCamera != value)
				{
					if (!value)
					{
						View.Camera.Reset();
						View.ZoomExtents();
					}
					_lockCamera = value;
				}
			}
		}

		#region Overrides of RenderGroup

		public override void Render(AreaVisualizerData data)
		{
			_curData = data;

			var mpos = _curData.MyPos;
			_model.Transform = new TranslateTransform3D(new Vector3D(mpos.X, mpos.Y, 0));

			var cam = View.Camera as PerspectiveCamera;

			// While we're here, lets lock the camera. :)
			if (LockCamera)
			{
				cam.Position = new Point3D(mpos.X, mpos.Y, cam.Position.Z);
				cam.LookDirection = new Vector3D(0, 0, -cam.Position.Z);
			}

			_playerLocationText.Dispatcher.BeginInvoke(new System.Action(() =>
			{
				_playerLocationText.Text = "Player Location: " + mpos.X + ", " + mpos.Y;
			}));

			_cameraPositionText.Dispatcher.BeginInvoke(new System.Action(() =>
			{
				_cameraPositionText.Text = "Camera Position: " + cam.Position.X.ToString("0.00") + ", " + cam.Position.Y.ToString("0.00") + ", " + cam.Position.Z.ToString("0.00");
			}));

			_cameraLookDirectionText.Dispatcher.BeginInvoke(new System.Action(() =>
			{
				_cameraLookDirectionText.Text = "Camera Look Direction: " + cam.LookDirection.X.ToString("0.00") + ", " + cam.LookDirection.Y.ToString("0.00") + ", " + cam.LookDirection.Z.ToString("0.00");
			}));
		}

		#endregion
	}
}