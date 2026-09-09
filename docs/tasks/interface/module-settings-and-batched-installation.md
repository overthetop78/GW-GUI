# Installation groupée des modules et fenêtres de paramètres par module

## But

Permettre de télécharger plusieurs modules d’émulation depuis la fenêtre `Mises à jour` sans fermer immédiatement GW GUI, puis terminer toutes leurs installations avec un seul redémarrage. Un module préparé porte temporairement l’état `Téléchargé`; le bouton court `Finir l’installation` lance l’updater pour tous les modules préparés, ferme GW GUI, applique la transaction et redémarre l’application. Après ce redémarrage, l’état existant reste `Installé`.

Séparer également les préférences communes d’émulation des paramètres propres aux modules. `Émulation > Préférences d’émulation…` conserve seulement Général, Raccourcis et Configurations. Chaque autre entrée dynamique du menu `Émulation` ouvre une fenêtre indépendante utilisant le contrôle générique actuel avec les données du module demandé.

Dans la fenêtre d’un module, la liste verticale des machines remplace le sélecteur de modèle. Les couleurs existantes continuent d’indiquer qu’une machine possède déjà une configuration enregistrée et qu’elle est utilisable. La partie droite affiche directement les onglets actuels de la machine. Dans l’onglet Général, un sélecteur affiche uniquement l’émulateur actuellement fourni pour cette machine, avec `Installer` lorsqu’il manque ou `Installé` lorsqu’il est présent.

## Limites retenues

- Aucun nombre de modules et aucune description longue ne figurent dans le bouton `Finir l’installation`.
- `Téléchargé` est uniquement l’état transitoire avant le redémarrage; `Installé` reste l’état définitif existant.
- Les onglets et réglages actuels des modules sont déplacés sans être reconstruits ni supprimés.
- La fenêtre d’un module ne contient aucun niveau Général, Raccourcis ou Configurations au-dessus de ses propres onglets.
- Aucun nouvel émulateur interne, externe ou alternatif n’est ajouté dans ce travail.
- Le choix entre plusieurs moteurs et les options différentes propres à chacun seront étudiés ultérieurement. Le sélecteur créé ici affiche uniquement le moteur actuellement associé à la machine par son module.
- GW GUI ne contient aucune condition propre à Amiga, Atari ou une future famille.

## Tâches

- [x] 1. Inscrire les nouveaux parcours dans l’architecture
  - [x] 1.1 Décrire l’installation différée et les deux niveaux de paramètres d’émulation
    - [x] Modifier `docs/architecture/emulation-module-updates.md` pour remplacer le redémarrage immédiat après chaque téléchargement par une file de modules préparés, l’état transitoire `Téléchargé`, le bouton `Finir l’installation` et une seule transaction suivie d’un seul redémarrage.
    - [x] Modifier `docs/architecture/emulation.md` pour séparer la fenêtre des préférences communes de la fenêtre générique d’un module, décrire l’ouverture dynamique par identifiant de module et préciser que cette fenêtre affiche directement la navigation verticale des machines et leurs onglets actuels.
    - [x] Modifier `docs/architecture/overview.md` pour distinguer `Émulation > Préférences d’émulation…` des entrées de modules qui ouvrent chacune leur propre boîte de dialogue.

