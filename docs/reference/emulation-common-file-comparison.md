# Comparaison des fichiers Common Amiga et Atari

Ce document recense uniquement les fichiers présents sous `Common`, avec des chemins relatifs à la
racine de `GWGUI.Emulation.Amiga` ou `GWGUI.Emulation.Atari`. Une absence dans une colonne signifie
qu’aucun fichier portant ce chemin n’existe dans l’autre projet. « Problème » désigne un écart de
structure ou de responsabilité à examiner ; une différence propre à une famille n’est pas automatiquement une anomalie.

## Suivi de réalisation

- [x] 1. Inventorier et comparer les fichiers des deux dossiers `Common`
  - [x] 1.1 Construire le tableau exhaustif et décrire les écarts
    - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour ajouter chaque fichier de
      `src/GWGUI.Emulation.Amiga/Common` et `src/GWGUI.Emulation.Atari/Common`, associer les chemins
      identiques et décrire dans la troisième colonne leur rôle ainsi que les différences constatées.
  - [x] 1.2 Vérifier l’exhaustivité du document
    - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour inscrire les nombres de
      fichiers recensés de chaque côté et le résultat du contrôle final.

## Résultat du contrôle

- Emulation.Amiga : 71 fichiers recensés, 0 manquant.
- Emulation.Atari : 160 fichiers recensés, 0 manquant.
- Tableaux : 55 chemins communs, 16 chemins propres à Amiga et 105 chemins propres à Atari.
- Chaque fichier apparaît exactement une fois dans la colonne de son projet ; aucun doublon ni erreur
  d'ordre alphabétique n'a été détecté.
- Harmonisation cartouche : compilations `GWGUI.Emulation.Amiga` et `GWGUI.Emulation.Atari` réussies,
  avec 0 avertissement et 0 erreur pour chacune, puis revalidées après déplacement des capacités.
- Erreurs de cartouche : neuf clés ajoutées aux 30 catalogues Atari ; audit Argos réussi pour 28 cultures
  et compilation Atari réussie avec 0 avertissement et 0 erreur, puis revalidés après correction de
  la formulation SECAM.
- `CommonConstants.cs` supprimé : 29 membres utiles ont été répartis dans six paires de fichiers ciblés
  et dans `MediaConstants.cs`, tandis que `Sha256HexLength`, sans consommateur, a été supprimé ;
  les doublons Atari `EmptyCollectionCount` et `FirstBufferIndex` ont également été supprimés de leurs
  anciennes classes. La limite native Atari de 32 descripteurs de ports reste distincte de la limite
  générique de 4 ports configurables. Compilations Amiga et Atari réussies avec 0 avertissement et
  0 erreur, y compris la recompilation Atari après cet audit final.
- Trois coquilles vides Amiga (`ControllerConstants.cs`, `CoreConstants.cs` et
  `CoreManagementConstants.cs`) ont été supprimées ; recompilation Amiga réussie avec 0 avertissement
  et 0 erreur.
- `CompatibilityConstants.cs` a finalement été supprimé des deux projets : ses nombres de ports ont
  rejoint `ControllerPortConstants.cs`, sa clé de média encore active a rejoint `SettingsTextConstants.cs`
  et ses deux derniers membres alimentaient uniquement un modèle `OptionRule` jamais lu. Ce modèle mort,
  ainsi que `VisibleGroups`, a été retiré sans modifier les réglages effectivement produits. Compilations
  Atari et Amiga réussies avec 0 avertissement et 0 erreur ; Atari a été revalidé après suppression des
  deux anciens messages de validation devenus inaccessibles.
- `ConfigurationMigrationConstants.cs` supprimé : aucune migration n'existait, seulement un refus de
  toute version différente de la version courante. La validation du schéma est maintenant effectuée
  directement après la désérialisation du document, comme dans le flux Amiga. Compilation Atari réussie
  avec 0 avertissement et 0 erreur.
- `ConfigurationOptionConstants.cs` supprimé : sa clé mémoire a rejoint `SettingsConstants.cs` et ses
  clés et valeurs par défaut vidéo/audio ont rejoint directement `VideoAudioSettingsConstants.cs`.
  Compilation Atari réussie avec 0 avertissement et 0 erreur.
- `ConfigurationSummaryFunctionsConstants.cs` aligné sur cinq membres génériques identiques. Les
  séparateurs et libellés propres aux familles ont rejoint `Common/Machines/Common/Constants` ; les états audio
  passent par les ressources traduites du module. Audits Argos réussis pour 28 cultures dans chaque
  module ; builds Amiga et Atari réussis avec 0 avertissement et 0 erreur.
- Liste complète du 26 septembre 2026 traitée : les deux `Common/Constants` contiennent exactement les
  mêmes 15 fichiers et les mêmes membres génériques. Les données de machines ont rejoint
  `Common/Machines/Common/Constants`, les constantes Libretro Atari ont rejoint `Emulators/Libretro/Constants`
  et les messages d'erreur ne sont plus des constantes de texte brut. Builds Amiga et Atari réussis
  avec 0 avertissement et 0 erreur.
- Contrôle d'architecture final : les 5 tests `EmulationArchitectureTests` réussissent. La commande de
  test signale seulement `NU1900` pour l'audit de vulnérabilités NuGet inaccessible dans l'environnement,
  sans erreur de compilation ni échec de test.
- Séparation des machines Atari : `AtariClassic` a été supprimé au profit de `Atari8Bit`, `Atari2600`,
  `Atari5200`, `Atari7800`, `AtariLynx`, `AtariJaguar` et `AtariST`. Les six familles non-ST construisent
  le contrat unique `HardwareModelDefinition`, agrégé par `HardwareModelCatalog`, puis chaque émulateur
  déclaré est résolu en un adaptateur de `Common/Interfaces/IEmulatorAdapter`. Build Atari réussi avec
  0 avertissement et 0 erreur ; les 13 tests `EmulationArchitectureTests` réussissent. L'audit final
  confirme 0 référence `Classic*` dans le code et les tests, 0 contrat familial parallèle, 0 dossier
  vide et 0 entrée manquante ou excédentaire dans l'inventaire.
- Infrastructure commune des machines : Amiga et Atari rangent désormais leurs catégories
  transversales sous `Common/Machines/Common`. Les dossiers directement sous `Machines` sont uniquement
  les familles matérielles et `Common`; les constantes et catalogues propres aux modèles restent dans
  leur famille. Builds Amiga et Atari réussis avec 0 avertissement et 0 erreur, puis 13 tests
  `EmulationArchitectureTests` réussis. L’audit final trouve 0 ancien namespace, 0 dossier vide et
  0 écart dans l’inventaire. Le lecteur Libretro temporaire compile avec 0 erreur et son avertissement
  MSBuild `MSB3539` préexistant.
- Harmonisation des contrats : `Common/Contracts` contient exactement `CoreContracts.cs` et
  `EmulatorContracts.cs` dans les deux modules, avec un contenu identique hors espace de noms. Les
  contrats de machines ont rejoint `Common/Machines/Common/Contracts` et les contrats natifs Atari ont rejoint
  `Emulators/Libretro/Contracts`. Builds Amiga et Atari réussis sans avertissement ni erreur ; les 6 tests
  `EmulationArchitectureTests` réussissent. Le lecteur Libretro temporaire compile avec 0 erreur et son
  avertissement MSBuild `MSB3539` préexistant relatif à `BaseIntermediateOutputPath`.
- Harmonisation des dictionnaires : `Common/Dictionaries` contient uniquement
  `EmulatorCatalog.cs` dans les deux modules, avec un contenu identique hors espace de noms. Les
  catalogues de machines ont rejoint `Common/Machines/Common/Dictionaries` et le catalogue technique Atari
  a rejoint `Emulators/Libretro/Dictionaries/CoreCatalog.cs`. Les builds Amiga et Atari réussissent
  sans avertissement ni erreur et les 7 tests `EmulationArchitectureTests` réussissent ; aucun dossier
  vide et aucune erreur de format de diff ne subsistent.
- Harmonisation des énumérations : aucun type ne reste sous `Common/Enums` dans les deux modules.
  Les enums de configuration ont rejoint `Common/Machines/Common/Enums`, les commandes PUAE ont rejoint
  `Emulators/PUAE/Enums` et le protocole Libretro a rejoint `Emulators/Libretro/Enums`.
  `VideoEnums.cs`, qui était vide, a été supprimé. Builds Amiga et Atari réussis sans avertissement
  ni erreur ; les 8 tests `EmulationArchitectureTests` réussissent. Le lecteur Libretro temporaire
  compile avec 0 erreur et son avertissement MSBuild `MSB3539` préexistant.
- Harmonisation des exceptions, factories, fonctions et interfaces : les dossiers racine
  `Common/Exceptions`, `Common/Factories` et `Common/Functions` sont absents des deux modules, car leur
  contenu appartient soit aux machines, soit à PUAE/Libretro. `Common/Interfaces` contient uniquement
  `IEmulatorAdapter.cs`, strictement identique hors espace de noms. Builds Amiga et Atari réussis sans
  avertissement ni erreur ; les 12 tests `EmulationArchitectureTests` réussissent. Le lecteur Libretro
  temporaire compile avec 0 erreur et son avertissement MSBuild `MSB3539` préexistant.

## Harmonisation des constantes communes

