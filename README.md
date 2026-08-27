# GW GUI

Interface Windows moderne pour Greaseweazle, développée en C#/.NET 10 et WPF.

Le projet vise la lecture, l’écriture, la conversion multiple, la maintenance,
les diagnostics et la visualisation intégrée des captures SCP, sans fenêtre de
console externe.

La spécification complète et les décisions validées se trouvent dans [`docs`](docs/README.md).

## Développement

```powershell
dotnet restore GWGUI.sln
dotnet build GWGUI.sln
dotnet run --project src/GWGUI.App
```

Les tests d'images disque locales sont conservés dans un projet séparé et
doivent être lancés explicitement sur une machine disposant du dossier `image_test` :

```powershell
dotnet test tests/GWGUI.LocalDiskImageTests/GWGUI.LocalDiskImageTests.csproj
```

Licence MIT.

## Créer les paquets Windows

Inno Setup 6 est nécessaire pour produire l’installateur. Le script construit une application autonome x64, un ZIP portable, un installateur bilingue et leurs sommes SHA-256 :

```powershell
powershell -ExecutionPolicy Bypass -File scripts/package.ps1 -Version 0.1.0
```

Le ZIP contient `portable.flag` : réglages, journaux et Host Tools gérés sont alors stockés dans `Data` à côté de l’application. L’installateur ne contient pas ce marqueur et utilise les dossiers utilisateur Windows.

Les captures française et anglaise des guides sont contrôlées comme de véritables PNG à 144 DPI (échelle Windows 150 %), avec une résolution suffisante et une présence effective dans la publication :

```powershell
powershell -ExecutionPolicy Bypass -File scripts/test-guide-images.ps1
```

Après la création des paquets, le test suivant installe silencieusement l’application dans un dossier isolé sous `dist`, contrôle son contenu, sa version et la langue Inno Setup sélectionnée, puis vérifie sa désinstallation complète. Il refuse de démarrer si une installation GW GUI est déjà enregistrée :

```powershell
powershell -ExecutionPolicy Bypass -File scripts/test-installer.ps1 -InstallerLanguage english
powershell -ExecutionPolicy Bypass -File scripts/test-installer.ps1 -InstallerLanguage french
```

Sur une session Windows disposant d’un bureau interactif, le wizard complet peut aussi être parcouru au clavier et contrôlé par UI Automation dans les deux langues. Le test vérifie les cinq pages, l’installation, l’absence de lancement automatique lorsque la case finale reste décochée, puis la désinstallation :

```powershell
powershell -ExecutionPolicy Bypass -File scripts/test-installer-interactive.ps1 -InstallerLanguage english
powershell -ExecutionPolicy Bypass -File scripts/test-installer-interactive.ps1 -InstallerLanguage french
```

L’arbre UI Automation du véritable exécutable peut aussi être contrôlé sur les cinq onglets. Le test impose la taille minimale logique 1280×720 en tenant compte du DPI réel, vérifie les cinq actions principales Lecture/Écriture/Conversion/Effacer/Nettoyer et échoue si un contrôle interactif visible n’a pas de nom accessible :

```powershell
powershell -ExecutionPolicy Bypass -File scripts/test-app-accessibility.ps1
```

Un second test fabrique un ancien installateur de contrôle, effectue une mise à niveau vers le paquet courant, vérifie la version inscrite et celle de l’exécutable, puis désinstalle et nettoie tout l’état isolé. Il refuse de démarrer si une installation GW GUI est déjà enregistrée pour l’utilisateur :

```powershell
powershell -ExecutionPolicy Bypass -File scripts/test-installer-upgrade.ps1 -CurrentVersion 0.1.0
```

Les tags Git `v*` déclenchent le même processus dans GitHub Actions et publient les trois fichiers de distribution dans une release GitHub.
