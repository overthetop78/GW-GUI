# Catalogue et mises à jour de GW GUI et des modules

## Périmètre

Le système recherche les versions de GW GUI et des modules d'émulation officiels, construit
un plan cohérent, puis laisse l'utilisateur choisir les composants et leurs versions. La
recherche n'installe rien. L'application des mises à jour est confiée à l'exécutable dédié
décrit au point 5 de `module-autonomy.md`.

La mise à jour de l'outil Greaseweazle existante dans `HostToolsOptionsController`,
`HostToolsOptionsState` et `GwInstallationManager` reste indépendante : elle gère `gw.exe`
dans les données utilisateur et ne consulte pas ce catalogue.

## Emplacement officiel

Le catalogue public est l'actif `update-catalog.json` de la release technique GitHub au tag
stable `component-catalog` :

`https://github.com/overthetop78/GW-GUI/releases/download/component-catalog/update-catalog.json`

Les workflows de l'application et des modules téléchargent l'actif existant, remplacent
uniquement les entrées qu'ils viennent de publier, puis le renvoient avec écrasement. Ainsi,
publier seulement Atari ne supprime ni les versions Amiga ni les versions de l'application.
Le catalogue n'est mis à jour qu'après la fabrication et la validation des paquets.
Les deux workflows partagent le groupe de concurrence `component-catalog-publication` afin que
leurs séquences lecture-modification-publication ne s'exécutent jamais en même temps.

Une release complète référence, pour l'application et les deux modules qu'elle contient, le tag
de cette release (`vX.Y.Z`). Une release de module isolée référence son tag
`module-<id>-vX.Y.Z`. Une publication sans label actualise le catalogue comme une release stable.
Une snapshot publie ses paquets et son catalogue comme artefact de workflow, mais ne remplace pas
l'actif stable `component-catalog`.

## Schéma du catalogue

Le document JSON possède `schemaVersion: 1`, `generatedAtUtc` et `components`. Chaque composant
contient un identifiant stable, son type et ses versions publiées :

```json
{
  "schemaVersion": 1,
  "generatedAtUtc": "2026-09-08T20:00:00Z",
  "components": [
    {
      "id": "gwgui",
      "kind": "application",
      "releases": [
        {
          "version": "0.1.0",
          "hostApiVersion": "1.0",
          "packageUrl": "https://github.com/overthetop78/GW-GUI/releases/download/v0.1.0/GW-GUI-0.1.0-win-x64-portable.zip",
          "sha256": "<64 caractères hexadécimaux>",
          "notesUrl": "https://github.com/overthetop78/GW-GUI/releases/tag/v0.1.0"
        }
      ]
    },
    {
      "id": "amiga",
      "kind": "module",
      "releases": [
        {
          "version": "1.0.0",
          "hostApiMinimum": "1.0",
          "hostApiMaximum": "1.0",
          "packageUrl": "https://github.com/overthetop78/GW-GUI/releases/download/module-amiga-v1.0.0/GW-GUI-Module-amiga-1.0.0-win-x64.zip",
          "sha256": "<64 caractères hexadécimaux>",
          "notesUrl": "https://github.com/overthetop78/GW-GUI/releases/tag/module-amiga-v1.0.0"
        }
      ]
    }
  ]
}
```

Les identifiants officiels sont `gwgui`, `amiga` et `atari`. Une version contient exactement
trois nombres ; une version d'API en contient deux. `packageUrl` et `notesUrl` sont des URL HTTPS
absolues et `sha256` est l'empreinte du paquet désigné. Une release d'application renseigne
`hostApiVersion`. Une release de module renseigne `hostApiMinimum` et `hostApiMaximum`.

## État installé

La version de l'application vient de son assembly informationnel, sans suffixe de build. Sa
version d'API vient de `EmulationHostApi.CurrentVersion`. Les modules installés viennent des
`module.json` validés et chargés par `EmulationModuleRegistry`. Les versions des cœurs PUAE,
Hatari, Atari800 et autres ne sont jamais interprétées comme des versions de module.

## Recherches et sélection

L'interface propose trois portées :

| Portée | Composants examinés |
|---|---|
| Application | `gwgui` uniquement |
| Modules | Tous les modules officiels actuellement installés |
| Ensemble | Application et modules installés dans un plan commun |

