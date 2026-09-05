# Journal vidéo : effets temporels, signaux et styles

[Sommaire](../emulation-improvements.md) · [Règles communes](rules.md)

- **2026-09-02 — Rémanence générale indépendante des technologies d’écran**
  - Question : comment proposer une traînée lumineuse générale sans confondre son intensité avec le
    temps de réponse ou la persistance interne des LCD/OLED et autres affichages spécialisés ?
  - Décision : ajouter un contrat temporel indépendant et un panneau permanent. `Rémanence générale`
    utilise une intensité `0..100`, neutre et initialisée à `0`, puis conserve par maximum une part de
    l’unique frame précédente ; aucune durée en millisecondes n’est introduite.
  - Motif : la spécification slang valide le besoin architectural d’un historique de frames, sans que
    GW GUI ne reprenne de shader ni de code tiers. L’algorithme original, sous MIT, possède son propre
    historique et intervient après les réponses propres aux technologies d’écran.
  - Compatibilité : compatible avec toutes les technologies, scalers, restaurations et réglages
    généraux ; une valeur nulle, une première frame, un changement de taille, une séquence non
    croissante ou la destruction du pipeline réinitialisent l’historique. Les quatre renderers
    utilisent le pipeline CPU commun lorsque l’effet est actif.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md`,
    `EmulationTemporalVideoConfiguration.cs:3`, `EmulationVideoProcessingConfiguration.cs`,
    `EmulationVideoProcessingConfigurationFunctions.cs:41`,
    `EmulationVideoProcessingCatalog.cs:18`, `EmulationResourceKeys.cs:94`, les deux clés dans les
    30 ressources `Emulation.resx` via Argos (correction française à `fr-FR/Emulation.resx:758`),
    `EmulationVideoProcessingSettingsSection.cs:138`,
    `CpuEmulationVideoProcessingPipeline.cs:389`, `OpenGlVideoSurface.cs:130`,
    `VeldridVideoSurface.cs:97`, puis les tests aux lignes `35`, `60`, `275` et `1977` de leurs
    fichiers respectifs.
  - Vérifications terminées avant cochage : normalisation/persistances Amiga et Atari/interface/effet
    et remises à zéro ciblés `5/5`, matrice quatre renderers `1/1`, classes complètes
    configuration/interface `16/16` et localisation `2/2`, compilation GWGUI.App sans avertissement
    ni erreur ; contrôle final de format réalisé séparément juste avant cochage.

- **2026-09-02 — Flou de mouvement limité à la frame précédente**
  - Question : comment différencier le flou de mouvement de la rémanence générale déjà cumulative ?
  - Décision : ajouter une intensité indépendante `0..100`, neutre et initialisée à `0`, qui mélange
    la frame courante avec la précédente puis mémorise la frame courante non mélangée. L’effet reste
    donc limité à une seule frame.
  - Motif : l’historique de frames prévu par la spécification slang convient à l’architecture, mais
    l’algorithme est original, sous MIT, sans code, shader, coefficient ni actif tiers.
  - Compatibilité : compatible avec toutes les technologies et tous les traitements indépendants ;
    historique séparé réinitialisé à la première frame, à zéro, au redimensionnement, en cas de
    séquence non croissante ou à la destruction. Il précède la rémanence générale dans la chaîne.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md`,
    `EmulationTemporalVideoConfiguration.cs:5`,
    `EmulationVideoProcessingConfigurationFunctions.cs:44`,
    `EmulationVideoProcessingCatalog.cs:19`, `EmulationResourceKeys.cs:95`, la clé ajoutée aux
    30 ressources `Emulation.resx` via Argos, `EmulationVideoProcessingSettingsSection.cs:144`,
    `CpuEmulationVideoProcessingPipeline.cs`, `OpenGlVideoSurface.cs:131`,
    `VeldridVideoSurface.cs:98`, puis les tests aux lignes `35`, `61`, `279` et `2023` de leurs
    fichiers respectifs.
  - Vérifications terminées avant cochage : normalisation/persistances/interface/localisation/effet
    ciblés `7/7`, matrice quatre renderers `1/1`, classes complètes configuration/localisation/interface
    `18/18` ; compilation et contrôle de format finaux réalisés juste avant cochage.

