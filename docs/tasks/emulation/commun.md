# Travaux communs restants

Dernière qualification : build Debug réussi, application et 23 modules présents ; 73 tests ciblés des axes analogiques et du nettoyage WPF réussis. Les fenêtres et le thread de fixture sont fermés par le helper central. Le démarrage réel de chaque machine avec son firmware et ses médias reste à qualifier.

Qualification Acorn : 38 contrôles Acorn et vues WPF existantes réussis (configuration, persistance B+, mémoire Opus et vraie vue WPF). La vue accepte Joystick → Aucun → Joystick avec visuel et associations vides ; sa fenêtre est fermée par le helper central, puis le thread de fixture est détruit. Audit RESX réussi : 28 cultures. Build Debug final réussi avec code 0 ; gwgui.exe et les 23 DLL de modules vérifiés dans build/Debug/GW GUI. Le profil Playdia vide a ensuite été retiré : le cœur HLE utilise désormais le visuel NES existant avec ses huit commandes virtuelles ; la photo de console a été supprimée de Controllers. Les 20 contrôles Bandai et du catalogue global des zones passent, code 0. Build Debug complet réussi ; application et 23 DLL de modules vérifiées. Aucune association physique initiale ajoutée.

Les qualifications précédentes restent acquises : 83 contrôles des six premières familles, 34 contrôles architecturaux et de manifestes ; les 23 projets Common sont découverts automatiquement. Stop puis Dispose préservent Stopped dans les quinze nouvelles familles et attendent le thread ; le parcours natif Wenquxing a confirmé la libération des cinq sessions.

