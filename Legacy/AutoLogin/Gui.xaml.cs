using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using log4net;
using Loki.Common;
using Loki.Game;
using System.Linq;

namespace Legacy.AutoLogin
{
	/// <summary>
	/// Interaction logic for Gui.xaml
	/// </summary>
	public partial class Gui : UserControl
	{
		private static readonly ILog Log = Logger.GetLoggerInstanceForType();

		public Gui()
		{
			InitializeComponent();
			GatewayComboBox.Items.Add("Auto-select Gateway");
			GatewayComboBox.Items.Add("Texas (US)");
			GatewayComboBox.Items.Add("Washington, D.C. (US)");
			GatewayComboBox.Items.Add("California (US)");
			GatewayComboBox.Items.Add("Amsterdam (EU)");
			GatewayComboBox.Items.Add("London (EU)");
			GatewayComboBox.Items.Add("Frankfurt (EU)");
			GatewayComboBox.Items.Add("Milan (EU)");
			GatewayComboBox.Items.Add("Singapore");
			GatewayComboBox.Items.Add("Australia");
			GatewayComboBox.Items.Add("Sao Paulo (BR)");
			GatewayComboBox.Items.Add("Paris (EU)");
			GatewayComboBox.Items.Add("Moscow (RU)");
			GatewayComboBox.Items.Add("Japan"); 
		}

		private void LoadCharactersButton_OnClick(object sender, RoutedEventArgs e)
		{
			CharactersComboBox.Items.Clear();

			var chars = new List<string>();
			using (LokiPoe.AcquireFrame())
			{
				if (LokiPoe.StateManager.IsSelectCharacterStateActive && !LokiPoe.StateManager.IsCreateCharacterStateActive)
				{
					if (LokiPoe.SelectCharacterState.IsCharacterListLoaded)
					{
						foreach (var ch in LokiPoe.SelectCharacterState.Characters.OrderBy(ce => ce.Name))
						{
							chars.Add(ch.Name);
						}
					}
				}
			}

			foreach (var @char in chars)
			{
				CharactersComboBox.Items.Add(@char);
			}

			if (!string.IsNullOrEmpty(AutoLoginSettings.Instance.Character))
			{
				var name = AutoLoginSettings.Instance.Character;
				for (int i = 0; i < CharactersComboBox.Items.Count; i++)
				{
					if (((string) (CharactersComboBox.Items[i])).Equals(name))
					{
						CharactersComboBox.SelectedIndex = i;
						break;
					}
				}
			}
		}

		private void CharactersComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (CharactersComboBox.SelectedIndex == -1)
				return;

			var name = (string) CharactersComboBox.Items[CharactersComboBox.SelectedIndex];
			if (!string.IsNullOrEmpty(name))
			{
				AutoLoginSettings.Instance.Character = name;
			}
		}

		private void GatewayComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (GatewayComboBox.SelectedIndex != -1)
			{
				AutoLoginSettings.Instance.Gateway = GatewayComboBox.SelectedItem.ToString();
			}
		}
	}
}