- **2026-09-02 — Scintillement modulé distinct des images noires**
  - Question : comment rendre le scintillement visible sans anticiper ni dupliquer l’insertion
    d’images noires prévue par une autre tâche ?
  - Décision : ajouter une intensité indépendante `0..100`, neutre et initialisée à `0`. Les frames
    impaires sont atténuées jusqu’à `50 %`, les frames paires restent intactes ; aucune frame n’est
    supprimée, même à l’intensité maximale.
  - Motif : la modulation par `VideoFrame.Sequence` est déterministe, ne requiert aucun historique et
    constitue une formule originale sous MIT, sans code, shader, coefficient ni actif tiers.
  - Compatibilité : compatible avec toutes les technologies, scalers, restaurations et effets
    temporels ; elle intervient avant le flou de mouvement et la rémanence générale. Les quatre
    renderers utilisent le pipeline CPU commun lorsque l’effet est actif.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md`,
    `EmulationTemporalVideoConfiguration.cs`,
    `EmulationVideoProcessingConfigurationFunctions.cs`,
    `EmulationVideoProcessingCatalog.cs`, `EmulationResourceKeys.cs`, la clé dans les 30 ressources
    `Emulation.resx` via Argos (correction française en `Scintillement`),
    `EmulationVideoProcessingSettingsSection.cs`, `CpuEmulationVideoProcessingPipeline.cs`,
    `OpenGlVideoSurface.cs`, `VeldridVideoSurface.cs`, puis les tests de configuration, interface,
    alternance bornée et matrice quatre renderers.
  - Vérifications terminées avant cochage : ciblage configuration/persistances/interface/localisation
    et rendu `7/7`, matrice quatre renderers `1/1`, classes complètes
    configuration/localisation/interface `18/18` ; compilation et contrôle de format finaux réalisés
    juste avant cochage.

- **2026-09-02 — Entrelacement simulé séparé du désentrelacement**
  - Question : comment ajouter un effet entrelacé sans le confondre avec la correction de sources
    entrelacées déjà placée dans la restauration ?
  - Décision : ajouter une intensité indépendante `0..100`, neutre et initialisée à `0`, qui atténue
    alternativement les lignes paires et impaires selon `VideoFrame.Sequence`, avec un plancher de
    `25 %` à l’intensité maximale.
  - Motif : l’effet agit après le rendu pour créer volontairement des champs alternés, tandis que le
    désentrelacement agit avant redimensionnement sur une source existante. La formule est originale,
    déterministe, sans historique ni actif tiers, sous MIT.
  - Compatibilité : compatible avec toutes les technologies, scalers, restaurations et autres effets
    temporels ; le repli CPU commun garantit le même résultat sur les quatre renderers.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md`, le contrat temporel, sa
    normalisation et son catalogue, la constante de localisation, la clé générée par Argos dans les
    30 ressources `Emulation.resx`, le panneau temporel, le pipeline CPU, les replis OpenGL/Veldrid,
    puis les tests de bornage, persistance, interface, alternance des champs et quatre renderers.
  - Vérifications terminées avant cochage : ciblage configuration/persistances/interface/localisation
    et rendu `7/7`, matrice quatre renderers `1/1`, classes complètes
    configuration/localisation/interface `18/18` ; compilation et contrôle de format finaux réalisés
    juste avant cochage.