## controller-artwork

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
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : documenter la géométrie saillante des boutons et l'état réel de la validation.
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner la vérification du build Debug après ce dernier ajustement.
    - [ ] Reprendre le HSS-0104 selon l'avis de l'utilisateur après le CX52
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : enregistrer sa validation ou le défaut précis restant et prévoir la correction correspondante avant de passer à une autre manette.
    - [ ] Terminer la répartition du registre des profils par marque
      - [ ] Créer `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Microsoft/MicrosoftProfileRegistry.cs` : enregistrer les profils Xbox dans leur propre dictionnaire.
      - [ ] Créer `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Nintendo/NintendoProfileRegistry.cs` : enregistrer les profils Nintendo dans leur propre dictionnaire.
      - [ ] Créer `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Generic/GenericProfileRegistry.cs` : enregistrer les joysticks multiplateformes dans leur propre dictionnaire.
      - [ ] Créer `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Amiga/AmigaProfileRegistry.cs` : enregistrer les profils Amiga dans leur propre dictionnaire.
      - [ ] Créer `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Atari/AtariProfileRegistry.cs` : enregistrer les profils Atari dans leur propre dictionnaire.
      - [ ] Créer `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Sega/SegaProfileRegistry.cs` : enregistrer les profils Sega dans leur propre dictionnaire.
      - [ ] Modifier `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkCatalog.Profiles.cs` : garder seulement l'assemblage du dictionnaire des marques.
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-inventory.md` : décrire la répartition vérifiée et le résultat du build de l'application.
    - [ ] Terminer commodore-cd32.png
      - [ ] Créer `artifacts/controller-sources/compose-amiga-cd32-controller.ps1` : composer la vue de face existante avec deux petites découpes photographiques montrant uniquement les gâchettes, en préservant leurs pixels.
      - [ ] Modifier `src/GWGUI.App/Assets/Controllers/commodore-cd32.png` : afficher ces découpes avec le contrôleur existant sur fond transparent.
      - [ ] Modifier `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/Profiles/Amiga/Joysticks/CommodoreCd32.cs` : placer les halos des gâchettes sur les découpes réelles.
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `commodore-cd32.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] competition-pro-cd32.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `competition-pro-cd32.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] dreamcast.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `dreamcast.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] flight-stick.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `flight-stick.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] generic-gamepad.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `generic-gamepad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] konix-speedking-left-hand.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `konix-speedking-left-hand.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] konix-speedking-right-hand.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `konix-speedking-right-hand.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] mattel-hyperscan-controller.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `mattel-hyperscan-controller.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] mattel-intellivision-controller.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `mattel-intellivision-controller.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] mega-drive-6.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `mega-drive-6.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-arcade-pad-6.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nec-arcade-pad-6.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-avenue-pad-3.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nec-avenue-pad-3.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-avenue-pad-6.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nec-avenue-pad-6.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-cordless-pad.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nec-cordless-pad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-coregrafx-ii-turbopad.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nec-coregrafx-ii-turbopad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-coregrafx-turbopad.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nec-coregrafx-turbopad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-duopad.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nec-duopad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-duor-turbopad.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nec-duor-turbopad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-pc-engine-lt-controls.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nec-pc-engine-lt-controls.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-pc-engine-mouse.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nec-pc-engine-mouse.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-pc-engine-pad.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nec-pc-engine-pad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-pc-engine-turbopad-ii.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nec-pc-engine-turbopad-ii.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-pc-engine-turbopad.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nec-pc-engine-turbopad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-pc-engine-turbostick.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nec-pc-engine-turbostick.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-pc-fx-mouse.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nec-pc-fx-mouse.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-pc-fx-pad.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nec-pc-fx-pad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-turboexpress-controls.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nec-turboexpress-controls.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-turbografx-turbopad.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nec-turbografx-turbopad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nec-turbografx-turbostick.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nec-turbografx-turbostick.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nintendo-64.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nintendo-64.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nintendo-game-boy-advance.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nintendo-game-boy-advance.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nintendo-game-boy-color.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nintendo-game-boy-color.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nintendo-game-boy.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nintendo-game-boy.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nintendo-gamecube-wavebird.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nintendo-gamecube-wavebird.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nintendo-gamecube.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nintendo-gamecube.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nintendo-nes-advantage.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nintendo-nes-advantage.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nintendo-nes-dogbone.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nintendo-nes-dogbone.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] nintendo-nes-max.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `nintendo-nes-max.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] philips-cdi-gamepad.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `philips-cdi-gamepad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] playstation-1.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `playstation-1.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] playstation-2.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `playstation-2.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] playstation-4.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `playstation-4.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] playstation-5.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `playstation-5.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] quickshot-deluxe.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `quickshot-deluxe.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] quickshot-ii-turbo.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `quickshot-ii-turbo.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] quickshot.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `quickshot.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] racing-wheel.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `racing-wheel.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] saturn-3d-control-pad.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `saturn-3d-control-pad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] saturn.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `saturn.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] sega-arcade-power-stick-3.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `sega-arcade-power-stick-3.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] sega-arcade-power-stick-6.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `sega-arcade-power-stick-6.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] sega-control-stick.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `sega-control-stick.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] sega-saturn-virtua-stick-hss0104.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `sega-saturn-virtua-stick-hss0104.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] sega-saturn-virtua-stick-hss0136.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `sega-saturn-virtua-stick-hss0136.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] sega-sg1000-ii.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `sega-sg1000-ii.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] sega-sg1000-joystick.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `sega-sg1000-joystick.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] sony-dualshock-1.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `sony-dualshock-1.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] sony-psp-1000.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `sony-psp-1000.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] super-nes-controller.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `super-nes-controller.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] super-nintendo.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `super-nintendo.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-360-big-button-pad.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `xbox-360-big-button-pad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-360-black.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `xbox-360-black.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-360-chatpad.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `xbox-360-chatpad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-360-transforming-dpad.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `xbox-360-transforming-dpad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-360-white.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `xbox-360-white.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-360-wireless-racing-wheel.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `xbox-360-wireless-racing-wheel.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-360-wireless-speed-wheel.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `xbox-360-wireless-speed-wheel.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-chatpad.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `xbox-chatpad.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-controller-s.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `xbox-controller-s.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-duke.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `xbox-duke.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-dvd-movie-playback-kit.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `xbox-dvd-movie-playback-kit.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-one.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `xbox-one.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-rematch-core.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `xbox-rematch-core.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] xbox-series.png
      - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : consigner pour `xbox-series.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
  - [ ] Créer `artifacts/controller-sources/cutout-duke.ps1` : préparer un détourage déterministe de la photo frontale du Duke et deux petites découpes de sa photo arrière montrant les gâchettes, sans modifier les pixels du matériel.
  - [ ] Modifier `src/GWGUI.App/Assets/Controllers/xbox-duke.png` : remplacer l'illustration inventée par les pixels du matériel photographié, avec seul le fond détouré de manière déterministe.
  - [ ] Modifier `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkCatalog.cs` : replacer les zones de la manette Duke sur ses commandes photographiées.
  - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : créditer la photo Duke et consigner les contrôles effectués.
  - [ ] Créer `artifacts/controller-sources/xbox-dvd-remote-original.jpg` : conserver une photographie de face de la télécommande DVD Xbox d'origine.
  - [ ] Modifier `src/GWGUI.App/Assets/Controllers/xbox-dvd-movie-playback-kit.png` : remplacer la télécommande reconstruite par la photographie du vrai modèle, fond seulement détouré.
  - [ ] Modifier `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkCatalog.cs` : replacer les halos sur les touches réelles de la télécommande.
  - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : créditer la photo de la télécommande et noter le contrôle des touches.
  - [ ] Créer `artifacts/controller-sources/xbox-controller-s-original.jpg` : conserver une photographie de face du Microsoft Controller S original.
  - [ ] Modifier `src/GWGUI.App/Assets/Controllers/xbox-controller-s.png` : remplacer l'illustration par la photo détourée sans changement des boutons Black/White.
  - [ ] Modifier `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkCatalog.cs` : recalculer les halos du Controller S.
  - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : créditer et vérifier le Controller S.
  - [ ] Créer `artifacts/controller-sources/xbox-chatpad-original.png` : conserver une photographie de face du Chatpad Microsoft pour Xbox One.
  - [ ] Modifier `src/GWGUI.App/Assets/Controllers/xbox-chatpad.png` : remplacer les touches inventées par les pixels de la photographie, avec détourage du fond uniquement.
  - [ ] Modifier `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkCatalog.cs` : recalculer les zones du Chatpad d'après les touches réelles.
  - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : créditer et vérifier le Chatpad Xbox One.
  - [ ] Créer `artifacts/controller-sources/sony-psp-1000-original.png` : conserver la source photographique de face du modèle PSP-1000.
  - [ ] Modifier `src/GWGUI.App/Assets/Controllers/sony-psp-1000.png` : remplacer toute vue oblique ou reconstruction par la vue de face photographiée, fond détouré seulement.
  - [ ] Modifier `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkCatalog.cs` : replacer les halos de la PSP-1000.
  - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : créditer et vérifier la PSP-1000.
  - [ ] Créer `artifacts/controller-sources/sony-dualshock-1-original.jpg` : conserver une photo de face de la DualShock d'origine.
  - [ ] Modifier `src/GWGUI.App/Assets/Controllers/sony-dualshock-1.png` : remplacer l'image oblique par la photo de face du modèle exact, détourée sans reconstruire les commandes.
  - [ ] Modifier `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkCatalog.cs` : replacer les halos de la DualShock d'origine.
  - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : créditer et vérifier la DualShock d'origine.
  - [ ] Créer `artifacts/controller-sources/sega-sg1000-joystick-original.jpg` : conserver une photo de dessus droite du joystick Sega SG-1000 exact.
  - [ ] Modifier `src/GWGUI.App/Assets/Controllers/sega-sg1000-joystick.png` : remplacer la vue fausse ou inclinée par la photo du vrai joystick, détourée uniquement.
  - [ ] Modifier `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkCatalog.cs` : replacer les halos du joystick SG-1000.
  - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : créditer et vérifier le joystick SG-1000.
  - [ ] Créer `artifacts/controller-sources/sega-sg1000-ii-original.jpg` : conserver une photo de face droite du contrôleur Sega SG-1000 II exact.
  - [ ] Modifier `src/GWGUI.App/Assets/Controllers/sega-sg1000-ii.png` : remplacer la vue fausse ou inclinée par la photo de la vraie manette, détourée uniquement.
  - [ ] Modifier `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkCatalog.cs` : replacer les halos SG-1000 II.
  - [ ] Modifier `docs/tasks/emulation/controller-artwork-fidelity.md` : créditer et vérifier la manette SG-1000 II.
  - [ ] Modifier `docs/tasks/emulation/controller-artwork.md` : détailler les autres remplacements Xbox 360, NEC, Nintendo et Virtua Stick après ce lot prioritaire.
  - [ ] Modifier `docs/tasks/emulation/controller-artwork-inventory.md` : consigner le build Debug et la présence de l'application et des huit modules.
