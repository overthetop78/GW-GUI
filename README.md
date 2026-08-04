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
