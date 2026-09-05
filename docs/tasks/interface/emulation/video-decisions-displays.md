# Journal vidéo : écrans spécialisés

[Sommaire](../emulation-improvements.md) · [Règles communes](rules.md)

- **2026-09-02 — VFD comme technologie d’affichage spécialisée**
  - Question : quels paramètres VFD peuvent être simulés honnêtement depuis une frame raster sans
    machine actuelle fournissant les segments physiques ?
  - Décision : ajouter VFD comme technologie exclusive avec phosphore bleu, vert, ambre ou rouge,
    intensité `70`, halo `25` et persistance `20` par défaut, les trois intensités restant réglables
    sur `0..100`.
  - Motif : les documents constructeur Noritake confirment l’émission par phosphore, les couleurs et
    la luminance ; la passe est explicitement une approximation raster monochrome avec halo 3×3 et
    historique borné, sans code ni actif tiers.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md:602`,
    `EmulationVideoDisplayTechnology.cs:10`, `EmulationVfdColor.cs:3`,
    `EmulationVfdVideoConfiguration.cs:5`, `EmulationVideoProcessingConfiguration.cs:16`,
    `EmulationVideoProcessingConfigurationFunctions.cs:84`,
    `EmulationVideoProcessingCatalog.cs:57`, `EmulationResourceKeys.cs:49`, les neuf clés dans les
    30 ressources `Emulation.resx` via Argos (`00-Base/Emulation.resx:832`, corrections françaises
    à `fr-FR/Emulation.resx:695`), `EmulationVideoProcessingSettingsSection.cs:302`,
    `CpuVfdVideoProcessingPasses.cs:6`, `CpuEmulationVideoProcessingPipeline.cs:216`,
    `OpenGlVideoSurface.cs:130`, `VeldridVideoSurface.cs:97`, puis les tests aux lignes `195`,
    `1669`, `39` et `35` de leurs fichiers respectifs.
  - Vérifications terminées avant cochage : ciblage VFD et matrice quatre renderers `4/4`,
    configuration/localisation/interface `18/18`, compilation GWGUI.App sans avertissement ni
    erreur et `git diff --check` sans erreur.

- **2026-09-02 — Matrice LED comme technologie d’affichage spécialisée**
  - Question : comment distinguer une matrice LED du LCD rétroéclairé par LED déjà présent sans
    prétendre disposer de la géométrie physique de la machine émulée ?
  - Décision : ajouter une technologie exclusive `Matrice LED`, avec select RGB, rouge, vert, ambre,
    bleu ou blanc, puis taille des cellules `35`, espacement `30`, diffusion `20` et luminosité `75`
    par défaut ; tous les réglages numériques sont bornés à `0..100`.
  - Motif : les guides matériels Adafruit confirment les panneaux à pas distincts et l’effet d’une
    plaque diffusante. La passe est donc explicitement une approximation raster par cellules de
    `2..8` pixels, masque circulaire, espace sombre et diffusion, sans code ni actif tiers.
  - Compatibilité : technologie exclusive des autres affichages, mais compatible avec chaque scaler,
    restauration et réglage général. WPF utilise le pipeline CPU commun ; OpenGL, Direct3D 11 et
    Vulkan utilisent son repli CPU déterministe.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md`,
    `EmulationVideoDisplayTechnology.cs:11`, `EmulationLedMatrixColor.cs:3`,
    `EmulationLedMatrixVideoConfiguration.cs:5`, `EmulationVideoProcessingConfiguration.cs:17`,
    `EmulationVideoProcessingConfigurationFunctions.cs:18`,
    `EmulationVideoProcessingCatalog.cs:61`, `EmulationResourceKeys.cs:50`, les douze clés dans les
    30 ressources `Emulation.resx` via Argos (corrections françaises à
    `fr-FR/Emulation.resx:704`), `EmulationVideoProcessingSettingsSection.cs:319`,
    `CpuLedMatrixVideoProcessingPasses.cs:6`, `CpuEmulationVideoProcessingPipeline.cs:318`,
    `OpenGlVideoSurface.cs:130`, `VeldridVideoSurface.cs:97`, puis les tests aux lignes `41`, `207`,
    `1714` et `37` de leurs fichiers respectifs.
  - Vérifications terminées avant cochage : ciblage normalisation/persistance/interface/localisation,
    rendu LED et matrice quatre renderers `7/7`, classes complètes configuration/localisation/interface
    `18/18`, compilation GWGUI.App sans avertissement ni erreur et `git diff --check` sans erreur.