- **2026-09-02 — Insertion d’images noires appliquée en dernier**
  - Question : comment garantir que cette option reste différente du scintillement et qu’un autre
    traitement temporel ne rééclaire pas la frame noire ?
  - Décision : ajouter un interrupteur indépendant, désactivé par défaut, qui conserve les séquences
    paires et met entièrement à noir les séquences impaires. La passe est la dernière de la chaîne.
  - Motif : l’alternance déterministe par `VideoFrame.Sequence` ne demande aucun historique et la
    position finale préserve la sémantique BFI. La formule originale reste sous MIT sans actif tiers.
  - Compatibilité : compatible avec toutes les technologies, scalers, restaurations et effets
    temporels ; distincte du scintillement qui conserve au moins la moitié de la lumière. Les quatre
    renderers utilisent le pipeline CPU commun lorsque l’option est active.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md`, le contrat temporel et son
    catalogue, la constante de localisation, la clé générée par Argos dans les 30 ressources
    `Emulation.resx` avec correction française, le panneau temporel, le pipeline CPU, les replis
    OpenGL/Veldrid, puis les tests de persistance, interface, ordre final et quatre renderers.
  - Vérifications terminées avant cochage : ciblage persistances/interface/localisation/rendu `6/6`,
    matrice quatre renderers `1/1`, classes complètes configuration/localisation/interface `18/18` ;
    compilation et contrôle de format finaux réalisés juste avant cochage.

- **2026-09-02 — Simulation composite GW GUI sans duplication moteur**
  - Question : l’effet composite peut-il être proposé sans recréer les normes, timings ou artefacts
    déjà produits par PUAE, Hatari ou Atari800 ?
  - Décision : créer un panneau permanent `Simulations de signal` et une intensité composite
    `0..100`, neutre à `0`, qui agit uniquement sur le `VideoFrame` déjà produit. Les options moteur
    PAL/NTSC, fréquence, région et artifacting ne sont ni lues, ni écrites, ni remplacées.
  - Motif : le catalogue common-shaders confirme la famille visuelle, tandis que l’audit des moteurs
    réserve la génération du signal à l’émulateur. La passe originale limite la chrominance, adoucit
    la luminance horizontalement et ajoute une faible alternance de phase ; elle reste sous MIT sans
    reprise de code ou d’actif tiers.
  - Compatibilité : compatible avec toutes les technologies et traitements ; placée après restauration
    et avant scaler. WPF, OpenGL, Direct3D 11 et Vulkan partagent le repli CPU déterministe lorsqu’elle
    est active.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md`,
    `EmulationSignalSimulationConfiguration.cs`, le contrat vidéo principal, sa normalisation et le
    catalogue, les constantes de localisation, deux clés générées via Argos dans les 30 ressources,
    le panneau permanent, `CpuCompositeVideoProcessingPasses.cs`, le pipeline CPU, les replis
    OpenGL/Veldrid, puis les tests de bornage, persistance, interface, phases composite et renderers.
  - Vérifications terminées avant cochage : ciblage configuration/persistances/interface/localisation
    et rendu `7/7`, matrice quatre renderers `1/1`, classes complètes
    configuration/localisation/interface `18/18` ; compilation et contrôle de format finaux réalisés
    juste avant cochage.

- **2026-09-02 — Simulation S-Video GW GUI à luminance séparée**
  - Question : comment rendre S-Video distinct de composite sans recréer une option de connectique du
    moteur ?
  - Décision : ajouter une intensité post-traitement `0..100`, neutre à `0`, qui préserve la luminance
    linéaire et ne lisse que la chrominance horizontale à hauteur maximale de `12 %`, sans phase ni
    dot crawl.
  - Motif : cette séparation reproduit la différence visuelle utile avec composite tout en agissant
    uniquement sur le `VideoFrame`. La passe originale reste sous MIT sans reprise tierce.
  - Compatibilité : compatible avec toutes les technologies et traitements ; les simulations
    explicitement demandées peuvent se composer, mais aucune norme, région, fréquence ou option
    d’artifacting du moteur n’est lue ou modifiée. Repli CPU commun sur les quatre renderers.
  - Modifications réalisées : référence, contrat et normalisation des simulations, catalogue et
    localisation, clé Argos dans 30 ressources, panneau, `CpuSVideoVideoProcessingPasses.cs`, pipeline,
    replis GPU, tests de persistance/interface, indépendance de séquence et luminance linéaire.
  - Vérifications terminées avant cochage : après correction du test pour mesurer la lumière linéaire,
    ciblage S-Video vert, matrice quatre renderers `1/1`, classes complètes
    configuration/localisation/interface `18/18` ; compilation et format vérifiés juste avant cochage.

- **2026-09-02 — Simulation RF GW GUI déterministe**
  - Question : comment évoquer une transmission RF sans toucher au tuner, à la fréquence ou à la
    région du moteur ?
  - Décision : ajouter une intensité post-traitement `0..100`, neutre à `0`, qui adoucit
    horizontalement l’image puis ajoute un bruit commun borné à `±8 %`, déterminé par position et
    numéro de frame.
  - Motif : cette dégradation reste visuelle et reproductible, sans état global, tout en étant
    distincte de composite et S-Video. La passe originale reste sous MIT sans reprise tierce.
  - Compatibilité : compatible avec toutes les technologies et traitements ; aucune option moteur
    n’est lue ou modifiée. Repli CPU commun sur WPF, OpenGL, Direct3D 11 et Vulkan.
  - Modifications réalisées : référence, contrat/normalisation/catalogue/localisation RF, clé Argos
    dans 30 ressources, panneau, `CpuRfVideoProcessingPasses.cs`, pipeline, replis GPU et tests de
    persistance/interface, déterminisme, changement de séquence et renderers.
  - Vérifications terminées avant cochage : ciblage RF `7/7`, matrice quatre renderers `1/1`, classes
    complètes configuration/localisation/interface `18/18` ; compilation et format vérifiés juste
    avant cochage.

