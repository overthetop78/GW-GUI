# Focus de l’écran d’émulation

[Sommaire](../emulation-improvements.md) · [Règles communes](rules.md)

## 1. Écran d’émulation

### Focus de l’écran

Dans l’onglet d’émulation actif, le focus doit revenir à la fenêtre d’émulation après une action ponctuelle effectuée dans l’interface lorsque la machine est allumée ou vient d’être allumée. L’extinction ne rend pas le focus à la machine éteinte.

Cela concerne notamment :

- le chargement ou le changement d’une image de disquette ;
- l’allumage de la machine ;
- le reset logiciel ou matériel ;
- la sauvegarde d’un état ;
- le chargement d’un état ;
- le basculement entre la manette et la souris ;
- les autres commandes comparables de l’instance.

Pendant l’ouverture d’une boîte de dialogue, celle-ci conserve le focus. Une fois l’opération terminée et la boîte de dialogue fermée, le focus revient à l’écran de l’instance affichée dans l’onglet actif.

Un clic dans la zone grise autour de l’image doit également redonner le focus à l’émulation sans capturer la souris.

Les clics dans l’interface et les raccourcis de GW GUI doivent continuer à fonctionner.

## Checklist détaillée — Point 1 : écran d’émulation

Cette checklist détaille uniquement le retour du focus du point 1. Dans l’ordre global, ce travail correspond au groupe 3. Les filtres vidéo et les habillages sont détaillés séparément dans les checklists des points 7 et 8 afin de ne pas dupliquer leurs tâches ici. Chaque dernière case constitue une modification atomique qui doit laisser le projet compilable, être vérifiée, puis être cochée avant la suivante.

