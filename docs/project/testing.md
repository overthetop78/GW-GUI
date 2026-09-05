# Tests et contrôles actuels

Les commandes de cette page s’exécutent depuis la racine du dépôt.

Le projet `tests/GWGUI.Tests` est conservé pour reconstruire la suite générale, mais ne contient actuellement aucun test. **Le workflow de release ne valide donc pas automatiquement les fonctionnalités métier** telles que la lecture, l’écriture, la conversion ou l’émulation.

## Tests des images disque

[GWGUI.LocalDiskImageTests](../../tests/GWGUI.LocalDiskImageTests/GWGUI.LocalDiskImageTests.csproj) contient les tests de reconnaissance, lecture, écriture, conversion et systèmes de fichiers des images disque. Il reste séparé de la solution principale et du workflow de release. Les tests utilisant le corpus privé nécessitent le dossier local `image_test`.

```powershell
dotnet test tests/GWGUI.LocalDiskImageTests/GWGUI.LocalDiskImageTests.csproj
```

## Contrôles exécutés pendant la release

Après la construction des paquets, le workflow prépare et vérifie les sources du wiki, contrôle certains aspects de l’accessibilité de l’application, teste l’installation en anglais et en français, puis teste une mise à jour depuis une ancienne installation simulée.

Ces contrôles peuvent aussi être lancés localement après le packaging. Exemple pour les paquets `0.1.3` :

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/test-app-accessibility.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/test-installer.ps1 -SetupPath dist/GW-GUI-0.1.3-win-x64-setup.exe -ExpectedVersion 0.1.3 -InstallerLanguage english
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/test-installer.ps1 -SetupPath dist/GW-GUI-0.1.3-win-x64-setup.exe -ExpectedVersion 0.1.3 -InstallerLanguage french
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/test-installer-upgrade.ps1 -CurrentVersion 0.1.3
```

| Contrôle | Ce qu’il vérifie |
|---|---|
| Accessibilité | Ouverture de l’application, redimensionnement à 1280 × 720 unités logiques en tenant compte du DPI, et présence de noms accessibles pour les commandes visibles dans les onglets. |
| Installation | Installation silencieuse dans un dossier isolé sous `dist`, fichiers attendus, version, langue, puis désinstallation et nettoyage. |
| Mise à jour | Installation d’une ancienne version simulée, ajout de restes d’un ancien runtime .NET, mise à jour et suppression de ces fichiers obsolètes, puis désinstallation. |

Les contrôles d’installation refusent de démarrer si une installation GW GUI est déjà enregistrée pour l’utilisateur. Le contrôle d’accessibilité pilote une fenêtre réelle et nécessite un bureau Windows accessible.
