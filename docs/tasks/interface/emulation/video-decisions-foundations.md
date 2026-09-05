# Journal vidéo : socle et réglages généraux

[Sommaire](../emulation-improvements.md) · [Règles communes](rules.md)

### Journal des décisions autonomes

Ce journal conserve les questions qui auraient pu bloquer l’avancement. Les numéros de ligne sont
ceux relevés au moment de la décision ; les titres de section restent les repères durables si une
modification ultérieure décale ces lignes.

- **2026-09-01 — Gamma**
  - Question : quelle plage, quelle valeur neutre et quelle conversion utiliser pour le gamma ?
  - Décision : utiliser la plage `-10..+10`, avec `0` neutre, et convertir la valeur affichée par
    `exposant = 2^(-valeur / 10)` ; une valeur positive éclaircit les tons moyens.
  - Motif : cette échelle symétrique est cohérente avec les autres réglages généraux et conserve une
    conversion exponentielle explicite, stable et réversible autour de la valeur neutre.
  - Modifications : `docs/reference/emulation-video-filters.md`, section `Gamma retenu`, lignes
    304 à 313 ; `docs/tasks/interface/emulation/video-decisions-foundations.md`, section `Décisions validées`, ligne
    1851. Ces valeurs restent révisables avant la création du contrat correspondant.

- **2026-09-01 — Contenu de Snapshot**
  - Question : Snapshot doit-il enregistrer l’image brute émise par le moteur ou l’image après les
    traitements vidéo de GW GUI ?
  - Décision : enregistrer la sortie finale des traitements GW GUI, avant tout bezel, cadre ou autre
    habillage externe futur ; la capture correspond à l’image visible dans la zone d’émulation.
  - Motif : un utilisateur attend d’une capture qu’elle reproduise le rendu qu’il a configuré. La
    frontière avant habillage conserve une image propre à l’émulation et évite de figer une future
    présentation de fenêtre dans le fichier capturé.
  - Modifications : `docs/reference/emulation-video-filters.md`, section `Snapshot`, lignes 168 à
    179 ; `docs/architecture/emulation.md`, section `Snapshot`, lignes 545 à 552 ;
    `docs/tasks/interface/emulation/video-decisions-foundations.md`, section `Décisions validées`, ligne 1852.
    Les surfaces actuelles concernées sont `WpfVideoSurface`, `OpenGlVideoSurface` et
    `VeldridVideoSurface` ; leur code n’est pas modifié par cette tâche documentaire.

- **2026-09-01 — Présélections vidéo initiales**
  - Question : quels noms, identifiants persistants, contenus et valeurs exactes fournir pour les
    premières présélections ?
  - Décision : fournir douze présélections couvrant Normal, deux CRT couleur, trois CRT
    monochromes, quatre écrans à pixels fixes, Plasma et Écran vectoriel. Les réglages généraux
    utilisent `-10..+10`, les intensités `0..100` et les durées des millisecondes ; le tableau de
    référence fixe chaque valeur et chaque identifiant persistant.
  - Motif : cette sélection couvre chaque technologie et chaque sous-choix déjà retenus sans créer
    de panneau supplémentaire. Les unités normalisées gardent les presets indépendants du shader ou
    du backend qui réalisera l’effet.
  - Modifications : `docs/reference/emulation-video-filters.md`, section `Présélections`, lignes 299
    à 330 ; `docs/tasks/interface/emulation/video-decisions-foundations.md`, section `Décisions validées`, ligne
    1861. Aucune constante ni ressource traduite n’est créée pendant cette tâche documentaire.

- **2026-09-01 — Bornes des valeurs temporelles**
  - Question : quelle borne commune utiliser pour les durées exprimées en millisecondes, et comment
    les distinguer des intensités de rémanence ?
  - Décision : borner les durées à `0..1000 ms` et conserver les intensités, y compris la rémanence
    sans suffixe `ms`, dans `0..100`.
  - Motif : une seconde couvre les temps de réponse et historiques visés tout en empêchant une
    configuration déraisonnable ; des unités distinctes évitent qu’un preset mélange durée et force.
  - Modifications : `docs/reference/emulation-video-filters.md`, section `Présélections`, lignes 303
    à 306 ; `docs/tasks/interface/emulation/video-decisions-foundations.md`, section `Décisions validées`, ligne
    1862 ; le code correspondant sera centralisé dans `EmulationVideoProcessingLimits.cs`.

- **2026-09-01 — Application immuable de la configuration vidéo commune**
  - Question : comment le panneau commun peut-il remplacer `VideoProcessing` dans une configuration
    immuable sans connaître `AmigaMachineConfiguration` ou `AtariMachineConfiguration` ?
  - Décision : ajouter `ApplyVideoProcessing` au contrat `IEmulationModule`, puis laisser chaque
    module spécialisé produire sa propre nouvelle configuration typée.
  - Motif : App reste indépendante des familles et ne sérialise pas une configuration en JSON pour
    la modifier ; chaque bibliothèque conserve la responsabilité de son type concret.
  - Modifications : `src/GWGUI.Emulation/Interfaces/IEmulationModule.cs`,
    `src/GWGUI.Emulation.Amiga/Modules/AmigaEmulationModule.cs`,
    `src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs`, puis raccordement dans
    `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs`.

