# Technologies vidéo et validation

[Sommaire](../emulation-improvements.md) · [Règles communes](rules.md)

- [x] Préserver la configuration vidéo dans toutes les reconstructions Atari
  - [x] Modifier src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs, src/GWGUI.Emulation.Atari/Functions/AtariInputSettingsFunctions.cs, src/GWGUI.Emulation.Atari/Functions/AtariStorageSettingsFunctions.cs et src/GWGUI.Emulation.Atari/Services/AtariMachine.cs pour conserver VideoProcessing dans chaque reconstruction non vidéo.
  - [x] Modifier tests/GWGUI.Tests/EmulationVideoConfigurationTests.cs pour vérifier qu’une reconstruction Atari non vidéo conserve une configuration vidéo non neutre.
  - [x] Exécuter uniquement tests/GWGUI.Tests/EmulationVideoConfigurationTests.cs, puis compiler src/GWGUI.Emulation.Atari/GWGUI.Emulation.Atari.csproj avec --no-restore.

- [x] Créer la chaîne de traitement sans effet avant d’implémenter Normal
  - [x] Créer le fichier vide src/GWGUI.App/Interfaces/Rendering/Emulation/IEmulationVideoProcessingPipeline.cs.
  - [x] Modifier src/GWGUI.App/Interfaces/Rendering/Emulation/IEmulationVideoProcessingPipeline.cs pour recevoir configuration, frame, tailles source/sortie et produire la sortie du backend sans dépendre d’une famille de machine.
  - [x] Créer le fichier vide src/GWGUI.App/Factories/Rendering/Emulation/EmulationVideoProcessingPipelineFactory.cs.
  - [x] Créer puis modifier src/GWGUI.App/Rendering/Emulation/Processing/PassthroughEmulationVideoProcessingPipeline.cs pour implémenter le contrat avec le renderer demandé, valider les tailles positives et retourner exactement la frame reçue.
  - [x] Modifier src/GWGUI.App/Factories/Rendering/Emulation/EmulationVideoProcessingPipelineFactory.cs pour choisir uniquement l’exécuteur correspondant au renderer déjà sélectionné.
  - [x] Modifier src/GWGUI.App/Interfaces/Rendering/Emulation/IEmulationVideoSurface.cs pour préciser que Snapshot expose la sortie traitée avant tout habillage externe, conformément à la décision validée, et raccorder le contrat de configuration déjà présent à la chaîne.
  - [x] Modifier src/GWGUI.App/Rendering/Emulation/Surfaces/WpfVideoSurface.cs, src/GWGUI.App/Rendering/Emulation/Surfaces/OpenGlVideoSurface.cs et src/GWGUI.App/Rendering/Emulation/Surfaces/VeldridVideoSurface.cs pour raccorder une chaîne vide qui reproduit exactement le rendu actuel.
  - [x] Créer le fichier vide tests/GWGUI.Tests/EmulationVideoProcessingPipelineTests.cs.
  - [x] Modifier tests/GWGUI.Tests/EmulationVideoProcessingPipelineTests.cs pour vérifier qu’une chaîne Normal neutre préserve pixels, rapport d’aspect, redimensionnement et repli WPF.
  - [x] Créer puis modifier src/GWGUI.App/Functions/Rendering/Emulation/EmulationVideoSurfaceFrameFunctions.cs et raccorder les trois surfaces afin que l’affichage et Snapshot consomment ensemble la frame traitée et sa conversion BGRA commune.
  - [x] Modifier src/GWGUI.App/Rendering/Emulation/Surfaces/WpfVideoSurface.cs pour accepter facultativement un IEmulationVideoProcessingPipeline interne dans les tests tout en conservant la fabrique WPF neutre par défaut.
  - [x] Ajouter dans tests/GWGUI.Tests/EmulationVideoProcessingPipelineTests.cs un test vérifiant que Snapshot reçoit la sortie après traitements GW GUI et avant tout habillage, sur le chemin commun utilisé par WPF, OpenGL et Veldrid.
  - [x] Exécuter uniquement tests/GWGUI.Tests/EmulationVideoProcessingPipelineTests.cs, puis compiler src/GWGUI.App/GWGUI.App.csproj avec --no-restore.

