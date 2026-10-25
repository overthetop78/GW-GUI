# BBC Micro — architecture commune utile à AcornEmu

Recherche du 25 octobre 2026. Cette fiche concerne d'abord les BBC Micro A/B. Les B+, Master et Compact devront être examinés séparément avant d'étendre un comportement à toute la gamme. L'équipement effectivement présent dans le Model A est décrit dans sa [fiche propre](Model%20A/model-a-reference.md).

## Sources techniques

| Référence | Nature et parties utiles |
| --- | --- |
| [Manuel de maintenance Acorn](https://acorn.huininga.nl/pub/unsorted/manuals/BBC%20Microcomputer%20Service%20Manual-HTML/BBCServiceManual.html) | Source constructeur ; §2–3 circuits, §4 extensions, §5 révisions ; [scan PDF](https://retroisle.com/others/acornbbc/OriginalDocs/BBC%20Model%20A%20and%20B%20Service%20manual.pdf). |
| [A Hardware Guide for the BBC Microcomputer](https://ftp.nvg.ntnu.no/pub/bbc/doc/A%20Hardware%20Guide%20for%20the%20BBC%20Microcomputer/bbc_hw_03.htm) | Description détaillée des circuits ; [chapitre des cavaliers](https://ftp.nvg.ntnu.no/pub/bbc/doc/A%20Hardware%20Guide%20for%20the%20BBC%20Microcomputer/bbc_hw_04.htm). Guide distinct du manuel de maintenance. |
| [BBC Microcomputer User Guide](https://www.bbcmicrobot.com/docs/BBC_User_Guide.pdf) | Manuel BBC de John Coll ; pagination imprimée : graphismes p.55+, son p.151+, appels MOS p.450+, carte mémoire p.500–502. |
| [The Advanced User Guide](https://basic.barrybks.co.uk/downloads/manuals/originals/BBCAdvancedUserGuide.pdf) | Référence de programmation ; chapitres matériel vidéo, VIA, interfaces et appels MOS. |
| [MOS Technology, MCS6500 Programming Manual, janvier 1976](https://lbaeza.neocities.org/mcs6500/) | Source du fabricant du CPU ; chapitres arithmétique, adressage, pile et interruptions. |
| [MOS Technology Data Catalog 1982](https://www.bitsavers.org/components/mosTechnology/_dataBooks/1982_MOS_Technology_Data_Catalog.pdf) | Fiche du VIA 6522, notamment registres et interruptions. |
| [Motorola MC6845](https://www.bitsavers.org/components/motorola/_dataSheets/6845.pdf) | Fiche constructeur du contrôleur d'affichage. |
| [Mullard SAA5050, juillet 1982](https://www.ecbazis.hu/pdf/saa5050.pdf) | Fiche constructeur du générateur de caractères télétexte. |
| [Texas Instruments SN76489AN](https://map.grauw.nl/resources/sound/texas_instruments_sn76489an.pdf) | Fiche constructeur du générateur sonore. |
| [Motorola MC6850](https://agelectronica.lat/pdfs/textos/M/MC68B50CP.PDF) | Fiche constructeur de l'ACIA série. |

Les transcriptions HTML peuvent contenir des erreurs d'OCR. Une divergence portant sur un numéro de composant, un bit ou une valeur de cavalier demande une vérification sur le scan et le schéma de la bonne révision de carte.

## CPU et bus

Le 6502 possède des registres A, X, Y, S, P et PC, un bus de données 8 bits et un espace d'adresses 16 bits. La page zéro occupe `0000–00FF`, la pile `0100–01FF`. Les vecteurs NMI, RESET et IRQ/BRK se trouvent respectivement en `FFFA/B`, `FFFC/D`, `FFFE/F`. L'arithmétique décimale est sélectionnée par le drapeau D ; les branches, franchissements de page, interruptions et opérations de pile ont leurs propres séquences de cycles. [MOS MCS6500 Programming Manual](https://lbaeza.neocities.org/mcs6500/).

**Conséquence pour l'émulation :** le temps doit avancer lors des accès du CPU, et pas seulement après chaque instruction. Une lecture d'E/S peut produire un effet matériel ; une instruction qui accède plusieurs fois à une adresse ne se réduit pas à son résultat final. Les opcodes non documentés, le comportement décimal NMOS et les séquences exactes de reset devront être confrontés à une référence NMOS et à des programmes de validation avant d'annoncer une compatibilité complète. Une fiche de 65C02 moderne ne suffit pas à définir ce CPU.

Le circuit BBC entrelace les accès RAM du CPU et de l'affichage. Les accès RAM/ROM sont normalement rapides ; certains périphériques imposent un cycle à 1 MHz. Le signal de reset général et celui qui distingue la mise sous tension ne sont pas identiques. [A Hardware Guide, chapitre 3](https://ftp.nvg.ntnu.no/pub/bbc/doc/A%20Hardware%20Guide%20for%20the%20BBC%20Microcomputer/bbc_hw_03.htm).

Le code [MAME bbc_m.cpp, set_cpu_clock](https://github.com/mamedev/mame/blob/84981cc15cfc1429e48f098d80c6dd97bc82a0bc/src/mame/acorn/bbc_m.cpp#L357) identifie comme accès lents `FCxx`, `FDxx`, puis dans `FExx` les blocs CRTC/ACIA/ULA série, VIA système/utilisateur et ADC. C'est une référence d'implémentation ; la phase d'alignement du bus doit aussi être reproduite.

## Organisation de la mémoire

| Zone hexadécimale | Attribution BBC A/B |
| --- | --- |
| `0000–3FFF` | RAM du Model A. |
| `4000–7FFF` | RAM supplémentaire du Model B ; comportement sur Model A à préciser dans sa fiche. |
| `8000–BFFF` | Fenêtre langage/ROM latérale, selon équipement et version de MOS. |
| `C000–FBFF` | ROM système. |
| `FC00–FCFF` | FRED, E/S d'extension. |
| `FD00–FDFF` | JIM, E/S d'extension. |
| `FE00–FEFF` | SHEILA, E/S internes. |
| `FF00–FFFF` | Fin de ROM système, points d'entrée et vecteurs. |

`PAGE` vaut normalement `0E00` dans une configuration cassette ; les extensions peuvent réserver davantage de mémoire. La mémoire écran est prise dans la RAM : `HIMEM` et la place disponible pour BASIC dépendent donc du mode choisi. [BBC User Guide, p.500–502](https://www.bbcmicrobot.com/docs/BBC_User_Guide.pdf).

### Décodage des E/S

Cette table donne les blocs de décodage. Une plage ne signifie pas que tous ses octets sont des registres distincts ou que le périphérique est monté.

| Bloc | Fonction | Model A standard |
| --- | --- | --- |
| `FE00–FE07` | CRTC : sélection/valeur, répétées. | Présent. |
| `FE08–FE0F` | ACIA : état/contrôle et données. | Présente pour la cassette. |
| `FE10–FE17` | ULA série. | Présente. |
| `FE18–FE1F` | Commandes associées à Econet selon équipement. | Extension. |
| `FE20–FE2F` | ULA vidéo ; contrôle et palette. | Présente. |
| `FE30–FE3F` | Sélection ROM. | Dépend du montage ; voir fiche Model A. |
| `FE40–FE5F` | VIA système, registres répétés. | Présent. |
| `FE60–FE7F` | VIA utilisateur. | Absent. |
| `FE80–FE9F` | Interface disque. | Absente. |
| `FEA0–FEBF` | Econet. | Absent hors variante équipée. |
| `FEC0–FEDF` | Conversion analogique. | Absente. |
| `FEE0–FEFF` | Tube. | Connecteur/interface externe non fournis d'origine. |

Référence logicielle : [MAME bbca_mem/bbcb_mem](https://github.com/mamedev/mame/blob/84981cc15cfc1429e48f098d80c6dd97bc82a0bc/src/mame/acorn/bbcb.cpp#L118). Le comportement d'une lecture non décodée et les miroirs exacts devront être confrontés au matériel ; une valeur de retour choisie par un autre émulateur n'est pas une preuve électrique.

## VIA système et liaison des périphériques

Le 6522 comporte deux ports de 8 bits, deux compteurs 16 bits, un registre à décalage et des lignes de contrôle. Ses registres sont les suivants :

| Décalage | Fonction |
| --- | --- |
| `0`, `1` | Port B, port A. |
| `2`, `3` | Directions DDRB, DDRA. |
| `4`, `5` | Compteur T1 bas/haut. |
| `6`, `7` | Verrou T1 bas/haut. |
| `8`, `9` | Compteur T2 bas/haut. |
| `A` | Registre à décalage. |
| `B`, `C` | Contrôles ACR et PCR. |
| `D`, `E` | Indicateurs IFR et autorisations IER. |
| `F` | Port A sans acquittement de handshake. |

IFR rassemble les événements CA2, CA1, registre à décalage, CB2, CB1, T2 et T1. Son bit 7 signale une interruption autorisée. Écrire un bit à 1 dans IFR efface l'événement correspondant ; dans IER, le bit 7 choisit activation ou désactivation des sources sélectionnées. Les acquittements à la lecture dépendent du registre et du mode PCR. [MOS Technology Data Catalog, section 6522](https://www.bitsavers.org/components/mosTechnology/_dataBooks/1982_MOS_Technology_Data_Catalog.pdf).

Sur le BBC, le port A du VIA système forme le bus lent clavier/son/parole ; le port B pilote un verrou adressable 74LS259. Celui-ci contrôle les validations des périphériques, deux lignes de calcul d'adresse écran et les voyants de verrouillage clavier. [A Hardware Guide, circuits VIA/clavier/son](https://ftp.nvg.ntnu.no/pub/bbc/doc/A%20Hardware%20Guide%20for%20the%20BBC%20Microcomputer/bbc_hw_03.htm).

**Conséquence :** modifier le port de données et modifier un signal de validation sont deux événements distincts. Un appel direct « produire une note » ou « lire une touche » court-circuiterait les registres que les logiciels manipulent.

## Affichage

### CRTC 6845

Le CRTC produit adresses et synchronisations ; il ne décode pas lui-même les pixels. Il possède un registre de sélection et 18 registres fonctionnels :

| Registres | Rôle |
| --- | --- |
| R0–R3 | Total horizontal, largeur affichée, position et largeur de synchronisation. |
| R4–R7 | Total vertical, ajustement, hauteur affichée et synchronisation verticale. |
| R8–R9 | Entrelacement et dernière ligne de raster d'un caractère. |
| R10–R11 | Forme et clignotement du curseur. |
| R12–R13 | Adresse de départ d'affichage. |
| R14–R15 | Adresse du curseur. |
| R16–R17 | Adresse capturée par le signal crayon optique. |

Plusieurs registres sont en écriture seule ; les largeurs de champ varient. La fiche Motorola distingue non-entrelacé, synchronisation entrelacée et vidéo entrelacée. [Fiche MC6845, tables 2–4](https://www.bitsavers.org/components/motorola/_dataSheets/6845.pdf). Les variantes 6845 ne sont pas toutes interchangeables : MAME sélectionne un HD6845S, ce qui reste un choix d'émulateur à comparer au composant monté.

### ULA vidéo et mémoire écran

L'ULA vidéo sélectionne la source télétexte ou bitmap, sérialise les pixels et applique la palette. `FE20` commande notamment le format ; `FE21` reçoit l'index de couleur logique et la couleur physique. Les composantes RGB du codage palette matériel sont inversées. Le MOS garde des copies des commandes en RAM et propose OSBYTE 154/155 pour ces modifications. [Advanced User Guide, vidéo et OSBYTE 154/155](https://basic.barrybks.co.uk/downloads/manuals/originals/BBCAdvancedUserGuide.pdf).

Les octets bitmap suivent l'adressage CRTC et les lignes de caractère : il ne s'agit pas d'une image linéaire rangée ligne par ligne. Le rebouclage de mémoire écran dépend des lignes de contrôle du verrou système. [A Hardware Guide, génération vidéo](https://ftp.nvg.ntnu.no/pub/bbc/doc/A%20Hardware%20Guide%20for%20the%20BBC%20Microcomputer/bbc_hw_03.htm).

### Télétexte SAA5050

Le SAA5050 est la variante anglaise de la famille ; les autres numéros correspondent à d'autres jeux de caractères. Il produit les signaux vidéo à partir de caractères et de commandes : couleurs, texte ou mosaïques, caractères arrondis, double hauteur, clignotement, fond et maintien des graphiques. Ces commandes ont un état qui évolue pendant la ligne ; l'affichage ne se réduit pas à une police Unicode. [Fiche Mullard SAA5050](https://www.ecbazis.hu/pdf/saa5050.pdf).

**Conséquence :** conserver les registres, compteurs et changements en cours d'image permettra de représenter les modes reprogrammés. « 50 Hz » et une résolution fixe ne suffisent pas à définir toute la vidéo. Le crayon optique du CRTC est une capacité du composant ; cela ne prouve pas la présence d'un périphérique physique sur un Model A standard.

## Son

Le SN76489AN dispose de trois générateurs de sons périodiques et d'un générateur de bruit. Les périodes utilisent 10 bits ; chaque canal possède une atténuation sur 4 bits, avec pas nominaux de 2 dB et une position muette. Les commandes choisissent un registre mémorisé ; une période complète demande deux transferts. Le bruit peut être périodique ou pseudo-aléatoire et sa cadence peut dépendre d'un canal tonal. [Fiche TI SN76489AN](https://map.grauw.nl/resources/sound/texas_instruments_sn76489an.pdf).

Les commandes BBC BASIC `SOUND` et `ENVELOPE` passent par le logiciel système : l'enveloppe BBC ne constitue pas un générateur ADSR supplémentaire dans le PSG. Le son doit donc suivre les écritures réellement envoyées au composant. [BBC User Guide, chapitres du son](https://www.bbcmicrobot.com/docs/BBC_User_Guide.pdf).

**Points à caractériser :** variante exacte du PSG, état initial, période zéro, rétroaction du bruit et courbe de sortie. Les différences entre variantes SN76489 doivent être vérifiées avant de réutiliser un composant sonore existant.

## Série et cassette

Le MC6850 expose deux adresses : état en lecture/contrôle en écriture, puis réception en lecture/émission en écriture. Il gère registre de données, sérialisation, sélection de longueur/parité/bits d'arrêt, diviseur d'horloge et interruptions. Le statut distingue donnée reçue, émission disponible, DCD, CTS, erreur de cadrage, dépassement de réception, parité et IRQ. Son reset logiciel est commandé dans le registre de contrôle. [Fiche Motorola MC6850](https://agelectronica.lat/pdfs/textos/M/MC68B50CP.PDF).

L'ULA série BBC fournit les horloges, la sélection cassette/série et la commande moteur. Sa présence pour la cassette n'implique pas que le connecteur et les circuits RS423 soient montés. [A Hardware Guide, interface série](https://ftp.nvg.ntnu.no/pub/bbc/doc/A%20Hardware%20Guide%20for%20the%20BBC%20Microcomputer/bbc_hw_03.htm).

**Conséquence :** la position du média, la porteuse, les transitions et le moteur font partie de l'état de la machine. Injecter directement un programme dans la RAM ne valide pas la lecture cassette matérielle.

## Clavier

Le clavier est interrogé par sa matrice, avec balayage et signal vers le VIA. Les touches BBC comprennent f0–f9, COPY, DELETE, les flèches, SHIFT LOCK, CAPS LOCK, SHIFT et CTRL ; BREAK possède une liaison de reset séparée. Les codes des touches hôtes ne sont pas les coordonnées de la matrice. [MAME, définition du clavier et BREAK](https://github.com/mamedev/mame/blob/84981cc15cfc1429e48f098d80c6dd97bc82a0bc/src/mame/acorn/bbc_kbd.cpp#L36).

**Conséquence :** distinguer la frappe physique, le texte collé et les commandes de reset. Le détail complet des coordonnées doit rester celui du clavier de la machine sélectionnée ; le clavier Master, par exemple, apporte des touches supplémentaires.

## ROM et appels système

Le MOS fournit des points d'entrée stables : OSRDCH `FFE0`, OSWRCH `FFEE`, OSWORD `FFF1`, OSBYTE `FFF4`, OSCLI `FFF7`. Les vecteurs modifiables en RAM permettent de rediriger des traitements ; OSWRCH passe par WRCHV en `020E`. [BBC User Guide, appels MOS](https://www.bbcmicrobot.com/docs/BBC_User_Guide.pdf).

**Conséquence :** ces adresses sont utiles au diagnostic. Elles ne remplacent pas l'émulation matérielle, car un programme peut accéder directement au CPU, à la RAM, aux périphériques et à la mémoire écran.

## Fichiers ROM et liaisons avec la machine

Les [téléchargements vérifiés et leurs empreintes](../acornemu-references.md#rom-disponibles-au-téléchargement) sont centralisés au niveau Acorn. Cette section explique les données chargées, indépendamment du nom du fichier qui les transporte.

| Fichier ou famille | Rôle | Liaison matérielle et logicielle |
| --- | --- | --- |
| `os01.rom`, `os12.rom` | Système MOS BBC A/B : démarrage, clavier, affichage, son, cassette, commandes et services. | Image de 16 Kio dans la fenêtre système `C000–FFFF` ; les accès `FC00–FEFF` sont dirigés vers les E/S plutôt que vers les octets ROM correspondants. |
| `basic1.rom`, `basic2.rom` | Langage BBC BASIC et assembleur 6502. | Code du langage dans `8000–BFFF`, selon les circuits de sélection réellement présents. Le CPU exécute ce code ; BASIC appelle les services du MOS. |
| ROM DFS | Système de fichiers disque. | ROM latérale liée aux services MOS et au contrôleur disque correspondant. L'image contient du logiciel, pas le contrôleur ni le contenu d'une disquette. |
| ROM NFS/DNFS | Services réseau, ou services disque/réseau réunis suivant la version. | Exigent l'équipement et la configuration associés ; ce ne sont pas des remplacements universels de MOS. |
| `bpos.rom` | Système B+. | Décodage, mémoire et logiciel propres au B+ ; ne remplace pas automatiquement le MOS A/B. |
| `mos320.rom` | Ensemble de 128 Kio pour le Master. | Plusieurs parties système/latérales à distribuer selon la carte Master. Charger tout le fichier dans une fenêtre de 16 Kio serait incorrect. |
| `os51.rom` | Partie système Compact. | À associer aux ROM de langage et de fichiers de ce modèle. |

Les rôles et associations sont à recouper avec les [profils B-Em](https://github.com/stardot/b-em/blob/a76c3655140a9fc3c70ecef25660932e0890b1e5/b-em.cfg), ses [fichiers ROM](https://github.com/stardot/b-em/tree/a76c3655140a9fc3c70ecef25660932e0890b1e5/roms) et la carte mémoire décrite plus haut. Les tailles des images listées au niveau Acorn ont été mesurées sur les téléchargements.

### Chemin d'exécution

1. La configuration désigne les images affectées aux rôles système, langage et éventuelles extensions.
2. Le chargement identifie leurs tailles, variantes et empreintes, puis place leurs octets dans les banques correspondant au matériel.
3. Le CPU reçoit RESET et lit le vecteur dans la ROM système. La ROM est exécutée comme du code 6502 ; le cœur ne doit pas reproduire seulement son texte d'accueil.
4. Les instructions du MOS et du langage passent par le bus : RAM, ROM ou registres des composants suivant l'adresse et le sens de l'accès.
5. Les composants produisent ensuite interruptions, images, sons et échanges cassette/disque. Les services du module les transmettent à l'application par les interfaces existantes.

Il s'agit du fonctionnement à représenter dans le cœur ; les liaisons GW GUI déjà présentes sont détaillées dans la [fiche Model A](Model%20A/model-a-reference.md#raccordement-aux-fichiers-déjà-présents-dans-gw-gui).

### Points de chargement à conserver séparés

- L'image ROM est une donnée immutable pendant l'exécution normale ; la banque sélectionnée est un état de la machine.
- Une ROM de 8 Kio, une ROM de 16 Kio et un ensemble de plusieurs ROM ne se chargent pas avec une règle unique de troncature ou de remplissage : le câblage et le format déterminent leur présentation.
- Un firmware intégré à un cœur externe, un fichier choisi par l'utilisateur et une source assembleur commentée ont des rôles différents. Les liens de téléchargement ne créent pas une dépendance réseau au démarrage de l'émulateur.
- L'état sauvegardé doit permettre de retrouver les images utilisées et la pagination ; le simple nom `os.rom` ne permet pas d'identifier leur contenu.

Ces points découlent de la distinction entre les fichiers vérifiés, le décodage de la carte et les mécanismes existants de sélection de firmware. Ils n'ajoutent aucun mécanisme de téléchargement au logiciel.

## Référence complémentaire : b2-libretro

Révision consultée : `9793f6a8040c416d147003cada29363b955b54e7`, dans le [dépôt du port](https://github.com/zoltanvb/b2-libretro/tree/9793f6a8040c416d147003cada29363b955b54e7). Cette étude porte sur le code ; aucun binaire n'a été exécuté.

### Machine et composants

| Sources du port | Apport à l'étude du BBC Micro |
| --- | --- |
| `src/beeb/include/beeb/BBCMicro.h` | Objet machine, mémoire, clavier, interfaces et progression de l'émulation. |
| `src/beeb/src/type.cpp` | Description des types et tables de pagination ; le type B alloue 32 768 octets de RAM. |
| `src/beeb/src/6502.cpp` | Implémentation CPU à examiner avec les cycles du bus BBC. |
| `src/beeb/src/6522.cpp` | VIA ; distinguer le composant de son câblage système/utilisateur. |
| `src/beeb/src/VideoULA.cpp`, `video.cpp` | ULA et traitement vidéo. |
| `src/beeb/src/sound.cpp` | Traitement du générateur sonore. |

Références : [BBCMicro.h](https://github.com/zoltanvb/b2-libretro/blob/9793f6a8040c416d147003cada29363b955b54e7/src/beeb/include/beeb/BBCMicro.h), [type.cpp](https://github.com/zoltanvb/b2-libretro/blob/9793f6a8040c416d147003cada29363b955b54e7/src/beeb/src/type.cpp#L937) et [sources des composants](https://github.com/zoltanvb/b2-libretro/tree/9793f6a8040c416d147003cada29363b955b54e7/src/beeb/src).

### Adaptation libretro et limites constatées

`create_core` construit `BBCMicro` avec le type choisi, l'interface disque, le coprocesseur et la mémoire de configuration, puis installe les ROM système et latérales. Cela permet d'identifier les paramètres de machine séparément des composants. [core.cpp, create_core](https://github.com/zoltanvb/b2-libretro/blob/9793f6a8040c416d147003cada29363b955b54e7/src/libretro/core.cpp#L275).

| Fonction | Constat sur le port consulté |
| --- | --- |
| Médias annoncés | `retro_get_system_info` déclare `ssd\|dsd`. |
| Cassette | Les commentaires de travail indiquent encore son ajout à faire ; ne pas déduire son fonctionnement de la présence d'une variable nommée `sound_tape`. |
| Sauvegardes d'état | `retro_serialize` et `retro_unserialize` renvoient `false` ; une taille de snapshot déclarée ne prouve pas un support fonctionnel. |
| Sortie temporelle | Le frontal reçoit 50 images/s et un échantillonnage de 250 000 Hz. |

Sources : [core.cpp](https://github.com/zoltanvb/b2-libretro/blob/9793f6a8040c416d147003cada29363b955b54e7/src/libretro/core.cpp#L64), [sérialisation](https://github.com/zoltanvb/b2-libretro/blob/9793f6a8040c416d147003cada29363b955b54e7/src/libretro/core.cpp#L1250), [constantes audio](https://github.com/zoltanvb/b2-libretro/blob/9793f6a8040c416d147003cada29363b955b54e7/src/libretro/core.h).

La documentation décrit aussi le changement de disque, l'amorçage automatique et l'association des commandes RetroPad au clavier. Ses dimensions vidéo annoncées sont 768 × 288, jusqu'à 768 × 576 en entrelacé. Ce sont les caractéristiques du rendu de ce port, pas de nouveaux modes matériels du BBC. [Documentation officielle Libretro](https://docs.libretro.com/library/b2/).

**Conclusion de recherche :** b2 est pertinent pour comprendre le matériel partagé A/B. Les limites de son adaptation libretro, notamment cassette et états, ne définissent pas les capacités à conserver dans AcornEmu.

## Questions encore ouvertes au niveau BBC Micro

- Comportements exacts des variantes CRTC, VIA et PSG présentes selon les cartes.
- Effets des lectures de zones non équipées et état du bus, à distinguer des valeurs conventionnelles des émulateurs.
- Fréquences et timings des modes reprogrammés, des demi-lignes et de l'entrelacement.
- État au démarrage des compteurs, verrous et RAM, et différences entre mise sous tension, BREAK et CTRL-BREAK.
- Chronologie complète du protocole cassette et de ses blocs de fichiers ; la spécification UEF décrit un conteneur, pas à elle seule le système de fichiers du MOS.

Ces points sont identifiés comme recherches complémentaires, pas présentés comme des caractéristiques déjà vérifiées sur une machine physique.
