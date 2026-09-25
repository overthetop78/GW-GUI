# Rangement complet de Common et des émulateurs

Cette feuille réalise uniquement la seconde phase d’organisation interne des
modules Atari et Amiga. Elle ne change ni les machines proposées, ni leur
comportement, ni les contrats publics de `GWGUI.Emulation`.

Architecture cible :

`GWGUI.App` → `GWGUI.Emulation` → `Common` du module familial → adaptateur dans
`Emulators/<Emulateur>`.

- [ ] 1. Figer la propriété et le nom cible de chaque fichier
  - [ ] 1.1 Inventorier les fichiers Atari
    - [ ] Modifier `docs/reference/emulator-adapter-file-map.md` pour ajouter à chaque fichier de `src/GWGUI.Emulation.Atari/Constants`, `Contracts`, `Dictionaries`, `Enums`, `Exceptions`, `Factories`, `Functions`, `Interfaces` et `Services` son chemin cible sous `Common/<catégorie>` ou `Emulators/<émulateur>/<catégorie>`, ainsi que son nom cible sans préfixe Atari lorsqu’il représente un rôle général du module.
  - [ ] 1.2 Inventorier les fichiers Amiga
    - [ ] Modifier `docs/reference/emulator-adapter-file-map.md` pour ajouter à chaque fichier de `src/GWGUI.Emulation.Amiga/Constants`, `Contracts`, `Dictionaries`, `Enums`, `Factories`, `Functions`, `Interfaces` et `Services` son chemin cible sous `Common/<catégorie>` ou `Emulators/PUAE/<catégorie>`, ainsi que son nom cible sans préfixe Amiga lorsqu’il représente un rôle général du module.
  - [ ] 1.3 Vérifier les équivalences entre familles
    - [ ] Modifier `docs/reference/emulator-adapter-file-map.md` pour associer les fichiers Atari et Amiga qui remplissent le même rôle, leur attribuer exactement le même chemin relatif et le même nom sous `Common`, et laisser explicitement sans équivalent les types propres à une machine ou à une famille.

- [ ] 2. Compléter les prises internes identiques
  - [ ] 2.1 Définir toutes les opérations communes nécessaires
    - [ ] Modifier `src/GWGUI.Emulation.Atari/Common/Interfaces/IEmulatorAdapter.cs` et `src/GWGUI.Emulation.Amiga/Common/Interfaces/IEmulatorAdapter.cs` avec les mêmes membres pour la création du runtime, l’état d’installation, la recherche et l’installation des versions, le lancement éventuel du processus hôte et la résolution des médias, sans exposer un type d’émulateur concret.
    - [ ] Modifier `src/GWGUI.Emulation.Atari/Common/Contracts/EmulatorCreationContext.cs` et `src/GWGUI.Emulation.Amiga/Common/Contracts/EmulatorCreationContext.cs` avec les mêmes membres et le même ordre pour fournir uniquement les services appartenant à la gestion commune.
  - [ ] 2.2 Ajouter les contrats internes nécessaires
    - [ ] Créer sous `src/GWGUI.Emulation.Atari/Common/Contracts/` et `src/GWGUI.Emulation.Amiga/Common/Contracts/` les contrats de résultat nécessaires aux nouveaux membres de `IEmulatorAdapter`, avec exactement les mêmes noms, chemins relatifs et membres dans les deux modules.
  - [ ] 2.3 Enregistrer les adaptateurs sans les exposer
    - [ ] Déplacer et renommer `src/GWGUI.Emulation.Atari/Dictionaries/AtariCoreCatalog.cs` en `src/GWGUI.Emulation.Atari/Common/Dictionaries/EmulatorCatalog.cs`, puis modifier son espace de noms et son contenu pour retourner uniquement les contrats internes communs et les adaptateurs enregistrés.
    - [ ] Déplacer et renommer `src/GWGUI.Emulation.Amiga/Dictionaries/AmigaCoreCatalog.cs` en `src/GWGUI.Emulation.Amiga/Common/Dictionaries/EmulatorCatalog.cs`, puis modifier son espace de noms et son contenu selon le même contrat que le catalogue Atari.

