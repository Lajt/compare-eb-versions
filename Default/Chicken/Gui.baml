   M S B A M L   `   `   `  ���� T  PDefault, Version=0.4.5619.1753, Culture=neutral, PublicKeyToken=1779d26ece6a7b371clr-namespace:Default.ChickenDefault.Chicken      Default.Chicken.Gui   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     /localclr-namespace:Default.Chicken     U QMahApps.Metro, Version=1.5.0.23, Culture=neutral, PublicKeyToken=f4fb5a3c4d1e5b4f:mahapps,http://metro.mahapps.com/winfx/xaml/controls  Dmc;http://schemas.openxmlformats.org/markup-compatibility/2006  .����    Default.Chicken.Settings    Instance � DataContext# Z  $
��Auto=�$
��Auto=�.���� $��0,5,0,0q� �� Orientation$ Vertical=�.+��  � RowDefinitions �� $
��Auto ��� $
��Auto � � ColumnDefinitions �� $
��Auto ��� $
��Auto �.+��� -    ��Name$ 
HpCheckBox��$��0��$��0��$��Center=�$	��HP%�� d� 	IsChecked ��  �� Path$ 	HpEnabled�*  $MahApps.Metro.Controls.NumericUpDown  $��0��$��1��$��5,0,0,0q���.     Maximum$ 95Q�	   Minimum$	 5Q�{��� 
 �� ElementName$
 
HpCheckBox��$ 	IsChecked�   Value �� $ HpThreshold��� -   $ 
EsCheckBox��$��1��$��0��$��0,5,0,0q�$��Center=�$	��ES%�� �� $ 	EsEnabled�  $��1��$��1��$��5,5,0,0q���.  $ 50Q�$	 0Q�{��� $
 
EsCheckBox��$ 	IsChecked� �� $ EsThreshold��� -   $ OnSightCheckBox��$��0,20,0,0q�$��Chicken on sight�� �� $ OnSightEnabled�&   System.Windows.Controls.DataGrid  -     Name$ MonstersDataGrid��$��0,5,0,0q�$
��Left=�$	��Top=�   AutoGenerateColumns .  $��1q�   GridLinesVisibility$	 All=�{��� $
 OnSightCheckBox��$ 	IsChecked����� $ Monsters� � ContextMenu �� $��12(�.@�m� -   $	��Add��m� -   $��Delete��   Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   MinWidth$	 130Q�   Header$
 Name��1  +System.Windows.Controls.DataGridBoundColumn   Binding �� $
 Name�8  2MahApps.Metro.Controls.DataGridNumericUpDownColumn  $ Range��   Maximum$	 250Q�   Minimum$ 5Q� �� $ Range� �� UpdateSourceTrigger$ PropertyChanged=�4  .System.Windows.Controls.DataGridComboBoxColumn  $ Action��   OnSightActions   ItemsSource# Z    SelectedItemBinding �� $ Action