# Contrats Sinclair

## Distribution identifiée

La recette officielle `libretro-super/recipes/linux/cores-linux-x64-generic` identifie `libretro/81-libretro` et `libretro/fuse-libretro`. Les paquets Windows x64 téléchargés sont décrits avec leurs SHA256, révisions et chemins dans `artifacts/sinclair-source/catalogue.json`. Sources 81 : `fa7094910d040baa5fd8b11dbf6a1a618330ecd9` ; Fuse : `e997e2bc32c888348f862f69f2c53babfedf7791`. Comparer les options effectivement exposées par les DLL avant génération des catalogues.

Le relevé natif `artifacts/sinclair-source/binary-options.json` confirme EightyOne `1.0a fa70949`, 22 options, et Fuse `1.6.0 e997e2b`, 38 options. Aucun cœur n'a été initialisé pour ce relevé ; les callbacks, bibliothèques et dossiers temporaires ont été libérés.

## Périmètre des modèles

| Machine demandée | Adaptation attestée dans les sources |
|---|---|
| ZX80 | Conserver le modèle matériel demandé ; aucun moteur compatible attesté dans les deux adaptateurs examinés. Le frontend 81 fixe `MACHINEZX81` dans `src/libretro.cpp` ; ne pas assimiler la présence d'autres machines dans ses anciennes sources à une sélection accessible. |
| ZX81 | EightyOne : machine ZX81, RAM 16 KiB fixée par `RAMPACK16`, ROM et état initial internes. |
| Spectrum 16K, 48K, 128K, +2, +2A et +3 | Fuse : identités explicites de `machine_list` et choix natif `fuse_machine`. |
| Compatibles Fuse | La table source inclut 48K NTSC, +3e, Spectrum SE, Timex TC2048, TC2068, TS2068, Pentagon 128K, 512K, 1024 et Scorpion 256K. Les données matérielles et firmwares de chacun doivent être relevés avant leur fabrication. |

## Supports et firmware

| Moteur | Distribution et chemin à adapter |
|---|---|
| EightyOne | Formats annoncés P, TZX et T81 ; contenu obligatoire, chargé en mémoire. Vérifier les branches de `retro_load_game` avant d'assimiler un état initial à un périphérique physique. |
| Fuse | Formats annoncés TZX, TAP, Z80, RZX, SCL, TRD, DSK, DCK, SNA, SZX, ZIP et IPF. Démarrage sans contenu annoncé ; interfaces de contrôle des disques présentes dans le frontend. Séparer bandes, disquettes, cartouches et snapshots selon leur chemin natif, pas selon une catégorie unique. |
| Firmware Fuse | Les informations de distribution identifient les ROM externes Pentagon et Scorpion sous `system/fuse`. Les ROM des extensions facultatives doivent être relevées séparément ; ne pas présenter une option dont le firmware manque comme fonctionnelle. |

`src/compat/file.c` incorpore les ROM Spectrum 48K, 128K, +2, +3, +3e, SE, TC2048 et TC2068 dans `mem_entries`. Les modèles utilisant ces ROM peuvent démarrer sans copie externe. Pentagon utilise `128p-0.rom`, `128p-1.rom` et `trdos.rom` ; 512K/1024 ajoute `gluck.rom`. Scorpion utilise `256s-0.rom` à `256s-3.rom`. Leur présence est exigée lorsque le modèle qui les consomme est choisi, bien que le catalogue global les marque facultatives. Currah uSpeech requiert `uspeech.rom` et `sp0256-al2.bin` ; ce périphérique doit rester indisponible sans son firmware.

Les callbacks Fuse d'insertion délèguent à `utils_open_file`, qui distingue le type du support. Les snapshots et enregistrements RZX ne deviennent pas des lecteurs inventés. Le frontend annonce aussi M3U dans la DLL relevée, contrairement à l'ancien fichier info : garder cette différence explicite. Les listes multi-disques se préparent dans l'adaptateur natif ; les chemins de médias et copies modifiables suivent les services de session existants.

Dans `src/compat/file.c`, `find_entry` résout les noms exacts des ROM internes avant tout accès au système de fichiers. Copier une ROM externe sous ces mêmes noms ne permet donc pas de la remplacer dans ce frontend ; ne pas proposer de sélecteurs qui prétendent le faire. Les seules banques externes configurables relevées ici sont celles de Pentagon et Scorpion, plus les deux fichiers du périphérique Currah. Les banques machine font 16 KiB ; chacun des deux fichiers Currah fait 2 KiB selon `fuse/peripherals/sound/uspeech.c`.

## Entrées et options

