# Tests et contrôles actuels

La suite automatisée actuelle est décrite directement dans ce document. Les anciens plans de
reconstruction ont été retirés après intégration de leurs résultats.

Les commandes de cette page s’exécutent depuis la racine du dépôt.

Le groupe 1.1 de `tests/GWGUI.Tests` vérifie les sept onglets et leurs états, le routage des commandes, les requêtes de dialogues et leurs réponses simulées, la disposition hors écran, le placement sur des géométries simulées et les propriétés/liaisons des contrôles. Les libellés des menus sont contrôlés dans les langues du catalogue sans imposer de traduction ni de lettre d’accès.

Les vues sont créées en mémoire sur un Dispatcher STA. Aucun affichage, handle de fenêtre natif ni focus réel n’est nécessaire. Les anciens scénarios interactifs ont été remplacés ; la modalité et le focus natifs de WPF ne font pas partie des résultats déclarés couverts.

Dernière validation locale en configuration `Release` : **1 579 tests réussis, 0 échec, 0 ignoré, en 18 secondes**.

Les 41 groupes couvrent les 122 tâches du plan, réparties entre l’application, l’interface, le matériel, les médias et l’émulation. Les scénarios exercent les opérations et leur annulation, les réglages et profils, la localisation, les commandes et le protocole, la reconnaissance des images, les conteneurs, les codecs, les systèmes de fichiers, les conversions et migrations, ainsi que les adaptateurs, entrées et sorties audio/vidéo de GW GUI.

Les données sont synthétiques et les accès aux fichiers de données sont simulés en mémoire, y compris pour les réglages, les conteneurs et les migrations. Les périphériques, processus, services réseau et cœurs externes sont remplacés aux interfaces ; les traitements intégrés de GW GUI sont réellement exécutés. Aucun scénario ne lance de moteur d’émulation ou `gw.exe`.

## Tests des images disque

[GWGUI.LocalDiskImageTests](../../tests/GWGUI.LocalDiskImageTests/GWGUI.LocalDiskImageTests.csproj) contient les tests de reconnaissance, lecture, écriture, conversion et systèmes de fichiers des images disque. Il reste séparé de la solution principale et du workflow de release. Les tests utilisant le corpus privé nécessitent le dossier local `image_test`.

```powershell
dotnet test tests/GWGUI.LocalDiskImageTests/GWGUI.LocalDiskImageTests.csproj
```

## Contrôles exécutés pendant la release

Le workflow `release.yml` restaure, compile en configuration `Release` et exécute `GWGUI.Tests` avant de construire les paquets, pour les déclenchements manuels (dont les snapshots) et les tags de release. Une erreur de compilation, un test en échec ou l’absence de test exécuté bloque la suite du workflow et la publication. Le nombre de tests est contrôlé directement dans la sortie de `dotnet test`. Les nouveaux groupes ajoutés à ce projet seront automatiquement inclus, sans filtre de catégorie.

Ce raccordement est configuré ; sa première exécution sur GitHub reste à vérifier lors d’une release.

Après la construction des paquets, le workflow prépare et vérifie les sources du wiki, teste l’installation en anglais et en français, puis teste une mise à jour depuis une ancienne installation simulée. L’ancien audit qui ouvre l’application a été retiré du workflow ; les exigences UI hors écran sont suivies dans les groupes Interface du plan.