- [x] Implémenter le premier socle Normal sur les quatre renderers
  - [x] Modifier docs/reference/emulation-video-filters.md pour fixer l’ordre et les conversions CPU exactes de luminosité, contraste, gamma, saturation et netteté, avec 0 strictement neutre.
  - [x] Créer l’exécution CPU de référence pour luminosité, contraste, gamma validé, saturation et netteté, avec conversions sRGB/linéaire testées.
  - [x] Ajouter le sélecteur nearest, bilinéaire, sharp-bilinear et bicubique dans l’exécution CPU/WPF avec comportement stable aux échelles non entières.
  - [x] Remplacer dans src/GWGUI.App/Rendering/Emulation/Surfaces/VeldridVideoSurface.cs le shader de copie fixe par la première chaîne portable commune Direct3D 11/Vulkan et ses buffers de paramètres.
  - [x] Remplacer dans src/GWGUI.App/Rendering/Emulation/Surfaces/OpenGlVideoSurface.cs `glDrawPixels` par texture, quad et programme GLSL capables d’exécuter les mêmes réglages et méthodes d’échantillonnage.
  - [x] Ajouter aux tests de pipeline des images déterministes couvrant chaque valeur neutre, les bornes validées et l’équivalence tolérée entre CPU, Direct3D 11, Vulkan et OpenGL.
  - [x] Exécuter les tests ciblés, compiler GWGUI.App sans restauration, puis vérifier Normal dans l’application avec les quatre renderers avant de cocher le groupe.

- [x] Implémenter CRT après validation de ses paramètres exacts et des licences
  - [x] Implémenter couleur et monochrome vert, ambre, blanc, gris et personnalisé dans la définition commune et l’exécution CPU de référence.
  - [x] Implémenter faisceau, masque, halo, courbure et vignettage dans des passes composables sans reprendre de code dont la licence n’est pas validée.
  - [x] Implémenter les scanlines horizontales et verticales uniquement dans le panneau CRT avec intensité, épaisseur, phase et compensation.
  - [x] Implémenter la trame volontaire horizontale et verticale uniquement dans CRT, séparément de la réduction du moiré accidentel.
  - [x] Porter les mêmes passes vers Veldrid et OpenGL, avec repli CPU/WPF et construction atomique en cas d’erreur.
  - [x] Ajouter des tests déterministes pour chaque sous-choix CRT, compatibilité, valeur neutre, redimensionnement et changement en direct.
  - [x] Vérifier visuellement chaque présélection CRT validée sur Amiga et Atari avec WPF, OpenGL, Direct3D 11 et Vulkan avant de cocher le groupe.

- [x] Implémenter les écrans à pixels fixes après CRT
  - [x] Implémenter le panneau partagé et le sous-choix LCD, LCD/LED ou OLED sans panneaux principaux dupliqués.
  - [x] Implémenter grille, sous-pixels, ordre des couleurs, netteté et paramètres communs dans l’exécution CPU de référence.
  - [x] Ajouter uniquement les paramètres conditionnels dont une différence LCD/LED/OLED a été documentée et validée.
  - [x] Implémenter rémanence et temps de réponse avec historique borné, sans les présenter comme désentrelacement.
  - [x] Porter les passes vers Veldrid et OpenGL, ajouter les tests déterministes et vérifier les quatre renderers avant de cocher le groupe.
  - [x] Reprendre le modèle après la validation visuelle signalant des contrôles sans effet.
    - [x] Extraire l’interface dans EmulationFixedPixelSettingsBlock.cs et regrouper type d’écran, structure des pixels, lumière/contraste et réponse temporelle dans quatre cartes qui restent dans la largeur disponible.
    - [x] Remplacer la saisie ARGB du monochrome par la palette commune vert, gris, ambre, bleu et blanc, en réutilisant les traductions existantes.
    - [x] Séparer grille, sous-pixels, lumière commune, LCD, LCD rétroéclairé LED, OLED, temps de réponse et persistance dans des fichiers Filter… propres à leur responsabilité.
    - [x] Calculer grille et sous-pixels dans les coordonnées de la frame fournie par l’émulateur, indépendamment de la définition physique de l’écran.
    - [x] Donner à LCD, LCD/LED et OLED des courbes différentes de rétroéclairage, plancher noir, halo local et contraste, sans afficher de rétroéclairage pour OLED.
    - [x] Raccorder temps de réponse et persistance au chemin Vulkan/Direct3D 11, où leurs paramètres étaient transmis mais non consommés.
    - [x] Regrouper les changements rapides des contrôles vidéo avant autosauvegarde et sérialiser la lecture/remplacement de machine.json entre instances.
    - [x] Ajouter les tests de distinction des technologies, palette monochrome, disposition conditionnelle, compilation SPIR-V, OpenGL, Direct3D 11 et concurrence de sauvegarde.

