# Travaux communs restants

## Lecteur Data Pack d'origine ADAM

- [x] Rendre cohérente la permanence du lecteur intégré, sans build.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/Common/Machines/Common/Functions/StorageFunctions.Settings.cs` : déclarer le premier lecteur Data Pack permanent, garder le second lecteur et les disquettes ajoutables et supprimables.
  - [x] Modifier `docs/tasks/emulation/commun.md` : consigner le raccordement de la permanence à la corbeille et les commandes réellement disponibles pour les lecteurs, sans compilation.

Le premier Data Pack était requis par DefaultSlots et la validation, mais son EmulationMediaDevice ne déclarait pas IsPermanent. La déclaration est corrigée pour le seul index zéro. Le contrôleur commun calcule CanRemove avec !IsPermanent, la liste désactive la corbeille avec CanRemove et RemoveDevice refuse aussi la suppression d'un périphérique permanent. Le second Data Pack et les lecteurs de disquettes restent amovibles. Vérification du code et du diff uniquement ; aucun build ni test compilé.

Options natives des lecteurs GearColeco : choix du lecteur ciblé par Disk Control, insertion/éjection/changement d'image et protection en écriture ou copie de travail dans le dossier de sauvegarde. L'adaptateur sélectionne automatiquement le lecteur ciblé et traduit IsReadOnly en mode d'écriture ; le mélange de médias protégés et inscriptibles est rejeté, car cette option native est globale. Aucun réglage natif de vitesse ou de géométrie du lecteur déclaré. Formats documentés : Data Pack 256 KiB, disquettes 160/320 KiB. Les dialogues matériels de ces lecteurs ne sont pas exposés actuellement, à la différence de la cartouche.

## Visuels et périphériques Thomson, sans build

- [x] Compléter les noms de visuels communs.
  - [x] Modifier `src/GWGUI.App/Resources/00-Base/Emulation/EmulationController.resx` : ajouter les huit noms de produits invariants manquants (Watara, Thomson, SNK, Sharp et Mattel), sans les dupliquer dans les langues.
- [x] Rétablir les périphériques de stockage Thomson avant le choix du moteur.
  - [x] Modifier `src/GWGUI.Emulation.Thomson/Common/Machines/Common/Functions/StorageFunctions.Settings.cs` : décrire les périphériques du moteur disponible lorsque le brouillon ne possède pas encore de moteur, sans changer le moteur choisi ni les capacités natives.
- [x] Identifier le crayon optique dans l'onglet Souris et permettre de désactiver son entrée.
  - [x] Modifier `src/GWGUI.Emulation.Thomson/Common/Machines/Common/Constants/SettingsConstants.cs`, `Functions/SettingsFunctions.cs` et `Functions/InputFunctions.Settings.cs` : nommer le crayon, exposer son activation et son contact, retirer le clic droit sans effet de la liste affichée.
  - [x] Modifier `src/GWGUI.Emulation.Thomson/Emulators/Theodore/Interop/Services/InputCallbackHost.cs` et `ExternalCore.cs` : transmettre l'activation et neutraliser les déplacements et le contact quand elle est désactivée, sans consommer un port joystick.
  - [x] Modifier `src/GWGUI.Emulation.Thomson/Resources/*/Emulation.resx` : ajouter les noms du crayon et de son contact dans la base et toutes les langues avec Argos, puis relire les résultats.
- [x] Consigner le contrôle des périphériques de pointage.
  - [x] Modifier `docs/tasks/emulation/commun.md` : préciser le raccordement réel du crayon optique, du clic gauche et des deux joysticks, ainsi que les limites constatées et les vérifications statiques sans build.

Les huit noms invariants manquants sont ajoutés dans la seule base App. Audit statique : tous les profils enregistrés possèdent désormais un nom dans cette base. Stockage Thomson : le brouillon Core=None ne déclarait aucun périphérique, ce qui masquait aussi le bouton d'ajout. La description utilise maintenant les capacités du moteur disponible pour la machine, sans sélectionner ni installer ce moteur ; les trois possibilités natives existantes (cassette, disquette, cartouche) redeviennent ajoutables.

Pointage : le code natif Theodore src/libretro.c lit deux joysticks et un RETRO_DEVICE_POINTER sur le port 2 pour le crayon optique, sans entrée RETRO_DEVICE_MOUSE. L'adaptateur transmettait déjà les coordonnées et le clic gauche à ce crayon. Le bloc de l'onglet Souris le nomme maintenant explicitement et permet de désactiver ses déplacements et son contact ; il reste activé par défaut. L'association du contact accepte toujours clavier, souris et manette. Le clic droit sans effet n'est plus proposé comme action de sortie ; il reste utilisable comme source pour le contact du crayon. Aucun port joystick supplémentaire ni fausse souris Thomson ajouté. Les vraies souris et le lecteur de code-barres documentés pour le matériel ne sont pas exposés par cette interface native.

Sources consultées : https://raw.githubusercontent.com/Zlika/theodore/master/src/libretro.c et https://dcmoto.pages-perso.free.fr/documentation/faq/connecteurs.html. Argos exécuté pour les deux nouveaux libellés, puis correction terminologique dans les 28 langues. Validation XML et présence unique des deux clés dans les 30 catalogues Thomson réussies. Contrôle du raccordement de l'activation au callback, des noms de visuels et du diff uniquement ; aucun build, aucun test compilé, aucune validation à l'exécution.

## Aides du saut d'images et des cartouches, sans build

- [x] Corriger les deux réglages et leurs traductions.
  - [x] Modifier `src/GWGUI.Emulation.Bandai/Emulators/BeetleWonderSwan/Functions/SettingsFunctions.cs` et `Common/Machines/Common/Constants/SettingsTextConstants.cs` : montrer le seuil uniquement en mode manuel, actualiser après changement du mode et fournir la clé d'aide détaillée.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/Emulators/GearColeco/Functions/SettingsFunctions.cs`, `Common/Machines/Common/Constants/SettingsTextConstants.cs` et `src/GWGUI.App/Views/Dialogs/Emulation/Storage/CartridgeSlotConfigurationDialog.cs` : fournir et afficher les deux niveaux d'aide du type de cartouche via le composant existant.
  - [x] Modifier `src/GWGUI.Emulation.Bandai/Resources/*/Emulation.resx` : traduire le libellé Seuil de déclenchement du saut d'images avec Argos dans toutes les langues.
  - [x] Modifier `src/GWGUI.Emulation.Bandai/Resources/*/Help.resx` : traduire les aides courte et détaillée du seuil avec Argos dans toutes les langues.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/Resources/*/Help.resx` : traduire les aides courte et détaillée des circuits de cartouche et du choix Automatique avec Argos dans toutes les langues.
  - [x] Modifier les cinq entrées concernées dans `src/GWGUI.Emulation.Bandai/Resources/*/{Emulation,Help}.resx` et `src/GWGUI.Emulation.Coleco/Resources/*/Help.resx` : corriger après Argos les erreurs de terminologie et les phrases perdues dans toutes les langues, sans modifier les autres ressources.
- [x] Terminer sans compilation.
  - [x] Modifier `docs/tasks/emulation/commun.md` : consigner la validation XML, la présence des cinq textes dans toutes les cultures, les raccordements d'aide et l'absence de build demandé par l'utilisateur.

Résultat : libellé du seuil de saut d'images corrigé ; champ visible uniquement avec le mode manuel et description reconstruite après changement de mode. Aides courte et détaillée raccordées au seuil vidéo et au type de cartouche GearColeco ; le dialogue cartouche transmet désormais aussi l'aide détaillée au composant existant, condition nécessaire à son icône (i). Les circuits natifs, leurs valeurs et le choix Automatique sont inchangés.

Argos utilisé pour les cinq textes, puis relecture et corrections dans les 28 traductions, la base et l'anglais. Contrôle statique réussi : 90 fichiers RESX valides et 150 entrées attendues présentes, sans autre clé modifiée ni supprimée ; diff sans erreur. Les deux niveaux d'aide ont été suivis jusqu'aux libellés de l'interface. Aucun build, aucune compilation de tests ni exécution de l'application ; l'exécutable existant ne contient pas encore ce lot, conformément à la demande utilisateur.
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

## Révision des réglages et cartes mémoire Sony demandée le 16 octobre

- [x] Visuel PocketStation et sélecteurs des manettes.
  - [x] Modifier les fichiers Sony de `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/` et les ressources du visuel : raccorder une vue de face et les cinq zones PocketStation.
  - [x] Modifier `src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationControllerSettingsLayout.cs` et le modèle des sélecteurs : rapprocher les légendes, laisser la place aux noms et aux visuels.
- [x] Valeurs et disposition des réglages Sony.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Functions/CoreSettingsFunctions.cs`, ses constantes et les métadonnées d'options concernées : respecter les valeurs natives, afficher unités et multiplicateurs, réunir les champs ROM et classer les champs vidéo.
  - [x] Modifier les contrats de champs et les composants de réglages existants dans `src/GWGUI.Emulation/Contracts/` et `src/GWGUI.App/Views/Controls/Emulation/Options/ModuleSettings/` : boutons radio pour deux valeurs exclusives, graduations et orientation verticale des bornes NTSC/PAL, mode automatique de fréquence avec curseur désactivé.
  - [x] Modifier les fonctions de disposition de réglages sous `src/GWGUI.App/Functions/Views/Emulation/Settings/` : cadres lisibles, champs dimensionnés selon leur contenu et bornes NTSC/PAL regroupées.
- [x] Cartes mémoire et dialogues de stockage.
  - [x] Modifier `src/GWGUI.App/Views/Dialogs/Emulation/Storage/CompactDiscDriveConfigurationDialog.cs`, `HardDiskDriveConfigurationDialog.cs` et les fonctions de présentation sous `src/GWGUI.App/Functions/Views/Emulation/Storage/` : informations fixes en texte, champs compacts, destination entière, nom de carte sans extension.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/StorageFunctions.cs` et le contrôleur de stockage App : conserver les cartes associées aux ports temporairement masqués et utiliser le dossier propre à la machine.
  - [x] Modifier les commandes de supports en exécution et leur raccordement aux dialogues existants : choisir ou créer plusieurs cartes puis les changer depuis Émulation, selon les capacités du cœur.
- [x] Compléter la commande multitap et les unités.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Functions/CoreSettingsFunctions.cs` et `Modules/SonyEmulationModule.cs` : présenter le multitap SwanStation par deux cases pour les ports et recomposer sa valeur native.
  - [x] Modifier les métadonnées et les composants de champs déjà concernés : grouper aussi les bornes NTSC sans suffixe et afficher les multiplicateurs des lecteurs CD.
- [x] Traductions des réglages Sony signalés et des aides liées.
  - [x] Modifier les catalogues concernés de `src/GWGUI.Emulation.Sony/Resources/` et `src/GWGUI.App/Resources/` : corriger avec Argos puis relire les libellés, valeurs et aides dans toutes les langues ; conserver les noms invariants dans la base.
