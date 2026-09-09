# Mises à jour indépendantes de GW GUI et des modules

## Principe

GW GUI connaît deux adresses distantes génériques : son propre catalogue d’application et le
répertoire officiel des modules installables. Il ne contient aucune liste codée en dur ni adresse
propre à Amiga, Atari ou un futur module. Chaque module installé déclare ensuite sa propre source de
mise à jour dans `updateCatalogUrl` de son `module.json`.

Les deux parcours restent séparés :

- la recherche **GW GUI** lit le catalogue d’application publié au tag technique
  `application-catalog` ;
- **Afficher les modules disponibles** lit `module-directory.json`, puis le catalogue indépendant
  de chaque module référencé ;
- la recherche **Modules installés** parcourt les manifestes chargés et lit l’adresse déclarée par
  chacun d’eux.

Le répertoire officiel sert uniquement à la première découverte. Il contient, pour chaque module,
son identifiant, son nom affiché et l’URL HTTPS directe de son catalogue. La première installation
peut aussi venir d’un ZIP choisi par l’utilisateur ou d’une URL saisie manuellement. Après
l’installation, le manifeste du module fournit l’adresse utilisée pour les recherches suivantes.

Le répertoire est produit depuis tous les fichiers `module-registry/*.json`. Ajouter un futur module
officiel consiste donc à ajouter un fichier de registre indépendant ; ni l’application ni le script
de génération ne reçoivent une nouvelle condition ou une nouvelle constante.

La mise à jour de l’outil Greaseweazle reste indépendante dans `HostToolsOptionsController`,
`HostToolsOptionsState` et `GwInstallationManager`.

## Catalogues de schéma 2

Chaque document contient une seule nature de composant :

```json
{
  "schemaVersion": 2,
  "kind": "application",
  "generatedAtUtc": "2026-09-09T10:00:00Z",
  "components": [
    {
      "id": "gwgui",
      "kind": "application",
      "releases": [
        {
          "version": "1.0.0",
          "hostApiVersion": "1.0",
          "packageUrl": "https://github.com/OWNER/REPOSITORY/releases/download/v1.0.0/GW-GUI-1.0.0-win-x64-portable.zip",
          "sha256": "<64 caractères hexadécimaux>",
          "notesUrl": "https://github.com/OWNER/REPOSITORY/releases/tag/v1.0.0"
        }
      ]
    }
  ]
}
```

```json
{
  "schemaVersion": 2,
  "kind": "module",
  "generatedAtUtc": "2026-09-09T10:00:00Z",
  "components": [
    {
      "id": "example",
      "kind": "module",
      "releases": [
        {
          "version": "1.0.0",
          "hostApiMinimum": "1.0",
          "hostApiMaximum": "1.0",
          "packageUrl": "https://github.com/OWNER/REPOSITORY/releases/download/module-example-v1.0.0/GW-GUI-Module-example-1.0.0-win-x64.zip",
          "sha256": "<64 caractères hexadécimaux>",
          "notesUrl": "https://github.com/OWNER/REPOSITORY/releases/tag/module-example-v1.0.0"
        }
      ]
    }
  ]
}
```

Un catalogue d’application ne peut contenir que `gwgui`. Un catalogue de module ne peut contenir
qu’un composant de type `module`, correspondant à l’identité attendue. Les versions de produit ont
trois nombres et les versions d’API deux nombres. Les URL doivent être HTTPS et les empreintes
SHA-256 contenir 64 caractères hexadécimaux.

`ModuleUpdateCatalogValidator` vérifie la nature du catalogue, l’identité du module et toutes les
releases avant d’en sélectionner la version compatible la plus récente. `UpdatePlanBuilder` vérifie
séparément le catalogue d’application ou le catalogue d’un module et ne fabrique jamais de plan
associant plusieurs sources distantes.

## Publication

`.github/workflows/release.yml` publie uniquement GW GUI, son lanceur et son updater. Après une
release stable ou sans label, il remplace `update-catalog.json` au tag `application-catalog`. Une
snapshot conserve le catalogue comme artefact du workflow sans remplacer le catalogue stable.