- [x] Implémenter Plasma après les écrans à pixels fixes
  - [x] Faire valider les paramètres exacts de cellules, diffusion, tramage temporel et rémanence, puis les inscrire dans docs/reference/emulation-video-filters.md.
  - [x] Compléter EmulationPlasmaVideoConfiguration, le catalogue, les ressources et le panneau conditionnel sans modifier les autres technologies.
  - [x] Implémenter la référence CPU, puis les passes Veldrid et OpenGL avec les mêmes valeurs.
  - [x] Ajouter les tests déterministes et vérifier Plasma avec les quatre renderers avant de cocher le groupe.
  - [x] Reprendre le modèle après la validation visuelle des quatre contrôles Plasma.
    - [x] Isoler le bloc d’interface dans EmulationPlasmaSettingsBlock.cs, supprimer l’agrégateur
      FilterPlasma.cs et placer chaque traitement dans son propre fichier : structure des cellules,
      profondeur des noirs, intensité des phosphores, réponse gamma, tramage temporel, diffusion
      lumineuse, persistance et limiteur automatique de luminosité fondé sur la luminance moyenne
      de l’image complète.
    - [x] Présenter les paramètres dans quatre cartes fonctionnelles sans cadre Plasma redondant :
      dalle et cellules, phosphores, gestion de la lumière et réponse temporelle.
    - [x] Supprimer les bandes RGB non résolubles en intégrant le motif selon le rapport entre la
      définition de la vidéo émulée et la taille de sortie.
    - [x] Remplacer le bruit du tramage temporel par une quantification Bayer animée et bornée.
    - [x] Rendre la diffusion lumineuse visible et dépendante des hautes lumières, à deux rayons
      exprimés dans les coordonnées de la vidéo émulée.
    - [x] Raccorder diffusion et persistance au shader Veldrid utilisé par Direct3D 11 et Vulkan,
      puis rendre la persistance décroissante afin que la valeur maximale ne fige pas l’image.

- [x] Implémenter l’écran vectoriel après Plasma
  - [x] Faire valider l’approximation raster, les paramètres de lignes, halo et persistance, puis les inscrire dans docs/reference/emulation-video-filters.md.
  - [x] Compléter EmulationVectorVideoConfiguration, le catalogue, les ressources et le panneau conditionnel sans prétendre recevoir des primitives vectorielles du moteur.
  - [x] Implémenter la détection/renforcement de lignes et la persistance dans la référence CPU, puis dans Veldrid et OpenGL.
  - [x] Ajouter les tests déterministes et vérifier l’écran vectoriel avec les quatre renderers avant de cocher le groupe.
  - [x] Séparer le traitement en FilterVectorLineDetection.cs,
    FilterVectorLineIntensity.cs, FilterVectorHalo.cs et FilterVectorPersistence.cs, puis présenter
    les réglages dans deux cartes sans cadre vectoriel redondant : détection et tracé, lueur et
    rémanence.
  - [x] Compléter le modèle vectoriel avec la largeur/focalisation du faisceau, le
    mode de phosphore (couleurs de la source, vert, ambre, blanc ou gris) et le rayon du halo. Ces
    paramètres doivent rester indépendants de la résolution physique de l’écran et s’appliquer à
    l’approximation raster issue de la vidéo émulée.