- [x] Terminer les valeurs présentées par les curseurs Sony.
  - [x] Modifier `src/GWGUI.Emulation/Contracts/EmulationSettingsField.cs`, `CoreSettingsFunctions.cs` et les deux composants App des curseurs : afficher les dimensions de référence du curseur de résolution et le cas instantané de la recherche CD, sans changer les valeurs natives.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Resources/*/Option.resx` : traduire le libellé du délai nul avec Argos dans toutes les langues.
- [x] Séparer les fonctions de rendu et préciser les dimensions.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Functions/CoreSettingsFunctions.cs` et `Resources/00-Base/Option.resx` : répartir les réglages de couleurs et HDR dans leurs cadres, garder le matériel dans Affichage.
  - [ ] Modifier `src/GWGUI.Emulation.Sony/Resources/*/Option.resx` : traduire le titre du cadre des couleurs, les valeurs HDR des captures et préciser la référence 320 × 240 des dimensions avec Argos dans toutes les langues.
- [x] Vérifications sans build.
  - [x] Modifier `docs/tasks/emulation/commun.md` : consigner les valeurs vérifiées dans les sources natives, les audits de traductions et de raccordement, et les limites de validation sans compilation.

Résultat dans les sources : PocketStation utilise la photo de face existante, copiée sans transformation (SHA256 identique), avec quatre directions et le bouton A. Le profil, son identifiant, le catalogue et les noms invariants sont raccordés. Sélecteurs de manette rapprochés de leurs légendes, noms accompagnés d'une infobulle, libellés des formulaires repliables sur plusieurs lignes.

Deux valeurs exclusives : boutons radio, avec restitution de leur identifiant natif et prise en compte des règles et des totaux mémoire. Les états activé/désactivé restent des cases. Le multitap SwanStation présente deux cases et recompose Disabled, Port1Only, Port2Only ou BothPorts sans transmettre les identifiants de présentation au cœur. Les options ROM rejoignent le bloc firmware. Affichage conserve la puce vidéo, la résolution, le format et le recadrage ; Rendu est divisé en cadres de rendu, couleurs, HDR, géométrie, textures et lignes NTSC/PAL selon les options présentes.

Valeurs natives vérifiées : Beetle propose 128, 256, 384, puis chaque multiple de 128 jusqu'à 2048. Le curseur respecte déjà le pas et affiche maintenant ses graduations. PCSX-ReARMed propose auto puis chaque entier de 30 à 100 : il s'agit du réglage temporel du CPU en pourcentage, sans ajout de 100, ni accélération globale de la lecture. La case automatique désactive le curseur ; l'aide explique la compensation des délais matériels et la fréquence calculée simpliste est retirée pour ce cœur. Référence : https://raw.githubusercontent.com/libretro/pcsx_rearmed/master/frontend/libretro_core_options.h.

Dimensions : les facteurs de résolution restent leurs valeurs natives. La référence 320 × 240 est explicitée dans les aides PS1 (la taille réelle varie selon le jeu et le mode vidéo), avec dimensions multipliées dans les choix et sous le curseur SwanStation. Les facteurs de résolution et de vitesse CD portent × ; les quantités d'anticipation portent secteurs dans leur libellé. Le zéro de recherche SwanStation est affiché comme absence de délai, conformément au cœur. Références : https://raw.githubusercontent.com/libretro/beetle-psx-libretro/master/libretro_core_options.h et https://raw.githubusercontent.com/libretro/swanstation/master/src/libretro/libretro_core_options.h.

Cartes mémoire : nom saisi sans extension, extension ajoutée à la création, initialiseur natif du format appelé après création du fichier (en-tête et répertoire PS1 ; flash effacée PS2). Identifiant et modèle fixes en texte dans la même ligne ; dossier de destination complet, sans cadre de saisie. Nouvelles cartes dans Storage/Sony/PlayStation/Memory Cards ou Storage/Sony/PlayStation2/Memory Cards ; Memory Stick dans Storage/Sony/PSP/Memory Stick. Les fichiers existants sont conservés à leur emplacement. Les chemins et activations des ports masqués par le multitap ne sont plus effacés lors du rétablissement des ports.

Depuis Émulation, les cartes gérées par fichiers ou dossiers disposent du même dialogue de sélection/création ; les cartes sélectionnées sont ajoutées aux supports de session puis retranscrites dans les options de carte du cœur, sans être envoyées comme images de jeu. Le retrait désactive la carte de session. La recréation de machine suit RequiresMachineRecreation, comme les autres supports qui l'exigent ; cela ne prétend pas fournir un échange sans redémarrage du jeu. Play conserve ses répertoires de cartes fixes, sans choix de fichier inventé. Les fenêtres ajoutées au parcours sont fermées dans finally.

Argos exécuté sur les libellés, valeurs et aides concernés dans les 28 langues, puis corrections manuelles des textes français et des termes de compilation/interprétation, couleurs et recherche dans toutes les langues. Les noms PocketStation et les titres NTSC/PAL/HDR restent dans la base. Audit : 151 RESX modifiés valides, sans clé supprimée, dupliquée ou vide ; nouveaux libellés présents dans les 30 catalogues Sony. Diff sans erreur. Aucun build, aucune compilation de tests, aucun lancement de cœur ou de fenêtre nouvelle, aucun média de test créé. Les raccordements sont contrôlés dans les sources ; le comportement réel et la mise en page après compilation restent à valider.

## Commit et build Debug demandés le 16 octobre

- [x] Corriger la compilation du dialogue de stockage.
  - [x] Modifier `src/GWGUI.App/Functions/Views/Emulation/Storage/StorageDialogUi.cs` : supprimer la seconde initialisation identique de TextWrapping dans CompactFields.
- [x] Regrouper les corrections et livrer le build.
  - [x] Modifier `docs/tasks/emulation/commun.md` : consigner le commit regroupé de toutes les modifications présentes, puis le résultat du build Debug application et tous les modules par `scripts/local-building.cmd --building=debug --modules=A`.
  - [x] Modifier `docs/tasks/emulation/commun.md` : consigner la présence de `build/Debug/GW GUI/gwgui.exe`, les 23 modules livrés et leur concordance avec les DLL compilées.

Résultat du build : commit regroupé créé, puis correction de la propriété TextWrapping ajoutée en double dans CompactFields, intégrée au même commit. Le lanceur CMD a échoué sur son chemin PowerShell fixé ; le même script a été exécuté par `pwsh -NoProfile -File scripts/local-building/build.ps1 -Configuration Debug -AllModules`. Construction complète terminée avec code zéro. Avertissements CA1416 dans plusieurs modules et CS8602 dans SNK, sans erreur finale.

Livraison vérifiée : `build/Debug/GW GUI/gwgui.exe` présent ; DLL App identique par SHA256 à la compilation ; SDK identique dans lib et Updater ; manifestes et DLL des 23 modules présents et identiques aux compilations ; staging Debug absent. Aucun test supplémentaire ni lancement de fenêtre pour cette demande de commit et build. Validation visuelle et essais interactifs à effectuer avec cette livraison.

## Emplacement cartouche intégré de la Watara Supervision

- [x] Empêcher la suppression du connecteur intégré.
  - [x] Modifier `src/GWGUI.Emulation.Watara/Emulators/Potator/Functions/MediaRuntimeFunctions.cs` : déclarer Cartridge0 permanent avec IsPermanent, utilisé par la liste et la protection de suppression existantes ; conserver le retrait de la cartouche en exécution.
  - [x] Modifier `docs/tasks/emulation/commun.md` : consigner le contrôle du raccordement et la vérification du diff.

Cause : le descripteur Cartridge0 omettait IsPermanent, dont le défaut est false. Le contrôleur commun fournit CanRemove = !IsPermanent et refuse déjà RemoveDevice pour un périphérique permanent ; la liste désactive le bouton selon CanRemove. Le descripteur Watara utilise maintenant cette protection existante. IsRemovable reste inchangé : la cartouche peut toujours être retirée ou remplacée depuis Émulation. Diff vérifié sans erreur. Aucun build ni commit supplémentaire pour cette correction.

## Périphériques Thomson intégrés et disposition des visuels

- [x] Rétablir le matériel intégré selon le modèle Thomson.
  - [x] Modifier `src/GWGUI.Emulation.Thomson/Common/Machines/Common/Functions/ConfigurationFunctions.cs` : définir les emplacements intégrés (cartouche sur tous les modèles, cassette MO6, disquette TO8D/TO9/TO9+) et refuser leur suppression lors de la validation.
  - [x] Modifier `src/GWGUI.Emulation.Thomson/Common/Machines/Common/Contracts/ConfigurationContracts.cs` : inclure ces emplacements même sans support inséré, en conservant les périphériques externes configurés et les associations existantes.
  - [x] Modifier `src/GWGUI.Emulation.Thomson/Emulators/Theodore/Functions/MediaRuntimeFunctions.cs` : déclarer permanents uniquement les périphériques intégrés du modèle.
- [x] Placer le choix du visuel au-dessus de son image.
  - [x] Modifier `src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationControllerSettingsLayout.cs` : retirer le choix du visuel de la ligne commune et le placer dans la colonne de droite, au-dessus de son image, dans les deux parcours existants.
- [x] Vérifier et documenter les corrections.
  - [x] Modifier `docs/tasks/emulation/commun.md` : consigner les modèles concernés, les protections existantes et les contrôles du diff, sans annoncer de nouvelle livraison compilée.

Corrections dans les sources : les emplacements intégrés sont ajoutés au constructeur, y compris au chargement des configurations existantes ; les lecteurs externes et supports associés sont conservés. La validation refuse de retirer un emplacement intégré et IsPermanent désactive sa suppression dans la liste existante. Cartouche sur les huit modèles ; cassette intégrée au MO6 ; disquette intégrée aux TO8D, TO9 et TO9+. Le MO5, TO7, TO7/70 et TO8 conservent leurs lecteurs externes facultatifs. Références : documentation Theodore (https://raw.githubusercontent.com/Zlika/theodore/master/README.md), guide Thomson MO6 (https://www.retronik.fr/DOCUMENTS/Info/Thomson_MOTO/Thomson%20MO6%20Guide%20%28Thomson%201986%29.pdf), pilote de son auteur MESS TO9 (https://mine.perso.lip6.fr/mess/to9.html.fr).

Disposition des manettes : sélecteur du visuel dans sa propre colonne, au-dessus de la photo, légende au-dessus du sélecteur ; type de manette conservé avec les associations à gauche. Les deux parcours de construction emploient cette disposition. Diff vérifié sans erreur ; pas de build ni de lancement graphique supplémentaire.

## Halos PocketStation et largeur du cadre CPU

- [x] Recaler les halos PocketStation.
  - [x] Modifier `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Sony/PocketStation/SonyPocketStation.cs` : corriger les cinq centres en tenant compte de la photo entière de 346 × 512 pixels et des coordonnées de rectangles utilisées par le moteur.
- [x] Déployer les réglages CPU sur la largeur disponible.
  - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs` : présenter les cadres CPU en pleine largeur.
  - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/ModuleSettings/EmulationModuleSettingsSection.Fields.cs` : rendre les textes des boutons radio repliables pour ne pas couper les choix longs.
- [x] Consigner les contrôles.
  - [x] Modifier `docs/tasks/emulation/commun.md` : consigner le recalage et la disposition vérifiés dans les sources, sans annoncer de validation graphique.

Halos : photo PocketStation examinée, dimensions PNG vérifiées à 346 × 512 ; rectangles recentrés sur les cinq boutons (centres de référence en pixels : haut 114/304, bas 108/378, gauche 64/336, droite 160/346, action 257/335). Le moteur positionne les halos au centre des rectangles et conserve le rapport de la photo ; tailles existantes conservées, photo inchangée.

CPU : suppression de la répartition des cadres compacts dans deux colonnes ; les cadres CPU occupent maintenant toute la largeur, les uns sous les autres. Les choix binaires disposent de TextBlock avec retour à la ligne et nom accessible entier. Valeurs natives et actions inchangées. Diff vérifié sans erreur. Pas de nouveau build, de commit, ni de validation graphique pour ce lot ; la livraison précédente ne contient pas encore ces corrections.

## Ajustements ROM, Rendu et rangement des cartes demandés sur captures

- [x] Simplifier les dimensions affichées.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Functions/CoreSettingsFunctions.cs` : afficher facteur et dimensions résultantes, sans résolution source ni flèche.
- [x] Corriger les formulaires ROM et Rendu.
  - [x] Modifier `src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationVideoSettingsLayout.cs` : permettre des champs sur une ou deux colonnes et des rangées de cadres adaptatives espacées.
  - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/ModuleSettings/EmulationModuleSettingsSection.Layout.cs` : étendre les champs BIOS, espacer les cadres Rendu, rapprocher les petits groupes et placer NTSC/PAL côte à côte avec leurs curseurs regroupés.
  - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/ModuleSettings/EmulationModuleSettingsSection.Slider.cs` : centrer les curseurs verticaux avec leur valeur juste en dessous et une hauteur proportionnée.
- [x] Redimensionner le dialogue et organiser les dossiers par type.
  - [x] Modifier `src/GWGUI.App/Views/Dialogs/Emulation/Storage/HardDiskDriveConfigurationDialog.cs` : adapter la fenêtre carte mémoire à son contenu, enlever les répétitions et le cadre imbriqué, garder le chemin entier lisible.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/StorageFunctions.cs` et `Common/Machines/Common/Constants/StorageConstants.cs` : fournir les destinations Storage/Memory Card/Sony/<Machine> et Storage/Memory Stick/Sony/PSP aux parcours existants, sans déplacer les fichiers associés.
- [x] Vérifier le lot.
  - [x] Modifier `docs/tasks/emulation/commun.md` : consigner le contrôle du diff et des raccordements, ainsi que les limites sans nouveau build ni lancement graphique.

Résultat dans les sources : libellés de résolution réduits à « facteur (largeur × hauteur px) », sans flèche ni dimensions initiales ajoutées. Le formulaire ROM place la légende au-dessus du champ complet, évitant que la longue légende de la case BIOS réduise la largeur du chemin. Cadres Rendu espacés de 18 unités ; petits cadres consécutifs compatibles et cadres NTSC/PAL réunis par deux, avec repli vertical sous 760 unités. Les groupes plus fournis gardent la largeur complète. Curseurs verticaux ramenés à 128 unités, centrés, avec valeur immédiatement en dessous.

Dialogue de carte mémoire : largeur 760, hauteur selon le contenu, suppression des métadonnées répétées déjà présentes dans le titre et sous-titre, suppression du cadre carte mémoire imbriqué. Destination conservée en texte entier repliable. Création de nouvelles cartes par défaut dans Storage/Memory Card/Sony/PlayStation et Storage/Memory Card/Sony/PlayStation2 ; nouveau dossier Memory Stick dans Storage/Memory Stick/Sony/PSP, au même niveau que HDD. Ces sous-chemins sont utilisés par les réglages et le dialogue de supports en exécution. Les chemins des supports déjà associés et les fichiers utilisateurs existants ne sont pas déplacés.

Contrôles : raccordements des champs, lectures des valeurs et chemins communs relus ; aucun nouveau texte traduisible, noms et valeurs invariants conservés hors catalogues de langue ; diff sans erreur. Aucun nouveau build, lancement graphique ou test interactif ; ces modifications restent à compiler et vérifier à l’écran.

## Reprise complète des captures multitap, valeurs CD et rendu Sony

- [x] Uniformiser les multitaps Sony.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Constants/ControllerOptionConstants.cs`, `Functions/CoreSettingsFunctions.cs` et `src/GWGUI.Emulation.Sony/Modules/SonyEmulationModule.cs` : présenter les modes combinés SwanStation, PCSX-ReARMed et PCEE2 par les deux cases de port existantes et recomposer chaque valeur native.
- [x] Donner une unité et un nom aux valeurs CD.
  - [x] Modifier `src/GWGUI.Emulation/Contracts/EmulationSettingsField.cs`, les fonctions de présentation App et les composants de sélection/curseur sous `src/GWGUI.App/` : transmettre et afficher une unité localisée commune avec les valeurs, sans dupliquer les nombres dans les langues.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Functions/CoreSettingsFunctions.cs` : présenter les anticipations CD en secteurs, zéro désactivé et 333000 comme disque entier, en conservant les valeurs natives.
- [x] Structurer le rendu et le dialogue CD.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Functions/CoreSettingsFunctions.cs` : classer séparément filtres/anticrénelage, outils de textures HD, caches, couleurs et géométrie.
  - [x] Modifier `src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationVideoSettingsLayout.cs` et les sélecteurs de réglages : placer les cases près de leur légende et rendre les choix longs lisibles.
  - [x] Modifier `src/GWGUI.App/Views/Dialogs/Emulation/Storage/CompactDiscDriveConfigurationDialog.cs` et `src/GWGUI.App/Functions/Views/Emulation/Storage/StorageConfigurationFieldFunctions.cs` : enlever les métadonnées répétées, séparer cases et réglages de vitesse/cache, remplacer les largeurs arbitraires par une grille lisible.
