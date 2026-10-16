# Travaux communs restants

## Passage à Aucun dans tous les modules

- [x] Reproduire puis corriger la notification intermédiaire du port.
  - [x] Créer `tests/GWGUI.Tests/Interface/EmulationViews/ControllerNoneSelectionTests.cs` : parcourir les 23 modules et les ports de leurs machines, appliquer les réglages à chaque notification pendant Aucun/périphérique/Aucun, et contrôler que chaque notification contient un choix valide.
  - [x] Modifier `src/GWGUI.App/Controllers/Emulation/Input/EmulationInputSettingsController.cs` : regrouper les changements de type et de visuel afin de notifier uniquement un état complet ; garantir la fin du regroupement dans finally.
  - [x] Modifier `tests/GWGUI.Tests/Interface/EmulationViews/ControllerNoneSelectionTests.cs` : distinguer les ports proposant Aucun des commandes intégrées qui ne le proposent pas, vérifier leur déclaration, et contrôler aussi leurs changements de catégorie sans ajouter de capacité aux modules.
- [x] Livrer le correctif commun.
  - [x] Modifier `docs/tasks/emulation/commun.md` : consigner les contrôles des 23 modules, le build Debug complet et le résultat du correctif, après leur réussite effective.

Exception Apple reproduite avant correction : le changement de type reconstruit ItemsSource des visuels, provoque une SelectionChanged intermédiaire et applique SelectedControllerId vide. Le contrôleur commun regroupe maintenant le changement du type, du visuel, des associations et des sorties ; une seule notification publie le port complet, avec libération du regroupement dans finally. Aucune validation de module assouplie et aucune exception masquée.

Contrôle des 23 modules, de leurs machines et des moteurs déclarés : 669 occurrences de ports déconnectables et 1 230 passages à Aucun réussis, en appliquant chaque notification immédiatement comme la fenêtre réelle. Les 95 occurrences de ports ne déclarant pas Aucun sont vérifiées selon leurs catégories existantes, sans leur ajouter de capacité. Les modèles sans moteur ou port n'engendrent pas de faux essai de déconnexion. Les 23 scénarios transversaux et le contrôle Acorn existant réussissent sur les DLL finales : 24 tests, zéro échec. Aucun cœur natif lancé, aucun média ou fichier temporaire créé ; les arbres de contrôle sont déchargés dans finally et l'infrastructure STA existante est réutilisée.

Build Debug complet réussi avec application et 23 modules ; exécutable présent, empreintes application/SDK/modules concordantes et staging absent. Le wrapper local-building.cmd pointe vers un chemin PowerShell inexistant ; le même build a été exécuté avec `pwsh -NoProfile -File scripts/local-building/build.ps1 -Configuration Debug -AllModules`, sans modifier le wrapper.

## Palettes WonderSwan dans Image

- [x] Déplacer les préréglages sans dupliquer les réglages communs.
  - [x] Créer `src/GWGUI.Emulation/Enums/EmulationVideoSettingsSection.cs` et modifier `Contracts/EmulationSettingsField.cs` : déclarer la destination Affichage/Image/Effets des champs vidéo, Affichage par défaut.
  - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/ModuleSettings/EmulationModuleSettingsSection.Layout.cs`, `EmulationVideoProcessingSettingsSection.cs`, `EmulationVideoProcessingSettingsSection.Layout.cs` et `EmulationImageParametersSettingsBlock.cs` : présenter les champs Image dans le bloc de paramètres existant et les exclure d'Affichage.
  - [x] Créer `src/GWGUI.Emulation.Bandai/Emulators/BeetleWonderSwan/Functions/PaletteFunctions.cs` et modifier sa fabrique et `Functions/SettingsFunctions.cs`, ainsi que `Emulators/Common/Interop/Factories/MachineFactory.cs` et `Emulators/Common/Interop/Services/ExternalCore.cs` : fournir les palettes uniquement aux trois WonderSwan dans Image et appliquer au cœur le défaut du modèle sans écraser un choix explicite.
- [x] Vérifier et livrer le lot.
  - [x] Créer `tests/GWGUI.Tests/Emulation/Bandai/MonochromePaletteTests.cs` : vérifier les trois défauts, le choix explicite et la portée des champs Image.
  - [x] Modifier `tests/GWGUI.Tests/Interface/SettingsViews/EmulationModuleSettingsNavigationScenarios.cs` : étendre le contrôle existant aux champs Image/Effets et à leur conservation après reconstruction du panneau.
  - [x] Mettre à jour `docs/tasks/emulation/commun.md` après les tests ciblés et le build Debug de l'application et des 23 modules ; consigner le résultat sans déclarer Pippin raccordé.

Résultat : palette monochrome déplacée dans le bloc Image existant, uniquement pour WonderSwan, WonderSwan Color et SwanCrystal. Les réglages communs restent accessibles. Défauts transmis au cœur : wonderswan, wondeswan_color (orthographe native), swancrystal ; les choix explicites sont conservés. Aucun nouveau libellé, aucune traduction dupliquée ni migration. Le contrat commun indique seulement la section vidéo ; l'application ne connaît aucun identifiant Bandai.

34 contrôles ciblés réussis : 32 Bandai, dont huit nouveaux cas de palettes, et deux contrôles existants d'interface vidéo, dont celui étendu aux champs Image/Effets après reconstruction. Build Debug complet réussi ; exécutable présent, 23 modules et deux copies du SDK vérifiés par empreinte, staging absent. Aucun cœur natif ni fixture externe lancé pour ce lot. Pippin conserve son libellé corrigé, sans raccordement de MAME Libretro : l'utilisateur a choisi d'attendre une DLL fonctionnelle.
## Correction du libellé Pippin

- [x] Documenter les vérifications demandées avant le raccordement.
  - [x] Modifier `docs/tasks/emulation/commun.md` : distinguer le pilote MAME officiel du pilote Libretro du catalogue et expliquer la palette monochrome WonderSwan sans changer les modèles ni retirer leurs options.

Vérification du 16 octobre : `libretro/mame`, révision `5f5a07748ce5c239c4118463ce8e6473743d4859`, conserve `MACHINE_NOT_WORKING` dans `src/mame/apple/pippin.cpp`. Le constat sur `mamedev/mame` ci-dessous ne vaut donc pas pour le port Libretro. Décision utilisateur : attendre une DLL fonctionnelle, sans raccordement expérimental ; aucun moteur supplémentaire ni compatibilité fonctionnelle déclarés.

Beetle WonderSwan : `wswan_mono_palette` concerne uniquement la colorisation des jeux monochromes. Les valeurs WonderSwan, WonderSwan Color et SwanCrystal imitent les teintes de leurs écrans, sans sélectionner le matériel et sans transformer un jeu monochrome en jeu couleur. La palette reste utile sur une machine couleur pour les jeux monochromes compatibles. Aucun choix retiré ni modèle modifié pour cette question explicative.

- [x] Corriger le nom et consigner la cause du sélecteur vide.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Resources/00-Base/Model.resx` : remplacer « Apple / Bandai Pippin @mark » par « Pippin » dans la ressource invariante.
  - [x] Modifier `docs/tasks/emulation/commun.md` : consigner que le catalogue Pippin déclare `Emulator.None` et aucune fabrique compatible ; distinguer cette absence de raccordement du pilote existant dans MAME.

Le sélecteur vide provient du catalogue local : `PippinModelCatalog` déclare `Emulator.None` et une liste de moteurs vide. Le module Apple ne possède que les adaptateurs AppleWin et Mini vMac. Le pilote Pippin existe dans MAME ; sa source officielle actuelle (`src/mame/apple/pippin.cpp`) enregistre la machine avec `MACHINE_SUPPORTS_SAVE`, sans `MACHINE_NOT_WORKING`. L'absence de raccordement local ne permet donc pas de conclure que MAME ne sait pas émuler Pippin. Le raccordement et la qualification de la DLL Libretro restent à réaliser ; aucune compatibilité native n'est déclarée par ce changement de libellé.

Suivi des demandes encore ouvertes. Les étapes terminées et leurs journaux détaillés ont été retirés. Le catalogue `liste-emulateur-libretro.md`, la méthode `methode-modules.md`, le chronomètre et les suivis Atari/Commodore/NEC sont conservés.

Dernière livraison vérifiée : correctif LRPS2 sans conversion des anciens libellés, build Debug réussi, application, SDK et 23 modules vérifiés par empreinte. Exécutable : `F:/GW GUI/build/Debug/GW GUI/gwgui.exe`. Les 390 contrôles Sony du lot LRPS2 sont réussis ; les 152 contrôles des entrées communes restent consignés dans `9694f5147`. Le lot précédent de constantes audio communes a passé 287 contrôles des quinze familles (`d37efb8d5`). Ces résultats ne valent pas qualification native exhaustive des cinquante DLL du catalogue.

