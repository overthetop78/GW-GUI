# Formats d’images de cassettes et bandes

Ce document fixe le périmètre des Readers, Writers, décodeurs et encodeurs séquentiels de
MediaEngine. Une cassette audio numérisée, une image structurée de micro-ordinateur et une image de
bande magnétique à enregistrements sont trois représentations différentes d’un média séquentiel.
Elles partagent une chronologie et des positions, mais pas les mêmes données ni les mêmes garanties.

## Règles communes

- Le document produit utilise `MediaKind.Tape` et une représentation `Sequential`.
- Chaque segment conserve sa position, sa durée ou sa longueur, son type connu et ses données
  originales. Un segment inconnu reste conservé et signalé.
- Une face, une piste, un canal ou un sens de lecture n’est déclaré que si le fichier, le choix de
  l’utilisateur ou le matériel le fournit. Un fichier mono ne prouve pas qu’une cassette n’avait
  qu’une face ; un fichier stéréo ne prouve pas qu’elle possédait deux pistes logiques.
- Les canaux audio restent distincts jusqu’au choix explicite d’un décodeur. Aucun mixage n’est
  appliqué silencieusement.
- Les Readers de conteneurs ne décodent pas eux-mêmes les protocoles des machines. Ils produisent
  des échantillons, impulsions, blocs ou enregistrements que des décodeurs enregistrés interprètent.
- Un Writer structuré refuse un document qui contient des informations que la destination ne peut
  pas conserver. Les chunks inconnus sont recopiés à l’identique ou provoquent un refus.
- L’Explorateur crée des dossiers et fichiers seulement à partir de blocs décodés et validés. Les
  silences, impulsions et segments inconnus restent visibles comme diagnostics.

## Familles de représentations

| Famille | Unité conservée | Exemples | Affichage principal | Exploration possible |
|---|---|---|---|---|
| Audio échantillonné | Échantillon par canal et fréquence | WAV | Forme d’onde, canaux, temps, silences et zones décodées | Après décodage d’un protocole reconnu |
| Signal ou impulsions structurés | Durée d’impulsion, porteuse, pause et bloc | Atari CAS `fsk `, TZX/CDT, Commodore TAP, UEF | Chronologie des impulsions et blocs sur une ou plusieurs lignes | Après décodage des blocs de la machine |
| Octets structurés sans chronologie complète | En-têtes et données déjà décodées | Atari CAS `data`, MSX CAS, Spectrum TAP | Suite de blocs avec durées inconnues ou estimables seulement par le protocole | Oui pour les blocs compris, avec pertes de chronologie signalées |
| Bande magnétique à enregistrements | Enregistrement, marque de bande, espace et fin de média | SIMH TAP | Lignes séquentielles séparées par fichiers et marques | Après identification du format des enregistrements |

## Tableau de décision