- [x] 2. Préparer plusieurs installations avant de lancer l’updater
  - [x] 2.1 Représenter les modules téléchargés pendant la session de GW GUI
    - [x] Créer `src/GWGUI.App/Contracts/Updates/PendingModuleInstallation.cs` avec l’identifiant, le nom affiché, la version, l’URL de catalogue et le composant validé nécessaire à la future transaction, sans dépendance graphique.
    - [x] Créer `src/GWGUI.App/Services/Updates/PendingModuleInstallationStore.cs` pour conserver une seule préparation par identifiant de module pendant l’exécution de GW GUI, exposer la liste courante, signaler ses changements et supprimer proprement les répertoires temporaires abandonnés.
    - [x] Modifier `src/GWGUI.App/Services/Updates/UpdatePackagePreparationService.cs` pour séparer la validation et l’extraction d’un nouveau module de la création du plan de lancement, puis créer un seul `PreparedUpdateLaunch` à partir de tous les composants préparés par le magasin.
    - [x] Modifier `src/GWGUI.App/Services/Updates/ModuleInstallationService.cs` pour retourner une préparation ajoutable au magasin sans créer ni lancer immédiatement un updater, tout en conservant les validations actuelles d’identité, de version, d’URL, de compatibilité et de SHA-256.
  - [x] 2.2 Partager la file entre toutes les ouvertures de la fenêtre Mises à jour
    - [x] Modifier `src/GWGUI.App/Views/Windows/Shell/MainWindow.xaml.cs` pour créer un seul `PendingModuleInstallationStore` pour la durée de la fenêtre principale, le transmettre au service de navigation et fournir sa méthode de nettoyage au contrôleur de cycle de vie.
    - [x] Modifier `src/GWGUI.App/Services/Windows/WpfWindowNavigationService.cs` pour recevoir le `PendingModuleInstallationStore` partagé et le transmettre à chaque nouvelle `UpdatesWindow`.
    - [x] Modifier `src/GWGUI.App/Views/Windows/Updates/UpdatesWindow.xaml.cs` pour recevoir le magasin partagé, le transmettre à `UpdateOptionsController` et libérer ses abonnements lors de la fermeture sans supprimer les téléchargements encore utiles à une réouverture de la fenêtre.
    - [x] Modifier `src/GWGUI.App/Controllers/MainWindow/MainWindowLifecycleController.cs` pour appeler le nettoyage fourni par la fenêtre principale lors de la fermeture définitive de GW GUI, sans lancer implicitement les installations préparées.