- [x] Retour automatique du focus vers l’instance d’émulation ouverte
  - [x] Limiter la restitution du focus à l’instance affichée dans l’onglet actif
    - [x] Transporter la sélection réelle du TabControl jusqu’au contrôleur de machine
      - [x] Dans src/GWGUI.App/Contracts/Machine/MachineControllerOptions.cs, ajouter `Func<bool> IsActive` avant les paramètres facultatifs; dans `OpenMachineAsync` de src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationSectionMachineFunctions.cs, conserver la référence du `MachineController` créé et fournir une fonction `IsActive` qui compare cette référence à `_machines.SelectedContent`.
      - [x] Dans src/GWGUI.App/Controllers/Emulation/Machine/MachineInputController.cs, ajouter le champ `Func<bool> _isActive` et le paramètre de constructeur qui l’initialise; dans le constructeur de src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs, transmettre `options.IsActive` à ce nouveau paramètre.
    - [x] Centraliser la restitution vers la cible active et courante
      - [x] Dans src/GWGUI.App/Controllers/Emulation/Machine/MachineInputController.cs, ajouter RestoreFocus avec exactement deux chemins : retourner lorsque _powered est faux ou lorsque _isActive() est faux; sinon appeler RelativeMouseCapture.Focus(_inputView, _inputHandle). Ne faire aucun appel à Capture, ReleasePointer, SetInputView ou _view.Screen.Focus() dans cette méthode.
  - [x] Rendre le clic de la zone grise au contrôleur d’entrée
    - [x] Raccorder uniquement le fond extérieur à l’écran
      - [x] Dans src/GWGUI.App/Controllers/Emulation/Machine/MachineInputController.cs, abonner `MouseLeftButtonDown` de `_view.DisplayHost` dans le constructeur, créer `DisplayHostMouseLeftButtonDown` pour appeler `RestoreFocus` uniquement lorsque `args.OriginalSource` est exactement `_view.DisplayHost`, puis désabonner ce gestionnaire dans `Dispose`.
  - [x] Restituer le focus après les commandes de la barre d’outils
    - [x] Faire transporter l’opération commune par la barre sans casser sa construction
      - [x] Dans src/GWGUI.App/Views/Controls/Emulation/Machine/MachineCommandBar.cs, ajouter le champ `Action _restoreFocus` et le paramètre de constructeur qui l’initialise; dans le constructeur de src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs, créer `_input` avant `_commands` et fournir `_input.RestoreFocus` au nouveau paramètre.
      - [x] Dans src/GWGUI.App/Views/Controls/Emulation/Machine/MachineCommandBar.cs, rendre `RunAsync` non statique et ajouter un bloc `finally` qui appelle `_restoreFocus()` après le `try/catch` existant, sans modifier `Command` ni les actions qui lui sont fournies.
    - [x] Retirer les restitutions particulières remplacées par le chemin commun
      - [x] Dans `TogglePowerAsync` de src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs, supprimer uniquement `if (_session.IsPowered) _video.InputView.Focus()` et laisser inchangées les mises à jour de session, d’entrée, de commandes, de visibilité vidéo et de statut.
      - [x] Dans `ExecuteShortcutAsync` de src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs, ajouter un bloc `finally` qui appelle `_input.RestoreFocus()` après le `try/catch`, sans modifier le `switch`, les actions appelées ni la gestion actuelle des erreurs.
  - [x] Restituer le focus après les commandes des lecteurs
    - [x] Faire transporter l’opération commune jusqu’aux boutons de média sans dupliquer les erreurs
      - [x] Dans src/GWGUI.App/Views/Controls/Emulation/Machine/MachineView.cs, ajouter `Action restoreFocus` à `SetDevices`, `DeviceItem` et `RunAsync`, transmettre ce paramètre à chaque appel intermédiaire et l’appeler dans un `finally` de `RunAsync`; dans `RebuildMediaDevices` de src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs, fournir `_input.RestoreFocus` au nouvel argument de `_view.SetDevices`. Ne modifier ni `InsertMediaAsync`, ni `EjectMediaAsync`, ni le `catch` qui appelle `showError`.
  - [x] Conserver la séquence du plein écran avec la même opération de focus
    - [x] Utiliser la restitution commune après le déplacement de Screen
      - [x] Dans `CompleteHostTransition` de src/GWGUI.App/Controllers/Emulation/Machine/MachineInputController.cs, remplacer uniquement `RelativeMouseCapture.Focus(_inputView, _inputHandle)` par `RestoreFocus`, sans déplacer la lecture de `_restorePointerAfterHostTransition`, la remise à zéro de `_hostTransition` ni la restauration conditionnelle de `_pointerCapture`.
      - [x] Dans `EnterFullscreen` de src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs, supprimer uniquement `_video.InputView.Focus()` et conserver dans le même ordre `BeginHostTransition`, le déplacement de `Screen`, l’affichage et l’activation de la fenêtre, puis `CompleteHostTransition`.
      - [x] Dans `ExitFullscreen` de src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs, supprimer uniquement `_video.InputView.Focus()` et conserver dans le même ordre `BeginHostTransition`, le replacement de `Screen`, la fermeture de la fenêtre, puis `CompleteHostTransition`.
      - [x] Dans `FullscreenContentRendered` de src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs, remplacer uniquement `_video.InputView.Focus()` par `_input.RestoreFocus()` après `_video.FitScreen()`.
  - [x] Verrouiller chaque comportement par des tests ciblés et rapides
    - [x] Préparer le fichier de tests unique du point 1
      - [x] Créer le fichier vide tests/GWGUI.Tests/MachineFocusTests.cs sans ajouter son contenu dans la même action.
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter uniquement les doubles minimaux de `IEmulatedMachine` et `IEmulationInput`, les créations de `MachineView` et les déclencheurs d’événements nécessaires aux scénarios suivants; vérifier que le projet de tests compile avant de cocher cette case.
    - [x] Vérifier la cible commune
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter le test d’une instance active et allumée qui appelle `RestoreFocus` puis vérifie le focus de la surface WPF courante et l’absence de capture; exécuter uniquement ce test avant de cocher la case.
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter le test qui remplace la surface par `SetInputView`, appelle `RestoreFocus` puis vérifie que la nouvelle surface, et non l’ancienne, reçoit le focus; exécuter uniquement ce test avant de cocher la case.
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter le test d’une instance éteinte qui appelle RestoreFocus puis vérifie que le focus existant ne change pas; exécuter uniquement ce test avant de cocher la case.
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter le test de deux contrôleurs dont les fonctions `IsActive` renvoient des valeurs opposées, puis vérifier que seul le contrôleur actif déplace le focus; exécuter uniquement ce test avant de cocher la case.
    - [x] Vérifier les deux zones de clic
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter le test qui déclenche `MouseLeftButtonDown` avec `DisplayHost` comme source d’origine puis vérifie le retour du focus sans capture; exécuter uniquement ce test avant de cocher la case.
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter le test qui déclenche le clic existant sur la surface avec la capture autorisée puis vérifie que le comportement de capture reste actif; exécuter uniquement ce test avant de cocher la case.
    - [x] Vérifier les commandes communes
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter le test de `MachineCommandBar` qui exécute une commande réussie puis une commande en erreur, et vérifie dans les deux cas un appel de restitution ainsi que la transmission de la seule erreur à `showError`; exécuter uniquement ce test avant de cocher la case.
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter le test des boutons de média de `MachineView` qui exécute une action terminée sans modification, représentant le retour Annuler, puis une action en erreur, et vérifie dans les deux cas un appel de restitution ainsi que la transmission de la seule erreur à `showError`; exécuter uniquement ce test avant de cocher la case.
    - [x] Terminer la validation du point
      - [x] Exécuter tous les tests de tests/GWGUI.Tests/MachineFocusTests.cs et corriger uniquement les régressions produites par les fichiers du point 1 avant de cocher cette case.
      - [x] Exécuter toute la suite tests/GWGUI.Tests/GWGUI.Tests.csproj et corriger uniquement les régressions produites par les fichiers du point 1 avant de cocher cette case. Suite exécutée à nouveau le 5 septembre 2026 : 151 tests réussis.
