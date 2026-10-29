# BBC Micro Model A — dossier technique pour AcornEmu

## Son mono AcornEmu — 25 octobre 2026

Le Model A possède un PSG TI SN76489AN, trois tons carrés et une voie de bruit, mélangés en mono. Les quatre voies sont internes au générateur ; elles ne constituent pas quatre sorties physiques. Le cœur produit un seul échantillon PCM signé de seize bits par frame audio, à 44 100 frames par seconde. Le contrat `AudioChunk` porte explicitement `EmulationAudioChannelLayout.Mono` ; les relais Common et la sortie existante conservent la responsabilité du volume, de la coupure, de la latence et de l'adaptation au périphérique hôte. App et Emulation ne sont pas modifiés pour cette implémentation.

### Fréquences et programmation

L'ULA fournit 4 000 000 Hz au circuit. Ses compteurs de tons avancent après division par seize et changent de niveau à chaque expiration ; une oscillation complète prend deux expirations. La fréquence vaut donc `4 000 000 / (32 × N)` Hz, avec un diviseur entier de dix bits. Sur cette variante TI, zéro désigne une période de 1024. Exemples : N=250 produit 500 Hz ; N=284 produit environ 440,140845 Hz. Il n'est pas possible d'obtenir exactement toute fréquence souhaitée avec ces diviseurs. Le passage en PCM à 44 100 Hz conserve la fréquence réalisable, sans transposer la note.

Les volumes disposent de quinze niveaux espacés nominalement de 2 dB et d'une position muette (15). Le bruit propose trois cadences fixes et une cadence liée au troisième ton ; il peut être périodique ou pseudo-aléatoire. Les paramètres du registre à décalage suivent la variante SN76489A utilisée pour BBC dans MAME : rétroaction depuis les bits 2 et 3, initialisation 0x10000 ; les étages de sortie donnent une séquence périodique utile de quinze pas. Cette distinction évite d'employer les paramètres du PSG Sega.

Le port A du VIA transporte les données et le port B programme le verrou IC32. Sa sortie 0 active le PSG à l'état bas. La carte transmet les données lors de cette activation et lorsqu'elles changent pendant la validation. Les écritures avec validation haute ne modifient pas le PSG. Le MOS pilote ces registres pour réaliser `SOUND` et `ENVELOPE` : aucune interprétation parallèle de commandes BASIC n'est ajoutée.

### Fichiers et échanges

Chemins relatifs à `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/` :

| Fichier | Rôle, propriété et liaison |
| --- | --- |
| `Constants/SoundConstants.cs` | Division interne, période zéro, état du bruit et amplitudes logarithmiques. Ne contient aucune fréquence de note imposée ni préférence de l'hôte. |
| `Services/Sn76489.cs` | Registres sélectionnés par les octets du bus, compteurs des trois tons, compteur et rétroaction du bruit, somme instantanée unipolaire. Ignore le BBC, le VIA, les fenêtres et les sorties physiques. |
| `Machines/BbcMicro/Common/Constants/AudioConstants.cs` | Horloge sonore 4 MHz, horloge CPU réutilisée depuis les constantes BBC, débit PCM 44 100 Hz et constante RC du couplage continu. |
| `Machines/BbcMicro/Common/Services/Audio.cs` | Possède le PSG et les échantillons en cours ; reçoit un cycle CPU, avance deux périodes PSG, intègre le signal sur chaque intervalle PCM et conserve la phase entière entre les prélèvements. Le filtre de couplage 10 kΩ × 4,7 µF retire le piédestal continu du mélange. `DrainSamples` transfère une copie indépendante et conserve l'intégrale partielle. Aucun thread ni périphérique audio n'est créé. |
| `Machines/BbcMicro/ModelA/Services/ModelAMachine.cs` | Possède le composant audio BBC, relie le VIA et IC32 au PSG, avance le son depuis le même Tick que le CPU et la vidéo, réinitialise le composant au reset matériel. |
| `Interfaces/IAcornMachine.cs` | `DrainAudio` remet les échantillons mono de la carte au cœur, sans exposer ses composants au runtime. |
| `Services/EmulatorCore.cs` | Crée des blocs mono avec séquence et horodatage fondé sur le total réel d'échantillons. La file est consommée par Common ; les resets, Stop et Dispose détachent les blocs en attente. Dispose détache aussi la carte dans son finally. |

### Vérifications et limites

Correction du raccordement physique : la fabrique AcornEmu transmettait encore null au paramètre audioOutputFactory de la session Common. Les échantillons étaient produits mais aucune sortie hôte ne pouvait être créée. Elle transmet désormais context.AudioOutputFactory comme les autres adaptateurs. Le nouveau contrôle passe par cette fabrique, vérifie la réception des échantillons mono par une sortie mémoire et sa libération par la session. Les huit contrôles audio passent après correction.


Les 85 contrôles Acorn passent, dont les nouveaux contrôles audio autonomes : mesure des passages par zéro pour 500 Hz, 440,140845 Hz et 122,0703125 Hz à ±0,06 Hz, exactement 44 100 échantillons en une seconde malgré des prélèvements non alignés, silence des voies coupées, indépendance des trois tons, atténuation de 2 dB, reproductibilité et distinction des deux bruits, période du bruit périodique, raccordement par le VIA, disposition mono, horodatages et purge des blocs. Les ROM, données et sorties des tests sont synthétiques ; aucun firmware utilisateur ni périphérique externe n'est requis.

L'intégration PCM est une moyenne temporelle à la cadence CPU, pas une reproduction complète de l'amplificateur et du haut-parleur analogiques. Le délai exact de capture du bus, la ligne READY du PSG et les limites électriques des écritures trop rapides ne sont pas encore simulés. La synthèse vocale optionnelle, les entrées audio externes et la cassette ne sont pas ajoutées par ce travail. Aucun sélecteur « original/transcodé » n'est nécessaire pour ce générateur analogique.

