using System;
using System.Windows;
using System.Windows.Controls;
using log4net;
using Loki.Common;
using Loki.Game;

namespace Legacy.PythonExample
{
	/// <summary>
	/// Interaction logic for Gui.xaml
	/// </summary>
	public partial class Gui : UserControl
	{
		private static readonly ILog Log = Logger.GetLoggerInstanceForType();

		private readonly PythonExample _pythonExample;

		internal Gui(PythonExample @this)
		{
			InitializeComponent();

			_pythonExample = @this;
			PyInputTextBox.Text = PythonExampleSettings.Instance.Code;
		}

		internal void ExecutePythonButton_Click(object sender, RoutedEventArgs e)
		{
			PythonExampleSettings.Instance.Code = PyInputTextBox.Text;
			Dispatcher.BeginInvoke(new Action(() =>
			{
				lock (_pythonExample)
				{
					_pythonExample.InitializeScriptManager();

					using (LokiPoe.AcquireFrame())
					{
						// For total control, GetStatement logic is this:
						var scope = _pythonExample.ScriptManager.Scope;
						var scriptSource =
							_pythonExample.ScriptManager.Engine.CreateScriptSourceFromString(PythonExampleSettings.Instance.Code);
						scope.SetVariable("ioproxy", _pythonExample.ScriptManager.IoProxy);
						scriptSource.Execute(scope);
						scope.GetVariable<Action>("Execute")();
					}
				}
			}));
		}

		private void PyInputTextBox_OnLostFocus(object sender, RoutedEventArgs e)
		{
			PythonExampleSettings.Instance.Code = PyInputTextBox.Text;
		}
	}
}