- [x] Traduire les textes concernés.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Resources/*/Option.resx` : ajouter les noms de groupes et du disque entier, raccourcir les choix et corriger les libellés HD des captures avec Argos dans toutes les langues, puis relire le français.
- [x] Compléter le classement après contrôle des clés natives.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Functions/CoreSettingsFunctions.cs` : reconnaître la clé super_sampling et le câble vidéo dans les groupes déjà définis.
- [x] Vérifier et compiler.
  - [x] Créer `tests/GWGUI.Tests/Emulation/Sony/SettingsPresentationTests.cs` : vérifier les quatre états natifs des multitaps, les unités et le choix disque entier, ainsi que la séparation des groupes vidéo, sans ressources externes.
  - [x] Modifier `tests/GWGUI.Tests/Emulation/Sony/SettingsPresentationTests.cs` : employer l’identifiant réel mednafen_psx_hw et lire les champs CD dans le descripteur du lecteur, puis exécuter les tests ciblés contre les bibliothèques compilées.
  - [x] Modifier `docs/tasks/emulation/commun.md` : consigner les tests ciblés, le build Debug complet et les fichiers livrés effectivement vérifiés, ou les limites rencontrées.

Résultat : modes multitap combinés SwanStation, PCSX-ReARMed et PCEE2 remplacés par deux interrupteurs de port ; les modes natifs propres à chaque cœur sont recomposés et les clés de présentation ne sont pas transmises. Beetle conserve ses deux options natives déjà indépendantes. Douze états des trois modes combinés vérifiés par aller-retour dans les tests.

Anticipation CD : chaque choix numérique et la valeur du curseur portent l’unité commune Visual.SectorsUnit, déjà traduite dans App. Zéro affiche Désactivé ; la valeur native 333000 affiche Disque entier (environ 750 Mo de RAM). Le nombre natif et le défaut 12 ne changent pas. Source PCSX-ReARMed : https://raw.githubusercontent.com/libretro/pcsx_rearmed/master/frontend/libretro_core_options.h. Les champs sont vérifiés dans le descripteur de configuration du lecteur, qui alimente le dialogue CD.

Rendu : filtres/anticrénelage, remplacement des textures HD et caches HD séparés, géométrie et couleurs conservées dans leurs groupes. Cases à gauche de la légende, textes de choix repliables. Les libellés HD incorrects des captures et les choix trop longs ont été réécrits avec Argos dans les 28 langues puis le français relu. Trente catalogues XML valides, nouveaux groupes et choix disque entier présents, sans doublons. Aucune duplication des nombres en ressources linguistiques : l’unité commune est ajoutée à la valeur lors de la présentation.

Dialogue CD : suppression de l’identifiant déjà donné dans le titre et du modèle identique, cases regroupées avant les réglages de vitesse/cache, grille à largeur partagée à la place des blocs de largeur 400. Légendes au-dessus des listes et curseurs, unité dans les valeurs, fenêtre redimensionnable. Pas de nouveau fichier de données utilisateur créé par ces vérifications.

Vérification finale : build Debug application et 23 modules terminé avec code zéro. Le lanceur CMD a échoué sur son ancien chemin PowerShell ; son script a été lancé avec pwsh du PATH et -AllModules. Avertissements CA1416 et CS8602 déjà observés au build précédent. Exécutable build/Debug/GW GUI/gwgui.exe présent ; App, SDK dans lib/Updater et 23 DLL de modules identiques par SHA256 aux compilations, manifestes présents, staging absent.

Tests compilés sans avertissement ni erreur ; anciennes copies de modules du dossier de tests remplacées par les DLL de la livraison pour éviter les incompatibilités de signature. Résultat final : 74 réussites, aucun échec (SettingsPresentationTests, ControllerConfigurationTests, NumericOptionRangeTests et ControllerArtworkCatalogTests). Les tests ajoutés n’utilisent ni fichiers externes, ni cœur natif, ni fenêtre ; aucun test temporaire créé. Aucune validation graphique ou partie de jeu effectuée. Diff vérifié sans erreur. Aucun commit ou push supplémentaire demandé ou effectué.

## Révision PS2 demandée : Cheats, DVD, présentation et périphériques

- [x] Qualifier les capacités des trois cœurs PS2 livrés.
  - [x] Modifier `docs/tasks/emulation/commun.md` : consigner pour PCSX2/LRPS2, PCEE2 et Play les interfaces réellement exposées pour DVD, ports manettes, USB, IR et Ethernet, avec références de sources natives ; identifier les fichiers de raccordement avant modification.

Audit des interfaces effectivement livrées (16 octobre) : PCSX2 est le cœur libretro LRPS2, pas l’exécutable autonome. PAD.cpp annonce DualShock 2, clavier USB, souris USB et combinaison clavier+souris ; les autres périphériques USB du PCSX2 autonome ne sont pas exposés par cette interface. DEV9 Ethernet est piloté par pcsx2_dev9_eth, avec Sockets et interface Auto imposés par le cœur. PCEE2 expose DualShock 2, multitaps et GunCon 2 USB ; son README indique explicitement que volants, micros et EyeToy ne sont pas raccordés. Il importe DEV9/Eth depuis pcsx2/inis/PCSX2.ini, donc GW GUI peut configurer son réseau natif. Ses seuls champs Storage déclarés sont les cartes mémoire gérées séparément : aucun réglage DVD exposé. Play libretro fournit ses manettes DualShock 2, sans commandes USB/IR/Ethernet dans main_libretro.cpp et son hôte d’entrée ; pas de nouveaux onglets sans interface de contrôle vérifiée.

Aucun des trois hôtes examinés ne fournit de contrôle IR. Les manettes spécifiques PS1 (GunCon, Justifier, neGcon, souris, etc.) ne deviennent pas des protocoles PS2 en changeant leur image : seules les dispositions PS1 compatibles avec la commande numérique/analogique déjà exposée seront proposées. Un tapis de danse numérique peut utiliser les boutons directionnels indépendants du joypad ; il doit conserver les appuis opposés simultanés.

Sources : https://github.com/WizzardSK/pcee2-libretro ; https://raw.githubusercontent.com/WizzardSK/pcee2-libretro/libretro/pcee2-libretro/Libretro.cpp ; https://raw.githubusercontent.com/WizzardSK/pcee2-libretro/libretro/pcsx2/Pcsx2Config.cpp ; https://raw.githubusercontent.com/libretro/ps2/master/pcsx2/PAD/PAD.cpp ; https://raw.githubusercontent.com/libretro/ps2/master/libretro/main.cpp ; https://raw.githubusercontent.com/jpd002/Play-/master/Source/ui_libretro/main_libretro.cpp.

- [x] Déplacer les triches et exposer Ethernet selon les capacités.
  - [x] Modifier `src/GWGUI.Emulation/Enums/EmulationMachineTab.cs`, `EmulationSettingsEditor.cs`, `EmulationDefaultFolderCategory.cs`, `src/GWGUI.App/Constants/Emulation/EmulationMachineTabConstants.cs` et `Views/Controls/Emulation/Options/ModuleSettings/EmulationModuleSettingsSection.Layout.cs` : ajouter les onglets Cheats/Ethernet conditionnels et l’action de dossier imposé dans le contrat commun.
  - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/ModuleSettings/EmulationModuleSettingsSection.Fields.cs` et `.Persistence.cs` : créer le dossier Storage/Cheats/<Marque>/<Machine>, fournir uniquement son bouton d’ouverture et persister le chemin imposé par le contrat générique.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Functions/CoreSettingsFunctions.cs`, `Emulators/Pcee2/Functions/CheatFunctions.cs`, `Emulators/Pcsx2/Functions/FileSystemFunctions.cs` et `Modules/SonyEmulationModule.cs` : isoler les triches dans leur onglet et router le réseau LRPS2 dans Ethernet.
  - [x] Créer `src/GWGUI.Emulation.Sony/Emulators/Pcee2/Functions/NetworkFunctions.cs` et modifier sa fabrique/son initialisation : décrire l’activation Ethernet et écrire les clés DEV9/Eth Sockets/Auto dans le fichier natif de préparation, sans remplacer les autres sections.
- [x] Corriger le lecteur DVD.
  - [x] Modifier `src/GWGUI.Emulation/Contracts/EmulationMediaDevice.cs`, `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/StorageFunctions.cs`, les contrats/liste/contrôleur de stockage App et `CompactDiscDriveConfigurationDialog.cs` : afficher DVD pour PS2 et transmettre le type descriptif ; ne proposer Configurer que si des champs natifs existent.
- [x] Refaire la présentation CPU/vidéo PS2 et ses valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Functions/CoreSettingsFunctions.cs` : répartir unités vectorielles, recompilation, compatibilité, filtres, synchronisation et affichage ; remplacer les nombres de mode par leurs vrais libellés et expliciter les multiplicateurs.
  - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs` et les dispositions communes concernées : rapprocher les légendes et contrôles, placer les cases et réglages dans des cadres cohérents avec des espacements lisibles.
- [x] Compléter les commandes et visuels compatibles.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Pcee2/Functions/ControllerConfigurationFunctions.cs`, sa fabrique, `Emulators/Common/Interop/Factories/CoreMachineFactory.cs`, `Common/Interfaces/IEmulatorControllerAdapter.cs`, `Common/Machines/Common/Functions/InputFunctions.Settings.cs`, `src/GWGUI.Emulation/Contracts/EmulationControllerPort.cs`, le contrat/contrôleur App de ports et `EmulationControllerSettingsLayout.cs` : transmettre un numéro d’affichage pour nommer les connecteurs manette Port 1/Port 2 avec la ressource commune, conserver USB 1/USB 2.
  - [x] Modifier les enums, fonctions Controller/Input et le résolveur natif Sony : ajouter le tapis numérique sur PS1/PS2 via les commandes joypad indépendantes, sans inventer un périphérique natif non exposé ; proposer les dispositions PS1 compatibles sur les manettes PS2.
  - [x] Créer le profil vectoriel `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Sony/PlayStation/SonyDanceMat.cs` et modifier les identifiants/catalogues communs : raccorder son visuel et ses six zones de commande.
- [x] Finaliser les raccordements après relecture.
  - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/ModuleSettings/EmulationModuleSettingsSection.Fields.cs`, `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Functions/CoreSettingsFunctions.cs`, `Constants/CoreSettingsConstants.cs`, le profil `SonyDanceMat.cs` et `Emulators/Pcee2/Functions/NetworkFunctions.cs` : employer la ressource commune du bouton, préciser les pourcentages et choix de mélange, fixer les bornes vectorielles, nettoyer les constantes et isoler la préparation INI pure pour vérification sans fichier externe.
- [ ] Traduire et vérifier.
  - [x] Modifier les catalogues App/Sony concernés dans toutes les langues avec Argos : onglets/actions nouveaux, Ethernet, lecteur DVD, tapis et libellés/aides CPU et vidéo des captures ; garder les valeurs invariantes dans la base.
  - [ ] Modifier/créer les tests Sony utiles sous `tests/GWGUI.Tests/Emulation/Sony/` : capacités des trois cœurs, dossier Cheats imposé, lecteur DVD et absence de configuration vide, Ethernet, tapis et préservation des valeurs natives ; aucun nouveau test externe permanent.
  - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner les contrôles, la compilation et les limites prouvées des interfaces USB/IR, avec résultats effectivement obtenus.
- [ ] Reprendre les traductions techniques et intégrer le périmètre précisé.
  - [x] Modifier `src/GWGUI.App/Resources/*/Emulation/EmulationTab.resx` et les catalogues Sony `Resources/*/Option.resx`, `Machine.resx`, `Controller.resx` : corriger les contresens de toutes les langues sur les ajouts PS2, relire les légendes et aides françaises concernées, supprimer les doublons et conserver Ethernet/DVD en base commune.
  - [x] Créer puis supprimer `artifacts/review-ps2-translations.py` : appliquer les corrections linguistiques contrôlées et vérifier XML, clés et paramètres sans laisser cet outil temporaire.
  - [x] Modifier `docs/tasks/emulation/commun.md` : consigner la vérification linguistique et ajouter les actions du catalogue Sony/licenciés PS1/PS2 et des onglets clavier/souris USB, y compris les périphériques non émulés.

Relecture linguistique (16 octobre) : conformément à la précision utilisateur, le titre Cheats reste invariant pour toutes les langues et n’existe que dans la base commune App. Ethernet et le nom DVD sont aussi dans la base ; les légendes traduisibles de lecteur DVD restent localisées. Suppression du doublon DanceMat créé dans Controller.resx. Corrections manuelles après Argos dans les 28 langues : niveaux de précision, cycles omis, recompilation, unités vectorielles, fréquence de l’Emotion Engine, transferts GPU, tramage, accentuation CAS, réseau, ordre des trames et noms de périphériques. Relecture de toutes les légendes et aides PCEE2 françaises. L’outil ponctuel de correction a été supprimé après usage. Les catalogues modifiés ont été analysés XML : aucune clé en double ni valeur vide ; titre Cheats présent uniquement en base et noms Emotion Engine conservés. Cette vérification ne prétend pas valider les traductions historiques étrangères non modifiées.

Périmètre des périphériques confirmé : appareils Sony et modèles officiellement associés ou mis en avant pour PS1/PS2 ; pas de copies génériques. Le catalogue doit inclure les appareils non émulés avec leurs images et leur disponibilité réelle. Les associations clavier/souris USB doivent être exclusivement dans les onglets Clavier/Souris, affichés selon le choix USB.

- [ ] Corriger le visuel de tapis selon le raccordement existant.
  - [ ] Créer `src/GWGUI.App/Assets/Controllers/sony-dance-mat.png` à partir de l’image Amazon fournie par l’utilisateur : supprimer câble/prise USB et détourer le seul fond extérieur, garder les blancs internes et inscriptions ; supprimer le JPG intermédiaire après contrôle du PNG transparent.
  - [ ] Modifier `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkFileNames.cs`, `ControllerArtworkCatalog.cs` et `Profiles/Sony/PlayStation/SonyDanceMat.cs` : remplacer le faux fichier .vector par l’image réelle, supprimer le générateur et son branchement spécial, ajuster les zones aux boutons photographiés.
  - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner la vérification de l’image et du chargement commun, puis recompiler après la correction.

Précision sur le tapis : image finale expressément choisie par l’utilisateur : https://m.media-amazon.com/images/I/71taHKzVsvL._AC_UF894,1000_QL80_.jpg ; autorisation explicite de retirer le câble/prise USB et de rendre le fond extérieur transparent. Il s’agit du visuel générique choisi, pas d’une identification Konami ou Sony.

- [ ] Raccorder les commandes visibles sur le tapis choisi.
  - [ ] Modifier `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/InputFunctions.Settings.cs`, `InputFunctions.Visuals.cs` et `tests/GWGUI.Tests/Emulation/Sony/SettingsPresentationTests.cs` : conserver les quatre directions indépendantes, Start/Select et ajouter les quatre boutons de face visibles sur l’image fournie ; vérifier leurs définitions sans ressource externe.
  - [ ] Modifier `docs/reference/sony-controller-sources.md` : consigner l’image fournie, le détourage demandé et l’absence d’identification fabricant, sans présenter ce visuel comme un nouveau modèle matériel Sony.

- [ ] Compléter le catalogue physique PS1/PS2 demandé, indépendamment des capacités des cœurs.
  - [ ] Modifier `docs/reference/sony-controller-sources.md` : lister les familles Sony et partenaires retenues, leurs connecteurs PS1/PS2, les sources primaires et les photos exactes ; exclure les clones génériques supplémentaires.
  - [ ] Modifier `src/GWGUI.Emulation/Contracts/EmulationControllerChoice.cs`, `src/GWGUI.Emulation.Sony/Common/Machines/Common/Enums/ControllerEnums.cs`, `Dictionaries/ControllerCatalog.cs` et `Functions/ControllerFunctions.cs` : étendre le catalogue existant aux familles officielles/licenciées et distinguer disponibilité physique et prise en charge réelle, sans faire passer un protocole inconnu pour un joypad natif.
  - [ ] Modifier `src/GWGUI.Emulation.Sony/Common/Machines/Common/Contracts/InputContracts.cs`, `Functions/InputFunctions.Settings.cs`, `InputFunctions.Visuals.cs`, `Modules/SonyEmulationModule.cs` et `Emulators/Pcsx2/Services/ExternalCore.cs` : présenter les deux USB PS2, conserver les identifiants existants PCEE2, isoler les associations clavier/souris dans leurs onglets conditionnels et transmettre seulement les types USB natifs connus à LRPS2.
  - [ ] Modifier `src/GWGUI.App/Views/Controls/Emulation/Input/EmulationControllerPortEditor.cs`, `Contracts/Emulation/Controllers/EmulationControllerPortSettings.cs`, `Controllers/Emulation/Input/EmulationInputSettingsController.cs`, `Functions/Views/Emulation/Settings/EmulationControllerSettingsLayout.cs` et `Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs` : afficher les limites de prise en charge et actualiser les onglets clavier/souris après choix USB.
  - [ ] Créer les photographies des nouveaux périphériques sous `src/GWGUI.App/Assets/Controllers/sony-*.png` ou `.jpg` selon le format source, puis modifier `GWGUI.App.csproj` si nécessaire, `ControllerArtworkFileNames.cs`, `Profiles/Sony/SonyProfileRegistry.cs`, les profils Sony de `ControllerVisualization`, `src/GWGUI.Emulation/Constants/EmulationControllerVisualIds.cs` et les ressources de base : charger chaque image par la voie commune avec son nom de modèle invariant.
  - [ ] Modifier les catalogues App/Sony traduisibles concernés dans les 28 langues avec Argos puis relecture : noms de familles, indications de prise en charge et onglets, sans retraduire les noms propres.
  - [ ] Modifier les tests Sony utiles sous `tests/GWGUI.Tests/Emulation/Sony/` et `docs/tasks/emulation/commun.md` : vérifier aller-retour des appareils du catalogue, disponibilité native par cœur et séparation USB/clavier/souris ; compiler l’application et tous les modules, contrôler les fichiers livrés et consigner les limites restantes.

- [ ] Compléter les fichiers de raccordement nécessaires au catalogue validé.
  - [ ] Créer `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/InputFunctions.Usb.cs` : décrire les deux USB physiques PS2, migrer les anciens choix clavier/souris LRPS2 sans modifier les ports manette natifs et fournir les associations exclusives des onglets Clavier/Souris.
  - [ ] Modifier `src/GWGUI.Emulation/Contracts/EmulationControllerPort.cs` et les contrats/éditeur/disposition App déjà listés : transmettre le type de connecteur USB pour adapter la légende du sélecteur de périphérique, sans déduire la connectique du texte affiché.

- [ ] Normaliser les images conformément à la demande de cadrage et de poids.
  - [ ] Modifier `src/GWGUI.App/Assets/Controllers/sony-dance-mat.png` : exporter le détourage validé à 512 pixels maximum en conservant l’alpha, contrôler le poids inférieur à 1 Mo et recalculer les zones si le cadrage change.
  - [ ] Modifier les nouvelles images `src/GWGUI.App/Assets/Controllers/sony-*` : retenir des vues de face réelles, détourer les fonds non transparents avec l’outil d’image et exporter les PNG à 512 pixels maximum et moins de 1 Mo ; supprimer les fichiers source intermédiaires après contrôle du résultat.

- [ ] Présenter le groupe Buzz! complet demandé.
  - [ ] Modifier `src/GWGUI.App/Assets/Controllers/sony-buzz.png` : détourer la photo de face `https://upload.wikimedia.org/wikipedia/commons/c/c8/Buzz_Buzzer.jpg`, composer quatre copies identiques alignées sans câble ni fond, exporter le PNG final à 512 pixels maximum et moins de 1 Mo ; supprimer le JPEG source après contrôle.
  - [ ] Modifier `src/GWGUI.Emulation/Enums/EmulationControllerVisualControl.cs`, les définitions et commandes Sony sous `Common/Machines/Common/Functions/InputFunctions.*.cs`, les profils Sony et `Resources/00-Base/Controller.resx` : créer vingt identifiants/halos distincts, rouge/bleu/orange/vert/jaune pour chacun des quatre joueurs ; permettre la préparation des associations même si un cœur ne les émule pas encore.
  - [ ] Modifier `docs/reference/sony-controller-sources.md` et `docs/tasks/emulation/commun.md` : consigner la photographie retenue, la composition des quatre joueurs, le poids et les contrôles effectués.

- [ ] Exporter les détourages sans surpoids.
  - [ ] Créer `artifacts/export-controller-artwork.ps1` : exporter les résultats de détourage en PNG alpha de 512 pixels maximum, rogner les seules marges transparentes lorsque demandé et libérer Bitmap/Graphics/Stream dans finally ; refuser un fichier final de 1 Mo ou plus.
  - [ ] Modifier les nouveaux PNG `src/GWGUI.App/Assets/Controllers/sony-*` avec cet export après chaque détourage, contrôler les dimensions et recalculer les zones sur le cadrage final ; supprimer `artifacts/export-controller-artwork.ps1` une fois tous les fichiers terminés.

- [ ] Définir les commandes physiques indépendamment des cœurs.
  - [ ] Créer `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/InputFunctions.Peripherals.cs` : déclarer les commandes et correspondances des périphériques spécialisés, en commençant par les vingt boutons distincts du groupe Buzz! ; stocker leurs associations sans exiger une implémentation native préalable.

## Reprise après arrêt prématuré : terminer les images et associations demandées

- [ ] Corriger les quatre détourages en attente.
  - [x] Modifier `src/GWGUI.App/Assets/Controllers/sony-jogcon.png` et `sony-guitar-hero-sg.png` : exporter les détourages de face contrôlés, alpha extérieur, maximum 512 pixels et moins de 1 Mo.
  - [x] Modifier `src/GWGUI.App/Assets/Controllers/sony-eyetoy.png` et créer `sony-hori-fighting-stick-ps.png` : refaire les détourages dont le résultat comporte encore un fond opaque ; contrôler le PNG final.
- [ ] Raccorder les images contrôlées et les commandes physiques.
  - [x] Modifier `ControllerArtworkFileNames.cs`, `Profiles/Sony/SonyProfileRegistry.cs` et créer les profils sous `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Sony/` : JogCon, Guitar Hero SG, Hori Fighting Stick PS, EyeToy et SingStar avec uniquement leurs commandes réelles.
  - [ ] Modifier `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/InputFunctions.Peripherals.cs`, `.Visuals.cs`, `.Usb.cs`, `Dictionaries/ControllerCatalog.cs` et les ressources de base : associations indépendantes des cœurs pour les profils ajoutés, noms exacts des modèles.
- [ ] Poursuivre les modèles manquants et corriger les sources refusées.
  - [ ] Modifier `docs/reference/sony-controller-sources.md` : inventaire précis des modèles encore sans image, sources remplacées et raccordements réellement terminés.
  - [ ] Modifier les images sous `src/GWGUI.App/Assets/Controllers/`, profils Sony, catalogue et commandes : remplacer les photos ASCII, Namco et Hori refusées puis ajouter les modèles approuvés encore manquants, sans les annoncer terminés auparavant.
- [ ] Vérifier le résultat complet.
  - [ ] Modifier les tests Sony et `docs/tasks/emulation/commun.md` : contrôler catalogue, associations et zones, consigner le build Debug application et tous les modules et le contrôle des fichiers livrés.

- [ ] Corriger les associations USB constatées pendant le raccordement.
  - [ ] Modifier `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/InputFunctions.Settings.cs` : conserver ButtonMappings et OutputBindings des deux USB synthétiques à la sauvegarde, comme pour les ports physiques.
  - [ ] Modifier `src/GWGUI.Emulation.Sony/Resources/*/Controller.resx` et `src/GWGUI.App/Resources/*/Emulation/EmulationController.resx` : traduire avec Argos les commandes de guitare et les indications USB nouvellement introduites, relire le français.

## Nettoyage demandé des images inutilisées (priorité en cours)

- [x] Retirer les images hors catalogue et les sources remplacées.
  - [x] Modifier `docs/tasks/emulation/commun.md` : consigner la liste vérifiée des fichiers sans référence de code et les photos encore nécessaires aux détourages.
  - [x] Supprimer sous `src/GWGUI.App/Assets/Controllers/` les anciens fichiers Sony refusés/remplacés : `sony-ascii-arcade-stick.jpg`, `sony-ascii-enhanced-pad.jpg`, `sony-ascii-specialized-pad.jpg`, `sony-eyetoy-source.png`, `sony-guitar-hero-sg-source.jpg`, `sony-hori-fighting-commander-2way.jpg`, `sony-hori-fighting-commander.jpg`, `sony-hori-fighting-stick-ps.jpg`, `sony-hori-fighting-stick.jpg`, `sony-jogcon.jpg`, `sony-namco-arcade-stick.jpg`, `sony-volume-controller-source.jpg`, `sony-volume-controller.jpg`.
  - [x] Déplacer les trois sources encore nécessaires `sony-ascii-specialized-pad-source.jpg`, `sony-ascii-arcade-stick-source.jpg`, `sony-namco-arcade-stick-source.jpg` de `src/GWGUI.App/Assets/Controllers/` vers `artifacts/controller-artwork-sources/` ; les supprimer après leurs exports finaux.
  - [x] Supprimer sous `src/GWGUI.App/Assets/Controllers/` les quatorze PNG sans aucune référence de code : `advanced-gravis-gamepad.png`, `atari-5200-trak-ball.png`, `bally-astrocade-controller.png`, `commodore-vc-1312-paddles.png`, `emerson-arcadia-2001-controller.png`, `fairchild-channel-f.png`, `gce-vectrex-controller.png`, `interton-vc-4000-controller.png`, `magnavox-odyssey-controller.png`, `mattel-hyperscan-controller.png`, `philips-cdi-paddle-controller.png`, `philips-cdi-remote-control.png`, `philips-cdi-roller-controller.png`, `philips-cdi-trackerball.png`.
  - [x] Modifier `docs/tasks/emulation/commun.md` : consigner le nombre de fichiers retirés, le poids libéré et le contrôle des références encore présentes ; conserver le nouveau Volume Controller en attente de raccordement.

Nettoyage initial trop large, corrigé : les 14 images non Sony sont restaurées à l’identique de HEAD. Les 4 photos Sony sans remplacement final sont également restaurées depuis leurs URL originales (tailles identiques à celles relevées avant suppression). Il reste uniquement 9 anciens fichiers Sony remplacés supprimés. Trois photographies de travail déplacées dans artifacts/controller-artwork-sources pour les détourages en cours ; elles ne sont plus dans les assets. Le PNG Volume Controller est conservé car son raccordement est la prochaine action. La suppression des 14 PNG non Sony listée ci-dessus a été annulée ; ne pas la reproduire. Une absence de référence ne suffit pas à autoriser un nettoyage hors du périmètre Sony demandé.

## Correction immédiate du nettoyage trop large

- [x] Rétablir le périmètre demandé : uniquement les doublons Sony remplacés.
  - [x] Restaurer depuis Git les quatorze PNG non Sony supprimés sous `src/GWGUI.App/Assets/Controllers/` : Gravis, Atari, Bally, Commodore, Emerson, Fairchild, Vectrex, Interton, Magnavox, Mattel et quatre Philips ; vérifier chaque fichier contre HEAD.
  - [x] Restaurer les photos Sony sans remplacement final encore contrôlé : `src/GWGUI.App/Assets/Controllers/sony-ascii-enhanced-pad.jpg`, `sony-hori-fighting-commander-2way.jpg`, `sony-hori-fighting-commander.jpg` et `sony-namco-arcade-stick.jpg`, depuis leurs URL originales.
  - [x] Modifier `docs/tasks/emulation/commun.md` : corriger le bilan du nettoyage et indiquer précisément les seuls doublons Sony supprimés ; ne supprimer aucune autre image non Sony.

Restauration contrôlée : 14/14 PNG non Sony présents et identiques à HEAD par identifiant de contenu Git. Photos Sony rétablies : ASCII Enhanced Pad (63 442 octets), Hori Commander 2Way (238 405), Hori Commander (25 728), Namco Arcade Stick (47 574). Les neuf suppressions Sony conservées correspondent aux anciens ASCII Arcade/Specialized, EyeToy source, Guitar Hero source, deux Hori Stick, JogCon JPG, Volume source et Volume JPG, tous remplacés par leurs PNG contrôlés.

## Suite après restauration : seuls les remplacements Sony

- [ ] Finaliser les trois remplacements Sony en cours.
  - [ ] Modifier `src/GWGUI.App/Assets/Controllers/sony-namco-arcade-stick.png` : exporter le détourage de face contrôlé ; après contrôle alpha et dimensions, supprimer uniquement `sony-namco-arcade-stick.jpg` et `artifacts/controller-artwork-sources/sony-namco-arcade-stick-source.jpg`.
  - [ ] Créer les profils `Profiles/Sony/PlayStation/SonyAsciiSpecializedPad.cs`, `SonyAsciiArcadeStick.cs`, `SonyNamcoArcadeStick.cs` et modifier les constantes, le registre, les ressources de base et les fonctions d’entrée Sony existantes : placer les zones sur leurs boutons et conserver les associations propres aux modèles.
  - [ ] Modifier `src/GWGUI.App/Controllers/Emulation/Input/EmulationInputSettingsController.cs` et les fonctions d’entrée Sony : préserver les commandes propres au visuel lors du rechargement de la configuration.
- [ ] Contrôler la compilation du travail en cours.
  - [ ] Modifier les tests Sony et `ControllerArtworkCatalogTests.cs` : vérifier les vingt commandes Buzz!, la sauvegarde des USB et les profils sans boutons EyeToy/SingStar, sans fichier externe de test.
  - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner le build Debug complet, les tests ciblés et les modèles restant à réaliser ; ne pas annoncer le catalogue complet avant sa réalisation.

## Restaurer le build précédent sans compilation

- [x] Remettre les binaires existants dans `build/Debug/GW GUI/`.
  - [x] Copier l’application et ses dépendances déjà présentes, en remettant les DLL et traductions précédentes de `tests/GWGUI.Tests/bin/Debug/net10.0-windows10.0.19041.0/win-x64/`.
  - [x] Copier les 23 modules précédents et leurs métadonnées dans `build/Debug/GW GUI/Modules/`, puis le lanceur et Updater précédents depuis leurs sorties binaires.
  - [x] Modifier `docs/tasks/emulation/commun.md` : consigner le contrôle SHA256 des anciens binaires, la présence de l’exécutable et des 23 modules, sans exécuter de build.

## Reprise des images fausses et des périphériques manquants — aucun build

- [x] Remplacer les photos Hori et ASCII restantes.
  - [x] Créer `artifacts/controller-artwork-sources/sony-hori-fighting-commander-2way.jpg`, `sony-hori-fighting-commander-10b.jpg` et `sony-ascii-enhanced-pad.jpg` depuis les photographies frontales identifiées ; conserver les anciens assets jusqu’au contrôle de leurs remplacements.
  - [x] Créer les PNG transparents `src/GWGUI.App/Assets/Controllers/sony-hori-fighting-commander-2way.png`, `sony-hori-fighting-commander-10b.png`, `sony-ascii-enhanced-pad.png`, maximum 512 pixels et moins de 1 Mo ; supprimer uniquement les anciennes photos Sony correspondantes après contrôle.
  - [x] Créer les profils Sony correspondants sous `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Sony/PlayStation/`, modifier le registre, les constantes, le catalogue et les associations propres aux visuels ainsi que les noms invariants en base.
- [x] Ajouter les périphériques manquants de la liste validée.
  - [x] Modifier `docs/reference/sony-controller-sources.md` : suivre chaque modèle approuvé avec son image et ses commandes réellement raccordées ; conserver explicitement les appareils restant à faire.
  - [x] Créer les photographies PNG et profils sous `src/GWGUI.App/Assets/Controllers/` et `Profiles/Sony/`, modifier le catalogue Sony existant, les commandes et les ressources pour les modèles validés encore absents, indépendamment de leur prise en charge native ; aucune compilation.

- [x] Normaliser l’export alpha des détourages Sony.
  - [x] Modifier `artifacts/export-controller-artwork.ps1` : exporter en pixels prémultipliés et composer sur une surface transparente pour ne pas conserver de couleurs RGB cachées sous alpha nul, sans modifier le masque ni les commandes du périphérique ; réexporter le Hori 2Way.

- [x] Ajouter les modèles PS1 spécialisés encore absents.
  - [x] Créer les photos sources dans `artifacts/controller-artwork-sources/` pour ASCII Grip, Biohazard Pad, Sphere 360, Reality Quest Glove et Fighter Stick V ; contrôler le modèle avant détourage.
  - [x] Créer les PNG correspondants sous `src/GWGUI.App/Assets/Controllers/sony-*.png`, maximum 512 pixels, fond transparent et moins de 1 Mo ; supprimer leurs seules sources de travail après contrôle.
  - [x] Créer leurs profils Sony PS1 et modifier les identifiants visuels, le registre, les noms de base, le catalogue et les commandes Sony : toutes les commandes physiques visibles, associations conservées indépendamment de la prise en charge native.

- [x] Raccorder les commandes exactes du Sphere 360.
  - [x] Créer `artifacts/controller-artwork-sources/sony-sphere-manual.jpg` avec le scan du manuel d’origine `https://www.game-rave.com/wp-content/uploads/2024/10/PSX-Demo-360-Controller-Demo-Manual-1-web.jpg` pour définir les boutons et les six axes physiques sans inventer de commandes ; supprimer ce scan de travail après raccordement.

- [x] Nommer les six axes physiques du Sphere 360.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Resources/*/Option.resx` : ajouter avec Argos les douze libellés des translations et rotations X/Y/Z dans toutes les langues ; relire les termes français, conserver les coordonnées comme valeurs mathématiques invariantes.

Instruction utilisateur actuelle : AUCUN BUILD, REBUILD ou test imposant une compilation sans nouvelle demande explicite. La compilation lancée prématurément a été arrêtée. Ancien build rétabli par copie uniquement : application du 16 octobre à 23:01 et 23 modules, DLL App/SDK/modules comparées par SHA256 aux copies précédentes. Les travaux actuels concernent les sources, images et halos, et ne sont pas présents dans cet ancien exécutable.

Point de contrôle : 34 profils Sony enregistrés, tous leurs fichiers et identifiants présents ; douze libellés des six axes du Sphere vérifiés dans tous les catalogues Option. Nouveaux périphériques Grip, Biohazard, Sphere, Glove et Fighter Stick V raccordés dans les sources ; compatibilité native ultérieure selon la demande utilisateur. Pas de catalogue complet annoncé.

- [x] Ajouter les périphériques musicaux validés.
  - [x] Créer les photographies sources dans `artifacts/controller-artwork-sources/` : `sony-beatmania.jpg`, `sony-guitar-freaks.jpg`, `sony-beatmania-iidx.jpg`, `sony-beatmania-iidx-arcade.jpg`, `sony-popn-music.jpg`, `sony-popn-music-arcade.jpg`, `sony-tatacon.jpg`, `sony-konami-ddr.jpg` ; retenir les modèles Konami/Namco d’origine, pas les copies modernes.
  - [x] Créer les PNG transparents correspondants sous `src/GWGUI.App/Assets/Controllers/`, maximum 512 pixels et moins de 1 Mo ; supprimer les seules photos de travail après contrôle.
  - [x] Créer leurs profils sous `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Sony/`, modifier les identifiants, le registre, le catalogue, les définitions des commandes musicales et les ressources de base : touches, disques, frettes, tambour et tapis selon leurs commandes réelles.

- [x] Consigner le lot musical et poursuivre les périphériques USB.
  - [x] Modifier `docs/reference/sony-controller-sources.md` : ajouter les huit sources, dimensions, poids et commandes ; corriger la mention obsolète de compilation en cours et consigner les contrôles statiques sans compilation.
  - [x] Créer dans `artifacts/controller-artwork-sources/` les photos `sony-usb-keyboard.jpg`, `sony-usb-mouse.jpg`, `sony-netplay.jpg` : Sony SCPH-10240, SCPH-10230 et Logitech NetPlay, vérifier les vraies prises et commandes.
  - [x] Créer `src/GWGUI.App/Assets/Controllers/sony-usb-keyboard.png`, `sony-usb-mouse.png`, `sony-netplay.png` : détourages transparents de face, maximum 512 pixels et moins de 1 Mo ; supprimer leurs seules photos temporaires après contrôle.
  - [x] Créer les profils `Profiles/Sony/PlayStation2/SonyUsbKeyboard.cs`, `SonyUsbMouse.cs`, `SonyNetPlay.cs` et modifier le catalogue, les identifiants, les ressources et les fonctions d'entrée Sony existantes : clavier dans son seul onglet, souris dans son seul onglet ; NetPlay réellement double connexion (clavier USB, manette sur port manette), conserver les deux fonctions sans inventer une manette USB.

  - [x] Modifier `src/GWGUI.App/Controllers/Emulation/Input/EmulationInputSettingsController.cs` et `src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationInputSettingsLayout.cs` : afficher la photo du clavier dans son onglet, réserver les associations USB clavier/souris à leurs onglets dédiés, maintenir l'indication d'onglet même si l'émulateur ne prend pas encore le périphérique en charge.
  - [x] Modifier `tests/GWGUI.Tests/Interface/EmulationViews/ControllerArtworkCatalogTests.cs` : tenir compte du visuel de clavier dont les touches se configurent dans l'onglet Clavier, sans lancer de compilation ; modifier `docs/reference/sony-controller-sources.md` pour consigner les trois photos et le raccordement.

- [x] Ajouter les manettes et sticks PS2 sélectionnés.
  - [x] Créer dans `artifacts/controller-artwork-sources/` les photos `sony-logitech-cordless.jpg`, `sony-logitech-cordless-action.jpg`, `sony-sega-saturn-ps2.jpg`, `sony-hori-fighting-stick-2.jpg`, `sony-hori-real-arcade-pro.jpg` : contrôler le vrai modèle PS2, exclure les pads Saturn d’origine et les variantes Hori PS3/PS4.
  - [x] Créer les cinq PNG transparents sous `src/GWGUI.App/Assets/Controllers/sony-*.png`, maximum 512 pixels et moins de 1 Mo ; supprimer seulement leurs photos de travail après contrôle.
  - [x] Créer les profils `Profiles/Sony/PlayStation2/SonyLogitechCordless.cs`, `SonyLogitechCordlessAction.cs`, `SonySegaSaturnPs2.cs`, `SonyHoriFightingStick2.cs`, `SonyHoriRealArcadePro.cs` ; modifier les constantes, le registre, le catalogue, les associations propres au modèle et les ressources de base, avec les fonctions physiques réellement présentes (MODE/VIBRATION Logitech, six boutons Sega et leurs correspondances imprimées, L3/R3 et SPEED/STICK du Hori Stick 2, huit sélecteurs turbo du Real Arcade Pro) ; créer `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/InputFunctions.Gamepads.cs` pour ces définitions, ajouter les contrôles visuels RepeatRate/StickMode dans `src/GWGUI.Emulation/Enums/EmulationControllerVisualControl.cs` et traduire les deux libellés Hori et les huit sélecteurs turbo numérotés de gauche à droite dans `src/GWGUI.Emulation.Sony/Resources/*/Option.resx` avec Argos.
  - [x] Modifier `docs/reference/sony-controller-sources.md` : consigner les cinq sources, poids et commandes, ainsi que les contrôles statiques sans compilation.

- [x] Ajouter les quatre volants USB Logitech PS2.
  - [x] Créer les photos sources `artifacts/controller-artwork-sources/sony-gt-force.jpg`, `sony-driving-force.jpg`, `sony-driving-force-pro.jpg`, `sony-driving-force-gt.jpg` : contrôler chaque modèle et son pédalier, retenir des vues permettant les halos de toutes les commandes visibles.
  - [x] Créer les quatre PNG transparents correspondants sous `src/GWGUI.App/Assets/Controllers/`, maximum 512 pixels et moins de 1 Mo ; supprimer leurs seules photos temporaires après contrôle.
  - [x] Créer les profils `Profiles/Sony/PlayStation2/SonyGtForce.cs`, `SonyDrivingForce.cs`, `SonyDrivingForcePro.cs`, `SonyDrivingForceGt.cs` et `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/InputFunctions.Wheels.cs` ; modifier les identifiants, le registre, les fonctions d'entrée et le catalogue existants : direction, pédales, boutons, palettes, levier et molette selon les vrais modèles, noms en base et nouveaux libellés traduits avec Argos dans toutes les langues ; déplacer les deux libellés SteeringLeft/SteeringRight non traduits de `src/GWGUI.Emulation.Sony/Resources/00-Base/Controller.resx` vers les catalogues Option et réutiliser les libellés communs Accélérateur/Frein existants. Ajouter les contrôles Accelerator, BrakePedal, GearUp/GearDown, DialLeft/DialRight/DialPress dans `src/GWGUI.Emulation/Enums/EmulationControllerVisualControl.cs` ; corriger le nouveau contrôle VIBRATION Logitech par un contrôle distinct VibrationMode dans cette énumération, les deux profils Logitech et `InputFunctions.Visuals.cs`, pour préserver le contrôle Guide des autres machines Sony.
  - [x] Modifier `docs/reference/sony-controller-sources.md` : consigner les quatre sources, dimensions, poids et commandes raccordées, ainsi que les contrôles statiques sans compilation.

- [x] Ajouter les variantes musicales, audio et SeaMic encore manquantes.
  - [x] Créer dans `artifacts/controller-artwork-sources/` les photos `sony-buzz-wireless.jpg`, `sony-singstar-wireless.jpg`, `sony-socom-headset.jpg`, `sony-logitech-vantage.jpg`, `sony-rock-band-microphone.jpg`, `sony-guitar-hero-microphone.jpg`, `sony-seamic.png`, `sony-kramer-striker.jpg` : vérifier les appareils PS2 d’origine ; SeaMic est une manette USB avec microphone, pas un simple micro sans boutons.
  - [x] Créer les huit PNG transparents correspondants sous `src/GWGUI.App/Assets/Controllers/` : Buzz! sans fil en quatre unités de face, SingStar sans fil par paire, autres périphériques complets sans câbles ; guitare Kramer dans sa finition d’origine sans les autocollants ajoutés sur l’exemplaire photographié ; maximum 512 pixels et moins de 1 Mo ; supprimer les seules sources temporaires après contrôle.
  - [x] Créer les profils `SonyBuzzWireless.cs`, `SonySingStarWireless.cs`, `SonySocomHeadset.cs`, `SonyLogitechVantage.cs`, `SonyRockBandMicrophone.cs`, `SonyGuitarHeroMicrophone.cs`, `SonySeamic.cs`, `SonyKramerStriker.cs` sous `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Sony/PlayStation2/` et `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/InputFunctions.Seamic.cs` ; modifier `ControllerArtworkFileNames.cs`, `SonyProfileRegistry.cs`, `EmulationControllerVisualIds.cs`, `ControllerCatalog.cs`, `InputFunctions.Usb.cs`, `InputFunctions.Visuals.cs` et `InputFunctions.Peripherals.cs` : vingt boutons Buzz!, guitare Kramer et commandes réelles du SeaMic ; aucun faux bouton de jeu sur les appareils audio qui n’en possèdent pas. Ajouter les noms invariants dans `src/GWGUI.App/Resources/00-Base/Emulation/EmulationController.resx`. Modifier aussi `src/GWGUI.App/Resources/*/Emulation/EmulationController.resx` avec Argos et relire le français : conserver une indication de non-compatibilité générale sans promettre des associations de boutons aux appareils audio qui n’en possèdent pas.
  - [x] Modifier `tests/GWGUI.Tests/Interface/EmulationViews/ControllerArtworkCatalogTests.cs` pour les appareils sans bouton de jeu et `docs/reference/sony-controller-sources.md` pour les huit sources, poids et commandes ; consigner les contrôles statiques, sans compilation.


- [x] Ajouter les deux périphériques PS2 Onimusha et Resident Evil.
  - [x] Créer `artifacts/controller-artwork-sources/sony-onimusha-katana.jpg`, `sony-onimusha-katana-back.jpg` et `sony-re4-chainsaw.jpg` depuis les références originales : HORI Soul Controller PS2 avec vues avant/arrière de la poignée démontable et texte du manuel consultable en ligne, NubyTech Resident Evil 4 PS2 rouge ; exclure la version GameCube jaune et identifier les vrais boutons. Le téléchargement du PDF est bloqué par le site et le rendu local n’a pas été exécuté (PyMuPDF absent) ; ne pas déclarer ces fichiers créés.
  - [x] Créer `src/GWGUI.App/Assets/Controllers/sony-onimusha-katana.png` et `sony-re4-chainsaw.png` : photos détourées complètes, contrôles de face lisibles, sans câbles, maximum 512 pixels, moins de 1 Mio ; supprimer seulement les deux sources après contrôle.
  - [x] Créer `Profiles/Sony/PlayStation2/SonyOnimushaKatana.cs` et `SonyRe4Chainsaw.cs` sous `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/` ainsi que `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/InputFunctions.SpecialControllers.cs` ; modifier `src/GWGUI.Emulation/Constants/EmulationControllerVisualIds.cs`, `ControllerArtworkFileNames.cs`, `Profiles/Sony/SonyProfileRegistry.cs`, `Common/Machines/Common/Dictionaries/ControllerCatalog.cs`, `InputFunctions.Usb.cs` et `InputFunctions.Visuals.cs` pour raccorder les boutons/axes réels et les gestes du modèle, indépendamment du support natif. Ajouter les noms dans `src/GWGUI.App/Resources/00-Base/Emulation/EmulationController.resx` et traduire les deux libellés de mouvement avec Argos dans `src/GWGUI.Emulation.Sony/Resources/*/Option.resx`.
  - [x] Modifier `docs/reference/sony-controller-sources.md` pour consigner les deux photos, exports, commandes et vérifications statiques, sans compilation.


- [x] Ajouter la télécommande DVD Sony et la canne ASCII Bass Landing.
  - [x] Créer `artifacts/controller-artwork-sources/sony-dvd-remote.jpg` et `sony-fishing-controller.png` : Sony SCPH-10150 et canne ASCII originale avec vue des commandes ; vérifier leurs boutons, stick et capteurs.
  - [x] Créer `src/GWGUI.App/Assets/Controllers/sony-dvd-remote.png` et `sony-fishing-controller.png` : détourage sans câbles, maximum 512 pixels et moins de 1 Mio ; supprimer seulement les deux sources après contrôle.
  - [x] Créer `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Sony/PlayStation2/SonyDvdRemote.cs`, `Profiles/Sony/PlayStation/SonyFishingController.cs` et `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/InputFunctions.RemoteAndFishing.cs` ; modifier `src/GWGUI.Emulation/Enums/EmulationControllerVisualControl.cs` (18 contrôles DVD distincts), `src/GWGUI.Emulation/Constants/EmulationControllerVisualIds.cs`, `ControllerArtworkFileNames.cs`, `Profiles/Sony/SonyProfileRegistry.cs`, `InputFunctions.Usb.cs` et `InputFunctions.Visuals.cs` pour les 49 touches DVD et les commandes de la canne ; les deux identifiants sont déjà prévus dans `ControllerCatalog.cs`. Ajouter les noms dans `src/GWGUI.App/Resources/00-Base/Emulation/EmulationController.resx` et vingt et un libellés traduits via Argos dans `src/GWGUI.Emulation.Sony/Resources/*/Option.resx`, en réutilisant les traductions de lecture/pause/arrêt/recherche existantes.
  - [x] Modifier `docs/reference/sony-controller-sources.md` pour consigner les sources, dimensions, poids, commandes et contrôles statiques sans compilation.


- [x] Ajouter les commandes Taito PS1 Densha de GO! et Jet de GO!.
  - [x] Créer dans `artifacts/controller-artwork-sources/` les photos `sony-densha-two-handle.jpg`, `sony-densha-one-handle.jpg`, `sony-densha-mamecon.jpg`, `sony-jet-controller.jpg` : vérifier les modèles PS1 Taito et les dispositions de leurs leviers/boutons.
  - [x] Créer les quatre PNG transparents correspondants dans `src/GWGUI.App/Assets/Controllers/`, maximum 512 pixels et moins de 1 Mio ; supprimer leurs seules photos temporaires après contrôle.
  - [x] Créer `SonyDenshaController.cs`, `SonyDenshaOneHandle.cs`, `SonyDenshaMamecon.cs`, `SonyJetController.cs` sous `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Sony/PlayStation/` et `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/InputFunctions.TrainAndJet.cs` ; modifier `EmulationControllerVisualIds.cs`, `ControllerArtworkFileNames.cs`, `SonyProfileRegistry.cs`, `ControllerCatalog.cs`, `InputFunctions.Peripherals.cs`, `InputFunctions.Usb.cs` et `InputFunctions.Visuals.cs` pour les leviers et boutons réellement présents, avec définitions propres au visuel. Ajouter six contrôles explicites de leviers dans `src/GWGUI.Emulation/Enums/EmulationControllerVisualControl.cs`, les noms dans `src/GWGUI.App/Resources/00-Base/Emulation/EmulationController.resx` et douze libellés traduits dans `src/GWGUI.Emulation.Sony/Resources/*/Option.resx` via Argos.
  - [x] Modifier `docs/reference/sony-controller-sources.md` : consigner références, exports et commandes, avec contrôles statiques sans compilation.


- [x] Ajouter les cinq commandes de train USB PS2 validées.
  - [x] Créer dans `artifacts/controller-artwork-sources/` les photos `sony-densha-usb.jpg`, `sony-densha-shinkansen.jpg`, `sony-densha-ryojohen.jpg`, `sony-train-mascon.jpg`, `sony-multi-train.jpg` : Type 2 TCPP-20009, Shinkansen TCPP-20011, Ryojōhen TCPP-20014, Train Mascon et Multi Train Controller PS2 ; vérifier chaque disposition et accessoire d’entrée.
  - [x] Créer les cinq PNG transparents correspondants sous `src/GWGUI.App/Assets/Controllers/`, maximum 512 pixels, moins de 1 Mio, sans câbles ; supprimer seulement leurs photos sources de travail après contrôle.
  - [x] Créer les cinq profils `SonyDenshaUsb.cs`, `SonyDenshaShinkansen.cs`, `SonyDenshaRyojohen.cs`, `SonyTrainMascon.cs`, `SonyMultiTrain.cs` sous `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Sony/PlayStation2/` et `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/InputFunctions.UsbTrains.cs` ; modifier `EmulationControllerVisualIds.cs`, `ControllerArtworkFileNames.cs`, `SonyProfileRegistry.cs`, `ControllerCatalog.cs`, `InputFunctions.TrainAndJet.cs`, `InputFunctions.Peripherals.cs`, `InputFunctions.Usb.cs` et `InputFunctions.Visuals.cs` pour les boutons et leviers réels de chaque modèle USB, avec définitions propres au visuel. Ajouter les douze contrôles physiques distincts dans `src/GWGUI.Emulation/Enums/EmulationControllerVisualControl.cs`, les noms dans `src/GWGUI.App/Resources/00-Base/Emulation/EmulationController.resx` et douze nouveaux libellés traduits avec Argos dans `src/GWGUI.Emulation.Sony/Resources/*/Option.resx` ; conserver ATS et A/B/C/D en valeurs invariantes.
  - [x] Modifier `docs/reference/sony-controller-sources.md` pour consigner les cinq sources, tailles, commandes et contrôles statiques, sans compilation.


- [x] Ajouter Flight Stick 2, Kids Station, Maestro et Tambourine.
  - [x] Créer dans `artifacts/controller-artwork-sources/` les photos `sony-hori-flight-stick-2.jpg`, `sony-kids-station.jpg`, `sony-maestro-baton.jpg`, `sony-tambourine.jpg` : modèles originaux/licenciés PS1/PS2, vérifier leurs commandes réelles.
  - [x] Créer les quatre PNG transparents correspondants sous `src/GWGUI.App/Assets/Controllers/`, maximum 512 pixels et moins de 1 Mio, sans câbles ; créer les vues de contrôle `artifacts/controller-artwork-sources/sony-flight-detail-1.jpg` et `sony-flight-detail-8.jpg`, puis supprimer seulement ces six photos de travail après contrôle.
  - [x] Créer `PlayStation/SonyKidsStation.cs`, `PlayStation/SonyMaestroBaton.cs`, `PlayStation/SonyTambourine.cs`, `PlayStation2/SonyHoriFlightStick2.cs` sous `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Sony/` et `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/InputFunctions.RarePeripherals.cs` ; modifier `ControllerArtworkFileNames.cs`, `SonyProfileRegistry.cs`, `InputFunctions.Usb.cs` et `InputFunctions.Visuals.cs` pour les touches, gestes et commandes de vol du modèle réel, en conservant les identifiants/catalogue déjà présents. Ajouter les quatre noms dans `src/GWGUI.App/Resources/00-Base/Emulation/EmulationController.resx` et les libellés dans toutes les langues `src/GWGUI.Emulation.Sony/Resources/*/Option.resx` avec Argos ; garder les inscriptions invariantes en commun.
  - [x] Modifier `docs/reference/sony-controller-sources.md` pour les quatre références, exports et contrôles statiques, sans compilation.


- [x] Ajouter les instruments PS2 restants et GameTrak (connexion réelle : DrumMania au port manette, autres modèles USB).
  - [x] Créer dans `artifacts/controller-artwork-sources/` les photos `sony-keyboardmania.jpg`, `sony-drummania.jpg`, `sony-parapara.jpg`, `sony-rock-band-guitar.jpg`, `sony-rock-band-drums.jpg`, `sony-guitar-hero-drums.jpg`, `sony-dj-hero.jpg`, `sony-gametrak.jpg` : références réelles des éditions PS2, commandes vérifiées.
  - [x] Créer les huit PNG correspondants sous `src/GWGUI.App/Assets/Controllers/`, détourés, sans câbles, maximum 512 pixels et moins de 1 Mio ; supprimer seulement leurs photos de travail après contrôle.
  - [x] Créer `SonyKeyboardMania.cs`, `SonyDrumMania.cs`, `SonyParaPara.cs`, `SonyRockBandGuitar.cs`, `SonyRockBandDrums.cs`, `SonyGuitarHeroDrums.cs`, `SonyDjHero.cs`, `SonyGameTrak.cs` sous `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Sony/PlayStation2/` et `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/InputFunctions.UsbInstruments.cs` ; modifier `ControllerCatalog.cs` (DrumMania au port manette PS2, visuels GuitarHeroDrums/DjHero), `EmulationControllerVisualIds.cs`, `ControllerArtworkFileNames.cs`, `SonyProfileRegistry.cs`, `InputFunctions.Usb.cs`, `InputFunctions.Visuals.cs` et `EmulationControllerVisualControl.cs` (touches musicales 10–24, cinq capteurs optiques, cinq frettes solo, cinq positions d’effet, onze surfaces de batterie, directions des fils GameTrak et deux directions du crossfader DJ Hero) pour les vrais boutons et capteurs. Ajouter les noms en base commune et traduire les nouveaux libellés dans tous les `src/GWGUI.Emulation.Sony/Resources/*/Option.resx` via Argos.
  - [x] Modifier `docs/reference/sony-controller-sources.md` avec références, dimensions, commandes et contrôles statiques sans compilation.


- [x] Terminer les quatre volants à port manette de la liste approuvée.
  - [x] Créer `artifacts/controller-artwork-sources/sony-mad-catz-wheel.jpg`, `sony-interact-v3.jpg`, `sony-fanatec-speedster-2.jpg` et `sony-fanatec-speedster-3.jpg`, plus `sony-speedster-2-detail.jpg` et `sony-speedster-3-detail.jpg`, plus `sony-mad-catz-pedals.jpg` : vues originales et commandes réelles, avec pédales correspondantes.
  - [x] Créer les quatre PNG transparents correspondants dans `src/GWGUI.App/Assets/Controllers/`, maximum 512 pixels et moins de 1 Mio, sans câbles ; supprimer seulement les sept photos de travail après contrôle.
  - [x] Créer `PlayStation/SonyMadCatzWheel.cs`, `PlayStation/SonyInteractV3.cs`, `PlayStation/SonyFanatecSpeedster2.cs`, `PlayStation2/SonyFanatecSpeedster3.cs` sous `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Sony/` et `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/InputFunctions.SerialWheels.cs` ; modifier `EmulationControllerVisualIds.cs`, `ControllerArtworkFileNames.cs`, `SonyProfileRegistry.cs`, `ControllerCatalog.cs`, `InputFunctions.Peripherals.cs`, `InputFunctions.Usb.cs` et `InputFunctions.Visuals.cs` pour les vraies commandes, avec définitions propres au modèle. Ajouter quatre noms dans `src/GWGUI.App/Resources/00-Base/Emulation/EmulationController.resx` ; réutiliser les traductions existantes des axes et leviers, garder MODE/SET/TUNING en inscriptions invariantes sans les dupliquer par langue.
  - [x] Modifier `docs/reference/sony-controller-sources.md` avec les quatre références et les contrôles statiques ; vérifier la liste approuvée contre les profils enregistrés, sans compilation.


- [x] Clôturer la vérification des périphériques et nettoyer les seuls fichiers de travail.
  - [x] Créer `artifacts/controller-artwork-sources/sony-sphere-manual.jpg` depuis le manuel original déjà référencé, maintenant accessible ; comparer les commandes au profil `Profiles/Sony/PlayStation/SonyAsciiSphere360.cs` et aux définitions de `InputFunctions.Peripherals.cs`, les modifier seulement si le manuel impose une correction ; modifier aussi `src/GWGUI.App/Assets/Controllers/sony-ascii-sphere-360.png` pour rétablir les quatre inscriptions 0/1/2/3 du sélecteur si la retouche les a tronquées.
  - [x] Modifier `docs/reference/sony-controller-sources.md` : consigner le résultat du manuel, la couverture de la liste approuvée, les contrôles finaux d’assets/registre/ressources et les limites de vérification sans compilation ; mettre à jour `docs/tasks/emulation/commun.md` en distinguant l’ancien suivi et l’interdiction de build actuelle.
  - [x] Supprimer seulement `artifacts/controller-artwork-sources/sony-sphere-manual.jpg` et `artifacts/export-controller-artwork.ps1` après leur dernier usage ; conserver toutes les images finales et les fichiers des autres fabricants.
  - [x] Modifier `docs/reference/sony-controller-sources.md` pour consigner le retrait effectif des deux derniers fichiers temporaires.


État de clôture : les reprises détaillées ci-dessus du catalogue Sony sont réalisées et vérifiées statiquement. Les anciennes entrées de suivi prévoyant une compilation avant l’instruction « AUCUN BUILD » sont historiques et annulées par cette instruction ; elles ne constituent pas des builds restant à lancer. Aucun test compilé n’est déclaré passé pour ces modifications. La compatibilité native sera traitée séparément, comme demandé.


- [x] Masquer les périphériques non acceptés par l’émulateur sélectionné, sans build.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Common/Machines/Common/Dictionaries/ControllerCatalog.cs` et `Functions/InputFunctions.Usb.cs` dans le même dossier Common/Machines/Common : filtrer les choix par les capacités existantes, ne pas afficher une ancienne sélection incompatible ni ses onglets clavier/souris.
  - [x] Modifier `src/GWGUI.App/Controllers/Emulation/Input/EmulationInputSettingsController.cs` et les trente `src/GWGUI.App/Resources/*/Emulation/EmulationController.resx` : retirer le message NotSupportedByEmulator et conserver les indications des onglets dédiés.
  - [x] Modifier `tests/GWGUI.Tests/Emulation/Sony/SettingsPresentationTests.cs` : remplacer le scénario autorisant Buzz non pris en charge, limiter le scénario clavier/souris à l’émulateur qui les accepte ; contrôler statiquement les modifications sans exécuter de compilation.


