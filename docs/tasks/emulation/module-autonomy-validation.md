# Autonomie des modules — relevé de validation

## Point 1 : traductions embarquées

Validation effectuée le 8 septembre 2026 sur les modifications de la feuille
`module-autonomy.md` ; les autres points ne sont pas encore validés.

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