Pour chaque composant, les versions supérieures à la version installée sont triées par
version décroissante. L'utilisateur peut conserver la version proposée ou choisir une autre
version disponible. Aucun composant absent de la portée n'entre dans le plan.

Une recherche Modules n'accepte qu'une release dont l'intervalle d'API contient l'API de
l'application installée. Une recherche Application n'accepte une nouvelle application que si
son API reste comprise dans les bornes de tous les modules installés conservés. Une recherche
Ensemble peut associer une nouvelle application et de nouveaux modules, puis valide la
compatibilité de l'état final complet.

Si une release de module exige une API plus récente, la recherche Modules l'indique comme
`ApplicationUpdateRequired` sans l'ajouter seule au plan. La recherche Ensemble choisit la
version d'application la plus récente qui fournit une API acceptée par ce module et par les
autres modules sélectionnés. En l'absence d'une telle version, la release reste indisponible
et aucune mise à jour incompatible n'est préparée.

## Points d'intégration

Les contrats JSON et la construction pure du plan appartiennent à une bibliothèque
`GWGUI.Updates`, partagée plus tard avec l'updater sans référence à `GWGUI.App` ni aux modules.
`GWGUI.App` ajoute le client HTTP et transforme les manifestes installés en état neutre.

La recherche utilisateur est ajoutée comme onglet distinct dans les préférences :

- `Views/Windows/Options/OptionsWindow.xaml` et `.xaml.cs` hébergent le nouvel onglet ;
- `Views/Controls/Options/OptionsUpdatesSection.xaml` et `.xaml.cs` affichent la portée,
  la recherche, les composants, les versions et l'état ;
- `Options/Controllers/UpdateOptionsController.cs` relie la vue au service sans effectuer de
  remplacement ;
- `Enums/Services/Navigation/OptionsSection.cs` permet d'ouvrir directement cet onglet ;
- les nouvelles chaînes appartiennent aux catalogues App `Options.resx` de toutes les cultures.

Le catalogue est généré par `scripts/build-update-catalog.ps1`. `.github/workflows/release.yml`
met à jour l'application et les deux modules officiels construits avec elle ;
`.github/workflows/module-release.yml` met à jour uniquement le module publié. Les paquets,
empreintes et notes continuent d'être publiés par leurs workflows respectifs.

## Limites de la recherche

Le catalogue est du contenu distant non fiable jusqu'à sa validation complète. Une erreur de
réseau, de JSON, de version, d'URL ou d'empreinte produit un résultat contrôlé. La recherche seule
ne modifie aucun fichier ; le téléchargement et la fermeture commencent uniquement après le choix
explicite d'installer le plan. Elle ne change pas la recherche ni l'installation des cœurs propres
aux modules.

## Protocole d'application des mises à jour

### Installation et droits

`gwgui.exe` est le lanceur placé à la racine de l'installation. Il charge
`lib/gwgui.app.dll` dans son propre processus. Les machines démarrées utilisent également ce
même exécutable avec leurs commandes hôtes ; plusieurs processus peuvent donc verrouiller les
fichiers de l'application et des modules. La fermeture existante de `MainWindow` enregistre les
réglages, demande l'arrêt de toutes les machines par `EmulationBlock.StopAllAsync()` et attend
leur terminaison dans la limite commune de dix secondes.

L'installateur choisit `{autopf}\GW GUI` avec `PrivilegesRequired=lowest`. L'updater travaille
avec les droits du processus utilisateur courant. Il contrôle l'écriture dans l'installation
pendant la préparation et abandonne avant toute fermeture si ces droits sont insuffisants.
Le mode portable conserve ses données dans `Data` sous l'installation ; le mode installé les
conserve dans `%APPDATA%\GW GUI`. Ni `Data`, ni les chemins de stockage configurés, ni les
données Greaseweazle ne font partie d'une transaction de mise à jour.

### Préparation dans l'application

L'application télécharge tous les paquets du plan sélectionné dans
`%TEMP%\GW GUI\Updates\<identifiant>`. Chaque téléchargement est annulable et son SHA-256 est
comparé à l'empreinte du catalogue avant extraction. Une archive d'application doit contenir
une racine unique `GW GUI`; une archive de module doit contenir uniquement
`Modules/<identifiant>`. Les chemins absolus, remontées `..` et liens de réanalyse sont refusés.

