# Contrats Mattel

## Périmètre

La recherche dans toutes les sections de `liste-emulateur-libretro.md` retient Intellivision avec `freeintv_libretro.dll`. Le module aura l'identité mattel ; aucune variante ni extension matérielle non demandée n'est ajoutée.

## Sources et paquet

Sources officielles : https://github.com/libretro/FreeIntv ; révision `ef3e0fe322bec62a7f916c0bb0834c08c348d0b4`. Le paquet officiel Windows x64 et son SHA256 sont enregistrés dans `artifacts/mattel-source/catalogue.json`. Le contrôle sans initialisation de machine relève `freeintv`, version `1.2  ef3e0fe`, formats `int|bin|rom`, chemin complet obligatoire, extraction autorisée, 352 × 224, 60 images/s, 44,1 kHz et 604 552 octets par état. Les deux options relevées sont enregistrées dans `artifacts/mattel-source/binary-options.json`. Ce contrôle ne valide pas encore le démarrage.

## Firmware et contenu

- Deux fichiers obligatoires sous le répertoire système natif : `exec.bin` (Executive ROM, MD5 `62e761035cb657903761800f4437b8af`) et `grom.bin` (Graphics ROM, MD5 `0cd5946c6473e42e8e4c2137785e427f`). Noms exacts, rôles séparés dans les champs firmware SDK existants.
- EXEC contient 4 096 mots de 16 bits big endian (8 192 octets), GROM 2 048 octets ; refuser les fichiers tronqués avant le lecteur natif qui ne contrôle pas ses lectures.
- `retro_get_system_info` annonce `int|bin|rom`, avec `need_fullpath=true`. `retro_load_game` appelle `LoadGame(info->path)` et renvoie true sans propager le résultat du lecteur : le module doit valider le contenu avant chargement selon les formats réels de `cart.c` : lecture limitée à 131 072 octets, Intellicart avec octet A8 ou complément du nombre de segments, segments adressés par pages de 256 mots et données big endian ; les CRC et tables d'activation sont ignorés par le cœur. Les images brutes utilisent ses cartes mémoire internes ou la carte par défaut, sans exposer son catalogue de jeux dans l'interface. Vérifier longueur, bornes et segments avant transfert ; ne pas inventer de sélecteur de mapping que le cœur n'expose pas.
- ECS n'est pas pris en charge. L'Intellivoice figure dans les sources et dans les états natifs ; la ROM de synthèse SP0256 est embarquée dans `ivoice.c` (table `mask`), aucun champ de firmware supplémentaire n'est justifié.
- Les surimpressions propres aux jeux sont recherchées sous `system/freeintv_overlays/<nom du jeu>.png`. Cette donnée n'autorise aucun sélecteur global de dossiers dans Général ; les options d'affichage réellement exposées doivent être raccordées sans remplacer les shaders GW GUI.

## Entrées attestées

Deux ports sont interrogés. `controller.c` distingue disque à 16 directions, douze touches du pavé (1–9, Clear, 0, Enter) et trois commandes d'action indépendantes (les boutons supérieurs physiques partagent la commande Top). Les manettes hôtes ne reçoivent aucune association initiale.

Le protocole FreeIntv utilise : directions joypad, axes analogiques gauches pour le disque, axes droits pour huit touches du pavé, L3 pour 0, R3 pour 5, L2 pour Clear et R2 pour Enter ; A/B/Y correspondent aux actions Left/Right/Top. X utilise la dernière touche sélectionnée, L/R ouvrent le pavé natif, Start pause et Select permute les deux contrôleurs. Ces identifiants sont des transports internes, pas des affectations physiques hôtes ni des noms de boutons à imposer à la manette Intellivision.

Le callback clavier interprète les caractères 0–9, `[` et `]` et écrit uniquement le port 0. Il ne faut donc pas exposer ce transport comme un second clavier de machine, ni supposer qu'il fournit les douze touches indépendamment pour les deux ports. L'adaptation des commandes de pavé doit couvrir le protocole réel de chacun des ports et être vérifiée à l'exécution.

## Options et états

Les sources exposent `default_p1_controller` (left/right, défaut right) et `freeintv_multiscreen_overlay` (disabled/enabled, défaut disabled). La première détermine l'ordre logique des deux contrôleurs ; la seconde active les pavés interactifs dans le rendu natif. Les labels visibles ne doivent pas reprendre les suffixes de redémarrage, mais les métadonnées du cycle de vie restent conservées.

Les exports de sérialisation incluent CPU, STIC, PSG et Intellivoice. Le paquet installé par le module a été vérifié avec les BIOS EXEC/GROM locaux valides et une cartouche de diagnostic générée : frames vidéo/audio, pause/reprise, états, resets, retrait/réinsertion de cartouche, arrêt des processus successifs et nettoyage après échec. Aucun jeu commercial n'a été essayé.

## Rendu et sauvegarde

Le rendu normal est 352 × 224. L'option de pavé interactif produit un espace natif de 1 074 × 600 avec un écran de jeu agrandi ; elle utilise le protocole pointer, distinct des commandes de manette. Les shaders et traitements GW GUI restent leurs blocs communs existants. Aucune carte mémoire amovible ni sauvegarde sur disque n'est exposée par ce cœur : les états natifs ne sont pas des cartes mémoire.

## Informations matérielles

La documentation interne de 1980 publiée par les Blue Sky Rangers identifie CP1610 à 894,886 kHz, 352 mots système de 16 bits (704 octets), scratchpad de 256 octets et GRAM de 512 octets. L'interface sépare 960 octets de mémoire système/scratchpad et 512 octets de mémoire graphique ; la géométrie 352 × 224 est celle du cœur, pas la résolution physique des cartes du STIC. Source : https://history.blueskyrangers.com/hardware/2609.html .
