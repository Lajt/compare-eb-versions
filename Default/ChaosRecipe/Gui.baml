   M S B A M L   `   `   `  ���� T  PDefault, Version=0.4.5619.1753, Culture=neutral, PublicKeyToken=1779d26ece6a7b379!clr-namespace:Default.ChaosRecipeDefault.ChaosRecipe      Default.ChaosRecipe.Gui   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     3local!clr-namespace:Default.ChaosRecipe     U QMahApps.Metro, Version=1.5.0.23, Culture=neutral, PublicKeyToken=f4fb5a3c4d1e5b4f:mahapps,http://metro.mahapps.com/winfx/xaml/controls  Dmc;http://schemas.openxmlformats.org/markup-compatibility/2006  .���� "   Default.ChaosRecipe.Settings    Instance � DataContext# Z  $
��Auto=�$
��Auto=�.���� $��0,5,0,0q� �� Orientation$ Vertical=�.+��  � RowDefinitions �� $
��Auto ��� $
��Auto � � ColumnDefinitions �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��	Stash tab���� $��0��$��1������  �� Path$ StashTab��� $��1��$��0��$��0,5,0,0q�$��Min iLvl��*  $MahApps.Metro.Controls.NumericUpDown  $��1��$��1��$��0,5,0,0q�$
��Left=�$	��Top=�   Minimum$ 1Q�   Value �� $ MinILvl��� $��0,10,0,0q�$#��Update stash data every Start�� d� 	IsChecked �� $ AlwaysUpdateStashData��� $��0,15,0,0q�$
��Left=�$	��Top=�$��Stash tab item limits��.��� $��0,5,0,0q� �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��Weapons��  $��0��$��1��$ 1Q� �� $ MaxItemCounts[0]��� $��1��$��0��$��0,5,0,0q�$��Body armors��  $��1��$��1��$��0,5,0,0q�$ 1Q� �� $ MaxItemCounts[1]��� $��2��$��0��$��0,5,0,0q�$��Helmets��  $��2��$��1��$��0,5,0,0q�$ 1Q� �� $ MaxItemCounts[2]��� $��3��$��0��$��0,5,0,0q�$��Boots��  $��3��$��1��$��0,5,0,0q�$ 1Q� �� $ MaxItemCounts[3]��� $��4��$��0��$��0,5,0,0q�$��Gloves��  $��4��$��1��$��0,5,0,0q�$ 1Q� �� $ MaxItemCounts[4]��� $��5��$��0��$��0,5,0,0q�$��Belts��  $��5��$��1��$��0,5,0,0q�$ 1Q� �� $ MaxItemCounts[5]��� $��6��$��0��$��0,5,0,0q�$��Amulets��  $��6��$��1��$��0,5,0,0q�$ 1Q� �� $ MaxItemCounts[6]��� $��7��$��0��$��0,5,0,0q�$��Rings��  $��7��$��1��$��0,5,0,0q�$ 1Q� �� $ MaxItemCounts[7]