Historique détaillé disponible localement : `git show 9694f5147:docs/tasks/emulation/commun.md`. Les anciens essais, empreintes de médias, PID et résultats restent dans cet historique ; ils ne sont pas répétés dans les tâches restantes.

## Revue transversale des quinze familles

Les 269 contrôles existants passent sur les DLL de la livraison Debug : Acorn 23, Apple 45, Bandai 20, Coleco 30, Epoch 15, Fairchild 6, Philips 5, Mattel 3, Sharp 10, Sinclair 7, SNK 70, Thomson 11, Watara 6, Wenquxing 12 et Panasonic 6. Le filtre et son inventaire couvrent bien les quinze familles. Aucun nouveau test ni artefact externe créé ; ces scénarios logiciels ne qualifient pas toutes les options, les supports natifs ou les périphériques physiques.

La recherche des enums de quantités dans ces quinze modules n'a pas retrouvé les types numériques inutiles corrigés côté Sony. Les enums de modes et d'identités natives restent justifiés ; les plages Potator sont déjà générées numériquement. Aucun remplacement mécanique de ces types n'est effectué. Les limites de la section « Périmètre conservé » et les qualifications natives encore ouvertes restent à traiter ; aucun module supplémentaire déclaré terminé par cette revue.

## Panasonic : BIOS intégré Opera et ROM externe

Comparaison avec libretro/opera-libretro 684926fe2079ba12b59d99082291c0a8fe780c67 : dix-huit clés et leurs valeurs/défauts présents. Le BIOS intégré Opera Boot était bloqué par l'obligation locale d'une ROM externe. Sans ROM système sélectionnée, l'adaptateur demande désormais `opera-boot-internal` ; une ROM externe garde la priorité et les deux champs ROM restent accessibles. Aucun cœur modifié, aucune migration et aucun nouveau libellé à traduire.

Qualification avec la DLL officielle Opera 1.0.0 684926f, SHA256 8B4BCD8677E6D275784656777C62E506A4E54EBED6709C11A2314602A95F2E20 : démarrage sans fichier dans System, 180 trames 320 × 240, processus hôte PID 49660 observé terminé et mapping vidéo absent après fermeture. Fixture externe, archive, DLL et sessions temporaires supprimées ; les seuls tests ont été reconstruits après retrait. Dix contrôles Panasonic permanents réussis, dont quatre nouveaux scénarios du choix de firmware. Build Debug complet réussi ; exécutable, deux copies du SDK et 23 modules vérifiés par empreinte, staging absent. Les autres limites de l'objectif restent ouvertes.

## Philips : options et matériels O2EM

Comparaison officielle : libretro/libretro-o2em 679d6fec04963f6e70a7ec217e3d0ebb1fe472fc, dix clés d'options présentes. L'option native `o2em_swap_gamepads` était masquée ; elle est maintenant exposée une seule fois dans Manettes, comme réglage global, avec défaut disabled et valeurs enabled/disabled inchangés. Les 28 traductions ont été produites avec Argos puis relues et corrigées ; audit des quatre catalogues traduit réussi. Neuf contrôles Philips réussis, dont quatre nouveaux cas vérifiant l'emplacement, l'unicité, le défaut, la sauvegarde et la transmission native de la permutation sans altérer les affectations. Aucun cœur natif ni fichier temporaire utilisé par ce lot. Build Debug complet réussi, exécutable, deux copies du SDK et 23 modules vérifiés par empreinte, staging absent.

Videopac+ G7400 et Brandt Jopac JO7400 intégrés comme deux modèles distincts utilisant la DLL O2EM du catalogue, sans autre moteur ajouté. BIOS g7400.bin et jopac.bin associés à leurs empreintes officielles distinctes ; clavier, deux ports de manette et cartouche restent accessibles. Noms invariants dans la base commune uniquement, photos complètes détourées classiquement en PNG transparents 512 × 382 et 512 × 393, crédits dans THIRD-PARTY-NOTICES.md. Catalogue des machines O2EM complété.

Quatre parcours natifs réussis avec O2EM 1.18 679d6fe, SHA256 B0EEB891041FAA1BEE48710236E3C8EE0F86E55055DFC79F0DE9BA805A2EEBEC : Atlantis et K.C. Munchkin sur chacun des deux modèles, 240 trames 340 × 250 par parcours et BIOS sélectionné confirmé. Hôtes PID 55840, 41548, 51320 et 19508 observés terminés, mappings vidéo absents après fermeture et empreintes des BIOS/cartouches originaux inchangées. Ces essais portent sur deux cartouches classiques compatibles, pas sur tous les logiciels Videopac+. Les deux BIOS de 1024 octets restent sous F:/Retro/ROMS-BOOT/Philips/VideopacG7400 et JopacJo7400.

Fixture externe, archive, DLL et tout le dossier de photos/sessions temporaires supprimés ; reconstruction des seuls tests après retrait et onze contrôles Philips permanents réussis. Build Debug complet réussi : exécutable, deux copies du SDK et 23 modules vérifiés par empreinte, staging absent. Aucun cœur natif modifié ni recompilé. Les autres limites de l'objectif restent ouvertes.
Lot Philips : 84 constantes textuelles numériques remplacées par des plages de pourcentages et des dimensions de résolution. Les six options O2EM/SAME CDi conservent leurs valeurs natives, défauts, ordre et libellés ; filtre O2EM limité à 5–95 %, volumes à 0–100 %, pas CPU CD-i préservés. Les 17 contrôles Philips passent. Build Debug complet réussi, exécutable, deux copies du SDK et 23 modules vérifiés ; staging absent. Aucun cœur natif ni ressource graphique lancé pour ce lot.
## Visuels restants
Lot visuel Sony conservé : DualShock originale remplacée par les photographies identifiées, masque classique et PNG transparent de 512 px. Le remplacement PSP du même lot est annulé à la demande utilisateur : la vue blanche frontale et son profil sont restaurés ; le bilan des anciens visuels ne vaut pas validation de la restauration actuelle.

Lot Apple : Kraft Premium II ajouté aux cinq visuels Apple II, avec quatre directions analogiques et un seul bouton de tir ; PNG transparent 512 × 504. Les 46 contrôles Apple et catalogue passent avec les DLL du build Debug ; exécutable, deux copies du SDK et 23 modules vérifiés, staging et fichiers temporaires supprimés.

