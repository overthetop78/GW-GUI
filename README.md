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
dotnet test GWGUI.sln
dotnet run --project src/GWGUI.App
```

Licence MIT.

## Créer les paquets Windows

Inno Setup 6 est nécessaire pour produire l’installateur. Le script construit une application autonome x64, un ZIP portable, un installateur bilingue et leurs sommes SHA-256 :

```powershell
powershell -ExecutionPolicy Bypass -File scripts/package.ps1 -Version 0.1.0
```

Le ZIP contient `portable.flag` : réglages, journaux et Host Tools gérés sont alors stockés dans `Data` à côté de l’application. L’installateur ne contient pas ce marqueur et utilise les dossiers utilisateur Windows.

Les tags Git `v*` déclenchent le même processus dans GitHub Actions et publient les trois fichiers de distribution dans une release GitHub.