Tous les paquets sont extraits et leur compatibilité finale est revalidée avant de créer
`update-plan.json`. La préparation copie aussi le dossier installé `Updater` dans le dossier
temporaire. Ainsi, l'exécutable qui effectue le remplacement et ses DLL ne sont jamais chargés
depuis l'installation qu'il modifie. Si un téléchargement, une empreinte, une archive, un droit
ou la compatibilité échoue, l'updater n'est pas lancé et aucun fichier installé n'est modifié.

Le plan contient son schéma, l'identifiant de transaction, le dossier d'installation, le chemin
de relance, les PID `gwgui.exe` issus exactement de cette installation, les délais, les chemins
de signal et de résultat, puis les composants préparés avec leur type, version et dossier
extrait. L'application lance la copie temporaire de `gwgui.updater.exe`, ferme la fenêtre des
options puis demande la fermeture normale de la fenêtre principale.

### Transaction hors processus

L'updater valide de nouveau le plan et tous ses chemins, puis attend la fin de chaque PID. Un
délai dépassé arrête la transaction sans remplacement. Pour une mise à jour d'application, il
sauvegarde les fichiers et dossiers installés hors `Data` et `Modules`, puis copie le contenu de
la nouvelle racine `GW GUI` en excluant également `Data` et `Modules`. Pour un module, il
sauvegarde et remplace seulement `Modules/<identifiant>`. Tous les chemins sauvegardés sont
inscrits dans le journal de transaction ; aucune donnée utilisateur n'est parcourue.

Si une copie échoue, l'updater supprime les éléments partiellement remplacés et restaure la
sauvegarde avant toute relance. Une fois les copies terminées, il relance exactement une fois
`gwgui.exe` avec les chemins du signal et du résultat. Aucune seconde instance n'est lancée tant
que cette vérification est en cours.

### Signal de démarrage et restauration

Le nouveau processus ne crée le fichier de signal qu'après la fin de `MainWindowLifecycleController.LoadAsync` :
les réglages, modules, capacités et contrôles principaux sont alors chargés et la fenêtre WPF
est opérationnelle. L'updater attend ce signal pendant trente secondes. S'il arrive, il écrit
un résultat de réussite et supprime la sauvegarde. S'il n'arrive pas, il ferme le processus
relancé, restaure l'état précédent, écrit le résultat d'échec puis relance une seule fois la
version restaurée pour présenter le message traduit.

`App.xaml.cs` lit les arguments internes de transaction et transmet leur état à
`MainWindow.xaml.cs`. Ce dernier signale la réussite après `LoadAsync` et présente le résultat
écrit par l'updater. `MainWindowLifecycleController.cs` reste le chemin unique d'arrêt propre :
l'updater ne tue pas une machine pendant le délai normal et ne commence jamais à copier avant
la disparition des processus indiqués.

### Fichiers distribués

`scripts/build.ps1` et `scripts/package.ps1` publient `GWGUI.Updater` dans le sous-dossier
`Updater`. `installer/GWGUI.iss` le distribue par sa règle récursive existante. Comme l'updater
actif s'exécute depuis `%TEMP%`, son propre dossier installé peut être remplacé avec le reste de
l'application. Les fichiers temporaires, archives et sauvegardes sont supprimés après réussite
ou restauration ; seul le résultat nécessaire au message du redémarrage survit jusqu'à sa
lecture, puis il est supprimé.

## État de réalisation et suites différées

Le catalogue commun, les trois portées de recherche, le choix des versions, la préparation complète,
le contrôle SHA-256, l'updater hors processus, le remplacement sélectif, la relance et la restauration
sont réalisés. Les workflows de l'application et des modules publient le catalogue avec des URL qui
désignent la release contenant réellement chaque archive ; une snapshot ne remplace pas le catalogue
stable et les publications concurrentes sont sérialisées.

Le déchargement ou le remplacement d'un module sans redémarrage reste différé. La publication d'un
SDK pour des modules tiers est un chantier séparé ; le catalogue réalisé couvre l'application et les
modules officiels.