- [ ] Réaliser les profils NEC
- [ ] Réaliser les profils Microsoft
- [ ] Réaliser les profils Sony
- [ ] Réaliser les profils Nintendo
- [ ] Réaliser les profils Sega
- [ ] Raccorder les Sega Saturn Virtua Stick HSS-0104 et HSS-0136
  - [ ] Revoir les 108 images une par une dans l'ordre alphabétique du dossier ; corriger tout écart avant la suivante.
    - [ ] Corriger le relief des boutons rouges du CX52 d'après les deux nouvelles vues de référence
      - [ ] Créer `artifacts/controller-sources/atari-5200-flanks-redrawn-preview.png` : composer les flancs redessinés avec la vue de dessus et vérifier visuellement la géométrie avant de remplacer l'image de l'application.
      - [ ] Modifier `src/GWGUI.App/Assets/Controllers/atari-5200-controller.png` : utiliser l'illustration corrigée seulement après contrôle visuel côte à côte avec les photographies des flancs.
      - [ ] Modifier `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Atari/5200/Atari5200Controller.cs` : ajuster les quatre zones d'action aux boutons saillants dessinés.
      - [ ] Modifier `docs/tasks/emulation/commun.md` : documenter la géométrie saillante des boutons et l'état réel de la validation.
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner la vérification du build Debug après ce dernier ajustement.
    - [ ] Reprendre le HSS-0104 selon l'avis de l'utilisateur après le CX52
      - [ ] Modifier `docs/tasks/emulation/commun.md` : enregistrer sa validation ou le défaut précis restant et prévoir la correction correspondante avant de passer à une autre manette.
    - [ ] Terminer la répartition du registre des profils par marque
      - [ ] Créer `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Microsoft/MicrosoftProfileRegistry.cs` : enregistrer les profils Xbox dans leur propre dictionnaire.
      - [ ] Créer `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Nintendo/NintendoProfileRegistry.cs` : enregistrer les profils Nintendo dans leur propre dictionnaire.
      - [ ] Créer `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Generic/GenericProfileRegistry.cs` : enregistrer les joysticks multiplateformes dans leur propre dictionnaire.
      - [ ] Créer `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Amiga/AmigaProfileRegistry.cs` : enregistrer les profils Amiga dans leur propre dictionnaire.
      - [ ] Créer `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Atari/AtariProfileRegistry.cs` : enregistrer les profils Atari dans leur propre dictionnaire.
      - [ ] Créer `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Sega/SegaProfileRegistry.cs` : enregistrer les profils Sega dans leur propre dictionnaire.
      - [ ] Modifier `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkCatalog.Profiles.cs` : garder seulement l'assemblage du dictionnaire des marques.
      - [ ] Modifier `docs/tasks/emulation/commun.md` : décrire la répartition vérifiée et le résultat du build de l'application.
    - [ ] Terminer commodore-cd32.png
      - [ ] Créer `artifacts/controller-sources/compose-amiga-cd32-controller.ps1` : composer la vue de face existante avec deux petites découpes photographiques montrant uniquement les gâchettes, en préservant leurs pixels.
      - [ ] Modifier `src/GWGUI.App/Assets/Controllers/commodore-cd32.png` : afficher ces découpes avec le contrôleur existant sur fond transparent.
      - [ ] Modifier `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Amiga/Joysticks/CommodoreCd32.cs` : placer les halos des gâchettes sur les découpes réelles.
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `commodore-cd32.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] competition-pro-cd32.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `competition-pro-cd32.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] dreamcast.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `dreamcast.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] flight-stick.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `flight-stick.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] generic-gamepad.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `generic-gamepad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] konix-speedking-left-hand.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `konix-speedking-left-hand.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] konix-speedking-right-hand.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `konix-speedking-right-hand.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] mattel-hyperscan-controller.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `mattel-hyperscan-controller.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] mega-drive-6.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `mega-drive-6.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-arcade-pad-6.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nec-arcade-pad-6.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-avenue-pad-3.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nec-avenue-pad-3.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-avenue-pad-6.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nec-avenue-pad-6.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-cordless-pad.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nec-cordless-pad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-coregrafx-ii-turbopad.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nec-coregrafx-ii-turbopad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-coregrafx-turbopad.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nec-coregrafx-turbopad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-duopad.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nec-duopad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-duor-turbopad.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nec-duor-turbopad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-pc-engine-lt-controls.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nec-pc-engine-lt-controls.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-pc-engine-mouse.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nec-pc-engine-mouse.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-pc-engine-pad.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nec-pc-engine-pad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-pc-engine-turbopad-ii.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nec-pc-engine-turbopad-ii.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-pc-engine-turbopad.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nec-pc-engine-turbopad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-pc-engine-turbostick.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nec-pc-engine-turbostick.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-pc-fx-mouse.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nec-pc-fx-mouse.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-pc-fx-pad.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nec-pc-fx-pad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-turboexpress-controls.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nec-turboexpress-controls.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-turbografx-turbopad.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nec-turbografx-turbopad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-turbografx-turbostick.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nec-turbografx-turbostick.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nintendo-64.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nintendo-64.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nintendo-game-boy-advance.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nintendo-game-boy-advance.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nintendo-game-boy-color.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nintendo-game-boy-color.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nintendo-game-boy.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nintendo-game-boy.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nintendo-gamecube-wavebird.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nintendo-gamecube-wavebird.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nintendo-gamecube.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nintendo-gamecube.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nintendo-nes-advantage.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nintendo-nes-advantage.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nintendo-nes-dogbone.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nintendo-nes-dogbone.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nintendo-nes-max.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `nintendo-nes-max.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] philips-cdi-gamepad.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `philips-cdi-gamepad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] playstation-1.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `playstation-1.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] playstation-2.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `playstation-2.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] playstation-4.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `playstation-4.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] playstation-5.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `playstation-5.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] quickshot-deluxe.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `quickshot-deluxe.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] quickshot-ii-turbo.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `quickshot-ii-turbo.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] quickshot.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `quickshot.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] racing-wheel.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `racing-wheel.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] saturn-3d-control-pad.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `saturn-3d-control-pad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] saturn.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `saturn.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] sega-arcade-power-stick-3.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `sega-arcade-power-stick-3.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] sega-arcade-power-stick-6.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `sega-arcade-power-stick-6.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] sega-control-stick.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `sega-control-stick.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] sega-saturn-virtua-stick-hss0104.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `sega-saturn-virtua-stick-hss0104.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] sega-saturn-virtua-stick-hss0136.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `sega-saturn-virtua-stick-hss0136.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] sega-sg1000-ii.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `sega-sg1000-ii.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] sega-sg1000-joystick.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `sega-sg1000-joystick.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] super-nes-controller.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `super-nes-controller.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] super-nintendo.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `super-nintendo.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-360-big-button-pad.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `xbox-360-big-button-pad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-360-black.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `xbox-360-black.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-360-chatpad.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `xbox-360-chatpad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-360-transforming-dpad.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `xbox-360-transforming-dpad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-360-white.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `xbox-360-white.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-360-wireless-racing-wheel.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `xbox-360-wireless-racing-wheel.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-360-wireless-speed-wheel.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `xbox-360-wireless-speed-wheel.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-chatpad.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `xbox-chatpad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-controller-s.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `xbox-controller-s.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-duke.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `xbox-duke.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-dvd-movie-playback-kit.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `xbox-dvd-movie-playback-kit.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-one.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `xbox-one.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-rematch-core.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `xbox-rematch-core.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-series.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `xbox-series.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
  - [ ] Créer `artifacts/controller-sources/cutout-duke.ps1` : préparer un détourage déterministe de la photo frontale du Duke et deux petites découpes de sa photo arrière montrant les gâchettes, sans modifier les pixels du matériel.
  - [ ] Modifier `src/GWGUI.App/Assets/Controllers/xbox-duke.png` : remplacer l'illustration inventée par les pixels du matériel photographié, avec seul le fond détouré de manière déterministe.
  - [ ] Modifier `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkCatalog.cs` : replacer les zones de la manette Duke sur ses commandes photographiées.
  - [ ] Modifier `docs/tasks/emulation/commun.md` : créditer la photo Duke et consigner les contrôles effectués.
  - [ ] Créer `artifacts/controller-sources/xbox-dvd-remote-original.jpg` : conserver une photographie de face de la télécommande DVD Xbox d'origine.
  - [ ] Modifier `src/GWGUI.App/Assets/Controllers/xbox-dvd-movie-playback-kit.png` : remplacer la télécommande reconstruite par la photographie du vrai modèle, fond seulement détouré.
  - [ ] Modifier `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkCatalog.cs` : replacer les halos sur les touches réelles de la télécommande.
  - [ ] Modifier `docs/tasks/emulation/commun.md` : créditer la photo de la télécommande et noter le contrôle des touches.
  - [ ] Créer `artifacts/controller-sources/xbox-controller-s-original.jpg` : conserver une photographie de face du Microsoft Controller S original.
  - [ ] Modifier `src/GWGUI.App/Assets/Controllers/xbox-controller-s.png` : remplacer l'illustration par la photo détourée sans changement des boutons Black/White.
  - [ ] Modifier `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkCatalog.cs` : recalculer les halos du Controller S.
  - [ ] Modifier `docs/tasks/emulation/commun.md` : créditer et vérifier le Controller S.
  - [ ] Créer `artifacts/controller-sources/xbox-chatpad-original.png` : conserver une photographie de face du Chatpad Microsoft pour Xbox One.
  - [ ] Modifier `src/GWGUI.App/Assets/Controllers/xbox-chatpad.png` : remplacer les touches inventées par les pixels de la photographie, avec détourage du fond uniquement.
  - [ ] Modifier `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkCatalog.cs` : recalculer les zones du Chatpad d'après les touches réelles.
  - [ ] Modifier `docs/tasks/emulation/commun.md` : créditer et vérifier le Chatpad Xbox One.
  - [ ] Créer `artifacts/controller-sources/sega-sg1000-joystick-original.jpg` : conserver une photo de dessus droite du joystick Sega SG-1000 exact.
  - [ ] Modifier `src/GWGUI.App/Assets/Controllers/sega-sg1000-joystick.png` : remplacer la vue fausse ou inclinée par la photo du vrai joystick, détourée uniquement.
  - [ ] Modifier `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkCatalog.cs` : replacer les halos du joystick SG-1000.
  - [ ] Modifier `docs/tasks/emulation/commun.md` : créditer et vérifier le joystick SG-1000.
  - [ ] Créer `artifacts/controller-sources/sega-sg1000-ii-original.jpg` : conserver une photo de face droite du contrôleur Sega SG-1000 II exact.
  - [ ] Modifier `src/GWGUI.App/Assets/Controllers/sega-sg1000-ii.png` : remplacer la vue fausse ou inclinée par la photo de la vraie manette, détourée uniquement.
  - [ ] Modifier `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkCatalog.cs` : replacer les halos SG-1000 II.
  - [ ] Modifier `docs/tasks/emulation/commun.md` : créditer et vérifier la manette SG-1000 II.
  - [ ] Modifier `docs/tasks/emulation/commun.md` : détailler les autres remplacements Xbox 360, NEC, Nintendo et Virtua Stick après ce lot prioritaire.
  - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner le build Debug et la présence de l'application et des huit modules.
