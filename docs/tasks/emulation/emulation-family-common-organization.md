# Rangement complet de Common et des émulateurs

Cette feuille réalise uniquement la seconde phase d’organisation interne des
modules Atari et Amiga. Elle ne change ni les machines proposées, ni leur
comportement, ni les contrats publics de `GWGUI.Emulation`.

Architecture cible :

`GWGUI.App` → `GWGUI.Emulation` → `Common` du module familial → adaptateur dans
`Emulators/<Emulateur>`.

- [x] 1. Figer la propriété et le nom cible de chaque fichier
  - [x] 1.1 Inventorier les fichiers Atari
    - [x] Modifier `docs/reference/emulator-adapter-file-map.md` pour ajouter à chaque fichier de `src/GWGUI.Emulation.Atari/Constants`, `Contracts`, `Dictionaries`, `Enums`, `Exceptions`, `Factories`, `Functions`, `Interfaces` et `Services` son chemin cible sous `Common/<catégorie>` ou `Emulators/<émulateur>/<catégorie>`, ainsi que son nom cible sans préfixe Atari lorsqu’il représente un rôle général du module.
  - [x] 1.2 Inventorier les fichiers Amiga
    - [x] Modifier `docs/reference/emulator-adapter-file-map.md` pour ajouter à chaque fichier de `src/GWGUI.Emulation.Amiga/Constants`, `Contracts`, `Dictionaries`, `Enums`, `Factories`, `Functions`, `Interfaces` et `Services` son chemin cible sous `Common/<catégorie>` ou `Emulators/PUAE/<catégorie>`, ainsi que son nom cible sans préfixe Amiga lorsqu’il représente un rôle général du module.
  - [x] 1.3 Vérifier les équivalences entre familles
    - [x] Modifier `docs/reference/emulator-adapter-file-map.md` pour associer les fichiers Atari et Amiga qui remplissent le même rôle, leur attribuer exactement le même chemin relatif et le même nom sous `Common`, et laisser explicitement sans équivalent les types propres à une machine ou à une famille.

- [x] 2. Compléter les prises internes identiques
  - [x] 2.1 Définir toutes les opérations communes nécessaires
    - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Interfaces/IEmulatorAdapter.cs` et `src/GWGUI.Emulation.Amiga/Common/Interfaces/IEmulatorAdapter.cs` avec les mêmes membres pour la création du runtime, l’état d’installation, la recherche et l’installation des versions, le lancement éventuel du processus hôte et la résolution des médias, sans exposer un type d’émulateur concret.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Contracts/EmulatorCreationContext.cs` et `src/GWGUI.Emulation.Amiga/Common/Contracts/EmulatorCreationContext.cs` avec les mêmes membres et le même ordre pour fournir uniquement les services appartenant à la gestion commune.
  - [x] 2.2 Ajouter les contrats internes nécessaires
    - [x] Créer `src/GWGUI.Emulation.Atari/Common/Contracts/EmulatorManagementContext.cs` et `src/GWGUI.Emulation.Amiga/Common/Contracts/EmulatorManagementContext.cs` avec exactement les mêmes membres pour transporter le client HTTP et le répertoire des cœurs nécessaires aux opérations de gestion.
  - [x] 2.3 Enregistrer les adaptateurs sans les exposer
    - [x] Déplacer et renommer `src/GWGUI.Emulation.Atari/Dictionaries/AtariCoreCatalog.cs` en `src/GWGUI.Emulation.Atari/Common/Dictionaries/EmulatorCatalog.cs`, puis modifier son espace de noms et son contenu pour retourner uniquement les contrats internes communs et les adaptateurs enregistrés.
    - [x] Déplacer et renommer `src/GWGUI.Emulation.Amiga/Dictionaries/AmigaCoreCatalog.cs` en `src/GWGUI.Emulation.Amiga/Common/Dictionaries/EmulatorCatalog.cs`, puis modifier son espace de noms et son contenu selon le même contrat que le catalogue Atari.

- [x] 3. Ranger toute la gestion commune Atari
  - [x] 3.1 Déplacer les catégories communes
    - [x] Déplacer, selon les chemins Atari validés dans `docs/reference/emulator-adapter-file-map.md`, les fichiers familiaux de `src/GWGUI.Emulation.Atari/Constants/` vers `src/GWGUI.Emulation.Atari/Common/Constants/` et modifier leurs espaces de noms et références.
    - [x] Déplacer, selon la carte validée, les fichiers familiaux de `src/GWGUI.Emulation.Atari/Contracts/` vers `src/GWGUI.Emulation.Atari/Common/Contracts/` et modifier leurs espaces de noms et références.
    - [x] Déplacer, selon la carte validée, les fichiers familiaux de `src/GWGUI.Emulation.Atari/Dictionaries/` vers `src/GWGUI.Emulation.Atari/Common/Dictionaries/` et modifier leurs espaces de noms et références.
    - [x] Déplacer, selon la carte validée, les fichiers familiaux de `src/GWGUI.Emulation.Atari/Enums/` vers `src/GWGUI.Emulation.Atari/Common/Enums/` et modifier leurs espaces de noms et références.
    - [x] Déplacer, selon la carte validée, les fichiers familiaux de `src/GWGUI.Emulation.Atari/Exceptions/` vers `src/GWGUI.Emulation.Atari/Common/Exceptions/` et modifier leurs espaces de noms et références.
    - [x] Déplacer, selon la carte validée, les fichiers familiaux de `src/GWGUI.Emulation.Atari/Factories/` vers `src/GWGUI.Emulation.Atari/Common/Factories/` et modifier leurs espaces de noms et références.
    - [x] Déplacer, selon la carte validée, les fichiers familiaux de `src/GWGUI.Emulation.Atari/Functions/` vers `src/GWGUI.Emulation.Atari/Common/Functions/` et modifier leurs espaces de noms et références.
    - [x] Déplacer, selon la carte validée, les fichiers familiaux de `src/GWGUI.Emulation.Atari/Interfaces/` vers `src/GWGUI.Emulation.Atari/Common/Interfaces/` et modifier leurs espaces de noms et références.
    - [x] Déplacer, selon la carte validée, les fichiers familiaux de `src/GWGUI.Emulation.Atari/Services/` vers `src/GWGUI.Emulation.Atari/Common/Services/` et modifier leurs espaces de noms et références.
  - [x] 3.2 Uniformiser uniquement les noms généraux
    - [x] Renommer, selon les correspondances validées dans `docs/reference/emulator-adapter-file-map.md`, les types généraux déplacés sous `src/GWGUI.Emulation.Atari/Common/` pour retirer le préfixe `Atari`, puis modifier leurs constructeurs et toutes leurs références sans renommer les types propres aux machines Atari.
  - [x] 3.3 Conserver la traduction des commandes par machine
    - [x] Modifier les catalogues et contrats déplacés sous `src/GWGUI.Emulation.Atari/Common/` pour conserver les commandes de joystick, clavier, souris et trackball dans les définitions de chaque machine et les transmettre à l’adaptateur sélectionné.

