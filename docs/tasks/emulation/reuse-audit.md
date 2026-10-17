# Audit de réutilisation — relevé initial partiel

- [x] Examiner les changements sans modifier leur comportement.
  - [x] Modifier `docs/tasks/emulation/reuse-audit.md` avec l’inventaire des changements UI et leurs composants existants de comparaison.
  - [x] Modifier `docs/tasks/emulation/reuse-audit.md` avec les comparaisons des périphériques, affectations analogiques, traductions, stockage et paramètres des émulateurs.
  - [x] Modifier `docs/tasks/emulation/reuse-audit.md` avec les défauts confirmés, les réutilisations constatées et les limites des vérifications.

Cet audit produit un relevé dans ce fichier ; il n’autorise pas de refonte supplémentaire ni de build.


## Périmètre et comparaisons

Examen des commits 403ce5836, 74ba79d94, 329fdaec4, 61bd9821e,
7efd5c1e5 et de la fusion 0fe45ec74, ainsi que du sélecteur partagé dans l’arbre
courant. Base de comparaison principale : ae57bace3. Les tâches des sections
USB/multitap, PSP/RPCS3, analogique, langue et CPU dans `commun.md` ont été relues.
Il s’agit d’un audit des sources et de leur réutilisation, pas d’un test d’émulation.

| Ensemble | Comparaison existante |
| --- | --- |
| Sélecteur | Ancien rendu dans `EmulationSectionLayoutFunctions.cs`, nouveau contrôle `EmulationConfigurationSelector.cs`, préférences et événement de suppression |
| CPU/vidéo | `EmulationModuleHardwareSettingsSection.cs`, `EmulationSettingsLayout.CompactCards`, `BoundedForm`, description des champs Sony |
| Champs de stockage | `StorageConfigurationFieldFunctions.cs` face à `EmulationModuleSettingsSection.Fields.cs` et `.Slider.cs`, fenêtres de stockage et contrôleur existants |
| Sticks/vibrations | `InputBindingEditorAnalogFunctions.cs`, `EmulationControllerOutputEditor.cs`, `GameInputControllerSource`, `InputBindingSyntax` |
| Périphériques/visuels | Famille `InputFunctions.*`, `ControllerCatalog`, adaptateurs d’émulateur, `ControllerPortFunctions`, profils et fichiers d’images existants |
| Configuration native | Préparation PCEE2 Cheats/réseau/cartes mémoire, stockage RPCS3, `CoreSettingsFunctions`, langue native PPSSPP/RPCS3 |
| Ressources/tests | Catalogues RESX, catalogue des photos et assertions Sony modifiées |


## Résultats confirmés

### Défauts encore présents

1. **Rendu des champs entretenu en parallèle.** `StorageConfigurationFieldFunctions.Create/Present`
   (lignes 23–93) et `EmulationModuleSettingsSection.CreateSelection/CreateSlider`
   (`.Fields.cs:51`, `.Slider.cs`) construisent séparément radios, listes et sliders.
   Les deux chemins existaient avant la base comparée ; les changements les ont à nouveau
   prolongés séparément (retour à la ligne, labels de choix et unités). Le défaut constaté
   n’est donc pas une création intégrale pendant cette discussion, mais l’entretien
   d’un doublon plutôt que l’extraction du rendu commun. La sauvegarde immédiate des
   options et la validation différée d’une boîte de dialogue restent des comportements
   distincts à préserver lors d’une éventuelle mutualisation.
2. **Modification INI recopiée dans deux nouveaux fichiers.** `Pcee2/Functions/NetworkFunctions.cs:52`
   et `MemoryCardFunctions.cs:39` ont le même `Set` local : recherche de section,
   insertion de section absente, limite de section, remplacement ou insertion de clé.
   Seuls les noms des sections diffèrent. Ces deux fichiers ont été ajoutés dans le
   périmètre examiné. Aucun éditeur INI commun adapté n’a été identifié dans les
   fichiers examinés : cela ne prouve pas son absence dans tout le dépôt.
   De plus, `NetworkFunctions.Prepare` prépare aussi les Cheats et cartes mémoire :
   le nom réseau dissimule une coordination de plusieurs responsabilités.
3. **Anciennes branches laissées après extension du catalogue des visuels.**
   `InputFunctions.Visuals.cs:6–36` traite PS1 et PS2 dans un premier switch, mais
   conserve ensuite les entrées PS1/PS2 du switch de repli et un test GunCon2/PS2.
   Ces entrées ne peuvent plus être atteintes pour ces modèles. L’entrée PS2 de repli
   a même été enrichie dans le diff alors qu’elle était devenue inaccessible.