- [ ] Vérifier la couverture et le fonctionnement
  - [ ] Modifier `src/GWGUI.App/Assets/Controllers/nec-arcade-pad-6.png` : retirer les quelques pixels détachés à l'extérieur de la silhouette sans altérer les boutons.
  - [ ] Modifier `src/GWGUI.App/Assets/Controllers/nec-pc-engine-turbostick.png` : nettoyer les pixels résiduels du fond sur les bords du PI-PD4 sans toucher aux boutons ; les retouches génératives tentées jusqu'ici conservent ces pixels.
  - [ ] Modifier `src/GWGUI.App/Assets/Controllers/nintendo-gamecube-wavebird.png` : retirer les franges colorées hors de la silhouette par une méthode expressément autorisée, sans altérer les commandes.
  - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner l'état de l'alpha WaveBird après nettoyage et la vérification des halos dans l'interface.
  - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner la couverture réelle des cinq modules, les contrôles image/transparence/zones, le résultat du build Debug de l'application et des modules et les essais fonctionnels de sélection et de halos.

## remaining-machines-emulators

- [ ] Compléter les systèmes de liste-emulateur-libretro.md
  - [ ] Établir la couverture et les actions par module
    - [x] Modifier `docs/tasks/emulation/commun.md` : comparer les quinze modules demandés et Sony aux DLL et machines du catalogue, y compris les entrées croisées ; relever les omissions concrètes et les limites natives déjà établies avant toute correction du code.
  - [ ] Compléter Nintendo
  - [ ] Compléter les autres associations, machines et familles
    - [ ] Modifier `docs/tasks/emulation/commun.md` : détailler les prochaines intégrations vérifiées avant tout changement de leurs fichiers.
  - [ ] Valider l’ensemble
    - [ ] Modifier `docs/tasks/emulation/commun.md` : noter la couverture finale, les contrôles pertinents et le résultat du build Debug avec tous les modules.

## remaining-validations

- [ ] 1. Valider les derniers comportements Amiga
  - [ ] 1.1 Mesurer et corriger la synchronisation audio
    - [ ] Modifier `docs/reference/emulation-machines.md` après deux essais de dix minutes en PAL et NTSC pour consigner la cible de tampon, les underruns, les overruns et toute correction appliquée dans le moteur Amiga.
  - [ ] 1.2 Valider la persistance des écritures ADF
    - [ ] Modifier `docs/reference/emulation-machines.md` après une écriture dans une copie de `Boot-DD-OFS.adf`, sa réouverture et la comparaison du SHA-256 de l’original en lecture seule, afin de consigner le résultat et toute correction appliquée.
  - [ ] 1.3 Valider les machines Amiga nécessitant leurs firmwares
    - [ ] Modifier `docs/reference/emulation-machines.md` après les essais CDTV, CD32 et CD32FR avec leurs ROM principales et étendues pour consigner les firmwares employés et les résultats.
  - [ ] 1.4 Effectuer les essais longs
    - [ ] Modifier `docs/reference/emulation-machines.md` après trente minutes PAL puis NTSC avec vidéo, audio et entrées pour consigner la stabilité et les compteurs audio.
- [ ] 2. Terminer la validation cassette Atari800
  - [ ] 2.1 Tester les parcours encore non vérifiés
    - [ ] Modifier `docs/reference/atari-libretro.md` après une lecture manuelle sans amorçage automatique, la modification des options moteur arrêté puis actif, l’amorçage automatique sur Atari 400 et les essais d’une cassette connue sur Atari 800XL et Atari 130XE.
- [ ] 3. Terminer la validation GameInput
  - [ ] 3.1 Vérifier la compatibilité des anciennes associations
    - [ ] Modifier `docs/reference/gameinput.md` après des essais avec une configuration `xinput:*`, une manette absente, une reconnexion et un changement d’ordre pour consigner les identités conservées et toute correction appliquée.
  - [ ] 3.2 Vérifier les distributions sans environnement de développement
    - [ ] Modifier `docs/reference/gameinput.md` après les essais de l’installateur et du ZIP portable sur une machine sans SDK .NET, avec et sans runtime GameInput suffisant, et après ajout d’un message traduit si l’initialisation échoue.
  - [ ] 3.3 Vérifier le matériel disponible
    - [ ] Modifier `docs/reference/gameinput.md` après les essais des boutons, sticks, gâchettes simultanées, Guide, Share, palettes, plusieurs manettes, reconnexions, clavier, souris et autres périphériques disponibles dans Amiga et Atari.
- [ ] 4. Analyser le CPU des machines masquées
  - [ ] 4.1 Mesurer le coût de zéro à quatre machines
    - [ ] Créer `docs/architecture/emulation-cpu-usage.md` avec des mesures répétées du CPU de GW GUI et des processus d’émulation pour zéro, une, deux, trois et quatre machines, visibles puis masquées, en distinguant la réception vidéo, l’audio, les entrées et les notifications encore actives.
  - [ ] 4.2 Corriger uniquement les traitements inutiles confirmés
    - [ ] Modifier `docs/architecture/emulation-cpu-usage.md` après les corrections de code justifiées par les mesures pour consigner le comportement final et les nouvelles valeurs comparables.
- [ ] 5. Valider les parcours secondaires de distribution indépendante
  - [ ] 5.1 Exécuter les quatre parcours reportés
    - [ ] Modifier `docs/project/testing.md` après installation d’un module depuis un ZIP puis une URL directe, mise à jour d’un module sans republier GW GUI et mise à jour de GW GUI en conservant les modules installés, afin de consigner les versions et résultats réellement observés.

## revision-types-entrees

- [ ] Terminer la qualification des types communs et des touches propres aux machines.
  - [ ] Compléter les décisions avant toute correction supplémentaire.
    - [ ] Modifier `docs/tasks/emulation/commun.md` : qualifier les constantes et enums encore concernés avec leur propriétaire, leurs consommateurs et leur rôle (protocole natif, donnée commune ou touche de machine), puis inscrire les actions concrètes nécessaires ; conserver cet inventaire dans le suivi commun.
    - [ ] Modifier `src/GWGUI.Emulation.Commodore/Common/Machines/Common/Functions/InputFunctions.Settings.cs` et `src/GWGUI.Emulation.Commodore/Common/Machines/Common/Constants/KeyboardConstants.cs` si leur revue confirme encore des touches génériques dans les affectations : limiter les commandes configurables aux touches propres aux machines et conserver le passage ordinaire du clavier dans son propriétaire existant.

Acquis : norme vidéo et fréquence sont séparées dans le SDK ; les quinze nouveaux modules partagent les choix et défauts audio identiques. Sony conserve sa liste de latences différente. Acorn et Apple décrivent leurs touches spéciales explicitement. Le convertisseur couvre les valeurs WPF distinctes du runtime courant, avec cinq exclusions internes/réservées, Windows distinct d’Amiga, F1-F24 et OemPipe distinct d’Oem102 ; le transport 32 bits et les combinaisons mixtes sont contrôlés. Le regroupement des types restant n’est pas déclaré terminé par ces résultats.

## Coleco : options des trois cœurs et démarrage rapide

Comparaison aux sources officielles inspectées : JollyCV 2790d59c7194d29e83f8603ffd7e9da17f11575e, huit clés natives présentes ; GearColeco 3161e3f63722a8ad3c0c7a934e17ef3d72ac93a3, treize clés natives présentes ; blueMSX e3086eb5d36d77fa11704cf53dc176686e70127d, treize clés locales sur quatorze, Sunrise IDE étant hors des matériels Coleco exposés. Les options pilotées par les machines et périphériques restent gérées par leurs configurations existantes.

