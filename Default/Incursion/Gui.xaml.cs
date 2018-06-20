using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Default.Incursion
{
    public partial class Gui : UserControl
    {
        public Gui()
        {
            InitializeComponent();
        }

        private void DataGridUnselectAll(object sender, SelectionChangedEventArgs e)
        {
            ((DataGrid) sender).UnselectAll();
        }

        public class InvertBoolConverter : IValueConverter
        {
            public static readonly InvertBoolConverter Instance = new InvertBoolConverter();

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                bool booleanValue = (bool) value;
                return !booleanValue;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                bool booleanValue = (bool) value;
                return !booleanValue;
            }
        }
    }
}