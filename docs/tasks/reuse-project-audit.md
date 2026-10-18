# Audit global des réutilisations et doublons

Principe demandé : retrouver la version qui existait AVANT l’introduction du doublon,
la réutiliser et l’adapter si nécessaire. Ne pas remplacer l’ancien composant par le
nouveau doublon. Inscrire chaque correction dès confirmation avec preuve et antériorité.
Corrections autorisées après le repérage. Aucun build ni commit demandé.

Périmètre confirmé par l’utilisateur : rechercher et corriger tous les doublons
inutiles dans tout le projet, sans exclusion par date, y compris ceux qui restent
à découvrir. La période Amstrad sert de repère historique, pas de filtre du code
à examiner. Les exclusions par ancienneté consignées ci-dessous sont annulées.

- [x] Inventorier l’ensemble du périmètre.
  - [x] Créer `docs/reference/reuse-audit/task-inventory.json` et `history.json` : fichiers de tâches actuels, chemins cités et commits depuis la première création du code Amstrad `ac7292d86` jusqu’à HEAD ; exclure les tâches supprimées/déplacées et l’historique antérieur.
  - [x] Créer temporairement `artifacts/reuse-audit-scan.ps1` et `artifacts/reuse-audit-baseline.json` pour analyser les méthodes C# de tous les projets et celles de la version antérieure à Amstrad ; créer `docs/reference/reuse-audit/method-clones.json`, `clone-groups.json`, `provenance.json` et `supporting-files.json` avec les correspondances et leurs origines ; supprimer les deux fichiers temporaires après exploitation.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec les comparaisons manuelles par domaine, les versions antérieures et les limites de couverture.

## Corrections déjà identifiées — restent à faire

- [x] Mutualiser les éditeurs de champs à partir de l’existant antérieur.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec le commit d’origine de `StorageConfigurationFieldFunctions.cs` et des méthodes `EmulationModuleSettingsSection.Fields.cs` / `.Slider.cs`, puis y définir les modifications du composant antérieur à réutiliser avant toute modification de code.
  - [x] Créer `src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationSettingsFieldControlFunctions.cs` à partir de la construction de champs existant avant `bdff8d10c` ; modifier `EmulationModuleSettingsSection.Fields.cs`, `.Slider.cs`, `.Persistence.cs` et `StorageConfigurationFieldFunctions.cs` pour l’appeler avec leurs callbacks et conserver leurs règles de sauvegarde distinctes.
- [x] Supprimer la duplication des modifications INI PCEE2.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec l’origine des méthodes `NetworkFunctions.BuildSettings` et `MemoryCardFunctions.BuildSettings`, et le composant antérieur applicable avant de définir les fichiers à modifier.
  - [x] Créer `src/GWGUI.Emulation.Sony/Emulators/Pcee2/Functions/IniSettingsFunctions.cs` en extrayant le setter de la version réseau `403ce5836` et en paramétrant la section ; modifier `NetworkFunctions.cs` et `MemoryCardFunctions.cs` pour l’appeler et supprimer les deux setters recopiés.