- [x] 4. Isoler chaque émulateur Atari
  - [x] 4.1 Déplacer les éléments encore propres aux cœurs
    - [x] Déplacer, selon les chemins validés dans `docs/reference/emulator-adapter-file-map.md`, les constantes, contrats, dictionnaires, énumérations, fonctions, interfaces et services propres à Hatari vers les catégories correspondantes sous `src/GWGUI.Emulation.Atari/Emulators/Hatari/`, puis aligner leurs espaces de noms.
    - [x] Déplacer de la même manière les fichiers propres à Atari800 sous `src/GWGUI.Emulation.Atari/Emulators/Atari800/`, puis aligner leurs espaces de noms.
    - [x] Déplacer de la même manière les fichiers propres à Stella sous `src/GWGUI.Emulation.Atari/Emulators/Stella/`, puis aligner leurs espaces de noms.
    - [x] Déplacer de la même manière les fichiers propres à ProSystem sous `src/GWGUI.Emulation.Atari/Emulators/ProSystem/`, puis aligner leurs espaces de noms.
    - [x] Déplacer de la même manière les fichiers propres à Beetle Lynx sous `src/GWGUI.Emulation.Atari/Emulators/BeetleLynx/`, puis aligner leurs espaces de noms.
    - [x] Déplacer de la même manière les fichiers propres à Virtual Jaguar sous `src/GWGUI.Emulation.Atari/Emulators/VirtualJaguar/`, puis aligner leurs espaces de noms.
  - [x] 4.2 Adapter les noms internes aux émulateurs
    - [x] Renommer dans chaque `src/GWGUI.Emulation.Atari/Emulators/<émulateur>/` les fichiers et types préfixés `Atari` lorsqu’ils décrivent uniquement l’émulateur concerné, puis modifier leurs références sans changer les identifiants invariants affichés ou persistés.
  - [x] 4.2.1 Limiter les imports à chaque émulateur
    - [x] Modifier les fichiers C# sous `src/GWGUI.Emulation.Atari/Emulators/Atari800/`, `Hatari/` et `VirtualJaguar/` pour importer localement leurs catégories internes nécessaires, sans ajouter de `global using` visible par `Common`.
  - [x] 4.3 Interdire les types concrets dans Common
    - [x] Modifier tous les fichiers sous `src/GWGUI.Emulation.Atari/Common/` qui référencent un espace de noms `GWGUI.Emulation.Atari.Emulators` pour remplacer cette dépendance par les interfaces et contrats internes communs.

- [x] 5. Ranger toute la gestion commune Amiga
  - [x] 5.1 Déplacer les catégories communes
    - [x] Déplacer, selon les chemins Amiga validés dans `docs/reference/emulator-adapter-file-map.md`, les fichiers familiaux des dossiers racine `Constants`, `Contracts`, `Dictionaries`, `Enums`, `Factories`, `Functions`, `Interfaces` et `Services` vers les catégories correspondantes sous `src/GWGUI.Emulation.Amiga/Common/`, puis modifier leurs espaces de noms et références.
  - [x] 5.2 Uniformiser uniquement les noms généraux
    - [x] Renommer, selon les correspondances validées dans `docs/reference/emulator-adapter-file-map.md`, les types généraux déplacés sous `src/GWGUI.Emulation.Amiga/Common/` pour retirer le préfixe `Amiga`, puis modifier leurs constructeurs et toutes leurs références sans renommer les types propres aux machines Amiga.
  - [x] 5.3 Conserver la traduction des commandes par machine
    - [x] Modifier les catalogues et contrats déplacés sous `src/GWGUI.Emulation.Amiga/Common/` pour conserver les commandes de joystick, clavier, souris et trackball dans les définitions de chaque machine et les transmettre à l’adaptateur sélectionné.

