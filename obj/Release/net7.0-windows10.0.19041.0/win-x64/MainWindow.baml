   M S B A M L   `   `   `  ���� B  >RayCast, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null!clr-namespace:RayCastRayCast      RayCast.MainWindow   W SPresentationCore, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      R NSystem.Xaml, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     'localclr-namespace:RayCast     -     � Title$  RayCast��$	��450��$	��800�� � WindowStyle$
 None=� � AllowsTransparency$
 True��
������  � Topmost$
 True�� � ShowInTaskbar$
 True�� � WindowStartupLocation$ CenterScreen=� � 
ResizeMode$ NoResize=� � 	Resources    BackgroundColor �� Color  ForegroundColor  BorderColor  
HoverColor  SelectionColor  SubtitleColor3  -System.Windows.Media.Effects.DropShadowEffect  
GlowEffect	   Color
   
BlurRadius   ShadowDepth   Opacity �� 
TargetType �� Property �� Value �� Triggers P� Property P� Value  SeparatorStyle(   "RayCast.IconToImageSourceConverter  IconToImageSourceConverter'   !RayCast.NullToVisibilityConverter 	 NullToVisibilityConverter.   (RayCast.InverseNullToVisibilityConverter $
  InverseNullToVisibilityConverter*   $RayCast.BooleanToVisibilityConverter   BooleanToVisibilityConverter1   +RayCast.InverseBooleanToVisibilityConverter ' #InverseBooleanToVisibilityConverter)   #RayCast.StringToVisibilityConverter  StringToVisibilityConverter(   "RayCast.CountToVisibilityConverter  CountToVisibilityConverter  CustomScrollBarStyle �� 
TargetType ]�Name ]� Thumb �� CornerRadius J� Effect��%  &	        &	      &	 &     &	 9     &	 L     &	 _     &	 r     '�� �     7  7  &	 �    &	     &		     &	
     &	     &	      &	 %    &	 *    &	 /    �� $ #FFFFFF���� $ #000000���� $ #E0E0E0���� $ #F5F5F5���� $ #E0E0E0���� $ #666666��  $	 #f8bfbf��$
 10Q�$ 0Q�$ 1Q��� " ��.���  � ��
 ���� ��  � ��$ 0q���  � ��$	 0,2q���  � ��$ Stretch=���  � ��$ 0q� P�  �@��
IsSelected$
 True��. ���  � ��8   ��  � ��$ 0q�P�  �@J�IsMouseOver$
 True��. ���  � ��8  ��  � ��$ 0q��� " ��.���  � ��$
 10,2q���  � ��# �  ��  � ��$ 1��              �� " ��.���  � ��
 ���� ��  � �� �	#80FFFFFF��  � ��$ 8����  � �� �� " ��.R�]� $ 
PART_Track��$
��True�� n� ���� " n�.���  � �� �� " n�.R��� ���#f8bfbf
�������$��1q�$	��2,7q�$ 4��   $	 	#40FFFFFF��$
 2Q�$ 0Q�$ 1Q� � ContextMenu �� .@�m� -   $��Ouvrir��m� -   $��Quitter��.���  � RowDefinitions �� $
��Auto ��� $��* �.+��� -    ��Name$ 	SearchBox��$��0��$��10q�$��32,12,12,12q�$��16(�#���   #���  $��1q�#���  $��Center=�.������ " ��.���  � �� �� " ��.R��� #��z��#��z��#��z��$ 25��$
}�True��   $	 #f8bfbf��$
 0Q�$ 0Q�$ 1Q�._�� .+��� $$��Recherche ce dont tu as besoin��$��16(����#ce9797$��32,0,0,0q�$��Center=�y��� *Text.IsEmpty+ �� RelativeSource �� *TemplatedParent+ �� 	Converter# [ ��  ��Name$ PART_ContentHost��$��0,5,5,5q�$��Center=���  ��Name$ SettingsButton��-   $��Right=�$��0,0,10,0q�$��36��$��36��
������ 
������ #��[ ���� " ��.R�� .+�A�  A�Name$ BackgroundEllipse��#��z��#��z��$��1��F�   F�Name$  SettingsIcon������� � ��� � ��  � ��g5 �g5 � �  �� ��g5 �g5 ����  � ��g5 �g5 � � ��� ��g5 �g5 �pz(�� Ӱ)� ��)$� ��) � ���)�� �)�� pz(�ا Ѡ�H��� �ВK���  VL� `�J�5� ���+�iP � (*�M p	& �K �"�M ����(\�0@�P\ �@�� `]V ��� @-Q ��� �\M Ѡ@�  �$ �`�� �C! p� �� ��� �� р�� �� ��Ŕ �� ��� �C! `��  �$ �P� �\M � �� @-Q �uy `]V ��q �P\ ���K �M Ӑ,H  �K �D �M  9B �iP Ѡ�# �5� �и! � 0�" ��� `�% ��� ѐ�E �ا �PE ��  �D ��  �D  � � �D $� PE � ��E �� �`�% p<� �0�" `�� и! �� ��# p � � 9B ���D p'!�,H �_"��K p'!р�q  ���uy �� �� �P� P� �`�� �HIӠ�� `�L�Ŕ ��O��� ��Oр�� ��O�p� ��O`�� `�L�@� �HI���� P� ���� ��@�� �����(\�0@ ���"p'!�p	&�_" (*p'!��+��`�Jp � � VL�� ВK`�� ��Hp<� �pz(�� 	#��z��! �� Stretch$! Uniform=�$��20��$��20��" �� Triggers" P�  �@J�IsMouseOver$
 True��. ��� # �� 
TargetName$# BackgroundEllipse��# [  � ���� $# SettingsIcon��# [  � ��P�  � ��$
 True��. ��� $# BackgroundEllipse��# [  � ���� $# SettingsIcon��# [  � �� P�  �@J�	IsFocused$
 True��. ���  � �� �#fff5f5��  � �� �#ee8c8c��  � ��$ 1q���  �@J�Effect   $	 #ee8c8c��$
 10Q�$ 0Q�$ 1Q�.�� $ �� BasedOn��%   '��       7  �� " ��8$   .��� $��1�� �� $��* ��� $
��Auto �.+��� -   % ��Name$% ResultsList��$��0��$��
10,0,10,10q�#���   #���  $��1q�#���  $
��Auto=�$
��Auto��$��0��$	��600��& �� SelectionMode$& Single=�y��� ' �� ElementName$' ResultsList��( �� Path$( Items.Count�# [ ���� .R��� .+�� $��5q�) � ColumnDefinitions) �� $��32 ��� $��* �.+��� $��0��$��24��$��24��$��0,0,10,0q����� *	AppIcon+y��� *	AppIcon+# [	 �� $��0��$��20(�$��0,0,10,0q����� *Icon+y��� *	AppIcon+# [
 �� $��1��.+��� $��SemiBold"����� *Title+�� ���#666666$��12(����� *Description+�� #��[ ���� " ��.���  � �� �� " ��.R��� #��z��#��z��#��z��$ 25��   $	 #f8bfbf��$
 0Q�$ 0Q�$ 1Q�._��� $��0,5,5,5q�$
��Auto=�$|�False��.����  ��%   '��       7  �� " ��8$   �� -   * ��Name$* IAResponseBox��$��0��$��10q�$��15q�$��14(�#���   #���  $��1q�#���  + �� 
IsReadOnly$
+ True��", �� VerticalScrollBarVisibility$
, Auto=�$- �� HorizontalScrollBarVisibility$- Disabled=�y��� *
IsIAActive+# [ ���� " ��.���  � �� �� " ��.R��� #��z��#��z��#��z��$ 15��   $	 #f8bfbf��$
 0Q�$ 0Q�$ 1Q�._��� $ PART_ContentHost��$��0,5,5,5q�$
��Auto=�$��Disabled=� ��%   '��       7  �� " ��8$   �� $��1��. �� Orientation$. 
Horizontal=�$��Right=�$��	0,0,10,10q