- [x] Retirer le classement des visuels devenu inaccessible.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec la comparaison historique de `InputFunctions.Visuals.cs`, pour conserver le classement antérieur adapté sans préserver une seconde liste morte.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/InputFunctions.Visuals.cs` pour ne conserver qu’un classement atteignable PS1/PS2 à partir de la version antérieure à son extension ; supprimer les branches de repli mortes sans prendre leurs listes enrichies récentes comme nouvelle référence.
- [x] Retirer le détail Sony de la fenêtre de stockage générique.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec la fenêtre et le contrat de stockage antérieurs à `ManagedStorageConfigurationDialog.cs`, puis définir leur adaptation avant toute modification.
  - [x] Modifier `src/GWGUI.App/Views/Dialogs/Emulation/Storage/ManagedStorageConfigurationDialog.cs`, `src/GWGUI.App/Controllers/Emulation/Storage/EmulationStorageSettingsController.cs` et `src/GWGUI.Emulation.Sony/Emulators/Rpcs3/Functions/StorageFunctions.cs` pour transmettre l’aide via le contrat de périphérique existant, retirer la clé Sony du code App et conserver StorageDialogUi antérieur.
- [x] Réaligner le test Memory Stick avec le changement effectif de chemin.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec l’origine du changement de `StorageConstants.MemoryStickSubdirectory` et de l’assertion `ConfigurationPresentationTests.cs:419` ; conserver le chemin demandé et adapter le test concerné.

  - [x] Modifier `tests/GWGUI.Tests/Emulation/Sony/ConfigurationPresentationTests.cs` pour attendre `Memory Stick/Sony/PSP`, chemin demandé et déjà produit par StorageConstants ; ne pas rétablir l’ancien chemin de production pour faire passer le test obsolète.

Ces cinq points proviennent du relevé partiel. Les nouvelles découvertes seront ajoutées
à la suite, avant leur correction. Les lectures seules ne sont pas cochées comme corrections.


Périmètre rectifié : le premier ajout des sources Amstrad est `ac7292d86`
(25 septembre 2026) ; `6ef78dc70` (26 septembre) ajoute son module et son projet.
Départ retenu : `ac7292d86`, avec comparaison à son parent pour retrouver l’existant.
Les README préparatoires seuls ne constituent pas le début du code. L’inventaire
antérieur trop large est remplacé, sans poursuivre les documents supprimés.


## Nouvelles découvertes hors Sony — tâches inscrites dès le relevé

Les sliders Plasma/Vector et les champs DotMatrix/VFD/EPaper existaient déjà avant
`ac7292d86`. Leur ancienneté ne les exclut pas : ils sont maintenant raccordés aux
primitives FixedPixel antérieures dans les composants communs, comme détaillé en fin de suivi.

- [x] Reprendre le traitement de contenu NEC existant pour le second cœur PC-98.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec l’origine de `Emulators/NekoProjectII/Functions/ContentFunctions.Prepare` et `Emulators/NP2Kai/Functions/ContentFunctions.Prepare` avant de définir l’appel commun : corps identiques de 375 tokens.
  - [x] Créer `src/GWGUI.Emulation.NEC/Emulators/Common/Interop/Functions/Pc98ContentFunctions.cs` à partir du traitement initial `2a9191065`, avec les profils, paramètres de commande et limites fournis par chaque cœur ; modifier les deux `ContentFunctions.cs` pour y déléguer la préparation et les deux `Constants/ContentConstants.cs` pour retirer les constantes de syntaxe désormais possédées par le traitement commun.
- [x] Reprendre la préparation de médias SNK existante dans les variantes MAME/FBA.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec l’antériorité et les consommateurs de `Mame/Functions/MediaRuntimeFunctions.Prepare`, `Mame2016/Functions/MediaRuntimeFunctions.Prepare`, ainsi que Mame2000/Mame2003 et FbAlpha2012/FbAlpha2012NeoGeo ; comparer les parties communes avant adaptation de l’ancien composant.
  - [x] Créer `src/GWGUI.Emulation.Snk/Emulators/Common/Interop/Functions/MameMediaFunctions.cs` depuis les routines initiales `a1f460542` ; modifier les `MediaRuntimeFunctions.cs` et les factories Mame/Mame2016 pour partager la préparation et les cartes tout en passant les limites, extensions et fonctions firmware natives.
  - [x] Créer `src/GWGUI.Emulation.Snk/Emulators/Common/Interop/Functions/ArchiveMediaFunctions.cs` depuis les routines initiales Mame2000 et FbAlpha2012 ; modifier les `MediaRuntimeFunctions.cs` de Mame2000/Mame2003 et FbAlpha2012/FbAlpha2012NeoGeo pour transmettre leurs tables de parents, extensions et paramètres natifs distincts.
- [x] Retirer les recopies internes NEC des callbacks et du protocole de processus.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec les versions d’origine de `Emulators/Common/Interop/Services/ExternalHostCallbacks.*` / `ProcessCore.*` et de `Emulators/BeetlePcfx/Services/*`, en séparant les différences natives des méthodes strictement identiques déjà fournies par le Common du même module.
  - [x] Modifier `src/GWGUI.Emulation.Nec/Emulators/Common/Interop/Services/ExternalHostCallbacks.cs`, `.Input.cs`, `.Environment.cs`, `.AudioVideo.cs` et `ExternalDiskControl.cs` en y déplaçant les corps du backend BeetlePcfx antérieur ; paramétrer le préfixe des erreurs et reprendre CaptureBasic ; modifier `Common/Interop/Exceptions/CoreExceptions.cs` pour réutiliser sa résolution des textes, et `BeetlePcfx/Services/ExternalCore.cs` pour utiliser ces callbacks avec ses propres erreurs ; supprimer les cinq anciennes copies sous `BeetlePcfx/Services/`.
  - [x] Modifier `src/GWGUI.Emulation.Nec/Emulators/Common/Interop/Services/ProcessCore.cs` en y déplaçant le ProcessCore antérieur BeetlePcfx et en paramétrant commande hôte, préfixes de canaux, erreurs et nom de diagnostic ; modifier `BeetlePcfx/Factories/BeetlePcfxMachineFactory.cs` pour transmettre ses constantes ; supprimer `BeetlePcfx/Services/ProcessCore.cs` après raccordement.
  - [x] Modifier `src/GWGUI.Emulation.Nec/Emulators/Common/Interop/Constants/ExternalHostCallbacksConstants.cs` et `Contracts/ControllerContracts.cs` en reprenant les définitions antérieures strictement identiques ; modifier `BeetlePcfx/Services/ExternalCore.cs` pour utiliser ces constantes ; supprimer `BeetlePcfx/Constants/ExternalHostCallbacksConstants.cs` et `BeetlePcfx/Contracts/ControllerContracts.cs` devenus redondants.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec les résultats de la vérification Roslyn des sources NEC/SNK et des raccordements modifiés, sans émission de binaires ; corriger dans les fichiers concernés toute erreur introduite par les extractions avant de poursuivre.
- [x] Réexaminer les recopies internes Sony des callbacks.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec l’antériorité des implémentations Common, Pcsx2 et Ppsspp de `RegisterLegacyOptions`, `RegisterVersionTwoOptions` et `HandleVideo`, puis définir la réutilisation de l’implémentation la plus ancienne applicable.
  - [x] Créer `src/GWGUI.Emulation/Functions/EmulationCoreOptionFunctions.cs` en extrayant les lecteurs Legacy/V2 de `cf174afc6` Amiga vers les contrats EmulationOption existants ; modifier `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Functions/CoreOptionMetadataFunctions.cs` et les trois `ExternalHostCallbacks.Environment.cs` Common/Pcsx2/Ppsspp pour les appeler et supprimer les lecteurs locaux.
  - [x] Modifier les trois `ExternalHostCallbacks.cs` et `ExternalHostCallbacks.Input.cs` Sony pour utiliser le `Common/Services/InputAccumulator.cs` existant pour l’accumulation et la consommation ; conserver les coordonnées et la publication clavier dans chaque propriétaire.
  - [x] Créer `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Functions/CoreVideoFunctions.cs` depuis le traitement vidéo Pcsx2 antérieur au Common ; modifier les trois `ExternalHostCallbacks.AudioVideo.cs` pour lui passer leur session matérielle empruntée, leur format et leur séquence.

Les duplications exactes entre modules indépendants sont inventoriées séparément ;
elles ne sont pas toutes déclarées erronées sans comparaison avec le SDK et les
responsabilités propres à chaque module.


Le double switch de famille Atari a été comparé à ses fichiers antérieurs
`Functions/AtariConfigurationFunctions.cs` et `AtariStorageConfigurationFunctions.cs` :
les deux corps étaient déjà identiques avant Amstrad. Le déplacement/renommage des
références a changé le hash sans créer ce doublon. Ce point est donc retiré des
corrections imputées à la période et conservé comme référence ancienne.

- [x] Reprendre les routines de firmware SNK déjà définies pour les variantes identiques.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec les origines de `FirmwareRuntimeFunctions.SelectBios/Inspect/Prepare/Valid` entre Mame/Mame2016, Mame2000/Mame2003/Mame2010, HbMame/Mame2015 et FbAlpha2012/FbAlpha2012NeoGeo ; conserver les différences de noms et révisions natives comme données du cœur.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Emulators/Common/Interop/Functions/FirmwareRuntimeFunctions.cs` pour reprendre depuis `a1f460542` les opérations Inspect/Prepare/SelectedBios communes ; modifier les neuf `FirmwareRuntimeFunctions.cs` Mame, Mame2016, Mame2000, Mame2003, Mame2010, HbMame, Mame2015, FbAlpha2012 et FbAlpha2012NeoGeo pour appeler ces opérations avec leurs validations natives.
  - [x] Modifier le même composant Common pour reprendre SelectBios Mame et Valid FBA depuis `a1f460542` avec les tables natives en paramètres ; remplacer les quatre méthodes correspondantes Mame/Mame2016 et FbAlpha2012/FbAlpha2012NeoGeo par des appels à ces traitements et supprimer leurs constantes devenues inutilisées.

Les constructeurs FileSystemEntry/Volume de MediaEngine et MediaFileSystems,
`AddRange` des lecteurs HDD et `CopyAsync` des writers de bande ont aussi des
correspondances antérieures à Amstrad. Ils restent à qualifier et à corriger lorsque
la duplication est inutile ; aucune exclusion par date ne s’applique.


- [x] Réutiliser l’infrastructure de processus antérieure dans les nouveaux modules.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec la comparaison de l’ancien `GWGUI.Emulation.Atari/Services/AtariProcessCore.cs` (`Request`, réponses, arrêt) aux copies de `Common/Services/ProcessCore.*` ; identifier les paramètres réellement spécifiques avant de définir l’adaptation du propriétaire antérieur, sans introduire de dépendance entre modules indépendants.
  - [x] Modifier `src/GWGUI.Emulation/Functions/EmulationHostProtocolFunctions.cs` pour extraire ExecuteRequest et ReadResponseAsync depuis le ProcessCore Atari antérieur ; modifier `src/GWGUI.Emulation/Functions/EmulationChildProcessLifetime.cs` pour reprendre Start et WaitForExit avec transfert de propriété et nettoyage en finally.
  - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Services/ProcessCore.Protocol.cs` et `src/GWGUI.Emulation.Atari/Common/Services/ProcessCore.Host.cs` pour appeler les primitives SDK de requête, lecture et lancement ; conserver le codec, les délais et les erreurs de ce module ainsi que ses finally de libération.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Common/Services/ProcessCore.Protocol.cs` et `src/GWGUI.Emulation.Acorn/Common/Services/ProcessCore.Host.cs` pour appeler les primitives SDK de requête, lecture et lancement ; conserver le codec, les délais et les erreurs de ce module ainsi que ses finally de libération.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Common/Services/ProcessCore.Protocol.cs` et `src/GWGUI.Emulation.Apple/Common/Services/ProcessCore.Host.cs` pour appeler les primitives SDK de requête, lecture et lancement ; conserver le codec, les délais et les erreurs de ce module ainsi que ses finally de libération.
  - [x] Modifier `src/GWGUI.Emulation.Bandai/Common/Services/ProcessCore.Protocol.cs` et `src/GWGUI.Emulation.Bandai/Common/Services/ProcessCore.Host.cs` pour appeler les primitives SDK de requête, lecture et lancement ; conserver le codec, les délais et les erreurs de ce module ainsi que ses finally de libération.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/Common/Services/ProcessCore.Protocol.cs` et `src/GWGUI.Emulation.Coleco/Common/Services/ProcessCore.Host.cs` pour appeler les primitives SDK de requête, lecture et lancement ; conserver le codec, les délais et les erreurs de ce module ainsi que ses finally de libération.
  - [x] Modifier `src/GWGUI.Emulation.Epoch/Common/Services/ProcessCore.Protocol.cs` et `src/GWGUI.Emulation.Epoch/Common/Services/ProcessCore.Host.cs` pour appeler les primitives SDK de requête, lecture et lancement ; conserver le codec, les délais et les erreurs de ce module ainsi que ses finally de libération.
  - [x] Modifier `src/GWGUI.Emulation.Fairchild/Common/Services/ProcessCore.Protocol.cs` et `src/GWGUI.Emulation.Fairchild/Common/Services/ProcessCore.Host.cs` pour appeler les primitives SDK de requête, lecture et lancement ; conserver le codec, les délais et les erreurs de ce module ainsi que ses finally de libération.
  - [x] Modifier `src/GWGUI.Emulation.Mattel/Common/Services/ProcessCore.Protocol.cs` et `src/GWGUI.Emulation.Mattel/Common/Services/ProcessCore.Host.cs` pour appeler les primitives SDK de requête, lecture et lancement ; conserver le codec, les délais et les erreurs de ce module ainsi que ses finally de libération.
  - [x] Modifier `src/GWGUI.Emulation.Panasonic/Common/Services/ProcessCore.Protocol.cs` et `src/GWGUI.Emulation.Panasonic/Common/Services/ProcessCore.Host.cs` pour appeler les primitives SDK de requête, lecture et lancement ; conserver le codec, les délais et les erreurs de ce module ainsi que ses finally de libération.
  - [x] Modifier `src/GWGUI.Emulation.Philips/Common/Services/ProcessCore.Protocol.cs` et `src/GWGUI.Emulation.Philips/Common/Services/ProcessCore.Host.cs` pour appeler les primitives SDK de requête, lecture et lancement ; conserver le codec, les délais et les erreurs de ce module ainsi que ses finally de libération.
  - [x] Modifier `src/GWGUI.Emulation.Sharp/Common/Services/ProcessCore.Protocol.cs` et `src/GWGUI.Emulation.Sharp/Common/Services/ProcessCore.Host.cs` pour appeler les primitives SDK de requête, lecture et lancement ; conserver le codec, les délais et les erreurs de ce module ainsi que ses finally de libération.
  - [x] Modifier `src/GWGUI.Emulation.Sinclair/Common/Services/ProcessCore.Protocol.cs` et `src/GWGUI.Emulation.Sinclair/Common/Services/ProcessCore.Host.cs` pour appeler les primitives SDK de requête, lecture et lancement ; conserver le codec, les délais et les erreurs de ce module ainsi que ses finally de libération.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Common/Services/ProcessCore.Protocol.cs` et `src/GWGUI.Emulation.Snk/Common/Services/ProcessCore.Host.cs` pour appeler les primitives SDK de requête, lecture et lancement ; conserver le codec, les délais et les erreurs de ce module ainsi que ses finally de libération.
  - [x] Modifier `src/GWGUI.Emulation.Thomson/Common/Services/ProcessCore.Protocol.cs` et `src/GWGUI.Emulation.Thomson/Common/Services/ProcessCore.Host.cs` pour appeler les primitives SDK de requête, lecture et lancement ; conserver le codec, les délais et les erreurs de ce module ainsi que ses finally de libération.
  - [x] Modifier `src/GWGUI.Emulation.Watara/Common/Services/ProcessCore.Protocol.cs` et `src/GWGUI.Emulation.Watara/Common/Services/ProcessCore.Host.cs` pour appeler les primitives SDK de requête, lecture et lancement ; conserver le codec, les délais et les erreurs de ce module ainsi que ses finally de libération.
  - [x] Modifier `src/GWGUI.Emulation.Wenquxing/Common/Services/ProcessCore.Protocol.cs` et `src/GWGUI.Emulation.Wenquxing/Common/Services/ProcessCore.Host.cs` pour appeler les primitives SDK de requête, lecture et lancement ; conserver le codec, les délais et les erreurs de ce module ainsi que ses finally de libération.
  - [x] Modifier les méthodes ReadResponseAsync des ProcessCore.cs Amstrad, Commodore, NEC, Nintendo, Sega et Sony (Common, Pcsx2, Ppsspp), dont les chemins exacts sont inventoriés dans clone-groups.json, pour appeler le lecteur SDK avec leur propre délai et message de longueur invalide.
    - [x] Modifier `src/GWGUI.Emulation.Amstrad/Emulators/Common/Services/ProcessCore.cs` : déléguer ReadResponseAsync au SDK avec son délai et son message de longueur invalide.
    - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Ppsspp/Services/ProcessCore.cs` : déléguer ReadResponseAsync au SDK avec son délai et son message de longueur invalide.
    - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Pcsx2/Services/ProcessCore.cs` : déléguer ReadResponseAsync au SDK avec son délai et son message de longueur invalide.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/Common/Interop/Services/ProcessCore.cs` : déléguer ReadResponseAsync au SDK avec son délai et son message de longueur invalide.
    - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Services/ProcessCore.cs` : déléguer ReadResponseAsync au SDK avec son délai et son message de longueur invalide.
    - [x] Modifier `src/GWGUI.Emulation.Commodore/Emulators/Common/Interop/Services/ProcessCore.cs` : déléguer ReadResponseAsync au SDK avec son délai et son message de longueur invalide.
    - [x] Modifier `src/GWGUI.Emulation.Nintendo/Emulators/Common/Interop/Services/ProcessCore.cs` : déléguer ReadResponseAsync au SDK avec son délai et son message de longueur invalide.
    - [x] Modifier `src/GWGUI.Emulation.Nec/Emulators/Common/Interop/Services/ProcessCore.cs` : déléguer ReadResponseAsync au SDK avec son délai et son message de longueur invalide.
- [x] Réutiliser les primitives audio et temporisation antérieures dans les nouveaux modules.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec la comparaison des anciens `AtariAudioOutputFunctions`, `AtariFrameTimer` et des copies `AudioFunctions.ApplyVolume`, `AudioOutputController.Write`, `FrameTimer.WaitUntil`, en distinguant les constantes matérielles de l’algorithme identique.
  - [x] Créer `src/GWGUI.Emulation/Functions/EmulationAudioFunctions.cs` depuis AtariAudioOutputFunctions antérieur, avec NormalizeVolume et ApplyVolume pour PCM 16 bits.
  - [x] Créer `src/GWGUI.Emulation/Services/EmulationFrameTimer.cs` depuis AtariFrameTimer antérieur, en adaptant son constructeur pour libérer les handles en finally après tout échec et en conservant le handle de cancellation emprunté.
  - [x] Créer `src/GWGUI.Emulation/Services/EmulationAudioOutputController.cs` depuis AtariAudioOutputController antérieur ; adapter la propriété des sorties, les finally, le volume commun et un point BeforeStart pour les besoins déjà existants de SNK.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/EmulationGlobalUsings.cs` avec un alias FrameTimer vers le SDK ; supprimer `src/GWGUI.Emulation.Acorn/Common/Services/FrameTimer.cs` après raccordement.
  - [x] Modifier `src/GWGUI.Emulation.Apple/EmulationGlobalUsings.cs` avec un alias FrameTimer vers le SDK ; supprimer `src/GWGUI.Emulation.Apple/Common/Services/FrameTimer.cs` après raccordement.
  - [x] Modifier `src/GWGUI.Emulation.Atari/EmulationGlobalUsings.cs` avec un alias FrameTimer vers le SDK ; supprimer `src/GWGUI.Emulation.Atari/Emulators/Common/Interop/Services/FrameTimer.cs` après raccordement.
  - [x] Modifier `src/GWGUI.Emulation.Bandai/EmulationGlobalUsings.cs` avec un alias FrameTimer vers le SDK ; supprimer `src/GWGUI.Emulation.Bandai/Common/Services/FrameTimer.cs` après raccordement.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/EmulationGlobalUsings.cs` avec un alias FrameTimer vers le SDK ; supprimer `src/GWGUI.Emulation.Coleco/Common/Services/FrameTimer.cs` après raccordement.
  - [x] Modifier `src/GWGUI.Emulation.Epoch/EmulationGlobalUsings.cs` avec un alias FrameTimer vers le SDK ; supprimer `src/GWGUI.Emulation.Epoch/Common/Services/FrameTimer.cs` après raccordement.
  - [x] Modifier `src/GWGUI.Emulation.Fairchild/EmulationGlobalUsings.cs` avec un alias FrameTimer vers le SDK ; supprimer `src/GWGUI.Emulation.Fairchild/Common/Services/FrameTimer.cs` après raccordement.
  - [x] Modifier `src/GWGUI.Emulation.Mattel/EmulationGlobalUsings.cs` avec un alias FrameTimer vers le SDK ; supprimer `src/GWGUI.Emulation.Mattel/Common/Services/FrameTimer.cs` après raccordement.
  - [x] Modifier `src/GWGUI.Emulation.Panasonic/EmulationGlobalUsings.cs` avec un alias FrameTimer vers le SDK ; supprimer `src/GWGUI.Emulation.Panasonic/Common/Services/FrameTimer.cs` après raccordement.
  - [x] Modifier `src/GWGUI.Emulation.Philips/EmulationGlobalUsings.cs` avec un alias FrameTimer vers le SDK ; supprimer `src/GWGUI.Emulation.Philips/Common/Services/FrameTimer.cs` après raccordement.
  - [x] Modifier `src/GWGUI.Emulation.Sharp/EmulationGlobalUsings.cs` avec un alias FrameTimer vers le SDK ; supprimer `src/GWGUI.Emulation.Sharp/Common/Services/FrameTimer.cs` après raccordement.
  - [x] Modifier `src/GWGUI.Emulation.Sinclair/EmulationGlobalUsings.cs` avec un alias FrameTimer vers le SDK ; supprimer `src/GWGUI.Emulation.Sinclair/Common/Services/FrameTimer.cs` après raccordement.
  - [x] Modifier `src/GWGUI.Emulation.Snk/EmulationGlobalUsings.cs` avec un alias FrameTimer vers le SDK ; supprimer `src/GWGUI.Emulation.Snk/Common/Services/FrameTimer.cs` après raccordement.
  - [x] Modifier `src/GWGUI.Emulation.Thomson/EmulationGlobalUsings.cs` avec un alias FrameTimer vers le SDK ; supprimer `src/GWGUI.Emulation.Thomson/Common/Services/FrameTimer.cs` après raccordement.
  - [x] Modifier `src/GWGUI.Emulation.Watara/EmulationGlobalUsings.cs` avec un alias FrameTimer vers le SDK ; supprimer `src/GWGUI.Emulation.Watara/Common/Services/FrameTimer.cs` après raccordement.
  - [x] Modifier `src/GWGUI.Emulation.Wenquxing/EmulationGlobalUsings.cs` avec un alias FrameTimer vers le SDK ; supprimer `src/GWGUI.Emulation.Wenquxing/Common/Services/FrameTimer.cs` après raccordement.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Common/Services/AudioOutputController.cs` en adaptateur du contrôleur SDK antérieur ; conserver seulement les propriétés de tampon et latence propres au module, sans recopier Write ni la gestion des sorties.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Common/Services/AudioOutputController.cs` en adaptateur du contrôleur SDK antérieur ; conserver seulement les propriétés de tampon et latence propres au module, sans recopier Write ni la gestion des sorties.
  - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Services/AudioOutputController.cs` en adaptateur du contrôleur SDK antérieur ; conserver seulement les propriétés de tampon et latence propres au module, sans recopier Write ni la gestion des sorties.
  - [x] Modifier `src/GWGUI.Emulation.Bandai/Common/Services/AudioOutputController.cs` en adaptateur du contrôleur SDK antérieur ; conserver seulement les propriétés de tampon et latence propres au module, sans recopier Write ni la gestion des sorties.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/Common/Services/AudioOutputController.cs` en adaptateur du contrôleur SDK antérieur ; conserver seulement les propriétés de tampon et latence propres au module, sans recopier Write ni la gestion des sorties.
  - [x] Modifier `src/GWGUI.Emulation.Epoch/Common/Services/AudioOutputController.cs` en adaptateur du contrôleur SDK antérieur ; conserver seulement les propriétés de tampon et latence propres au module, sans recopier Write ni la gestion des sorties.
  - [x] Modifier `src/GWGUI.Emulation.Fairchild/Common/Services/AudioOutputController.cs` en adaptateur du contrôleur SDK antérieur ; conserver seulement les propriétés de tampon et latence propres au module, sans recopier Write ni la gestion des sorties.
  - [x] Modifier `src/GWGUI.Emulation.Mattel/Common/Services/AudioOutputController.cs` en adaptateur du contrôleur SDK antérieur ; conserver seulement les propriétés de tampon et latence propres au module, sans recopier Write ni la gestion des sorties.
  - [x] Modifier `src/GWGUI.Emulation.Panasonic/Common/Services/AudioOutputController.cs` en adaptateur du contrôleur SDK antérieur ; conserver seulement les propriétés de tampon et latence propres au module, sans recopier Write ni la gestion des sorties.
  - [x] Modifier `src/GWGUI.Emulation.Philips/Common/Services/AudioOutputController.cs` en adaptateur du contrôleur SDK antérieur ; conserver seulement les propriétés de tampon et latence propres au module, sans recopier Write ni la gestion des sorties.
  - [x] Modifier `src/GWGUI.Emulation.Sharp/Common/Services/AudioOutputController.cs` en adaptateur du contrôleur SDK antérieur ; conserver seulement les propriétés de tampon et latence propres au module, sans recopier Write ni la gestion des sorties.
  - [x] Modifier `src/GWGUI.Emulation.Sinclair/Common/Services/AudioOutputController.cs` en adaptateur du contrôleur SDK antérieur ; conserver seulement les propriétés de tampon et latence propres au module, sans recopier Write ni la gestion des sorties.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Common/Services/AudioOutputController.cs` en adaptateur du contrôleur SDK antérieur ; conserver seulement les propriétés de tampon et latence propres au module, sans recopier Write ni la gestion des sorties.
  - [x] Modifier `src/GWGUI.Emulation.Thomson/Common/Services/AudioOutputController.cs` en adaptateur du contrôleur SDK antérieur ; conserver seulement les propriétés de tampon et latence propres au module, sans recopier Write ni la gestion des sorties.
  - [x] Modifier `src/GWGUI.Emulation.Watara/Common/Services/AudioOutputController.cs` en adaptateur du contrôleur SDK antérieur ; conserver seulement les propriétés de tampon et latence propres au module, sans recopier Write ni la gestion des sorties.
  - [x] Modifier `src/GWGUI.Emulation.Wenquxing/Common/Services/AudioOutputController.cs` en adaptateur du contrôleur SDK antérieur ; conserver seulement les propriétés de tampon et latence propres au module, sans recopier Write ni la gestion des sorties.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Common/Machines/Common/Functions/AudioFunctions.cs` pour déléguer NormalizeVolume/ApplyVolume au SDK et conserver les autres règles audio matérielles.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Common/Functions/AudioFunctions.cs` pour déléguer NormalizeVolume/ApplyVolume au SDK et conserver les autres règles audio matérielles.
  - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/Common/Functions/AudioFunctions.cs` pour déléguer NormalizeVolume/ApplyVolume au SDK et conserver les autres règles audio matérielles.
  - [x] Modifier `src/GWGUI.Emulation.Bandai/Common/Functions/AudioFunctions.cs` pour déléguer NormalizeVolume/ApplyVolume au SDK et conserver les autres règles audio matérielles.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/Common/Functions/AudioFunctions.cs` pour déléguer NormalizeVolume/ApplyVolume au SDK et conserver les autres règles audio matérielles.
  - [x] Modifier `src/GWGUI.Emulation.Epoch/Common/Functions/AudioFunctions.cs` pour déléguer NormalizeVolume/ApplyVolume au SDK et conserver les autres règles audio matérielles.
  - [x] Modifier `src/GWGUI.Emulation.Fairchild/Common/Functions/AudioFunctions.cs` pour déléguer NormalizeVolume/ApplyVolume au SDK et conserver les autres règles audio matérielles.
  - [x] Modifier `src/GWGUI.Emulation.Mattel/Common/Machines/Common/Functions/AudioFunctions.cs` pour déléguer NormalizeVolume/ApplyVolume au SDK et conserver les autres règles audio matérielles.
  - [x] Modifier `src/GWGUI.Emulation.Panasonic/Common/Machines/Common/Functions/AudioFunctions.cs` pour déléguer NormalizeVolume/ApplyVolume au SDK et conserver les autres règles audio matérielles.
  - [x] Modifier `src/GWGUI.Emulation.Philips/Common/Machines/Common/Functions/AudioFunctions.cs` pour déléguer NormalizeVolume/ApplyVolume au SDK et conserver les autres règles audio matérielles.
  - [x] Modifier `src/GWGUI.Emulation.Sharp/Common/Machines/Common/Functions/AudioFunctions.cs` pour déléguer NormalizeVolume/ApplyVolume au SDK et conserver les autres règles audio matérielles.
  - [x] Modifier `src/GWGUI.Emulation.Sinclair/Common/Machines/Common/Functions/AudioFunctions.cs` pour déléguer NormalizeVolume/ApplyVolume au SDK et conserver les autres règles audio matérielles.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Common/Machines/Common/Functions/AudioFunctions.cs` pour déléguer NormalizeVolume/ApplyVolume au SDK et conserver les autres règles audio matérielles.
  - [x] Modifier `src/GWGUI.Emulation.Thomson/Common/Machines/Common/Functions/AudioFunctions.cs` pour déléguer NormalizeVolume/ApplyVolume au SDK et conserver les autres règles audio matérielles.
  - [x] Modifier `src/GWGUI.Emulation.Watara/Common/Machines/Common/Functions/AudioFunctions.cs` pour déléguer NormalizeVolume/ApplyVolume au SDK et conserver les autres règles audio matérielles.
  - [x] Modifier `src/GWGUI.Emulation.Wenquxing/Common/Machines/Common/Functions/AudioFunctions.cs` pour déléguer NormalizeVolume/ApplyVolume au SDK et conserver les autres règles audio matérielles.
  - [x] Modifier `tests/GWGUI.Tests/Emulation/Audio/AudioBufferScenarios.cs` pour vérifier que les sorties reçues restent sous la responsabilité du test appelant et que celles produites par une factory sont libérées par le contrôleur, sans artefact externe.
- [x] Réutiliser l’installation et le diagnostic des cœurs au lieu des implémentations recopiées.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec les prédécesseurs Atari/Amstrad des copies de `CoreFunctions.Release` (`DownloadAsync`, extraction), `CoreFunctions.Diagnostics` (architecture/exports) et des lecteurs de manifestes ; vérifier ce que le SDK fournissait déjà avant chaque copie.
  - [x] Modifier `src/GWGUI.Emulation/Functions/ConfigurationFileAccessFunctions.cs` pour reprendre la séquence atomique antérieure (écriture temporaire, remplacement choisi par le propriétaire, suppression en finally), sans modifier ReplaceFile ni son verrouillage existant.
  - [x] Créer `src/GWGUI.Emulation/Functions/EmulationCoreReleaseFunctions.cs` depuis AtariCoreReleaseFunctions antérieur : téléchargement, extraction ZIP, JSON et remplacement de bibliothèque ; conserver les modèles de progression et messages dans les adaptateurs.
  - [x] Créer `src/GWGUI.Emulation/Functions/EmulationCoreDiagnosticFunctions.cs` depuis AtariCoreDiagnosticFunctions antérieur : SHA256, architecture PE, version et exports ; ajouter explicitement System.IO.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/Common/Functions/CoreFunctions.Release.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/Common/Interop/Functions/CoreFunctions.Diagnostics.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Emulators/Common/Functions/CoreFunctions.Release.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Emulators/Common/Interop/Functions/CoreFunctions.Diagnostics.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/Common/Functions/CoreFunctions.Release.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/Common/Interop/Functions/CoreFunctions.Diagnostics.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Bandai/Emulators/Common/Functions/CoreFunctions.Release.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Bandai/Emulators/Common/Interop/Functions/CoreFunctions.Diagnostics.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/Emulators/Common/Functions/CoreFunctions.Release.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/Emulators/Common/Interop/Functions/CoreFunctions.Diagnostics.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Epoch/Emulators/Common/Functions/CoreFunctions.Release.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Epoch/Emulators/Common/Interop/Functions/CoreFunctions.Diagnostics.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Fairchild/Emulators/Common/Functions/CoreFunctions.Release.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Fairchild/Emulators/Common/Interop/Functions/CoreFunctions.Diagnostics.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Mattel/Emulators/Common/Functions/CoreFunctions.Release.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Mattel/Emulators/Common/Interop/Functions/CoreFunctions.Diagnostics.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Nec/Emulators/Common/Interop/Functions/CoreManifestFunctions.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Emulators/Common/Interop/Functions/CoreManifestFunctions.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Panasonic/Emulators/Opera/Functions/CoreFunctions.Release.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Panasonic/Emulators/Opera/Interop/Functions/CoreFunctions.Diagnostics.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Philips/Emulators/Common/Functions/CoreFunctions.Release.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Philips/Emulators/Common/Interop/Functions/CoreFunctions.Diagnostics.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/Common/Interop/Functions/CoreManifestFunctions.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Sharp/Emulators/Common/Functions/CoreFunctions.Release.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Sharp/Emulators/Common/Interop/Functions/CoreFunctions.Diagnostics.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Sinclair/Emulators/Common/Functions/CoreFunctions.Release.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Sinclair/Emulators/Common/Interop/Functions/CoreFunctions.Diagnostics.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Emulators/Common/Functions/CoreFunctions.Release.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Emulators/Common/Interop/Functions/CoreFunctions.Diagnostics.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Functions/CoreManifestFunctions.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Thomson/Emulators/Theodore/Functions/CoreFunctions.Release.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Thomson/Emulators/Theodore/Interop/Functions/CoreFunctions.Diagnostics.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Watara/Emulators/Potator/Functions/CoreFunctions.Release.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Watara/Emulators/Potator/Interop/Functions/CoreFunctions.Diagnostics.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Wenquxing/Emulators/Wqxemu/Functions/CoreFunctions.Release.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Wenquxing/Emulators/Wqxemu/Interop/Functions/CoreFunctions.Diagnostics.cs` pour appeler les mécanismes SDK antérieurs extraits et conserver ses catalogues, types de manifestes, noms de fichiers et erreurs.
- [x] Réutiliser la mécanique de persistance des configurations avant les copies intermodules.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec l’ancienne `AtariConfigurationStoreFunctions.WriteDocumentAtomicallyAsync` et les seize exemplaires actuels, en conservant les documents propres à chaque module et le verrouillage commun déjà existant.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour appeler ConfigurationFileAccessFunctions.WriteAtomicallyAsync en conservant le nom temporaire unique, JsonOptions, WriteThrough, FlushAsync et le ReplaceFile antérieur.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour appeler ConfigurationFileAccessFunctions.WriteAtomicallyAsync en conservant le nom temporaire unique, JsonOptions, WriteThrough, FlushAsync et le ReplaceFile antérieur.
  - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour appeler ConfigurationFileAccessFunctions.WriteAtomicallyAsync en conservant le nom temporaire unique, JsonOptions, WriteThrough, FlushAsync et le ReplaceFile antérieur.
  - [x] Modifier `src/GWGUI.Emulation.Bandai/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour appeler ConfigurationFileAccessFunctions.WriteAtomicallyAsync en conservant le nom temporaire unique, JsonOptions, WriteThrough, FlushAsync et le ReplaceFile antérieur.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour appeler ConfigurationFileAccessFunctions.WriteAtomicallyAsync en conservant le nom temporaire unique, JsonOptions, WriteThrough, FlushAsync et le ReplaceFile antérieur.
  - [x] Modifier `src/GWGUI.Emulation.Epoch/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour appeler ConfigurationFileAccessFunctions.WriteAtomicallyAsync en conservant le nom temporaire unique, JsonOptions, WriteThrough, FlushAsync et le ReplaceFile antérieur.
  - [x] Modifier `src/GWGUI.Emulation.Fairchild/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour appeler ConfigurationFileAccessFunctions.WriteAtomicallyAsync en conservant le nom temporaire unique, JsonOptions, WriteThrough, FlushAsync et le ReplaceFile antérieur.
  - [x] Modifier `src/GWGUI.Emulation.Mattel/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour appeler ConfigurationFileAccessFunctions.WriteAtomicallyAsync en conservant le nom temporaire unique, JsonOptions, WriteThrough, FlushAsync et le ReplaceFile antérieur.
  - [x] Modifier `src/GWGUI.Emulation.Panasonic/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour appeler ConfigurationFileAccessFunctions.WriteAtomicallyAsync en conservant le nom temporaire unique, JsonOptions, WriteThrough, FlushAsync et le ReplaceFile antérieur.
  - [x] Modifier `src/GWGUI.Emulation.Philips/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour appeler ConfigurationFileAccessFunctions.WriteAtomicallyAsync en conservant le nom temporaire unique, JsonOptions, WriteThrough, FlushAsync et le ReplaceFile antérieur.
  - [x] Modifier `src/GWGUI.Emulation.Sharp/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour appeler ConfigurationFileAccessFunctions.WriteAtomicallyAsync en conservant le nom temporaire unique, JsonOptions, WriteThrough, FlushAsync et le ReplaceFile antérieur.
  - [x] Modifier `src/GWGUI.Emulation.Sinclair/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour appeler ConfigurationFileAccessFunctions.WriteAtomicallyAsync en conservant le nom temporaire unique, JsonOptions, WriteThrough, FlushAsync et le ReplaceFile antérieur.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour appeler ConfigurationFileAccessFunctions.WriteAtomicallyAsync en conservant le nom temporaire unique, JsonOptions, WriteThrough, FlushAsync et le ReplaceFile antérieur.
  - [x] Modifier `src/GWGUI.Emulation.Thomson/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour appeler ConfigurationFileAccessFunctions.WriteAtomicallyAsync en conservant le nom temporaire unique, JsonOptions, WriteThrough, FlushAsync et le ReplaceFile antérieur.
  - [x] Modifier `src/GWGUI.Emulation.Watara/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour appeler ConfigurationFileAccessFunctions.WriteAtomicallyAsync en conservant le nom temporaire unique, JsonOptions, WriteThrough, FlushAsync et le ReplaceFile antérieur.
  - [x] Modifier `src/GWGUI.Emulation.Wenquxing/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour appeler ConfigurationFileAccessFunctions.WriteAtomicallyAsync en conservant le nom temporaire unique, JsonOptions, WriteThrough, FlushAsync et le ReplaceFile antérieur.

Ces groupes décrivent les copies d’algorithmes effectivement observées et leur
prédécesseur ; la création d’un moteur unique ou d’une dépendance entre modules
n’est pas présupposée. Les différences de domaine doivent rester chez leur propriétaire.


- [x] Réutiliser le classement des options existant dans Sharp.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec les prédécesseurs de `Px68k/Functions/SettingsFunctions.cs` et `X1/Functions/SettingsFunctions.cs` : `GetOptions` et `Create` sont identiques et appellent déjà `Common.Machines.Common.Functions.SettingsDescriptionFunctions.Native`. Définir l’adaptation du classement antérieur à partir des catalogues propres aux deux cœurs.
  - [x] Modifier `src/GWGUI.Emulation/Functions/EmulationCoreOptionFunctions.cs` avec le calcul des valeurs et le groupement de blocs extraits des sources initiales Sharp ; modifier `src/GWGUI.Emulation.Sharp/Common/Machines/Common/Functions/SettingsFunctions.cs` pour adapter son Native antérieur avec les catalogues et ressources du cœur.
  - [x] Modifier `src/GWGUI.Emulation.Sharp/Emulators/Px68k/Functions/SettingsFunctions.cs` et `src/GWGUI.Emulation.Sharp/Emulators/X1/Functions/SettingsFunctions.cs` pour déléguer au Common existant au lieu de recalculer et regrouper les options.
- [x] Réutiliser le classement des options existant dans Sinclair.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec les prédécesseurs de `EightyOne/Functions/SettingsFunctions.cs` et `Fuse/Functions/SettingsFunctions.cs` : mêmes `GetOptions` et `Create`, mêmes points de délégation Common ; conserver les catalogues et valeurs natives distincts.
  - [x] Modifier `src/GWGUI.Emulation.Sinclair/Common/Machines/Common/Functions/SettingsFunctions.cs` pour adapter son Native antérieur aux primitives SDK existantes, en conservant IsAvailable et les ressources Sinclair.
  - [x] Modifier `src/GWGUI.Emulation.Sinclair/Emulators/EightyOne/Functions/SettingsFunctions.cs` et `src/GWGUI.Emulation.Sinclair/Emulators/Fuse/Functions/SettingsFunctions.cs` pour transmettre leurs catalogues, défauts et IsAvailable au Common existant.
- [x] Réutiliser la description des lecteurs NEC au lieu de recopier sa construction.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec l’antériorité de `NP2Kai/Functions/StorageFunctions.cs`, `NekoProjectII/Functions/StorageFunctions.cs` et `Quasi88/Functions/StorageFunctions.cs` : même méthode `Devices`, mais constantes du cœur séparées. Définir la reprise de l’ancien constructeur de description avec ses données explicites.
  - [x] Modifier `src/GWGUI.Emulation.Nec/Emulators/Common/Interop/Functions/CoreStorageSettingsFunctions.cs` avec la construction ComputerDevices extraite des sources initiales 2a9191065 ; modifier les trois `Emulators/NP2Kai/Functions/StorageFunctions.cs`, `Emulators/NekoProjectII/Functions/StorageFunctions.cs` et `Emulators/Quasi88/Functions/StorageFunctions.cs` du module NEC pour transmettre leurs indices, formats et préfixes propres.

Les conversions `OptionValueFunctions.ToNative` des deux cœurs Beetle PSX ont aussi
un corps identique pour certaines énumérations. Elles référencent toutefois des types
et constantes natives propres aux deux adaptateurs : ce sont des correspondances de
données à examiner, pas une preuve suffisante pour déplacer les enums d’un cœur dans
un autre. Elles restent des candidates, sans correction arbitraire.


## Antériorités établies pour la reprise des corrections

| Groupe | Existant à reprendre | Duplication / extension ultérieure |
| --- | --- | --- |
| Champs UI / stockage | `EmulationModuleSettingsSection.Fields.cs` présent dès `cf174afc6` (16 septembre), et sa logique de sliders avant extraction | `StorageConfigurationFieldFunctions.cs` ajouté par `bdff8d10c` (13 octobre). Repartir du renderer antérieur de Fields, en séparant construction et sauvegarde ; ne pas substituer le renderer stockage récent à l’ancien. |
| INI PCEE2 | `NetworkFunctions.cs`, `403ce5836` | `MemoryCardFunctions.cs`, `329fdaec4`. Adapter d’abord le setter antérieur à une section explicite, puis l’appeler pour les cartes ; ne pas remplacer le réseau par le setter récent. |
| Callbacks NEC | `BeetlePcfx/Services/ExternalHostCallbacks.Input.cs`, chemin créé par `62fe7071b` (28 septembre), algorithme suivi jusqu’à `cf174afc6` | `Common/Interop/Services/ExternalHostCallbacks.Input.cs`, `2a9191065` (7 octobre). La reprise doit partir du backend PC-FX plus ancien et de ses prédécesseurs, pas traiter le nouveau Common comme vérité historique. |
| NEC PC-98 | Deux fichiers ContentFunctions ajoutés ensemble par `2a9191065` | Aucun ordre antérieur établi entre ces deux copies. Préserver les constantes propres à chaque adaptateur et rechercher la préparation de contenu qui existait avant ce commit avant d’en choisir un comme base. |
| SNK MAME/FBA | Les variantes auditées sont ajoutées ensemble par `a1f460542` (11 octobre) | Pas d’antériorité chronologique entre elles : ne pas déclarer automatiquement Mame2016 comme base. La reprise doit comparer la préparation de contenu et de firmware déjà existante dans le projet avant `a1f460542`. |
| Sharp | Deux renderers ajoutés ensemble par `4d2df31ae` (13 octobre) ; ils délèguent déjà les champs à l’ancien Common | Reprendre le classement/options Common existant avant ce commit et passer les catalogues du cœur en données. |
| Sinclair | Deux renderers ajoutés ensemble par `033f73ce2` (13 octobre) ; même délégation Common | Même reprise du Common antérieur, sans remplacer EightyOne par la copie Fuse ni l’inverse. |
| Infrastructure générique | Avant Amstrad : `AtariProcessCore.cs`, `AtariFrameTimer.cs`, `AtariAudioOutputFunctions.cs`, `AtariConfigurationStoreFunctions.cs`, `AtariCoreReleaseFunctions.cs` | Copies dans les nouveaux Common/Services des modules de la période. Comparer aussi le SDK disponible au moment de chaque ajout et conserver les paramètres/contrats du module. |
| Stockage géré | `StorageDialogUi` et l’éditeur général de champs existaient avant le nouveau dialog | `ManagedStorageConfigurationDialog.cs`, `329fdaec4`, réutilise déjà StorageDialogUi mais porte une clé Sony en dur. Adapter l’ancien contrat d’aide du périphérique et son passage à la fenêtre ; ne pas créer une nouvelle hiérarchie de fenêtres. |

Les tâches de métadonnées ci-dessus restent ouvertes lorsque le prédécesseur précis
n’est pas établi ; aucune correction de code n’est considérée réalisée par ce tableau.


- [x] Reprendre l’hôte d’options natif antérieur au lieu des copies de ses algorithmes.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec l’origine des familles `CoreOptionHost`, `OptionCallbackHost` et des fonctions de registration/visibilité/lecture des options ; comparer les callbacks et catalogues propres au cœur aux routines génériques, à partir des implémentations Atari/Amstrad antérieures aux nouveaux modules.
  - [x] Modifier `src/GWGUI.Emulation/Functions/EmulationCoreOptionFunctions.cs` en classe partial ; créer `src/GWGUI.Emulation/Functions/EmulationCoreOptionFunctions.Native.cs` depuis AtariCoreOptionFunctions antérieur pour les lecteurs complets Legacy/V1/V2, catégories et opérations de callbacks, avec factories des contrats locaux.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/Common/Interop/Functions/CoreFunctions.Options.cs` pour appeler les lecteurs SDK complets avec ses contrats natifs et supprimer les parcours de pointeurs recopiés.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Emulators/Common/Interop/Functions/CoreFunctions.Options.cs` pour appeler les lecteurs SDK complets avec ses contrats natifs et supprimer les parcours de pointeurs recopiés.
  - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/Common/Interop/Functions/CoreFunctions.Options.cs` pour appeler les lecteurs SDK complets avec ses contrats natifs et supprimer les parcours de pointeurs recopiés.
  - [x] Modifier `src/GWGUI.Emulation.Bandai/Emulators/Common/Interop/Functions/CoreFunctions.Options.cs` pour appeler les lecteurs SDK complets avec ses contrats natifs et supprimer les parcours de pointeurs recopiés.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/Emulators/Common/Interop/Functions/CoreFunctions.Options.cs` pour appeler les lecteurs SDK complets avec ses contrats natifs et supprimer les parcours de pointeurs recopiés.
  - [x] Modifier `src/GWGUI.Emulation.Epoch/Emulators/Common/Interop/Functions/CoreFunctions.Options.cs` pour appeler les lecteurs SDK complets avec ses contrats natifs et supprimer les parcours de pointeurs recopiés.
  - [x] Modifier `src/GWGUI.Emulation.Fairchild/Emulators/Common/Interop/Functions/CoreFunctions.Options.cs` pour appeler les lecteurs SDK complets avec ses contrats natifs et supprimer les parcours de pointeurs recopiés.
  - [x] Modifier `src/GWGUI.Emulation.Mattel/Emulators/Common/Interop/Functions/CoreFunctions.Options.cs` pour appeler les lecteurs SDK complets avec ses contrats natifs et supprimer les parcours de pointeurs recopiés.
  - [x] Modifier `src/GWGUI.Emulation.Philips/Emulators/Common/Interop/Functions/CoreFunctions.Options.cs` pour appeler les lecteurs SDK complets avec ses contrats natifs et supprimer les parcours de pointeurs recopiés.
  - [x] Modifier `src/GWGUI.Emulation.Sharp/Emulators/Common/Interop/Functions/CoreFunctions.Options.cs` pour appeler les lecteurs SDK complets avec ses contrats natifs et supprimer les parcours de pointeurs recopiés.
  - [x] Modifier `src/GWGUI.Emulation.Sinclair/Emulators/Common/Interop/Functions/CoreFunctions.Options.cs` pour appeler les lecteurs SDK complets avec ses contrats natifs et supprimer les parcours de pointeurs recopiés.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Emulators/Common/Interop/Functions/CoreFunctions.Options.cs` pour appeler les lecteurs SDK complets avec ses contrats natifs et supprimer les parcours de pointeurs recopiés.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/Common/Interop/Services/CoreOptionHost.cs` pour déléguer la lecture et la visibilité des callbacks au SDK ; conserver ses valeurs typées, ses présentations et la propriété existante des chaînes ExternalCoreUtf8String.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Emulators/Common/Interop/Services/CoreOptionHost.cs` pour déléguer la lecture et la visibilité des callbacks au SDK ; conserver ses valeurs typées, ses présentations et la propriété existante des chaînes ExternalCoreUtf8String.
  - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/Common/Interop/Services/CoreOptionHost.cs` pour déléguer la lecture et la visibilité des callbacks au SDK ; conserver ses valeurs typées, ses présentations et la propriété existante des chaînes ExternalCoreUtf8String.
  - [x] Modifier `src/GWGUI.Emulation.Bandai/Emulators/Common/Interop/Services/CoreOptionHost.cs` pour déléguer la lecture et la visibilité des callbacks au SDK ; conserver ses valeurs typées, ses présentations et la propriété existante des chaînes ExternalCoreUtf8String.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/Emulators/Common/Interop/Services/CoreOptionHost.cs` pour déléguer la lecture et la visibilité des callbacks au SDK ; conserver ses valeurs typées, ses présentations et la propriété existante des chaînes ExternalCoreUtf8String.
  - [x] Modifier `src/GWGUI.Emulation.Epoch/Emulators/Common/Interop/Services/CoreOptionHost.cs` pour déléguer la lecture et la visibilité des callbacks au SDK ; conserver ses valeurs typées, ses présentations et la propriété existante des chaînes ExternalCoreUtf8String.
  - [x] Modifier `src/GWGUI.Emulation.Fairchild/Emulators/Common/Interop/Services/CoreOptionHost.cs` pour déléguer la lecture et la visibilité des callbacks au SDK ; conserver ses valeurs typées, ses présentations et la propriété existante des chaînes ExternalCoreUtf8String.
  - [x] Modifier `src/GWGUI.Emulation.Mattel/Emulators/Common/Interop/Services/CoreOptionHost.cs` pour déléguer la lecture et la visibilité des callbacks au SDK ; conserver ses valeurs typées, ses présentations et la propriété existante des chaînes ExternalCoreUtf8String.
  - [x] Modifier `src/GWGUI.Emulation.Philips/Emulators/Common/Interop/Services/CoreOptionHost.cs` pour déléguer la lecture et la visibilité des callbacks au SDK ; conserver ses valeurs typées, ses présentations et la propriété existante des chaînes ExternalCoreUtf8String.
  - [x] Modifier `src/GWGUI.Emulation.Sharp/Emulators/Common/Interop/Services/CoreOptionHost.cs` pour déléguer la lecture et la visibilité des callbacks au SDK ; conserver ses valeurs typées, ses présentations et la propriété existante des chaînes ExternalCoreUtf8String.
  - [x] Modifier `src/GWGUI.Emulation.Sinclair/Emulators/Common/Interop/Services/CoreOptionHost.cs` pour déléguer la lecture et la visibilité des callbacks au SDK ; conserver ses valeurs typées, ses présentations et la propriété existante des chaînes ExternalCoreUtf8String.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Emulators/Common/Interop/Services/CoreOptionHost.cs` pour déléguer la lecture et la visibilité des callbacks au SDK ; conserver ses valeurs typées, ses présentations et la propriété existante des chaînes ExternalCoreUtf8String.
  - [x] Modifier les `ExternalHostCallbacks.Environment.cs` Amstrad, Commodore, NEC, Nintendo et Sega pour appeler les lecteurs SDK Legacy/V2 simples, avec adaptation aux contrats locaux et sans leur substituer le lecteur complet dont les règles de valeurs par défaut diffèrent.
    - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/Common/Interop/Services/ExternalHostCallbacks.Environment.cs` : remplacer Legacy/V2 et StringAt par les lecteurs SDK simples et conserver le contrat local.
    - [x] Modifier `src/GWGUI.Emulation.Amstrad/Emulators/Common/Interop/Services/ExternalHostCallbacks.Environment.cs` : remplacer Legacy/V2 et StringAt par les lecteurs SDK simples et conserver le contrat local.
    - [x] Modifier `src/GWGUI.Emulation.Apple/Emulators/Common/Interop/Services/ExternalHostCallbacks.Environment.cs` : remplacer Legacy/V2 et StringAt par les lecteurs SDK simples et conserver le contrat local.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/Common/Interop/Services/ExternalHostCallbacks.Environment.cs` : remplacer Legacy/V2 et StringAt par les lecteurs SDK simples et conserver le contrat local.
    - [x] Modifier `src/GWGUI.Emulation.Bandai/Emulators/Common/Interop/Services/ExternalHostCallbacks.Environment.cs` : remplacer Legacy/V2 et StringAt par les lecteurs SDK simples et conserver le contrat local.
    - [x] Modifier `src/GWGUI.Emulation.Coleco/Emulators/Common/Interop/Services/ExternalHostCallbacks.Environment.cs` : remplacer Legacy/V2 et StringAt par les lecteurs SDK simples et conserver le contrat local.
    - [x] Modifier `src/GWGUI.Emulation.Commodore/Emulators/Common/Interop/Services/ExternalHostCallbacks.Environment.cs` : remplacer Legacy/V2 et StringAt par les lecteurs SDK simples et conserver le contrat local.
    - [x] Modifier `src/GWGUI.Emulation.Epoch/Emulators/Common/Interop/Services/ExternalHostCallbacks.Environment.cs` : remplacer Legacy/V2 et StringAt par les lecteurs SDK simples et conserver le contrat local.
    - [x] Modifier `src/GWGUI.Emulation.Fairchild/Emulators/Common/Interop/Services/ExternalHostCallbacks.Environment.cs` : remplacer Legacy/V2 et StringAt par les lecteurs SDK simples et conserver le contrat local.
    - [x] Modifier `src/GWGUI.Emulation.Mattel/Emulators/Common/Interop/Services/ExternalHostCallbacks.Environment.cs` : remplacer Legacy/V2 et StringAt par les lecteurs SDK simples et conserver le contrat local.
    - [x] Modifier `src/GWGUI.Emulation.Nec/Emulators/Common/Interop/Services/ExternalHostCallbacks.Environment.cs` : remplacer Legacy/V2 et StringAt par les lecteurs SDK simples et conserver le contrat local.
    - [x] Modifier `src/GWGUI.Emulation.Nintendo/Emulators/Common/Interop/Services/ExternalHostCallbacks.Environment.cs` : remplacer Legacy/V2 et StringAt par les lecteurs SDK simples et conserver le contrat local.
    - [x] Modifier `src/GWGUI.Emulation.Philips/Emulators/Common/Interop/Services/ExternalHostCallbacks.Environment.cs` : remplacer Legacy/V2 et StringAt par les lecteurs SDK simples et conserver le contrat local.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/Common/Interop/Services/ExternalHostCallbacks.Environment.cs` : remplacer Legacy/V2 et StringAt par les lecteurs SDK simples et conserver le contrat local.
    - [x] Modifier `src/GWGUI.Emulation.Sharp/Emulators/Common/Interop/Services/ExternalHostCallbacks.Environment.cs` : remplacer Legacy/V2 et StringAt par les lecteurs SDK simples et conserver le contrat local.
    - [x] Modifier `src/GWGUI.Emulation.Sinclair/Emulators/Common/Interop/Services/ExternalHostCallbacks.Environment.cs` : remplacer Legacy/V2 et StringAt par les lecteurs SDK simples et conserver le contrat local.
    - [x] Modifier `src/GWGUI.Emulation.Snk/Emulators/Common/Interop/Services/ExternalHostCallbacks.Environment.cs` : remplacer Legacy/V2 et StringAt par les lecteurs SDK simples et conserver le contrat local.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` après comparaison des OptionCallbackHost Panasonic/Thomson/Watara/Wenquxing : définir leur réutilisation en conservant leur validation stricte et leurs options masquées propres, avant modification.
  - [x] Créer `src/GWGUI.Emulation/Services/EmulationLegacyOptionHost.cs` depuis le premier OptionCallbackHost Thomson f22f8e913 ; adapter les seuls contrats de données avec des factories et utiliser ExternalCoreUtf8String existant avec nettoyage en finally.
  - [x] Modifier `src/GWGUI.Emulation.Panasonic/Emulators/Opera/Interop/Services/OptionCallbackHost.cs` en adaptateur du bloc SDK issu de Thomson, en transmettant sa création de CoreOption, son choix de valeurs et ses options masquées natives.
  - [x] Modifier `src/GWGUI.Emulation.Thomson/Emulators/Theodore/Interop/Services/OptionCallbackHost.cs` en adaptateur du bloc SDK issu de Thomson, en transmettant sa création de CoreOption, son choix de valeurs et ses options masquées natives.
  - [x] Modifier `src/GWGUI.Emulation.Watara/Emulators/Potator/Interop/Services/OptionCallbackHost.cs` en adaptateur du bloc SDK issu de Thomson, en transmettant sa création de CoreOption, son choix de valeurs et ses options masquées natives.
  - [x] Modifier `src/GWGUI.Emulation.Wenquxing/Emulators/Wqxemu/Interop/Services/OptionCallbackHost.cs` en adaptateur du bloc SDK issu de Thomson, en transmettant sa création de CoreOption, son choix de valeurs et ses options masquées natives.
  - [x] Modifier `src/GWGUI.Emulation/Functions/EmulationCoreOptionFunctions.cs` pour adapter le lecteur simple V2 antérieur au layout V1 (préfixe et description), sans modifier les règles de valeurs par défaut ; modifier `src/GWGUI.Emulation.Nintendo/Emulators/Common/Interop/Services/ExternalHostCallbacks.Environment.cs` et `src/GWGUI.Emulation.Sega/Emulators/Common/Interop/Services/ExternalHostCallbacks.Environment.cs` pour déléguer V1 et supprimer ses helpers locaux devenus inutilisés ; rétablir le filtre et les API graphiques Nintendo identifiés dans les trois affectations originales de catalogue.
- [x] Reprendre le code de sauvegarde d’état antérieur pour les mécaniques identiques.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec les prédécesseurs des copies de `StateFunctions.Saved.cs`, `StateFunctions.File.cs` et `StateStore.cs` ; conserver les signatures, formats et contrats particuliers et ne mutualiser que les mécanismes d’écriture/lecture/chemins effectivement identiques.
  - [x] Créer `src/GWGUI.Emulation/Functions/EmulationStateFileFunctions.cs` depuis les primitives AtariStateFileFunctions et AtariSavedStateFunctions antérieures : cadrage binaire, hachage et lecture paramétrés par les contrats locaux ; ajouter explicitement System.IO et conserver le stream emprunté.
  - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/Common/Functions/StateFunctions.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Amstrad/Common/Services/StateStore.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Common/Machines/Common/Functions/StateFunctions.File.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Common/Machines/Common/Functions/StateFunctions.Saved.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Machines/Common/Functions/StateFunctions.Saved.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Bandai/Common/Machines/Common/Functions/StateFunctions.File.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Bandai/Common/Machines/Common/Functions/StateFunctions.Saved.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/Common/Machines/Common/Functions/StateFunctions.File.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/Common/Machines/Common/Functions/StateFunctions.Saved.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Commodore/Common/Services/StateStore.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Epoch/Common/Machines/Common/Functions/StateFunctions.File.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Epoch/Common/Machines/Common/Functions/StateFunctions.Saved.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Fairchild/Common/Machines/Common/Functions/StateFunctions.File.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Fairchild/Common/Machines/Common/Functions/StateFunctions.Saved.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Mattel/Common/Machines/Common/Functions/StateFunctions.File.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Mattel/Common/Machines/Common/Functions/StateFunctions.Saved.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Microsoft/Common/Services/StateStore.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Nec/Common/Services/StateStore.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Services/StateStore.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Panasonic/Common/Machines/Common/Functions/StateFunctions.File.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Panasonic/Common/Machines/Common/Functions/StateFunctions.Saved.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Philips/Common/Machines/Common/Functions/StateFunctions.File.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Philips/Common/Machines/Common/Functions/StateFunctions.Saved.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Services/StateStore.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Sharp/Common/Machines/Common/Functions/StateFunctions.File.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Sharp/Common/Machines/Common/Functions/StateFunctions.Saved.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Sinclair/Common/Machines/Common/Functions/StateFunctions.File.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Sinclair/Common/Machines/Common/Functions/StateFunctions.Saved.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Common/Machines/Common/Functions/StateFunctions.File.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Common/Machines/Common/Functions/StateFunctions.Saved.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Common/Services/StateStore.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Thomson/Common/Machines/Common/Functions/StateFunctions.File.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Thomson/Common/Machines/Common/Functions/StateFunctions.Saved.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Watara/Common/Machines/Common/Functions/StateFunctions.File.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Watara/Common/Machines/Common/Functions/StateFunctions.Saved.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Wenquxing/Common/Machines/Common/Functions/StateFunctions.File.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
  - [x] Modifier `src/GWGUI.Emulation.Wenquxing/Common/Machines/Common/Functions/StateFunctions.Saved.cs` pour déléguer seulement le cadrage des octets et les hachages au SDK ; conserver ses en-têtes, sa signature, ses retries, ses tailles, son remplacement atomique et ses erreurs.
- [x] Reprendre les buffers et contextes graphiques génériques antérieurs.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec l’origine des groupes identiques de `SharedVideoWriter`, `VideoBufferSet` et `OpenGlHardwareRenderContext.*`, leurs propriétaires réels et leurs libérations ; distinguer les contraintes de processus/cœur des primitives recopiées avant de définir l’adaptation de l’ancien composant.
  - [x] Créer `src/GWGUI.Emulation/Services/EmulationVideoBufferSet.cs` depuis AtariVideoBufferSet antérieur, avec le nombre de buffers fourni par le module et le retour de chaque location même après erreur.
  - [x] Créer `src/GWGUI.Emulation/Services/EmulationSharedVideoWriter.cs` depuis AtariSharedVideoWriter antérieur, avec les constantes de nom et de capacité en paramètres et libération des vues/maps en finally ; ajouter explicitement System.IO.
  - [x] Modifier `src/GWGUI.Emulation/Functions/EmulationHostProtocolFunctions.cs` en classe partial ; créer `src/GWGUI.Emulation/Functions/EmulationHostProtocolFunctions.Video.cs` depuis les primitives vidéo AtariCoreHostFunctions antérieures, en conservant les transferts de propriété des maps et leurs finally ; ajouter explicitement System.IO.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Common/Services/SharedVideoWriter.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Common/Services/VideoBufferSet.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Common/Services/SharedVideoWriter.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Common/Services/VideoBufferSet.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Services/SharedVideoWriter.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Services/VideoBufferSet.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Bandai/Common/Services/SharedVideoWriter.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Bandai/Common/Services/VideoBufferSet.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/Common/Services/SharedVideoWriter.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/Common/Services/VideoBufferSet.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Epoch/Common/Services/SharedVideoWriter.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Epoch/Common/Services/VideoBufferSet.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Fairchild/Common/Services/SharedVideoWriter.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Fairchild/Common/Services/VideoBufferSet.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Mattel/Common/Services/SharedVideoWriter.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Mattel/Common/Services/VideoBufferSet.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Panasonic/Common/Services/SharedVideoWriter.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Philips/Common/Services/SharedVideoWriter.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Philips/Common/Services/VideoBufferSet.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Sharp/Common/Services/SharedVideoWriter.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Sharp/Common/Services/VideoBufferSet.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Sinclair/Common/Services/SharedVideoWriter.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Sinclair/Common/Services/VideoBufferSet.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Common/Services/SharedVideoWriter.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Common/Services/VideoBufferSet.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Thomson/Common/Services/SharedVideoWriter.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Watara/Common/Services/SharedVideoWriter.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Wenquxing/Common/Services/SharedVideoWriter.cs` en adaptateur du composant SDK antérieur avec ses constantes locales de buffers, noms et capacités.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Common/Functions/CoreFunctions.Host.cs` pour déléguer calcul de capacité et trames redimensionnables au SDK avec son nom hôte et sa capacité minimale.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Common/Functions/CoreFunctions.Host.cs` pour déléguer calcul de capacité et trames redimensionnables au SDK avec son nom hôte et sa capacité minimale.
  - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/Common/Functions/CoreFunctions.Host.cs` pour déléguer calcul de capacité et trames redimensionnables au SDK avec son nom hôte et sa capacité minimale.
  - [x] Modifier `src/GWGUI.Emulation.Bandai/Common/Functions/CoreFunctions.Host.cs` pour déléguer calcul de capacité et trames redimensionnables au SDK avec son nom hôte et sa capacité minimale.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/Common/Functions/CoreFunctions.Host.cs` pour déléguer calcul de capacité et trames redimensionnables au SDK avec son nom hôte et sa capacité minimale.
  - [x] Modifier `src/GWGUI.Emulation.Epoch/Common/Functions/CoreFunctions.Host.cs` pour déléguer calcul de capacité et trames redimensionnables au SDK avec son nom hôte et sa capacité minimale.
  - [x] Modifier `src/GWGUI.Emulation.Fairchild/Common/Functions/CoreFunctions.Host.cs` pour déléguer calcul de capacité et trames redimensionnables au SDK avec son nom hôte et sa capacité minimale.
  - [x] Modifier `src/GWGUI.Emulation.Mattel/Common/Functions/CoreFunctions.Host.cs` pour déléguer calcul de capacité et trames redimensionnables au SDK avec son nom hôte et sa capacité minimale.
  - [x] Modifier `src/GWGUI.Emulation.Panasonic/Common/Functions/CoreFunctions.Host.cs` pour déléguer calcul de capacité et trames redimensionnables au SDK avec son nom hôte et sa capacité minimale.
  - [x] Modifier `src/GWGUI.Emulation.Philips/Common/Functions/CoreFunctions.Host.cs` pour déléguer calcul de capacité et trames redimensionnables au SDK avec son nom hôte et sa capacité minimale.
  - [x] Modifier `src/GWGUI.Emulation.Sharp/Common/Functions/CoreFunctions.Host.cs` pour déléguer calcul de capacité et trames redimensionnables au SDK avec son nom hôte et sa capacité minimale.
  - [x] Modifier `src/GWGUI.Emulation.Sinclair/Common/Functions/CoreFunctions.Host.cs` pour déléguer calcul de capacité et trames redimensionnables au SDK avec son nom hôte et sa capacité minimale.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Common/Functions/CoreFunctions.Host.cs` pour déléguer calcul de capacité et trames redimensionnables au SDK avec son nom hôte et sa capacité minimale.
  - [x] Modifier `src/GWGUI.Emulation.Thomson/Common/Functions/CoreFunctions.Host.cs` pour déléguer calcul de capacité et trames redimensionnables au SDK avec son nom hôte et sa capacité minimale.
  - [x] Modifier `src/GWGUI.Emulation.Watara/Common/Functions/CoreFunctions.Host.cs` pour déléguer calcul de capacité et trames redimensionnables au SDK avec son nom hôte et sa capacité minimale.
  - [x] Modifier `src/GWGUI.Emulation.Wenquxing/Common/Functions/CoreFunctions.Host.cs` pour déléguer calcul de capacité et trames redimensionnables au SDK avec son nom hôte et sa capacité minimale.
  - [x] Créer `src/GWGUI.Emulation/Services/EmulationOpenGlHardwareRenderContext.cs` depuis le contexte Sega e5caa3d7a antérieur aux copies, et `.Framebuffer.cs` depuis la partie Nintendo 9af3b8e2e antérieure à sa copie Sega ; conserver leurs adaptations fonctionnelles actuelles, injecter le texte des erreurs et maintenir les finally ainsi que la vérification de destruction de la fenêtre.
  - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/Common/Interop/Services/OpenGlHardwareRenderContext.cs` en adaptateur du contexte SDK avec son fournisseur de messages ; supprimer `src/GWGUI.Emulation.Sega/Emulators/Common/Interop/Services/OpenGlHardwareRenderContext.Framebuffer.cs` après raccordement.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Emulators/Common/Interop/Services/OpenGlHardwareRenderContext.cs` en adaptateur du contexte SDK avec son fournisseur de messages ; supprimer `src/GWGUI.Emulation.Nintendo/Emulators/Common/Interop/Services/OpenGlHardwareRenderContext.Framebuffer.cs` après raccordement.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Services/OpenGlHardwareRenderContext.cs` en adaptateur du contexte SDK avec son fournisseur de messages ; supprimer `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Services/OpenGlHardwareRenderContext.Framebuffer.cs` après raccordement.

Ces trois groupes restent des tâches de comparaison détaillée et de définition de
la correction. Les correspondances de code ne suffisent pas à remplacer des états
natifs de formats distincts, à déplacer les données de machines dans le SDK, ou à
fusionner des contextes graphiques possédés par des processus différents.


## Relevé de couverture — phase de repérage

- Période de modifications : `ac7292d86` inclus → HEAD (`0fe45ec74` au relevé),
  soit 650 commits. Les sources antérieures sont consultées comme références de
  réutilisation, pas comme travaux supplémentaires à corriger.
- Les 32 documents de tâches actuels sont indexés, y compris les documents sans
  cases à cocher (`methode-modules`, listes, chronomètre, README) et les deux listes
  actuelles sous project/reference. Aucun document supprimé n’est poursuivi.
- Comparaison AST de 7 104 fichiers C# actuels (33 projets, dont les modules), avec
  2 981 fichiers C# de la version antérieure à Amstrad. 11 452 corps de méthodes ou
  constructeurs d’au moins 30 tokens actuels, 5 308 dans la référence.
- 727 groupes de corps strictement identiques dans l’état actuel : 56 candidats
  anciens et 671 candidats apparus/accrus ou structurellement modifiés durant la
  période. Parmi eux, 333 groupes de la seconde catégorie dépassent 80 tokens.
  **Ces nombres ne sont pas un nombre de bugs.** Les déplacements, changements
  d’identifiants, adaptateurs indépendants et tables natives demandent la lecture
  de leur responsabilité. Le double switch Atari a notamment été exclu manuellement.
- 120 autres fichiers de production/support (XAML, projets, scripts Python,
  PowerShell et CMD) comparés comme fichiers complets : aucun groupe entièrement
  identique après normalisation des espaces. Cela ne prouve pas l’absence de
  blocs partiels ou de duplications fonctionnelles.

Les correspondances détaillées figurent dans `docs/reference/reuse-audit/clone-groups.json` ;
`method-clones.json` conserve les compteurs et limites, `provenance.json` les origines
vérifiées, `history.json` la seule période demandée, `task-inventory.json` les références
aux documents actuels. Ce sont des preuves de repérage, pas de nouveaux catalogues runtime.

## Distinction des suites à donner

1. Les tâches de correction locales (champs UI, INI, code mort Sony, clé Sony générique,
   assertions Memory Stick, contenu NEC/SNK, classement Sharp/Sinclair) sont concrètement
   rattachées aux sources et à leur version antérieure lorsqu’elle est établie.
2. Les copies internes NEC/PC-FX ont un prédécesseur précis plus ancien que le Common
   ultérieur. La compatibilité native doit être conservée avant de supprimer les recopies.
3. Les groupes intermodules sont des tâches de reprise des algorithmes génériques,
   pas une autorisation de fusionner les moteurs ou de partager leurs états natifs.
   Les contrats de documents, tables d’enums natives et données de machines distincts
   ne sont pas déclarés inutiles du seul fait d’un hash identique.
4. Les doublons MediaEngine et familles Atari antérieurs à la borne sont également
   à qualifier et à corriger. Les champs recopiés des technologies d’affichage sont
   déjà raccordés au code antérieur, avec leurs profils conservés.

La phase de repérage couvre maintenant tous les projets et la période demandée.
La qualification détaillée et la correction des candidats intermodules restent à faire
et sont indiquées comme tâches ouvertes ; il serait faux de les annoncer corrigés ou
intégralement validés à l’exécution. Les corrections de production sont détaillées
dans les cases cochées ; les modifications du sélecteur qui précédaient l’audit restent intactes.


### Exécution : champs communs

Base réellement consultée : `bdff8d10c^`, fichiers Fields et Slider. Le renderer
antérieur reste la base de la fabrique extraite ; ses callbacks de sauvegarde sont
paramétrés. Les besoins ajoutés ensuite (radios, retour à la ligne, unités et libellés
de sliders, orientation verticale, valeur automatique) sont adaptés explicitement
sur cette base, sans remplacer l’ancien éditeur par celui du stockage. La lecture
commune des valeurs est également extraite de l’ancienne Persistence ; les chemins
et actions de sauvegarde restent à leur propriétaire.


Champs : construction, lecture des valeurs et slider communs extraits de la base
antérieure ; le stockage conserve uniquement sa présentation de dialogue. Syntaxe
Roslyn vérifiée. Validation fonctionnelle/compilation encore à faire dans les contrôles finaux.
INI : le setter source est lu dans `403ce5836`, et adapté uniquement pour recevoir
la liste et le nom de section. Les propriétés réseau et cartes restent inchangées.

Visuels : comparaison avec 403ce5836^ effectuée. Le premier switch PS1 est conservé
et adapté comme actuellement pour PS2 ; seules les branches ultérieures déjà
couvertes par ce switch sont retirées, sans modifier les listes actives.

Stockage géré : ConfigurationOptionDetailedResourceKey existe avant 329fdaec4.
Cette propriété reçoit la clé d'aide côté RPCS3 ; le contrôleur la traduit et la
fenêtre utilise le texte reçu avec le composant StorageDialogUi antérieur.

PC-98 : les deux Prepare apparaissent ensemble dans 2a9191065. Avant ce commit,
le PrepareContentPath NEC ne faisait que résoudre le chemin du premier média ;
StContentFunctions Atari préparait des images sans écrire ces profils PC-98.
Aucun de ces anciens traitements ne remplace la construction de commande PC-98.
Extraction du corps initial identique, sans choisir un cœur plus récent comme base.
Les profils, exécutables, limites, répertoires firmware et indices restent fournis
par les constantes de chaque adaptateur ; seules les règles identiques sont partagées.
Memory Stick : le diff 403ce5836 confirme le changement en Memory Stick/Sony/PSP ;
seule son assertion obsolète a été corrigée, sans changement de production.

SNK : comparaison des trois paires et des sources initiales a1f460542 effectuée.
Les anciennes SessionMediaFunctions Sharp/Sinclair ne préparent pas les archives
MAME ni les commandes de démarrage : elles ne remplacent pas ces règles SNK.
Mame/Mame2016 ne diffèrent que par la capacité de commande (4096/512) et leurs
constantes firmware natives ; Mame2000/2003 conservent des tables de parents
réellement différentes. FBA2012/NeoGeo ont le même traitement mais des catalogues
natifs indépendants. Les helpers reprennent ces corps initiaux, avec adaptation des
namespaces déplacés depuis et préservation de la propriété du contenu et des finally.

NEC callbacks : les quatre corps de callbacks sont identiques après résolution des
namespaces, hormis les clés des erreurs. ExternalDiskControl a les mêmes délégués
SDK et séquences ; ses deux Capture recopient CaptureBasic. Les constantes des
callbacks sont identiques. ProcessCore a les mêmes méthodes et finally mais des
commandes hôtes et noms de canaux distincts, à conserver en paramètres explicites.
La base reprise est bien le backend PC-FX plus ancien, déplacé vers Common ; les
ressources BeetlePcfx et Core restent distinctes via leur préfixe, sans traduction
nouvelle. Le CoreHost PC-FX conserve sa sélection de cœur et son protocole natif.

Vérification NEC/SNK : analyse sémantique Roslyn de toutes les sources des deux
projets avec leurs références NuGet/projets déjà disponibles : zéro erreur après
correction des alias FirmwareConstants PC-98 et ajout explicite de System.IO dans
le fichier Environment déplacé. Aucun Emit, aucun exécutable ou module reconstruit.
Cela ne remplace pas les futurs essais des émulateurs.

Sony : c2eb676f3 ajoute Pcsx2/PPSSPP en recopiant Mesen, lui-même issu de Yabause,
Caprice32 puis du fichier Amiga cf174afc6 antérieur à la période. Le Common Sony
apparaît ensuite dans 25e2ea61d. Les lecteurs Legacy/V2 sont repris depuis cf174afc6,
avec adaptation des anciens contrats Amiga aux contrats SDK actuels et maintien de
WithCurrentValues côté Sony. Le poll réutilise InputAccumulator existant, sans nouveau
propriétaire natif. La lecture vidéo matérielle et la session empruntée restent Sony.

Vérification SDK/Sony : analyse sémantique de toutes les sources avec références
aux contrats SDK actuels en mémoire, générateur Regex du SDK exécuté sans Emit :
zéro erreur. Les premiers échecs venaient du vérificateur (imports implicites IO et
chargement du générateur), corrigé sans modifier le logiciel pour les contourner.
Aucun binaire produit ; validation en exécution encore requise.

SNK firmware : les routines apparaissent dans a1f460542, sans ordre historique
entre les variantes. Le Common possédait déjà CopyVerified : il reste responsable
de la vérification et de la copie. Les inspections ZIP et ouvertures après copie
sont extraites depuis les corps initiaux. Les tables BIOS MAME/MAME2016 diffèrent
réellement ; HbMame impose un BIOS par défaut contrairement à Mame2015, et
Mame2000/2003/2010 utilisent des noms et tailles ROM différents. Ces règles restent
chez chaque cœur ; la mutualisation porte sur les opérations et les deux sélecteurs
strictement identiques, avec les catalogues natifs transmis en données.

Firmware SNK : opérations communes raccordées, tables et validations natives
préservées ; analyse sémantique SDK/SNK à zéro erreur sans émission de binaires.

Processus : Request et ReadResponse des 16 modules Common sont identiques ; quinze
partagent aussi WaitForHostExit. Le prédécesseur Atari ac7292d86^ contient déjà ces
mécaniques. Le SDK actuel fournit le codec binaire et Attach, mais pas leur séquence
de requête/lecture ni le lancement avec transfert de propriété. Extension de ces
propriétaires SDK existants depuis la base Atari, avec les améliorations de nettoyage
fonctionnelles actuelles ; chaque module conserve ses enums, erreurs, protocole,
configuration, processus, maps vidéo et arrêt dans ses finally. Aucun moteur global.

Vérification processus : zéro erreur sémantique sur le SDK et les 22 modules
modifiés, sans émission. Le vérificateur exécute désormais le générateur Regex pour
Commodore aussi, dont la méthode partielle ne pouvait être vérifiée sans générateur.
Les huit délais des anciens lecteurs asynchrones ont été conservés, y compris le
délai configurable Sony et les constantes Amstrad/Commodore.

Audio/temps : les fonctions PCM et le timer sont présents avant Amstrad dans Atari.
Quinze timers actuels ont le même corps, le seizième Atari garde le constructeur
initial ; tous attendent la même horloge Windows. Le constructeur initial est repris
avec libération explicite des deux handles après échec, sans utiliser la copie récente
comme composant source. Treize contrôleurs audio sont identiques ; Atari conserve
son ancienne politique de sortie, tandis que Watara et SNK ont des propriétés de
latence/tampon différentes, à conserver dans leurs adaptateurs. La base part du
contrôleur Atari antérieur ; la règle de propriété demandée impose de conserver
les sorties reçues et de libérer seulement celles créées par la factory, en finally.
Les tests Atari qui exigeaient la destruction de la sortie empruntée doivent donc
être alignés avec cette règle explicite, sans modifier les échantillons attendus.

Audio/temps : les 16 timers et contrôleurs sont raccordés, le calcul PCM reste
identique. Les propriétés BufferStatus/latence de Watara et SNK demeurent dans leurs
adaptateurs. Le nom ancien InterleavedStereo a été adapté au contrat SDK actuel
InterleavedSamples. Analyse sémantique SDK, Atari, Sharp, Watara et SNK à zéro erreur ;
les autres modules et les scénarios de tests restent à vérifier en fin de groupe.

Installation/diagnostic : AtariCoreReleaseFunctions et AtariCoreDiagnosticFunctions
sont consultés dans ac7292d86^ ; les mêmes boucles de téléchargement (81920 octets),
lecture PE et fichiers JSON sont recopiées dans les 16 modules. Le SDK
ExternalCoreLibrary charge la bibliothèque : il ne doit pas être utilisé pour une
inspection PE sans exécution. ConfigurationFileAccessFunctions possède déjà le
verrouillage et les remplacements ; il est étendu pour héberger la séquence atomique
sans changer la politique de remplacement : Move direct pour les manifestes de
release, ReplaceFile existant pour les configurations. Les quatre manifestes natifs
NEC/Nintendo/Sega/Sony gardent leur format anonyme et leurs chemins ; seule leur
écriture JSON utilise le mécanisme partagé, sans ajout arbitraire de verrouillage.

Persistance : le corps des seize WriteDocumentAtomicallyAsync est identique. La
version Atari ac7292d86^ utilise déjà ConfigurationFileAccessFunctions.ReplaceFile,
avec nom temporaire GUID, FileOptions.WriteThrough, SerializeAsync et FlushAsync.
Le helper atomique ajouté au propriétaire SDK conserve exactement cette séquence ;
le callback de chaque module garde le document et JsonOptions propres à son domaine.
Les lectures, récupérations et sauvegardes de secours ne changent pas.

Sharp/Sinclair : les Common SettingsFunctions sont relus dans 4d2df31ae^ et
033f73ce2^ : Native était déjà leur constructeur de champs. Les deux copies de
chaque module ajoutent le même tri et les mêmes valeurs ; Sinclair applique aussi
IsAvailable, qui doit rester dans son adaptateur. Les titres diffèrent selon les
modules : ils restent des données de leur Common. Les boucles de valeurs et de
groupement sont extraites depuis les sources initiales des ajouts dans le SDK,
avec construction des champs déléguée au Native antérieur, sans recopier de nouveau
ces boucles entre les deux Common.

Lecteurs NEC : les trois Devices sont identiques et apparaissent ensemble dans
2a9191065. Le Common de description de stockage est le propriétaire antérieur de
leur agrégation ; il est adapté pour construire les lecteurs à partir des constantes
de chaque adaptateur, sans remplacer les formats de l’un par ceux d’un autre.

Options natives : AtariCoreOptionFunctions et AtariCoreOptionHost dans ac7292d86^
possèdent déjà les lecteurs complets V1/V2, catégories et valeurs par défaut validées.
Les lecteurs simples Amiga et les lecteurs complets Atari ont des comportements
distincts : ils ne sont pas interchangeables. Les pointeurs restent empruntés, les
contrats CoreOption/Category des modules restent leurs données, construites via des
factories. Les CoreOptionHost utilisent déjà ExternalCoreUtf8String du SDK et le
libèrent dans leur finally ; aucun second cache ou propriétaire natif ne sera ajouté.
Les présentations et restrictions locales restent dans les adaptateurs ; les boucles
ABI et les lectures/visibilités recopiées sont partagées.

OptionCallbackHost : le premier bloc est celui de Thomson f22f8e913, copié ensuite
vers Wenquxing, Watara et Panasonic. Il impose 4096 variables maximum et rejette les
descripteurs mal formés ; cette politique diffère des lecteurs précédents. Le bloc
Thomson entier est repris dans le SDK avec factories des records locaux ; les listes
d’options masquées, notamment Panasonic, restent dans leurs adaptateurs. Le propriétaire
SDK de chaînes ExternalCoreUtf8String remplace les pointeurs bruts du bloc initial,
avec maintien des corrections de nettoyage intervenues depuis.

Raccordements options : le contrôle des affectations originales confirme le filtre
IsSupportedNintendoOption/WithSupportedGraphicsApis, et un lecteur simple V1 chez
Nintendo et Sega. Ces adaptations sont inscrites avant correction : le V1 partage
les règles du V2 simple avec un préfixe de trois pointeurs et une description à
l’indice 2 ; il ne doit pas être remplacé par le lecteur Atari validant les défauts.
Le filtre Nintendo est conservé à chaque entrée, indépendamment du lecteur partagé.

Options : lecteurs complets, callbacks communs et bloc strict Thomson raccordés.
Les mappers natifs et leurs règles de présentation restent locaux ; aucune ressource
ou type de configuration de machine n’est déplacé dans un catalogue parallèle.
Les politiques de visibilité propres à Atari et aux autres hôtes ont été reprises
chacune depuis leur méthode originale, après détection de la différence IsExposed.
SDK, Atari, Sharp, NEC, Amstrad, Commodore, Sony, les quatre hôtes stricts et les
raccordements V1 Nintendo/Sega : analyse sémantique à zéro erreur, sans émission.

Sauvegardes : AtariStateFileFunctions et AtariSavedStateFunctions de ac7292d86^
contiennent déjà l’enveloppe magic + longueur little-endian + JSON + état, et le
hachage SHA256 des dossiers avec chemins canoniques. Les signatures, empreintes de
configuration/contenu et validations des en-têtes restent des contrats des modules.
Le ReadOnce des seize modules fait des validations et retries supplémentaires,
alors que les sept StateStore natifs ont leur propre ouverture/erreur et remplacement.
Seuls leurs streams déjà ouverts sont empruntés par le SDK ; les callers gardent
l’ouverture, les finally et leur politique File.Replace/File.Move distincte.

Sauvegardes : cadrage, écritures temporaires et hachages raccordés. Le SDK possède
et ferme le stream temporaire qu’il crée pour WriteFile ; Read emprunte le stream
ouvert selon la politique du module. Magic, suffixes, JSON, File.Replace/File.Move,
retries et validations d’en-tête sont conservés. HashFile Commodore reste disponible
pour son appelant Machine.Commands, en déléguant au hachage SDK. Le script ayant
utilisé la variable PowerShell réservée Error a été corrigé, et les fichiers de ce
seul groupe ont été régénérés depuis leur source antérieure puis analysés.
SDK, Atari, Sharp, Sony, Commodore et Microsoft : zéro erreur sémantique sans Emit.

Vidéo/graphique : AtariVideoBufferSet et AtariSharedVideoWriter existent dans
ac7292d86^, ainsi que le protocole de trames redimensionnables. Les buffers restent
loués au pool emprunté ; les maps et vues sont créées/fermées par le composant SDK,
avec les paramètres de capacité et noms donnés par chaque module. Le ReadResizable
transfère les nouvelles maps au caller seulement après succès et libère les anciennes.

OpenGL : le contexte initial est Sega/Flycast e5caa3d7a ; la partie framebuffer
apparaît chez Nintendo dans 9af3b8e2e puis est copiée vers Sega (948c0e509). Les trois
paires actuelles ont des corps identiques, hors namespaces et résolution des erreurs.
Les propriétaires antérieurs sont donc distincts pour ces deux parties : ne pas
prendre Sony comme base de Sega ou Sega comme base historique du framebuffer Nintendo.
Le SDK reprend ces composants et conserve leurs évolutions fonctionnelles actuelles,
notamment Core profile, FBO et libérations déjà vérifiées. Fenêtre, DC, contexte GL,
FBO et buffers épinglés sont possédés et libérés en finally ; GetModuleHandle fournit
un module emprunté, qui ne doit pas être libéré par ce contexte. La fenêtre est
vérifiée détruite avec IsWindow, sur le thread propriétaire conservé par les callers.

- [x] Terminer les raccordements graphiques et retirer les accumulateurs recopiés.
  - [x] Modifier les seize `src/GWGUI.Emulation.*/Emulators/Common/Functions/CoreFunctions.Host.cs` pour rétablir sur les adaptateurs vidéo les attributs Windows de leurs méthodes originales.
  - [x] Modifier `src/GWGUI.Emulation/Services/EmulationInputAccumulator.cs` pour ajouter Reset sous son verrou existant, sans modifier Update/Consume ; ce propriétaire existe depuis d6277ffa1, avant Amstrad.
  - [x] Modifier les sept `src/GWGUI.Emulation.{Amstrad,Commodore,Nec,Nintendo,Sega,Sony,Microsoft}/EmulationGlobalUsings.cs` pour aliaser InputAccumulator vers le propriétaire SDK ; supprimer leurs sept `Common/Services/InputAccumulator.cs` recopiés après la création Amstrad.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec les résultats de vérification des raccordements SDK et modules, sans émission de binaires.

Vidéo/OpenGL et entrées : SDK, Amstrad, Commodore, NEC, Nintendo, Sega, Sony et
Microsoft analysés sans émission : zéro erreur sémantique. Les attributs Windows
des méthodes de protocole d’origine sont rétablis. Les sept copies InputAccumulator
sont supprimées ; Reset est porté par le propriétaire SDK antérieur d6277ffa1.

- [x] Relever les candidats restants après les corrections.
  - [x] Créer temporairement `artifacts/reuse-audit-residual.ps1` pour comparer les corps AST actuels des candidats ; créer `docs/reference/reuse-audit/remaining-groups.json` avec les correspondances restantes et `docs/tasks/reuse-project-audit.md` avec le nombre de candidats et la qualification restant à effectuer.
  - [x] Supprimer `artifacts/reuse-audit-residual.ps1` après production du relevé ; ne conserver que le résultat d’audit dans `docs/reference/reuse-audit/remaining-groups.json`.

Le relevé après les premiers raccordements contient encore 577 groupes de corps
identiques parmi les candidats initiaux. Ce nombre ne vaut pas 577 défauts :
les constantes natives, types de configuration et adaptateurs de modules peuvent
avoir des politiques distinctes. La qualification complète reste ouverte.

- [x] Reprendre les traitements média préexistants recopiés pour Amstrad.
  - [x] Modifier `src/GWGUI.MediaEngine/Functions/Crc16Calculator.cs` avec une entrée ReadOnlySpan utilisant Update déjà présent avant Amstrad ; modifier les ComputeCrc de `Images/Reading/Decoding/Sequential/Amstrad/AmstradCpcTapeDecoder.cs` et `Images/Writing/Encoding/Sequential/Amstrad/AmstradCpcTapeEncoder.cs` pour lui passer la graine et le polynôme Amstrad. Les copies sont introduites par f2fb3caf3.
  - [x] Modifier `src/GWGUI.MediaEngine/Functions/PcmSampleFunctions.cs` pour y déplacer AddPulses de `Images/Writing/Encoding/Sequential/Spectrum/SpectrumTapeEncoder.cs` antérieur à Amstrad ; modifier cet encodeur et `Images/Writing/Encoding/Sequential/Amstrad/AmstradCpcTapeEncoder.cs` pour appeler ce propriétaire PCM avec la fréquence TStates fournie par TzxConstants.

- [x] Réutiliser les conversions clavier Atari antérieures aux tables recopiées.
  - [x] Créer `src/GWGUI.Emulation/Functions/ExternalCoreKeyboardFunctions.cs` et `Constants/ExternalCoreKeyboardConstants.cs` depuis AtariKeyboardFunctions et AtariKeyboardConstants présents dans ac7292d86^ ; partager Character, AddRange et la table native standard sans déplacer IsConsoleKeyActive Atari.
  - [x] Modifier les Character et AddRange de `src/GWGUI.Emulation.*/**/KeyboardFunctions.cs` et `InputFunctions.Keyboard.cs` pour déléguer les boucles identiques au SDK ; conserver leurs tables de données, touches propres et politiques IsModifier/Modifiers.
  - [x] Modifier les CreateKeyboardMap et CharacterFor des six `ExternalHostCallbacks.Input.cs` NEC, Nintendo, Sega et Sony (Common, Pcsx2, Ppsspp) pour réutiliser la table antérieure, en excluant les quatre touches Atari/Amiga absentes de leurs tables actuelles.
  - [x] Modifier `src/GWGUI.Emulation.Mattel/Emulators/Common/Interop/Functions/KeyboardFunctions.cs` et `src/GWGUI.Emulation.Epoch/Emulators/Common/Interop/Functions/InputFunctions.Keyboard.cs` pour conserver leurs Character spécifiques (aucun caractère Mattel, chiffres seuls Epoch), distincts des corps partagés ; supprimer IsPrintable privé devenu inutilisé dans les adaptateurs de Character.
  - [x] Modifier `src/GWGUI.Emulation.Wenquxing/Emulators/Wqxemu/Interop/Functions/KeyboardFunctions.cs` pour conserver AddRange original qui utilise Dictionary.Add, avec ses noms first/nativeFirst et son rejet des doublons.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Emulators/Common/Interop/Functions/CoreFunctions.Options.cs` pour rétablir le namespace des catégories ; modifier `Common/Machines/Common/Functions/StateFunctions.Saved.cs` pour conserver HashPath avec les dépendances de son adaptateur et déléguer seulement le hachage commun ; rétablir HashFile encore appelé dans les fichiers Saved de Panasonic/Wenquxing via le propriétaire SDK.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec la vérification des conversions et des modules concernés sans émission.