Ces contrôles peuvent aussi être lancés localement après le packaging. Exemple pour les paquets `0.1.3` :

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/test-installer.ps1 -SetupPath dist/GW-GUI-0.1.3-win-x64-setup.exe -ExpectedVersion 0.1.3 -InstallerLanguage english
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/test-installer.ps1 -SetupPath dist/GW-GUI-0.1.3-win-x64-setup.exe -ExpectedVersion 0.1.3 -InstallerLanguage french
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/test-installer-upgrade.ps1 -CurrentVersion 0.1.3
```

| Contrôle | Ce qu’il vérifie |
|---|---|
| Installation | Installation silencieuse dans un dossier isolé sous `dist`, fichiers attendus, version, langue, puis désinstallation et nettoyage. |
| Mise à jour | Installation d’une ancienne version simulée, ajout de restes d’un ancien runtime .NET, mise à jour et suppression de ces fichiers obsolètes, puis désinstallation. |

Les contrôles d’installation refusent de démarrer si une installation GW GUI est déjà enregistrée pour l’utilisateur.

## Validation du socle MediaEngine commun

La validation locale du socle commun a été exécutée en configuration `Debug`, sans le corpus privé
`image_test`. Les quatre projets concernés compilent sans avertissement ni erreur :

```powershell
dotnet build src/GWGUI.MediaEngine/GWGUI.MediaEngine.csproj --no-restore --configuration Debug --verbosity quiet
dotnet build src/GWGUI.App/GWGUI.App.csproj --no-restore --configuration Debug --verbosity quiet
dotnet build src/GWGUI.Emulation.Amiga/GWGUI.Emulation.Amiga.csproj --no-restore --configuration Debug --verbosity quiet
dotnet build src/GWGUI.Emulation.Atari/GWGUI.Emulation.Atari.csproj --no-restore --configuration Debug --verbosity quiet
```

Les tests ciblés du registre de reconnaissance, des représentations de disquettes, de la conversion,
du partage du document et des frontières entre projets ont ensuite produit **10 réussites, 0 échec
et 0 test ignoré** :

```powershell
dotnet test tests/GWGUI.Tests/GWGUI.Tests.csproj --no-restore --configuration Debug --filter "FullyQualifiedName~MediaRecognitionRegistryTests|FullyQualifiedName~FloppyMediaReadingTests|FullyQualifiedName~MediaConversionServiceTests|FullyQualifiedName~MediaExplorerTests|FullyQualifiedName~MediaEngineProjectBoundaryTests" --verbosity quiet
```

Le script de construction Debug standard a ensuite terminé avec succès :

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build.ps1 -Configuration Debug
```

Le script a produit l'application sans module d'émulation préinstallé. La présence de
`build/Debug/GW GUI/gwgui.exe` a été vérifiée après sa terminaison.

## Validation des vues de médias

Les tests généraux du Visualiseur ont été exécutés en configuration `Debug`, sans fichier du
corpus privé. Les **14 scénarios ont réussi, sans échec ni test ignoré** :

```powershell
dotnet test tests/GWGUI.Tests/GWGUI.Tests.csproj --no-restore --configuration Debug --filter "FullyQualifiedName~MediaVisualizationRoutingTests|FullyQualifiedName~FloppyVisualizationTests|FullyQualifiedName~OtherMediaVisualizationTests|FullyQualifiedName~MediaVisualizationLayoutTests" --verbosity minimal
```

Ils contrôlent le routage de `Flux`, `Sectors`, `Blocks`, `OpticalTracks` et `Sequential`, la
séparation des informations de flux et de secteurs, les sélections 64 bits, les informations
absentes, la progression, les tailles de fenêtre, les ressources de thèmes et les noms accessibles.
Les documents Blocks, OpticalTracks et Sequential employés ici sont synthétiques : leurs Readers
de fichiers sont ajoutés dans les feuilles suivantes.

Le build destiné à la vérification visuelle a ensuite été recréé avec la commande standard :

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build.ps1 -Configuration Debug
```

Le script a terminé avec succès et la présence de
`build/Debug/GW GUI/gwgui.exe` a été vérifiée. Dans cette version :

- ouvrir un fichier `.scp` depuis l'onglet **Visualisation** affiche la vue Flux ;
- ouvrir une image de disquette sectorielle reconnue, par exemple ADF, ST, MSA, ATR, D64, D71,
  D81, DSK, IMG, IMD, TD0, HFE ou 86F, affiche la vue Sectors sans fabriquer de faux flux ;
- les vues Blocks, OpticalTracks et Sequential sont enregistrées et couvertes par les tests
  synthétiques ; elles deviendront accessibles depuis l'onglet **Visualisation** dès que les
  Readers HDD, CD/DVD/optiques et cassette/bande des feuilles suivantes fourniront ces
  représentations.

## Ancien contrôle interactif, manuel uniquement

`scripts/test-app-accessibility.ps1` reste disponible pour ouvrir l’exécutable empaqueté, contrôler son redimensionnement avec le DPI Windows et inspecter les noms accessibles. Ce script nécessite un bureau ; il n’est plus appelé par le workflow de release ni par `GWGUI.Tests`. Les tests hors écran ne sont pas présentés comme un remplacement de sa vérification du cadre natif.
