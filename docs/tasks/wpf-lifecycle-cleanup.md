# Nettoyage du cycle de vie WPF et graphique

- [x] 1. Inscrire le correctif dans l’ordre de travail
  - [x] Créer `docs/tasks/wpf-lifecycle-cleanup.md` avec les actions ordonnées qui garantissent la fermeture des fenêtres, des surfaces graphiques, des threads et du `Dispatcher`.
  - [x] Modifier `docs/tasks/README.md` pour placer ce correctif après le chantier média terminé et avant les validations manuelles.
- [x] 2. Garantir la libération des ressources de l’application
  - [x] Modifier `src/GWGUI.App/Presenters/Emulation/Machine/MachineVideoPresenter.cs` pour attendre réellement la fin du thread de présentation GPU et toujours libérer la surface, le jeton d’annulation et le signal.
  - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs` pour fermer aussi une fenêtre plein écran cachée, détacher son contenu et terminer le présentateur vidéo avant la session d’émulation.
  - [x] Modifier `src/GWGUI.App/Rendering/Emulation/Processing/SoftwareVideoFrameProcessingWorker.cs` pour conserver la tâche de traitement, annuler sa file, attendre sa fin et libérer le pipeline seulement après l’arrêt réel du travail.
  - [x] Modifier `src/GWGUI.App/Rendering/Emulation/Surfaces/WpfVideoSurface.cs` pour annuler les travaux de rendu, vider la source d’image et libérer le bitmap lors de la destruction.
  - [x] Modifier `src/GWGUI.App/Rendering/Emulation/Surfaces/OpenGlVideoSurface.cs` pour rendre la destruction idempotente, vider les captures conservées et confirmer la destruction du contexte et du handle natif.
  - [x] Modifier `src/GWGUI.App/Rendering/Emulation/Surfaces/VeldridVideoSurface.cs` pour rendre la destruction idempotente, attendre l’inactivité du périphérique, vider les captures et détruire ses ressources et son handle natif.
  - [x] Modifier `src/GWGUI.App/Services/Visualization/ScpInspectorController.cs` pour exposer une fermeture qui annule l’analyse, détache les événements, vide le contenu et ferme la fenêtre d’inspection détachée.
  - [x] Modifier `src/GWGUI.App/Views/Windows/Shell/MainWindow.xaml.cs` pour appeler la fermeture du contrôleur d’inspection pendant le cycle de fermeture de la fenêtre principale.
  - [x] Modifier `src/GWGUI.App/App.xaml.cs` pour fermer et vider les éventuelles fenêtres secondaires restantes avant la fin du `Dispatcher` de l’application.
- [x] 3. Garantir la libération de l’infrastructure WPF des tests
  - [x] Modifier `tests/GWGUI.Tests/Application/TestInfrastructure/StaExecutionScenarios.cs` pour fermer et vider toutes les fenêtres sur leur thread STA après chaque scénario, arrêter explicitement l’`Application` et le `Dispatcher`, attendre la fin du thread et exécuter les finaliseurs après son arrêt.
- [x] 4. Consigner le contrat de fermeture obtenu
  - [x] Modifier `docs/project/testing.md` pour documenter le nettoyage automatique des fenêtres, surfaces, threads et dispatchers sans prétendre avoir exécuté les tests manuels d’images.
- [x] 5. Corriger la fermeture déclenchée par la fenêtre plein écran elle-même
  - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs` pour ne pas rappeler `Close()` depuis l’événement `Closed`, tout en conservant la fermeture forcée des fenêtres cachées lors de l’arrêt de la machine.
- [x] 6. Supprimer les diagnostics temporaires à la demande de l’utilisateur
  - [x] Supprimer tous les fichiers de diagnostic contenus dans `artifacts/diagnostics/vscode-load`, puis supprimer `artifacts` et ses sous-dossiers devenus vides.
- [x] 7. Sans objet — conserver l'infrastructure existante de `GWGUI.Tests`
  - [x] Modifier `docs/tasks/wpf-lifecycle-cleanup.md` pour consigner qu'aucun changement propre à cette campagne n'est conservé dans `GWGUI.Tests`, car l'accumulation constatée provient de l'audit `GWGUI.LocalDiskImageTests`.
- [x] 8. Rendre permanent le nettoyage des ressources graphiques créées par les tests et scripts.
  - [x] Modifier `docs/project/rules.md` pour imposer la fermeture et la libération dans un bloc `finally` de chaque fenêtre, surface graphique, arbre visuel, application, dispatcher et thread créés par un test ou un script, y compris après erreur ou interruption.
  - [x] Modifier `.codex/config.toml` pour imposer la même règle à chaque future intervention de Codex et éviter toute nouvelle accumulation de ressources dans DWM.
- [x] 9. Appliquer la prévention au processus d'audit qui provoque l'accumulation DWM.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/Program.cs` afin de libérer après chaque exécution toutes les ressources graphiques et WPF chargées par l'audit, y compris en cas d'erreur.
  - [x] Modifier `scripts/analyze-media-data.ps1` afin que chaque processus du validateur soit terminé et libéré avant le passage au média suivant, y compris en cas d'erreur ou d'interruption.
  - [x] Modifier `docs/project/testing.md` pour documenter le nettoyage dans `GWGUI.LocalDiskImageTests` et le script continu.
  - [x] Modifier `docs/tasks/wpf-lifecycle-cleanup.md` avec le résultat concret de la correction, sans relancer l'audit du corpus.

Résultat : GWGUI.LocalDiskImageTests ferme et libère les ressources WPF éventuellement chargées dans son `finally`. Le script continu crée les processus dotnet sans fenêtre, attend leur fin et les libère avant le média suivant. Sa syntaxe est valide et le projet local compile sans avertissement ni erreur ; aucun audit du corpus ni test de GWGUI.Tests n'a été lancé après la correction de périmètre.