Claviers : les tables natives des six callbacks ont le même ensemble de codes
que la table Atari, hors LeftAmiga/RightAmiga/Break/Undo. Character utilise les
mêmes règles ASCII, Maj et Verr. Maj. Les politiques différentes Mattel (aucun
caractère) et Epoch (chiffres) sont conservées. Wenquxing garde Dictionary.Add.
Analyse SDK et de tous les modules effectuée : les adaptations Apple (dépendances
de médias), Panasonic/Wenquxing (HashFile utilisé) ont été rétablies puis ces
quatre projets réanalysés avec zéro erreur. MediaEngine : zéro erreur sans Emit.

- [x] Reprendre le cadrage des réponses hôtes du propriétaire Atari antérieur.
  - [x] Modifier `src/GWGUI.Emulation/Functions/EmulationHostProtocolFunctions.cs` avec WriteResponse repris du Run Atari de ac7292d86^ : créer et fermer le buffer de réponse, réinitialiser après erreur, écrire le blob. Le transport est emprunté ; les traitements de commande et les formats restent fournis par chaque module.
  - [x] Modifier `src/GWGUI.Emulation.Amstrad/Emulators/Common/Services/CoreHost.cs` pour déléguer son enveloppe de réponse à WriteResponse, en gardant le switch, les états de séquence, les finally de cœur et son format original.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Common/Services/CoreHost.cs` pour déléguer son enveloppe de réponse à WriteResponse, en gardant le switch, les états de séquence, les finally de cœur et son format original.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Services/CoreHost.cs` pour déléguer son enveloppe de réponse à WriteResponse, en gardant le switch, les états de séquence, les finally de cœur et son format original.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Ppsspp/Services/CoreHost.cs` pour déléguer son enveloppe de réponse à WriteResponse, en gardant le switch, les états de séquence, les finally de cœur et son format original.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Pcsx2/Services/CoreHost.cs` pour déléguer son enveloppe de réponse à WriteResponse, en gardant le switch, les états de séquence, les finally de cœur et son format original.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/Common/Services/CoreHost.cs` pour déléguer son enveloppe de réponse à WriteResponse, en gardant le switch, les états de séquence, les finally de cœur et son format original.
  - [x] Modifier `src/GWGUI.Emulation.Sharp/Common/Services/CoreHost.cs` pour déléguer son enveloppe de réponse à WriteResponse, en gardant le switch, les états de séquence, les finally de cœur et son format original.
  - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Services/CoreHost.cs` pour déléguer son enveloppe de réponse à WriteResponse, en gardant le switch, les états de séquence, les finally de cœur et son format original.
  - [x] Modifier `src/GWGUI.Emulation.Mattel/Common/Services/CoreHost.cs` pour déléguer son enveloppe de réponse à WriteResponse, en gardant le switch, les états de séquence, les finally de cœur et son format original.
  - [x] Modifier `src/GWGUI.Emulation.Epoch/Common/Services/CoreHost.cs` pour déléguer son enveloppe de réponse à WriteResponse, en gardant le switch, les états de séquence, les finally de cœur et son format original.
  - [x] Modifier `src/GWGUI.Emulation.Watara/Common/Services/CoreHost.cs` pour déléguer son enveloppe de réponse à WriteResponse, en gardant le switch, les états de séquence, les finally de cœur et son format original.
  - [x] Modifier `src/GWGUI.Emulation.Wenquxing/Common/Services/CoreHost.cs` pour déléguer son enveloppe de réponse à WriteResponse, en gardant le switch, les états de séquence, les finally de cœur et son format original.
  - [x] Modifier `src/GWGUI.Emulation.Bandai/Common/Services/CoreHost.cs` pour déléguer son enveloppe de réponse à WriteResponse, en gardant le switch, les états de séquence, les finally de cœur et son format original.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Common/Services/CoreHost.cs` pour déléguer son enveloppe de réponse à WriteResponse, en gardant le switch, les états de séquence, les finally de cœur et son format original.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Common/Services/CoreHost.cs` pour déléguer son enveloppe de réponse à WriteResponse, en gardant le switch, les états de séquence, les finally de cœur et son format original.
  - [x] Modifier `src/GWGUI.Emulation.Commodore/Emulators/Common/Interop/Services/CoreHost.cs` pour déléguer son enveloppe de réponse à WriteResponse, en gardant le switch, les états de séquence, les finally de cœur et son format original.
  - [x] Modifier `src/GWGUI.Emulation.Thomson/Common/Services/CoreHost.cs` pour déléguer son enveloppe de réponse à WriteResponse, en gardant le switch, les états de séquence, les finally de cœur et son format original.
  - [x] Modifier `src/GWGUI.Emulation.Fairchild/Common/Services/CoreHost.cs` pour déléguer son enveloppe de réponse à WriteResponse, en gardant le switch, les états de séquence, les finally de cœur et son format original.
  - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/Common/Interop/Services/CoreHost.cs` pour déléguer son enveloppe de réponse à WriteResponse, en gardant le switch, les états de séquence, les finally de cœur et son format original.
  - [x] Modifier `src/GWGUI.Emulation.Sinclair/Common/Services/CoreHost.cs` pour déléguer son enveloppe de réponse à WriteResponse, en gardant le switch, les états de séquence, les finally de cœur et son format original.
  - [x] Modifier `src/GWGUI.Emulation.Nec/Emulators/BeetlePcfx/Services/CoreHost.cs` pour déléguer son enveloppe de réponse à WriteResponse, en gardant le switch, les états de séquence, les finally de cœur et son format original.
  - [x] Modifier `src/GWGUI.Emulation.Nec/Emulators/Common/Interop/Services/CoreHost.cs` pour déléguer son enveloppe de réponse à WriteResponse, en gardant le switch, les états de séquence, les finally de cœur et son format original.
  - [x] Modifier `src/GWGUI.Emulation.Panasonic/Common/Services/CoreHost.cs` pour déléguer son enveloppe de réponse à WriteResponse, en gardant le switch, les états de séquence, les finally de cœur et son format original.
  - [x] Modifier `src/GWGUI.Emulation.Philips/Common/Services/CoreHost.cs` pour déléguer son enveloppe de réponse à WriteResponse, en gardant le switch, les états de séquence, les finally de cœur et son format original.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Emulators/Common/Interop/Services/CoreHost.cs` pour déléguer son enveloppe de réponse à WriteResponse, en gardant le switch, les états de séquence, les finally de cœur et son format original.