- **2026-09-02 — Matrice de points distincte de la matrice LED et des segments**
  - Question : quels paramètres permettent une simulation utile depuis une frame raster sans
    confondre matrice de points LCD, matrice LED et futur affichage à segments ?
  - Décision : ajouter une technologie exclusive `Matrice de points` avec palettes LCD vert, LCD
    gris, ambre ou bleu, forme ronde ou carrée, taille `55`, contraste `70` et réponse `120 ms` par
    défaut. Taille et contraste utilisent `0..100`, la réponse `0..1000 ms`.
  - Motif : les références Crystalfontz et Newhaven confirment la matrice LCD et une taille de point
    matérielle. La passe originale moyenne la luminance par cellule, masque chaque point et interpole
    fond/encre ; un historique séparé applique la réponse exponentielle, sans actif ni code tiers.
  - Compatibilité : technologie exclusive des autres affichages, compatible avec tous les scalers,
    restaurations et réglages généraux ; repli CPU commun déterministe pour WPF, OpenGL, Direct3D 11
    et Vulkan.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md`,
    `EmulationVideoDisplayTechnology.cs:12`, `EmulationDotMatrixPalette.cs:3`,
    `EmulationDotMatrixShape.cs:3`, `EmulationDotMatrixVideoConfiguration.cs:5`,
    `EmulationVideoProcessingConfiguration.cs:18`,
    `EmulationVideoProcessingConfigurationFunctions.cs:19`,
    `EmulationVideoProcessingCatalog.cs:66`, `EmulationResourceKeys.cs`, les douze clés dans les
    30 ressources `Emulation.resx` via Argos (corrections françaises à
    `fr-FR/Emulation.resx:716`), `EmulationVideoProcessingSettingsSection.cs:339`,
    `CpuDotMatrixVideoProcessingPasses.cs:6`, `CpuEmulationVideoProcessingPipeline.cs:255`,
    `OpenGlVideoSurface.cs:132`, `VeldridVideoSurface.cs:99`, puis les tests aux lignes `43`, `221`,
    `1768` et `39` de leurs fichiers respectifs.
  - Vérifications terminées avant cochage : ciblage normalisation/persistance/interface/localisation,
    rendu spatial/temporel et matrice quatre renderers `7/7`, classes complètes
    configuration/localisation/interface `18/18`, compilation GWGUI.App sans avertissement ni erreur
    et `git diff --check` sans erreur.

- **2026-09-02 — Affichages à 7, 14 et 16 segments**
  - Question : comment rendre un affichage à segments depuis une frame raster sans prétendre décoder
    les chiffres ou caractères propres à une machine ?
  - Décision : ajouter une technologie exclusive avec dispositions 7, 14 ou 16 segments, couleurs
    rouge, verte, ambre, bleue ou blanche, épaisseur `55`, contraste `80`, halo `20` et réponse
    `30 ms` par défaut ; les trois intensités utilisent `0..100` et la réponse `0..1000 ms`.
  - Motif : les fiches Broadcom confirment les géométries sept et quatorze segments, l’émission LED
    uniforme et l’intérêt d’une surface contrastée. La passe reste une approximation raster
    géométrique, complétée par seize segments, sans décodage sémantique ni actif ou code tiers.
  - Compatibilité : technologie exclusive des autres affichages, compatible avec tous les scalers,
    restaurations et réglages généraux ; historique de réponse séparé et repli CPU commun
    déterministe pour WPF, OpenGL, Direct3D 11 et Vulkan.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md`,
    `EmulationVideoDisplayTechnology.cs:13`, `EmulationSegmentDisplayLayout.cs:3`,
    `EmulationSegmentDisplayColor.cs:3`, `EmulationSegmentDisplayVideoConfiguration.cs:5`,
    `EmulationVideoProcessingConfiguration.cs:19`,
    `EmulationVideoProcessingConfigurationFunctions.cs:20`,
    `EmulationVideoProcessingCatalog.cs:71`, `EmulationResourceKeys.cs`, les quinze clés dans les
    30 ressources `Emulation.resx` via Argos (corrections françaises à
    `fr-FR/Emulation.resx:728`), `EmulationVideoProcessingSettingsSection.cs:363`,
    `CpuSegmentDisplayVideoProcessingPasses.cs:6`, `CpuEmulationVideoProcessingPipeline.cs:298`,
    `OpenGlVideoSurface.cs:133`, `VeldridVideoSurface.cs:100`, puis les tests aux lignes `45`, `235`,
    `1839` et `41` de leurs fichiers respectifs.
  - Vérifications terminées avant cochage : ciblage normalisation/persistance/interface/localisation,
    géométrie/couleurs/halo/réponse et matrice quatre renderers `7/7`, classes complètes
    configuration/localisation/interface `18/18`, compilation GWGUI.App sans avertissement ni erreur
    et `git diff --check` sans erreur.

