   M S B A M L   `   `   `  ���� T  PDefault, Version=0.4.5619.1753, Culture=neutral, PublicKeyToken=1779d26ece6a7b373clr-namespace:Default.QuestBotDefault.QuestBot  O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem     Default.QuestBot.Gui   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     U QMahApps.Metro, Version=1.5.0.23, Culture=neutral, PublicKeyToken=f4fb5a3c4d1e5b4f:mahapps,http://metro.mahapps.com/winfx/xaml/controls  Dmc;http://schemas.openxmlformats.org/markup-compatibility/2006  -questBotclr-namespace:Default.QuestBot  3system&clr-namespace:System;assembly=mscorlib  .����    Default.QuestBot.Settings    Instance � DataContext# Z  .@��� $��1q�$��Current quest��.���� $
��Auto=�$
��Auto=�.��� $��0,5,0,0q� � RowDefinitions �� $
��Auto ��� $
��Auto � � ColumnDefinitions �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��Quest:���� $��0��$��1������  �� Path$ CurrentQuestName� �� Mode$ TwoWay=��� $��1��$��0��$��State:���� $��1��$��1������ $ CurrentQuestState�$ TwoWay=��� $��1q�$��Grinding��.���� .@��� $��1q�$��Rules��.���� $��Disabled=�$
��Auto=�.���� $
��Left=�$	��Top=� �� Orientation$ Vertical=�.+��� ���� $ GrindingRules����� .R�� ���� .R��� $��0,5,5,0q����#CF5A07$��1q�._��� $��5,5,5,5q�$ Vertical=�.+�]� .+��� $��Quest��$
��Left=���  ��Name$ DeleteRuleButton��-   $��	0,-5,-5,0q�$��Right=�$	��Top=�$��0q�$��×��$��Right=�#��[" ��� -   $
��Left=�$	��Top=� �� DisplayMemberPath$
 Name��	   	QuestList#��Z	 
 �� SelectedItem
 �� $ Quest�$ TwoWay=��� $��0,5,0,0q�$
��Left=�$��Center=�$��	Level cap��*  $MahApps.Metro.Controls.NumericUpDown  $
��Left=�$	��Top=�   Maximum$	 100Q�   Minimum$ 1Q�   Value �� $ LevelCap��� $��Areas���� ���� $ Areas����� .R��� $ Vertical=�.+���  ��Name$ AreaSelectionComboBox��-   $
��Left=�$	��Top=�$
 Name��   AreaList#��Z 
 �� $
 Area�$ TwoWay=�
 � Tag ��  �� RelativeSource ��  �� AncestorType$ ItemsControlL�$ DataContext��� $ 
Horizontal=�.+��� $��0,5,0,5q�$
��Left=�$��Center=�$
��Pool��  $��0,5,0,5q�$
��Left=�$	��Top=�$ 1Q� �� $
 Pool��� $ AddAreaButton��-   $��0,5,0,0q�$
��Left=�$	��Top=�$��Add area��$��7(� �� $ Areas��� -   $ AddRuleButton��$��0,10,0,0q�$
��Left=�$	��Top=�$��Add grinding rule���� $��1q�$��Settings��.���� $
��Auto=�$
��Auto=�.���� $��0,5,0,0q�$ Vertical=�.+��  �� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��Center=�$��Exploration percent��  $��0��$��1��$��5,0,0,0q���.  $	 100Q�$ 0Q� �� $ ExplorationPercent��� $��1��$��0��$��0,5,0,0q�$��Center=�$��
Max deaths��  $��1��$��1��$��5,5,0,0q���.  $ 0Q� �� $ 	MaxDeaths��� $��0,10,0,0q�$��Monster tracking�� d� 	IsChecked �� $ TrackMob��� $��0,5,0,0q�$��Use hideout�� �� $ 
UseHideout��� $��1q�$��Manual bosses��.���� $��Disabled=�$
��Auto=�.���� $��0,5,0,0q�$
��Left=�$	��Top=�$ Vertical=�.+��� $ ��Focus bot window near boss�� �� $ 
NotifyBoss��� $��0,10,0,0q����� $ BossesByAct����� .R�� ���� .R���  �� HeaderStringFormat$ ACT {0}������ $	 Act�.���� ���� $ Elements����� .R��� $ Vertical=�.+��� $��0,2,0,2q����� $
 Name�$ OneTime=� �� $ Enabled��� $��1q�$��Optional quests��.���� $��Disabled=�$
��Auto=�.���� ���� $ OptionalQuestsByAct����� .R�� ���� .R��� $ ACT {0}������ $	 Act�.���� ���� $ Elements����� .R��� $ Vertical=�.+��� $��0,2,0,2q����� $
 Name�$ OneTime=� �� $ Enabled��� $��1q�$��Rewards��.���� $��Disabled=�$
��Auto=�.���� $��0,10,0,0q�$
��Left=�$	��Top=�$ Vertical=�.+��� $ 
Horizontal=�.+��� $��Class:��$��14(�$��SemiBold"��� -   $ CharClassComboBox�� �� 
IsEditable .     CharacterClasses#��Z  �� SelectedValue �� $ CharacterClass�$ TwoWay=�{��� 3   -Default.QuestBot.Gui+CharClassToBoolConverter   Instance �� 	Converter# Z $ SelectedValue� �� *Self+ �� ConverterParameter ��true���� -   $ ResetButton��$��10,0,0,0q�$��Reset���� $��0,15,0,0q����� $ RewardQuestsByAct����� .R�� ���� .R��� $ ACT {0}������ $	 Act�.���� ���� $ Elements����� .R��� $ Vertical=�.+��� $��0,5,0,0q����� $
 Name�$ OneTime=��� -   $	��150��$��0,5,0,5q� .   �� SelectedValuePath$
 Name�� �� $ SelectedReward�{��� # Z  �� ElementName$ CharClassComboBox��$ SelectedValue� ��	false������ .R��� $��SemiBold"����� 1   +Default.QuestBot.Gui+RarityToColorConverter   Instance# Z ���� ��`� *   $Default.QuestBot.Gui+RewardConverter   Instance `� 	Converter# Z .1��� $ Id��� $ CharClassComboBox��$ SelectedValue��� $��1q�$
��Misc��.���� $��Disabled=�$
��Auto=�.���� $��0,5,0,0q�$
��Left=�$	��Top=�$ Vertical=�.+��� $ ��Enter corrupted side areas�� �� $ EnterCorruptedAreas��� $��0,5,0,0q�$&�� Talk to all quest givers in town�� �� $ TalkToQuestgivers