Le script initial d’inscription de ce groupe a échoué avant écriture (apostrophe
PowerShell). Les 25 raccordements déjà réalisés sont inscrits ci-dessus avec leur
état réel ; cette correction du suivi ne vaut pas validation de leur exécution.

- [x] Vérifier les raccordements des enveloppes hôtes et la vue App.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec les résultats des analyses SDK/modules/App sans Emit et les erreurs restant à corriger avant toute validation.

Enveloppes hôtes : analyse du SDK et des 23 modules, zéro erreur après raccordement.
App : la première invocation d’analyse omettait unsafe, les références WinRT et les
sources générées WPF existantes. Après correction du harnais sans modifier le code
pour ces diagnostics : SDK et App, zéro erreur sémantique sans Emit.

- [x] Vérifier les consommateurs et nettoyer les changements de présentation du code.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec l’analyse sémantique sans Emit des tests préexistants utilisant les composants partagés, et les corrections nécessaires relevées avant de les réaliser.
  - [x] Modifier les 25 `src/GWGUI.Emulation.*/**/CoreHost.cs` raccordés et les adaptateurs audio/clavier nouveaux pour aligner les blocs et expressions ajoutés ; retirer uniquement les espaces de fin de ligne et lignes finales superflues des fichiers modifiés par cet audit.
  - [x] Modifier `tests/GWGUI.Tests/Emulation/Sony/SettingsPresentationTests.cs` pour importer le namespace des enums Sony de ControllerType.Joystick, import manquant déjà dans HEAD et révélé par l’analyse complète des consommateurs. Cette action a été inscrite après réalisation à cause du nouvel échec de la commande PowerShell d’inscription ; cet écart de suivi est conservé explicitement.

Tests existants : toutes les sources ont été analysées contre les compilations en
mémoire du SDK, des modules et d’App. Un import enum Sony manquait déjà dans HEAD
dans SettingsPresentationTests ; aucun autre diagnostic sémantique de test relevé.
Le contrôle après ajout de cet import reste à effectuer. Aucun test exécuté et
aucun binaire de build réécrit. Diff --check ne signale plus de défaut d’espacement.

- [x] Reprendre les primitives de mapping souris/manettes antérieures.
  - [x] Modifier `src/GWGUI.Emulation/Functions/EmulationInputMappingFunctions.cs` pour reprendre PhysicalMouse, TryRemovePrefix et ApplyDeadZone des fonctions Amiga/Atari de ac7292d86^ ; adapter ResolveSourceController avec une surcharge conservant le repli historique des neuf ControllerForSource. Conserver la signature trois arguments et son comportement strict pour ses consommateurs actuels.
  - [x] Modifier `src/GWGUI.App/Functions/Input/Controllers/ControllerInputMap.cs` pour déléguer ControllerForSource à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.
  - [x] Modifier `src/GWGUI.Emulation.Amstrad/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` pour déléguer ControllerForSource à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.
  - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` pour déléguer ControllerForSource à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.
  - [x] Modifier `src/GWGUI.Emulation.Commodore/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` pour déléguer ControllerForSource à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.
  - [x] Modifier `src/GWGUI.Emulation.Microsoft/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` pour déléguer ControllerForSource à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.
  - [x] Modifier `src/GWGUI.Emulation.Nec/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` pour déléguer ControllerForSource à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` pour déléguer ControllerForSource à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.
  - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` pour déléguer ControllerForSource à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` pour déléguer ControllerForSource à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.
  - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` pour déléguer PhysicalMouse à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.
  - [x] Modifier `src/GWGUI.Emulation.Microsoft/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` pour déléguer PhysicalMouse à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.
  - [x] Modifier `src/GWGUI.Emulation.Nec/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` pour déléguer PhysicalMouse à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` pour déléguer PhysicalMouse à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.
  - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` pour déléguer PhysicalMouse à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` pour déléguer PhysicalMouse à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` pour déléguer TryRemovePrefix à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` pour déléguer TryRemovePrefix à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.
  - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` pour déléguer TryRemovePrefix à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.
  - [x] Modifier `src/GWGUI.Emulation.Microsoft/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` pour déléguer TryRemovePrefix à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.
  - [x] Modifier `src/GWGUI.Emulation.Nec/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` pour déléguer TryRemovePrefix à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` pour déléguer TryRemovePrefix à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.
  - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` pour déléguer TryRemovePrefix à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` pour déléguer TryRemovePrefix à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Common/Machines/Common/Functions/ControllerFunctions.cs` pour déléguer ApplyDeadZone à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Common/Machines/Common/Functions/ControllerFunctions.cs` pour déléguer ApplyDeadZone à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.
  - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/Common/Functions/ControllerFunctions.cs` pour déléguer ApplyDeadZone à la primitive SDK antérieure adaptée, avec le même repli et les mêmes constantes.

- [x] Reprendre les lecteurs ABI d’environnement Atari antérieurs aux modules ajoutés.
  - [x] Créer `src/GWGUI.Emulation/Functions/ExternalCoreEnvironmentFunctions.cs` depuis AtariEnvironmentFunctions de ac7292d86^ : lecteurs des descripteurs d’entrée, des ports, de la mémoire et conversion de langue ; paramétrer les plafonds et les factories des records locaux, sans posséder les pointeurs natifs empruntés.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/Common/Interop/Functions/RuntimeFunctions.Environment.cs` pour déléguer les quatre lecteurs identiques à la fonction Atari partagée, en conservant son plafond optionnel et ses records de domaine.
  - [x] Modifier `src/GWGUI.Emulation.Mattel/Emulators/Common/Interop/Functions/RuntimeFunctions.Environment.cs` pour déléguer les quatre lecteurs identiques à la fonction Atari partagée, en conservant son plafond optionnel et ses records de domaine.
  - [x] Modifier `src/GWGUI.Emulation.Fairchild/Emulators/Common/Interop/Functions/RuntimeFunctions.Environment.cs` pour déléguer les quatre lecteurs identiques à la fonction Atari partagée, en conservant son plafond optionnel et ses records de domaine.
  - [x] Modifier `src/GWGUI.Emulation.Bandai/Emulators/Common/Interop/Functions/RuntimeFunctions.Environment.cs` pour déléguer les quatre lecteurs identiques à la fonction Atari partagée, en conservant son plafond optionnel et ses records de domaine.
  - [x] Modifier `src/GWGUI.Emulation.Epoch/Emulators/Common/Interop/Functions/RuntimeFunctions.Environment.cs` pour déléguer les quatre lecteurs identiques à la fonction Atari partagée, en conservant son plafond optionnel et ses records de domaine.
  - [x] Modifier `src/GWGUI.Emulation.Sinclair/Emulators/Common/Interop/Functions/RuntimeFunctions.Environment.cs` pour déléguer les quatre lecteurs identiques à la fonction Atari partagée, en conservant son plafond optionnel et ses records de domaine.
  - [x] Modifier `src/GWGUI.Emulation.Sharp/Emulators/Common/Interop/Functions/RuntimeFunctions.Environment.cs` pour déléguer les quatre lecteurs identiques à la fonction Atari partagée, en conservant son plafond optionnel et ses records de domaine.
  - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/Common/Interop/Functions/RuntimeFunctions.Environment.cs` pour déléguer les quatre lecteurs identiques à la fonction Atari partagée, en conservant son plafond optionnel et ses records de domaine.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/Emulators/Common/Interop/Functions/RuntimeFunctions.Environment.cs` pour déléguer les quatre lecteurs identiques à la fonction Atari partagée, en conservant son plafond optionnel et ses records de domaine.
  - [x] Modifier `src/GWGUI.Emulation.Philips/Emulators/Common/Interop/Functions/RuntimeFunctions.Environment.cs` pour déléguer les quatre lecteurs identiques à la fonction Atari partagée, en conservant son plafond optionnel et ses records de domaine.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Emulators/Common/Interop/Functions/RuntimeFunctions.Environment.cs` pour déléguer les quatre lecteurs identiques à la fonction Atari partagée, en conservant son plafond optionnel et ses records de domaine.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Emulators/Common/Interop/Functions/RuntimeFunctions.Environment.cs` pour déléguer les quatre lecteurs identiques à la fonction Atari partagée, en conservant son plafond optionnel et ses records de domaine.