- [x] 2. Rendre `Common/Constants` structurellement identique entre Amiga et Atari
  - [x] 2.1 Classer chaque constante commune par responsabilité
    - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour ajouter l'inventaire membre
      par membre des fichiers `Common/Constants/*.cs`, avec la décision explicite : conserver dans les
      deux projets, ajouter le membre manquant, ou déplacer la constante hors de `Common`.
  - [x] 2.2 Appliquer les alignements et identifier les constantes uniques
    - [x] 2.2.1 Aligner les constantes audio
      - [x] Modifier `src/GWGUI.Emulation.Amiga/Common/Constants/AudioConstants.cs` pour déclarer les
        mêmes invariants de tampon, d'échantillon et de volume que le fichier Atari.
      - [x] Modifier `src/GWGUI.Emulation.Amiga/Common/Contracts/AudioContracts.cs` pour utiliser les
        valeurs de choix déjà détenues par `SettingsDescriptionFunctionsConstants`.
      - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Constants/AudioConstants.cs` et
        `SettingsConstants.cs` pour sortir `VideoAudioSettingsConstants` du fichier audio.
    - [x] 2.2.2 Aligner les dix-sept autres paires de constantes
      - [x] Séparer dans `src/GWGUI.Emulation.Amiga/Common/Constants` et
        `src/GWGUI.Emulation.Atari/Common/Constants` chaque classe de constantes actuellement regroupée,
        afin que chaque fichier porte le nom de son unique classe.
      - [x] Modifier les deux `ConfigurationStoreConstants.cs`, les deux services
        `Common/Services/ConfigurationStore.cs` et les fonctions de persistance Atari afin d'utiliser
        exactement les mêmes noms et valeurs de stockage, sans valeur équivalente codée en dur.
      - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Services/ConfigurationStore.cs` pour supprimer
        le suivi `IsLoading` sans consommateur au lieu d'ajouter artificiellement ce code à Amiga.
      - [x] Modifier les deux `ConfigurationStoreConstants.cs` et leurs consommateurs pour remplacer
        les versions de schéma, séparateurs temporaires, paramètres de verrou et options JSON encore
        écrits en dur par des membres constants identiques entre Amiga et Atari.
      - [x] Modifier `src/GWGUI.Emulation.Amiga/Common/Constants/CartridgeConstants.cs` pour ajouter
        le contrat générique de cartouche, indépendamment de l'absence actuelle d'émulateur Amiga compatible.
      - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Constants/CartridgeConstants.cs` pour ne conserver
        que les constantes génériques identiques au contrat Amiga.
      - [x] Modifier les quatre fichiers `Emulators/Stella/Constants/EmulatorConstants.cs`,
        `Emulators/ProSystem/Constants/EmulatorConstants.cs`,
        `Emulators/BeetleLynx/Constants/EmulatorConstants.cs` et
        `Emulators/VirtualJaguar/Constants/EmulatorConstants.cs` pour attribuer à chaque émulateur ses
        propres extensions de cartouche.
      - [x] Modifier les quatre fichiers `Emulators/Stella/Factories/StellaMachineFactory.cs`,
        `Emulators/ProSystem/Factories/ProSystemMachineFactory.cs`,
        `Emulators/BeetleLynx/Factories/BeetleLynxMachineFactory.cs` et
        `Emulators/VirtualJaguar/Factories/VirtualJaguarMachineFactory.cs` pour relayer ces extensions
        à la couche commune par leur adaptateur existant.
      - [x] Modifier `Emulators/Stella/Factories/StellaMachineFactory.cs` et
        `Emulators/VirtualJaguar/Factories/VirtualJaguarMachineFactory.cs` pour déclarer explicitement
        leur prise en charge de la région de cartouche sans tester leur identité dans `Common`.
      - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Factories/MachineFactory.cs` pour exposer aux
        fonctions communes les capacités de cartouche déclarées par chaque adaptateur concret.
      - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Functions/MediaFunctions.Cartridge.cs`,
        `Common/Functions/StorageFunctions.Devices.cs`, `Common/Services/ExternalCore.Content.cs` et
        `Common/Services/ExternalCore.Media.cs` pour consommer ces capacités sans table d'émulateurs
        dans `Common`.
      - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/Atari800/Constants/Atari800MediaConstants.cs`
        pour séparer les extensions de cartouche ordinateur et Atari 5200 détenues par cet émulateur.
      - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/Atari800/Factories/Atari800MachineFactory.cs`
        et `Common/Factories/MachineFactory.cs` pour relayer les extensions Atari800 selon la machine
        par l'adaptateur existant.
      - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Functions/StorageFunctions.Devices.cs` pour
        supprimer les dernières extensions de cartouche propres à Atari800 codées dans `Common`.
      - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour actualiser la décision,
        la ligne d'inventaire et les totaux après l'ajout du contrat Amiga.
      - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Functions/StorageFunctions.Devices.cs` pour
        typer explicitement la collection vide utilisée lorsqu'un adaptateur ne gère aucune cartouche.
      - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour inscrire la réussite des
        compilations Amiga puis Atari après cette harmonisation.
      - [x] Modifier les deux `Common/Constants/CartridgeConstants.cs` pour déclarer la capacité de
        région par défaut au lieu de laisser le booléen dans la factory commune Atari.
      - [x] Modifier `Emulators/Stella/Constants/EmulatorConstants.cs` et
        `Emulators/VirtualJaguar/Constants/EmulatorConstants.cs` pour y déclarer leur capacité de région.
      - [x] Modifier `Common/Factories/MachineFactory.cs`,
        `Emulators/Stella/Factories/StellaMachineFactory.cs` et
        `Emulators/VirtualJaguar/Factories/VirtualJaguarMachineFactory.cs` pour utiliser ces constantes.
      - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour inscrire la nouvelle
        réussite des compilations Amiga puis Atari après le déplacement de ces booléens.
    - [x] 2.2.3 Remplacer les textes bruts des erreurs de cartouche par les traductions du module
      - [x] Créer `src/GWGUI.Emulation.Atari/Common/Exceptions/CartridgeExceptions.cs` pour résoudre
        les clés `Emulation.Atari.Error.Cartridge.*` dans les ressources du module et construire des
        `EmulationException` explicitement localisées.
      - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Exceptions/EmulationException.cs` pour indiquer
        si son message a déjà été traduit par le module.
      - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Contracts/CoreContracts.cs`,
        `Common/Functions/CoreFunctions.Host.cs` et `Common/Services/ProcessCore.Protocol.cs` pour
        conserver cette indication lors du passage par le processus hôte.
      - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Functions/RuntimeFunctions.cs` pour transmettre
        les erreurs déjà traduites à l'App avec `EmulationErrorService.TranslateLocalized` et conserver
        le code générique pour les autres erreurs.
      - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Functions/MediaFunctions.Cartridge.cs`,
        `Common/Services/ExternalCore.Content.cs`, `Common/Services/ExternalCore.Media.cs` et
        `Emulators/VirtualJaguar/Functions/VirtualJaguarOptionFunctions.cs` pour appeler
        `CartridgeExceptions` aux neuf points d'erreur concernés.
      - [x] Supprimer `src/GWGUI.Emulation.Atari/Common/Constants/CartridgeErrors.cs` après disparition
        de tous ses consommateurs.
      - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/00-Base/Emulation.resx` et
        `Resources/en-US/Emulation.resx` avec les neuf textes anglais de référence, puis modifier tous
        les `Resources/<culture>/Emulation.resx` au moyen de `scripts/tools/translate-resx-argos.py`.
      - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour retirer
        `CartridgeErrors.cs` de l'inventaire, actualiser les totaux et inscrire les validations Argos
        et compilation.
      - [x] Modifier les 30 fichiers `src/GWGUI.Emulation.Atari/Resources/<culture>/Emulation.resx`
        avec `scripts/tools/translate-resx-argos.py --replace` pour reformuler et retraduire la phrase
        SECAM dont la première traduction Argos française était incorrecte.
      - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour inscrire le nouvel audit
        Argos et la nouvelle compilation Atari après cette correction.
    - [x] 2.2.4 Démanteler le fichier fourre-tout Atari `CommonConstants.cs`
      - [x] Créer dans les deux projets `Common/Constants/BufferConstants.cs`,
        `ControllerPortConstants.cs`, `CoreDirectoryConstants.cs`, `ErrorContextConstants.cs`,
        `ExternalCoreInteropConstants.cs`, `HashConstants.cs` et `SavedStateConstants.cs` avec les
        mêmes classes et membres génériques extraits de `CommonConstants.cs`.
      - [x] Modifier les deux `Common/Constants/MediaConstants.cs` pour y ajouter les séparateurs
        génériques de chemin et d'extensions avec les mêmes noms et valeurs.
      - [x] Modifier les consommateurs sous `src/GWGUI.Emulation.Atari/Common/Functions` et
        `Common/Services` pour remplacer chaque référence à `CommonConstants` par la classe de
        responsabilité correspondante.
      - [x] Modifier les consommateurs sous `src/GWGUI.Emulation.Atari/Emulators` pour remplacer
        chaque référence à `CommonConstants` par la classe de responsabilité correspondante.
      - [x] Modifier `src/GWGUI.Emulation.Amiga/Emulators/PUAE/Services/ExternalCore.cs`,
        `ExternalHostCallbacks.Environment.cs`, `ExternalHostCallbacks.AudioVideo.cs`, `CoreHost.cs`
        et `CoreReleaseService.cs` pour utiliser les mêmes constantes génériques déjà codées en dur.
      - [x] Modifier `src/GWGUI.Emulation.Amiga/Emulators/PUAE/Constants/ExternalCoreConstants.cs`
        pour retirer les noms de répertoires désormais détenus par `CoreDirectoryConstants`.
      - [x] Supprimer `src/GWGUI.Emulation.Atari/Common/Constants/CommonConstants.cs` après disparition
        de tous ses consommateurs.
      - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour retirer
        `CommonConstants.cs`, ajouter les sept paires de fichiers, actualiser les totaux et inscrire
        les compilations Amiga puis Atari.
      - [x] Supprimer les deux `Common/Constants/HashConstants.cs` après confirmation que
        `Sha256HexLength` était une constante morte sans consommateur.
      - [x] Modifier `src/GWGUI.Emulation.Amiga/Common/Constants/SavedStateConstants.cs` et
        `Emulators/PUAE/Services/ExternalCore.cs` pour employer la limite Amiga réelle sans modifier
        le comportement existant.
      - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour corriger le nombre de
        paires ajoutées, les totaux et réinscrire les compilations finales Amiga puis Atari.
      - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Constants/CompatibilityConstants.cs` et
        `Common/Functions/MediaFunctions.Compatibility.cs` pour employer
        `BufferConstants.EmptyCollectionCount` sans doublon local.
      - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour consigner que
        `EnvironmentConstants.MaximumControllerPortCount` reste distinct : il borne les descripteurs
        natifs analysés à 32, tandis que `ControllerPortConstants` borne les ports configurables à 4.
      - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Constants/StateConstants.cs` et
        `Common/Functions/StateFunctions.Store.cs`, `StateFunctions.Saved.cs` pour employer
        `BufferConstants.FirstBufferIndex` sans doublon local.
      - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour consigner la suppression
        des deux doublons réels et la distinction sémantique de la limite native à 32 détectée par
        l'audit final.
      - [x] Compiler `GWGUI.Emulation.Atari` et consigner le résultat final après suppression des
        doublons.
    - [x] 2.2.5 Aligner les autres paires de constantes communes
      - [x] Nettoyer les anciens fichiers de constantes devenus vides
        - [x] Supprimer `src/GWGUI.Emulation.Amiga/Common/Constants/ControllerConstants.cs`, devenu
          vide tandis que le fichier Atari ne contient que des membres explicitement uniques à Atari.
        - [x] Supprimer `src/GWGUI.Emulation.Amiga/Common/Constants/CoreConstants.cs`, devenu vide après
          déplacement de ses constantes vers leurs propriétaires.
        - [x] Supprimer `src/GWGUI.Emulation.Amiga/Common/Constants/CoreManagementConstants.cs`, devenu
          vide après déplacement de ses constantes vers leurs propriétaires.
        - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour retirer ces trois
          fichiers vides de l'inventaire et actualiser les totaux.
        - [x] Compiler `GWGUI.Emulation.Amiga` après la suppression des trois fichiers vides et
          consigner le résultat.
      - [x] Sortir les clés de traduction de `CompatibilityConstants.cs`
        - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Constants/SettingsTextConstants.cs` pour
          y déclarer les huit clés de traduction des explications de compatibilité.
        - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Functions/MediaFunctions.Compatibility.cs`
          pour utiliser ces clés depuis leur propriétaire textuel.
        - [x] Modifier les deux `Common/Constants/ControllerPortConstants.cs` pour y déclarer les
          nombres génériques de un, deux et quatre ports avec les mêmes noms et valeurs.
        - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Functions/MediaFunctions.Compatibility.cs`
          pour employer `ControllerPortConstants` pour la validation et les deux ports ST.
        - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Constants/CompatibilityConstants.cs` pour
          retirer les huit clés déplacées, les nombres de ports déplacés et `CoreManagedValue`, ancien
          marqueur remplacé par le contrat typé `OptionAvailability` et sans consommateur.
        - [x] Créer `src/GWGUI.Emulation.Amiga/Common/Constants/CompatibilityConstants.cs` avec les
          deux invariants génériques restants, identiques au fichier Atari.
        - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour consigner ce déplacement.
        - [x] Compiler `GWGUI.Emulation.Atari` et consigner le résultat après ce nettoyage.
        - [x] Compiler `GWGUI.Emulation.Amiga` et consigner le résultat après l'ajout du contrat
          générique de compatibilité.
      - [x] Supprimer le modèle mort de compatibilité des options
        - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Contracts/MediaContracts.cs` pour retirer
          `VisibleGroups` et `Options`, qui ne sont lus par aucun consommateur.
        - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Contracts/MachineContracts.cs` pour supprimer
          le contrat `OptionRule` devenu sans consommateur.
        - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Functions/MediaFunctions.Compatibility.cs`
          pour retirer la construction et la validation des règles d'options mortes.
        - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Enums/SettingsEnums.cs` pour supprimer
          `SettingsGroup`, devenu sans consommateur.
        - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Constants/SettingsTextConstants.cs` pour
          retirer les sept clés d'explication devenues sans consommateur et conserver celle du média
          Jaguar CD encore utilisée.
        - [x] Supprimer les deux `Common/Constants/CompatibilityConstants.cs`, désormais sans usage.
        - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour retirer ces fichiers,
          corriger les totaux et consigner la suppression du modèle mort.
        - [x] Compiler `GWGUI.Emulation.Atari`, puis `GWGUI.Emulation.Amiga`, et consigner les résultats.
        - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Constants/ErrorMessages.cs` pour supprimer
          les deux messages bruts devenus inaccessibles avec `OptionRule`.
        - [x] Compiler `GWGUI.Emulation.Atari` et consigner le résultat final après cette suppression.
      - [x] Supprimer la fausse migration de configuration Atari
        - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Functions/ConfigurationFunctions.Persistence.cs`
          pour valider directement la version du document désérialisé.
        - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Services/ConfigurationStore.cs` pour appeler
          directement la désérialisation du stockage, comme le flux Amiga.
        - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Functions/ConfigurationFunctions.cs` pour
          supprimer `ConfigurationMigrationFunctions` et son import JSON devenu inutile.
        - [x] Supprimer `src/GWGUI.Emulation.Atari/Common/Constants/ConfigurationMigrationConstants.cs`.
        - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour retirer ce fichier de
          l'inventaire et actualiser les totaux.
        - [x] Compiler `GWGUI.Emulation.Atari` et consigner le résultat final.
      - [x] Supprimer le découpage artificiel `ConfigurationOptionConstants.cs`
        - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Constants/SettingsConstants.cs` pour y déplacer
          la clé de mémoire principale.
        - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Constants/VideoAudioSettingsConstants.cs` pour
          y déplacer directement les cinq clés vidéo/audio et les trois valeurs par défaut.
        - [x] Modifier les consommateurs sous `src/GWGUI.Emulation.Atari/Common`, `Emulators` et
          `Modules` pour employer les propriétaires réels de ces constantes.
        - [x] Supprimer `src/GWGUI.Emulation.Atari/Common/Constants/ConfigurationOptionConstants.cs`.
        - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour retirer ce fichier de
          l'inventaire et actualiser les totaux.
        - [x] Compiler `GWGUI.Emulation.Atari` et consigner le résultat final.
      - [x] Aligner `ConfigurationSummaryFunctionsConstants.cs`
        - [x] Modifier les deux `Common/Constants/ConfigurationSummaryFunctionsConstants.cs` pour
          conserver les mêmes constantes génériques avec des noms explicites, supprimer les membres
          inutilisés et nommer clairement les membres propres à chaque famille.
        - [x] Modifier `src/GWGUI.Emulation.Amiga/Common/Constants/StorageConstants.cs` et
          `FirmwareCatalogConstants.cs` pour recevoir les constantes Amiga actuellement mal rangées.
        - [x] Modifier les deux fonctions `Common/Functions/ConfigurationFunctions*.cs` pour employer
          les nouveaux propriétaires et résoudre les textes audio par les ressources du module.
        - [x] Modifier `src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs` pour employer le nom
          explicite de la constante TOS.
        - [x] Modifier les ressources `00-Base` et `en-US` Amiga et Atari pour ajouter les deux textes
          de résumé audio.
        - [x] Modifier toutes les autres cultures Amiga et Atari avec
          `scripts/tools/translate-resx-argos.py`.
        - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour consigner l'alignement.
        - [x] Compiler `GWGUI.Emulation.Amiga`, puis `GWGUI.Emulation.Atari`, et consigner les résultats.
      - [x] Reprendre exhaustivement la liste de divergences fournie
        - [x] Établir les propriétaires réels avant les déplacements
          - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour ajouter, pour chaque
            fichier cité dans la liste fournie, ses classes, ses membres, ses consommateurs et la décision
            concrète : créer le pendant manquant, aligner la paire, déplacer hors de `Common` ou supprimer
            un élément réellement mort.
        - [x] Aligner validation, contenu et contrôleurs
          - [x] Modifier les fichiers `Common/Constants/ConfigurationValidationFunctionsConstants.cs`,
            `ContentConstants.cs`, `ControllerConstants.cs`, `ControllerPortFunctionsConstants.cs` et
            leurs consommateurs afin que tout contrat générique existe sous le même nom des deux côtés.
        - [x] Aligner module et moteur
          - [x] Modifier la paire `EmulationModuleConstants.cs`, conserver la paire déjà alignée
            `ErrorContextConstants.cs`, puis supprimer `EngineConstants.cs` après avoir relié son unique
            consommateur à la constante générique existante.
        - [x] Aligner les constantes de micrologiciel
          - [x] Modifier les paires `FirmwareCatalogConstants.cs` et `FirmwareConstants.cs`, puis modifier
            ou déplacer `FirmwareRuntimeConstants.cs` et `FirmwareScanFunctionsConstants.cs` avec leurs
            consommateurs, afin de séparer les invariants génériques des données de famille ou de cœur.
        - [x] Aligner matériel et entrées
          - [x] Modifier ou déplacer `HardwareSettingsConstants.cs`,
            `HardwareSettingsFunctionsConstants.cs`, `InputSettingsConstants.cs`,
            `InputSettingsFunctionsConstants.cs` et `InputSnapshotFunctionsConstants.cs`, ainsi que
            leurs consommateurs, selon leur propriétaire constaté.
        - [x] Aligner machine, médias, modèles et exécution
          - [x] Modifier les paires `MachineConfigurationConstants.cs`, `MachineConstants.cs`,
            `MediaConstants.cs`, `ModelConstants.cs`, `RuntimeConstants.cs` et `SavedStateConstants.cs`,
            puis modifier ou déplacer `MachineOptionConstants.cs`, `MachineOptionFunctionsConstants.cs`,
            `MachineValues.cs` et `MouseSettingsConstants.cs` avec leurs consommateurs selon leur
            propriétaire constaté.
        - [x] Aligner réglages, raccourcis, états, stockage et vidéo
          - [x] Modifier les paires `SettingsChoiceConstants.cs`, `SettingsConstants.cs`,
            `SettingsTextConstants.cs`, `StateConstants.cs`, `StorageConstants.cs` et `VideoConstants.cs`,
            puis modifier ou déplacer `ShortcutConstants.cs`, `StateStoreConstants.cs` et
            `VideoAudioSettingsConstants.cs` avec leurs consommateurs selon leur propriétaire constaté.
        - [x] Aligner l'hébergement et le cycle de vie des cœurs
          - [x] Modifier ou déplacer les fichiers `CoreHostConstants.cs`, `CoreHostErrors.cs`,
            `CoreHostFunctionsConstants.cs`, `CoreHostValues.cs`, `CoreLifecycleConstants.cs`,
            `CoreOptionConstants.cs`, `CoreOptionProbeConstants.cs`, `CoreOptionProbeValues.cs`,
            `CoreReleaseConstants.cs`, `CoreReleaseErrors.cs`, `DiskControlConstants.cs`,
            `DiskControlErrors.cs`, `EmulatorCatalogConstants.cs`, `EmulatorCatalogErrors.cs`,
            `EnvironmentConstants.cs`, `EnvironmentFunctionsConstants.cs`, `InputConstants.cs`,
            `KeyboardConstants.cs`, `ProcessCoreConstants.cs`, `ScpMediaFunctionsConstants.cs`,
            `SessionMediaConstants.cs`, `SessionMediaErrors.cs` et `ErrorMessages.cs`, ainsi que leurs
            consommateurs, selon leur propriétaire constaté.
        - [x] Actualiser l'inventaire après l'alignement complet
          - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour reconstruire tous les
            tableaux alphabétiques de `Common`, recalculer les totaux et consigner chaque décision finale.
  - [x] 2.3 Vérifier l'identité structurelle et le fonctionnement
    - [x] Modifier `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` pour comparer les types
      et membres déclarés par les deux dossiers `Common/Constants`, sans imposer l'égalité des valeurs
      propres aux familles.
    - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour inscrire les résultats de
      compilation, de tests et du contrôle final des constantes.
- [x] 3. Réorganiser le tableau comparatif de `Common`
  - [x] 3.1 Présenter l'inventaire par dossier et par ordre alphabétique
    - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour remplacer les trois tableaux
      globaux par un tableau pour chaque dossier relatif sous `Common`, avec les fichiers Amiga et Atari
      alignés alphabétiquement par nom et les absences visibles dans la colonne correspondante.
  - [x] 3.2 Vérifier le nouvel inventaire
    - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour actualiser les totaux et
      confirmer que chaque fichier actuel de `Common` apparaît exactement une fois dans les tableaux.

### Décisions pour `Common/Constants`

Une constante propre à une famille peut rester dans ce dossier lorsqu'elle représente une donnée déjà
exposée à App ou partagée par plusieurs émulateurs de la famille. Elle doit alors porter un commentaire
explicite `Unique to GWGUI.Emulation.Amiga` ou `Unique to GWGUI.Emulation.Atari`. Seules les données
réellement propres à un cœur ou à son protocole doivent être déplacées sous `Emulators`.

| Fichier | Constat membre par membre | Décision |
|---|---|---|
| `AudioConstants.cs` | Aucun membre commun ; Atari ajoute aussi `VideoAudioSettingsConstants`. | Conserver des bornes audio génériques identiques ; déplacer les valeurs de filtre PUAE et les options audio/vidéo propres aux machines vers leurs propriétaires. |
| `CartridgeConstants.cs` | Même contrat de région dans les deux familles ; seule la valeur de la clé interne porte l'identifiant familial. | Conserver ce contrat générique dans les deux `Common` ; déclarer les capacités et extensions dans chaque adaptateur d'émulateur concret. |
| `ConfigurationConstants.cs` | Seuls cinq membres de résumé sont communs ; classes de validation Amiga et de migration/options Atari différentes. | Aligner stockage et résumé génériques ; déplacer validation, migration et options propres aux configurations concernées. |
| `ControllerConstants.cs` | Le fichier Amiga vide a été supprimé ; le fichier Atari ne contient que des tables et bornes explicitement uniques à Atari. | Conserver uniquement le fichier Atari tant que ces données familiales sont partagées par plusieurs de ses émulateurs. |
| `CoreConstants.cs` | Le fichier Amiga était vide et aucun fichier Atari n'existait. | Fichier supprimé ; les constantes concrètes restent chez leurs propriétaires. |
| `CoreManagementConstants.cs` | Le fichier Amiga était vide et aucun fichier Atari n'existait. | Fichier supprimé ; les constantes concrètes restent chez leurs propriétaires. |
| `EmulationModuleConstants.cs` | Deux membres communs ; identifiants familiaux nommés différemment et sept membres Amiga supplémentaires. | Renommer les concepts communs de façon identique ; déplacer les valeurs sans rapport avec le module. |
| `FirmwareConstants.cs` | Seul `DirectoryName` est commun ; catalogues et scan divergent. | Conserver les primitives génériques ; déplacer empreintes, noms de ROM et règles de scan sous les machines ou émulateurs propriétaires. |
| `InputConstants.cs` | Sous-ensembles communs, mais commandes et ressources familiales mélangées. | Conserver les identifiants d'entrée universels ; déplacer les mappings propres aux machines. |
| `MachineConstants.cs` | Aucun membre commun ; Atari ajoute six classes annexes. | Conserver les bornes et états génériques avec les mêmes noms ; déplacer options, matériel et messages familiaux. |
| `MediaConstants.cs` | Amiga vide ; Atari ajoute huit classes et `DefaultMountOrder`. | Ajouter la constante générique réellement commune ; déplacer cartouche, SCP, session et disque vers leurs propriétaires. |
| `ModelConstants.cs` | Classes différentes et exclusivement familiales. | Retirer le fichier générique et déplacer les données sous `Common/Machines/...`. |
| `RuntimeConstants.cs` | Classes entièrement différentes. | Déplacer protocole/processus sous les émulateurs et les messages vers les exceptions traduites ; ne conserver que les invariants d'exécution communs. |
| `SettingsChoiceConstants.cs` | Quatre membres communs, avec de nombreux choix Amiga et six choix Atari exclusifs. | Conserver les choix universels ; déplacer les choix de machines/émulateurs vers leurs catalogues de réglages. |
| `SettingsConstants.cs` | Un seul membre commun. | Conserver seulement les clés génériques avec des noms communs ; déplacer les clés de matériel et d'émulateur. |
| `SettingsTextConstants.cs` | Trente-et-un membres communs, mais de nombreux libellés familiaux de chaque côté. | Conserver les libellés génériques ; déplacer les libellés propres aux machines dans leurs fonctions de réglages. |
| `StateConstants.cs` | Aucun membre commun ; Atari ajoute une classe complète de format. | Aligner format et stockage génériques ; remplacer les messages bruts par le service d'erreur traduit et déplacer les identifiants familiaux. |
| `StorageConstants.cs` | Sept membres communs ; extensions, formats et bus familiaux divergents. | Conserver les primitives génériques ; déplacer formats/extensions sous les machines ou adaptateurs qui les acceptent. |
| `VideoConstants.cs` | Amiga vide ; quatre constantes génériques Atari. | Ajouter les quatre invariants génériques côté Amiga avec les mêmes noms et valeurs. |

### Relevé exhaustif de la liste fournie

Le propriétaire ci-dessous est déterminé à partir des consommateurs actuels. « Paire » signifie que le
fichier doit exister sous le même chemin et porter les mêmes concepts des deux côtés ; les valeurs peuvent
différer lorsqu'elles décrivent réellement la famille. « Émulateur » signifie que le fichier ne doit plus
se trouver dans le `Common` familial.

| Fichier cité | Consommateurs constatés | Décision concrète |
|---|---|---|
| `ConfigurationValidationFunctionsConstants.cs` | Validation Amiga uniquement. | Déplacer l'en-tête Kickstart dans les constantes de micrologiciel Amiga et supprimer ce fichier général sans pendant Atari. |
| `ContentConstants.cs` | Construction des extensions Atari. | Déplacer le séparateur générique dans la paire `MediaConstants.cs` et supprimer ce fichier isolé. |
| `ControllerConstants.cs` | Contrat et fonctions de contrôleur Atari. | Créer la paire avec les bornes génériques ; déplacer les listes d'actions propres aux machines Atari dans leurs catalogues. |
| `ControllerPortFunctionsConstants.cs` | Résolution de périphériques Atari. | Conserver seulement les concepts de port génériques dans une paire ; déplacer Booster Grip, Genesis et Joy 2B+ vers les définitions Atari. |
| `CoreHostConstants.cs`, `CoreHostErrors.cs`, `CoreHostFunctionsConstants.cs`, `CoreHostValues.cs` | Hôte de processus Atari ; pendant partiel sous `Emulators/PUAE`. | Sortir le protocole concret du `Common` familial et le ranger avec l'implémentation d'émulateur qui l'emploie ; traduire les erreurs exposées. |
| `CoreLifecycleConstants.cs` | Cycle de vie Libretro Atari. | Déplacer vers l'implémentation Libretro des émulateurs Atari. |
| `CoreOptionConstants.cs`, `CoreOptionProbeConstants.cs`, `CoreOptionProbeValues.cs` | ABI et sonde d'options Libretro Atari. | Déplacer vers l'implémentation Libretro des émulateurs Atari. |
| `CoreReleaseConstants.cs`, `CoreReleaseErrors.cs` | Téléchargement des cœurs Atari ; pendant partiel PUAE. | Garder dans `Common` uniquement les primitives de gestion d'émulateurs réellement communes et déplacer les formats/protocoles propres au fournisseur ; relayer les erreurs par le service traduit. |
| `DiskControlConstants.cs`, `DiskControlErrors.cs` | API disque Libretro Atari. | Déplacer vers l'implémentation Libretro ; relayer les erreurs par le service traduit. |
| `EmulationModuleConstants.cs` | Modules et services des deux familles. | Aligner la paire sur les identifiants de module et de ressources ; déplacer les valeurs Amiga sans rapport avec le module. |
| `EmulatorCatalogConstants.cs`, `EmulatorCatalogErrors.cs` | Catalogue de cœurs Atari. | Aligner avec la gestion PUAE côté Amiga pour les notions génériques ; conserver les sources propres à chaque émulateur dans son dossier et traduire les erreurs. |
| `EngineConstants.cs` | Identifiant de machine Atari. | Créer le même invariant générique côté Amiga et l'utiliser dans les deux moteurs. |
| `EnvironmentConstants.cs`, `EnvironmentFunctionsConstants.cs` | ABI d'environnement Libretro Atari. | Déplacer vers l'implémentation Libretro des émulateurs Atari. |
| `ErrorMessages.cs` | Validations Atari, machines et Hatari. | Supprimer les textes bruts ; employer des codes du service d'erreur et les ressources traduites au propriétaire réel. |
| `FirmwareCatalogConstants.cs` | Catalogues familiaux des deux côtés. | Conserver une paire pour les primitives de catalogue et déplacer empreintes, modèles et libellés sous les familles de machines concernées. |
| `FirmwareConstants.cs` | Stockage et identification des micrologiciels. | Aligner les primitives de chemin, extension, empreinte et taille ; déplacer les ROM Atari concrètes et leurs empreintes sous leurs familles. |
| `FirmwareRuntimeConstants.cs`, `FirmwareScanFunctionsConstants.cs` | Résolution et scan Atari. | Fusionner les invariants génériques dans la paire de micrologiciel ; déplacer les noms EmuTOS/KAOSTOS vers la famille Atari concernée. |
| `HardwareSettingsConstants.cs`, `HardwareSettingsFunctionsConstants.cs` | Présentation de matériel Atari. | Déplacer les unités génériques vers les constantes de réglages appariées et les cultures/choix Atari sous les familles concernées. |
| `InputConstants.cs` | ABI d'entrée Libretro Atari. | Déplacer vers l'implémentation Libretro ; ne garder dans la paire familiale que les concepts d'entrée exposés à l'interface commune. |
| `InputSettingsConstants.cs` | Touches propres aux machines Atari. | Déplacer sous les familles Atari concernées. |
| `InputSettingsFunctionsConstants.cs` | Libellés et identifiants de commandes des deux familles. | Aligner les concepts génériques de la paire et déplacer les commandes propres aux machines dans leurs catalogues. |
| `InputSnapshotFunctionsConstants.cs` | Traduction des instantanés des deux familles. | Aligner clavier/souris et rôles universels ; laisser les boutons propres aux machines dans leurs catalogues. |
| `KeyboardConstants.cs` | Codes clavier Libretro Atari. | Déplacer vers l'implémentation Libretro des émulateurs Atari. |
| `MachineConfigurationConstants.cs` | Valeurs par défaut des deux familles. | Conserver une paire pour l'identifiant de module ; déplacer modèle, région et état audio par défaut vers les définitions familiales. |
| `MachineConstants.cs` | Boucle machine Atari et erreurs machine Amiga. | Aligner uniquement les invariants génériques de cycle de vie ; remplacer les textes bruts par les erreurs traduites et déplacer les limites propres au moteur. |
| `MachineOptionConstants.cs`, `MachineOptionFunctionsConstants.cs`, `MachineValues.cs` | Options Atari et Hatari. | Déplacer sous les familles Atari ou `Emulators/Hatari` selon chaque consommateur. |
| `MediaConstants.cs` | Primitives média des deux familles. | Aligner la paire, y compris le séparateur de liste et l'ordre de montage génériques. |
| `ModelConstants.cs` | Modèles familiaux des deux côtés. | Déplacer les valeurs de modèles sous `Common/Machines/<famille>` et conserver une paire générale uniquement si un invariant commun subsiste. |
| `MouseSettingsConstants.cs` | Réglages de souris Atari. | Répartir entre les constantes génériques de réglages appariées et les familles Atari consommatrices. |
| `ProcessCoreConstants.cs` | Processus hôte Atari ; pendant sous PUAE. | Déplacer avec les implémentations de processus des émulateurs. |
| `RuntimeConstants.cs` | Région Atari ; marqueur SCP Amiga. | Déplacer région et SCP vers leurs propriétaires ; conserver une paire seulement pour les invariants d'exécution réellement communs. |
| `SavedStateConstants.cs` | Limite d'état des deux familles. | Conserver la paire et le même membre ; les valeurs peuvent différer selon le moteur. |
| `ScpMediaFunctionsConstants.cs` | Message brut SCP Atari. | Supprimer le texte brut et employer le service d'erreur traduit du média. |
| `SessionMediaConstants.cs`, `SessionMediaErrors.cs` | Sessions multi-média Atari/Libretro. | Déplacer avec l'implémentation d'émulateur ; traduire les erreurs exposées. |
| `SettingsChoiceConstants.cs` | Choix de réglages des deux familles. | Aligner les choix universels et déplacer les choix de machines/émulateurs vers leurs propriétaires. |
| `SettingsConstants.cs` | Clés de réglages des deux familles. | Aligner les clés génériques ; déplacer les clés PUAE et celles des machines Atari vers leurs propriétaires. |
| `SettingsTextConstants.cs` | Ressources de réglages des deux familles. | Aligner les clés de ressources génériques et déplacer les clés propres aux machines vers leurs fonctions de réglages. |
| `ShortcutConstants.cs` | Raccourcis média Atari. | Créer la paire pour les actions réellement communes ou déplacer les raccourcis propres à l'adaptateur Atari. |
| `StateConstants.cs`, `StateStoreConstants.cs` | Format et stockage d'état des deux familles, actuellement regroupés côté Amiga. | Séparer les deux classes dans les deux mêmes fichiers, aligner les invariants et conserver seulement les valeurs familiales nécessaires. |
| `StorageConstants.cs` | Présentation des supports des deux familles. | Aligner les notions génériques et déplacer formats, bus et clés propres aux machines vers leurs propriétaires. |
| `VideoAudioSettingsConstants.cs` | Réglages génériques Atari dispersés côté Amiga. | Créer la paire et y réunir les bornes et choix vidéo/audio communs ; laisser les clés d'émulateur dans chaque adaptateur. |
| `VideoConstants.cs` | Tampons vidéo des deux familles. | Paire déjà alignée ; aucune modification fonctionnelle requise. |

## Inventaire alphabétique par dossier

### `Common/Constants`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| `Common/Constants/AudioConstants.cs` | `Common/Constants/AudioConstants.cs` | Même chemin, même classe et mêmes membres génériques ; les valeurs familiales peuvent différer. |
| `Common/Constants/BufferConstants.cs` | `Common/Constants/BufferConstants.cs` | Même chemin, même classe et mêmes membres génériques ; les valeurs familiales peuvent différer. |
| `Common/Constants/CartridgeConstants.cs` | `Common/Constants/CartridgeConstants.cs` | Même chemin, même classe et mêmes membres génériques ; les valeurs familiales peuvent différer. |
| `Common/Constants/ConfigurationStoreConstants.cs` | `Common/Constants/ConfigurationStoreConstants.cs` | Même chemin, même classe et mêmes membres génériques ; les valeurs familiales peuvent différer. |
| `Common/Constants/ConfigurationSummaryFunctionsConstants.cs` | `Common/Constants/ConfigurationSummaryFunctionsConstants.cs` | Même chemin, même classe et mêmes membres génériques ; les valeurs familiales peuvent différer. |
| `Common/Constants/ControllerConstants.cs` | `Common/Constants/ControllerConstants.cs` | Même chemin, même classe et mêmes membres génériques ; les valeurs familiales peuvent différer. |
| `Common/Constants/ControllerPortConstants.cs` | `Common/Constants/ControllerPortConstants.cs` | Même chemin, même classe et mêmes membres génériques ; les valeurs familiales peuvent différer. |
| `Common/Constants/ControllerPortFunctionsConstants.cs` | `Common/Constants/ControllerPortFunctionsConstants.cs` | Même chemin, même classe et mêmes membres génériques ; les valeurs familiales peuvent différer. |
| `Common/Constants/CoreDirectoryConstants.cs` | `Common/Constants/CoreDirectoryConstants.cs` | Même chemin, même classe et mêmes membres génériques ; les valeurs familiales peuvent différer. |
| `Common/Constants/EmulationModuleConstants.cs` | `Common/Constants/EmulationModuleConstants.cs` | Même chemin, même classe et mêmes membres génériques ; les valeurs familiales peuvent différer. |
| `Common/Constants/ErrorContextConstants.cs` | `Common/Constants/ErrorContextConstants.cs` | Même chemin, même classe et mêmes membres génériques ; les valeurs familiales peuvent différer. |
| `Common/Constants/ExternalCoreInteropConstants.cs` | `Common/Constants/ExternalCoreInteropConstants.cs` | Même chemin, même classe et mêmes membres génériques ; les valeurs familiales peuvent différer. |
| `Common/Constants/MediaConstants.cs` | `Common/Constants/MediaConstants.cs` | Même chemin, même classe et mêmes membres génériques ; les valeurs familiales peuvent différer. |
| `Common/Constants/SavedStateConstants.cs` | `Common/Constants/SavedStateConstants.cs` | Même chemin, même classe et mêmes membres génériques ; les valeurs familiales peuvent différer. |
| `Common/Constants/VideoConstants.cs` | `Common/Constants/VideoConstants.cs` | Même chemin, même classe et mêmes membres génériques ; les valeurs familiales peuvent différer. |

### `Common/Contracts`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| `Common/Contracts/CoreContracts.cs` | `Common/Contracts/CoreContracts.cs` | Fichiers identiques hors espace de noms : options génériques exposées par un cœur d’émulation. |
| `Common/Contracts/EmulatorContracts.cs` | `Common/Contracts/EmulatorContracts.cs` | Fichiers identiques hors espace de noms : contextes génériques de création et de gestion d’un émulateur. |

### `Common/Dictionaries`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| `Common/Dictionaries/EmulatorCatalog.cs` | `Common/Dictionaries/EmulatorCatalog.cs` | Fichiers identiques hors espace de noms : découverte générique des adaptateurs par `IEmulatorAdapter`. |

### `Common/Interfaces`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| `Common/Interfaces/IEmulatorAdapter.cs` | `Common/Interfaces/IEmulatorAdapter.cs` | Fichiers strictement identiques : unique contrat générique reliant la famille à ses adaptateurs. |
### `Common/Machines/AmigaCD32/Constants`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| `Common/Machines/AmigaCD32/Constants/ModelConstants.cs` | — | Spécifique à la famille Amiga ; aucun fichier générique correspondant côté Atari. |

### `Common/Machines/AmigaCD32/Dictionaries`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| `Common/Machines/AmigaCD32/Dictionaries/ModelCatalog.cs` | — | Spécifique à la famille Amiga ; aucun fichier générique correspondant côté Atari. |

### `Common/Machines/AmigaCDTV/Constants`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| `Common/Machines/AmigaCDTV/Constants/ModelConstants.cs` | — | Spécifique à la famille Amiga ; aucun fichier générique correspondant côté Atari. |

### `Common/Machines/AmigaCDTV/Dictionaries`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| `Common/Machines/AmigaCDTV/Dictionaries/ModelCatalog.cs` | — | Spécifique à la famille Amiga ; aucun fichier générique correspondant côté Atari. |

### `Common/Machines/AmigaComputers/Constants`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| `Common/Machines/AmigaComputers/Constants/ModelConstants.cs` | — | Spécifique à la famille Amiga ; aucun fichier générique correspondant côté Atari. |

### `Common/Machines/AmigaComputers/Dictionaries`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| `Common/Machines/AmigaComputers/Dictionaries/ModelCatalog.cs` | — | Spécifique à la famille Amiga ; aucun fichier générique correspondant côté Atari. |

### `Common/Machines/Atari2600/Constants`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| — | `Common/Machines/Atari2600/Constants/ModelConstants.cs` | Identifiant, ressource, fréquence, mémoire et ports propres à l’Atari 2600. |

### `Common/Machines/Atari2600/Dictionaries`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| — | `Common/Machines/Atari2600/Dictionaries/ModelCatalog.cs` | Construit la définition Atari 2600 avec le contrat commun `HardwareModelDefinition`. |

### `Common/Machines/Atari5200/Constants`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| — | `Common/Machines/Atari5200/Constants/ModelConstants.cs` | Identifiant, ressource, fréquence, mémoire et ports propres à l’Atari 5200. |

### `Common/Machines/Atari5200/Dictionaries`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| — | `Common/Machines/Atari5200/Dictionaries/ModelCatalog.cs` | Construit la définition Atari 5200 avec le contrat commun `HardwareModelDefinition`. |

### `Common/Machines/Atari7800/Constants`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| — | `Common/Machines/Atari7800/Constants/ModelConstants.cs` | Identifiant, ressource, fréquence, mémoire et ports propres à l’Atari 7800. |

### `Common/Machines/Atari7800/Dictionaries`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| — | `Common/Machines/Atari7800/Dictionaries/ModelCatalog.cs` | Construit la définition Atari 7800 avec le contrat commun `HardwareModelDefinition`. |

### `Common/Machines/Atari8Bit/Constants`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| — | `Common/Machines/Atari8Bit/Constants/EightBitSettingsCatalogConstants.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Machines/Atari8Bit/Constants/EightBitSettingsConstants.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Machines/Atari8Bit/Constants/EightBitSettingsFunctionsConstants.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Machines/Atari8Bit/Constants/ModelConstants.cs` | Identifiants, ressources et caractéristiques propres aux Atari 400, 800, 800 XL, 130 XE, XL/XE et XEGS. |

### `Common/Machines/Atari8Bit/Contracts`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| — | `Common/Machines/Atari8Bit/Contracts/SettingsContracts.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |

### `Common/Machines/Atari8Bit/Dictionaries`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| — | `Common/Machines/Atari8Bit/Dictionaries/ModelCatalog.cs` | Construit uniquement les définitions 8 bits avec le contrat commun `HardwareModelDefinition`. |
| — | `Common/Machines/Atari8Bit/Dictionaries/SettingsCatalog.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |

### `Common/Machines/Atari8Bit/Enums`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| — | `Common/Machines/Atari8Bit/Enums/SettingsEnums.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |

### `Common/Machines/Atari8Bit/Functions`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| — | `Common/Machines/Atari8Bit/Functions/SettingsFunctions.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |

### `Common/Machines/AtariJaguar/Constants`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| — | `Common/Machines/AtariJaguar/Constants/ModelConstants.cs` | Identifiants, ressources et caractéristiques propres aux Jaguar et Jaguar CD. |

### `Common/Machines/AtariJaguar/Dictionaries`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| — | `Common/Machines/AtariJaguar/Dictionaries/ModelCatalog.cs` | Construit les définitions Jaguar et Jaguar CD avec le contrat commun `HardwareModelDefinition`. |

### `Common/Machines/AtariLynx/Constants`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| — | `Common/Machines/AtariLynx/Constants/ModelConstants.cs` | Identifiant, ressource et caractéristiques propres à l’Atari Lynx. |