- [x] 3. Afficher l’état `Téléchargé` et terminer les installations ensemble
  - [x] 3.1 Adapter le contrôleur et la liste des modules disponibles
    - [x] Modifier `src/GWGUI.App/Options/Controllers/UpdateOptionsController.cs` pour ajouter chaque préparation au magasin, laisser GW GUI ouvert, interdire le second téléchargement du même module et reconstruire les lignes du catalogue avec les états distincts disponible, téléchargement en cours, téléchargé et installé.
    - [x] Modifier `src/GWGUI.App/Options/Controllers/UpdateOptionsController.cs` pour déplacer la confirmation de fermeture et de redémarrage vers la commande `Finir l’installation`, créer un plan unique avec tous les modules téléchargés et appeler une seule fois `LaunchUpdater`.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Options/OptionsUpdatesSection.xaml.cs` pour exposer la commande `Finir l’installation`, son activation lorsque le magasin contient au moins un module et le rafraîchissement des lignes lorsque la file change.
  - [x] 3.2 Adapter la présentation sans modifier les autres pages de mise à jour
    - [x] Modifier `src/GWGUI.App/Views/Controls/Options/OptionsUpdatesSection.xaml` pour afficher le badge court `Téléchargé` sur les modules préparés et un bouton global court `Finir l’installation`, visible uniquement lorsqu’au moins une installation attend le redémarrage.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Options/OptionsUpdatesSection.xaml` pour conserver exactement le badge `Installé` après redémarrage et les boutons `Installer` des modules encore absents.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Options/OptionsUpdatesSection.xaml` pour afficher près du bouton global une explication brève indiquant que terminer l’installation fermera et redémarrera GW GUI, sans allonger le texte du bouton.

- [x] 4. Séparer les préférences communes des paramètres propres aux modules
  - [x] 4.1 Limiter la fenêtre commune à Général, Raccourcis et Configurations
    - [x] Renommer `src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs` en `EmulationPreferencesSection.cs`, renommer la classe et retirer la création, la sélection et le cycle de vie des onglets de modules tout en conservant les pages Général, Raccourcis et Configurations.
    - [x] Renommer `src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionLayoutFunctions.cs`, `OptionsEmulationSectionSettingsFunctions.cs` et `OptionsEmulationSectionConfigurationFunctions.cs` avec le préfixe `EmulationPreferencesSection`, puis adapter leurs classes partielles et leurs références sans modifier leur comportement.
    - [x] Renommer `src/GWGUI.App/Views/Windows/EmulationOptions/EmulationOptionsWindow.xaml` et `EmulationOptionsWindow.xaml.cs` en `EmulationPreferencesWindow.xaml` et `EmulationPreferencesWindow.xaml.cs`, héberger `EmulationPreferencesSection` et conserver l’enregistrement automatique actuel des réglages communs.
  - [x] 4.2 Créer une boîte de dialogue générique pour un module
    - [x] Créer `src/GWGUI.App/Views/Windows/EmulationModuleOptions/EmulationModuleOptionsWindow.xaml` avec une zone unique pour `EmulationModuleSettingsSection`, un bouton Fermer et aucun onglet commun au-dessus du contenu du module.
    - [x] Créer `src/GWGUI.App/Views/Windows/EmulationModuleOptions/EmulationModuleOptionsWindow.xaml.cs` pour recevoir un `IEmulationModule`, construire le contrôle générique existant, conserver ses événements de sauvegarde et de vidéo et produire le titre localisé `Paramètres d’émulation {nom du module}`.
    - [x] Modifier `src/GWGUI.App/App.xaml.cs` pour rafraîchir séparément les textes d’une `EmulationPreferencesWindow` et ceux d’une `EmulationModuleOptionsWindow`, y compris les ressources fournies par le module.

- [x] 5. Faire ouvrir la bonne fenêtre par le menu Émulation
  - [x] 5.1 Séparer les demandes de navigation communes et propres aux modules
    - [x] Modifier `src/GWGUI.App/Interfaces/Services/Navigation/IWindowNavigationService.cs` pour remplacer `ShowEmulationOptions(AppSettings, string?)` par `ShowEmulationPreferences(AppSettings)` et `ShowEmulationModuleOptions(AppSettings, string moduleId)`.
    - [x] Modifier `src/GWGUI.App/Services/Windows/WpfWindowNavigationService.cs` pour ouvrir `EmulationPreferencesWindow` depuis l’entrée générale, résoudre l’identifiant demandé dans `EmulationModuleRegistry` et ouvrir `EmulationModuleOptionsWindow` avec le module correspondant.
    - [x] Modifier `src/GWGUI.App/Views/Windows/Shell/MainWindow.xaml.cs` pour envoyer l’entrée `Préférences d’émulation…` vers la fenêtre commune et chaque entrée dynamique de module vers sa boîte de dialogue indépendante.

- [x] 6. Remplacer le sélecteur de modèle par une navigation verticale des machines
  - [x] 6.1 Réutiliser les machines et leurs états existants
    - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs` pour remplacer la `ComboBox` des modèles par une `ListBox` verticale alimentée par les mêmes `EmulationMachineChoice`, afficher à droite les onglets actuels de la machine sélectionnée et conserver les opérations actuelles de création, chargement et sauvegarde de configuration.
    - [x] Modifier `src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationMachineChoiceLayout.cs` pour fournir le modèle et le style de la liste verticale, reprendre les couleurs actuelles lorsque `HasSavedConfiguration` vaut vrai et conserver un état neutre pour les machines sans configuration.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs` pour placer l’action actuelle de création dans l’onglet Général de la machine sélectionnée et ne créer aucun écran intermédiaire avant ses onglets.
  - [x] 6.2 Conserver toutes les fonctions lors du changement de machine
    - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs` pour préserver l’onglet actif, recharger le moteur, les firmwares, les entrées, le stockage et le profil vidéo de la machine choisie, puis actualiser immédiatement sa couleur après la création ou la suppression d’une configuration.
    - [x] Modifier `src/GWGUI.App/Views/Windows/EmulationModuleOptions/EmulationModuleOptionsWindow.xaml` pour dimensionner la colonne des machines et la zone d’onglets afin que la liste reste lisible avec de nombreux modèles et que les réglages conservent leur espace actuel.

- [x] 7. Simplifier le choix et l’installation de l’émulateur actuel
  - [x] 7.1 Présenter le moteur associé à la machine dans l’onglet Général
    - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationCoreManagementPanel.cs` pour remplacer le grand panneau actuel par une ligne contenant un sélecteur d’émulateur, puis à sa droite le bouton `Installer` lorsque le moteur manque ou le libellé `Installé` lorsqu’il est présent.
    - [x] Modifier `src/GWGUI.App/Controllers/Emulation/Options/EmulationEmulatorManagementController.cs` pour alimenter le sélecteur avec l’unique `EmulatorId` actuellement retourné par le module pour la machine sélectionnée, conserver la recherche et l’installation de ses versions existantes et rafraîchir l’état après installation.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs` pour reconstruire cette ligne lors du changement de machine afin qu’Atari présente le moteur actuellement associé au modèle choisi et qu’Amiga présente son moteur actuel.