- [x] Raccorder les capacités USB déclarées par chaque adaptateur Sony à la sélection et à la transmission native, sans build.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Common/Interfaces/IEmulatorControllerAdapter.cs`, `Emulators/Common/Interop/Factories/CoreMachineFactory.cs`, `Emulators/Pcsx2/Factories/Pcsx2MachineFactory.cs` et `Emulators/Pcee2/Factories/Pcee2MachineFactory.cs` : déclarer les types USB acceptés par chaque cœur (LRPS2 clavier/souris, PCEE2 GunCon 2, aucun pour les autres adaptateurs sans prise en charge).
  - [x] Modifier `src/GWGUI.Emulation.Sony/Common/Machines/Common/Dictionaries/ControllerCatalog.cs` : utiliser les déclarations de l’adaptateur pour les choix et leur validation, retirer la liste USB indépendante. Modifier les deux `Emulators/Common/Interop/Services/ExternalCore.cs` et `Emulators/Pcsx2/Services/ExternalCore.cs` : initialiser également ArcadeStick avant chargement comme les autres périphériques utilisant JOYPAD.
  - [x] Modifier `tests/GWGUI.Tests/Emulation/Sony/ControllerPortResolutionTests.cs` et `SettingsPresentationTests.cs` : couvrir les identifiants natifs des périphériques compatibles JOYPAD et les choix USB propres à chaque cœur ; vérifier les sources sans compiler.
  - [x] Modifier `docs/reference/sony-controller-sources.md` : consigner les capacités des interfaces libretro vérifiées et les chemins de transmission existants, sans prétendre avoir exécuté les émulateurs.


- [x] Corriger les transitions USB clavier/souris/aucun et la conservation de la sélection Sony.
  - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Input/InputBindingEditor.xaml.cs`, `InputBindingEditorBindingFunctions.cs` et `src/GWGUI.App/Controllers/Emulation/Input/EmulationInputSettingsController.cs` : distinguer les touches identiques du clavier émulé des raccourcis remappés afin que les affectations par défaut ne bloquent pas les changements USB ; conserver la validation des remappages incompatibles.
  - [x] Modifier `tests/GWGUI.Tests/Interface/EmulationViews/ControllerNoneSelectionTests.cs` : reproduire les transitions clavier → souris → aucun avec les pages clavier/souris créées et vérifier les onglets décrits et le rechargement des choix après changement de machine ; libérer les contrôles en finally.