`.github/workflows/module-release.yml` publie un seul module depuis un tag
`module-<id>-vX.Y.Z`. Il crée l’archive, son fichier `.sha256` et la release du module, puis remplace
le catalogue au tag `module-<id>-catalog`. Ce workflow découvre l’identité depuis les manifestes du
dépôt ; aucun tableau Amiga/Atari n’est entretenu.

Le modèle `sdk/module-template/.github/workflows/release-module.yml` applique le même principe dans
le dépôt indépendant d’un auteur, avec un tag `vX.Y.Z` et un tag de catalogue `module-catalog`.
L’adresse complète de ce dernier appartient au `module.json` de ce module.

`.github/workflows/module-directory.yml` reconstruit le répertoire depuis `module-registry` et
remplace `module-directory.json` dans la release technique `module-directory`. Cette publication est
indépendante des versions de GW GUI et des versions propres aux modules.

`scripts/build-update-catalog.ps1` accepte uniquement `-Scope Application` ou `-Scope Module`. Le
paramètre `-Repository OWNER/REPOSITORY` détermine les URL publiées. Une publication de module exige
également `-Module`, `-ModuleTag` et son paquet déjà construit. Il n’existe plus de portée `All`.

## État installé et interface

La version de l’application vient de son assembly informationnel et sa version d’API de
`EmulationHostApi.CurrentVersion`. Les modules installés viennent des `module.json` validés et
chargés par `EmulationModuleRegistry`. La version d’un cœur PUAE, Hatari, Atari800 ou autre n’est
jamais utilisée comme version du module.

L’onglet **Mises à jour** présente deux sections indépendantes. `ApplicationUpdateService` consulte
seulement `UpdateEndpoints.ApplicationCatalogUrl`. `ModuleDirectoryService` consulte l’unique
`UpdateEndpoints.ModuleDirectoryUrl`, valide le répertoire, puis affiche la dernière version de
chaque module compatible avec l’API hôte. Un module déjà installé reste visible mais son bouton
d’installation est désactivé. `ModuleUpdateService` consulte seulement les `UpdateCatalogUrl` des
modules installés.

L’installation initiale accepte :

- une archive locale ayant exactement la racine `Modules/<id>` ;
- une URL HTTPS directe vers un catalogue de module, dont la release compatible la plus récente est
  téléchargée et vérifiée.

Une confirmation explicite précède la préparation et le redémarrage.

## Préparation et sécurité des archives

`UpdatePackagePreparationService` télécharge les paquets sous
`%TEMP%/GW GUI/Updates/<transaction>`, vérifie leur SHA-256 et appelle `UpdateArchiveValidator`.
Une archive d’application doit avoir une racine unique `GW GUI`. Une archive de module doit avoir
exactement `Modules/<id>`, un manifeste valide de schéma 2, une identité et une version conformes,
une DLL d’entrée présente et une URL de catalogue HTTPS cohérente. Les chemins absolus, remontées
`..` et liens de réanalyse sont refusés.

Le plan sérialisé distingue `UpdateApplication`, `InstallModule` et `UpdateModule`. Tous les paquets
sont préparés avant la fermeture. La copie temporaire de `gwgui.updater.exe` est lancée seulement
après cette validation.

## Transaction hors processus

L’updater attend la fin des PID `gwgui.exe` appartenant à l’installation. Pour une mise à jour de
l’application, il sauvegarde et remplace l’installation hors `Data` et `Modules` : tous les modules
installés sont donc conservés. Pour une mise à jour de module, il sauvegarde et remplace uniquement
`Modules/<id>`. Pour une installation, il crée ce dossier et le supprime si une opération ultérieure
de la transaction échoue.

Toute erreur restaure les éléments remplacés. Après copie, l’updater relance exactement une fois
`gwgui.exe`. Le nouveau processus émet son signal après
`MainWindowLifecycleController.LoadAsync`. Sans signal dans le délai prévu, l’updater ferme ce
processus, restaure l’état précédent et relance la version restaurée afin d’afficher le résultat.

Les données portables sous `Data`, les données utilisateur sous `%APPDATA%/GW GUI`, les chemins de
stockage configurés et les données Greaseweazle ne font jamais partie de la transaction.

## Limite actuelle

Ajouter, retirer ou remplacer un module demande un redémarrage de GW GUI. Le déchargement à chaud
n’est pas pris en charge.