Réutiliser le clavier hôte commun ; ne pas fabriquer d'affectations de touches physiques depuis les variables `81_joypad_*`. Ces variables sont un transport natif vers le clavier ZX81, pas un deuxième éditeur du clavier hôte. Les manettes restent sans association physique automatique. Relever le polling clavier, les périphériques et commandes effectivement accessibles dans chaque frontend avant de définir les descriptions de ports.

EightyOne poll le clavier dans `src/keybovl.c` quand le port sélectionné est un clavier, par les codes des entrées natives de sa table. Fuse propose trois ports avec Cursor, Kempston, Sinclair 1/2, Timex 1/2, Fuller, souris Kempston et clavier Sinclair ; le périphérique du port doit être défini explicitement avant de charger le contenu. Le clavier physique doit disposer d'un chemin accessible indépendamment des affectations de manettes, sans transmettre implicitement des boutons non associés. Les commandes physiques communes restent dans le contrat SDK ; les identifiants des sous-types natifs restent dans l'adaptateur.

Le frontend EightyOne déclare précisément deux ports natifs : index 0 Cursor Joystick et index 1 clavier Sinclair. Le frontend Fuse configure par défaut l'index 2 comme clavier Sinclair. Réserver ces chemins au clavier commun de l'application ; ne pas présenter le port clavier comme une manette ni l'effacer lors d'un changement de manette. Les deux autres ports Fuse peuvent porter les périphériques de commande choisis par l'utilisateur.

La table 81 expose chargement accéléré, contenu mémoire 8–16 KiB, bordure, haute résolution, Chroma 81, rendu et son ; classer seulement les options utiles dans leurs onglets. Exclure les fonctions d'overlay qui doublonnent l'interface commune et tout catalogue de jeux. Fuse possède une table d'options versionnée et un choix de modèle : le modèle choisi dans l'application détermine ce choix natif, sans sélection contradictoire dans Général.

Fuse : accélération dans CPU ; bordure et palette dans Vidéo ; son de chargement, haut-parleur, stéréo AY et volumes dans Audio ; chargement accéléré avec les commandes de bande ; permutation des boutons de souris dans Souris. Les variables de remappage joypad vers clavier, transparence d'overlay, maintien de touches d'overlay, affichage du type de joystick et affichage de vitesse ne créent pas de nouveaux cadres. Taille automatique des états reste interne à la sauvegarde. Les périphériques audio optionnels utilisent leurs capacités et firmware attestés, pas une liste de cases générée sans contrôle.

## Contrôles avant commit

La capacité physique du Spectrum SE est de 280 KiB, confirmée par son concepteur [Andrew Owen](https://andrewowen.net/blog/updating-a-40-year-old-computer-design/). Les neuf banques HOME de 16 KiB dans `fuse/machines/spec_se.c` ne représentent que 144 KiB ; les banques DOCK et EXROM sont allouées séparément. La description matérielle ne doit pas confondre une seule zone native avec la capacité totale du prototype.

Les contrats ne constituent pas une validation runtime. Vérifier les DLL réellement installées, première image et audio, clavier et affectations persistées, supports, états, reset, arrêt après erreur et libération effective des ressources. Les commandes communes restent indépendantes du backend ; chargement, exports, callbacks et codes natifs restent dans Interop et les adaptateurs nommés.

## Enregistrement des supports

La sérialisation et la restauration de l'état de machine sont des opérations distinctes de l'enregistrement des modifications d'une disquette ou d'une cassette. Le contrat de contrôle des disques employé par l'adaptateur expose insertion, éjection, sélection, remplacement, ajout et interrogation des images ; il ne fournit aucune commande pour sauvegarder une image modifiée. Le module ne doit donc proposer aucune commande de sauvegarde de support sans implémentation native. Une éventuelle écriture automatique interne au cœur n'est pas démontrée par ce constat et exige un essai avec la DLL effectivement téléchargée.

Les DSK en lecture seule ont été exécutées avec contrôle des empreintes des originaux avant et après insertion, éjection et échange ; les fichiers originaux sont restés identiques. Ce contrôle prouve leur protection dans ces scénarios, pas la persistance de modifications réalisées par un programme émulé. Les sources consultées indiquent des réponses d'annulation aux dialogues d'enregistrement dans le frontend de compatibilité, mais cette lecture ne remplace pas la vérification du comportement du binaire livré.

La fréquence matérielle du prototype Spectrum SE est de 3,528 MHz selon l'[annonce de son concepteur Andrew Owen](https://groups.google.com/g/comp.sys.sinclair/c/60pEQJ807o4), datée du 13 octobre 2002. La description la conserve distincte de la vitesse globale d'émulation.
