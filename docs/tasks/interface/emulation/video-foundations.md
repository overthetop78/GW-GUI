# Filtres vidéo : décisions et socle commun

[Sommaire](../emulation-improvements.md) · [Règles communes](rules.md)

Suite : [ergonomie](video-settings.md) · [découplage hôte](video-host-separation.md) · [technologies et validation](video-technologies.md).

### Filtres vidéo

Il faut établir une liste large des filtres existants, sans la limiter aux machines Amiga et Atari actuellement prises en charge. Cette recherche devra également servir aux futures machines ajoutées à GW GUI.

La recherche doit déterminer :

- ce qui est déjà proposé par Libretro ou par les émulateurs utilisés ;
- ce qui peut être réutilisé ou reproduit dans GW GUI ;
- ce qui devra être développé directement dans l’application.

Les collections de shaders Libretro peuvent servir de références pour reproduire des effets similaires dans GW GUI. Avant de reprendre directement le code d’un shader, il faudra vérifier sa licence. La manière de l’intégrer ou de reproduire son effet reste à étudier.

Les effets évoqués comprennent notamment les filtres CRT, les scanlines, les rendus LCD et le moiré horizontal et vertical. La liste exacte doit être établie pendant la recherche.

Les choix de signal vidéo déjà fournis par les émulateurs, tels que RGB, composite, S-Video, RF, PAL ou NTSC selon le moteur, restent des options de l’émulateur. Ils ne doivent pas être recréés ni dupliqués comme filtres propres à GW GUI. La possibilité de proposer plus tard un effet visuel inspiré d’un type de signal ne sera étudiée que comme un effet distinct et explicitement sélectionné.

Références de recherche :