- [x] Ajouter les filtres indépendants avancés un groupe à la fois
  - [x] Faire valider xBR, inscrire sa source et sa licence dans docs/reference/emulation-video-filters.md, puis l’implémenter et le vérifier seul dans les quatre renderers.
  - [x] Faire valider xBRZ, inscrire sa source et sa licence dans docs/reference/emulation-video-filters.md, puis l’implémenter et le vérifier seul dans les quatre renderers.
  - [x] Faire valider HQx, inscrire sa source et sa licence dans docs/reference/emulation-video-filters.md, puis l’implémenter et le vérifier seul dans les quatre renderers.
  - [x] Faire valider ScaleFX, inscrire sa source et sa licence dans docs/reference/emulation-video-filters.md, puis l’implémenter et le vérifier seul dans les quatre renderers.
  - [x] Faire valider ScaleNx, inscrire sa source et sa licence dans docs/reference/emulation-video-filters.md, puis l’implémenter et le vérifier seul dans les quatre renderers.
  - [x] Faire valider SABR, inscrire sa source et sa licence dans docs/reference/emulation-video-filters.md, puis l’implémenter et le vérifier seul dans les quatre renderers.
  - [x] Faire valider le dé-dithering, vérifier qu’il ne duplique aucun traitement moteur, puis l’implémenter et le tester seul.
  - [x] Faire valider le débruitage, vérifier qu’il ne duplique aucun traitement moteur, puis l’implémenter et le tester seul.
  - [x] Faire valider la réduction des bandes, vérifier qu’elle ne duplique aucun traitement moteur, puis l’implémenter et la tester seule.
  - [x] Faire valider la netteté avancée sans dupliquer le réglage général, puis l’implémenter et la tester seule.
  - [x] Faire valider le désentrelacement GW GUI, vérifier qu’il ne duplique aucun traitement moteur, puis l’implémenter et le tester seul.
  - [x] Faire confirmer toute incompatibilité entre filtres indépendants avant d’ajouter sa boîte Oui/Non et vérifier que Non ne modifie aucune valeur.
  - [x] Ajouter les tests et validations visuelles propres à chaque filtre avant de cocher sa tâche individuelle.

