# Contrats Fairchild vérifiés

Source officielle FreeChaF, révision 76c7a84f1f7e80f3e6f2bba96fe100cb24e99124. DLL Windows x64 téléchargée avec empreinte dans artifacts/fairchild-source/catalogue.json. Relevé du code src/libretro.c et src/memory.c ; le code fait foi lorsque les métadonnées sont périmées. Aucun lancement du nouveau module encore validé.

## Modèles et firmware

Channel F et Channel F System II utilisent FreeChaF. La source tente sl90025.bin en premier, puis sl31253.bin ; elle lit ensuite sl31254.bin. Isoler les fichiers système de chaque modèle pour éviter que la présence du BIOS II change le modèle original.

- Channel F : sl31253.bin, MD5 ac9804d4c0e9d07e33472e3726ed15c3 ; sl31254.bin, MD5 da98f4bb3242ab80d76629021bb27585.
- System II : sl90025.bin, MD5 95d339631d867c8f1d15a5f2ec26069d ; sl31254.bin commun.

La source actuelle propose une émulation HLE expérimentale si une partie du BIOS manque. Ne pas présenter un démarrage HLE comme une vérification de BIOS. La documentation plus ancienne indique les BIOS obligatoires ; contrôler aussi le comportement de la DLL téléchargée avant de figer la configuration.

## Cartouches, états et fermeture

Le contrat runtime accepte bin|rom|chf avec need_fullpath=false ; la cartouche est fournie par buffer. Aucun sous-système libretro. Une cartouche de 262 144 octets active le mapper multicart natif. Les autres tailles ne doivent pas recevoir un mapper inventé.

Les états sont sérialisés et restaurés par le cœur ; taille à sonder réellement. SAVE_RAM et RTC renvoient zéro : aucune Memory Card ni sauvegarde RAM persistante à fabriquer. La RAM SDK correspond aux registres F8 ; la VRAM native comprend 8192 octets. Ne pas confondre mémoire native exposée et périphérique physique.

Dans cette révision, MEMORY_loadCartROM alloue ROM par malloc et retro_unload_game/retro_deinit sont vides : ils ne libèrent pas ce buffer. Employer le processus isolé existant pour les sessions du module et attendre sa sortie effective. La libération de la DLL seule ne prouve pas la libération de cette allocation native. Tous les objets possédés côté hôte doivent rester protégés par finally ; ne pas réutiliser indéfiniment le cœur dans le même processus pour remplacer des cartouches.

## Commandes

Deux manettes physiques, huit commandes chacune : avant, arrière, gauche, droite, rotation gauche, rotation droite, tirer vers le haut et pousser vers le bas. Les commandes sont numériques dans la source actuelle : Up=4, Down=5, Left=6, Right=7, rotation droite=A=8, pousser=B=0, tirer=X=9, rotation gauche=Y=1. Créer une enum locale des IDs natifs et des actions remappables, sans nouvelle touche clavier étrangère.

Start=3 affiche ou ferme l’overlay des boutons de console. Select=2 échange les joueurs. Ce sont des fonctions du cœur, pas des boutons supplémentaires de la manette physique. La source actuelle lit uniquement JOYPAD ; les mentions d’axes analogiques du README ne correspondent pas à cette boucle. Ne pas implémenter d’axes que la DLL ne consomme pas.

## Options, image et son

Une option native : freechaf_fast_scrclr, disabled par défaut, enabled en alternative. Elle accélère l’effacement de l’écran ; la classer dans Vidéo avec traduction contextualisée. Conserver les shaders et réglages vidéo communs.

Le cœur annonce NTSC et 44 100 Hz audio. Son image est agrandie trois fois depuis la zone visible 102 × 64 pour son overlay. Relever dimensions et cadence effectives via get_system_av_info plutôt que déduire la géométrie de la console des seules dimensions du buffer.

## Structure à reprendre

Le matériel possède 8192 pixels codés sur deux bits, soit 2048 octets de VRAM, et 64 registres de travail dans le F8. Le tableau `VIDEO_Buffer_raw[8192]` de FreeChaF représente un pixel par octet ; sa taille ne représente pas la mémoire physique. Source primaire de rétro-ingénierie : [Sean Riddle, Channel F specs](https://www.seanriddle.com/chanfspecs.html), relevés du matériel et du brochage des cartouches. L’information affichée est donc 2 KiB de VRAM.

Reprendre les services génériques de catalogue/installation, persistance, transport isolé, buffers, machine et fermeture depuis les modules validés. Adapter séparément les BIOS multiples, le chargement par buffer, les états, le protocole numérique des deux manettes et l’option d’effacement. Exclure les potentiomètres, pistolets, pavé SCV, save natifs SCV et alias Power/Reset d’Epoch. Les images de machines restent dans le module ; la vraie manette du catalogue reste dans App. Aucun sélecteur de dossiers globaux ni document dans Assets.

## Images livrées et provenance

- Channel F : [Fairchild-Channel-F.png](https://commons.wikimedia.org/wiki/File:Fairchild-Channel-F.png), Evan-Amos ; CC BY-SA 3.0 (https://creativecommons.org/licenses/by-sa/3.0/). Photo conservée, console isolée par contour et réduction à 512 × 340 pixels ; contribution de détourage sous la même licence.
- Channel F System II : [Fairchild-Channel-F-System-II-Console.jpg](https://commons.wikimedia.org/wiki/File:Fairchild-Channel-F-System-II-Console.jpg), Evan-Amos ; domaine public. Fond retiré puis console réduite à 512 × 288 pixels.
- Manette Channel F : extrait de cette même photographie System II, Evan-Amos, domaine public ; seule la vraie manette est détourée, le visuel est conservé uniquement dans App.
- Marque : [Fairchild Semiconductor Logo.svg](https://commons.wikimedia.org/wiki/File:Fairchild_Semiconductor_Logo.svg), Fairchild Semiconductor ; PD-textlogo. Rendu PNG transparent réduit à 512 × 182 pixels, distinct des consoles.
- Les sources, leurs empreintes et les dimensions sont conservées dans artifacts/fairchild-source/images. Tous les PNG ont été inspectés visuellement après traitement. Aucun fichier de crédits ni doublon de manette n’est incorporé au module.
