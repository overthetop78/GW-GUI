# Améliorations de l’interface d’émulation

## Objet

Ce document regroupe les améliorations prévues pour l’écran d’émulation et les options Amiga et Atari de GW GUI. Il décrit le comportement attendu, sans encore imposer un découpage ou un ordre de réalisation. Les sections 1 à 6 correspondent aux six images examinées.

## Principes communs

- Une machine possède au maximum une configuration enregistrée : il n’existe pas plusieurs profils nommés pour une même machine.
- L’interface doit toujours permettre d’identifier la marque et la machine actuellement modifiées.
- Les listes de machines et de périphériques compatibles existent déjà et doivent être réutilisées.
- Les éléments déjà présents et fonctionnels ne doivent pas être recréés sous une autre forme.
- Les fonctions communes doivent avoir un comportement cohérent sur Amiga et Atari lorsque les émulateurs le permettent.

## 1. Écran d’émulation

### Focus sans capture de la souris

Quand l’utilisateur clique sur une marge grise autour de l’image émulée, l’écran d’émulation redevient la cible des entrées clavier et manette. Ce clic ne capture pas la souris : sa capture reste une action distincte et explicite.

Comportement attendu :

- les contrôles de GW GUI continuent de recevoir normalement les clics qui leur sont destinés ;
- les raccourcis de l’application restent utilisables et prioritaires ;
- les entrées non consommées par l’interface sont transmises à l’émulateur actif ;
- cliquer sur une marge grise redonne le focus sans transmettre ce clic à la machine émulée ;
- cela ne provoque ni capture, ni déplacement, ni masquage du pointeur.

L’émulation ne doit plus cesser de répondre simplement parce qu’un contrôle de l’interface avait reçu le focus auparavant.

### Filtres vidéo

GW GUI doit proposer des filtres communs aux moteurs D3D11, Vulkan, OpenGL et WPF, dans la limite de leurs possibilités. Les shaders Libretro serviront de références afin de reproduire des effets similaires dans notre moteur. Toute reprise directe de code devra respecter la licence du shader concerné.

Familles à étudier :

- rendu pixelisé, bilinéaire et bilinéaire net ;
- mise à l’échelle entière ;
- agrandissement de pixel art : xBR/xBRZ, ScaleFX, SABR, HQx et ScaleNx ;
- scanlines ;
- moiré et courbure CRT horizontale et verticale ;
- masques CRT : grille d’ouverture, shadow mask et slot mask ;
- largeur et luminosité du faisceau ;
- coins arrondis, centrage et overscan ;
- bloom, glow et halation ;
- convergence et décalage des canaux RVB ;
- entrelacement et scintillement ;
- simulations composite, S-Video, NTSC et PAL ;
- bavure des couleurs et franges chromatiques ;
- reproduction ou atténuation du dithering ;
- grille LCD, sous-pixels et matrice de points ;
- rémanence et temps de réponse LCD ;
- profils colorimétriques de machines portables ;
- netteté, défloutage, débruitage et désentrelacement ;
- luminosité, contraste, gamma, saturation et correction colorimétrique.

Cette liste définit le domaine de recherche, pas le contenu définitif du premier lot. Il faudra évaluer l’intérêt, le coût et les réglages pertinents de chaque effet.

Les effets simples en une passe doivent être distingués des chaînes multipasses utilisant plusieurs textures, des tables de couleurs ou les images précédentes. Une compatibilité complète avec les préréglages RetroArch représenterait un sous-système important ; une première étape pourra reproduire une sélection d’effets directement dans GW GUI.

Références :