- [x] 6. Isoler complètement PUAE
  - [x] 6.1 Déplacer tous les éléments PUAE
    - [x] Déplacer, selon les chemins validés dans `docs/reference/emulator-adapter-file-map.md`, toutes les constantes, contrats, fonctions et services qui connaissent Libretro PUAE, sa DLL, son téléchargement, son protocole hôte ou ses options vers les catégories correspondantes sous `src/GWGUI.Emulation.Amiga/Emulators/PUAE/`, puis aligner leurs espaces de noms.
  - [x] 6.1.1 Limiter les imports à PUAE
    - [x] Modifier les fichiers C# sous `src/GWGUI.Emulation.Amiga/Emulators/PUAE/` pour importer localement leurs catégories internes nécessaires et modifier `src/GWGUI.Emulation.Amiga/EmulationGlobalUsings.cs` afin qu’aucun import PUAE ne soit globalement visible par `Common`.
  - [x] 6.2 Retirer les appels directs depuis Common et Modules
    - [x] Modifier `src/GWGUI.Emulation.Amiga/Common/Services/Machine.cs` pour remplacer toute utilisation directe de `ExternalCore` ou d’un autre type PUAE par les résultats fournis par `IEmulatorAdapter`.
    - [x] Modifier `src/GWGUI.Emulation.Amiga/Modules/AmigaEmulationModule.cs` pour remplacer les appels directs à `CoreHost`, `CoreProvider`, `CoreReleaseService` et `ExternalCore` par le catalogue et les interfaces internes de `Common`.
    - [x] Modifier `src/GWGUI.Emulation.Amiga/Common/Services/Engine.cs` pour sélectionner uniquement un `IEmulatorAdapter` par son identifiant, sans contenir l’identifiant ou le type PUAE.
  - [x] 6.3 Adapter les noms internes à PUAE
    - [x] Renommer sous `src/GWGUI.Emulation.Amiga/Emulators/PUAE/` les fichiers et types préfixés `Amiga` lorsqu’ils décrivent uniquement PUAE, puis modifier leurs références sans changer les identifiants invariants affichés ou persistés.
  - [x] 6.4 Corriger la propriété des réglages des machines Atari 8 bits
    - [x] Déplacer les fichiers `AtariEightBitSettings*`, `AtariEightBitNativeSetting.cs` et `AtariEightBitSettingDisposition.cs` de `src/GWGUI.Emulation.Atari/Emulators/Atari800/` vers les catégories correspondantes de `src/GWGUI.Emulation.Atari/Common/`, puis modifier leurs espaces de noms, car ils définissent les possibilités stables des machines et non l’API de l’émulateur Atari800.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Functions/AtariRuntimeOptionFunctions.cs` pour ne plus dépendre d’une constante appartenant à l’adaptateur Atari800.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Functions/StorageSettingsFunctions.cs` pour employer une erreur de validation commune au lieu d’une constante appartenant à Hatari.

- [x] 7. Relier les modules uniquement à Common
  - [x] 7.1 Corriger le module Atari
    - [x] Modifier `src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs` pour utiliser seulement les contrats, catalogues et services placés sous `src/GWGUI.Emulation.Atari/Common/`, sans référencer un espace de noms ou un type sous `Emulators/`.
  - [x] 7.2 Corriger le module Amiga
    - [x] Modifier `src/GWGUI.Emulation.Amiga/Modules/AmigaEmulationModule.cs` pour utiliser seulement les contrats, catalogues et services placés sous `src/GWGUI.Emulation.Amiga/Common/`, sans référencer un espace de noms ou un type sous `Emulators/`.
  - [x] 7.3 Mettre à jour les imports globaux
    - [x] Modifier `src/GWGUI.Emulation.Atari/EmulationGlobalUsings.cs` pour importer les nouveaux espaces de noms de `Common` nécessaires sans importer globalement les implémentations sous `Emulators/`.
    - [x] Modifier `src/GWGUI.Emulation.Amiga/EmulationGlobalUsings.cs` pour importer les nouveaux espaces de noms de `Common` nécessaires sans importer globalement les implémentations sous `Emulators/`.

- [x] 8. Préparer la reproduction pour Amstrad
  - [x] 8.1 Documenter la structure à copier
    - [x] Modifier `src/GWGUI.Emulation.Amstrad/Common/README.md` avec la liste exacte des fichiers communs Atari et Amiga ayant les mêmes chemins relatifs, noms et membres à reproduire pour Amstrad.
    - [x] Modifier `src/GWGUI.Emulation.Amstrad/Emulators/README.md` pour préciser qu’un nouvel émulateur fournit ses catégories internes, implémente les prises de `Common` et ne communique jamais directement avec `GWGUI.Emulation`.