- [Documentation des shaders Libretro](https://docs.libretro.com/guides/shaders/)
- [Collection officielle Slang](https://github.com/libretro/slang-shaders)
- [Collection Common Shaders](https://github.com/libretro/common-shaders)

### Organisation des filtres

Les filtres sont développés une seule fois et sont communs aux machines compatibles. Le filtre sélectionné et ses réglages sont cependant enregistrés séparément dans la configuration de chaque machine et utilisés par chacune de ses instances.

L’onglet **Vidéo** de la configuration Amiga ou Atari doit proposer :

- des présélections avec des réglages de base déjà choisis ;
- la possibilité de modifier les réglages ;
- l’enregistrement immédiat de chaque changement dans la configuration de la machine ;
- l’application immédiate du changement à l’instance correspondante lorsqu’elle fonctionne ;
- l’utilisation des réglages enregistrés au prochain démarrage si aucune instance ne fonctionne.

Les filtres et leurs réglages ne doivent pas être ajoutés ailleurs dans l’interface.

L’interface doit permettre de choisir une fonctionnalité dans une liste, puis d’afficher son panneau de configuration. Chaque fonctionnalité peut y être activée ou désactivée et, lorsqu’elle est active, ses réglages deviennent accessibles.

Il n’est pas nécessaire de transformer toute la page en groupes rigides. Les compatibilités restent gérées par familles logiques :

- les effets compatibles peuvent être combinés, par exemple un rendu CRT avec des scanlines ;
- lorsqu’une fonctionnalité activée est incompatible avec une ou plusieurs fonctionnalités déjà actives, l’application propose d’abord de les désactiver ;
- après confirmation, l’activation de la nouvelle fonctionnalité désactive automatiquement les fonctionnalités incompatibles ;
- sans confirmation, la combinaison actuelle reste inchangée.

Les réglages généraux de l’image — luminosité, contraste, gamma, saturation et netteté — restent toujours visibles indépendamment de la fonctionnalité sélectionnée. Leur valeur neutre est 0. Les réglages autres que le gamma utilisent une plage de -10 à +10. La représentation et la plage du gamma restent à définir en fonction de l’implémentation retenue.

La recherche devra établir la liste des groupes ainsi que les effets compatibles ou incompatibles entre eux.

### Séparation des réglages internes et externes à l’émulateur

Dans les onglets **Vidéo** et **Audio**, les réglages doivent être séparés visuellement entre :

- les options propres à l’émulateur ;
- les traitements réalisés par GW GUI sans demander de changement à l’émulateur.

Les traitements réalisés par GW GUI doivent pouvoir être modifiés en direct. La modification en direct des options internes de l’émulateur sera étudiée plus tard et ne fait pas partie de la demande actuelle.

Les intitulés des deux cadres ne sont pas encore choisis. Les formulations **Réglages de l’émulateur**, **Traitement vidéo par GW GUI** et **Traitement audio par GW GUI** ne sont pas retenues comme libellés définitifs.

## Checklist détaillée — Point 7 : recherche et architecture des filtres vidéo

Ce point produit la recherche et les décisions d’architecture demandées. Il ne crée aucun filtre, shader, réglage de configuration ou contrôle d’interface avant validation du catalogue et de l’architecture.

### Décisions validées

- Les cinq réglages généraux restent toujours visibles : luminosité, contraste, gamma, saturation et netteté.
- Le gamma GW GUI utilise la plage -10 à +10, la valeur 0 est neutre et `exposant = 2^(-valeur / 10)` ; une valeur positive éclaircit les tons moyens et une valeur négative les assombrit.
- Snapshot contient la sortie finale des traitements GW GUI, avant tout futur bezel, cadre ou habillage externe ; la capture correspond ainsi à l’image visible dans la zone d’émulation sur les quatre renderers.
- L’échantillonnage utilise un sélecteur unique et une seule méthode active.
- La technologie d’affichage utilise un sélecteur unique : **Normal**, **CRT**, **Écran à pixels fixes**, **Plasma** ou **Écran vectoriel**. Le panneau de paramètres change avec ce choix.
- CRT utilise un sous-choix couleur ou monochrome vert, ambre, blanc ou gris, avec palettes prédéfinies et future teinte personnalisée. Scanlines et trame/moiré volontaire appartiennent uniquement au panneau CRT.
- Écran à pixels fixes utilise un sous-choix LCD, LCD rétroéclairé par LED ou OLED. Les réglages communs restent partagés ; un réglage propre n’apparaît que s’il produit une différence visible réelle.
- La rémanence et le temps de réponse des pixels appartiennent au panneau Écran à pixels fixes. Le désentrelacement reste un traitement indépendant ou une option interne du moteur.
- Plasma et Écran vectoriel seront réalisés après le premier socle, mais font partie de la cible complète.
- Les scalers, la restauration, les traitements temporels, VFD, matrices LED ou de points, segments, papier électronique, projection, simulations de signal et effets stylistiques restent dans le catalogue pour les étapes ultérieures.
- Toutes les technologies validées seront réalisées pas à pas ; aucune ne doit être retirée de la checklist parce qu’elle est planifiée plus tard.
- Les douze présélections initiales sont Normal, CRT Arcade couleur, CRT Téléviseur couleur, CRT Monochrome vert, ambre et blanc, LCD couleur, LCD monochrome, LCD rétroéclairé LED, OLED, Plasma et Écran vectoriel ; leurs identifiants et valeurs exactes sont définis dans docs/reference/emulation-video-filters.md.
- Les intensités vidéo utilisent `0..100` et les durées temporelles utilisent `0..1000 ms` ; une rémanence sans suffixe `ms` est une intensité.

- [x] Créer le document de recherche avant d’y inscrire des résultats
  - [x] Créer le fichier vide docs/reference/emulation-video-filters.md.
  - [x] Modifier docs/reference/emulation-video-filters.md pour décrire le périmètre, distinguer les filtres réalisés par GW GUI des options de signal fournies par les émulateurs et reprendre les questions encore ouvertes de la section Filtres vidéo.

- [x] Établir le catalogue depuis les sources de référence
  - [x] Modifier docs/reference/emulation-video-filters.md à partir de la documentation officielle Libretro et des catalogues officiels slang-shaders et common-shaders pour recenser les familles de filtres, notamment CRT, scanlines, LCD et moiré horizontal ou vertical, avec un lien vers chaque source consultée.
  - [x] Modifier docs/reference/emulation-video-filters.md pour inscrire, pour chaque filtre ou famille, son effet, ses réglages utiles, ses dépendances éventuelles, ses combinaisons connues et son statut de licence ; ne recopier aucun code de shader dont la licence n’autorise pas clairement l’usage retenu.
  - [x] Modifier docs/reference/emulation-video-filters.md après examen des moteurs Amiga et Atari utilisés par le projet pour séparer leurs options RGB, composite, S-Video, RF, PAL, NTSC ou équivalentes des effets propres à GW GUI.
  - [x] Modifier docs/reference/emulation-video-filters.md pour inclure les filtres utiles aux futures machines sans limiter le catalogue aux capacités Amiga et Atari actuelles.

- [x] Comparer le catalogue aux quatre surfaces de rendu actuelles
  - [x] Modifier docs/reference/emulation-video-filters.md après examen de src/GWGUI.App/Interfaces/Rendering/Emulation/IEmulationVideoSurface.cs, src/GWGUI.App/Presenters/Emulation/Machine/MachineVideoPresenter.cs, src/GWGUI.App/Rendering/Emulation/Surfaces/WpfVideoSurface.cs, src/GWGUI.App/Rendering/Emulation/Surfaces/OpenGlVideoSurface.cs et src/GWGUI.App/Rendering/Emulation/Surfaces/VeldridVideoSurface.cs pour décrire où les pixels sont disponibles et où un traitement commun peut être appliqué.
  - [x] Modifier docs/reference/emulation-video-filters.md pour comparer WPF, OpenGL, Direct3D 11 et Vulkan pour chaque famille de filtre, indiquer ce qui peut partager une définition et ce qui exige une implémentation de backend, sans choisir silencieusement de supprimer un renderer.
  - [x] Modifier docs/reference/emulation-video-filters.md pour décrire l’effet du traitement envisagé sur Snapshot, le rapport d’aspect, le redimensionnement, le repli actuel vers WPF et l’application immédiate à une instance ouverte.

- [x] Valider les groupes, compatibilités et réglages avant l’architecture définitive
  - [x] Modifier docs/reference/emulation-video-filters.md pour proposer, à partir du catalogue établi, les groupes logiques, les combinaisons compatibles et les incompatibilités nécessitant la confirmation décrite dans la demande.
  - [x] Modifier docs/reference/emulation-video-filters.md pour proposer les présélections et les réglages propres à chaque fonctionnalité, sans dupliquer luminosité, contraste, gamma, saturation et netteté.
  - [x] Modifier docs/reference/emulation-video-filters.md après validation pour remplacer les propositions par les technologies, sous-choix, compatibilités et familles ultérieures retenus.
  - [x] Modifier docs/tasks/interface/emulation/video-foundations.md pour inscrire toutes les décisions fonctionnelles déjà validées sans marquer comme validés le gamma, Snapshot ou les valeurs exactes des présélections.
  - [x] Déterminer la plage, la valeur neutre et la conversion du gamma, puis modifier docs/reference/emulation-video-filters.md et la section Filtres vidéo du présent document avec la décision exacte.
  - [x] Déterminer si Snapshot contient l’image traitée ou l’image brute, puis inscrire cette décision dans docs/reference/emulation-video-filters.md et docs/architecture/emulation.md.
  - [x] Déterminer le nom, le contenu et les valeurs exactes des présélections avant de créer leurs constantes ou leurs ressources.

- [x] Inscrire l’architecture validée sans commencer son implémentation
  - [x] Modifier docs/architecture/emulation.md pour décrire la séparation validée entre configuration commune, catalogue de filtres, chaîne de traitement et implémentations propres aux backends.
  - [x] Modifier docs/architecture/emulation.md pour décrire l’enregistrement par configuration de machine, l’application immédiate à la seule instance correspondante et l’utilisation au prochain démarrage lorsqu’aucune instance n’est ouverte.
  - [x] Modifier docs/architecture/emulation.md pour décrire l’emplacement unique des contrôles dans l’onglet Vidéo, la séparation visuelle avec les options internes de l’émulateur et le maintien permanent des cinq réglages généraux.
  - [x] Modifier docs/tasks/interface/emulation/video-foundations.md pour ajouter la checklist d’implémentation progressive donnant les fichiers et actions retenus ; ne créer ni contrat, ni shader, ni contrôle pendant cette seule tâche documentaire.

### Checklist d’implémentation progressive — socle validé, à exécuter tâche par tâche

Chaque dernière case ci-dessous est une modification atomique. Elle doit laisser le projet compilable, être vérifiée, puis être cochée avant la suivante. Les shaders Libretro restent uniquement des références tant que la licence du fichier exact et de toutes ses dépendances n’est pas inscrite dans docs/reference/emulation-video-filters.md.

- [x] Auditer la checklist avant de commencer le code
  - [x] Vérifier les dossiers de responsabilités, les projets, les surfaces, les configurations, les tests et le script de traduction réellement présents dans le dépôt.
  - [x] Ajouter les tâches atomiques oubliées pour les types communs, les fonctions, les présélections, Snapshot, Argos et leurs tests sans déplacer de responsabilité vers un mauvais projet.
  - [x] Relire la checklist corrigée, vérifier que chaque dépendance précède son utilisation et qu’aucun groupe parent n’est coché avant ses enfants.

- [x] Créer le modèle commun de configuration avant toute interface ou tout shader
  - [x] Créer les enums communs un par un
    - [x] Créer le fichier vide src/GWGUI.Emulation/Enums/EmulationVideoDisplayTechnology.cs.
    - [x] Modifier src/GWGUI.Emulation/Enums/EmulationVideoDisplayTechnology.cs pour déclarer uniquement Normal, Crt, FixedPixel, Plasma et Vector, sans texte visible ni valeur propre à un moteur.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Enums/EmulationVideoSampling.cs.
    - [x] Modifier src/GWGUI.Emulation/Enums/EmulationVideoSampling.cs pour déclarer uniquement Nearest, Bilinear, SharpBilinear et Bicubic, sans ajouter les scalers avancés dans ce sélecteur.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Enums/EmulationCrtColorMode.cs.
    - [x] Modifier src/GWGUI.Emulation/Enums/EmulationCrtColorMode.cs pour déclarer Color, Green, Amber, White, Gray et Custom sans texte visible.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Enums/EmulationFixedPixelTechnology.cs.
    - [x] Modifier src/GWGUI.Emulation/Enums/EmulationFixedPixelTechnology.cs pour déclarer Lcd, LedBacklitLcd et Oled sans dupliquer les réglages communs.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Enums/EmulationCrtMask.cs.
    - [x] Modifier src/GWGUI.Emulation/Enums/EmulationCrtMask.cs pour déclarer None, ApertureGrille, ShadowMask et SlotMask sans texte visible.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Enums/EmulationPatternOrientation.cs.
    - [x] Modifier src/GWGUI.Emulation/Enums/EmulationPatternOrientation.cs pour déclarer Horizontal et Vertical et le réutiliser pour scanlines et trame volontaire.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Enums/EmulationSubpixelLayout.cs.
    - [x] Modifier src/GWGUI.Emulation/Enums/EmulationSubpixelLayout.cs pour déclarer Monochrome, Rgb et Bgr sans dépendance graphique.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Enums/EmulationVideoPreset.cs.
    - [x] Modifier src/GWGUI.Emulation/Enums/EmulationVideoPreset.cs avec exactement Normal, CrtArcadeColor, CrtTelevisionColor, CrtGreen, CrtAmber, CrtWhite, LcdColor, LcdMonochrome, LedBacklitLcd, Oled, Plasma et Vector.
  - [x] Créer les constantes communes avant les contrats qui les consomment
    - [x] Créer le fichier vide src/GWGUI.Emulation/Constants/EmulationVideoProcessingLimits.cs.
    - [x] Modifier src/GWGUI.Emulation/Constants/EmulationVideoProcessingLimits.cs avec `-10..+10` pour les cinq réglages généraux, `0..100` pour les intensités et des bornes temporelles explicites en millisecondes, sans valeur de preset.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Constants/EmulationVideoProcessingDefaults.cs.
    - [x] Modifier src/GWGUI.Emulation/Constants/EmulationVideoProcessingDefaults.cs avec les seules valeurs neutres communes, sans enum, fonction, dictionnaire ni texte visible.
  - [x] Créer les contrats sérialisables un par un
    - [x] Créer le fichier vide src/GWGUI.Emulation/Contracts/EmulationImageAdjustments.cs.
    - [x] Modifier src/GWGUI.Emulation/Contracts/EmulationImageAdjustments.cs pour transporter les cinq valeurs générales avec leurs valeurs neutres validées, sans conversion graphique.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Contracts/EmulationCrtVideoConfiguration.cs.
    - [x] Modifier src/GWGUI.Emulation/Contracts/EmulationCrtVideoConfiguration.cs pour transporter couleur/palette, faisceau, masque, géométrie, scanlines et trame volontaire, sans shader ni ressource WPF.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Contracts/EmulationFixedPixelVideoConfiguration.cs.
    - [x] Modifier src/GWGUI.Emulation/Contracts/EmulationFixedPixelVideoConfiguration.cs pour transporter technologie, grille, sous-pixels et réponse temporelle, avec valeurs propres facultatives.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Contracts/EmulationPlasmaVideoConfiguration.cs.
    - [x] Modifier src/GWGUI.Emulation/Contracts/EmulationPlasmaVideoConfiguration.cs pour transporter uniquement les paramètres Plasma validés à cette étape.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Contracts/EmulationVectorVideoConfiguration.cs.
    - [x] Modifier src/GWGUI.Emulation/Contracts/EmulationVectorVideoConfiguration.cs pour transporter uniquement les paramètres de ligne, halo et persistance validés à cette étape.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Contracts/EmulationVideoProcessingConfiguration.cs.
    - [x] Modifier src/GWGUI.Emulation/Contracts/EmulationVideoProcessingConfiguration.cs pour agréger technologie, échantillonnage, réglages généraux et configurations propres, avec Normal et valeurs neutres par défaut.
  - [x] Créer les fonctions communes séparément des contrats et du catalogue
    - [x] Créer le fichier vide src/GWGUI.Emulation/Functions/EmulationImageAdjustmentFunctions.cs.
    - [x] Modifier src/GWGUI.Emulation/Functions/EmulationImageAdjustmentFunctions.cs pour borner les cinq réglages et convertir uniquement le gamma par `2^(-valeur / 10)`, sans traiter de pixels.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Functions/EmulationVideoProcessingConfigurationFunctions.cs.
    - [x] Modifier src/GWGUI.Emulation/Functions/EmulationVideoProcessingConfigurationFunctions.cs pour normaliser et valider une configuration sérialisée, créer des sous-configurations neutres manquantes et ne dépendre d’aucun renderer.
  - [x] Exposer et enregistrer le contrat commun sans casser les anciennes configurations
    - [x] Modifier src/GWGUI.Emulation/Interfaces/IEmulationConfiguration.cs pour exposer EmulationVideoProcessingConfiguration en plus de VideoRenderer.
    - [x] Modifier src/GWGUI.Emulation.Amiga/Contracts/AmigaMachineConfiguration.cs pour porter le nouveau contrat facultatif et produire les valeurs neutres lorsqu’il est absent d’un ancien JSON.
    - [x] Modifier src/GWGUI.Emulation.Atari/Contracts/AtariMachineConfiguration.cs avec exactement la même règle de compatibilité ascendante.
    - [x] Modifier src/GWGUI.Emulation.Amiga/Modules/AmigaEmulationModule.cs et src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs pour préserver la configuration vidéo commune dans ApplySettings sans transformer les options natives des moteurs.
    - [x] Modifier src/GWGUI.Emulation.Atari/Contracts/AtariConfigurationDocument.cs pour transporter facultativement EmulationVideoProcessingConfiguration en dernier, afin que le JSON du schéma courant dépourvu de ce membre reste lisible.
    - [x] Modifier src/GWGUI.Emulation.Amiga/Services/AmigaConfigurationStore.cs, src/GWGUI.Emulation.Atari/Services/AtariConfigurationStore.cs et src/GWGUI.Emulation.Atari/Functions/AtariConfigurationStoreFunctions.cs uniquement si leur sérialisation explicite exige le nouveau membre ; ne pas ajouter de migration lorsque la désérialisation facultative suffit.
    - [x] Créer le fichier vide tests/GWGUI.Tests/EmulationVideoConfigurationTests.cs.
    - [x] Modifier tests/GWGUI.Tests/EmulationVideoConfigurationTests.cs pour vérifier valeurs neutres, sérialisation aller-retour Amiga/Atari et lecture d’anciens documents sans propriété vidéo commune.
    - [x] Exécuter uniquement tests/GWGUI.Tests/EmulationVideoConfigurationTests.cs, puis compiler src/GWGUI.Emulation/GWGUI.Emulation.csproj, src/GWGUI.Emulation.Amiga/GWGUI.Emulation.Amiga.csproj et src/GWGUI.Emulation.Atari/GWGUI.Emulation.Atari.csproj avec --no-restore.

- [x] Construire l’interface commune et son enregistrement immédiat avant les effets
  - [x] Créer le catalogue et le panneau sans texte brut
    - [x] Créer le fichier vide src/GWGUI.Emulation/Dictionaries/EmulationVideoProcessingCatalog.cs.
    - [x] Modifier src/GWGUI.Emulation/Dictionaries/EmulationVideoProcessingCatalog.cs pour décrire technologies, sous-choix, paramètres, valeurs neutres, dépendances et compatibilités avec uniquement des identifiants et clés de ressources, puis associer les douze EmulationVideoPreset à des configurations complètes conformes au tableau validé.
    - [x] Créer le fichier vide src/GWGUI.App/Views/Controls/Emulation/Options/EmulationVideoProcessingSettingsSection.cs.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationVideoProcessingSettingsSection.cs pour créer les deux sélecteurs, le panneau conditionnel et les cinq réglages permanents, sans logique Amiga ou Atari.
    - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationVideoSettingsLayout.cs pour séparer visuellement les options natives du moteur et les traitements GW GUI, sans ajouter ces contrôles ailleurs.
    - [x] Modifier src/GWGUI.Emulation/Interfaces/IEmulationModule.cs pour appliquer une EmulationVideoProcessingConfiguration commune à une configuration immuable sans exposer son type spécialisé à App.
    - [x] Modifier src/GWGUI.Emulation.Amiga/Modules/AmigaEmulationModule.cs et src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs pour implémenter ApplyVideoProcessing en conservant toutes les autres valeurs de la configuration typée.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour utiliser le panneau commun dans l’onglet Vidéo de chaque module et transmettre ses changements au chemin d’enregistrement automatique existant.
  - [x] Ajouter et vérifier tous les textes avant la validation visuelle
    - [x] Modifier src/GWGUI.App/Constants/Localization/EmulationResourceKeys.cs pour ajouter une constante par libellé de technologie, paramètre, incompatibilité, limitation et présélection, sans texte traduit.
    - [x] Modifier scripts/translate-resx-argos.py pour écrire le catalogue neutre dans src/GWGUI.App/Resources/00-Base tout en conservant la commande documentée et les catalogues de culture existants.
    - [x] Ajouter dans src/GWGUI.App/Resources/00-Base/Emulation.resx toutes les clés neutres des technologies, paramètres, incompatibilités, présélections validées et limitations de backend.
    - [x] Pour chaque nouvelle clé, exécuter `python scripts/translate-resx-argos.py Emulation.resx <clé> "<texte anglais>"` afin d’ajouter la base, en-US et toutes les traductions ; ne pas remplir manuellement les catalogues à la place d’Argos.
    - [x] Relire les sorties Argos dans tous les catalogues src/GWGUI.App/Resources/*/Emulation.resx, corriger seulement les traductions manifestement erronées et vérifier qu’aucun texte brut n’est ajouté au code.
    - [x] Créer le fichier vide tests/GWGUI.Tests/EmulationVideoLocalizationTests.cs.
    - [x] Modifier tests/GWGUI.Tests/EmulationVideoLocalizationTests.cs pour vérifier la présence de chaque clé dans toutes les langues et le remplacement immédiat des textes du panneau lors d’un changement de langue.
  - [x] Appliquer les changements à la seule instance correspondante
    - [x] Modifier src/GWGUI.App/Interfaces/Rendering/Emulation/IEmulationVideoSurface.cs pour recevoir une EmulationVideoProcessingConfiguration normalisée, sans encore modifier les pixels.
    - [x] Modifier src/GWGUI.App/Rendering/Emulation/Surfaces/WpfVideoSurface.cs, src/GWGUI.App/Rendering/Emulation/Surfaces/OpenGlVideoSurface.cs et src/GWGUI.App/Rendering/Emulation/Surfaces/VeldridVideoSurface.cs pour conserver la configuration reçue sans changer le rendu actuel.
    - [x] Modifier src/GWGUI.App/Presenters/Emulation/Machine/MachineVideoPresenter.cs pour conserver la configuration courante et la réappliquer lors d’un changement ou d’un repli de surface.
    - [x] Modifier src/GWGUI.App/Contracts/Machine/MachineControllerOptions.cs et src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationSectionMachineFunctions.cs pour transmettre la configuration vidéo initiale enregistrée avec VideoRenderer lors de la création du controller.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs pour transmettre la nouvelle configuration à MachineVideoPresenter sans recréer IEmulatedMachine.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationSectionConfigurationFunctions.cs pour transmettre la configuration vidéo commune avec VideoRenderer à l’unique MachineController ciblé par ModuleId et ConfigurationId.
    - [x] Créer le fichier vide tests/GWGUI.Tests/EmulationVideoSettingsSectionTests.cs.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationVideoProcessingSettingsSection.cs pour confirmer un remplacement direct entre deux technologies simulées incompatibles, sans avertissement pour activer ou désactiver Normal, avec une fonction de confirmation injectable pour les tests.
    - [x] Créer puis modifier src/GWGUI.App/Functions/Views/Emulation/Machine/EmulationOpenMachineConfigurationFunctions.cs et raccorder EmulationSectionConfigurationFunctions.cs afin d’exposer une sélection générique testable par ModuleId et ConfigurationId sans construire de machine réelle.
    - [x] Créer puis modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationConfigurationPersistenceFunctions.cs et raccorder EmulationModuleSettingsSection.cs afin de rendre testable la décision brouillon ou autosauvegarde sans charger la fenêtre d’options.
    - [x] Modifier tests/GWGUI.Tests/EmulationVideoSettingsSectionTests.cs pour vérifier affichage conditionnel, cinq réglages permanents, brouillons, enregistrement automatique, confirmation des incompatibilités et ciblage d’une seule instance.
    - [x] Exécuter uniquement tests/GWGUI.Tests/EmulationVideoSettingsSectionTests.cs, puis compiler src/GWGUI.App/GWGUI.App.csproj avec --no-restore.
