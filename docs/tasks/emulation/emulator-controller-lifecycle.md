# Nettoyage du cycle de vie du contrôleur d’émulateur

- [x] 1. Libérer le contrôleur d’émulateur
  - [x] 1.1 Détacher la vue
    - [x] Modifier `src/GWGUI.App/Controllers/Emulation/Options/EmulationEmulatorManagementController.cs` pour détacher `Install.Click`, `Cancel.Click` et `Emulators.SelectionChanged` avant de remplacer ou libérer la vue.
  - [x] 1.2 Arrêter les opérations possédées
    - [x] Modifier `src/GWGUI.App/Controllers/Emulation/Options/EmulationEmulatorManagementController.cs` pour annuler et attendre l’opération active dans un bloc `finally`, puis libérer son `CancellationTokenSource`.
  - [x] 1.3 Rendre la libération sûre
    - [x] Modifier `src/GWGUI.App/Controllers/Emulation/Options/EmulationEmulatorManagementController.cs` pour ajouter une libération asynchrone idempotente et empêcher tout résultat terminé après la fermeture de modifier la vue.

- [x] 2. Libérer la section de configuration
  - [x] 2.1 Détacher le contrôleur
    - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs` pour utiliser un gestionnaire `ConfigurationChanged` détachable, libérer le contrôleur dans un bloc `finally` et vider son arbre visuel.

- [x] 3. Attendre le nettoyage avant la destruction de la fenêtre
  - [x] 3.1 Fermer la fenêtre en deux étapes
    - [x] Modifier `src/GWGUI.App/Views/Windows/EmulationModuleOptions/EmulationModuleOptionsWindow.xaml.cs` pour intercepter la fermeture, attendre la libération de la section, détacher les événements globaux, vider `ModuleContent`, puis autoriser la fermeture définitive.
  - [x] 3.2 Garantir la destruction après erreur
    - [x] Modifier `src/GWGUI.App/Views/Windows/EmulationModuleOptions/EmulationModuleOptionsWindow.xaml.cs` pour exécuter le détachement, le vidage et la fermeture finale dans un bloc `finally`.

- [x] 4. Vérifier le cycle de vie
  - [x] 4.1 Vérifier l’annulation et le détachement
    - [x] Modifier `tests/GWGUI.Tests/Interface/SettingsViews/EmulationModuleSettingsNavigationScenarios.cs` pour vérifier que la fermeture annule et attend une opération active et que l’ancienne vue ne reçoit plus aucun événement.
    - [x] Modifier `tests/GWGUI.Tests/Interface/SettingsViews/SettingsViewsTests.cs` pour enregistrer le scénario STA d’annulation et de détachement du contrôleur.
  - [x] 4.2 Vérifier la réouverture
    - [x] Modifier `tests/GWGUI.Tests/Interface/SettingsViews/SettingsViewsTests.cs` pour enregistrer le scénario STA vérifiant qu’une nouvelle fenêtre peut être ouverte après la fermeture complète de la précédente sans réutiliser son contrôleur ni son arbre visuel.
  - [x] 4.3 Exécuter les vérifications ciblées
    - [x] Modifier `docs/tasks/emulation/emulator-controller-lifecycle.md` avec le résultat de la compilation et des scénarios STA ciblés après leur exécution réussie.

Résultat : `GWGUI.Tests` compile sans erreur. Les scénarios STA `EmulatorControllerDisposalCancelsAndDetaches` et `ModuleWindowReopensWithANewVisualTree` passent, ainsi que les scénarios existants `UnsavedModuleMachineChoosesExactlyOneEmulator` et `EachModuleUsesTheGenericSettingsWindow`.
