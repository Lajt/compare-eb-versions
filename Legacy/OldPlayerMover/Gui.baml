   M S B A M L   `   `   `  ���� S  OLegacy, Version=0.4.5619.1753, Culture=neutral, PublicKeyToken=1779d26ece6a7b37=#clr-namespace:Legacy.OldPlayerMoverLegacy.OldPlayerMover      Legacy.OldPlayerMover.Gui   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     >OldPlayerMover#clr-namespace:Legacy.OldPlayerMover     U QMahApps.Metro, Version=1.5.0.23, Culture=neutral, PublicKeyToken=f4fb5a3c4d1e5b4f:mahapps,http://metro.mahapps.com/winfx/xaml/controls  .���� $
��Auto=�.���� 2   ,Legacy.OldPlayerMover.OldPlayerMoverSettings    Instance � DataContext# Z  .+��  � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � � ColumnDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��AvoidWallHugging: ��$��3q� ^� ToolTip$  ���� -    ��Name$ CheckBoxAvoidWallHugging��$��0��$��1��$��3,5,3,1q�$�� �� d� 	IsChecked ��  �� Path$ AvoidWallHugging� �� Mode$ TwoWay=��� $��1��$��0��$��MoveRange: ��$��3,5,3,1q�$  ��*  $MahApps.Metro.Controls.NumericUpDown  $��1��$��1��$��3q�	   Value	 �� $ 	MoveRange�$ TwoWay=��� $��2��$��0��$��SingleUseDistance: ��$��3,5,3,1q�$  ��  $��2��$��1��$��3q�	 �� $ SingleUseDistance�$ TwoWay=��� $��3��$��0��$��DebugInputApi: ��$��3q�$  ���� -   $ CheckBoxDebugInputApi��$��3��$��1��$��3,5,3,1q�$�� �� �� $ DebugInputApi�$ TwoWay=��� $��4��$��0��$��UseMouseSmoothing: ��$��3q�$  ���� -   $ CheckBoxUseMouseSmoothing��$��4��$��1��$��3,5,3,1q�$�� �� �� $ UseMouseSmoothing�$ TwoWay=��� $��4��$��2��$��- EXPERIMENTAL -��$��3q�$  ���� $��5��$��0��$��MouseSmoothDistance: ��$��3,5,3,1q�$  ��  $��5��$��1��$��3q�	 �� $ MouseSmoothDistance�$ TwoWay=�{��� $ UseMouseSmoothing�$ OneWay=��� $��6��$��0��$��PathRefreshRateMs: ��$��3,5,3,1q�$  ��  $��6��$��1��$��3q�	 �� $ PathRefreshRateMs�$ TwoWay=��� $��7��$��0��$��DebugAdjustments: ��$��3q�$  ���� -   $ CheckBoxDebugAdjustments��$��7��$��1��$��3,5,3,1q�$�� �� �� $ DebugAdjustments�$ TwoWay=��� $��8��$��0��$��ForceAdjustCombatAreas: ��$��3q�$  ���� -   $$ CheckBoxForceAdjustCombatAreas��$��8��$��1��$��3,5,3,1q�$�� �� �� $ ForceAdjustCombatAreas�$ TwoWay=�� .+�&   System.Windows.Controls.DataGrid  -   
  Name$#
 DataGridForcedAdjustmentAreas��   CanUserReorderColumns .     CanUserSortColumns .     AutoGenerateColumns .   J� 	AllowDrop .  $
��Auto��$
��Auto������ *ForcedAdjustmentAreas+   Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ 	Area Name��   Width5  /System.Windows.Controls.DataGridLengthConverter$
 Auto 1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *Value+