Le démarrage rapide patch_coleco_rom était rangé dans Vidéo. Il est désormais dans Général, bloc système traduit existant ; les trois options vidéo restent dans Vidéo. Les deux valeurs enabled/disabled, le défaut disabled, la sauvegarde et la transmission native sont conservés. Trente-deux contrôles Coleco réussis, dont deux nouveaux cas ; quarante-quatre contrôles Coleco/Wenquxing passent sur les DLL livrées. Build Debug complet réussi, exécutable, deux copies du SDK et 23 modules vérifiés par empreinte, staging absent. Aucun cœur natif modifié. Les qualifications NC3000 complémentaires restent celles consignées ci-dessous et ne sont pas déclarées réussies par ces contrôles logiciels.
## Réglages globaux des manettes : titre commun

L’affichage commun imposait « Multitap » à tous les réglages globaux, y compris la permutation des ports O2EM et les réglages Opera. Il utilise maintenant le titre fourni par l’appelant, avec Emulation.Input.Behavior en repli. Cette ressource existe une fois dans les trente répertoires de catalogues App, base comprise et deux écritures chinoises incluses ; aucune nouvelle traduction ni copie locale n’est créée. Les variables portent désormais le sens de réglages globaux, sans changer leur placement ni leur transmission.

Deux contrôles WPF vérifient le titre par défaut, le titre explicite et la présence unique du contrôle global avec deux ports ; arbre détaché dans finally, fixture STA et nettoyage global existants conservés. Les 105 contrôles ciblés Interface/EmulationViews, Sony ControllerConfiguration, Philips et Panasonic passent avec les DLL de livraison courantes. La première compilation des tests avait recopié des DLL modules plus anciennes : les empreintes différentes ont été constatées et les DLL de livraison employées avant reprise, sans correction produit supplémentaire. Build Debug complet réussi, application, deux copies du SDK et 23 modules vérifiés par empreinte, staging absent. Aucun nouveau fichier temporaire ni cœur natif lancé pour ce lot. Les limites natives et photographiques de l’objectif restent ouvertes.
## Bandai : quantités numériques WonderSwan

Les 110 années (1920–2029), 31 jours (1–31) et seize seuils de saut d’images (15–60 %, pas 3) sont désormais des plages numériques ; les quatre fréquences audio sont une liste d’entiers en hertz. Les défauts sont nommés et la conversion en kHz réutilise les constantes communes existantes. Les mois nommés et les modes catégoriels restent explicites. Aucun choix natif, ordre, libellé, invariant, visibilité ou comportement de rechargement n’est modifié ; aucune nouvelle traduction.

Quatre cas de régression comparent les listes complètes aux empreintes des valeurs/libellés/invariants relevées avant modification, avec contrôle des défauts, nombres de choix et visibilité. Les 24 contrôles Bandai passent avec la DLL finale livrée. Build Debug des 23 modules réussi, exécutable et deux copies du SDK présents, modules et SDK identiques aux compilations par SHA256, staging absent. Aucun cœur natif ni fichier temporaire lancé/créé pour ce lot. Les étapes terminées sont retirées et ce lot est commité sans push ; l’objectif global et ses limites restent ouverts.
## Images des seize familles : revue des machines

Quatre-vingt-trois PNG examinés dans les seize dossiers Assets/Machines : tous ont une dimension maximale de 512 px et au moins une zone transparente réelle. Les cinq planches couvrent chaque fichier ; contrôle visuel du cadrage et de la lisibilité à taille réduite, sans déclarer ces planches preuve exhaustive de l’identité de chaque photographie. La limite du Spectrum SE reste ouverte et son image actuelle est conservée.

To8D.png était issu d’une vue avec câbles superposés au boîtier et bord droit tronqué. Remplacement par la photographie complète Frédéric Bisson (Retrogaming Days V, 24 avril 2016), Wikimedia Commons, CC BY 2.0. Masque manuel de silhouette à la résolution originale 4912 × 3264, redimensionnement proportionnel et marge transparente : PNG final 512 × 328. Contours et façade examinés en détail, puis résultat sur fonds clair et sombre ; boîtier, clavier, inscriptions et cartouche insérée conservés, aucun élément généré ou reconstruit. Attribution et traitements indiqués dans THIRD-PARTY-NOTICES.md.

Sources candidates, masque, détails, aperçus, relevé et cinq planches supprimés avec tout le dossier temporaire possédé gwgui-machine-artwork-review-20261016 avant livraison. Aucun nouveau test ni script conservé. Les onze contrôles Thomson existants passent avec la DLL livrée. Build Debug complet réussi, exécutable, deux copies du SDK et 23 modules vérifiés par SHA256, staging et dossier temporaire absents. Le lot visuel est commité sans push ; les autres réserves de l’objectif restent ouvertes.
## Matrice des configurations des seize familles

Audit temporaire repris sur les fabriques et interfaces SDK existantes avec les DLL du dernier build : seize familles, 90 modèles et 121 couples machine/cœur déclarés. Les défauts des champs visibles et des périphériques sont valides ; 2001 essais sur les premiers/derniers choix des champs actifs passent après application et sérialisation, avec vérification de RuntimeOptions lorsque l’identifiant de champ correspond directement à la clé native. Seize cas réussis, aucune correction produit nécessaire. La matrice ne teste ni toutes les combinaisons entre champs, ni tous les choix intermédiaires, ni l’effet dans la DLL native.

Fixture TemporaryModuleSettingsReviewTests.cs supprimée et absence de tous ses dossiers temporaires confirmée ; aucun cœur natif chargé. Reconstruction des seuls tests après retrait : dix-huit contrôles permanents ModuleConfigurationReviewTests réussis, zéro échec et aucun avertissement de compilation. Pas de build complet supplémentaire pour cet audit sans modification produit. Bilan à regrouper avec un prochain lot fonctionnel, sans commit documentaire isolé ; les limites ci-dessous restent ouvertes.
## Architecture : contrôles existants des modules

Les 95 contrôles existants sélectionnés passent sur les 23 modules, dont les seize familles demandées : namespaces correspondant aux dossiers, absence de références d’adaptateurs dans Common, catalogues sans recherche de types par namespace, contrats d’adaptateurs sans types ABI, composition Common et création des modules par leurs fabriques. Les références App/SDK aux modules de familles sont également contrôlées. DLL de la livraison courante utilisées ; aucun nouveau test ni infrastructure, aucun cœur natif et aucune correction produit nécessaire. Les temporaires propres aux essais de fabriques sont supprimés par leurs finally existants. Bilan à regrouper avec une prochaine correction fonctionnelle, sans commit documentaire isolé.

Revalidation des réserves externes : HEAD libretro PCEE2 reste 1457dc45b956280bf02563627fd081beb7759e29 et HEAD master WQXEmu reste 05e34f90afe9ef448eb7198e443fbce8031c781c ; aucune évolution officielle constatée justifiant de répéter les essais déjà faits. La recherche Spectrum SE retrouve les mêmes photographies ZXF03/ZXF11 et les références du prototype ; aucun meilleur original qualifié. Les images de YABUS.ZX ou du projet Chloe ultérieur ne sont pas substituées au matériel demandé.
## Localisation : clés des champs exposés

Audit temporaire des seize familles avec les DLL de livraison : 2887 références de ressources recueillies dans les blocs, champs, aides, choix et commandes d’entrée exposés pour les modèles et moteurs déclarés. Toutes se résolvent par le mécanisme module puis App. Les clés ayant une source en-US sont présentes dans les 29 cultures vérifiées ; les invariants conservés uniquement dans la base sont distingués. Zéro clé ou entrée de culture manquante dans ce périmètre. Ce contrôle de présence ne prouve pas à lui seul la justesse linguistique de chaque traduction.

Aucune correction de ressource ni traduction supplémentaire nécessaire. Fixture TemporaryModuleLocalizationReviewTests.cs et dossier temporaire gwgui-module-localization-review-20261016, rapport compris, supprimés. Reconstruction des seuls tests sans avertissement, puis trois contrôles permanents ModuleLocalizationTests réussis sur le mécanisme de repli et les cultures. Aucun cœur natif ni nouveau build complet pour cet audit sans modification produit ; bilan à regrouper avec le prochain lot fonctionnel, sans commit documentaire isolé.
## Wenquxing : correction des codes de navigation

Build complet réussi code zéro, application, deux copies du SDK et 23 DLL présentes et comparées par empreinte, staging absent. Dix-neuf contrôles Wenquxing réussis avec la fixture native ; six nouveaux cas permanents couvrent chacun les cinq modèles, les codes de navigation, les appuis, les relâchements et l’absence de répétition d’événement pour une touche maintenue.

