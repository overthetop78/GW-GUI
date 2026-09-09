# Scripts du projet

Ce document décrit tous les fichiers présents dans `scripts`. Les commandes sont à exécuter depuis la racine du dépôt, sauf indication contraire.

## Vue d’ensemble

| Script | Usage | Appelé automatiquement par |
|---|---|---|
| `build.ps1` | Construction locale de GW GUI | Aucun |
| `build-update-catalog.ps1` | Production du catalogue de mises à jour | Workflows de publication de l’application et des modules |
| `build-wiki.ps1` | Validation et construction du wiki | Publication du wiki et workflow de publication de l’application |
| `emulation-modules.ps1` | Découverte et validation des modules | Scripts de construction, de paquetage et de catalogue |
| `organize-application-output.ps1` | Organisation interne des DLL publiées | `build.ps1` et `package.ps1` |
| `package.ps1` | Création des paquets de l’application | Workflow de publication de l’application |
| `package-module.ps1` | Création du paquet indépendant d’un module | Workflow de publication d’un module |
| `publish-release.cmd` | Déclenchement interactif d’une publication | Aucun |
| `publish-wiki.cmd` | Raccourci Windows de publication du wiki | Aucun |
| `publish-wiki.ps1` | Publication du dépôt wiki | `publish-wiki.cmd` |
| `stop-debug-gwgui.ps1` | Arrêt manuel des processus du build Debug | Aucun |
| `test-app-accessibility.ps1` | Validation manuelle de l’accessibilité de l’interface | Aucun |
| `test-installer.ps1` | Validation d’une installation propre | Workflow de publication de l’application |
| `test-installer-upgrade.ps1` | Validation d’une mise à niveau | Workflow de publication de l’application |
| `translate-resx-argos.py` | Traduction et contrôle des ressources RESX | Aucun |

## Construction et paquetage

### `build.ps1`

Construit une version locale directement utilisable dans `build/Debug/GW GUI` ou `build/Release/GW GUI`.

```powershell
.\scripts\build.ps1 [-Configuration Debug|Release]
```

Sans `-Configuration`, le script construit successivement Debug et Release. Pour chaque configuration, il ferme les processus qui utilisent l’exécutable du dossier cible, supprime l’ancien résultat et le répertoire intermédiaire, puis publie l’application, tous les modules découverts, le lanceur et le programme de mise à jour. Il appelle `organize-application-output.ps1` avant de vérifier la présence de `gwgui.exe`.

Le résultat principal se trouve dans `build/<Configuration>/GW GUI`. Il contient les modules découverts dans le dépôt afin de permettre leur développement et leurs essais locaux. Ce contenu de développement ne représente pas le paquet distribué de GW GUI, qui est volontairement livré sans module. Les fichiers intermédiaires sont créés sous `build/.staging` puis supprimés à la fin.

### `emulation-modules.ps1`

Fournit les fonctions communes de découverte des modules. Ce fichier est chargé avec l’opérateur PowerShell de dot-sourcing ; il ne réalise aucune opération lorsqu’il est exécuté seul.

```powershell
. .\scripts\emulation-modules.ps1
$modules = Get-GwGuiEmulationModules -RepositoryRoot (Resolve-Path .)
$module = Resolve-GwGuiEmulationModule -RepositoryRoot (Resolve-Path .) -Module atari
```

`Get-GwGuiEmulationModules` recherche chaque dossier direct `src/GWGUI.Emulation.*` contenant `module.json`. La fonction vérifie notamment le projet homonyme, le schéma du manifeste, l’identifiant, les versions, les limites d’API hôte, les doublons et la correspondance entre `entryAssembly`, `AssemblyName` et le dossier. `Resolve-GwGuiEmulationModule` sélectionne un module par identifiant de manifeste ou suffixe de dossier.

L’ajout d’un module conforme ne demande donc aucune modification manuelle de la liste dans les scripts qui utilisent ces fonctions.

### `organize-application-output.ps1`

Réorganise les DLL d’un dossier publié et réécrit les chemins correspondants dans le manifeste `.deps.json`.

```powershell
.\scripts\organize-application-output.ps1 `
  -OutputDirectory <dossier> `
  [-ApplicationAssemblyName gwgui.app] `
  [-RootAssemblyName gwgui]
```

Le script place les ressources de langue dans `Languages`, les bibliothèques GW GUI dans `lib` et les autres dépendances dans des sous-dossiers de `lib` selon leur famille. Il déplace également le manifeste de dépendances de l’application dans `lib`. Le dossier fourni est modifié sur place et doit déjà contenir les manifestes `.deps.json` attendus.

Ce script est une étape interne de `build.ps1` et `package.ps1`.

### `package-module.ps1`

Crée l’archive distribuable d’un module découvert par son `module.json`.

```powershell
.\scripts\package-module.ps1 `
  -Module <identifiant-ou-suffixe> `
  [-Configuration Release] `
  [-DistDirectory dist]
```