- [x] Terminer le partage des données natives d’environnement.
  - [x] Modifier `src/GWGUI.Emulation/Functions/ExternalCoreEnvironmentFunctions.cs` pour reprendre CopyNativeLogTemplate Atari antérieur avec marqueur et conversions fournis par le module ; aligner les nouveaux blocs du SDK sans changer leur contenu.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/Common/Interop/Functions/RuntimeFunctions.Environment.cs` pour déléguer CopyNativeLogTemplate avec ses constantes originales et aligner les expressions ajoutées.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Emulators/Common/Interop/Functions/RuntimeFunctions.Environment.cs` pour déléguer CopyNativeLogTemplate avec ses constantes originales et aligner les expressions ajoutées.
  - [x] Modifier `src/GWGUI.Emulation.Epoch/Emulators/Common/Interop/Functions/RuntimeFunctions.Environment.cs` pour déléguer CopyNativeLogTemplate avec ses constantes originales et aligner les expressions ajoutées.
  - [x] Modifier `src/GWGUI.Emulation.Bandai/Emulators/Common/Interop/Functions/RuntimeFunctions.Environment.cs` pour déléguer CopyNativeLogTemplate avec ses constantes originales et aligner les expressions ajoutées.
  - [x] Modifier `src/GWGUI.Emulation.Sharp/Emulators/Common/Interop/Functions/RuntimeFunctions.Environment.cs` pour déléguer CopyNativeLogTemplate avec ses constantes originales et aligner les expressions ajoutées.
  - [x] Modifier `src/GWGUI.Emulation.Mattel/Emulators/Common/Interop/Functions/RuntimeFunctions.Environment.cs` pour déléguer CopyNativeLogTemplate avec ses constantes originales et aligner les expressions ajoutées.
  - [x] Modifier `src/GWGUI.Emulation.Fairchild/Emulators/Common/Interop/Functions/RuntimeFunctions.Environment.cs` pour déléguer CopyNativeLogTemplate avec ses constantes originales et aligner les expressions ajoutées.
  - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/Common/Interop/Functions/RuntimeFunctions.Environment.cs` pour déléguer CopyNativeLogTemplate avec ses constantes originales et aligner les expressions ajoutées.
  - [x] Modifier `src/GWGUI.Emulation.Philips/Emulators/Common/Interop/Functions/RuntimeFunctions.Environment.cs` pour déléguer CopyNativeLogTemplate avec ses constantes originales et aligner les expressions ajoutées.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/Emulators/Common/Interop/Functions/RuntimeFunctions.Environment.cs` pour déléguer CopyNativeLogTemplate avec ses constantes originales et aligner les expressions ajoutées.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Emulators/Common/Interop/Functions/RuntimeFunctions.Environment.cs` pour déléguer CopyNativeLogTemplate avec ses constantes originales et aligner les expressions ajoutées.
  - [x] Modifier `src/GWGUI.Emulation.Sinclair/Emulators/Common/Interop/Functions/RuntimeFunctions.Environment.cs` pour déléguer CopyNativeLogTemplate avec ses constantes originales et aligner les expressions ajoutées.
  - [x] Modifier ou supprimer `src/GWGUI.Emulation.Acorn/Emulators/Common/Interop/Enums/EnvironmentLanguage.cs` pour retirer uniquement EnvironmentLanguage devenu inutilisé après le raccordement de CurrentLanguage.
  - [x] Modifier ou supprimer `src/GWGUI.Emulation.Apple/Emulators/Common/Interop/Enums/RuntimeEnums.cs` pour retirer uniquement EnvironmentLanguage devenu inutilisé après le raccordement de CurrentLanguage.
  - [x] Modifier ou supprimer `src/GWGUI.Emulation.Bandai/Emulators/Common/Enums/RuntimeEnums.cs` pour retirer uniquement EnvironmentLanguage devenu inutilisé après le raccordement de CurrentLanguage.
  - [x] Modifier ou supprimer `src/GWGUI.Emulation.Coleco/Emulators/Common/Enums/RuntimeEnums.cs` pour retirer uniquement EnvironmentLanguage devenu inutilisé après le raccordement de CurrentLanguage.
  - [x] Modifier ou supprimer `src/GWGUI.Emulation.Epoch/Emulators/Common/Enums/RuntimeEnums.cs` pour retirer uniquement EnvironmentLanguage devenu inutilisé après le raccordement de CurrentLanguage.
  - [x] Modifier ou supprimer `src/GWGUI.Emulation.Atari/Emulators/Common/Enums/RuntimeEnums.cs` pour retirer uniquement EnvironmentLanguage devenu inutilisé après le raccordement de CurrentLanguage.
  - [x] Modifier ou supprimer `src/GWGUI.Emulation.Mattel/Emulators/Common/Enums/RuntimeEnums.cs` pour retirer uniquement EnvironmentLanguage devenu inutilisé après le raccordement de CurrentLanguage.
  - [x] Modifier ou supprimer `src/GWGUI.Emulation.Fairchild/Emulators/Common/Enums/RuntimeEnums.cs` pour retirer uniquement EnvironmentLanguage devenu inutilisé après le raccordement de CurrentLanguage.
  - [x] Modifier ou supprimer `src/GWGUI.Emulation.Sharp/Emulators/Common/Interop/Enums/EnvironmentLanguage.cs` pour retirer uniquement EnvironmentLanguage devenu inutilisé après le raccordement de CurrentLanguage.
  - [x] Modifier ou supprimer `src/GWGUI.Emulation.Philips/Emulators/Common/Enums/RuntimeEnums.cs` pour retirer uniquement EnvironmentLanguage devenu inutilisé après le raccordement de CurrentLanguage.
  - [x] Modifier ou supprimer `src/GWGUI.Emulation.Sinclair/Emulators/Common/Interop/Enums/EnvironmentLanguage.cs` pour retirer uniquement EnvironmentLanguage devenu inutilisé après le raccordement de CurrentLanguage.
  - [x] Modifier ou supprimer `src/GWGUI.Emulation.Snk/Emulators/Common/Interop/Enums/EnvironmentLanguage.cs` pour retirer uniquement EnvironmentLanguage devenu inutilisé après le raccordement de CurrentLanguage.
  - [x] Supprimer `src/GWGUI.Emulation.Sharp/Emulators/Common/Interop/Constants/EnvironmentFunctionsConstants.cs` dont les codes de langue ne sont plus référencés depuis le raccordement au propriétaire commun.
  - [x] Supprimer `src/GWGUI.Emulation.Acorn/Emulators/Common/Interop/Constants/EnvironmentFunctionsConstants.cs` dont les codes de langue ne sont plus référencés depuis le raccordement au propriétaire commun.
  - [x] Supprimer `src/GWGUI.Emulation.Bandai/Emulators/Common/Interop/Constants/EnvironmentFunctionsConstants.cs` dont les codes de langue ne sont plus référencés depuis le raccordement au propriétaire commun.
  - [x] Supprimer `src/GWGUI.Emulation.Atari/Emulators/Common/Interop/Constants/EnvironmentFunctionsConstants.cs` dont les codes de langue ne sont plus référencés depuis le raccordement au propriétaire commun.
  - [x] Supprimer `src/GWGUI.Emulation.Epoch/Emulators/Common/Interop/Constants/EnvironmentFunctionsConstants.cs` dont les codes de langue ne sont plus référencés depuis le raccordement au propriétaire commun.
  - [x] Supprimer `src/GWGUI.Emulation.Apple/Emulators/Common/Interop/Constants/EnvironmentFunctionsConstants.cs` dont les codes de langue ne sont plus référencés depuis le raccordement au propriétaire commun.
  - [x] Supprimer `src/GWGUI.Emulation.Philips/Emulators/Common/Interop/Constants/EnvironmentFunctionsConstants.cs` dont les codes de langue ne sont plus référencés depuis le raccordement au propriétaire commun.
  - [x] Supprimer `src/GWGUI.Emulation.Mattel/Emulators/Common/Interop/Constants/EnvironmentFunctionsConstants.cs` dont les codes de langue ne sont plus référencés depuis le raccordement au propriétaire commun.
  - [x] Supprimer `src/GWGUI.Emulation.Sinclair/Emulators/Common/Interop/Constants/EnvironmentFunctionsConstants.cs` dont les codes de langue ne sont plus référencés depuis le raccordement au propriétaire commun.
  - [x] Supprimer `src/GWGUI.Emulation.Fairchild/Emulators/Common/Interop/Constants/EnvironmentFunctionsConstants.cs` dont les codes de langue ne sont plus référencés depuis le raccordement au propriétaire commun.
  - [x] Supprimer `src/GWGUI.Emulation.Snk/Emulators/Common/Interop/Constants/EnvironmentFunctionsConstants.cs` dont les codes de langue ne sont plus référencés depuis le raccordement au propriétaire commun.
  - [x] Supprimer `src/GWGUI.Emulation.Coleco/Emulators/Common/Interop/Constants/EnvironmentFunctionsConstants.cs` dont les codes de langue ne sont plus référencés depuis le raccordement au propriétaire commun.

- [x] Retirer les propriétaires de contenu natif recopiés après Amstrad.
  - [x] Modifier `src/GWGUI.Emulation/Services/ExternalCoreUtf8String.cs`, propriétaire existant depuis 16d504d5b, avec un constructeur de chemin ANSI Windows conservant le constructeur UTF-8 antérieur et la libération CoTaskMem.
  - [x] Créer `src/GWGUI.Emulation/Services/ExternalCoreLoadedContent.cs` depuis AtariLoadedContent et AtariContentFunctions présents dans ac7292d86^ ; reprendre les allocations GameInfo/données, leur transfert et leur nettoyage en finally. La validation de formats et les politiques de copie/écriture de médias restent au module.
  - [x] Modifier `src/GWGUI.Emulation.Sinclair/Emulators/Common/Interop/Functions/MediaFunctions.Content.cs` et `src/GWGUI.Emulation.Sinclair/EmulationGlobalUsings.cs` pour utiliser le propriétaire SDK après Validate ; supprimer `src/GWGUI.Emulation.Sinclair/Emulators/Common/Interop/Services/ContentPath.cs` et `src/GWGUI.Emulation.Sinclair/Emulators/Common/Interop/Services/LoadedContent.cs` après raccordement, sans toucher aux contenus Thomson/Watara qui assurent une écriture différée distincte.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/Emulators/Common/Interop/Functions/MediaFunctions.Content.cs` et `src/GWGUI.Emulation.Coleco/EmulationGlobalUsings.cs` pour utiliser le propriétaire SDK après Validate ; supprimer `src/GWGUI.Emulation.Coleco/Emulators/Common/Interop/Services/ContentPath.cs` et `src/GWGUI.Emulation.Coleco/Emulators/Common/Interop/Services/LoadedContent.cs` après raccordement, sans toucher aux contenus Thomson/Watara qui assurent une écriture différée distincte.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Emulators/Common/Interop/Functions/MediaFunctions.Content.cs` et `src/GWGUI.Emulation.Snk/EmulationGlobalUsings.cs` pour utiliser le propriétaire SDK après Validate ; supprimer `src/GWGUI.Emulation.Snk/Emulators/Common/Interop/Services/ContentPath.cs` et `src/GWGUI.Emulation.Snk/Emulators/Common/Interop/Services/LoadedContent.cs` après raccordement, sans toucher aux contenus Thomson/Watara qui assurent une écriture différée distincte.
  - [x] Modifier `src/GWGUI.Emulation.Sharp/Emulators/Common/Interop/Functions/MediaFunctions.Content.cs` et `src/GWGUI.Emulation.Sharp/EmulationGlobalUsings.cs` pour utiliser le propriétaire SDK après Validate ; supprimer `src/GWGUI.Emulation.Sharp/Emulators/Common/Interop/Services/ContentPath.cs` et `src/GWGUI.Emulation.Sharp/Emulators/Common/Interop/Services/LoadedContent.cs` après raccordement, sans toucher aux contenus Thomson/Watara qui assurent une écriture différée distincte.
  - [x] Modifier `src/GWGUI.Emulation.Philips/Emulators/Common/Interop/Functions/MediaFunctions.Content.cs` et `src/GWGUI.Emulation.Philips/EmulationGlobalUsings.cs` pour utiliser le propriétaire SDK après Validate ; supprimer `src/GWGUI.Emulation.Philips/Emulators/Common/Interop/Services/ContentPath.cs` et `src/GWGUI.Emulation.Philips/Emulators/Common/Interop/Services/LoadedContent.cs` après raccordement, sans toucher aux contenus Thomson/Watara qui assurent une écriture différée distincte.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Emulators/Common/Interop/Functions/MediaFunctions.Content.cs` et `src/GWGUI.Emulation.Apple/EmulationGlobalUsings.cs` pour utiliser le propriétaire SDK après Validate ; supprimer `src/GWGUI.Emulation.Apple/Emulators/Common/Interop/Services/ContentPath.cs` et `src/GWGUI.Emulation.Apple/Emulators/Common/Interop/Services/LoadedContent.cs` après raccordement, sans toucher aux contenus Thomson/Watara qui assurent une écriture différée distincte.
  - [x] Modifier `src/GWGUI.Emulation.Bandai/Emulators/Common/Interop/Functions/MediaFunctions.Content.cs` et `src/GWGUI.Emulation.Bandai/EmulationGlobalUsings.cs` pour utiliser le propriétaire SDK après Validate ; supprimer `src/GWGUI.Emulation.Bandai/Emulators/Common/Interop/Services/ContentPath.cs` et `src/GWGUI.Emulation.Bandai/Emulators/Common/Interop/Services/LoadedContent.cs` après raccordement, sans toucher aux contenus Thomson/Watara qui assurent une écriture différée distincte.
  - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/Common/Interop/Functions/MediaFunctions.Content.cs` et `src/GWGUI.Emulation.Atari/EmulationGlobalUsings.cs` pour utiliser le propriétaire SDK après Validate ; supprimer `src/GWGUI.Emulation.Atari/Emulators/Common/Interop/Services/ContentPath.cs` et `src/GWGUI.Emulation.Atari/Emulators/Common/Interop/Services/LoadedContent.cs` après raccordement, sans toucher aux contenus Thomson/Watara qui assurent une écriture différée distincte.
  - [x] Modifier `src/GWGUI.Emulation.Fairchild/Emulators/Common/Interop/Functions/MediaFunctions.Content.cs` et `src/GWGUI.Emulation.Fairchild/EmulationGlobalUsings.cs` pour utiliser le propriétaire SDK après Validate ; supprimer `src/GWGUI.Emulation.Fairchild/Emulators/Common/Interop/Services/ContentPath.cs` et `src/GWGUI.Emulation.Fairchild/Emulators/Common/Interop/Services/LoadedContent.cs` après raccordement, sans toucher aux contenus Thomson/Watara qui assurent une écriture différée distincte.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/Common/Interop/Functions/MediaFunctions.Content.cs` et `src/GWGUI.Emulation.Acorn/EmulationGlobalUsings.cs` pour utiliser le propriétaire SDK après Validate ; supprimer `src/GWGUI.Emulation.Acorn/Emulators/Common/Interop/Services/ContentPath.cs` et `src/GWGUI.Emulation.Acorn/Emulators/Common/Interop/Services/LoadedContent.cs` après raccordement, sans toucher aux contenus Thomson/Watara qui assurent une écriture différée distincte.
  - [x] Modifier `src/GWGUI.Emulation.Epoch/Emulators/Common/Interop/Functions/MediaFunctions.Content.cs` et `src/GWGUI.Emulation.Epoch/EmulationGlobalUsings.cs` pour utiliser le propriétaire SDK après Validate ; supprimer `src/GWGUI.Emulation.Epoch/Emulators/Common/Interop/Services/ContentPath.cs` et `src/GWGUI.Emulation.Epoch/Emulators/Common/Interop/Services/LoadedContent.cs` après raccordement, sans toucher aux contenus Thomson/Watara qui assurent une écriture différée distincte.
  - [x] Modifier `src/GWGUI.Emulation.Mattel/Emulators/Common/Interop/Functions/MediaFunctions.Content.cs` et `src/GWGUI.Emulation.Mattel/EmulationGlobalUsings.cs` pour utiliser le propriétaire SDK après Validate ; supprimer `src/GWGUI.Emulation.Mattel/Emulators/Common/Interop/Services/ContentPath.cs` et `src/GWGUI.Emulation.Mattel/Emulators/Common/Interop/Services/LoadedContent.cs` après raccordement, sans toucher aux contenus Thomson/Watara qui assurent une écriture différée distincte.

- [x] Actualiser la qualification après les regroupements de code natif.
  - [x] Modifier les `RuntimeFunctions.Environment.cs` d’Acorn, Sharp, Sinclair et SNK pour retirer les imports des quatre namespaces ne contenant plus que les enums de langue supprimés.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Emulators/Common/Interop/Functions/RuntimeFunctions.Environment.cs` pour retirer également l’import du namespace Interop.Enums supprimé, les autres enums Apple restant dans Emulators.Common.Enums.
  - [x] Modifier les six `RuntimeFunctions.Environment.cs` de Bandai, Coleco, Epoch, Fairchild, Mattel et Philips pour retirer les imports Common.Enums devenus vides avec la suppression de leur enum de langue.
  - [x] Modifier `src/GWGUI.Emulation.Snk/EmulationGlobalUsings.cs` pour retirer l’alias LoadedContent ; recréer `Emulators/Common/Interop/Services/LoadedContent.cs` en conservant OwnContentDirectory, ContentFilePath et leur libération finale propres à SNK, tout en possédant le composant d’allocations SDK ; modifier `Emulators/Common/Interop/Functions/MediaFunctions.Content.cs` pour construire cet adaptateur spécifique après Validate. Le premier raccordement générique a omis ces deux membres de l’original SNK, révélés par l’analyse complète.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec la vérification SDK, modules, App et tests sans Emit après regroupement des contenus, entrées et environnements.
  - [x] Modifier `docs/reference/reuse-audit/remaining-groups.json` et créer `docs/reference/reuse-audit/qualification.json` avec les corps encore identiques, les dépendances de domaine et les décisions restant à examiner ; modifier `docs/tasks/reuse-project-audit.md` avec les nouveaux doublons confirmés avant leur correction.

Contrôle après ces réparations : SDK, 23 modules, MediaEngine, App et toutes les
sources de GWGUI.Tests : zéro erreur sémantique sans Emit. Les tests ne sont pas
exécutés. Les 561 groupes encore égaux comprennent des adaptateurs courts vers
les traitements désormais partagés ; ils restent à qualifier individuellement.

Nouvelle copie confirmée : seize records ExternalCoreExports ne décrivent que
les delegates SDK ; ils possèdent deux signatures, distinguées uniquement par
LoadGameSpecial. Le propriétaire Atari antérieur à Amstrad avait 21 delegates ;
les douze variantes à 22 imposent aussi LoadGameSpecial. Le SDK possède déjà
ExternalCoreLibrary, ExternalCoreApi et ExternalCoreExportNames : y adapter le
chargement initial et conserver cette exigence par profil, sans table parallèle.

- [x] Réutiliser le chargeur SDK pour les tables de fonctions natives recopiées.
  - [x] Créer `src/GWGUI.Emulation/Contracts/ExternalCoreExports.cs` depuis les 21 delegates Atari antérieurs et ajouter LoadGameSpecial optionnel ; modifier `Services/ExternalCoreLibrary.cs` pour résoudre la table avec une exigence LoadGameSpecial fournie par le profil local.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/Common/Interop/Functions/CoreFunctions.cs`, `src/GWGUI.Emulation.Acorn/Emulators/Common/Interop/Contracts/ExternalCoreContracts.cs` et `src/GWGUI.Emulation.Acorn/EmulationGlobalUsings.cs` pour reprendre la table SDK avec le même caractère obligatoire de LoadGameSpecial ; retirer seulement le record recopié, conserver les autres contrats locaux.
  - [x] Modifier `src/GWGUI.Emulation.Bandai/Emulators/Common/Interop/Functions/CoreFunctions.cs`, `src/GWGUI.Emulation.Bandai/Emulators/Common/Interop/Contracts/ExternalCoreContracts.cs` et `src/GWGUI.Emulation.Bandai/EmulationGlobalUsings.cs` pour reprendre la table SDK avec le même caractère obligatoire de LoadGameSpecial ; retirer seulement le record recopié, conserver les autres contrats locaux.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/Emulators/Common/Interop/Functions/CoreFunctions.cs`, `src/GWGUI.Emulation.Coleco/Emulators/Common/Interop/Contracts/ExternalCoreContracts.cs` et `src/GWGUI.Emulation.Coleco/EmulationGlobalUsings.cs` pour reprendre la table SDK avec le même caractère obligatoire de LoadGameSpecial ; retirer seulement le record recopié, conserver les autres contrats locaux.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Emulators/Common/Interop/Functions/CoreFunctions.cs`, `src/GWGUI.Emulation.Snk/Emulators/Common/Interop/Contracts/ExternalCoreContracts.cs` et `src/GWGUI.Emulation.Snk/EmulationGlobalUsings.cs` pour reprendre la table SDK avec le même caractère obligatoire de LoadGameSpecial ; retirer seulement le record recopié, conserver les autres contrats locaux.
  - [x] Modifier `src/GWGUI.Emulation.Philips/Emulators/Common/Interop/Functions/CoreFunctions.cs`, `src/GWGUI.Emulation.Philips/Emulators/Common/Interop/Contracts/ExternalCoreContracts.cs` et `src/GWGUI.Emulation.Philips/EmulationGlobalUsings.cs` pour reprendre la table SDK avec le même caractère obligatoire de LoadGameSpecial ; retirer seulement le record recopié, conserver les autres contrats locaux.
  - [x] Modifier `src/GWGUI.Emulation.Panasonic/Emulators/Opera/Interop/Functions/CoreFunctions.cs`, `src/GWGUI.Emulation.Panasonic/Emulators/Opera/Interop/Contracts/ExternalCoreContracts.cs` et `src/GWGUI.Emulation.Panasonic/EmulationGlobalUsings.cs` pour reprendre la table SDK avec le même caractère obligatoire de LoadGameSpecial ; retirer seulement le record recopié, conserver les autres contrats locaux.
  - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/Common/Interop/Functions/CoreFunctions.cs`, `src/GWGUI.Emulation.Atari/Emulators/Common/Interop/Contracts/ExternalCoreContracts.cs` et `src/GWGUI.Emulation.Atari/EmulationGlobalUsings.cs` pour reprendre la table SDK avec le même caractère obligatoire de LoadGameSpecial ; retirer seulement le record recopié, conserver les autres contrats locaux.
  - [x] Modifier `src/GWGUI.Emulation.Sinclair/Emulators/Common/Interop/Functions/CoreFunctions.cs`, `src/GWGUI.Emulation.Sinclair/Emulators/Common/Interop/Contracts/ExternalCoreContracts.cs` et `src/GWGUI.Emulation.Sinclair/EmulationGlobalUsings.cs` pour reprendre la table SDK avec le même caractère obligatoire de LoadGameSpecial ; retirer seulement le record recopié, conserver les autres contrats locaux.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Emulators/Common/Interop/Functions/CoreFunctions.cs`, `src/GWGUI.Emulation.Apple/Emulators/Common/Interop/Contracts/ExternalCoreContracts.cs` et `src/GWGUI.Emulation.Apple/EmulationGlobalUsings.cs` pour reprendre la table SDK avec le même caractère obligatoire de LoadGameSpecial ; retirer seulement le record recopié, conserver les autres contrats locaux.
  - [x] Modifier `src/GWGUI.Emulation.Mattel/Emulators/Common/Interop/Functions/CoreFunctions.cs`, `src/GWGUI.Emulation.Mattel/Emulators/Common/Interop/Contracts/ExternalCoreContracts.cs` et `src/GWGUI.Emulation.Mattel/EmulationGlobalUsings.cs` pour reprendre la table SDK avec le même caractère obligatoire de LoadGameSpecial ; retirer seulement le record recopié, conserver les autres contrats locaux.
  - [x] Modifier `src/GWGUI.Emulation.Wenquxing/Emulators/Wqxemu/Interop/Functions/CoreFunctions.cs`, `src/GWGUI.Emulation.Wenquxing/Emulators/Wqxemu/Interop/Contracts/ExternalCoreContracts.cs` et `src/GWGUI.Emulation.Wenquxing/EmulationGlobalUsings.cs` pour reprendre la table SDK avec le même caractère obligatoire de LoadGameSpecial ; retirer seulement le record recopié, conserver les autres contrats locaux.
  - [x] Modifier `src/GWGUI.Emulation.Sharp/Emulators/Common/Interop/Functions/CoreFunctions.cs`, `src/GWGUI.Emulation.Sharp/Emulators/Common/Interop/Contracts/ExternalCoreContracts.cs` et `src/GWGUI.Emulation.Sharp/EmulationGlobalUsings.cs` pour reprendre la table SDK avec le même caractère obligatoire de LoadGameSpecial ; retirer seulement le record recopié, conserver les autres contrats locaux.
  - [x] Modifier `src/GWGUI.Emulation.Fairchild/Emulators/Common/Interop/Functions/CoreFunctions.cs`, `src/GWGUI.Emulation.Fairchild/Emulators/Common/Interop/Contracts/ExternalCoreContracts.cs` et `src/GWGUI.Emulation.Fairchild/EmulationGlobalUsings.cs` pour reprendre la table SDK avec le même caractère obligatoire de LoadGameSpecial ; retirer seulement le record recopié, conserver les autres contrats locaux.
  - [x] Modifier `src/GWGUI.Emulation.Watara/Emulators/Potator/Interop/Functions/CoreFunctions.cs`, `src/GWGUI.Emulation.Watara/Emulators/Potator/Interop/Contracts/ExternalCoreContracts.cs` et `src/GWGUI.Emulation.Watara/EmulationGlobalUsings.cs` pour reprendre la table SDK avec le même caractère obligatoire de LoadGameSpecial ; retirer seulement le record recopié, conserver les autres contrats locaux.
  - [x] Modifier `src/GWGUI.Emulation.Thomson/Emulators/Theodore/Interop/Functions/CoreFunctions.cs`, `src/GWGUI.Emulation.Thomson/Emulators/Theodore/Interop/Contracts/ExternalCoreContracts.cs` et `src/GWGUI.Emulation.Thomson/EmulationGlobalUsings.cs` pour reprendre la table SDK avec le même caractère obligatoire de LoadGameSpecial ; retirer seulement le record recopié, conserver les autres contrats locaux.
  - [x] Modifier `src/GWGUI.Emulation.Epoch/Emulators/Common/Interop/Functions/CoreFunctions.cs`, `src/GWGUI.Emulation.Epoch/Emulators/Common/Interop/Contracts/ExternalCoreContracts.cs` et `src/GWGUI.Emulation.Epoch/EmulationGlobalUsings.cs` pour reprendre la table SDK avec le même caractère obligatoire de LoadGameSpecial ; retirer seulement le record recopié, conserver les autres contrats locaux.

