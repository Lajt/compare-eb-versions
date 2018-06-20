using System.Windows;
using System.Windows.Controls;

namespace Default.Chicken
{
    public partial class Gui : UserControl
    {
        public Gui()
        {
            InitializeComponent();
        }

        private void MonstersAdd(object sender, RoutedEventArgs e)
        {
            Settings.Instance.Monsters.Add(new Settings.MonsterEntry());
        }

        private void MonstersDelete(object sender, RoutedEventArgs e)
        {
            var selected = MonstersDataGrid.SelectedItem as Settings.MonsterEntry;
            if (selected != null) Settings.Instance.Monsters.Remove(selected);
        }
    }
}