Le script publie le projet du module pour Windows x64, vérifie la présence du manifeste, de la DLL d’entrée et du fichier `.deps.json`, puis crée `GW-GUI-Module-<id>-<version>-win-x64.zip` et son fichier `.sha256`. L’archive conserve la racine `Modules/<id>`. Les bibliothèques communes fournies par l’hôte et les PDB de Release sont exclues. Le répertoire de travail temporaire est supprimé, y compris après une erreur.

`-DistDirectory` doit désigner un emplacement situé dans le dépôt. Le script est appelé par le workflow de publication d’un module. Il peut aussi être exécuté directement pour produire une archive de module sans reconstruire ni republier GW GUI.

### `package.ps1`

Crée les paquets de l’application GW GUI sans module d’émulation.

```powershell
.\scripts\package.ps1 `
  -Version <X.Y.Z> `
  [-Configuration Release] `
  [-DistDirectory dist] `
  [-SkipInstaller]
```

Le script nettoie les sorties temporaires et les anciens paquets d’application correspondant à la version, tout en conservant les données de `dist/portable/GW GUI/Data`. Il publie uniquement l’application, le lanceur et le programme de mise à jour, organise les DLL, retire les PDB et crée l’archive portable. Il n’appelle pas `package-module.ps1` et n’ajoute aucun dossier `Modules`. Sans `-SkipInstaller`, il construit aussi l’installateur avec Inno Setup. Il produit enfin `SHA256SUMS.txt`.

Le répertoire de distribution doit être situé dans le dépôt. Inno Setup est requis uniquement pour produire l’installateur.

### `build-update-catalog.ps1`

Construit `update-catalog.json` à partir des paquets déjà produits.

```powershell
.\scripts\build-update-catalog.ps1 `
  -Scope Application|Module `
  -Repository <OWNER/REPOSITORY> `
  [-Version <X.Y.Z>] `
  [-ApplicationTag <tag>] `
  [-Module <identifiant-ou-suffixe>] `
  [-ModuleTag <tag>] `
  [-ExistingCatalog <fichier>] `
  [-DistDirectory dist] `
  [-OutputPath dist/update-catalog.json]
```

Pour l’application, le script produit un catalogue de schéma 2 et de nature `application`, contenant
uniquement `gwgui`, l’URL du paquet, celle des notes et la version d’API hôte. Pour un module, il
produit un catalogue de nature `module`, contenant uniquement l’identité demandée, ses bornes d’API
et les URL construites depuis `-Repository` et `-ModuleTag`. Il n’existe pas de portée `All` : chaque
module possède son propre catalogue. Le script vérifie les sommes SHA-256 depuis le fichier
`.sha256` ou `SHA256SUMS.txt`, remplace une release de même version et trie les versions par ordre
décroissant.

Le script écrit uniquement le catalogue local. Le workflow de l’application le publie sous
`application-catalog`; le workflow du module le publie sous `module-<id>-catalog`, ou sous le tag
technique choisi dans le dépôt indépendant du module.

## Wiki et publication

### `build-wiki.ps1`

Valide les sources du wiki et les copie dans `build/wiki` sous une forme compatible avec GitHub Wiki.

```powershell
.\scripts\build-wiki.ps1
```

Le script vérifie les pages Markdown, l’unicité de leurs noms, les liens locaux, les images et la présence du guide de chaque langue de l’interface. Il transforme ensuite les liens locaux vers leurs adresses GitHub Wiki et recrée entièrement `build/wiki`.

Il est appelé par `publish-wiki.ps1` et par le workflow de publication de l’application.

### `publish-wiki.cmd`

Raccourci Windows qui lance `publish-wiki.ps1` avec la stratégie d’exécution PowerShell adaptée.

```cmd
scripts\publish-wiki.cmd
```

### `publish-wiki.ps1`

Construit puis publie le wiki GitHub.

```powershell
.\scripts\publish-wiki.ps1
```

Après `build-wiki.ps1`, le script vérifie l’identité Git, clone `GW-GUI.wiki.git` dans un dossier temporaire unique sous `build`, remplace uniquement les fichiers suivis par `.gwgui-wiki-files.json`, crée un commit s’il existe des changements et les pousse. Le clone temporaire est supprimé à la fin. Une authentification GitHub fonctionnelle et une identité Git configurée sont nécessaires.

### `publish-release.cmd`

Déclenche interactivement le workflow GitHub de publication de l’application.

```cmd
scripts\publish-release.cmd
```

Le script vérifie `gh` et son authentification, demande les nombres Major, Minor et Revision, recherche `.github/release-notes/vX.Y.Z.md`, puis demande le type de publication : Latest, Pre-release ou aucun label. Il lance ensuite `release.yml` sur `main`. Il ne construit pas les paquets localement et n’attend pas la fin du workflow.

Le code et les notes de version à publier doivent déjà être commités et poussés sur `main`.

## Validation

### `test-app-accessibility.ps1`

Effectue un contrôle manuel de l’accessibilité native et du redimensionnement DPI de l’application.

```powershell
.\scripts\test-app-accessibility.ps1 `
  [-ApplicationPath <gwgui.exe>] `
  [-MinimumLogicalWidth 1280] `
  [-MinimumLogicalHeight 720]
```