- [x] Réutiliser la lecture des libellés et chemins de disques native antérieure.
  - [x] Modifier `src/GWGUI.Emulation/Functions/ExternalCoreEnvironmentFunctions.cs` avec ReadDiskText depuis AtariDiskControlFunctions de ac7292d86^, en paramétrant la capacité du buffer ; garder les callbacks natifs empruntés et la libération du buffer en finally.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/Common/Interop/Functions/MediaFunctions.Disk.cs` pour déléguer ReadText au lecteur commun avec sa capacité actuelle ; conserver la sélection de disque, ses erreurs et la politique des callbacks vides dans le module.
  - [x] Modifier `src/GWGUI.Emulation.Amstrad/Emulators/Common/Interop/Services/ExternalDiskControl.cs` pour déléguer ReadText au lecteur commun avec sa capacité actuelle ; conserver la sélection de disque, ses erreurs et la politique des callbacks vides dans le module.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Emulators/Common/Interop/Functions/MediaFunctions.Disk.cs` pour déléguer ReadText au lecteur commun avec sa capacité actuelle ; conserver la sélection de disque, ses erreurs et la politique des callbacks vides dans le module.
  - [x] Modifier `src/GWGUI.Emulation.Sinclair/Emulators/Common/Interop/Functions/MediaFunctions.Disk.cs` pour déléguer ReadText au lecteur commun avec sa capacité actuelle ; conserver la sélection de disque, ses erreurs et la politique des callbacks vides dans le module.
  - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/Common/Interop/Services/ExternalDiskControl.cs` pour déléguer ReadText au lecteur commun avec sa capacité actuelle ; conserver la sélection de disque, ses erreurs et la politique des callbacks vides dans le module.
  - [x] Modifier `src/GWGUI.Emulation.Sharp/Emulators/Common/Interop/Functions/MediaFunctions.Disk.cs` pour déléguer ReadText au lecteur commun avec sa capacité actuelle ; conserver la sélection de disque, ses erreurs et la politique des callbacks vides dans le module.
  - [x] Modifier `src/GWGUI.Emulation.Philips/Emulators/Common/Interop/Functions/MediaFunctions.Disk.cs` pour déléguer ReadText au lecteur commun avec sa capacité actuelle ; conserver la sélection de disque, ses erreurs et la politique des callbacks vides dans le module.
  - [x] Modifier `src/GWGUI.Emulation.Commodore/Emulators/Common/Interop/Services/ExternalDiskControl.cs` pour déléguer ReadText au lecteur commun avec sa capacité actuelle ; conserver la sélection de disque, ses erreurs et la politique des callbacks vides dans le module.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/Emulators/Common/Interop/Functions/MediaFunctions.Disk.cs` pour déléguer ReadText au lecteur commun avec sa capacité actuelle ; conserver la sélection de disque, ses erreurs et la politique des callbacks vides dans le module.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Ppsspp/Services/ExternalDiskControl.cs` pour déléguer ReadText au lecteur commun avec sa capacité actuelle ; conserver la sélection de disque, ses erreurs et la politique des callbacks vides dans le module.
  - [x] Modifier `src/GWGUI.Emulation.Mattel/Emulators/Common/Interop/Functions/MediaFunctions.Disk.cs` pour déléguer ReadText au lecteur commun avec sa capacité actuelle ; conserver la sélection de disque, ses erreurs et la politique des callbacks vides dans le module.
  - [x] Modifier `src/GWGUI.Emulation.Fairchild/Emulators/Common/Interop/Functions/MediaFunctions.Disk.cs` pour déléguer ReadText au lecteur commun avec sa capacité actuelle ; conserver la sélection de disque, ses erreurs et la politique des callbacks vides dans le module.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Pcsx2/Services/ExternalDiskControl.cs` pour déléguer ReadText au lecteur commun avec sa capacité actuelle ; conserver la sélection de disque, ses erreurs et la politique des callbacks vides dans le module.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Emulators/Common/Interop/Services/ExternalDiskControl.cs` pour déléguer ReadText au lecteur commun avec sa capacité actuelle ; conserver la sélection de disque, ses erreurs et la politique des callbacks vides dans le module.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Services/ExternalDiskControl.cs` pour déléguer ReadText au lecteur commun avec sa capacité actuelle ; conserver la sélection de disque, ses erreurs et la politique des callbacks vides dans le module.
  - [x] Modifier `src/GWGUI.Emulation.Epoch/Emulators/Common/Interop/Functions/MediaFunctions.Disk.cs` pour déléguer ReadText au lecteur commun avec sa capacité actuelle ; conserver la sélection de disque, ses erreurs et la politique des callbacks vides dans le module.
  - [x] Modifier `src/GWGUI.Emulation.Nec/Emulators/Common/Interop/Services/ExternalDiskControl.cs` pour déléguer ReadText au lecteur commun avec sa capacité actuelle ; conserver la sélection de disque, ses erreurs et la politique des callbacks vides dans le module.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Emulators/Common/Interop/Functions/MediaFunctions.Disk.cs` pour déléguer ReadText au lecteur commun avec sa capacité actuelle ; conserver la sélection de disque, ses erreurs et la politique des callbacks vides dans le module.
  - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/Common/Interop/Functions/MediaFunctions.Disk.cs` pour déléguer ReadText au lecteur commun avec sa capacité actuelle ; conserver la sélection de disque, ses erreurs et la politique des callbacks vides dans le module.

- [x] Reprendre le traitement des cartes MAME antérieur dans HBMAME et MAME 2015.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Emulators/Common/Interop/Functions/MameMediaFunctions.cs` pour partager PrepareMemoryCard avec la validation fournie par le cœur et rendre CompleteMemoryCard statique ; conserver son format 2048 octets et son nom memory-card.neo, déjà présents dans les trois traitements.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Emulators/Mame/Functions/MediaRuntimeFunctions.cs` et `Mame2016/Functions/MediaRuntimeFunctions.cs` pour appeler CompleteMemoryCard sur son propriétaire commun.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Emulators/HbMame/Functions/MediaRuntimeFunctions.cs` et `HbMame/Factories/HbMameMachineFactory.cs` pour reprendre le record et le traitement MAME antérieurs ; conserver ses validations et sa commande native propres.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Emulators/Mame2015/Functions/MediaRuntimeFunctions.cs` et `Mame2015/Factories/Mame2015MachineFactory.cs` pour reprendre le record et le traitement MAME antérieurs ; conserver sa table de parents, ses validations et sa commande native propres.