Défaut identifié dans la table de l’adaptateur : les six valeurs standard RETROK ne correspondent pas aux constantes réellement utilisées par crates/wqxemu-libretro/src/lib.rs dans la révision officielle 05e34f90afe9ef448eb7198e443fbce8031c781c. Remplacement par Left=0x250, Up=0x251, Right=0x252, Down=0x253, PageUp=0x254, PageDown=0x255 ; aucune DLL native modifiée. Avec WQXEmu 0.1.0 officiel, SHA256 E9738DCB0D3F50D622363A699674ABA076FF2D0CDFE73262EE5D91F88D1BC933, la même suite Right/Right/Return atteint désormais le choix des joueurs et réagit aux touches. Les captures précédentes restaient dans les menus. Dernier parcours neuf de 6312 appels RunFrame, PID 53700 : choix des joueurs lisible mais aucun plateau ou tour de jeu confirmé. Le lancement effectif reste ouvert. Un parcours restauré dans une nouvelle session a donné un écran blanc après sélection : il est exclu de la qualification du jeu ; les parcours suivants repartent d’un démarrage neuf. Processus terminés après Dispose, mappings absents et empreintes des trois firmwares originaux inchangées à chaque essai.

Après observation, suppression de TemporaryNc3000SelectionTests.cs et du dossier possédé gwgui-nc3000-selection-20261016 avec la DLL, l’archive, les captures, l’état et les sessions. Reconstruction des seuls tests sans avertissement ni erreur, puis dix-huit contrôles permanents Wenquxing réussis. Aucun nouveau script, média, cœur ou fichier temporaire conservé dans le dépôt ; aucun nouveau texte utilisateur ni traduction nécessaire. La correction des six codes est livrée avec son test permanent, sans déclarer les jeux ni les limites audio résolus.
## Wenquxing : matrice native NC3000

