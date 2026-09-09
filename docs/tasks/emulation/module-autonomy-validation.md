# Autonomie des modules — relevé de validation

## Point 1 : traductions embarquées

Validation effectuée le 8 septembre 2026 sur les modifications de la feuille
`module-autonomy.md`. Les validations suivantes sont relevées par point.

- Audit Argos Amiga : réussi, 29 cultures, 1 catalogue, 406 entrées localisées.
- Audit Argos Atari : réussi, 29 cultures, 1 catalogue, 1 989 entrées localisées.
- Transfert : 26 clés neutres Amiga et 97 clés neutres Atari, avec les traductions existantes.
  Les copies de valeurs identiques à la base ne sont pas conservées dans les cultures.
- `dotnet test tests/GWGUI.Tests/GWGUI.Tests.csproj -c Debug --filter FullyQualifiedName~Localization --no-restore --verbosity minimal` : 36 tests réussis, aucun échec ni test ignoré.
  Dont 3 nouveaux tests autonomes conservés pour la priorité, l'indépendance des catalogues,
  le repli App, les clés absentes, les valeurs vides et les cultures de recherche/formatage.
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build.ps1 -Configuration Debug` :
  réussi ; sortie `build/Debug/GW GUI/gwgui.exe`, avec publication séparée des deux modules.

Une première compilation a relevé un accès à `_moduleId` dans une méthode statique après mon
raccordement ; la méthode de reconstruction des associations a été corrigée avant les résultats
ci-dessus. Les premières invocations de Python et du build ont nécessité une exécution hors du
bac à sable pour l'alias Python et la restauration réseau ; aucune dépendance n'a été remplacée.

Essai temporaire `TemporaryModuleLocalizationTests` : réussi (1 scénario). Les deux assemblies
contiennent chacune 30 catalogues embarqués. Pour toutes les cultures et toutes les machines
déclarées, les noms, titres de blocs, champs, explications et choix décrits par les modules se
résolvent. Le repli `es-MX` vers l'anglais et les bindings WPF lors du passage français/allemand
ont été vérifiés sur les clés embarquées. Le scénario n'a créé aucun dossier de données.

La concurrence module/App a été contrôlée par le test autonome en mémoire, sans altérer les
ressources de production. Les essais n'exécutent aucun cœur et ne remplacent pas une validation
interactive des panneaux complets ni d'une session d'émulation, qui reste à effectuer.

Nettoyage : le fichier `TemporaryModuleLocalizationTests.cs` a été supprimé après l'essai.
Aucune fixture ni ressource factice n'est conservée. Seul `ModuleLocalizationTests.cs` est ajouté
aux tests permanents ; les fichiers de compilation habituels restent dans les sorties ignorées.

## Point 2 : manifestes obligatoires et compatibilité

Validation effectuée le 8 septembre 2026. Les manifestes Amiga et Atari déclarent chacun
`moduleVersion: 1.0.0`, avec API minimale et maximale `1.0`. Aucun chargement des DLL seules
sans manifeste n'est conservé. Le point 2 ne comprend pas les paquets indépendants ni
l'isolation des dépendances privées prévus au point 3.

- Tests permanents en mémoire : 29 cas dans `EmulationModuleManifestTests.cs`, couvrant les
  champs obligatoires, les chemins interdits, les versions numériques, les bornes inclusives,
  le JSON mal formé, les champs dupliqués et le schéma inconnu.
- Commande finale : `dotnet test tests/GWGUI.Tests/GWGUI.Tests.csproj -c Debug --filter "FullyQualifiedName~EmulationModuleManifestTests|FullyQualifiedName~Localization" --no-restore --verbosity minimal`.
  Résultat : 65 réussites, aucun échec ni test ignoré, dont les 36 tests de localisation existants.
  Cette exécution a recompilé la DLL de tests après retrait de la factory temporaire.
- Essai temporaire `TemporaryModuleDiscoveryTests` : un scénario réussi avec les DLL réelles
  et une factory factice, dans `build/.module-manifest-validation`, sans toucher aux données utilisateur.
  Dossier absent/vide, DLL directement dans Modules ignorée, Amiga seul, Atari seul et les deux
  ensemble : résultats attendus. Les deux catalogues de machines sont accessibles.
- Refus vérifiés sans bloquer le module valide : manifeste absent, JSON invalide, API incompatible,
  identité différente de la factory, DLL absente, DLL invalide, doublon d'identifiant sans distinction
  de casse, assembly sans factory et exception pendant `Create`. Les diagnostics sont collectés
  pendant l'essai ; les succès indiquent version et chemin.
- Retrait temporaire du dossier Amiga : Atari reste chargé et le fichier témoin dans
  `Data/Emulation/Machines/Amiga` reste intact. Le dossier Amiga est ensuite remis en place.
- Routage sur les modules découverts : `--amiga-core-host` et `--atari-core-host` ouvrent leur
  communication locale, reçoivent `Dispose`, répondent avec succès et terminent avec code 0.
  Le protocole Atari utilise son en-tête de version existant. `--atari-option-probe` avec un
  cœur absent est reconnu et renvoie le code d'échec 1. Les arguments vides sont laissés à App.
  Aucun cœur natif ni jeu n'est démarré par ces essais ; ils ne remplacent pas un essai interactif
  d'émulation dans le nouveau paquet.
- Build : `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build.ps1 -Configuration Debug`
  réussi. Exécutable vérifié : `build/Debug/GW GUI/gwgui.exe`. Les deux sous-dossiers de Modules
  contiennent leur `module.json` et leur DLL, version d'assembly `1.0.0.0`. Aucune DLL de module
  ne reste directement à la racine de Modules.

Les premières exécutions ont relevé des erreurs dans les nouveaux tests : assertion exigeant
le type exact au lieu du sous-type de `JsonException`, puis simulation incomplète des protocoles
hôtes. Les tests ont été corrigés après relecture des protocoles existants, sans modifier les moteurs.

Nettoyage effectué : suppression de `TemporaryModuleDiscoveryTests.cs` et du dossier
`build/.module-manifest-validation`, avec ses DLL copiées, manifestes factices et données témoins.
Le paquet Debug final ne contient aucun de ces fichiers. Seul le test autonome des manifestes
est conservé pour ce point. Aucun commit ni push n'a été effectué pour ce chantier.

## Point 3 : paquets indépendants et paquet complet

Validation effectuée le 8 septembre 2026.

- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build.ps1 -Configuration Debug` :
  réussi. `build/Debug/GW GUI/gwgui.exe` est présent. Chaque dossier de module contient
  `module.json`, sa DLL d'entrée, son PDB et son fichier `.deps.json`. Les contrats,
  `gwgui.mediaengine` et DiscUtils restent uniquement dans les bibliothèques communes.
