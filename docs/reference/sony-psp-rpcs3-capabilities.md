# PSP / RPCS3 — audit et reprise des paramètres

## Sources et limites vérifiées

Le wiki demandé https://wiki.rpcs3.net/index.php?title=Main_Page et ses pages de configuration renvoient HTTP 403 dans l’outil de lecture. Références complémentaires : documentation officielle PPSSPP et sources officielles des interfaces utilisées.

- PPSSPP simule le Memory Stick par le dossier `g_Config.memStickDirectory`, fourni via le répertoire de sauvegarde libretro. Pas d’image disque Memory Stick. L’option `ppsspp_memstick_inserted` (enabled/disabled) existe et contrôle l’insertion. https://github.com/hrydgard/ppsspp/blob/master/libretro/libretro.cpp ; https://github.com/hrydgard/ppsspp/blob/master/libretro/libretro_core_options.h ; https://www.ppsspp.org/docs/faq/ .
- PPSSPP distingue les classes PSP-1000 et PSP-2000/3000/Go (mémoire étendue), sans émuler séparément chaque coque. Les boutons transmis par libretro sont les quatre directions, triangle/cercle/croix/carré, L/R, Start/Select ; les deux paires d’axes analogiques sont lues mais seule la première est un stick physique sur PSP. Home, luminosité, volume, Note, WLAN et Hold ne sont pas des boutons transmis par ce cœur. https://dev.ppsspp.org/docs/settings/system/ .
- RPCS3 utilisé ici est le cœur https://github.com/WizzardSK/rpcs3-libretro (branche libretro), distribué par le buildbot Libretro. Le stockage est constitué de dossiers `dev_hdd0` et `dev_hdd1`, avec chemins VFS configurables ; aucun montage natif d’image HDD/SSD n’est proposé. Le second dossier est le cache des jeux. Les options de vitesse HDD/cache doivent être présentées avec le stockage interne, et la vitesse BD-ROM avec le Blu-ray.
- Le README du cœur exclut actuellement les périphériques PS3 autres que DualShock 3 (guitares, Move, caméras). Il ne faut pas les présenter comme utilisables simplement parce que RPCS3 autonome les connaît. L’interface annonce sept emplacements de manette, ce ne sont pas sept prises physiques ; les handlers libretro remplacent la liaison Bluetooth/USB côté émulation. https://github.com/WizzardSK/rpcs3-libretro/blob/libretro/rpcs3/libretro/libretro_pad_handler.cpp .
- La sélection native de manette passe par `retro_set_controller_port_device` puis `libretro_input_set_controller`. Le cœur conserve un indicateur connecté par emplacement ; la gestion de la connexion dans son handler comporte aussi une limite (retour toujours connecté), qu’un simple compteur dans GW GUI ne corrige pas à lui seul. https://github.com/WizzardSK/rpcs3-libretro/blob/libretro/rpcs3/libretro/libretro_input.cpp .
- Le réseau est exposé par les options `rpcs3_network_enabled`, `rpcs3_psn_status`, DNS et paramètres associés : il doit être présenté dans Ethernet. Les fonctionnalités de l’application autonome ne sont pas toutes des options du cœur.

## Réalisation

Modifications réalisées dans les sources, sans nouveau build ni exécution des émulateurs :

- **PSP** : Cheats dans son onglet ; choix de résolution basés sur les dimensions natives (`480x272` → `1× (480 × 272 px)`, `960x544` → `2× (960 × 544 px)`) ; lecteur nommé UMD.
- **Memory Stick** : ajout/retrait du lecteur relié à `ppsspp_memstick_inserted`, sans bouton Configurer, sélecteur de dossier ni ouverture de dossier. Les nouvelles configurations utilisent `Emulation/Storage/Memory Stick/Sony/PSP`. Les chemins déjà enregistrés sont conservés pour préserver les données ; retirer le lecteur ne retire pas son chemin ni ses fichiers. PPSSPP reçoit toujours le même répertoire de sauvegarde.
- **RPCS3** : Blu-ray avec sa seule option de vitesse BD-ROM ; stockage interne permanent avec options de vitesse HDD et limite du cache. Ces deux dernières options ne sont plus présentes dans la page générale de stockage. Le stockage est indiqué comme automatique ; aucune création ou ouverture d’image disque n’est proposée puisque cette interface ne la prend pas en charge. Les dossiers natifs existants restent à leur emplacement actuel.
- **Organisation** : CPU réparti entre recompilation, unités de calcul, synchronisation et compatibilité ; paramètres système dans Général, réglages graphiques dans Vidéo, réseau/PSN/DNS/UPnP et notifications RPCN dans Ethernet.
- **Manettes RPCS3** : choix de 1 à 7, une par défaut ; emplacements logiques nommés RF 1 à RF 7, sans prétendre émuler une liaison radio. Les affectations masquées sont conservées et les emplacements au-delà du nombre choisi reçoivent le périphérique natif NONE. La limite du handler natif sur la détection de connexion reste celle indiquée ci-dessus.
- **Vibrations** : un seul sélecteur de manette physique pour les commandes forte/faible, avec deux associations conservées vers les moteurs basse/haute fréquence. Les autres types de sortie gardent leur présentation existante.
- **Traductions** : ressources ciblées dans les 30 catalogues du module et les 30 catalogues App, via Argos, avec relecture française complète des libellés et aides RPCS3. Les modes numériques invariants sont conservés dans la base commune. Les valeurs de cache utilisent maintenant leurs clés de traduction plutôt qu’un texte littéral.

