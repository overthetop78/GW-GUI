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
| `docs/` | Documentation technique, procédures, références, interface, évolutions futures et seules tâches encore ouvertes. |
| `.github/` | Workflows et notes de version. |
| `.codex/` | Consignes locales pour l’assistant de développement. |
| `build/`, `dist/` | Sorties locales générées, ignorées par Git. |

L’organisation et les règles d’entretien de la documentation sont décrites dans
[`documentation.md`](documentation.md). Une feuille terminée ne sert pas d’archive : son résultat utile est
transféré dans la documentation durable, puis la feuille est supprimée.
