   M S B A M L   `   `   `  ���� \  XHelixToolkit.Wpf, Version=2015.1.715.0, Culture=neutral, PublicKeyToken=52aa3500039caf0dM8clr-namespace:HelixToolkit.Wpf;assembly=HelixToolkit.WpfHelixToolkit.Wpf  
 Legacy   Legacy.AreaVisualizer.Gui   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     Fh8clr-namespace:HelixToolkit.Wpf;assembly=HelixToolkit.Wpf     .��� .+�&    HelixToolkit.Wpf.HelixViewport3D  -      Name$  	HelixView��   ZoomExtentsWhenLoaded . $
��auto��$
��auto��   Children.    HelixToolkit.Wpf.SunLight  $   HelixToolkit.Wpf.DefaultLights  (   "HelixToolkit.Wpf.GridLinesVisual3D     Width$ 8Q�   Length$ 8Q�   MinorDistance$ 1Q�   MajorDistance$ 1Q�   	Thickness$
 0.01Q�9� $��Options��$��	10,10,0,0q�$	��Top=�$	��512��$
��Left=�$	��248��.��� .+��� $��3q� �� CanVerticallyScroll . .+��� -   	 ��Name$	 ReloadExilePatherButton��$	��120��$
��Left=�$��10q�$��Reload ExilePather���� -   $	 RefreshButton��$	��120��$
��Left=�$��10q�$��Refresh���� -   
 ��Name$
 LockCameraToPlayer��$��Lock Camera To Player�� d� 	IsChecked$ FalseY��� -   $
 ShowLocalPlayer��$��Show Player��$
 TrueY��� -   $
 ShowNavMesh��$��Show NavMesh��$
 TrueY��� -   $
 ShowNavGrid��$��Show NavGrid��$
 TrueY��� $��	Controls:���� $��  RMB: Rotate���� $��  MMB: Zoom���� $��  Shift+RMB: Pan���� $��  Ctrl+RMB: Zoom���� $��  Home: Zoom Extents���� $&��   Double Click MMB: Reset Camera���� $(��"  Ctrl+U/D/L/R/F/B: Look From Side���� -    ��Name$ PlayerLocationText��$��Player Location: 0, 0���� -	   $ CameraPositionText��$��Camera Position: 0, 0, 0���� -
   $ CameraLookDirectionText��$$��Camera Look Direction: 0, 0, 0���� $y�Hidden=�$��Pathfind To:��� $y�Hidden=�$��65��.+��� $��X��$
��Left=�$��	10,10,0,0q�$	��Top=��� -    ��Name$ 	PathfindX��$
��Left=�$��23��$��22,7,0,0q� �� TextWrapping$
 Wrap=�$�� ��$	��Top=�$��48���� $��Y��$
��Left=�$��	75,10,0,0q�$	��Top=��� -   $ 	PathfindY��$
��Left=�$��23��$��87,7,0,0q�$
 Wrap=�$�� ��$	��Top=�$��48���� $��Wall Distance��$
��Left=�$��	10,35,0,0q�$	��Top=��� -   $ PathfindWallDistance��$
��Left=�$��23��$��	87,35,0,0q�$
 Wrap=�$��7��$	��Top=�$	��120��