Sources consultées : [Acorn Hardware Guide, §3.17](https://acorn.huininga.nl/pub/mirrors/ftp.nvg.org/pub/bbc/doc/A%20Hardware%20Guide%20for%20the%20BBC%20Microcomputer/bbc_hw_03.htm), [fiche TI SN76489AN](https://map.grauw.nl/resources/sound/texas_instruments_sn76489an.pdf), [MAME, SN76496 et variantes](https://github.com/mamedev/mame/blob/master/src/devices/sound/sn76496.cpp), [MAME, configuration BBC](https://github.com/mamedev/mame/blob/master/src/mame/acorn/bbcb.cpp), [jsbeeb, soundchip.js](https://github.com/mattgodbolt/jsbeeb/blob/master/src/soundchip.js) et [via.js](https://github.com/mattgodbolt/jsbeeb/blob/master/src/via.js), notamment couplage continu et bus lent. jsbeeb emploie une représentation du bruit sur quinze bits ; les paramètres ci-dessus suivent la représentation avec étages de sortie de MAME.

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
| Son et cassette | PSG mono émulé et transmis par Common (voir section audio). Cassette UEF/CSW/WAV/RAW lue et enregistrée par ACIA/ULA (voir section stockage). |
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


## Inventaire cassette local — 25 octobre 2026

Dossier consulté en lecture seule : F:\Retro\ef_tosec_backup_2024. Les répertoires Acorn BBC contiennent 95 fichiers UEF et 2 WAV : Ascend (2022-05-30)(Inufuto)(JP).wav et Ruptus (2022-06-02)(Inufuto)(JP).wav. Le total est 97 images, et non 97 UEF. Les UEF contiennent 9 770 chunks 0100 (données implicites), 10 274 chunks 0110 (porteuse), 494 chunks 0112 (silence), 93 chunks 0000 (origine) et 2 chunks 0005 (machine cible). Les sources UEF sont brutes ou gzip ; les lecteurs existants traitent ces deux emballages. Aucun fichier utilisateur n'est modifié.

La collection contient aussi des UEF et WAV Electron et Atom, ainsi que 14 CSW et des TAP Atom. Ces formats ou emballages ne prouvent pas la compatibilité avec le BBC Model A : le protocole, la machine et la RAM requise restent distincts du type de fichier. Les images SSD, DSD, ADL et les programmes BIN ne sont pas des images cassette à monter dans ce lecteur.

## Lecteur cassette AcornEmu — 26 octobre 2026

La prise cassette appartient au Model A ; le magnétophone externe se branche explicitement par Ajouter dans Stockage. Aucun lecteur externe n'est configuré par défaut. Le branchement `acornemu.cassette-connected` persiste même à vide. Retirer le lecteur supprime ce branchement et ses médias. Les images acceptées en lecture et enregistrement sont UEF, CSW, WAV et RAW ; leur conteneur ne garantit ni le protocole BBC, ni la compatibilité du programme avec les 16/32 Kio installés.

### Réglages et utilisation

Les réglages du périphérique Cassette sont décrits par `EmulationCassetteSettings` et présentés dans le dialogue de configuration du stockage existant.

| Réglage | Fonction |
| --- | --- |
| Débit automatique, 300 ou 1 200 bauds | Automatique suit le registre ACIA programmé par le MOS. Une valeur explicite impose le débit série ; elle reste indépendante de la cadence PCM. |
| Lecture réelle | Le CPU et la bande avancent à leur cadence matérielle, avec les délais configurés et les bruits mécaniques activables. |
| Lecture accélérée | Le matériel continue de décoder les bits ; le cœur peut exécuter jusqu'à 32 images matérielles par présentation pendant l'activité cassette. |
| Chargement direct | Les LOAD/CHAIN standard passant par OSFILE utilisent les blocs validés pour charger la RAM. Un chargeur personnalisé poursuit le parcours matériel accéléré. |
| Vitesse de bande | 25 à 400 %, 100 % par défaut ; distincte du choix de débit et du mode de chargement. |
| Multiplicateur d'avance/retour | 1 à 100, 20 par défaut ; déplacement temporisé en mode réel et immédiat dans les deux autres modes. |
| Latence de lecture / écriture | Délais de démarrage distincts, 0 à 10 000 ms, zéro par défaut. |
| Bruits mécaniques | Moteur, avance, retour, arrêt et éjection synthétisés seulement en mode réel. |
| Protection en écriture | Bloque l'enregistrement indépendamment de l'attribut de lecture seule du fichier. |

Après insertion, Play est prêt mais le MOS doit aussi commander le moteur. `*TAPE` choisit le système cassette ; `*CAT` lit le catalogue ; `CHAIN ""` charge et exécute le BASIC ; `*RUN` convient aux programmes machine prévus pour cette commande. Après Stop ou un déplacement, réactiver Play. FastForward et Rewind rejoignent les repères de blocs ; le repère précède la porteuse afin de laisser le récepteur se synchroniser. Les extrémités de bande restent accessibles lorsqu'aucun bloc suivant/précédent n'existe.

REC ouvre le choix du chemin et du format. Une image absente est créée vide puis montée ; sélectionner une image existante conserve son contenu. Si le chemin et le profil RAW sont inchangés, la position courante est conservée. Changer le profil d'un RAW déjà monté le recharge depuis le début avec sa nouvelle interprétation. Après REC, `SAVE "NOM"` commande le moteur et transmet les données par l'ACIA. Le signal écrit remplace la zone parcourue et conserve les zones avant/après ; il prolonge la bande si nécessaire. La coupure moteur après SAVE, Stop, éjection et extinction déclenchent la sauvegarde des changements. Les écrivains utilisent la publication atomique commune ; une erreur de sauvegarde n'est pas traitée comme un succès.

Le voyant réutilise `FromLedStates`, `MediaActivity` et `FramePresented` existants. Le transport expose son opération, son activité de lecture/enregistrement et les déplacements ; App conserve son affichage commun. Le moteur arrêté, Pause et la fin de bande ne sont pas signalés comme une lecture active. Le voyant représente le lecteur virtuel, sans ajouter de LED matérielle à la carte BBC.

### RAW : emplacement des paramètres

Le défaut est **48 000 Hz, mono, PCM signé 16 bits little-endian**. Un fichier RAW n'a pas d'en-tête ; ces valeurs décrivent son interprétation numérique, et non une fréquence native de la cassette analogique.

- Lecture : configuration du lecteur Cassette, appliquée au prochain montage lorsque l'image ne porte pas déjà son propre profil.
- Enregistrement : après le choix RAW dans REC, dialogue du profil de l'image. Le profil est conservé avec le média monté et réutilisé lors des sauvegardes.
- Conversion : dialogues RAW d'entrée et de sortie dans le parcours de conversion existant ; les paramètres d'entrée restent distincts de ceux de sortie.

Les choix proposés sont 22 050, 44 100, 48 000, 96 000 et 192 000 Hz ; mono/stéréo ; 8/16/24/32 bits ; signé/non signé ; little/big-endian. Le runtime fournit au lecteur un signal mono 16 bits ; les documents et conversions PCM conservent leur précision native et leurs canaux. Une conversion RAW 24 bits vers WAV 24 bits puis RAW au même profil conserve notamment les bits de poids faible. Aucun rééchantillonnage n'est imposé lorsque le profil cible reste identique.

### Parcours et responsabilités des fichiers

Les chemins suivants sont relatifs à `src/`. Dans les lignes Acorn, le préfixe est `GWGUI.Emulation.Acorn/`.

| Fichier ou groupe | Rôle et liaisons |
| --- | --- |
| `GWGUI.MediaEngine/Contracts/MediaCassetteSignal.cs` | Signal mono, cadence, repères et blocs validés avec métadonnées. Aucune adresse CPU exécutée dans MediaEngine. |
| `GWGUI.MediaEngine/Functions/CassetteSignalFunctions.cs` | Lecture des quatre conteneurs, rendu du signal UEF, écriture atomique par les écrivains existants/enregistrés. Les données avant/après enregistrement sont fournies par le magnétophone. |
| `GWGUI.MediaEngine/Contracts/CassettePcmProfile.cs`, `Functions/CassettePcmFunctions.cs` | Validation du profil RAW, normalisation du codage, conservation des canaux/précision, conversion demandée de cadence et d'encodage. |
| `GWGUI.MediaEngine/Images/Formats/Tape/Csw/CswReader.cs`, `CswWriter.cs` | CSW 1/2 en lecture, RLE/Z-RLE ; écriture CSW 2 Z-RLE des durées de passages par zéro. |
| `GWGUI.MediaEngine/Images/Formats/Tape/Raw/RawTapeReader.cs`, `RawTapeWriter.cs` | Signal sans en-tête avec profil explicite. La reconnaissance automatique ne remplace pas un disque RAW déjà reconnu. |
| `GWGUI.MediaEngine/Images/Formats/Tape/Uef/UefReader.cs`, `UefWriter.cs`, `Tape/Wav/WavTapeReader.cs`, `WavTapeWriter.cs` | Lecteurs/écrivains de conteneurs existants réutilisés ; images vides valides pour REC. |
| `GWGUI.MediaEngine/Functions/FskSerialFunctions.cs` et `Images/Reading/Decoding/Sequential/Acorn/AcornTapeDecoder.cs` | Trames et positions avec cadences fractionnaires ; reconnaissance des blocs BBC à 300/1 200 bauds, y compris mélangés sur une bande. CRC conservés dans le résultat. |
| `GWGUI.MediaEngine/Functions/CassetteBlockFunctions.cs` | Réutilise les décodeurs existants pour la présentation des blocs et leurs repères. Le système de fichiers cassette existant fournit les entrées de l'explorateur. |
| `GWGUI.MediaEngine/Images/Conversion/Sequential/CassetteSignalRepresentationConverter.cs` | Conversion du signal entre UEF et PCM sans exiger des fichiers reconnus ; WAV/CSW/RAW utilisent ensuite les écrivains PCM. |
| `GWGUI.MediaEngine/Images/Conversion/Sequential/SequentialMediaConversionService.cs` et `Images/Writing/Encoding/Sequential/Acorn/AcornTapeEncoder.cs` | Conversion des blocs/fichiers reconnus par les protocoles existants, avec porteuses et silences nécessaires au chargement matériel. |
| `GWGUI.MediaEngine/Functions/MechanicalSoundFunctions.cs` | Synthèse des bruits de transport ; aucune fenêtre, aucun thread audio ni fichier sonore externe détenu. |
| `GWGUI.MediaEngine/Interfaces/IMediaRuntimePreparation.cs`, `Services/MediaRuntimePreparationService.cs` | Accès commun à la lecture/écriture cassette, aux formats et aux sons. |
| `GWGUI.Emulation/Contracts/EmulationCassetteSignal.cs`, `EmulationCassetteSettings.cs`, `EmulationCassettePcmSettings.cs` | Contrats et descriptions de réglages génériques pour les émulateurs internes. |
| `GWGUI.Emulation/Interfaces/IEmulationMediaProcessing.cs`, `Services/EmulationMediaProcessingService.cs` | Relais vers MediaEngine ; aucun type MediaEngine exposé au module. |
| `Common/Machines/BbcMicro/Functions/CassetteFunctions.cs` | Passage du BBC Common par le service Emulation, avec protocole `acorn-bbc`. |
| `Common/Interfaces/IEmulatorCassetteTransport.cs`, `Common/Services/Machine.Cassette.cs` | Commandes transport par la file du cœur, état et activité vers les interfaces déjà utilisées par App. |
| `Emulators/AcornEmu/Services/Acia6850.cs` | Réception et émission série temporisées, registre de contrôle, parité, bits d'arrêt, dépassement et IRQ. |
| `Emulators/AcornEmu/Machines/BbcMicro/Common/Constants/CassetteConstants.cs`, `Services/Cassette.cs` | Horloges BBC, moteur, modulation/démodulation, position, latences, transport, zone enregistrée et protection. État en mémoire, aucun fichier ouvert conservé. |
| `Emulators/AcornEmu/Machines/BbcMicro/Common/Functions/CassetteFunctions.cs` | Chargement direct MOS partagé BBC : validation des blocs et de la destination RAM avant copie et consommation de bande. |
| `Emulators/AcornEmu/Machines/BbcMicro/ModelA/Services/ModelAMachine.cs` | Câblage ACIA FE08–FE0F, ULA FE10–FE17, IRQ, horloge CPU et appel du chargement direct partagé. |
| `Emulators/AcornEmu/Services/EmulatorCore.cs` | Montage, paramètres par image, modes de lecture, mélange des bruits, sauvegarde et actualisation des repères après écriture. |
| `Emulators/AcornEmu/Factories/AcornEmuMachineFactory.cs` | Lecteur optionnel, formats et champs de réglage, stockage de configuration et relais des voyants communs. |
| `GWGUI.App/Controllers/Emulation/Storage/EmulationStorageSettingsController.cs` | Ouvre le dialogue existant pour les paramètres d'un lecteur cassette. |
| `GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs` | Choix REC du format et profil RAW, création/montage, commandes de transport existantes. Annulation avant modification du média. |
| `GWGUI.App/Controllers/MainWindow/ConversionTabController.cs`, `Services/Conversion/ConversionBatchExecutor.cs` | Sélection et transmission des paramètres RAW d'entrée/sortie pour les conversions. |

Le module accède uniquement à Emulation. MediaEngine prépare et écrit les conteneurs, tandis qu'AcornEmu émule le matériel BBC. Les flux de lecture/écriture appartiennent aux lecteurs et écrivains et sont libérés par leur portée ; les images utilisateur restent intactes pendant les essais synthétiques.

### Signal, formats et limites précises

Les tons cassette BBC sont 1 200/2 400 Hz. L'horloge ACIA 19 200 Hz et ses diviseurs donnent notamment 300/1 200 bauds ; ce débit est distinct des 48 000 échantillons/seconde du signal UEF et des 44 100 échantillons/seconde de la sortie audio du PSG. Les fractions de cadence sont conservées à 22,05 et 44,1 kHz. Le bit 7 de l'ULA commande le moteur et le bit 6 choisit RS423. Le démodulateur produit les données et la détection de porteuse ; en lecture matérielle, le MOS interprète ensuite les blocs et leurs CRC.

UEF lit les données implicites 0100, explicites 0102, définies 0104, Atom 0105, porteuses 0110/0111, silences 0112/0116, fréquence 0113, cycles de sécurité 0114, phase 0115 et débit 0117. Le chunk Atom n'affirme pas la compatibilité d'un programme Atom avec le BBC. Les autres chunks de signal non interprétés restent explicitement refusés ; les données multiplexées 0101 et toutes les protections propriétaires ne sont pas annoncées comme prises en charge. La formule historique du gap entier est `1/(2 × n × fréquence de base)`.

L'enregistrement UEF utilise des cycles 0114, changements de fréquence et silences : il conserve la chronologie/polarité du signal numérique, sans supposer des données BBC 8N1. Il reconstruit le signal, pas les métadonnées originales du conteneur. CSW conserve les impulsions et leur cadence, sans amplitude PCM ; un silence sans transition devient un niveau maintenu. WAV et RAW conservent les échantillons. La conversion vers un format d'impulsions annonce la perte de l'amplitude. Les bruits mécaniques ne sont jamais inscrits dans l'image.

Le chargement direct exige des blocs complets, CRC valides, nom/numéros cohérents et une destination RAM valide ; il laisse la bande inchangée si ces conditions échouent. Il ne remplace pas les chargeurs propriétaires ni les protections ; ces derniers utilisent le matériel émulé. Les bruits sont une synthèse fonctionnelle du transport et non un enregistrement d'un modèle commercial de magnétophone.

### Validation du 26 octobre 2026

Les 35 contrôles ciblés cassette ont réussi : lecture/écriture des quatre conteneurs, images vides, profils RAW, conservation des bits PCM 24 bits, exploration des fichiers BBC, conversions de blocs et de signaux sans fichiers reconnus, bandes mêlant 300/1 200 bauds, trames UEF explicites/définies, enregistrement matériel, protection, latences et chargement direct synthétique. Les tests temporaires et leurs fichiers ont ensuite été supprimés ; les 77 contrôles AcornEmu conservés et les 13 contrôles existants de compatibilité séquentielle passent après recompilation.

`scripts/local-building.cmd --building=debug --modules=acorn` a terminé avec succès. L'exécutable est `F:\GW GUI\build\Debug\GW GUI\gwgui.exe`, avec Acorn demandé et Example fourni automatiquement par le script. Cette validation synthétique ne remplace pas l'essai utilisateur d'un programme réel. Aucun commit cassette n'a été créé.

### Références des formats et du matériel

- [Spécification UEF, copie conservée par PlayUEF](https://github.com/8bitkick/PlayUEF/blob/master/docs/UEFspecification.html) : chunks, trames, phases et cadences.
- [Spécification CSW](https://k1.spdns.de/Develop/Projects/zxsp/Info/File%20Formats/CSW%20technical%20specifications.html) et [copie Acorn](https://acorn.huininga.nl/pub/unsorted/software/pc/CSW/csw.html) : versions, durées d'impulsions, RLE/Z-RLE.
- [BBC Micro Service Manual](https://acorn.huininga.nl/pub/unsorted/manuals/BBC%20Microcomputer%20Service%20Manual-HTML/BBCServiceManual.html) et [BBC User Guide](https://www.bbcmicrobot.com/docs/BBC_User_Guide.pdf) : circuit cassette, contrôle moteur, commandes et débits.
- [jsbeeb acia.js](https://github.com/mattgodbolt/jsbeeb/blob/master/src/acia.js), [serial.js](https://github.com/mattgodbolt/jsbeeb/blob/master/src/serial.js), [tapes.js](https://github.com/mattgodbolt/jsbeeb/blob/master/src/tapes.js) : comparaison du matériel et du parcours UEF.

## Disquettes, Winchester et réseau — travail ouvert le 26 octobre 2026

Le Model A de base ne possède ni contrôleur disque, ni lecteur externe, ni Econet. Ces équipements sont des extensions matérielles à configurer explicitement, distinctes du simple montage d'une image. La RAM, les connecteurs et les ROM latérales nécessaires doivent rester des choix visibles de la configuration équipée ; le Model A standard doit conserver son démarrage cassette.

| Équipement | Matériel et logiciel associés | État constaté avant ce travail |
| --- | --- | --- |
| Disquettes DFS | Contrôleur Intel 8271, ou carte WD1770 distincte, ROM DFS adaptée ; lecteurs 40/80 pistes et une/deux faces. | Lecteurs SSD/DSD et exploration DFS présents dans MediaEngine/MediaFileSystems. AcornEmu ne câble encore aucun contrôleur disque ni ROM DFS. |
| Disquettes ADFS | Contrôleur double densité et ROM ADFS adaptés au montage ; les formats ADFS et DFS restent distincts. | FileCore/ADFS existe, mais le lecteur de fichiers est restreint à l'image 800 Kio/1 024 octets par bloc. Les anciens répertoires BBC et les disques Winchester doivent être traités aussi. |
| Winchester | Interface hôte Acorn sur le bus 1 MHz, contrôleur SCSI/SASI et ROM ADFS ; unités et protection configurées séparément. | Le dialogue HDD, les conteneurs courants et la vue CHS existent ; le couple BBC `.dat`/`.dsc` n'est pas enregistré dans MediaEngine. |
| Econet | Réseau Acorn avec contrôleur et ROM réseau ; les fichiers viennent d'un serveur réseau, pas d'une image disque montée comme lecteur local. | Absent d'AcornEmu. Les registres et le protocole ne sont pas remplacés par un faux lecteur HDD. |

`GWGUI.App/Views/Dialogs/Emulation/Storage/HardDiskDriveConfigurationDialog.cs` est le dialogue HDD existant à réutiliser. `EmulationStorageDeviceSettings` transporte déjà les paramètres disquette et le choix d'interface HDD. `BlockMediaView.xaml.cs` et `SkiaHardDiskGeometryRenderer.cs` présentent déjà les surfaces correspondant aux têtes ; deux surfaces peuvent appartenir au même plateau physique, distinction qui dépend de la géométrie déclarée. `HardDiskGeometry` porte cylindres, têtes, secteurs par piste et taille de secteur. La géométrie physique ne doit pas être inventée à partir de la seule capacité d'une image.

Les fichiers Winchester utilisés par b2, B-em et BeebEm associent un `.dat` aux secteurs et un `.dsc` au descripteur. Le descripteur SCSI conserve notamment les cylindres aux octets 13–14 (ordre big-endian), les têtes à l'octet 15 ; le montage étudié utilise 33 secteurs de 256 octets par piste. Ces données doivent alimenter les surfaces et le parcours des pistes. Une description absente ou invalide doit rester distinguée d'une géométrie connue.

Sources : [b2, images SCSI et configuration](https://github.com/tom-seddon/b2/blob/master/doc/Overview.md#scsi-hard-disks), [B-em, contrôleurs et formats pris en charge](https://github.com/stardot/b-em), [B-em, lecture du descripteur et interface SCSI](https://github.com/stardot/b-em/blob/master/src/scsi.c), [Acorn Winchester Disc Filing System User Guide](https://github.com/bitshifters/bbc-documents/blob/master/Hard%20Disk/Acorn_WinchesterDiscFilingSystemUG.pdf). Ces références décrivent les extensions ; leur disponibilité dans un autre émulateur ne constitue pas une implémentation dans AcornEmu.

### Images et visualisation raccordées le 26 octobre

`GWGUI.MediaEngine/Images/Formats/HardDisk/Acorn/AcornWinchesterFormat.cs` et `AcornWinchesterReader.cs` lisent les secteurs DAT de 256 octets et la géométrie DSC, enregistrés par `MediaRecognitionComposition`. Les octets déclarés mais absents du DAT restent indisponibles et produisent un diagnostic ; une description manquante ne crée aucune géométrie fictive. `AcornWinchesterWriter.cs`, enregistré dans `MediaWritingComposition`, réutilise `AtomicMediaFileSetWriter` pour publier DAT et DSC ensemble avec restauration sur erreur. Les champs inconnus du DSC sont conservés. Une écriture exige une géométrie et des données complètes.

Les fichiers ADFS BBC S/M/L utilisent respectivement 40×1, 80×1 et 80×2 pistes de 16 secteurs de 256 octets. `AcornAdfGeometry.cs`, `AdfReader.cs` et `AcornAdfWriter.cs` prennent en charge leurs 160/320/640 Kio, ADS/ADM/ADL et ADF, dans le parcours ADF existant. Les faces sont rangées par cylindre puis face, conformément aux définitions `diskdefs_acorn.cfg` déjà présentes. Ces définitions alimentent déjà les décodeurs/encodeurs MFM et la sélection de format pour les captures ; elles ne sont pas dupliquées. Une IMG d'une taille identique n'est pas assimilée à ADFS par cette seule taille. La conversion conserve la géométrie et n'annonce pas un agrandissement S→M/L comme une simple copie.

Dans `GWGUI.MediaFileSystems/FileSystems/Acorn`, le lecteur `Adfs/AcornAdfsFileSystemReader.cs` réutilise ses résolveurs FileCore et reconnaît désormais les répertoires BBC de 47 entrées/1 280 octets à partir du secteur 2. Les attributs BBC viennent des bits hauts du nom ; le dernier octet d'une entrée est son numéro de séquence. `AcornAdfsDirectoryReader.cs` et `AcornAdfsFileReader.cs` utilisent la taille réelle des secteurs. `AcornAdfsFileSystemReader.Blocks.cs` donne au même lecteur un accès secteur borné aux volumes HDD, sans charger le disque entier. Les répertoires FileCore de 77 entrées/2 048 octets conservent leur parcours existant.

`BlockMediaVisualizationProvider.cs` décrit les pistes de chaque surface CHS en LBA et vérifie leur disponibilité par des lectures de 64 Kio au maximum. `MediaVisualizationController.cs` parcourt toutes les surfaces au lieu de la seule surface 0, puis affiche les résultats ; la présence d'une plage dans le catalogue ne suffit plus à afficher une lecture réussie. Les erreurs de données manquantes restent visibles et l'annulation est propagée. `BlockMediaInspectorPresenter.cs` conserve les limites des pistes ; `SkiaHardDiskGeometryRenderer.cs` dessine leurs anneaux par surface, et `BlockMediaView.xaml.cs` relie la piste sélectionnée à son LBA, avec zoom et déplacement. Les surfaces sont connues par les têtes ; le nombre de plateaux physiques reste inconnu quand le descripteur ne l'indique pas.

Validation synthétique : aller-retour DAT/DSC, champs du descripteur conservés, remplacement atomique de la source, géométrie absente/troncature distinguées ; aller-retour des trois formats ADFS BBC, exploration de fichiers et sous-répertoires, adressage des faces et absence de reconnaissance ADFS d'une IMG arbitraire. Les tests utilisant ces fichiers ont été supprimés avec leurs images. Les 22 contrôles conservés de stockage Acorn, conversion et visualisation réussissent. **Les contrôleurs disquette/Winchester et Econet ne sont pas encore raccordés à AcornEmu à ce stade.**

Le build intermédiaire Debug Acorn a réussi le 26 octobre après ces changements : `F:\GW GUI\build\Debug\GW GUI\gwgui.exe`, module Acorn et MediaEngine dans `lib` présents. Il permet les essais de formats, exploration et visualisation ; il n'annonce pas encore de nouveaux lecteurs opérationnels dans AcornEmu.

### Accès aux secteurs depuis les émulateurs internes

`GWGUI.MediaEngine/Interfaces/IMediaRuntimeDisk.cs` et `Services/MediaRuntimeDisk.cs` réutilisent les représentations secteurs/blocs et leurs écrivains. `MediaDiskSector` conserve adresse physique, numéro logique, données et diagnostics. Les lectures HDD restent bornées à un secteur ; les changements s'accumulent en mémoire, puis `FlushAsync` publie l'image par son écrivain existant. Les données non modifiées et les champs DSC sont préservés. La protection en écriture reste effective. Une publication refusée conserve les changements pour réessayer et laisse la source intacte.

`GWGUI.Emulation/Interfaces/IEmulationDiskImage.cs`, `Contracts/EmulationDiskSector.cs` et `Services/EmulationMediaProcessingService.Disks.cs` exposent ces opérations par `IEmulationMediaProcessing.OpenDiskAsync`. Le module passe par `Common/Machines/BbcMicro/Functions/DiskFunctions.cs` ; il reçoit uniquement les types SDK. Le cœur propriétaire doit sérialiser les appels, sauvegarder, détacher l'image et la libérer dans finally ; la libération effectue une dernière sauvegarde si nécessaire. Aucun flux n'est conservé ouvert par cet accès sectoriel. Une géométrie inconnue reste à zéro et les coordonnées physiques d'un secteur HDD inconnu restent à -1.

Quatre essais temporaires ont vérifié les secteurs DFS, ADFS et Winchester, leur sauvegarde/réouverture, les secteurs inchangés, la protection et la reprise après verrouillage de la destination. Ces essais et leurs images ont été supprimés ; 77 contrôles AcornEmu conservés passent après recompilation. Le relais commun est disponible ; son utilisation par les contrôleurs d'AcornEmu reste l'étape suivante.

La broche NMI du CPU est désormais décrite par `Interfaces/ICpuBus.cs` et mémorisée cycle par cycle dans `Services/Cpu6502.Interrupts.cs`. Une activation n'interrompt qu'une fois tant que la ligne reste active ; NMI passe avant IRQ et utilise le vecteur FFFA/FFFB avec le retour et l'état empilés, puis RTI rétablit l'exécution. Deux contrôles synthétiques supplémentaires vérifient priorité, pile, sept cycles d'entrée et réarmement sur un nouveau front ; les 79 contrôles AcornEmu passent. La carte Model A laisse encore NMI inactive en attendant le câblage du contrôleur.

Repères pour le contrôleur Intel 8271 : statut/commande au registre 0, résultat/paramètres au registre 1, reset au registre 2, données au registre 4 ; statut BUSY 80, résultat 10, interruption 08, demande non DMA 04. L'interruption de données est acquittée par l'accès au registre 4, celle du résultat par le registre 1. Les lecteurs 0/1 sont sélectionnés par les bits 6/7 de la commande ; le registre spécial de sortie 23 porte notamment le choix de face. La puce est séparée du décodage FE80 propre à la carte BBC. Référence étudiée : [B-em i8271.c](https://github.com/stardot/b-em/blob/master/src/i8271.c), notamment statuts, commandes et acquittements ; les flux conteneurs restent gérés par le relais SDK décrit ci-dessus.

### Raccordement DFS/8271 vérifié le 26 octobre 2026

`Services/Intel8271.cs`, `.Commands.cs` et `.Transfers.cs` reproduisent les registres, la lecture/écriture multisécteur, les identifiants, la vérification, le positionnement et le formatage des secteurs existants. Le transfert non DMA demande un acquittement par octet, avec erreur en cas de retard. Le délai mécanique dépend du réglage du lecteur ; le débit des octets reste celui du circuit. Les commandes scan et écriture de secteurs supprimés, ainsi que la création d'une topologie de piste différente lors du formatage, restent absentes.

`Machines/BbcMicro/Common/Services/Disks.cs` câble le contrôleur sur FE80–FE9F et NMI. `SidewaysRoms.cs` fournit BASIC en banque 15 et la ROM DFS externe en banque 14, sélectionnées par ROMSEL FE30–FE3F ; les autres banques sont vides. `ModelAMachine.cs` partage leur horloge avec le CPU. Le montage est optionnel : sans lecteur configuré, la carte disque reste absente.

`Constants/StorageConstants.cs` et `Functions/StorageFunctions.cs` décrivent deux lecteurs 5,25 pouces SS/DS, 40/80 pistes, leurs vitesses et protection en écriture via les dialogues existants. `AcornEmuMachineFactory.cs` conserve ces réglages, expose la ROM DFS requise par l'extension et relaie les deux voyants par les états communs. `EmulatorCore.Disks.cs` possède les images SDK : montage contrôlé, sauvegarde à l'arrêt moteur et à l'extinction, éjection et libération. `Common/Machines/BbcMicro/Functions/DiskFunctions.cs` utilise exclusivement le SDK ; les écritures sont publiées dans le fichier image monté.

La création d'image vierge est raccordée au modèle par `FloppyDriveModelChoice.BlankFormats`, dictionnaire des extensions et identifiants du catalogue. `FloppyDriveConfigurationDialog.cs` propose ces formats puis utilise `CreateBlankDiskAsync` du relais SDK/MediaEngine. `Images/Creation/BlankSectorImageFactory.cs` prépare la géométrie et les deux secteurs de catalogue de chaque face DFS ; l'écrivain SSD/DSD existant publie le résultat atomiquement. Les quatre essais temporaires de création/réouverture par le SDK ont réussi, y compris la conservation de la destination après rejet d'un format inconnu ; leurs fichiers et sources ont été supprimés.

L'exploration DSD conserve les catalogues indépendants des lecteurs logiques 0 et 2 sous deux dossiers dans le volume. `MediaFileSystems/FileSystems/Acorn/BbcDfs/BbcDfsFileSystemReader.Sides.cs` projette les pistes entrelacées sur le même lecteur DFS, sans dupliquer son décodage de fichiers. Treize contrôles autonomes de stockage réussissent, dont les quatre catalogues vierges et la relecture de fichiers de même nom avec des contenus distincts sur les deux faces, au-delà de la première piste.

La reconnaissance des ROM de service suit les octets de type et de copyright de l'en-tête. Une ROM de service inconnue reste inconnue ; elle peut être proposée pour le logement de l'extension sans être présentée comme une DFS reconnue. Les DFS connues sont partiellement compatibles avec le Model A puisqu'elles exigent cette extension, indépendamment de l'émulateur choisi.

Validation : les 93 contrôles AcornEmu autonomes passent. Trois essais temporaires avec MOS 1.20 et BASIC II ont exécuté `*DISC`, `SAVE`, `NEW`, `LOAD`, `LIST`, `RUN` et `*CAT` : DFS 0.9H en 16 et 32 Kio, puis DNFS 1.20 en 32 Kio. Le programme relu s'exécute et les secteurs modifiés sont publiés dans l'image SSD temporaire. Les ROM du dossier utilisateur ont seulement été lues ; les essais et leurs images ont été supprimés, puis les 93 contrôles conservés reconstruits et exécutés. WD1770/ADFS, Winchester et Econet restent à raccorder ; ce résultat ne valide pas encore ces extensions.

Build intermédiaire du 26 octobre : `scripts/local-building.cmd --building=debug --modules=acorn` réussi. `build/Debug/GW GUI/gwgui.exe`, le module Acorn et les bibliothèques SDK/MediaEngine dans `lib` sont présents. Ce build permet les essais utilisateur cassette/DFS ; Example est inclus automatiquement par le script. Aucun commit créé.

### Carte Acorn WD1770 et disquettes ADFS

Le choix de carte dans Stockage est `Intel 8271` ou `Acorn WD1770`, enregistré par `acornemu.floppy.controller`. Son changement demande le cycle d'arrêt/redémarrage de la machine. Les lecteurs restent optionnels. Les libellés d'interface réutilisent les traductions communes ; les noms des circuits et `DFS / ADFS ROM` sont invariants. La ROM système disque reste externe : une ROM DFS pour 8271 ne remplace pas une ROM destinée au WD1770 ; ADFS 1.30 fournit le système ADFS utilisé par les essais.

`Constants/Wd1770Constants.cs` et `Services/Wd1770.cs`, `.Commands.cs`, `.Transfers.cs` séparent états/cadences, registres, commandes et transferts. Le composant gère positionnement, secteurs simples/multiples, identifiants, pistes régulières, DRQ, IRQ et erreurs de protection, secteur absent, CRC et octet non servi. Le formatage des pistes utilise les identifiants déjà présents dans l'image sectorielle ; il ne crée pas une topologie protégée différente. Les marques de données supprimées ne sont pas conservées dans les images sectorielles exposées par ce relais.

`Machines/BbcMicro/Common/Services/Disks.cs` réalise le câblage Acorn : verrou de contrôle à FE80, registres WD1770 à FE84–FE87 et NMI issue de DRQ/IRQ. Le verrou sélectionne lecteur, face, densité et reset. `ModelAMachine.cs` l'avance avec les autres composants ; `EmulatorCore.Disks.cs` utilise les mêmes protections, sauvegardes et voyants que le 8271. Les images ADS/ADM/ADL et les ADF BBC sont proposées avec cette carte ; les ADF Archimedes à secteurs de 1 024 octets sont distinguées des disquettes BBC prises en charge.

`StorageFunctions.cs` complète les formats vierges selon les capacités du lecteur : S sur 40/80 pistes, M sur 80 pistes et L sur un lecteur double face de 80 pistes. `MediaFileSystems/FileSystems/Acorn/Adfs/AcornAdfsFormatter.cs` produit les cartes libres des secteurs 0/1, leurs sommes de contrôle et le répertoire racine Hugo des secteurs 2 à 6 ; MediaEngine les place dans sa géométrie et utilise son écrivain existant. `AcornFileCoreOldMap.cs` convertit, pour ADFS L, les adresses logiques regroupées par face vers les pistes physiques entrelacées du conteneur. Le Winchester conserve son adressage linéaire.

Les onze contrôles WD1770 autonomes passent, ainsi que les 104 contrôles AcornEmu ; les dix-huit contrôles de stockage couvrent aussi les cartes vides et les fichiers franchissant une piste ou une face ADFS L. Trois essais temporaires ont utilisé MOS 1.20, BASIC II et ADFS 1.30 en 32 Kio, avec les images S/M/L créées par le SDK : `*ADFS`, `*MOUNT 0`, `SAVE`, `NEW`, `LOAD`, `LIST`, `RUN` et `*CAT` réussissent, puis les modifications sont publiées. La ROM ADFS a été lue depuis la source publique, conservée uniquement en mémoire ; les images et le test temporaire ont été supprimés. Winchester et Econet restent ouverts.

Le build Debug suivant, lancé avec `scripts/local-building.cmd --building=debug --modules=acorn`, a réussi après cette validation et la reconstruction des 104 tests conservés. L'exécutable, le module Acorn et les bibliothèques SDK, MediaEngine et MediaFileSystems dans `lib` ont été vérifiés. Ce build ajoute WD1770 et ADFS aux essais utilisateur ; aucun commit créé.

Références : [câblage et protocole WD1770 de B-em](https://github.com/stardot/b-em/blob/master/src/wd1770.c), [ROM externe ADFS 1.30](https://github.com/stardot/b-em/blob/master/roms/general/adfs130.rom), [Guide To Disc Formats, cartes anciennes et ordre des faces ADFS L](https://www.geraldholdsworth.co.uk/documents/DiscImage.pdf), pages 12–13 et 21–23.

### Interface Winchester — repères de raccordement

L'interface Acorn commence à FC40 : données à +0, statut à +1, sélection à +2, commande IRQ à +3. Le statut expose C/D en bit 7, I/O en 6, REQ en 5, IRQ en 4, BSY en 1 et MSG en 0. La transaction traverse sélection, commande, transfert de données, résultat puis message et bus libre. Les commandes ADFS observées comprennent disponibilité, état d'erreur, formatage, lecture/écriture à commande de six octets, paramètres de géométrie et mise en marche/arrêt. Quatre unités sont distinguées ; les commandes six octets portent le LUN dans les trois bits hauts du deuxième octet et une adresse de bloc sur 21 bits. Le transfert BBC utilise des blocs de 256 octets, indépendamment du conteneur hôte. Source technique : [B-em, interface SCSI Acorn](https://github.com/stardot/b-em/blob/master/src/scsi.c).

Les points existants à employer sont `GWGUI.Emulation/HardDisks/HardDiskImageFormat.cs`, `HardDiskImageCreation.cs`, `HardDiskImageValidation.cs`, `EmulationMediaDevice.HardDiskFormats` et `EmulationStorageDeviceSettings.InterfaceId`, avec `HardDiskDriveConfigurationDialog.cs` dans App. Le profil BIOS `DiskChsGeometry` ne porte que têtes/secteurs et raisonne en secteurs de 512 octets ; il ne suffit pas à représenter le descripteur Winchester 256 octets. La création doit donc transmettre la géométrie déclarée au service média, puis réutiliser `AcornWinchesterWriter` pour publier DAT et DSC ensemble. `AcornAdfsFormatter.CreateHeader` peut préparer le système de fichiers natif ; le relais sectoriel existant fournit lecture, écritures et publication des modifications. À cette étape, le contrôleur Winchester n'est pas encore raccordé.

### Contrôleur Winchester — protocole et paramètres validés

`Constants/AcornScsiConstants.cs` définit registres et résultats. `Services/AcornScsi.cs` gère sélection, phases, REQ, IRQ et horloge ; `.Commands.cs` décode les commandes et `.Transfers.cs` accède aux secteurs SDK. Le bus effectue des transferts de 256 octets, y compris lorsque le conteneur a des secteurs de 512 octets : une écriture partielle conserve leur autre moitié. Lecture/écriture six et dix octets, disponibilité, état d'erreur, identification, capacité, paramètres, mise en marche/arrêt, synchronisation et formatage bas niveau sont séparés des conteneurs. La temporisation est configurable par unité et le contrôle de plage précède le transfert.

`Machines/BbcMicro/Common/Services/Winchester.cs` câble les quatre unités empruntées et leur activité. Le bus de `ModelAMachine.cs` décode FC40–FC43, avance le contrôleur et transmet IRQ au CPU. Une carte non installée répond comme le bus absent. Les images ne sont ni sauvegardées ni libérées par le contrôleur ; ces opérations restent au propriétaire du cœur. Le catalogue active la carte lorsqu'une unité Winchester est configurée.

`Functions/StorageFunctions.HardDisks.cs` décrit quatre unités SCSI facultatives, leurs protections, la redirection et les latences distinctes de lecture et d'écriture. Les vitesses proposées sont 100/200/400/800 % ou sans délai. Le dialogue HDD existant présente ces champs et les conserve avec la configuration. `Dictionaries/HardDiskFormats.cs` fournit le profil DAT/DSC, en secteurs de 256 octets et 33 secteurs par piste. La création utilise une géométrie explicite et propose une image vide ou préparée en ADFS ; elle refuse toute destination DAT ou DSC déjà présente. Les dimensions ne sont pas déduites arbitrairement d'un fichier inconnu.

`Services/EmulatorCore.HardDisks.cs` possède les images, les monte et les détache, publie les changements à l'arrêt de l'activité et à l'extinction, puis les libère. La redirection utilise une copie privée stable du couple DAT/DSC. Les indices d'activité 8 à 11 sont projetés vers les quatre emplacements HDD par la fabrique, avec le voyant commun. La ROM ADFS reste externe et devient requise avec cette extension, même sans lecteur disquette.

La sonde ADFS lit les données pendant la sélection : cet accès libère maintenant le bus, faute de quoi le démarrage gardait BSY actif. Un contrôle autonome couvre la sonde et la transaction suivante. La propriété calculée `EmulationMediaSlot.ProtocolValue` est exclue du JSON : la catégorie et l'indice suffisent à restaurer les quatre unités, y compris celles que l'ancien protocole numérique ne représente pas.

Pour reproduire la configuration vérifiée : sélectionner 32 Kio, fournir MOS 1.20, BASIC II, la ROM de caractères et ADFS 1.30 externes ; ajouter HDD 0 dans Stockage, choisir SCSI et créer un DAT préparé en Acorn ADFS. Les autres unités sont facultatives. Après démarrage, `*ADFS`, `*MOUNT 0` et `*CAT` accèdent au premier volume. Les champs de géométrie décrivent le DAT/DSC, et non un nombre de plateaux physiques certain : le visualisateur affiche les surfaces/têtes connues, sans inventer leur assemblage en plateaux.

Après suppression des cinq essais temporaires et de leurs images, les 124 contrôles AcornEmu conservés réussissent. Aucun répertoire temporaire Winchester ne subsiste.

Le build intermédiaire Debug Acorn a réussi avec ces unités Winchester : `F:\GW GUI\build\Debug\GW GUI\gwgui.exe`, `Modules/acorn/gwgui.emulation.acorn.dll` et le SDK dans `lib` ont été vérifiés après la fin du script. Example est ajouté par le script existant. Ce build permet les essais utilisateur des disquettes et du Winchester ; Econet reste absent.

### Econet — matériel et échanges à raccorder

Le réseau est une extension facultative. Le 68B54 convertit les données série et demande les transferts par NMI. FE18 lit le numéro de station et masque la NMI Econet ; lire FE20 la réactive, tandis qu'écrire FE20 garde sa fonction vidéo. Les interruptions du contrôleur disquette restent indépendantes de ce masque. L'absence d'horloge réseau est une condition matérielle détectable, distincte de l'absence d'un serveur. Références : [Acorn, manuel de maintenance, section Econet](https://chrisacorns.computinghistory.org.uk/docs/Acorn/Manuals/Acorn_BBCSMOct85_Sec2.pdf), [Acorn, guide de conception Econet et schéma BBC](https://acorn.huininga.nl/pub/scans/Acorn/Econet%20design%20and%20installation%20guide/0472%2C209%20Econet%20design%20and%20installation%20guide.pdf).

Le MC6854 occupe FEA0–FEBF, avec quatre adresses répétées. En lecture, +0 et +1 donnent SR1/SR2 ; +2/+3 retirent les données reçues. En écriture, +0 est CR1. Son bit AC sélectionne CR2/CR3 à +1 et donnée finale/CR4 à +3 ; +2 alimente l'émission. Les deux FIFO ont trois octets. Les statuts distinguent données, adresse initiale, fin de trame valide, débordement, abandon, erreur FCS, absence d'horloge, disponibilité d'émission et interruption. CR2 acquitte les statuts et peut prioriser ceux de réception. Référence d'implémentation étudiée : [BeebEm, Econet.cpp](https://github.com/stardot/beebem-windows/blob/master/Src/Econet.cpp) ; le câblage BBC est dans [BeebMem.cpp](https://github.com/stardot/beebem-windows/blob/master/Src/BeebMem.cpp).

Une trame Econet commence par station/réseau destination puis station/réseau source. Un transfert usuel comporte scout, accusé de scout, données et accusé final. La ROM réseau gère ces échanges sur le BBC ; le serveur distant fournit ses fichiers. Le transport hôte et le serveur ne remplacent donc pas le contrôleur par un lecteur local. Référence : [Acorn, PRM Econet, paquets et trames](https://www.riscos.com/support/developers/prm/econet.html).

AUN transporte les services Econet sur UDP avec accusés, délais et retransmissions adaptés à IP. Son échange ne correspond pas directement aux quatre trames matérielles : un raccordement AUN doit effectuer cette adaptation et conserver identifiants, erreurs et échéances. Un transport de trames UDP de type BeebEm et AUN sont des modes distincts. Le BBC garde sa ROM externe, son numéro de station et ses registres dans les deux cas. Référence : [Acorn, PRM AUN, protocoles](https://www.riscos.com/support/developers/prm/aun.html). Les deux transports sont désormais raccordés ; leur validation est détaillée ci-dessous.

Le composant `Services/Mc6854.cs`, `.Receive.cs` et `.Transmit.cs` sépare registres/statuts, réception et émission. `Constants/Mc6854Constants.cs` regroupe les bits. Les octets traversent des FIFO de trois entrées à la cadence émulée ; les données remises au transport sont des trames délimitées sans FCS. Une trame reçue porte explicitement sa validité FCS. Le mot série peut comporter 5 à 8 bits ; les trames BBC usuelles utilisent huit bits. La puce ne possède ni socket ni ROM.

`Machines/BbcMicro/Common/Services/Econet.cs` porte station et validation NMI. `ModelAMachine.cs` additionne cette NMI à celle du contrôleur disque et avance l'ADLC avec l'horloge CPU. Les registres FEA0–FEBF sont répétés tous les quatre octets ; FE18–FE1F masque le réseau, la lecture FE20–FE27 le valide et les écritures vidéo gardent leur fonction. Les méthodes de `IAcornMachine` transmettent trames, callback d'émission et état de ligne sans exposer le transport au CPU. Le catalogue active désormais la carte selon les options ; elle reste absente par défaut. Son transport est raccordé par les callbacks du cœur.

Douze contrôles autonomes passent : cadence, copies des trames reçues, FIFO, marqueurs d'adresse/fin, FCS invalide, perte d'horloge, acquittements, priorités, fin et manque de données à l'émission, station et porte NMI, répétition des adresses et absence de carte. Les 136 contrôles AcornEmu conservés passent après ce câblage ; cela ne valide pas encore un échange avec une ROM ou un serveur réseau.

`Constants/NetworkConstants.cs`, `Contracts/EconetSettings.cs` et `Functions/NetworkFunctions.cs` déclarent activation, station, horloge, mode, adresse locale, port UDP et stations distantes. Défauts : carte absente, station 2, horloge 250 kHz, mode déconnecté, adresse 127.0.0.1, port 32768, aucune station distante. Les cadences proposées sont 125/250/500 kHz ; l'ADLC consomme respectivement 128/64/32 cycles CPU par octet. La table des destinations utilise `réseau.station=IPv4:port`, avec entrées séparées par un point-virgule, par exemple `0.254=127.0.0.1:32769`. Les doublons et adresses invalides sont refusés avant ouverture d'une socket. Les modes disponibles sont déconnecté, trames UDP et AUN. Une destination AUN doit avoir un point IPv4/port unique dans la table, car son en-tête ne porte pas le numéro de station source.

`AcornEmuMachineFactory.cs` expose ces champs dans l'onglet réseau existant ; leurs changements nécessitent l'extinction/rallumage de la machine. La ROM `acornemu.nfs` est requise avec l'extension. `FirmwareFunctions.OptionRoms` la place en banque 13, indépendamment de DFS/ADFS en 14 et BASIC en 15. Common normalise les banques NFS de 8 Kio dans une banque de 16 Kio complétée par FF. Les huit champs sont traduits par Argos dans toutes les langues du module ; les noms techniques et l'exemple de table restent dans la base commune. Le titre générique de l'onglet est traduit en « Réseau » dans App.

ROM externes identifiées, sans intégration au code :

| ROM | Taille | SHA-1 | Téléchargement |
|---|---:|---|---|
| Acorn NFS 3.34 | 8192 | `8564d944fedd17caf2023454ba5a7c3947aedc1f` | [NFS-3.34.rom](https://rk.nvg.ntnu.no/bbc/rom/Acorn/fs/NFS-3.34.rom) |
| Acorn NFS 3.35K | 8192 | `7081f1d834645e501c8c9862cf4a50f282ca1f32` | [NFS-3.35K.rom](https://rk.nvg.ntnu.no/bbc/rom/Acorn/fs/NFS-3.35K.rom) |

Les identités sont dans le dictionnaire Common ; leur compatibilité Model A reste partielle car elles nécessitent cette extension. Le fichier utilisateur `eco_3.35k.rom`, SHA-1 `7462ced7b83d74b822815bc00ed40a89f84e0276`, correspond à l'identité Torch NET déjà connue et n'est pas confondu avec ces ROM Acorn. Les contrôles conservés couvrent paramètres, emplacement NFS et normalisation des banques ; les échanges matériels sont décrits ci-dessous.

### Econet — transport, durée de vie et validation

`Services/EconetTransport.cs` possède une socket IPv4 UDP non bloquante et une file bornée de trames. Le BBC l'appelle tous les 128 cycles CPU, y compris dans les boucles d'attente NFS : le transport n'attend pas la prochaine image vidéo. Il n'ouvre aucun thread. En mode UDP/BeebEm, chaque datagramme est une trame Econet sans FCS. Les destinations sont celles de la table configurée ; une source inconnue est ignorée. La diffusion est envoyée une fois par point d'accès. Le maintien des drapeaux entre les trames évite de présenter une attente réseau comme une ligne immédiatement inactive.

`Services/EconetAun.cs` conserve les étapes scout, données et accusés. L'en-tête AUN comporte type, port, contrôle, compteur de retransmission et identifiant little-endian. La classe synthétise l'accusé de scout côté BBC et n'acquitte les données qu'après l'accusé AUN correspondant. En réception, elle attend l'accusé final du BBC avant d'envoyer l'accusé positif AUN. Les messages immédiats, réponses, scouts longs et diffusions gardent leurs données ; les paquets tronqués sont refusés. Les échéances sont de 250 ms émulées, avec trois retransmissions maximales ; les doublons terminés réutilisent leur réponse pendant deux secondes. Un refus AUN ne devient jamais un succès BBC. L'identifiant et le correspondant identifient l'accusé ; port et contrôle peuvent être nuls, comme dans [l'implémentation AUN du noyau Linux](https://kernel.googlesource.com/pub/scm/linux/kernel/git/pjw/omap-devel/+/f187e9fd68577cdd5f914659b6f7f11124e40485/net/econet/af_econet.c).

Le câblage BBC distingue le niveau CTS, le verrou NMI et les événements RX acquittés. Le repos reste lisible sans redemander continuellement la même NMI ; une nouvelle activité réarme les événements. CLR RX acquitte les statuts observés par les lectures de registres. RTS maintient l'émetteur jusqu'à la fin de la trame engagée ; le couper dès l'écriture du dernier octet tronquait les scouts NFS. Ces comportements ont été corrigés à partir des accès de NFS 3.34, puis couverts par des contrôles autonomes. Référence de la ROM : [désassemblage annoté NFS 3.34](https://acornaeology.uk/acorn-nfs/3.34.html), notamment INACTIVE, préparation TX et opérations immédiates.

`Services/EmulatorCore.Network.cs` relie le transport à `IAcornMachine`, sans donner de socket au CPU ou au contrôleur. Stop détache les callbacks et ferme la socket dans finally ; Dispose le fait également après une erreur de sauvegarde ou de démarrage. Le constructeur du transport libère la socket en cas d'échec du bind. Reset remet les transactions en attente à zéro. Le changement de configuration se fait par extinction/rallumage de la machine, sans fermer GW GUI.

Quatre scénarios temporaires ont réussi : aller-retour UDP brut et réutilisation du port après fermeture ; AUN bidirectionnel, accusés et suppression des doublons ; refus d'un port occupé puis libération effective à l'extinction ; appels réseau exécutés par NFS 3.34 avec MOS 1.20 et BASIC II externes. Ce dernier scénario transmet `44 33 22 11` sur le port Econet 99 par OSWORD 16, attend le résultat par OSBYTE 50, puis émet la demande de type distant 88 et reçoit `01 00 34 03` dans la RAM prévue. Le correspondant est une socket locale de test qui accuse les données et répond au type distant ; cela ne constitue pas un serveur de fichiers complet. Les sockets sont fermées et les ROM/fichiers temporaires supprimés après les essais ; aucune ROM n'est intégrée au code.

Pour utiliser un serveur existant : installer l'extension dans Réseau, choisir un numéro de station, sélectionner UDP/BeebEm ou AUN selon le serveur, définir adresse/port locaux et la table des correspondants, puis fournir la ROM NFS externe. Les numéros et ports doivent être distincts entre machines du même réseau. `*NET` sélectionne le système réseau. Le serveur reste un pair configuré ; AcornEmu n'en fabrique pas un à partir d'une image HDD.

### Bilan du stockage et des extensions — build de contrôle

| Demande | Réalisation et preuve |
|---|---|
| Configurer les lecteurs | Magnétophone, deux lecteurs disquette optionnels, quatre unités Winchester optionnelles ; dialogues existants et configurations persistantes. Les tests des fabriques vérifient ajout/retrait, protections, redirection, vitesses, latences, ROM requises et conservation des options. |
| Lire et écrire les images | UEF/CSW/WAV/RAW pour cassette ; SSD/DSD et ADFS S/M/L pour disquette ; DAT/DSC pour Winchester. Les scénarios avec ROM ont sauvegardé, rechargé et exécuté les programmes ; les écrivains conservent les secteurs non modifiés et publient les fichiers atomiquement. |
| Visualiser le HDD | Chaque surface/tête de la géométrie connue possède son disque visible et sa sélection ; la sélection est reliée au LBA. Un nombre de plateaux physiques n'est pas inventé à partir d'une géométrie logique. |
| Scanner les pistes | Le descripteur HDD parcourt tous les couples cylindre/tête. Chaque plage est effectivement lue avec un tampon borné ; les données absentes restent indisponibles et l'annulation est propagée. Le test Winchester contrôle huit pistes sur quatre surfaces et leur sélection. |
| Explorer les fichiers | Les lecteurs DFS et ADFS existants parcourent fichiers et sous-répertoires, y compris les deux faces DFS et les secteurs BBC de 256 octets sur HDD. Les tests vérifient les contenus et les adresses ADFS L franchissant pistes et faces. Les parcours cassette restent ceux des services médias communs. |
| Voyants | Les activités des contrôleurs pendant lecture et écriture sont transmises aux emplacements des deux disquettes et des quatre HDD par les états communs ; le magnétophone réutilise également le relais commun. |
| Réseau | Extension Econet configurée dans l'onglet Réseau existant, ROM NFS externe, station et horloge, UDP/BeebEm et AUN. Les essais avec NFS 3.34 valident une transmission réelle et la réception d'une réponse distante par les registres et la NMI. |
| Réutilisation et architecture | Les dialogues et parcours de conversion, visualisation et exploration sont étendus. Acorn accède aux images via Common et le SDK ; MediaEngine porte les conteneurs et MediaFileSystems les fichiers. Le transport réseau est interne à AcornEmu et n'ajoute aucune dépendance de paquet. |
| Traductions et durée de vie | Nouveaux champs traduits par Argos dans toutes les langues, invariants dans la base. Les scénarios vérifient sauvegarde/protection, fermeture des sockets après erreur et extinction, puis réutilisation du port. Les tests et données externes temporaires ont été supprimés. |

Après nettoyage, **164 contrôles AcornEmu conservés** et **18 contrôles de stockage Acorn** réussissent. Les quatre essais réseau temporaires avaient réussi avant suppression. Le script `scripts/local-building.cmd --building=debug --modules=acorn` a ensuite réussi ; l'exécutable, les DLL App/SDK/MediaEngine dans `lib` et le module Acorn ont été vérifiés dans `F:\GW GUI\build\Debug\GW GUI`. Le dossier du module contient uniquement sa DLL, son PDB, son fichier deps et son manifeste ; les dépendances communes restent dans `lib`. Example est inclus par le script existant. Aucun commit créé.

Limites matérielles documentées : les contrôleurs disquette traitent les pistes régulières existantes, sans reproduire toutes les protections ni créer une nouvelle topologie physique par formatage. Le transport réseau utilise des trames délimitées, sans simulation analogique du câble ; l'essai NFS utilise un correspondant de test, et non un serveur de fichiers complet. Ces limites ne remplacent pas une réussite de lecture/écriture par un succès fictif.

Validation : cinq essais temporaires avec les ROM externes MOS 1.20, BASIC II et ADFS 1.30 réussissent. Chaque unité 0 à 3 monte un volume, exécute SAVE, NEW, LOAD, LIST, RUN et CAT, puis publie le programme dans le DAT. Les scénarios des unités secondaires conservent une unité 0 de démarrage distincte, inchangée. Dans cette configuration ADFS, les unités Winchester utilisent les numéros 0 à 3 ; une tentative de monter 4 sollicite le contrôleur disquette. ADFS réessaie l'unité 0 lorsqu'elle manque ; cette attente au démarrage n'a pas été validée jusqu'à son terme. Le cinquième scénario vérifie le montage par EmulatorCore, la copie redirigée DAT/DSC, l'éjection/remontage, l'extinction, la libération et la protection en écriture ; l'image source reste identique. Ces essais et leurs images ont été supprimés. Les trois nouveaux libellés de lecteur sont traduits dans toutes les langues par Argos. Econet reste à implémenter.

Le contrat `IEmulationDiskImage.Metadata` expose les métadonnées existantes en lecture seule et `UpdateMetadata` prépare les clés prises en charge. `EmulationDiskMetadataKeys.AcornScsiParameters` désigne le DSC encodé en Base64. MODE SENSE le lit, MODE SELECT le transmet par le relais SDK. `MediaRuntimeDisk.cs` valide une géométrie de même capacité, conserve les champs DSC non remplacés et utilise la publication DAT/DSC atomique lors de `FlushAsync`. Un format ou une clé non pris en charge est refusé ; aucun redimensionnement implicite n'a lieu.

Les 118 contrôles AcornEmu conservés passent, dont quatorze pour le SCSI et le bus. Un essai temporaire supplémentaire a vérifié la modification du DSC, la conservation exacte du DAT et des champs restants, le rejet d'une capacité incohérente et de l'écriture protégée, puis la réouverture. Le verrouillage volontaire du DAT fait échouer la publication sans toucher aux deux fichiers ; une nouvelle tentative après libération réussit. Le test temporaire et ses images ont été supprimés, puis les tests conservés reconstruits.

Sources supplémentaires : [B-em, géométries sectorielles et organisation des faces](https://github.com/stardot/b-em/blob/master/src/sdf-geo.c), [Guide To Disc Formats de Gerald Holdsworth](https://www.geraldholdsworth.co.uk/documents/DiscImage.pdf), définitions Greaseweazle v1.23 déjà conservées dans `GWGUI.MediaEngine/Images/Formats/Definitions/diskdefs_acorn.cfg`. Répertoires BBC : en-tête à 0, entrées à 5, nom à 1 228, titre à 1 241, séquence finale à 1 274 et signature finale à 1 275 ; adresses exprimées en secteurs de 256 octets.


### Correction des dialogues de lecteurs et des données médias — 26 octobre 2026

Le catalogue disponible sert au bouton Ajouter ; `Engine.CreateRuntimeAsync` transmet à App uniquement les emplacements effectivement configurés. La barre de lecteurs réutilise son affichage existant.

Le contrôleur 8271/WD1770 est réglé dans la configuration du lecteur de disquettes et reste commun aux deux lecteurs BBC. Les choix de lecteurs présentent faces SS/DS, densité SD/DD et capacité en Kio. Les limites de cylindres sont conservées, car deux supports de même capacité peuvent avoir des géométries différentes.

`GWGUI.Emulation.Acorn/Emulators/AcornEmu/Dictionaries/FloppyDriveModels.cs` possède les profils physiques BBC, les formats autorisés et la validation matérielle. Les constantes matérielles résident dans `Constants/FloppyDriveConstants.cs`. Le contrôleur actif détermine les modèles et les formats proposés : le 8271 exclut la double densité. Le module transmet un identifiant de format à `CreateBlankDiskAsync` ; MediaEngine réutilise `BlankSectorImageFactory` et ses écrivains existants, sans choisir le lecteur ou la machine. La fenêtre affiche le nom du format, son extension et sa géométrie ; un choix unique est automatique, plusieurs choix restent disponibles dans la liste.

La configuration cassette regroupe chargement, mécanique et profil RAW. Débit automatique/300/1200 et mode de chargement utilisent des boutons radio. Automatique suit le débit du circuit série réglé par le logiciel BBC ; il ne détecte pas le débit dans le fichier. Les curseurs mécaniques sont courts, à crans, avec présélections et visibles seulement en temps réel. Les délais concernent le démarrage du moteur avant lecture/enregistrement. Le groupe RAW distingue fréquence d'échantillonnage, mono/stéréo, profondeur, signe et little-endian. La hauteur est bornée et le contenu peut défiler, avec les boutons de validation séparés.

`MechanicalSoundFunctions` et ses traitements cassette ainsi que ses fichiers Floppy/HardDisk/Optical rendent les bruits mécaniques en mono, sans posséder de sortie audio ni modifier les médias. Les paramètres fixes et identifiants sont dans `MechanicalSoundConstants`, les profils synthétiques dans `MechanicalSoundProfiles`, et les identifiants sont relayés au SDK par `EmulationMechanicalSoundConstants`. Ces profils représentent des familles de mécanismes, pas l'enregistrement exact d'un lecteur commercial. Les consommateurs de MediaEngine peuvent appeler le même rendu ; les modules y accèdent exclusivement par Emulation.

Les opérations comprennent moteur, lecture, écriture, recherche et arrêt selon le mécanisme, ainsi qu'insertion/éjection pour disquettes et lecteurs optiques, et éjection cassette. Les profils distinguent disquettes 5,25/3,5 pouces, Winchester et HDD 3,5/2,5 pouces, mécanismes optiques à tiroir ou à fente. Cela fournit le service optique commun sans ajouter de lecteur CD au Model A.

`EmulatorCore.FloppySound.cs` et `EmulatorCore.HardDiskSound.cs` mélangent le rendu au son de la machine. Les compteurs des contrôleurs conservent les lectures, écritures et mouvements terminés entre deux images vidéo. Chaque lecteur peut activer ses bruits et choisir son profil ; les opérations accélérées restent silencieuses. Les files d'événements sont bornées. Les sons ne sont ni enregistrés sur les images ni utilisés pour simuler leurs données.

Contrôles : les 165 tests AcornEmu ont réussi avant les derniers ajouts de modèles et de sons. Les scénarios autonomes de compatibilité, continuité sonore et profils ont ensuite été complétés. Ils restent à exécuter : compilation et remplacement du build sont suspendus à la demande utilisateur. Aucun nouveau build achevé ni nouveau commit ne valide ce dernier ensemble.


### Correction des destinations et de la création des images — 26 octobre 2026

AcornEmu ouvre et modifie l’image montée, en respectant sa protection en écriture. Les fonctions de copie redirigée et les sous-dossiers imposés `HardDisks`/`Floppies` ont été supprimés. Les options de redirection ne sont plus proposées pour AcornEmu. Les chemins HDD utilisés par les dialogues communs restent ceux déjà configurés par App.

`App/Views/Dialogs/Emulation/Storage/MediaCreationDialog.cs` confirme le fichier choisi et sépare création et montage : « Créer » publie l’image sans changer le média monté et laisse le dialogue ouvert ; « Créer et monter » crée puis transmet l’image à l’appelant pour montage ; « Fermer » quitte le dialogue et « Annuler » abandonne avant création. La création refuse une destination déjà existante. Une fermeture pendant l’écriture est bloquée jusqu’à la fin de l’opération.

`FloppyDriveConfigurationDialog.cs` utilise ce dialogue pour les formats vierges du modèle sélectionné. « Créer et monter » valide les réglages du lecteur et transmet le chemin à `EmulationStorageSettingsController`, qui remplace le média de cet emplacement par les contrats existants. « Créer » seul conserve le montage précédent.

Depuis REC, `MachineController.cs` conserve le choix du format cassette et du profil RAW, puis ouvre la même validation. « Créer » seul ne modifie pas la bande en cours ; « Créer et monter » monte la nouvelle image via `MachineSession.InsertAsync` puis arme l’enregistrement. Les paramètres RAW proviennent aussi des blocs de configuration déclarés par le module, pas seulement de la liste plate de champs.

Les trois nouveaux messages sont traduits dans les 29 langues avec Argos ; le français a été relu. Vérification XML des ressources et vérification statique des références et des espaces Git effectuées. Aucune compilation, aucun essai GUI, aucune modification du build : validation compilée restant à faire à la reprise autorisée.


### Configuration des extensions disquette dans AcornEmu

L'onglet Extensions contient le cadre Extensions disquette. Les boutons radio Intel 8271 et Acorn WD1770 choisissent le contrôleur actif commun aux deux lecteurs. Chaque contrôleur a son champ ROM externe, enregistré dans les options de la configuration. Le champ du contrôleur non sélectionné est grisé ; son chemin est conservé. Solidisk rend les deux champs modifiables, ce qui prépare les deux possibilités sans imposer une ROM par lecteur.

Les nouvelles options sont `acornemu.floppy.rom8271`, `acornemu.floppy.rom1770` et `acornemu.floppy.solidisk`. Le choix actif reste `acornemu.floppy.controller`. L'ancien champ `acornemu.dfs` est relu pour la configuration ancienne tant que les champs séparés ne sont pas enregistrés. Une fois un nouveau champ enregistré, il ne sert plus de repli pour l'autre contrôleur.

Les deux ROM peuvent être renseignées avec Solidisk ; seule celle du contrôleur actif est exposée dans la banque disque au démarrage. Cela évite de laisser une ancienne ROM piloter les registres d'un contrôleur absent. Le choix d'interface nécessite un redémarrage de la machine émulée ; il ne ferme pas GW GUI. Il n'y a pas de choix par lecteur ni de bascule automatique au montage d'une image. Le second chemin ne correspond pas à une seconde ROM de système de fichiers sur le même contrôleur : cette interface propose une ROM par contrôleur.

`FloppyExtensionConstants.cs` définit les clés et ressources de cette extension. `FloppyExtensionFunctions.cs` décrit ses champs, leurs autorisations et la projection des ROM candidates vers chaque contrôleur. La reconnaissance matérielle reste dans les définitions Common/Machines : `FirmwareDiskController.cs` et le champ `FirmwareDefinition.DiskController`. Une identité non évaluée reste inconnue ; le nom du fichier ne suffit pas à certifier son interface. Le gestionnaire App réutilise la présentation ROM avec une liste distincte par onglet, et interdit l'action Utiliser sur une destination grisée.

Les formats d'image acceptés sont issus des profils de lecteur de MediaEngine. Leur liste ne dépend plus du contrôleur sélectionné. Le montage conserve la validation de la géométrie et des capacités physiques du lecteur ; la lecture effective dépend ensuite du contrôleur actif, de sa ROM et du programme exécuté, notamment pour les protections qui accèdent directement au matériel.


### Réglages et création des lecteurs — révision du 27 octobre 2026

Le modèle de lecteur de disquette continue de provenir du catalogue physique MediaEngine (faces, densité, géométrie et formats vierges). Les huit profils 5,25 pouces restent disponibles indépendamment du contrôleur commun : monter un conteneur accepté par un lecteur ne certifie pas que la ROM et le contrôleur sauront exécuter son contenu. Les réglages exposent des vitesses discrètes, une protection et les seuls contrôles d'activation/volume sonore. Aucun sélecteur de banque sonore n'est proposé.

Le profil sonore de disquette est automatiquement celui du lecteur BBC Micro enregistré, dont la marque exacte est inconnue. Le service commun applique les replis déjà définis, dont Alps Electric pour une opération manquante. Pour Winchester, la banque ne possède aucun enregistrement attesté Rodime RO203E : le profil enregistré Miniscribe 8425 est un repli de mécanisme Winchester, pas un enregistrement présenté comme celui d'un Winchester Acorn officiel. Le choix arbitraire Seagate ST3120A conservé dans une ancienne configuration n'est plus appliqué.

Les profils HDD appartiennent au module Acorn, dans `AcornEmu/Dictionaries/HardDiskFormats.cs` et `Constants/WinchesterModelConstants.cs`. Les présélections Winchester 110 (306 cylindres, 4 têtes) et 130 (640 cylindres, 6 têtes) utilisent la disposition DAT/DSC de 33 secteurs de 256 octets. Le profil Acorn SCSI autorise les géométries personnalisées compatibles, dans la limite des commandes à six octets du contrôleur. Le module transmet la géométrie et le format demandés au service de création existant ; MediaEngine ne fournit aucun catalogue de lecteurs matériels.

Les deux présélections imposent leur géométrie et leur capacité exactes, sans taille personnalisée ni arrondi silencieux. Le profil compatible permet une géométrie explicite dans les limites du conteneur et du contrôleur. La préallocation est transmise jusqu'au fichier temporaire DAT avant la publication atomique DAT/DSC ; elle n'est plus désactivée par le profil. Les deux fichiers sont publiés ensemble sans écraser une destination créée entre-temps.

La création de disquette demande un format et un fichier de destination. Les actions Créer et Créer et monter sont disponibles lorsque ces deux choix sont renseignés ; Annuler devient Fermer après une création sans montage. La sauvegarde laisse choisir le fichier et ne réutilise le dossier initial que s'il existe.

Les fenêtres utilisent le nom localisé de la machine. Les identifiants de périphérique et l'interface HDD figurent dans l'en-tête, les groupes de réglages sont déclarés par le module et les curseurs verticaux ont une hauteur bornée. Aucun texte de préparation Atari ne s'applique au dialogue générique.

Les quatre unités logiques du cœur restent disponibles. Le manuel officiel ADFS décrit un Winchester à l'adresse 0 et réserve 1–3 aux extensions futures ; il ne documente pas quatre unités standard livrées avec la machine. Cette distinction ne supprime pas les unités effectivement émulées.

Sources : [manuel de maintenance Winchester Acorn](https://www.theoddys.com/acorn/acorn_system_filing_systems/winchester_disks/Acorn%20Winchester%20Disc%20Service%20Manual.pdf), [manuel Acorn ADFS](https://8bs.com/othrdnld/manuals/hnooijen/Acorn/Advanced-Disc-Filing-System-User-Guide.pdf). Le tableau matériel secondaire du Winchester 130 mentionne aussi un format à 32 secteurs ; le profil DAT/DSC actuel utilise explicitement les 33 secteurs attendus par ses lecteurs, sans confondre les capacités des deux dispositions.
