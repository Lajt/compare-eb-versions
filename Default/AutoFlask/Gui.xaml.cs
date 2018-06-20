using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Default.AutoFlask
{
    public partial class Gui : UserControl
    {
        public Gui()
        {
            InitializeComponent();
        }

        private void AddFlaskTrigger(object sender, RoutedEventArgs e)
        {
            var button = (Button) sender;
            var areas = (ObservableCollection<FlaskTrigger>) button.Tag;
            areas.Add(new FlaskTrigger());
        }

        private void RemoveFlaskTrigger(object sender, RoutedEventArgs e)
        {
            var button = (Button) sender;
            var trigger = (FlaskTrigger) button.DataContext;
            var flask = (Settings.FlaskEntry) button.Tag;
            flask.Triggers.Remove(trigger);
        }

        public class DescriptionConverter : IValueConverter
        {
            public static readonly DescriptionConverter Instance = new DescriptionConverter();

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value == null)
                    return "null";

                var field = value.GetType().GetField(value.ToString());
                foreach (var attrib in field.GetCustomAttributes(false))
                {
                    var desc = attrib as DescriptionAttribute;
                    if (desc != null) return desc.Description;
                }
                return value.ToString();
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public class VisibilityConverter : IValueConverter
        {
            public static readonly VisibilityConverter Instance = new VisibilityConverter();

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var trigger = (TriggerType) value;
                var param = (string) parameter;

                if (trigger == TriggerType.Hp)
                    return param == "Hp" ? Visibility.Visible : Visibility.Collapsed;

                if (trigger == TriggerType.Es)
                    return param == "Es" ? Visibility.Visible : Visibility.Collapsed;

                if (trigger == TriggerType.Mobs)
                    return param == "Mobs" || param == "MobsOrAttack" ? Visibility.Visible : Visibility.Collapsed;

                if (trigger == TriggerType.Attack)
                    return param == "Attack" || param == "MobsOrAttack" ? Visibility.Visible : Visibility.Collapsed;

                return Visibility.Collapsed;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}