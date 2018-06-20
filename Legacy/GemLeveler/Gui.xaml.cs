using System.Windows;
using System.Windows.Controls;
using log4net;
using Loki.Common;

namespace Legacy.GemLeveler
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
		}

		private void RefreshSkillsButton_OnClick(object sender, RoutedEventArgs e)
		{
			GemLevelerSettings.Instance.RefreshSkillGemsList();
		}

		private void AddGlobalNameIgnoreButton_OnClick(object sender, RoutedEventArgs e)
		{
			var text = GlobalNameIgnoreTextBox.Text;
			if (string.IsNullOrEmpty(text))
				return;
			if (!GemLevelerSettings.Instance.GlobalNameIgnoreList.Contains(text))
			{
				GemLevelerSettings.Instance.GlobalNameIgnoreList.Add(text);
				GemLevelerSettings.Instance.UpdateGlobalNameIgnoreList();
				GlobalNameIgnoreTextBox.Text = "";
			}
			else
			{
				Log.ErrorFormat(
					"[AddGlobalNameIgnoreButtonOnClick] The skillgem {0} is already in the GlobalNameIgnoreList.", text);
			}
		}

		private void RemoveGlobalNameIgnoreButton_OnClick(object sender, RoutedEventArgs e)
		{
			var text = GlobalNameIgnoreTextBox.Text;
			if (string.IsNullOrEmpty(text))
				return;
			if (GemLevelerSettings.Instance.GlobalNameIgnoreList.Contains(text))
			{
				GemLevelerSettings.Instance.GlobalNameIgnoreList.Remove(text);
				GemLevelerSettings.Instance.UpdateGlobalNameIgnoreList();
				GlobalNameIgnoreTextBox.Text = "";
			}
			else
			{
				Log.ErrorFormat("[RemoveGlobalNameIgnoreButtonOnClick] The skillgem {0} is not in the GlobalNameIgnoreList.", text);
			}
		}

		private void AddSkillGemButton_OnClick(object sender, RoutedEventArgs e)
		{
			if (AllSkillGemsComboBox.SelectedIndex == -1)
				return;

			var str = AllSkillGemsComboBox.SelectedValue.ToString();

			if (string.IsNullOrEmpty(str))
				return;

			if (GemLevelerSettings.Instance.SkillGemsToLevelList.Contains(str))
				return;

			GemLevelerSettings.Instance.SkillGemsToLevelList.Add(str);
		}

		private void RemoveSkillGemButton_OnClick(object sender, RoutedEventArgs e)
		{
			if (AllSkillGemsComboBox.SelectedIndex == -1)
				return;

			var str = AllSkillGemsComboBox.SelectedValue.ToString();

			if (string.IsNullOrEmpty(str))
				return;

			if (!GemLevelerSettings.Instance.SkillGemsToLevelList.Contains(str))
				return;

			GemLevelerSettings.Instance.SkillGemsToLevelList.Remove(str);
		}

		private void AddAllSkillGemButton_OnClick(object sender, RoutedEventArgs e)
		{
			foreach (var item in AllSkillGemsComboBox.Items)
			{
				var str = item.ToString();

				if (string.IsNullOrEmpty(str))
					continue;

				if (GemLevelerSettings.Instance.SkillGemsToLevelList.Contains(str))
					continue;

				GemLevelerSettings.Instance.SkillGemsToLevelList.Add(str);
			}
		}

		private void RemoveAllSkillGemButton_OnClick(object sender, RoutedEventArgs e)
		{
			foreach (var item in AllSkillGemsComboBox.Items)
			{
				var str = item.ToString();

				if (string.IsNullOrEmpty(str))
					continue;

				if (!GemLevelerSettings.Instance.SkillGemsToLevelList.Contains(str))
					continue;

				GemLevelerSettings.Instance.SkillGemsToLevelList.Remove(str);
			}
		}
	}
}