   M S B A M L   `   `   `  ����   RayCast    RayCast.SettingsWindow   W SPresentationCore, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      R NSystem.Xaml, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$  Paramètres��$	��600��$	��400�� � WindowStartupLocation$ CenterScreen=�   BackgroundColor#���     ForegroundColor#���   � 	Resources  �� 
TargetType �� Property �� Value �� Triggers P� Property P� Value  SelectionColor  
HoverColor  BorderColor	 �� 
TargetType��%  '��       '�� 5     '�� �     '�� 0    �� " ��.���  � ��# �  ��  � ��# �   �� " ��.���  � ��# �  ��  � ��# �    P�  �@��
IsSelected$
 True��. ���  � ��# �  ��  � ��
 �   �P�  �@J�IsMouseOver$
 True��. ���  � ��# �  �� " ��.���  � ��# �  ��  � ��# �   �� " ��.���  � ��$ Stretch=���  � ��$ 1����  � ��$ 0,8,0,8q���  � ��# �  ��  � �� �� "	 ��.R��� #��z��#��z��$��Stretch=�.��� $��10q�#���   
 � RowDefinitions
 �� $��* ��� $
��Auto �.+��� $��0��$
��Auto=�#���   .���� $��0,0,0,10q�#���   .+��� $��Moteur de recherche��$��0,0,0,10q�#���   #���  .��� #���    � ColumnDefinitions �� $
��Auto ��� $��* �.+��� $��Moteur par défaut :��$��0��#���  �� -    ��Name$ SearchEngineComboBox��$��1��$��5q�#���   #���  .@��� $��Google��#���  �� $
��Bing��#���  �� $��
DuckDuckGo��#���  �� $��Ecosia��#���  �� $��Qwant��#���  � .+��� $
��Auto��$��Stretch=�$��1��#���  �� $��	Apparence��$��0,0,0,10q�#���   #���  .��� #���    �� $
��Auto ��� $��* �.+��� $��Thème :��$��0��#���  �� -   $ ThemeComboBox��$��1��$��5q�#���   #���  .@��� $��Light���� $
��Dark���� $��System��� .+��� $
��Auto��$��Stretch=�$��1��#���  �� $��
Raccourcis��$��0,0,0,10q�#���   #���  .��� #���   
 �� $
��Auto ��� $
��Auto � �� $
��Auto ��� $��* �.+��� -    ��Name$ EnableHotkeyCheckBox��$��Activer le raccourci��$��0��$��0��$��2��$��0,0,0,5q�#���  �� $��Raccourci :��$��1��$��0��#���  �� $��1��$��1�� �� Orientation$ 
Horizontal=�.+��� -    ��Name$ HotkeyTextBox��$	��120��$��5,0,0,0q� �� 
IsReadOnly$
 True��#���   #���  {��� *	IsChecked+ �� ElementName$ EnableHotkeyCheckBox���� -    ��Name$ SetHotkeyButton��$��Définir...��$��70��$��5,0,0,0q�#���   #���  � .+��� $
��Auto��$��Stretch=�$��1��#���  �� $��
Démarrage��$��0,0,0,10q�#���   #���  .��� #���    �� $
��Auto ��� $��* �.+��� -   $ StartupCheckBox��$%��Lancer au démarrage de Windows��$��0��$��2��#���  � .+��� $
��Auto��$��Stretch=�$��1��#���  �� $&�� Ordre d'affichage des résultats��$��0,0,0,10q�#���   #���  .��� #���    �� $
��Auto ��� $��* �.+��� $��Ordre :��$��0��#���  �� -   $ OrderComboBox��$��1��$��5q�#���   #���  .@��� $��IA,Application,Web���� $��IA,Web,Application���� $��Application,IA,Web���� $��Application,Web,IA���� $��Web,IA,Application���� $��Web,Application,IA��� .+��� $
��Auto��$��Stretch=�$��1��#���  �� $��Thème de l'IA��$��0,0,0,10q�#���   #���  .��� #���    �� $
��Auto ��� $��* �.+��� $��Thème IA :��$��0��#���  �� -   $ AIThemeComboBox��$��1��$��5q�#���   #���  .@��� $��Light���� $
��Dark���� $��1��$ 
Horizontal=�$��Right=�#���   .+��� -	   $��Enregistrer��$��80��$��0,0,10,0q�#���   #���  �� -
   $��Annuler��$��80��#���   #���  