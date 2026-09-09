# Séparation des préférences, de l’émulation et des mises à jour

## But

Remplacer la fenêtre `Options` qui rassemble actuellement des fonctions différentes par trois accès distincts :

- `Options > Préférences…` pour les réglages généraux de GW GUI ;
- un menu principal `Émulation` pour les préférences communes et les modules installés ;
- `Options > Mises à jour…` pour la version de GW GUI, la découverte, l’installation et la mise à jour des modules.

Le menu `Émulation` doit être construit à partir des modules réellement chargés. GW GUI ne doit contenir aucune entrée codée en dur pour Amiga, Atari ou un futur module. Les réglages, les configurations et les valeurs enregistrées doivent conserver leur fonctionnement actuel pendant le déplacement des écrans.

## Organisation visible retenue

### Menu Options

- `Préférences…`
- `Mises à jour…`
- les entrées existantes des journaux, diagnostics et outils matériels

### Menu Émulation

- `Préférences d’émulation…`
- un séparateur
- une entrée par module installé, avec son nom traduit par le module

`Préférences d’émulation…` ouvre la nouvelle fenêtre sur sa page générale. Une entrée de module ouvre la même fenêtre directement sur la page de ce module.

### Fenêtre Préférences

- Général
- Journaux
- Contrôleurs et lecteurs
- Moteurs
- Profils
- Manettes

### Fenêtre Émulation

- Général
- Raccourcis
- Configurations
- une page par module installé

### Fenêtre Mises à jour

- état et mise à jour de GW GUI ;
- catalogue des modules disponibles ;
- état et mise à jour de chaque module installé ;
- installation avancée depuis un ZIP ou l’URL directe d’un catalogue.

Les modules installés doivent être présentés par une navigation verticale afin de rester utilisables lorsque leur nombre augmente. Chaque état visible doit être explicite, accompagné d’une icône et d’une couleur cohérente : installé et à jour, mise à jour disponible, opération en cours ou erreur. Les icônes d’état restent génériques et ne doivent pas imposer de logo propre à une famille dans GW GUI.

## Tâches

- [x] 1. Inscrire la nouvelle organisation dans l’architecture
  - [x] 1.1 Décrire la responsabilité de chaque fenêtre
    - [x] Modifier `docs/architecture/overview.md` pour remplacer la fenêtre générale `Options` par les fenêtres `Préférences`, `Émulation` et `Mises à jour`, décrire leurs contenus respectifs et indiquer leurs entrées dans les menus de la fenêtre principale.
    - [x] Modifier `docs/architecture/emulation.md` pour décrire le menu principal `Émulation`, l’ouverture de la page générale ou de la page d’un module et la création dynamique des entrées à partir des modules chargés.
    - [x] Modifier `docs/architecture/emulation-module-updates.md` pour placer la découverte, l’installation et la mise à jour des modules dans la fenêtre dédiée `Mises à jour`, avec une navigation qui reste utilisable quel que soit le nombre de modules.

- [x] 2. Séparer les contrats de navigation des trois fenêtres
  - [x] 2.1 Remplacer le contrat de navigation commun aux options
    - [x] Renommer `src/GWGUI.App/Enums/Services/Navigation/OptionsSection.cs` en `PreferencesSection.cs`, renommer l’enum en `PreferencesSection` et ne conserver que `General`, `Logs`, `HostTools`, `Hardware`, `Engines`, `Profiles` et `Controllers`.
    - [x] Modifier `src/GWGUI.App/Interfaces/Services/Navigation/IWindowNavigationService.cs` pour remplacer `ShowOptions` par `ShowPreferences`, puis ajouter `ShowEmulationOptions(AppSettings settings, string? moduleId = null)` et `ShowUpdates()`.
    - [x] Modifier `src/GWGUI.App/Services/Windows/WpfWindowNavigationService.cs` pour créer et afficher séparément la fenêtre des préférences, la fenêtre d’émulation et la fenêtre des mises à jour, en leur donnant toujours la fenêtre principale comme propriétaire.
    - [x] Modifier `src/GWGUI.App/Controllers/MainWindow/MainWindowLifecycleController.cs` pour remplacer les appels existants à `ShowOptions` par `ShowPreferences` et conserver l’ouverture directe de la section matérielle lorsque le parcours actuel le demande.

- [x] 3. Limiter la fenêtre Préférences aux réglages généraux de GW GUI
  - [x] 3.1 Renommer et alléger la fenêtre existante
    - [x] Déplacer `src/GWGUI.App/Views/Windows/Options/OptionsWindow.xaml` vers `src/GWGUI.App/Views/Windows/Preferences/PreferencesWindow.xaml`, renommer la classe XAML et conserver uniquement les onglets Général, Journaux, Contrôleurs et lecteurs, Moteurs, Profils et Manettes avec leur contenu actuel.
    - [x] Déplacer `src/GWGUI.App/Views/Windows/Options/OptionsWindow.xaml.cs` vers `src/GWGUI.App/Views/Windows/Preferences/PreferencesWindow.xaml.cs`, renommer la classe et retirer la création, les événements, le rafraîchissement et la sélection des sections Émulation et Mises à jour tout en conservant l’enregistrement automatique actuel des préférences.
    - [x] Modifier `src/GWGUI.App/App.xaml.cs` pour rafraîchir les textes localisés d’une `PreferencesWindow` ouverte après un changement de langue.