- [ ] Vérifier la couverture et le fonctionnement
  - [ ] Modifier `src/GWGUI.App/Assets/Controllers/nec-arcade-pad-6.png` : retirer les quelques pixels détachés à l'extérieur de la silhouette sans altérer les boutons.
  - [ ] Modifier `src/GWGUI.App/Assets/Controllers/nec-pc-engine-turbostick.png` : nettoyer les pixels résiduels du fond sur les bords du PI-PD4 sans toucher aux boutons ; les retouches génératives tentées jusqu'ici conservent ces pixels.
  - [ ] Modifier `src/GWGUI.App/Assets/Controllers/nintendo-gamecube-wavebird.png` : retirer les franges colorées hors de la silhouette par une méthode expressément autorisée, sans altérer les commandes.
  - [ ] Modifier `docs/tasks/emulation/controller-artwork-inventory.md` : consigner l'état de l'alpha WaveBird après nettoyage et la vérification des halos dans l'interface.
  - [ ] Modifier `docs/tasks/emulation/controller-artwork-inventory.md` : consigner la couverture réelle des cinq modules, les contrôles image/transparence/zones, le résultat du build Debug de l'application et des modules et les essais fonctionnels de sélection et de halos.

## remaining-machines-emulators

- [ ] Compléter les systèmes de liste-emulateur-libretro.md
  - [ ] Établir la couverture et les actions par module
  - [ ] Compléter Nintendo
  - [ ] Compléter les autres associations, machines et familles
    - [ ] Modifier `docs/tasks/emulation/remaining-machines-emulators.md` : détailler les prochaines intégrations vérifiées avant tout changement de leurs fichiers.
  - [ ] Valider l’ensemble
    - [ ] Modifier `docs/tasks/emulation/remaining-machines-emulators.md` : noter la couverture finale, les contrôles pertinents et le résultat du build Debug avec tous les modules.

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