- **2026-09-02 — Simulation PAL GW GUI sans changement de norme**
  - Question : comment évoquer PAL sans modifier le standard ou les timings déjà gérés par le moteur ?
  - Décision : intensité post-traitement `0..100`, neutre à `0`, mélange chromatique vertical borné
    et faible phase alternée `±2,5 %` selon la parité des lignes, indépendante de la séquence.
  - Motif : l’effet reste uniquement visuel ; la norme, la région et la fréquence demeurent des
    options moteur. Passe originale MIT sans reprise tierce.
  - Compatibilité : toutes technologies et traitements, repli CPU commun sur les quatre renderers.
  - Modifications : référence, contrat/normalisation/catalogue/localisation PAL, clé Argos dans
    30 ressources, panneau, `CpuPalVideoProcessingPasses.cs`, pipeline, replis GPU et tests.
  - Vérifications : ciblage PAL `7/7`, matrice quatre renderers `1/1`, classes complètes
    configuration/localisation/interface `18/18` ; compilation et format vérifiés avant cochage.

- **2026-09-02 — Simulation NTSC GW GUI sans changement de norme**
  - Question : comment évoquer NTSC sans modifier standard, fréquence, région ou timing du moteur ?
  - Décision : intensité post-traitement `0..100`, neutre à `0`, avec léger mélange de luminance,
    retard chromatique horizontal et phase de teinte à trois états reproductible par séquence.
  - Motif : l’effet reste visuel et explicitement nommé ; passe originale MIT sans reprise tierce.
  - Compatibilité : toutes technologies et traitements, repli CPU commun sur les quatre renderers,
    aucune option moteur lue ou modifiée.
  - Modifications : référence, contrat/normalisation/catalogue/localisation NTSC, clé Argos dans
    30 ressources avec correction française, panneau, `CpuNtscVideoProcessingPasses.cs`, pipeline,
    replis GPU et tests de déterminisme, phase et renderers.
  - Vérifications : ciblage NTSC `7/7`, matrice quatre renderers `1/1`, classes complètes
    configuration/localisation/interface `18/18` ; compilation et format vérifiés avant cochage.

- **2026-09-02 — Grain stylistique distinct du bruit RF**
  - Question : comment ajouter du grain sans réutiliser ni confondre le bruit de transmission RF ?
  - Décision : nouveau contrat et panneau `Effets stylistiques`, intensité `0..100` neutre à `0`,
    bruit monochrome déterministe borné à `±7 %` et appliqué à la résolution de sortie.
  - Motif : RF reste une dégradation du signal avant scaler ; le grain est un effet final fin. Passe
    originale MIT sans code, texture ou actif tiers.
  - Compatibilité : toutes technologies et traitements, repli CPU commun sur les quatre renderers.
  - Modifications : référence, contrat stylistique, configuration/normalisation/catalogue,
    localisation et deux clés Argos dans 30 ressources avec correction française, panneau,
    `CpuGrainVideoProcessingPasses.cs`, pipeline, replis GPU et tests.
  - Vérifications : ciblage grain `7/7`, matrice quatre renderers `1/1`, classes complètes
    configuration/localisation/interface `18/18` ; compilation et format vérifiés avant cochage.

- **2026-09-02 — VHS déterministe à la résolution de sortie**
  - Question : quels défauts VHS restent distincts du grain et des simulations de signal ?
  - Décision : intensité `0..100` neutre à `0`, décalage par ligne jusqu’à trois pixels, bavure rouge
    et bleue jusqu’à `45 %`, et atténuation déterministe d’une ligne sur dix-sept.
  - Motif : effet stylistique final original MIT, sans code ni actif tiers.
  - Compatibilité : toutes technologies et traitements ; repli CPU commun aux quatre renderers.
  - Modifications : référence, contrat/normalisation/catalogue/localisation VHS, clé Argos dans
    30 ressources, panneau, `CpuVhsVideoProcessingPasses.cs`, pipeline, replis GPU et tests.
  - Vérifications : test VHS corrigé pour comparer les lignes en excluant alpha et tenir compte du
    transfert sRGB, matrice quatre renderers `1/1`, classes complètes `18/18` ; build et format avant coche.

- **2026-09-02 — Aberration chromatique spatiale et déterministe**
  - Question : quelle séparation RGB reste lisible sans devenir une seconde simulation de signal ?
  - Décision : intensité `0..100` neutre à `0`, rouge et bleu décalés en sens opposés jusqu’à
    trois pixels avec interpolation, vert conservé et bords pincés.
  - Motif : effet stylistique final original MIT, sans code, shader, texture ni actif tiers.
  - Compatibilité : toutes technologies et traitements ; après VHS, avant grain et effets
    temporels ; repli CPU commun aux quatre renderers.
  - Modifications : référence, contrat/normalisation/catalogue/localisation, clé Argos dans
    30 ressources, panneau, `CpuChromaticAberrationVideoProcessingPasses.cs`, pipeline, replis GPU
    et tests.
  - Vérifications : test dédié `1/1`, matrice quatre renderers `1/1`, classes complètes
    configuration/localisation/UI `18/18` ; build et format avant coche.

