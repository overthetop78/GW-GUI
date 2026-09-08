# Autonomie des modules — relevé de validation

## Point 1 : traductions embarquées

Validation effectuée le 8 septembre 2026 sur les modifications de la feuille
`module-autonomy.md`. Le point 2 est relevé séparément ci-dessous ; les points 3 à 5 restent à réaliser.

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