- [ ] Réutiliser les types communs et retirer les entrées redondantes
  - [ ] Établir les corrections concrètes avant modification de code
    - [ ] Inventorier puis qualifier chaque donnée concernée.
    - [ ] Remplacer la liste générique de clavier Commodore par les seules touches propres aux machines.
    - [ ] Modifier `docs/tasks/emulation/revision-types-entrees-inventaire.json` : qualifier les autres constantes/enums et touches spéciales de tous les modules, en distinguant protocole, données partagées et compatibilité persistée ; décisions restantes explicitement non terminées.
    - [ ] Modifier `docs/tasks/emulation/revision-types-entrees.md` : développer dans l'ordre les actions de code identifiées par l'inventaire, avec chemins réels, contenu à remplacer et appelants à migrer ; traiter SDK puis propriétaires Common puis adaptateurs puis descriptions/persistance/session puis ressources de toutes les langues, avant la vérification finale.

- [ ] Commiter les corrections restantes autorisées.
  - [ ] Commiter tous les fichiers restants, y compris le catalogue Libretro restauré, les modules, le SDK, les ressources, les registres et la restauration des outils, sans push.

### Entrées mixtes et correction Acorn — vérification actuelle

Les 94 tests ciblés `ControllerInputTests`, `EmulationViewsTests` et `AcornModuleTests` passent. Le script `scripts/local-building.cmd --building=debug --modules=A` a terminé avec le code 0 ; `build/Debug/GW GUI/gwgui.exe` et les assemblages des 23 manifestes de modules sont présents. La capture conserve plusieurs boutons détaillés simultanés et attend le dernier relâchement après un réappui. La rangée supérieure et le pavé numérique restent distincts ; aucune suppression automatique de MAJ/AltGr ni interdiction générale des touches alphanumériques n'a été ajoutée.