## Photos PSP et halos

Trois photos de référence retouchées avec imagegen, détourées avec transparence alpha et réduites à 512 pixels. Les originaux générés restent dans le dossier de génération ; seules les copies de travail téléchargées dans le dépôt et le script temporaire de traduction ont été supprimés.

| Modèle | Source photo | PNG final | Taille | Halos |
| --- | --- | --- | ---: | ---: |
| PSP-2000 | https://i.ebayimg.com/images/g/9wIAAOSwJ1pnGHwa/s-l1200.jpg | `sony-psp-2000.png`, 512 × 225 | 114 256 octets | 16 |
| PSP-3000 | https://i.gzn.jp/img/2008/08/21/psp3000/psp300001.jpg | `sony-psp-3000.png`, 512 × 241 | 130 115 octets | 16 |
| PSP Go ouverte | https://av.watch.impress.co.jp/img/avw/list/754/891/important_image.jpg | `sony-psp-go.png`, 512 × 384 | 274 219 octets | 14 |

Les trois profils sont enregistrés et sélectionnables à côté de la PSP-1000. Les 16 commandes natives restent configurables pour chaque visuel. Sur la Go, L/R sont cachés derrière l’écran ouvert : leurs associations restent dans la liste, sans faux boutons dessinés sur la photo. Aucun bouton système non transmis par le cœur n’a été ajouté.

## Vérifications effectives

- Analyse de syntaxe Roslyn de **39 fichiers C# modifiés ou nouveaux**, aucune erreur de syntaxe ; il ne s’agit pas d’une compilation ni d’une validation sémantique complète.
- XML et unicité des clés de **241 catalogues RESX** du module/App concernés ; présence des nouvelles traductions dans les 30 catalogues de chaque ensemble.
- Dimensions, taille inférieure à 1 Mio, format RGBA et transparence des trois PNG ; présence de 16/16/14 zones distinctes avec coordonnées dans l’image.
- Vérification des identifiants des constantes RPCS3 employées dans le nouveau classement ; `git diff --check` sans erreur.
- Scénarios sources ajoutés ou adaptés : résolution/Cheats PSP, retrait/réinsertion Memory Stick avec conservation du dossier, séparation Blu-ray/HDD/cache/réseau, nombre de manettes et conservation des affectations masquées, sélecteur unique des deux moteurs, profils et zones PSP. **Ces tests n’ont pas été compilés ni exécutés.**

Aucun build, commit ou test d’exécution supplémentaire n’a été lancé pour cette reprise.


## Précision du 17 octobre : chemins fixes et options LRPS2

Le stockage RPCS3 reste fixe pour l’instant, sans sélecteur de dossier. `CoreDirectoryFunctions.NativeDataDirectories` place le système persistant dans `<SaveDirectory>/rpcs3/System`. Le cœur utilise ensuite `<SystemDirectory>/rpcs3/dev_hdd0` et `dev_hdd1` par défaut. Son initialisation `Emulator::Init` crée ces dossiers et leurs sous-dossiers quand `g_cfg.vfs.init_dirs` est activé (réglage natif normal) ; GW GUI n’a pas à créer une seconde arborescence ni à déplacer les données. Une configuration VFS native déjà présente reste respectée.

Sources vérifiées : https://github.com/WizzardSK/rpcs3-libretro/blob/libretro/rpcs3/libretro/libretro_core.cpp (initialisation du répertoire RPCS3) et https://github.com/WizzardSK/rpcs3-libretro/blob/libretro/rpcs3/Emu/System.cpp (création de dev_hdd0/dev_hdd1 dans Emulator::Init).

Correction du traitement du dernier cadre Général LRPS2 : les trois options n’ont plus de masquage. Le déblocage des langues, les améliorations des jeux et la base GameIndex externe sont visibles dans une section Compatibilité de Cheats, distincte des codes de triche. Le chemin GameIndex reste dans leur section avec ses conditions de disponibilité existantes. Les clés et traductions existantes sont réutilisées.
