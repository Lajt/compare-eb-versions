   M S B A M L   `   `   `  ���� S  OLegacy, Version=0.4.5619.1753, Culture=neutral, PublicKeyToken=1779d26ece6a7b375clr-namespace:Legacy.GemLevelerLegacy.GemLeveler      Legacy.GemLeveler.Gui   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     6
gemLevelerclr-namespace:Legacy.GemLeveler     .���� $
��Auto=�.���� *   $Legacy.GemLeveler.GemLevelerSettings    Instance � DataContext# Z  .+��  � RowDefinitions �� $
��Auto ��� $
��Auto � � ColumnDefinitions �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��DebugStatements: ��$��3,5,3,1q� ^� ToolTip$- 'Should the plugin log debug statements?���� -    ��Name$ DebugStatementsCheckBox��$��0��$��1��$��3q� d� 	IsChecked ��  �� Path$ DebugStatements� �� Mode$ TwoWay=��  �� $
��Auto � �� $
��Auto �.+��� -   $��RefreshSkillsButton��$��0��$��0��$��Refresh Skills��$��3,5,3,1q�$N HRefreshes the list of skills if they change when the bot is not running.���  �� $
��Auto � �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��GlobalNameIgnore: ��$��3q�$; 5The skillgem name to add/remove from the ignore list.���� -   	 ��Name$	 GlobalNameIgnoreTextBox��$��0��$��1��$	��125���� -   
 ��Name$
 AddGlobalNameIgnoreButton��$��0��$��2��$	��Add��$��3,5,3,1q�$0 *Adds the skillgem name to the ignore list.���� -   $"
 RemoveGlobalNameIgnoreButton��$��0��$��3��$��Remove��$��3,5,3,1q�$3 -Removes the skillgem name to the ignore list.��� $��0,6,0,0q� �� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��GlobalNameIgnoreList: ��$��3q�$7 1A list of skillgem names to ignore from leveling.���� -    ��Name$! GlobalNameIgnoreListListBox��$��0��$��1������ $ GlobalNameIgnoreList�$ TwoWay=�� $��0,6,0,0q� �� $
��Auto � �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��AllSkillGems: ��$��3q�$' !User skillgems that are equipped.���� -    ��Name$ AllSkillGemsComboBox��$��0��$��1������ $ AllSkillGems�$ OneWay=��� -   $
 AddSkillGemButton��$��0��$��2��$	��Add��$��3,5,3,1q�$- 'Adds the skillgem to the leveling list.���� -	   $
 RemoveSkillGemButton��$��0��$��3��$��Remove��$��3,5,3,1q�$2 ,Removes the skillgem from the leveling list.���� -
   $
 AddAllSkillGemButton��$��0��$��4��$��Add All��$��3,5,3,1q�$- 'Adds all skillgem to the leveling list.���� -   $
 RemoveAllSkillGemButton��$��0��$��5��$��
Remove All��$��3,5,3,1q�$2 ,Removes all skillgem from the leveling list.��� $��0,6,0,0q� �� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��SkillGemsToLevelList: ��$��3q�$# A list of skillgems to level.���� -   $! SkillGemsToLevelListListBox��$��0��$��1������ $ SkillGemsToLevelList�$ TwoWay=