Ce résultat ne vaut pas validation matérielle de l'interception Windows/GameInput. Les contrôles analogiques et les doubles représentations ont été corrigés et vérifiés dans la tranche suivante.

Contrôles analogiques terminés : pression composée vérifiée dans les neuf consommateurs (Mattel, Philips, Sharp, Sinclair, SNK, Thomson, Watara, Panasonic, Epoch), sous la zone morte, à mi-course et sans modificateur maintenu. Les 94 tests ciblés passent après ajout des références de modules nécessaires dans le projet de tests. Joystick est conservé uniquement dans la base commune ; les 29 copies localisées ont été retirées. Les nouveaux libellés SHIFT allemand et espagnol sont Umschalt et Mayús ; le classement des autres inscriptions reste à examiner avant de prétendre à une revue exhaustive.

La tranche Acorn/associations physiques est compilée et ses 94 tests ciblés passent. Les 191 identités EmulationKey ont été regroupées par fonction et famille en conservant leurs valeurs numériques explicites ; les tests restent verts après ce regroupement. Le dernier build complet a livré les 23 modules, l'exécutable et tous les entryAssembly déclarés. Aucun parcours matériel réel supplémentaire n'est déclaré validé par ces tests.

- [x] Compléter la vérification des inscriptions de modificateurs dans les autres cultures.
  - [x] Modifier `src/GWGUI.App/Resources/*/Emulation/EmulationKeyboard.resx` uniquement pour les inscriptions locales attestées différentes de SHIFT, après comparaison avec les noms clavier officiels ; conserver les identités LeftShift/RightShift et les valeurs réellement identiques dans 00-Base.

SHIFT vérifié dans les 29 pages Microsoft locales, via la combinaison de menu contextuel F10. Les cinq cultures supplémentaires da-DK, fi-FI, it-IT, nb-NO et sv-SE affichent désormais Skift, Vaihto, MAIUSC, Skift et Skift. Les 21 cultures employant Shift utilisent la base commune ; les huit variantes locales ont une entrée distincte. XML et unicité des clés vérifiés dans tous les catalogues concernés.

