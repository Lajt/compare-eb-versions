   M S B A M L   `   `   `  ���� T  PDefault, Version=0.4.5619.1753, Culture=neutral, PublicKeyToken=1779d26ece6a7b37/clr-namespace:Default.MapBotDefault.MapBot      Default.MapBot.Gui   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     .localclr-namespace:Default.MapBot     U QMahApps.Metro, Version=1.5.0.23, Culture=neutral, PublicKeyToken=f4fb5a3c4d1e5b4f:mahapps,http://metro.mahapps.com/winfx/xaml/controls  Dmc;http://schemas.openxmlformats.org/markup-compatibility/2006  .����   � 	Resources     DataGridCheckBoxStyle  MetroCheckBox �� BasedOn �� 
TargetType �� Property �� Value��%:   &	        7  �� 8   " ��.���  � ��$ Center=�.@��� $��1q�$   Default.MapBot.GeneralSettings   Instance � DataContext# Z $��General��.���� $
��Auto=�$
��Auto=�.���  � RowDefinitions �� $
��Auto ��� $
��Auto � � ColumnDefinitions �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��Global��.���� 	 �� Orientation$	 Vertical=�.+�� $��0,5,0,0q� �� $
��Auto ��� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��Center=�$��Max map tier��*  $MahApps.Metro.Controls.NumericUpDown  $��0��$��1��$��5,0,0,0q���.  
   Maximum$
 16Q�   Minimum$ 1Q�   Value ��  �� Path$ 
MaxMapTier��� $��1��$��0��$��0,5,0,0q�$��Center=�$��Monster remaining��  $��1��$��1��$��5,5,0,0q���.  $
 50Q�$ 0Q� �� $ MobRemaining��� $��2��$��0��$��0,5,0,0q�$��Center=�$��Exploration percent��  $��2��$��1��$��5,5,0,0q���.  $	
 100Q�$ 0Q� �� $ ExplorationPercent��� $��0,10,0,0q�$��Monster tracking�� d� 	IsChecked �� $ TrackMob��� $��0,5,0,0q�$��Fast transition�� �� $ FastTransition��� $��0,5,0,0q�$��Strict monster remaining�� �� $ StrictMobRemaining��� $��0,5,0,0q�$ ��Strict exploration percent�� �� $ StrictExplorationPercent��� $��0��$��1��$��Selling��.���� $	 Vertical=�.+��� -    ��Name$ SellEnabledCheckBox��$��0,5,0,0q�$��Enabled�� �� $ SellEnabled��� $��0,5,0,0q�$��Sell ignored maps�� �� $ SellIgnoredMaps�{���  �� ElementName$ SellEnabledCheckBox��$ 	IsChecked��  �� $
��Auto ��� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��0,5,0,0q�$��Center=�$��Max map tier��{��� $ SellEnabledCheckBox��$ 	IsChecked�  $��0��$��1��$��5,5,0,0q�$
 15Q�$ 1Q�{��� $ SellEnabledCheckBox��$ 	IsChecked� �� $ MaxSellTier��� $��1��$��0��$��0,5,0,0q�$��Center=�$��Max map priority��{��� $ SellEnabledCheckBox��$ 	IsChecked�  $��1��$��1��$��5,5,0,0q�{��� $ SellEnabledCheckBox��$ 	IsChecked� �� $ MaxSellPriority��� $��2��$��0��$��0,5,0,0q�$��Center=�$��Min map amount��{��� $ SellEnabledCheckBox��$ 	IsChecked�  $��2��$��1��$��5,5,0,0q�$ 0Q�{��� $ SellEnabledCheckBox��$ 	IsChecked� �� $ MinMapAmount��� $��1��$��0��$
��Misc��.���� $	 Vertical=�.+��� $��0,5,0,0q�$��Use hideout�� �� $ 
UseHideout��� $��0,5,0,0q�$��Break rhoa nests�� �� $ 
BreakNests��� $��0,5,0,0q�$��Ignore hidden auras�� �� $ IgnoreHiddenAuras��� $��0,5,0,0q�$��Run unid maps�� �� $ RunUnId��� $��0,5,0,0q�$��Stop after current map�� �� $ StopRequested�9� $��0,5,0,0q�$��Trials��.���� $	 Vertical=�.+��� $��0,5,0,0q�$��Piercing Truth�� �� $ Trials.PiercingTruth��� $��0,5,0,0q�$��Swirling Fear�� �� $ Trials.SwirlingFear��� $��0,5,0,0q�$��Crippling Grief�� �� $ Trials.CripplingGrief��� $��0,5,0,0q�$��Burning Rage�� �� $ Trials.BurningRage��� $��0,5,0,0q�$��Lingering Pain�� �� $ Trials.LingeringPain��� $��0,5,0,0q�$��Stinging Doubt�� �� $ Trials.StingingDoubt��� $��1��$��1��$��Atlas��.���� $	 Vertical=�.+�� $��0,5,0,0q� �� $
��Auto ��� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��Center=�$��Shaped maps priority��  $��0��$��1��$��5,0,0,0q���.   �� $ ShapedPriority��� $��1��$��0��$��0,5,0,0q�$��Center=�$��Shaper influence priority��  $��1��$��1��$��5,5,0,0q���.   �� $ ShaperInfluencePriority��� $��2��$��0��$��0,5,0,0q�$��Center=�$��Elder influence priority��  $��2��$��1��$��5,5,0,0q���.   �� $ ElderInfluencePriority��� -   $ AtlasExploreCheckbox��$��0,10,0,0q�$��Atlas exploration mode�� �� $ AtlasExplorationEnabled��� $��1q�# Z $��	Upgrading��.���� $
��Auto=�$
��Auto=�.���  �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��Magic��.��� $��0,5,0,0q� �� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$
��Tier�� � ToolTip$< 6Minimum tier to start using Transmutation Orbs on maps�� �� $ MagicUpgrade.TierEnabled�  $��0��$��1��$��5,0,0,0q�$
 16Q�$ 1Q�{��� $ MagicUpgrade.TierEnabled� �� $ MagicUpgrade.Tier��� $��1��$��0��$��Priority��$@ :Minimum priority to start using Transmutation orbs on maps�� �� $" MagicUpgrade.PriorityEnabled�  $��1��$��1��$��5,0,0,0q�{��� $" MagicUpgrade.PriorityEnabled� �� $ MagicUpgrade.Priority��� $��0��$��1��$
��Rare��.��� $��0,5,0,0q� �� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$
��Tier��$6 0Minimum tier to start using Alchemy Orbs on maps�� �� $ RareUpgrade.TierEnabled�  $��0��$��1��$��5,0,0,0q�$
 16Q�$ 1Q�{��� $ RareUpgrade.TierEnabled� �� $ RareUpgrade.Tier��� $��1��$��0��$��Priority��$: 4Minimum priority to start using Alchemy Orbs on maps�� �� $! RareUpgrade.PriorityEnabled�  $��1��$��1��$��5,0,0,0q�{��� $! RareUpgrade.PriorityEnabled� �� $ RareUpgrade.Priority��� $��1��$��0��$��Chisel��.��� $��0,5,0,0q� �� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$
��Tier��$> 8Minimum tier to start using Cartographer Chisels on maps�� �� $ ChiselUpgrade.TierEnabled�  $��0��$��1��$��5,0,0,0q�$
 16Q�$ 1Q�{��� $ ChiselUpgrade.TierEnabled� �� $ ChiselUpgrade.Tier��� $��1��$��0��$��Priority��$B <Minimum priority to start using Cartographer Chisels on maps�� �� $# ChiselUpgrade.PriorityEnabled�  $��1��$��1��$��5,0,0,0q�{��� $# ChiselUpgrade.PriorityEnabled� �� $ ChiselUpgrade.Priority��� $��1��$��1��$
��Vaal��.��� $��0,5,0,0q� �� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$
��Tier��$3 -Minimum tier to start using Vaal Orbs on maps�� �� $ VaalUpgrade.TierEnabled�  $��0��$��1��$��5,0,0,0q�$
 16Q�$ 1Q�{��� $ VaalUpgrade.TierEnabled� �� $ VaalUpgrade.Tier��� $��1��$��0��$��Priority��$7 1Minimum priority to start using Vaal Orbs on maps�� �� $! VaalUpgrade.PriorityEnabled�  $��1��$��1��$��5,0,0,0q�{��� $! VaalUpgrade.PriorityEnabled� �� $ VaalUpgrade.Priority��� $��2��$��0��$��	Fragments��.��� $��0,5,0,0q� �� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$
��Tier��$? 9Minimum tier to start using Sacrifice Fragments with maps�� �� $! FragmentUpgrade.TierEnabled�  $��0��$��1��$��5,0,0,0q�$
 16Q�$ 1Q�{��� $! FragmentUpgrade.TierEnabled� �� $ FragmentUpgrade.Tier��� $��1��$��0��$��Priority��$C =Minimum priority to start using Sacrifice Fragments with maps�� �� $% FragmentUpgrade.PriorityEnabled�  $��1��$��1��$��5,0,0,0q�{��� $% FragmentUpgrade.PriorityEnabled� �� $ FragmentUpgrade.Priority��� $��2��$��1��$��Magic to Rare��.��� $��0,5,0,0q� �� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$
��Tier��$E ?Minimum tier to start using Scouring+Alchemy orbs on magic maps�� �� $" MagicRareUpgrade.TierEnabled�  $��0��$��1��$��5,0,0,0q�$
 16Q�$ 1Q�{��� $" MagicRareUpgrade.TierEnabled� �� $ MagicRareUpgrade.Tier��� $��1��$��0��$��Priority��$I CMinimum priority to start using Scouring+Alchemy orbs on magic maps�� �� $&  MagicRareUpgrade.PriorityEnabled�  $��1��$��1��$��5,0,0,0q�{��� $&  MagicRareUpgrade.PriorityEnabled� �� $ MagicRareUpgrade.Priority��� $��3��$��0��$��2��$��Existing rare maps��.���� $	 Vertical=�.+�	� $��0,5,0,0q�$	��Run�� 	� 	GroupName$ g2�� �� $ ExistingRares�,   &Default.MapBot.Gui+EnumToBoolConverter   Instance �� 	Converter# Z "   Default.MapBot.ExistingRares
   Run �� ConverterParameter# Z 	� $��0,5,0,0q�$��
Do not run��$ g2�� �� $ ExistingRares�# Z    NoRun# Z 	� $��0,5,0,0q�$&�� Do not run if reroll is required��$ g2�� �� $ ExistingRares�# Z    NoReroll# Z 	� $��0,5,0,0q�$.��(Downgrade to magic if reroll is required��$ g2�� �� $ ExistingRares�# Z    	Downgrade# Z �� $��4��$��0��$��2��$��Rare reroll��.���� $	 Vertical=�.+�	� $��0,5,0,0q�$��Scouring+Alchemy��$ g1�� �� $ RerollMethod�# Z    Default.MapBot.RareReroll   	ScourAlch# Z 	� $��0,5,0,0q�$��Chaos��$ g1�� �� $ RerollMethod�# Z    Chaos# Z �� $��1q�    Default.MapBot.MapSettings   Instance# Z $
��Maps��.���� $
��Auto=�$��Disabled=�.���  �� $
��Auto ��� $��* � �� $
��Auto �.+��� $��0��$	 Vertical=�.+��� $��Search:���� -    ��Name$ MapSearchTextBox��$	��300��$
��Left=�*  $MahApps.Metro.Controls.TextBoxHelper   ClearTextButton . &   System.Windows.Controls.DataGrid  -     Name$ MapGrid��$��1��$��0,10,0,0q�    AutoGenerateColumns  .  $��1q�!   CanUserAddRows! .  "   CanUserDeleteRows" .  #   GridLinesVisibility$	# All=����� $ MapList�$   Columns$ 0	  *System.Windows.Controls.DataGridTextColumn	  ,
  &System.Windows.Controls.DataGridColumn% 
  Header$
% Name��& 
  
IsReadOnly& . 1  +System.Windows.Controls.DataGridBoundColumn'   Binding' �� $
 Name�( �� Mode$( OneTime=�	  $
% Tier��& . ' �� $
 Tier�$( OneTime=�	  $
% Type��& . ' �� $
 Type�$( OneTime=�8  2MahApps.Metro.Controls.DataGridNumericUpDownColumn  $% Priority��' �� $ Priority�) �� UpdateSourceTrigger$) PropertyChanged=�4  .System.Windows.Controls.DataGridCheckBoxColumn    MetroDataGridCheckBox*   EditingElementStyle#* �  +   ElementStyle#+ [  $% Ignore��' �� $ Ignored�$) PropertyChanged=�  #* �  $% IB��' �� $ IgnoredBossroom�$) PropertyChanged=�, 
  HeaderStyle, ��   MetroDataGridColumnHeader# [ =  7System.Windows.Controls.Primitives.DataGridColumnHeader"  .���  �@�ToolTip$ Ignore bossroom��+ �� # [ " ��.�-     #* �  #+ [  $% Sextant��' �� $ Sextant�$) PropertyChanged=�  $% ZM��-   Minimum$- 0Q�' �� $ ZanaMod�$) PropertyChanged=�, �� # [ "  .���  �@�ToolTip$ Zana mod��  $% MR��.   Maximum$. 50Q�$- -1Q�' �� $ MobRemaining�$) PropertyChanged=�, �� # [ "  .���  �@�ToolTip$ Monster remaining��  #* �  #+ [  $% S��' �� $ StrictMobRemaining�$) PropertyChanged=�, �� # [ "  .���  �@�ToolTip$ Strict monster remaining��  $% E%��$	. 100Q�$- -1Q�' �� $ ExplorationPercent�$) PropertyChanged=�, �� # [ "  .���  �@�ToolTip$ Exploration percent��  #* �  #+ [  $% S��' �� $ StrictExplorationPercent�$) PropertyChanged=�, �� # [ "  .���  �@�ToolTip$  Strict exploration percent��  #* �  #+ [  $% MT��/   IsThreeState/ . ' �� $ TrackMob�$) PropertyChanged=�, �� # [ "  .���  �@�ToolTip$ Monster tracking��  #* �  $% FT��/ . ' �� $ FastTransition�$) PropertyChanged=�, �� # [ "  .���  �@�ToolTip$ Fast transition��+ �� # [ " ��.�-   �� $��1q�"   Default.MapBot.AffixSettings0   Instance# Z0 $��Affixes��.���� $
��Auto=�$��Disabled=�.���  �� $
��Auto ��� $��* � �� $
��Auto �.+��� $��0��$	 Vertical=�.+��� $��Search:���� -   $ AffixSearchTextBox��$	��300��$
��Left=� .   -   $ 	AffixGrid��$��1��$��0,10,0,0q�  .  $��1q�! .  " .  $	# All=����� $ 	AffixList�$ 	  $
% Name��& . ' �� $
 Name�$( OneTime=�	  $% Description��& . ' �� $ Description�$( OneTime=�  #* �  #+ [  $% Reroll on magic��' �� $ RerollMagic�$) PropertyChanged=�  #* �  #+ [  $% Reroll on rare��' �� $ 
RerollRare�$) PropertyChanged=��� $��1q�   Default.MapBot.Statistics1   Instance# Z1 $��
Statistics��.���� $
��Auto=�$
��Auto=�.���  �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��Maps entered���� $��0��$��1��$��5,0,0,0q����� $ TotalEntered�$( OneWay=��� $��1��$��0��$��Maps finished���� $��1��$��1��$��5,0,0,0q����� $ TotalFinished�$( OneWay=��� $��2��$��0��$��Average tier entered���� $��2��$��1��$��5,0,0,0q����� $ AverageTierEntered�$( OneWay=��� $��3��$��0��$��0,10,0,0q�$��
Maps found���� $��3��$��1��$��5,10,0,0q����� $ 
TotalFound�$( OneWay=��� $��4��$��0��$��
Map income���� $��4��$��1��$��5,0,0,0q����� $ 	MapIncome�$( OneWay=��� $��5��$��0��$��Average tier found���� $��5��$��1��$��5,0,0,0q����� $ AverageTierFound�$( OneWay=��� $��7��$��0��$��0,10,0,0q�$��Total uptime���� $��7��$��1��$��5,10,0,0q����� $ TotalTimeSpent�$( OneWay=��� $��8��$��0��$��Time in current map���� $��8��$��1��$��5,0,0,0q����� $ CurrentTimeSpent�$( OneWay=��� $��9��$��0��$��Average time per map���� $��9��$��1��$��5,0,0,0q����� $ AverageTimeSpent�$( OneWay=