- [Documentation des shaders Libretro](https://docs.libretro.com/guides/shaders/)
- [Collection officielle Slang](https://github.com/libretro/slang-shaders)
- [Collection Common Shaders](https://github.com/libretro/common-shaders)
- [CRT EasyMode](https://github.com/libretro/glsl-shaders/blob/master/crt/shaders/crt-easymode.glsl)
- [CRT Guest Advanced NTSC](https://github.com/libretro/slang-shaders/blob/master/crt/crt-guest-advanced-ntsc.slangp)
- [Exemple de rendu Game Boy](https://github.com/libretro/slang-shaders/blob/master/handheld/gameboy.slangp)

## 2. Identification de la machine modifiée

### Configuration existante

La liste existante des machines doit signaler visuellement celles qui possèdent déjà une configuration. Une couleur, une icône ou un indicateur discret peut être employé sans réduire la lisibilité. Cela signifie uniquement que la configuration unique de cette machine existe.

### Contexte visible

Dans la partie **Émulation**, le titre ou un en-tête permanent doit afficher clairement :

- la marque actuellement modifiée, par exemple Amiga ou Atari ;
- la machine actuellement modifiée, par exemple Amiga 500 ou Atari ST.

Cet ajout ne concerne que la partie Émulation.

### Bouton de sauvegarde

- **Créer** si aucune configuration n’existe pour cette machine ;
- **Modifier** si elle existe déjà.

Aucun nom de profil supplémentaire n’est demandé.

## 3. Liste des configurations

La liste textuelle actuelle doit être remplacée par une présentation plus lisible :

- une liste permet de sélectionner la marque ;
- le tableau situé en dessous affiche les configurations enregistrées pour cette marque ;
- une ligne représente la configuration unique d’une machine.

Colonnes principales :

- **Machine** ;
- **RAM** ;
- **Lecteurs**, représentés par des icônes explicites ;
- **Périphériques**, avec des icônes pour le clavier, la souris, le joystick ou la manette configurés ;
- **Actions**.

Actions :

- **Modifier/Sélectionner** charge la configuration dans l’onglet approprié et y conduit directement l’utilisateur ;
- **Supprimer** ouvre une confirmation Oui/Non.

Cette page ne propose ni lancement de la machine ni nom de profil.

## 4. Aides contextuelles

Une petite icône d’information doit être placée immédiatement après le nom de chaque champ des onglets Amiga et Atari.

Au survol ou au clic, un petit panneau de type post-it doit :

- expliquer brièvement le rôle du champ ;
- employer un vocabulaire accessible sans connaissance préalable de l’émulation ;
- préciser les conséquences importantes d’un choix lorsque cela est utile ;
- être disponible dans toutes les langues prises en charge ;
- rester distinct des messages expliquant pourquoi une option est indisponible.

Le mécanisme doit être commun à tous les onglets.

## 5. Destination des ROM

La liste des ROM détectées affiche déjà leur identification et leur compatibilité. Ces informations doivent être conservées. Il faut seulement ajouter une colonne indiquant la destination ou le rôle de la ROM, par exemple :

- Kickstart ;
- ROM étendue ;
- ROM CD32 ;
- ROM CDTV ;
- TOS ;
- autre destination reconnue par l’émulateur concerné.

La destination doit provenir du résultat de détection déjà calculé. Pour Amiga comme pour Atari, cette information ne doit plus être perdue entre l’analyse du fichier et son affichage.

## 6. Associations et représentation du périphérique virtuel

### Disposition

La représentation du périphérique virtuel sélectionné doit se trouver à côté du tableau des associations, jamais en dessous. Elle doit rester visible et permettre une modification rapide.

La colonne **État** est réduite à la largeur de son icône. Le texte **Valide** disparaît afin de gagner de la place sans masquer l’information ; une infobulle peut expliquer l’icône.

### Périphériques à représenter

Pour la première version, seules les représentations des périphériques basiques déjà reconnus par les émulateurs sont nécessaires. La liste existe déjà dans l’application : elle doit être utilisée directement, sans reconstruire une liste de compatibilité.

Des représentations supplémentaires pourront être ajoutées plus tard.

Chaque périphérique pris en charge nécessite :

- une image adaptée ;
- des zones positionnées sur ses directions, boutons et commandes ;
- une surimpression permettant de mettre une zone en évidence lors d’un appui réel.

### Interaction

Un clic simple sur une commande du périphérique virtuel doit immédiatement :

1. sélectionner la ligne correspondante dans le tableau ;
2. activer la capture d’une nouvelle association pour cette ligne.

La capture accepte la prochaine entrée autorisée provenant du clavier, de la souris ou de n’importe quelle manette physique.

Il n’y a ni double-clic, ni bouton d’assignation supplémentaire sur le dessin, ni avertissement particulier.

Une modification ne change pas statiquement le dessin. Lorsqu’une entrée physique associée est réellement pressée, la zone correspondante du périphérique virtuel s’allume ou se colore.

### Suppression du choix de la manette physique

Le champ **Périphérique de la manette 1**, ainsi que son équivalent pour les autres ports, doit être retiré de cet écran.

Le choix du **type de périphérique virtuel** reste nécessaire : il indique à l’émulateur si le port contient un joystick Amiga, une manette CD32 ou un périphérique Atari donné.

Chaque association contient déjà sa source physique. L’utilisateur doit pouvoir capturer une entrée provenant de n’importe quelle manette sans sélectionner cette dernière au préalable.

Le routage d’exécution doit respecter ce modèle :

- le tableau en cours détermine le port virtuel ;
- chaque ligne détermine elle-même son périphérique physique et sa commande source ;
- les associations clavier et souris continuent de fonctionner de la même manière ;
- plusieurs ports ou joueurs ne nécessitent aucun sélecteur global de manette physique ;
- les anciennes configurations sans identifiant exploitable conservent un comportement de secours compatible.

Masquer uniquement le champ ne suffit pas : le moteur d’entrée doit cesser d’imposer une manette physique unique à toutes les associations d’un port.

### Nom et identifiant du périphérique

Quand une manette est connectée, son nom lisible doit être affiché dans les associations. Quand elle est déconnectée, son identifiant technique reste une valeur de secours. Ce comportement fonctionne déjà : l’identifiant visible sur la capture provenait d’une manette déconnectée et son nom est revenu après reconnexion et réouverture de l’onglet.

## Contraintes à préserver

- Ne pas introduire plusieurs profils par machine.
- Ne pas demander de nom de profil.
- Ne pas ajouter de lancement depuis la liste des configurations.
- Ne pas recréer les listes de compatibilité existantes.
- Ne pas remplacer les informations ROM existantes : ajouter uniquement leur destination.
- Ne pas afficher une manette physique à côté du tableau : le dessin représente le périphérique virtuel émulé.
- Ne pas exiger le choix préalable d’une manette physique.
- Ne pas supprimer l’identifiant de secours d’un périphérique déconnecté.
- Ne pas capturer la souris lors d’un clic sur une marge grise de l’écran d’émulation.