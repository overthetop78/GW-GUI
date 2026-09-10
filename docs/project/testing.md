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

## Ancien contrôle interactif, manuel uniquement

`scripts/test-app-accessibility.ps1` reste disponible pour ouvrir l’exécutable empaqueté, contrôler son redimensionnement avec le DPI Windows et inspecter les noms accessibles. Ce script nécessite un bureau ; il n’est plus appelé par le workflow de release ni par `GWGUI.Tests`. Les tests hors écran ne sont pas présentés comme un remplacement de sa vérification du cadre natif.