- [x] 4. Créer la fenêtre dédiée aux préférences d’émulation et aux modules installés
  - [x] 4.1 Déplacer le contrôle d’émulation sans modifier ses fonctions
    - [x] Créer `src/GWGUI.App/Views/Windows/EmulationOptions/EmulationOptionsWindow.xaml` avec le titre localisé, le contrôle `OptionsEmulationSection`, un bouton Fermer et une taille adaptée aux réglages détaillés des machines.
    - [x] Créer `src/GWGUI.App/Views/Windows/EmulationOptions/EmulationOptionsWindow.xaml.cs` pour configurer `OptionsEmulationSection`, enregistrer les changements dans `settings.json`, préserver l’enregistrement automatique et adapter le titre au module et à la machine en cours de modification.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs` pour exposer une méthode sélectionnant la page générale ou la page correspondant à un identifiant de module, sans ajouter d’identifiant Amiga, Atari ou d’une autre famille dans App.
    - [x] Modifier `src/GWGUI.App/App.xaml.cs` pour rafraîchir les textes communs et ceux fournis par les modules dans toute `EmulationOptionsWindow` ouverte après un changement de langue.

- [x] 5. Ajouter le menu principal Émulation et ses modules dynamiques
  - [x] 5.1 Construire le menu depuis le registre des modules chargés
    - [x] Modifier `src/GWGUI.App/Views/Controls/Shell/MainMenu.xaml` pour ajouter le menu principal localisé `Émulation`, son entrée `Préférences d’émulation…`, son séparateur et le conteneur recevant les modules installés.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Shell/MainMenu.xaml.cs` pour créer une entrée par `IEmulationModule`, obtenir son libellé avec la localisation du module, conserver son identifiant dans l’entrée et émettre séparément les demandes d’ouverture générale et d’ouverture d’un module.
    - [x] Modifier `src/GWGUI.App/Views/Windows/Shell/MainWindow.xaml.cs` pour fournir les modules chargés à `MainMenu`, ouvrir `EmulationOptionsWindow` sur la page demandée et reconstruire les libellés dynamiques lors d’un changement de langue.

- [x] 6. Créer la fenêtre dédiée aux mises à jour
  - [x] 6.1 Sortir le gestionnaire de mises à jour des préférences
    - [x] Créer `src/GWGUI.App/Views/Windows/Updates/UpdatesWindow.xaml` avec un en-tête, une navigation verticale, une zone de contenu, les états illustrés et un bouton Fermer.
    - [x] Créer `src/GWGUI.App/Views/Windows/Updates/UpdatesWindow.xaml.cs` pour héberger `OptionsUpdatesSection`, créer `UpdateOptionsController`, gérer son cycle de vie et rafraîchir les textes localisés.
    - [x] Modifier `src/GWGUI.App/App.xaml.cs` pour rafraîchir les textes localisés de toute `UpdatesWindow` ouverte après un changement de langue.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Shell/MainMenu.xaml`, `MainMenu.xaml.cs` et `src/GWGUI.App/Views/Windows/Shell/MainWindow.xaml.cs` pour ajouter `Mises à jour…` sous `Options` et transmettre sa demande à `IWindowNavigationService.ShowUpdates()`.
  - [x] 6.2 Recomposer l’affichage du gestionnaire
    - [x] Modifier `src/GWGUI.App/Views/Controls/Options/OptionsUpdatesSection.xaml` pour séparer visuellement GW GUI, le catalogue des modules, les modules installés et l’installation avancée, présenter les modules installés dans la navigation verticale et afficher pour chaque élément son icône, sa version et son état explicite.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Options/OptionsUpdatesSection.xaml.cs` pour exposer les sélections de la navigation, conserver les événements actuels de recherche, d’installation et d’annulation et permettre l’affichage de la page du module installé sélectionné.
    - [x] Modifier `src/GWGUI.App/Options/Controllers/UpdateOptionsController.cs` pour alimenter les nouvelles pages sans modifier les services actuels de catalogue, d’installation ou de mise à jour, et pour distinguer visuellement les états installé et à jour, mise à jour disponible, opération en cours et erreur.