- `scripts/package-module.ps1` a produit Amiga et Atari en Debug puis en Release. Les archives
  Release finales `GW-GUI-Module-amiga-1.0.0-win-x64.zip` et
  `GW-GUI-Module-atari-1.0.0-win-x64.zip` contiennent chacune uniquement
  `Modules/<id>/module.json`, la DLL d'entrée et son `.deps.json`. Aucun PDB ni contrat partagé
  n'est inclus. Les deux empreintes `.sha256` correspondent aux archives.
- Une première vérification a trouvé les DLL `DiscUtils.*` dupliquées parce que leur nom de
  fichier ne commence pas par `LTRData`. Le filtre du script a été corrigé et les deux
  archives ont été reconstruites avant les résultats finaux ci-dessus.
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/package.ps1 -Version 0.1.0
  -Configuration Release` : réussi. Le portable, l'installateur, les deux archives de modules,
  leurs empreintes et `SHA256SUMS.txt` sont produits dans `dist`. Le portable et la source de
  l'installateur contiennent les deux dossiers de modules sans PDB.
- Installation portable temporaire dans `dist/.module-validation` : le premier démarrage charge
  Amiga et Atari. Application fermée, Amiga a été remplacé par le contenu exact de son archive
  indépendante ; le redémarrage charge encore Amiga et Atari. Après retrait du dossier Atari,
  Amiga reste chargé. Les dossiers `Data/Emulation/Machines/amiga/Configurations` et
  `Data/Emulation/Machines/atari/Configurations` restent présents après le retrait du module.
- Tests ciblés :
  `dotnet test tests/GWGUI.Tests/GWGUI.Tests.csproj -c Debug --no-restore --filter
  "FullyQualifiedName~EmulationModuleManifestTests|FullyQualifiedName~ModuleLocalizationTests"` :
  32 réussites, aucun échec ni test ignoré. Les catalogues embarqués et leurs replis restent
  couverts ; aucun nouveau test reposant sur une DLL externe n'est conservé.

Le smoke test portable est donc effectué avec les deux modules, le remplacement d'un seul et
le retrait de l'autre. L'installateur Release a été compilé avec succès par Inno Setup et son
contenu inclut les deux dossiers de modules. Les smoke tests installateur anglais, français et
upgrade ne sont pas exécutés : `test-installer.ps1` s'est arrêté avant toute installation car
une installation GW GUI est déjà enregistrée sur le poste. Ils restent à exécuter sur un poste
ou environnement sans cette inscription ; l'installation existante n'a pas été modifiée.

Nettoyage du point 3 : `dist/.module-validation` et son extraction de remplacement ont été
supprimés. Aucun dossier de smoke test installateur n'a été créé, puisque le contrôle s'est
arrêté avant l'installation. Les répertoires de travail `.module-package-*` et
`.application-publish` sont absents. Les archives, empreintes, paquet portable, source
d'installateur et installateur compilé restent dans `dist` comme sorties du point 3.

## Point 4 : catalogue et recherche des mises à jour

Validation effectuée le 9 septembre 2026. `build-update-catalog.ps1` a généré un catalogue v1
à partir du portable et des deux archives de modules déjà présents dans `dist`. Les trois
empreintes ont été recalculées et comparées aux fichiers de contrôle avant l'écriture. Une
seconde génération limitée au module Amiga, avec le premier catalogue en entrée, a conservé
les composants `gwgui` et `atari` ainsi que leurs releases.

Les six tests autonomes de `UpdatePlanBuilderTests` réussissent : module à jour, choix explicite
d'une version compatible, API hôte trop ancienne, recherche Modules isolée, recherche
Application isolée et plan Ensemble comprenant une application et un module compatibles.

L'essai temporaire `TemporaryUpdateCatalogTests` réussit avec deux scénarios. Un client HTTP en
mémoire fournit le catalogue sans accès réseau ; les commandes Application, Modules et Ensemble
produisent respectivement une, une et deux lignes. La section WPF actualise aussi ses libellés
entre `fr-FR` et `en-US`. Aucun paquet n'est téléchargé, aucun fichier installé n'est remplacé
et aucun dossier de données utilisateur n'est utilisé par cet essai.

Nettoyage du point 4 : `TemporaryUpdateCatalogTests.cs` a été supprimé. Il n'avait créé aucun
catalogue sur disque ni aucune sortie dédiée. La DLL de tests a été reconstruite après ce retrait
et les six tests autonomes de `UpdatePlanBuilderTests` réussissent encore, sans échec ni test ignoré.

## Point 5 : application transactionnelle des mises à jour

Validation effectuée le 9 septembre 2026 dans `build/.update-validation`, sans toucher à
l'installation existante ni à ses données. Les dix scénarios temporaires réussissent ensemble :

- une archive de module ayant la racine attendue est extraite, tandis qu'une remontée `..` est
  refusée sans écrire hors de la destination ;
- un module seul est remplacé, et une transaction Application + Amiga + Atari remplace uniquement
  ces composants ;
- l'application seule est remplacée hors `Data` et `Modules`, sans transformer une installation
  en mode portable, puis l'état précédent est restauré en conservant les données ;
- un PID encore actif produit le dépassement prévu, tandis qu'un PID déjà absent est accepté ;
- la disparition du paquet préparé pendant le remplacement produit une restauration complète ;
- l'annulation pendant la lecture HTTP interrompt la préparation et supprime le dossier de travail ;
- une empreinte invalide arrête la préparation avant extraction et laisse l'installation intacte ;
- une préparation valide écrit le plan, extrait le module et copie l'updater temporaire ;
- les messages de résultat existent en français et en anglais ;
- l'updater réellement exécuté sur une copie du build remplace l'application, relance le vrai
  `gwgui.exe`, reçoit son signal après chargement et conserve le fichier témoin ainsi que `Data` ;
- avec un nouveau `gwgui.exe` volontairement invalide, l'updater restaure l'exécutable précédent,
  conserve `Data` et relance cette version restaurée une seule fois.

Ce dernier essai a révélé avant le résultat final que l'exception directe de `Process.Start`
n'entrait pas dans la branche de restauration. Le lancement est maintenant capturé comme un
échec de démarrage ; le scénario corrigé et les dix scénarios groupés réussissent.

`scripts/package.ps1 -Version 0.1.0 -Configuration Release -SkipInstaller` réussit après cette
correction. Le portable final et `dist/publish/win-x64` contiennent
`Updater/gwgui.updater.exe` et ses dépendances. L'exécutable est présent dans l'archive portable,
son empreinte correspond à `SHA256SUMS.txt` et aucun dossier de staging du paquet ne subsiste.

Nettoyage du point 5 : `TemporaryUpdaterTransactionTests.cs` et
`TemporaryUpdateEndToEndTests.cs` ont été supprimés, ainsi que la référence de projet et
`InternalsVisibleTo` ajoutés uniquement pour ces essais. `build/.update-validation` et le dossier
relatif créé par la première invocation ont été supprimés après vérification de leur chemin.
Aucun dossier `.update-validation` ou `.staging` ne subsiste.

Après ce nettoyage, la DLL de tests a été reconstruite et les six tests en mémoire de
`UpdatePlanBuilderTests` réussissent. L'audit RESX réussit avec 29 cultures, 22 catalogues et
41 755 entrées localisées. Le build final exécuté par
`powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build.ps1 -Configuration Debug`
réussit ; `build/Debug/GW GUI/gwgui.exe` et
`build/Debug/GW GUI/Updater/gwgui.updater.exe` sont présents.

Correction finale du catalogue : les sérialisations locales stable et snapshot contiennent trois
composants et pointent toutes vers le tag qui porte effectivement leurs paquets, respectivement
`v0.1.0` et `v0.1.0-snapshot`. Les fichiers de validation ont ensuite été supprimés. Le workflow
stable et la publication sans label republient le catalogue ; la snapshot ne remplace pas le
catalogue stable. Les workflows Application et Module utilisent le même groupe de concurrence
pour sérialiser la modification de l'actif.

La lecture de l'état installé ignore maintenant les dossiers de modules dont le manifeste a déjà
été refusé par le registre ; ils ne bloquent donc pas la recherche des modules effectivement
chargés. L'application compile sans avertissement et les six tests de plan réussissent après
cette correction.

## Point 7 : découverte automatique des futurs modules

Validation effectuée le 9 septembre 2026. `scripts/emulation-modules.ps1` découvre les dossiers
directs `src/GWGUI.Emulation.*` qui possèdent un projet homonyme et un `module.json` conforme. Un
dépôt factice sous `build/.module-script-validation` contenant Alpha, Beta et Gamma a confirmé la
découverte des trois familles et la sélection de Beta par son identifiant, sans modifier de liste.
Les essais ont également confirmé le refus d'une identité différente du nom de dossier et d'un
identifiant dupliqué. Le dépôt factice et son script d'essai ont ensuite été supprimés.

`scripts/package.ps1 -Version 0.1.0 -Configuration Release -SkipInstaller` a ensuite réussi avec
`build/.module-package-validation` comme sortie. Les archives indépendantes de tous les manifestes
réels ont été produites et intégrées dans le paquet complet. Le catalogue créé avec `-Scope All`
contient un composant par manifeste découvert et chaque composant possède son archive correspondante.
Le dossier `build/.module-package-validation` a été supprimé après ces contrôles.
