   M S B A M L   `   `   `  ���� T  PDefault, Version=0.4.5619.1753, Culture=neutral, PublicKeyToken=1779d26ece6a7b375clr-namespace:Default.AutoFlaskDefault.AutoFlask      Default.AutoFlask.Gui   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     1localclr-namespace:Default.AutoFlask     U QMahApps.Metro, Version=1.5.0.23, Culture=neutral, PublicKeyToken=f4fb5a3c4d1e5b4f:mahapps,http://metro.mahapps.com/winfx/xaml/controls  Dmc;http://schemas.openxmlformats.org/markup-compatibility/2006  .����     Default.AutoFlask.Settings    Instance � DataContext# Z  $
��Auto=�$
��Auto=�.���� $��5,5,0,0q�$
��Left=�$	��Top=� �� Orientation$ Vertical=�.+��  � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � � ColumnDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��Center=�$��
Life flask�� � ToolTip$U ONon-instant Life flask will be used if your HP percent is below specified value��*  $MahApps.Metro.Controls.NumericUpDown  $��0��$��1��$��5,0,5,0q���.     Maximum$ 95Q�   Minimum$ 5Q�   Value �� 	 �� Path$	 	HpPercent��� $��0��$��2��$��Center=�$	��%HP���� $��1��$��0��$��0,5,0,0q�$��Center=�$��Instant Life flask��$Q KInstant Life flask will be used if your HP percent is below specified value��  $��1��$��1��$��5,5,5,0q���.  $ 95Q�$ 5Q� �� $	 HpPercentInstant��� $��1��$��2��$��0,5,0,0q�$��Center=�$	��%HP���� $��2��$��0��$��0,5,0,0q�$��Center=�$��
Mana flask��$I CMana flask will be used if your MP percent is below specified value��  $��2��$��1��$��5,5,5,0q���.  $ 95Q�$ 5Q� �� $	 	MpPercent��� $��2��$��2��$��0,5,0,0q�$��Center=�$	��%MP���� $��3��$��0��$��0,5,0,0q�$��Center=�$��Quicksilver flask��$T NQuicksilver flask will be used if there are no monsters within specified range��  $��3��$��1��$��5,5,5,0q���.  $	 250Q�$ 0Q� �� $	 QsilverRange��� $��3��$��2��$��0,5,0,0q�$��Center=�$��	mob range��9� $��0,20,0,0q�$��Utility flasks��.���� ���� $	 UtilityFlasks����� .R��� ���� $��14(�$
��Bold"����� $
	 Name�.���� $ Vertical=�.+��� ���� $	 Triggers����� .R��� $ Vertical=�.+��� 
 ��Name$
 TriggerTypeComboBox��   TriggerTypes#��Z  �� SelectedItem �� $
	 Type����� .R��� $��SemiBold"����� 0   *Default.AutoFlask.Gui+DescriptionConverter   Instance �� 	Converter# Z � $��5,0,0,0q� �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��0,5,0,0q�$��Center=�$	��HP%��$D >Flask will be used if your HP percent is below specified value��y���  �� ElementName$ TriggerTypeComboBox��$	 SelectedItem�/   )Default.AutoFlask.Gui+VisibilityConverter   Instance# Z  �� ConverterParameter$ Hp��  $��0��$��1��$��5,5,0,0q�$ 95Q�$ 5Q�y��� $ TriggerTypeComboBox��$	 SelectedItem�# Z $ Hp�� �� $	 MyHpPercent��� $��1��$��0��$��0,5,0,0q�$��Center=�$	��ES%��$D >Flask will be used if your ES percent is below specified value��y��� $ TriggerTypeComboBox��$	 SelectedItem�# Z $ Es��  $��1��$��1��$��5,5,0,0q�$ 95Q�$ 5Q�y��� $ TriggerTypeComboBox��$	 SelectedItem�# Z $ Es�� �� $	 MyEsPercent��� $��2��$��0��$��0,5,0,0q�$��Center=�$��Rarity��$ Monster's rarity��y��� $ TriggerTypeComboBox��$	 SelectedItem�# Z $ MobsOrAttack���� $��2��$��1��$��5,5,0,0q�   Rarities#��Z  �� $	 	MobRarity�y��� $ TriggerTypeComboBox��$	 SelectedItem�# Z $ MobsOrAttack���� $��3��$��0��$��0,5,0,0q�$��Center=�$��Range��$8 2Monsters must be within specified range around you��y��� $ TriggerTypeComboBox��$	 SelectedItem�# Z $
 Mobs��  $��3��$��1��$��5,5,0,0q�$	 250Q�$ 5Q�y��� $ TriggerTypeComboBox��$	 SelectedItem�# Z $
 Mobs�� �� $	 MobRange��� $��4��$��0��$��0,5,0,0q�$��Center=�$��Count��$$ Minimum monster count in range��y��� $ TriggerTypeComboBox��$	 SelectedItem�# Z $
 Mobs��  $��4��$��1��$��5,5,0,0q�$ 1Q�y��� $ TriggerTypeComboBox��$	 SelectedItem�# Z $
 Mobs�� �� $	 MobCount��� $��5��$��0��$��0,5,0,0q�$��Center=�$��Min HP%��$  Minimum monster HP percent��y��� $ TriggerTypeComboBox��$	 SelectedItem�# Z $ Attack��  $��5��$��1��$��5,5,0,0q�$ 95Q�$ 0Q�y��� $ TriggerTypeComboBox��$	 SelectedItem�# Z $ Attack�� �� $	 MobHpPercent��� -   $��0,10,0,0q�$
��Left=�$��Bottom=�$��Remove trigger��$��7(�
 � Tag ��  �� RelativeSource ��  �� AncestorType$ ItemsControlL�$	 DataContext��� $��	0,10,0,10q����#CF5A07��  ��Name$ AddTriggerButton��-   $��0,5,0,0q�$
��Left=�$��Bottom=�$��Add trigger��$��9(� �� $	 Triggers�9� $��0,15,0,0q�$��Unique flasks��.���� ���� $	 UniqueFlasks����� .R��� ���� $��14(�$
��Bold"����� $
	 Name�.���� $ Vertical=�.+��� ���� $	 Triggers����� .R��� $ Vertical=�.+��� $
 TriggerTypeComboBox��#��Z  �� $
	 Type����� .R��� $��SemiBold"����� # Z � $��5,0,0,0q� �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��0,5,0,0q�$��Center=�$	��HP%��$D >Flask will be used if your HP percent is below specified value��y��� $ TriggerTypeComboBox��$	 SelectedItem�# Z $ Hp��  $��0��$��1��$��5,5,0,0q�$ 95Q�$ 5Q�y��� $ TriggerTypeComboBox��$	 SelectedItem�# Z $ Hp�� �� $	 MyHpPercent��� $��1��$��0��$��0,5,0,0q�$��Center=�$	��ES%��$D >Flask will be used if your ES percent is below specified value��y��� $ TriggerTypeComboBox��$	 SelectedItem�# Z $ Es��  $��1��$��1��$��5,5,0,0q�$ 95Q�$ 5Q�y��� $ TriggerTypeComboBox��$	 SelectedItem�# Z $ Es�� �� $	 MyEsPercent��� $��2��$��0��$��0,5,0,0q�$��Center=�$��Rarity��$ Monster's rarity��y��� $ TriggerTypeComboBox��$	 SelectedItem�# Z $ MobsOrAttack���� $��2��$��1��$��5,5,0,0q�#��Z  �� $	 	MobRarity�y��� $ TriggerTypeComboBox��$	 SelectedItem�# Z $ MobsOrAttack���� $��3��$��0��$��0,5,0,0q�$��Center=�$��Range��$8 2Monsters must be within specified range around you��y��� $ TriggerTypeComboBox��$	 SelectedItem�# Z $
 Mobs��  $��3��$��1��$��5,5,0,0q�$	 250Q�$ 5Q�y��� $ TriggerTypeComboBox��$	 SelectedItem�# Z $
 Mobs�� �� $	 MobRange��� $��4��$��0��$��0,5,0,0q�$��Center=�$��Count��$$ Minimum monster count in range��y��� $ TriggerTypeComboBox��$	 SelectedItem�# Z $
 Mobs��  $��4��$��1��$��5,5,0,0q�$ 1Q�y��� $ TriggerTypeComboBox��$	 SelectedItem�# Z $
 Mobs�� �� $	 MobCount��� $��5��$��0��$��0,5,0,0q�$��Center=�$��Min HP%��$  Minimum monster HP percent��y��� $ TriggerTypeComboBox��$	 SelectedItem�# Z $ Attack��  $��5��$��1��$��5,5,0,0q�$ 95Q�$ 0Q�y��� $ TriggerTypeComboBox��$	 SelectedItem�# Z $ Attack�� �� $	 MobHpPercent��� -   $��0,10,0,0q�$
��Left=�$��Bottom=�$��Remove trigger��$��7(� ��  �� $ ItemsControlL�$	 DataContext��� $��	0,10,0,10q����#CF5A07�� $ AddTriggerButton��-   $��0,5,0,0q�$
��Left=�$��Bottom=�$��Add trigger��$��9(� �� $	 Triggers�9� $��0,15,0,0q�$��Debuff removal��.���� $ Vertical=�.+��� $��0,5,0,0q�$��Freeze�� d� 	IsChecked �� $	 RemoveFreeze��� $��0,5,0,0q�$��Shock�� �� $	 RemoveShock��� $��0,5,0,0q�$��Ignite�� �� $	 RemoveIgnite��� $��0,5,0,0q�$��Silence curse�� �� $	 RemoveSilence��� $��0,5,0,0q�$��Bleed while moving�� �� $	 RemoveBleed��� $��0,5,0,0q�$ 
Horizontal=�.+��� -    ��Name$ CbloodCheckBox��$��Corrupted Blood�� �� $	 RemoveCblood�  $��5,0,0,0q�$ 20Q�$ 1Q�{��� $ CbloodCheckBox��$	 	IsChecked� �� $	 MinCbloodStacks��� $��0,5,0,0q�$ 
Horizontal=�.+��� -   $ PoisonCheckBox��$��Poison�� �� $	 RemovePoison�  $��5,0,0,0q�$ 1Q�{��� $ PoisonCheckBox��$	 	IsChecked� �� $	 MinPoisonStacks