- [x] 9. Normaliser les fins de ligne du dépôt
  - [x] 9.1 Définir la règle Git
    - [x] Modifier `.gitattributes` pour déclarer les fichiers texte du dépôt en CRLF sous Windows tout en conservant les formats qui exigent LF, puis garder `*.pdf binary`.
  - [x] 9.2 Normaliser les fichiers touchés
    - [x] Modifier les fichiers texte concernés sous `src/GWGUI.Emulation.Atari/`, `src/GWGUI.Emulation.Amiga/`, `src/GWGUI.Emulation/`, `src/GWGUI.App/`, `tests/GWGUI.Tests/` et `docs/` pour appliquer les fins de ligne déclarées sans modifier leur contenu fonctionnel.
  - [x] 9.3 Aligner les fichiers partiels PUAE
    - [x] Déplacer `src/GWGUI.Emulation.Amiga/Emulators/PUAE/Services/ExternalHostCallbacks/ExternalHostCallbacks.AudioVideo.cs`, `ExternalHostCallbacks.Environment.cs` et `ExternalHostCallbacks.Input.cs` vers `src/GWGUI.Emulation.Amiga/Emulators/PUAE/Services/` afin que leur chemin corresponde à l’espace de noms de la classe partielle.
  - [x] 9.4 Qualifier les consommateurs utilisant plusieurs familles
    - [x] Modifier `tests/GWGUI.Tests/Emulation/MachineAdapters/MachineConfigurationMappingScenarios.cs` et `MachineCapabilitiesScenarios.cs` pour employer des alias Atari et Amiga explicites lorsque les nouveaux noms communs sont identiques.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/MachineAdapters/MachineAdapterFailureScenarios.cs` pour fournir à la machine Amiga les médias résolus désormais transmis par son adaptateur.
  - [x] 9.5 Séparer la capacité média propre à Atari
    - [x] Créer `src/GWGUI.Emulation.Atari/Common/Interfaces/IEmulatorMediaAdapter.cs` avec les opérations de préparation des contenus et médias propres aux machines Atari.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Interfaces/IEmulatorAdapter.cs`, `Common/Factories/AtariMachineFactory.cs` et `Common/Services/AtariExternalCore.cs` pour conserver dans `IEmulatorAdapter` uniquement les membres ayant un équivalent Amiga et utiliser `IEmulatorMediaAdapter` pour les capacités Atari supplémentaires.
  - [x] 9.6 Synchroniser la carte finale
    - [x] Modifier `docs/reference/emulator-adapter-file-map.md` pour remplacer l’inventaire de préparation par l’inventaire final des fichiers Atari et Amiga, avec leur propriétaire et leur chemin réel après réorganisation.

- [x] 10. Vérifier l’architecture et le fonctionnement conservé
  - [x] 10.1 Vérifier les chemins et les espaces de noms
    - [x] Modifier `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` pour vérifier que les espaces de noms suivent les dossiers, que les prises communes Atari et Amiga ont les mêmes chemins, noms et membres, et qu’aucun fichier de `Common` ou `Modules` ne référence un espace de noms sous `Emulators/`.
  - [x] 10.2 Vérifier les adaptateurs enregistrés
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Atari/AtariEmulatorAdapterTests.cs` pour vérifier que chaque dossier Atari fournit un adaptateur enregistré uniquement par le catalogue commun et compatible avec les mêmes machines qu’avant le rangement.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Amiga/AmigaEmulatorAdapterTests.cs` pour vérifier que PUAE fournit toutes les opérations attendues par la prise commune sans être référencé directement par le module ou les services communs.
  - [x] 10.3 Vérifier la sélection publique inchangée
    - [x] Modifier `tests/GWGUI.Tests/Emulation/EmulatorManagerTests.cs` pour vérifier que les listes de cœurs, leurs descriptions, leur état d’installation, leur sélection et leur persistance restent identiques après le rangement.
  - [x] 10.4 Consigner les vérifications finales
    - [x] Modifier `docs/tasks/emulation/emulation-family-common-organization.md` avec les résultats de la compilation, des tests ciblés, de l’audit des dépendances, de l’audit des espaces de noms et de `git diff --check`, uniquement après leur réussite.

## Résultats

- `dotnet build tests/GWGUI.Tests/GWGUI.Tests.csproj --no-restore` réussit sans avertissement ni erreur.
- Les 8 tests ciblés d’architecture, d’adaptateurs et de sélection réussissent.
- Les 240 tests d’émulation et d’architecture concernés réussissent.
- L’audit final trouve 0 espace de noms discordant, 0 dépendance de `Common` ou `Modules` vers `Emulators` et 0 fichier absent de la carte finale.
- La suite complète exécute 926 tests : 918 réussissent et 8 échouent dans des domaines extérieurs à cette feuille (`MediaEngineProjectBoundaryTests`, explorateur de médias et aide localisée).

- [x] 11. Uniformiser l’interface du cœur interne
  - [x] 11.1 Employer le même nom dans les deux familles
    - [x] Déplacer et renommer `src/GWGUI.Emulation.Atari/Common/Interfaces/IAtariCore.cs` et `src/GWGUI.Emulation.Amiga/Common/Interfaces/IAmigaCore.cs` en `Common/Interfaces/IEmulatorCore.cs`, puis renommer les types et modifier toutes leurs références dans leur module familial et leurs tests.
  - [x] 11.2 Revalider après l’uniformisation
    - [x] Modifier `docs/tasks/emulation/emulation-family-common-organization.md` avec les résultats de la compilation, des tests d’émulation et des audits finaux après le renommage.

- [x] 12. Synchroniser le modèle Amstrad final
  - [x] 12.1 Ajouter l’interface du cœur commune
    - [x] Modifier `src/GWGUI.Emulation.Amstrad/Common/README.md` pour ajouter `Interfaces/IEmulatorCore.cs` à la liste exacte des fichiers communs à Atari et Amiga.

