# Développement avec .NET

Les commandes de cette page s’exécutent depuis la racine du dépôt.

Pour restaurer les dépendances, compiler la solution ou lancer le projet WPF directement :

```powershell
dotnet restore GWGUI.sln
dotnet build GWGUI.sln
dotnet run --project src/GWGUI.App
```

## Organisation du dépôt

| Dossier | Contenu |
|---|---|
| `src/` | Application, lanceur et bibliothèques de production. |
| `tests/` | Projets de tests généraux et d’images disque. |
| `scripts/` | Compilation, packaging, publication, contrôles et entretien des traductions. |
| `installer/` | Configuration Inno Setup, langues et prérequis de l’installateur. |
| `wiki/` | Sources de l’aide utilisateur et images partagées. |
| `docs/` | Documentation technique, décisions et suivi des travaux ; son tri reste à poursuivre. |
| `.github/` | Workflows et notes de version. |
| `.codex/` | Consignes locales pour l’assistant de développement. |
| `build/`, `dist/` | Sorties locales générées, ignorées par Git. |
