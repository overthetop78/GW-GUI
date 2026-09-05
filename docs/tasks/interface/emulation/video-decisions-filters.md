# Journal vidéo : Plasma, vectoriel et filtres avancés

[Sommaire](../emulation-improvements.md) · [Règles communes](rules.md)

- **2026-09-02 — Paramètres exacts du modèle Plasma**
  - Question : quelles conversions appliquer aux cellules, à la diffusion, au tramage temporel et à
    la rémanence avant d’écrire leur référence CPU ?
  - Décision : utiliser quatre intensités `0..100` neutres à zéro ; cellules RGB avec atténuation
    maximale de `35 %` et interstice maximal de `20 %`, tramage Bayer 4×4 séquencé d’amplitude
    maximale `4 %`, diffusion 3×3 maximale `50 %`, puis rémanence maximale sur une unique image
    précédente pondérée par l’intensité.
  - Motif : ces formules originales sont bornées, déterministes et portables dans les shaders, sans
    dépendance de licence tierce ni historique non borné.
  - Modifications : `docs/reference/emulation-video-filters.md`, section `Plasma` ; la définition
    existante `EmulationPlasmaVideoConfiguration` conserve ses quatre champs validés.

- **2026-09-02 — Approximation raster de l’écran vectoriel**
  - Question : comment définir lignes, halo et persistance sans primitives vectorielles fournies par
    les moteurs actuels ?
  - Décision : utiliser le gradient Sobel de luminance linéaire, un seuil `0..1` avec transition
    `0,10`, un renforcement de ligne `0..100`, un halo 3×3 maximal `50 %` et une persistance sur une
    seule image précédente pondérée par `0..100`.
  - Motif : cette approximation conserve explicitement son origine raster, reste déterministe et
    portable, et ne revendique pas la précision d’un moteur transmettant des primitives.
  - Modifications : `docs/reference/emulation-video-filters.md`, section
    `Écran vectoriel — approximation raster`.

- **2026-09-02 — Variante xBR intégrable**
  - Question : quelle variante, quelle source et quelle licence retenir pour le premier scaler pixel
    art sans casser les quatre renderers ni les échelles non entières ?
  - Décision : adapter le niveau 1 mono-passe de `xbr-lv3.glsl` de Hyllian, avec ses constantes
    publiées (`Y=48`, transition de coin `1,10..1,90`) et une sélection déterministe du coin au poids
    maximal ; le choix `xBR` rejoint le sélecteur d’échantillonnage existant.
  - Motif : le fichier de référence porte explicitement la licence MIT, la variante reste symétrique,
    bornée et portable en CPU, GLSL 1.20 et shader Veldrid, sans runtime ni texture Libretro.
  - Modifications : `docs/reference/emulation-video-filters.md`, section
    `xBR — niveau 1 mono-passe`, et `THIRD-PARTY-NOTICES.md`.

- **2026-09-02 — xBRZ sans contamination GPL**
  - Question : comment proposer xBRZ alors que le fichier Libretro de référence attribue cette
    partie à un code GPL-3.0 dont l’exception de liaison vise MAME seulement ?
  - Décision : ne copier aucun code xBRZ et développer une formule originale 3×3, documentée sous
    `xBRZ — réimplémentation compatible avec la licence MIT de GW GUI`, exécutée par la référence CPU
    commune dans les quatre surfaces.
  - Motif : la séparation protège la licence MIT du projet, donne un résultat strictement identique
    sur tous les backends et évite d’alourdir encore les shaders monolithiques.
  - Modifications : `docs/reference/emulation-video-filters.md`, modèle commun, référence CPU,
    surfaces et tests dédiés.