| Format | Identification | Structure réellement conservée | Faces, pistes, canaux et sens | Lecture retenue | Écriture retenue |
|---|---|---|---|---|---|
| WAV | Conteneur RIFF/RF64 avec type `WAVE`, chunks `fmt ` et `data` | Échantillons PCM, fréquence, profondeur et canaux ; aucun bloc de machine imposé | Canaux explicites ; faces, pistes logiques et sens absents sans contexte | PCM entier 8, 16, 24 et 32 bits, mono ou multicanal ; chunks inconnus conservés | PCM avec mêmes paramètres ; production d’un nouveau fichier à partir d’échantillons ou d’un encodeur |
| Atari CAS | Premier chunk `FUJI`, puis chunks `baud`, `data` et `fsk ` | Octets, débit, durée de porteuse et impulsions FSK selon le chunk | Une chronologie ; face et piste non décrites par le format | Oui, avec conservation des chunks inconnus | Oui pour les chunks compris ; inconnus recopiés ou refusés |
| TZX / CDT / TSX | En-tête `ZXTape!` suivi de `0x1A` et version | Suite de blocs typés : données, impulsions, tonalités, pauses, contrôles et métadonnées | Marqueurs et regroupements possibles ; faces et pistes seulement si des blocs ou un contexte les décrivent | Oui pour TZX 1.20 et blocs inventoriés ; CDT/TSX classés par contexte de machine | Oui en conservant l’ordre et les blocs compris ; inconnus conservés verbatim |
| Spectrum TAP | Aucun en-tête global ; blocs précédés d’une longueur little-endian 16 bits | Octets de blocs Spectrum avec flag et checksum ; chronologie détaillée absente | Une suite de blocs ; aucune face, piste ou canal | Oui après choix explicite Spectrum et validation de toute la chaîne de longueurs | Oui pour des blocs Spectrum complets ; pas depuis un signal non décodé |
| Commodore TAP | `C64-TAPE-RAW` ou `C16-TAPE-RAW`, version et plateforme | Durées d’impulsions brutes ; les versions changent l’encodage des longues impulsions | Une suite d’impulsions ; standard vidéo explicite, faces et pistes absentes | Oui pour versions 0, 1 et 2 avec plateforme compatible | Oui à partir d’impulsions représentables par la version choisie |
| UEF | `UEF File!\0` ou conteneur entier gzip, puis version et chunks | Porteuses, flux asynchrones, bits, pauses, tonalités de sécurité, marqueurs et métadonnées | Des marqueurs peuvent distinguer bandes, faces et positions ; phase et débit peuvent varier | Oui pour UEF 0.10 et chunks cassette inventoriés | Oui en conservant les chunks compris et les chunks inconnus verbatim |
| MSX CAS | Séquence d’en-tête `1F A6 DE BA CC 13 7D 74` alignée sur 8 octets ; pas de signature de conteneur globale sûre | Octets déjà décodés et séparateurs d’en-tête ; débits, espaces et longueurs de porteuse absents | Une suite logique ; aucune face, piste, durée ou canal | Oui seulement après contexte MSX explicite ou validation complète des séparateurs | Oui comme suite d’octets MSX ; conversion depuis audio seulement après décodage sans erreur |
| SIMH TAP | Pas de signature globale ; enregistrements encadrés par deux longueurs 32 bits little-endian et marqueurs réservés | Enregistrements, drapeau d’erreur, marques de bande, espaces d’effacement et fin de média | Ordre avant/arrière permis par les deux longueurs ; pistes physiques et densité absentes | Oui après validation structurelle complète ou sélection explicite | Oui avec enregistrements pairs, marques et indicateurs d’erreur conservés |

## Détails par format

### WAV

Le Reader WAV expose les chunks, les paramètres PCM et les échantillons sans décider quelle machine
les a produits. Une forme d’onde peut être affichée immédiatement. Le registre des décodeurs essaie
ensuite uniquement les protocoles compatibles avec le contexte demandé et retourne un score, les
blocs reconnus et les zones non décodées. Les formats compressés et flottants sont reportés ; ils ne
doivent pas être traités comme du PCM entier.

Un Writer WAV peut réécrire les échantillons ou recevoir le signal produit par un encodeur. La
fréquence, les canaux et la profondeur de sortie sont explicites. Une conversion ne doit pas réduire
automatiquement plusieurs canaux en mono.

### Atari CAS

Le marqueur `FUJI` ouvre une suite de chunks avec identifiant, longueur et valeur auxiliaire. `baud`
change le débit, `data` conserve des octets et une durée de marque, et `fsk ` conserve des durées
alternées SPACE/MARK en dixièmes de milliseconde. Le Reader garde l’ordre et la chronologie calculable
de ces chunks. Le Writer peut produire les mêmes structures sans passer obligatoirement par WAV.

### TZX, CDT et TSX

Le Reader valide l’en-tête, la version, la longueur de chaque bloc et les cibles des boucles, appels
et sauts avant d’exposer la chronologie. Le même conteneur peut être utilisé par plusieurs familles de
machines ; l’extension ou le contexte choisit les décodeurs compatibles, sans dupliquer le parser.
Les blocs inconnus dont la longueur est définie sont conservés afin de permettre un aller-retour.

### Spectrum TAP

