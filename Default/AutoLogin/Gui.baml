   M S B A M L   `   `   `  ���� T  PDefault, Version=0.4.5619.1753, Culture=neutral, PublicKeyToken=1779d26ece6a7b375clr-namespace:Default.AutoLoginDefault.AutoLogin      Default.AutoLogin.Gui   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     1localclr-namespace:Default.AutoLogin     U QMahApps.Metro, Version=1.5.0.23, Culture=neutral, PublicKeyToken=f4fb5a3c4d1e5b4f:mahapps,http://metro.mahapps.com/winfx/xaml/controls  Dmc;http://schemas.openxmlformats.org/markup-compatibility/2006  .����     Default.AutoLogin.Settings    Instance � DataContext# Z  $
��Auto=�$
��Auto=�.���� $��5,5,0,0q�.+���  �� Orientation$ 
Horizontal=�.+��� $��	Character�� � ToolTip$+ %Name of your Path of Exile character.���� -    ��Name$ CharacterTextbox��$��75��$��5,0,0,0q�����  �� Path$ 	Character��� -    ��Name$ SetCharNameButton��$��10,0,0,0q�$��Set to current��$��Normal"�$> 8Get the name of the character you are currently playing.���� $
��Left=�$	��Top=�.+��� $��0,25,0,0q�$��Delays��.���  � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � � ColumnDefinitions �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��Login delay initial (sec)��$1 +Initial delay before doing a login attempt.��*  $MahApps.Metro.Controls.NumericUpDown  $��0��$��1��$��5,0,0,0q�	   HasDecimals	 . 
   Interval$	
 0.5Q�   Minimum$	 0.5Q�   Value �� $ LoginDelayInitial��� $��1��$��0��$��0,5,0,0q�$!��Login delay increment (sec)��$d ^Additional delay which will be added to the initial one after each unsuccessful login attempt.��  $��1��$��1��$��5,5,0,0q�	 . $ 0Q� �� $ LoginDelayStep��� $��2��$��0��$��0,5,0,0q�$��Login delay final (sec)��$, &Delay time cannot go higher than this.��  $��2��$��1��$��5,5,0,0q�	 .  �� $ LoginDelayFinal��� $��3��$��0��$��0,5,0,0q�$#��Login delay random factor (%)��$> 8Delay time will be randomized according to this percent.��  $��3��$��1��$��5,5,0,0q�   Maximum$	 100Q�$ 0Q� �� $ LoginDelayRandPct��� $��4��$��0��$��0,5,0,0q�$%��Character selection delay (sec)��$7 1Delay before doing a character selection attempt.��  $��4��$��1��$��5,5,0,0q�	 . $	
 0.5Q�$	 0.5Q� �� $ CharSelectDelay��� $��0,20,0,0q�$��Authentication��.���  �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+��� -    ��Name$ UseCredentialsCheckbox��$��0��$��0��$��2��$��Login using credentials��$� �Should the bot auto-login using the Email, Password, and Gateway fields? If this is unchecked, the bot will used the stored client credentials to login with.�� d� 	IsChecked �� $ LoginUsingUserCredentials��� $��1��$��0��$��0,10,0,0q�$��Email��$! Your Path of Exile account.��{���  �� ElementName$ UseCredentialsCheckbox��$ 	IsChecked��� $��1��$��1��$	��150��$��5,10,0,0q�{��� $ UseCredentialsCheckbox��$ 	IsChecked����� $ Email��� $��2��$��0��$��0,5,0,0q�$��Password��$* $Your Path of Exile account password.��{��� $ UseCredentialsCheckbox��$ 	IsChecked��� $��2��$��1��$	��150��$��5,5,0,0q�{��� $ UseCredentialsCheckbox��$ 	IsChecked����� $ Password��� -   $ 
UseGateway��$��3��$��0��$��2��$��0,10,0,0q�$��Login using gateway��$8 2Should the bot auto-login using the Gateway field?�� �� $ LoginUsingGateway��� $��4��$��0��$��0,10,0,0q�$��Gateway��$  The Path of Exile gateway.��{��� $ 
UseGateway��$ 	IsChecked��� $��4��$��1��$��5,10,0,0q�   GatewayList#��Z {��� $ 
UseGateway��$ 	IsChecked� �� SelectedItem �� $ Gateway