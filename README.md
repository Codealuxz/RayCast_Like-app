<<<<<<< HEAD
# RayCast v1.1.0
=======
# RayCast
>>>>>>> b759362d32535175e990742b02ab9f1f12ceaced

RayCast est une application de lancement rapide pour Windows, inspirée de Spotlight (macOS) et Raycast (macOS). Elle permet d'accéder rapidement à vos applications, fichiers et fonctionnalités système.

## Fonctionnalités

- 🔍 Recherche rapide d'applications installées
- 🌐 Recherche web intégrée (Google, Ecosia, Bing, DuckDuckGo, Qwant)
- 🤖 Intégration avec Gemini AI pour des suggestions intelligentes
- 🧮 Calculatrice intégrée
- ⌨️ Raccourcis clavier personnalisables (ne marche pas)
- 🎨 Thèmes clair et sombre
- 💻 Interface minimaliste et élégante

## Raccourcis clavier

- `Ctrl + Espace` : Ouvrir/fermer RayCast
- `Échap` : Fermer la fenêtre
- `Entrée` : Exécuter l'élément sélectionné

## Installation avec installeur

1. Téléchargez la dernière version depuis la section Releases
2. Exécutez le fichier d'installation
3. L'application se lancera automatiquement


## Installation sans installeur

1. Téléchargez la dernière version depuis la section Releases
2. Extrayez l'archive dans le dossier de votre choix
3. Exécutez `RayCast.exe`
4. Configurez votre clé API Gemini :
<<<<<<< HEAD
   - Rendez-vous sur [Google AI Studio](https://aistudio.google.com/app/apikey)
   - Connectez-vous avec votre compte Google (ou créez-en un)
   - Cliquez sur « Créer une clé API »
   - Copiez la clé générée (ex : AIza...)
   - Ouvrez le fichier `config.json` dans le dossier de l'application.
   - Ajoutez votre clé Gemini ici :
   ```json
   "GeminiApiKey": "VOTRE_CLE_GEMINI_ICI"
=======
   - Le fichier ``SettingsWindow.xaml.cs``
   - Ajouter votre api key de Gemini ici : 
   ```c#
   private const string GEMINI_API_KEY = "REPLACE_BY_YOUR_GEMINI_API_KEY";
>>>>>>> b759362d32535175e990742b02ab9f1f12ceaced
   ```
5. Puis recompilez et lancer utiliser la commande : ``.\run.bat``


## Configuration

L'application peut être configurée via le menu contextuel de l'icône dans la barre des tâches :

- Thème (clair/sombre)
- Moteur de recherche par défaut
- Affichage dans la barre des tâches

<<<<<<< HEAD

**Attention :** Ne partagez jamais votre clé publiquement.

=======
>>>>>>> b759362d32535175e990742b02ab9f1f12ceaced
## Développement

### Prérequis

- Visual Studio 2022
- .NET 6.0 SDK
- Windows 10 ou supérieur

### Compilation

1. Clonez le dépôt
2. Ouvrez la solution dans Visual Studio
3. Restaurez les packages NuGet
4. Compilez le projet

## Licence

Ce projet est sous licence MIT. Voir le fichier LICENSE pour plus de détails.

## Contribution

Les contributions sont les bienvenues ! N'hésitez pas à ouvrir une issue ou à soumettre une pull request.

## Support

Si vous rencontrez des problèmes ou avez des suggestions, n'hésitez pas à ouvrir une issue sur GitHub. 