- [x] 8. Ajouter et traduire les textes nouveaux
  - [x] 8.1 Compléter les ressources communes
    - [x] Modifier `src/GWGUI.App/Resources/00-Base/Options.resx` et `src/GWGUI.App/Resources/00-Base/Emulation.resx` pour ajouter `Téléchargé`, `Finir l’installation`, l’explication du redémarrage groupé, le titre générique de la fenêtre d’un module et les libellés communs de la navigation verticale, sans dupliquer les noms de modules, de machines ou d’émulateurs.
    - [x] Exécuter `scripts/translate-resx-argos.py` sur les nouvelles clés communes afin de modifier tous les catalogues `src/GWGUI.App/Resources/*/Options.resx` et `src/GWGUI.App/Resources/*/Emulation.resx` des langues prises en charge.

- [x] 9. Adapter les tests permanents utiles
  - [x] 9.1 Vérifier l’installation groupée sans téléchargement réel
    - [x] Créer `tests/GWGUI.Tests/Updates/PendingModuleInstallationScenarios.cs` avec des préparations factices en mémoire pour vérifier l’ajout de plusieurs identifiants, le refus d’un doublon, la conservation après fermeture de la fenêtre Mises à jour et la création d’un seul plan contenant tous les modules.
    - [x] Créer `tests/GWGUI.Tests/Updates/PendingModuleInstallationTests.cs` pour enregistrer les scénarios du magasin et vérifier qu’un seul lancement d’updater est demandé par `Finir l’installation`.
  - [x] 9.2 Vérifier la séparation des fenêtres et la navigation des machines
    - [x] Modifier `tests/GWGUI.Tests/Interface/Navigation/NavigationScenarios.cs` et `NavigationTests.cs` pour vérifier l’ouverture distincte des préférences communes et de la fenêtre générique du module demandé, son propriétaire et son titre localisé.
    - [x] Modifier `tests/GWGUI.Tests/Interface/Navigation/EmulationMenuScenarios.cs` et `EmulationMenuTests.cs` pour confirmer que plusieurs identifiants arbitraires ouvrent la même classe de fenêtre avec le bon module, sans nom de famille codé en dur.
    - [x] Créer `tests/GWGUI.Tests/Interface/SettingsViews/EmulationModuleSettingsNavigationScenarios.cs` avec un module factice en mémoire pour vérifier la liste verticale, la sélection d’une machine, la couleur d’une configuration existante et la conservation des onglets actuels.
    - [x] Modifier `tests/GWGUI.Tests/Interface/SettingsViews/SettingsViewsTests.cs` pour enregistrer les scénarios de la fenêtre de module et vérifier le sélecteur contenant uniquement le moteur actuellement fourni.
    - [x] Modifier `tests/GWGUI.Tests/Interface/SettingsViews/SettingsFailureScenarios.cs` pour conserver le signalement des échecs d’enregistrement dans les nouvelles fenêtres après le déplacement des contrôles.

