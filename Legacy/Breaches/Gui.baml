   M S B A M L   `   `   `  ���� S  OLegacy, Version=0.4.5619.1753, Culture=neutral, PublicKeyToken=1779d26ece6a7b371clr-namespace:Legacy.BreachesLegacy.Breaches      Legacy.Breaches.Gui   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     2breachesclr-namespace:Legacy.Breaches     .���� $
��Auto=�.���� &    Legacy.Breaches.BreachesSettings    Instance � DataContext# Z  .+��  � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � � ColumnDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��	Enabled: ��$��3q� ^� ToolTip$P JShould Breach logic run? If false, Breaches will be skipped when possible.���� -    ��Name$ CheckBoxEnabled��$��0��$��1��$��3,5,3,1q�$�� �� d� 	IsChecked ��  �� Path$ Enabled� �� Mode$ TwoWay=��� $��1��$��0��$��Open: ��$��3q�$Y SShould Breaches be interacted with? If false, the bot will stand near them instead.���� -   $ CheckBoxOpen��$��1��$��1��$��3,5,3,1q�$�� �� �� $
 Open�$ TwoWay=