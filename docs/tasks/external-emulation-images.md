# Images externes des manettes et modules d’émulation

- [x] Distribuer les images dans le dossier Assets racine.
  - [x] Modifier `src/GWGUI.Emulation.*/GWGUI.Emulation.*.csproj`, les constantes et catalogues d’images des modules, `IEmulationModule.cs`, `EmulationMachineDefinition.cs`, leurs consommateurs et tests : déclarer tous les fichiers Assets comme contenu externe sous `Assets/Emulation/<module>/` et utiliser des chemins relatifs explicites.
  - [x] Modifier `EmulationAssetFunctions.cs`, `ControllerArtworkCatalog.cs`, créer leurs constantes de chemins et modifier `GWGUI.App.csproj` : lire les images externes avec fermeture des flux et copier les manettes sous `Assets/Controllers/`.
  - [x] Modifier `scripts/release/package/organize_app/shared-component-files.ps1`, `scripts/local-building/build.ps1`, `scripts/release/package/package.ps1` et `src/GWGUI.Updater/UpdateInstallationTransaction.cs`, créer `src/GWGUI.Emulation/Constants/EmulationAssetPaths.cs` : distribuer les Assets des modules à la racine, installer et sauvegarder/restaurer leurs fichiers pendant les mises à jour, conserver les Assets des autres modules lors d’une mise à jour de l’application.
  - [x] Créer puis supprimer `artifacts/validate-external-images.ps1` et son dossier temporaire : vérifier isolément la copie des Assets à la racine, l’installation et restauration des fichiers de modules et la préservation pendant les mises à jour de l’application, en exécutant les sources réelles sans build de GW GUI.
  - [x] Modifier les tests existants d’images et de transaction, les documents de référence et ce fichier après contrôle des chemins, du contenu MSBuild et des scripts, sans build de GW GUI ni fermeture de l’application.

## Résultat vérifié

Les 24 modules déclarent désormais tous leurs Assets en Content avec copie vers `Assets/Emulation/<id>/`, sans EmbeddedResource pour ces fichiers. L’évaluation MSBuild sans compilation a confirmé 215 PNG de modules et les 220 fichiers de manettes distribués sous `Assets/Controllers/`. Les fichiers source restent dans les dossiers Assets existants de leurs projets.

Les contrats SDK exposent `BrandImagePath` et `ImagePath` au lieu de noms de ressources embarquées ; toutes leurs implémentations et leurs consommateurs du dépôt sont adaptés. Les images sont lues depuis le dossier de l’application, chargées intégralement puis figées ; les flux sont fermés après chargement. Ce changement du contrat impose de reconstruire ensemble le SDK, l’hôte et les modules concernés avant distribution ; aucun paquet n’a été publié ici.

Les scripts de build et de packaging placent les Assets des modules sélectionnés à la racine de l’application. Les archives indépendantes conservent le format de paquet validé `Modules/<id>/`, avec les Assets en contenu externe du module préparé ; l’installateur de mise à jour les place ensuite sous `Assets/Emulation/<id>/` et retire leur copie du dossier Modules. La mise à jour d’application préserve le sous-dossier Emulation appartenant aux modules. Leurs images font partie de la sauvegarde/restauration du module concerné.

Contrôles isolés exécutés sur les sources réelles : copie à la racine, absence de duplication sous Modules, installation, mise à jour, restauration explicite, restauration automatique après échec et conservation des images des autres modules. Les tests existants d’images lisent maintenant les fichiers externes. Le script temporaire et ses dossiers de validation ont été supprimés. Aucun build, lancement, arrêt de GW GUI ni remplacement des fichiers de l’essai utilisateur.
