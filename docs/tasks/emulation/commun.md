# Travaux communs restants

Suivi des demandes encore ouvertes. Les étapes terminées et leurs journaux détaillés ont été retirés. Le catalogue `liste-emulateur-libretro.md`, la méthode `methode-modules.md`, le chronomètre et les suivis Atari/Commodore/NEC sont conservés.

Dernière livraison vérifiée : correctif LRPS2 sans conversion des anciens libellés, build Debug réussi, application, SDK et 23 modules vérifiés par empreinte. Exécutable : `F:/GW GUI/build/Debug/GW GUI/gwgui.exe`. Les 390 contrôles Sony du lot LRPS2 sont réussis ; les 152 contrôles des entrées communes restent consignés dans `9694f5147`. Le lot précédent de constantes audio communes a passé 287 contrôles des quinze familles (`d37efb8d5`). Ces résultats ne valent pas qualification native exhaustive des cinquante DLL du catalogue.

Historique détaillé disponible localement : `git show 9694f5147:docs/tasks/emulation/commun.md`. Les anciens essais, empreintes de médias, PID et résultats restent dans cet historique ; ils ne sont pas répétés dans les tâches restantes.

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
    - [ ] mattel-intellivision-controller.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `mattel-intellivision-controller.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
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
    - [ ] sony-dualshock-1.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `sony-dualshock-1.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
    - [ ] sony-psp-1000.png
      - [ ] Modifier `docs/tasks/emulation/commun.md` : consigner pour `sony-psp-1000.png` le modèle réel, la référence visuelle, la transparence, les commandes visibles et la correspondance des halos ; si un écart est trouvé, insérer d'abord les actions concrètes qui corrigent le PNG et ses zones.
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
  - [ ] Créer `artifacts/controller-sources/sony-dualshock-1-original.jpg` : conserver une photo de face de la DualShock d'origine.
  - [ ] Modifier `src/GWGUI.App/Assets/Controllers/sony-dualshock-1.png` : remplacer l'image oblique par la photo de face du modèle exact, détourée sans reconstruire les commandes.
  - [ ] Modifier `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkCatalog.cs` : replacer les halos de la DualShock d'origine.
  - [ ] Modifier `docs/tasks/emulation/commun.md` : créditer et vérifier la DualShock d'origine.
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

## Modules demandés : limites encore ouvertes

- [ ] Remplacer la photographie du Spectrum SE par une meilleure photo authentique.
  - [ ] Modifier `src/GWGUI.Emulation.Sinclair/Assets/Machines/SpectrumSe.png` : utiliser une photographie exploitable du prototype SE, complète et identifiable, détourée classiquement à 512 px ; conserver la photographie actuelle tant qu’aucun meilleur candidat n’est trouvé.
  - [ ] Modifier `THIRD-PARTY-NOTICES.md` et `docs/tasks/emulation/commun.md` : attribuer la nouvelle source et relever le contrôle du cadrage et de la transparence après remplacement.
- [ ] Résoudre ou requalifier le parcours OpenGL PCEE2 à partir d’un nouvel élément probant.
  - [ ] Modifier `docs/tasks/emulation/commun.md` : relever toute évolution officielle de `GLContextWGL.cpp` ou du binaire, puis le résultat d’un essai justifié par cette évolution ; conserver OpenGL non qualifié tant que le renderer demandé n’est pas réellement ouvert et que le partage du contexte échoue.
- [ ] Compléter les interactions Wenquxing encore non qualifiées.
  - [ ] Modifier `docs/tasks/emulation/commun.md` : qualifier le lancement effectif des jeux NC3000 et les conditions produisant du son sur les modèles autres que CC800 ; garder les originaux de `F:/Retro/ROMS-BOOT/Wenquxing` intacts. La navigation NC2000/NC3000 est consignée ci-dessous.

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
