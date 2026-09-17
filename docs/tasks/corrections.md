# Corrections à effectuer

- [x] 1. Fermer le média en cours avant d’ouvrir le suivant
  - [x] 1.1 Mettre les demandes d’ouverture en série
    - [x] Modifier `src/GWGUI.App/Services/DiskImages/DiskImageWorkspaceController.cs` pour remplacer `_sharedLoadGeneration` par un seul chargement actif : annuler le chargement courant, attendre sa fermeture et son nettoyage complets, abandonner les demandes intermédiaires remplacées, puis ouvrir uniquement la dernière image demandée.
    - [x] Modifier `src/GWGUI.App/Services/DiskImages/DiskImageCancellationScope.cs` pour annuler les opérations appartenant au chargement commun et ne libérer leurs sources d’annulation qu’après la fin effective des tâches propriétaires.
    - [x] Modifier `src/GWGUI.App/Services/DiskImages/Visualization/CassetteLoadingPresenter.cs` pour remplacer le numéro de génération par le jeton du chargement commun et interrompre immédiatement les délais de présentation lors de son annulation.
  - [x] 1.2 Vérifier qu’aucun ancien chargement ne reste actif
    - [x] Modifier `tests/GWGUI.Tests/Interface/VisualizerViews/VisualizerDocumentScenarios.cs` pour vérifier que l’ouverture d’une nouvelle image ferme et nettoie la précédente avant de commencer, et qu’une série de demandes rapides ne conserve que la dernière demande restante.
    - [x] Modifier `tests/GWGUI.Tests/Interface/VisualizerViews/VisualizerDocumentScenarios.cs` pour vérifier dans `LateCompletion` le panneau de reconnaissance tant que l’exploration n’a pas encore retourné le SCP ; la barre d’état sert ensuite à la préparation des pistes et disparaît à la fin du chargement.
    - [x] Exécuter `scripts/local-building.cmd --building=debug --modules=0` et vérifier `build/Debug/GW GUI/gwgui.exe` après la mise en série des ouvertures.

- [x] 2. Déclencher la présentation des cassettes depuis le type réel du média
  - [x] 2.1 Retirer la décision fondée sur le nom du fichier
    - [x] Modifier `src/GWGUI.App/Services/DiskImages/DiskImageWorkspaceController.cs` pour supprimer `isCassette` et la comparaison directe de l’extension `.cas`.
  - [x] 2.2 Utiliser la reconnaissance retournée par MediaEngine
    - [x] Modifier `src/GWGUI.App/Services/DiskImages/DiskImageWorkspaceController.cs` pour demander la présentation de la cassette uniquement lorsque le document chargé par MediaEngine indique `MediaKind.Tape`.
    - [x] Modifier `src/GWGUI.App/Services/DiskImages/Visualization/CassetteLoadingPresenter.cs` pour recevoir le résultat réel de reconnaissance du média et la génération du chargement actif, puis arrêter immédiatement sa présentation lorsqu’un autre média est ouvert.
  - [x] 2.3 Vérifier la décision fondée sur le contenu reconnu
    - [x] Modifier `tests/GWGUI.Tests/Interface/VisualizerViews/VisualizerDocumentScenarios.cs` pour vérifier qu’un média séquentiel reconnu déclenche la présentation cassette indépendamment de son extension et qu’un autre média renommé en `.cas` ne la déclenche pas.

- [x] 3. Retirer les extensions de média écrites directement dans le contrôleur
  - [x] 3.1 Retirer la décision devenue inutile
    - [x] Modifier `src/GWGUI.App/Services/DiskImages/DiskImageWorkspaceController.cs` pour supprimer la comparaison avec la chaîne brute `".scp"`, le pipeline utilisant désormais le type réel du média.
