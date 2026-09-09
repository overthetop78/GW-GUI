# Validation de la distribution indépendante des modules

## Point de départ

Aucune version de GW GUI n’a encore été distribuée. La première publication utilisera directement
l’architecture indépendante : le paquet de l’application ne contiendra aucun module et chaque module
sera installé séparément depuis son propre paquet ou son propre catalogue.

## Installations et remplacements temporaires

Le 9 septembre 2026, les quatre tests temporaires ciblés ont réussi sous
`build/.independent-module-validation` : installation d’un module absent depuis un ZIP, installation
depuis un catalogue HTTP en mémoire, mise à jour d’un seul module avec préservation des autres et
restauration après échec, puis suppression d’un module nouvellement installé lorsqu’une opération
ultérieure échoue. Résultat : 4 réussites, 0 échec, 0 test ignoré.

Le fichier de tests, sa référence temporaire à `GWGUI.Updater` et tous les artefacts de ce contrôle
ont ensuite été supprimés.

## Paquet de l’application

Le paquet portable `0.0.0-validation` produit le 9 septembre 2026 contenait 126 entrées : aucun
dossier `Modules`, aucun `module.json`, aucune DLL officielle Amiga ou Atari et aucune archive de
module dans le dossier de distribution. Les seuls fichiers distribués par ce contrôle étaient le ZIP
portable et `SHA256SUMS.txt`. Toutes les sorties créées pour ce contrôle ont ensuite été supprimées.

## Paquets des modules officiels

Les paquets Amiga et Atari 1.0.0 ont été produits séparément le 9 septembre 2026. Chaque archive
contenait quatre entrées sous `Modules/<id>`, dont un `module.json` de schéma 2 et la DLL d’entrée
attendue. Le manifeste Amiga désigne le catalogue `module-amiga-catalog` et le manifeste Atari le
catalogue `module-atari-catalog`, chacun sous son URL GitHub HTTPS propre. Les deux fichiers
`.sha256` correspondaient à leur archive. Le contrôle précédent confirme qu’aucun de ces fichiers ne
se trouve dans le paquet GW GUI. Toutes les sorties créées pour ce contrôle ont ensuite été supprimées.

## Ressources et build final

L’audit Argos du 9 septembre 2026 a réussi : 29 cultures, 22 catalogues et 41 979 entrées
localisées. Le build Debug complet exécuté avec `scripts/build.ps1 -Configuration Debug` a réussi et
a produit `build/Debug/GW GUI/gwgui.exe` (345 088 octets).

Les tests permanents ciblés `EmulationModuleManifestTests`, `UpdatePlanBuilderTests` et
`ModuleUpdateCatalogValidatorTests` ont ensuite réussi : 45 réussites, 0 échec et 0 test ignoré.
Le dossier d’artefacts créé uniquement pour cette exécution a été supprimé.

## État envoyé sur GitHub

L’implémentation locale validée a été commitée sous `31f46d2e` puis poussée sur `origin/main` le
9 septembre 2026.

Le 9 septembre 2026, la stratégie NuGet.org Trusted Publishing `GW-GUI SDK` a été créée et apparaît
active. Elle appartient à `overthetop78`, autorise uniquement le dépôt `overthetop78/GW-GUI` et le
workflow `sdk-release.yml`, sans environnement GitHub, pour publier de nouveaux paquets et de
nouvelles versions correspondant exactement à `GWGUI.Emulation.SDK`. La variable GitHub Actions
`NUGET_USER` est configurée avec le nom public `overthetop78`. Le workflow utilise OIDC et ne dépend
d’aucune clé API permanente enregistrée dans GitHub.

## Publications externes

Le module Amiga 1.0.0 a été publié par l’exécution GitHub Actions `34349387654` :

- tag et release : `module-amiga-v1.0.0`,
  `https://github.com/overthetop78/GW-GUI/releases/tag/module-amiga-v1.0.0` ;
- archive : `GW-GUI-Module-amiga-1.0.0-win-x64.zip`, SHA-256
  `ad05a22ba6e82f35873ee00b593e9b8ae05ad7082c1f7b63812b5faed004aad1` ;
- catalogue : `module-amiga-catalog`,
  `https://github.com/overthetop78/GW-GUI/releases/download/module-amiga-catalog/update-catalog.json`.

Le module Atari 1.0.0 a été publié par l’exécution GitHub Actions `34349734028` :

- tag et release : `module-atari-v1.0.0`,
  `https://github.com/overthetop78/GW-GUI/releases/tag/module-atari-v1.0.0` ;
- archive : `GW-GUI-Module-atari-1.0.0-win-x64.zip`, SHA-256
  `36e7cfb5f8e9bde301bd94923b6220bf1e581ff833e9b744f1e62f8cdf6b0b26` ;
- catalogue : `module-atari-catalog`,
  `https://github.com/overthetop78/GW-GUI/releases/download/module-atari-catalog/update-catalog.json`.

Les deux catalogues publiés ont été téléchargés puis contrôlés : chacun contient uniquement son
module 1.0.0, l’URL de son archive et le SHA-256 correspondant à l’actif GitHub publié.

