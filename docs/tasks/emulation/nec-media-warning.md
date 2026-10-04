# Avertissement de média requis au démarrage NEC

- [x] Remonter le refus du cœur sans perdre sa cause
  - [x] Créer `src/GWGUI.Emulation.Nec/Common/Exceptions/CoreMediaRequiredException.cs` : définir une exception interne dédiée au démarrage sans média.
  - [x] Créer `src/GWGUI.Emulation.Nec/Common/Enums/CoreHostErrorKind.cs` : définir les codes du protocole hôte pour erreur générale et média requis.
  - [x] Modifier `src/GWGUI.Emulation.Nec/Emulators/BeetlePce/Services/ExternalCore.cs` et `src/GWGUI.Emulation.Nec/Emulators/BeetlePcfx/Services/ExternalCore.cs` : émettre l'exception dédiée lorsque le cœur refuse le démarrage sans média.
  - [x] Modifier les fichiers `CoreHost.cs` et `ProcessCore.cs` des émulateurs BeetlePce et BeetlePcfx : transmettre le code de refus au processus principal et reconstruire l'exception dédiée.
- [x] Afficher et journaliser un avertissement utile
  - [x] Modifier `src/GWGUI.Emulation/Contracts/EmulationRequiredMachineMediaMessageContext.cs` : porter les noms invariants des supports proposés en plus de leurs catégories.
  - [x] Modifier `src/GWGUI.Emulation.Nec/Common/Services/Machine.Commands.cs` : transformer seulement le refus de média en `RequiredMediaMissing` de sévérité Warning, en listant HuCARD et CD-ROM selon les périphériques configurés.
  - [x] Modifier `src/GWGUI.App/Services/Logging/ErrorLog.cs` : ajouter une publication et un journal d'avertissements sans trace d'exception.
  - [x] Modifier `src/GWGUI.App/Presenters/Common/ControlErrorPresenter.cs` : afficher les noms des supports et journaliser l'avertissement comme tel.
  - [x] Modifier `src/GWGUI.App/Services/Terminal/TerminalPanelController.cs` et `src/GWGUI.App/Views/Windows/Shell/MainWindow.EventsAndCommands.cs`, `MainWindow.xaml.cs` : afficher les avertissements en orange dans la console.
- [x] Vérifier le résultat
  - [x] Modifier `src/GWGUI.Emulation.Nec/Common/Services/Machine.Commands.cs` : exposer en interne la construction pure de l'avertissement pour une vérification ciblée.
  - [x] Créer `tests/GWGUI.Tests/Architecture/NecMediaWarningTests.cs` : vérifier automatiquement les supports requis avec et sans lecteur CD configuré, sans média ou fichier externe.
  - [x] Modifier `docs/tasks/emulation/nec-media-warning.md` : consigner la compilation Debug de l'application et des huit modules, la présence des artefacts, et les vérifications ciblées du refus HuCARD/CD-ROM.

Vérifications : `scripts/local-building.cmd --building=debug --modules=A` terminé avec code 0 ; `build/Debug/GW GUI/gwgui.exe` et les huit DLL `Modules/<id>/gwgui.emulation.<id>.dll` présents. Les deux cas de `NecMediaWarningTests` passent : sans lecteur CD configuré, HuCARD seule ; avec lecteur, HuCARD et CD-ROM. Un ancien test `NecBeetlePceSelectsPcEngineModels`, lancé séparément, attend encore Beetle PCE FAST pour SuperGrafx alors que la configuration choisit Beetle SGX ; il échoue indépendamment de cet avertissement.