- [x] Terminer le format des anciennes photographies de machines encore hors 512 px.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Assets/Machines/BbcMaster128.png` et `src/GWGUI.Emulation.Acorn/Assets/Machines/BbcMicro.png` : redimensionner classiquement les photographies originales sur des PNG transparents 512×512, sans génération et sans changer leurs proportions ; tailles actuelles 330×214 et 500×348.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Assets/Machines/AppleII.png` et `src/GWGUI.Emulation.Apple/Assets/Machines/Macintosh128K.png` : redimensionner classiquement les photographies originales sur des PNG transparents 512×512, sans génération et sans changer leurs proportions ; tailles actuelles 330×304 et 330×387.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Assets/Machines/NeoGeoMvs.png` : redimensionner classiquement la photographie originale sur un PNG transparent 512×512, sans génération et sans changer ses proportions ; taille actuelle 323×187.

Audit actuel des 16 familles demandées : tous les manifestes `module.json` et registres `module-registry/<id>.json` sont présents. Les cinq photographies ci-dessus ont déjà de vrais pixels transparents, vérifiés avec les bitmaps chargés puis libérés dans finally ; le PNG BBC Micro est indexé et sa transparence ne doit pas être déduite du seul type de couleur IHDR. L'existence des registres et dimensions ne prouve pas encore la qualification native de chaque moteur ou la fidélité visuelle de toutes les images.

Acorn : les deux photographies originales sont maintenant centrées sur une toile transparente 512×512. Redimensionnement classique validé par l'utilisateur ; aucun détail reconstruit ni généré. Les images finales ont été affichées et contrôlées. Les imperfections de détourage présentes dans les originaux ne sont pas déclarées corrigées par ce seul redimensionnement.

Les cinq redimensionnements classiques sont terminés, contrôlés visuellement et vérifiés en PNG 512×512 avec fond transparent. Les photographies originales restent les sources : aucune modification de légende, touche, logo, angle ou proportion n'a été introduite. La variante générée du BBC Master, qui inventait des détails, n'a jamais été intégrée au dépôt.

- [x] Consigner la livraison et la couverture déclarée.
  - [x] Modifier `docs/tasks/emulation/commun.md` : noter le build Debug réussi, la présence des 23 modules et des cinq PNG embarqués 512×512 transparents ; distinguer la présence des références DLL de leur qualification native.
  - [x] Modifier `docs/tasks/emulation/methode-modules.md` : comparer le champ machine des tableaux au propriétaire réel ; utiliser les URL SourceUri du module avant toute recherche de dépôt.

Livraison vérifiée après redimensionnement : `scripts/local-building.cmd --building=debug --modules=A` terminé avec code 0 ; `build/Debug/GW GUI/gwgui.exe`, 23 manifestes et leurs entryAssembly sont présents. Les cinq flux PNG ont été ouverts depuis les ressources des DLL livrées : dimensions 512×512 et pixel de fond transparent confirmés, streams et bitmaps libérés dans finally.

La comparaison des références DLL des 16 familles avec les colonnes Machines du catalogue inclut les autres titres (Thomson, Wenquxing, 3DO, ColecoVision/blueMSX). Aucun nom de DLL d'émulation attendu n'est absent du code hors CDi2015 retiré explicitement. RemoteJoy correspond à la fonction d'affichage distant PSP du tableau ; sa présence n'a pas été déclarée comme un moteur d'émulation. Cette comparaison textuelle ne prouve pas l'implémentation ou la qualification complète des adaptateurs. Le BBC Micro Model A demeure sans entrée native dans `B2ModelCatalog` et `B2ModelFunctions.DefaultValue` : la couverture des modèles Acorn reste donc incomplète, même si la DLL b2 est référencée.

- [x] Consigner la limite native confirmée de b2 sans simuler un autre modèle.
  - [x] Modifier `docs/tasks/emulation/liste-emulateur-libretro.md` : ajouter après la table Acorn la distinction entre le Model A demandé et les neuf configurations réellement exposées par le port b2 à la révision déclarée ; conserver le Model A dans le périmètre demandé.
  - [x] Modifier `docs/tasks/emulation/commun.md` : corriger le diagnostic de correspondance manquante par la limite de la table native et conserver l'intégration d'un autre moteur en attente de la décision utilisateur.
  - [x] Modifier `docs/tasks/emulation/methode-modules.md` : vérifier le tableau natif qui construit les options plutôt qu'un README ou le nom générique du système.

Limite Acorn établie depuis les sources épinglées : `src/libretro/core.h` déclare `MACHINE_TYPES_COUNT 9` et les mêmes neuf entrées que `B2ModelCatalog` ; `core.cpp` construit l'option b2_model à partir de cette table et crée le BBCMicro depuis son entrée sélectionnée. Aucune valeur Model A n'est exposée. Le défaut de couverture n'est donc pas une conversion oubliée dans GW GUI pour cette révision. La recherche et l'intégration d'un autre moteur ont été proposées à l'utilisateur ; aucune substitution Model A → Model B et aucune modification de cœur ne sont réalisées.

- [x] Exécuter les contrôles existants des contrats de modules.
  - [x] Modifier `docs/tasks/emulation/commun.md` : consigner les résultats des tests existants Acorn, Apple, Bandai, Coleco, Epoch, Fairchild, Sony, SNK et contrats SDK, en distinguant ces tests de l'exécution de chaque DLL native.
  - [x] Modifier `tests/GWGUI.Tests/Emulation/Sony/ControllerConfigurationTests.cs` : renommer le cas de pression partielle, attendre zéro pour les trois axes non associés et conserver l'assertion de pression progressive de l'axe associé.

Contrôles existants Acorn, Apple, Bandai, Coleco, Epoch, Fairchild, Sony, SNK et contrats SDK : 293 réussites. Un test Sony attendait encore la transmission implicite des axes non associés, supprimée dans le commit 5317ac74f ; son attente est maintenant zéro pour les trois canaux non affectés, en conservant le contrôle de pression partielle de l'axe affecté. Aucun changement du code de production n'a été requis par ce contrôle.

- [x] Étendre les contrôles d'architecture existants à tous les modules.
  - [x] Modifier `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` : découvrir les projets familiaux avec leurs manifestes, vérifier pour chacun les namespaces, les dépendances Common/Emulators et l'absence de transport natif dans IEmulatorAdapter ; contrôler toutes les références familiales d'App et du SDK, pas seulement trois familles.
  - [x] Modifier `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` : autoriser l'absence du dossier Emulators lorsqu'il est vide, exiger Common et vérifier également son catalogue ; garder l'interdiction de références backend dans Modules.
  - [x] Créer `src/GWGUI.Emulation.Amstrad/Common/Interfaces/IEmulatorCapabilities.cs`, modifier `Emulators/CrocoDS/Factories/CrocoDSMachineFactory.cs`, `Emulators/Caprice32/Factories/Caprice32MachineFactory.cs` et `Modules/AmstradEmulationModule.cs` : décrire les capacités souris/paramètres vidéo dans les adaptateurs, supprimer les comparaisons de l'identifiant CrocoDS dans le module.
  - [x] Créer `src/GWGUI.Emulation.Nec/Common/Interfaces/IEmulatorFirmwareSettings.cs` et `IEmulatorPersistentStorage.cs`, modifier les fabriques Quasi88, NekoProjectII, NP2Kai et BeetlePcfx : exposer les slots de firmware et la préparation du stockage persistant par des capacités sans ABI native.
  - [x] Créer `src/GWGUI.Emulation.Nec/Common/Machines/Common/Functions/FirmwareFunctions.cs`, modifier `Emulators/Common/Interop/Functions/FirmwareFunctions.cs` et `Modules/NecEmulationModule.cs` : déplacer la gestion neutre des chemins/scan vers Common, conserver les slots et la préparation native dans les adaptateurs, remplacer l'appel direct PC-FX par sa capacité de stockage persistant.
  - [x] Créer `src/GWGUI.Emulation.Amstrad/Common/Interfaces/IEmulatorConfigurationSelection.cs` et modifier `Emulators/CrocoDS/Factories/CrocoDSMachineFactory.cs`, `Modules/AmstradEmulationModule.cs` : déplacer dans CrocoDS le filtrage des médias/options lors du changement de moteur et utiliser sa capacité souris pour la capture du pointeur.
  - [x] Modifier `src/GWGUI.Emulation.Nec/Emulators/Common/Interop/Services/ExternalCore.cs` : qualifier le helper natif FirmwareFunctions afin de distinguer préparation native et gestion commune des chemins.
  - [x] Modifier `tests/GWGUI.Tests/Emulation/Nec/NecEmulatorCatalogTests.cs` : sélectionner explicitement le helper firmware commun dans le contrôle des slots et de leur application.
  - [x] Modifier `src/GWGUI.Emulation.Nec/Emulators/Common/Interop/Functions/FirmwareFunctions.cs` et `Common/Machines/Common/Functions/FirmwareFunctions.cs` : supprimer les délégations Apply/Scan devenues sans appelant côté natif et conserver les slots vides pour un identifiant inconnu.
  - [x] Créer `src/GWGUI.Emulation/Functions/EmulationAdapterFunctions.cs` et modifier les `Common/Dictionaries/EmulatorCatalog.cs` Amstrad, Commodore et NEC : découvrir les implémentations du contrat sans dépendance de Common aux classes concrètes ; conserver l'ordre des enums et les choix par défaut existants.
  - [x] Créer `src/GWGUI.Emulation.Nec/Common/Interfaces/IEmulatorMachineDefaults.cs` et modifier les fabriques BeetlePceFast, BeetleSgx, BeetlePcfx, Geargrafx, Quasi88 et NekoProjectII : déclarer le moteur par défaut pour les machines existantes dans l'adaptateur, sans changer les identifiants persistés.
  - [x] Modifier `tests/GWGUI.Tests/Emulation/Amstrad/AmstradEmulatorAdapterTests.cs` : vérifier la neutralité d'une manette sans association au lieu de l'ancienne transmission implicite.
  - [x] Créer `Common/Interfaces/IEmulatorConfigurationValidation.cs` dans Amstrad et NEC, modifier leurs `Common/Machines/Common/Functions/ConfigurationFunctions.cs` et les fabriques CrocoDS/BeetlePcfx : déléguer la validation spécifique au moteur par une interface sans protocole natif.
  - [x] Créer `src/GWGUI.Emulation.Amstrad/Common/Interfaces/IEmulatorStorageSettings.cs`, modifier `Common/Machines/Common/Functions/StorageFunctions.cs` et `Emulators/CrocoDS/Factories/CrocoDSMachineFactory.cs` : déplacer la description et les restrictions du stockage CrocoDS derrière cette capacité.
  - [x] Modifier `src/GWGUI.Emulation.Nec/Common/Constants/ConfigurationStoreConstants.cs`, `Common/Services/ConfigurationStore.cs` et `Emulators/BeetlePcfx/Constants/PcFxBackupMemoryConstants.cs` : conserver Memory comme dossier de configuration commun, supprimer sa dépendance à une constante de cœur.
  - [x] Créer `src/GWGUI.Emulation.Nec/Common/Interfaces/IEmulatorFirmwareInspection.cs`, modifier `Common/Machines/Common/Dictionaries/FirmwareCatalog.cs`, `Emulators/Geargrafx/Factories/GeargrafxMachineFactory.cs` et `Emulators/Geargrafx/Functions/GeargrafxFirmwareFunctions.cs` : réserver la reconnaissance des noms propres à Geargrafx à son adaptateur, garder les empreintes matérielles dans le catalogue commun.

208 tests ciblés réussis : 47 contrôles de namespaces, contrats et références App/SDK sur les 23 projets ; parcours de configuration et vues existants Amstrad, NEC et Commodore. Les dépendances directes trouvées dans Modules et les trois catalogues ont été retirées ; souris, vidéo, sélection et stockage CrocoDS, slots des trois moteurs NEC, mémoire PC-FX et reconnaissance Geargrafx passent par des capacités sans ABI. Les valeurs par défaut et les chemins persistés sont conservés. La recherche supplémentaire a aussi repéré des références relatives `new Emulators...` dans d'autres anciens catalogues : ce contrôle lexical de namespaces complets ne suffit pas à déclarer toute dépendance de types vérifiée ; ces références restent à revoir séparément.

  - [x] Modifier `docs/tasks/emulation/commun.md` : consigner la portée et les résultats réels de ces contrôles sans les assimiler à une qualification native.
  - [x] Modifier `docs/tasks/emulation/methode-modules.md` : préciser que les références relatives aux adaptateurs doivent aussi être contrôlées, au-delà des namespaces complets.

Build complet confirmé : `scripts/local-building.cmd --building=debug --modules=A` a terminé avec le code 0. `build/Debug/GW GUI/gwgui.exe` et les 23 assemblages entryAssembly des manifestes livrés sont présents. Ce résultat complète les 208 contrôles ciblés ; il ne vaut pas exécution native de tous les moteurs.