Le SDK `GWGUI.Emulation.SDK` 1.0.0 a été publié par l’exécution GitHub Actions `34350086555` avec le
tag `sdk-v1.0.0`. NuGet.org a accepté le paquet principal et le paquet de symboles après échange OIDC.
La release GitHub `https://github.com/overthetop78/GW-GUI/releases/tag/sdk-v1.0.0` contient
`GWGUI.Emulation.SDK.1.0.0.nupkg` et `GWGUI.Emulation.SDK.1.0.0.snupkg` avec les notes de version du
SDK.

GW GUI 0.3.0 a été publié comme release **Latest** par l’exécution GitHub Actions `34352538487` :

- tag et release : `v0.3.0`,
  `https://github.com/overthetop78/GW-GUI/releases/tag/v0.3.0` ;
- archive portable : `GW-GUI-0.3.0-win-x64-portable.zip`, SHA-256
  `6ed1897eac7937de91c6830d60bcebc4967f2401d2326004ab37e3dfa6356b64` ;
- installateur : `GW-GUI-0.3.0-win-x64-setup.exe` ;
- catalogue : `application-catalog`, avec un unique composant `gwgui` de version `0.3.0` pointant
  vers l’archive portable de cette release.

Les notes publiées correspondent à `.github/release-notes/v0.3.0.md`. L’archive portable publiée a
été téléchargée et son empreinte contrôlée. Ses 126 entrées comprennent `GW GUI/gwgui.exe`, aucun
dossier `Modules`, aucun `module.json`, aucune DLL officielle Amiga ou Atari et aucune archive de
module. Les actifs téléchargés uniquement pour ce contrôle ont ensuite été supprimés.

Le paquet `GWGUI.Emulation.SDK` est également visible dans l’index public NuGet.org avec la version
`1.0.0`.

Le README NuGet du SDK a ensuite été ajouté et contrôlé dans un paquet local 1.0.1 : il se trouve à
la racine du `.nupkg`, présente l’installation, le démarrage d’un module, les principaux contrats et
la compatibilité, et renvoie vers les guides complets. Le modèle de module référence maintenant cette
révision sans changement de l’API hôte `1.0` ni du schéma `2`.

Le SDK 1.0.1 a été publié par l’exécution GitHub Actions `34354428912` avec le tag `sdk-v1.0.1` et la
release `https://github.com/overthetop78/GW-GUI/releases/tag/sdk-v1.0.1`. La release contient le
paquet principal et le paquet de symboles. Le paquet principal publié a été téléchargé et contrôlé :
son SHA-256 vaut `d653051973655608bc3e0ab9715b5bdbf060a3b60bd3e7279e4c0947d5236e11` et son
`README.md` se trouve bien à la racine. NuGet.org a accepté cette version ; son apparition dans
l’index public restait soumise au délai d’indexation lors du premier contrôle immédiat.

Le build Debug complet du 9 septembre 2026 a réussi après restauration des dépendances et produit
`build/Debug/GW GUI/gwgui.exe` avec les modules Amiga et Atari destinés aux essais locaux.

## Sélection des modules dans les builds locaux

Le 9 septembre 2026, `scripts/build.ps1` a été corrigé pour créer un dossier `Modules` vide par
défaut et n’ajouter des modules que par `--Module <id>[,<id>...]` ou `--AllModules`. Les options
contradictoires, l’identifiant inconnu et la sélection dupliquée ont été refusés avant construction.

Trois builds Debug successifs ont validé le résultat sans module, avec le seul module `amiga`, puis
avec tous les modules découverts (`amiga` et `atari`). Un paquet portable temporaire a également
confirmé que son dossier exécutable et son ZIP contiennent l’entrée `GW GUI\Modules\` vide, sans
manifeste ni DLL de module. Cette sortie temporaire a été supprimée avec tout le dossier `build`.

Le build final a ensuite été recréé sans option de module. `build` contient uniquement `Debug`,
`build/Debug/GW GUI/gwgui.exe` est présent, et `build/Debug/GW GUI/Modules` existe avec zéro entrée
et aucune DLL officielle Amiga ou Atari ailleurs dans la distribution.

## Découverte des modules officiels

Le 9 septembre 2026, le répertoire local a été construit génériquement depuis les fichiers
`module-registry/amiga.json` et `module-registry/atari.json`. Le document obtenu utilise le schéma 1,
classe Amiga puis Atari et référence directement leurs catalogues indépendants déjà publiés. Aucun
identifiant ni dépôt propre à ces modules n’a été ajouté au code ou au script de génération.

Les 7 tests ciblés `ModuleDirectoryCatalogValidatorTests` et `ModuleDirectoryServiceTests` ont
réussi. Ils couvrent le schéma, le tri, les doublons, les URL invalides, le choix de la dernière
version compatible avec l’API hôte et la désactivation de l’installation pour un module déjà
présent. L’audit Argos a également réussi avec 29 cultures, 22 catalogues et 42 259 entrées
localisées.

Le build Debug propre exécuté avec `scripts/build.ps1 -Configuration Debug` a réussi après
restauration des dépendances. `build` contient uniquement `Debug`, le dossier
`build/Debug/GW GUI/Modules` est vide, et `gwgui.exe` ainsi que `lib/gwgui.app.dll` portent la version
produit `0.3.0`.
