   M S B A M L   `   `   `  ���� T  PDefault, Version=0.4.5619.1753, Culture=neutral, PublicKeyToken=1779d26ece6a7b375clr-namespace:Default.IncursionDefault.Incursion      Default.Incursion.Gui   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     1localclr-namespace:Default.Incursion     U QMahApps.Metro, Version=1.5.0.23, Culture=neutral, PublicKeyToken=f4fb5a3c4d1e5b4f:mahapps,http://metro.mahapps.com/winfx/xaml/controls  Dmc;http://schemas.openxmlformats.org/markup-compatibility/2006  .����     Default.Incursion.Settings    Instance � DataContext# Z   � 	Resources    DataGridCheckBoxStyle  MetroCheckBox �� BasedOn �� 
TargetType �� Property �� Value  DataGridComboBoxStyle  MetroComboBox��%r   &	        7  &	 ,     7  �� 8   " ��.���  � ��$ Center=��� 8   " ��.���  � ��$
 Left=�.@��� $��1q�$��
Incursions��.���� $
��Auto=�$��Disabled=�.��� $��0,5,0,0q� � RowDefinitions �� $
��Auto ��� $
��Auto ��� $��* � � ColumnDefinitions �� $
��Auto �.+��� $��0��$3��-Open portals before incursions (only in maps)��	 d� 	IsChecked	 �� 
 �� Path$
 PortalBeforeIncursion��� $��1��$��0,5,0,0q�$:��4Finish area after incursion (QuestBot grinding only)��	 �� $
 LeaveAfterIncursion�&   System.Windows.Controls.DataGrid  -   $��2��$��0,25,0,0q�   AutoGenerateColumns .  $��1q�   CanUserAddRows .     CanUserDeleteRows .     GridLinesVisibility$	 All=����� $
 IncursionRooms�   Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$
 Name��   
IsReadOnly . 1  +System.Windows.Controls.DataGridBoundColumn   Binding �� $

 Name� �� Mode$ OneTime=�4  .System.Windows.Controls.DataGridComboBoxColumn     ElementStyle# [ $ 
Prioritize��   PriorityActions   ItemsSource# Z    SelectedItemBinding �� $
 PriorityAction� �� UpdateSourceTrigger$ PropertyChanged=�4  .System.Windows.Controls.DataGridCheckBoxColumn    MetroDataGridCheckBox   EditingElementStyle# �     ElementStyle# [  $ Never change�� �� $
 NoChange�$ PropertyChanged=�  # �  # [  $ Never upgrade�� �� $
 	NoUpgrade�$ PropertyChanged=��� $��1q�$��Temple of Atzoatl��.���� $
��Auto=�$
��Auto=�.���� $��0,5,0,0q�.+��� -    ��Name$ SkipTempleCheckBox��$��Skip temple��	 �� $
 
SkipTemple��� $��0,5,0,0q�$��Ignore bossroom��	 �� $
 IgnoreBossroom�{���  �� ElementName$ SkipTempleCheckBox��$
 	IsChecked�/   )Default.Incursion.Gui+InvertBoolConverter   Instance �� 	Converter# Z �� $��0,5,0,0q�$��Monster tracking��	 �� $
 TrackMobInTemple�{��� $ SkipTempleCheckBox��$
 	IsChecked�# Z �� $��0,7,0,0q� �� Orientation$ 
Horizontal=�.+�*	  $MahApps.Metro.Controls.NumericUpDown	    	  Maximum$	  100Q�! 	  Minimum$! 10Q�{��� $ SkipTempleCheckBox��$
 	IsChecked�# Z " 	  Value" �� $
 ExplorationPercent��� $��Exploration percent��{��� $ SkipTempleCheckBox��$
 	IsChecked�# Z 