   M S B A M L   `   `   `  ���� O  Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem  S OLegacy, Version=0.4.5619.1753, Culture=neutral, PublicKeyToken=1779d26ece6a7b37A%clr-namespace:Legacy.ItemFilterEditorLegacy.ItemFilterEditor A%clr-namespace:Legacy.ItemFilterEditorLegacy.ItemFilterEditor !   Legacy.ItemFilterEditor.Gui   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     9system&clr-namespace:System;assembly=mscorlib     BitemFilterEditor%clr-namespace:Legacy.ItemFilterEditor     U QMahApps.Metro, Version=1.5.0.23, Culture=neutral, PublicKeyToken=f4fb5a3c4d1e5b4f:mahapps,http://metro.mahapps.com/winfx/xaml/controls  <ItemFilterEditor%clr-namespace:Legacy.ItemFilterEditor    � 	Resources  I  CLegacy.ItemFilterEditor.MyTextToStringObservableCollectionConverter   TextToStringListConverter S� 
MethodName S� 
ObjectType   System.Enum  CategoryEnumValues S� MethodParameters M� TypeName��%d   &	        &	        S� $ 	GetValues��"   M� $' !itemFilterEditor:MyEvaluationType��.���� .@��� $��Filter Editor��.��� .+��� $��
10,10,0,10q�$��Categories & Filters��$
��Left=�$	��357��.��� .+�R� -    R�Name$ TreeViewCategories��  CategoryMenu � ContextMenu# �  ���� *
Categories+����  �� ItemsSource �� *	Filters+.R��� ���� *Description+ �� ItemTemplate �� 	 �� DataType$  Legacy.ItemFilterEditor.Filter"	  .R��� ���� *Description+���� 
 �� 
TargetType"
 Q�.���  �� Property � �� �� Value
 ���� ��  � �� �	#FFFFFFFF �� Triggers ��  �� Binding �� *	Enabled+ �� Value	 False.a���  � ��
 �  ��   �� 	StaysOpen  
FilterMenu��%�   &	       &	 m     ��  . .@�m� -   $��Add Category...��m� -   $��Delete Category��m� -   $��Add Filter...����  . .@�m� -   $��Delete Filter��m� -   $��Enable Filter��m� -   $��Disable Filter���� $��Settings��$��372,10,10,44q�.��� -    �Name$ GridSettings��$��Stretch=�$��Stretch=�.+�� -	   $ CategoryGrid��$y�	Collapsed=�.+��� $��Description��$
��Left=�$��	10,12,0,0q�$	��Top=��� $
��Left=�$��23��$��	75,10,0,0q� �� TextWrapping$
 Wrap=�$	��Top=�$	��291������ *Description+�� $��	0,38,0,41q�$
��Auto=�.��� $��Stretch=�.+��� $��Category��$
��Left=�$��	10,10,0,0q�$	��Top=��� $
��Left=�$��	66,10,0,0q�$	��Top=�$	��120�� �� SelectedValue �� *Type+����  �� Source# [ �� $��Save Category��$��	0,0,10,10q�$��26��$��Bottom=�$��Right=�$	��356��� -
   $ ItemFilterGrid��$y�	Collapsed=�.+��� $��Description��$
��Left=�$��	10,12,0,0q�$	��Top=��� -    ��Name$ TextBoxDescription��$
��Left=�$��23��$��	75,10,0,0q�$
 Wrap=�$	��Top=�$	��291������  �� ElementName$ TreeViewCategories�� �� Path$ SelectedItem.Description��� $��	0,38,0,41q�$
��Auto=�.����  �� CanVerticallyScroll . .+�9� $��Names and Types��.��� -   $ GridTypesNames��$��Stretch=�$
��Auto��.+��� $��Names��$
��Left=�$��	10,10,0,0q�$	��Top=��� $��Stretch=�$	��123��$��	10,31,0,0q�$ NoWrap=� �� AcceptsReturn . $	��Top=�" �� VerticalScrollBarVisibility$
 Auto=�$ �� HorizontalScrollBarVisibility$
 Auto=����� $ TreeViewCategories��$ SelectedItem.Names� �� 	Converter# [   �� Empty �� TargetNullValue# Z �� $��Regex��$
��Left=�$��	91,10,0,0q�$	��Top=� d� IsThreeState .   d� 	IsChecked  �� $ TreeViewCategories��$ SelectedItem.NameRegex��� $��	Match Any��$
��Left=�$��
179,10,0,0q�$	��Top=� .   �� $ TreeViewCategories��$, &SelectedItem.NameMatchAnyRatherThanAll��� $��Types��$
��Left=�$��
10,159,0,0q�$	��Top=��� $��Stretch=�$	��123��$��
10,180,0,0q�$ NoWrap=� . $	��Top=�$
 Auto=�$
 Auto=����� $ TreeViewCategories��$ SelectedItem.Types�# [  # Z �� $��Regex��$
��Left=�$��
91,159,0,0q�$	��Top=� .   �� $ TreeViewCategories��$ SelectedItem.TypeRegex��� $��	Match Any��$
��Left=�$��179,159,0,0q�$	��Top=� .   �� $ TreeViewCategories��$, &SelectedItem.TypeMatchAnyRatherThanAll�9� $��Rarities��.��� -   $ GridRarities��$��Stretch=�$
��Auto��.+��� $��Rarities��$
��Left=�$��	10,10,0,0q�$	��Top=�A� $��	10,31,0,0q�.+��� -   ! ��Name$! CheckBoxRarityNormal��$��Normal��&   Legacy.ItemFilterEditor.MyRarity"   Normal
# � Tag## Z" �� -   $! CheckBoxRarityMagic��$��Magic��$   Magic## Z$ �� -   $! CheckBoxRarityRare��$
��Rare��%   Rare## Z% �� -   $! CheckBoxRarityUnique��$��Unique��&   Unique## Z& �� -   $! CheckBoxRarityQuest��$��Quest��'   Quest## Z' �� -   $! CheckBoxRarityGem��$��	Skill Gem��
(   Gem## Z( �� -   $! CheckBoxRarityCurrency��$��Currency��)   Currency## Z) �� -   $! CheckBoxRarityProphecy��$��Prophecy��*   Prophecy## Z* 9� $��Socket Colors��.��� -   $ GridSocketColors��$��Stretch=�$
��Auto��.+��� $��Socket Colors��$
��Left=�$��	10,10,0,0q�$	��Top=��� $
��Left=�$��49��$��	10,31,0,0q�$
 Wrap=� . $	��Top=�$	��340��$
 Auto=�$
 Auto=����� $ TreeViewCategories��$ SelectedItem.SocketColors�# [  # Z 9� $��Socket Count��.��� -   $ GridSocketCount��$��Stretch=�$
��Auto��.+��� $��Minimum Sockets��$
��Left=�$��	10,13,0,0q�$	��Top=��� $
��Left=�$��23��$��
120,10,0,0q�$
 Wrap=�$	��Top=�$	��115������ $ TreeViewCategories��$ SelectedItem.MinSockets�# Z �� $��Maximum Sockets��$
��Left=�$��	10,41,0,0q�$	��Top=��� $
��Left=�$��23��$��
120,38,0,0q�$
 Wrap=�$	��Top=�$	��115������ $ TreeViewCategories��$ SelectedItem.MaxSockets�# Z 9� $��Links��.��� -   $ 	GridLinks��$��Stretch=�$
��Auto��.+��� $��Minimum Links��$
��Left=�$��	10,13,0,0q�$	��Top=��� $
��Left=�$��23��$��
120,10,0,0q�$
 Wrap=�$	��Top=�$	��115������ $ TreeViewCategories��$ SelectedItem.MinLinks�# Z �� $��Maximum Links��$
��Left=�$��	10,41,0,0q�$	��Top=��� $
��Left=�$��23��$��
120,38,0,0q�$
 Wrap=�$	��Top=�$	��115������ $ TreeViewCategories��$ SelectedItem.MaxLinks�# Z 9� $��	Item Size��.��� -   $ GridSize��$��Stretch=�$
��Auto��.+��� $��Minimum Size��$
��Left=�$��	10,13,0,0q�$	��Top=��� $
��Left=�$��23��$��
120,10,0,0q�$
 Wrap=�$	��Top=�$��33������ $ TreeViewCategories��$ SelectedItem.MinWidth�# Z �� $
��Left=�$��23��$��
169,10,0,0q�$
 Wrap=�$	��Top=�$��33������ $ TreeViewCategories��$ SelectedItem.MinHeight�# Z �� $��Maximum Size��$
��Left=�$��	10,41,0,0q�$	��Top=��� $
��Left=�$��23��$��
120,38,0,0q�$
 Wrap=�$	��Top=�$��33������ $ TreeViewCategories��$ SelectedItem.MaxWidth�# Z �� $
��Left=�$��23��$��
169,38,0,0q�$
 Wrap=�$	��Top=�$��33������ $ TreeViewCategories��$ SelectedItem.MaxHeight�# Z �� $��x��$
��Left=�$��
158,13,0,0q�$	��Top=��� $��x��$
��Left=�$��
158,41,0,0q�$	��Top=�9� $��
Item Level��.��� -   $ GridItemLevel��$��Stretch=�$
��Auto��.+��� $��Minimum Level��$
��Left=�$��	10,13,0,0q�$	��Top=��� $
��Left=�$��23��$��
120,10,0,0q�$
 Wrap=�$	��Top=�$	��115������ $ TreeViewCategories��$ SelectedItem.ItemLevelMin�# Z �� $��Maximum Level��$
��Left=�$��	10,41,0,0q�$	��Top=��� $
��Left=�$��23��$��
120,38,0,0q�$
 Wrap=�$	��Top=�$	��115������ $ TreeViewCategories��$ SelectedItem.ItemLevelMax�# Z 9� $��Quality��.��� -   $ GridQuality��$��Stretch=�$
��Auto��.+��� $��Minimum Quality��$
��Left=�$��	10,13,0,0q�$	��Top=��� $
��Left=�$��23��$��
120,10,0,0q�$
 Wrap=�$	��Top=�$	��115������ $ TreeViewCategories��$ SelectedItem.MinQuality�# Z �� $��Maximum Quality��$
��Left=�$��	10,41,0,0q�$	��Top=��� $
��Left=�$��23��$��
120,38,0,0q�$
 Wrap=�$	��Top=�$	��115������ $ TreeViewCategories��$ SelectedItem.MaxQuality�# Z 9� $��Affixes��.��� -   $ GridAffixes��$��Stretch=�$
��Auto��.+��� $��Stretch=�$	��150��$��	10,10,0,0q�$ NoWrap=� . $	��Top=�$
 Auto=�$
 Auto=����� $ TreeViewCategories��$ SelectedItem.Affixes�# [  # Z 9� $��
Identified��.��� $��Stretch=�.+��� -   $! CheckBoxIdentifiedEnabled��$��
Identified��$��0,0,0,5q� .   �� $ TreeViewCategories��$$ SelectedItem.HasToBeIdentified�9� $��Highest Implicit Value��.��� -   $ GridHighestImplicitValue��$��Stretch=�$
��Auto��.+��� $��Minimum Implicit Value��$
��Left=�$��	10,13,0,0q�$	��Top=��� $
��Left=�$��23��$��
120,10,0,0q�$
 Wrap=�$	��Top=�$	��115������ $ TreeViewCategories��$' !SelectedItem.HighestImplicitValue�# Z 9� $��Elder / Shaper Items��.��A� -   + A�Name$+ GridElderShaper��$��Stretch=�$
��Auto��.+��� -    $! CheckBoxElderEnabled��$��Elder�� .   �� $ TreeViewCategories��$ SelectedItem.HasToBeElder��� -!   $! CheckBoxShaperEnabled��$��Shaper�� .   �� $ TreeViewCategories��$  SelectedItem.HasToBeShaper��� -"   $"! CheckBoxElderOrShaperEnabled��$��ElderOrShaper�� .   �� $ TreeViewCategories��$' !SelectedItem.HasToBeElderOrShaper��� -#   $
��Save��$��374,0,10,10q�$��29��$��Bottom=�$��Left, Stretch=��� $��Extra Settings��.���� $
��Auto=�.���� 6  0Legacy.ItemFilterEditor.ItemFilterEditorSettings,   Instance- � DataContext#- Z, .+�� . � RowDefinitions. �� $
��Auto ��� $
��Auto ��� $
��Auto �/ � ColumnDefinitions/ �� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��DebugPickupLimit: ��$��3q�0 ^� ToolTip$0  ���� -$   $! CheckBoxDebugPickupLimit��$��0��$��1��$��3,5,3,1q�$�� ��  �� $ DebugPickupLimit�1 �� Mode$1 TwoWay=��� $��1��$��0��$��LimitWisdomPickup: ��$��3q�$0  ���� -%   $! CheckBoxLimitWisdomPickup��$��1��$��1��$��3,5,3,1q�$�� ��  �� $ LimitWisdomPickup�$1 TwoWay=�*  $MahApps.Metro.Controls.NumericUpDown  $��1��$��2��$��3q�2   Value2 �� $ WisdomPickupLimit�$1 TwoWay=��� $��2��$��0��$��LimitPortalPickup: ��$��3q�$0  ���� -&   $! CheckBoxLimitPortalPickup��$��2��$��1��$��3,5,3,1q�$�� ��  �� $ LimitPortalPickup�$1 TwoWay=�  $��2��$��2��$��3q�2 �� $ PortalPickupLimit�$1 TwoWay=