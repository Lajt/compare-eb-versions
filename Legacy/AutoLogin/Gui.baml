   M S B A M L   `   `   `  ���� W  SExilebuddy, Version=0.4.5619.1753, Culture=neutral, PublicKeyToken=8dde325853d11466;,clr-namespace:Exilebuddy;assembly=Exilebuddy
Exilebuddy  S OLegacy, Version=0.4.5619.1753, Culture=neutral, PublicKeyToken=1779d26ece6a7b373clr-namespace:Legacy.AutoLoginLegacy.AutoLogin    Legacy.AutoLogin.Gui   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     C
exilebuddy,clr-namespace:Exilebuddy;assembly=Exilebuddy     4	autoLoginclr-namespace:Legacy.AutoLogin       � 	Resources  *   $Exilebuddy.MultiBoolEnabledConverter   MultiBoolEnabledConverter��%   &	          .���� $
��Auto=�.���� (  "Legacy.AutoLogin.AutoLoginSettings   Instance � DataContext# Z .+��  � RowDefinitions �� $
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
��Auto � � ColumnDefinitions �� $	��200 ��� $
��Auto �$	��400���� $
��Auto �.+��� $��0��$��0��$��AutoLogin: ��$��3q� ^� ToolTip$� �Should Exilebuddy automatically log you in at the login screen? If this is unchecked, you will need to manually login on disconnects.���� -    ��Name$ CheckBoxAutoLogin��$��0��$��1��$��3,5,3,1q�$�� �� d� 	IsChecked ��  �� Path$ 	AutoLogin�	 �� Mode$	 TwoWay=��� $��1��$��0��$��MaxLoginAttempts: ��$��3,5,3,1q�$� }The max number of attempts to try logging in before stopping a bot for safety. Enter -1 if you do not want to limit attempts.���� $��1��$��1��$��3q����� $ MaxLoginAttempts�$	 TwoWay=�{��� $ 	AutoLogin�$	 OneWay=��� $��2��$��0��$!��LoginUsingUserCredentials: ��$��3q�$� �Should the bot auto-login using the Email, Password, and Gateway fields? If this is unchecked, the bot will used the stored client credentials to login with.��{��� $ 	AutoLogin�$	 OneWay=��� -   $' !CheckBoxLoginUsingUserCredentials��$��2��$��1��$��3,5,3,1q�$�� �� �� $ LoginUsingUserCredentials�$	 TwoWay=�{��� $ 	AutoLogin�$	 OneWay=��� $��3��$��0��$��Email: ��$��3,5,3,1q�$! Your Path of Exile account.���� $��3��$��1��$��3q����� $ Email�$	 TwoWay=�.��{�`� 
 `� 	Converter#
 [  .1���  �� ElementName$ CheckBoxAutoLogin��$ 	IsChecked��� $' !CheckBoxLoginUsingUserCredentials��$ 	IsChecked�.���� $��4��$��0��$��
Password: ��$��3,5,3,1q�$* $Your Path of Exile account password.���� $��4��$��1��$��3q� �� TextWrapping$
 Wrap=�$	��400������ $ Password�$	 TwoWay=�.��{�`� #
 [  .1��� $ CheckBoxAutoLogin��$ 	IsChecked��� $' !CheckBoxLoginUsingUserCredentials��$ 	IsChecked�.���� $��5��$��0��$��LoginUsingGateway: ��$��3q�$8 2Should the bot auto-login using the Gateway field?��{��� $ 	AutoLogin�$	 OneWay=��� -   $ CheckBoxLoginUsingGateway��$��5��$��1��$��3,5,3,1q�$�� �� �� $ LoginUsingGateway�$	 TwoWay=�{��� $ 	AutoLogin�$	 OneWay=��� $��6��$��0��$��	Gateway: ��$��3,5,3,1q�$  The Path of Exile gateway.���� $��6��$��1��$��3q�$
 Wrap=�$	��400������ $ Gateway�$	 TwoWay=�.��{�`� #
 [  .1��� $ CheckBoxAutoLogin��$ 	IsChecked��� $ CheckBoxLoginUsingGateway��$ 	IsChecked�.���� -    ��Name$ GatewayComboBox��$��6��$��2���� $��7��$��0��$��DelayBeforeLoginAttempt:��$��3,5,3,1q�$� �Should Exilebuddy wait a certain amount of time before each login attempt? If this is unchecked, login attempts will happen as soon as possible.���� -   $% CheckBoxDelayBeforeLoginAttempt��$��7��$��1��$��3,5,3,1q�$�� �� �� $ DelayBeforeLoginAttempt�$	 TwoWay=�{��� $ 	AutoLogin�$	 OneWay=��� $��8��$��0��$��LoginAttemptDelay:��$��	3,10,3,-4q�$� �How long should Exilebuddy wait before each login attempt if DelayBeforeLoginAttempt is checked. The format is Hours:Minutes:Seconds.��   Exilebuddy.TimePicker  $��8��$��1��$
��Left=�$	��1.5q�$��0,3,0,0q�   SelectedTime �� *LoginAttemptDelay+$	 TwoWay=�{�`� #
 [  .1��� $ CheckBoxAutoLogin��$ 	IsChecked��� $% CheckBoxDelayBeforeLoginAttempt��$ 	IsChecked��  �� $
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
��Auto � �� $	��200 ��� $
��Auto �$	��450��.+��� $��0��$��0��$��AutoSelectCharacter: ��$��3q�$  ���� -   $! CheckBoxAutoSelectCharacter��$��0��$��1��$��3,5,3,1q�$�� �� �� $ AutoSelectCharacter�$	 TwoWay=��  �� $
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
��Auto � �� $	��200 ��� $
��Auto �$	��450���� $��70 �.+��� $��0��$��0��$��Characters: ��$��3q�$  ���� -   $ CharactersComboBox��$��0��$��1���� -    ��Name$ LoadCharactersButton��$��0��$��2��$��Refresh���  �� $
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
��Auto � �� $	��200 ��� $
��Auto �$	��450��.+��� $��1��$��0��$��Character: ��$��3,5,3,1q�$P JThe name of the character to login with if AutoSelectCharacter is checked.���� $��1��$��1��$��3q����� $ 	Character�$	 TwoWay=�{��� $ AutoSelectCharacter�$	 OneWay=��� $��2��$��0��$$��DelayBeforeSelectingCharacter:��$��3,5,3,1q�$s mShould Exilebuddy wait before selecting a character? If this is unchecked, it will login as soon as possible.���� -	   $+ %CheckBoxDelayBeforeSelectingCharacter��$��2��$��1��$��3,5,3,1q�$�� �� �� $# DelayBeforeSelectingCharacter�$	 TwoWay=�{��� $ AutoSelectCharacter�$	 OneWay=��� $��3��$��0��$��SelectCharacterDelay:��$��	3,10,3,-4q�$� �How long should Exilebuddy wait before selecting a character to login with if DelayBeforeSelectingCharacter is checked. The format is Hours:Minutes:Seconds.��  $��3��$��1��$
��Left=�$	��Top=�$	��1.5q�$��0,3,0,0q� �� *SelectCharacterDelay+$	 TwoWay=�{�`� #
 [  .1��� $! CheckBoxAutoSelectCharacter��$ 	IsChecked��� $+ %CheckBoxDelayBeforeSelectingCharacter��$ 	IsChecked