- [ ] 3. Ranger toute la gestion commune Atari
  - [ ] 3.1 Déplacer les catégories communes
    - [ ] Déplacer, selon les chemins Atari validés dans `docs/reference/emulator-adapter-file-map.md`, les fichiers familiaux de `src/GWGUI.Emulation.Atari/Constants/` vers `src/GWGUI.Emulation.Atari/Common/Constants/` et modifier leurs espaces de noms et références.
    - [ ] Déplacer, selon la carte validée, les fichiers familiaux de `src/GWGUI.Emulation.Atari/Contracts/` vers `src/GWGUI.Emulation.Atari/Common/Contracts/` et modifier leurs espaces de noms et références.
    - [ ] Déplacer, selon la carte validée, les fichiers familiaux de `src/GWGUI.Emulation.Atari/Dictionaries/` vers `src/GWGUI.Emulation.Atari/Common/Dictionaries/` et modifier leurs espaces de noms et références.
    - [ ] Déplacer, selon la carte validée, les fichiers familiaux de `src/GWGUI.Emulation.Atari/Enums/` vers `src/GWGUI.Emulation.Atari/Common/Enums/` et modifier leurs espaces de noms et références.
    - [ ] Déplacer, selon la carte validée, les fichiers familiaux de `src/GWGUI.Emulation.Atari/Exceptions/` vers `src/GWGUI.Emulation.Atari/Common/Exceptions/` et modifier leurs espaces de noms et références.
    - [ ] Déplacer, selon la carte validée, les fichiers familiaux de `src/GWGUI.Emulation.Atari/Factories/` vers `src/GWGUI.Emulation.Atari/Common/Factories/` et modifier leurs espaces de noms et références.
    - [ ] Déplacer, selon la carte validée, les fichiers familiaux de `src/GWGUI.Emulation.Atari/Functions/` vers `src/GWGUI.Emulation.Atari/Common/Functions/` et modifier leurs espaces de noms et références.
    - [ ] Déplacer, selon la carte validée, les fichiers familiaux de `src/GWGUI.Emulation.Atari/Interfaces/` vers `src/GWGUI.Emulation.Atari/Common/Interfaces/` et modifier leurs espaces de noms et références.
    - [ ] Déplacer, selon la carte validée, les fichiers familiaux de `src/GWGUI.Emulation.Atari/Services/` vers `src/GWGUI.Emulation.Atari/Common/Services/` et modifier leurs espaces de noms et références.
  - [ ] 3.2 Uniformiser uniquement les noms généraux
    - [ ] Renommer, selon les correspondances validées dans `docs/reference/emulator-adapter-file-map.md`, les types généraux déplacés sous `src/GWGUI.Emulation.Atari/Common/` pour retirer le préfixe `Atari`, puis modifier leurs constructeurs et toutes leurs références sans renommer les types propres aux machines Atari.
  - [ ] 3.3 Conserver la traduction des commandes par machine
    - [ ] Modifier les catalogues et contrats déplacés sous `src/GWGUI.Emulation.Atari/Common/` pour conserver les commandes de joystick, clavier, souris et trackball dans les définitions de chaque machine et les transmettre à l’adaptateur sélectionné.

- [ ] 4. Isoler chaque émulateur Atari
  - [ ] 4.1 Déplacer les éléments encore propres aux cœurs
    - [ ] Déplacer, selon les chemins validés dans `docs/reference/emulator-adapter-file-map.md`, les constantes, contrats, dictionnaires, énumérations, fonctions, interfaces et services propres à Hatari vers les catégories correspondantes sous `src/GWGUI.Emulation.Atari/Emulators/Hatari/`, puis aligner leurs espaces de noms.
    - [ ] Déplacer de la même manière les fichiers propres à Atari800 sous `src/GWGUI.Emulation.Atari/Emulators/Atari800/`, puis aligner leurs espaces de noms.
    - [ ] Déplacer de la même manière les fichiers propres à Stella sous `src/GWGUI.Emulation.Atari/Emulators/Stella/`, puis aligner leurs espaces de noms.
    - [ ] Déplacer de la même manière les fichiers propres à ProSystem sous `src/GWGUI.Emulation.Atari/Emulators/ProSystem/`, puis aligner leurs espaces de noms.
    - [ ] Déplacer de la même manière les fichiers propres à Beetle Lynx sous `src/GWGUI.Emulation.Atari/Emulators/BeetleLynx/`, puis aligner leurs espaces de noms.
    - [ ] Déplacer de la même manière les fichiers propres à Virtual Jaguar sous `src/GWGUI.Emulation.Atari/Emulators/VirtualJaguar/`, puis aligner leurs espaces de noms.
  - [ ] 4.2 Adapter les noms internes aux émulateurs
    - [ ] Renommer dans chaque `src/GWGUI.Emulation.Atari/Emulators/<émulateur>/` les fichiers et types préfixés `Atari` lorsqu’ils décrivent uniquement l’émulateur concerné, puis modifier leurs références sans changer les identifiants invariants affichés ou persistés.
  - [ ] 4.3 Interdire les types concrets dans Common
    - [ ] Modifier tous les fichiers sous `src/GWGUI.Emulation.Atari/Common/` qui référencent un espace de noms `GWGUI.Emulation.Atari.Emulators` pour remplacer cette dépendance par les interfaces et contrats internes communs.