- **2026-09-02 — Papier électronique à palette et historique dédiés**
  - Question : quels paramètres rendent le papier électronique utile sans le réduire à un simple
    filtre niveaux de gris ni dupliquer la réponse LCD ?
  - Décision : ajouter une technologie exclusive avec modes monochrome, seize niveaux de gris ou
    4096 couleurs, contraste `70`, tramage `35`, rafraîchissement `500 ms` et image fantôme `20` par
    défaut ; les intensités utilisent `0..100` et le rafraîchissement `0..1000 ms`.
  - Motif : E Ink documente seize gris, 4096 couleurs, le tramage, les vitesses de rafraîchissement et
    le ghosting. La passe originale quantifie avec une matrice Bayer et utilise un historique propre,
    sans code ni actif tiers.
  - Compatibilité : technologie exclusive des autres affichages, compatible avec tous les scalers,
    restaurations et réglages généraux ; repli CPU commun déterministe pour WPF, OpenGL,
    Direct3D 11 et Vulkan.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md`,
    `EmulationVideoDisplayTechnology.cs:14`, `EmulationEPaperColorMode.cs:3`,
    `EmulationEPaperVideoConfiguration.cs:5`, `EmulationVideoProcessingConfiguration.cs:20`,
    `EmulationVideoProcessingConfigurationFunctions.cs:22`,
    `EmulationVideoProcessingCatalog.cs:77`, `EmulationResourceKeys.cs`, les neuf clés dans les
    30 ressources `Emulation.resx` via Argos (corrections françaises à
    `fr-FR/Emulation.resx:743`), `EmulationVideoProcessingSettingsSection.cs:392`,
    `CpuEPaperVideoProcessingPasses.cs:6`, `CpuEmulationVideoProcessingPipeline.cs:341`,
    `OpenGlVideoSurface.cs:134`, `VeldridVideoSurface.cs:101`, puis les tests aux lignes `48`, `249`,
    `1911` et `43` de leurs fichiers respectifs.
  - Vérifications terminées avant cochage : ciblage normalisation/persistance/interface/localisation,
    modes/contraste/tramage/rafraîchissement/ghosting et matrice quatre renderers `7/7`, classes
    complètes configuration/localisation/interface `18/18`, compilation GWGUI.App sans avertissement
    ni erreur et `git diff --check` sans erreur.

- **2026-09-02 — Projection à traitement optique raster**
  - Question : la projection apporte-t-elle un rendu distinct des technologies d’écran déjà
    présentes, et quels réglages peuvent rester compréhensibles et bornés ?
  - Décision : ajouter une technologie exclusive avec flou optique `20`, diffusion lumineuse `15`,
    texture de toile `10` et convergence RGB `5` par défaut, chaque intensité étant réglable sur
    `0..100`. La convergence translate le rouge et le bleu autour du vert jusqu’à trois pixels.
  - Motif : la documentation Epson distingue l’alignement des panneaux rouge et bleu sur le vert et
    l’influence de la surface de projection. Une passe originale reproduit ces propriétés sans code,
    shader ni actif tiers ; elle reste sous licence MIT.
  - Compatibilité : technologie exclusive des autres affichages, compatible avec tous les scalers,
    restaurations et réglages généraux ; traitement spatial sans historique, avec repli CPU commun
    déterministe pour WPF, OpenGL, Direct3D 11 et Vulkan.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md`,
    `EmulationVideoDisplayTechnology.cs:15`, `EmulationProjectionVideoConfiguration.cs:3`,
    `EmulationVideoProcessingConfiguration.cs:21`,
    `EmulationVideoProcessingConfigurationFunctions.cs:23`,
    `EmulationVideoProcessingCatalog.cs:82`, `EmulationResourceKeys.cs:54`, les cinq clés dans les
    30 ressources `Emulation.resx` via Argos (corrections françaises dans `fr-FR/Emulation.resx`),
    `EmulationVideoProcessingSettingsSection.cs:414`,
    `CpuProjectionVideoProcessingPasses.cs:5`, `CpuEmulationVideoProcessingPipeline.cs:475`,
    `OpenGlVideoSurface.cs:135`, `VeldridVideoSurface.cs:102`, puis les tests de configuration,
    interface et rendu, dont `EmulationVideoProcessingPipelineTests.cs:1973`.
  - Vérifications terminées avant cochage : ciblage normalisation/persistance/interface/localisation,
    flou/diffusion/texture/convergence et matrice quatre renderers `7/7`, classes complètes
    configuration/localisation/interface `18/18`, compilation GWGUI.App sans avertissement ni erreur
    et `git diff --check` sans erreur.
