   M S B A M L   `   `   `  ���� S  OLegacy, Version=0.4.5619.1753, Culture=neutral, PublicKeyToken=1779d26ece6a7b373clr-namespace:Legacy.MonolithsLegacy.Monoliths      Legacy.Monoliths.Gui   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     4	monolithsclr-namespace:Legacy.Monoliths     .���� $
��Auto=�.���� .@��� $��Settings��.���� (   "Legacy.Monoliths.MonolithsSettings    Instance � DataContext# Z  .+��  � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � � ColumnDefinitions �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��	Enabled: ��$��3q� ^� ToolTip$J DShould Monolith logic run? If false, Monoliths will be skipped over.���� -    ��Name$ CheckBoxEnabled��$��0��$��1��$��3,5,3,1q�$�� �� d� 	IsChecked ��  �� Path$ Enabled� �� Mode$ TwoWay=��� $��1��$��0��$��Open: ��$��3q�$Z TShould Monoliths be interacted with? If false, the bot will stand near them instead.���� -   $ CheckBoxOpen��$��1��$��1��$��3,5,3,1q�$�� �� �� $
 Open�$ TwoWay=��  �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��Minimal Essences: ��$��3q�$N HHow many essences the monolith should have at minimal (or -1 to ignore).���� -   $��TextBoxMinEssences��$��0��$��1��$��30��$��3q����� $ MinEssences�$ TwoWay=��� $��1��$��0��$��Maximum Essences: ��$��3q�$N HHow many essences the monolith should have at maximum (or -1 to ignore).���� -   $��TextBoxMaxEssences��$��1��$��1��$��30��$��3q����� $ MaxEssences�$ TwoWay=��  �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � �� $
��Auto ��� $��* �.+��� $��0��$��0��$ ��WhitelistEssenceMetadata: ��$��3q�$% The essence metadata whitelist.��&   System.Windows.Controls.DataGrid  $��0��$��1��	   CanUserSortColumns	 .  
   AutoGenerateColumns
 .   J� 	AllowDrop .  ���� *WhitelistEssenceMetadata+   Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ Metadata Pattern��   Width5  /System.Windows.Controls.DataGridLengthConverter$ * 1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *Value+�� $��1��$��0��$ ��WhitelistMonsterMetadata: ��$��3q�$% The monster metadata whitelist.��  $��1��$��1��	 .  
 .   .  ���� *WhitelistMonsterMetadata+   $ Metadata Pattern��$ *  �� *Value+�� $��2��$��0��$ ��BlacklistEssenceMetadata: ��$��3q�$% The essence metadata blacklist.��  $��2��$��1��	 .  
 .   .  ���� *BlacklistEssenceMetadata+   $ Metadata Pattern��$ *  �� *Value+�� $��3��$��0��$ ��BlacklistMonsterMetadata: ��$��3q�$% The monster metadata blacklist.��  $��3��$��1��	 .  
 .   .  ���� *BlacklistMonsterMetadata+   $ Metadata Pattern��$ *  �� *Value+