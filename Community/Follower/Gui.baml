   M S B A M L   `   `   `  ���� V  RCommunity, Version=0.4.5619.1753, Culture=neutral, PublicKeyToken=1779d26ece6a7b377 clr-namespace:Community.FollowerCommunity.Follower      Community.Follower.Gui   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     5follower clr-namespace:Community.Follower     U QMahApps.Metro, Version=1.5.0.23, Culture=neutral, PublicKeyToken=f4fb5a3c4d1e5b4f:mahapps,http://metro.mahapps.com/winfx/xaml/controls  .���� $
��Auto=�.���� )   #Community.Follower.FollowerSettings    Instance � DataContext# Z  .+��  � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � � ColumnDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��Leader's Character Name: ��$��3,5,3,1q� ^� ToolTip$8 2The name of the character to follow as the leader.���� $��0��$��1��$��3q�����  �� Path$ Leader� �� Mode$ TwoWay=��� $��1��$��0��$��Follow Distance: ��$��3,5,3,1q�$F @The max distance allowed before moving towards the leader again.��*  $MahApps.Metro.Controls.NumericUpDown  $��1��$��1��$��3q�   Value �� $ FollowDistance�$ TwoWay=��� $��2��$��0��$��Stop Outside Boss Door: ��$��3,5,3,1q�$0 *Should the bot stop outside the boss door?���� $��2��$��1��$��3q� d� 	IsChecked �� $ StopOutsideBossDoor�$ TwoWay=