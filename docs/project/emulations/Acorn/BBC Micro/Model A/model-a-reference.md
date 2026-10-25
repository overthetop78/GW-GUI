# BBC Micro Model A — dossier technique pour AcornEmu

Recherche du 25 octobre 2026. Les caractéristiques communes aux BBC Micro sont regroupées dans [l'architecture de gamme](../bbc-micro-architecture.md). Les références transversales à AcornEmu se trouvent [au niveau Acorn](../../acornemu-references.md).

## Identification et périmètre

### Vidéo AcornEmu : ROM externe et balayage — implémentation du 25 octobre 2026

Le cœur est une implémentation C# interne au module Acorn : il exécute le CPU et les composants dans le processus de GW GUI, sans Libretro, B2, programme auxiliaire ou DLL d'émulateur externe. Les images ROM restent des données utilisateur externes.

**ROM de caractères.** Le réglage ROM possède désormais un troisième emplacement obligatoire, `acornemu.characters`, distinct des champs MOS et langage. Choisir le fichier externe `saa5050`. Le profil matériel Model A déclare le rôle `CharacterGenerator` dans Common. La reconnaissance et le contrôle des 960 octets sont communs ; l'adaptateur ne fait que relier ce rôle au champ et demander son chargement. La ROM n'est pas remplacée par les caractères du MOS et ses octets ne sont pas intégrés au code.

Les six fichiers ajoutés ont été identifiés par SHA-1. SAA5050 est classé officiel pour le Model A. Les autres relevés sont présentés comme partiels : jeux nationaux différents et qualité de relevé signalée BAD_DUMP dans MAME. Le fichier SAA5051 utilisateur est l'ancien relevé, et non le relevé corrigé actuel.

| Fichier local | Jeu de caractères | SHA-1 |
| --- | --- | --- |
| `saa5050` | Anglais, référence BBC | `6c8daba70374e5aa3a6402f24cdc5f8677d58a0f` |
| `saa5051` | Allemand, ancien relevé MAME 0.203 | `9ab9d24b845fe2964fba2f4770d54025d2c8026a` |
| `saa5052` | Suédois | `ec4bc515e28e851a6433f7ca0a11ede0f1d21a68` |
| `saa5055` | ASCII américain | `c5ce7fe84df6de6a317fa0e87bda413c82c04618` |
| `saa5056` | Hébreu | `2d1ff08b4dda15cf70832881750a962189455f41` |
| `saa5057` | Cyrillique | `5a93445dde03066073e2909a935900e5f8439d81` |

Le catalogue reconnaît également les variantes 5053/5054 et le relevé SAA5051 corrigé `07cd9b7edbed6ef7b527622533d6957f5d56aa91`. Une image de 960 octets non identifiée reste inconnue et sélectionnable pour ce rôle. Références : [MAME actuel, saa5050.cpp](https://github.com/mamedev/mame/blob/master/src/devices/video/saa5050.cpp), [MAME 0.203, saa5050.cpp](https://github.com/mamedev/mame/blob/mame0203/src/devices/video/saa5050.cpp), [fiche Philips/Mullard SAA5050](https://www.ecbazis.hu/pdf/saa5050.pdf).

**Découpe et fonctionnement.** Les chemins suivants sont relatifs à `Emulators/AcornEmu/` :

| Fichier | Responsabilité et liaison |
| --- | --- |
| `Services/Crtc6845.cs` | Registres masqués, compteurs de caractères/lignes/raster, adresse écran verrouillée au début de l'image, ajustement vertical, signaux HSync/VSync, interlace vidéo et forme/clignotement du curseur. Ne possède ni RAM ni palette. |
| `Services/TeletextSaa5050.cs` | Copie possédée de la ROM externe, glyphes, arrondis, couleurs, clignotement, dissimulation, double hauteur, mosaïques jointives/séparées et maintien. Les attributs sont reconstruits pour chaque ligne et leurs effets immédiats/différés sont distingués. |
| `Machines/BbcMicro/Common/Services/Video.cs` | Assemble CRTC, ULA et télétexte autour de la RAM empruntée. Avance à chaque cycle CPU, à raison d'un caractère tous les un/deux cycles selon le contrôle ULA. Dessine les caractères pendant leur lecture et détache les images terminées. |
| `Machines/BbcMicro/Common/Constants/VideoConstants.cs` | Géométrie de présentation 640×256 et largeurs du curseur ULA. |
| `Machines/BbcMicro/ModelA/Services/ModelAMachine.cs` | Appelle l'horloge vidéo depuis le bus CPU et raccorde le front VSync au VIA. Conserve le quota nominal de cycles des appels de présentation, tandis que le CRTC programme son balayage. |
| `Factories/AcornEmuMachineFactory.cs`, `Functions/FirmwareFunctions.cs`, `Dictionaries/ModelCatalog.cs` | Décrivent les champs ROM/vidéo et transmettent les valeurs et images choisies à la machine ; aucun circuit ne dépend d'App. |

Les écritures de RAM, palette et contrôle ULA affectent les pixels à venir, sans redessiner les pixels déjà balayés. Le curseur fonctionne aussi en bitmap, avec sa hauteur, son masquage, son clignotement et les largeurs ULA. Les deux phases des couleurs clignotantes ULA dépendent du bit 0 de contrôle écrit par le logiciel ; elles ne sont plus inventées à partir du compteur d'images. Références : [MAME, mc6845.cpp](https://github.com/mamedev/mame/blob/master/src/devices/video/mc6845.cpp) et [bbc_v.cpp](https://github.com/mamedev/mame/blob/master/src/mame/acorn/bbc_v.cpp).

**Sortie matérielle et présentation.** Le sélecteur `acornemu.video-output`, dans Vidéo, propose UHF PAL par défaut, composite monochrome, composite couleur (S39) et RGB. Le mode monochrome produit une image de luminance. Les trois sorties couleur fournissent pour l'instant la même image RGB propre : les différences analogiques ne sont pas recréées dans le cœur. Le RGB représente l'installation du connecteur SK3 sur la carte existante du Model A, pas une autre machine ni un boîtier d'émulation extérieur. Le composite couleur représente le montage S39/condensateur suivant la révision. Le choix est enregistré et appliqué à chaud par les interfaces runtime existantes, sans réinitialiser le CPU ni la RAM. Les commandes de présentation vidéo existantes restent utilisées.

Le traitement de signal existant reste après l'image du cœur : `SoftwareEmulationVideoProcessingPipeline` applique déjà les connexions composite/RF et les standards PAL/NTSC/SECAM avant les autres traitements de présentation. Les profils existants peuvent donc donner ces rendus sans ajouter un encodeur analogique dans AcornEmu. Le sélecteur matériel du module ne sélectionne pas automatiquement un profil VideoPresentation ; ses filtres et intensités restent ceux choisis dans l'application. Les noms invariants UHF PAL/RGB restent dans 00-Base ; les nouveaux libellés sont traduits par le script Argos existant.

**Validation et limites.** Les 77 contrôles Acorn passent. Avant leur suppression, les deux essais temporaires ont vérifié le démarrage, l'affichage et `PRINT 12345` en 16 et 32 Kio ; les six ROM SAA505x locales produisent une image. L'aperçu SAA5050 a été inspecté et le curseur réaligné ; le test et l'image PNG ont ensuite été supprimés. Les contrôles autonomes couvrent notamment palette et bit flash modifiés pendant l'image, curseur bitmap, compteurs/masques CRTC, glyphes externes, double hauteur, maintien, séparation, clignotement et dissimulation télétexte.

Il s'agit d'un balayage par caractère dans une surface utile fixe de 640×256, pas d'une simulation de chaque transition électrique : les délais exacts des pipelines, les demi-lignes d'entrelacement, le cadrage analogique complet et les formes d'onde PAL/RF restent des limites. Les signaux HSync/VSync sont calculés ; les bordures et toutes les géométries hors de la surface utile ne sont pas exposées comme une nouvelle surface dynamique.

### Vérification des composants et options vidéo — 25 octobre 2026

Dans `Emulators/AcornEmu/Machines/BbcMicro/Common/Services/Video.cs`, la première implémentation CRTC conserve les registres, exploite l'adresse de départ, le nombre de colonnes, la hauteur des caractères et le nombre de lignes affichées ; le curseur utilise ses registres dans le rendu télétexte. Les synchronisations et le balayage programmables complets, le curseur bitmap et les changements pendant le balayage restent à compléter. La synchronisation de la carte demeure nominale, fixée par les cycles par image.

L'ULA possède déjà son registre de contrôle, ses 16 entrées de palette, le décodage des pixels selon le mode et les couleurs clignotantes. Il n'y a pas à recréer ces fonctions. Le rendu actuel lit toutefois RAM et registres au moment de construire toute l'image : des changements effectués en cours de balayage ne sont pas restitués au moment où ils ont eu lieu. Ces compléments précèdent les filtres existants de VideoPresentation.

Recherche locale : les 24 fichiers du dossier utilisateur `Emulation/Machines/acorn/Firmware` ont été vérifiés par taille et SHA-1 ; aucun n'est la ROM SAA5050 de référence. Aucun fichier nommé SAA5050/chargen/télétexte n'a été trouvé parmi les sources du dépôt inspectées, hors sorties de compilation. Cette recherche n'est pas un inventaire de tous les disques du PC.

La [définition du composant dans MAME](https://github.com/mamedev/mame/blob/master/src/devices/video/saa5050.cpp) identifie le fichier `saa5050` de 960 octets : CRC32 `201490f3`, SHA-1 `6c8daba70374e5aa3a6402f24cdc5f8677d58a0f`. La [page Planet Emulation SAA5050](https://www.planetemu.net/en/rom/mame-roms-merged/saa5050) propose `saa5050.zip` contenant un fichier du même nom et du même CRC. Le téléchargement n'a pas été effectué. Le choix pour AcornEmu est un fichier externe sélectionné dans ROM, avec un rôle propre au générateur de caractères, distinct du MOS et du langage. Son chargement n'est pas encore implémenté. Cette ROM interne au circuit réel fournit les glyphes, tandis que les attributs et le fonctionnement temporel restent à émuler.

Options matérielles documentées : la sortie UHF PAL couleur est présente sur le Model A ; la prise BNC fournit la vidéo composite normalement monochrome. Le manuel décrit l'ajout de couleur par le lien S39 sur les cartes Issue 4 et suivantes, ou par un condensateur sur les précédentes. La prise RGB est une extension du Model A, avec ajout du connecteur SK3. Il s'agit de sorties et de montages matériels, pas d'une autre carte graphique ni d'une RAM vidéo séparée. Sources : [Service Manual, section 7.1](https://www.retroisle.com/others/acornbbc/OriginalDocs/BBC%20Model%20A%20and%20B%20Service%20manual.pdf), [Hardware Guide, section 5.17](https://ftp.nvg.ntnu.no/pub/bbc/doc/A%20Hardware%20Guide%20for%20the%20BBC%20Microcomputer/bbc_hw_05.htm). Aucun nouveau filtre ni réglage vidéo n'a été créé pendant cette vérification.

### Réglages RAM et ROM disponibles dans AcornEmu

Le Model A propose désormais **16 ou 32 Kio**, avec **16 Kio par défaut**. L'extension ne change ni le processeur ni les autres équipements du modèle. Le guide matériel Acorn décrit la carte prévue pour 32 Kio, dont seulement 16 Kio sont montés sur le Model A : [guide matériel, introduction](https://www.manualslib.com/manual/1422370/Acorn-Computers-Limited-Bbc-A.html?page=7).

`Common/Machines/BbcMicro/Constants/ModelAConstants.cs` définit les valeurs enregistrées `16` et `32`. `Functions/MemoryFunctions.cs` résout l'option commune `acorn_main_memory` et rejette toute autre valeur. La fabrique AcornEmu expose ce choix dans RAM avec redémarrage requis. Les fonctions Common de réglages remplacent l'information fixe par ce sélecteur ; le résumé utilise la capacité sélectionnée. L'option suit le stockage de configuration existant.

`Emulators/AcornEmu/Dictionaries/ModelCatalog.cs` transmet la capacité à `Machines/BbcMicro/ModelA/Services/ModelAMachine.cs`. La carte alloue cette RAM et l'emprunte au rendu `Machines/BbcMicro/Common/Services/Video.cs`. À 16 Kio, les adresses 4000–7FFF reflètent 0000–3FFF ; à 32 Kio, elles désignent une mémoire indépendante. Le changement ne crée pas une seconde copie de la mémoire vidéo.

Les ROM choisies remplissent deux rôles : **MOS système** et **langage**, BASIC par défaut. Le MOS est nécessaire au démarrage ; BASIC lui-même peut être remplacé par un autre langage ou logiciel ROM adapté, tel que VIEW. Le jalon actuel exige une image dans chacun des deux emplacements. La machine réelle possède aussi des emplacements de ROM supplémentaires ; leur sélection et leur matériel associé ne sont pas encore proposés par ce cœur. Les deux champs ne limitent donc pas les fichiers possibles à deux versions. Voir le [guide matériel Acorn](https://www.manualslib.com/manual/1422370/Acorn-Computers-Limited-Bbc-A.html?page=7) et les [instructions de montage des ROM de Computer Concepts](https://chrisacorns.computinghistory.org.uk/docs/CC/CC_ROMFittingInst.pdf).

Le profil commun `Common/Machines/BbcMicro/Dictionaries/FirmwareCompatibilityCatalog.cs` classe comme **officiels pour le Model A** les MOS 0.10, 0.92, 1.00 et 1.20 ainsi que BASIC I et II, selon les identités et associations MAME détaillées ci-dessous. Les variantes visant d'autres machines restent évaluées séparément ; l'origine Acorn ne suffit pas à les déclarer officielles pour ce modèle. Les fichiers inconnus restent inconnus.

L'application affiche une icône de statut avec le texte déjà traduit en infobulle et comme nom accessible. La colonne de statut ne partage plus la largeur de celle du rôle ROM. Aucune ressource de traduction ni contrat du SDK n'a été ajouté pour cet affichage.

Validation : 41 contrôles AcornEmu réussis avec les essais temporaires des ROM locales, dont le démarrage et `PRINT 12345` en 16 et 32 Kio ; fichier temporaire supprimé après utilisation. Après couverture des six identités officielles, les 42 contrôles permanents AcornEmu passent. Les vérifications RAM couvrent les alias, la mémoire haute indépendante, la lecture vidéo, le reset, le sélecteur, l'application du choix, le résumé et le rejet des capacités invalides.

Le modèle étudié est le **BBC Microcomputer Model A britannique**, construit par Acorn et lancé avec la gamme BBC Micro en 1981. La référence `ANA01` désigne le Model A ; `ANA02` désigne une version avec Econet dans l'inventaire matériel du pilote MAME. Le numéro de modèle, la révision de carte, les ROM et les extensions sont des informations distinctes. [MAME, identification des machines](https://github.com/mamedev/mame/blob/84981cc15cfc1429e48f098d80c6dd97bc82a0bc/src/mame/acorn/bbcb.cpp#L5).

La fiche de collection du [Centre for Computing History](https://www.computinghistory.org.uk/det/3041/Acorn-BBC-Micro-Model-A/) situe le lancement en décembre 1981 et décrit la filiation avec le prototype Proton. Son inventaire des connecteurs mélange cependant les possibilités de gamme et l'équipement du Model A : pour les interfaces effectivement montées, les documents constructeur et les cartes photographiées sont plus précis.

## Caractéristiques d'origine

| Élément | Configuration Model A |
| --- | --- |
| CPU | 6502A, NMOS, 2 MHz ; accès à certains périphériques ralentis. |
| RAM | 16 Kio, soit 16 384 octets. |
| ROM | 16 Kio de MOS et 16 Kio de langage BASIC avec assembleur 6502. |
| Clavier | QWERTY à 73 touches, dont dix fonctions f0–f9. |
| Son | Trois voix tonales et une voie de bruit, haut-parleur interne. |
| Stockage de base | Magnétophone externe, 300 ou 1 200 bauds, commande du moteur. |
| Sorties installées | Téléviseur UHF canal 36, vidéo composite BNC ; cassette DIN 7 broches. |
| Modes MOS accessibles avec 16 Kio | 4, 5, 6 et 7. |
| Alimentation | Intégrée dans le boîtier. |

Source : [Acorn, Service Manual, §1.2.1](https://acorn.huininga.nl/pub/unsorted/manuals/BBC%20Microcomputer%20Service%20Manual-HTML/BBCServiceManual.html). Les fréquences de traitement du cœur et le débit de sortie audio de l'application sont deux paramètres différents.

## Équipement absent et extensions

| Fonction | État de base | Conséquence pour la description du modèle |
| --- | --- | --- |
| Sortie RGB | Connecteur non monté. | Le traitement interne RGB ne signifie pas qu'une prise RGB est disponible. |
| RS423 | Connectique et adaptation électrique absentes. | L'ACIA reste utilisée pour la cassette. |
| Interface analogique/joysticks | Absente. | Pas de joysticks analogiques natifs sans extension. |
| VIA utilisateur, imprimante parallèle et user port | Absents. | Ne pas exposer leurs registres comme un périphérique équipé. |
| Connecteurs inférieurs 1 MHz et Tube | Absents. | Les coprocesseurs et périphériques correspondants supposent une mise à niveau. |
| Disque, contrôleur et DFS | Absents. | Une image disque seule ne suffit pas à donner un lecteur à cette machine. |
| Econet | Absent sur ANA01 ; variante/extension distincte. | Décrire explicitement la configuration équipée. |
| Synthèse vocale | Option, pas le son de base. | Le PSG standard ne doit pas être remplacé par le système de parole. |

Le [Model A Issue 7 photographié par Chris Whytehead](https://chrisacorns.computinghistory.org.uk/Computers/BBCAI7.html) montre les prises manquantes et les zones non peuplées. C'est un exemplaire non mis à niveau, avec 16 Kio et MOS 1.20. Le [Model A Issue 2 de la même collection](https://chrisacorns.computinghistory.org.uk/Computers/BBCAI2.html) possède au contraire plusieurs ajouts, dont 32 Kio, RGB, interface analogique et port parallèle. Une photo ou une étiquette « Model A » ne suffit donc pas à déterminer la configuration d'origine.

Le manuel Acorn décrit des mises à niveau partielles : RAM et VIA utilisateur peuvent être nécessaires pour certains logiciels Model B ; la pagination de ROM nécessite aussi son circuit de sélection. Cela établit des variantes matérielles, pas une simple option logicielle « rendre tout compatible ». [Service Manual, §4.7](https://acorn.huininga.nl/pub/unsorted/manuals/BBC%20Microcomputer%20Service%20Manual-HTML/BBCServiceManual.html).

## Mémoire : le point sensible des 16 Kio

Les 16 Kio sont constitués de huit circuits DRAM de 16 K × 1 bit. Le montage 32 Kio ajoute un second groupe de huit circuits. Le cavalier S25 intervient dans la sélection des banques ; son état et le décodage doivent correspondre à la carte étudiée. [Hardware Guide, RAM et S25](https://ftp.nvg.ntnu.no/pub/bbc/doc/A%20Hardware%20Guide%20for%20the%20BBC%20Microcomputer/bbc_hw_04.htm).

### Adresses `4000–7FFF`

Il faut distinguer capacité physique et adresses vues par le CPU. Le pilote [MAME bbca_mem](https://github.com/mamedev/mame/blob/84981cc15cfc1429e48f098d80c6dd97bc82a0bc/src/mame/acorn/bbcb.cpp#L118) relie toute la plage `0000–7FFF` au périphérique RAM configuré à 16 Kio. Le code [B-Em m6502_reset](https://github.com/stardot/b-em/blob/a76c3655140a9fc3c70ecef25660932e0890b1e5/src/6502.c#L1027) fait aussi partager la mémoire entre les deux moitiés pour le Model A.

**Résultat de comparaison logicielle :** ces implémentations traitent les deux moitiés comme des alias de la même capacité physique. Ce n'est pas une seconde banque indépendante. La correspondance exacte CPU/vidéo et les effets de S25 restent à confirmer sur le schéma de la révision retenue avant d'en faire une règle matérielle définitive dans AcornEmu.

Le manuel de maintenance couvre plusieurs révisions et son texte numérisé contient des repères de circuits parfois incohérents. Les numéros d'IC doivent être repris du schéma, pas harmonisés arbitrairement depuis une transcription.

## Affichage utilisable avec 16 Kio

| Mode | Image ou texte | Couleurs logiques simultanées | RAM réservée à l'écran |
| --- | --- | --- | --- |
| 4 | Bitmap 320 × 256 ; texte 40 × 32. | 2 | 10 Kio. |
| 5 | Bitmap 160 × 256 ; texte 20 × 32. | 4 | 10 Kio. |
| 6 | Texte 40 × 25. | 2 | 8 Kio. |
| 7 | Télétexte 40 × 25. | Attributs télétexte. | 1 Kio. |

Les modes 0–3 ne font pas partie de la configuration MOS standard à 16 Kio. Le Model B ajoute la mémoire permettant leur utilisation. Ces modes sont des réglages logiciels du matériel vidéo commun, pas quatre circuits graphiques différents. [Service Manual, §1.2.1–1.2.2](https://acorn.huininga.nl/pub/unsorted/manuals/BBC%20Microcomputer%20Service%20Manual-HTML/BBCServiceManual.html).

**Calcul de capacité**, avec `PAGE=0E00` et avant espace des variables/pile BASIC : 16 384 − 3 584 − mémoire écran donne environ 2 560 octets en modes 4/5, 4 608 en mode 6 et 11 776 en mode 7. Ce calcul est une borne explicative, pas une mesure de mémoire BASIC libre après démarrage ; la valeur réelle dépend du MOS, des réservations et du programme.

La sortie UHF est décrite comme PAL 625 lignes/50 Hz par Acorn. Il faut la distinguer de la géométrie logique programmée par le CRTC. La configuration initiale de comparaison de MAME utilise 16 MHz, 1 024 périodes par ligne et 312 lignes : **calcul** `16 000 000 / (1 024 × 312) ≈ 50,080128 Hz`. Ce résultat n'impose pas une cadence fixe à tous les modes réels. [MAME, configuration bbca](https://github.com/mamedev/mame/blob/84981cc15cfc1429e48f098d80c6dd97bc82a0bc/src/mame/acorn/bbcb.cpp#L436).

La vidéo composite n'est pas uniformément couleur sur toutes les révisions : le manuel distingue le signal de luminance et l'ajout de couleur sur certaines cartes. L'option de rendu doit donc être documentée à partir du montage choisi, et non déduite du seul connecteur BNC. [Service Manual, §2.2 et §3.4](https://acorn.huininga.nl/pub/unsorted/manuals/BBC%20Microcomputer%20Service%20Manual-HTML/BBCServiceManual.html).

## ROM : configurations identifiées

La révision du circuit imprimé ne fixe pas à elle seule la version du système. B-Em fournit notamment deux profils Model A : OS 0.10 et OS 1.20. [B-Em, b-em.cfg](https://github.com/stardot/b-em/blob/a76c3655140a9fc3c70ecef25660932e0890b1e5/b-em.cfg#L7).

Les associations suivantes proviennent de la configuration `bbca` de MAME ; elles décrivent cette implémentation, pas toutes les combinaisons historiquement commercialisées.

| MOS | Langage associé dans MAME | Particularité |
| --- | --- | --- |
| 0.10 | BASIC I | Pas de pagination MOS ; BASIC est répliqué dans les pages de la représentation logicielle. |
| 0.92 | BASIC I | Variante de ROM distincte. |
| 1.00 | BASIC II | Variante de ROM distincte. |
| 1.20 | BASIC II | Configuration par défaut du pilote. |

### Empreintes de référence

Chaque image ci-dessous mesure `0x4000`, soit 16 384 octets. Ce sont des identifiants de contrôle publiés dans le code, pas des images ROM ajoutées au dépôt.

| Image | CRC32 | SHA-1 |
| --- | --- | --- |
| `os01.rom` | `45ee0980` | `4b0ece6dc139d5d3f4fabd023716fb6f25149b80` |
| `os092.rom` | `59ef7eb8` | `dca33995c0d008a527efe923d03333394b01022c` |
| `os10.rom` | `9679b8f8` | `d35f6723132aabe3c4d00fc16fd9ecc6768df753` |
| `os12.rom` | `3c14fc70` | `0d9bcaf6a393c9ce2359ed700ddb53c232c2c45d` |
| `basic1.rom` | `b3364108` | `890f6e3e7fab3340f75b85e93ff29332bc9ecb2e` |
| `basic2.rom` | `79434781` | `4a7393f3a45ea309f744441c16723e2ef447a281` |

Source : [MAME, ROM_START(bbca)](https://github.com/mamedev/mame/blob/84981cc15cfc1429e48f098d80c6dd97bc82a0bc/src/mame/acorn/bbcb.cpp#L778). Aucun droit de redistribution des ROM n'est déduit de leur présence dans une liste d'empreintes.

## Télécharger les ROM du Model A

Les deux images nécessaires au fonctionnement de base sont **le MOS et BASIC**. Le Model A et le Model B britanniques peuvent utiliser les mêmes images ; leur différence matérielle ne requiert pas systématiquement une ROM différente. Les jeux ci-dessous correspondent aux associations déjà identifiées dans les sources d'émulation.

| Jeu | Image système | Image langage | Fonctionnement concerné |
| --- | --- | --- | --- |
| MOS 1.20 + BASIC II | [os12.rom, téléchargement](https://raw.githubusercontent.com/stardot/b-em/a76c3655140a9fc3c70ecef25660932e0890b1e5/roms/os/os12.rom) | [basic2.rom, téléchargement](https://raw.githubusercontent.com/stardot/b-em/a76c3655140a9fc3c70ecef25660932e0890b1e5/roms/general/basic2.rom) | Référence documentée pour une machine avec le système 1.20, également observé sur un Model A physique. |
| MOS 0.10 + BASIC I | [os01.rom, téléchargement](https://raw.githubusercontent.com/stardot/b-em/a76c3655140a9fc3c70ecef25660932e0890b1e5/roms/os/os01.rom) | [basic1.rom, téléchargement](https://raw.githubusercontent.com/stardot/b-em/a76c3655140a9fc3c70ecef25660932e0890b1e5/roms/general/basic1.rom) | Variante ancienne ; la pagination doit correspondre à ce système et au montage de la machine. |

Les quatre liens ont été lus et leurs SHA-1 comparés à la table de référence ci-dessus, le 25 octobre 2026. Les [miroirs jsbeeb](../../acornemu-references.md#miroirs-vérifiés-dans-jsbeeb) fournissent les mêmes contenus. Chaque image mesure 16 384 octets ; les noms `os.rom` et `BASIC.ROM` de jsbeeb désignent respectivement les mêmes contenus que `os12.rom` et `basic2.rom` de B-Em.

Pour MOS 0.92 et 1.00, les empreintes sont identifiées dans MAME ; aucun lien binaire direct n'a été vérifié dans cette recherche. Un lien n'est donc pas fabriqué à partir du seul nom attendu.

### À quoi sert chaque fichier ROM ?

| Fichier | Contenu exécuté | Avec quoi il communique | Chargement |
| --- | --- | --- | --- |
| `os12.rom` | Démarrage MOS 1.20, gestion des entrées/sorties et services du système. | CPU, RAM de travail, VIA, CRTC, ULA vidéo, ACIA/ULA série et langage BASIC. | Banque système ; lecture en `C000–FBFF` et `FF00–FFFF`, avec les pages d'E/S intercalées. |
| `basic2.rom` | Interpréteur BASIC II et assembleur intégré. | CPU et RAM du programme ; appels MOS pour clavier, caractères, graphismes, son et fichiers. | Fenêtre de langage `8000–BFFF` selon la sélection de ROM. |
| `os01.rom` | Variante MOS 0.10 avec ses propres routines et limitations. | Même famille de matériel, mais comportements logiciels à conserver pour cette version. | Banque système correspondant au montage ancien ; ne pas lui attribuer automatiquement la pagination du MOS 1.20. |
| `basic1.rom` | BASIC I, associé au profil ancien étudié. | CPU, RAM et services du MOS chargé. | Fenêtre de langage ; les copies de banques effectuées dans MAME sont une représentation logicielle du profil. |

Les ROM ne se transmettent pas des objets C# : le **CPU exécute leurs instructions**, avec des appels et lectures/écritures sur les adresses de la machine. Le cœur expose les composants qui répondent à ces accès. Les interfaces C# concernent la liaison entre le cœur, le Common du module et GW GUI.

Les rôles reposent sur les [déclarations de ROM MAME](https://github.com/mamedev/mame/blob/84981cc15cfc1429e48f098d80c6dd97bc82a0bc/src/mame/acorn/bbcb.cpp#L778), les [profils B-Em](https://github.com/stardot/b-em/blob/a76c3655140a9fc3c70ecef25660932e0890b1e5/b-em.cfg#L7) et les références MOS de la fiche de gamme.

### Ce qui ne constitue pas une ROM de base du Model A

Une ROM DFS apporte le logiciel de fichiers disque, pas un lecteur ni son contrôleur. Une ROM réseau exige la configuration réseau correspondante. Un fichier cassette contient le média, pas le système. Les autres modèles disposent d'images système propres : les [ROM B+, Master et Compact trouvées](../../acornemu-references.md#dépôt-b-em) sont documentées séparément des deux ROM du Model A.

## Du fichier ROM à la machine en fonctionnement

### Démarrage avec MOS 1.20

La lecture directe de l'image `os12.rom` vérifiée donne les vecteurs suivants. Ce sont des valeurs mesurées dans ce fichier précis, à l'offset `adresse CPU − C000` ; elles ne sont pas généralisées aux autres MOS.

| Adresse CPU | Octets interprétés little-endian | Destination |
| --- | --- | --- |
| `FFFA–FFFB` | Vecteur NMI | `0D00`, dans la RAM. |
| `FFFC–FFFD` | Vecteur RESET | `D9CD`, dans le MOS. |
| `FFFE–FFFF` | Vecteur IRQ/BRK | `DC1C`, dans le MOS. |

Le CPU lit le vecteur RESET et exécute le MOS à `D9CD`. Les routines effectuent ensuite l'initialisation de la machine et du langage par leurs accès matériels. Les adresses NMI/IRQ montrent pourquoi une sauvegarde de RAM seule ne suffit pas : les autorisations et événements des composants interviennent dans les interruptions.

Pour suivre les routines instruction par instruction, le [réassemblage MOS commenté](https://tobylobster.github.io/mos/) propose [os120_acme.a](https://tobylobster.github.io/mos/os120_acme.a). Ce fichier est une **source assembleur**, pas une ROM binaire à placer directement dans le chargeur de firmware. Le site explique comment produire le binaire original à partir de cette source.

### Circuit des données pendant l'exécution

| Échange | Parcours et effet |
| --- | --- |
| Instruction | CPU → bus → ROM ou RAM → décodage de l'instruction et cycles suivants. |
| Lecture de touche | Touche maintenue → matrice clavier → liaison VIA → interruption ou interrogation → MOS → BASIC/programme. |
| Caractère affiché | Programme → service MOS → mémoire écran et registres → CRTC/ULA/SAA5050 → image produite. |
| Effet graphique direct | Programme → RAM écran ou registres vidéo ; le passage par les fonctions MOS n'est pas obligatoire. |
| Note sonore | Programme/MOS → bus lent du VIA et validation du PSG → compteurs et mélange sonore → sortie audio. |
| Réception cassette | Média et moteur → signal cassette → ULA série/ACIA → statut et interruption → lecture des données par le CPU. |
| Enregistrement cassette | CPU/MOS → données et commandes ACIA/ULA → signal et moteur → média de destination. |
| Changement de banque | Écriture CPU → sélection ROM lorsque le montage le permet → lectures suivantes dans la banque sélectionnée. |
| Synchronisation | Horloges → compteurs vidéo, timers, son et série → événements matériels → CPU ou production de sorties. |

Cette table relie les éléments techniques documentés dans [l'architecture BBC Micro](../bbc-micro-architecture.md). Elle définit les échanges à représenter, pas des méthodes C# qui existeraient déjà dans AcornEmu.

## Raccordement aux fichiers déjà présents dans GW GUI

Les chemins suivants sont relatifs à `src/GWGUI.Emulation.Acorn/`. Leur rôle a été relevé dans les sources locales. Le premier cœur AcornEmu est désormais créé ; ses fichiers et ses limites sont détaillés ci-dessous.

| Fichier existant | Rôle et liaison |
| --- | --- |
| `Modules/AcornEmulationModule.cs` | Façade du module : reçoit les demandes par les contrats Emulation et délègue au moteur Common. |
| `Common/Services/Engine.cs` | Moteur commun du module : configuration, sélection d'adaptateur et création du runtime selon les services fournis par l'hôte. |
| `Common/Dictionaries/EmulatorCatalog.cs` | Obtient les adaptateurs par le service Common de découverte, les identifie et filtre leurs modèles compatibles. |
| `Common/Machines/BbcMicro/Constants/ModelConstants.cs` | Identifiants et valeurs descriptives des modèles BBC Micro, utilisés par leur catalogue. |
| `Common/Machines/BbcMicro/Dictionaries/ModelCatalog.cs` | Décrit déjà `BbcMicroModelA`, avec 16 Kio, 6502A, clavier, cassette et catégories de firmware système/BASIC. Cela décrit le matériel, pas un cœur Model A exécutable. |
| `Common/Machines/BbcMicro/Dictionaries/KeyboardDictionary.cs` | Données de clavier BBC Micro utilisées par les réglages d'entrée de la famille. |
| `Common/Machines/BbcMicro/Functions/BbcMicroInputSettingsFunctions.cs` | Construction des réglages d'entrée spécifiques aux machines BBC Micro à partir des données existantes. |
| `Common/Interfaces/IEmulatorAdapter.cs` | Contrat local réunissant les facettes de configuration, création, capacités, firmware, médias et entrées de l'adaptateur. |
| `Common/Interfaces/IEmulatorFirmwareAdapter.cs` | Contrat des emplacements firmware, de l'inspection, de la compatibilité et de la préparation des images pour une session. |
| `Common/Interfaces/IEmulatorCore.cs` | Contrat local du cœur : runtime, commandes et identité, construit à partir des interfaces publiques d'Emulation. |
| `Common/Services/Machine.cs` | Session commune : possède le cœur, conserve la configuration, les médias et la file de commandes ; présente les facettes vidéo, audio, entrées, état et cycle de vie. |
| `Common/Services/Machine.Lifecycle.cs` | Partie du cycle de vie de cette même session ; elle doit rester le point commun de conduite du cœur. |
| `Common/Services/Machine.Commands.cs` | Partie des commandes de session ; leur exécution est reliée au cœur par son contrat. |
| `Common/Services/AudioOutputController.cs` | Sortie audio de la session, distincte de l'émulation du PSG et de ses registres. |
| `Common/Services/StateStore.cs` | Service existant de persistance des états ; le contenu matériel du futur cœur devra passer par les contrats de sauvegarde existants. |
| `Emulators/B2/Factories/B2MachineFactory.cs` | Adaptateur B2 existant ; fournit ses réglages et conversions de modèles. Ce fichier n'implémente pas le CPU ou le matériel BBC. |
| `Emulators/B2/Functions/ModelFunctions.cs` | Correspondance entre modèles GW GUI et options B2 ; les cas actuels comprennent B, B+, B+128 et Master 128. |
| `Emulators/Common/Interop/Factories/MachineFactory.cs` | Fabrique partagée des cœurs externes : création du processus et raccordement libretro. Elle est liée à ce mode d'exécution ; elle n'est pas à considérer automatiquement comme la base d'un cœur interne. |

### Chaîne d'interconnexion à conserver

`App ↔ Emulation ← Common du module Acorn ↔ adaptateur ↔ cœur ↔ composants de la machine`.

L'Application et Emulation connaissent les contrats ; les détails du 6502, de la mémoire BBC et des périphériques restent dans le module. Le Common assure les échanges avec Emulation. Les médias passent par les services existants exposés par Emulation vers MediaEngine. Les images ROM sont des données de firmware du modèle ; elles ne remplacent aucune interface d'interconnexion.

## Découpe et documentation du futur cœur Model A

La découpe distingue les composants réutilisables, leurs connexions sur la carte BBC, les paramètres du modèle et l'adaptation aux interfaces du module. L'inventaire des premiers fichiers réellement créés figure dans la section suivante ; les responsabilités du tableau restent à compléter au fil des jalons.

| Responsabilité à isoler | Données et fonctionnement | Liaisons à expliquer dans les futurs fichiers |
| --- | --- | --- |
| CPU 6502A | Registres, instructions, modes d'adressage, cycles, reset et interruptions. | Accès au bus et lignes IRQ/NMI/RESET. |
| Bus et décodage | Routage lecture/écriture, alias RAM et registres répétés. | CPU, RAM, ROM et composants ; ralentissements selon l'adresse. |
| RAM | 16 Kio physiques et mémoire de travail. | CPU et lecture vidéo ; une seule mémoire partagée. |
| ROM | Images identifiées, disposition et sélection de banque. | Chargeur de firmware, bus et configuration du modèle. |
| VIA 6522 | Ports, directions, timers, flags, autorisations et acquittements. | Horloge, clavier, verrou système, son et IRQ. |
| Verrou et câblage système | Validations clavier/son, voyants et lignes de mémoire écran. | Ports VIA et entrées des composants concernés. |
| CRTC | Compteurs, registres, adresses écran, curseur et synchronisations. | Horloge vidéo, RAM, ULA et signaux vers le VIA. |
| ULA vidéo | Sérialisation, palette et sélection bitmap/télétexte. | CRTC, données écran et sortie vidéo. |
| SAA5050 | Caractères, mosaïques et attributs télétexte. | Flux de caractères, synchronisations et sortie vidéo. |
| PSG | Périodes, atténuation, bruit et évolution des compteurs. | Validation par le bus lent, horloge et tampon audio produit. |
| Clavier | Matrice et touches pressées/relâchées. | Entrées communes traduites, balayage VIA et reset BREAK séparé. |
| ACIA et ULA série | Registres, cadences, statut, porteuse, moteur et interruptions. | CPU, horloges, cassette et IRQ. |
| Modèle A | Présence des composants, configuration ROM/RAM et câblage. | Assemblage des éléments communs ; différences avec le futur Model B. |
| Adaptation du cœur | Exécution, entrées, médias, sorties et sérialisation. | `IEmulatorCore` et services Common déjà présents. |

Pour **chaque fichier créé ou modifié**, la documentation de cette fiche et les commentaires de source devront expliquer :

- son chemin exact, son rôle et la raison de sa séparation ;
- les types et fonctions qu'il contient, leurs paramètres, résultats et unités ;
- les données qu'il possède et celles qu'il emprunte, leur initialisation et leur évolution ;
- qui l'appelle, ce qu'il appelle et les interfaces ou signaux utilisés ;
- le déroulement des lectures, écritures, cycles et interruptions qui le concernent ;
- sa participation au reset, aux états sauvegardés et à la libération des ressources ;
- sa portée commune ou spécifique au modèle, ainsi que les sources utilisées pour ses comportements.

## Premier jalon AcornEmu : démarrage et clavier

Implémentation du 25 octobre 2026. Le cœur démarre effectivement sur **MOS 1.20 et BASIC II externes**, affiche `BBC Computer 16K`, arrive dans BASIC et exécute une commande saisie au clavier. Il ne s'agit pas d'un affichage de texte d'accueil simulé : le processeur exécute les instructions des ROM.

### Inventaire des fichiers créés

Tous les chemins de cette table sont relatifs à `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/`.

| Fichier | Rôle, données et liaisons |
| --- | --- |
| `Constants/EmulatorConstants.cs` | Identité `acornemu`, nom, version du cœur et identifiants `acornemu.mos`/`acornemu.basic` des champs de firmware. Utilisé par la fabrique, le chargeur et le runtime. |
| `Constants/HardwareConstants.cs` | Fréquence CPU, taille RAM/ROM et géométrie du premier rendu. Partagé par la carte, la vidéo et le contrat de runtime ; ces constantes ne sont pas des réglages de l'hôte. |
| `Interfaces/ICpuBus.cs` | Accès mémoire, progression d'un cycle et ligne IRQ. Le CPU emprunte ce contrat à la carte sans connaître les services de GW GUI. |
| `Services/Cpu6502.cs` | Registres A/X/Y/S/P/PC, pile, vecteurs, IRQ, accès au bus, comptage des cycles et arithmétique binaire/décimale NMOS. Le bus possède la mémoire ; le CPU ne la duplique pas. |
| `Services/Cpu6502.Instructions.cs` | Décode les instructions officielles et leurs adressages, pénalités de page, branches, appels/retours et opérations mémoire. Complète la même classe CPU, sans second état de processeur. |
| `Services/Via6522.cs` | Registres des ports, directions, timers, IFR/IER, contrôles et acquittements. Les fonctions de broches sont raccordées par la carte ; `Tick` avance le VIA à 1 MHz. La ligne IRQ est lue par le CPU. |
| `Dictionaries/KeyboardDictionary.cs` | Coordonnées de la matrice BBC A/B à partir des identités de touches du module. Les deux touches SHIFT ou CTRL physiques peuvent désigner le même contact BBC. |
| `Services/Keyboard.cs` | État des contacts maintenus, interrogation ligne/colonne et commande BREAK. Ne lit aucun périphérique Windows : les touches arrivent par l'instantané du contrat d'entrée. |
| `Services/Video.cs` | Registres CRTC, contrôle/palette ULA, lecture de RAM écran et création d'une image XRGB8888. Le rendu bitmap tient compte du champ ULA et de l'horloge de caractères ; la vidéo emprunte RAM et ROM à la carte. |
| `Services/ModelAMachine.cs` | Assemble le CPU, le VIA, le clavier et la vidéo ; possède 16 Kio de RAM et des copies des ROM externes. Décode les adresses et les écritures du verrou système, pilote les horloges, le reset et une durée de frame. |
| `Functions/FirmwareFunctions.cs` | Lit les fichiers externes et identifie MOS 1.20/BASIC II par SHA-1. Retourne les candidats aux emplacements du contrat firmware. Les fichiers sont ouverts pendant la lecture seulement et restent inchangés. |
| `Factories/AcornEmuMachineFactory.cs` | Adaptateur découvert par le catalogue Common ; annonce le Model A, décrit les deux champs ROM, réutilise les réglages d'entrée/stockage/résumé Common et construit la session avec le cœur interne. Le cœur est livré dans le module et n'a pas de DLL native à installer. |
| `Services/EmulatorCore.cs` | Implémente `IEmulatorCore` : initialisation de la carte, instantanés d'entrée, exécution par frame, publication de l'image et reset. La session Common possède sa durée de vie. Le cœur ne crée ni fenêtre, ni processus, ni thread. |

Le fichier existant `Common/Machines/Common/Enums/CoreEnums.cs` ajoute `AcornEmu = 1` et conserve `B2 = 0`. La ressource du nom technique invariant est ajoutée dans la base commune du module. Aucune image MOS, BASIC ou caractère n'est intégrée aux sources ou aux ressources.

### Fonctionnement d'une session

1. Le catalogue Common découvre `AcornEmuMachineFactory` comme les autres adaptateurs. Le Model A dispose ainsi d'un runtime dans le parcours déjà utilisé par l'application.
2. Les champs ROM conservent les chemins utilisateur dans les options de configuration. La reconnaissance s'appuie sur les empreintes, pas sur un nom de fichier imposé.
3. La fabrique construit le `Machine` Common avec un `EmulatorCore`. Son `finally` libère le cœur si la construction ne lui en a pas transféré la propriété.
4. Lors de l'initialisation sur le thread possédé par Common, le cœur lit les ROM externes et construit `ModelAMachine`. Le CPU prend le vecteur RESET de l'image MOS chargée.
5. Chaque accès CPU avance le bus. Un cycle sur deux avance le VIA ; la carte fait évoluer le balayage clavier et publie la synchronisation verticale à la durée de frame nominale.
6. Les écritures du CPU dans les registres VIA, CRTC et ULA modifient les composants correspondants. Le MOS écrit ses caractères dans la RAM écran, que la vidéo lit pour produire l'image.
7. `EmulatorCore` transmet cette image, sa géométrie, son numéro de séquence et sa date au contrat vidéo. Common conserve la présentation, la file de commandes et l'arrêt de la session.
8. Après arrêt, la destruction détache la carte et les sorties. Les fichiers de ROM et les services empruntés à l'hôte restent à leur propriétaire.

### État exact du premier jalon

| Fonction | État |
| --- | --- |
| Instructions officielles 6502 | Programme fonctionnel Klaus Dormann exécuté jusqu'à sa boucle de succès `3469`. Cela ne valide pas tous les cycles transitoires du NMOS. |
| ROM externes | MOS 1.20 et BASIC II identifiés par leurs empreintes de référence. Les variantes anciennes documentées ne sont pas encore activées dans l'inspection du premier jalon. |
| Démarrage matériel | Bannière 16 Kio et arrivée dans BASIC vérifiées en exécutant les ROM. |
| Clavier | Saisie `PRINT 42`, RETURN, puis résultat `42` vérifiés dans la mémoire écran. |
| Vidéo bitmap | Rendu initial des modes standard ; un test vérifie les pixels du mode 4. Les effets raster et les reprogrammations avancées restent à traiter. |
| Télétexte | Premier rendu lisible à partir des glyphes bitmap contenus dans la ROM MOS externe. Les arrondis, la double hauteur, les effets de maintien et tous les timings SAA5050 restent à implémenter. |
| Horloges | Cadence nominale de 39 936 cycles CPU par frame. Les accès lents 1 MHz et les lectures transitoires exactes ne sont pas encore reproduits. |
| VIA | Ports, principaux timers et interruptions utiles au démarrage ; registre à décalage, modes avancés et détails des fronts à compléter. |
| Son et cassette | Pas encore émulés. Aucun lecteur ni sortie audio n'est annoncé comme fonctionnel par ce cœur. |
| États sauvegardés | Désactivés tant que la sérialisation de tous les composants n'est pas implémentée. |

### Vérifications effectuées

`tests/GWGUI.LocalEmulationTests/Emulation/AcornEmuTests.cs` contient sept tests autonomes, sans ROM, fichier ni application externe : intégration Common et options de firmware, addition/retenue, JMP indirect NMOS, arithmétique décimale, acquittement timer VIA, alias RAM/protection ROM et rendu mode 4.

Deux tests temporaires supplémentaires ont lu les ROM et le programme CPU en mémoire, sans enregistrer leurs octets sur disque : validation fonctionnelle 6502 et démarrage/saisie BASIC. Ils ont passé avant suppression de leur source temporaire. Les neuf cas ciblés ont passé ensemble. Ils n'ont créé aucune fenêtre, aucun processus d'émulation séparé ni dossier de session.

Référence CPU utilisée : [6502_65C02_functional_tests de Klaus Dormann](https://github.com/Klaus2m5/6502_65C02_functional_tests), programme `bin_files/6502_functional_test.bin`. Les ROM utilisées pour l'essai sont exactement celles de B-Em dont les SHA-1 sont listés dans cette fiche.

### Détails des sources conservés pour la suite

| Source et révision | Fichiers examinés et détail retenu |
| --- | --- |
| MAME `84981cc15cfc1429e48f098d80c6dd97bc82a0bc` | `src/mame/acorn/bbcb.cpp` : carte `bbca_mem`, matériel `bbca`, vecteurs de ROM et raccordement VIA. `bbc_kbd.cpp` : coordonnées des lignes/colonnes et liaison BREAK séparée. |
| B-Em `a76c3655140a9fc3c70ecef25660932e0890b1e5` | `src/6502.c` : alias de la RAM Model A. `src/sysvia.c` : bus lent sur port A, verrou IC32 sur port B, et inversion spéciale de CA2 pour OS 0.10 ; cette dernière appartient au futur support de cette variante. `b-em.cfg` : associations de ROM ; certains profils du dépôt activent des extensions, donc leur nom seul ne définit pas un Model A standard. |
| b2-libretro `9793f6a8040c416d147003cada29363b955b54e7` | `src/beeb/src/type.cpp` : Model B à 32 768 octets et séparation des types. `BBCMicro.h` : cœur, mémoire, entrée et fonctions de machine. `src/libretro/core.cpp` : adaptation au frontal, médias disques et limites cassette/états déjà consignées. |
| jsbeeb `801dc90317514cb6a35500abda5abaf2f9fbc8aa` | `src/models.js` : distinction des composants et paramètres de modèle. `src/video.js` : ULA et palette ; le stock décode A0 pour distinguer contrôle/palette. Les extensions VideoNuLA présentes dans cette source ne sont pas des composants du Model A d'origine. |

Ces références complètent les tableaux de registres et de branchements déjà conservés ici et dans le document BBC Micro. Elles indiquent les comportements retenus, sans prétendre que tous les détails de ces émulateurs sont déjà portés dans le premier jalon.

### Tester le build Debug

Le build ciblé a réussi avec `scripts/local-building.cmd --building=debug --modules=acorn`. L'exécutable est `build/Debug/GW GUI/gwgui.exe`. Le seul module de marque produit est Acorn ; Example reste fourni automatiquement par le script existant. La DLL Acorn distribuée a la même empreinte que la DLL compilée.

- Créer une machine **BBC Micro Model A** et sélectionner **AcornEmu**.
- Dans les réglages ROM, choisir les fichiers **MOS 1.20** et **BASIC II** externes dont les téléchargements figurent plus haut.
- Enregistrer la configuration et lancer la machine ; attendre l'invite BASIC, puis essayer une commande comme `PRINT 42`.
- Le premier jalon ne demande pas de cassette pour démarrer. Les retours de test guideront les étapes suivantes.

## Cassette et logiciels

Le lecteur est externe ; le matériel interne effectue les échanges série et la commande moteur. Une configuration cassette doit conserver le média inséré, sa position, le moteur, le signal entrant, la porteuse et l'état ACIA/ULA. Le [fonctionnement commun série/cassette](../bbc-micro-architecture.md#série-et-cassette) précise les composants concernés.

Le manuel BBC décrit notamment `*TAPE`, `CHAIN`, `LOAD`, `SAVE` et `*CAT`. Ces commandes permettent de définir des observations attendues depuis le système réel ; elles ne prouvent pas qu'un fichier est compatible 16 Kio. [BBC User Guide, cassette et systèmes de fichiers](https://www.bbcmicrobot.com/docs/BBC_User_Guide.pdf).

Un média destiné à un BBC Micro peut exiger 32 Kio, un MOS particulier, un disque ou une extension. La mention « BBC Micro » ne garantit donc pas son fonctionnement sur Model A. Le conteneur UEF et ses durées sont décrits [au niveau Acorn](../../acornemu-references.md#médias--ce-qui-peut-être-commun).

## Entrées et réglages pertinents

Cette table traduit les caractéristiques recherchées en informations à pouvoir décrire. Elle ne crée pas de nouvelles interfaces ni de réglages dans le code.

| Catégorie | Données du Model A à représenter |
| --- | --- |
| Modèle | Model A, révision matérielle et extensions distinctes. |
| CPU | 6502A et fonctionnement nominal ; les ralentissements sont matériels. |
| RAM | 16 Kio d'origine ; une extension éventuelle doit être explicitement identifiée. |
| ROM | Version MOS, version BASIC, disposition et circuit de sélection présents. |
| Vidéo | Circuit CRTC/ULA/télétexte et type de sortie correspondant au montage. |
| Son | PSG et haut-parleur ; parole uniquement avec l'extension correspondante. |
| Stockage | Cassette ; autres supports seulement avec leur matériel. |
| Clavier | Matrice BBC, relâchement des touches, BREAK et combinaisons de reset. |
| Contrôleurs | Clavier de base ; ports additionnels liés à une extension réellement décrite. |
| Souris | Aucun périphérique standard identifié pour le Model A d'origine dans les sources consultées. |

## Observations de référence pour une future validation

Ces observations découlent de la documentation et restent à exécuter sur AcornEmu quand le cœur existera. Ce ne sont pas des tests déjà passés.

| Domaine | Observation recherchée |
| --- | --- |
| Démarrage | Identification 16 Kio, langage BASIC, version MOS cohérente avec les images choisies. |
| Mémoire | Résultat des lectures/écritures dans les deux moitiés de la zone RAM et protection des ROM. |
| Clavier | Matrice, touches maintenues, verrouillages, interruption clavier et reset. |
| Vidéo | Modes 4–7, couleurs, curseur, défilement et attributs télétexte. |
| Son | Trois tonalités indépendantes, bruit, niveaux, commandes successives et enveloppes MOS. |
| Cassette | Lecture et écriture à chaque débit, arrêt moteur, reprise et erreurs de réception. |
| Temps | Timers VIA, synchronisation vidéo et accès lents comparés à une référence. |
| État | Reprise cohérente au milieu d'une image, d'un son ou d'un transfert cassette. |

## Partir du Model B de b2-libretro

Le Model B de b2-libretro est une référence proche pour étudier le Model A : les composants CPU, VIA système, vidéo et son couvrent une grande partie du matériel commun décrit dans [la fiche BBC Micro](../bbc-micro-architecture.md#référence-complémentaire--b2-libretro). La [liste officielle des modèles du cœur](https://docs.libretro.com/library/b2/#core-options) ne propose pas de Model A.

Le type B du port utilise explicitement 32 768 octets de RAM dans [type.cpp](https://github.com/zoltanvb/b2-libretro/blob/9793f6a8040c416d147003cada29363b955b54e7/src/beeb/src/type.cpp#L937). Il ne suffit donc pas de changer son nom dans le frontal pour obtenir le Model A.

| Élément du Model B | Travail d'adaptation identifié pour le Model A |
| --- | --- |
| RAM de 32 Kio et pagination | Représenter les 16 Kio physiques, leurs alias et l'adressage vidéo correspondant. |
| Périphériques supplémentaires | Reproduire les réponses du modèle non équipé : VIA utilisateur, ADC, disque et autres extensions selon la configuration réelle. |
| ROM et sélection latérale | Choisir les images et le montage du Model A, avec traitement distinct de MOS 0.10. |
| Chargement de disquettes | Prévoir le chemin cassette du Model A ; le port b2 consulté ne fournit pas cette prise en charge. |
| Réglages vidéo | Conserver le matériel programmable commun, avec les modes et réservations mémoire accessibles sur 16 Kio. |

Cette table est une **déduction d'adaptation** à partir des différences matérielles déjà sourcées dans cette fiche et du port étudié. Elle ne signifie pas que tous ces changements se limitent à des paramètres existants dans b2. Elle ne choisit pas non plus de transformer b2 en AcornEmu : b2 sert ici à comprendre et comparer les traitements communs.

## Informations restant à établir précisément

| Sujet | État de la recherche et pièce nécessaire |
| --- | --- |
| Révision matérielle initiale d'AcornEmu | Aucun choix imposé par cette recherche. Les cartes Issues 2 et 7 fournissent des exemples documentés différents. |
| RAM miroir et cavaliers | Comparaison logicielle effectuée ; vérifier les signaux CAS et le schéma de la révision sélectionnée. |
| Pagination ROM de base | Le montage physique et MOS 0.10 diffèrent du profil paginé. Vérifier S12/S13, IC76 et la disposition exacte des ROM. |
| Reset | Distinguer état électrique à froid, BREAK et actions supplémentaires du MOS avec CTRL-BREAK. |
| Variantes des circuits | Identifier les marquages CRTC, VIA et PSG sur la carte de référence. |
| Cassette bas niveau | Documenter complètement blocs, CRC, leader, erreurs et tolérances depuis les routines MOS et la documentation cassette. |
| Connecteurs | Relever les brochages depuis le schéma ; ne pas appliquer celui du Model B à un connecteur absent. |
| Mesures physiques et chronologie commerciale | Dimensions, poids, prix par date et consommation ne sont pas consolidés ici : les fiches génériques consultées divergent et ces chiffres ne définissent pas le cœur matériel. |

Le dossier rassemble les faits recoupés, les détails de programmation et les références du Model A. Les vérifications effectives sont celles du premier jalon décrit ci-dessus ; les autres révisions et les fonctions encore absentes restent à traiter.

## Saisie selon le clavier hôte

Le premier essai utilisateur confirme le démarrage, le clavier alphabétique et l'exécution de `PRINT "HELLO WORLD"`. Il relève que les symboles suivent encore les positions BBC (guillemet par SHIFT+2) et que le pavé numérique manque. Le traitement est complété pour suivre la disposition active du clavier hôte, AZERTY ou autre.

### Parcours et responsabilités

| Fichier | Rôle et liaison |
| --- | --- |
| `src/GWGUI.App/Functions/Input/Keyboard/EmulationKeyMapper.HostCharacters.cs` | Traduit les touches maintenues par `ToUnicodeEx`, avec la disposition active du thread de saisie et les modificateurs. Ce fichier ne connaît aucune marque ni machine émulée. Le drapeau 4 préserve l'état Windows des touches mortes. |
| `src/GWGUI.App/Controllers/Emulation/Machine/MachineInputController.cs` | Conserve la résolution prioritaire des raccourcis globaux et des affectations de machine. Retire les sources déjà consommées de la lecture physique complémentaire, puis publie les touches et caractères via le contrat d'entrée. Les cibles explicitement affectées restent des commandes, sans traduction de caractère concurrente. |
| `src/GWGUI.Emulation/Contracts/EmulationInputSnapshot.cs` | Transporte `Keys` et `HostCharacters`, dictionnaire des caractères par touche hôte maintenue. Cette donnée est indépendante du matériel émulé ; une entrée peut aussi contenir uniquement des commandes physiques. |
| `src/GWGUI.Emulation/Functions/EmulationInputMappingFunctions.cs` et `Services/EmulationInputFrameStore.cs` | Copient et conservent les caractères dans les instantanés de frame. L'accumulateur existant conserve également cette propriété avec les états absolus. |
| `src/GWGUI.Emulation/Functions/EmulationHostProtocolFunctions.cs` | Transporte les mêmes caractères entre processus par l'interface de protocole commune déjà utilisée par les modules. Après les touches : nombre d'associations (−1 en leur absence), puis identité de touche sur 32 bits et caractère UTF-16 sur 16 bits. Les nombres invalides et doublons sont refusés. |
| `src/GWGUI.Emulation.Acorn/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` | Les projections existantes par `with` conservent les caractères pendant le passage Common. Aucun chemin App→Acorn direct n'est ajouté. |
| `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Dictionaries/KeyboardDictionary.cs` | Décrit les contacts BBC et le SHIFT requis pour les symboles ; décrit aussi la correspondance des touches de pavé numérique avec les contacts existants. Les positions de `@`, des crochets et de la barre oblique inverse ont été vérifiées contre MAME. |
| `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Services/Keyboard.cs` | Convertit les caractères hôtes en contacts matériels, puis applique les commandes physiques. Reconstruit la matrice à chaque instantané pour relâcher touches et SHIFT synthétique. Les lettres conservent la gestion CAPS LOCK/SHIFT LOCK du MOS. Un espace ne modifie pas SHIFT. |
| `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Services/ModelAMachine.cs` | Fournit les touches et caractères à la matrice avant les interrogations du VIA. |

Ainsi, le guillemet de la touche 3 française devient SHIFT+2 côté BBC ; Maj+3 français devient 3 sans SHIFT côté BBC. Les chiffres, opérateurs, séparateur décimal et entrée du pavé numérique utilisent des touches BBC existantes, sans inventer un pavé matériel au Model A. CTRL/ALT restent des modificateurs de commandes ; AltGr conserve sa traduction hôte.

### Limites et vérifications

La matrice BBC possède un seul contact SHIFT : deux symboles maintenus nécessitant des états SHIFT opposés ne peuvent pas être présentés simultanément. Les caractères absents du clavier BBC, notamment les lettres accentuées, ne sont pas remplacés par le chiffre de la touche physique. La composition de texte multicaractère et les méthodes de saisie IME ne sont pas représentées par cette association d'un caractère à une touche maintenue.

Les tests autonomes couvrent les symboles, SHIFT synthétique, relâchement, pavé numérique, copie des caractères, passage Common et protocole entre processus. Les contrôles des raccourcis vérifient une source réintroduite par la lecture physique et une affectation explicite. 61 cas ciblés passent, puis le contrôle supplémentaire SHIFT+espace passe séparément. Quatre contrôles temporaires des dispositions Windows française, américaine et allemande passent ; leurs dispositions sont libérées dans `finally` et le fichier de tests temporaire est supprimé.

Références : [ToUnicodeEx, Microsoft](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-tounicodeex) ; [matrice BBC, MAME `bbc_kbd.cpp`](https://github.com/mamedev/mame/blob/84981cc15cfc1429e48f098d80c6dd97bc82a0bc/src/mame/acorn/bbc_kbd.cpp). Ces sources décrivent respectivement la traduction Windows et les contacts matériels, deux responsabilités distinctes.

## Firmwares du dossier utilisateur — 25 octobre 2026

Les 24 fichiers de `C:\Users\overt\AppData\Roaming\GW GUI\Emulation\Machines\acorn\Firmware` correspondent par SHA-1 aux déclarations de [MAME `bbcb.cpp`, révision 84981cc15cfc1429e48f098d80c6dd97bc82a0bc](https://github.com/mamedev/mame/blob/84981cc15cfc1429e48f098d80c6dd97bc82a0bc/src/mame/acorn/bbcb.cpp). Il faut distinguer identification, compatibilité matérielle et fonctionnement effectivement obtenu avec AcornEmu.

| Fichier | Identification par empreinte | Intérêt pour le Model A |
| --- | --- | --- |
| `basic1.rom` | BASIC I — `890f6e3e7fab3340f75b85e93ff29332bc9ecb2e` | ROM de langage de Model A ; MAME l'associe aux MOS 0.92 et 0.10. |
| `basic2.rom` | BASIC II — `4a7393f3a45ea309f744441c16723e2ef447a281` | ROM de langage de Model A ; couple documenté avec MOS 1.00/1.20. |
| `cm62024.bin` | Parole VSM — `b369809275cb67dfd8a749265e91adb2d2558ae6` | Données du synthétiseur de parole, pas un MOS ni un langage 6502. |
| `dfs0.9h.rom` | DFS 0.9H — `dbbec4d2540a854c120be3194c7566a2b79d153b` | Extension disque de 8 Kio ; contrôleur disque et banque ROM supplémentaires nécessaires. |
| `dfs10.rom` | DFS/NET — `161f585dc45665ea77433c84afd2f95049f7f5a0` | Extension disque/réseau ; même contenu que `usdnfs10.rom` dans MAME. |
| `dnfs120-201666.rom` | DNFS — `7e3c536baeae84d6498a14e8405319e01ee78232` | Extension disque/réseau, pas une ROM système de remplacement. |
| `eco_3.35k.rom` | NET Torch — `7462ced7b83d74b822815bc00ed40a89f84e0276` | Extension réseau des Torch 301/725 ; matériel non présent sur le Model A de base. |
| `hope_dfs.rom` | DFS 1.00 Hope — `4c9f7cf141ad028fe5e517c45e6e3a43b1a76d24` | Extension disque du clone Dolphin, matériel et banque ROM nécessaires. |
| `hope_os_basic.rom` | BASIC/MOS Hope — `0a07161fa2fbc102fb306233bd81790c7b04f793` | Image combinée de 32 Kio du Dolphin : BASIC dans la première moitié, MOS dans la seconde, selon ROM_COPY de MAME. À essayer comme variante, pas comme Model A officiel. |
| `mcp_1.01_ci.rom` | MCP Torch — `be10872aeb88714bd56462a2e86929953dee1c01` | Commande d'un système Torch et de ses extensions ; le nom du fichier indique 1.01, son en-tête annonce 1.00. |
| `mcp_1.22_ci.rom` | MCP 1.22 Torch — `409762deafb76b1f86be39bfbf2f812d5de3ff92` | Même famille Torch ; pas un MOS BBC Model A. |
| `nos12.rom` | MOS norvégien — `2b6aecd33a43f296c20832524e47cc7e3a9c3b17` | Variante Model B norvégien, proche du MOS BBC ; dispositions et glyphes nationaux distincts. |
| `os_de.rom` | MOS allemand — `aadf90338ee9d1c85dfa73beba50e930c2a38f10` | Variante Model B allemand, à distinguer d'une validation Model A. |
| `os01.rom` | MOS 0.10 — `4b0ece6dc139d5d3f4fabd023716fb6f25149b80` | MOS Model A officiel ; balayage clavier CA2 et pagination antérieurs à 1.20. |
| `os092.rom` | MOS 0.92 — `dca33995c0d008a527efe923d03333394b01022c` | MOS Model A officiel, avec BASIC I dans MAME. |
| `os10.rom` | MOS 1.00 — `d35f6723132aabe3c4d00fc16fd9ecc6768df753` | MOS Model A officiel, avec BASIC II dans MAME. |
| `os12.rom` | MOS 1.20 — `0d9bcaf6a393c9ce2359ed700ddb53c232c2c45d` | Couple déjà démarré avec BASIC II dans AcornEmu. |
| `sist1_progs.bin` | Programmes Cisco SIST1 — `4398ba29c871fa397654aa182c63ccdcad597625` | ROM de service du terminal industriel SIST1, pas un système Model A. |
| `sist1_startup.bin` | Démarrage Cisco SIST1 — `5ea266f47ff83821ccdbec006b8506b2e892b115` | Langage/démarrage du terminal SIST1 et de ses extensions, pas un BASIC standard. |
| `unix_1.00.rom` | UNIX Host 1.00 Torch — `e37d043c8df30c49ba8717e1aa0b92105cb0c937` | Communication avec le système UNIX Torch 725 ; coprocesseur et matériel associé nécessaires. |
| `usbasic3.rom` | BASIC III américain — `b39014610a968789afd7695aa04d1277d874405c` | Variante américaine du Model B ; à essayer sans la déclarer identique à BASIC II. |
| `usmos10.rom` | MOS américain — `83d91d089dca092d2c8b7c3650ff8143c9069b89` | Variante Model B américain, normalement NTSC, alors que le jalon Model A est PAL. |
| `viewa210.rom` | VIEW A2.1 — `88c93df1854f5fbe6cd6e5f0e29a8bf4ea3b5614` | Traitement de texte en ROM latérale ; ce n'est pas BASIC. RAM et implantation des banques à vérifier. |
| `vm61002.bin` | Parole VSM — `66876702d1d95eecc034d20f25047f893a27cde5` | Données de parole, inutilisables à la place de MOS/BASIC. |

L'affichage doit présenter aussi les fichiers non reconnus. Une empreinte absente du catalogue ne suffit pas à interdire un essai : les contraintes de taille et de disposition des octets restent celles du matériel. L'identification des extensions ne signifie pas que leur contrôleur ou leur banque ROM sont déjà émulés.

### Essais et présentation effective

Onze couples ont été essayés avec les fichiers locaux, sans modification des sources ni fenêtre d'émulation. Le test a suivi le CPU et les sorties OSWRCH, puis vérifié la commande et le résultat `12345` en mémoire écran lorsque le mode texte BBC le permet.

| MOS | ROM de langage | Résultat effectif |
| --- | --- | --- |
| 1.20 | BASIC II | Démarrage 16 Kio et `PRINT 12345` réussis. |
| 1.00 | BASIC II | Même contrôle réussi. |
| 0.92 | BASIC I | Même contrôle réussi. |
| 0.10 | BASIC I | Même contrôle réussi ; cela ne valide pas tous les timings CA2 de cette révision. |
| Norvégien | BASIC II | Même contrôle réussi ; variante nationale classée partielle. |
| Allemand | BASIC II | Même contrôle réussi, bannière `Acorn OS 16K` ; variante nationale classée partielle. |
| Américain | BASIC III américain | Pas de prompt obtenu dans cet essai ; PC D9D2, aucune sortie OSWRCH. La variante reste proposée comme partielle, sans annoncer un démarrage réussi ; le profil matériel américain n'est pas reproduit. |
| 1.20 | BASIC I | Démarrage et calcul réussis. |
| 1.20 | BASIC III américain | Démarrage et calcul réussis ; BASIC III reste classé partiel car il vise une variante différente. |
| Hope, moitié haute du fichier combiné | Hope BASIC, moitié basse | Démarrage et calcul réussis ; la bannière Dolphin comporte des caractères résiduels. Classé partiel. |
| 1.20 | VIEW A2.1 | Arrive à l'accueil VIEW, `No text`, `Editing No File`, `Screen Mode 7`. Il s'agit d'un traitement de texte, pas d'un interpréteur pour `PRINT`. Classé partiel. |

Ces essais caractérisent ce premier jalon, pas l'intégralité des appels MOS, de chaque langage ou des périphériques absents.

`Emulators/AcornEmu/Dictionaries/FirmwareDictionary.cs` réunit les 24 identités SHA-1 avec noms, versions, rôles et compatibilités. `Functions/FirmwareFunctions.cs` produit maintenant au moins une ligne pour chaque fichier : l'image Hope fournit deux choix, système et langage, soit 25 candidats pour les 24 fichiers du dossier. Les ROM de parole et les extensions disque/réseau/Torch/Cisco sont visibles comme incompatibles avec les deux emplacements actuellement proposés ; elles ne sont ni supprimées ni assimilées à un MOS ou à BASIC.

Les fichiers inconnus gardent le statut inconnu et peuvent être essayés si leur taille permet un chargement : 16 Kio directement, 8 Kio dans la moitié basse d'une banque de langage complétée à FF, 32 Kio comme concaténation langage puis MOS. Cette dernière disposition est identifiée pour Hope ; elle reste une hypothèse explicite lors du choix d'une image inconnue. Les en-têtes `(C)` des ROM latérales et les vecteurs de reset MOS peuvent suggérer un rôle sans garantir la compatibilité. Sans rôle identifié, les destinations possibles apparaissent séparément pour laisser le choix à l'utilisateur.

Le chargement n'exige plus une empreinte répertoriée. Il extrait uniquement en mémoire la partie choisie du fichier ; aucun firmware ni fichier extrait n'est écrit. L'emplacement de langage conserve son identifiant de configuration `acornemu.basic` et son libellé actuel BASIC ROM ; choisir VIEW y remplace donc BASIC. Les contrôles de capacité matérielle restent actifs pour les tailles qui ne peuvent pas être adressées.

Les 31 cas ciblés passent, dont le contrôle temporaire de visibilité et d'identification de tous les fichiers locaux. Les tests utilisant les ROM externes ont été supprimés après utilisation. Les vérifications permanentes sont autonomes et portent sur les en-têtes synthétiques, les choix inconnus et le découpage des formats matériels.