- [ ] 5. Ranger toute la gestion commune Amiga
  - [ ] 5.1 Déplacer les catégories communes
    - [ ] Déplacer, selon les chemins Amiga validés dans `docs/reference/emulator-adapter-file-map.md`, les fichiers familiaux des dossiers racine `Constants`, `Contracts`, `Dictionaries`, `Enums`, `Factories`, `Functions`, `Interfaces` et `Services` vers les catégories correspondantes sous `src/GWGUI.Emulation.Amiga/Common/`, puis modifier leurs espaces de noms et références.
  - [ ] 5.2 Uniformiser uniquement les noms généraux
    - [ ] Renommer, selon les correspondances validées dans `docs/reference/emulator-adapter-file-map.md`, les types généraux déplacés sous `src/GWGUI.Emulation.Amiga/Common/` pour retirer le préfixe `Amiga`, puis modifier leurs constructeurs et toutes leurs références sans renommer les types propres aux machines Amiga.
  - [ ] 5.3 Conserver la traduction des commandes par machine
    - [ ] Modifier les catalogues et contrats déplacés sous `src/GWGUI.Emulation.Amiga/Common/` pour conserver les commandes de joystick, clavier, souris et trackball dans les définitions de chaque machine et les transmettre à l’adaptateur sélectionné.

- [ ] 6. Isoler complètement PUAE
  - [ ] 6.1 Déplacer tous les éléments PUAE
    - [ ] Déplacer, selon les chemins validés dans `docs/reference/emulator-adapter-file-map.md`, toutes les constantes, contrats, fonctions et services qui connaissent Libretro PUAE, sa DLL, son téléchargement, son protocole hôte ou ses options vers les catégories correspondantes sous `src/GWGUI.Emulation.Amiga/Emulators/PUAE/`, puis aligner leurs espaces de noms.
  - [ ] 6.2 Retirer les appels directs depuis Common et Modules
    - [ ] Modifier `src/GWGUI.Emulation.Amiga/Common/Services/Machine.cs` pour remplacer toute utilisation directe de `AmigaExternalCore` ou d’un autre type PUAE par les résultats fournis par `IEmulatorAdapter`.
    - [ ] Modifier `src/GWGUI.Emulation.Amiga/Modules/AmigaEmulationModule.cs` pour remplacer les appels directs à `AmigaCoreHost`, `AmigaCoreProvider`, `AmigaCoreReleaseService` et `AmigaExternalCore` par le catalogue et les interfaces internes de `Common`.
    - [ ] Modifier `src/GWGUI.Emulation.Amiga/Common/Services/Engine.cs` pour sélectionner uniquement un `IEmulatorAdapter` par son identifiant, sans contenir l’identifiant ou le type PUAE.
  - [ ] 6.3 Adapter les noms internes à PUAE
    - [ ] Renommer sous `src/GWGUI.Emulation.Amiga/Emulators/PUAE/` les fichiers et types préfixés `Amiga` lorsqu’ils décrivent uniquement PUAE, puis modifier leurs références sans changer les identifiants invariants affichés ou persistés.