4. **Dépendance Sony dans une fenêtre générique App.**
   `ManagedStorageConfigurationDialog.cs:28` contient directement la clé
   `Emulation.Sony.Storage.ManagedDirectory.Help`. La fenêtre reçoit déjà `moduleId`
   et des champs génériques mais l’aide ne vient pas du contrat du périphérique.
   Le nouveau type générique ManagedStorage transporte ainsi un détail Sony dans l’App.
5. **Test non adapté au changement de chemin.** `ConfigurationPresentationTests.cs:419`
   attend encore `Sony/PSP/Memory Stick`, tandis que `StorageConstants.cs:16`
   fournit désormais `Memory Stick/Sony/PSP`. Le diff confirme que le chemin a changé
   dans le périmètre examiné et que cette assertion ancienne est restée inchangée.
   Un build réussi de l’application et des modules ne valide pas cette assertion.

### Réutilisations effectivement constatées

- Le rendu CPU appelle les helpers existants `BoundedForm` et `CompactCards` ;
  aucun second conteneur adaptatif n’a été ajouté pour RPCS3.
- Les sticks groupés prolongent `InputBindingEditor`, déduisent les groupes à partir
  des paires `OppositeDirectionId` et écrivent les associations `InputBindingSyntax`
  existantes. Pas de seconde configuration de sticks ni de moteur d’entrée ajouté.
- Les vibrations restent dans `EmulationControllerOutputEditor` et réutilisent
  `GameInputControllerSource` et les associations de sortie existantes.
- Les nouveaux périphériques prolongent le catalogue Sony et les profils d’images
  existants. Plusieurs familles réutilisent `PlayStationButtonDefinitions` et
  `WheelAxesDefinitions` ; les fichiers partiels supplémentaires ne constituent
  pas chacun un éditeur ou un moteur autonome.
- Le stockage RPCS3 décrit ses champs avec `CoreSettingsFunctions.Blocks` et utilise
  le contrôleur/les contrats de stockage existants. Le défaut de clé Sony signalé
  plus haut reste toutefois réel.
- La conversion de langue Sony produit les valeurs natives PPSSPP/RPCS3 à partir
  de la culture App. Les autres fonctions de langue trouvées retournent des identifiants
  d’API libretro ou choisissent la langue de l’App : elles n’ont pas le même contrat.
- Le sélecteur Marque/Machine est maintenant partagé par les deux vues dans
  l’arbre courant. Cette correction n’est pas encore compilée.

### Images et ressources

Les 90 fichiers `Assets/Controllers/sony-*` ont tous un nom référencé dans les
sources ; aucune constante Sony du fichier `ControllerArtworkFileNames` n’est sans
utilisation qualifiée. Aucun groupe de fichiers Sony identiques par SHA-256 et aucun
fichier de plus de 1 Mio n’a été trouvé. Cela ne valide ni la qualité visuelle,
ni le détourage, ni l’absence de deux photos différentes du même modèle.
Le faux nom `.vector` du tapis n’est plus présent dans le système de visuels ;
il référence `sony-dance-mat.png` et un profil de zones.

Un contrôle ciblé des nouvelles entrées invariantes entièrement numériques/majuscules
n’a pas trouvé de duplication de ces nouvelles entrées dans les cultures Sony/App
examinées. Ce contrôle ne vaut pas relecture sémantique de toutes les traductions.


## Bilan

Cinq défauts ouverts sont confirmés dans les sources examinées ; le problème de
mutualisation du sélecteur a déjà été repris dans l’arbre de travail. Les cinq défauts
ne sont pas marqués corrigés. L’audit ne justifie ni de déclarer l’ensemble du travail
correct, ni de conclure que tous les composants ont été recréés indépendamment.

Pendant cet audit, seul ce relevé a été écrit. Pas de correction de code supplémentaire,
de suppression d’images, de modification de configuration personnelle, de test exécuté,
de build ou de commit. Les constats de fonctionnement sont fondés sur les appels et
conditions des sources, pas sur une session d’émulation réelle. Le diff documentaire
a été vérifié sans erreur de whitespace.


## Reprise du périmètre global demandée par l’utilisateur

Le relevé ci-dessus était limité à une partie récente Sony/App et ne répondait pas
à la demande globale. La couverture complète et les corrections à faire sont suivies
dans `docs/tasks/reuse-project-audit.md`. Le présent relevé ne vaut pas audit du projet.
