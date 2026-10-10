# Contrats Philips et Magnavox

La recherche complète dans `liste-emulateur-libretro.md` retient quatre machines : Philips CD-i, P2000T, Magnavox Odyssey² et Philips Videopac G7000. Trois cœurs sont livrés : M2000, O2EM et SAME_CDI. Les deux consoles à cartouche partagent O2EM. CDi 2015 a été retiré à la demande de l’utilisateur après son plantage natif ; ses options, ressources et adaptateur ne sont plus livrés. Les relevés ci-dessous sur ce cœur constituent l’historique du diagnostic abandonné, pas une capacité actuelle du module.

## Sources et paquets

Les sources officielles et leurs révisions, DLL Windows x64 et SHA256 sont conservées dans `artifacts/philips-source/catalogue.json`. Les exports et options ont été relevés avec les callbacks SDK existants, sans appel à retro_init ni démarrage de machine. Les DLL et délégués ont été libérés dans finally. Le résultat est conservé dans `binary-options.json` ; le script temporaire de sonde est supprimé après utilisation.

| Cœur | Bibliothèque observée | Extensions observées | Chemin complet | Options |
|---|---|---|---|---|
| O2EM | 1.18 679d6fe | bin | Non | 10 |
| M2000 | v0.9.3 | cas | Oui | 1 |
| CDi 2015 | 0.160 7adbf44 | chd | Oui | 9 |
| SAME_CDI | 0.287 | chd, iso, cue, m3u | Oui | 17 |

## Cartouches Odyssey² et Videopac