- **2026-09-02 — Bloom borné des hautes lumières**
  - Question : comment proposer un bloom général sans dupliquer les halos propres aux technologies ?
  - Décision : intensité `0..100` neutre à `0`, seuil linéaire à `60 %`, diffusion dans un
    rayon de deux pixels et réaddition bornée à `35 %`.
  - Motif : effet stylistique final original MIT, sans code, shader, texture ni actif tiers.
  - Compatibilité : toutes technologies et traitements ; les halos CRT, vectoriel et VFD restent
    des paramètres distincts ; repli CPU commun aux quatre renderers.
  - Modifications : référence, contrat/normalisation/catalogue/localisation Bloom, clé Argos dans
    30 ressources, panneau, `CpuBloomVideoProcessingPasses.cs`, pipeline, replis GPU et tests.
  - Vérifications : test dédié `1/1`, matrice quatre renderers `1/1`, classes complètes
    configuration/localisation/UI `18/18` ; build et format avant coche.

- **2026-09-02 — Sépia progressif en lumière linéaire**
  - Question : quelle transformation sépia reste progressive et préserve un état réellement neutre ?
  - Décision : intensité `0..100` neutre à `0`, luminance Rec. 709 puis cible chaude
    `1,07 / 0,93 / 0,74`, mélangée à la couleur source et bornée.
  - Motif : effet stylistique final original MIT, sans code, shader, texture ni actif tiers.
  - Compatibilité : toutes technologies et traitements ; les niveaux de gris appliqués ensuite
    retirent explicitement la teinte ; repli CPU commun aux quatre renderers.
  - Modifications : référence, contrat/normalisation/catalogue/localisation Sépia, clé Argos dans
    30 ressources, panneau, `CpuSepiaVideoProcessingPasses.cs`, pipeline, replis GPU et tests.
  - Vérifications : test dédié `1/1`, matrice quatre renderers `1/1`, classes complètes
    configuration/localisation/UI `18/18` ; build et format avant coche.

- **2026-09-02 — Niveaux de gris progressifs par luminance**
  - Question : comment distinguer l’effet global des palettes et modes monochromes des écrans ?
  - Décision : intensité `0..100` neutre à `0`, luminance linéaire Rec. 709 mélangée aux trois
    composantes ; à `100`, RGB sont égaux sans modifier la technologie choisie.
  - Motif : effet stylistique final original MIT, sans code, shader, texture ni actif tiers.
  - Compatibilité : toutes technologies et traitements ; après sépia, dont la teinte est retirée
    à intensité maximale ; repli CPU commun aux quatre renderers.
  - Modifications : référence, contrat/normalisation/catalogue/localisation Grayscale, clé Argos
    dans 30 ressources avec correction française « Niveaux de gris », panneau,
    `CpuGrayscaleVideoProcessingPasses.cs`, pipeline, replis GPU et tests.
  - Vérifications : test dédié `1/1`, matrice quatre renderers `1/1`, classes complètes
    configuration/localisation/UI `18/18` ; build et format avant coche.

- **2026-09-02 — Validation réelle finale du point 7**
  - Question : comment couvrir rapidement les transitions d’affichage sans confondre les options
    natives des moteurs avec les traitements communs de GW GUI ?
  - Décision : utiliser une seule instance Debug, conserver les réglages dans leurs configurations
    Amiga et Atari respectives, puis compléter l’observation réelle Direct3D 11 par la matrice
    automatisée des quatre renderers et du repli WPF.
  - Modifications vérifiées : configurations Amiga `be1c4348b0a84927b53a29e940251e6f`
    et Atari `6c532ba3b52f451680029fa836a020fc`; aucune modification de code supplémentaire.
  - Vérifications : même PID `12004` pour Amiga et Atari ; technologies CRT et Plasma persistées
    séparément ; Amiga 1200 ouvert en Direct3D 11 ; redimensionnement et plein écran fonctionnels ;
    matrices WPF/OpenGL/Direct3D 11/Vulkan et repli WPF couvertes par les tests ciblés ; fermeture
    propre ; journal d’erreurs inchangé à `13 227` octets ; suite finale `145/145` réussie.