- [x] 13. Harmoniser les noms de Common entre les familles
  - [x] 13.1 Établir la correspondance des noms communs
    - [x] Modifier `docs/reference/emulator-adapter-file-map.md` pour ajouter, pour chaque fichier encore préfixé `Atari` ou `Amiga` sous `Common`, son nom générique cible ou la justification précise imposant de conserver un nom de machine.
  - [x] 13.2 Regrouper les éléments audio Atari
    - [x] Créer `src/GWGUI.Emulation.Atari/Common/Constants/AudioConstants.cs` avec les constantes de `AtariAudioConstants.cs` et `AtariAudioOutputConstants.cs`, modifier leurs références, puis supprimer ces deux anciens fichiers.
    - [x] Renommer les fichiers, types et références audio génériques sous `src/GWGUI.Emulation.Atari/Common/Functions/` et `src/GWGUI.Emulation.Atari/Common/Services/` afin d'utiliser les noms `AudioFunctions`, `AudioOutputFunctions`, `AudioBuffer` et `AudioOutputController`.
  - [x] 13.3 Regrouper les éléments audio Amiga
    - [x] Créer `src/GWGUI.Emulation.Amiga/Common/Constants/AudioConstants.cs` avec les constantes de `AmigaAudioConfigurationConstants.cs`, modifier leurs références, puis supprimer l'ancien fichier.
    - [x] Renommer `src/GWGUI.Emulation.Amiga/Common/Contracts/AmigaAudioConfiguration.cs` en `AudioConfiguration.cs`, renommer le type et modifier toutes ses références sans mélanger le contrat de configuration avec les constantes audio.
  - [x] 13.4 Préparer le même emplacement pour Amstrad
    - [x] Créer `src/GWGUI.Emulation.Amstrad/Common/Constants/AudioConstants.cs` avec une classe statique vide servant d'emplacement commun jusqu'à l'ajout des constantes audio Amstrad.
  - [x] 13.5 Harmoniser les autres domaines communs
    - [x] Renommer les fichiers et types généralistes encore préfixés sous `src/GWGUI.Emulation.Atari/Common/` selon la correspondance documentée, puis modifier toutes leurs références en conservant les noms désignant réellement une machine ou une technologie Atari.
    - [x] Renommer les fichiers et types généralistes encore préfixés sous `src/GWGUI.Emulation.Amiga/Common/` selon la correspondance documentée, puis modifier toutes leurs références en conservant les noms désignant réellement une machine ou une technologie Amiga.

- [x] 14. Clarifier la propriété des traductions
  - [x] 14.1 Documenter le passage des textes localisés
    - [x] Modifier `src/GWGUI.Emulation/README.md` pour préciser que l'App affiche les textes, tandis que chaque module fournit par `IEmulationModuleLocalization` les traductions propres à ses machines et émulateurs afin que l'App ne connaisse aucun module concret.

- [x] 15. Supprimer les répertoires devenus vides
  - [x] 15.1 Nettoyer Atari, Amiga et Amstrad
    - [x] Supprimer les répertoires vides sous `src/GWGUI.Emulation.Atari/`, `src/GWGUI.Emulation.Amiga/` et `src/GWGUI.Emulation.Amstrad/` après vérification de leur chemin absolu et de l'absence de fichiers.

