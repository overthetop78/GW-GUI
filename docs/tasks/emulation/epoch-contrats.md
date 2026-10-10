# Contrats Epoch vérifiés

Relevé du 2026-10-10. Sources téléchargées sous `artifacts/epoch-source/source/`, documentation et métadonnées officielles sous `artifacts/epoch-source/`. Les DLL Windows ont été sondées avec les callbacks SDK existants, sans initialisation du cœur. Toutes les acquisitions de la sonde sont libérées dans finally.

## Catalogue

- Cassette Vision et Cassette Vision Jr : PD777, version native 1.01.01. La variante Jr conserve les jeux mais n'a pas les potentiomètres de la console originale.
- Super Cassette Vision Epoch, Yeno et Lady : EmuSCV, version native 0.15.20260921084155. Fixer `emuscv_console` selon la machine physique : EPOCH, YENO ou EPOCHLADY. Yeno fonctionne à 50 Hz ; les autres à 60 Hz. Ne pas permettre à une option de changer silencieusement l'identité physique choisie.

## BIOS, cartouches et sauvegardes

PD777 n'a aucun BIOS. Le chargement natif exige le chemin et annonce `bin777|ptn777` ; la fonction de chargement recherche un `.ptn777` portant le même nom que le `.bin777`. Elle sait aussi lire les deux fichiers dans un ZIP. Préserver ces fichiers compagnons quand l'hôte isole le contenu. En cas de chargement manquant, le cœur lance sa démonstration : ce démarrage ne constitue pas une validation du jeu choisi. Les états et la mémoire de sauvegarde native sont indisponibles ; ne pas afficher une commande fictive ni injecter une fausse carte mémoire.

EmuSCV exige un BIOS de 4096 octets, `upd7801g.s01`, MD5 `635a978fd40db9a18ee44eff449fc126`. Les formats natifs sont `.cart`, `.bin`, `.rom`, `.0`, `.zip`. Un dump `.0` peut dépendre de `.1`, `.2`, `.3` voisins. Dans un ZIP, la cartouche porte le nom de l'archive. Le cœur écrit les cartouches converties et les fichiers RAM `.save` dans son dossier de sauvegarde ; ne pas les perdre au nettoyage de la session. Les états natifs sont disponibles, la mémoire de sauvegarde SDK vaut zéro.

## Commandes physiques et affectations

PD777 utilise deux ports pour les commandes intégrées, jamais un appareil « aucun ». Push 1/2/3/4 correspondent aux bits 9/0/1/8 ; Select/Start/Aux aux bits 2/3/11. Les leviers gauche/droite utilisent 6/7. La commande de sélection de parcours utilise 4/5 : ces touches ne représentent pas un joystick vertical physique. Potentiomètres : port 0 axe gauche X = potentiomètre 2, Y = 1 ; port 1 X = 3, Y = 4. La variante Jr doit omettre les potentiomètres physiques absents. Ne pas transformer ces quatre axes en boutons sans adaptation explicite.

EmuSCV utilise deux joysticks, tirs droit/gauche 8/0. Le pavé numérique est lu via `RETRO_DEVICE_KEYBOARD` : chiffres standards ou pavé 0–9, Return/KPEnter et Backspace/Delete. Les touches de console Pause, Reset, Power passent par F9, F11, F12. Exposer les actions physiques affectables, synthétiser uniquement leur entrée native après réaffectation, et éviter qu'un ancien alias hôte reste actif. SELECT commande l'overlay du cœur ; ce n'est pas une touche physique supplémentaire à inventer.

## Réutilisation et adaptation

Réutiliser de Coleco les services génériques de téléchargement, installation, persistance, buffers, protocole isolé, hôte, audio et fermeture. Conserver les trois surcharges de gestionnaire basées sur la configuration courante. Vérifier chaque helper référencé, dont le timer possédé, avant compilation.

Adapter les enums et catalogues, contrôleurs intégrés, pavés et potentiomètres, BIOS, cartouches avec fichiers compagnons, commandes disponibles et sauvegardes appartenant au cœur. Supprimer intégralement les propriétés ADAM, data packs, spinners Coleco et chargements de sous-systèmes. Classer les options par leur fonction réelle, pas selon les catégories brutes natives. Conserver les réglages vidéo et audio communs à leur emplacement existant.

Les cinq modèles ont exécuté vingt images avec les DLL natives et des contenus synthétiques. Les trois variantes SCV ont sauvegardé puis restauré un état de 81 775 octets ; PD777 ne fournit pas cette capacité. Après chaque session, la DLL était déchargée. Les cinq fenêtres de paramètres ont été ouvertes via la découverte réelle du paquet, tous leurs onglets activés ont été parcourus et aucun libellé Epoch non résolu ni erreur de fenêtre n'a été détecté. Résultats : epoch-verification.json. Ces essais synthétiques vérifient le protocole et le cycle de vie ; ils ne prouvent pas la compatibilité de jeux commerciaux. Les photographies et traductions sont incorporées ; le logo de marque provient du fichier Epoch_logo.svg de Wikimedia Commons, distinct des photographies de consoles.