### `Common/Machines/AtariLynx/Dictionaries`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| — | `Common/Machines/AtariLynx/Dictionaries/ModelCatalog.cs` | Construit la définition Lynx avec le contrat commun `HardwareModelDefinition`. |

### `Common/Machines/AtariST/Constants`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| — | `Common/Machines/AtariST/Constants/ModelConstants.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Machines/AtariST/Constants/TosConstants.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |

### `Common/Machines/AtariST/Contracts`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| — | `Common/Machines/AtariST/Contracts/ModelContracts.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |

### `Common/Machines/AtariST/Dictionaries`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| — | `Common/Machines/AtariST/Dictionaries/ModelCatalog.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |

### `Common/Machines/AtariST/Enums`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| — | `Common/Machines/AtariST/Enums/ModelEnums.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |

### `Common/Machines/AtariST/Functions`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| — | `Common/Machines/AtariST/Functions/ModelFunctions.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Machines/AtariST/Functions/TosFunctions.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |

### `Common/Machines/Common/Constants`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| — | `Common/Machines/Common/Constants/ControllerActionConstants.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Machines/Common/Constants/ControllerDeviceConstants.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| `Common/Machines/Common/Constants/FirmwareCatalogConstants.cs` | `Common/Machines/Common/Constants/FirmwareCatalogConstants.cs` | Même chemin dans les deux modules ; l’implémentation reste adaptée à la famille. |
| `Common/Machines/Common/Constants/FirmwareConstants.cs` | `Common/Machines/Common/Constants/FirmwareConstants.cs` | Même chemin dans les deux modules ; l’implémentation reste adaptée à la famille. |
| — | `Common/Machines/Common/Constants/FirmwareRuntimeConstants.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Machines/Common/Constants/FirmwareScanFunctionsConstants.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Machines/Common/Constants/HardwareSettingsConstants.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Machines/Common/Constants/HardwareSettingsFunctionsConstants.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Machines/Common/Constants/InputSettingsConstants.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| `Common/Machines/Common/Constants/InputSettingsFunctionsConstants.cs` | `Common/Machines/Common/Constants/InputSettingsFunctionsConstants.cs` | Même chemin dans les deux modules ; l’implémentation reste adaptée à la famille. |
| `Common/Machines/Common/Constants/InputSnapshotFunctionsConstants.cs` | `Common/Machines/Common/Constants/InputSnapshotFunctionsConstants.cs` | Même chemin dans les deux modules ; l’implémentation reste adaptée à la famille. |
| `Common/Machines/Common/Constants/MachineConfigurationConstants.cs` | `Common/Machines/Common/Constants/MachineConfigurationConstants.cs` | Même chemin dans les deux modules ; l’implémentation reste adaptée à la famille. |
| `Common/Machines/Common/Constants/MachineConstants.cs` | `Common/Machines/Common/Constants/MachineConstants.cs` | Même chemin dans les deux modules ; l’implémentation reste adaptée à la famille. |
| — | `Common/Machines/Common/Constants/MachineOptionConstants.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Machines/Common/Constants/MachineOptionFunctionsConstants.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Machines/Common/Constants/MachineValues.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| `Common/Machines/Common/Constants/ModelConstants.cs` | — | Constantes propres aux modèles Amiga ; côté Atari, chaque famille possède désormais ses propres constantes. |
| — | `Common/Machines/Common/Constants/MouseSettingsConstants.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| `Common/Machines/Common/Constants/RuntimeConstants.cs` | `Common/Machines/Common/Constants/RuntimeConstants.cs` | Même chemin dans les deux modules ; l’implémentation reste adaptée à la famille. |
| `Common/Machines/Common/Constants/SettingsConstants.cs` | `Common/Machines/Common/Constants/SettingsConstants.cs` | Même chemin dans les deux modules ; l’implémentation reste adaptée à la famille. |
| `Common/Machines/Common/Constants/SettingsDescriptionChoicesConstants.cs` | `Common/Machines/Common/Constants/SettingsDescriptionChoicesConstants.cs` | Même chemin dans les deux modules ; l’implémentation reste adaptée à la famille. |
| `Common/Machines/Common/Constants/SettingsDescriptionTextConstants.cs` | `Common/Machines/Common/Constants/SettingsDescriptionTextConstants.cs` | Même chemin dans les deux modules ; l’implémentation reste adaptée à la famille. |
| — | `Common/Machines/Common/Constants/ShortcutConstants.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Machines/Common/Constants/StateConstants.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| `Common/Machines/Common/Constants/StateStoreConstants.cs` | `Common/Machines/Common/Constants/StateStoreConstants.cs` | Même chemin dans les deux modules ; l’implémentation reste adaptée à la famille. |
| `Common/Machines/Common/Constants/StorageConstants.cs` | `Common/Machines/Common/Constants/StorageConstants.cs` | Même chemin dans les deux modules ; l’implémentation reste adaptée à la famille. |
| — | `Common/Machines/Common/Constants/VideoAudioSettingsConstants.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |

### `Common/Machines/Common/Contracts`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| `Common/Machines/Common/Contracts/AudioContracts.cs` | — | Configuration audio propre aux machines Amiga actuelles. |
| `Common/Machines/Common/Contracts/ConfigurationContracts.cs` | `Common/Machines/Common/Contracts/ConfigurationContracts.cs` | Même responsabilité de configuration de machine ; schémas adaptés à chaque famille. |
| `Common/Machines/Common/Contracts/ControllerContracts.cs` | `Common/Machines/Common/Contracts/ControllerContracts.cs` | Même responsabilité de configuration des contrôleurs ; périphériques adaptés à chaque famille. |
| `Common/Machines/Common/Contracts/FirmwareContracts.cs` | `Common/Machines/Common/Contracts/FirmwareContracts.cs` | Même responsabilité de description du micrologiciel ; catalogues adaptés à chaque famille. |
| — | `Common/Machines/Common/Contracts/HardwareModelContracts.cs` | Contrat matériel unique reliant les constantes et catalogues des familles Atari aux fonctions communes. |
| `Common/Machines/Common/Contracts/InputContracts.cs` | `Common/Machines/Common/Contracts/InputContracts.cs` | Même responsabilité de configuration des entrées ; options adaptées à chaque famille. |
| — | `Common/Machines/Common/Contracts/MachineContracts.cs` | État d’exécution détaillé actuellement propre aux machines Atari. |
| `Common/Machines/Common/Contracts/MediaContracts.cs` | `Common/Machines/Common/Contracts/MediaContracts.cs` | Même responsabilité de configuration des médias ; capacités adaptées à chaque famille. |
| `Common/Machines/Common/Contracts/ModelContracts.cs` | — | Description détaillée des modèles actuellement propre aux machines Amiga. |
| — | `Common/Machines/Common/Contracts/RuntimeContracts.cs` | Règles de raccourcis actuellement propres aux machines Atari. |
| `Common/Machines/Common/Contracts/StateContracts.cs` | `Common/Machines/Common/Contracts/StateContracts.cs` | Même responsabilité de sauvegarde d’état ; formats adaptés à chaque famille. |

### `Common/Machines/Common/Dictionaries`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| — | `Common/Machines/Common/Dictionaries/CompatibilityCatalog.cs` | Matrice de compatibilité propre aux modèles Atari. |
| `Common/Machines/Common/Dictionaries/ControllerCatalog.cs` | `Common/Machines/Common/Dictionaries/ControllerCatalog.cs` | Même responsabilité de catalogue de contrôleurs ; données adaptées à chaque famille. |
| `Common/Machines/Common/Dictionaries/FirmwareCatalog.cs` | `Common/Machines/Common/Dictionaries/FirmwareCatalog.cs` | Même responsabilité de catalogue de micrologiciels ; données adaptées à chaque famille. |
| — | `Common/Machines/Common/Dictionaries/HardwareModelCatalog.cs` | Agrège sans duplication les catalogues Atari 8 bits, 2600, 5200, 7800, Lynx et Jaguar. |
| `Common/Machines/Common/Dictionaries/MachineCatalog.cs` | `Common/Machines/Common/Dictionaries/MachineCatalog.cs` | Même responsabilité de liste des machines ; données adaptées à chaque famille. |
| `Common/Machines/Common/Dictionaries/ModelCatalog.cs` | `Common/Machines/Common/Dictionaries/ModelCatalog.cs` | Même responsabilité de catalogue des modèles ; données adaptées à chaque famille. |

### `Common/Machines/Common/Enums`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| `Common/Machines/Common/Enums/ControllerEnums.cs` | `Common/Machines/Common/Enums/ControllerEnums.cs` | Même responsabilité de types de contrôleurs ; valeurs adaptées à chaque famille. |
| `Common/Machines/Common/Enums/CoreEnums.cs` | `Common/Machines/Common/Enums/CoreEnums.cs` | Même responsabilité de sélection de l’émulateur ; valeurs adaptées à chaque famille. |
| `Common/Machines/Common/Enums/FirmwareEnums.cs` | `Common/Machines/Common/Enums/FirmwareEnums.cs` | Même responsabilité de catégories de micrologiciels ; valeurs adaptées à chaque famille. |
| — | `Common/Machines/Common/Enums/HardwareModelEnums.cs` | Vocabulaire matériel partagé par les contrats et catalogues des familles Atari non-ST. |
| — | `Common/Machines/Common/Enums/InputEnums.cs` | Options de configuration d’entrée propres aux machines Atari. |
| — | `Common/Machines/Common/Enums/MachineEnums.cs` | Familles et modèles propres à Atari. |
| `Common/Machines/Common/Enums/MediaEnums.cs` | `Common/Machines/Common/Enums/MediaEnums.cs` | Même responsabilité de catégories de médias ; valeurs adaptées à chaque famille. |
| — | `Common/Machines/Common/Enums/RuntimeEnums.cs` | Région et disponibilité des raccourcis propres à l’exécution Atari. |
| — | `Common/Machines/Common/Enums/SettingsEnums.cs` | Disponibilité des options et onglets propres aux réglages Atari. |
| — | `Common/Machines/Common/Enums/StateEnums.cs` | Catégories d’états sauvegardés propres à Atari. |
| — | `Common/Machines/Common/Enums/StorageEnums.cs` | Bus de stockage propres aux machines Atari. |

### `Common/Machines/Common/Exceptions`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| `Common/Machines/Common/Exceptions/MachineExceptions.cs` | — | Construction des erreurs génériques propres au cycle de vie de la machine Amiga. |

### `Common/Machines/Common/Functions`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| `Common/Machines/Common/Functions/ConfigurationFunctions.cs` | `Common/Machines/Common/Functions/ConfigurationFunctions.cs` | Même responsabilité de configuration ; implémentation adaptée à la famille. |
| — | `Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` | Persistance du schéma de machine Atari. |
| — | `Common/Machines/Common/Functions/ConfigurationFunctions.Summary.cs` | Résumé de configuration Atari. |
| — | `Common/Machines/Common/Functions/EmulationPeripheralConversionFunctions.cs` | Conversion des périphériques Atari vers le contrat public. |
| — | `Common/Machines/Common/Functions/FirmwareFunctions.cs` | Gestion des micrologiciels Atari. |
| — | `Common/Machines/Common/Functions/FirmwareFunctions.Scan.cs` | Analyse des micrologiciels Atari. |
| — | `Common/Machines/Common/Functions/FirmwareFunctions.Selection.cs` | Sélection des micrologiciels Atari. |
| — | `Common/Machines/Common/Functions/HardDiskFormats.cs` | Formats de disques durs Atari. |
| — | `Common/Machines/Common/Functions/HardwareModelFunctions.cs` | Construit et valide le contrat matériel consommé par le `Common` et les adaptateurs. |
| — | `Common/Machines/Common/Functions/InputFunctions.Keyboard.cs` | Conversion du clavier Atari. |
| `Common/Machines/Common/Functions/InputFunctions.Settings.cs` | `Common/Machines/Common/Functions/InputFunctions.Settings.cs` | Même responsabilité de réglages d’entrée ; implémentation adaptée à la famille. |
| `Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` | `Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` | Même responsabilité de capture d’entrée ; implémentation adaptée à la famille. |
| `Common/Machines/Common/Functions/InputFunctions.Visuals.cs` | `Common/Machines/Common/Functions/InputFunctions.Visuals.cs` | Même responsabilité de représentation des entrées ; implémentation adaptée à la famille. |
| — | `Common/Machines/Common/Functions/MachineFunctions.cs` | Fonctions de cycle de vie des machines Atari. |
| — | `Common/Machines/Common/Functions/MachineFunctions.Hardware.cs` | Présentation du matériel Atari. |
| `Common/Machines/Common/Functions/MediaFunctions.cs` | — | Conversion et activité des médias Amiga regroupées dans un fichier de famille. |
| — | `Common/Machines/Common/Functions/MediaFunctions.Activity.cs` | Activité des médias Atari. |
| — | `Common/Machines/Common/Functions/MediaFunctions.Cartridge.cs` | Préparation des cartouches Atari. |
| — | `Common/Machines/Common/Functions/MediaFunctions.Cassette.cs` | Commandes cassette Atari. |
| — | `Common/Machines/Common/Functions/MediaFunctions.Compatibility.cs` | Compatibilité des médias Atari. |
| — | `Common/Machines/Common/Functions/MediaFunctions.Conversion.cs` | Conversion des médias Atari vers le contrat public. |
| — | `Common/Machines/Common/Functions/MediaFunctions.Runtime.cs` | État d’exécution des médias Atari. |
| — | `Common/Machines/Common/Functions/RuntimeFunctions.cs` | État, options, messages et raccourcis d’exécution Atari. |
| `Common/Machines/Common/Functions/SettingsFunctions.Builders.cs` | — | Construction des réglages Amiga. |
| `Common/Machines/Common/Functions/SettingsFunctions.Choices.cs` | — | Choix de réglages Amiga. |
| `Common/Machines/Common/Functions/SettingsFunctions.cs` | `Common/Machines/Common/Functions/SettingsFunctions.cs` | Même responsabilité de description des réglages ; implémentation adaptée à la famille. |
| — | `Common/Machines/Common/Functions/SettingsFunctions.Machine.AudioAndChoices.cs` | Réglages audio et choix Atari. |
| — | `Common/Machines/Common/Functions/SettingsFunctions.Machine.Builders.cs` | Construction des réglages Atari. |
| — | `Common/Machines/Common/Functions/SettingsFunctions.Machine.Hardware.cs` | Réglages fondés sur le contrat matériel commun aux familles Atari non-ST. |
| — | `Common/Machines/Common/Functions/SettingsFunctions.Machine.St.cs` | Réglages des machines Atari ST. |
| — | `Common/Machines/Common/Functions/StateFunctions.Saved.cs` | Validation des états sauvegardés Atari. |
| — | `Common/Machines/Common/Functions/StateFunctions.Store.cs` | Stockage des métadonnées d’état Atari. |
| `Common/Machines/Common/Functions/StorageFunctions.cs` | — | Réglages de stockage Amiga regroupés dans un fichier de famille. |
| — | `Common/Machines/Common/Functions/StorageFunctions.Configuration.cs` | Classification du stockage Atari. |
| — | `Common/Machines/Common/Functions/StorageFunctions.Devices.cs` | Périphériques de stockage Atari. |
| — | `Common/Machines/Common/Functions/StorageFunctions.Settings.cs` | Réglages de stockage Atari. |

### `Common/Services`

| Emulation.Amiga | Emulation.Atari | Description et problème |
|---|---|---|
| — | `Common/Services/AudioBuffer.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Services/AudioOutputController.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Services/CassetteInputController.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| `Common/Services/ConfigurationStore.cs` | `Common/Services/ConfigurationStore.cs` | Même chemin dans les deux modules ; l’implémentation reste adaptée à la famille. |
| — | `Common/Services/ContentPath.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Services/CoreHost.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Services/CoreOptionHost.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Services/CoreProvider.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Services/CoreReleaseService.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Services/DiskControl.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| `Common/Services/Engine.cs` | `Common/Services/Engine.cs` | Même chemin dans les deux modules ; l’implémentation reste adaptée à la famille. |
| — | `Common/Services/ExternalCore.Content.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Services/ExternalCore.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Services/ExternalCore.Media.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Services/ExternalHostCallbacks.AudioVideoInput.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Services/ExternalHostCallbacks.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Services/ExternalHostCallbacks.Environment.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Services/ExternalHostCallbacks.EnvironmentDetails.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Services/FirmwareScanner.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Services/FrameTimer.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| `Common/Services/InputAccumulator.cs` | — | Spécifique à la famille Amiga ; aucun fichier générique correspondant côté Atari. |
| — | `Common/Services/InputFrameStore.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Services/KeyboardState.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Services/LoadedContent.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| `Common/Services/Machine.Commands.cs` | `Common/Services/Machine.Commands.cs` | Même chemin dans les deux modules ; l’implémentation reste adaptée à la famille. |
| `Common/Services/Machine.cs` | `Common/Services/Machine.cs` | Même chemin dans les deux modules ; l’implémentation reste adaptée à la famille. |
| `Common/Services/Machine.Lifecycle.cs` | `Common/Services/Machine.Lifecycle.cs` | Même chemin dans les deux modules ; l’implémentation reste adaptée à la famille. |
| — | `Common/Services/ProcessCore.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Services/ProcessCore.Host.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Services/ProcessCore.Protocol.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| — | `Common/Services/SharedVideoWriter.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |
| `Common/Services/StateStore.cs` | — | Spécifique à la famille Amiga ; aucun fichier générique correspondant côté Atari. |
| — | `Common/Services/VideoBufferSet.cs` | Spécifique à la famille Atari ; aucun fichier générique correspondant côté Amiga. |

## 3. Aligner `Common/Contracts` entre Amiga et Atari

- [x] Rendre les contrats réellement communs identiques dans les deux modules.
  - [x] Classer les contrats actuels selon leur propriétaire réel.
    - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour ajouter le tableau de décision de chaque fichier de `Common/Contracts` Amiga et Atari.
  - [x] Sortir de `Common/Contracts` les contrats propres aux machines ou aux émulateurs.
    - [x] Déplacer les contrats propres aux machines Amiga de `src/GWGUI.Emulation.Amiga/Common/Contracts` vers `src/GWGUI.Emulation.Amiga/Common/Machines/Common/Contracts` et adapter leurs espaces de noms et consommateurs.
    - [x] Déplacer les contrats propres aux machines Atari de `src/GWGUI.Emulation.Atari/Common/Contracts` vers `src/GWGUI.Emulation.Atari/Common/Machines/Common/Contracts` et adapter leurs espaces de noms et consommateurs.
    - [x] Déplacer les contrats propres à Libretro de `src/GWGUI.Emulation.Atari/Common/Contracts` vers `src/GWGUI.Emulation.Atari/Emulators/Libretro/Contracts` et adapter leurs espaces de noms et consommateurs.
  - [x] Uniformiser les fichiers qui restent dans `Common/Contracts`.
    - [x] Modifier les fichiers `src/GWGUI.Emulation.Amiga/Common/Contracts/*.cs` et `src/GWGUI.Emulation.Atari/Common/Contracts/*.cs` afin d'avoir les mêmes noms de fichiers, types et membres génériques des deux côtés.
  - [x] Protéger l'alignement architectural.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` pour comparer les fichiers et membres de `Common/Contracts` entre Amiga et Atari.
  - [x] Actualiser l'inventaire documentaire.
    - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour régénérer les tableaux alphabétiques après les déplacements.
  - [x] Vérifier le résultat.
    - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour consigner la réussite des builds Amiga/Atari, des tests d'architecture, l'absence de dossiers vides et l'égalité finale de `Common/Contracts`.

### Décision pour les contrats

| Contrat actuel | Propriétaire retenu | Motif |
|---|---|---|
| `AudioContracts.cs` | `Common/Machines/Common/Contracts` | Configuration propre à la machine, actuellement Amiga uniquement. |
| `ConfigurationContracts.cs` | `Common/Machines/Common/Contracts` | Schéma persistant propre à chaque famille de machines. |
| `ControllerContracts.cs` | Séparé entre machine et émulateur | La configuration appartient à la machine ; les descripteurs natifs appartiennent au cœur d’émulation. |
| `CoreContracts.cs` | `Common/Contracts` pour les options génériques ; Libretro pour l’installation et l’interopération | `CoreOption` est consommé par l’interface commune ; les autres types Atari décrivent Libretro. |
| `EmulatorContracts.cs` | `Common/Contracts` pour les deux contextes génériques ; émulateur pour le contenu préparé et le catalogue concret | Les contextes sont le raccord commun ; les données concrètes dépendent de l’émulateur. |
| `FirmwareContracts.cs` | `Common/Machines/Common/Contracts` | Modèle de micrologiciel propre à la famille de machines. |
| `InputContracts.cs` | `Common/Machines/Common/Contracts` | Configuration persistante propre à la famille de machines. |
| `MachineContracts.cs` | `Common/Machines/Common/Contracts` | État et commandes d’exécution propres à la machine Atari. |
| `MediaContracts.cs` | Séparé entre machine et émulateur | Configuration/compatibilité côté machine ; état de disque et média préparé côté Libretro. |
| `ModelContracts.cs` | `Common/Machines/Common/Contracts` | Description propre aux modèles Amiga. |
| `RuntimeContracts.cs` | Séparé entre machine et émulateur | Raccourcis côté machine ; messages d’environnement côté Libretro. |
| `StateContracts.cs` | `Common/Machines/Common/Contracts` | Format d’état sauvegardé propre à la famille et à son schéma. |

## 4. Aligner `Common/Dictionaries` entre Amiga et Atari

- [x] Rendre les dictionnaires réellement communs identiques dans les deux modules.
  - [x] Classer les dictionnaires selon leur propriétaire réel.
    - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour ajouter la décision de propriété de chaque fichier de `Common/Dictionaries`.
  - [x] Sortir les catalogues de machines de `Common/Dictionaries`.
    - [x] Déplacer les catalogues Amiga de contrôleurs, micrologiciels, machines et modèles vers `src/GWGUI.Emulation.Amiga/Common/Machines/Common/Dictionaries`, puis adapter leurs espaces de noms et consommateurs.
    - [x] Déplacer les catalogues Atari de compatibilité, contrôleurs, micrologiciels, machines et modèles vers `src/GWGUI.Emulation.Atari/Common/Machines/Common/Dictionaries`, puis adapter leurs espaces de noms et consommateurs.
  - [x] Uniformiser le catalogue générique des émulateurs.
    - [x] Modifier les deux `Common/Dictionaries/EmulatorCatalog.cs` pour exposer les mêmes membres génériques à partir de `IEmulatorAdapter`, sans données de cœur concret dans le catalogue commun.
    - [x] Créer `src/GWGUI.Emulation.Atari/Emulators/Libretro/Dictionaries/CoreCatalog.cs` pour conserver le catalogue d’installation et d’interopération propre aux cœurs Libretro, puis adapter ses consommateurs.
  - [x] Protéger l’alignement architectural.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` pour comparer les noms de fichiers, types et membres publics de `Common/Dictionaries` entre Amiga et Atari.
  - [x] Actualiser l’inventaire documentaire.
    - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour régénérer les tableaux alphabétiques concernés après les déplacements.
  - [x] Vérifier le résultat.
    - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour consigner les builds, les tests d’architecture, l’absence de dossiers vides et l’égalité finale de `Common/Dictionaries`.

### Décision pour les dictionnaires

| Dictionnaire actuel | Propriétaire retenu | Motif |
|---|---|---|
| `CompatibilityCatalog.cs` | `Common/Machines/Common/Dictionaries` | Matrice propre aux modèles Atari. |
| `ControllerCatalog.cs` | `Common/Machines/Common/Dictionaries` | Périphériques proposés selon le modèle de machine. |
| `EmulatorCatalog.cs` | `Common/Dictionaries` pour la découverte générique ; Libretro pour les métadonnées techniques Atari | La découverte par `IEmulatorAdapter` est commune ; DLL, archives et versions appartiennent au cœur. |
| `FirmwareCatalog.cs` | `Common/Machines/Common/Dictionaries` | Catalogue de ROM et micrologiciels propre à chaque famille. |
| `MachineCatalog.cs` | `Common/Machines/Common/Dictionaries` | Liste des machines exposées par chaque famille. |
| `ModelCatalog.cs` | `Common/Machines/Common/Dictionaries` | Modèles et identifiants propres à chaque famille. |

## 5. Aligner `Common/Enums` entre Amiga et Atari

- [x] Retirer de `Common/Enums` les énumérations qui ne sont pas réellement communes aux familles.
  - [x] Classer chaque énumération selon son propriétaire réel.
    - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour ajouter la décision de propriété de chaque fichier de `Common/Enums`.
  - [x] Déplacer les énumérations de machines.
    - [x] Déplacer les énumérations Amiga de contrôleurs, micrologiciels, médias et sélection d’émulateur vers `src/GWGUI.Emulation.Amiga/Common/Machines/Common/Enums`, puis adapter les espaces de noms et consommateurs.
    - [x] Déplacer les énumérations Atari de contrôleurs, micrologiciels, entrées, machines, médias, réglages, états, stockage et sélection d’émulateur vers `src/GWGUI.Emulation.Atari/Common/Machines/Common/Enums`, puis adapter les espaces de noms et consommateurs.
  - [x] Déplacer les énumérations propres aux cœurs.
    - [x] Créer les fichiers nécessaires sous `src/GWGUI.Emulation.Amiga/Emulators/PUAE/Enums` pour les commandes du processus PUAE et adapter leurs consommateurs.
    - [x] Créer les fichiers nécessaires sous `src/GWGUI.Emulation.Atari/Emulators/Libretro/Enums` pour le protocole, l’environnement et les erreurs Libretro et adapter leurs consommateurs.
  - [x] Supprimer les coquilles sans contenu.
    - [x] Supprimer `src/GWGUI.Emulation.Atari/Common/Enums/VideoEnums.cs`, qui ne déclare aucun type.
  - [x] Protéger l’alignement architectural.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` pour vérifier que `Common/Enums` contient exactement les mêmes fichiers et types dans les deux modules.
  - [x] Actualiser l’inventaire documentaire.
    - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour régénérer les tableaux alphabétiques concernés après les déplacements.
  - [x] Vérifier le résultat.
    - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour consigner les builds, les tests d’architecture, l’absence de dossiers vides et l’égalité finale de `Common/Enums`.

### Décision pour les énumérations

| Fichier actuel | Propriétaire retenu | Motif |
|---|---|---|
| `ControllerEnums.cs` | `Common/Machines/Common/Enums` | Types de contrôleurs propres aux machines de la famille. |
| `CoreEnums.cs` | Séparé entre machine et émulateur | La sélection d’émulateur appartient à la configuration de machine ; le protocole hôte appartient au cœur concret. |
| `FirmwareEnums.cs` | `Common/Machines/Common/Enums` | Catégories et états de micrologiciels propres à la famille. |
| `InputEnums.cs` | `Common/Machines/Common/Enums` | Options de réglage propres aux machines Atari. |
| `MachineEnums.cs` | `Common/Machines/Common/Enums` | Familles et modèles Atari. |
| `MediaEnums.cs` | `Common/Machines/Common/Enums` | Catégories et métadonnées de médias propres à la famille. |
| `RuntimeEnums.cs` | Séparé entre machine et Libretro | Région et raccourcis côté machine ; langue et erreurs côté protocole Libretro. |
| `SettingsEnums.cs` | `Common/Machines/Common/Enums` | Onglets de réglages Atari. |
| `StateEnums.cs` | `Common/Machines/Common/Enums` | Catégories d’états sauvegardés Atari. |
| `StorageEnums.cs` | `Common/Machines/Common/Enums` | Bus de stockage Atari. |
| `VideoEnums.cs` | Suppression | Fichier vide sans type déclaré. |

## 6. Aligner `Common/Exceptions`, `Factories`, `Functions` et `Interfaces`

- [x] Aligner les quatre derniers groupes demandés entre Amiga et Atari.
  - [x] 6.1 Aligner `Common/Exceptions`.
    - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour consigner la propriété réelle de `MachineExceptions.cs`, `CartridgeExceptions.cs`, `EmulationException.cs` et `ErrorMessages.cs`.
    - [x] Déplacer `src/GWGUI.Emulation.Amiga/Common/Exceptions/MachineExceptions.cs` vers `src/GWGUI.Emulation.Amiga/Common/Machines/Common/Exceptions/MachineExceptions.cs`, adapter son espace de noms et ses consommateurs.
    - [x] Déplacer les exceptions Atari propres aux machines vers `src/GWGUI.Emulation.Atari/Common/Machines/Common/Exceptions`, les exceptions propres à Libretro vers `src/GWGUI.Emulation.Atari/Emulators/Libretro/Exceptions`, puis adapter leurs consommateurs.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` pour contrôler l’égalité finale de `Common/Exceptions`.
  - [x] 6.2 Aligner `Common/Factories`.
    - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour consigner que `MachineFactory.cs` est la base des adaptateurs Libretro Atari et non une factory générique aux familles.
    - [x] Déplacer `src/GWGUI.Emulation.Atari/Common/Factories/MachineFactory.cs` vers `src/GWGUI.Emulation.Atari/Emulators/Libretro/Factories/MachineFactory.cs`, adapter son espace de noms et ses consommateurs.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` pour contrôler l’égalité finale de `Common/Factories`.
  - [x] 6.3 Aligner `Common/Functions`.
    - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour classer chaque fichier de fonctions comme générique identique, fonction de machine ou fonction d’émulateur.
    - [x] Déplacer les fonctions Amiga propres aux machines vers `src/GWGUI.Emulation.Amiga/Common/Machines/Common/Functions` et celles propres à PUAE vers `src/GWGUI.Emulation.Amiga/Emulators/PUAE/Functions`, avec leurs espaces de noms et consommateurs.
    - [x] Déplacer les fonctions Atari propres aux machines vers `src/GWGUI.Emulation.Atari/Common/Machines/Common/Functions` et celles propres à Libretro vers `src/GWGUI.Emulation.Atari/Emulators/Libretro/Functions`, avec leurs espaces de noms et consommateurs.
    - [x] Modifier les fichiers restant dans les deux `Common/Functions` pour qu’ils aient les mêmes noms, types et membres génériques, ou les déplacer si aucun contenu n’est réellement commun.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` pour contrôler l’égalité finale de `Common/Functions`.
  - [x] 6.4 Aligner `Common/Interfaces`.
    - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour classer les quatre interfaces actuelles selon la frontière famille–émulateur ou leur dépendance à Libretro.
    - [x] Déplacer `ICoreReleaseService.cs`, `IEmulatorCore.cs` et `IEmulatorMediaAdapter.cs` vers `src/GWGUI.Emulation.Atari/Emulators/Libretro/Interfaces`, puis adapter leurs espaces de noms et consommateurs.
    - [x] Déplacer `src/GWGUI.Emulation.Amiga/Common/Interfaces/IEmulatorCore.cs` vers `src/GWGUI.Emulation.Amiga/Emulators/PUAE/Interfaces/IEmulatorCore.cs`, puis adapter son espace de noms et ses consommateurs.
    - [x] Conserver les deux `Common/Interfaces/IEmulatorAdapter.cs` strictement identiques comme unique contrat générique famille–émulateur.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` pour comparer le contenu complet de `Common/Interfaces` entre Amiga et Atari.
  - [x] 6.5 Actualiser l’inventaire et vérifier l’ensemble.
    - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour régénérer les tableaux alphabétiques de tous les dossiers concernés et leurs nouveaux propriétaires.
    - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour consigner les builds, les tests d’architecture, l’absence de dossiers vides, l’absence d’anciens espaces de noms et les égalités finales.

### Décision pour les exceptions

| Fichier actuel | Propriétaire retenu | Motif |
|---|---|---|
| `MachineExceptions.cs` | `Common/Machines/Common/Exceptions` Amiga | Erreurs d’état, de média et de cycle de vie propres à la machine Amiga. |
| `CartridgeExceptions.cs` | `Emulators/Libretro/Exceptions` Atari | Erreurs structurées émises à la frontière des cœurs Atari gérant les cartouches. |
| `EmulationException.cs` | `Emulators/Libretro/Exceptions` Atari | Exception structurée du protocole hôte Libretro. |
| `ErrorMessages.cs` | `Emulators/Libretro/Exceptions` Atari | Résolution localisée actuellement consommée par le pipeline et les adaptateurs Libretro. |

### Décision pour les interfaces

| Interface actuelle | Propriétaire retenu | Motif |
|---|---|---|
| `IEmulatorAdapter.cs` | `Common/Interfaces` | Contrat générique identique reliant une famille à ses adaptateurs d’émulation. |
| `IEmulatorCore.cs` | `Emulators/PUAE/Interfaces` ou `Emulators/Libretro/Interfaces` | Contrat bas niveau différent selon le protocole du cœur concret. |
| `ICoreReleaseService.cs` | `Emulators/Libretro/Interfaces` | Installation et versions propres au catalogue de cœurs Libretro Atari. |
| `IEmulatorMediaAdapter.cs` | `Emulators/Libretro/Interfaces` | Préparation technique des médias pour le pipeline Libretro Atari. |

## 7. Séparer les familles de machines Atari

- [x] Remplacer le regroupement artificiel `AtariClassic` par les familles de machines réelles et vérifier leur raccordement au `Common`.
  - [x] 7.1 Extraire les primitives matérielles réellement partagées de `AtariClassic`.
    - [x] Créer `src/GWGUI.Emulation.Atari/Common/Machines/Common/Contracts/HardwareModelContracts.cs` avec les contrats descriptifs communs actuellement déclarés comme `ClassicModelDefinition` et `ClassicPortDefinition`.
    - [x] Créer `src/GWGUI.Emulation.Atari/Common/Machines/Common/Enums/HardwareModelEnums.cs` avec les capacités matérielles communes actuellement déclarées sous les noms `ClassicCpu`, `ClassicRegion`, `ClassicVideoCapability`, `ClassicAudioCapability`, `ClassicPortCapability` et `ClassicStorageCapability`.
    - [x] Créer `src/GWGUI.Emulation.Atari/Common/Machines/Common/Functions/HardwareModelFunctions.cs` avec les fonctions communes de création et de vérification des définitions matérielles, sans constante propre à une famille.
  - [x] 7.2 Donner à chaque famille ses constantes et son catalogue.
    - [x] Créer `src/GWGUI.Emulation.Atari/Common/Machines/Atari8Bit/Constants/ModelConstants.cs` et `src/GWGUI.Emulation.Atari/Common/Machines/Atari8Bit/Dictionaries/ModelCatalog.cs` pour y placer exclusivement les Atari 400, 800, 800 XL, 130 XE, XL/XE et XEGS.
    - [x] Créer `src/GWGUI.Emulation.Atari/Common/Machines/Atari2600/Constants/ModelConstants.cs` et `src/GWGUI.Emulation.Atari/Common/Machines/Atari2600/Dictionaries/ModelCatalog.cs` pour le modèle Atari 2600.
    - [x] Créer `src/GWGUI.Emulation.Atari/Common/Machines/Atari5200/Constants/ModelConstants.cs` et `src/GWGUI.Emulation.Atari/Common/Machines/Atari5200/Dictionaries/ModelCatalog.cs` pour le modèle Atari 5200.
    - [x] Créer `src/GWGUI.Emulation.Atari/Common/Machines/Atari7800/Constants/ModelConstants.cs` et `src/GWGUI.Emulation.Atari/Common/Machines/Atari7800/Dictionaries/ModelCatalog.cs` pour le modèle Atari 7800.
    - [x] Créer `src/GWGUI.Emulation.Atari/Common/Machines/AtariLynx/Constants/ModelConstants.cs` et `src/GWGUI.Emulation.Atari/Common/Machines/AtariLynx/Dictionaries/ModelCatalog.cs` pour le modèle Atari Lynx.
    - [x] Créer `src/GWGUI.Emulation.Atari/Common/Machines/AtariJaguar/Constants/ModelConstants.cs` et `src/GWGUI.Emulation.Atari/Common/Machines/AtariJaguar/Dictionaries/ModelCatalog.cs` pour les modèles Atari Jaguar et Jaguar CD.
  - [x] 7.3 Raccorder les catalogues séparés sans recréer un faux groupe de machines.
    - [x] Créer `src/GWGUI.Emulation.Atari/Common/Machines/Common/Dictionaries/HardwareModelCatalog.cs` pour agréger les catalogues des familles réelles et fournir la recherche commune aux consommateurs.
    - [x] Modifier les fichiers consommateurs sous `src/GWGUI.Emulation.Atari/Common/Machines` afin que les constantes alimentent les contrats et catalogues attendus par les fonctions communes.
    - [x] Modifier les fichiers consommateurs sous `src/GWGUI.Emulation.Atari/Emulators` afin que chaque adaptateur utilise les contrats et interfaces du `Common` sans type parallèle ni donnée recopiée.
    - [x] Renommer `src/GWGUI.Emulation.Atari/Common/Machines/Common/Functions/SettingsFunctions.Machine.Classic.cs` en `src/GWGUI.Emulation.Atari/Common/Machines/Common/Functions/SettingsFunctions.Machine.Hardware.cs` et remplacer les noms `Classic` qui désignent à tort l’ensemble de ces familles.
    - [x] Modifier `src/GWGUI.Emulation.Atari/EmulationGlobalUsings.cs` pour référencer les six familles explicites et retirer tous les espaces de noms `AtariClassic`.
    - [x] Modifier les fichiers d’erreurs Atari concernés pour remplacer les identifiants `ClassicModel` par des identifiants décrivant le catalogue matériel.
  - [x] 7.4 Supprimer l’ancien regroupement et protéger la séparation et ses raccordements.
    - [x] Supprimer les cinq fichiers sous `src/GWGUI.Emulation.Atari/Common/Machines/AtariClassic` après migration complète de leur contenu.
    - [x] Supprimer les dossiers désormais vides sous `src/GWGUI.Emulation.Atari/Common/Machines/AtariClassic` après avoir vérifié qu’ils ne contiennent plus aucun fichier.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` pour exiger `Atari8Bit`, `Atari2600`, `Atari5200`, `Atari7800`, `AtariLynx`, `AtariJaguar` et `AtariST`, interdire `AtariClassic` et vérifier les raccordements entre contrats communs et adaptateurs.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Machines/Common/Dictionaries/ModelCatalog.cs` pour construire les modèles exposés au `Common` à partir de `StModelCatalog` et `HardwareModelCatalog`, sans recopier leurs clés de ressources.
    - [x] Supprimer `src/GWGUI.Emulation.Atari/Common/Machines/Common/Constants/ModelConstants.cs` après suppression de ses constantes de modèles dupliquées.
    - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour actualiser l’inventaire alphabétique et expliquer la propriété et le raccordement de chaque nouveau dossier de machines.
  - [x] 7.5 Vérifier la séparation.
    - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour consigner la réussite du build Atari, des tests d’architecture, l’absence de référence à `AtariClassic`, l’absence de type parallèle aux contrats communs et l’absence de dossier vide.
  - [x] 7.6 Retirer les descriptions documentaires obsolètes de `AtariClassic`.
    - [x] Modifier `docs/reference/emulator-adapter-file-map.md` pour remplacer le regroupement `AtariClassic` par les six familles non-ST et leur contrat matériel commun.
    - [x] Modifier `docs/reference/emulation-module-file-map.md` pour remplacer les anciennes entrées `AtariClassic*` par les fichiers et types matériels actuels.
    - [x] Modifier `docs/tasks/emulation/emulation-family-common-organization.md` pour consigner les sept dossiers de familles Atari actuels.

### Raccordement des familles Atari

| Niveau | Propriétaire | Raccordement |
|---|---|---|
| Constantes de modèle | Chaque dossier `Atari8Bit`, `Atari2600`, `Atari5200`, `Atari7800`, `AtariLynx`, `AtariJaguar` ou `AtariST` | Les identifiants, ressources et caractéristiques restent avec la machine ou le groupe de machines qui les définit. |
| Contrat matériel | `Common/Machines/Common/Contracts/HardwareModelContracts.cs` | Chaque catalogue non-ST produit directement un `HardwareModelDefinition` ; aucun contrat concurrent n’existe dans les familles ou les émulateurs. |
| Vocabulaire et fonctions | `Common/Machines/Common/Enums/HardwareModelEnums.cs` et `Common/Machines/Common/Functions/HardwareModelFunctions.cs` | Les capacités, la création, l’indexation et la compatibilité sont partagées sans recopier les données des machines. |
| Agrégation | `Common/Machines/Common/Dictionaries/HardwareModelCatalog.cs` | Réunit les six catalogues non-ST ; `ModelCatalog.cs` prend ses noms depuis cette agrégation et depuis `StModelCatalog`. |
| Interface famille–émulateur | `Common/Interfaces/IEmulatorAdapter.cs` | `CoreCatalog` résout l’émulateur déclaré par chaque définition en un adaptateur qui implémente cette interface commune. |
| Adaptateurs concrets | `Emulators/Atari800` et `Emulators/Libretro` | Consomment `HardwareModelCatalog`, `HardwareRegion` et `HardwarePortCapability` ; aucun type `Classic*` parallèle n’est conservé. |

## 8. Regrouper l’infrastructure commune aux machines

- [x] Créer `Common/Machines/Common` dans les modules Amiga et Atari pour les catégories partagées par leurs machines.
  - [x] 8.1 Déplacer l’infrastructure commune Atari.
    - [x] Déplacer les fichiers `src/GWGUI.Emulation.Atari/Common/Machines/Constants/*.cs` vers `src/GWGUI.Emulation.Atari/Common/Machines/Common/Constants/` et remplacer leur espace de noms par `GWGUI.Emulation.Atari.Common.Machines.Common.Constants`.
    - [x] Déplacer les fichiers `src/GWGUI.Emulation.Atari/Common/Machines/Contracts/*.cs` vers `src/GWGUI.Emulation.Atari/Common/Machines/Common/Contracts/` et remplacer leur espace de noms par `GWGUI.Emulation.Atari.Common.Machines.Common.Contracts`.
    - [x] Déplacer les fichiers `src/GWGUI.Emulation.Atari/Common/Machines/Dictionaries/*.cs` vers `src/GWGUI.Emulation.Atari/Common/Machines/Common/Dictionaries/` et remplacer leur espace de noms par `GWGUI.Emulation.Atari.Common.Machines.Common.Dictionaries`.
    - [x] Déplacer les fichiers `src/GWGUI.Emulation.Atari/Common/Machines/Enums/*.cs` vers `src/GWGUI.Emulation.Atari/Common/Machines/Common/Enums/` et remplacer leur espace de noms par `GWGUI.Emulation.Atari.Common.Machines.Common.Enums`.
    - [x] Déplacer les fichiers `src/GWGUI.Emulation.Atari/Common/Machines/Functions/*.cs` vers `src/GWGUI.Emulation.Atari/Common/Machines/Common/Functions/` et remplacer leur espace de noms par `GWGUI.Emulation.Atari.Common.Machines.Common.Functions`.
  - [x] 8.2 Déplacer l’infrastructure commune Amiga.
    - [x] Déplacer les fichiers `src/GWGUI.Emulation.Amiga/Common/Machines/Constants/*.cs` vers `src/GWGUI.Emulation.Amiga/Common/Machines/Common/Constants/` et remplacer leur espace de noms par `GWGUI.Emulation.Amiga.Common.Machines.Common.Constants`.
    - [x] Déplacer les fichiers `src/GWGUI.Emulation.Amiga/Common/Machines/Contracts/*.cs` vers `src/GWGUI.Emulation.Amiga/Common/Machines/Common/Contracts/` et remplacer leur espace de noms par `GWGUI.Emulation.Amiga.Common.Machines.Common.Contracts`.
    - [x] Déplacer les fichiers `src/GWGUI.Emulation.Amiga/Common/Machines/Dictionaries/*.cs` vers `src/GWGUI.Emulation.Amiga/Common/Machines/Common/Dictionaries/` et remplacer leur espace de noms par `GWGUI.Emulation.Amiga.Common.Machines.Common.Dictionaries`.
    - [x] Déplacer les fichiers `src/GWGUI.Emulation.Amiga/Common/Machines/Enums/*.cs` vers `src/GWGUI.Emulation.Amiga/Common/Machines/Common/Enums/` et remplacer leur espace de noms par `GWGUI.Emulation.Amiga.Common.Machines.Common.Enums`.
    - [x] Déplacer `src/GWGUI.Emulation.Amiga/Common/Machines/Exceptions/MachineExceptions.cs` vers `src/GWGUI.Emulation.Amiga/Common/Machines/Common/Exceptions/MachineExceptions.cs` et remplacer son espace de noms par `GWGUI.Emulation.Amiga.Common.Machines.Common.Exceptions`.
    - [x] Déplacer les fichiers `src/GWGUI.Emulation.Amiga/Common/Machines/Functions/*.cs` vers `src/GWGUI.Emulation.Amiga/Common/Machines/Common/Functions/` et remplacer leur espace de noms par `GWGUI.Emulation.Amiga.Common.Machines.Common.Functions`.
  - [x] 8.3 Raccorder les nouveaux espaces de noms.
    - [x] Modifier `src/GWGUI.Emulation.Atari/EmulationGlobalUsings.cs` pour importer les catégories de `Common/Machines/Common` et retirer les anciens imports directs sous `Machines`.
    - [x] Modifier `src/GWGUI.Emulation.Amiga/EmulationGlobalUsings.cs` pour importer les catégories de `Common/Machines/Common` et retirer les anciens imports directs sous `Machines`.
    - [x] Modifier les fichiers C# Atari et Amiga qui référencent explicitement les anciens espaces de noms `Common.Machines.Constants` ou `Common.Machines.Functions` afin de viser `Common.Machines.Common`.
  - [x] 8.4 Protéger et documenter la structure.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` pour exiger le dossier `Machines/Common`, ses catégories, et interdire les catégories directement sous `Machines`.
    - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour régénérer l’inventaire alphabétique avec les chemins `Common/Machines/Common/...` et consigner la portée des fichiers déplacés.
    - [x] Modifier `docs/reference/emulator-adapter-file-map.md`, `docs/reference/emulation-module-file-map.md` et `docs/tasks/emulation/emulation-family-common-organization.md` pour décrire le nouveau niveau `Machines/Common`.
  - [x] 8.5 Vérifier le déplacement.
    - [x] Modifier les fichiers sous `tests/GWGUI.Tests/Emulation` qui importent explicitement `GWGUI.Emulation.Atari.Common.Machines.*` ou `GWGUI.Emulation.Amiga.Common.Machines.*` afin d’ajouter le niveau `Common` aux catégories déplacées.
    - [x] Modifier `tests/GWGUI.LocalDiskImageTests/TemporaryLibretroMediaReader/TemporaryLibretroMediaReader.cs` pour importer les contrats et enums depuis `Common.Machines.Common`.
    - [x] Modifier `docs/reference/emulation-common-file-comparison.md` pour consigner les builds Amiga et Atari, les tests d’architecture, l’absence d’ancien espace de noms et l’absence de dossier vide.