Ce TAP est une suite simple de blocs et partage son extension avec Commodore TAP et SIMH TAP. Il ne
peut donc être choisi par extension. Le Reader doit valider chaque longueur jusqu’à la fin et exige
un contexte Spectrum explicite. Il expose les octets, flags et checksums, tout en signalant que les
temporisations originales ne sont plus disponibles.

### Commodore TAP

Le conteneur garde les durées d’impulsions plutôt que les fichiers décodés. La signature distingue
les familles C64 et C16 ; les champs version, plateforme et standard vidéo influencent le calcul du
temps. Le Reader ne fabrique pas de fichiers Commodore. Un décodeur séparé transforme éventuellement
les impulsions en blocs et conserve les zones non reconnues.

### UEF

UEF commence directement par son en-tête ou est entièrement compressé avec gzip. Les chunks sont
lus dans l’ordre ; leur version détermine parfois les unités. Les chunks cassette peuvent représenter
porteuse, données, bits, silences, fréquence, phase et marqueurs de bandes ou faces. Les autres usages
de UEF ne classent pas automatiquement le document comme cassette. Le Writer conserve les chunks
inconnus à l’identique lorsqu’ils ont été lus sans corruption.

### MSX CAS

MSX CAS stocke des octets déjà décodés. La séquence d’en-tête doit commencer sur un multiple de huit
octets, mais elle ne conserve ni débit, ni silence, ni distinction entre en-tête long et court. Le
Visualiseur affiche donc des blocs avec chronologie inconnue. Un encodeur peut produire un signal
standard, mais celui-ci est une reconstruction et doit être présenté comme tel.

### SIMH TAP

Chaque enregistrement possède une longueur initiale et finale identique ; une donnée de longueur
impaire est complétée à l’alignement pair. Le bit fort signale une erreur, les 24 bits faibles donnent
la longueur, et des valeurs réservées représentent marque de bande, espace d’effacement et fin de
média. Cette structure permet le parcours avant et arrière. Elle ne conserve pas la densité, le
nombre de pistes magnétiques ni les signaux physiques de la bande d’origine.

## Faces, pistes, canaux et lignes du Visualiseur

- Une cassette audio est affichée sur une chronologie par canal. Une seconde face est une seconde
  chronologie seulement si l’utilisateur ou un marqueur de format l’associe explicitement.
- Une image structurée peut répartir ses segments sur plusieurs lignes pour garder une échelle
  lisible. Ce retour à la ligne est une disposition graphique, pas une nouvelle piste physique.
- Une bande SIMH est découpée visuellement par marques de bande et groupes d’enregistrements. Le
  nombre de pistes physiques reste inconnu dans ce format.
- Le sens normal va du début vers la fin du document. Un sens inverse est exposé uniquement si le
  format, le matériel ou le choix de face le demande.

## Ordre d’implémentation

1. WAV valide les échantillons, canaux et la chronologie générique.
2. Atari CAS valide dans un même format les octets et les impulsions structurées.
3. TZX/CDT/TSX valide les blocs variés, contrôles et contextes de machines.
4. Spectrum TAP, Commodore TAP et MSX CAS valident les collisions d’extension et les pertes de
   chronologie propres aux formats simples.
5. UEF valide les chunks, marqueurs de faces et conteneurs gzip.
6. SIMH TAP valide les enregistrements, marques et parcours avant ou arrière des bandes numériques.

## Références

- [Microsoft — RIFF et WAVE](https://learn.microsoft.com/en-us/windows/win32/xaudio2/resource-interchange-file-format--riff-)
- [Atari800 — implémentation et description du format CAS](https://sources.debian.org/src/atari800/4.1.0-3/src/img_tape.c)
- [World of Spectrum — formats TAP et TZX](https://worldofspectrum.org/faq/reference/formats.htm)
- [VICE — formats TAP et T64](https://vice-emu.sourceforge.io/vice_17.html)
- [UEF File Format specification](http://electrem.emuunlim.com/UEFSpecs.htm)
- [MSX Wiki — format CAS](https://direct.msx.org/wiki/Emulation_related_file_formats)
- [Open SIMH — Magtape Representation and Handling](https://opensimh.org/simdocs/simh_magtape.html)