- **2026-09-01 — Catalogue neutre utilisé par Argos**
  - Question : comment exécuter la commande Argos imposée alors que le script cherche
    `Resources/Emulation.resx`, fichier absent du dépôt ?
  - Décision : corriger le script pour cibler `Resources/00-Base/<catalogue>`, qui est le catalogue
    neutre déclaré par `GWGUI.App.csproj`, sans changer sa syntaxe de commande.
  - Motif : la commande documentée reste identique et chaque clé continue d’être ajoutée par Argos
    dans le catalogue neutre, en-US et toutes les cultures prises en charge.
  - Modifications : `scripts/translate-resx-argos.py`, fonction `main`, avant tout ajout de clé vidéo.

- **2026-09-01 — Ordre de propagation de la configuration vidéo**
  - Question : comment `MachineVideoPresenter` peut-il réappliquer la configuration courante lors
    d’un changement ou d’un repli de surface alors que `IEmulationVideoSurface` ne sait pas encore
    la recevoir ?
  - Décision : ajouter avant la propagation deux tâches atomiques donnant aux surfaces un contrat
    de configuration et un stockage normalisé sans effet visuel, puis exécuter la propagation dans
    l’ordre surface, presenter, controller et instance ciblée.
  - Motif : chaque dépendance précède ainsi son utilisation ; la future chaîne de traitement
    consommera ce contrat déjà présent sans modifier le rendu pendant cette étape.
  - Modifications : checklist `Appliquer les changements à la seule instance correspondante`, puis
    `IEmulationVideoSurface` et les trois surfaces de rendu.

- **2026-09-01 — Configuration vidéo initiale du controller**
  - Question : d’où `MachineController` reçoit-il la configuration vidéo initiale alors que
    `MachineControllerOptions.Machine` est une instance `IEmulatedMachine` sans paramètres persistés ?
  - Décision : ajouter la configuration commune à `MachineControllerOptions` et la fournir depuis
    `EmulationMachineRuntime.Configuration` au même endroit que `VideoRenderer`.
  - Motif : le controller reçoit ainsi un instantané cohérent de la configuration persistée sans
    ajouter de réglage d’interface au contrat de la machine émulée.
  - Modifications : `MachineControllerOptions.cs` et `EmulationSectionMachineFunctions.cs` avant
    la tâche `MachineController.cs`.

- **2026-09-01 — Confirmation des technologies d’écran incompatibles**
  - Question : comment tester la confirmation exigée alors que le panneau remplace actuellement la
    technologie active sans consulter le catalogue d’incompatibilités ?
  - Décision : demander confirmation uniquement lors du passage direct d’une technologie simulée
    non normale à une autre technologie simulée incompatible ; Normal reste une activation ou une
    désactivation sans avertissement.
  - Motif : le sélecteur garantit déjà l’exclusivité, mais un remplacement CRT vers LCD, Plasma ou
    Vector peut surprendre ; une fonction de confirmation injectable rend ce choix testable.
  - Modifications : `EmulationVideoProcessingSettingsSection.cs` avant ses tests comportementaux.

- **2026-09-01 — Couture de test du ciblage d’une instance**
  - Question : comment vérifier le ciblage d’une seule instance sans construire une fenêtre complète
    et une machine émulée réelle autour du gestionnaire privé `ConfigurationSaved` ?
  - Décision : extraire une fonction interne générique qui recherche exactement la clé
    `(ModuleId, ConfigurationId)` et applique une action au seul élément trouvé ; le gestionnaire
    continuera de contrôler que le contenu trouvé est un `MachineController`.
  - Motif : le test couvre la règle de sélection indépendamment du démarrage d’un moteur, tandis
    que le chemin de production conserve son dictionnaire et son type concret.
  - Modifications : nouveau fichier de fonctions sous
    `Functions/Views/Emulation/Machine`, puis utilisation dans
    `EmulationSectionConfigurationFunctions.cs`.

- **2026-09-01 — Couture de test brouillon ou autosauvegarde**
  - Question : comment tester la décision de persistance vidéo sans charger les styles globaux de
    toute la fenêtre d’options dans le runner WPF ?
  - Décision : extraire la seule décision « stocker le brouillon si la machine n’a aucune
    configuration, sinon sauvegarder » dans une fonction interne appelée par
    `EmulationModuleSettingsSection`.
  - Motif : le test vérifie la vraie règle de production sans dépendre des ressources visuelles du
    sélecteur de machine ; le verrou et la notification restent dans le contrôle.
  - Modifications : nouveau fichier sous `Functions/Views/Emulation/Settings`, puis raccordement
    dans `EmulationModuleSettingsSection.cs`.

