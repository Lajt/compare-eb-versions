   M S B A M L   `   `   `  ���� T  PDefault, Version=0.4.5619.1753, Culture=neutral, PublicKeyToken=1779d26ece6a7b377 clr-namespace:Default.EXtensionsDefault.EXtensions      Default.EXtensions.Gui   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     7
extensions clr-namespace:Default.EXtensions     U QMahApps.Metro, Version=1.5.0.23, Culture=neutral, PublicKeyToken=f4fb5a3c4d1e5b4f:mahapps,http://metro.mahapps.com/winfx/xaml/controls  Dmc;http://schemas.openxmlformats.org/markup-compatibility/2006  .���� !   Default.EXtensions.Settings    Instance � DataContext# Z  .@��� $��1q�$��Stashing��.���� $
��Auto=�$
��Auto=�.���� $��0,5,0,0q�$
��Left=�$	��Top=� �� Orientation$ Vertical=�.+���  � IsSharedSizeScope . $ Vertical=�.+��� ����  �� Path$ GeneralStashingRules����� .R��  � RowDefinitions �� $
��Auto � � ColumnDefinitions �� $
��Auto � z� SharedSizeGroup$ Column1���� $
��Auto �$ Column2�� � 	Resources 	 �� 
TargetType
 �� Property%   Default.EXtensions.RarityColors   Normal �� Value �� Triggers2   ,Default.EXtensions.Settings+StashingCategory   Rare �� Value �� Binding   Rare   Unique   Unique
   Gem
   Gem   Card   Card   Prophecy   Prophecy   Essence   Currency   Currency��%�  '��       �� "	 ��.��� 
 � ��# Z  �� # Z  �� $
 Name�.a��� 
 � ��# Z �� # Z  �� $
 Name�.a��� 
 � ��# Z �� # Z  �� $
 Name�.a��� 
 � ��# Z �� # Z  �� $
 Name�.a��� 
 � ��# Z �� # Z  �� $
 Name�.a��� 
 � ��# Z �� # Z  �� $
 Name�.a��� 
 � ��# Z �� # Z  �� $
 Name�.a��� 
 � ��# Z .+��� $��0��$��0��$��0,3,0,0q�$
��Left=�$	��Top=����� $
 Name��� $��0��$��1��$��5,3,0,0q�$
��Left=�$	��Top=����� $
 Tabs� �� Mode$ TwoWay=�9� $��0,10,0,0q�$��Individual currency��.���� $��0,5,0,0q� . $ Vertical=�.+��� ���� $ CurrencyStashingRules����� .R��  �� $
��Auto � �� $
��Auto �$ Column1���� $
��Auto �$ Column2��.+��� $��0��$��0��$
��Left=�$	��Top=�#��Z ���� $
 Name� d� 	IsChecked �� $ Enabled��� $��0��$��1��$��5,0,0,0q�$
��Left=�$	��Top=�{��� $ Enabled����� $
 Tabs�$ TwoWay=��� $��1q�$��	Vendoring��.���� $
��Auto=�$
��Auto=�.���� $
��Left=�$	��Top=�$ 
Horizontal=�.+��� $��Currency exchange��.���� $��0,5,0,0q�$ Vertical=�.+��� $ 
Horizontal=�.+��� $��Exchange act���� $��5,0,0,0q�   CurrencyExchangeActs#��Z   �� SelectedItem  �� $ CurrencyExchangeAct��� $��0,10,0,0q��� $��0,10,0,0q�$$��Transmutation → Augmentation�� �� $ TransExchange.Enabled��  �� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��0,5,0,0q�$��Center=�$	��Min��{��� $ TransExchange.Enabled�*  $MahApps.Metro.Controls.NumericUpDown  $��0��$��1��$��5,5,0,0q�!   Minimum$! 4Q�{��� $ TransExchange.Enabled�"   Value" �� $ TransExchange.Min��� $��1��$��0��$��0,5,0,0q�$��Center=�$
��Save��{��� $ TransExchange.Enabled�  $��1��$��1��$��5,5,0,0q�$! 0Q�{��� $ TransExchange.Enabled�" �� $ TransExchange.Save��� $��0,10,0,0q��� $��0,10,0,0q�$!��Augmentation → Alteration�� �� $ AugsExchange.Enabled��  �� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��0,5,0,0q�$��Center=�$	��Min��{��� $ AugsExchange.Enabled�  $��0��$��1��$��5,5,0,0q�$! 4Q�{��� $ AugsExchange.Enabled�" �� $ AugsExchange.Min��� $��1��$��0��$��0,5,0,0q�$��Center=�$
��Save��{��� $ AugsExchange.Enabled�  $��1��$��1��$��5,5,0,0q�$! 0Q�{��� $ AugsExchange.Enabled�" �� $ AugsExchange.Save��� $��0,10,0,0q��� $��0,10,0,0q�$��Alteration → Jeweller�� �� $ AltsExchange.Enabled��  �� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��0,5,0,0q�$��Center=�$	��Min��{��� $ AltsExchange.Enabled�  $��0��$��1��$��5,5,0,0q�$! 2Q�{��� $ AltsExchange.Enabled�" �� $ AltsExchange.Min��� $��1��$��0��$��0,5,0,0q�$��Center=�$
��Save��{��� $ AltsExchange.Enabled�  $��1��$��1��$��5,5,0,0q�$! 0Q�{��� $ AltsExchange.Enabled�" �� $ AltsExchange.Save��� $��0,10,0,0q��� $��0,10,0,0q�$��Jeweller → Fusing�� �� $ JewsExchange.Enabled��  �� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��0,5,0,0q�$��Center=�$	��Min��{��� $ JewsExchange.Enabled�  $��0��$��1��$��5,5,0,0q�$! 2Q�{��� $ JewsExchange.Enabled�" �� $ JewsExchange.Min��� $��1��$��0��$��0,5,0,0q�$��Center=�$
��Save��{��� $ JewsExchange.Enabled�  $��1��$��1��$��5,5,0,0q�$! 0Q�{��� $ JewsExchange.Enabled�" �� $ JewsExchange.Save��� $��0,10,0,0q��� $��0,10,0,0q�$��Chance → Scouring�� �� $ ChancesExchange.Enabled��  �� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��0,5,0,0q�$��Center=�$	��Min��{��� $ ChancesExchange.Enabled�  $��0��$��1��$��5,5,0,0q�$! 2Q�{��� $ ChancesExchange.Enabled�" �� $ ChancesExchange.Min��� $��1��$��0��$��0,5,0,0q�$��Center=�$
��Save��{��� $ ChancesExchange.Enabled�  $��1��$��1��$��5,5,0,0q�$! 0Q�{��� $ ChancesExchange.Enabled�" �� $ ChancesExchange.Save��� $��0,10,0,0q��� $��0,10,0,0q�$��Scouring → Regret�� �� $ ScoursExchange.Enabled��  �� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��0,5,0,0q�$��Center=�$	��Min��{��� $ ScoursExchange.Enabled�  $��0��$��1��$��5,5,0,0q�$! 4Q�{��� $ ScoursExchange.Enabled�" �� $ ScoursExchange.Min��� $��1��$��0��$��0,5,0,0q�$��Center=�$
��Save��{��� $ ScoursExchange.Enabled�  $��1��$��1��$��5,5,0,0q�$! 0Q�{��� $ ScoursExchange.Enabled�" �� $ ScoursExchange.Save���  . $ Vertical=�.+��� $��Divination card exchange��.��� $��0,5,0,0q� �� $
��Auto ��� $
��Auto ��� $
��Auto � �� $
��Auto �$ CardGcp���� $
��Auto �.+��� -   # ��Name$# CardsEnabledCheckbox��$��0��$��0��$��2��$��Enabled�� �� $ CardsEnabled��� $��1��$��0��$��0,5,0,0q�$��Center=�$��Min sets to go��{��� $ �� ElementName$$ CardsEnabledCheckbox��$ 	IsChecked�  $��1��$��1��$��5,5,0,0q�%   Maximum$% 30Q�$! 1Q�{��� $$ CardsEnabledCheckbox��$ 	IsChecked�" �� $ MinCardSets��� $��2��$��0��$��0,5,0,0q�$��Center=�$��Max sets per run��{��� $$ CardsEnabledCheckbox��$ 	IsChecked�  $��2��$��1��$��5,5,0,0q�$% 30Q�$! 1Q�{��� $$ CardsEnabledCheckbox��$ 	IsChecked�" �� $ MaxCardSets��� $��
GCP recipe��.��� $��0,5,0,0q� �� $
��Auto ��� $
��Auto ��� $
��Auto � �� $
��Auto �$ CardGcp���� $
��Auto �.+��� -   $# GcpEnabledCheckbox��$��0��$��0��$��2��$��Enabled�� �� $ 
GcpEnabled��� $��1��$��0��$��0,5,0,0q�$��Center=�$��Max quality��{��� $$ GcpEnabledCheckbox��$ 	IsChecked�  $��1��$��1��$��5,5,0,0q�$% 23Q�$! 1Q�{��� $$ GcpEnabledCheckbox��$ 	IsChecked�" �� $ GcpMaxQ��� $��2��$��0��$��0,5,0,0q�$��Center=�$��	Max level��{��� $$ GcpEnabledCheckbox��$ 	IsChecked�  $��2��$��1��$��5,5,0,0q�$% 21Q�$! 1Q�{��� $$ GcpEnabledCheckbox��$ 	IsChecked�" �� $ 	GcpMaxLvl��� $��1q�$��Chests & Shrines��.���� $
��Auto=�$
��Auto=�.���� $��5,5,0,0q�$
��Left=�$	��Top=�$ Vertical=�.+��  �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��0,5,0,0q�$��Center=�$��Chest open range��  $��0��$��1��$��5,5,0,0q���.  $	% 200Q�$! -1Q�" �� $ ChestOpenRange��� $��1��$��0��$��0,5,0,0q�$��Center=�$��Strongbox open range��  $��1��$��1��$��5,5,0,0q���.  $	% 200Q�$! -1Q�" �� $ StrongboxOpenRange��� $��2��$��0��$��0,5,0,0q�$��Center=�$��Shrine open range��  $��2��$��1��$��5,5,0,0q���.  $	% 200Q�$! -1Q�" �� $ ShrineOpenRange��� $��3��$��0��$��0,5,0,0q�$��Center=�$��Max strongbox rarity���� $��3��$��1��$��5,5,0,0q���.  &   Rarities#��Z&   �� $ MaxStrongboxRarity��� $��0,15,0,0q�$ 
Horizontal=�.+�9� $��Chests��.���� ���� $ Chests����� .R��� .+��� $��0,5,0,0q����� $
 Name� �� $ Enabled�9� $��10,0,0,0q�$��Strongboxes��.���� ���� $ Strongboxes����� .R��� .+��� $��0,5,0,0q����� $
 Name� �� $ Enabled�9� $��10,0,0,0q�$��Shrines��.���� ���� $ Shrines����� .R��� .+��� $��0,5,0,0q����� $
 Name� �� $ Enabled��� $��1q�$��Stuck detection��.���� $
��Auto=�$
��Auto=�.��� $��0,5,0,0q�$
��Left=�$	��Top=� �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� -   $# SdEnabledCheckbox��$��0��$��0��$��Enabled�� �� $ StuckDetectionEnabled��� $��1��$��0��$��0,10,0,0q�$��Center=�$��Max stucks per instance��{��� $$ SdEnabledCheckbox��$ 	IsChecked�  $��1��$��1��$��5,10,0,0q�$! 1Q�{��� $$ SdEnabledCheckbox��$ 	IsChecked�" �� $ MaxStucksPerInstance��� $��2��$��0��$��0,5,0,0q�$��Center=�$!��Max stuck count small range��{��� $$ SdEnabledCheckbox��$ 	IsChecked�  $��2��$��1��$��5,5,0,0q�$! 1Q�{��� $$ SdEnabledCheckbox��$ 	IsChecked�" �� $ MaxStuckCountSmall��� $��3��$��0��$��0,5,0,0q�$��Center=�$"��Max stuck count medium range��{��� $$ SdEnabledCheckbox��$ 	IsChecked�  $��3��$��1��$��5,5,0,0q�$! 1Q�{��� $$ SdEnabledCheckbox��$ 	IsChecked�" �� $ MaxStuckCountMedium��� $��4��$��0��$��0,5,0,0q�$��Center=�$ ��Max stuck count long range��{��� $$ SdEnabledCheckbox��$ 	IsChecked�  $��4��$��1��$��5,5,0,0q�$! 1Q�{��� $$ SdEnabledCheckbox��$ 	IsChecked�" �� $ MaxStuckCountLong��� $��1q�$
��Misc��.���� $
��Auto=�$
��Auto=�.���� $��0,5,0,0q�$
��Left=�$	��Top=�$ Vertical=�.+��� $ ��Sell excess Portal Scrolls�� �� $ SellExcessPortals��� $��0,10,0,0q�$��Loot all visible items�� �� $ LootVisibleItems��� $��0,10,0,0q�$'��!Use chat command to enter hideout�� �� $ UseChatForHideout��� $��0,10,0,0q�$ 
Horizontal=�.+��� -   $# DelaysCheckBox��$	��Top=�$��Artificial delays (ms)�� �� $ ArtificialDelays��� $ Vertical=�    Bvc��%   &	        �� .+�(  "MahApps.Metro.Controls.RangeSlider  -   '  Name$' DelaysBounds��$��95��$��5,0,0,0q�(   IsSnapToTickEnabled( . ) � Maximum$
) 1000Q�*   MinRange$* 50Q�+   MinRangeWidth$+ 0Q�, � Minimum$, 50Q�-   TickFrequency$- 50Q�{��� $$ DelaysCheckBox��$ 	IsChecked�.   
LowerValue. �� $ MinArtificialDelay�/   
UpperValue/ �� $ MaxArtificialDelay��� $��Center=�$��10(�y��� $$ DelaysCheckBox��$ 	IsChecked�0 �� 	Converter#0 [  ��`� 1 �� StringFormat$1 	{0} - {1}��.1��� $$ DelaysBounds��$ 
LowerValue��� $$ DelaysBounds��$ 
UpperValue��� $��0,10,0,0q�$ 
Horizontal=�.+��� $��Center=�$#��Inventory squares for townrun��  $��5,0,0,0q�$% 60Q�$! 0Q�" �� $ MinInventorySquares��� $��0,20,0,0q�$��Inventory currency��&   System.Windows.Controls.DataGrid  -   2  Name$2 InventoryCurrencyGrid��$��0,5,0,0q�$
��Left=�$	��Top=�3   AutoGenerateColumns3 .  $��1q�4   GridLinesVisibility$	4 All=����� $ InventoryCurrencies�5 � ContextMenu5 �� $��12(�.@�m� -   $	��Add��m� -   $��Delete��6   Columns6 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn7   MinWidth$	7 100Q�8   Header$
8 Name��1	  +System.Windows.Controls.DataGridBoundColumn9 	  Binding9 �� $
 Name�8
  2MahApps.Metro.Controls.DataGridNumericUpDownColumn
  $	8 Row��: 
  Maximum$: 5Q�; 
  Minimum$; -1Q�9 �� $	 Row�< �� UpdateSourceTrigger$< PropertyChanged=�
  $8 Column��$: 12Q�$; -1Q�9 �� $ Column�$< PropertyChanged=�
  $8 Restock��$; -1Q�9 �� $ Restock�$< PropertyChanged=