Divergence identifiée après le correctif de codes hôtes : dans [WQXEmu keyboard.rs, révision 05e34f90afe9ef448eb7198e443fbce8031c781c](https://github.com/AloysHF/WQXEmu/blob/05e34f90afe9ef448eb7198e443fbce8031c781c/crates/wqxemu-core/src/keyboard.rs), LAYOUT_NC3000 place O, L, UP, DN, P, ENT, PGDN et RT aux lignes 0 à 7 de la colonne 3. La référence citée par l’auteur, [NC2000 key_new.cpp, révision 60d3082147a1f7ce75419fc71bc01f20f00c1cd1](https://github.com/wangyu-/NC2000/blob/60d3082147a1f7ce75419fc71bc01f20f00c1cd1/key_new.cpp), les place aux mêmes lignes en colonne 1 dans items3000_col1 ; la table legacy de key.cpp est explicitement inactive et n’est pas retenue pour cette comparaison.

Aucune touche LAYOUT_NC3000 ne cible la colonne 1. Dans la source WQXEmu inspectée, clavier et RetroPad convergent vers key_id_for_host_key puis FrontendInputState.sync et Machine.set_key ; nc3000.rs interprète directement ligne=key_id>>3, colonne=key_id&7, avec une exception pour Power seulement. Les deux sources d’entrée ne fournissent donc pas une seconde table permettant de sélectionner cette colonne. Cela constitue une piste native précise pour les validations NC3000 non concluantes, sans prouver à lui seul quelle révision exacte a produit chaque instruction du binaire distribué ni requalifier une partie comme réussie. Ne pas remapper Entrée vers une autre lettre dans GW GUI pour masquer cette divergence ; une qualification/correction du cœur est nécessaire avant de revendiquer les commandes de jeu correctes.

Aucun cœur, adaptateur ou firmware modifié pour cette comparaison ; aucun téléchargement conservé sur disque, aucune fixture, aucun processus natif et aucun nouveau build. Le dernier build corrigé et les dix-huit contrôles permanents restent ceux du commit a775830af. Bilan à regrouper avec un prochain lot fonctionnel, sans commit documentaire isolé.
## Revue des visuels de périphériques des familles demandées

Revue antérieure à la restauration frontale Intellivision/PSP ci-dessous : quarante-trois PNG propres aux familles demandées examinés sur huit planches avec superposition des rectangles des zones déclarées dans leurs profils. Les profils réutilisés d’autres marques et les contrôles masqués ne sont pas déclarés qualifiés par cet inventaire. Trois décalages confirmés sur sony-negcon.png : Start et les commandes I/R2, II/L2 se trouvaient sous leurs boutons ; leurs trois zones sont recentrées sur la photographie existante dans SonyNeGcon.cs. Superposition finale des halos examinée à la taille native, sans changer photo, commandes du module ou affectations physiques. La revue des planches ne constitue pas une certification exhaustive de chaque détail photographique.

Build Debug complet réussi ; application, DLL App, deux copies du SDK et 23 modules vérifiés par empreinte, staging absent. Les 25 contrôles permanents Sony ControllerConfigurationTests et ControllerArtworkCatalogTests passent ; aucun nouveau test ni cœur natif exécuté. Dossier possédé gwgui-family-controller-review-20261016 supprimé avec inventaire, 43 superpositions, huit planches et aperçu final. Aucun fichier de travail conservé dans le dépôt. Livraison : F:/GW GUI/build/Debug/GW GUI/gwgui.exe. Les limites Spectrum SE, PCEE2 et Wenquxing ci-dessous restent ouvertes.
## Vues de face Intellivision et PSP restaurées

À la demande utilisateur, restauration exacte du PNG Intellivision antérieur à e27766e4e et du PNG PSP blanc de face antérieur à d37d587ae. Identité des deux fichiers vérifiée avec les blobs Git ; aucun redimensionnement, détourage, changement de couleur ou montage supplémentaire. Intellivision conserve son cadrage original 419 × 659 ; PSP conserve son cadrage original 512 × 224.

Vingt zones Intellivision recalées sur la vue unique : douze touches, quatre directions, deux boutons supérieurs communs et les deux boutons inférieurs distincts. Les seize zones PSP reprennent le profil de la vue blanche frontale. Superpositions examinées sur les deux images restaurées ; commandes des modules et affectations physiques conservées. Crédits des photographies désormais retirées supprimés de THIRD-PARTY-NOTICES.md ; anciens bilans concernés corrigés.

Dossier temporaire possédé gwgui-front-controller-restoration-20261016 supprimé avec les deux aperçus. Aucun nouveau test ou script conservé ; contrôle visuel et comparaison exacte des PNG, sans lancer de test, de cœur natif ou de build pour cette correction des sources App. La livraison compilée précédente ne contient donc pas encore cette restauration. Lot regroupé en un commit, sans push.
## Modules demandés : limites encore ouvertes

- [ ] Remplacer la photographie du Spectrum SE par une meilleure photo authentique.
  - [ ] Modifier `src/GWGUI.Emulation.Sinclair/Assets/Machines/SpectrumSe.png` : utiliser une photographie exploitable du prototype SE, complète et identifiable, détourée classiquement à 512 px ; conserver la photographie actuelle tant qu’aucun meilleur candidat n’est trouvé.
  - [ ] Modifier `THIRD-PARTY-NOTICES.md` et `docs/tasks/emulation/commun.md` : attribuer la nouvelle source et relever le contrôle du cadrage et de la transparence après remplacement.
- [ ] Résoudre ou requalifier le parcours OpenGL PCEE2 à partir d’un nouvel élément probant.
  - [ ] Modifier `docs/tasks/emulation/commun.md` : relever toute évolution officielle de `GLContextWGL.cpp` ou du binaire, puis le résultat d’un essai justifié par cette évolution ; conserver OpenGL non qualifié tant que le renderer demandé n’est pas réellement ouvert et que le partage du contexte échoue.
- [ ] Compléter les interactions Wenquxing encore non qualifiées.
  - [ ] Modifier `docs/tasks/emulation/commun.md` : qualifier le lancement effectif des jeux NC3000 et la production PCM des NC1020/PC1000 ; conserver les limites natives audio NC2000/NC3000 établies ci-dessous ; garder les originaux de `F:/Retro/ROMS-BOOT/Wenquxing` intacts. La navigation NC2000/NC3000 est consignée ci-dessous.

### Limites à conserver lors de la reprise

| Point | Preuve ou limite connue | Conséquence pour la suite |
| --- | --- | --- |
| Spectrum SE | Photo authentique ZX Shed issue 2, source 2048 × 653 déjà tronquée en haut. ZXF03/11, ZXShed02, Spectrumpedia, ZX Notizie, archives zxse et autres pistes détaillées dans l’historique n’ont pas fourni de meilleure photo. | Continuer avec une nouvelle piste, sans remplacer par un Spectrum 48K, un Timex de série ou un logo. |
| PCEE2 OpenGL | Contexte partagé WGL refusé lorsque le contexte principal est actif sur un autre thread ; le détachement expérimental ne rend pas le nouveau contexte GS courant et aboutit à un échec GLAD. Source examinée : `1457dc45b956280bf02563627fd081beb7759e29`, WGL inchangé. | Les essais de format de pixels et bootstrap déjà rejetés ne sont pas à répéter sans changement. Vulkan et logiciel sont qualifiés séparément. Aucun cœur natif modifié ni recompilé. |
| Play! | `deinit` peut rester bloqué ; le host est récupéré après son délai d’arrêt, sans prétendre à une sortie native normale. | Conserver cette limite ; un nouveau binaire ou une nouvelle preuve est nécessaire pour requalifier la sortie normale. |
| Wenquxing | Les cinq modèles démarrent avec LCD 160 × 80, états et sauvegardes. Les essais prolongés NC2000/NC3000 atteignent les applications ; les touches NC3000 ont été corrigées. GAME et le son gardent les réserves détaillées ci-dessous. PCM non nul observé uniquement sur CC800 ; pas d’écoute physique qualifiée pour les cinq modèles. | Distinguer démarrage, navigation, génération du PCM et écoute. Les douze dumps restent dans `F:/Retro/ROMS-BOOT/Wenquxing`. |
| Validation physique des entrées | Les axes, demi-axes, pressions et combinaisons mixtes sont couverts par des scénarios logiciels ; les actions de code Dreamcast Fishing/Maracas et de nettoyage graphique de l’ancienne fiche analogique étaient cochées. | Les essais matériels restent dans le groupe GameInput ci-dessus : commandes, plusieurs appareils, reconnexions et types de périphériques. La suppression de la fiche ne les déclare pas réussis. |

### Périmètre conservé

Uniquement les DLL du catalogue ; Acorn conserve b2. BBC Model A, Macintosh 128K/512K/Plus/Classic, ZX80 et X68030 restent identifiés sans compatibilité attribuée à une DLL qui ne les expose pas. Pippin est dans Apple, avec visuels filaire et infrarouge conservés, sans cœur du catalogue qualifié. Les modèles Sony sans DLL qualifiée restent également sans moteur fictif.

Les qualifications natives détaillées déjà consignées pour les autres familles, les supports de `F:/Retro`, les contrôles de nettoyage et les corrections livrées restent consultables dans l’historique du suivi. Le nettoyage documentaire ne les répète pas et ne ferme aucune limitation non résolue. Pour les retouches de photos, traitement classique uniquement ; fichiers de travail dans le temporaire Windows, supprimés après usage.

### LRPS2 : comparaison et correction livrées

LRPS2 : source officielle libretro/ps2 master 3660a1fe8b21ef2a8227a05405d1de0058590f29, libretro/libretro_core_options.h. Comparaison en mémoire : 78 clés natives, 78 constantes locales et 78 déclarations Define dans les cinq catalogues partiels ; aucune clé absente et aucune constante orpheline. Le point historique des identifiants manquants est donc clos. Cette comparaison des clés ne qualifie pas chaque comportement natif ni les conditions de compilation du binaire distribué.
Retrait de LegacyValues et de ses deux chemins de conversion : seules les valeurs natives courantes sont présentées/transmises, sans migration des anciens libellés de rendu et de résolution. Liaison vidéo commune, nombres natifs et configuration HDD conservés. Contrôles Sony : 390 réussites, zéro échec ; quatre résolutions natives avec chacun des deux modes logiciels, configuration source conservée, et autres scénarios Sony existants. Aucun nouveau test externe ni fichier temporaire, pas de nouveau texte à traduire. Le nettoyage documentaire est regroupé avec cette correction fonctionnelle.

Livraison LRPS2 vérifiée : build complet terminé code zéro, application présente, SDK (lib et Updater) et 23 modules identiques par SHA256 aux compilations ; staging absent. Sept fichiers de suivi conservés, catalogue et méthode inchangés ; les étapes cochées de ce lot sont retirées du suivi. Aucun nouveau fichier temporaire ni test externe. Un seul commit local sans push pour le correctif et le nettoyage, sans déclarer les limites encore ouvertes résolues.

### Wenquxing : correction des fonctions NC3000

Disposition officielle examinée : AloysHF/WQXEmu, révision 05e34f90afe9ef448eb7198e443fbce8031c781c, docs/Keyboard-Controls.md et crates/wqxemu-core/src/keyboard.rs. NC3000 utilise GAME/F5, CALC/F6, TIME/F7, DICT/F9, LEX/F10 et STUDY/F11. Les quatre autres modèles conservent leurs sept fonctions. Les trois nouvelles identités SDK sont 259–261 ; aucun numéro existant ne change. Neuf associations spéciales sont proposées sur NC3000, dix sur les autres modèles. Les légendes du matériel restent invariantes.

Défaut reproduit puis corrigé dans l’adaptateur : CALC ouvrait le calendrier NC3000. Après correction, les LCD montrent une calculatrice pour CALC, le calendrier pour TIME et trois écrans de dictionnaires pour DICT, LEX et STUDY. GAME permet d’atteindre le menu des jeux par deux appuis successifs ; le lancement effectif d’un jeu reste non qualifié. NC2000 atteint son menu après 3600 appels RunFrame ; ses trois touches essayées ouvrent la calculatrice, le calendrier et le dictionnaire. Les anciennes observations après 17 secondes ne constituent donc pas un blocage persistant du démarrage.

Essais avec le lanceur livré et WQXEmu 0.1.0 officiel, SHA256 E9738DCB0D3F50D622363A699674ABA076FF2D0CDFE73262EE5D91F88D1BC933. Dernier parcours : NC3000 PID 55308, 4356 appels RunFrame ; NC2000 PID 46652, 3978 appels. Deux scénarios réussis, sorties zéro, mappings vidéo absents après fermeture et SHA256 des firmwares originaux inchangés. Aucun cœur modifié ni recompilé. Zéro bloc PCM reçu sur ces parcours : génération audio et écoute restent non qualifiées sur ces deux modèles.

Contrôles permanents après suppression de la fixture externe : 164 réussites, zéro échec. Build complet terminé code zéro ; application, SDK dans lib et Updater, et 23 modules vérifiés identiques par SHA256 aux compilations, staging absent. Fixture native, captures, DLL téléchargée, archive et dossier temporaire supprimés après usage ; originaux de F:/Retro conservés.
### Couverture déclarative revue le 16 octobre 2026

Cinquante fabriques concrètes retrouvées sous Emulators : Acorn 1, Apple 2, Bandai 3, Coleco 3, Epoch 2, Fairchild 1, Philips 3, Mattel 1, Sharp 2, Sinclair 2, SNK 16, Thomson 1, Watara 1, Wenquxing 1, Panasonic 1, Sony 10. Les noms de DLL des constantes correspondent aux moteurs attendus ; Sony possède les deux déclarations particulières PPSSPP et PCSX2 dans ExternalCoreConstants, et Pokket conserve son identifiant installé documenté. Entrées croisées prises en compte : blueMSX/Coleco, Opera/3DO, Potator/Watara, Théodore/Thomson, WQXEmu/Wenquxing et moteurs arcade SNK. Les modèles explicitement demandés figurent dans les catalogues ; les limites natives de Model A, Macintosh classiques, ZX80, X68030 et Pippin restent celles relevées ci-dessus. Cette comparaison du code ne remplace pas une qualification native ni un contrôle de chaque option.

Reprise des deux pistes externes : HEAD officiel WizzardSK/pcee2-libretro, branche libretro, toujours 1457dc45b956280bf02563627fd081beb7759e29 ; comparaison GitHub : zéro commit supplémentaire, aucun nouvel essai WGL justifié. L’entretien Spectrum SE de Subliminal Extacy 03 ne fournit que /screens/1/1021.webp, couverture de la revue, sans nouvelle photo du prototype. Le PNG SE actuel est conservé.

NC3000 GAME, essai ciblé : le premier appui F5 affiche les fiches, le second ouvre le menu de six jeux (dont labyrinthe, Huarong Dao et démineur). Le menu des jeux est donc accessible avec le mappage livré ; aucune correction supplémentaire appliquée. Le lancement effectif d’un jeu n’est pas déduit de ce menu. Parcours de 4716 appels RunFrame, PID 53472, même WQXEmu 0.1.0 et SHA256 que ci-dessus ; sortie zéro, mapping vidéo absent et SHA256 des trois firmwares originaux inchangés. Zéro bloc PCM reçu. Fixture native compilée sans reconstruire les références du projet de tests, puis supprimée avec sa DLL téléchargée, son archive et ses captures. Après retrait, reconstruction des seuls tests et douze contrôles permanents Wenquxing réussis, zéro échec.

### Sony : confirmation de la terminaison des hôtes PCSX2 et PPSSPP

Revue des propriétaires : dans les quinze modules demandés, les méthodes DisposeAsync de Common/Services/Machine.Commands.cs et Dispose de Common/Services/ProcessCore.Protocol.cs ont le même enchaînement de libération après normalisation des espaces. Le thread est attendu par StopAsync ; les CoreHost libèrent leurs flux et mappings avec using et leur cœur dans finally. Sony dispose de trois hôtes distincts : Common/Interop, Pcsx2 et Ppsspp. Les contextes graphiques Sony examinés conservent leurs finally de destruction ; cette revue ne mesure pas la mémoire DWM.

Défaut confirmé et corrigé dans src/GWGUI.Emulation.Sony/Emulators/Pcsx2/Services/ProcessCore.cs et Emulators/Ppsspp/Services/ProcessCore.cs : ReleaseHost et TerminateProcess ignoraient le résultat de l’attente de cinq secondes après Kill, puis masquaient toutes les erreurs. Les deux propriétaires attendent désormais la terminaison effective ; seule la course où le processus est déjà sorti est ignorée. Handles, tampons, entrées et états restent libérés dans finally, même si l’observation de sortie échoue. Le délai initial de cinq secondes est conservé et nommé dans leurs Constants/ProcessCoreConstants.cs ; aucune DLL de cœur modifiée.

Validation : 175 contrôles préexistants repris, puis 183 réussites avec les quatre cas de régression permanents et quatre essais de processus possédés. Enfants observés quittés avant retour : PCSX2 Dispose PID 52036, terminaison PID 53924 ; PPSSPP Dispose PID 30224, terminaison PID 46392. Aucun cœur natif ni fenêtre graphique chargé par ces quatre essais. Fixture TemporaryProcessExitTests.cs supprimée ; les seuls tests ont ensuite été reconstruits, 179 contrôles permanents réussis, zéro échec. Aucun nouvel artefact temporaire conservé. Ces résultats ne qualifient ni un démarrage de jeu supplémentaire ni la baisse de mémoire DWM.

Livraison vérifiée après correction : build complet terminé code zéro ; gwgui.exe présent, SDK dans lib et Updater et 23 modules identiques par SHA256 aux sorties de compilation. Staging et fixture temporaire absents. Aucun nouveau texte utilisateur, aucune migration et aucun nouveau fichier de travail conservé. Les étapes terminées de cette revue sont retirées du suivi ; les limites natives encore ouvertes restent inchangées.
### Wenquxing : limites audio identifiées dans la source officielle
Qualification NC1020/PC1000 : 6096 appels RunFrame par modèle avec WQXEmu 0.1.0, même SHA256 E9738DCB0D3F50D622363A699674ABA076FF2D0CDFE73262EE5D91F88D1BC933. Les LCD montrent calculatrice et dictionnaire, saisie de 12 confirmée sur PC1000. NC1020 : aucun bloc PCM reçu. PC1000 : 6096 blocs, 4480560 trames annoncées à 44100 Hz, tous les échantillons nuls. La réception du callback PC1000 est vérifiée ; ni ces menus ni les touches F1–F4 n’ont produit de signal non nul, ce qui ne prouve pas l’absence de son dans tous les logiciels. Pas d’écoute physique validée. Hôtes PID 52204 et 37296 terminés, mappings vidéo absents et firmwares originaux inchangés par SHA256. Fixture, DLL, archive, sessions et captures supprimées avec gwgui-wqx-audio-20261016 ; reconstruction des seuls tests puis douze contrôles permanents réussis. Aucun code produit modifié et aucun build complet supplémentaire nécessaire. Bilan à regrouper avec le prochain lot fonctionnel, sans commit documentaire isolé.

Révision AloysHF/WQXEmu 05e34f90afe9ef448eb7198e443fbce8031c781c, toujours HEAD master au contrôle. Dans crates/wqxemu-core/src/machines/nc2000.rs et nc3000.rs, Audio est construit, réinitialisé, sauvegardé et vidé, sans aucun appel qui alimente son tampon ; les accès son simulent la consommation matérielle. Le trait Machine ne génère pas d’audio commun en dehors des machines. Cela explique le PCM absent des deux parcours natifs précédents : la sortie sonore de ces deux modèles n’est pas implémentée dans cette source officielle. Refaire les mêmes essais de menus ne la produira pas.

NC1020 alimente Audio via les commandes des registres 0x20, 0x22 et 0x23 ; PC1000 et CC800 appellent output_frame pour leur bip. Autre limite native confirmée : audio.rs produit un vecteur mono (un échantillon par instant), drain_audio le conserve tel quel, mais crates/wqxemu-libretro/src/lib.rs le passe directement à audio_batch_cb avec len / 2 comme nombre de trames stéréo. Le frontend interprète donc les échantillons consécutifs comme gauche/droite, sans conversion mono vers stéréo et avec une durée divisée par deux à fréquence annoncée inchangée. Ce constat de source ne qualifie pas l’écoute physique.

L’adaptateur actuel installe les callbacks sample/batch, applique la fréquence AV et recopie les deux échantillons annoncés par trame conformément au contrat Libretro. Aucun correctif de DLL native ni compensation dépendant de ce défaut n’est introduit dans GW GUI. Les prochaines qualifications utiles restent les commandes son NC1020/PC1000 et les jeux ; les limites NC2000/NC3000 et le format natif sont identifiés, sans les déclarer corrigés. Aucun nouveau test, téléchargement sur disque, artefact ou build pour cette consultation de source ; ce suivi sera regroupé avec une prochaine correction cohérente.

### Quantités numériques des options Sony

Les quantités natives examinées sont des listes d’entiers en lecture seule, conservées dans les constantes de leur adaptateur. PCEE2 : latence, échelle et zone morte des axes, facteur anisotrope ; Beetle PSX HW : erreur de phase et budgets RAM/VRAM du cache ; RPCS3 : échelle des fréquences, délai de réveil, nombre de threads LLVM, nombre maximal de préemptions et délai de récupération. Les deux variantes Beetle partagent leurs nombres de cycles et d’échantillons SPU dans OptionNumericConstants, avec l’ordre natif 2 puis 1 conservé pour les cycles.

Quatorze fichiers d’enums de quantités et leurs conversions inutiles sont supprimés. Les choix numériques génèrent les libellés invariants ; le formatage commun des budgets mémoire conserve les anciens affichages MB/GB, dont 1 GB et 1.5 GB. Les libellés traduits particuliers (automatique, désactivé, valeurs par défaut, degrés et secondes) emploient les mêmes ressources. Aucun nouveau texte utilisateur ni migration ; identifiants, ordre, valeurs natives, défauts et contraintes des seize options conservés. Les modes, formats et autres catégories gardent leurs types ; cette correction ne déclare pas la revue de tous les types du dépôt terminée.

Contrôles : seize cas vérifient les valeurs, défauts, libellés et ordre exacts ; l’ensemble Sony atteint 410 réussites, zéro échec. Compilation du module réussie sans avertissement ; aucune référence aux enums et constantes d’affichage supprimés. Aucun nouveau test externe, téléchargement ou artefact temporaire pour ce lot.

Livraison regroupée vérifiée : build complet terminé code zéro ; application présente, SDK dans lib et Updater et 23 modules identiques par SHA256 aux compilations, staging absent. Les étapes terminées sont retirées du suivi. Ce regroupement complète le même commit local de quantités Sony, sans nouvelle publication ni push ; les autres limites du suivi restent ouvertes.

Qualification NC3000 complémentaire : trois parcours avec le même binaire officiel ont essayé la validation du menu et ses touches de fonction (Entrée, chiffres 0/1, Espace, F1/F2 et directions). Les LCD 160 × 80 montrent toujours des écrans de sélection de jeux ; aucun démarrage de jeu n’est prouvé. Le mappage local Entrée transmet 13, conforme à RETROK_RETURN et HostKey::Return/ENT dans la source officielle inspectée ; aucune preuve ne justifie de remplacer ce code dans l’adaptateur. La cause de ce comportement n’est pas attribuée sans preuve au firmware ou au binaire. Aucun PCM reçu. Hôtes PID 43048, 35324 et 49872 observés terminés, mappings vidéo absents après Dispose et SHA256 des trois firmwares originaux inchangés. Fixture externe et dossier temporaire gwgui-nc3000-game-20261016 supprimés ; reconstruction des seuls tests puis douze contrôles permanents Wenquxing réussis. Ce complément de qualification est regroupé avec le correctif fonctionnel Coleco, sans commit documentaire isolé.
