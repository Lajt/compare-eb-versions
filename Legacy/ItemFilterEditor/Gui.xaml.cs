using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using log4net;
using Loki.Bot;
using Loki.Common;
using Loki.Game.GameData;

namespace Legacy.ItemFilterEditor
{
	public class MyTextToStringObservableCollectionConverter : IValueConverter
	{
		#region Implementation of IValueConverter

		/// <summary>
		/// Converts a value. 
		/// </summary>
		/// <returns>
		/// A converted value. If the method returns null, the valid null value is used.
		/// </returns>
		/// <param name="value">The value produced by the binding source.</param><param name="targetType">The type of the binding target property.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var lst = value as IList<string>;
			if (lst == null)
				return null;

			return string.Join(Environment.NewLine, lst);
		}

		/// <summary>
		/// Converts a value. 
		/// </summary>
		/// <returns>
		/// A converted value. If the method returns null, the valid null value is used.
		/// </returns>
		/// <param name="value">The value that is produced by the binding target.</param><param name="targetType">The type to convert to.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string s = value.ToString();
			if (targetType == typeof(ObservableCollection<string>))
				return
					new ObservableCollection<string>(s.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
			return new List<string>(s.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
		}

		#endregion
	}

	/// <summary>
	/// Interaction logic for Gui.xaml
	/// </summary>
	public partial class Gui : UserControl
	{
		private static readonly ILog Log = Logger.GetLoggerInstanceForType();

		public Gui()
		{
			InitializeComponent();
			DataContext = ConfigurableItemEvaluator.Instance;
		}

		private Filter Filter => TreeViewCategories.SelectedItem as Filter;

		private Category Category => TreeViewCategories.SelectedItem as Category;

		private void HandleTreeItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			object item = TreeViewCategories.SelectedItem;
			if (item is Filter)
			{
				ItemFilterGrid.Visibility = Visibility.Visible;
				CategoryGrid.Visibility = Visibility.Collapsed;

				CategoryGrid.DataContext = null;
				ItemFilterGrid.DataContext = Filter;

				TreeViewCategories.ContextMenu = TreeViewCategories.Resources["FilterMenu"] as ContextMenu;
			}
			else
			{
				ItemFilterGrid.Visibility = Visibility.Collapsed;
				CategoryGrid.Visibility = Visibility.Visible;


				CategoryGrid.DataContext = Category;
				ItemFilterGrid.DataContext = null;

				TreeViewCategories.ContextMenu = TreeViewCategories.Resources["CategoryMenu"] as ContextMenu;
			}

			TreeViewCategories.UpdateLayout();
		}

		private void HandleRarityChanged(object sender, RoutedEventArgs e)
		{
			var cb = sender as CheckBox;
			if (cb == null)
			{
				return;
			}

			object item = TreeViewCategories.SelectedItem;

			if (item is Filter)
			{
				var filter = item as Filter;

				if (filter.Rarities == null)
				{
					filter.Rarities = new List<Rarity>();
				}

				var cbRarity = (Rarity)((int)(sender as CheckBox).Tag);

				// Unchecked, so remove all rarities.
				if (cb.IsChecked == false)
				{
					filter.Rarities.RemoveAll(r => r == cbRarity);
				}
				else if (!filter.Rarities.Contains(cbRarity))
				{
					filter.Rarities.Add(cbRarity);
				}
			}
		}

		#region Context Menu

		private void AddCategoryHandler(object sender, RoutedEventArgs e)
		{
			ConfigurableItemEvaluator.Instance.Categories.Add(new Category
			{
				Description = "New Category"
			});
		}

		private void DeleteCategoryHandler(object sender, RoutedEventArgs e)
		{
			if ((TreeViewCategories.SelectedItem as Category) == null)
			{
				return;
			}

			ConfigurableItemEvaluator.Instance.Categories.Remove(TreeViewCategories.SelectedItem as Category);
		}

		private void AddFilterHandler(object sender, RoutedEventArgs e)
		{
			var cat = TreeViewCategories.SelectedItem as Category;

			if (cat == null)
			{
				return;
			}

			cat.Filters.Add(new Filter
			{
				Description = "New Filter"
			});
		}

		private void DeleteFilterHandler(object sender, RoutedEventArgs e)
		{
			var filter = Filter;

			if (filter == null)
			{
				return;
			}

			// Now, this is where GUIDs come in *very fucking handy*
			Category category = null;

			// Note: This is an observable collection. Therefor; we can't use LINQ!
			foreach (var cat in ConfigurableItemEvaluator.Instance.Categories)
			{
				if (cat.Filters.Contains(filter))
				{
					category = cat;
					break;
				}
			}

			if (category != null)
			{
				if (MessageBox.Show("Are you sure you want to delete the filter '" + filter.Description + "'?",
					Util.RandomWindowTitle("Delete Filter - Confirm"),
					MessageBoxButton.YesNo,
					MessageBoxImage.Question) ==
				    MessageBoxResult.Yes)
				{
					category.Filters.Remove(filter);
				}
			}
		}

		private void EnableFilterHandler(object sender, RoutedEventArgs e)
		{
			var filter = Filter;

			if (filter == null)
			{
				return;
			}

			filter.Enabled = true;
			TreeViewCategories.UpdateLayout();
		}

		private void DisableFilterHandler(object sender, RoutedEventArgs e)
		{
			var filter = Filter;

			if (filter == null)
			{
				return;
			}

			filter.Enabled = false;
			TreeViewCategories.UpdateLayout();
		}

		#endregion

		#region Save

		private void SaveButtonClick(object sender, RoutedEventArgs e)
		{
			try
			{
				if (!ConfigurableItemEvaluator.Instance.VerifyFilters())
				{
					MessageBox.Show("The filter contains errors. Please correct them first.", Util.RandomWindowTitle("Error"),
						MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				ConfigurableItemEvaluator.Instance.Save(ConfigurableItemEvaluator.DefaultPath);

				ItemEvaluator.Refresh();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), Util.RandomWindowTitle("Exception"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		#endregion

		#region Rarity

		private void HandleRarityCheckboxLoaded(object sender, RoutedEventArgs e)
		{
			var cb = sender as CheckBox;

			if (cb != null)
			{
				var r = (Rarity) (int)cb.Tag;

				if (Filter != null)
				{
					if (Filter.Rarities != null)
					{
						cb.IsChecked = Filter.Rarities.Contains(r);
					}
					else
					{
						// Drew: By default, no rarity if null. This should fix the rarity check boxes from carrying over.
						cb.IsChecked = false;
					}
				}
				else
				{
					// Drew: Need to clear it here too I guess.
					cb.IsChecked = false;
				}
			}
		}

		private void HandleRarityDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			HandleRarityCheckboxLoaded(sender, null);
		}

		#endregion

		/*private void Window_Closing(object sender, CancelEventArgs e)
		{
			try
			{
				if (!_evaluator.VerifyFilters())
				{
					MessageBox.Show("The filter contains errors. Please correct them first.", Util.RandomWindowTitle("Error"),
						MessageBoxButton.OK, MessageBoxImage.Error);
					e.Cancel = true;
					return;
				}

				_evaluator.Save(ConfigurableItemEvaluator.DefaultPath);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), Util.RandomWindowTitle("Exception"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}*/
	}
}