- [x] 16. Vérifier le rangement harmonisé
  - [x] 16.1 Renforcer les contrôles d'architecture
    - [x] Modifier `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` pour vérifier les noms communs harmonisés, la présence d'`AudioConstants.cs` dans les trois familles et l'absence de répertoires vides suivis dans la structure source.
  - [x] 16.2 Adapter les tests audio
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Audio/AudioBufferScenarios.cs` et les tests Amiga concernés pour employer les nouveaux noms génériques et vérifier que les valeurs et comportements audio restent inchangés.
  - [x] 16.3 Consigner les vérifications finales
    - [x] Modifier `docs/tasks/emulation/emulation-family-common-organization.md` avec les résultats de la compilation, des tests ciblés, des audits de noms et de `git diff --check`, puis cocher les groupes achevés.

## Résultats de l'harmonisation des noms

- `dotnet build tests/GWGUI.Tests/GWGUI.Tests.csproj --no-restore` réussit sans avertissement ni erreur.
- Les 8 tests ciblés d'architecture, d'adaptateurs et d'audio réussissent.
- Les 241 tests d'émulation et d'architecture concernés réussissent ; le seul échec supplémentaire reste le test préexistant `MediaEngineProjectBoundaryTests.LoadedAssembliesStayWithinMediaLibraryBoundaries`, hors de ce rangement.
- L'audit final trouve 0 fichier `Common` préfixé par sa famille, 0 répertoire vide et les trois fichiers `Common/Constants/AudioConstants.cs` attendus.
- `git diff --check` ne signale aucune erreur ni aucun avertissement de fin de ligne.

- [x] 17. Harmoniser le découpage complet de Common
  - [x] 17.1 Définir la structure commune canonique
    - [x] Modifier `docs/reference/emulator-adapter-file-map.md` pour définir, dans chaque catégorie de `Common`, la liste canonique des fichiers communs à Atari, Amiga et Amstrad, ainsi que les fichiers supplémentaires autorisés lorsqu'ils décrivent réellement une machine, un format ou une capacité propre à une famille.
  - [x] 17.2 Regrouper les constantes Atari
    - [x] Modifier les fichiers sous `src/GWGUI.Emulation.Atari/Common/Constants/` pour regrouper les petites classes apparentées dans les fichiers canoniques de leur domaine et supprimer les anciens fichiers dispersés.
  - [x] 17.3 Regrouper les constantes Amiga
    - [x] Modifier les fichiers sous `src/GWGUI.Emulation.Amiga/Common/Constants/` pour employer les mêmes fichiers canoniques qu'Atari, déplacer les constantes existantes dans leur domaine et créer les domaines communs réellement nécessaires.
  - [x] 17.4 Aligner le squelette Amstrad
    - [x] Modifier les fichiers sous `src/GWGUI.Emulation.Amstrad/Common/Constants/` et `src/GWGUI.Emulation.Amstrad/Common/README.md` pour fournir les mêmes emplacements canoniques sans inventer de valeurs propres à des machines Amstrad encore absentes.
  - [x] 17.5 Harmoniser les noms de données équivalentes
    - [x] Modifier les classes de constantes canoniques Atari et Amiga afin que les données ayant le même rôle utilisent le même nom de membre lorsque cela ne change ni une valeur persistée ni un identifiant externe.
  - [x] 17.6 Harmoniser les contrats et les enums
    - [x] Modifier les fichiers sous `src/GWGUI.Emulation.Atari/Common/Contracts/`, `Enums/` et `Dictionaries/` pour regrouper les petits types apparentés par domaine, employer les noms génériques canoniques et conserver séparément uniquement les types propres aux machines Atari.
    - [x] Modifier les fichiers sous `src/GWGUI.Emulation.Amiga/Common/Contracts/`, `Enums/` et `Dictionaries/` pour reprendre les mêmes domaines et noms canoniques, en conservant séparément uniquement les types propres aux machines Amiga.
  - [x] 17.7 Harmoniser les fonctions
    - [x] Modifier les fichiers sous `src/GWGUI.Emulation.Atari/Common/Functions/` pour regrouper les fonctions apparentées dans les domaines canoniques sans déplacer dans `Common` une traduction propre à un émulateur.
    - [x] Modifier les fichiers sous `src/GWGUI.Emulation.Amiga/Common/Functions/` pour reprendre les mêmes domaines et noms canoniques, créer les domaines communs utiles et laisser absentes les fonctions sans comportement Amiga réel.
  - [x] 17.8 Harmoniser les interfaces, services et fabriques
    - [x] Modifier les fichiers sous `src/GWGUI.Emulation.Atari/Common/Interfaces/`, `Services/`, `Factories/` et `Exceptions/` pour employer les prises et services canoniques, regrouper les petits éléments apparentés et conserver séparément les capacités réellement propres à Atari.
    - [x] Modifier les fichiers sous `src/GWGUI.Emulation.Amiga/Common/Interfaces/`, `Services/`, `Factories/` et `Exceptions/` pour reprendre les mêmes prises et services canoniques lorsqu'un rôle équivalent existe, sans créer de comportement factice.
  - [x] 17.9 Isoler les données par machine
    - [x] Déplacer les constantes, contrats, enums, catalogues et fonctions propres à une machine sous le dossier de machine validé dans `src/GWGUI.Emulation.Atari/Common/` et modifier leurs espaces de noms et références.
    - [x] Déplacer les constantes, contrats, enums, catalogues et fonctions propres à une machine sous le dossier de machine validé dans `src/GWGUI.Emulation.Amiga/Common/` et modifier leurs espaces de noms et références.

- [x] 18. Vérifier le nouveau découpage de Common
  - [x] 18.1 Vérifier la structure canonique
    - [x] Modifier `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` pour vérifier les fichiers canoniques de toutes les catégories de `Common` et l'absence des anciens fichiers dispersés.
  - [x] 18.2 Vérifier le fonctionnement conservé
    - [x] Modifier les tests d'émulation concernés sous `tests/GWGUI.Tests/Emulation/` pour employer les nouveaux types de constantes uniquement lorsqu'ils sont directement testés, sans ajouter de test sans comportement observable.
  - [x] 18.3 Consigner les résultats
    - [x] Modifier `docs/tasks/emulation/emulation-family-common-organization.md` avec le résultat de la compilation, des tests ciblés, de l'audit complet de `Common` et de `git diff --check`, puis cocher les tâches réellement terminées.
## Résultats du découpage complet de Common

- Les constantes générales Atari, Amiga et Amstrad utilisent les 18 mêmes fichiers canoniques.
- Les petits contrats, enums et constantes sont regroupés par domaine et par responsabilité.
- Dans chaque module, les catégories transversales aux machines sont regroupées sous
  `Common/Machines/Common`, tandis que les données de modèles restent sous leur famille matérielle.
- Atari sépare désormais `Atari8Bit`, `Atari2600`, `Atari5200`, `Atari7800`, `AtariLynx`,
  `AtariJaguar` et `AtariST`. Les six familles non-ST produisent le contrat matériel commun
  `HardwareModelDefinition`, puis `HardwareModelCatalog` les agrège pour les fonctions communes et
  les adaptateurs d’émulation.
- Amiga sépare désormais `AmigaComputers`, `AmigaCDTV` et `AmigaCD32`, puis agrège leurs catalogues par l'interface générale.
- La compilation des tests réussit sans avertissement ni erreur et les 241 tests d'émulation et d'architecture ciblés réussissent.
- L'audit final trouve 0 répertoire vide et `git diff --check` ne signale aucune erreur.

- [x] 19. Corriger la propriété concrète des émulateurs et le relais des traductions
  - [x] 19.1 Retirer les métadonnées concrètes de Common
    - [x] Créer `EmulatorConstants.cs` dans les six adaptateurs Atari avec leur identité, DLL,
      source, révision et compatibilités, puis modifier le catalogue commun pour découvrir ces données.
    - [x] Créer `Emulators/PUAE/Constants/PuaeConstants.cs` et modifier le catalogue Amiga pour que
      PUAE fournisse lui-même son identité et sa définition localisable.
  - [x] 19.2 Nettoyer les données et noms internes
    - [x] Modifier les fichiers PUAE pour retirer les préfixes internes `Amiga`, réutiliser les
      empreintes de firmwares et extensions communes et regrouper les constantes d'installation.
    - [x] Renommer les fichiers et types `AtariJaguarCd*` sous Virtual Jaguar en `JaguarCd*` et
      modifier Atari800 pour réutiliser la clé de réglage possédée par `Common/Machines/Atari8Bit`.
    - [x] Supprimer tous les répertoires vides du dépôt hors `.git`, dont les neuf
      répertoires vides sous `src/GWGUI.Emulation.Amstrad`.
  - [x] 19.3 Prouver le relais de localisation
    - [x] Modifier `tests/GWGUI.Tests/Interface/SettingsViews/EmulationModuleSettingsNavigationScenarios.cs`
      et `SettingsViewsTests.cs` pour vérifier qu'une description fournie uniquement par
      `IEmulationModuleLocalization` est affichée par l'App.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` pour vérifier la
      propriété des métadonnées, les noms internes et l'absence de dossiers vides.