- [ ] 10. Valider le résultat complet
  - [x] 10.1 Exécuter les validations automatisées à la fin des modifications
    - [x] Exécuter les tests ciblés de `tests/GWGUI.Tests/Updates`, `tests/GWGUI.Tests/Interface/Navigation` et `tests/GWGUI.Tests/Interface/SettingsViews`, puis ajouter leur résultat dans une section `Résultats de validation` de ce fichier.
    - [x] Exécuter l’audit de ressources du projet avec Argos et inscrire dans ce fichier le nombre de cultures et de catalogues contrôlés.
    - [x] Exécuter `scripts/build.ps1 -Configuration Debug`, vérifier `build/Debug/GW GUI/gwgui.exe` ainsi que le dossier `Modules` vide du build propre, puis inscrire le résultat dans ce fichier.
  - [x] 10.2 Corriger l'action de fin d'installation après le premier essai utilisateur
    - [x] Modifier `src/GWGUI.App/Views/Controls/Options/OptionsUpdatesSection.xaml` et `.xaml.cs` pour retirer l'action de fin d'installation du contenu défilant de la page des modules et supprimer le chevauchement de la liste.
    - [x] Modifier `src/GWGUI.App/Views/Windows/Updates/UpdatesWindow.xaml` et `.xaml.cs` pour transformer le bouton de pied de fenêtre `Fermer` en `Finir l'installation` tant qu'au moins un module est téléchargé, puis restaurer `Fermer` lorsqu'aucune installation n'est en attente.
    - [x] Modifier `src/GWGUI.App/Views/Windows/Updates/UpdatesWindow.xaml.cs` pour intercepter le bouton `X` lorsqu'une installation est en attente et lui faire demander la même confirmation que `Finir l'installation`, sans perdre les modules téléchargés en cas de refus.
    - [x] Modifier `src/GWGUI.App/Options/Controllers/UpdateOptionsController.cs` pour exposer la commande groupée à la fenêtre et conserver une seule transaction suivie d'un seul redémarrage.
    - [x] Adapter les tests ciblés de la fenêtre Mises à jour, puis reconstruire le build Debug propre.
  - [x] 10.3 Annuler les téléchargements lorsque la fermeture est refusée
    - [x] Modifier `src/GWGUI.App/Views/Windows/Updates/UpdatesWindow.xaml.cs` pour faire terminer l'installation après `Oui`, mais supprimer les préparations téléchargées et fermer la fenêtre après `Non` lorsque la fermeture est demandée par le bouton `X`.
    - [x] Modifier `src/GWGUI.App/Options/Controllers/UpdateOptionsController.cs` pour permettre au bouton `X` d'appeler l'installation groupée après sa propre confirmation sans afficher deux demandes successives.
    - [x] Ajouter et traduire avec Argos un message propre à la fermeture qui indique clairement que `Non` annule les téléchargements, puis adapter le test permanent du pied de fenêtre.
    - [x] Relancer les tests ciblés, l'audit Argos et le build Debug propre.
  - [x] 10.4 Garantir la présence de toutes les traductions
    - [x] Modifier `scripts/translate-resx-argos.py` pour que `--audit` signale toute clé traduisible absente d'une des 28 cultures Argos, tout en conservant le repli volontaire de `en-US` vers `00-Base` et l'absence physique des valeurs invariantes.
    - [x] Modifier le nettoyage du script pour conserver une traduction Argos identique à l'anglais afin que l'audit puisse la distinguer d'une clé oubliée.
    - [x] Exécuter `scripts/translate-resx-argos.py --sync-all` pour compléter avec Argos toutes les traductions manquantes dans les ressources de GW GUI, d'Amiga et d'Atari, puis relancer l'audit renforcé sur les trois ensembles.
    - [x] Modifier `docs/project/scripts.md` pour préciser que l'audit contrôle aussi la complétude des textes traduisibles.
  - [ ] 10.5 Faire vérifier les parcours visibles
    - [ ] Modifier ce fichier après validation utilisateur pour consigner le téléchargement successif d’au moins deux modules, leurs badges `Téléchargé`, l’unique bouton `Finir l’installation`, le redémarrage unique et leurs badges `Installé` après reprise.
    - [ ] Modifier ce fichier après validation utilisateur pour consigner l’ouverture séparée des préférences communes, des fenêtres Amiga et Atari, la navigation verticale de leurs machines, leurs couleurs de configuration et l’état d’installation du moteur actuel.

## Résultats de validation

- 9 septembre 2026 — tests ciblés `Updates`, `Interface/Navigation` et `Interface/SettingsViews` : 155 réussis, 0 échec, 0 ignoré.
- 9 septembre 2026 — audit Argos : 29 cultures, 22 catalogues et 42 768 entrées localisées contrôlés, audit réussi.
- 9 septembre 2026 — `scripts/build.ps1 -Configuration Debug` : build réussi. `build/Debug/GW GUI/gwgui.exe` est présent et `build/Debug/GW GUI/Modules` est présent avec 0 entrée.
- 9 septembre 2026 — correction après essai utilisateur : le pied de fenêtre et le bouton `X` suivent désormais l'état des modules téléchargés ; 156 tests ciblés réussis, 0 échec, puis nouveau build Debug propre réussi.
- 9 septembre 2026 — refus de la fermeture avec téléchargements en attente : `Non` vide désormais la file et ferme la fenêtre ; 156 tests ciblés réussis, audit Argos réussi sur 29 cultures, 22 catalogues et 42 796 entrées, puis build Debug propre réussi.
- 9 septembre 2026 — audit de complétude renforcé : toutes les clés traduisibles sont présentes dans les 28 langues Argos. Audits réussis pour GW GUI (29 cultures, 22 catalogues, 43 682 entrées), Amiga (29 cultures, 1 catalogue, 420 entrées) et Atari (29 cultures, 1 catalogue, 2 100 entrées).
