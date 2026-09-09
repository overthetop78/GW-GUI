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
