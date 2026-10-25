# AcornEmu — références communes aux modèles Acorn

Recherche effectuée le 25 octobre 2026. Ce dossier prépare AcornEmu, le nouvel émulateur du module Acorn. Le premier modèle étudié est le BBC Micro Model A.

## Répartition des informations

| Portée | Document |
| --- | --- |
| AcornEmu, sources transversales et possibilités de partage entre familles | Ce document |
| Architecture des BBC Micro A/B et composants réutilisables dans cette gamme | [BBC Micro — architecture](BBC%20Micro/bbc-micro-architecture.md) |
| Équipement, variantes et limites du Model A | [Model A — référence](BBC%20Micro/Model%20A/model-a-reference.md) |

Les modèles resteront séparés dans AcornEmu. Les rapprochements ci-dessous sont des résultats de recherche et des pistes de partage, pas des choix de code déjà réalisés. Une caractéristique du BBC Micro ne s'applique pas automatiquement à l'Atom, à l'Electron ou aux machines ARM.

## Sources d'émulation consultées

Les révisions sont figées pour retrouver exactement les sources consultées. Une implémentation existante sert de comparaison ; elle ne remplace pas le schéma de la machine.

| Projet | Révision consultée | Intérêt pour cette recherche |
| --- | --- | --- |
| [MAME](https://github.com/mamedev/mame/tree/84981cc15cfc1429e48f098d80c6dd97bc82a0bc/src/mame/acorn) | `84981cc15cfc1429e48f098d80c6dd97bc82a0bc` | Configuration explicite `bbca`, composants, ROM et périphériques séparés. |
| [B-Em](https://github.com/stardot/b-em/tree/a76c3655140a9fc3c70ecef25660932e0890b1e5) | `a76c3655140a9fc3c70ecef25660932e0890b1e5` | Profils Model A, logique CPU/mémoire et matériel BBC. |
| [jsbeeb](https://github.com/mattgodbolt/jsbeeb/tree/801dc90317514cb6a35500abda5abaf2f9fbc8aa) | `801dc90317514cb6a35500abda5abaf2f9fbc8aa` | Description des modèles, composants et séparation entre identité de machine et options de session. |
| [b2](https://github.com/tom-seddon/b2/tree/e1c9b84a9460909d4dc2030ad1d4e050012b1910) | `e1c9b84a9460909d4dc2030ad1d4e050012b1910` | Comparaison BBC Micro/Master/Compact et outils de débogage. Le README renvoie également vers un port libretro distinct. |
| [b2-libretro](https://github.com/zoltanvb/b2-libretro/tree/9793f6a8040c416d147003cada29363b955b54e7) | `9793f6a8040c416d147003cada29363b955b54e7` | Port de Zoltan Balogh ; cœur BBC et adaptation libretro séparés. Model B comme référence technique proche du Model A. |

### b2-libretro : source complémentaire prioritaire pour le BBC Micro

Étude ajoutée à la demande de l'utilisateur le 25 octobre 2026. Le dépôt du port est distinct du b2 d'origine ; la comparaison doit se faire sur sa propre révision. La [documentation Libretro](https://docs.libretro.com/library/b2/) décrit des configurations Model B, B+ et Master, sans profil Model A proposé dans sa liste.

Le code du port sépare `src/beeb`, qui contient la machine et ses composants, de `src/libretro`, qui assure les échanges avec le frontal. Il permet donc d'étudier les composants BBC indépendamment des choix d'entrée, de présentation et de chargement de RetroArch. Les détails utiles sont consignés [dans la fiche BBC Micro](BBC%20Micro/bbc-micro-architecture.md#référence-complémentaire--b2-libretro), et les écarts vers le Model A [dans sa fiche](BBC%20Micro/Model%20A/model-a-reference.md#partir-du-model-b-de-b2-libretro).

Le fichier [src/COPYING du port](https://github.com/zoltanvb/b2-libretro/blob/9793f6a8040c416d147003cada29363b955b54e7/src/COPYING) fournit la GPL version 3. Étudier son fonctionnement ne décide pas d'une reprise de code ni d'une dépendance au cœur b2 dans AcornEmu.

Le fichier [models.js de jsbeeb](https://github.com/mattgodbolt/jsbeeb/blob/801dc90317514cb6a35500abda5abaf2f9fbc8aa/src/models.js) sépare notamment type de CPU, ROM, contrôleur de disque, fréquence et banques. Cela fournit un exemple concret de description de modèles. Ce constat ne prouve ni la présence d'un profil Model A utilisable dans chaque projet, ni une compatibilité totale.

Les licences doivent être examinées fichier par fichier avant toute reprise de code. Le pilote BBC de MAME porte un en-tête BSD-3-Clause ; cela ne donne aucun droit supplémentaire sur les ROM. Les autres dépôts possèdent leurs propres conditions : [B-Em COPYING](https://github.com/stardot/b-em/blob/a76c3655140a9fc3c70ecef25660932e0890b1e5/COPYING), [b2, section Licence](https://github.com/tom-seddon/b2/blob/e1c9b84a9460909d4dc2030ad1d4e050012b1910/README.md#licence), [jsbeeb](https://github.com/mattgodbolt/jsbeeb/tree/801dc90317514cb6a35500abda5abaf2f9fbc8aa).

## ROM disponibles au téléchargement

Liens vérifiés le 25 octobre 2026. Les fichiers ci-dessous ont été lus en mémoire pour vérifier leur taille et calculer leur SHA-1 ; aucune image ROM n'a été enregistrée dans le dépôt. Les liens sont fixés à une révision pour conserver un contenu reproductible.

### Dépôt B-Em

Source : [répertoire ROM de B-Em](https://github.com/stardot/b-em/tree/a76c3655140a9fc3c70ecef25660932e0890b1e5/roms).

| Téléchargement direct | Octets | Usage et SHA-1 vérifié |
| --- | --- | --- |
| [os01.rom](https://raw.githubusercontent.com/stardot/b-em/a76c3655140a9fc3c70ecef25660932e0890b1e5/roms/os/os01.rom) | 16 384 | MOS 0.10 BBC A/B ; `4b0ece6dc139d5d3f4fabd023716fb6f25149b80`. |
| [os12.rom](https://raw.githubusercontent.com/stardot/b-em/a76c3655140a9fc3c70ecef25660932e0890b1e5/roms/os/os12.rom) | 16 384 | MOS 1.20 BBC A/B ; `0d9bcaf6a393c9ce2359ed700ddb53c232c2c45d`. |
| [basic1.rom](https://raw.githubusercontent.com/stardot/b-em/a76c3655140a9fc3c70ecef25660932e0890b1e5/roms/general/basic1.rom) | 16 384 | BASIC I ; `890f6e3e7fab3340f75b85e93ff29332bc9ecb2e`. |
| [basic2.rom](https://raw.githubusercontent.com/stardot/b-em/a76c3655140a9fc3c70ecef25660932e0890b1e5/roms/general/basic2.rom) | 16 384 | BASIC II ; `4a7393f3a45ea309f744441c16723e2ef447a281`. |
| [dfs09.rom](https://raw.githubusercontent.com/stardot/b-em/a76c3655140a9fc3c70ecef25660932e0890b1e5/roms/general/dfs09.rom) | 8 192 | DFS du profil B avec contrôleur 8271 ; `5cc0f14b8f46855c70eaa653cca4ad079b458732`. |
| [dfs226.rom](https://raw.githubusercontent.com/stardot/b-em/a76c3655140a9fc3c70ecef25660932e0890b1e5/roms/general/dfs226.rom) | 16 384 | Variante DFS à associer à son contrôleur et à son profil ; `cf2ebc422a8d24ec6f1a0320520c38a0e704109a`. |
| [bpos.rom](https://raw.githubusercontent.com/stardot/b-em/a76c3655140a9fc3c70ecef25660932e0890b1e5/roms/os/bpos.rom) | 16 384 | ROM système B+ ; `d9426dd7454ca199cdef9435daa332cdf405c965`. |
| [mos320.rom](https://raw.githubusercontent.com/stardot/b-em/a76c3655140a9fc3c70ecef25660932e0890b1e5/roms/os/mos320.rom) | 131 072 | Ensemble Master MOS 3.20, à découper selon le modèle ; `0275719aa7746dd3b627f95ccc4362b564063a5e`. |
| [os51.rom](https://raw.githubusercontent.com/stardot/b-em/a76c3655140a9fc3c70ecef25660932e0890b1e5/roms/os/os51.rom) | 16 384 | ROM système Master Compact ; `1c5094095160665d6dd8968bff52758f5e461f27`. |

Le [fichier de profils B-Em](https://github.com/stardot/b-em/blob/a76c3655140a9fc3c70ecef25660932e0890b1e5/b-em.cfg) précise les associations. La présence de `bpos.rom` ou `os51.rom` seule ne signifie pas que tous les firmwares du modèle sont réunis. Les ROM complémentaires sont dans le même dépôt ; elles devront être associées lors de l'étude de chaque modèle.

### Miroirs vérifiés dans jsbeeb

Ces quatre téléchargements possèdent exactement les mêmes tailles et SHA-1 que leurs équivalents B-Em :

| Téléchargement direct jsbeeb | Équivalent B-Em |
| --- | --- |
| [os.rom](https://raw.githubusercontent.com/mattgodbolt/jsbeeb/801dc90317514cb6a35500abda5abaf2f9fbc8aa/public/roms/os.rom) | `os12.rom`, MOS 1.20. |
| [BASIC.ROM](https://raw.githubusercontent.com/mattgodbolt/jsbeeb/801dc90317514cb6a35500abda5abaf2f9fbc8aa/public/roms/BASIC.ROM) | `basic2.rom`, BASIC II. |
| [os01.rom](https://raw.githubusercontent.com/mattgodbolt/jsbeeb/801dc90317514cb6a35500abda5abaf2f9fbc8aa/public/roms/os01.rom) | `os01.rom`, MOS 0.10. |
| [a01/BASIC1.rom](https://raw.githubusercontent.com/mattgodbolt/jsbeeb/801dc90317514cb6a35500abda5abaf2f9fbc8aa/public/roms/a01/BASIC1.rom) | `basic1.rom`, BASIC I. |

Un nom différent n'implique donc pas une ROM différente. L'identification par taille et empreinte permet de reconnaître la même image sans exiger le nom choisi par un autre émulateur.

### Sources pour comprendre le contenu

- [MOS Reassembly](https://tobylobster.github.io/mos/) : réassemblage commenté du MOS, avec une source assembleur et des explications de ses routines. C'est une référence de fonctionnement, distincte d'un simple fichier binaire.
- [BBC Micro ROM Library](https://tobylobster.github.io/rom_library/) : catalogue et téléchargements pour identifier les variantes de ROM et les extensions. Une entrée du catalogue ne constitue pas une exigence du Model A.

Le rôle et la mise en mémoire des images BBC sont décrits [dans la fiche de gamme](BBC%20Micro/bbc-micro-architecture.md#fichiers-rom-et-liaisons-avec-la-machine), et le choix des deux ROM du Model A [dans sa fiche](BBC%20Micro/Model%20A/model-a-reference.md#télécharger-les-rom-du-model-a).

## Médias : ce qui peut être commun

Le format UEF décrit des médias associés aux BBC Micro, Electron et Atom. C'est un conteneur de blocs, éventuellement gzip ; son extension ne dit donc pas à elle seule quelle machine, quel signal ou quelle forme de données il contient. La spécification indique un en-tête avec signature, version mineure puis majeure, et des données little-endian sauf indication contraire. Pour les cassettes, les paramètres initiaux sont une fréquence de base de 1 200 Hz, un débit de 1 200 bauds et une phase de 180 degrés. [Spécification originale de Thomas Harte](https://electrem.emuunlim.com/UEFSpecs.htm), sections « General Structure » et « Tape ».

| Élément UEF | Sens utile à l'émulation |
| --- | --- |
| `0x0100` | Données avec cadrage série implicite. |
| `0x0110` | Porteuse sans octets utiles. |
| `0x0111` | Porteuse avec octet intermédiaire. |
| `0x0112` / `0x0116` | Intervalles sans signal, avec encodages de durée différents. |

Cette liste n'est pas toute la spécification. La lecture de quelques blocs courants ne constitue pas une prise en charge complète des protections, des débits variables ou de tous les médias UEF. [Spécification UEF](https://electrem.emuunlim.com/UEFSpecs.htm).

**Déduction pour AcornEmu :** la représentation des durées et transitions d'un média pourrait être partagée, tandis que le câblage, le décodage cassette et les registres restent propres à chaque machine. La lecture d'une image ne doit pas être confondue avec son acceptation par le matériel émulé. Dans GW GUI, les échanges de médias suivent les interfaces existantes d'Emulation et de MediaEngine.

## Éléments susceptibles d'être partagés

Cette classification décrit les responsabilités découvertes ; le partage réel sera établi à partir des modèles étudiés.

| Élément | Portée envisagée | Condition |
| --- | --- | --- |
| CPU NMOS 6502 et chronologie des accès | Modèles équipés d'un 6502 compatible | Vérifier le composant exact, les signaux et les particularités du bus. |
| VIA 6522, ACIA 6850, générateur sonore | Modèles portant effectivement ces composants | Conserver leur état matériel ; relier séparément leurs broches à chaque carte. |
| Ordonnancement des événements | AcornEmu | Pouvoir exprimer plusieurs horloges sans assimiler fréquence CPU et fréquence d'affichage. |
| Mémoire et décodage d'adresses | Description de chaque modèle | Déterminer les miroirs, les zones non équipées et les extensions depuis les circuits. |
| Présentation, commandes et cycle de vie | Interfaces communes déjà présentes dans le module | Adapter les capacités du modèle et du cœur réellement disponibles. |
| Sauvegarde d'état | AcornEmu, avec contenu propre aux composants | Conserver aussi les événements en attente et la position des médias, pas seulement la RAM. |

Les deux premières lignes reposent sur les composants identifiés dans les documents BBC cités dans le dossier de gamme. Leur réutilisation dans d'autres familles Acorn reste à établir : cette recherche ne généralise pas le matériel du BBC Micro à tout Acorn.

## Limite de cette recherche

Les sources ont été lues et confrontées ; aucun des émulateurs externes de référence n'a été lancé pour cette étude. Un premier cœur AcornEmu est désormais implémenté : démarrage sur ROM externes et interaction clavier/BASIC, vérifiés par des essais ciblés. Les fichiers, les résultats et les fonctions restant à réaliser sont détaillés dans la fiche Model A. Les contradictions ou détails électriques non tranchés restent consignés au niveau de la gamme ou du modèle concerné.
