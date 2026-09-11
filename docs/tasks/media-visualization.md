# Affichage graphique des images de médias

Cette feuille contient la base visible des vues Flux, Sectors, Blocks, OpticalTracks et Sequential.
Elle commence après le premier commit du socle MediaEngine et se termine par le deuxième commit
demandé. Les tâches sont exécutées et cochées une par une, dans l’ordre.

- [x] 1. Finaliser l’affichage graphique de tous les supports

  Le premier commit demandé a été créé après l’achèvement du socle commun. Les actions graphiques
  commencent maintenant.

  - [x] 1.1 Construire la présentation commune du Visualiseur
    - [x] Modifier `docs/project/media-support-planning.md` pour définir les choix graphiques à appliquer aux vues Flux, Sectors, Blocks, OpticalTracks et Sequential, en distinguant les informations physiques, logiques et inconnues réellement fournies par chaque représentation.
    - [x] Créer `src/GWGUI.App/Enums/ViewModels/Visualization/MediaInspectorEntryLevel.cs` avec les niveaux `Information`, `Warning` et `Error` utilisés par les entrées du panneau commun.
    - [x] Créer `src/GWGUI.App/Contracts/ViewModels/Visualization/MediaInspectorEntry.cs` avec le libellé, la valeur, l’unité éventuelle et le niveau d’information ou d’erreur.
    - [x] Créer `src/GWGUI.App/Contracts/ViewModels/Visualization/MediaInspectorSection.cs` avec un titre, une icône et une liste ordonnée de valeurs affichables.
    - [x] Créer `src/GWGUI.App/Contracts/ViewModels/Visualization/MediaInspectorModel.cs` avec le titre du support, ses sections d’informations, l’élément sélectionné et uniquement les propriétés fournies par le document média.
    - [x] Créer `src/GWGUI.App/Views/Controls/Visualization/MediaInspectorPanel.xaml` avec des cartes de synthèse et de détails réutilisables par tous les supports.
    - [x] Créer `src/GWGUI.App/Views/Controls/Visualization/MediaInspectorPanel.xaml.cs` pour recevoir un `MediaInspectorModel` sans connaître le format du fichier ouvert.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerTabSection.xaml` pour organiser une zone de rendu principale, une barre d’outils courte, une légende, une progression et le panneau d’informations adaptable à la largeur disponible.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerTabSection.xaml.cs` pour sélectionner automatiquement la vue correspondant à Flux, Sectors, Blocks, OpticalTracks ou Sequential et conserver zoom, sélection et position entre les actualisations du même document.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerTabSection.xaml.cs` pour conserver visible la première vue enregistrée avant le premier appel à `ShowDocument`, afin de préserver l’affichage Flux existant pendant la transition.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerTabSection.xaml.cs` pour recevoir et mémoriser ensemble le `MediaImageDocument` et son `MediaVisualizationDescriptor`, puis différer l’activation lorsqu’une vue de représentation n’est pas encore enregistrée.
    - [x] Modifier `src/GWGUI.App/Services/DiskImages/DiskImageWorkspaceController.cs` pour transmettre le document et son descripteur à la vue sélectionnée sans décider du rendu d’après l’extension.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerHeaderSection.xaml` pour afficher le nom, le format, le support, la représentation et uniquement les sélecteurs utiles à la vue active.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerHeaderSection.xaml.cs` pour alimenter ces informations depuis le document et masquer les commandes incompatibles avec sa représentation.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerTabSection.xaml.cs` pour transmettre chaque document et descripteur reçus à `VisualizerHeaderSection`.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerLegend.xaml` pour afficher une légende compacte composée d’éléments colorés et expliqués, propre à la représentation active.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerLegend.xaml.cs` pour construire la légende depuis le descripteur sans liste codée par extension.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerTabSection.xaml.cs` pour transmettre le descripteur actif à `VisualizerLegend` lors de chaque document affiché.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/TrackProgressStrip.xaml` pour représenter une progression générique par pistes, secteurs, plages, pistes optiques ou segments.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/TrackProgressStrip.xaml.cs` pour recevoir l’unité, le total, l’avancement et l’état sans dépendre de `SkiaScpRenderer`.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerTrackOverview.xaml` pour afficher dynamiquement les surfaces, plateaux, couches, faces, canaux ou lignes annoncés par le descripteur.
    - [x] Modifier `src/GWGUI.App/ViewModels/Visualization/TrackSegment.cs` pour identifier chaque élément par une position `long` et une surface génériques au lieu des propriétés limitées aux cylindres et têtes de disquette.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/TrackProgressStrip.xaml` pour rendre chaque segment de progression sélectionnable sans modifier son rendu compact.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/TrackProgressStrip.xaml.cs` pour accepter les positions `long`, exposer la sélection d’un élément et conserver les appels existants de progression des pistes.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerTrackOverview.xaml.cs` pour générer ces lignes de progression et synchroniser leur sélection avec la vue principale.
    - [x] Créer `src/GWGUI.App/Contracts/Views/Visualization/IMediaVisualizationView.cs` avec l’événement et la méthode de sélection communs reliant une vue de support à l’aperçu de progression.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerTabSection.xaml.cs` pour configurer l’aperçu depuis le descripteur de chaque document et transmettre les sélections entre l’aperçu et la vue active.

  - [x] 1.2 Finaliser la vue d’une disquette en flux
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/ScpDiskView.xaml` pour présenter séparément les faces disponibles, la surface circulaire du flux, le zoom et la sélection d’une piste ou d’une révolution.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/ScpDiskView.xaml.cs` pour conserver la face sélectionnée, gérer le zoom et la révolution choisie, relier le pointeur aux pistes préparées et exposer la sélection au panneau commun sans construire lui-même les informations affichées.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerTabSection.xaml.cs` pour relier les deux vues Flux existantes à l’aperçu commun malgré leur hébergement dans une même grille de représentation.
    - [x] Modifier `src/GWGUI.App/Contracts/Rendering/Scp/ScpRenderRequest.cs` pour transmettre au renderer la révolution sélectionnée sur la piste active.
    - [x] Modifier `src/GWGUI.App/Rendering/Scp/PreparedScpTrack.cs` pour conserver séparément les arcs de flux de chaque révolution réellement capturée.
    - [x] Modifier `src/GWGUI.App/Rendering/Scp/ScpTrackPreparationFunctions.cs` pour préparer toutes les révolutions d’une piste dans leur ordre et conserver séparément le meilleur décodage disponible.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/ScpDiskView.xaml.cs` pour inclure l’index de révolution choisi dans chaque requête de rendu.
    - [x] Modifier `src/GWGUI.App/Rendering/Scp/SkiaScpRenderer.cs` pour préparer progressivement chaque piste et dessiner uniquement les transitions, révolutions, densités et anomalies réellement présentes dans la capture.
    - [x] Modifier `src/GWGUI.App/Rendering/Scp/ScpTrackDrawingFunctions.cs` pour appliquer une palette lisible sur thèmes clair et sombre sans confondre absence de décodage, zone vide et erreur physique.
    - [x] Modifier `src/GWGUI.App/Presenters/Visualization/ScpInspectorPresenter.cs` pour produire le modèle commun avec face, piste, révolution, durées, transitions, encodage détecté et structures décodées disponibles.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerTabSection.xaml.cs` pour exposer l’affichage et le masquage du modèle d’inspection commun sans laisser le controller manipuler la mise en page.
    - [x] Modifier `src/GWGUI.App/Services/Visualization/ScpInspectorController.cs` pour alimenter `MediaInspectorPanel` dans la vue principale et dans la fenêtre détachée.
    - [x] Modifier `src/GWGUI.App/Views/Windows/Visualization/ScpInspectorWindow.xaml` pour héberger `MediaInspectorPanel` avec le modèle de la sélection Flux.
    - [x] Modifier `src/GWGUI.App/Views/Windows/Visualization/ScpInspectorWindow.xaml.cs` pour recevoir et actualiser le modèle commun de la sélection Flux.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerTabSection.xaml` pour retirer la couche flottante qui hébergeait l’ancien panneau SCP.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerTabSection.xaml.cs` pour supprimer les accès publics devenus inutiles à l’ancien panneau SCP et à son canevas.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerTabSection.xaml` pour ajouter au panneau commun une commande courte permettant de détacher les informations affichées.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerTabSection.xaml.cs` pour exposer la demande de détachement du panneau d’informations commun.
    - [x] Modifier `src/GWGUI.App/Services/Visualization/ScpInspectorController.cs` pour raccorder le détachement commun, supprimer la gestion de déplacement et retirer toutes les dernières écritures vers l’ancien panneau SCP.
    - [x] Supprimer `src/GWGUI.App/Views/Controls/Visualization/ScpInspectorPanel.xaml` après migration de la vue principale et de la fenêtre détachée vers `MediaInspectorPanel`.
    - [x] Supprimer `src/GWGUI.App/Views/Controls/Visualization/ScpInspectorPanel.xaml.cs` après suppression du contrôle XAML correspondant.

  - [x] 1.3 Finaliser la vue d’une disquette sectorielle
    - [x] Créer `src/GWGUI.App/Enums/Rendering/Sectors/SectorMediaElementState.cs` avec les états `Available`, `Missing`, `IntegrityUnknown` et `IntegrityInvalid` fondés uniquement sur les données de l’image.
    - [x] Créer `src/GWGUI.App/Contracts/Rendering/Sectors/SectorMediaRenderModel.cs` avec les faces, pistes, secteurs, tailles, identifiants, positions, états connus et correspondance avec les fichiers lorsque l’Explorateur la fournit.
    - [x] Créer `src/GWGUI.App/Constants/Rendering/Sectors/SectorMediaRenderConstants.cs` avec les limites géométriques et la palette propres au rendu sectoriel.
    - [x] Créer `src/GWGUI.App/Rendering/Sectors/SkiaSectorMediaRenderer.cs` pour dessiner les faces comme des surfaces de pistes concentriques divisées en secteurs, avec l’ordre et la direction fournis par le descripteur.
    - [x] Créer `src/GWGUI.App/Views/Controls/Visualization/SectorMediaView.xaml` avec les faces disponibles, la carte sectorielle, le zoom et la sélection, sans commandes propres aux révolutions du flux.
    - [x] Créer `src/GWGUI.App/Views/Controls/Visualization/SectorMediaView.xaml.cs` pour recevoir le modèle sectoriel, gérer la face et le zoom, relier le pointeur à un secteur et exposer sa sélection au presenter sans construire lui-même les informations du panneau commun.
    - [x] Créer `src/GWGUI.App/Presenters/Visualization/SectorMediaInspectorPresenter.cs` pour construire le modèle de rendu depuis `SectorImage` et présenter les informations de face, piste et secteur sans déduire de défaut physique depuis une image de données.
    - [x] Modifier `src/GWGUI.App/Services/DiskImages/DiskImageWorkspaceController.cs` pour envoyer les images sectorielles à `SectorMediaView` et cesser de fabriquer un SCP synthétique.
    - [x] Modifier `src/GWGUI.App/Views/Windows/Shell/MainWindow.xaml.cs` pour retirer la construction et l’injection devenues inutiles de `SectorImageFluxVisualizer`.

  - [x] 1.4 Finaliser la vue d’une image de disque dur
    - [x] Créer `src/GWGUI.MediaEngine/Visualization/Providers/BlockMediaVisualizationProvider.cs` pour décrire les plages 64 bits réellement annoncées par une représentation `Blocks` sans inventer de géométrie CHS.
    - [x] Modifier `src/GWGUI.MediaEngine/Composition/MediaVisualizationComposition.cs` pour enregistrer le provider `Blocks` dans l’orchestration commune.
    - [x] Créer `src/GWGUI.App/Enums/Rendering/Blocks/BlockMediaRangeState.cs` avec les états `Available`, `Reserved`, `Allocated`, `Free` et `Unknown` sans attribuer un état que la source ne fournit pas.
    - [x] Créer `src/GWGUI.App/Contracts/Rendering/Blocks/BlockMediaRenderModel.cs` avec les plages LBA, partitions, volumes, zones réservées, allouées, libres ou inconnues et la géométrie CHS uniquement lorsqu’elle est établie.
    - [x] Créer `src/GWGUI.App/Constants/Rendering/Blocks/BlockMediaRenderConstants.cs` avec les seuils d’agrégation, la géométrie et la palette propres aux cartes de blocs.
    - [x] Créer `src/GWGUI.App/Rendering/Blocks/SkiaBlockMediaRenderer.cs` pour agréger les grandes plages sans créer un élément graphique par secteur et dessiner une carte logique sélectionnable.
    - [x] Créer `src/GWGUI.App/Rendering/Blocks/SkiaHardDiskGeometryRenderer.cs` pour représenter plusieurs plateaux et surfaces seulement lorsque le document fournit une géométrie CHS exploitable.
    - [x] Créer `src/GWGUI.App/Views/Controls/Visualization/BlockMediaView.xaml` avec la carte logique et, lorsqu’elle existe, la vue CHS, ainsi que la sélection de partition, volume, plage ou surface.
    - [x] Créer `src/GWGUI.App/Views/Controls/Visualization/BlockMediaView.xaml.cs` pour recevoir le modèle de blocs, choisir la vue disponible, synchroniser le zoom et exposer les sélections de plage ou de surface au presenter sans construire lui-même le panneau commun.
    - [x] Créer `src/GWGUI.App/Presenters/Visualization/BlockMediaInspectorPresenter.cs` pour construire les plages disponibles et inconnues depuis le document, puis présenter capacité, adressage, partitions, volumes, systèmes de fichiers et géométrie sans inventer le nombre de plateaux.
    - [x] Modifier `src/GWGUI.App/Services/DiskImages/DiskImageWorkspaceController.cs` pour enregistrer `BlockMediaView`, lui transmettre les documents `Blocks` et afficher le modèle commun correspondant aux sélections connues.

  - [x] 1.5 Finaliser la vue d’une image de CD ou DVD
    - [x] Créer `src/GWGUI.MediaEngine/Visualization/Providers/OpticalMediaVisualizationProvider.cs` pour décrire les faces et pistes réellement annoncées par une représentation `OpticalTracks`.
    - [x] Modifier `src/GWGUI.MediaEngine/Composition/MediaVisualizationComposition.cs` pour enregistrer le provider `OpticalTracks` dans l’orchestration commune.
    - [x] Créer `src/GWGUI.App/Enums/Rendering/Optical/OpticalTrackKind.cs` avec les valeurs `Unknown`, `Data` et `Audio` afin de ne jamais déduire la nature d’une piste absente de la source.
    - [x] Créer `src/GWGUI.App/Contracts/Rendering/Optical/OpticalMediaRenderModel.cs` avec les faces, couches, sessions, pistes, index, plages de secteurs et la nature audio ou données réellement décrites.
    - [x] Créer `src/GWGUI.App/Constants/Rendering/Optical/OpticalMediaRenderConstants.cs` avec la géométrie et la palette propres au rendu optique.
    - [x] Créer `src/GWGUI.App/Rendering/Optical/SkiaOpticalMediaRenderer.cs` pour dessiner un disque par face, répartir couches, sessions et pistes selon leur ordre réel et agréger les secteurs lorsque nécessaire.
    - [x] Créer `src/GWGUI.App/Views/Controls/Visualization/OpticalMediaView.xaml` avec le disque, les sélecteurs de face, couche et session disponibles, le zoom et la sélection de piste.
    - [x] Créer `src/GWGUI.App/Views/Controls/Visualization/OpticalMediaView.xaml.cs` pour recevoir le modèle optique, synchroniser les sélecteurs, le zoom et la piste choisie avec le rendu, puis exposer cette sélection au presenter.
    - [x] Créer `src/GWGUI.App/Presenters/Visualization/OpticalMediaInspectorPresenter.cs` pour construire le modèle depuis la représentation optique et présenter face, couche, session, piste, index, mode, durée et volume associé sans déduire une structure absente.
    - [x] Modifier `src/GWGUI.App/Services/DiskImages/DiskImageWorkspaceController.cs` pour enregistrer `OpticalMediaView`, lui transmettre les documents `OpticalTracks` et afficher leur modèle d’inspection commun.

  - [x] 1.6 Finaliser la vue d’une image de cassette ou de bande
    - [x] Créer `src/GWGUI.MediaEngine/Visualization/Providers/SequentialMediaVisualizationProvider.cs` pour décrire les segments temporels et leurs voies réellement annoncés par une représentation `Sequential`.
    - [x] Modifier `src/GWGUI.MediaEngine/Composition/MediaVisualizationComposition.cs` pour enregistrer le provider `Sequential` dans l’orchestration commune.
    - [x] Créer `src/GWGUI.App/Enums/Rendering/Sequential/SequentialSegmentKind.cs` avec les valeurs `Unknown`, `Signal`, `Silence` et `DecodedBlock` sans déduire un contenu absent de la source.
    - [x] Créer `src/GWGUI.App/Contracts/Rendering/Sequential/SequentialMediaRenderModel.cs` avec les faces, pistes, canaux, segments, positions temporelles, silences, blocs décodés et la forme d’onde facultative.
    - [x] Créer `src/GWGUI.App/Constants/Rendering/Sequential/SequentialMediaRenderConstants.cs` avec les limites de lignes, la géométrie et la palette du rendu temporel.
    - [x] Créer `src/GWGUI.App/Rendering/Sequential/SkiaSequentialMediaRenderer.cs` pour répartir la chronologie sur plusieurs lignes et voies, préparer progressivement les segments et conserver leur ordre ainsi que leur sens de lecture.
    - [x] Créer `src/GWGUI.App/Views/Controls/Visualization/SequentialMediaView.xaml` avec les lignes temporelles, les sélecteurs de face, piste ou canal disponibles, le zoom horizontal et la sélection d’un segment.
    - [x] Créer `src/GWGUI.App/Views/Controls/Visualization/SequentialMediaView.xaml.cs` pour recevoir le modèle séquentiel, préparer progressivement ses segments, synchroniser défilement, zoom et sélection puis exposer la sélection au presenter.
    - [x] Créer `src/GWGUI.App/Presenters/Visualization/SequentialMediaInspectorPresenter.cs` pour construire le modèle depuis la représentation séquentielle et présenter position, durée, canal, piste, type de segment, fichier ou bloc reconnu et erreurs de décodage disponibles.
    - [x] Modifier `src/GWGUI.App/Services/DiskImages/DiskImageWorkspaceController.cs` pour enregistrer `SequentialMediaView`, lui transmettre les documents `Sequential` et afficher leur modèle d’inspection commun.

  - [x] 1.7 Harmoniser l’apparence et l’accessibilité
    - [x] Modifier `src/GWGUI.App/Resources/ApplicationStyles.xaml` avec les styles communs des surfaces, sélecteurs, légendes, badges, cartes d’informations et états de sélection du Visualiseur sur thèmes clair et sombre.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerTabSection.xaml` pour conserver une utilisation complète aux tailles minimale et maximale de la fenêtre, avec panneau d’informations repliable et défilement uniquement dans les zones nécessaires.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/MediaInspectorPanel.xaml` pour fournir ordre de tabulation, noms accessibles, contraste et lecture correcte des valeurs indisponibles.

  - [x] 1.8 Ajouter tous les textes du Visualiseur
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/SequentialMediaView.xaml.cs` pour utiliser un libellé court dédié dans le sélecteur de piste au lieu du résumé multiligne historique `Visual.Track`.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerTrackOverview.xaml.cs` pour représenter une seule fois les éléments sans surface déclarée, sans les dupliquer sur chaque face, couche ou voie disponible.
    - [x] Modifier `scripts/translate-resx-argos.py` pour permettre à `--sync-all` de cibler une seule culture et ainsi traduire les fichiers de ressources un par un dans l’ordre de la feuille.
    - [x] Modifier `docs/project/scripts.md` pour documenter l’option de culture ciblée ajoutée au script Argos.
    - [x] Modifier `src/GWGUI.App/Resources/00-Base/Visualizer.resx` avec les clés communes et les valeurs anglaises ou invariantes nécessaires aux cinq représentations, sans dupliquer CPU, CHS, LBA, CD, DVD ni les noms de formats.
    - [x] Modifier `scripts/translate-resx-argos.py` pour permettre à `--sync-all` de limiter la synchronisation à un seul catalogue de ressources.
    - [x] Modifier `docs/project/scripts.md` pour documenter le filtre de catalogue utilisable avec la synchronisation Argos ciblée.
    - [x] Modifier `src/GWGUI.App/Resources/ar-SA/Visualizer.resx` avec les traductions arabes des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/cs-CZ/Visualizer.resx` avec les traductions tchèques des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/da-DK/Visualizer.resx` avec les traductions danoises des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/de-DE/Visualizer.resx` avec les traductions allemandes des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/el-GR/Visualizer.resx` avec les traductions grecques des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/en-US/Visualizer.resx` avec les textes anglais des nouvelles clés du Visualiseur qui ne sont pas déjà hérités de la base commune.
    - [x] Modifier `src/GWGUI.App/Resources/es-ES/Visualizer.resx` avec les traductions espagnoles des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/fi-FI/Visualizer.resx` avec les traductions finnoises des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/fr-FR/Visualizer.resx` avec les traductions françaises des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/he-IL/Visualizer.resx` avec les traductions hébraïques des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/hu-HU/Visualizer.resx` avec les traductions hongroises des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/id-ID/Visualizer.resx` avec les traductions indonésiennes des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/it-IT/Visualizer.resx` avec les traductions italiennes des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/ja-JP/Visualizer.resx` avec les traductions japonaises des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/ko-KR/Visualizer.resx` avec les traductions coréennes des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/nb-NO/Visualizer.resx` avec les traductions norvégiennes des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/nl-NL/Visualizer.resx` avec les traductions néerlandaises des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/pl-PL/Visualizer.resx` avec les traductions polonaises des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/pt-BR/Visualizer.resx` avec les traductions portugaises du Brésil des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/pt-PT/Visualizer.resx` avec les traductions portugaises du Portugal des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/ro-RO/Visualizer.resx` avec les traductions roumaines des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/ru-RU/Visualizer.resx` avec les traductions russes des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/sv-SE/Visualizer.resx` avec les traductions suédoises des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/th-TH/Visualizer.resx` avec les traductions thaïlandaises des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/tr-TR/Visualizer.resx` avec les traductions turques des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/uk-UA/Visualizer.resx` avec les traductions ukrainiennes des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/vi-VN/Visualizer.resx` avec les traductions vietnamiennes des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/zh-Hans/Visualizer.resx` avec les traductions chinoises simplifiées des nouvelles clés du Visualiseur produites avec Argos.
    - [x] Modifier `src/GWGUI.App/Resources/zh-Hant/Visualizer.resx` avec les traductions chinoises traditionnelles des nouvelles clés du Visualiseur produites avec Argos.

  - [x] 1.9 Ajouter les tests généraux des vues
    - [x] Créer `tests/GWGUI.Tests/Interface/VisualizerViews/MediaVisualizationRoutingTests.cs` avec le routage de Flux, Sectors, Blocks, OpticalTracks et Sequential vers leur vue sans décision fondée sur l’extension.
    - [x] Créer `tests/GWGUI.Tests/Interface/VisualizerViews/FloppyVisualizationTests.cs` avec la séparation des informations Flux et Sectors, la progression et la sélection simulées sans fichier du corpus local.
    - [x] Créer `tests/GWGUI.Tests/Interface/VisualizerViews/OtherMediaVisualizationTests.cs` avec les sélections Blocks, OpticalTracks et Sequential, les informations absentes et les grandes plages simulées sans fichier du corpus local.
    - [x] Créer `tests/GWGUI.Tests/Interface/VisualizerViews/MediaVisualizationLayoutTests.cs` avec les tailles minimales, le panneau replié, les thèmes clair et sombre et les noms accessibles des commandes.

  - [x] 1.10 Produire le build destiné à la vérification visuelle
    - [x] Modifier `src/GWGUI.App/Services/Visualization/ScpInspectorController.cs` pour importer le type WPF `SelectionChangedEventArgs` utilisé par le contrôleur après la migration du panneau commun.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerTrackOverview.xaml.cs` pour importer l’enum commun `MediaRepresentationKind` utilisé par le routage des libellés de surface.
    - [x] Modifier `src/GWGUI.App/Services/DiskImages/DiskImageWorkspaceController.cs` pour importer l’enum commun `MediaRepresentationKind` utilisé par l’enregistrement et le routage des nouvelles vues.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerHeaderSection.xaml.cs` pour importer `System.IO.Path` utilisé pour présenter le nom du fichier reconnu.
    - [x] Modifier `src/GWGUI.App/Presenters/Visualization/BlockMediaInspectorPresenter.cs` pour utiliser la longueur logique déjà validée par le type Blocks sans conserver d’accès nullable averti par le compilateur.
    - [x] Modifier `src/GWGUI.App/Presenters/Visualization/BlockMediaInspectorPresenter.cs` pour importer `System.IO.InvalidDataException` utilisé par la validation de la représentation Blocks.
    - [x] Modifier `tests/GWGUI.Tests/Interface/VisualizerViews/VisualizerDocumentScenarios.cs` pour construire `DiskImageWorkspaceController` avec sa nouvelle orchestration et sans l’ancien convertisseur sectoriel injecté.
    - [x] Modifier `tests/GWGUI.Tests/Interface/VisualizerViews/InspectorSelectionScenarios.cs` pour vérifier le nouveau `MediaInspectorPanel` et son modèle commun après suppression de l’ancien inspecteur SCP.
    - [x] Modifier `tests/GWGUI.Tests/Interface/VisualizerViews/MediaVisualizationLayoutTests.cs` pour qualifier explicitement l’application WPF et éviter la collision avec le namespace de tests `Application`.
    - [x] Modifier `tests/GWGUI.Tests/Interface/VisualizerViews/FloppyVisualizationTests.cs` pour importer l’enum commun des états de progression utilisé par le test synthétique.
    - [x] Modifier `tests/GWGUI.Tests/Interface/VisualizerViews/MediaVisualizationLayoutTests.cs` pour vérifier le repli du conteneur réel du panneau d’informations au lieu de la visibilité interne du contrôle conservé.
    - [x] Modifier `tests/GWGUI.Tests/Interface/VisualizerViews/MediaVisualizationLayoutTests.cs` pour vérifier les ressources dynamiques des thèmes clair et sombre après leur application effective au contrôle.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/MediaInspectorPanel.xaml.cs` afin que son nom accessible suive immédiatement le titre du modèle commun effectivement affiché.
    - [x] Modifier `tests/GWGUI.Tests/Interface/VisualizerViews/MediaVisualizationLayoutTests.cs` pour vérifier directement les noms accessibles attachés aux commandes sans créer de pairs UI Automation qui bloquent ensuite le dispatcher WPF partagé.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerTabSection.xaml` pour attribuer explicitement aux commandes d’inspection leurs noms accessibles traduits, indépendamment de la création d’un pair UI Automation.
    - [x] Modifier `tests/GWGUI.Tests/Interface/VisualizerViews/MediaVisualizationLayoutTests.cs` pour limiter le scénario des noms accessibles aux propriétés concernées et ne pas laisser un second arbre visuel arrangé sur le dispatcher partagé.
    - [x] Modifier `docs/project/testing.md` avec le résultat des tests généraux du Visualiseur et de `scripts/build.ps1 -Configuration Debug`, la présence vérifiée de `build/Debug/GW GUI/gwgui.exe` et les chemins manuels à ouvrir pour observer Flux, Sectors, Blocks, OpticalTracks et Sequential.

  Après achèvement et validation de toutes les cases de cette feuille, créer le deuxième commit
  demandé avec cette base graphique. Continuer ensuite avec la première case non cochée de
  [`media-exploration.md`](media-exploration.md). Aucun essai manuel du corpus `image_test` ne
  commence avant l’achèvement des feuilles suivantes et le troisième commit qui les termine.