- Source : [libretro-o2em](https://github.com/libretro/libretro-o2em), `libretro.c` et `libretro_core_options.h` récupérés à la révision enregistrée.
- Cartouche bin chargée en mémoire, sans démarrage sans contenu. BIOS choisi par `o2em_bios` et chargé depuis le répertoire système. Odyssey² : `o2rom.bin` ; Videopac G7000 : `c52.bin`. Le paquet expose aussi `g7400.bin` et `jopac.bin` : ces options ne transforment pas silencieusement le modèle G7000 en G7400.
- Empreintes BIOS documentées : o2rom.bin `562d5ebf9e030a40d6fabfc2f33139fd`, c52.bin `f1071cdb0b6b10dde94d3bc8a6146387`, g7400.bin `c500ff71236068e0dc0d0603d265ae76`, jopac.bin `279008e4a0db2dc5f1c048853b033828` ; vérifier les tailles dans le chargeur avant validation des supports.
- Deux ports joypad : directions natives 4/5/6/7, action B=0. Le premier port expose aussi le clavier virtuel et des raccourcis numériques ; ce sont des commandes de cœur, pas des boutons physiques supplémentaires de la manette. Le clavier natif est interrogé via RETRO_DEVICE_KEYBOARD et reste utilisable sans association de manette.
- Toutes les sources physiques de manettes sont initialement vides. Les numéros du protocole définissent les destinations émulées, jamais une association hôte.
- Pas de mémoire SAVE_RAM exposée ; pas de carte mémoire fictive. États sérialisés disponibles dans le code. `retro_unload_game` est vide, tandis que `retro_deinit` ferme audio, voix, affichage et libère les buffers ; remplacement par processus isolé et attente de sa sortie avant nettoyage.
- Les options BIOS/région sont appliquées au lancement ; les réglages image/audio utiles sont triés avant leur présentation, sans doubler les traitements communs.

## P2000T

- Source : [M2000](https://github.com/p2000t/M2000), `src/libretro/m2000_libretro.c`, `src/P2000.c` et `P2000.h`.
- Cassette `.cas` par chemin complet ; la cassette fournie est ouverte en lecture seule. Sans contenu, le cœur ouvre ou crée `Default.cas` dans son répertoire de sauvegarde. Le support modifiable doit être conservé après arrêt ; ne pas le supprimer avec la session.
- Monitor et BASIC sont incorporés (`monitor_rom`, `basic_nl_rom`) : aucun faux sélecteur BIOS externe. Le cœur initialise 32 KiB de RAM ; VRAM native 0x1000. Les informations matérielles affichées seront distinguées des buffers de rendu 480 × 480.
- Clavier toujours interrogé, sans callback clavier actif : SET_KEYBOARD_CALLBACK fournit une table nulle, acceptée par le contrat SDK existant. Les identités hôte communes restent communes ; seules START, STOP, ZOEK, CODE et autres touches réellement absentes nécessitent des identités spécifiques modifiables, à relever dans la matrice avant extension SDK.
- Unique option `m2000_keyboard_mapping`, valeurs `symbolic|positional`, défaut `symbolic`. Ne pas créer un catalogue de touches de clavier émises depuis une manette.
- Entrées joypad natives simulant des touches clavier ; ne pas les affecter automatiquement. Aucun port joystick intégré n'est attesté par cette option seule.
- Cadence 50 Hz, audio 30000 Hz ; états registers + VRAM + RAM. Pas de SAVE_RAM : la sauvegarde repose sur le fichier cassette. `retro_unload_game` ferme la cassette ; `retro_deinit` appelle TrashP2000 et libère les buffers.

## Philips CD-i

- Sources : [MAME 2015 CD-i](https://github.com/libretro/mame2015-libretro), `README.cdi2015.md`, `src/osd/retro/` et `src/mame/drivers/cdi.c` ; [SAME_CDI](https://github.com/libretro/same_cdi), README, `src/osd/libretro/` et `src/mame/drivers/cdi.cpp`.
- CDi 2015 : CHD par chemin complet ; archive BIOS `cdimono1.zip` dans le système ou à côté du contenu. Conserver les noms des membres de l'archive, les comparer au driver et ne pas incorporer le BIOS au module.
- SAME_CDI : CHD/ISO/CUE/M3U par chemin complet ; BIOS `System/same_cdi/bios/cdimono1.zip`. Les fichiers CUE gardent leurs compagnons BIN et chemins relatifs. Le lecteur reste distinct du support monté.
- SAME_CDI actuel possède un contrôle des disques et des états dans `disk_control.h` et `libretro.cpp`. La documentation historique les déclarant absents est dépassée pour ces sources : reprendre la table native basique/étendue du SDK, et vérifier réellement insertion, remplacement et états avant de déclarer ces fonctions opérationnelles.
- Les deux cœurs contiennent les exports de sérialisation ; leur réussite dépend d'une machine démarrée. CDi 2015 mesure son buffer par une sauvegarde native ; SAME_CDI peut annoncer zéro avant initialisation effective. Ne pas conclure que des stubs garantissent une sauvegarde fonctionnelle.
- La NVRAM est enregistrée par les chemins MAME sous Saves, pas par une image de Memory Card inventée. SAME_CDI expose les options NVRAM et séparation par jeu/système. Respecter les fichiers produits et recopier les sauvegardes avant destruction des dossiers de session.
- SAME_CDI annonce PAL/NTSC et un DVC optionnel `vmpega.rom` dans le BIOS (262144 octets, CRC db264e8b). Ne pas imposer cette ROM à un jeu sans DVC.
- Les entrées CD-i utilisent le chemin MAME : joypad, analogique et éventuellement souris/pistolet selon l'option choisie. Relever le mappage du driver et des lecteurs d'entrée avant les profils de manette ; aucun bouton ou axe physique affecté automatiquement.
- Une DLL MAME et ses allocations restent détenues par le processus isolé. Déchargement, fermeture, NVRAM puis arrêt effectif du processus doivent être vérifiés, y compris après erreur de BIOS ou de disque.

## Preuves encore requises avant fin du module

Les métadonnées et options sont prouvées par les DLL sans initialisation. Démarrage des machines, supports modifiables, sauvegardes, entrées analogiques et rendu des onglets ne sont pas encore vérifiés : ils appartiennent au groupe de validation du plan Philips. Les photographies et traductions restent à produire après le tri des paramètres.

## Frontière des adaptateurs
La fabrique commune crée une machine à partir de IEmulatorCore et de la politique de remplacement des supports fournie par la fabrique concrète. Les exports et callbacks Libretro restent dans Interop ; un autre backend implémente le contrat sans ces types. La première compilation a identifié des contrats génériques omis et le raccordement InternalsVisibleTo absent, à corriger avant validation.

Compilation du module Philips réussie le 2026-10-10 19:27:43 : 0 erreur, 0 avertissement. Cette vérification couvre les contrats et fabriques ; elle ne valide pas encore les machines en exécution ni leur interface.

Le bouton III de la manette CD-i active simultanément les boutons I et II. La notice technique du fabricant de l’adaptateur décrit explicitement cette combinaison : https://www.theworldofcdi.com/wp-content/uploads/2018/11/cdi-adapter-instructions.pdf (page 2). Le driver des deux cœurs ne présente que deux bits de bouton ; la troisième commande utilisateur les active ensemble sans inventer un bouton natif distinct.


Essai natif M2000 v0.9.3 : le tableau desc de retro_load_game contient exactement dix entrées sans sentinelle null (source attestée, m2000_libretro.c lignes 557–568). La lecture du onzième pointeur a provoqué une violation mémoire dans CopyInputDescriptors. La limite de dix est définie dans les constantes M2000 et appliquée uniquement à son adaptateur Libretro ; les autres cœurs conservent la lecture terminée par sentinelle.

### Frontière entre moteurs et protocole natif

La fabrique commune retourne IEmulatorCore et le catalogue découvre MachineFactory, sans exiger LibretroMachineFactory. Les quatre adaptateurs actuellement intégrés choisissent ce backend ; une autre DLL peut fournir son propre adaptateur sur les mêmes contrats. Le magasin commun conserve uniquement des snapshots SDK. La conversion des identifiants de périphériques et des callbacks Libretro est dans NativeInputFunctions sous Interop, sans méthode native dans InputFrameStore. Cela ne constitue pas une validation d’un futur moteur qui n’existe pas encore.

Le démarrage réel de cdi2015 a révélé un accès mémoire invalide lors de retro_serialize_size avant la création de la machine MAME. Le moteur crée celle-ci lors du premier retro_run (libretro.c, lignes 548–568), alors que sa taille d’état appelle immédiatement retro_save_state (lignes 658–674). La règle propre à cet adaptateur reporte cette interrogation après la première frame. SAME_CDI actuel initialise MAME dans retro_load_game et conserve son ordre natif.

Après correction, cdi2015 termine son initialisation. Le diagnostic appelle retro_run séparément et constate encore C0000005 après OSD initialization complete, avant le retour de la première frame ; MODE1 et AUDIO donnent le même défaut. Cette seconde erreur reste à résoudre. SAME_CDI réussit avec le CHD de contrôle MODE1 : frame 768 × 280, sauvegarde et restauration de 5 578 692 octets, processus fermé. Aucune compatibilité avec un jeu commercial n'est revendiquée par ces fixtures.

Diagnostic Windows du 2026-10-10 : le défaut est une tentative d'exécution (ExceptionInformation[0] = 8) à 0x73B54E00. La DLL est chargée à 0x7FFF73A80000 ; le rappel lu à RBX+8 contient exactement 0x0000000073B54E00. Le retour sur la pile est DLL+0x11E317, immédiatement après callq *0x8(%rbx). Le désassemblage de DLL+0xD4E00 montre une entrée de fonction valide à 0x7FFF73B54E00 : les bits hauts manquent dans le rappel interne. Les fichiers cdi-exception.log et cdi-disassembly.log conservent ces relevés. Les noms affichés par LLDB à partir du dernier export ne désignent pas ces fonctions internes dépourvues de symboles.

La structure native delegate_mfp utilise FPTR pour m_function, puis int pour m_this_delta. osdcomm.h définit FPTR en UINT32 sans PTR64 et en UINT64 avec PTR64. Le Makefile active PTR64 sous Windows selon uname ou un fragment du PATH ; une compilation Windows x64 doit donc explicitement recevoir PTR64=1. Le rappel tronqué et l'objet décalé observés sont cohérents avec une représentation native compilée sur 32 bits dans cette DLL x64. La reconstruction correcte et son essai restent nécessaires ; l'adaptateur ne doit ni réécrire les pointeurs internes du moteur ni masquer ce défaut. Aucun callback vidéo hôte n'a été appelé avant l'exception.

Audit complémentaire des entrées et fichiers compagnons : m2000_keyboard.h atteste CODE à la position 32 (transport RETROK_BACKQUOTE), le double zéro du pavé à la position 18 (RETROK_KP_PERIOD), CLR LN à la position 40 (RETROK_KP_MULTIPLY) et le point du pavé à la position 16 (RETROK_KP_ENTER). CODE, 00 et CLR LN sont désormais des touches propres réaffectables, en plus de START/STOP/ZOEK. Le transport P2000 distingue les touches du pavé PC de leurs codes natifs ; les autres moteurs gardent leur conversion. Le contrôle ciblé applique une nouvelle touche physique pour chacune des six actions, vérifie que l'ancien alias n'active plus le callback natif, et contrôle le point, plus, moins et Enter du pavé. Les noms physiques restent dans les ressources invariantes de base. Le convertisseur de codes clavier a été déplacé dans Interop.

O2EM charge les échantillons The Voice depuis system/voice. Dans src/voice.c, les noms sont E480.WAV à E4FF.WAV, puis E880.WAV à EFFF.WAV ; l'initialisation n'active la voix que lorsqu'au moins un WAV a été décodé. Le réglage de volume ne suffit pas à fournir ces données. Le module sélectionne désormais un jeu optionnel de WAV fourni dans une archive ZIP via les champs firmware existants. Les noms de fichiers et le format WAVE canonique réellement accepté par le décodeur natif sont validés ; le BIOS est préparé indépendamment et les WAV sont placés dans system/voice. Les chemins internes de l'archive ne sont pas extraits ; seuls les noms natifs reconnus sont retenus, avec refus des doublons.

Le contrôle ciblé conserve la sélection The Voice après sérialisation/restauration de la configuration, refuse un WAV invalide au scan et à la préparation, vérifie les octets copiés et la présence séparée du BIOS, puis obtient une frame de la machine O2EM et confirme la fermeture de son processus. Les deux onglets ROM Videopac et l'onglet Clavier P2000 ont été rendus et inspectés avec leurs nouveaux libellés, sans ajout de sélecteur global de dossiers. Le WAV de contrôle est synthétique et silencieux : cet essai ne prouve pas le rendu vocal d'un jeu commercial.

Installation cdi2015 : CoreReleaseService.GetAvailableAsync interroge l'archive officielle du catalogue, puis InstallAsync extrait cette archive et enregistre son hash dans le manifeste. Le script de reconstruction locale ne modifie pas ce parcours. Une DLL reconstruite et testée sous artifacts ne prouve donc pas que le moteur installé par App fonctionne. La validation finale doit porter sur le binaire installé et sa provenance ; aucune correction de distribution n'est encore réalisée. Le script build-native.ps1 est syntaxiquement valide et refuse explicitement l'absence de g++ avant de créer un processus. La compilation native reste non exécutée faute de compilateur disponible.

Validation finale après retrait de cdi2015 : build Debug A réussi avec les quinze modules présents, puis contrôle temporaire utilisant le SDK et le module Philips publiés. Les 33 onglets des quatre machines sont rendus sans erreur WPF ; CD-i expose uniquement SAME CDi. Les cycles P2000T/Odyssey²/Videopac et le démarrage SAME CDi, ses frames et sauvegarde/restauration sont vérifiés ; tous les PID contrôlés sont sortis. Le disque CD-i synthétique prouve le démarrage BIOS et le cycle natif, pas la compatibilité des jeux commerciaux. La reconstruction cdi2015 est abandonnée et ne conditionne plus Philips.