- [ ] 7. Relier les modules uniquement à Common
  - [ ] 7.1 Corriger le module Atari
    - [ ] Modifier `src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs` pour utiliser seulement les contrats, catalogues et services placés sous `src/GWGUI.Emulation.Atari/Common/`, sans référencer un espace de noms ou un type sous `Emulators/`.
  - [ ] 7.2 Corriger le module Amiga
    - [ ] Modifier `src/GWGUI.Emulation.Amiga/Modules/AmigaEmulationModule.cs` pour utiliser seulement les contrats, catalogues et services placés sous `src/GWGUI.Emulation.Amiga/Common/`, sans référencer un espace de noms ou un type sous `Emulators/`.
  - [ ] 7.3 Mettre à jour les imports globaux
    - [ ] Modifier `src/GWGUI.Emulation.Atari/EmulationGlobalUsings.cs` pour importer les nouveaux espaces de noms de `Common` nécessaires sans importer globalement les implémentations sous `Emulators/`.
    - [ ] Modifier `src/GWGUI.Emulation.Amiga/EmulationGlobalUsings.cs` pour importer les nouveaux espaces de noms de `Common` nécessaires sans importer globalement les implémentations sous `Emulators/`.

- [ ] 8. Préparer la reproduction pour Amstrad
  - [ ] 8.1 Documenter la structure à copier
    - [ ] Modifier `src/GWGUI.Emulation.Amstrad/Common/README.md` avec la liste exacte des fichiers communs Atari et Amiga ayant les mêmes chemins relatifs, noms et membres à reproduire pour Amstrad.
    - [ ] Modifier `src/GWGUI.Emulation.Amstrad/Emulators/README.md` pour préciser qu’un nouvel émulateur fournit ses catégories internes, implémente les prises de `Common` et ne communique jamais directement avec `GWGUI.Emulation`.

- [ ] 9. Normaliser les fins de ligne du dépôt
  - [ ] 9.1 Définir la règle Git
    - [ ] Modifier `.gitattributes` pour déclarer les fichiers texte du dépôt en CRLF sous Windows tout en conservant les formats qui exigent LF, puis garder `*.pdf binary`.
  - [ ] 9.2 Normaliser les fichiers touchés
    - [ ] Modifier les fichiers texte concernés sous `src/GWGUI.Emulation.Atari/`, `src/GWGUI.Emulation.Amiga/`, `src/GWGUI.Emulation/`, `src/GWGUI.App/`, `tests/GWGUI.Tests/` et `docs/` pour appliquer les fins de ligne déclarées sans modifier leur contenu fonctionnel.

- [ ] 10. Vérifier l’architecture et le fonctionnement conservé
  - [ ] 10.1 Vérifier les chemins et les espaces de noms
    - [ ] Modifier `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` pour vérifier que les espaces de noms suivent les dossiers, que les prises communes Atari et Amiga ont les mêmes chemins, noms et membres, et qu’aucun fichier de `Common` ou `Modules` ne référence un espace de noms sous `Emulators/`.
  - [ ] 10.2 Vérifier les adaptateurs enregistrés
    - [ ] Modifier `tests/GWGUI.Tests/Emulation/Atari/AtariEmulatorAdapterTests.cs` pour vérifier que chaque dossier Atari fournit un adaptateur enregistré uniquement par le catalogue commun et compatible avec les mêmes machines qu’avant le rangement.
    - [ ] Modifier `tests/GWGUI.Tests/Emulation/Amiga/AmigaEmulatorAdapterTests.cs` pour vérifier que PUAE fournit toutes les opérations attendues par la prise commune sans être référencé directement par le module ou les services communs.
  - [ ] 10.3 Vérifier la sélection publique inchangée
    - [ ] Modifier `tests/GWGUI.Tests/Emulation/EmulatorManagerTests.cs` pour vérifier que les listes de cœurs, leurs descriptions, leur état d’installation, leur sélection et leur persistance restent identiques après le rangement.
  - [ ] 10.4 Consigner les vérifications finales
    - [ ] Modifier `docs/tasks/emulation/emulation-family-common-organization.md` avec les résultats de la compilation, des tests ciblés, de l’audit des dépendances, de l’audit des espaces de noms et de `git diff --check`, uniquement après leur réussite.