- [x] Traiter le catalogue ultérieur sans regrouper plusieurs effets dans une seule modification
  - [x] Refaire VFD avec seuil d’émission, verre fumé, structure graphique/matrice, taille et
    espacement des cellules, halo avec rayon et persistance exprimée en millisecondes ; séparer
    chaque fonction dans son propre fichier, organiser l’interface en trois cartes et raccorder les
    paramètres aux quatre renderers dans les coordonnées de la vidéo émulée.
  - [x] Refaire la matrice LED dans les coordonnées de la vidéo émulée : cellules régulières rondes
    ou carrées, couleur réellement appliquée, luminosité pouvant atteindre l’extinction, espacement,
    halo (intensité et rayon) et profondeur des noirs indépendants ; séparer chaque fonction dans son
    propre fichier, organiser l’interface en deux cartes et compiler ce code uniquement lorsque la
    technologie Matrice LED est sélectionnée.
  - [x] Préparer puis implémenter la matrice de points lorsqu’une machine prise en charge le nécessite.
  - [x] Préparer puis implémenter les affichages à segments lorsqu’une machine prise en charge le nécessite.
    - [x] Compléter les traitements séparés FilterSegmentDisplay… et EmulationSegmentDisplaySettingsBlock.cs : regrouper cellules, géométrie, émission et réponse lumineuse dans quatre cartes ; couvrir les options par les tests de configuration, d’interface et de pipeline. Refonte enregistrée dans le commit e59f3fb5.
  - [x] Préparer puis implémenter le papier électronique après validation de son utilité et de ses paramètres.
    - [x] Compléter les traitements séparés FilterEPaper… et EmulationEPaperSettingsBlock.cs : organiser encre/couleur, surface du papier et rafraîchissement dans trois cartes ; ajouter densité d’encre, luminosité et teinte du papier, saturation, texture et adoucissement des contours, avec traductions et tests. Refonte enregistrée dans le commit e59f3fb5.
  - [x] Préparer puis implémenter la projection après validation de son utilité et de ses paramètres.
    - [x] Séparer les sept traitements dans FilterProjectionOpticalBlur.cs, FilterProjectionDiffusion.cs, FilterProjectionConvergence.cs, FilterProjectionLightOutput.cs, FilterProjectionAmbientLight.cs, FilterProjectionVignette.cs et FilterProjectionScreenTexture.cs ; composer leurs shaders dans ProjectionVideoShader.cs et leurs contrôles dans EmulationProjectionSettingsBlock.cs, en deux cartes « Optique et lumière » et « Écran de projection ». Corriger la convergence GPU et rendre la toile fixe ; ajouter puissance lumineuse, lumière ambiante et assombrissement des bords. Traductions, tests des pixels GPU sur OpenGL/Direct3D 11/Vulkan et build Debug vérifiés ; commit 619a6dfc.
  - [x] Préparer puis implémenter la rémanence générale sans la confondre avec la réponse des écrans à pixels fixes.
  - [x] Préparer puis implémenter le flou de mouvement.
  - [x] Préparer puis implémenter le scintillement.
  - [x] Préparer puis implémenter l’entrelacement.
  - [x] Préparer puis implémenter l’insertion d’images noires.
  - [x] Préparer puis implémenter la simulation composite comme effet explicitement nommé, uniquement si elle ne duplique pas une option du moteur ciblé.
  - [x] Préparer puis implémenter la simulation S-Video comme effet explicitement nommé, uniquement si elle ne duplique pas une option du moteur ciblé.
  - [x] Préparer puis implémenter la simulation RF comme effet explicitement nommé, uniquement si elle ne duplique pas une option du moteur ciblé.
  - [x] Préparer puis implémenter la simulation PAL comme effet explicitement nommé, uniquement si elle ne duplique pas une option du moteur ciblé.
  - [x] Préparer puis implémenter la simulation NTSC comme effet explicitement nommé, uniquement si elle ne duplique pas une option du moteur ciblé.
  - [x] Préparer puis implémenter le grain.
  - [x] Préparer puis implémenter l’effet VHS.
  - [x] Préparer puis implémenter l’aberration chromatique.
  - [x] Préparer puis implémenter le bloom.
  - [x] Préparer puis implémenter le sépia.
  - [x] Préparer puis implémenter les niveaux de gris.
  - [x] Pour chaque tâche, inscrire d’abord sources, licence, paramètres, compatibilités et tests dans docs/reference/emulation-video-filters.md, puis vérifier les quatre renderers avant de la cocher.

- [x] Terminer la validation globale du point 7 après toutes les étapes retenues
  - [x] Exécuter tous les tests vidéo ciblés et corriger uniquement les régressions du point 7.
  - [x] Exécuter toute la suite tests/GWGUI.Tests/GWGUI.Tests.csproj et corriger uniquement les régressions du point 7.
  - [x] Exécuter scripts/build.ps1 -Configuration Debug et vérifier qu’aucun avertissement nouveau n’est introduit.
  - [x] Dans une seule exécution réelle, vérifier Amiga et Atari, l’enregistrement par machine, l’application à la seule instance ouverte, le prochain démarrage sans instance, les quatre renderers, le redimensionnement, le plein écran et le repli WPF.
  - [x] Dans la même exécution, vérifier que les options natives des moteurs restent séparées, que les cinq réglages généraux restent visibles et que les panneaux conditionnels n’apparaissent que pour la technologie choisie.
  - [x] Fermer l’instance utilisée pour la validation et vérifier les journaux d’erreurs avant de cocher le groupe.