- [x] Retirer le dernier cadre Général LRPS2 signalé par l’utilisateur.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Pcsx2/Dictionaries/OptionCatalog.System.cs` et `Emulators/Pcsx2/Dictionaries/OptionCatalog.Cpu.cs` et `Emulators/Pcsx2/Functions/FileSystemFunctions.cs` : masquer dans les paramètres les options de déblocage des langues, GameIndex externe et améliorations propres aux jeux, ainsi que le sélecteur GameIndex.yaml ; conserver leurs valeurs natives existantes.


- [x] Vérifier et corriger les capacités multitap, ports manette et cartes mémoire des trois cœurs PS2.
  - [x] Modifier `docs/reference/sony-controller-sources.md` : relever les capacités exposées dans les sources officielles LRPS2/PCEE2/Play! et les écarts avec les adaptateurs existants, avant de définir les corrections nécessaires.
  - [x] Créer `src/GWGUI.Emulation.Sony/Emulators/Pcee2/Functions/MemoryCardFunctions.cs` : calculer les emplacements actifs et leurs noms de fichiers ; compléter la section INI MemoryCards avec les six emplacements de multitap selon la configuration. Modifier `Emulators/Common/Interop/Functions/MemoryCardFunctions.cs` pour utiliser le nombre et les noms de fichiers PCEE2, et `Common/Machines/Common/Functions/StorageFunctions.cs` pour conserver les indices physiques des cartes et nommer les slots 1B–1D/2B–2D ; modifier `Emulators/Pcee2/Functions/NetworkFunctions.cs` pour écrire les paramètres mémoire avec les autres paramètres INI déjà gérés.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/StorageFunctions.cs` : désactiver le libellé générique numéroté pour laisser apparaître les identifiants physiques MC 1B–1D/2B–2D.
  - [x] Modifier `tests/GWGUI.Tests/Emulation/Sony/SettingsPresentationTests.cs` : vérifier les 2/5/5/8 cartes selon le mode multitap, les noms natifs et les clés INI du bon port ; vérifier la conservation des sections INI étrangères sans créer de fichiers.
  - [x] Modifier `tests/GWGUI.Tests/Emulation/Sony/ExternalFileSettingsTests.cs` pour vérifier l’absence du dernier cadre Général sans perdre les valeurs internes ; compléter `tests/GWGUI.Tests/Interface/EmulationViews/ControllerNoneSelectionTests.cs` pour vérifier que les remappages réservés restent signalés tandis que les touches identiques du clavier restent valides.
  - [x] Modifier `docs/reference/sony-controller-sources.md` : consigner le raccordement réalisé et les contrôles statiques, sans build non demandé.