- [x] 7. Traduire tous les nouveaux textes
  - [x] 7.1 Ajouter les ressources communes puis toutes les traductions
    - [x] Modifier `src/GWGUI.App/Resources/00-Base/Menus.resx` et `src/GWGUI.App/Resources/00-Base/Options.resx` pour ajouter les titres des trois fenêtres, les commandes du menu Émulation, les rubriques de la fenêtre Mises à jour et les libellés de ses états, sans dupliquer les noms de modules, versions ou autres valeurs invariantes.
    - [x] Exécuter `scripts/translate-resx-argos.py` sur les nouvelles clés de `Menus.resx` et `Options.resx` afin de modifier tous les fichiers `src/GWGUI.App/Resources/*/Menus.resx` et `src/GWGUI.App/Resources/*/Options.resx` correspondant aux langues prises en charge.

- [x] 8. Adapter les tests permanents utiles
  - [x] 8.1 Vérifier la navigation et le caractère dynamique des modules
    - [x] Modifier `tests/GWGUI.Tests/Interface/Navigation/NavigationScenarios.cs` et `NavigationTests.cs` pour vérifier les trois demandes de fenêtre, l’ouverture générale du menu Émulation, l’ouverture directe d’un module et l’attribution correcte du propriétaire des fenêtres.
    - [x] Modifier `tests/GWGUI.Tests/Interface/Navigation/ControlContractScenarios.cs` pour vérifier que les nouveaux éléments de menu et les états des mises à jour ont des libellés localisés résolus et restent accessibles au clavier.
    - [x] Modifier `tests/GWGUI.Tests/Interface/SettingsViews/SettingsFailureScenarios.cs` et `SettingsViewsTests.cs` pour utiliser `PreferencesWindow` et vérifier la conservation du signalement d’échec d’enregistrement dans `EmulationOptionsWindow`.
    - [x] Ajouter `tests/GWGUI.Tests/Interface/Navigation/EmulationMenuScenarios.cs` et `EmulationMenuTests.cs` avec des modules factices en mémoire afin de vérifier qu’aucune famille n’est codée en dur, que zéro module ne crée aucune entrée et que plusieurs modules créent une entrée localisée ouvrant la bonne page.

- [x] 9. Vérifier l’ensemble après la séparation
  - [x] 9.1 Exécuter les contrôles automatisés
    - [x] Modifier `src/GWGUI.App/Views/Windows/Updates/UpdatesWindow.xaml.cs` et `tests/GWGUI.Tests/Interface/Navigation/ControlContractScenarios.cs` pour corriger l’accessibilité du constructeur détectée par la première compilation, avec un constructeur public sans dépendance interne et une surcharge interne réservée aux tests.
    - [x] Modifier `src/GWGUI.App/Views/Windows/Updates/UpdatesWindow.xaml.cs`, `src/GWGUI.App/Views/Controls/Options/OptionsUpdatesSection.xaml.cs` et `tests/GWGUI.Tests/Interface/Navigation/ControlContractScenarios.cs` pour exposer au test le contrat de navigation sans dépendre de la matérialisation de l’arbre visuel d’une fenêtre non affichée.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Options/OptionsUpdatesSection.xaml` pour rendre explicitement la navigation verticale accessible par tabulation, comme l’exige le contrôle multilingue du clavier.
    - [x] Exécuter les tests ciblés de `tests/GWGUI.Tests/Interface/Navigation` et `tests/GWGUI.Tests/Interface/SettingsViews`, puis inscrire leur résultat dans une section `Résultats de validation` ajoutée à `docs/tasks/interface/options-emulation-updates-navigation.md`.
    - [x] Exécuter l’audit de ressources utilisé par le projet et inscrire dans `docs/tasks/interface/options-emulation-updates-navigation.md` le nombre de cultures contrôlées ainsi que toute correction effectuée.
    - [x] Exécuter `scripts/build.ps1 -Configuration Debug`, vérifier la présence de `build/Debug/GW GUI/gwgui.exe` et inscrire le résultat dans `docs/tasks/interface/options-emulation-updates-navigation.md`.
  - [x] 9.2 Faire vérifier les trois parcours visibles
    - [x] Modifier `docs/tasks/interface/options-emulation-updates-navigation.md` après validation utilisateur pour consigner l’ouverture et l’usage de Préférences, des préférences générales d’émulation, des pages Amiga et Atari fournies par les modules installés et de la fenêtre Mises à jour.

## Résultats de validation

- 2026-09-09 — Tests ciblés `Interface/Navigation` et `Interface/SettingsViews` : 129 réussis, 0 échec, 0 ignoré.
- 2026-09-09 — Audit RESX Argos : 29 cultures, 22 catalogues et 42 592 entrées localisées contrôlés ; aucune correction supplémentaire nécessaire.
- 2026-09-09 — Build Debug propre : réussi ; `build/Debug/GW GUI/gwgui.exe` présent et dossier `Modules` créé vide.
- 2026-09-09 — Validation utilisateur : les fenêtres Préférences, Préférences d’émulation et Mises à jour s’ouvrent ; le catalogue installe Amiga et Atari ; le redémarrage automatique charge leurs entrées dynamiques et leurs paramètres. Les améliorations demandées après cette validation sont suivies séparément : téléchargement groupé avant un redémarrage unique, fenêtre indépendante par module et navigation verticale des machines.
