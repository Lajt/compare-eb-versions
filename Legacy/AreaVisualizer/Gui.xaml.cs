using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using log4net;
using Loki.Bot.Pathfinding;
using Loki.Common;
using Loki.Game;
using Loki.Bot.Pathfinding.RD;

namespace Legacy.AreaVisualizer
{
	/// <summary>
	/// Interaction logic for Gui.xaml
	/// </summary>
	public partial class Gui : UserControl
	{
		private RenderMesh _renderMesh;
		private RenderLocalPlayer _renderLocalPlayer;
		private RenderNavGrid _renderNavGrid;
		private readonly List<RenderGroup> _renderGroups = new List<RenderGroup>();

		private readonly DispatcherTimer _tickTimer;

		private readonly Func<bool> IsEnabledFunc;
		private readonly Func<AreaVisualizerData> GetDataFunc;

		public void OnDeinitialize()
		{
			_tickTimer?.Stop();
		}

		public Gui(Func<bool> isEnabledFunc, Func<AreaVisualizerData> getDataFunc)
		{
			InitializeComponent();

			IsEnabledFunc = isEnabledFunc;
			GetDataFunc = getDataFunc;

			_renderMesh = new RenderMesh(HelixView);
			_renderMesh.Enabled = true;
			_renderGroups.Add(_renderMesh);

			_renderLocalPlayer = new RenderLocalPlayer(HelixView, PlayerLocationText, CameraPositionText, CameraLookDirectionText);
			_renderLocalPlayer.Enabled = true;
			_renderGroups.Add(_renderLocalPlayer);

			_renderNavGrid = new RenderNavGrid(HelixView);
			_renderNavGrid.Enabled = true;
			_renderGroups.Add(_renderNavGrid);

			// This ctor is called from the main gui thread when the control is being added to the Plugins tab.
			// Because of this, we have to track enable/disable state beforehand since they will get invoked before this ctor is called.
			_tickTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Normal, TimerCallback, Dispatcher);

			// When the mesh changes due to obstacles, force reload so the display shows the obstacles correctly
			RDPathfinder.MeshChanged += (sender, args) =>
				Dispatcher.BeginInvoke(new Action(() =>
				{
					GetDataFunc().ForceReload = true;
				}));
		}

		private void TimerCallback(object sender, EventArgs e)
		{
			// Make sure everything is loaded first.
			if (!LokiPoe.IsBotFullyLoaded)
				return;

			if (!IsEnabledFunc())
			{
				return;
			}

			LokiPoe.BeginDispatchIfNecessary(Dispatcher, () =>
			{
				var data = GetDataFunc();
				if (data.IsValid)
				{
					foreach (var renderGroup in _renderGroups)
					{
						renderGroup.Render(data);
					}
					data.IsValid = false;
				}

				_renderNavGrid.Enabled = ShowNavGrid.IsChecked == true;
				_renderLocalPlayer.Enabled = ShowLocalPlayer.IsChecked == true;
				_renderLocalPlayer.LockCamera = LockCameraToPlayer.IsChecked == true;
				_renderMesh.Enabled = ShowNavMesh.IsChecked == true;
			});
		}

		private static readonly ILog Log = Logger.GetLoggerInstanceForType();

		private void ReloadExilePatherButton_OnClick(object sender, RoutedEventArgs e)
		{
			Dispatcher.BeginInvoke(new Action(() =>
			{
				using (LokiPoe.AcquireFrame())
				{
					ExilePather.Reload(true);
				}
				GetDataFunc().ForceReload = true;
				Log.InfoFormat("[ReloadExilePatherButton_OnClick] Done!");
			}));
		}

		private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
		{
			Dispatcher.BeginInvoke(new Action(() =>
			{
				GetDataFunc().ForceReload = true;
			}));
		}
	}
}