- [x] Reprendre les paramètres PSP / RPCS3 à partir des captures utilisateur du 17 octobre.
  - [x] Créer `docs/reference/sony-psp-rpcs3-capabilities.md` : relever le fonctionnement Memory Stick/commandes/modèles PPSSPP, stockage/connexions/réseau du cœur RPCS3 et les limites face à l’application autonome ; consigner le refus HTTP 403 du wiki fourni.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Functions/CoreSettingsFunctions.cs`, `Common/Machines/Common/Functions/StorageFunctions.cs`, `Emulators/Ppsspp/Services/ExternalCore.cs` et `src/GWGUI.Emulation.Sony/Modules/SonyEmulationModule.cs` : corriger les choix de résolution PSP, placer ses codes de triche dans Cheats, gérer le Memory Stick au dossier fixe existant avec ajout/retrait raccordé à l’option native et sans choix/ouverture de dossier ; nommer le lecteur UMD.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Rpcs3/Factories/Rpcs3MachineFactory.cs`, `Emulators/Common/Interop/Factories/CoreMachineFactory.cs`, `Emulators/Common/Interop/Functions/CoreSettingsFunctions.cs` et `Common/Machines/Common/Functions/StorageFunctions.cs` : organiser CPU/vidéo/réseau en blocs appropriés, nommer Blu-ray, séparer les options HDD/cache des options du lecteur optique. Créer `Emulators/Rpcs3/Functions/StorageFunctions.cs` pour représenter le stockage interne réellement géré par le cœur et ses options.
  - [x] Modifier `src/GWGUI.Emulation/Enums/EmulationStorageConfigurationKind.cs`, `src/GWGUI.Emulation/Contracts/EmulationStorageDeviceSettings.cs`, `src/GWGUI.App/Controllers/Emulation/Storage/EmulationStorageSettingsController.cs`, `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/StorageFunctions.cs` et créer `src/GWGUI.App/Views/Dialogs/Emulation/Storage/ManagedStorageConfigurationDialog.cs` : présenter les réglages du stockage interne géré sans faux sélecteur d’image disque ; conserver les fichiers existants.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Rpcs3/Factories/Rpcs3MachineFactory.cs` et `Common/Machines/Common/Functions/InputFunctions.Settings.cs`, `Emulators/Common/Interop/Services/ExternalCore.cs` : ajouter un choix du nombre de manettes, conserver les affectations des manettes masquées, transmettre leur nombre au cœur et identifier les emplacements sans les présenter comme des prises physiques.
  - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Input/EmulationControllerOutputEditor.cs` : utiliser un seul choix de manette physique pour ses deux moteurs de vibration, en conservant leurs associations natives distinctes.
  - [x] Créer les trois images transparentes optimisées `src/GWGUI.App/Assets/Controllers/sony-psp-2000.png`, `sony-psp-3000.png`, `sony-psp-go.png` depuis des photos de référence de face ; créer leurs profils sous `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Sony/` avec les halos des commandes natives vérifiées. Modifier `ControllerArtworkFileNames.cs`, `SonyProfileRegistry.cs`, `src/GWGUI.Emulation/Constants/EmulationControllerVisualIds.cs` et `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/InputFunctions.Visuals.cs` pour les raccorder sans inventer de boutons non exposés par PPSSPP.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Rpcs3/Dictionaries/OptionCatalog.cs` : retirer le statut global du cache disque afin de le présenter seulement dans le stockage interne.
  - [x] Créer `artifacts/translate-sony-settings.py` pour appliquer les traductions ciblées avec les modèles Argos existants, puis modifier `src/GWGUI.Emulation.Sony/Resources/00-Base/Option.resx`, les catalogues Option des 29 cultures et les ressources communes App correspondantes : réparer les libellés et valeurs techniques, ajouter les traductions manquantes avec Argos et relire le français ; garder les noms de modèles/unités invariants dans la base commune.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/StorageFunctions.cs` et `Emulators/Ppsspp/Constants/OptionConstants.cs` pour conserver le dossier Memory Stick lors du retrait et réutiliser la clé native ; modifier `Emulators/Common/Interop/Functions/CoreSettingsFunctions.cs` pour ranger les réglages RPCS3 de système et de rendu dans leurs onglets ; ajuster les halos des gâchettes dans `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Sony/PSP/SonyPsp2000.cs` et `SonyPsp3000.cs` aux photos finales.
  - [x] Modifier `src/GWGUI.App/Contracts/Emulation/Storage/EmulationStorageDeviceItem.cs`, `Controllers/Emulation/Storage/EmulationStorageSettingsController.cs` et `Views/Controls/Emulation/Storage/EmulationStorageDeviceList.cs` : afficher le support du stockage interne comme automatique avec la traduction commune existante, au lieu de « Non utilisé ».
  - [x] Modifier `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Sony/PSP/SonyPspGo.cs` : rapporter les positions verticales des halos à la hauteur finale réelle de 384 pixels.
  - [x] Modifier les tests sources Sony et interface concernés pour les résolutions, Cheats, Memory Stick, Blu-ray/stockage interne, nombre de manettes et moteurs de vibration ; vérifier la syntaxe C#, les ressources et les raccordements sans build non demandé. Supprimer `artifacts/translate-sony-settings.py` et uniquement les nouveaux fichiers temporaires d’audit/retouche après usage, conserver les images finales et les originaux.
  - [x] Modifier `docs/reference/sony-psp-rpcs3-capabilities.md` avec les changements réalisés, les limites confirmées et les vérifications effectives.