- **2026-09-02 — Source HQx permissive**
  - Question : quelle implémentation HQx peut être adaptée sans LUT externe ni licence incompatible ?
  - Décision : adapter le noyau HQ2x mono-passe de mGBA (`hq2x.fs`), sous MIT, dans la référence CPU
    commune et conserver ses seuils, motifs et poids.
  - Motif : cette source est explicite, autonome et compatible avec la licence MIT de GW GUI ; le
    repli commun garantit l’identité des quatre surfaces.
  - Modifications : référence, notice tierce, modèle, pipeline, surfaces et tests HQx.

- **2026-09-02 — Adaptation portable de ScaleFX**
  - Question : faut-il intégrer les cinq passes GPU de ScaleFX séparément dans chacun des quatre
    renderers, ou conserver une référence unique malgré son coût CPU ?
  - Décision : adapter la chaîne MIT officielle de Sp00kyFox en une reconstruction CPU 3× à palette
    préservée, puis présenter son résultat par une copie neutre dans les quatre surfaces.
  - Motif : la source et ses cinq passes sont explicitement MIT, mais les pipelines WPF, OpenGL et
    Veldrid n’exposent pas le même mécanisme de textures intermédiaires ; la référence commune évite
    des résultats différents et reste cohérente avec les replis xBRZ et HQx déjà validés.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md:416` (source, licence et
    comportement), `THIRD-PARTY-NOTICES.md:25` (notice MIT),
    `EmulationVideoSampling.cs:12` et `EmulationResourceKeys.cs:55` (modèle et constante), les 30
    fichiers `Resources/*/Emulation.resx` aux lignes finales `682`, `819` ou `1284` via Argos,
    `CpuScaleFxVideoScaler.cs:1` et `CpuEmulationVideoProcessingPipeline.cs:332` (traitement),
    `OpenGlVideoSurface.cs:122` et `VeldridVideoSurface.cs:89` (repli commun), puis
    `EmulationVideoProcessingPipelineTests.cs:196`, `:260`, `:957` et `:1059` (couverture).

- **2026-09-02 — ScaleNx sans reprise du shader GPL**
  - Question : peut-on intégrer le shader Scale3x officiel alors que son en-tête impose la GNU GPL ?
  - Décision : ne copier aucun code du shader et réimplémenter indépendamment les règles publiques
    de Scale2x/Scale3x dans la référence CPU commune, sous la licence MIT du projet.
  - Motif : les règles de voisinage sont simples et vérifiables, tandis qu’une copie du fichier GPL
    serait incompatible avec la politique de licence déjà validée ; le repli commun maintient la
    même image sur les quatre surfaces.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md:438`,
    `EmulationVideoSampling.cs:13`, `EmulationResourceKeys.cs:56`, les 30 ressources
    `Emulation.resx` via Argos, `CpuScaleNxVideoScaler.cs:1`,
    `CpuEmulationVideoProcessingPipeline.cs:339`, `OpenGlVideoSurface.cs:122`,
    `VeldridVideoSurface.cs:89`, puis `EmulationVideoProcessingPipelineTests.cs:197`, `:265`,
    `:963` et `:1095`.

- **2026-09-02 — SABR sans reprise du shader GPL**
  - Question : la variante SABR v3.0 peut-elle être intégrée directement au projet MIT ?
  - Décision : ne copier aucun élément de la source GPL-2.0-or-later et développer un scaler
    original à interpolation diagonale, documenté sous la licence MIT de GW GUI.
  - Motif : l’en-tête de la source est sans ambiguïté ; un noyau CPU commun à voisinage 3×3 permet
    néanmoins de fournir l’effet anti-crénelé attendu et une sortie identique sur les quatre
    renderers sans contamination de licence.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md:456`,
    `EmulationVideoSampling.cs:14`, `EmulationResourceKeys.cs:57`, les 30 ressources
    `Emulation.resx` via Argos, `CpuSabrVideoScaler.cs:1`,
    `CpuEmulationVideoProcessingPipeline.cs:346`, `OpenGlVideoSurface.cs:122`,
    `VeldridVideoSurface.cs:89`, puis `EmulationVideoProcessingPipelineTests.cs:198`, `:270`,
    `:969` et `:1134`.

- **2026-09-02 — Dé-dithering indépendant avant scaler**
  - Question : un dé-dithering GW GUI duplique-t-il une option Amiga/Atari, et quelle première
    variante possède une licence compatible et un comportement borné ?
  - Décision : aucun doublon n’existe dans les deux modules ; adapter le détecteur de damiers MIT de
    Hyllian dans une passe CPU commune, avec une intensité `0..100` et valeur neutre `0`.
  - Motif : le motif en damier est vérifiable sans heuristique temporelle, la source est explicitement
    MIT et l’exécution avant scaler empêche que le redimensionnement masque le motif original.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md:476`,
    `THIRD-PARTY-NOTICES.md:47`, `EmulationImageRestorationConfiguration.cs:1`,
    `EmulationVideoProcessingConfiguration.cs:11`,
    `EmulationVideoProcessingConfigurationFunctions.cs:17`,
    `EmulationVideoProcessingCatalog.cs:13`, `EmulationResourceKeys.cs:40`, les 30 ressources
    `Emulation.resx` via Argos, `EmulationVideoProcessingSettingsSection.cs:113`,
    `EmulationImageRestorationFunctions.cs:1`, `CpuEmulationVideoProcessingPipeline.cs:47`,
    `OpenGlVideoSurface.cs:23`, `VeldridVideoSurface.cs:44`, puis les tests de configuration,
    panneau et pipeline aux lignes `33`, `43`, `327` et `1210` de leurs fichiers respectifs.

- **2026-09-02 — Débruitage bilatéral original avant scaler**
  - Question : un débruitage GW GUI duplique-t-il une option Amiga/Atari, et la passe bilatérale
    Libretro peut-elle être intégrée directement au projet MIT ?
  - Décision : aucun doublon n’existe dans les deux modules ; ne reprendre aucun code du shader
    bilatéral GPL-2.0-or-later et développer une passe bilatérale 3×3 originale, avec intensité
    `0..100`, valeur neutre `0`, après le dé-dithering et avant le scaler.
  - Motif : la pondération spatiale et colorimétrique réduit les petites variations d’un aplat sans
    mélanger les deux côtés d’un contour marqué ; elle reste distincte de la reconnaissance des
    damiers et donne un résultat déterministe commun aux quatre renderers.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md:497`,
    `EmulationImageRestorationConfiguration.cs:7`,
    `EmulationVideoProcessingConfigurationFunctions.cs:27`,
    `EmulationVideoProcessingCatalog.cs:14`, `EmulationResourceKeys.cs:83`, les 30 ressources
    `Emulation.resx` via Argos (`00-Base/Emulation.resx:824`, correction française validée à
    `fr-FR/Emulation.resx:687`), `EmulationVideoProcessingSettingsSection.cs:120`,
    `EmulationImageRestorationFunctions.cs:53`, `CpuEmulationVideoProcessingPipeline.cs:49`,
    `OpenGlVideoSurface.cs:126`, `VeldridVideoSurface.cs:93`, puis les tests de configuration,
    panneau et pipeline aux lignes `33`, `44`, `374` et `1292` de leurs fichiers respectifs.
  - Vérifications terminées avant cochage : tests ciblés `3/3`, équivalence renderer `4/4`, suite
    vidéo/configuration/interface/localisation `105/105`, compilation GWGUI.App sans avertissement
    ni erreur et `git diff --check` sans erreur.

- **2026-09-02 — Réduction des bandes originale sans grain**
  - Question : la réduction des bandes du catalogue duplique-t-elle une option Amiga/Atari, et la
    source Libretro peut-elle être reprise dans le projet MIT ?
  - Décision : aucun doublon n’existe dans les modules ; ne reprendre aucun code de la source
    GPL/LGPL issue de mpv et créer une reconstruction déterministe des faibles marches, avec
    intensité `0..100`, valeur neutre `0`, après débruitage et avant scaler.
  - Motif : la sélection d’une direction de gradient cohérente lisse les transitions quantifiées,
    tout en rejetant les pics locaux comme bruit et les grands écarts comme contours ; aucun grain
    pseudo-aléatoire n’est ajouté.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md:520`,
    `EmulationImageRestorationConfiguration.cs:8`,
    `EmulationVideoProcessingConfigurationFunctions.cs:28`,
    `EmulationVideoProcessingCatalog.cs:15`, `EmulationResourceKeys.cs:84`, les 30 ressources
    `Emulation.resx` via Argos (`00-Base/Emulation.resx:825`, `fr-FR/Emulation.resx:688`),
    `EmulationVideoProcessingSettingsSection.cs:123`,
    `EmulationImageRestorationFunctions.cs:98`, `CpuEmulationVideoProcessingPipeline.cs:51`,
    `OpenGlVideoSurface.cs:127`, `VeldridVideoSurface.cs:94`, puis les tests de configuration,
    panneau et pipeline aux lignes `33`, `45`, `421` et `1371` de leurs fichiers respectifs.
  - Vérifications terminées avant cochage : tests ciblés `3/3`, équivalence renderer `4/4`, suite
    vidéo/configuration/interface/localisation `110/110`, compilation GWGUI.App sans avertissement
    ni erreur et `git diff --check` sans erreur.

- **2026-09-02 — Netteté avancée distincte sous le nom Récupération de détails**
  - Question : comment proposer une netteté avancée sans dupliquer le réglage général déjà présent ?
  - Décision : conserver `Netteté` comme ajustement global `-10..+10` après mise à l’échelle, et
    ajouter `Récupération de détails` comme restauration positive `0..100`, neutre à `0`, à la
    résolution source avant scaler ; aucun moteur Amiga/Atari n’expose cette fonction.
  - Motif : la passe renforce uniquement le résidu de micro-détail, réduit progressivement sa force
    près des contours marqués et borne la sortie autour des extrema locaux ; elle ne sert ni à
    flouter, ni à régler la netteté finale du modèle d’affichage.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md:541`,
    `EmulationImageRestorationConfiguration.cs:9`,
    `EmulationVideoProcessingConfigurationFunctions.cs:29`,
    `EmulationVideoProcessingCatalog.cs:16`, `EmulationResourceKeys.cs:85`, les 30 ressources
    `Emulation.resx` via Argos (`00-Base/Emulation.resx:826`, correction française validée à
    `fr-FR/Emulation.resx:689`), `EmulationVideoProcessingSettingsSection.cs:126`,
    `EmulationImageRestorationFunctions.cs:142`, `CpuEmulationVideoProcessingPipeline.cs:53`,
    `OpenGlVideoSurface.cs:128`, `VeldridVideoSurface.cs:95`, puis les tests de configuration,
    panneau et pipeline aux lignes `34`, `46`, `468` et `1451` de leurs fichiers respectifs.
  - Vérifications terminées avant cochage : tests ciblés `3/3`, équivalence renderer `4/4`, suite
    vidéo/configuration/interface/localisation `115/115`, compilation GWGUI.App sans avertissement
    ni erreur et `git diff --check` sans erreur.

- **2026-09-02 — Désentrelacement spatial avec champ explicite**
  - Question : comment désentrelacer sans métadonnée d’entrelacement ou de dominance de champ, et
    sans dupliquer une option moteur ?
  - Décision : aucun doublon n’existe dans les modules Amiga/Atari ; proposer un select
    `Désactivé`, `Bob — lignes paires`, `Bob — lignes impaires` et `Fusion verticale`, sans
    détection automatique ni historique implicite.
  - Motif : le choix explicite évite de prétendre connaître le champ dominant ; Bob reconstruit les
    lignes manquantes depuis le champ conservé, tandis que Fusion réduit le peigne par pondération
    verticale au prix d’un adoucissement documenté. Le mode désactivé reste strictement neutre.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md:562`,
    `EmulationDeinterlacingMode.cs:3`, `EmulationImageRestorationConfiguration.cs:11`,
    `EmulationVideoProcessingConfigurationFunctions.cs:30`,
    `EmulationVideoProcessingCatalog.cs:17`, `EmulationResourceKeys.cs:86`, les cinq clés dans les
    30 ressources `Emulation.resx` via Argos (`00-Base/Emulation.resx:827`, corrections françaises
    validées à `fr-FR/Emulation.resx:690`), `EmulationVideoProcessingSettingsSection.cs:129`,
    `EmulationImageRestorationFunctions.cs:11`, `CpuEmulationVideoProcessingPipeline.cs:47`,
    `OpenGlVideoSurface.cs:129`, `VeldridVideoSurface.cs:96`, puis les tests de configuration,
    panneau et pipeline aux lignes `34`, `47`, `515` et `1531` de leurs fichiers respectifs.
  - Vérifications terminées avant cochage : tests ciblés `3/3`, équivalence renderer `4/4`, suite
    vidéo/configuration/interface/localisation `120/120`, compilation GWGUI.App sans avertissement
    ni erreur et `git diff --check` sans erreur.

- **2026-09-02 — Audit des incompatibilités des filtres indépendants**
  - Question : quelles combinaisons exigent une nouvelle confirmation Oui/Non ?
  - Décision : aucune incompatibilité interne n’existe parmi les filtres implémentés ; ne créer
    aucune boîte inutile. Les scalers sont exclusifs par leur select unique et les cinq restaurations
    sont composables dans un ordre fixe. La confirmation existante reste réservée au remplacement
    d’une technologie d’affichage active.
  - Motif : un test exécute simultanément toutes les restaurations avec chacun des dix scalers sans
    effacer de valeur ; le test d’interface vérifie désormais qu’un refus de remplacement conserve
    l’intégralité de la configuration, y compris scaler et restaurations.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md:614`,
    `EmulationVideoProcessingPipelineTests.cs:1563` et
    `EmulationVideoSettingsSectionTests.cs:224`.
  - Vérifications terminées avant cochage : tests ciblés `2/2`, suite
    vidéo/configuration/interface/localisation `121/121`, compilation GWGUI.App sans avertissement
    ni erreur et `git diff --check` sans erreur.

- **2026-09-02 — Validation visuelle consolidée des filtres avancés**
  - Question : comment laisser une preuve visuelle comparable de chaque scaler et restauration sur
    les quatre renderers malgré leurs tailles natives différentes hors écran ?
  - Décision : produire une image déterministe contenant damier, bandes, bruit, micro-détail,
    entrelacement et diagonales ; générer douze cellules (témoin, six scalers, cinq restaurations),
    vérifier que chaque cellule active diffère du témoin, puis écrire une planche PNG par renderer.
  - Motif : les tests dédiés assurent déjà l’équivalence fonctionnelle 4/4 ; les planches natives
    complètent cette preuve sans comparer abusivement WPF 598×48 aux hôtes GPU 1510×88. OpenGL,
    Direct3D 11 et Vulkan produisent exactement le même PNG et le même SHA-256.
  - Modifications réalisées : `EmulationVideoProcessingPipelineTests.cs:635`, image source à
    `:1784`, écriture PNG généralisée à `:1837` et racine du dépôt à `:1875`. Artefacts :
    `build/validation/advanced-filter-validation/advanced-wpf.png`, `advanced-opengl.png`,
    `advanced-direct3d11.png` et `advanced-vulkan.png`.
  - Vérifications terminées avant cochage : planche ciblée `1/1`, suite finale
    vidéo/configuration/interface/localisation `122/122`, compilation GWGUI.App sans avertissement
    ni erreur et `git diff --check` sans erreur.
