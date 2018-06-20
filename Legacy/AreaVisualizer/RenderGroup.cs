using System;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace Legacy.AreaVisualizer
{
	public abstract class RenderGroup
	{
		private bool _enabled;

		public RenderGroup(HelixViewport3D viewport)
		{
			View = viewport;
		}

		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				if (_enabled != value)
				{
					if (value)
					{
						AddChild(Visual);
					}
					else
					{
						RemoveChild(Visual);
					}

					_enabled = value;
				}
			}
		}

		public HelixViewport3D View { get; private set; }

		public Visual3D Visual { get; protected set; }

		public abstract void Render(AreaVisualizerData data);

		protected void AddChild(Visual3D vis)
		{
			if (vis == null)
			{
				return;
			}

			if (!View.CheckAccess())
			{
				View.Dispatcher.BeginInvoke(new Action<Visual3D>(AddChild), vis);
				return;
			}

			if (View.Children.Contains(vis))
			{
				return;
			}

			View.Children.Add(vis);
		}

		protected void RemoveChild(Visual3D vis)
		{
			if (vis == null)
			{
				return;
			}

			if (!View.CheckAccess())
			{
				View.Dispatcher.BeginInvoke(new Action<Visual3D>(RemoveChild), vis);
				return;
			}

			if (View.Children.Contains(vis))
			{
				View.Children.Remove(vis);
			}
		}

		protected void ReplaceChild(Visual3D vis)
		{
			if (vis == null)
			{
				return;
			}

			if (!View.CheckAccess())
			{
				View.Dispatcher.BeginInvoke(new Action<Visual3D>(ReplaceChild), vis);
				return;
			}


			if (View.Children.Contains(vis))
			{
				View.Children.Remove(vis);
			}
			View.Children.Add(vis);
		}
	}
}