- [x] Corriger le déplacement des options LRPS2 et compléter les demandes du 17 octobre.
  - [x] Déplacer réellement les options du dernier cadre Général.
    - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Pcsx2/Dictionaries/OptionCatalog.System.cs`, `OptionCatalog.Cpu.cs` et `Emulators/Common/Interop/Functions/CoreSettingsFunctions.cs` : rétablir les trois options visibles dans une section Compatibilité de l’onglet Cheats, distincte de l’activation des codes ; conserver les traductions communes et le sélecteur GameIndex dans cette même section.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Sony/ExternalFileSettingsTests.cs` : vérifier leur présence hors Général, le chemin GameIndex et les conditions natives d’activation.
  - [x] Confirmer le stockage RPCS3 à chemins fixes.
    - [x] Modifier `docs/reference/sony-psp-rpcs3-capabilities.md` : préciser les chemins natifs et leur création automatique confirmée dans le cœur ; conserver les dossiers fixes sans sélecteur et sans déplacement des données.
  - [x] Ajouter l’affectation groupée des sticks dans l’éditeur commun.
    - [x] Créer `docs/reference/emulation-analog-bindings.md` : inventorier les contrats et modules analogiques existants, définir leur raccordement au choix direct d’une manette et d’un stick physique sans supprimer les associations directionnelles clavier.
    - [x] Créer `src/GWGUI.App/Views/Controls/Emulation/Input/InputBindingEditorAnalogFunctions.cs` et modifier `InputBindingEditor.xaml`, `InputBindingEditor.xaml.cs`, `InputBindingEditorBindingFunctions.cs` dans le même dossier : proposer une affectation groupée de manette/stick ou d’axe réel à partir des paires existantes, synchroniser les choix avec les valeurs sauvegardées et garder les directions clavier configurables pour tous les modules utilisant cet éditeur.
    - [x] Créer `artifacts/translate-analog-bindings.py` et modifier les 30 `src/GWGUI.App/Resources/*/Emulation/EmulationController.resx` : traduire avec Argos les libellés communs des sticks, axes et associations personnalisées, relire le français.
    - [x] Modifier `tests/GWGUI.Tests/Interface/EmulationViews/ControllerNoneSelectionTests.cs` : vérifier les groupes, la sélection physique, la conservation des directions clavier, les indices d’axes non consécutifs et la relecture des valeurs ; vérifier statiquement le C#, le XAML et les ressources, sans compilation.
    - [x] Supprimer seulement `artifacts/translate-analog-bindings.py` après usage ; modifier `docs/reference/emulation-analog-bindings.md` et `docs/tasks/emulation/commun.md` pour consigner les résultats et limites effectifs.


- [x] Commit et build demandés le 17 octobre.
  - [x] Modifier `docs/tasks/emulation/commun.md` pour consigner le contrôle du diff et inclure l’ensemble des modifications de cette reprise dans le commit Git.
  - [x] Modifier `src/GWGUI.App/Controllers/Emulation/Storage/EmulationStorageSettingsController.cs` : supprimer l’argument de statut automatique ajouté par erreur à l’appel du dialogue optique ; conserver ce statut uniquement sur la ligne du stockage géré.
  - [x] Créer ou mettre à jour `build/Debug/GW GUI/gwgui.exe` et les 23 modules de `build/Debug/GW GUI/Modules/` avec `scripts/local-building.cmd --building=debug --modules=A` hors bac à sable ; corriger les fichiers sources concernés si la compilation révèle une erreur.
  - [x] Modifier `docs/tasks/emulation/commun.md` avec le résultat effectif du script et le contrôle des sorties ; inclure ce suivi et les éventuelles corrections dans le commit final.


Résultat du build demandé le 17 octobre : premier appel arrêté sur l’argument surnuméraire du dialogue optique, corrigé dans le contrôleur. Second appel `scripts/local-building.cmd --building=debug --modules=A` terminé avec code 0 ; application, lanceur, updater et 23 modules compilés. Vérification effective de `build/Debug/GW GUI/gwgui.exe` et de l’assembly d’entrée et du manifeste des 23/23 modules sous `Modules/<id>/`. Avertissements CA1416 et CS8602 signalés dans les modules ; aucune erreur au second appel. Les tests automatisés n’ont pas été exécutés par cette commande.


- [x] Suivre la langue de l’application et retirer les axes bruts redondants des manettes reconnues.
  - [x] Adapter la langue du système émulé.
    - [x] Créer `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Functions/SystemLanguageFunctions.cs` : convertir la culture d’interface de l’application vers les valeurs natives PPSSPP/RPCS3, avec repli anglais pour une langue non prise en charge ; modifier `CoreSettingsFunctions.cs` et `Emulators/Ppsspp/Functions/PpssppOptionFunctions.cs` pour appliquer cette valeur à l’exécution et retirer son sélecteur des paramètres utilisateur.
    - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Functions/CoreSettingsFunctions.cs` : déplacer les notifications RPCS3 dans Vidéo, la compatibilité des jeux dans Cheats, la région dans ROM et le bouton de confirmation dans les options globales Manettes. Modifier les 30 `src/GWGUI.Emulation.Sony/Resources/*/Option.resx` avec Argos pour le cadre Notifications.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Sony/SettingsPresentationTests.cs` et `ConfigurationPresentationTests.cs` pour couvrir les langues natives, la priorité sur les anciennes sélections et l’absence de sélecteur.
  - [x] Retirer les choix d’axes bruts qui doublonnent les sticks standard.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Input/InputBindingEditorAnalogFunctions.cs` : proposer uniquement les sticks gauche/droit pour une manette reconnue, conserver les axes réellement déclarés pour les appareils sans vue gamepad standard.
    - [x] Modifier `tests/GWGUI.Tests/Interface/EmulationViews/ControllerNoneSelectionTests.cs` : vérifier qu’une manette standard avec axes bruts n’affiche pas les combinaisons redondantes, en conservant le cas des axes non standard 2/7.
  - [x] Modifier `docs/reference/sony-psp-rpcs3-capabilities.md` et `docs/reference/emulation-analog-bindings.md` avec le raccordement et les contrôles statiques effectifs ; ne pas lancer de build non demandé.


- [x] Réorganiser les cadres CPU RPCS3.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Functions/CoreSettingsFunctions.cs` pour séparer précision PPU, précision SPU, exécution SPU, recompilation, compatibilité et synchronisation ; modifier les 30 `src/GWGUI.Emulation.Sony/Resources/*/Option.resx` avec Argos pour les intitulés propres à la PS3, sans référence à l’Emotion Engine.
  - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs` pour présenter les petits cadres CPU par deux avec le conteneur adaptatif existant, et conserver la largeur entière des cadres qui ont besoin de place ; modifier `tests/GWGUI.Tests/Emulation/Sony/SettingsPresentationTests.cs` pour vérifier le classement et les titres.
  - [x] Modifier `docs/reference/sony-psp-rpcs3-capabilities.md` et `docs/tasks/emulation/commun.md` avec les vérifications statiques, sans build non demandé.


- [x] Commit et build Debug des corrections de langue, de manettes et de présentation CPU.
  - [x] Modifier `docs/tasks/emulation/commun.md` pour consigner la demande de commit et de build Debug de l’application et de tous les modules.
  - [x] Enregistrer les fichiers modifiés dans un commit Git local (`.git/index` et objets Git).
  - [x] Créer ou mettre à jour `build/Debug/GW GUI/gwgui.exe` et `build/Debug/GW GUI/Modules/*` avec `scripts/local-building.cmd --building=debug --modules=A`, puis vérifier l’exécutable et chaque module.
  - [x] Modifier `docs/tasks/emulation/commun.md` pour consigner le résultat effectif du build, puis intégrer ce suivi au commit local.

  Résultat : script terminé avec le code 0 ; exécutable et manifestes/DLL des 23 modules vérifiés. Avertissements CA1416 et CS8602 dans SNK ; aucun contrôle visuel effectué.


- [x] Différer le chargement des supports tant que la machine est arrêtée.
  - [x] Modifier `src/GWGUI.App/Services/Emulation/MachineSession.cs` pour conserver les insertions/éjections à l’arrêt dans la session, différer la recréation et sa validation au prochain allumage, et conserver la libération de la machine précédente.
  - [x] Modifier `tests/GWGUI.Tests/Emulation/EmulationContracts/SessionLifecycleScenarios.cs` et `EmulationContractsTests.cs` pour couvrir les changements hors tension, leur transmission à l’allumage et la reprise après rejet d’une combinaison de supports.
  - [x] Modifier `docs/tasks/emulation/commun.md` avec le diagnostic confirmé par le journal et les vérifications statiques ; ne pas lancer de nouveau build non demandé.

  Diagnostic : le journal du 17 octobre 2026 à 19:29 confirme le rejet de `Media` par
  la validation MiniVMac. La configuration enregistrée contient HD0 inséré ; l’ajout
  d’une disquette fournit deux supports au validateur. La session recréait la machine
  lors de chaque insertion/éjection hors tension. Elle conserve désormais cette sélection
  et diffère la recréation au prochain allumage, y compris après une mise à jour de sa
  fabrique hors tension. La limite MiniVMac existante n’est pas supprimée : cette
  correction ne prétend pas ajouter le chargement simultané de deux disques au cœur.
  Les scénarios existants vérifient le chargement différé pour les deux valeurs de
  RequiresMachineRecreation et l’absence d’appel natif hors tension ; un scénario
  couvre le rejet à l’allumage, l’éjection puis une nouvelle tentative réussie.
  Vérifications effectuées : syntaxe Roslyn des trois fichiers C#, `git diff --check`.
  Tests modifiés mais non exécutés ; aucun nouveau build ni modification des fichiers
  de configuration ou des images personnelles.


- [ ] Commit de la correction des supports hors tension.
  - [x] Modifier `docs/tasks/emulation/commun.md` pour consigner la demande de commit sans build.
  - [ ] Enregistrer les quatre fichiers modifiés dans les objets Git et `.git/index`, puis intégrer le suivi terminé au commit.