## Résultats de la propriété des émulateurs et de la localisation

- La compilation de `GWGUI.Tests` réussit sans erreur ; seuls deux avertissements `NU1900` signalent
  l'indisponibilité réseau de l'audit NuGet.
- Les 41 tests ciblés d'architecture, d'adaptateurs et de vues réussissent.
- Les 264 tests d'émulation, d'architecture et de vues concernés réussissent ; l'unique échec du lot
  élargi reste `MediaEngineProjectBoundaryTests.LoadedAssembliesStayWithinMediaLibraryBoundaries`,
  extérieur à ce rangement et déjà consigné.
- Les sept descriptions d'émulateurs Atari et Amiga sont présentes dans les 30 catalogues de chaque
  module et leur clé est relayée jusqu'à l'App.

- [x] 20. Rendre les données génériques et les erreurs réellement indépendantes des émulateurs
  - [x] 20.1 Remplacer les clés PUAE présentes dans Common par des clés Amiga
    - [x] Modifier `src/GWGUI.Emulation.Amiga/Common/Constants/SettingsConstants.cs` et ses
      consommateurs pour employer uniquement des clés `gwgui_amiga_*` dans Common et Modules.
    - [x] Créer `src/GWGUI.Emulation.Amiga/Emulators/PUAE/Constants/PuaeOptionConstants.cs` et
      `Functions/PuaeOptionFunctions.cs` avec la conversion entre les clés génériques et natives.
    - [x] Modifier `src/GWGUI.Emulation.Amiga/Common/Interfaces/IEmulatorAdapter.cs`,
      `Emulators/PUAE/Factories/PuaeMachineFactory.cs` et `Modules/AmigaEmulationModule.cs` pour
      normaliser les anciennes configurations et préparer la configuration native à la frontière.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/MachineAdapters/MachineConfigurationMappingScenarios.cs`
      pour vérifier l'aller-retour entre clés génériques et clés PUAE.
  - [x] 20.2 Centraliser la conversion des erreurs vers le contrat public
    - [x] Créer `src/GWGUI.Emulation/Services/EmulationErrorService.cs` et
      `Exceptions/EmulationLocalizedException.cs`, puis modifier `Enums/EmulationMessageCode.cs`
      pour transporter une erreur localisée ou une erreur générique réutilisable.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Functions/RuntimeFunctions.cs`,
      `src/GWGUI.Emulation.Amiga/Common/Services/Machine.Commands.cs` et
      `src/GWGUI.App/Presenters/Common/ControlErrorPresenter.cs` pour relayer ce contrat jusqu'à App.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/MachineAdapters/MachineAdapterFailureScenarios.cs`
      pour vérifier la catégorie, le code public et la conservation de l'exception technique interne.
  - [x] 20.3 Regrouper et traduire toutes les erreurs PUAE
    - [x] Créer `src/GWGUI.Emulation.Amiga/Emulators/PUAE/Exceptions/PuaeExceptions.cs` avec des
      appels aux clés `Emulation.Error.PUAE.*`, sans constante contenant une phrase d'erreur.
    - [x] Modifier les services sous `src/GWGUI.Emulation.Amiga/Emulators/PUAE/Services/` pour
      remplacer leurs phrases et constantes d'erreur par les appels à `PuaeExceptions`.
    - [x] Supprimer `src/GWGUI.Emulation.Amiga/Emulators/PUAE/Constants/ExternalDiskControlConstants.cs`
      devenu vide et retirer les anciennes phrases des autres fichiers de constantes PUAE.
    - [x] Modifier les 30 fichiers `src/GWGUI.Emulation.Amiga/Resources/*/Emulation.resx` avec
      `scripts/tools/translate-resx-argos.py`, puis corriger par le même script la phrase paramétrée
      afin que les paramètres `{0}` et `{1}` restent identiques dans toutes les cultures.
  - [x] 20.4 Retirer les clés natives des émulateurs Atari de Common
    - [x] Créer les constantes et fonctions de conversion nécessaires sous
      `src/GWGUI.Emulation.Atari/Emulators/Hatari/`, `Atari800/`, `Stella/` et `VirtualJaguar/`, avec
      les clés natives actuellement présentes dans `Common/Constants` et `Common/Machines/Atari8Bit`.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Constants/SettingsConstants.cs`,
      `MachineConstants.cs`, `MediaConstants.cs`, `AudioConstants.cs` et `SettingsTextConstants.cs`
      ainsi que leurs consommateurs pour ne conserver que des identifiants génériques Atari.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Interfaces/IEmulatorAdapter.cs`,
      `Common/Factories/MachineFactory.cs`, les fabriques sous `Emulators/*/Factories/` et
      `Modules/AtariEmulationModule.cs` pour convertir les configurations uniquement à la frontière.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/MachineAdapters/MachineConfigurationMappingScenarios.cs`
      et `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` pour vérifier les conversions
      et interdire les préfixes natifs dans Common et Modules.
  - [x] 20.4.1 Retirer la compatibilité de configuration et restaurer les interfaces inchangées
    - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Interfaces/IEmulatorAdapter.cs`,
      `IEmulatorMediaAdapter.cs` et `src/GWGUI.Emulation.Amiga/Common/Interfaces/IEmulatorAdapter.cs`
      pour retrouver exactement leurs membres antérieurs au rangement interne.
    - [x] Modifier les catalogues, fabriques et modules Atari et Amiga pour conserver les métadonnées
      et conversions dans les classes concrètes, sans normalisation d'ancienne configuration au chargement.
    - [x] Modifier les fonctions d'options sous `Emulators/` et
      `tests/GWGUI.Tests/Emulation/MachineAdapters/MachineConfigurationMappingScenarios.cs` pour
      supprimer les conversions et assertions de compatibilité avec les anciennes clés natives.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` pour vérifier que
      les deux `IEmulatorAdapter` conservent la surface contractuelle antérieure au rangement.
  - [x] 20.4.2 Restaurer le contrat public de GWGUI.Emulation
    - [x] Modifier `src/GWGUI.Emulation/Enums/EmulationMessageCode.cs` et
      `src/GWGUI.App/Presenters/Common/ControlErrorPresenter.cs` pour retirer la valeur publique
      ajoutée et réutiliser le transport de texte existant.
    - [x] Supprimer `src/GWGUI.Emulation/Exceptions/EmulationLocalizedException.cs` et modifier
      `src/GWGUI.Emulation/Services/EmulationErrorService.cs` pour choisir uniquement entre un
      message de module déjà localisé et un code générique existant.
    - [x] Modifier `src/GWGUI.Emulation.Amiga/Common/Services/Machine.Commands.cs` et les tests
      concernés afin que les erreurs PUAE localisées atteignent App sans nouvelle interface publique.
  - [x] 20.5 Verrouiller la chaîne commune de connexion des émulateurs
    - [x] Modifier `docs/reference/emulator-adapter-file-map.md` pour décrire exactement la chaîne
      `App -> GWGUI.Emulation -> module familial -> IEmulatorAdapter -> émulateur`, sans attribuer
      aux interfaces génériques une donnée ou une opération propre à un cœur.
    - [x] Modifier `src/GWGUI.Emulation.Amstrad/Common/README.md` pour reproduire exactement le
      contrat `IEmulatorAdapter`, `EmulatorCreationContext` et `EmulatorManagementContext` commun
      à Atari et Amiga quand le projet Amstrad sera créé, sans créer de classes factices.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` pour comparer le
      texte normalisé des interfaces et contextes communs Atari/Amiga, vérifier leur surface exacte
      et interdire toute dépendance App ou GWGUI.Emulation vers un module familial concret.
  - [x] 20.5.1 Limiter le texte localisé aux erreurs provenant de PUAE
    - [x] Modifier `src/GWGUI.Emulation.Amiga/Common/Services/Machine.cs` et `Machine.Commands.cs`
      pour accepter une fonction interne facultative de présentation des erreurs de démarrage, sans
      ajouter de membre à une interface.
    - [x] Modifier `src/GWGUI.Emulation.Amiga/Emulators/PUAE/Factories/PuaeMachineFactory.cs` pour
      injecter la conversion du texte PUAE déjà localisé vers le message public existant.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/MachineAdapters/MachineAdapterFailureScenarios.cs`
      uniquement si nécessaire pour vérifier qu'un cœur synthétique reste une erreur générique.
  - [x] 20.5.2 Supprimer la limite arbitraire de taille des fichiers
    - [x] Modifier `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` pour retirer le
      rejet des fichiers dépassant 200 lignes tout en conservant les contrôles architecturaux utiles.
    - [x] Modifier `docs/reference/emulator-adapter-file-map.md` et
      `docs/tasks/emulation/emulation-family-common-organization.md` pour supprimer cette règle et
      décrire uniquement un découpage fondé sur les responsabilités.
    - [x] Modifier `src/GWGUI.Emulation.Amiga/Common/Services/Machine.Commands.cs` pour restaurer
      l'espacement lisible supprimé uniquement afin de satisfaire l'ancienne limite.
  - [x] 20.6 Vérifier et nettoyer le résultat
    - [x] Modifier `docs/tasks/emulation/emulation-family-common-organization.md` avec les résultats
      de l'audit Argos, de la compilation, des tests ciblés et de `git diff --check`.
    - [x] Supprimer les répertoires vides du dépôt hors `.git` après les compilations et inscrire le
      résultat dans `docs/tasks/emulation/emulation-family-common-organization.md`.

## Résultats de l'indépendance des émulateurs et des erreurs

- Les contrats publics préexistants de `GWGUI.Emulation` et les interfaces familiales préexistantes
  sont inchangés ; `IEmulatorAdapter` est textuellement identique entre Atari et Amiga hors espace
  de noms.
- Les audits trouvent 0 clé native Atari ou PUAE dans `Common` et `Modules`, ainsi que 0 phrase
  d'erreur PUAE hors de `Emulators/PUAE/Exceptions`.
- L'audit Argos réussit pour les 28 cultures, le catalogue Amiga et ses 1 876 entrées localisées.
- La compilation de `GWGUI.Tests` réussit sans erreur ; les avertissements `NU1900` proviennent de
  l'indisponibilité réseau de l'audit NuGet.
- Le lot élargi exécute 265 tests : les 264 tests d'émulation, d'architecture concernée et de vues
  réussissent. Le seul échec est le contrôle MediaEngine préexistant sur sa référence à
  `gwgui.mediaanalysis`, extérieur à ce rangement.
- L'audit final trouve 0 répertoire vide hors `.git` et `git diff --check` ne signale aucune erreur.