Le script lance l’application dans une session Windows avec bureau, attend sa fenêtre principale, la redimensionne selon son DPI, parcourt les onglets principaux avec UI Automation et vérifie que les contrôles interactifs visibles possèdent un nom accessible. Il ferme le processus à la fin et le force après le délai prévu si nécessaire.

Ce contrôle n’est pas lancé en CI car il exige un bureau interactif. Il est conservé : aucun autre script ne vérifie les noms accessibles natifs et le comportement de la fenêtre selon le DPI.

### `test-installer.ps1`

Valide une installation propre, son enregistrement Windows et sa désinstallation.

```powershell
.\scripts\test-installer.ps1 `
  [-SetupPath <installateur.exe>] `
  [-InstallDirectory <dossier-dans-dist>] `
  [-ExpectedVersion <X.Y.Z>] `
  [-InstallerLanguage french]
```

Le script refuse d’écraser une installation existante, installe silencieusement le paquet, vérifie l’exécutable, le désinstalleur, la version, la langue enregistrée, l’absence de PDB et l’absence d’anciens documents hors ligne. Il exécute ensuite le désinstalleur et confirme le nettoyage du dossier et de l’enregistrement utilisateur.

Il est conservé et exécuté par le workflow de publication de l’application.

### `test-installer-upgrade.ps1`

Valide la mise à niveau d’une ancienne installation vers la version courante.

```powershell
.\scripts\test-installer-upgrade.ps1 `
  -CurrentSetupPath <installateur.exe> `
  [-CurrentVersion <X.Y.Z>] `
  [-PreviousVersion <X.Y.Z>] `
  [-InstallDirectory <dossier-dans-dist>]
```

Le script fabrique avec Inno Setup un ancien installateur temporaire depuis la publication courante, l’installe, ajoute des fichiers témoins représentant une ancienne distribution autonome, puis installe la version courante. Il vérifie la nouvelle version et la suppression des fichiers obsolètes avant de désinstaller le produit et de retirer ses fichiers temporaires.

Il est conservé et exécuté par le workflow de publication de l’application. Inno Setup est requis.

Les trois scripts de test ont donc chacun une couverture utile : les deux contrôles d’installateur font partie de la publication, et le contrôle d’accessibilité couvre un comportement interactif qui ne peut pas être vérifié par ces workflows. Aucun script de test du dossier n’est inutilisé ou inutile.

## Utilitaires

### `stop-debug-gwgui.ps1`

Arrête manuellement les processus dont l’exécutable se trouve dans le dossier du build Debug.

```powershell
.\scripts\stop-debug-gwgui.ps1 [-BuildDirectory 'build/Debug/GW GUI']
```

Le script demande d’abord la fermeture normale de chaque processus trouvé, attend jusqu’à trois secondes, puis force l’arrêt de ceux qui restent. Il ne supprime aucun fichier. Aucun autre script ne l’appelle ; il sert lorsqu’un build Debug lancé manuellement doit être arrêté.

### `translate-resx-argos.py`

Traduit, synchronise, nettoie et contrôle les ressources `.resx` avec les modèles Argos installés.

```powershell
python .\scripts\translate-resx-argos.py <ressource> <clé> <texte-anglais>
python .\scripts\translate-resx-argos.py --entry <clé> <texte-anglais> [--entry ...]
python .\scripts\translate-resx-argos.py --sync-all
python .\scripts\translate-resx-argos.py --clean-only
python .\scripts\translate-resx-argos.py --audit
python .\scripts\translate-resx-argos.py --repair-mixed
python .\scripts\translate-resx-argos.py --format
```

`--root` permet de choisir un autre dossier de ressources que `src/GWGUI.App/Resources`. Le mode d’ajout insère les clés dans la ressource de base et traduit les textes traduisibles dans toutes les cultures. `--sync-all` complète les entrées absentes, `--clean-only` retire les doublons et replis identiques, `--audit` ne modifie rien et contrôle la structure, les clés, les paramètres réservés et les fragments non traduits, `--repair-mixed` retraduit les entrées mixtes, et `--format` normalise la présentation XML des éléments `data`.

Les modes de traduction modifient directement les fichiers `.resx`. Les termes protégés, paramètres et valeurs invariantes sont préservés. Les modèles Argos nécessaires doivent déjà être installés.