- [x] Compléter la réutilisation des primitives audio/vidéo Atari antérieures.
  - [x] Modifier `src/GWGUI.Emulation/Functions/EmulationVideoFunctions.cs` pour y reprendre FrameLength, CopyRows et Timestamp d’AtariVideoFunctions présent avant Amstrad.
  - [x] Modifier `src/GWGUI.Emulation/Functions/EmulationAudioFunctions.cs` pour reprendre CopyBatch, SingleFrame et RetainNewestFrames d’AtariAudioFunctions présent avant Amstrad ; conserver le PCM stéréo du contrat AudioChunk.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/EmulationGlobalUsings.cs` pour aliaser les primitives SDK ; supprimer `src/GWGUI.Emulation.Acorn/Emulators/Common/Interop/Functions/VideoFunctions.cs` et `src/GWGUI.Emulation.Acorn/Emulators/Common/Interop/Functions/NativeAudioFunctions.cs` après raccordement, ces fichiers ne contenant que les primitives stéréo et copie de trames identiques.
  - [x] Modifier `src/GWGUI.Emulation.Apple/EmulationGlobalUsings.cs` pour aliaser les primitives SDK ; supprimer `src/GWGUI.Emulation.Apple/Emulators/Common/Interop/Functions/VideoFunctions.cs` et `src/GWGUI.Emulation.Apple/Emulators/Common/Interop/Functions/NativeAudioFunctions.cs` après raccordement, ces fichiers ne contenant que les primitives stéréo et copie de trames identiques.
  - [x] Modifier `src/GWGUI.Emulation.Atari/EmulationGlobalUsings.cs` pour aliaser les primitives SDK ; supprimer `src/GWGUI.Emulation.Atari/Emulators/Common/Interop/Functions/VideoFunctions.cs` et `src/GWGUI.Emulation.Atari/Emulators/Common/Interop/Functions/NativeAudioFunctions.cs` après raccordement, ces fichiers ne contenant que les primitives stéréo et copie de trames identiques.
  - [x] Modifier `src/GWGUI.Emulation.Bandai/EmulationGlobalUsings.cs` pour aliaser les primitives SDK ; supprimer `src/GWGUI.Emulation.Bandai/Emulators/Common/Interop/Functions/VideoFunctions.cs` et `src/GWGUI.Emulation.Bandai/Emulators/Common/Interop/Functions/NativeAudioFunctions.cs` après raccordement, ces fichiers ne contenant que les primitives stéréo et copie de trames identiques.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/EmulationGlobalUsings.cs` pour aliaser les primitives SDK ; supprimer `src/GWGUI.Emulation.Coleco/Emulators/Common/Interop/Functions/VideoFunctions.cs` et `src/GWGUI.Emulation.Coleco/Emulators/Common/Interop/Functions/NativeAudioFunctions.cs` après raccordement, ces fichiers ne contenant que les primitives stéréo et copie de trames identiques.
  - [x] Modifier `src/GWGUI.Emulation.Epoch/EmulationGlobalUsings.cs` pour aliaser les primitives SDK ; supprimer `src/GWGUI.Emulation.Epoch/Emulators/Common/Interop/Functions/VideoFunctions.cs` et `src/GWGUI.Emulation.Epoch/Emulators/Common/Interop/Functions/NativeAudioFunctions.cs` après raccordement, ces fichiers ne contenant que les primitives stéréo et copie de trames identiques.
  - [x] Modifier `src/GWGUI.Emulation.Fairchild/EmulationGlobalUsings.cs` pour aliaser les primitives SDK ; supprimer `src/GWGUI.Emulation.Fairchild/Emulators/Common/Interop/Functions/VideoFunctions.cs` et `src/GWGUI.Emulation.Fairchild/Emulators/Common/Interop/Functions/NativeAudioFunctions.cs` après raccordement, ces fichiers ne contenant que les primitives stéréo et copie de trames identiques.
  - [x] Modifier `src/GWGUI.Emulation.Mattel/EmulationGlobalUsings.cs` pour aliaser les primitives SDK ; supprimer `src/GWGUI.Emulation.Mattel/Emulators/Common/Interop/Functions/VideoFunctions.cs` et `src/GWGUI.Emulation.Mattel/Emulators/Common/Interop/Functions/NativeAudioFunctions.cs` après raccordement, ces fichiers ne contenant que les primitives stéréo et copie de trames identiques.
  - [x] Modifier `src/GWGUI.Emulation.Philips/EmulationGlobalUsings.cs` pour aliaser les primitives SDK ; supprimer `src/GWGUI.Emulation.Philips/Emulators/Common/Interop/Functions/VideoFunctions.cs` et `src/GWGUI.Emulation.Philips/Emulators/Common/Interop/Functions/NativeAudioFunctions.cs` après raccordement, ces fichiers ne contenant que les primitives stéréo et copie de trames identiques.
  - [x] Modifier `src/GWGUI.Emulation.Sharp/EmulationGlobalUsings.cs` pour aliaser les primitives SDK ; supprimer `src/GWGUI.Emulation.Sharp/Emulators/Common/Interop/Functions/VideoFunctions.cs` et `src/GWGUI.Emulation.Sharp/Emulators/Common/Interop/Functions/NativeAudioFunctions.cs` après raccordement, ces fichiers ne contenant que les primitives stéréo et copie de trames identiques.
  - [x] Modifier `src/GWGUI.Emulation.Sinclair/EmulationGlobalUsings.cs` pour aliaser les primitives SDK ; supprimer `src/GWGUI.Emulation.Sinclair/Emulators/Common/Interop/Functions/VideoFunctions.cs` et `src/GWGUI.Emulation.Sinclair/Emulators/Common/Interop/Functions/NativeAudioFunctions.cs` après raccordement, ces fichiers ne contenant que les primitives stéréo et copie de trames identiques.
  - [x] Modifier `src/GWGUI.Emulation.Snk/EmulationGlobalUsings.cs` pour aliaser les primitives SDK ; supprimer `src/GWGUI.Emulation.Snk/Emulators/Common/Interop/Functions/VideoFunctions.cs` et `src/GWGUI.Emulation.Snk/Emulators/Common/Interop/Functions/NativeAudioFunctions.cs` après raccordement, ces fichiers ne contenant que les primitives stéréo et copie de trames identiques.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Common/Machines/Common/Functions/AudioFunctions.cs` pour déléguer SingleFrame et RetainNewestFrames au traitement Atari antérieur ; conserver les paramètres de durée et de taille de buffer propres au module.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Common/Functions/AudioFunctions.cs` pour déléguer SingleFrame et RetainNewestFrames au traitement Atari antérieur ; conserver les paramètres de durée et de taille de buffer propres au module.
  - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/Common/Functions/AudioFunctions.cs` pour déléguer SingleFrame et RetainNewestFrames au traitement Atari antérieur ; conserver les paramètres de durée et de taille de buffer propres au module.
  - [x] Modifier `src/GWGUI.Emulation.Bandai/Common/Functions/AudioFunctions.cs` pour déléguer SingleFrame et RetainNewestFrames au traitement Atari antérieur ; conserver les paramètres de durée et de taille de buffer propres au module.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/Common/Functions/AudioFunctions.cs` pour déléguer SingleFrame et RetainNewestFrames au traitement Atari antérieur ; conserver les paramètres de durée et de taille de buffer propres au module.
  - [x] Modifier `src/GWGUI.Emulation.Epoch/Common/Functions/AudioFunctions.cs` pour déléguer SingleFrame et RetainNewestFrames au traitement Atari antérieur ; conserver les paramètres de durée et de taille de buffer propres au module.
  - [x] Modifier `src/GWGUI.Emulation.Fairchild/Common/Functions/AudioFunctions.cs` pour déléguer SingleFrame et RetainNewestFrames au traitement Atari antérieur ; conserver les paramètres de durée et de taille de buffer propres au module.
  - [x] Modifier `src/GWGUI.Emulation.Mattel/Common/Machines/Common/Functions/AudioFunctions.cs` pour déléguer SingleFrame et RetainNewestFrames au traitement Atari antérieur ; conserver les paramètres de durée et de taille de buffer propres au module.
  - [x] Modifier `src/GWGUI.Emulation.Panasonic/Common/Machines/Common/Functions/AudioFunctions.cs` pour déléguer RetainNewestFrames (SingleFrame est absent de ce fichier) au traitement Atari antérieur ; conserver les paramètres de durée et de taille de buffer propres au module.
  - [x] Modifier `src/GWGUI.Emulation.Philips/Common/Machines/Common/Functions/AudioFunctions.cs` pour déléguer SingleFrame et RetainNewestFrames au traitement Atari antérieur ; conserver les paramètres de durée et de taille de buffer propres au module.
  - [x] Modifier `src/GWGUI.Emulation.Sharp/Common/Machines/Common/Functions/AudioFunctions.cs` pour déléguer SingleFrame et RetainNewestFrames au traitement Atari antérieur ; conserver les paramètres de durée et de taille de buffer propres au module.
  - [x] Modifier `src/GWGUI.Emulation.Sinclair/Common/Machines/Common/Functions/AudioFunctions.cs` pour déléguer SingleFrame et RetainNewestFrames au traitement Atari antérieur ; conserver les paramètres de durée et de taille de buffer propres au module.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Common/Machines/Common/Functions/AudioFunctions.cs` pour déléguer SingleFrame et RetainNewestFrames au traitement Atari antérieur ; conserver les paramètres de durée et de taille de buffer propres au module.
  - [x] Modifier `src/GWGUI.Emulation.Thomson/Common/Machines/Common/Functions/AudioFunctions.cs` pour déléguer SingleFrame et RetainNewestFrames au traitement Atari antérieur ; conserver les paramètres de durée et de taille de buffer propres au module.
  - [x] Modifier `src/GWGUI.Emulation.Watara/Common/Machines/Common/Functions/AudioFunctions.cs` pour déléguer SingleFrame et RetainNewestFrames au traitement Atari antérieur ; conserver les paramètres de durée et de taille de buffer propres au module.
  - [x] Modifier `src/GWGUI.Emulation.Wenquxing/Common/Machines/Common/Functions/AudioFunctions.cs` pour déléguer SingleFrame et RetainNewestFrames au traitement Atari antérieur ; conserver les paramètres de durée et de taille de buffer propres au module.

- [ ] Vérifier les derniers raccordements et qualifier les adaptateurs restants.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/Emulators/Common/Interop/Services/ExternalCore.cs`, `src/GWGUI.Emulation.Snk/Emulators/FbAlpha/Factories/FbAlphaMachineFactory.cs` et `FbNeo/Factories/FbNeoMachineFactory.cs` pour expliciter au compilateur que LoadGameSpecial est obligatoire dans ces trois profils, vérifié par ResolveExports(true), sans changer le chargement natif.
  - [x] Modifier `src/GWGUI.Emulation/Functions/ExternalCoreEnvironmentFunctions.cs` pour convertir explicitement la capacité int en nuint attendue par les callbacks de texte disque ; les anciennes constantes étaient converties implicitement, contrairement au paramètre partagé.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec l’analyse sémantique SDK, modules, MediaEngine, App et tests sans Emit après les modifications exports/disques/cartes/audio/vidéo.
  - [x] Créer temporairement `artifacts/reuse-audit-code-baseline.tar` et `artifacts/reuse-audit-code-baseline/` avec les seuls fichiers C#/PowerShell/Python/CMD de `ac7292d86^`, sans documents de tâches ; créer `docs/reference/reuse-audit/additional-code-groups.json` avec les candidats des fichiers C# hors src et les fonctions PowerShell, en distinguant les corps déjà présents avant Amstrad.
  - [x] Modifier `src/GWGUI.Emulation/Functions/ExternalCoreEnvironmentFunctions.cs` pour partager la lecture des ports avec les deux politiques antérieures : Atari limite le nombre de types par troncature, Amiga refuse le dépassement et ignore les noms vides. Ajouter TryCopyControllerPorts avec ces paramètres, sans altérer le chemin Atari existant.
  - [x] Modifier les huit `ExternalHostCallbacks.Environment.cs` d’Amstrad, Commodore, NEC Common, Nintendo, Sega et Sony Common/Pcsx2/Ppsspp pour reprendre le lecteur Amiga antérieur, adapté dans le composant commun ; préserver leurs plafonds, le retour false et les anciens ports en cas de dépassement.
  - [x] Modifier `src/GWGUI.Emulation.Commodore/Emulators/Common/Interop/Services/ExternalHostCallbacks.Environment.cs` pour convertir explicitement son plafond uint de types en int vérifié, type attendu par le lecteur partagé ; conserver sa valeur de profil.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec la vérification des deux politiques de lecture de ports après raccordement et l’analyse sémantique des consommateurs sans Emit.
  - [x] Modifier les huit paires `ExternalHostCallbacks.cs` / `.Environment.cs` d’Amstrad, Commodore, NEC Common, Nintendo, Sega et Sony Common/Pcsx2/Ppsspp pour utiliser le propriétaire de chaînes `ExternalCoreUtf8String` du SDK à la place de leurs dictionnaires de pointeurs bruts ; utiliser AddTo, conserver le cache et la fermeture dans leurs finally existants.
  - [x] Modifier `src/GWGUI.Emulation.Commodore/Emulators/Common/Interop/Services/ExternalHostCallbacks.cs` pour terminer la libération du cache et sa remise à zéro en finally même si une chaîne échoue ; conserver la remontée de la première erreur après avoir tenté chaque libération. Les sept autres caches possèdent déjà leur finally de fermeture.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec le contrôle de fermeture des huit caches et l’analyse sémantique après leur raccordement.
  - [x] Modifier `src/GWGUI.MediaEngine/Functions/BitPrimitives.cs` pour y extraire l’écriture little-endian 24 bits ajoutée deux fois dans f2fb3caf3 ; modifier `Images/Formats/Tape/Voc/VocWriter.cs` et `Images/Writing/Encoding/Sequential/Amstrad/AmstradCpcTapeEncoder.cs` pour appeler ce propriétaire commun existant, puis supprimer leurs deux méthodes privées recopiées.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Emulators/Common/Interop/Functions/MameMediaFunctions.cs` pour rendre sa validation Argument accessible aux adaptateurs ; modifier les `MediaRuntimeFunctions.cs` HBMAME et MAME 2015 pour l’appeler et supprimer leurs fonctions et constantes de capacité Argument recopiées, toutes fixées à 1024 octets. Conserver leurs limites de commande natives distinctes.
  - [x] Modifier `docs/reference/reuse-audit/remaining-groups.json` et `qualification.json` pour inclure aussi les candidats antérieurs à Amstrad, distinguer les adaptateurs appelant désormais le SDK et inscrire les candidats encore non qualifiés, sans exclure leur date.
  - [x] Créer `scripts/temp/common/DotNetProcess.ps1` depuis Invoke-DotNetProcess introduit dans `2bf15b134` ; modifier `scripts/temp/extract-media-files.ps1` et `read-media-with-libretro.ps1` pour charger cette fonction et supprimer leurs copies. Raccorder également `analyze-media-data.ps1`, qui possède la même gestion du processus avec un message de démarrage français à conserver par paramètre. Protéger Dispose par finally même si Kill échoue.
  - [x] Modifier `src/GWGUI.App/Services/PhysicalDiskReading/InternalPhysicalDiskReader.cs` pour rendre son convertisseur CreateOptions accessible en interne ; modifier `tests/GWGUI.Tests/Hardware/PhysicalReading/ReadAcquisitionScenarios.cs` pour l’appeler à la place des copies CreateProviderOptions/AddMilliseconds. Les deux convertisseurs sont présents dans `8ea733760` ; les assertions de séquences matérielles des tests restent indépendantes.
  - [x] Modifier `tests/GWGUI.Tests/Hardware/PhysicalReading/ReadAcquisitionScenarios.cs` pour importer explicitement le namespace du lecteur réutilisé ; modifier `scripts/temp/common/DotNetProcess.ps1` pour aligner l’indentation de son finally ; modifier `docs/tasks/reuse-project-audit.md` avec la vérification syntaxique PowerShell et sémantique C# sans Emit après ces raccordements et WriteUInt24.
  - [x] Créer `docs/reference/reuse-audit/current-project-groups.json` en recalculant les correspondances de tous les fichiers C# et PowerShell actuels du dépôt, sans limiter la recherche aux groupes historiques ou à src ; enregistrer aussi les correspondances où les identifiants ont changé, à qualifier manuellement avant correction.
  - [x] Modifier `src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationSettingsFieldControlFunctions.cs` et créer sa partie `.Video.cs` pour reprendre Slider, Choice et Field du bloc FixedPixel initial `e35d81e88` ; paramétrer les minimums, maximums, pas, largeurs et marges des consommateurs, conserver la notification continue des filtres distincte de la sauvegarde différée des paramètres des émulateurs.
  - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationFixedPixelSettingsBlock.cs`, `EmulationPlasmaSettingsBlock.cs`, `EmulationVectorSettingsBlock.cs`, `EmulationDotMatrixSettingsBlock.cs`, `EmulationEPaperSettingsBlock.cs`, `EmulationProjectionSettingsBlock.cs`, `EmulationSegmentDisplaySettingsBlock.cs`, `EmulationLedMatrixSettingsBlock.cs` et `EmulationVfdSettingsBlock.cs` pour appeler ces champs communs et supprimer leurs records ChoiceValue locaux ; préserver les marges FixedPixel, la largeur Plasma/Vector, les pas de durée et la désactivation conditionnelle VFD.
  - [x] Modifier `src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationVideoSettingsLayout.cs` pour y reprendre la construction des cadres FixedPixel de `e35d81e88`, avec titre optionnel, position, étendue et marges fournies par le bloc ; modifier les neuf `Emulation*SettingsBlock.cs` cités ci-dessus pour l’appeler. Supprimer leurs méthodes Field devenues inutilisées ; raccorder directement le checkbox SegmentDisplay à CreateVideoField.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec les profils de champs et cadres préservés et le résultat de l’analyse sémantique App sans Emit après mutualisation des neuf blocs.
  - [x] Créer `tests/GWGUI.LocalDiskImageTests/MediaAuditTree.cs` depuis Flatten du Program initial `2bf15b134`, avec la lecture des enfants fournie pour conserver les types FileEntryAudit et FileSystemEntry ; modifier `Program.cs`, `MediaAuditValidator.cs`, `MediaAuditValidatorSelfTests.cs`, `TemporaryMediaSignatureCatalog.cs`, `TemporaryNamedMediaFormatIdentification.cs` et `MediaFileExtractor/Program.cs` pour l’appeler et supprimer les six copies ; modifier `MediaFileExtractor/GWGUI.MediaFileExtractor.csproj` pour lier ce fichier source commun sans dépendance envers l’exécutable d’audit.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec la vérification sémantique sans Emit des deux outils après raccordement du parcours commun, puis actualiser `docs/reference/reuse-audit/current-project-groups.json` pour retirer les copies corrigées.
  - [x] Modifier `scripts/temp/common/DotNetProcess.ps1` pour placer l’affectation StartInfo dans le try propriétaire du Process et corriger l’indentation de son finally, afin que Dispose reste garanti dès sa création ; modifier `docs/tasks/reuse-project-audit.md` avec le contrôle syntaxique final.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec les corrections concrètes des doublons encore présents, après comparaison de leurs propriétaires et consommateurs dans tous les projets ; inclure les contrôles d’affichage, les outils et les tests, et inscrire chaque fichier à modifier avant sa correction.
  - [x] Modifier `src/GWGUI.MediaEngine/Images/Models/Blocks/BlockMediaImageRepresentation.cs` pour reprendre AddRange du lecteur VHD et le contrôle de couverture présent dans les writers depuis `8ea733760`, commit d’introduction commun. Conserver l’addition checked du VHD et l’addition non vérifiée des cinq autres lecteurs au moyen d’un paramètre ; parcourir les plages déjà triées et immuables du modèle pour la couverture.
  - [x] Modifier les lecteurs `src/GWGUI.MediaEngine/Images/Formats/HardDisk/Chd/ChdHardDiskReader.cs`, `Qcow2/Qcow2Reader.cs`, `Vdi/VdiReader.cs`, `Vhd/VhdReader.cs`, `Vhdx/VhdxReader.cs` et `Vmdk/VmdkReader.cs` pour appeler AddRange sur le modèle existant et supprimer leurs six copies ; préserver le transfert de propriété des sources de données.
  - [x] Modifier les writers `src/GWGUI.MediaEngine/Images/Formats/HardDisk/Chd/ChdHardDiskWriter.cs`, `Qcow2/Qcow2Writer.cs`, `Vdi/VdiWriter.cs`, `Vhd/VhdWriter.cs`, `Vhdx/VhdxWriter.cs`, `Vmdk/VmdkWriter.cs` et `Raw/RawHardDiskWriter.cs` pour appeler le contrôle de couverture du modèle et supprimer leurs sept boucles ; conserver dans Raw ses gardes de capacité et de taille de bloc propres.
  - [x] Modifier `src/GWGUI.MediaEngine/Images/Reading/Blocks/BlockMediaDataReader.cs` pour y reprendre WriteLogicalDataAsync du VHD initial `8ea733760` sous CopyToAsync avec le buffer de 64 Kio et les mêmes lectures/écritures ; modifier les writers VHD et VMDK pour l’appeler et supprimer leurs copies. Le flux de sortie est emprunté et reste sous la responsabilité de l’appelant ; conserver le writer Raw, dont les états inconnus ne sont pas traités comme dans le lecteur logique.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec la comparaison des profils HDD et le contrôle sémantique de MediaEngine sans Emit après raccordement.
  - [x] Modifier `src/GWGUI.MediaEngine/Images/Reading/Blocks/BlockMediaDataReader.cs` pour y reprendre CopyAsync du writer Spectrum TAP introduit dans `8ea733760` sous une surcharge CopyToAsync(MediaDataRange, Stream, CancellationToken), sans changer le buffer de 64 Kio ni fermer la source ou le flux empruntés ; modifier les writers `Images/Formats/Tape/MsxCas/MsxCasWriter.cs`, `SpectrumTap/SpectrumTapWriter.cs`, `Tzx/TzxWriter.cs` et `Uef/UefWriter.cs` pour l’appeler et supprimer leurs quatre copies introduites dans le même commit.
  - [x] Modifier `src/GWGUI.MediaEngine/Functions/PcmSampleFunctions.cs` pour y reprendre TryReadSampleRate des encodeurs initiaux `8ea733760` en réutilisant son TryReadInt antérieur, avec le seuil existant de 8000 Hz ; modifier les encodeurs `Images/Writing/Encoding/Sequential/Acorn/AcornTapeEncoder.cs`, `Commodore/CommodoreTapeEncoder.cs`, `Msx/MsxTapeEncoder.cs`, `Spectrum/SpectrumTapeEncoder.cs` pour l’appeler et supprimer leurs quatre copies ; modifier `Amstrad/AmstradCpcTapeEncoder.cs` pour utiliser cette validation en conservant son repli DefaultSampleRate.
  - [x] Modifier `src/GWGUI.MediaEngine/Images/Reading/Blocks/BlockMediaDataReader.cs` pour y reprendre ReadRangeAsync du décodeur Spectrum initial `8ea733760`, avec une surcharge pour la plage complète ; modifier les décodeurs `Images/Reading/Decoding/Sequential/Acorn/AcornTapeDecoder.cs`, `Msx/MsxTapeDecoder.cs` et `Spectrum/SpectrumTapeDecoder.cs` pour l’appeler et supprimer leurs boucles de lecture. Conserver le retour null, la limite int.MaxValue et les validations d’offset existantes.
  - [x] Modifier `src/GWGUI.MediaEngine/Contracts/SequentialMediaSegment.cs` pour y reprendre Clone, déjà identique dans les encodeurs Atari et TZX initiaux `8ea733760`, sous AtPosition ; modifier `Images/Writing/Encoding/Sequential/Atari/AtariCassetteEncoder.cs` et `Tzx/TzxSignalEncoder.cs` pour appeler cette méthode et supprimer leurs copies.
  - [x] Créer `src/GWGUI.Updates/Services/UpdateJsonOptions.cs` depuis CreateJsonOptions présent dès `2db16a34f`, dans le projet Updates déjà référencé par App et Updater ; modifier les six services App `ApplicationUpdateService.cs`, `ModuleDirectoryService.cs`, `ModuleInstallationService.cs`, `ModuleUpdateService.cs`, `UpdatePackagePreparationService.cs`, `UpdateStartupCoordinator.cs` et `src/GWGUI.Updater/UpdatePlanReader.cs` pour appeler cette fabrique et supprimer leurs sept copies. Chaque appel retourne une instance indépendante avec les mêmes options Web et enums camelCase ; ne pas modifier le profil distinct UpdateArchiveValidator.
  - [x] Modifier `src/GWGUI.App/ViewModels/Operations/Options/OperationOptionViewModelBase.cs` pour y reprendre CaptureEnabledOptions, CaptureValues et la boucle ApplyOptions du WriteOperationViewModel initial `ba22eebf9`, antérieur à la copie Conversion `eea7343fb` ; modifier `ViewModels/Operations/WriteOperationViewModel.cs` et `ViewModels/Conversion/ConversionOperationViewModel.cs` pour appeler ces opérations sur leurs listes propres. Conserver l’affectation ExpertArguments et ses notifications dans chaque modèle ; ne pas fusionner leurs options BuildOptions différentes.
  - [x] Modifier `src/GWGUI.App/Rendering/Emulation/Processing/FilterBilinear.cs` et `FilterBicubic.cs` pour utiliser Read déjà présent dans FilterSampling dès leur introduction commune `66dc8c791`, puis supprimer leurs copies ; supprimer Sample inutilisé de `Software/SoftwareEmulationVideoProcessingPipeline.Resampling.cs` après vérification de tous les partiels. Conserver Lerp, utilisé par les traitements temporels.
  - [x] Modifier `src/GWGUI.App/Rendering/Emulation/Processing/FilterSampling.cs` pour reprendre Read de FilterXbr initial `66dc8c791` sous ReadSrgbColor ; modifier `FilterXbr.cs` et `FilterXbrz.cs` pour l’appeler et supprimer leurs copies. Conserver la conversion LinearToSrgb, distincte du ReadColor linéaire existant.
  - [x] Modifier `src/GWGUI.App/Rendering/Emulation/Processing/FilterSampling.cs` pour reprendre Sample de FilterVectorBeamFocus initial `41038682f` sous ReadScalar ; modifier `FilterVectorBeamFocus.cs`, `FilterVectorBeamWidth.cs`, `FilterVectorHalo.cs` et `FilterVectorLineDetection.cs` pour l’appeler et supprimer les quatre copies ; conserver l’indexation à un canal, distincte du RGB à trois canaux.
  - [x] Modifier `src/GWGUI.App/Rendering/Emulation/Processing/FilterSampling.cs` pour reprendre SmoothStep du Plasma initial `41038682f` ; modifier `FilterPlasmaAutomaticBrightnessLimiter.cs`, `FilterPlasmaBlackDepth.cs`, `FilterPlasmaCellStructure.cs`, `FilterDotMatrix.cs` et `FilterLedMatrixCellStructure.cs` pour l’appeler et supprimer leurs copies, sans modifier les shaders.
  - [x] Modifier `src/GWGUI.App/Rendering/Emulation/Processing/FilterSampling.cs` pour reprendre Distance SABR/ScaleFX initial `66dc8c791` avec les six composantes flottantes fournies en paramètres ; modifier `FilterSabr.cs` et `FilterScaleFx.cs` pour y déléguer leur métrique et conserver leurs types Color propres ainsi que l’ordre des opérations.
  - [x] Modifier l’indentation des expressions de délégation et l’espacement des nouveaux membres dans les fichiers C# modifiés depuis `b96c7629b`, sans reformater les fichiers voisins ; modifier `docs/tasks/reuse-project-audit.md` avec le contrôle des littéraux de shaders avant/après et de diff --check.
  - [x] Modifier `docs/tasks/reuse-project-audit.md` avec le contrôle sémantique sans Emit des projets modifiés et de leurs consommateurs après ces raccordements ; modifier `docs/reference/reuse-audit/current-project-groups.json` avec le relevé actualisé ; la qualification générale reste ouverte.
  - [x] Qualifier et corriger les candidats suivants du relevé courant.
    - [x] Modifier `docs/tasks/reuse-project-audit.md` avec les origines des snapshots OpenGL/Veldrid (`66dc8c791`) et des options Greaseweazle (`8ea733760`) ; inscrire leurs raccordements au composant existant ou leur extraction avant modification des sources.
    - [x] Modifier `src/GWGUI.App/Functions/Rendering/Emulation/EmulationVideoSnapshotFunctions.cs` pour reprendre EnsureSnapshot/UpdateSnapshot des surfaces initiales sous Ensure avec un cache fourni par référence et un pipeline emprunté ; réutiliser EmulationVideoSurfaceFrameFunctions.Process pour ce chemin et CreateAsync. Modifier `Rendering/Emulation/Surfaces/OpenGlVideoSurface.cs` et `VeldridVideoSurface.cs` pour déléguer EnsureSnapshot et supprimer UpdateSnapshot. Conserver les caches et les pipelines propres à chaque surface, leur invalidation, leurs finally et les bitmaps gelés du chemin asynchrone.
    - [x] Créer `src/GWGUI.Infrastructure/Hardware/Greaseweazle/GreaseweazleMediaOptionFunctions.cs` depuis ReadRequired et ReadEnum du provider initial `8ea733760` ; modifier `GreaseweazleMediaAcquisitionProvider.cs` et `GreaseweazleMediaPhysicalWriter.cs` pour les appeler et supprimer leurs copies. Conserver les parsers entiers et booléens distincts, dont les messages d’erreur diffèrent.
    - [x] Modifier `docs/tasks/reuse-project-audit.md` avec la vérification sémantique sans Emit d’Infrastructure/App et les origines des boucles hôtes : canaux et lecture de commandes Amiga dans `e0a41e045`, puis adaptation Atari dans `d6277ffa1`. Les messages, formats de réponse et cœurs restent propres aux modules ; la fermeture du cœur précède celle des canaux.
    - [x] Créer `src/GWGUI.Emulation/Functions/EmulationHostProtocolFunctions.Host.cs` pour reprendre l’ouverture du canal et la boucle de commandes Amiga antérieures, sous RunConnectedSession et RunCommandLoop ; fournir le serveur, le délai, la lecture d’en-tête, la valeur Dispose et les callbacks de réponse par le propriétaire actuel. Fermer writer, reader et pipe en finally après le callback, qui conserve le finally du cœur ; ne pas créer de catalogue, DTO ou état de cœur partagé.
    - [x] Inscrire dans `docs/tasks/reuse-project-audit.md` les 25 fichiers CoreHost à raccorder dans leur ordre d’exécution, avec la suppression des TryReadCommand locaux devenus inutilisés ; préserver les mappings vidéo et les erreurs natives.
    - [x] Raccorder les 25 hôtes aux boucles antérieures du SDK.
      - [x] Modifier `src/GWGUI.Emulation.Acorn/Common/Services/CoreHost.cs` pour appeler RunConnectedSession et RunCommandLoop en conservant serveur, délai, lecteur de commande, réponses, état local et fermeture du cœur avant les canaux ; supprimer son TryReadCommand si devenu inutilisé.
      - [x] Modifier `src/GWGUI.Emulation.Amstrad/Emulators/Common/Services/CoreHost.cs` pour appeler RunConnectedSession et RunCommandLoop en conservant serveur, délai, lecteur de commande, réponses, état local et fermeture du cœur avant les canaux ; supprimer son TryReadCommand si devenu inutilisé.
      - [x] Modifier `src/GWGUI.Emulation.Apple/Common/Services/CoreHost.cs` pour appeler RunConnectedSession et RunCommandLoop en conservant serveur, délai, lecteur de commande, réponses, état local et fermeture du cœur avant les canaux ; supprimer son TryReadCommand si devenu inutilisé.
      - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Services/CoreHost.cs` pour appeler RunConnectedSession et RunCommandLoop en conservant serveur, délai, lecteur de commande, réponses, état local et fermeture du cœur avant les canaux ; supprimer son TryReadCommand si devenu inutilisé.
      - [x] Modifier `src/GWGUI.Emulation.Bandai/Common/Services/CoreHost.cs` pour appeler RunConnectedSession et RunCommandLoop en conservant serveur, délai, lecteur de commande, réponses, état local et fermeture du cœur avant les canaux ; supprimer son TryReadCommand si devenu inutilisé.
      - [x] Modifier `src/GWGUI.Emulation.Coleco/Common/Services/CoreHost.cs` pour appeler RunConnectedSession et RunCommandLoop en conservant serveur, délai, lecteur de commande, réponses, état local et fermeture du cœur avant les canaux ; supprimer son TryReadCommand si devenu inutilisé.
      - [x] Modifier `src/GWGUI.Emulation.Commodore/Emulators/Common/Interop/Services/CoreHost.cs` pour appeler RunConnectedSession et RunCommandLoop en conservant serveur, délai, lecteur de commande, réponses, état local et fermeture du cœur avant les canaux ; supprimer son TryReadCommand si devenu inutilisé.
      - [x] Modifier `src/GWGUI.Emulation.Epoch/Common/Services/CoreHost.cs` pour appeler RunConnectedSession et RunCommandLoop en conservant serveur, délai, lecteur de commande, réponses, état local et fermeture du cœur avant les canaux ; supprimer son TryReadCommand si devenu inutilisé.
      - [x] Modifier `src/GWGUI.Emulation.Fairchild/Common/Services/CoreHost.cs` pour appeler RunConnectedSession et RunCommandLoop en conservant serveur, délai, lecteur de commande, réponses, état local et fermeture du cœur avant les canaux ; supprimer son TryReadCommand si devenu inutilisé.
      - [x] Modifier `src/GWGUI.Emulation.Mattel/Common/Services/CoreHost.cs` pour appeler RunConnectedSession et RunCommandLoop en conservant serveur, délai, lecteur de commande, réponses, état local et fermeture du cœur avant les canaux ; supprimer son TryReadCommand si devenu inutilisé.
      - [x] Modifier `src/GWGUI.Emulation.Nec/Emulators/BeetlePcfx/Services/CoreHost.cs` pour appeler RunConnectedSession et RunCommandLoop en conservant serveur, délai, lecteur de commande, réponses, état local et fermeture du cœur avant les canaux ; supprimer son TryReadCommand si devenu inutilisé.
      - [x] Modifier `src/GWGUI.Emulation.Nec/Emulators/Common/Interop/Services/CoreHost.cs` pour appeler RunConnectedSession et RunCommandLoop en conservant serveur, délai, lecteur de commande, réponses, état local et fermeture du cœur avant les canaux ; supprimer son TryReadCommand si devenu inutilisé.
      - [x] Modifier `src/GWGUI.Emulation.Nintendo/Emulators/Common/Interop/Services/CoreHost.cs` pour appeler RunConnectedSession et RunCommandLoop en conservant serveur, délai, lecteur de commande, réponses, état local et fermeture du cœur avant les canaux ; supprimer son TryReadCommand si devenu inutilisé.
      - [x] Modifier `src/GWGUI.Emulation.Panasonic/Common/Services/CoreHost.cs` pour appeler RunConnectedSession et RunCommandLoop en conservant serveur, délai, lecteur de commande, réponses, état local et fermeture du cœur avant les canaux ; supprimer son TryReadCommand si devenu inutilisé.
      - [x] Modifier `src/GWGUI.Emulation.Philips/Common/Services/CoreHost.cs` pour appeler RunConnectedSession et RunCommandLoop en conservant serveur, délai, lecteur de commande, réponses, état local et fermeture du cœur avant les canaux ; supprimer son TryReadCommand si devenu inutilisé.
      - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/Common/Interop/Services/CoreHost.cs` pour appeler RunConnectedSession et RunCommandLoop en conservant serveur, délai, lecteur de commande, réponses, état local et fermeture du cœur avant les canaux ; supprimer son TryReadCommand si devenu inutilisé.
      - [x] Modifier `src/GWGUI.Emulation.Sharp/Common/Services/CoreHost.cs` pour appeler RunConnectedSession et RunCommandLoop en conservant serveur, délai, lecteur de commande, réponses, état local et fermeture du cœur avant les canaux ; supprimer son TryReadCommand si devenu inutilisé.
      - [x] Modifier `src/GWGUI.Emulation.Sinclair/Common/Services/CoreHost.cs` pour appeler RunConnectedSession et RunCommandLoop en conservant serveur, délai, lecteur de commande, réponses, état local et fermeture du cœur avant les canaux ; supprimer son TryReadCommand si devenu inutilisé.
      - [x] Modifier `src/GWGUI.Emulation.Snk/Common/Services/CoreHost.cs` pour appeler RunConnectedSession et RunCommandLoop en conservant serveur, délai, lecteur de commande, réponses, état local et fermeture du cœur avant les canaux ; supprimer son TryReadCommand si devenu inutilisé.
      - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Services/CoreHost.cs` pour appeler RunConnectedSession et RunCommandLoop en conservant serveur, délai, lecteur de commande, réponses, état local et fermeture du cœur avant les canaux ; supprimer son TryReadCommand si devenu inutilisé.
      - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Pcsx2/Services/CoreHost.cs` pour appeler RunConnectedSession et RunCommandLoop en conservant serveur, délai, lecteur de commande, réponses, état local et fermeture du cœur avant les canaux ; supprimer son TryReadCommand si devenu inutilisé.
      - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Ppsspp/Services/CoreHost.cs` pour appeler RunConnectedSession et RunCommandLoop en conservant serveur, délai, lecteur de commande, réponses, état local et fermeture du cœur avant les canaux ; supprimer son TryReadCommand si devenu inutilisé.
      - [x] Modifier `src/GWGUI.Emulation.Thomson/Common/Services/CoreHost.cs` pour appeler RunConnectedSession et RunCommandLoop en conservant serveur, délai, lecteur de commande, réponses, état local et fermeture du cœur avant les canaux ; supprimer son TryReadCommand si devenu inutilisé.
      - [x] Modifier `src/GWGUI.Emulation.Watara/Common/Services/CoreHost.cs` pour appeler RunConnectedSession et RunCommandLoop en conservant serveur, délai, lecteur de commande, réponses, état local et fermeture du cœur avant les canaux ; supprimer son TryReadCommand si devenu inutilisé.
      - [x] Modifier `src/GWGUI.Emulation.Wenquxing/Common/Services/CoreHost.cs` pour appeler RunConnectedSession et RunCommandLoop en conservant serveur, délai, lecteur de commande, réponses, état local et fermeture du cœur avant les canaux ; supprimer son TryReadCommand si devenu inutilisé.
    - [x] Créer `docs/reference/reuse-audit/host-loop-comparison.json` en comparant les AST des 25 hôtes avant/après depuis `f312811bc` : corps des commandes, réponses d’erreur, finally du cœur, préfixe vidéo, serveur et délai ; conserver les empreintes comme preuve et corriger toute différence involontaire avant validation.
    - [x] Modifier `docs/reference/reuse-audit/host-loop-comparison.json` avec la comparaison des lecteurs de commandes et valeurs Dispose, des lecteurs/writers UTF-8 avec leaveOpen, et la présence du finally du cœur dans le callback exécuté avant fermeture des canaux.
    - [x] Modifier `docs/tasks/reuse-project-audit.md` avec l’analyse sémantique du SDK et des consommateurs après raccordement des boucles ; poursuivre la qualification des notifications dans un groupe suivant.
  - [x] Reprendre les notifications et options des modèles antérieurs.
    - [x] Créer `src/GWGUI.App/ViewModels/ObservableObject.cs` depuis Set et PropertyChanged du MainWindowViewModel initial `832932d91`, antérieur aux modèles Read/Write/Conversion ; y isoler OnPropertyChanged protégé et Set sans allocation de callback ni état global. Conserver une notification seulement après changement pour Set et les notifications explicites inconditionnelles pour OnPropertyChanged.
    - [x] Modifier `ViewModels/Main/MainWindowViewModel.cs`, `ViewModels/Operations/ReadOperationViewModel.cs`, `WriteOperationViewModel.cs` et `ViewModels/Conversion/ConversionOperationViewModel.cs` dans `src/GWGUI.App` pour hériter du propriétaire commun et retirer leurs Set/événements recopiés ; conserver dans Read la notification supplémentaire SequenceKind après SequenceKindIndex et l’ordre des événements.
    - [x] Modifier `src/GWGUI.App/ViewModels/Operations/Options/OperationOptionViewModelBase.cs` et `ValueOptionViewModel.cs` pour réutiliser les notifications et Set communs ; conserver Changed comme point d’appel protégé pour les dérivés existants.
    - [x] Modifier `src/GWGUI.App/ViewModels/Input/InputBindingRow.cs`, `ViewModels/Visualization/TrackSegment.cs` et `Views/Controls/Options/ControllerPresentation/ControllerInputRow.cs` pour hériter du propriétaire commun et supprimer leurs événements et wrappers de notification recopiés ; conserver leurs notifications inconditionnelles et dépendantes.
    - [x] Modifier `src/GWGUI.App/Localization/Sources/LocalizationSource.cs` pour utiliser le même événement de notification par instance, en conservant les notifications Version puis Item[] et son compteur existant.
    - [x] Modifier `src/GWGUI.App/ViewModels/Operations/Options/OperationOptionViewModelBase.cs` pour reprendre les captures et ApplyOptions du Read initial `d0962bd99`, plus ancien que Write, dans les fonctions communes existantes ; y reprendre aussi BuildOptions avec l’exclusion optionnelle NoVerify fournie par Write. Modifier Read/Write/Conversion pour appeler ces opérations sans modifier leurs listes ou leurs arguments expert.
    - [x] Créer `tests/GWGUI.Tests/Interface/NotificationTests.cs` avec des cas autonomes vérifiant le nom et l’émetteur de notification des opérations, les égalités de valeurs, l’ordre SequenceKindIndex/SequenceKind, les répétitions inconditionnelles de Binding et les événements Version/Item[] ; ne créer ni fichier de données, ni fenêtre, ni thread ou chargement de DLL externe.
    - [x] Modifier les 25 `CoreHost.cs` raccordés ci-dessus pour supprimer les espaces de fin de ligne introduits par la remise en indentation des corps ; conserver les fins de ligne et le contenu des chaînes. Modifier `docs/tasks/reuse-project-audit.md` avec le résultat de diff --check.
    - [x] Modifier `docs/tasks/reuse-project-audit.md` avec la comparaison des profils de notification et l’analyse sémantique App/tests sans Emit, en indiquant que les tests ajoutés ne sont pas exécutés ; modifier `docs/reference/reuse-audit/current-project-groups.json` avec les candidats restants.
  - [x] Raccorder les installations restantes aux mécanismes antérieurs.
    - [x] Modifier `src/GWGUI.Emulation/Functions/EmulationCoreReleaseFunctions.cs` pour adapter l’extraction antérieure avec une option conservant ExtractToFile et son horodatage, sans changer les appels Atari existants ; modifier `src/GWGUI.Emulation/Functions/EmulationCoreDiagnosticFunctions.cs` avec la validation PE Amiga de `aa0ef2450` et ses trois messages fournis par le module.
    - [x] Modifier `src/GWGUI.Emulation.Amstrad/Emulators/Common/Services/CoreReleaseService.cs` pour remplacer le téléchargement recopié par DownloadAsync commun (81 920 octets, progression seulement si total positif), l’extraction par le lecteur ZIP commun en conservant ExtractToFile, Hash par CalculateSha256 et, si présente, la validation PE par celle d’Amiga ; conserver les manifestes, chemins, erreurs, nettoyage et vérifications spécifiques.
    - [x] Modifier `src/GWGUI.Emulation.Commodore/Emulators/Common/Interop/Services/CoreReleaseService.cs` pour remplacer le téléchargement recopié par DownloadAsync commun (81 920 octets, progression seulement si total positif), l’extraction par le lecteur ZIP commun en conservant ExtractToFile, Hash par CalculateSha256 et, si présente, la validation PE par celle d’Amiga ; conserver les manifestes, chemins, erreurs, nettoyage et vérifications spécifiques.
    - [x] Modifier `src/GWGUI.Emulation.Nintendo/Emulators/Common/Interop/Services/CoreReleaseService.cs` pour remplacer le téléchargement recopié par DownloadAsync commun (81 920 octets, progression seulement si total positif), l’extraction par le lecteur ZIP commun en conservant ExtractToFile, Hash par CalculateSha256 et, si présente, la validation PE par celle d’Amiga ; conserver les manifestes, chemins, erreurs, nettoyage et vérifications spécifiques.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/Common/Interop/Services/CoreReleaseService.cs` pour remplacer le téléchargement recopié par DownloadAsync commun (81 920 octets, progression seulement si total positif), l’extraction par le lecteur ZIP commun en conservant ExtractToFile, Hash par CalculateSha256 et, si présente, la validation PE par celle d’Amiga ; conserver les manifestes, chemins, erreurs, nettoyage et vérifications spécifiques.
    - [x] Modifier `src/GWGUI.Emulation.Nec/Emulators/BeetlePcfx/Services/CoreReleaseService.cs` pour remplacer le téléchargement recopié par DownloadAsync commun (81 920 octets, progression seulement si total positif), l’extraction par le lecteur ZIP commun en conservant ExtractToFile, Hash par CalculateSha256 et, si présente, la validation PE par celle d’Amiga ; conserver les manifestes, chemins, erreurs, nettoyage et vérifications spécifiques.
    - [x] Modifier `src/GWGUI.Emulation.Nec/Emulators/Common/Interop/Services/CoreReleaseService.cs` pour remplacer le téléchargement recopié par DownloadAsync commun (81 920 octets, progression seulement si total positif), l’extraction par le lecteur ZIP commun en conservant ExtractToFile, Hash par CalculateSha256 et, si présente, la validation PE par celle d’Amiga ; conserver les manifestes, chemins, erreurs, nettoyage et vérifications spécifiques.
    - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Ppsspp/Services/CoreReleaseService.cs` pour remplacer le téléchargement recopié par DownloadAsync commun (81 920 octets, progression seulement si total positif), l’extraction par le lecteur ZIP commun en conservant ExtractToFile, Hash par CalculateSha256 et, si présente, la validation PE par celle d’Amiga ; conserver les manifestes, chemins, erreurs, nettoyage et vérifications spécifiques.
    - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Services/CoreReleaseService.cs` pour remplacer le téléchargement recopié par DownloadAsync commun (81 920 octets, progression seulement si total positif), l’extraction par le lecteur ZIP commun en conservant ExtractToFile, Hash par CalculateSha256 et, si présente, la validation PE par celle d’Amiga ; conserver les manifestes, chemins, erreurs, nettoyage et vérifications spécifiques.
    - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Pcsx2/Services/CoreReleaseService.cs` pour remplacer le téléchargement recopié par DownloadAsync commun (81 920 octets, progression seulement si total positif), l’extraction par le lecteur ZIP commun en conservant ExtractToFile, Hash par CalculateSha256 et, si présente, la validation PE par celle d’Amiga ; conserver les manifestes, chemins, erreurs, nettoyage et vérifications spécifiques.
    - [x] Modifier `src/GWGUI.Emulation.Amstrad/Emulators/Common/Services/ExternalCoreInstaller.cs` pour supprimer le Hash recopié, rediriger ses appels éventuels vers CalculateSha256 et réutiliser, si présente, l’extraction ZIP commune en conservant son horodatage et son message ; conserver le téléchargement CopyToAsync et les manifestes propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Commodore/Emulators/Common/Interop/Services/ExternalCoreInstaller.cs` pour supprimer le Hash recopié, rediriger ses appels éventuels vers CalculateSha256 et réutiliser, si présente, l’extraction ZIP commune en conservant son horodatage et son message ; conserver le téléchargement CopyToAsync et les manifestes propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Nec/Emulators/BeetlePceFast/Services/ExternalCoreInstaller.cs` pour supprimer le Hash recopié, rediriger ses appels éventuels vers CalculateSha256 et réutiliser, si présente, l’extraction ZIP commune en conservant son horodatage et son message ; conserver le téléchargement CopyToAsync et les manifestes propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Nec/Emulators/BeetlePcfx/Services/ExternalCoreInstaller.cs` pour supprimer le Hash recopié, rediriger ses appels éventuels vers CalculateSha256 et réutiliser, si présente, l’extraction ZIP commune en conservant son horodatage et son message ; conserver le téléchargement CopyToAsync et les manifestes propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Pcsx2/Services/ExternalCoreInstaller.cs` pour supprimer le Hash recopié, rediriger ses appels éventuels vers CalculateSha256 et réutiliser, si présente, l’extraction ZIP commune en conservant son horodatage et son message ; conserver le téléchargement CopyToAsync et les manifestes propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Ppsspp/Services/ExternalCoreInstaller.cs` pour supprimer le Hash recopié, rediriger ses appels éventuels vers CalculateSha256 et réutiliser, si présente, l’extraction ZIP commune en conservant son horodatage et son message ; conserver le téléchargement CopyToAsync et les manifestes propres au module.

    - [x] Modifier `docs/reference/reuse-audit/qualification.json` et `docs/tasks/reuse-project-audit.md` avec les origines Amiga/Atari, les différences conservées et la vérification sémantique du SDK et des modules concernés sans Emit.

  - [x] Réutiliser les primitives antérieures des callbacks d’environnement.
    - [x] Modifier `src/GWGUI.Emulation/Functions/ExternalCoreEnvironmentFunctions.cs` avec SetPixelFormat et SetLogInterface repris d’Atari `2a145e278`, adaptés à l’acceptation RGB1555 déjà présente ; laisser le format inchangé si le pointeur ou la valeur est refusé et emprunter le délégué de journalisation sans le libérer.
    - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/Common/Interop/Services/ExternalHostCallbacks.EnvironmentDetails.cs` pour déléguer SetPixelFormat et SetLogInterface aux fonctions communes antérieures, en conservant les autres callbacks et leurs états propres.
    - [x] Modifier `src/GWGUI.Emulation.Coleco/Emulators/Common/Interop/Services/ExternalHostCallbacks.EnvironmentDetails.cs` pour déléguer SetPixelFormat et SetLogInterface aux fonctions communes antérieures, en conservant les autres callbacks et leurs états propres.
    - [x] Modifier `src/GWGUI.Emulation.Sinclair/Emulators/Common/Interop/Services/ExternalHostCallbacks.EnvironmentDetails.cs` pour déléguer SetPixelFormat et SetLogInterface aux fonctions communes antérieures, en conservant les autres callbacks et leurs états propres.
    - [x] Modifier `src/GWGUI.Emulation.Apple/Emulators/Common/Interop/Services/ExternalHostCallbacks.EnvironmentDetails.cs` pour déléguer SetPixelFormat et SetLogInterface aux fonctions communes antérieures, en conservant les autres callbacks et leurs états propres.
    - [x] Modifier `src/GWGUI.Emulation.Bandai/Emulators/Common/Interop/Services/ExternalHostCallbacks.EnvironmentDetails.cs` pour déléguer SetPixelFormat et SetLogInterface aux fonctions communes antérieures, en conservant les autres callbacks et leurs états propres.
    - [x] Modifier `src/GWGUI.Emulation.Philips/Emulators/Common/Interop/Services/ExternalHostCallbacks.EnvironmentDetails.cs` pour déléguer SetPixelFormat et SetLogInterface aux fonctions communes antérieures, en conservant les autres callbacks et leurs états propres.
    - [x] Modifier `src/GWGUI.Emulation.Sharp/Emulators/Common/Interop/Services/ExternalHostCallbacks.EnvironmentDetails.cs` pour déléguer SetPixelFormat et SetLogInterface aux fonctions communes antérieures, en conservant les autres callbacks et leurs états propres.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/Common/Interop/Services/ExternalHostCallbacks.EnvironmentDetails.cs` pour déléguer SetPixelFormat et SetLogInterface aux fonctions communes antérieures, en conservant les autres callbacks et leurs états propres.
    - [x] Modifier `src/GWGUI.Emulation.Fairchild/Emulators/Common/Interop/Services/ExternalHostCallbacks.EnvironmentDetails.cs` pour déléguer SetPixelFormat et SetLogInterface aux fonctions communes antérieures, en conservant les autres callbacks et leurs états propres.
    - [x] Modifier `src/GWGUI.Emulation.Mattel/Emulators/Common/Interop/Services/ExternalHostCallbacks.EnvironmentDetails.cs` pour déléguer SetPixelFormat et SetLogInterface aux fonctions communes antérieures, en conservant les autres callbacks et leurs états propres.
    - [x] Modifier `src/GWGUI.Emulation.Epoch/Emulators/Common/Interop/Services/ExternalHostCallbacks.EnvironmentDetails.cs` pour déléguer SetPixelFormat et SetLogInterface aux fonctions communes antérieures, en conservant les autres callbacks et leurs états propres.
    - [x] Modifier `src/GWGUI.Emulation.Snk/Emulators/Common/Interop/Services/ExternalHostCallbacks.EnvironmentDetails.cs` pour déléguer SetPixelFormat et SetLogInterface aux fonctions communes antérieures, en conservant les autres callbacks et leurs états propres.
    - [x] Modifier les neuf `CoreReleaseService.cs` et les douze `ExternalHostCallbacks.EnvironmentDetails.cs` listés ci-dessus pour aligner les nouvelles délégations sur huit espaces et rétablir la ligne vide après le téléchargement, sans changement de comportement.
    - [x] Modifier `docs/reference/reuse-audit/qualification.json` et `docs/tasks/reuse-project-audit.md` avec les primitives de callbacks raccordées, les refus de valeurs et les résultats sémantiques sans Emit ; actualiser `docs/reference/reuse-audit/current-project-groups.json` après les corrections.

  - [x] Raccorder les écritures natives communes des callbacks.
    - [x] Modifier `src/GWGUI.Emulation/Functions/ExternalCoreEnvironmentFunctions.cs` pour reprendre WritePointer, WriteBoolean, les deux WriteInteger et WriteUnsignedLong du CoreFunctions Atari antérieur : pointeur nul refusé, booléens 0/1, conversion uint vers int vérifiée et ulong vers long non vérifiée.
    - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/Common/Interop/Functions/CoreFunctions.cs` pour remplacer les corps des cinq écritures natives par des délégations aux fonctions communes, sans déplacer les catalogues et validations de cœur propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Coleco/Emulators/Common/Interop/Functions/CoreFunctions.cs` pour remplacer les corps des cinq écritures natives par des délégations aux fonctions communes, sans déplacer les catalogues et validations de cœur propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Sinclair/Emulators/Common/Interop/Functions/CoreFunctions.cs` pour remplacer les corps des cinq écritures natives par des délégations aux fonctions communes, sans déplacer les catalogues et validations de cœur propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Apple/Emulators/Common/Interop/Functions/CoreFunctions.cs` pour remplacer les corps des cinq écritures natives par des délégations aux fonctions communes, sans déplacer les catalogues et validations de cœur propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Bandai/Emulators/Common/Interop/Functions/CoreFunctions.cs` pour remplacer les corps des cinq écritures natives par des délégations aux fonctions communes, sans déplacer les catalogues et validations de cœur propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Philips/Emulators/Common/Interop/Functions/CoreFunctions.cs` pour remplacer les corps des cinq écritures natives par des délégations aux fonctions communes, sans déplacer les catalogues et validations de cœur propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Sharp/Emulators/Common/Interop/Functions/CoreFunctions.cs` pour remplacer les corps des cinq écritures natives par des délégations aux fonctions communes, sans déplacer les catalogues et validations de cœur propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/Common/Interop/Functions/CoreFunctions.cs` pour remplacer les corps des cinq écritures natives par des délégations aux fonctions communes, sans déplacer les catalogues et validations de cœur propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Fairchild/Emulators/Common/Interop/Functions/CoreFunctions.cs` pour remplacer les corps des cinq écritures natives par des délégations aux fonctions communes, sans déplacer les catalogues et validations de cœur propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Mattel/Emulators/Common/Interop/Functions/CoreFunctions.cs` pour remplacer les corps des cinq écritures natives par des délégations aux fonctions communes, sans déplacer les catalogues et validations de cœur propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Epoch/Emulators/Common/Interop/Functions/CoreFunctions.cs` pour remplacer les corps des cinq écritures natives par des délégations aux fonctions communes, sans déplacer les catalogues et validations de cœur propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Snk/Emulators/Common/Interop/Functions/CoreFunctions.cs` pour remplacer les corps des cinq écritures natives par des délégations aux fonctions communes, sans déplacer les catalogues et validations de cœur propres au module.
    - [x] Modifier `docs/reference/reuse-audit/qualification.json` et `docs/tasks/reuse-project-audit.md` avec la provenance et la vérification sémantique des écritures natives ; actualiser `docs/reference/reuse-audit/current-project-groups.json` après raccordement.

  - [x] Construire le lot demandé en Debug avec tous les modules.
    - [x] Modifier `build/Debug/GW GUI/gwgui.exe` et les sorties `build/Debug/GW GUI/Modules/<id>/` via `scripts\local-building.cmd --building=debug --modules=A` après commit des sources ; modifier `docs/tasks/reuse-project-audit.md` avec le résultat du script et la présence de chaque assembly de module annoncé par son manifeste.
  - [x] Reprendre la persistance des configurations depuis son propriétaire antérieur.
    - [x] Modifier `src/GWGUI.Emulation/Functions/ConfigurationFileAccessFunctions.cs` avec StorePath, ResolvePath, IsInsideBase et WriteDocumentAtomicallyAsync générique depuis Atari `9a104458f` et sa version avant Amstrad, en réutilisant WriteAtomicallyAsync existant ; conserver les chemins hors base absolus, les séparateurs stockés, le temporaire unique, le tampon 4 096 octets, WriteThrough, FlushAsync et les options JSON fournies par le module.
    - [x] Modifier `src/GWGUI.Emulation.Coleco/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour déléguer StorePath, ResolvePath et WriteDocumentAtomicallyAsync au propriétaire commun et supprimer IsInsideBase recopié ; conserver les documents typés, validations, JSON et dossiers propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Panasonic/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour déléguer StorePath, ResolvePath et WriteDocumentAtomicallyAsync au propriétaire commun et supprimer IsInsideBase recopié ; conserver les documents typés, validations, JSON et dossiers propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Snk/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour déléguer StorePath, ResolvePath et WriteDocumentAtomicallyAsync au propriétaire commun et supprimer IsInsideBase recopié ; conserver les documents typés, validations, JSON et dossiers propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Acorn/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour déléguer StorePath, ResolvePath et WriteDocumentAtomicallyAsync au propriétaire commun et supprimer IsInsideBase recopié ; conserver les documents typés, validations, JSON et dossiers propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Sinclair/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour déléguer StorePath, ResolvePath et WriteDocumentAtomicallyAsync au propriétaire commun et supprimer IsInsideBase recopié ; conserver les documents typés, validations, JSON et dossiers propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Bandai/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour déléguer StorePath, ResolvePath et WriteDocumentAtomicallyAsync au propriétaire commun et supprimer IsInsideBase recopié ; conserver les documents typés, validations, JSON et dossiers propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Fairchild/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour déléguer StorePath, ResolvePath et WriteDocumentAtomicallyAsync au propriétaire commun et supprimer IsInsideBase recopié ; conserver les documents typés, validations, JSON et dossiers propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour déléguer StorePath, ResolvePath et WriteDocumentAtomicallyAsync au propriétaire commun et supprimer IsInsideBase recopié ; conserver les documents typés, validations, JSON et dossiers propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Wenquxing/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour déléguer StorePath, ResolvePath et WriteDocumentAtomicallyAsync au propriétaire commun et supprimer IsInsideBase recopié ; conserver les documents typés, validations, JSON et dossiers propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Epoch/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour déléguer StorePath, ResolvePath et WriteDocumentAtomicallyAsync au propriétaire commun et supprimer IsInsideBase recopié ; conserver les documents typés, validations, JSON et dossiers propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Sharp/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour déléguer StorePath, ResolvePath et WriteDocumentAtomicallyAsync au propriétaire commun et supprimer IsInsideBase recopié ; conserver les documents typés, validations, JSON et dossiers propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Watara/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour déléguer StorePath, ResolvePath et WriteDocumentAtomicallyAsync au propriétaire commun et supprimer IsInsideBase recopié ; conserver les documents typés, validations, JSON et dossiers propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Apple/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour déléguer StorePath, ResolvePath et WriteDocumentAtomicallyAsync au propriétaire commun et supprimer IsInsideBase recopié ; conserver les documents typés, validations, JSON et dossiers propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Thomson/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour déléguer StorePath, ResolvePath et WriteDocumentAtomicallyAsync au propriétaire commun et supprimer IsInsideBase recopié ; conserver les documents typés, validations, JSON et dossiers propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Philips/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour déléguer StorePath, ResolvePath et WriteDocumentAtomicallyAsync au propriétaire commun et supprimer IsInsideBase recopié ; conserver les documents typés, validations, JSON et dossiers propres au module.
    - [x] Modifier `src/GWGUI.Emulation.Mattel/Common/Machines/Common/Functions/ConfigurationFunctions.Persistence.cs` pour déléguer StorePath, ResolvePath et WriteDocumentAtomicallyAsync au propriétaire commun et supprimer IsInsideBase recopié ; conserver les documents typés, validations, JSON et dossiers propres au module.
    - [x] Réutiliser le chargement et la validation API de la sonde native antérieure.
    - [x] Modifier `src/GWGUI.Emulation/Services/ExternalCoreLibrary.cs` avec Inspect générique depuis la sonde Atari `2a145e278` : charger et libérer la bibliothèque dans using/finally, résoudre API/version et SystemInfo, vérifier les exports puis la version, et lire les métadonnées pendant que la bibliothèque reste chargée ; fournir vérificateur, erreur API et lecteur typé depuis le module, sans nouveau DTO.
    - [x] Modifier `src/GWGUI.Emulation.Coleco/Emulators/Common/Interop/Functions/CoreFunctions.ExternalProbe.cs` pour déléguer le chargement et la séquence API à ExternalCoreLibrary.Inspect ; conserver les contrôles de chemin, erreurs localisées, contexte, résolution des exports propres au module et création de son ExternalCoreInfo avant libération de la bibliothèque.
    - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/Common/Interop/Functions/CoreFunctions.ExternalProbe.cs` pour déléguer le chargement et la séquence API à ExternalCoreLibrary.Inspect ; conserver les contrôles de chemin, erreurs localisées, contexte, résolution des exports propres au module et création de son ExternalCoreInfo avant libération de la bibliothèque.
    - [x] Modifier `src/GWGUI.Emulation.Sinclair/Emulators/Common/Interop/Functions/CoreFunctions.ExternalProbe.cs` pour déléguer le chargement et la séquence API à ExternalCoreLibrary.Inspect ; conserver les contrôles de chemin, erreurs localisées, contexte, résolution des exports propres au module et création de son ExternalCoreInfo avant libération de la bibliothèque.
    - [x] Modifier `src/GWGUI.Emulation.Bandai/Emulators/Common/Interop/Functions/CoreFunctions.ExternalProbe.cs` pour déléguer le chargement et la séquence API à ExternalCoreLibrary.Inspect ; conserver les contrôles de chemin, erreurs localisées, contexte, résolution des exports propres au module et création de son ExternalCoreInfo avant libération de la bibliothèque.
    - [x] Modifier `src/GWGUI.Emulation.Fairchild/Emulators/Common/Interop/Functions/CoreFunctions.ExternalProbe.cs` pour déléguer le chargement et la séquence API à ExternalCoreLibrary.Inspect ; conserver les contrôles de chemin, erreurs localisées, contexte, résolution des exports propres au module et création de son ExternalCoreInfo avant libération de la bibliothèque.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/Common/Interop/Functions/CoreFunctions.ExternalProbe.cs` pour déléguer le chargement et la séquence API à ExternalCoreLibrary.Inspect ; conserver les contrôles de chemin, erreurs localisées, contexte, résolution des exports propres au module et création de son ExternalCoreInfo avant libération de la bibliothèque.
    - [x] Modifier `src/GWGUI.Emulation.Snk/Emulators/Common/Interop/Functions/CoreFunctions.ExternalProbe.cs` pour déléguer le chargement et la séquence API à ExternalCoreLibrary.Inspect ; conserver les contrôles de chemin, erreurs localisées, contexte, résolution des exports propres au module et création de son ExternalCoreInfo avant libération de la bibliothèque.
    - [x] Modifier `src/GWGUI.Emulation.Epoch/Emulators/Common/Interop/Functions/CoreFunctions.ExternalProbe.cs` pour déléguer le chargement et la séquence API à ExternalCoreLibrary.Inspect ; conserver les contrôles de chemin, erreurs localisées, contexte, résolution des exports propres au module et création de son ExternalCoreInfo avant libération de la bibliothèque.
    - [x] Modifier `src/GWGUI.Emulation.Sharp/Emulators/Common/Interop/Functions/CoreFunctions.ExternalProbe.cs` pour déléguer le chargement et la séquence API à ExternalCoreLibrary.Inspect ; conserver les contrôles de chemin, erreurs localisées, contexte, résolution des exports propres au module et création de son ExternalCoreInfo avant libération de la bibliothèque.
    - [x] Modifier `src/GWGUI.Emulation.Apple/Emulators/Common/Interop/Functions/CoreFunctions.ExternalProbe.cs` pour déléguer le chargement et la séquence API à ExternalCoreLibrary.Inspect ; conserver les contrôles de chemin, erreurs localisées, contexte, résolution des exports propres au module et création de son ExternalCoreInfo avant libération de la bibliothèque.
    - [x] Modifier `src/GWGUI.Emulation.Mattel/Emulators/Common/Interop/Functions/CoreFunctions.ExternalProbe.cs` pour déléguer le chargement et la séquence API à ExternalCoreLibrary.Inspect ; conserver les contrôles de chemin, erreurs localisées, contexte, résolution des exports propres au module et création de son ExternalCoreInfo avant libération de la bibliothèque.
    - [x] Modifier `src/GWGUI.Emulation.Philips/Emulators/Common/Interop/Functions/CoreFunctions.ExternalProbe.cs` pour déléguer le chargement et la séquence API à ExternalCoreLibrary.Inspect ; conserver les contrôles de chemin, erreurs localisées, contexte, résolution des exports propres au module et création de son ExternalCoreInfo avant libération de la bibliothèque.
    - [x] Modifier `docs/reference/reuse-audit/qualification.json` et `docs/tasks/reuse-project-audit.md` avec la comparaison des sondes et la vérification sémantique sans Emit ; actualiser `docs/reference/reuse-audit/current-project-groups.json`.

  - [x] Modifier `docs/reference/reuse-audit/qualification.json` et `docs/tasks/reuse-project-audit.md` avec la provenance, les seize profils de constantes identiques et la vérification sémantique sans Emit ; indiquer que le relevé global sera recalculé après le groupe suivant des sondes natives.

  - [ ] Modifier `docs/reference/reuse-audit/qualification.json` et `docs/tasks/reuse-project-audit.md` avec la qualification des groupes OnEnvironment/HandleEnvironment, Initialize et InstallAsync restants du relevé courant : comparer leurs propriétaires antérieurs, symboles résolus, profils et ressources ; inscrire chaque fichier et correction nécessaire avant modification des sources. La recherche continue sur tout le projet et ne se limite pas à ces groupes.
  - [ ] Supprimer uniquement `artifacts/reuse-audit-code-baseline.tar` et `artifacts/reuse-audit-code-baseline/` après l’audit complémentaire, après vérification de leurs chemins absolus dans le workspace.

Contrôle après exports/disques/cartes/audio/vidéo : SDK, 23 modules, MediaEngine,
App et sources de GWGUI.Tests : zéro erreur sémantique sans Emit. L’inventaire AST
initial couvrait les 33 projets sous src ; les 248 fichiers C# actuellement hors
src doivent aussi être comparés explicitement pour respecter tout le projet.
Les tâches supprimées/déplacées ne sont pas recherchées. Les deux outils Argos ont
des traitements distincts : labels RESX avec protection de tokens et contexte,
contre texte ROM brut avec contraintes d’encodage/taille et traduction en deux
étapes éventuelle. Ils ne sont pas remplacés l’un par l’autre.

Complément hors src : 248 fichiers C# et 17 fonctions PowerShell analysés.
Quatre groupes : Flatten, CreateProviderOptions et Invoke-DotNetProcess ont déjà
exactement les mêmes occurrences avant Amstrad ; leur ancienneté ne les exclut
pas des corrections. La recherche porte sur tout le projet sans filtre de date.
Les deux tests de catalogue Nintendo/NEC ont les mêmes assertions mais visent des
ModelCatalog/EmulatorCatalog distincts et des jeux d’InlineData distincts : conservés.
Leur structure identique ne constitue pas un nouveau mécanisme de production.

Nouvelle boucle native confirmée : CaptureControllerInfo est copiée depuis le
callback Amiga présent dans ac7292d86^ : 16 ports, rejet au-delà de 64 types et
filtrage des noms vides. Atari tronque à son plafond et garde les noms vides.
Ces politiques ne sont pas interchangeables ; le lecteur antérieur partagé est
adapté avec des paramètres, sans remplacer Atari par la copie récente Sony.
SDK, Atari et les sept modules consommateurs des callbacks simples : zéro erreur
sémantique après ce raccordement. Le plafond uint Commodore est converti avec
checked ; le refus de dépassement n’affecte pas les ports déjà exposés.

Copie supplémentaire confirmée : les huit caches NativeString réallouent et
libèrent eux-mêmes leurs chaînes alors que ExternalCoreUtf8String est antérieur
à Amstrad dans le SDK. Le propriétaire antérieur doit être repris ; ses fonctions
de transfert évitent de laisser une allocation sans propriétaire si Add échoue.
Les huit caches sont raccordés. Les allocations sont transférées à leur dictionnaire
via AddTo, libérées avant Clear et le nettoyage Commodore est maintenant protégé
par finally avec remontée de la première erreur après toutes les tentatives.
SDK et tous les modules consommateurs : zéro erreur sémantique sans Emit.

Vérification après mutualisation des outils et options physiques : quatre scripts PowerShell sans erreur syntaxique ; SDK, 23 modules, MediaEngine, App et GWGUI.Tests sans erreur sémantique (sans Emit). Aucun outil média ni test exécuté, aucun build lancé.

Filtres vidéo : reprise des primitives FixedPixel du 3 septembre 2026 et de ses cadres, adaptées dans les composants communs existants. Largeurs de valeur 38/36/42, choix 180/160, marges 2/8 ou 3/9, pas 1 ou 5 et activation VFD conservés. SDK et App : zéro erreur sémantique sans Emit.

Parcours des outils médias : parent puis descendants dans le même ordre, sans filtrage ni matérialisation supplémentaire. Les deux types restent distincts ; seul le parcours reprend la fonction initiale. SDK, MediaEngine, App, auditeur et extracteur : zéro erreur sémantique sans Emit. Le relevé courant est recalculé après ces corrections ; les groupes sont des candidats, pas une déclaration automatique de code inutile.

Contrôle final du helper PowerShell : aucune erreur syntaxique ; Process possédé dans son try/finally dès sa création, attente de sortie après Kill et Dispose garanti même après erreur du nettoyage.

HDD : les lecteurs et writers concernés ont été introduits ensemble dans 8ea733760. Fusion des plages sur le modèle existant, addition vérifiée du VHD conservée, gardes Raw conservées ; lecture logique et copie VHD/VMDK reprises dans le lecteur existant. Aucun transfert de propriété ni fermeture de source empruntée ajouté. MediaEngine : zéro erreur sémantique sans Emit.

Après b96c7629b : corrections HDD, cassette, options de conversion/écriture, options JSON et primitives de filtres. Les littéraux bruts des shaders des 17 fichiers de rendu modifiés sont strictement inchangés après normalisation des fins de ligne. Diff --check sans erreur ; indentation des nouvelles délégations corrigée.

Vérification de la suite du commit : SDK, 23 modules, Updates, Updater, MediaEngine, App et sources GWGUI.Tests sans erreur sémantique, sans Emit. Aucun test exécuté ni build lancé. Le relevé courant sans filtre de date est recalculé ; ses groupes restent des candidats à comparer, notamment les dispatchs natifs et les modèles typés. La réserve temporaire de sources antérieures reste nécessaire à ces comparaisons.

Snapshots et options Greaseweazle : Infrastructure, Updates, SDK et App sans erreur sémantique sans Emit. Le premier contrôle App avait omis le projet Updates du jeu de références en mémoire ; invocation corrigée avec les sources Updates, sans changement de code pour contourner cette référence ancienne.

Boucles hôtes : SDK, 23 modules, Infrastructure, Updates, MediaEngine, App et sources GWGUI.Tests sans erreur sémantique sans Emit. Comparaison AST de 25 hôtes dans host-loop-comparison.json : commandes, erreurs, lecteurs, Dispose, vidéo et profils de connexion inchangés ; le finally du cœur reste dans le callback exécuté avant fermeture du pipe. Ce contrôle ne constitue pas une exécution des hôtes.

Notifications : propriétaire par instance repris du MainWindowViewModel 832932d91 ; neuf classes raccordées sans nouvelle dépendance externe. Set conserve le contrôle d’égalité et l’émetteur ; Read conserve SequenceKindIndex puis SequenceKind ; Binding et localisation conservent les notifications inconditionnelles. Capture et application d’options reprises du Read d0962bd99, plus ancien que Write ; NoVerify reste exclu seulement pour Write. Cinq tests autonomes ajoutés et analysés, non exécutés. SDK, 23 modules, Infrastructure, Updates, MediaEngine, App et GWGUI.Tests : zéro erreur sémantique sans Emit. Diff --check passe après retrait des espaces de fin de ligne des hôtes. Relevé global actualisé ; la qualification générale demeure incomplète.

Installations restantes : origine Amiga aa0ef2450, adaptée ensuite dans Atari 444ea3273. Neuf services et six installateurs raccordés aux fonctions existantes : téléchargement 81 920 octets, SHA256, recherche ZIP et validation PE. ExtractToFile conserve son horodatage ; messages, manifestes, progression positive et vérificateur optionnel Amstrad conservés. Les téléchargements CopyToAsync des installateurs restent inchangés. SDK et six modules : zéro erreur sémantique sans Emit ; diff --check passe. Aucun test ni build exécuté. Le relevé global précédent reste un instantané antérieur à ce lot.

Callbacks : SetPixelFormat et SetLogInterface repris du propriétaire Atari 2a145e278 dans ExternalCoreEnvironmentFunctions ; douze modules raccordés. Acceptation RGB1555 actuelle conservée, pointeur nul et format inconnu refusés sans changer le format, délégué Log emprunté et toujours enraciné par son propriétaire. SDK et douze modules : zéro erreur sémantique sans Emit. Diff --check passe. Relevé recalculé : 582 groupes exacts et 219 groupes avec identifiants normalisés, restant des candidats à qualifier ; les grands dispatchs et Initialize restent à examiner.

Écritures natives : cinq adaptateurs par module raccordés dans douze modules au propriétaire commun. Pointeur nul refusé, booléens natifs 0/1, conversion uint vérifiée avant contrôle du pointeur et conversion ulong non vérifiée conservées. Propriétaire initial Atari 2a145e278, surcharges présentes avant Amstrad. SDK et douze consommateurs : zéro erreur sémantique sans Emit. Diff --check passe. Nouveau relevé global : 580 groupes exacts et 219 groupes avec identifiants normalisés ; qualification manuelle générale toujours incomplète. Aucun build ni test exécuté.

Build demandé le 18/10/2026 : commit ac627b1bc, scripts\local-building.cmd --building=debug --modules=A terminé avec code 0. Application build/Debug/GW GUI/gwgui.exe présente ; les 23 assemblies de modules sont présents dans Modules/<id> et vérifiés depuis les manifestes sources. Avertissements CA1416 (plateforme Windows) et CS8602 (nullabilité SNK) signalés, aucune erreur. Les modifications de la suite de l’audit ne font pas partie de ce build.

Persistance : reprise du propriétaire Atari 9a104458f avec ses évolutions antérieures à Amstrad (temporaire unique et remplacement verrouillé). Seize profils de constantes comparés et identiques : .., /, suffixe .tmp, GUID N, tampon 4 096. Documents et options JSON restent propres aux modules. WriteThrough, FlushAsync, nettoyage finally et chemins hors base conservés. SDK et seize modules : zéro erreur sémantique sans Emit ; diff --check passe. Corrections postérieures au build ac627b1bc. Le relevé global sera recalculé après les sondes natives.

Sondes natives : chargement et contrôle API repris de la sonde Atari 2a145e278 dans ExternalCoreLibrary.Inspect<T>. Bibliothèque possédée libérée par using/finally, lecture des métadonnées terminée avant sa fermeture. Douze comparaisons AST face à ac627b1bc : contrôles de chemin, catches, exports, version API, erreur API et construction des métadonnées identiques. Profil SNK requireLoadGameSpecial conservé dans son callback de résolution. SDK et douze modules : zéro erreur sémantique sans Emit ; diff --check passe. Aucune bibliothèque native chargée pour ces vérifications. Nouveau relevé : 576 groupes exacts et 219 groupes avec identifiants normalisés. Les sondes conservent leurs contrôles et métadonnées typés, et ne sont pas déclarées entièrement dédupliquées. Ces corrections restent postérieures au build ac627b1bc.