- **2026-09-01 — Préservation vidéo dans les reconstructions Atari**
  - Question : les reconstructions explicites de `AtariMachineConfiguration` conservent-elles le
    nouveau dernier membre `VideoProcessing` après une modification sans rapport avec la vidéo ?
  - Décision : transmettre explicitement la configuration vidéo dans les chemins firmware, média,
    entrée, stockage et configuration courante utilisée par les états sauvegardés, puis ajouter un
    test de non-régression.
  - Motif : le paramètre facultatif masque l’oubli à la compilation et réinitialise silencieusement
    les filtres ; les opérations non vidéo doivent conserver la valeur enregistrée.
  - Modifications : fichiers Atari concernés et `EmulationVideoConfigurationTests.cs` avant la
    création de la chaîne de traitement.

- **2026-09-01 — Exécuteur neutre avant la fabrique de chaînes**
  - Question : quel exécuteur la fabrique peut-elle sélectionner alors qu’aucune implémentation de
    `IEmulationVideoProcessingPipeline` ne précède sa tâche ?
  - Décision : créer un exécuteur pass-through commun portant le renderer demandé et retournant la
    trame inchangée après validation des tailles ; les groupes CPU, OpenGL et Veldrid le remplaceront
    ensuite progressivement.
  - Motif : la fabrique a une dépendance concrète, la chaîne vide reste vérifiable et aucun effet
    Normal n’est implémenté prématurément.
  - Modifications : nouveau fichier sous `Rendering/Emulation/Processing` avant la fabrique.

- **2026-09-01 — Chemin commun entre traitement et Snapshot**
  - Question : comment démontrer que les trois familles de surface capturent bien la sortie traitée
    alors qu’elles dupliquent encore l’appel de chaîne puis la conversion BGRA ?
  - Décision : extraire une fonction commune retournant la `VideoFrame` traitée et ses pixels
    BGRA, puis l’utiliser dans WPF, OpenGL et Veldrid avant affichage et mise à jour de Snapshot.
  - Motif : le test peut couvrir le point commun aux quatre renderers, et aucune surface ne peut
    capturer par mégarde la trame brute après l’introduction d’un effet.
  - Modifications : nouveau fichier sous `Functions/Rendering/Emulation`, raccordement des trois
    surfaces, puis test Snapshot.

- **2026-09-01 — Injection déterministe du pipeline WPF pour Snapshot**
  - Question : comment prouver que `WpfVideoSurface.Snapshot` reçoit une trame modifiée par la
    chaîne plutôt que la source brute, sans implémenter prématurément un effet Normal ?
  - Décision : permettre au constructeur interne WPF de recevoir facultativement un pipeline de
    test, tout en conservant la création WPF neutre par défaut utilisée par la fabrique.
  - Motif : un double déterministe peut remplacer la trame, puis le test lit les pixels réels de
    Snapshot ; OpenGL et Veldrid partagent déjà la même fonction en amont.
  - Modifications : `WpfVideoSurface.cs` avant le test Snapshot.

- **2026-09-01 — Conversions CPU des cinq réglages généraux**
  - Question : quelles conversions exactes appliquer à luminosité, contraste, saturation et netteté,
    dont seule la plage était validée ?
  - Décision : traiter en lumière linéaire, dans l’ordre luminosité, contraste, gamma, saturation et
    netteté ; utiliser respectivement un décalage `valeur / 20`, un contraste
    `2^(valeur / 5)` autour de 0,18, le gamma validé, une saturation `1 + valeur / 10` autour de
    la luminance Rec. 709, puis un masque flou ou un lissage 3×3 de force absolue `valeur / 10`.
  - Motif : les conversions sont symétriques autour de 0, bornées, testables et n’ajoutent aucune
    dépendance à un shader tiers.
  - Modifications : `docs/reference/emulation-video-filters.md` avant l’exécution CPU.

- **2026-09-01 — Validation visuelle CRT sans médias propriétaires**
  - Question : comment vérifier les présélections CRT sur Amiga et Atari lorsque le workspace ne
    contient ni ROM, ni disque, ni configuration de machine exécutable ?
  - Décision : utiliser deux mires déterministes distinctes, portant les rapports logiques Amiga et
    Atari, puis présenter les cinq présélections CRT dans les surfaces WPF, OpenGL, Direct3D 11 et
    Vulkan. Vérifier automatiquement que chaque rendu est non noir et distinct, produire une
    planche par renderer et inspecter visuellement les quatre planches. Cette validation ne prétend
    pas qu’une ROM Amiga ou Atari absente a été lancée.
  - Motif : les filtres de GW GUI reçoivent une `VideoFrame` après le moteur et ne dépendent pas du
    contenu propriétaire qui l’a produite. Les mires couvrent les couleurs, gradients, diagonales et
    damiers nécessaires pour voir palettes, masque, scanlines, courbure, halo et vignettage.
  - Modifications : `tests/GWGUI.Tests/EmulationVideoProcessingPipelineTests.cs` et planches
    générées dans `build/validation/crt-validation` ; ordre des colonnes : Arcade couleur, Téléviseur
    couleur, vert, ambre, blanc ; lignes : Amiga puis Atari.
