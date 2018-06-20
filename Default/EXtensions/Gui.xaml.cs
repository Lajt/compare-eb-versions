using System.Windows;
using System.Windows.Controls;

namespace Default.EXtensions
{
    public partial class Gui : UserControl
    {
        public Gui()
        {
            InitializeComponent();
        }

        private void InventoryCurrencyAdd(object sender, RoutedEventArgs e)
        {
            Settings.Instance.InventoryCurrencies.Add(new Settings.InventoryCurrency());
        }

        private void InventoryCurrencyDelete(object sender, RoutedEventArgs e)
        {
            var selected = InventoryCurrencyGrid.SelectedItem as Settings.InventoryCurrency;
            if (selected != null) Settings.Instance.InventoryCurrencies.Remove(selected);
        }
    }
}