   M S B A M L   `   `   `  ���� S  OLegacy, Version=0.4.5619.1753, Culture=neutral, PublicKeyToken=1779d26ece6a7b37=#clr-namespace:Legacy.ObjectExplorerLegacy.ObjectExplorer      Legacy.ObjectExplorer.Gui   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     >objectExplorer#clr-namespace:Legacy.ObjectExplorer     .��� 2   ,Legacy.ObjectExplorer.ObjectExplorerSettings    Instance � DataContext# Z   � ColumnDefinitions �� -    ��Name$ LeftColDefinition���� -   $ SplitterColDefinition��$
��Auto ��� -   $ RightColDefinition��.+��� -    ��Name$ GridSplitter��
���   � �� ResizeDirection$ Columns=�$��1��$��Stretch=�$��Stretch=� �� ResizeBehavior$ PreviousAndNext=�$��2��� $��0��.+��� -    ��Name$ ButtonRefresh��$��Refresh All��$��Left, Stretch=�$��
10,10,10,0q�$	��Top=�$
��Auto��R� -    R�Name$ TreeViewObjects��$��Left, Stretch=�$��Top, Stretch=�$��10,35,10,10q�� $��2��.+��� -   $ ButtonRefreshObject��$��Refresh��$��Left, Stretch=�$
��Auto��$	��Top=�$��
10,10,10,0q��� -   $ 
ButtonSave��$
��Save��$��Left, Stretch=�$��Bottom=�$��
10,0,10,10q�$
��Auto���� $��Left, Stretch=�$��Top, Stretch=�$��10,35,10,40q�.���� -	   	 ��Name$	 
tabControl��$��Left, Stretch=�$��Top, Stretch=�.@��� $	��Raw��.���� -
   
 ��Name$
 TextBoxInfoRaw�� �� 
IsReadOnly .  �� AcceptsReturn .  �� TextWrapping$ NoWrap=