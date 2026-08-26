# Améliorations souhaitées pour l’interface d’émulation

## But du document

Ce document reprend uniquement les demandes et les idées formulées à partir des six images de l’interface.

Il distingue les demandes validées des pistes encore à étudier. La fin du document contient l’ordre général retenu et les checklists techniques détaillées des points 1 à 8.

## 1. Écran d’émulation

### Focus de l’écran

Dans l’onglet d’émulation actif, le focus doit toujours revenir à la fenêtre d’émulation après une action ponctuelle effectuée dans l’interface.

Cela concerne notamment :

- le chargement ou le changement d’une image de disquette ;
- l’extinction et l’allumage de la machine ;
- le reset logiciel ou matériel ;
- la sauvegarde d’un état ;
- le chargement d’un état ;
- le basculement entre la manette et la souris ;
- les autres commandes comparables de l’instance.

Pendant l’ouverture d’une boîte de dialogue, celle-ci conserve le focus. Une fois l’opération terminée et la boîte de dialogue fermée, le focus revient à l’écran de l’instance affichée dans l’onglet actif.

Un clic dans la zone grise autour de l’image doit également redonner le focus à l’émulation sans capturer la souris.

Les clics dans l’interface et les raccourcis de GW GUI doivent continuer à fonctionner.

### Filtres vidéo

Il faut établir une liste large des filtres existants, sans la limiter aux machines Amiga et Atari actuellement prises en charge. Cette recherche devra également servir aux futures machines ajoutées à GW GUI.

La recherche doit déterminer :

- ce qui est déjà proposé par Libretro ou par les émulateurs utilisés ;
- ce qui peut être réutilisé ou reproduit dans GW GUI ;
- ce qui devra être développé directement dans l’application.

Les collections de shaders Libretro peuvent servir de références pour reproduire des effets similaires dans GW GUI. Avant de reprendre directement le code d’un shader, il faudra vérifier sa licence. La manière de l’intégrer ou de reproduire son effet reste à étudier.

Les effets évoqués comprennent notamment les filtres CRT, les scanlines, les rendus LCD et le moiré horizontal et vertical. La liste exacte doit être établie pendant la recherche.

Les choix de signal vidéo déjà fournis par les émulateurs, tels que RGB, composite, S-Video, RF, PAL ou NTSC selon le moteur, restent des options de l’émulateur. Ils ne doivent pas être recréés ni dupliqués comme filtres propres à GW GUI. La possibilité de proposer plus tard un effet visuel inspiré d’un type de signal ne sera étudiée que comme un effet distinct et explicitement sélectionné.

Références de recherche :

- [Documentation des shaders Libretro](https://docs.libretro.com/guides/shaders/)
- [Collection officielle Slang](https://github.com/libretro/slang-shaders)
- [Collection Common Shaders](https://github.com/libretro/common-shaders)

### Organisation des filtres

Les filtres sont développés une seule fois et sont communs aux machines compatibles. Le filtre sélectionné et ses réglages sont cependant enregistrés séparément dans la configuration de chaque machine et utilisés par chacune de ses instances.

L’onglet **Vidéo** de la configuration Amiga ou Atari doit proposer :

- des présélections avec des réglages de base déjà choisis ;
- la possibilité de modifier les réglages ;
- l’enregistrement immédiat de chaque changement dans la configuration de la machine ;
- l’application immédiate du changement à l’instance correspondante lorsqu’elle fonctionne ;
- l’utilisation des réglages enregistrés au prochain démarrage si aucune instance ne fonctionne.

Les filtres et leurs réglages ne doivent pas être ajoutés ailleurs dans l’interface.

L’interface doit permettre de choisir une fonctionnalité dans une liste, puis d’afficher son panneau de configuration. Chaque fonctionnalité peut y être activée ou désactivée et, lorsqu’elle est active, ses réglages deviennent accessibles.

Il n’est pas nécessaire de transformer toute la page en groupes rigides. Les compatibilités restent gérées par familles logiques :

- les effets compatibles peuvent être combinés, par exemple un rendu CRT avec des scanlines ;
- lorsqu’une fonctionnalité activée est incompatible avec une ou plusieurs fonctionnalités déjà actives, l’application propose d’abord de les désactiver ;
- après confirmation, l’activation de la nouvelle fonctionnalité désactive automatiquement les fonctionnalités incompatibles ;
- sans confirmation, la combinaison actuelle reste inchangée.

Les réglages généraux de l’image — luminosité, contraste, gamma, saturation et netteté — restent toujours visibles indépendamment de la fonctionnalité sélectionnée. Leur valeur neutre est 0. Les réglages autres que le gamma utilisent une plage de -10 à +10. La représentation et la plage du gamma restent à définir en fonction de l’implémentation retenue.

La recherche devra établir la liste des groupes ainsi que les effets compatibles ou incompatibles entre eux.

### Séparation des réglages internes et externes à l’émulateur

Dans les onglets **Vidéo** et **Audio**, les réglages doivent être séparés visuellement entre :

- les options propres à l’émulateur ;
- les traitements réalisés par GW GUI sans demander de changement à l’émulateur.

Les traitements réalisés par GW GUI doivent pouvoir être modifiés en direct. La modification en direct des options internes de l’émulateur sera étudiée plus tard et ne fait pas partie de la demande actuelle.

Les intitulés des deux cadres ne sont pas encore choisis. Les formulations **Réglages de l’émulateur**, **Traitement vidéo par GW GUI** et **Traitement audio par GW GUI** ne sont pas retenues comme libellés définitifs.

### Idée future : habillages d’écran

Cette partie est une idée à conserver pour plus tard. Elle ne doit pas être réalisée maintenant.

Il faudra étudier la possibilité d’afficher un habillage en plein écran :

- une télévision ou un écran d’ordinateur pour les ordinateurs et consoles de salon ;
- le corps de la console pour une console portable.

Sans habillage, le plein écran classique actuel reste disponible.

Le choix de l’habillage est enregistré dans la configuration de la machine. Plusieurs habillages peuvent être proposés pour une même machine, notamment lorsque le matériel a connu plusieurs modèles ou plusieurs couleurs, par exemple Lynx et Lynx II, ou différentes couleurs de Game Boy Color.

Pour une console portable, les habillages représentent uniquement cette console et ses variantes. Il n’est pas obligatoire que tout le boîtier reste visible : l’habillage peut ne montrer que le contour de l’écran d’une Game Boy, d’une Lynx, d’une Game Gear ou d’une autre console portable. Une partie extérieure du boîtier peut donc être coupée, mais l’écran de la console doit rester entièrement visible, correctement placé et sans déformation.

Au départ, les habillages sont décoratifs. Une évolution ultérieure pourra éventuellement afficher les boutons pressés, les voyants allumés ou d’autres éléments animés.

L’utilisation des habillages en mode fenêtré reste à étudier. Les images nécessaires pourront être recherchées et rendues transparentes au besoin lorsque cette fonction sera effectivement abordée.

## 2. Identification de la machine modifiée

Il faut rendre plus évident ce que l’utilisateur est en train de modifier dans les options d’émulation.

### Machines possédant déjà une configuration

Dans la liste des machines, celles qui possèdent déjà une configuration doivent être distinguées par un fond d’une nuance de gris clair et un texte vert forêt en gras. Une icône autre qu’une coche peut également accompagner cette présentation ; son apparence reste à choisir.

Cette indication doit être visible :

- dans la liste déroulante lorsqu’elle est ouverte ;
- sur la machine sélectionnée lorsque la liste est fermée.

### Barre de titre

Lorsque l’utilisateur modifie une machine dans la partie **Émulation**, la barre de titre de la fenêtre doit indiquer la marque et la machine concernées. Cette indication reste visible dans tous les sous-onglets, contrairement au champ **Modèle**, qui n’est visible que dans l’onglet **Général** de la marque.

Cette information ne doit pas être ajoutée dans le libellé de l’onglet.

Le format retenu est **Options — Amiga : Amiga 500**, avec le format correspondant pour Atari, par exemple **Options — Atari : Atari ST**.

Dans l’onglet général **Configuration**, où aucune machine particulière n’est en cours de modification, la barre de titre reste simplement **Options**. Aucun titre **Options — Émulation — Configurations** ne doit être ajouté.

### Chargement d’une configuration existante

Lorsqu’une machine possédant déjà une configuration est sélectionnée, ses réglages enregistrés doivent être chargés dans l’onglet correspondant à cette machine, côté Amiga ou Atari.

### Création et enregistrement des changements

Le comportement dépend de l’existence de la configuration :

- si la configuration de la machine n’existe pas encore, les changements effectués dans l’interface ne sont pas enregistrés dans un fichier tant que l’utilisateur ne clique pas sur **Créer** ;
- si l’utilisateur sélectionne une autre machine avant de cliquer sur **Créer**, les valeurs non enregistrées sont abandonnées ;
- après la création, le bouton **Créer** disparaît immédiatement ;
- si la configuration de cette machine est ensuite supprimée depuis la liste des configurations, le bouton **Créer** réapparaît lorsque cette machine est sélectionnée ;
- aucune configuration existante n’affiche de bouton **Modifier**, puisque chaque changement est enregistré automatiquement ;
- aucun enregistrement automatique ne demande de confirmation.

Tous les champs modifiables d’une configuration existante doivent être couverts par l’enregistrement automatique. Il faudra vérifier chaque champ de l’ensemble des onglets Amiga et Atari.

Le moment de l’enregistrement dépend du contrôle :

- liste, case, bouton de choix, curseur ou autre sélecteur : dès que la valeur change ;
- champ de saisie : lorsque le champ perd le focus ;
- association de clavier, souris ou manette : dès que l’association est créée, remplacée, supprimée ou restaurée ;
- action modifiant plusieurs valeurs : toutes les valeurs concernées sont enregistrées à la fin de l’action.

Il n’est pas prévu de créer plusieurs configurations nommées pour une même machine ni d’ajouter un nom de profil.

## 3. Tableau des configurations

La présentation textuelle actuelle doit être remplacée par un tableau plus joli et plus facile à lire.

### Filtre par marque

Une liste déroulante permet de choisir la marque dont les configurations doivent être affichées. Elle contient uniquement les marques pour lesquelles au moins une configuration existe.

Si aucune marque ne possède de configuration, la liste déroulante est vide, le tableau est vide et rien n’est sélectionné.

Si la dernière configuration de la marque affichée est supprimée, aucune autre marque ne doit être sélectionnée automatiquement. La liste déroulante revient à un état sans sélection et le tableau devient vide jusqu’à ce que l’utilisateur choisisse lui-même une autre marque.

### Absence de sélection dans le tableau

Le tableau ne possède aucun mécanisme de sélection de ligne. Un simple clic sur une ligne ne la sélectionne pas et ne charge rien.

Les seules interactions disponibles sur une configuration sont :

- un double-clic sur sa ligne pour la modifier ;
- le bouton avec une icône de crayon pour la modifier ;
- le bouton avec une icône de poubelle pour demander sa suppression.

Cliquer ailleurs ne conserve ou ne crée aucune sélection.

### Informations affichées

Les configurations sont classées par ordre alphabétique du nom de la machine. Un système de tri supplémentaire n’est pas nécessaire, car le nombre de configurations par marque restera limité.

Une machine ne possédant qu’une seule configuration, son nom suffit à identifier la ligne. Aucun identifiant technique ne doit être affiché.

Les colonnes retenues sont :

- **Machine**, placée en premier ;
- **CPU** ;
- **RAM totale** ;
- **Lecteurs** ;
- **Périphériques** ;
- **Actions**.

Le moteur d’affichage vidéo, par exemple Direct3D 11, ainsi que les informations audio ne doivent pas être ajoutés au tableau. Il ne faut pas ajouter d’informations inutiles simplement pour remplir le tableau.

### Lecteurs

La colonne **Lecteurs** affiche une icône pour chaque lecteur configuré. Deux lecteurs sont représentés par deux icônes et non par une seule icône accompagnée du nombre deux.

Les icônes doivent distinguer les différents types de lecteurs, par exemple disquette, disque dur, CD, cassette ou cartouche selon les machines prises en charge.

### Périphériques

La colonne **Périphériques** utilise des icônes pour indiquer les périphériques configurés, notamment le clavier, la souris, les joysticks et les manettes.

Le nombre de joysticks ou de manettes configurés doit être visible en affichant le nombre correspondant d’icônes.

### Modification

Le bouton avec une icône de crayon et le double-clic sur la ligne déclenchent exactement la même action :

- ouvrir l’onglet **Général** de la marque correspondante ;
- afficher la machine concernée ;
- charger sa configuration complète afin de remplir les champs de tous les onglets de cette marque.

Cette page ne permet pas de lancer la machine.

### Suppression

Le bouton avec une icône de poubelle ouvre une boîte de dialogue Oui/Non avant toute suppression.

La boîte de dialogue affiche seulement le minimum permettant d’identifier sans ambiguïté la configuration supprimée : la marque et la machine. Elle ne doit pas devenir une fiche détaillée et ne doit afficher ni CPU, ni RAM, ni ROM, ni lecteurs, ni périphériques, ni réglages vidéo ou audio, ni identifiant technique.

Si la configuration supprimée était chargée dans l’éditeur de sa marque :

- les données correspondantes sont retirées de la mémoire ;
- les champs reviennent à leurs valeurs de base ;
- le bouton **Créer** réapparaît puisque cette machine ne possède plus de configuration.

Lorsque l’utilisateur ouvre l’onglet **Général** d’une marque, l’application relit les configurations existantes. La présence du bouton **Créer** dépend donc de l’existence réelle de la configuration de la machine affichée.

### Points restant à décider

- la présentation graphique définitive du tableau et de ses icônes.

## 4. Aides sur les champs

Une petite icône **(i)** doit être placée immédiatement après le nom de chaque champ dont le nom seul ne permet pas réellement de comprendre son rôle, ses choix ou leurs conséquences.

Cette aide concerne les champs, pas les boutons ni les titres de groupes.

L’icône doit toujours être visible. Sa taille normale constitue sa zone cliquable ; aucune zone invisible plus grande n’est demandée. L’infobulle au survol confirme que le pointeur se trouve bien sur l’icône.

### Aide rapide au survol

Lorsque la souris survole l’icône, une explication rapide apparaît.

Cette explication :

- tient sur une seule ligne ;
- indique simplement à quoi sert le champ ;
- disparaît lorsque le survol se termine ;
- ne contient pas de texte long ni de défilement.

### Aide détaillée au clic

Un clic sur l’icône ouvre une aide plus détaillée avec une présentation de type post-it.

Cette aide explique simplement ce que fait le champ, les choix disponibles et leurs différences utiles. Le texte reste court, clair et concis, sans longs paragraphes ni mise en forme de documentation. Un défilement n’est utilisé que si le contenu concis ne tient réellement pas dans le post-it.

Une fois le post-it ouvert, n’importe quelle touche du clavier ou un nouveau clic le ferme.

### Périmètre et traductions

Le système doit être utilisé pour les champs concernés dans les différents onglets Amiga et Atari.

Pour chaque champ portant une icône **(i)**, deux textes distincts — l’aide courte au survol et l’aide concise au clic — doivent être ajoutés aux ressources et traduits dans toutes les langues prises en charge par GW GUI. Aucun de ces textes ne doit être écrit directement dans le code.

## 5. Destination des ROM

Ajouter à la liste existante des ROM détectées une colonne indiquant dans quel champ la ROM sera placée pour la machine actuellement affichée. Cette destination doit réutiliser l’information de routage déjà employée par le bouton **Utiliser**, qui place déjà la ROM dans le bon champ ; elle ne doit pas être recalculée par une seconde logique.

Une ROM correspond à un seul champ pour cette machine. Les autres machines et leurs éventuelles utilisations ne doivent pas être prises en compte dans cette colonne.

### Affichage

La destination est affichée sous la forme d’un nom simple, dans le même style que la colonne indiquant la compatibilité.

Le texte doit reprendre directement le libellé déjà traduit du champ cible, par exemple **Kickstart** ou **ROM étendue**. Il ne faut pas créer une nouvelle traduction uniquement pour cette colonne.

La longueur affichée est limitée. Si le libellé est trop long, il est tronqué avec une ellipse. Le nombre maximal exact de caractères et la position de cette nouvelle colonne par rapport à la compatibilité restent à fixer avant l’implémentation ; aucune valeur ni position ne doit être choisie sans validation.

Si aucune destination ne peut être déterminée pour la machine affichée, la cellule reste vide.

### Comportement

Cette nouvelle colonne est uniquement informative. Elle ne modifie pas le fonctionnement actuel du bouton **Utiliser** ni les autres informations déjà affichées pour les ROM.

## 6. Associations des manettes et joysticks

### Disposition générale

Une représentation réaliste du périphérique émulé sélectionné doit être affichée à droite du tableau des associations.

Le tableau et le bloc réservé à cette représentation conservent leur disposition. Si la largeur disponible diminue, le tableau ne doit pas être réduit : seule l’image du périphérique est redimensionnée à l’intérieur de son bloc.

Lorsque le tableau défile verticalement, le bloc du périphérique reste fixe et visible afin de faciliter la définition des associations.

La colonne **État** du tableau doit être réduite à la largeur nécessaire pour conserver uniquement son icône. Le texte **Valide** est retiré afin de gagner de la place sans masquer l’information importante.

Les boutons **Assigner** du tableau restent disponibles.

### Ports émulés

Les ports sont déjà présentés dans des onglets distincts et un seul tableau de port est visible à la fois.

Le visuel affiché à droite correspond donc simplement au périphérique du port actuellement ouvert. Lorsque l’utilisateur change d’onglet de port, le tableau et le visuel correspondant à ce port sont affichés ensemble. Plusieurs représentations ne doivent pas être affichées simultanément.

### Périphériques à représenter

Pour commencer, il faut réaliser les images des périphériques basiques déjà reconnus par les émulateurs. La liste de ces périphériques existe déjà dans l’application et doit être utilisée directement.

Des représentations supplémentaires pourront être ajoutées plus tard.

Chaque représentation doit être :

- réaliste ;
- vue de face ;
- correctement réalisée, et non remplacée par un dessin générique de mauvaise qualité ;
- fournie avec un fond transparent ;
- accompagnée de zones de surimpression correctement placées sur ses directions, boutons et autres commandes ;
- accompagnée, au passage de la souris sur une zone cliquable, d’un petit halo ou d’un changement de couleur du halo permettant de voir immédiatement quelle commande peut être assignée.

### Réutilisation du système existant

Le système de représentation déjà utilisé dans l’onglet général **Manettes** doit être repris et adapté. Il ne faut pas en créer une copie indépendante pour les périphériques émulés.

Chaque image possède sa propre définition des positions, dimensions et formes de ses zones, puisque les commandes ne se trouvent pas au même endroit d’un périphérique à l’autre. Ces coordonnées propres à l’image sont exprimées en pourcentage par rapport à celle-ci afin de rester correctement alignées lorsque l’image est redimensionnée dans son bloc.

Les différences portent principalement sur les images utilisées, les commandes représentées et la taille disponible.

### Refonte de la surimpression commune

Le rendu général des surimpressions existantes doit être revu, car son apparence actuelle n’est pas satisfaisante, particulièrement pour les commandes analogiques.

Cette amélioration concerne le système commun afin que le nouvel affichage des périphériques émulés et l’affichage existant des manettes bénéficient du même rendu corrigé.

Le style général des halos et des zones blanches doit être revu conformément aux comportements décrits ci-dessous. Le fait qu’un halo puisse recouvrir une partie de l’image n’est pas considéré comme un problème à corriger, et aucun autre style de couleur, de bordure ou d’agrandissement n’est décidé dans ce document.

Pour un stick analogique de manette, la surimpression doit être ronde comme le stick représenté. Ce rond se déplace depuis la position centrale dans la même direction que le stick physique, avec un déplacement correspondant à son inclinaison. Il ne faut pas afficher de trait terminé par un point.

Les joysticks à manche et les gâchettes analogiques utilisent le même principe : un halo ancré au centre de la commande et dont la longueur augmente progressivement selon la valeur analogique reçue.

- pour un joystick à manche, le halo s’étire depuis le centre dans la direction du manche ;
- pour une gâchette analogique, le halo s’étire depuis le centre vers le bas selon la pression exercée.

La forme précise de ce halo commun reste à tester et à valider lors de sa réalisation.

### Seuil des commandes analogiques

Un seuil doit être appliqué avant de modifier la surimpression d’un stick, d’un joystick ou d’une gâchette analogique. Tant que la valeur reste sous ce seuil, le visuel conserve sa position neutre afin de ne pas afficher les petits mouvements parasites du périphérique.

Le pourcentage définitif n’est pas encore choisi. Il devra être testé avec plusieurs périphériques.

Lorsqu’une machine ou un port possède déjà un réglage de zone morte analogique, il faudra étudier la réutilisation de cette valeur pour que le visuel corresponde au comportement réel de l’entrée. Le visualiseur général, qui ne dépend pas de la configuration d’une machine, aura besoin d’une valeur par défaut commune.

Les autres rendus précis seront conçus et validés lorsque cette amélioration sera réalisée ; ils ne doivent pas être inventés dans le présent document.
### Visualisation des appuis

Lorsqu’une entrée physique déjà associée est utilisée, la commande correspondante doit être mise en évidence sur la représentation du périphérique émulé.

Tous les appuis simultanés doivent être représentés en même temps, quel que soit leur nombre et quelle que soit leur origine.

Pour une commande analogique, la surimpression suit les comportements définis dans la section **Refonte de la surimpression commune** : halo rond mobile pour un stick de manette et halo progressif ancré au centre pour un joystick à manche ou une gâchette.

Une modification d’association ne change pas de manière permanente la représentation. La mise en évidence sert à montrer les entrées reçues en direct.

### Modification depuis la représentation

Un clic sur une commande de la représentation doit :

1. sélectionner la ligne correspondante dans le tableau ;
2. activer immédiatement la capture d’une nouvelle association.

Il ne faut ni double-clic ni bouton supplémentaire sur la représentation. Les boutons **Assigner** existants dans le tableau sont toutefois conservés et déclenchent la même capture.

### Sources des associations

La capture ne doit exiger aucune sélection préalable du périphérique physique.

Une association peut provenir de n’importe quel périphérique d’entrée pris en charge par GW GUI, notamment :

- une manette ou un joystick physique ;
- le clavier ;
- la souris ;
- un trackball ;
- les autres périphériques d’entrée qui seront pris en charge.

Le champ permettant de choisir globalement une manette physique, comme **Périphérique de la manette 1**, ainsi que ses équivalents, doit être retiré de cet écran.

Cela ne concerne pas le choix du type de périphérique émulé, qui reste nécessaire.

### Nom affiché pour un périphérique déconnecté

Aucun changement n’est demandé concernant l’identifiant technique visible lorsqu’une manette est déconnectée. Après reconnexion et retour dans l’onglet, le nom de la manette est déjà correctement affiché.

## Points généraux restant à décider ou à étudier

- la présence et l’apparence éventuelle d’une icône accompagnant la couleur des machines déjà configurées ;
- la présentation graphique définitive du tableau des configurations et de ses icônes ;
- la liste complète des filtres vidéo, leurs groupes, leurs réglages et leur méthode technique de réalisation ;
- les aspects encore ouverts de l’idée future des habillages d’écran ;
- le pourcentage du seuil utilisé avant tout changement visuel analogique ;
- les périphériques supplémentaires à représenter après les modèles basiques.

## Ordre général de réalisation

Cet ordre de groupes est validé. Les checklists détaillées de chaque point devront respecter cet ordre global, même lorsque deux éléments appartiennent à une même section fonctionnelle du présent document.

1. Enregistrement automatique fiable de toutes les configurations.
2. Nouveau tableau des configurations et suppression correcte.
3. Retour automatique du focus à l’émulation.
4. Destination des ROM.
5. Aides contextuelles.
6. Réutilisation et amélioration du visualiseur de manettes.
7. Recherche et architecture des filtres vidéo.
8. Habillages d’écran, beaucoup plus tard.

## Règles de rédaction et de suivi des tâches

- Les groupes et les tâches sont écrits dans leur ordre réel d’exécution.
- Une sous-tâche d’action est cochée uniquement après l’écriture, la création, la modification, la copie ou le déplacement demandé et sa vérification.
- Une tâche finalisée est cochée lorsque toutes ses sous-tâches sont cochées. Le même principe s’applique ensuite en remontant jusqu’au groupe général.
- Une lecture, une recherche ou une réflexion n’est jamais une tâche isolée : elle fait partie d’une action qui produit ou modifie dans la même sous-tâche un fichier identifié.
- Lorsqu’un fichier doit être créé, sa création précède toujours l’ajout de son contenu.
- Toute modification indique le fichier concerné avant de décrire les changements à y effectuer.
- Un déplacement de code commence par le déplacement ou la copie du code existant en conservant exactement son fonctionnement. La suppression de l’ancien emplacement intervient seulement après vérification du déplacement. Toute modification fonctionnelle éventuelle constitue une tâche ultérieure séparée.
- Aucun comportement n’est modifié, corrigé ou remplacé par préférence personnelle. Une correction non prévue n’est effectuée que si une erreur réelle est constatée.

## Checklist détaillée — Point 1 : écran d’émulation

Cette checklist détaille uniquement le retour du focus du point 1. Dans l’ordre global, ce travail correspond au groupe 3. Les filtres vidéo et les habillages sont détaillés séparément dans les checklists des points 7 et 8 afin de ne pas dupliquer leurs tâches ici.

- [ ] Retour automatique du focus vers l’instance d’émulation ouverte
  - [ ] Centraliser la restitution du focus sans modifier la capture de la souris
    - [ ] Fournir une opération unique de restitution du focus
      - [ ] Modifier src/GWGUI.App/Controllers/Emulation/Machine/MachineInputController.cs pour ajouter une méthode de focus qui utilise la vue et le handle d’entrée actuellement actifs.
      - [ ] Dans src/GWGUI.App/Controllers/Emulation/Machine/MachineInputController.cs, garantir que cette méthode ne déclenche jamais la capture relative de la souris et ne modifie pas l’état de capture existant.
      - [ ] Dans src/GWGUI.App/Controllers/Emulation/Machine/MachineInputController.cs, conserver la mise à jour de la cible après un changement de surface vidéo afin que la restitution vise toujours la surface courante.
    - [ ] Raccorder le clic dans la zone grise à cette opération
      - [ ] Modifier src/GWGUI.App/Controllers/Emulation/Machine/MachineInputController.cs pour écouter le clic sur MachineView.DisplayHost et appeler la restitution du focus uniquement lorsque le clic se trouve hors de MachineView.Screen.
      - [ ] Modifier src/GWGUI.App/Controllers/Emulation/Machine/MachineInputController.cs pour détacher ce gestionnaire dans Dispose et éviter qu’une instance fermée conserve un abonnement.
      - [ ] Conserver dans src/GWGUI.App/Controllers/Emulation/Machine/MachineInputController.cs le comportement actuel d’un clic dans l’écran, y compris la capture uniquement lorsqu’elle est déjà autorisée par le mode d’entrée.
  - [ ] Restituer le focus après toutes les commandes ponctuelles de l’instance
    - [ ] Raccorder les commandes de la barre d’outils
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/MachineCommandBar.cs pour recevoir l’opération commune de restitution du focus.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/MachineCommandBar.cs pour exécuter cette opération en fin de commande, que la commande réussisse ou produise une erreur gérée.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs pour fournir à MachineCommandBar l’opération de focus de MachineInputController sans créer une seconde logique de focus.
    - [ ] Couvrir les commandes actuellement exposées par MachineCommandActions
      - [ ] Vérifier et ajuster dans src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs le chemin commun utilisé après allumage ou extinction, pause ou reprise, reset logiciel, reset matériel, sauvegarde rapide, chargement rapide, capture d’écran, plein écran ou mode fenêtré, activation ou coupure audio et basculement manette-souris.
      - [ ] Conserver dans src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs la priorité des raccourcis de GW GUI et ne pas ajouter de capture automatique après leur exécution.
    - [ ] Couvrir le chargement, le remplacement et l’éjection des médias
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/MachineView.cs pour transmettre l’opération commune de restitution du focus aux commandes des lecteurs sans dupliquer le traitement des erreurs.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs afin que InsertMediaAsync rende le focus après la fermeture de la boîte de sélection, y compris après Annuler, une réussite ou une erreur gérée.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs afin que EjectMediaAsync rende le focus après l’éjection et la reconstruction de la barre des lecteurs.
    - [ ] Préserver le focus pendant les transitions qui doivent le posséder
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs pour ne rendre le focus à l’émulation qu’après la fermeture d’une boîte de dialogue et non pendant son affichage.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs pour conserver la séquence BeginHostTransition, déplacement de l’écran, CompleteHostTransition lors du passage plein écran ou fenêtré, puis appliquer la même opération commune de focus à la surface replacée.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs pour viser uniquement l’instance contenue dans l’onglet actif et ne jamais donner le focus à une autre machine ouverte.
  - [ ] Verrouiller le comportement par des tests
    - [ ] Créer le fichier de tests du focus
      - [ ] Créer tests/GWGUI.Tests/MachineFocusTests.cs avant d’y ajouter le moindre scénario.
    - [ ] Ajouter les scénarios de restitution du focus
      - [ ] Modifier tests/GWGUI.Tests/MachineFocusTests.cs pour vérifier qu’un clic dans la zone grise redonne le focus sans capturer la souris.
      - [ ] Modifier tests/GWGUI.Tests/MachineFocusTests.cs pour vérifier qu’un clic dans l’écran conserve le comportement de capture existant.
      - [ ] Modifier tests/GWGUI.Tests/MachineFocusTests.cs pour vérifier la restitution après réussite et après erreur gérée d’une commande de barre d’outils.
      - [ ] Modifier tests/GWGUI.Tests/MachineFocusTests.cs pour vérifier la restitution après fermeture ou annulation d’une commande de média.
      - [ ] Modifier tests/GWGUI.Tests/MachineFocusTests.cs pour vérifier que seule l’instance de l’onglet actif reçoit le focus.
      - [ ] Exécuter les tests ajoutés et la suite GWGUI.Tests, puis corriger uniquement les régressions provoquées par les modifications du focus dans les fichiers déjà cités.

## Checklist détaillée — Point 2 : identification et enregistrement de la machine modifiée

Dans l’ordre général, l’enregistrement automatique de ce point constitue le groupe 1. Son fonctionnement fiable doit donc être réalisé avant le tableau des configurations du point 3. La présentation du sélecteur et la barre de titre sont réalisées après la stabilisation de l’état enregistré ou temporaire dont elles dépendent.

- [ ] Enregistrement automatique fiable des configurations Amiga et Atari
  - [ ] Distinguer une configuration enregistrée d’un brouillon non créé
    - [ ] Centraliser l’état de la machine éditée
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour déterminer l’existence d’une configuration à partir de _saved, du module et de MachineId, sans déduire cet état de la présence visuelle d’un bouton.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour conserver séparément la configuration actuellement affichée et son état enregistré ou temporaire pendant toute la reconstruction des sous-onglets.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour empêcher tout gestionnaire de changement déclenché pendant ReloadAsync, SelectMachine ou RebuildEditor de lancer une sauvegarde.
    - [ ] Charger la bonne configuration lors du choix d’une machine
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour charger depuis _saved l’unique configuration correspondant à la machine sélectionnée lorsqu’elle existe.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour créer avec IEmulationModule.CreateConfiguration un nouveau brouillon aux valeurs de base lorsqu’aucune configuration n’existe.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour abandonner le brouillon précédent lors d’un changement de machine et ne reporter aucune de ses valeurs sur le nouveau brouillon.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour reconstruire tous les sous-onglets à partir de la configuration chargée et supprimer toute valeur visuelle ou donnée interne provenant de la machine précédente.
  - [ ] Remplacer le bouton Enregistrer par le bouton Créer réservé aux brouillons
    - [ ] Ajouter la ressource Common.Create avant de l’utiliser
      - [ ] Modifier src/GWGUI.App/Resources/00-Base/Actions.resx pour créer la clé Common.Create.
      - [ ] Modifier src/GWGUI.App/Resources/ar-SA/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/cs-CZ/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/da-DK/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/de-DE/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/el-GR/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/en-US/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/es-ES/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/fi-FI/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/fr-FR/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/he-IL/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/hu-HU/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/id-ID/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/it-IT/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/ja-JP/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/ko-KR/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/nb-NO/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/nl-NL/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/pl-PL/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/pt-BR/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/pt-PT/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/ro-RO/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/ru-RU/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/sv-SE/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/th-TH/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/tr-TR/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/uk-UA/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/vi-VN/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/zh-Hans/Actions.resx.
      - [ ] Modifier src/GWGUI.App/Resources/zh-Hant/Actions.resx.
    - [ ] Afficher et exécuter Créer uniquement pour une machine non enregistrée
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour conserver une référence au bouton construit par BuildGeneralHeader et lui affecter Common.Create.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour afficher ce bouton uniquement lorsque la machine sélectionnée ne possède aucune configuration enregistrée.
      - [ ] Renommer SaveAsync en CreateConfigurationAsync dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs et limiter cette méthode à la création du brouillon actuellement affiché.
      - [ ] Modifier CreateConfigurationAsync dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour capturer ensemble les champs génériques, le firmware, le stockage et les entrées avant le premier appel à IEmulationModule.SaveConfigurationAsync.
      - [ ] Modifier CreateConfigurationAsync dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour ajouter la configuration créée à _saved, signaler ConfigurationSaved, reconstruire l’état visuel et faire disparaître Créer immédiatement après une réussite.
      - [ ] Modifier CreateConfigurationAsync dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour conserver le brouillon et le bouton Créer si l’écriture échoue, sans annoncer la configuration comme existante.
  - [ ] Créer un seul chemin sérialisé d’enregistrement automatique
    - [ ] Remplacer les sauvegardes particulières par une sauvegarde commune
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour remplacer _saveInputGate par un verrou commun à toutes les sauvegardes de la configuration affichée.
      - [ ] Créer dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs une méthode PersistExistingConfigurationAsync qui ne fait aucune écriture lorsque la configuration est encore un brouillon.
      - [ ] Modifier PersistExistingConfigurationAsync dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour capturer les champs génériques, appliquer les entrées, appliquer le stockage, enregistrer par IEmulationModule.SaveConfigurationAsync, remplacer la version correspondante dans _saved et signaler ConfigurationSaved.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour sérialiser les demandes simultanées et regrouper celles produites par une même action, tout en garantissant que la dernière valeur affichée est celle écrite dans le fichier.
      - [ ] Supprimer PersistInputSettingsAsync de src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs seulement après avoir raccordé EmulationInputSettingsController.SettingsChanged à PersistExistingConfigurationAsync avec le même comportement fonctionnel.
      - [ ] Conserver la présentation d’erreur actuelle dans ExecuteAsync et modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour ne mettre à jour _saved et ne signaler ConfigurationSaved qu’après une écriture réussie.
  - [ ] Raccorder chaque type de champ générique au moment de sauvegarde prévu
    - [ ] Enregistrer les sélecteurs et cases dès leur changement
      - [ ] Modifier CreateSelection dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler PersistExistingConfigurationAsync à chaque changement utilisateur, y compris lorsque RefreshSettingsOnChange reconstruit les champs dépendants.
      - [ ] Modifier CreateToggle dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler PersistExistingConfigurationAsync après le changement et après l’application des règles dépendantes.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs pour appliquer complètement les règles mutuellement exclusives avant de déclencher une seule sauvegarde contenant toutes les valeurs modifiées par l’action.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs pour conserver le recalcul immédiat de la RAM totale sans provoquer une écriture supplémentaire.
    - [ ] Enregistrer les champs de saisie à la perte de focus
      - [ ] Modifier CreateField dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour raccorder les éditeurs Text, Number et Percentage à PersistExistingConfigurationAsync lors de LostKeyboardFocus.
      - [ ] Modifier CreatePath dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour sauvegarder à la perte de focus et après la sélection réussie d’un fichier par Parcourir.
      - [ ] Modifier CreateDirectoryPath dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour sauvegarder à la perte de focus et après la sélection réussie d’un dossier par Parcourir.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour ne lancer aucune sauvegarde lorsque Parcourir est annulé ou lorsque la valeur finale n’a pas changé.
  - [ ] Raccorder les composants spécialisés à la même sauvegarde
    - [ ] Enregistrer les changements de firmware
      - [ ] Ajouter un événement ConfigurationChanged dans src/GWGUI.App/Controllers/Emulation/Firmware/EmulationFirmwareManagementController.cs et le déclencher une seule fois après l’application réussie de Utiliser.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour raccorder cet événement à PersistExistingConfigurationAsync après SetConfiguration, sans écrire un brouillon non créé.
    - [ ] Enregistrer les changements de stockage
      - [ ] Ajouter un événement SettingsChanged dans src/GWGUI.App/Controllers/Emulation/Storage/EmulationStorageSettingsController.cs.
      - [ ] Modifier src/GWGUI.App/Controllers/Emulation/Storage/EmulationStorageSettingsController.cs pour déclencher SettingsChanged une seule fois après l’ajout, la suppression ou la configuration validée d’un lecteur ou de son média.
      - [ ] Modifier src/GWGUI.App/Controllers/Emulation/Storage/EmulationStorageSettingsController.cs pour ne rien signaler après l’annulation d’une boîte de dialogue ou une action qui ne change aucune donnée.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appliquer le stockage puis appeler PersistExistingConfigurationAsync lorsqu’il signale une modification.
    - [ ] Enregistrer les changements de clavier, souris et manettes
      - [ ] Modifier src/GWGUI.App/Controllers/Emulation/Input/EmulationInputSettingsController.cs pour faire remonter par SettingsChanged le changement du type de périphérique émulé, de la manette physique actuellement affichée et de toute association, sans créer une écriture distincte par ligne lors d’une restauration globale.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appliquer l’ensemble des entrées puis appeler PersistExistingConfigurationAsync, uniquement si la configuration existe déjà.
  - [ ] Synchroniser l’éditeur lorsque l’existence de la configuration change ailleurs
    - [ ] Recharger l’état après création ou suppression
      - [ ] Modifier ReloadAsync dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour actualiser _saved sans remplacer la machine actuellement affichée par une autre configuration.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour demander au module déjà ouvert de recharger son état après la suppression d’une configuration de ce module, sans provoquer ce rechargement après chaque enregistrement automatique.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour vider la configuration supprimée de la mémoire, reconstruire les valeurs de base de la même machine et faire réapparaître Créer.
  - [ ] Verrouiller la création et l’enregistrement automatique par des tests
    - [ ] Créer le fichier de tests avant ses scénarios
      - [ ] Créer tests/GWGUI.Tests/EmulationModuleSettingsAutoSaveTests.cs.
    - [ ] Ajouter les scénarios des brouillons et des configurations existantes
      - [ ] Modifier tests/GWGUI.Tests/EmulationModuleSettingsAutoSaveTests.cs pour vérifier qu’aucun changement générique, firmware, stockage ou entrée n’écrit de fichier avant Créer.
      - [ ] Modifier tests/GWGUI.Tests/EmulationModuleSettingsAutoSaveTests.cs pour vérifier que changer de machine abandonne toutes les valeurs du brouillon précédent.
      - [ ] Modifier tests/GWGUI.Tests/EmulationModuleSettingsAutoSaveTests.cs pour vérifier que Créer enregistre tous les sous-onglets, disparaît après réussite et reste visible après échec.
      - [ ] Modifier tests/GWGUI.Tests/EmulationModuleSettingsAutoSaveTests.cs pour vérifier les sauvegardes des sélecteurs, cases, champs à perte de focus, chemins, firmware, stockage et associations d’entrée.
      - [ ] Modifier tests/GWGUI.Tests/EmulationModuleSettingsAutoSaveTests.cs pour vérifier qu’une action modifiant plusieurs valeurs produit une configuration finale complète sans écriture intermédiaire incomplète.
      - [ ] Modifier tests/GWGUI.Tests/EmulationModuleSettingsAutoSaveTests.cs pour vérifier qu’une suppression externe vide l’état chargé et fait réapparaître Créer pour la même machine.
      - [ ] Exécuter EmulationModuleSettingsAutoSaveTests et la suite GWGUI.Tests, puis corriger uniquement les régressions provoquées par les fichiers modifiés dans ce groupe.

- [ ] Identification visuelle des machines possédant déjà une configuration
  - [ ] Porter l’état enregistré dans chaque élément du sélecteur
    - [ ] Étendre le contrat de présentation avant de modifier son apparence
      - [ ] Modifier src/GWGUI.App/Contracts/Emulation/Machine/EmulationMachineChoice.cs pour ajouter HasSavedConfiguration sans modifier Definition, DisplayName ni la valeur retournée par ToString.
      - [ ] Modifier la création des choix dans le constructeur, ReloadAsync et RefreshLocalizedContent de src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour calculer HasSavedConfiguration à partir de _saved.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour reconstruire les choix après une création ou une suppression tout en conservant la même machine affichée.
  - [ ] Appliquer la même présentation dans la liste ouverte et le champ fermé
    - [ ] Créer le générateur de présentation avant de l’utiliser
      - [ ] Créer src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationMachineChoiceTemplateFunctions.cs.
    - [ ] Construire la présentation validée des machines configurées
      - [ ] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationMachineChoiceTemplateFunctions.cs pour afficher normalement une machine non configurée et appliquer aux machines configurées un fond gris clair, un texte vert forêt et une graisse forte.
      - [ ] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationMachineChoiceTemplateFunctions.cs pour utiliser le même contenu redimensionnable dans la liste déroulante ouverte et dans la zone fermée du ComboBox.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour affecter ce modèle de présentation à _machines sans ajouter l’état au texte ni au libellé de l’onglet.
  - [ ] Verrouiller l’affichage du sélecteur par des tests
    - [ ] Créer le fichier de tests avant ses scénarios
      - [ ] Créer tests/GWGUI.Tests/EmulationMachineChoicePresentationTests.cs.
    - [ ] Ajouter les scénarios ouvert, fermé, créé et supprimé
      - [ ] Modifier tests/GWGUI.Tests/EmulationMachineChoicePresentationTests.cs pour vérifier le style normal d’un brouillon et le fond gris clair avec texte vert forêt en gras d’une configuration existante.
      - [ ] Modifier tests/GWGUI.Tests/EmulationMachineChoicePresentationTests.cs pour vérifier que le rendu configuré est identique lorsque le sélecteur est ouvert ou fermé.
      - [ ] Modifier tests/GWGUI.Tests/EmulationMachineChoicePresentationTests.cs pour vérifier le changement immédiat d’apparence après Créer et le retour à l’apparence normale après suppression.
      - [ ] Exécuter EmulationMachineChoicePresentationTests et corriger uniquement les régressions provenant de cette présentation.

- [ ] Barre de titre indiquant la marque et la machine réellement modifiées
  - [ ] Faire remonter le contexte d’édition jusqu’à la fenêtre Options
    - [ ] Créer le contrat du contexte avant les événements qui l’utilisent
      - [ ] Créer src/GWGUI.App/Contracts/Emulation/Machine/EmulationMachineEditingContext.cs avec le nom localisé de la marque et le nom localisé de la machine.
    - [ ] Signaler le contexte depuis l’éditeur de marque
      - [ ] Ajouter un événement EditingContextChanged dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs et l’émettre après ReloadAsync, chaque changement de machine et RefreshLocalizedContent.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour construire le contexte avec le libellé localisé du module et DisplayName de la machine, indépendamment du sous-onglet Général, CPU, RAM, ROM, Vidéo, Audio, Stockage, Clavier, Souris ou Manettes affiché.
    - [ ] Transmettre uniquement le contexte de l’onglet de marque actif
      - [ ] Ajouter un événement EditingContextChanged dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs et y transmettre le contexte de la section Amiga ou Atari actuellement ouverte.
      - [ ] Modifier ModuleTabSelectionChanged dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour publier un contexte vide dans Général, Raccourcis ou Configuration, et le contexte courant uniquement dans un onglet de marque.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour ignorer les événements provenant d’une section de marque qui n’est plus l’onglet actif.
  - [ ] Mettre à jour le titre sans modifier les onglets
    - [ ] Raccorder la navigation générale de la fenêtre
      - [ ] Modifier src/GWGUI.App/Views/Windows/Options/OptionsWindow.xaml pour raccorder SelectionChanged du TabControl Navigation au recalcul du titre.
      - [ ] Modifier src/GWGUI.App/Views/Windows/Options/OptionsWindow.xaml.cs pour écouter OptionsEmulationSection.EditingContextChanged et mémoriser le contexte actif.
      - [ ] Modifier src/GWGUI.App/Views/Windows/Options/OptionsWindow.xaml.cs pour afficher le titre localisé Options seul lorsque l’onglet principal n’est pas Émulation, lorsque Général, Raccourcis ou Configuration est ouvert, ou lorsqu’aucun contexte de machine n’existe.
      - [ ] Modifier src/GWGUI.App/Views/Windows/Options/OptionsWindow.xaml.cs pour construire Options — Amiga : Amiga 500, ou son équivalent Atari, à partir du titre Options et des deux noms localisés existants lorsque l’éditeur d’une machine est actif.
      - [ ] Modifier RefreshLocalizedContent dans src/GWGUI.App/Views/Windows/Options/OptionsWindow.xaml.cs pour recalculer immédiatement ce titre après un changement de langue sans modifier les libellés des TabItem dans src/GWGUI.App/Views/Windows/Options/OptionsWindow.xaml, src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs ou src/GWGUI.App/Functions/Views/Emulation/Machine/EmulationMachineTabs.cs.
  - [ ] Verrouiller le titre par des tests
    - [ ] Créer le fichier de tests avant ses scénarios
      - [ ] Créer tests/GWGUI.Tests/OptionsWindowEmulationTitleTests.cs.
    - [ ] Ajouter les scénarios de navigation et de localisation
      - [ ] Modifier tests/GWGUI.Tests/OptionsWindowEmulationTitleTests.cs pour vérifier Options dans tous les onglets principaux autres qu’Émulation et dans Général, Raccourcis ou Configuration de la partie Émulation.
      - [ ] Modifier tests/GWGUI.Tests/OptionsWindowEmulationTitleTests.cs pour vérifier le format Options — Amiga : Amiga 500 et le format Atari correspondant dans tous les sous-onglets d’une machine.
      - [ ] Modifier tests/GWGUI.Tests/OptionsWindowEmulationTitleTests.cs pour vérifier la mise à jour lors du changement de machine, de marque et de langue sans ajouter ces informations au texte des onglets.
      - [ ] Exécuter OptionsWindowEmulationTitleTests et la suite GWGUI.Tests, puis corriger uniquement les régressions provoquées par les fichiers du titre.
## Checklist détaillée — Point 3 : tableau des configurations

Dans l’ordre général, ce point constitue le groupe 2. Il utilise l’état fiable des configurations établi au point 2 et doit être terminé avant le retour automatique du focus du point 1. Le tableau n’a aucun état de sélection : seules la marque du filtre, l’action crayon, le double-clic et l’action poubelle produisent un changement.

- [ ] Données structurées des lignes de configuration
  - [ ] Déplacer les fonctions de présentation déjà utilisées avant de les réemployer
    - [ ] Créer le fichier commun avant d’y copier les fonctions
      - [ ] Créer src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationSettingsValuePresentationFunctions.cs.
    - [ ] Copier les fonctions sans changer leur résultat
      - [ ] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationSettingsValuePresentationFunctions.cs pour y copier DisplayValue, DefaultNumericValue et FormatMemorySize depuis src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs avec exactement les mêmes formats, valeurs de repli et règles de localisation.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs pour appeler les fonctions copiées tout en laissant provisoirement les anciennes fonctions privées en place.
      - [ ] Exécuter les tests matériels et la suite GWGUI.Tests afin de vérifier que les onglets CPU et RAM affichent strictement les mêmes valeurs après le raccordement.
      - [ ] Supprimer DisplayValue, DefaultNumericValue et FormatMemorySize de src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs uniquement après la vérification du comportement déplacé.
  - [ ] Centraliser les glyphes déjà utilisés avant de les afficher dans le tableau
    - [ ] Déplacer le glyphe du clavier sans changer les onglets existants
      - [ ] Modifier src/GWGUI.App/Constants/Emulation/EmulationInputSettingsConstants.cs pour ajouter KeyboardIcon avec la valeur actuelle du glyphe Clavier.
      - [ ] Modifier src/GWGUI.App/Constants/Emulation/EmulationMachineTabConstants.cs pour remplacer uniquement la valeur littérale du glyphe Clavier par EmulationInputSettingsConstants.KeyboardIcon.
      - [ ] Exécuter OptionsControllersTabPlacementTests et les tests visuels existants avant de réutiliser KeyboardIcon dans le tableau.
    - [ ] Ajouter le glyphe d’action manquant
      - [ ] Modifier src/GWGUI.App/Constants/Controls/Visual/ControlVisualConstants.cs pour ajouter EditGlyph destiné au bouton crayon, sans modifier DeleteGlyph ni les boutons qui l’utilisent déjà.
  - [ ] Créer le contrat propre au tableau sans modifier la liste utilisée pour lancer les machines
    - [ ] Créer le fichier de ligne avant le présentateur
      - [ ] Créer src/GWGUI.App/Contracts/Emulation/Configurations/EmulationConfigurationTableRow.cs.
    - [ ] Définir uniquement les données des colonnes validées
      - [ ] Modifier src/GWGUI.App/Contracts/Emulation/Configurations/EmulationConfigurationTableRow.cs pour porter le module, la configuration, le nom localisé de la machine, le CPU, la RAM totale, les icônes de lecteurs et les icônes de périphériques.
      - [ ] Modifier src/GWGUI.App/Contracts/Emulation/Configurations/EmulationConfigurationTableRow.cs pour définir un élément d’icône contenant le glyphe et son nom accessible, afin de répéter réellement une icône par lecteur ou périphérique, sans modifier src/GWGUI.App/Contracts/Emulation/Configurations/EmulationConfigurationListItem.cs utilisé pour lancer les machines.
  - [ ] Construire les lignes à partir des contrats structurés existants
    - [ ] Créer le présentateur avant de remplir le tableau
      - [ ] Créer src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs.
    - [ ] Produire Machine, CPU et RAM totale sans analyser le résumé textuel
      - [ ] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour obtenir le nom localisé de la machine depuis sa définition et sa clé de ressource existantes.
      - [ ] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour obtenir le CPU depuis le champ structuré de l’onglet CPU renvoyé par IEmulationModule.Describe et le formater avec EmulationSettingsValuePresentationFunctions.
      - [ ] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour additionner les valeurs numériques structurées des champs RAM visibles et formater le total avec EmulationSettingsValuePresentationFunctions.
      - [ ] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour laisser CPU ou RAM vide uniquement lorsque le module ne fournit réellement aucune donnée correspondante.
    - [ ] Produire une icône par lecteur configuré
      - [ ] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour appeler IEmulationStorageSettingsManager.DescribeStorageSettings lorsque le module fournit ce service.
      - [ ] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour parcourir ConfiguredSlots, retrouver chaque périphérique dans AvailableDevices et produire une icône pour chaque occurrence configurée.
      - [ ] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour réutiliser les glyphes disquette, disque dur, CD, cassette et cartouche de src/GWGUI.App/Constants/Machine/MachinePresentationConstants.cs selon EmulationMediaType.
    - [ ] Produire une icône par périphérique d’entrée existant ou défini
      - [ ] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour appeler IEmulationInputSettingsManager.DescribeInputSettings lorsque le module fournit ce service.
      - [ ] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour produire une icône Clavier lorsque Keyboard existe et une icône Souris lorsque Mouse existe.
      - [ ] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour produire une icône distincte pour chaque port dont SelectedControllerId n’est pas None, en utilisant le glyphe clavier ou souris lorsque ce type est explicitement sélectionné et le glyphe manette pour les autres types.
      - [ ] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour utiliser le libellé déjà traduit du type sélectionné comme nom accessible de l’icône, sans ajouter d’infobulle de port.
    - [ ] Exclure les informations refusées et classer les lignes
      - [ ] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour ne produire aucun identifiant technique, ROM, moteur vidéo, format vidéo, état audio ou bouton de lancement.
      - [ ] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour classer les lignes par nom de machine avec la comparaison alphabétique de la langue affichée.
  - [ ] Verrouiller la production des lignes par des tests
    - [ ] Créer le fichier de tests avant ses scénarios
      - [ ] Créer tests/GWGUI.Tests/EmulationConfigurationTablePresenterTests.cs.
    - [ ] Ajouter les scénarios des colonnes structurées
      - [ ] Modifier tests/GWGUI.Tests/EmulationConfigurationTablePresenterTests.cs pour vérifier le nom, le CPU et la RAM totale des configurations Amiga et Atari.
      - [ ] Modifier tests/GWGUI.Tests/EmulationConfigurationTablePresenterTests.cs pour vérifier qu’un lecteur configuré produit exactement une icône et que deux lecteurs du même type produisent deux icônes.
      - [ ] Modifier tests/GWGUI.Tests/EmulationConfigurationTablePresenterTests.cs pour vérifier les icônes clavier, souris et chaque port manette ou joystick configuré.
      - [ ] Modifier tests/GWGUI.Tests/EmulationConfigurationTablePresenterTests.cs pour vérifier l’absence d’identifiant, ROM, vidéo, audio et action de lancement dans le contrat de ligne.
      - [ ] Modifier tests/GWGUI.Tests/EmulationConfigurationTablePresenterTests.cs pour vérifier le classement alphabétique localisé des machines.
      - [ ] Exécuter EmulationConfigurationTablePresenterTests et la suite GWGUI.Tests, puis corriger uniquement les régressions provoquées par les données du tableau.

- [ ] Tableau non sélectionnable et filtre par marque
  - [ ] Ajouter les traductions propres au tableau avant de construire ses en-têtes
    - [ ] Créer les clés communes dans la ressource de base
      - [ ] Modifier src/GWGUI.App/Resources/00-Base/Emulation.resx pour créer Emulation.Configuration.Brand, Emulation.Configuration.Readers, Emulation.Configuration.Peripherals et Emulation.Configuration.DeleteConfirm.
    - [ ] Traduire les quatre clés dans toutes les langues existantes
      - [ ] Modifier src/GWGUI.App/Resources/ar-SA/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/cs-CZ/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/da-DK/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/de-DE/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/el-GR/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/en-US/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/es-ES/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/fi-FI/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/fr-FR/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/he-IL/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/hu-HU/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/id-ID/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/it-IT/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/ja-JP/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/ko-KR/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/nb-NO/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/nl-NL/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/pl-PL/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/pt-BR/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/pt-PT/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/ro-RO/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/ru-RU/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/sv-SE/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/th-TH/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/tr-TR/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/uk-UA/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/vi-VN/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/zh-Hans/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/zh-Hant/Emulation.resx.
  - [ ] Créer un véritable tableau sans modèle de sélection de ligne
    - [ ] Créer le contrôle avant de remplacer la liste existante
      - [ ] Créer src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs.
    - [ ] Construire les en-têtes et les lignes avec les styles existants
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour créer les colonnes Machine, CPU, RAM totale, Lecteurs, Périphériques et Actions dans cet ordre.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour utiliser un ItemsControl dans un ScrollViewer et ne créer ni SelectedItem, ni SelectedIndex, ni état visuel de ligne sélectionnée.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour utiliser les ressources TableHeaderText, CardBrush et BorderBrush existantes afin d’obtenir des en-têtes et séparations cohérents avec les autres tableaux.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour afficher chaque liste d’icônes dans la cellule correspondante sans ajouter de nombre, de texte permanent ou d’infobulle de port.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour créer dans Actions un bouton crayon utilisant Common.Modify et un bouton poubelle utilisant Common.Delete.
    - [ ] Exposer uniquement les actions autorisées par une ligne
      - [ ] Ajouter EditRequested et DeleteRequested dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs en transmettant directement EmulationConfigurationTableRow.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour déclencher EditRequested sur un double-clic de ligne ou sur le bouton crayon avec exactement le même chemin interne.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour empêcher un double-clic sur le bouton poubelle de remonter jusqu’à l’action Modifier.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour ne déclencher aucune action lors d’un simple clic ailleurs dans la ligne.
  - [ ] Ajouter le filtre contenant uniquement les marques configurées
    - [ ] Préparer les champs de la section avant le nouveau contenu
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour ajouter un ComboBox de marque, une collection de EmulationModuleListItem et une instance de EmulationConfigurationTable sans retirer encore _configurationList ni _removeConfiguration.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour raccorder SelectionChanged du ComboBox à une méthode qui filtre les lignes sans sélectionner une ligne du tableau.
    - [ ] Construire la page Configuration avec le filtre et le nouveau tableau
      - [ ] Modifier BuildConfigurationsTab dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionLayoutFunctions.cs pour placer le libellé Marque et son ComboBox au-dessus du nouveau tableau.
      - [ ] Modifier BuildConfigurationsTab dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionLayoutFunctions.cs pour ajouter le nouveau tableau tout en laissant provisoirement l’ancienne liste hors de la zone visible jusqu’à la validation des nouvelles actions.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour construire les lignes par EmulationConfigurationTablePresenter après chaque chargement.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour alimenter le ComboBox uniquement avec les modules possédant au moins une configuration.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour afficher uniquement les lignes de la marque choisie et garder le tableau vide lorsqu’aucune marque n’est choisie.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour préserver la marque choisie pendant un rechargement si elle existe encore, sans choisir automatiquement une autre marque si elle a disparu.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour laisser le ComboBox et le tableau vides lorsqu’aucune configuration n’existe.
    - [ ] Retirer l’ancienne liste seulement après le raccordement complet
      - [ ] Exécuter les tests du tableau avec le nouveau contrôle actif et vérifier que les actions Modifier et Supprimer atteignent la configuration exacte sans dépendre d’une sélection.
      - [ ] Supprimer _configurationList, _removeConfiguration et leur gestionnaire SelectionChanged de src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs après cette vérification.
      - [ ] Supprimer l’ancien ListBox, le bouton Supprimer global et RemoveConfiguration de src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionLayoutFunctions.cs après cette vérification.
      - [ ] Supprimer DeleteSelectedConfigurationAsync de src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs uniquement après le raccordement de la suppression directe par ligne.
  - [ ] Actualiser les textes après un changement de langue
    - [ ] Recréer les présentations localisées sans perdre le filtre
      - [ ] Modifier RefreshLocalizedContent dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour actualiser le libellé Marque, les en-têtes, les boutons et les noms accessibles des icônes.
      - [ ] Modifier RefreshLocalizedContent dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour reconstruire les lignes localisées en conservant la marque actuellement choisie lorsqu’elle existe encore.

- [ ] Ouverture de la configuration par le crayon ou le double-clic
  - [ ] Extraire la création différée d’un éditeur de marque avant de la réutiliser
    - [ ] Déplacer le code existant sans en changer le fonctionnement
      - [ ] Créer dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs une méthode GetOrCreateModuleSection qui reprend exactement la création de EmulationModuleSettingsSection, l’abonnement à ConfigurationSaved, l’ajout dans _moduleSections et l’affectation à TabItem.Content.
      - [ ] Modifier ModuleTabSelectionChanged dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour appeler GetOrCreateModuleSection et vérifier que l’ouverture manuelle des onglets Amiga et Atari reste identique.
      - [ ] Supprimer de ModuleTabSelectionChanged dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs l’ancien bloc de création seulement après cette vérification.
  - [ ] Fournir une entrée explicite dans l’éditeur pour la configuration choisie
    - [ ] Charger toute la configuration et revenir à Général
      - [ ] Ajouter EditConfigurationAsync dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs avec le module et IEmulationConfiguration attendus.
      - [ ] Modifier EditConfigurationAsync dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour actualiser _saved, affecter la configuration complète, sélectionner sa machine et fixer _selectedTab à EmulationMachineTab.General avant RebuildEditor.
      - [ ] Modifier EditConfigurationAsync dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour remplir dès la reconstruction les champs de tous les sous-onglets à partir de la même configuration, sans lancer la machine.
  - [ ] Raccorder les deux gestes à la même méthode d’ouverture
    - [ ] Créer un chemin unique depuis le tableau
      - [ ] Ajouter EditConfigurationAsync dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs avec EmulationConfigurationTableRow comme paramètre.
      - [ ] Modifier cette méthode dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour sélectionner le TabItem de row.Module, obtenir sa section par GetOrCreateModuleSection et appeler son EditConfigurationAsync.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour raccorder l’unique événement EditRequested du tableau à cette méthode, de sorte que le crayon et le double-clic ne puissent pas diverger.

- [ ] Suppression directe et confirmée d’une configuration
  - [ ] Identifier exactement la configuration sans utiliser de ligne sélectionnée
    - [ ] Créer le chemin de suppression par ligne
      - [ ] Ajouter DeleteConfigurationAsync dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs avec EmulationConfigurationTableRow comme paramètre.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour raccorder DeleteRequested du tableau à DeleteConfigurationAsync sans écrire row dans un SelectedItem temporaire.
  - [ ] Demander Oui ou Non avec un récapitulatif minimal
    - [ ] Afficher seulement ce qui identifie sans ambiguïté la configuration
      - [ ] Modifier DeleteConfigurationAsync dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour ouvrir une boîte Oui/Non utilisant Emulation.Configuration.DeleteConfirm avec le nom localisé de la marque et de la machine.
      - [ ] Modifier DeleteConfigurationAsync dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour ne pas afficher l’identifiant, les ROM, les lecteurs, les périphériques, la vidéo ou l’audio dans cette confirmation.
      - [ ] Modifier DeleteConfigurationAsync dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour ne rien supprimer et ne rien recharger lorsque la réponse est Non.
  - [ ] Supprimer puis remettre l’interface dans l’état exact attendu
    - [ ] Exécuter la suppression seulement après confirmation
      - [ ] Modifier DeleteConfigurationAsync dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour appeler row.Module.DeleteConfigurationAsync avec row.Configuration.Id uniquement après Oui.
      - [ ] Modifier DeleteConfigurationAsync dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour présenter toute erreur avec ControlErrorPresenter et conserver la ligne lorsque la suppression échoue.
    - [ ] Actualiser le filtre, le tableau et l’éditeur chargé
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour recharger les configurations après une réussite et conserver la marque choisie si elle possède encore au moins une configuration.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour vider le choix de marque et le tableau si la dernière configuration de la marque affichée vient d’être supprimée, sans sélectionner une autre marque disponible.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour demander à la EmulationModuleSettingsSection correspondante de retirer la configuration supprimée de _saved et de la mémoire si elle y était chargée.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour reconstruire dans ce cas les valeurs de base de la même machine et faire réapparaître Créer conformément au point 2.
  - [ ] Verrouiller les interactions et la suppression par des tests
    - [ ] Créer le fichier de tests avant ses scénarios
      - [ ] Créer tests/GWGUI.Tests/OptionsEmulationConfigurationTableTests.cs.
    - [ ] Ajouter les scénarios du filtre et de l’absence de sélection
      - [ ] Modifier tests/GWGUI.Tests/OptionsEmulationConfigurationTableTests.cs pour vérifier que le filtre contient uniquement les marques possédant une configuration et que le tableau contient leurs machines par ordre alphabétique.
      - [ ] Modifier tests/GWGUI.Tests/OptionsEmulationConfigurationTableTests.cs pour vérifier qu’un clic simple ne sélectionne rien et ne déclenche aucune action.
      - [ ] Modifier tests/GWGUI.Tests/OptionsEmulationConfigurationTableTests.cs pour vérifier que le crayon et le double-clic appellent la même méthode avec la même configuration.
      - [ ] Modifier tests/GWGUI.Tests/OptionsEmulationConfigurationTableTests.cs pour vérifier l’ouverture de la bonne marque dans Général avec toute la configuration chargée et sans lancement de machine.
    - [ ] Ajouter les scénarios de confirmation et de remise à zéro
      - [ ] Modifier tests/GWGUI.Tests/OptionsEmulationConfigurationTableTests.cs pour vérifier que Non conserve la configuration et que Oui supprime uniquement l’identifiant transmis par la ligne.
      - [ ] Modifier tests/GWGUI.Tests/OptionsEmulationConfigurationTableTests.cs pour vérifier le contenu minimal marque-machine de la confirmation.
      - [ ] Modifier tests/GWGUI.Tests/OptionsEmulationConfigurationTableTests.cs pour vérifier la conservation du filtre tant que la marque possède une ligne et sa remise à vide après suppression de sa dernière ligne.
      - [ ] Modifier tests/GWGUI.Tests/OptionsEmulationConfigurationTableTests.cs pour vérifier qu’aucune autre marque n’est sélectionnée automatiquement après cette suppression.
      - [ ] Modifier tests/GWGUI.Tests/OptionsEmulationConfigurationTableTests.cs pour vérifier la remise à zéro de l’éditeur chargé et la réapparition de Créer.
      - [ ] Exécuter OptionsEmulationConfigurationTableTests et la suite GWGUI.Tests, puis corriger uniquement les régressions provoquées par le nouveau tableau.
## Checklist détaillée — Point 4 : aides contextuelles sur les champs

Cette checklist correspond au groupe 5 de l’ordre général de réalisation. Elle concerne uniquement les champs des éditeurs de machine Amiga et Atari dont le libellé ne suffit pas à comprendre le rôle, les choix ou les conséquences. Elle ne crée aucune aide sur les boutons ni sur les titres de groupes. Le choix exact des champs est écrit et validé avant toute création de texte ou de clé de ressource.

- [ ] Définition exacte des champs qui recevront une aide
  - [ ] Établir l’inventaire depuis les champs réellement affichés par les machines Amiga et Atari
    - [ ] Inscrire l’inventaire dans le document avant de modifier les contrats
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md pour ajouter, à la section 4, un tableau recensant les champs visibles produits par src/GWGUI.Emulation.Amiga/Functions/AmigaSettingsDescriptionFunctions.cs et src/GWGUI.Emulation.Atari/Functions/AtariSettingsDescriptionFunctions.cs pour tous les modèles pris en charge.
      - [ ] Modifier ce tableau dans AMELIORATIONS_INTERFACE_EMULATION.md pour ajouter les champs fixes construits directement par src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs et src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationControllerSettingsLayout.cs, sans y ajouter les boutons, les titres de cartes, les titres d’onglets, les colonnes de tableaux ni les actions d’association.
      - [ ] Modifier ce tableau dans AMELIORATIONS_INTERFACE_EMULATION.md pour indiquer, pour chaque champ, son identifiant ou sa clé de libellé, les machines et onglets où il apparaît, et uniquement l’un des deux états « aide nécessaire » ou « aucune aide ».
    - [ ] Figer le contenu fonctionnel des aides avant de créer leurs ressources
      - [ ] Modifier le tableau de AMELIORATIONS_INTERFACE_EMULATION.md après validation pour conserver uniquement les champs approuvés comme nécessitant une aide et ne pas déduire de nouveaux champs à partir de préférences techniques.
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md pour écrire, face à chaque champ approuvé, le texte court d’une seule ligne destiné au survol et le texte concis destiné au clic.
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md pour faire décrire au texte au clic le rôle du champ, ses choix disponibles et leurs différences utiles sans ajouter de long paragraphe, de documentation générale ou d’information sans rapport avec le champ.
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md pour identifier explicitement les champs fixes qui doivent disparaître dans un point ultérieur et ne pas leur créer provisoirement une aide, uniquement lorsque cette suppression a déjà été validée.

- [ ] Transport des deux aides depuis la description de la machine jusqu’à l’interface
  - [ ] Étendre le contrat commun sans confondre les explications de compatibilité Atari
    - [ ] Ajouter la seconde clé d’aide au contrat avant de l’utiliser
      - [ ] Modifier src/GWGUI.Emulation/Contracts/EmulationSettingsField.cs pour conserver ExplanationResourceKey comme clé de l’aide courte et ajouter DetailedExplanationResourceKey comme clé distincte de l’aide concise au clic, sans modifier les propriétés homonymes indépendantes de src/GWGUI.Emulation.Atari/Contracts/AtariOptionRule.cs, src/GWGUI.Emulation.Atari/Contracts/AtariMediaCompatibilityRule.cs et src/GWGUI.Emulation.Atari/Contracts/AtariHardwareField.cs.
    - [ ] Étendre le champ de contrôle utilisé par les mises en page
      - [ ] Modifier src/GWGUI.App/Contracts/Emulation/Settings/EmulationSettingsControlField.cs pour ajouter les deux textes localisés optionnels ShortHelp et DetailedHelp après Label et Control, sans modifier le comportement des constructions qui ne fournissent aucune aide.
      - [ ] Modifier src/GWGUI.App/Contracts/Views/Emulation/Settings/EmulationCpuSettingsContent.cs pour transporter des EmulationSettingsControlField pour le modèle de CPU, la précision, le FPU, la vitesse d’origine et la vitesse réglable, tout en conservant séparément le résumé actuel du processeur.
  - [ ] Créer un seul chemin de conversion pour tous les champs décrits par un module
    - [ ] Ajouter le convertisseur commun avant de remplacer les constructions existantes
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour ajouter CreateControlField, qui crée le contrôle existant par CreateField, localise LabelResourceKey, localise les deux clés d’aide seulement lorsqu’elles sont renseignées et retourne un EmulationSettingsControlField.
      - [ ] Modifier CreateControlField dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour laisser ShortHelp et DetailedHelp tous les deux vides lorsqu’aucune aide n’a été validée pour le champ.
    - [ ] Faire passer les formulaires génériques par le convertisseur commun
      - [ ] Modifier AddBlocks dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour produire ses champs avec CreateControlField sans changer l’ordre, les blocs, le nombre de colonnes, la visibilité ni les contrôles actuels.
      - [ ] Vérifier avec les tests des éditeurs Amiga et Atari que les onglets Général, ROM, Vidéo, Audio et les options d’émulateur de Stockage conservent exactement leurs champs et leur ordre après ce raccordement.
    - [ ] Faire passer CPU et RAM par le même convertisseur
      - [ ] Modifier BuildCpuSettingsTab dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs pour construire le contenu de chaque champ CPU avec CreateControlField sans changer le résumé, les choix ni les règles existantes.
      - [ ] Modifier BuildMemorySettingsTab dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs pour remplacer les constructions directes de EmulationSettingsControlField par CreateControlField sans changer le calcul de RAM totale.
      - [ ] Vérifier avec les tests des éditeurs Amiga et Atari que les valeurs, états activés, visibilités conditionnelles et totaux CPU/RAM restent identiques avant de retirer toute ancienne construction devenue inutilisée.
    - [ ] Faire passer Souris et Manettes par le même convertisseur
      - [ ] Modifier BuildInputSettingsTab dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleInputSettingsSection.cs pour produire les champs décrits par le module avec CreateControlField sans modifier les associations d’entrée ni leur enregistrement.
      - [ ] Modifier src/GWGUI.App/Controllers/Emulation/Input/EmulationInputSettingsController.cs et src/GWGUI.App/Contracts/Emulation/Controllers/EmulationControllerPortSettings.cs uniquement pour transporter les deux aides des champs fixes approuvés dans l’inventaire, sans ajouter d’aide à un sélecteur exclu ou destiné à être retiré.
      - [ ] Vérifier avec les tests d’entrée existants que clavier, souris, trackball, manettes et joysticks conservent leurs sources de capture et leurs associations après ce raccordement.

- [ ] Contrôle réutilisable du libellé et de son icône d’information
  - [ ] Créer le contrôle avant de remplacer les TextBlock de libellé
    - [ ] Créer le fichier du contrôle commun
      - [ ] Créer src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs.
    - [ ] Reproduire d’abord le libellé existant sans aide
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour afficher le texte avec le même alignement et la même gestion du retour à la ligne que le libellé fourni par la mise en page appelante.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour ne créer aucune icône lorsque ShortHelp et DetailedHelp sont absents et conserver ainsi l’affichage actuel des champs sans aide.
    - [ ] Ajouter l’icône toujours visible uniquement aux champs approuvés
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour placer immédiatement après le texte une icône utilisant ControlVisualConstants.InformationGlyph lorsque les deux aides sont présentes.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour rendre cliquable uniquement la surface visible normale de l’icône, sans marge transparente ni zone invisible agrandie.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour conserver l’icône visible en permanence et ne pas conditionner son affichage au survol ou au focus.
  - [ ] Afficher l’aide courte exactement pendant le survol
    - [ ] Utiliser l’infobulle du contrôle commun
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour affecter ShortHelp à l’infobulle de l’icône et empêcher le retour à la ligne ou le défilement de cette aide d’une seule ligne.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour laisser WPF fermer l’infobulle dès la fin du survol sans la transformer en aide persistante.
  - [ ] Afficher l’aide concise au clic sous forme de post-it
    - [ ] Construire le post-it dans le même contrôle réutilisable
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour créer un Popup ancré à l’icône, contenant le nom du champ et DetailedHelp dans une présentation compacte de type post-it.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour limiter la taille du post-it et n’activer un ScrollViewer que lorsque le texte concis dépasse réellement l’espace disponible.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour ne pas ajouter de longs paragraphes, de liste de documentation ou de commandes supplémentaires autour du texte fourni par la ressource.
    - [ ] Fermer le post-it sur la prochaine action demandée
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour fermer le post-it sur n’importe quelle touche clavier reçue par la fenêtre qui le contient.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour fermer le post-it sur le clic suivant, y compris hors du contrôle, sans que le clic d’ouverture le ferme immédiatement.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour détacher les gestionnaires de fenêtre lors de la fermeture et de Unloaded afin de ne conserver ni fenêtre ni éditeur en mémoire.

- [ ] Utilisation du contrôle commun dans toutes les mises en page concernées
  - [ ] Ajouter le nouveau chemin sans casser les formulaires sans aide
    - [ ] Faire accepter les champs enrichis par les deux grilles communes
      - [ ] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationSettingsLayout.cs pour ajouter une surcharge de CompactForm acceptant des EmulationSettingsControlField et créant chaque libellé avec EmulationSettingsFieldLabel.
      - [ ] Modifier l’ancienne surcharge de CompactForm dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationSettingsLayout.cs pour déléguer à la nouvelle avec des aides vides, afin que l’onglet Général d’Émulation hors éditeur de machine conserve son fonctionnement actuel.
      - [ ] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationHardwareSettingsLayout.cs pour faire accepter des EmulationSettingsControlField par SettingsFields et SettingsFieldGrid sans changer les largeurs, marges, colonnes ni contrôles.
      - [ ] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationHardwareSettingsLayout.cs pour lier la visibilité de tout EmulationSettingsFieldLabel à celle du contrôle correspondant, afin que le texte et l’icône disparaissent ensemble.
      - [ ] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationHardwareSettingsLayout.cs pour ajouter une surcharge sans aide déléguant au nouveau chemin pour les appelants hors du périmètre Amiga/Atari.
  - [ ] Adapter les présentations spécialisées après la disponibilité du chemin commun
    - [ ] Utiliser le libellé commun dans CPU et RAM
      - [ ] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationCpuSettingsLayout.cs pour consommer les EmulationSettingsControlField du contrat CPU et afficher leurs libellés par SettingsFieldGrid sans modifier les trois cartes Processeur, Compatibilité et Accélération.
      - [ ] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationMemorySettingsLayout.cs pour transmettre directement les EmulationSettingsControlField à SettingsFieldGrid sans perdre leurs aides ni modifier les cadres RAM principale, extensions et total.
    - [ ] Utiliser le libellé commun dans Souris et Manettes
      - [ ] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationInputSettingsLayout.cs pour transmettre directement les EmulationSettingsControlField de la souris et des options analogiques à SettingsFields sans les réduire à Label et Control.
      - [ ] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationControllerSettingsLayout.cs pour transmettre directement les EmulationSettingsControlField des comportements de manette à SettingsFields sans modifier les tableaux d’associations.
      - [ ] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationControllerSettingsLayout.cs pour utiliser EmulationSettingsFieldLabel sur les seuls sélecteurs fixes approuvés dans l’inventaire et ne pas ajouter d’icône aux titres, aux ports, aux boutons Assigner ou aux lignes d’action.


- [ ] Textes courts et concis dans toutes les langues prises en charge
  - [ ] Créer les clés approuvées dans la ressource de base avant les traductions
    - [ ] Ajouter chaque paire de textes validée au catalogue neutre
      - [ ] Modifier src/GWGUI.App/Resources/00-Base/Emulation.resx pour créer, pour chaque champ approuvé, une clé d’aide courte et une clé d’aide concise correspondant exactement aux deux textes validés dans AMELIORATIONS_INTERFACE_EMULATION.md.
      - [ ] Modifier src/GWGUI.App/Resources/00-Base/Emulation.resx pour garantir que chaque aide courte tient sur une seule ligne et que chaque aide au clic reste courte, claire et limitée au rôle, aux choix et aux différences utiles du champ.
  - [ ] Traduire chaque paire sans écrire de texte dans le code
    - [ ] Ajouter les clés dans chaque catalogue de langue existant
      - [ ] Modifier src/GWGUI.App/Resources/ar-SA/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/cs-CZ/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/da-DK/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/de-DE/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/el-GR/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/en-US/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/es-ES/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/fi-FI/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/fr-FR/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/he-IL/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/hu-HU/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/id-ID/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/it-IT/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/ja-JP/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/ko-KR/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/nb-NO/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/nl-NL/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/pl-PL/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/pt-BR/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/pt-PT/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/ro-RO/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/ru-RU/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/sv-SE/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/th-TH/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/tr-TR/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/uk-UA/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/vi-VN/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/zh-Hans/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/zh-Hant/Emulation.resx.
  - [ ] Affecter uniquement les paires validées aux champs correspondants
    - [ ] Raccorder les clés Amiga après leur création dans tous les catalogues
      - [ ] Modifier src/GWGUI.Emulation.Amiga/Functions/AmigaSettingsDescriptionFunctions.cs pour renseigner ExplanationResourceKey et DetailedExplanationResourceKey uniquement sur les champs Amiga approuvés dans l’inventaire et laisser les deux propriétés vides pour tous les autres champs.
    - [ ] Raccorder les clés Atari après leur création dans tous les catalogues
      - [ ] Modifier src/GWGUI.Emulation.Atari/Functions/AtariSettingsDescriptionFunctions.cs pour renseigner ExplanationResourceKey et DetailedExplanationResourceKey uniquement sur les champs Atari approuvés dans l’inventaire, laisser les deux propriétés vides pour tous les autres champs et ne pas réutiliser les explications des règles de compatibilité Atari.
    - [ ] Raccorder les éventuels champs fixes approuvés
      - [ ] Modifier uniquement les constructeurs identifiés dans le tableau de AMELIORATIONS_INTERFACE_EMULATION.md pour fournir les deux clés aux champs fixes approuvés, sans généraliser l’icône à tous les libellés de l’application.

- [ ] Vérification fonctionnelle, visuelle et multilingue des aides
  - [ ] Verrouiller le comportement du contrôle commun par des tests WPF
    - [ ] Créer le fichier de tests avant ses scénarios
      - [ ] Créer tests/GWGUI.Tests/EmulationSettingsFieldHelpTests.cs.
    - [ ] Ajouter les scénarios de visibilité et de survol
      - [ ] Modifier tests/GWGUI.Tests/EmulationSettingsFieldHelpTests.cs pour vérifier qu’un champ sans paire d’aides n’affiche aucune icône et qu’un champ possédant les deux aides affiche toujours l’icône immédiatement après son libellé.
      - [ ] Modifier tests/GWGUI.Tests/EmulationSettingsFieldHelpTests.cs pour vérifier que l’infobulle contient uniquement l’aide courte d’une ligne et disparaît après le survol.
      - [ ] Modifier tests/GWGUI.Tests/EmulationSettingsFieldHelpTests.cs pour vérifier que la visibilité conditionnelle d’un contrôle masque également son libellé et son icône.
    - [ ] Ajouter les scénarios du post-it
      - [ ] Modifier tests/GWGUI.Tests/EmulationSettingsFieldHelpTests.cs pour vérifier qu’un clic sur l’icône ouvre un seul post-it avec le nom du champ et l’aide concise, sans fermer ce post-it pendant le clic d’ouverture.
      - [ ] Modifier tests/GWGUI.Tests/EmulationSettingsFieldHelpTests.cs pour vérifier que toute touche clavier ferme le post-it.
      - [ ] Modifier tests/GWGUI.Tests/EmulationSettingsFieldHelpTests.cs pour vérifier que le clic suivant ferme le post-it, qu’il provienne du contrôle ou d’une autre zone de la fenêtre.
      - [ ] Modifier tests/GWGUI.Tests/EmulationSettingsFieldHelpTests.cs pour vérifier que les gestionnaires de fenêtre sont détachés après fermeture ou déchargement du contrôle.
      - [ ] Modifier tests/GWGUI.Tests/EmulationSettingsFieldHelpTests.cs pour vérifier que le défilement du post-it reste désactivé lorsque le texte tient et devient disponible uniquement lorsqu’il dépasse la taille maximale.
  - [ ] Verrouiller l’exhaustivité du catalogue approuvé
    - [ ] Créer le fichier de tests du catalogue avant ses scénarios
      - [ ] Créer tests/GWGUI.Tests/EmulationSettingsHelpCatalogTests.cs.
    - [ ] Vérifier les paires et le périmètre Amiga/Atari
      - [ ] Modifier tests/GWGUI.Tests/EmulationSettingsHelpCatalogTests.cs pour parcourir les descriptions de tous les modèles Amiga et Atari et vérifier qu’un champ possède soit les deux clés d’aide, soit aucune des deux.
      - [ ] Modifier tests/GWGUI.Tests/EmulationSettingsHelpCatalogTests.cs pour comparer les champs portant une aide à l’inventaire approuvé de AMELIORATIONS_INTERFACE_EMULATION.md et détecter toute aide ajoutée ou oubliée.
      - [ ] Modifier tests/GWGUI.Tests/EmulationSettingsHelpCatalogTests.cs pour vérifier que toutes les clés courtes et détaillées existent dans le catalogue Emulation et que les aides courtes ne contiennent aucun retour à la ligne.
      - [ ] Modifier tests/GWGUI.Tests/EmulationSettingsHelpCatalogTests.cs pour vérifier les onglets génériques, CPU, RAM, Souris et Manettes afin qu’ils transmettent les deux textes jusqu’au EmulationSettingsFieldLabel correspondant.
  - [ ] Vérifier toutes les traductions et l’actualisation de langue
    - [ ] Compléter les contrôles de localisation existants
      - [ ] Modifier tests/GWGUI.Tests/LocalizationTests.cs pour vérifier que chaque paire d’aide de 00-Base/Emulation.resx existe dans les 29 catalogues traduits, en conservant le contrôle d’égalité des clés déjà présent.
      - [ ] Modifier tests/GWGUI.Tests/EmulationSettingsFieldHelpTests.cs pour changer la langue, reconstruire l’éditeur par RefreshLocalizedContent et vérifier que le libellé, l’infobulle et le post-it utilisent tous la nouvelle langue.
      - [ ] Modifier tests/GWGUI.Tests/EmulationSettingsFieldHelpTests.cs pour vérifier l’ancrage et la lecture du post-it dans une langue de droite à gauche sans modifier le comportement des champs.
  - [ ] Valider l’ensemble sans corriger d’autres comportements par préférence
    - [ ] Exécuter les tests ciblés puis la suite complète
      - [ ] Exécuter EmulationSettingsFieldHelpTests, EmulationSettingsHelpCatalogTests et LocalizationTests, puis corriger uniquement les erreurs introduites par les aides contextuelles.
      - [ ] Exécuter la suite GWGUI.Tests et corriger uniquement les régressions causées par le transport des métadonnées, le nouveau libellé ou les ressources d’aide.
      - [ ] Vérifier manuellement dans les éditeurs Amiga et Atari que l’icône reste visible, que le survol tient sur une ligne, que le clic ouvre le post-it, que toute touche ou le clic suivant le ferme, et qu’aucune icône n’apparaît sur un bouton ou un titre.
## Checklist détaillée — Point 5 : destination des ROM détectées

Cette checklist correspond au groupe 4 de l’ordre général de réalisation. Elle ajoute uniquement une information de destination à chaque ROM détectée pour la machine actuellement affichée. Le routage devient une donnée commune employée à la fois par la ligne informative et par le bouton Utiliser ; aucune seconde table de correspondance n’est ajoutée dans l’interface.

- [ ] Paramètres d’affichage restant à valider avant le code
  - [ ] Inscrire les deux valeurs manquantes dans le document
    - [ ] Fixer la limite de caractères sans l’inventer pendant l’implémentation
      - [ ] Modifier la section 5 de AMELIORATIONS_INTERFACE_EMULATION.md pour remplacer la mention du nombre maximal restant à fixer par la valeur validée, en précisant que l’ellipse fait partie de cette limite.
    - [ ] Fixer la position de la destination dans la ligne existante
      - [ ] Modifier la section 5 de AMELIORATIONS_INTERFACE_EMULATION.md pour indiquer la position validée de la destination par rapport au nom de la ROM et à la compatibilité, sans ajouter d’en-tête ou de texte permanent non demandé.

- [ ] Source unique du champ de destination utilisée par les modules
  - [ ] Transporter l’identifiant du champ cible avec chaque ROM détectée
    - [ ] Étendre le contrat avant de modifier les scanners
      - [ ] Modifier src/GWGUI.Emulation/Contracts/EmulationFirmwareCandidate.cs pour ajouter DestinationFieldId comme valeur optionnelle après les cinq données existantes, sans changer leur ordre, et laisser cette valeur vide lorsqu’aucun champ ne peut être déterminé pour la machine ayant produit le candidat.
  - [ ] Extraire le routage Amiga actuellement contenu dans Utiliser
    - [ ] Ajouter une fonction commune de type de ROM vers identifiant de champ
      - [ ] Modifier src/GWGUI.Emulation.Amiga/Modules/AmigaEmulationModule.cs pour ajouter une fonction unique qui associe AmigaFirmwareType.Kickstart à AmigaSettingsConstants.KickstartPath, AmigaFirmwareType.ExtendedRom à AmigaSettingsConstants.ExtendedRomPath et AmigaFirmwareType.RomKey à AmigaSettingsConstants.RomKeyPath.
      - [ ] Modifier cette fonction dans src/GWGUI.Emulation.Amiga/Modules/AmigaEmulationModule.cs pour retourner une destination vide pour AmigaFirmwareType.Unknown et toute valeur sans champ pris en charge.
    - [ ] Renseigner le candidat depuis le résultat déjà obtenu par le scan
      - [ ] Modifier ScanFirmwareAsync dans src/GWGUI.Emulation.Amiga/Modules/AmigaEmulationModule.cs pour affecter DestinationFieldId à partir du AmigaFirmwareType déjà fourni par AmigaFirmwareCatalog.Scan, sans inspecter une seconde fois le fichier pour construire la ligne.
    - [ ] Faire utiliser exactement la même fonction au bouton Utiliser
      - [ ] Modifier UseFirmware dans src/GWGUI.Emulation.Amiga/Modules/AmigaEmulationModule.cs pour conserver l’inspection actuelle du fichier au moment de l’utilisation, appeler la même fonction de routage avec le type alors constaté et modifier uniquement le chemin correspondant à cet identifiant.
      - [ ] Modifier UseFirmware dans src/GWGUI.Emulation.Amiga/Modules/AmigaEmulationModule.cs pour refuser une destination vide et conserver l’erreur actuelle pour une ROM qui n’est plus utilisable, sans choisir un champ par défaut.
      - [ ] Modifier UseFirmware dans src/GWGUI.Emulation.Amiga/Modules/AmigaEmulationModule.cs pour ne jamais employer le texte affiché, le nom du fichier ou le libellé traduit afin de décider entre Kickstart, ROM étendue et Clé ROM.
  - [ ] Extraire le routage Atari actuellement contenu dans Utiliser
    - [ ] Ajouter une fonction commune du résultat de scan vers l’identifiant du champ système
      - [ ] Modifier src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs pour ajouter une fonction unique qui retourne AtariSettingsConstants.SystemFirmware uniquement lorsque le AtariScannedFirmware peut être transformé en sélection pour la machine courante.
      - [ ] Modifier cette fonction dans src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs pour retourner une destination vide lorsque la ROM est inconnue sans définition exploitable, illisible, incompatible, intégrée ou non utilisée.
    - [ ] Renseigner le candidat depuis le scan déjà exécuté
      - [ ] Modifier ScanFirmwareAsync dans src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs pour affecter DestinationFieldId à partir du AtariScannedFirmware déjà obtenu pour le modèle courant, sans créer une correspondance par nom de modèle dans l’application.
    - [ ] Faire utiliser exactement la même fonction au bouton Utiliser
      - [ ] Modifier UseFirmware dans src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs pour conserver le nouveau scan de contrôle effectué au moment de l’utilisation, obtenir l’identifiant par la même fonction puis créer la sélection Atari uniquement lorsque cet identifiant est AtariSettingsConstants.SystemFirmware.
      - [ ] Modifier UseFirmware dans src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs pour conserver le remplacement par catégorie déjà réalisé dans Firmwares et ne modifier aucune autre donnée de AtariMachineConfiguration.
      - [ ] Modifier UseFirmware dans src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs pour ne jamais déduire la destination depuis DisplayName, Version, le nom du fichier ou le texte localisé.

- [ ] Résolution du libellé traduit pour la seule machine affichée
  - [ ] Retrouver le champ cible dans la description courante du module
    - [ ] Ajouter le résolveur dans l’éditeur qui possède déjà la machine et sa configuration
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour ajouter une méthode qui reçoit DestinationFieldId et recherche exactement ce Id parmi les blocs et champs visibles retournés par _module.Describe pour _configuration.MachineId et _configuration.
      - [ ] Modifier cette méthode dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour retourner LocExtension.Get du LabelResourceKey du champ trouvé, afin de reprendre Kickstart, ROM étendue, Clé ROM ou ROM système sans créer une traduction réservée à la colonne.
      - [ ] Modifier cette méthode dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour retourner une chaîne vide lorsque DestinationFieldId est vide, lorsque le champ n’existe pas dans la description courante ou lorsque son bloc, son onglet ou le champ lui-même n’est pas visible.
      - [ ] Modifier cette méthode dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour ne consulter aucune autre machine ni aucune autre marque après l’échec de la recherche dans la configuration affichée.
  - [ ] Fournir ce résolveur au contrôleur ROM sans déplacer la logique de machine
    - [ ] Étendre le constructeur avant de l’utiliser pendant le rafraîchissement
      - [ ] Modifier src/GWGUI.App/Controllers/Emulation/Firmware/EmulationFirmwareManagementController.cs pour recevoir et conserver une fonction résolvant un identifiant de champ en libellé pour la configuration courante.
      - [ ] Modifier la création de EmulationFirmwareManagementController dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour lui transmettre la méthode commune de résolution sans lui transmettre la liste de toutes les machines.
    - [ ] Résoudre chaque cellule pendant la construction de la liste
      - [ ] Modifier RefreshAsync dans src/GWGUI.App/Controllers/Emulation/Firmware/EmulationFirmwareManagementController.cs pour résoudre le libellé à partir de firmware.DestinationFieldId et le transmettre à la ligne de cette même ROM.
      - [ ] Modifier RefreshAsync dans src/GWGUI.App/Controllers/Emulation/Firmware/EmulationFirmwareManagementController.cs pour transmettre une valeur vide lorsque le résolveur ne trouve pas le champ, sans masquer la ROM ni modifier sa compatibilité.

- [ ] Colonne informative dans la ligne des ROM détectées
  - [ ] Préparer les dimensions après validation des paramètres
    - [ ] Ajouter uniquement les constantes nécessaires à la nouvelle cellule
      - [ ] Modifier src/GWGUI.App/Constants/Emulation/EmulationFirmwareSettingsConstants.cs pour ajouter uniquement la limite de caractères validée et la largeur réservée à la destination, calculée pour conserver lisibles l’identité de la ROM et la compatibilité dans la largeur actuelle, sans changer FirmwareCompatibilityColumnWidth ni FirmwareRowMinimumHeight.
  - [ ] Préserver d’abord exactement le badge de compatibilité existant
    - [ ] Extraire sa construction avant d’ajouter le second badge
      - [ ] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs pour extraire dans une fonction commune la bordure, le rayon, les marges, le padding et l’alignement actuellement créés directement pour le badge de compatibilité.
      - [ ] Modifier FirmwareRow dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs pour appeler cette fonction avec les couleurs retournées par FirmwareBadgeColors, sans changer le texte, les couleurs, la largeur ou la position actuels de la compatibilité.
      - [ ] Supprimer de FirmwareRow dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs l’ancienne construction directe du badge seulement après avoir raccordé et vérifié la fonction commune.
  - [ ] Ajouter la destination à la position validée
    - [ ] Étendre la ligne avec une valeur purement affichée
      - [ ] Modifier la signature de FirmwareRow dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs pour recevoir le libellé de destination déjà résolu, sans recevoir IEmulationConfiguration, IEmulationModule ou une liste de machines.
      - [ ] Modifier la grille de FirmwareRow dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs pour ajouter la colonne à la position inscrite dans la section 5 et conserver les colonnes existantes dans leur ordre relatif.
      - [ ] Modifier FirmwareRow dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs pour afficher la destination avec la même forme, les mêmes dimensions verticales et la même présentation simple que le badge de compatibilité, en utilisant les couleurs neutres existantes de l’interface plutôt qu’une nouvelle signification colorée.
      - [ ] Modifier FirmwareRow dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs pour créer une cellule vide lorsque le libellé de destination est vide, sans texte de remplacement, icône, tiret ou avertissement.
    - [ ] Appliquer la limite de caractères et l’ellipse
      - [ ] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs pour limiter le libellé au nombre de caractères validé et terminer le texte tronqué par une ellipse comprise dans cette limite.
      - [ ] Modifier le TextBlock de destination dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs pour appliquer également TextTrimming.CharacterEllipsis si la largeur disponible devient inférieure à la largeur réservée, sans ajouter de défilement horizontal.
      - [ ] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs pour ne pas ajouter d’infobulle, de clic, de menu ou d’action à la cellule informative.
  - [ ] Raccorder le contrôleur à la nouvelle signature
    - [ ] Transmettre la destination sans modifier l’objet sélectionné
      - [ ] Modifier RefreshAsync dans src/GWGUI.App/Controllers/Emulation/Firmware/EmulationFirmwareManagementController.cs pour appeler FirmwareRow avec le libellé de destination en plus du nom, de la version, de la compatibilité et du chemin déjà transmis, tout en gardant EmulationFirmwareCandidate comme unique Tag du ListBoxItem.

- [ ] Conservation exacte du comportement de sélection et d’utilisation
  - [ ] Laisser la destination strictement informative
    - [ ] Empêcher la cellule d’introduire une nouvelle action
      - [ ] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs pour que la destination ne possède aucun gestionnaire de clic et laisse le ListBoxItem parent conserver son comportement normal de sélection.
      - [ ] Modifier src/GWGUI.App/Controllers/Emulation/Firmware/EmulationFirmwareManagementController.cs pour ne jamais appeler UseFirmware depuis la construction, l’affichage ou le clic de la cellule de destination.

  - [ ] Actualiser la destination avec le contexte déjà utilisé par la liste
    - [ ] Recalculer l’affichage lors des rafraîchissements existants
      - [ ] Modifier RefreshAsync dans src/GWGUI.App/Controllers/Emulation/Firmware/EmulationFirmwareManagementController.cs pour reconstruire les cellules de destination après un changement de machine, de configuration ou de langue par le même chemin qui reconstruit déjà la liste des ROM.
      - [ ] Modifier RefreshLocalizedContent dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour conserver le rechargement de l’onglet ROM avec la configuration affichée et ne pas mémoriser un libellé traduit provenant de la langue précédente.

- [ ] Tests du routage partagé, de l’affichage et des non-régressions
  - [ ] Verrouiller les destinations produites par Amiga et Atari
    - [ ] Compléter les tests de module existants
      - [ ] Modifier tests/GWGUI.Tests/EmulationModuleFirmwareTests.cs pour vérifier que les candidats Kickstart, ROM étendue et Clé ROM reçoivent respectivement les identifiants AmigaSettingsConstants.KickstartPath, ExtendedRomPath et RomKeyPath, et qu’une ROM Amiga inconnue reçoit une destination vide.
      - [ ] Modifier tests/GWGUI.Tests/EmulationModuleFirmwareTests.cs pour vérifier qu’une ROM Atari sélectionnable pour la machine testée reçoit AtariSettingsConstants.SystemFirmware et qu’une ROM incompatible ou sans définition exploitable reçoit une destination vide.
      - [ ] Modifier tests/GWGUI.Tests/EmulationModuleFirmwareTests.cs pour vérifier que UseFirmware modifie exactement le champ annoncé par DestinationFieldId et laisse inchangés les autres chemins ROM ou catégories de firmware.
      - [ ] Modifier tests/GWGUI.Tests/EmulationModuleFirmwareTests.cs pour remplacer le contenu d’un fichier après son scan et vérifier que UseFirmware conserve sa validation actuelle au moment du clic au lieu de faire confiance à une destination devenue périmée.
  - [ ] Verrouiller la résolution du libellé de la machine affichée
    - [ ] Créer le fichier de tests du résolveur avant ses scénarios
      - [ ] Créer tests/GWGUI.Tests/EmulationFirmwareDestinationTests.cs.
    - [ ] Ajouter les scénarios de machine, de visibilité et de langue
      - [ ] Modifier tests/GWGUI.Tests/EmulationFirmwareDestinationTests.cs pour vérifier que chaque identifiant Amiga retrouve le LabelResourceKey du champ correspondant dans la description de la machine affichée.
      - [ ] Modifier tests/GWGUI.Tests/EmulationFirmwareDestinationTests.cs pour vérifier que AtariSettingsConstants.SystemFirmware retrouve uniquement le champ ROM système de l’Atari affiché.
      - [ ] Modifier tests/GWGUI.Tests/EmulationFirmwareDestinationTests.cs pour vérifier qu’un identifiant absent, un champ masqué ou une destination vide produit une cellule vide sans rechercher une autre machine.
      - [ ] Modifier tests/GWGUI.Tests/EmulationFirmwareDestinationTests.cs pour vérifier en français et en anglais que la cellule reprend exactement LocExtension.Get du LabelResourceKey du champ, sans clé de traduction propre à la destination.
  - [ ] Verrouiller la nouvelle cellule et sa limite
    - [ ] Ajouter les tests WPF de la ligne dans le même fichier
      - [ ] Modifier tests/GWGUI.Tests/EmulationFirmwareDestinationTests.cs pour vérifier le nombre et l’ordre validé des colonnes de FirmwareRow, ainsi que la présence simultanée de l’identité, de la destination et de la compatibilité.
      - [ ] Modifier tests/GWGUI.Tests/EmulationFirmwareDestinationTests.cs pour vérifier que le badge de compatibilité conserve son texte, ses couleurs et ses dimensions après l’extraction de sa construction.
      - [ ] Modifier tests/GWGUI.Tests/EmulationFirmwareDestinationTests.cs pour vérifier qu’un libellé court reste intact, qu’un libellé dépassant la limite validée se termine par une ellipse dans cette limite et qu’une destination absente laisse la cellule vide.
      - [ ] Modifier tests/GWGUI.Tests/EmulationFirmwareDestinationTests.cs pour vérifier que la cellule ne possède aucun gestionnaire d’action, que son clic sélectionne seulement le ListBoxItem parent, que UpdateUseButton dépend toujours uniquement de Compatibility et que UseSelected transmet toujours le candidat stocké dans Tag.
  - [ ] Vérifier le bouton Utiliser et l’ensemble de l’application
    - [ ] Exécuter les tests ciblés puis la suite complète
      - [ ] Exécuter EmulationModuleFirmwareTests et EmulationFirmwareDestinationTests, puis corriger uniquement les erreurs introduites par la destination partagée ou son affichage.
      - [ ] Exécuter la suite GWGUI.Tests et corriger uniquement les régressions provoquées par DestinationFieldId, le résolveur de libellé ou la nouvelle colonne.
      - [ ] Vérifier manuellement avec une machine Amiga et une machine Atari que chaque ROM affiche le champ réellement modifié par Utiliser, que les ROM sans destination gardent une cellule vide et que les autres machines ne sont jamais mentionnées.
## Checklist détaillée — Point 6 : associations et visualisation des manettes et joysticks

- [ ] Sécurisation du visualiseur et de l’éditeur d’associations existants
  - [ ] Verrouiller le rendu actuel avant de déplacer sa définition
    - [ ] Compléter les tests d’images et de coordonnées existants
      - [ ] Modifier tests/GWGUI.Tests/ControllerVisualizationTests.cs pour enregistrer le modèle, l’image, les dimensions normalisées et les zones actuellement utilisées par chacun des profils physiques déjà pris en charge.
      - [ ] Modifier tests/GWGUI.Tests/ControllerVisualizationTests.cs pour vérifier que chaque valeur de ControllerVisualModel conserve son image actuelle pendant la migration vers les profils de rendu communs.
      - [ ] Modifier tests/GWGUI.Tests/ControllerSignalVisualizationTests.cs pour vérifier séparément les états neutre et appuyé des boutons numériques ainsi que les valeurs minimales, intermédiaires et maximales des axes et des gâchettes analogiques.
      - [ ] Modifier tests/GWGUI.Tests/ControllerSignalVisualizationTests.cs pour vérifier que plusieurs boutons et axes actifs au même instant sont tous dessinés dans une seule image.
  - [ ] Verrouiller la capture actuelle avant de partager son point d’entrée
    - [ ] Étendre les scénarios de capture sans modifier leur fonctionnement
      - [ ] Modifier tests/GWGUI.Tests/InputBindingEditorCaptureTests.cs pour vérifier que le bouton Assigner lance toujours la capture du clavier, de la souris et de n’importe quelle manette autorisée par les sources configurées.
      - [ ] Modifier tests/GWGUI.Tests/InputBindingEditorCaptureTests.cs pour vérifier que la capture d’une manette enregistre son identifiant dans l’association sans exiger qu’elle ait été choisie dans un sélecteur global.
      - [ ] Modifier tests/GWGUI.Tests/InputBindingEditorCaptureTests.cs pour vérifier que la fin, l’annulation et le délai maximal de capture restent identiques lorsque son déclenchement sera partagé avec le visuel.
  - [ ] Consigner les correspondances réellement exposées par les émulateurs
    - [ ] Ajouter au document la table de travail qui empêchera la création de périphériques inutilisés
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md pour ajouter, à partir de AmigaInputSettingsFunctions.ControllerDefinitions et des listes Amiga déjà existantes, les identifiants exacts des périphériques proposés, leurs commandes et leur machine d’utilisation, sans ajouter de périphérique historique absent de l’application.
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md pour ajouter, à partir de AtariInputSettingsFunctions.ControllerDefinitions, ControllerActions et des listes Atari déjà existantes, les identifiants exacts des périphériques proposés, leurs commandes et leur machine d’utilisation, sans ajouter de périphérique historique absent de l’application.
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md pour marquer dans cette table uniquement les périphériques basiques dont l’image doit être réalisée pendant le point 6 et laisser les autres comme évolutions futures.
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md pour faire valider le traitement visuel de Automatique, Aucun et Clavier avant d’ajouter un profil ou une image pour ces choix.

- [ ] Transformation du visualiseur existant en moteur commun piloté par des profils
  - [ ] Créer les contrats de description avant d’y déplacer les coordonnées
    - [ ] Créer les fichiers des profils de rendu communs
      - [ ] Créer src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkProfile.cs pour décrire l’image, ses limites normalisées et ses zones interactives sans dépendre d’un périphérique physique ou émulé.
      - [ ] Créer src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualZone.cs pour décrire l’identifiant de commande, la position, la taille, la forme et le comportement numérique ou analogique d’une zone en pourcentages de l’image concernée.
      - [ ] Créer src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualState.cs pour porter simultanément les valeurs de toutes les commandes visibles sans conserver leur état dans l’image ou dans le profil.
      - [ ] Créer src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerOverlayRenderer.cs pour dessiner et tester les zones d’un ControllerArtworkProfile avec un ControllerVisualState.
  - [ ] Déplacer les descriptions physiques sans changer leur rendu
    - [ ] Copier les données actuellement codées dans les méthodes spécialisées
      - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkCatalog.cs pour associer à chaque ControllerVisualModel un ControllerArtworkProfile contenant d’abord exactement son image et ses coordonnées actuelles.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualizer.Artwork.cs pour faire dessiner un profil physique par ControllerOverlayRenderer tout en conservant les formes, les seuils et les halos actuels à cette étape.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualInput.cs pour produire un ControllerVisualState équivalent à partir de GameInputLiveState, y compris lorsque plusieurs commandes sont actives.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualizer.cs pour accepter le profil et l’état communs tout en maintenant temporairement Model et State pour l’onglet général Manettes.
      - [ ] Modifier tests/GWGUI.Tests/ControllerVisualizationTests.cs pour comparer le rendu de chaque profil migré à sa référence verrouillée avant de retirer ses anciennes coordonnées.
    - [ ] Retirer les coordonnées dupliquées seulement après le passage au moteur commun
      - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualizer.Artwork.cs pour supprimer les coordonnées et les branchements remplacés uniquement lorsque les dix-neuf profils physiques passent par ControllerArtworkCatalog et leurs tests.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualizer.Modern.cs pour conserver seulement le dessin de secours réellement utilisé quand aucune image n’est disponible, sans recréer les profils déplacés.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualizer.Retro.cs pour conserver seulement le dessin de secours réellement utilisé quand aucune image n’est disponible, sans recréer les profils déplacés.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualizer.Special.cs pour conserver seulement le dessin de secours réellement utilisé quand aucune image n’est disponible, sans recréer les profils déplacés.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Options/OptionsControllersSection.xaml.cs pour alimenter le même ControllerVisualizer avec le profil physique et l’adaptateur communs, sans créer un second visualiseur.
      - [ ] Modifier tests/GWGUI.Tests/ControllerVisualizationTests.cs et tests/GWGUI.Tests/ControllerSignalVisualizationTests.cs pour vérifier que l’onglet général Manettes conserve tous ses modèles et tous ses signaux après le retrait des anciens chemins de rendu.
- [ ] Lecture commune des associations provenant de toutes les sources autorisées
  - [ ] Centraliser la valeur d’une association avant de retirer le périphérique global
    - [ ] Créer le fichier de tests du résolveur avant ses scénarios
      - [ ] Créer tests/GWGUI.Tests/EmulationInputMappingTests.cs.
    - [ ] Étendre le résolveur partagé d’entrée
      - [ ] Modifier src/GWGUI.Emulation/Functions/EmulationInputMappingFunctions.cs pour lire une association clavier, souris ou contrôleur dans un EmulationInputSnapshot et retourner sa valeur normalisée en plus de son état appuyé.
      - [ ] Modifier src/GWGUI.Emulation/Functions/EmulationInputMappingFunctions.cs pour résoudre un identifiant réel inclus dans l’association vers cette manette précise, un indice numérique existant vers la manette correspondante et une association sans identifiant vers toute manette qui fournit la commande.
      - [ ] Modifier src/GWGUI.Emulation/Functions/EmulationInputMappingFunctions.cs pour combiner les valeurs de plusieurs manettes sans empêcher deux manettes, le clavier, la souris ou un autre périphérique autorisé d’activer simultanément des commandes différentes.
      - [ ] Modifier tests/GWGUI.Tests/EmulationInputMappingTests.cs pour couvrir les associations clavier, boutons et molette de souris, contrôleur identifié, contrôleur indexé, contrôleur non identifié et plusieurs sources actives simultanément.
  - [ ] Faire utiliser le même résolveur par Amiga et Atari
    - [ ] Remplacer les décisions fondées sur DeviceId sans changer les commandes émulées
      - [ ] Modifier src/GWGUI.Emulation.Amiga/Functions/AmigaInputSnapshotFunctions.cs pour évaluer chaque association par EmulationInputMappingFunctions au lieu de choisir d’abord une unique manette avec AmigaControllerBinding.DeviceId.
      - [ ] Modifier src/GWGUI.Emulation.Atari/Functions/AtariInputSnapshotFunctions.cs pour évaluer chaque association par EmulationInputMappingFunctions au lieu de choisir d’abord une unique manette avec AtariControllerBinding.DeviceId.
      - [ ] Modifier tests/GWGUI.Tests/AmigaControllerMappingTests.cs pour vérifier qu’un même port accepte des associations issues de plusieurs manettes, du clavier et de la souris sans sélection physique préalable.
      - [ ] Modifier tests/GWGUI.Tests/AtariControllerMappingTests.cs pour vérifier qu’un même port accepte des associations issues de plusieurs manettes, du clavier et de la souris sans sélection physique préalable.
      - [ ] Modifier tests/GWGUI.Tests/AmigaControllerMappingTests.cs et tests/GWGUI.Tests/AtariControllerMappingTests.cs pour vérifier que les associations numériques par port déjà enregistrées conservent leur comportement multijoueur.
    - [ ] Autoriser la souris dans l’éditeur des commandes de périphérique
      - [ ] Modifier src/GWGUI.Emulation.Amiga/Functions/AmigaInputSettingsFunctions.cs pour inclure EmulationInputSource.Mouse dans les sources des associations de périphériques qui utilisent l’éditeur commun, sans modifier les commandes proposées par ControllerDefinitions.
      - [ ] Modifier src/GWGUI.Emulation.Atari/Functions/AtariInputSettingsFunctions.cs pour inclure EmulationInputSource.Mouse dans les sources des associations de périphériques qui utilisent l’éditeur commun, sans modifier les commandes proposées par ControllerDefinitions.
      - [ ] Modifier tests/GWGUI.Tests/InputBindingEditorCaptureTests.cs pour vérifier qu’une association Amiga ou Atari peut être capturée depuis un clic, une molette ou un autre signal de souris pris en charge.

- [ ] Retrait du sélecteur physique inutile sans casser les anciennes configurations
  - [ ] Retirer le contrôle de l’éditeur après le basculement du routage
    - [ ] Modifier les objets d’interface qui transportent encore Device
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/EmulationControllerPortEditor.cs pour retirer la ComboBox Device et l’abonnement ControllerCaptured qui sélectionne automatiquement une manette physique.
      - [ ] Modifier src/GWGUI.App/Contracts/Emulation/Settings/EmulationControllerPortSettings.cs pour retirer la ComboBox Device du contrat utilisé par la disposition.
      - [ ] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationControllerSettingsLayout.cs pour retirer le champ « Périphérique de la manette 1 », les autres champs équivalents et les paramètres devenus inutiles de détection des manettes.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationControllerSettingsSection.cs pour ne plus détecter ni injecter une liste de manettes physiques dans chaque port.
      - [ ] Modifier tests/GWGUI.Tests/EmulationControllerSettingsLayoutTests.cs pour vérifier que chaque port affiche son type émulé et ses associations, mais aucun sélecteur préalable de manette physique.
  - [ ] Préserver les fichiers existants sans continuer à utiliser leur sélection globale
    - [ ] Maintenir la compatibilité de lecture et d’écriture
      - [ ] Modifier src/GWGUI.App/Controllers/Emulation/Input/EmulationInputSettingsController.cs pour ne plus lire un choix Device depuis l’interface et conserver la valeur PhysicalDeviceId déjà chargée lors d’un enregistrement, sans l’utiliser pour choisir les sources.
      - [ ] Modifier tests/GWGUI.Tests/AmigaConfigurationStoreTests.cs et tests/GWGUI.Tests/AtariConfigurationStoreTests.cs pour vérifier que PhysicalDeviceId, AmigaControllerBinding.DeviceId et AtariControllerBinding.DeviceId restent relus et réécrits pour la compatibilité des anciens fichiers sans redevenir une restriction d'entrée.

- [ ] Catalogue des représentations émulées réellement retenues
  - [ ] Valider les fichiers nécessaires avant de créer les images
    - [ ] Créer le fichier de tests du catalogue avant ses scénarios
      - [ ] Créer tests/GWGUI.Tests/EmulatedControllerArtworkCatalogTests.cs.
    - [ ] Créer le manifeste vérifiable à partir de la table approuvée
      - [ ] Créer src/GWGUI.App/Views/Controls/Options/ControllerVisualization/EmulatedControllerArtworkCatalog.cs pour indexer un profil par module, machine et identifiant de périphérique, car un même identifiant peut représenter des appareils différents selon la machine.
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md pour inscrire dans la checklist, avant leur création, le chemin exact de chaque image basique approuvée, son périphérique, ses machines compatibles et les identifiants de commandes placés dessus.
      - [ ] Modifier tests/GWGUI.Tests/EmulatedControllerArtworkCatalogTests.cs pour comparer le manifeste aux listes déjà retournées par les modules et refuser un profil déclaré pour un périphérique absent de ces listes.
  - [ ] Créer uniquement les images approuvées dans la table
    - [ ] Créer le dossier d'images avant son premier fichier
      - [ ] Créer le dossier src/GWGUI.App/Assets/Controllers/Emulated.
    - [ ] Ajouter chaque représentation réaliste avant son profil exécutable
      - [ ] Créer chaque fichier inscrit et validé dans AMELIORATIONS_INTERFACE_EMULATION.md sous src/GWGUI.App/Assets/Controllers/Emulated avec une vue de face, un fond transparent et un cadrage permettant de réduire l’image sans déplacer ses commandes.
      - [ ] Modifier src/GWGUI.App/GWGUI.App.csproj pour embarquer les fichiers de src/GWGUI.App/Assets/Controllers/Emulated seulement après la création du premier fichier réel.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/EmulatedControllerArtworkCatalog.cs après chaque image pour ajouter ses zones avec leurs positions propres en pourcentages, sans reprendre les positions d’une autre image.
      - [ ] Modifier tests/GWGUI.Tests/EmulatedControllerArtworkCatalogTests.cs après chaque profil pour vérifier que son fichier existe, que son fond contient de la transparence, que toutes ses zones restent dans l’image et que chaque identifiant de zone appartient aux définitions de la machine concernée.
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md après la validation visuelle de chaque image pour cocher seulement son fichier, son profil et ses zones réellement acceptés.
- [ ] Intégration du visuel à droite du tableau de chaque port
  - [ ] Créer le suivi en direct avant de construire le panneau
    - [ ] Créer le fichier de tests du moniteur avant ses scénarios
      - [ ] Créer tests/GWGUI.Tests/EmulationBindingLiveMonitorTests.cs.
    - [ ] Créer le composant qui transforme les entrées physiques en état visuel
      - [ ] Créer src/GWGUI.App/Views/Controls/Emulation/Input/EmulationBindingLiveMonitor.cs pour lire GameInputControllerReader.ReadPhysicalInput, évaluer toutes les associations du port par EmulationInputMappingFunctions et produire un ControllerVisualState commun.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/EmulationBindingLiveMonitor.cs pour conserver simultanément tous les boutons, directions, axes, touches du clavier, actions de souris, trackball et autres sources prises en charge qui sont actifs pendant la même lecture.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/EmulationBindingLiveMonitor.cs pour ne surveiller que le port visible, démarrer lors du chargement du panneau et arrêter son temporisateur lors de son masquage ou de son déchargement.
      - [ ] Modifier tests/GWGUI.Tests/EmulationBindingLiveMonitorTests.cs pour vérifier les appuis simultanés, le changement de port visible, l’arrêt du suivi hors écran et l’absence d’état permanent après modification d’une association.
  - [ ] Construire une disposition stable autour du tableau existant
    - [ ] Ajouter un seul visualiseur au port actuellement affiché
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/EmulationControllerPortEditor.cs pour posséder un ControllerVisualizer commun, un EmulationBindingLiveMonitor et le profil correspondant au module, à la machine et au type émulé du port.
      - [ ] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationControllerSettingsLayout.cs pour placer le tableau à gauche et le bloc du visualiseur à droite dans le contenu de chaque onglet de port.
      - [ ] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationControllerSettingsLayout.cs pour garder la largeur utile du tableau et celle du bloc visuel lorsque la fenêtre le permet, laisser le défilement dans le tableau et empêcher le visuel de défiler avec ses lignes.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualizer.cs pour réduire uniformément seulement l’image à l’intérieur de son bloc quand l’espace disponible diminue, sans modifier les pourcentages de ses zones.
      - [ ] Modifier src/GWGUI.App/Controllers/Emulation/Input/EmulationInputSettingsController.cs pour fournir au port le module, la machine et le type sélectionné puis actualiser le profil après un changement de type avec le même chemin qui actualise déjà les définitions du tableau.
      - [ ] Modifier tests/GWGUI.Tests/EmulationControllerSettingsLayoutTests.cs pour vérifier qu’un seul visuel est affiché dans le port actif, qu’il reste à côté du tableau pendant son défilement et qu’un changement de port ou de type charge le profil correspondant.
  - [ ] Réduire la colonne État sans perdre son information
    - [ ] Conserver uniquement l’icône déjà compréhensible
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/InputBindingEditor.xaml pour réduire la largeur de la colonne État, conserver son icône et retirer seulement le texte visible « Valide », « Conflit », « Réservé » ou « Non assigné » de chaque ligne.
      - [ ] Modifier src/GWGUI.App/ViewModels/Input/InputBindingRow.cs pour conserver StateText comme information accessible ou infobulle de l’icône sans le réafficher dans la cellule.
      - [ ] Modifier tests/GWGUI.Tests/EmulationControllerSettingsLayoutTests.cs pour vérifier que l’icône, sa couleur et son texte accessible changent toujours avec l’état tandis que le texte n’occupe plus une colonne visible.

- [ ] Capture d’une association depuis la représentation virtuelle
  - [ ] Partager le déclenchement actuel avant d’ajouter le clic sur l’image
    - [ ] Extraire un unique début de capture
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/InputBindingEditorCaptureFunctions.cs pour extraire d’AssignClicked une méthode qui reçoit l’identifiant de commande, retrouve sa ligne, la rend visible et lance exactement la capture actuelle.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/InputBindingEditorCaptureFunctions.cs pour faire appeler cette même méthode par le bouton Assigner, sans modifier son texte, son délai, son annulation ni ses sources.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/InputBindingEditor.xaml.cs pour exposer au visualiseur une méthode de capture par identifiant de commande sans exposer les détails du temporisateur.
      - [ ] Modifier tests/GWGUI.Tests/InputBindingEditorCaptureTests.cs pour vérifier que le bouton et l’appel par identifiant sélectionnent la même ligne et produisent exactement la même association pour chaque source autorisée.
  - [ ] Relier les zones de l’image à leur ligne et à la capture
    - [ ] Ajouter le survol puis le clic simple
      - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualizer.cs et src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerOverlayRenderer.cs pour détecter la zone normalisée sous la souris et afficher un petit halo, ou le changement de couleur validé du halo, tant que la souris reste dessus.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualizer.cs pour émettre au clic simple l’identifiant exact de la commande de la zone sans attendre un double-clic et sans afficher de bouton supplémentaire sur l’image.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/EmulationControllerPortEditor.cs pour transmettre cet identifiant à InputBindingEditor, sélectionner la ligne correspondante et démarrer immédiatement sa capture.
      - [ ] Modifier tests/GWGUI.Tests/ControllerVisualizationTests.cs pour vérifier le hit-test après redimensionnement, le halo de survol et l’identifiant émis par chaque zone cliquée.
      - [ ] Modifier tests/GWGUI.Tests/InputBindingEditorCaptureTests.cs pour vérifier qu'un clic sur une zone accepte ensuite une touche de clavier, un signal de souris pris en charge et un bouton de n'importe quelle manette, sans présélection physique.
- [ ] Refonte partagée des halos et des commandes analogiques
  - [ ] Valider le rendu commun avant de remplacer les halos actuels
    - [ ] Produire des comparaisons visibles et consigner le choix retenu
      - [ ] Modifier tests/GWGUI.Tests/ControllerSignalVisualizationTests.cs pour générer une planche comparant sur plusieurs images physiques et émulées le halo neutre, le survol, l’appui numérique, le stick de manette, le manche de joystick et la gâchette aux valeurs testées.
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md après présentation de cette planche pour inscrire uniquement la forme, la couleur et l’opacité effectivement validées, sans considérer le recouvrement de l’image comme une erreur.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerOverlayRenderer.cs pour appliquer le halo numérique validé à la place des halos blancs généraux existants, sans ajouter de bordure, de couleur d’accent ou d’agrandissement non validé.
  - [ ] Remplacer le trait et le point du stick de manette
    - [ ] Déplacer un halo rond selon les deux axes
      - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerOverlayRenderer.cs pour dessiner sur un stick de manette un halo rond centré au repos puis déplacé proportionnellement dans la direction réellement fournie par ses axes.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualizer.Artwork.cs pour retirer le trait et le petit point analogiques seulement après que tous les profils physiques utilisent le halo rond commun.
      - [ ] Modifier tests/GWGUI.Tests/ControllerSignalVisualizationTests.cs pour vérifier le centre, les directions cardinales, les diagonales et plusieurs amplitudes sans jamais produire l’ancien trait avec point.
  - [ ] Unifier le principe du manche de joystick et de la gâchette
    - [ ] Créer une extension de halo ancrée au centre
      - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerOverlayRenderer.cs pour étendre progressivement un halo depuis le centre selon une direction et une amplitude fournies par le profil.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualZone.cs pour permettre au manche de joystick de fournir sa direction réelle et à la gâchette de fixer cette direction vers le bas avec la pression comme amplitude.
      - [ ] Modifier tests/GWGUI.Tests/ControllerSignalVisualizationTests.cs pour vérifier qu’un manche étend le halo du centre vers toutes ses directions et qu’une gâchette utilise le même calcul du centre vers le bas.
  - [ ] Déterminer puis appliquer le seuil avant tout changement visuel
    - [ ] Comparer plusieurs valeurs sans inventer le pourcentage final
      - [ ] Modifier tests/GWGUI.Tests/ControllerSignalVisualizationTests.cs pour générer une planche de seuils avec plusieurs pourcentages et plusieurs amplitudes proches du repos sur les périphériques physiques disponibles pendant la vérification.
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md après les essais pour inscrire le pourcentage commun retenu, les appareils essayés et le fait que la zone morte propre à une machine ou à un port reste prioritaire lorsqu’elle existe.
      - [ ] Modifier src/GWGUI.Emulation/Contracts/EmulationControllerPort.cs pour distinguer explicitement une zone morte configurée d’une absence de réglage, sans modifier la valeur fonctionnelle déjà utilisée par Atari.
      - [ ] Modifier src/GWGUI.Emulation.Atari/Functions/AtariInputSettingsFunctions.cs pour marquer le DeadZonePercent existant comme réglage configuré et src/GWGUI.Emulation.Amiga/Functions/AmigaInputSettingsFunctions.cs pour laisser le visualiseur utiliser le seuil commun tant qu’aucun réglage machine n’existe.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/EmulationBindingLiveMonitor.cs pour appliquer d’abord la zone morte configurée du port, sinon le seuil commun validé, avant de produire un changement visuel analogique.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualInput.cs pour appliquer le même seuil commun validé dans le visualiseur général Manettes.
      - [ ] Modifier tests/GWGUI.Tests/ControllerSignalVisualizationTests.cs et tests/GWGUI.Tests/EmulationBindingLiveMonitorTests.cs pour vérifier qu’aucun halo ne bouge sous le seuil, que le premier changement apparaît au seuil validé et que la zone morte du port est prioritaire.

- [ ] Validation complète du point 6 sur Amiga, Atari et le visualiseur général
  - [ ] Vérifier les profils contre les commandes réellement affichées
    - [ ] Compléter les contrôles automatiques du catalogue
      - [ ] Modifier tests/GWGUI.Tests/EmulatedControllerArtworkCatalogTests.cs pour parcourir chaque machine Amiga et Atari, demander ses périphériques existants et vérifier que chaque profil basique approuvé référence uniquement les commandes affichées par son InputBindingEditor.
      - [ ] Modifier tests/GWGUI.Tests/EmulatedControllerArtworkCatalogTests.cs pour vérifier que les variantes partageant un identifiant mais pas une forme utilisent des clés module-machine-périphérique distinctes.
      - [ ] Modifier tests/GWGUI.Tests/EmulatedControllerArtworkCatalogTests.cs pour vérifier qu’un périphérique non encore illustré ne provoque ni image incorrecte ni réutilisation automatique d’un autre profil.
  - [ ] Vérifier les interactions complètes de chaque port
    - [ ] Ajouter les scénarios WPF de bout en bout
      - [ ] Modifier tests/GWGUI.Tests/EmulationControllerSettingsLayoutTests.cs pour ouvrir successivement plusieurs ports Amiga et Atari et vérifier que le visuel, le tableau et le moniteur correspondent toujours au port actuellement ouvert.
      - [ ] Modifier tests/GWGUI.Tests/EmulationControllerSettingsLayoutTests.cs pour vérifier que le tableau garde sa place, que seul son contenu défile, que le bloc visuel reste fixe et que seule son image réduit sa taille lorsque la largeur disponible baisse.
      - [ ] Modifier tests/GWGUI.Tests/InputBindingEditorCaptureTests.cs pour cliquer chaque type de zone virtuelle, capturer plusieurs catégories de sources et vérifier que la ligne, l’association enregistrée et le halo en direct correspondent à la même commande.
      - [ ] Modifier tests/GWGUI.Tests/InputBindingEditorCaptureTests.cs pour vérifier qu’une manette déconnectée conserve l’affichage technique existant et retrouve son nom selon le comportement actuel après reconnexion et retour dans l’onglet.
  - [ ] Exécuter les validations ciblées puis générales
    - [ ] Corriger uniquement les régressions provoquées par le point 6
      - [ ] Exécuter ControllerVisualizationTests, ControllerSignalVisualizationTests, InputBindingEditorCaptureTests, EmulationBindingLiveMonitorTests, EmulatedControllerArtworkCatalogTests et EmulationControllerSettingsLayoutTests, puis corriger uniquement les erreurs introduites par le moteur commun, le visuel émulé ou la capture partagée.
      - [ ] Exécuter AmigaControllerMappingTests, AtariControllerMappingTests, AmigaConfigurationStoreTests et AtariConfigurationStoreTests, puis corriger uniquement les régressions provoquées par la résolution par association ou le retrait du sélecteur physique.
      - [ ] Exécuter la suite GWGUI.Tests et corriger uniquement les régressions causées par les fichiers et comportements modifiés dans cette checklist.
      - [ ] Vérifier manuellement un joystick Amiga, une manette CD32 et les périphériques Atari basiques approuvés avec plusieurs touches ou boutons maintenus, les axes proches du repos, les changements de port, le défilement du tableau et la capture déclenchée depuis l’image.
      - [ ] Vérifier manuellement dans l’onglet général Manettes que les images physiques, les appuis simultanés et les nouveaux halos utilisent bien le même moteur sans copie du visualiseur.

## Checklist détaillée — Point 7 : recherche et architecture des filtres vidéo

- [ ] Catalogue vérifié des traitements vidéo envisageables
  - [ ] Créer le document de recherche avant d’y inscrire les effets
    - [ ] Créer le fichier de catalogue
      - [ ] Créer docs/EMULATION_VIDEO_FILTER_CATALOG.md.
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md pour ajouter les sections Sources, Licence, Famille, Effet, Paramètres, Combinaisons, Coût, Backends et Décision, sans déclarer encore de filtre retenu.
  - [ ] Inventorier les collections officielles et chaque dépendance réellement examinée
    - [ ] Ajouter les sources et leur portée exacte dans le catalogue
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md pour référencer la documentation officielle des shaders Libretro, le dépôt libretro/slang-shaders, sa spécification Slang et le dépôt libretro/common-shaders avec leur URL directe et leur date de consultation.
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md pour inventorier les familles disponibles utiles à GW GUI, notamment mise à l’échelle, netteté et défloutage, anti-aliasing, scanlines, CRT, LCD, masque, courbure, moiré, glow, traitement du dithering, désentrelacement et traitements analogiques, sans considérer cette liste comme une sélection.
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md pour décrire les capacités nécessaires à chaque candidat : nombre de passes, textures intermédiaires, historique d’images, textures LUT, taille de sortie, filtrage du sampler, paramètres exposés et dépendances incluses.
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md pour relever la licence dans chaque fichier de shader, préréglage, include et texture dont une reprise est envisagée, sans attribuer automatiquement une licence unique à toute la collection Libretro.
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md pour classer chaque candidat entre référence visuelle seulement, reproduction à écrire dans GW GUI, reprise directe compatible avec la licence du projet, reprise imposant des obligations supplémentaires et candidat à exclure.
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md pour conserver RGB, composite, S-Video, RF, PAL et NTSC comme options d’émulateur lorsqu’ils sont déjà fournis par celui-ci et ne classer un effet inspiré d’un signal parmi les traitements GW GUI que s’il est distinct et explicitement validé.
  - [ ] Inventorier les effets déjà exposés par les émulateurs de GW GUI
    - [ ] Écrire le résultat de l’inventaire dans le même catalogue
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md à partir de src/GWGUI.Emulation.Amiga/Functions/AmigaSettingsDescriptionFunctions.cs pour lister les réglages vidéo Amiga déjà transmis à l’émulateur et les exclure des doublons GW GUI.
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md à partir de src/GWGUI.Emulation.Atari/Functions/AtariSettingsDescriptionFunctions.cs et des catalogues Atari utilisés par chaque famille pour lister les réglages déjà transmis aux émulateurs et les exclure des doublons GW GUI.
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md pour identifier séparément les réglages généraux d’image demandés — luminosité, contraste, gamma, saturation et netteté — qui seront appliqués hors émulateur.
  - [ ] Construire les familles logiques et la matrice de compatibilité avant l’interface
    - [ ] Inscrire toutes les combinaisons à faire valider
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md pour regrouper les effets qui représentent des choix exclusifs d’une même fonction, sans autoriser simultanément deux facteurs d’un même algorithme d’agrandissement ou deux technologies d’écran incompatibles.
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md pour lister séparément les effets complémentaires susceptibles d’être cumulés, comme un rendu CRT et des scanlines, sans valider automatiquement toutes les combinaisons possibles.
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md pour établir une matrice symétrique indiquant pour chaque paire : compatible, incompatible, dépendante d’un ordre précis ou encore à mesurer.
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md pour définir l’ordre de traitement proposé uniquement pour les combinaisons techniquement justifiées et laisser les autres ordres non retenus.
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md après validation pour marquer les effets de la première implémentation, leurs présélections de base et les réglages modifiables, sans créer les autres effets.

- [ ] Audit écrit des quatre chemins de rendu actuels
  - [ ] Consigner les formats et responsabilités avant de choisir l’architecture
    - [ ] Ajouter la section d’architecture actuelle au catalogue
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md à partir de src/GWGUI.Emulation/Contracts/VideoFrame.cs pour documenter les formats de pixels, le pitch, le rapport d’aspect, la séquence et le timestamp disponibles à l’entrée du rendu.
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md à partir de src/GWGUI.App/Presenters/Emulation/Machine/MachineVideoPresenter.cs pour documenter la remise de la dernière image, le repli automatique vers WPF, le changement de surface et le calcul actuel de la fréquence d’images.
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md à partir de src/GWGUI.App/Rendering/Emulation/Surfaces/WpfVideoSurface.cs pour documenter la conversion BGRA32 et l’écriture directe dans le WriteableBitmap.
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md à partir de src/GWGUI.App/Rendering/Emulation/Surfaces/OpenGlVideoSurface.cs pour documenter le contexte WGL, glDrawPixels, l’absence actuelle de texture et l’absence de chaîne de shaders.
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md à partir de src/GWGUI.App/Rendering/Emulation/Surfaces/VeldridVideoSurface.cs pour documenter la texture source, le quad, le shader actuel, la pipeline commune Direct3D 11/Vulkan et la recréation des ressources quand la taille change.
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md pour noter que Snapshot contient actuellement les pixels sources dans les trois surfaces et qu’aucun changement vers une capture filtrée ne sera réalisé sans validation séparée.
  - [ ] Établir la matrice des possibilités par backend
    - [ ] Ajouter les capacités et les lacunes sans promettre un résultat identique non testé
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md pour indiquer, effet par effet, si WPF, OpenGL, Direct3D 11 et Vulkan peuvent exécuter le traitement sur GPU, sur CPU, avec plusieurs passes, avec historique et avec LUT dans l’architecture actuelle ou après une évolution identifiée.
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md pour distinguer un même réglage logique commun de ses implémentations propres aux backends et ne pas imposer un processeur CPU unique avant toutes les surfaces.
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md pour définir les règles de repli à faire valider lorsqu’un backend ne peut pas exécuter un effet : effet désactivé, traitement CPU accepté ou changement de moteur explicitement demandé, sans choisir silencieusement à la place de l’utilisateur.

- [ ] Essais mesurés avant le choix des implémentations de backend
  - [ ] Créer le fichier de tests de capacité avant ses scénarios
    - [ ] Créer le banc d’essai vidéo
      - [ ] Créer tests/GWGUI.Tests/EmulationVideoBackendCapabilityTests.cs.
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoBackendCapabilityTests.cs pour produire dans le test une mire BGRA32 commune, sans ajouter de fichier binaire de référence avant d’en avoir besoin.
  - [ ] Mesurer les chemins actuels sans les remplacer pendant l’essai
    - [ ] Ajouter les scénarios de surface et écrire leurs résultats dans le catalogue
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoBackendCapabilityTests.cs pour présenter la même mire non filtrée avec WpfVideoSurface, OpenGlVideoSurface et VeldridVideoSurface en Direct3D 11 puis Vulkan, et vérifier les dimensions, le rapport d’aspect et le repli déjà géré par MachineVideoPresenter.
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoBackendCapabilityTests.cs pour mesurer séparément le coût de conversion BGRA32, le transfert de texture et la présentation d’une image aux résolutions réellement utilisées par les machines existantes.
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoBackendCapabilityTests.cs pour essayer un réglage général simple, un passage à texture intermédiaire et une chaîne de deux passages dans chaque backend uniquement lorsque le chemin actuel le permet.
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoBackendCapabilityTests.cs pour rendre explicite l’impossibilité actuelle de la chaîne OpenGL tant que glDrawPixels n’a pas été remplacé, au lieu de déclarer ce backend compatible sans preuve.
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md avec les résultats mesurés, les appareils graphiques et pilotes essayés, le coût par backend et les limites observées dans les essais précédents.
  - [ ] Valider l’architecture de WPF et d’OpenGL avant les modifications de production
    - [ ] Inscrire le choix puis détailler uniquement la branche retenue
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md pour comparer, d’après les essais, le traitement CPU et les possibilités WPF réellement utilisables, puis inscrire l’option retenue pour WPF avec ses limites.
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md pour comparer le remplacement de glDrawPixels par un quad texturé OpenGL et toute solution commune réellement prouvée par les essais, puis inscrire l’option retenue pour OpenGL.
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md après validation de ces deux choix pour ajouter sous ce point les fichiers exacts à créer ou modifier pour les backends WPF et OpenGL retenus, avant toute modification de leur code de production.
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoBackendCapabilityTests.cs pour transformer les essais retenus en tests de non-régression et retirer uniquement les branches expérimentales explicitement refusées.

- [ ] Contrats communs de configuration vidéo par machine
  - [ ] Créer les valeurs persistantes avant les catalogues d’exécution
    - [ ] Créer les fichiers des trois contrats communs
      - [ ] Créer src/GWGUI.Emulation/Contracts/EmulationVideoProcessingConfiguration.cs.
      - [ ] Modifier src/GWGUI.Emulation/Contracts/EmulationVideoProcessingConfiguration.cs pour porter les réglages généraux, les fonctionnalités configurées et leur ordre validé sans dépendre d’Amiga, d’Atari ou d’un backend graphique.
      - [ ] Créer src/GWGUI.Emulation/Contracts/EmulationVideoFeatureConfiguration.cs.
      - [ ] Modifier src/GWGUI.Emulation/Contracts/EmulationVideoFeatureConfiguration.cs pour enregistrer l’identifiant stable d’une fonctionnalité, son activation, sa présélection éventuelle et uniquement ses paramètres validés.
      - [ ] Créer src/GWGUI.Emulation/Contracts/EmulationImageAdjustmentConfiguration.cs.
      - [ ] Modifier src/GWGUI.Emulation/Contracts/EmulationImageAdjustmentConfiguration.cs pour définir luminosité, contraste, saturation et netteté neutres à 0, leurs bornes -10 à +10 et le gamma selon la représentation validée dans docs/EMULATION_VIDEO_FILTER_CATALOG.md.
  - [ ] Ajouter la configuration commune aux deux formats de machine
    - [ ] Étendre l’interface puis les contrats concrets
      - [ ] Modifier src/GWGUI.Emulation/Interfaces/IEmulationConfiguration.cs pour exposer une EmulationVideoProcessingConfiguration commune en plus du moteur de rendu déjà enregistré.
      - [ ] Modifier src/GWGUI.Emulation.Amiga/Contracts/AmigaMachineConfiguration.cs pour enregistrer la configuration vidéo commune avec des valeurs neutres lorsque la propriété est absente.
      - [ ] Modifier src/GWGUI.Emulation.Atari/Contracts/AtariMachineConfiguration.cs pour enregistrer la même configuration commune sans créer une variante Atari des effets.
  - [ ] Migrer les anciens fichiers sans modifier leurs autres réglages
    - [ ] Mettre à jour les versions et les documents de stockage
      - [ ] Modifier src/GWGUI.Emulation.Amiga/Services/AmigaConfigurationStore.cs et src/GWGUI.Emulation.Amiga/Contracts/AmigaMachineConfiguration.cs pour accepter les versions actuelles, enregistrer la prochaine version de schéma disponible et ajouter uniquement une configuration vidéo neutre aux anciens fichiers.
      - [ ] Modifier src/GWGUI.Emulation.Atari/Constants/AtariConstants.cs pour augmenter CurrentConfigurationSchemaVersion d’une version au moment de cette migration.
      - [ ] Modifier src/GWGUI.Emulation.Atari/Contracts/AtariConfigurationDocument.cs et src/GWGUI.Emulation.Atari/Functions/AtariConfigurationStoreFunctions.cs pour sérialiser et restaurer EmulationVideoProcessingConfiguration.
      - [ ] Modifier src/GWGUI.Emulation.Atari/Functions/AtariConfigurationMigrationFunctions.cs pour migrer explicitement le schéma précédent vers la nouvelle version avec une configuration vidéo neutre, sans modifier les firmwares, médias, options, entrées, dossiers, audio ni moteur de rendu.
  - [ ] Verrouiller les valeurs et migrations avant le moteur de filtres
    - [ ] Créer le fichier de tests commun avant ses scénarios
      - [ ] Créer tests/GWGUI.Tests/EmulationVideoProcessingConfigurationTests.cs.
    - [ ] Ajouter les scénarios communs, Amiga et Atari
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoProcessingConfigurationTests.cs pour vérifier les valeurs neutres, les bornes validées, les identifiants de fonctionnalités, les paramètres et l’ordre sérialisé.
      - [ ] Modifier tests/GWGUI.Tests/AmigaConfigurationStoreTests.cs pour vérifier le chargement d’un fichier du schéma précédent, l’ajout neutre et le cycle chargement-enregistrement sans perte des autres données.
      - [ ] Modifier tests/GWGUI.Tests/AtariConfigurationStoreTests.cs pour vérifier la migration explicite du schéma précédent et le même cycle sans perte.

- [ ] Graphe logique partagé sans imposer une implémentation graphique unique
  - [ ] Créer les descriptions d’effet à partir de la sélection validée
    - [ ] Créer les fichiers du catalogue d’exécution
      - [ ] Créer src/GWGUI.App/Contracts/Rendering/Emulation/VideoFilters/EmulationVideoEffectDefinition.cs.
      - [ ] Modifier src/GWGUI.App/Contracts/Rendering/Emulation/VideoFilters/EmulationVideoEffectDefinition.cs pour décrire l’identifiant, les paramètres, les valeurs par défaut, les dépendances, les incompatibilités et les capacités nécessaires d’un effet retenu.
      - [ ] Créer src/GWGUI.App/Contracts/Rendering/Emulation/VideoFilters/EmulationVideoPassDefinition.cs.
      - [ ] Modifier src/GWGUI.App/Contracts/Rendering/Emulation/VideoFilters/EmulationVideoPassDefinition.cs pour décrire une passe, ses entrées, sa taille de sortie, son sampler, son historique et ses textures auxiliaires sans contenir de ressource propre à une machine.
      - [ ] Créer src/GWGUI.App/Rendering/Emulation/VideoFilters/EmulationVideoEffectCatalog.cs.
      - [ ] Modifier src/GWGUI.App/Rendering/Emulation/VideoFilters/EmulationVideoEffectCatalog.cs pour n’inscrire que les effets marqués comme retenus dans docs/EMULATION_VIDEO_FILTER_CATALOG.md.
      - [ ] Créer src/GWGUI.App/Rendering/Emulation/VideoFilters/EmulationVideoFilterGraphBuilder.cs.
      - [ ] Modifier src/GWGUI.App/Rendering/Emulation/VideoFilters/EmulationVideoFilterGraphBuilder.cs pour valider la configuration, ordonner les passes compatibles et produire le même graphe logique pour tous les backends capables de l’exécuter.
  - [ ] Centraliser les incompatibilités utilisées par l’interface et le rendu
    - [ ] Ajouter une seule validation du graphe
      - [ ] Créer src/GWGUI.App/Rendering/Emulation/VideoFilters/EmulationVideoConfigurationValidator.cs.
      - [ ] Modifier src/GWGUI.App/Rendering/Emulation/VideoFilters/EmulationVideoConfigurationValidator.cs pour retourner les fonctionnalités incompatibles, les paramètres hors bornes et les capacités absentes du backend actif.
      - [ ] Modifier src/GWGUI.App/Rendering/Emulation/VideoFilters/EmulationVideoFilterGraphBuilder.cs pour refuser un graphe invalide par cette validation sans désactiver silencieusement un autre effet.
      - [ ] Créer tests/GWGUI.Tests/EmulationVideoFilterGraphTests.cs avant d’y ajouter les scénarios du graphe.
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoFilterGraphTests.cs pour vérifier les effets seuls, les combinaisons validées, l’ordre des passes, les incompatibilités symétriques, les dépendances et les capacités de backend.
- [ ] Exécution des graphes dans les surfaces vidéo
  - [ ] Créer l’interface de backend avant les implémentations
    - [ ] Créer les contrats d’exécution et de capacité
      - [ ] Créer src/GWGUI.App/Interfaces/Rendering/Emulation/VideoFilters/IEmulationVideoFilterBackend.cs.
      - [ ] Modifier src/GWGUI.App/Interfaces/Rendering/Emulation/VideoFilters/IEmulationVideoFilterBackend.cs pour configurer un graphe validé, présenter une VideoFrame et libérer toutes ses ressources.
      - [ ] Créer src/GWGUI.App/Contracts/Rendering/Emulation/VideoFilters/EmulationVideoBackendCapabilities.cs.
      - [ ] Modifier src/GWGUI.App/Contracts/Rendering/Emulation/VideoFilters/EmulationVideoBackendCapabilities.cs pour déclarer les passes, l’historique, les LUT, les formats, les tailles et les types de sampler réellement pris en charge.
      - [ ] Modifier src/GWGUI.App/Interfaces/Rendering/Emulation/IEmulationVideoSurface.cs pour recevoir une EmulationVideoProcessingConfiguration et exposer ses capacités sans déplacer la réception de VideoFrame hors de la surface.
  - [ ] Préparer les ressources retenues sans reprendre un shader non validé
    - [ ] Créer les emplacements avant le premier fichier de production
      - [ ] Créer le dossier src/GWGUI.App/Assets/VideoFilters/Shaders.
      - [ ] Créer le dossier src/GWGUI.App/Assets/VideoFilters/Textures seulement si le premier effet retenu dépend réellement d’une LUT ou d’une texture auxiliaire.
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md après la sélection des effets pour inscrire, avant leur création, le chemin exact de chaque shader, include, préréglage et texture à ajouter ainsi que sa source et sa licence vérifiées.
      - [ ] Créer THIRD-PARTY-NOTICES.md seulement si le premier fichier tiers directement repris exige une notice dans le projet.
      - [ ] Modifier THIRD-PARTY-NOTICES.md pour inscrire la provenance, l’auteur, la licence et les mentions exigées avant d’intégrer chaque fichier concerné.
      - [ ] Ajouter dans AMELIORATIONS_INTERFACE_EMULATION.md, pour chaque ressource retenue, une action Créer avec son chemin exact puis une action Modifier distincte décrivant son contenu, avant d’exécuter ces deux actions.
      - [ ] Modifier src/GWGUI.App/GWGUI.App.csproj après la création du premier fichier pour embarquer seulement les shaders, includes et textures réellement utilisés et leur notice éventuelle.
  - [ ] Étendre la surface Veldrid commune à Direct3D 11 et Vulkan
    - [ ] Créer l’exécuteur Veldrid avant de déplacer le shader actuel
      - [ ] Créer src/GWGUI.App/Rendering/Emulation/VideoFilters/VeldridVideoFilterBackend.cs.
      - [ ] Modifier src/GWGUI.App/Rendering/Emulation/VideoFilters/VeldridVideoFilterBackend.cs pour compiler les passes retenues avec Veldrid.SPIRV, créer leurs pipelines, textures intermédiaires, samplers, buffers de paramètres et ressources auxiliaires.
      - [ ] Modifier src/GWGUI.App/Rendering/Emulation/Surfaces/VeldridVideoSurface.cs pour copier d’abord son quad, sa texture source et son passage actuel vers VeldridVideoFilterBackend en conservant exactement le rendu non filtré.
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoBackendCapabilityTests.cs pour vérifier le passage neutre Direct3D 11 et Vulkan avant de retirer les ressources correspondantes de VeldridVideoSurface.
      - [ ] Modifier src/GWGUI.App/Rendering/Emulation/Surfaces/VeldridVideoSurface.cs pour retirer son ancien pipeline interne seulement après que VeldridVideoFilterBackend présente correctement le passage neutre sur les deux backends.
    - [ ] Ajouter les passes retenues après le passage neutre
      - [ ] Modifier src/GWGUI.App/Rendering/Emulation/VideoFilters/VeldridVideoFilterBackend.cs pour allouer et réutiliser les textures intermédiaires selon le graphe, puis les recréer uniquement lors d’un changement de dimensions ou de graphe qui le nécessite.
      - [ ] Modifier src/GWGUI.App/Rendering/Emulation/VideoFilters/VeldridVideoFilterBackend.cs pour fournir aux shaders les tailles source et sortie, le numéro et le temps de l’image, les paramètres utilisateur, les historiques et les LUT uniquement lorsque la définition de passe les demande.
      - [ ] Modifier src/GWGUI.App/Rendering/Emulation/VideoFilters/VeldridVideoFilterBackend.cs pour appliquer la dernière passe au framebuffer de la swapchain et conserver le noir hors de l’image ajustée au rapport d’aspect.
      - [ ] Modifier src/GWGUI.App/Rendering/Emulation/Surfaces/VeldridVideoSurface.cs pour transmettre les changements de configuration au backend sans recréer le GraphicsDevice lorsque les capacités et les ressources existantes le permettent.
  - [ ] Réaliser uniquement les branches WPF et OpenGL validées par les essais
    - [ ] Exécuter les tâches exactes ajoutées après la décision technique
      - [ ] Modifier les fichiers WPF inscrits précédemment dans AMELIORATIONS_INTERFACE_EMULATION.md pour exécuter le graphe avec l’approche validée et conserver WpfVideoSurface comme repli fonctionnel.
      - [ ] Modifier les fichiers OpenGL inscrits précédemment dans AMELIORATIONS_INTERFACE_EMULATION.md pour remplacer d’abord glDrawPixels par le chemin neutre validé, vérifier le même affichage, puis ajouter les passes sans supprimer l’ancien chemin avant cette vérification.
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoBackendCapabilityTests.cs pour vérifier les capacités réellement annoncées par WPF et OpenGL et refuser un effet qui exige une capacité absente.
  - [ ] Conserver le repli et les changements de moteur
    - [ ] Transmettre la configuration aux surfaces successives
      - [ ] Modifier src/GWGUI.App/Factories/Rendering/Emulation/EmulationVideoSurfaceFactory.cs pour construire chaque surface avec ses capacités et son backend de filtres validé.
      - [ ] Modifier src/GWGUI.App/Presenters/Emulation/Machine/MachineVideoPresenter.cs pour conserver la configuration vidéo courante, l’appliquer à la nouvelle surface lors d’un changement de moteur et l’appliquer également à WPF lors du repli après erreur.
      - [ ] Modifier src/GWGUI.App/Presenters/Emulation/Machine/MachineVideoPresenter.cs pour laisser Snapshot représenter les pixels sources comme aujourd’hui jusqu’à une décision explicite concernant les captures filtrées.
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoBackendCapabilityTests.cs pour vérifier qu’un changement Direct3D 11, Vulkan, OpenGL ou WPF ne perd pas la configuration et qu’un repli n’active jamais silencieusement un effet non pris en charge.

- [ ] Réglages généraux et effets retenus
  - [ ] Implémenter d’abord un passage entièrement neutre
    - [ ] Ajouter les réglages toujours disponibles sans altérer l’image par défaut
      - [ ] Modifier src/GWGUI.App/Rendering/Emulation/VideoFilters/EmulationVideoEffectCatalog.cs pour déclarer le traitement commun de luminosité, contraste, gamma, saturation et netteté avec les valeurs neutres et les bornes validées.
      - [ ] Exécuter les actions Modifier ajoutées précédemment dans AMELIORATIONS_INTERFACE_EMULATION.md pour les fichiers exacts du traitement général, puis implémenter les cinq réglages dans l’ordre validé sans reproduire une option interne de l’émulateur.
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoFilterGraphTests.cs pour vérifier qu’une configuration entièrement neutre conserve chaque pixel de la mire et que les valeurs positives et négatives agissent uniquement sur le réglage demandé.
  - [ ] Ajouter chaque famille retenue dans l’ordre du catalogue
    - [ ] Terminer un effet avant de commencer le suivant
      - [ ] Modifier src/GWGUI.App/Rendering/Emulation/VideoFilters/EmulationVideoEffectCatalog.cs pour ajouter un effet retenu seulement après la création de toutes ses passes et ressources validées.
      - [ ] Modifier le backend concerné dans src/GWGUI.App/Rendering/Emulation/VideoFilters après chaque nouvel effet pour prendre en charge ses capacités sans recopier sa définition logique dans chaque surface.
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoFilterGraphTests.cs et tests/GWGUI.Tests/EmulationVideoBackendCapabilityTests.cs après chaque effet pour vérifier ses paramètres, sa valeur neutre, ses passes, ses combinaisons autorisées et ses backends réellement pris en charge.
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md après chaque effet terminé pour enregistrer son état réel, son coût mesuré et les écarts éventuels entre backends avant de cocher sa famille.
- [ ] Interface commune dans l’onglet Vidéo de la machine
  - [ ] Valider les derniers textes et formats avant les ressources
    - [ ] Inscrire les choix manquants dans le document principal
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md pour inscrire les deux titres retenus pour distinguer les options commandant l’émulateur et les traitements appliqués hors émulateur, sans utiliser comme titres définitifs les formulations déjà refusées.
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md pour inscrire la représentation, la valeur neutre, les bornes et le pas retenus pour le gamma à partir de l’implémentation validée.
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md pour inscrire les libellés courts retenus pour la liste des fonctionnalités, l’activation, les présélections et la confirmation d’incompatibilité.
  - [ ] Créer le gestionnaire commun avant le contrôle WPF
    - [ ] Créer les fichiers de gestion de configuration vidéo
      - [ ] Créer src/GWGUI.Emulation/Interfaces/IEmulationVideoProcessingSettingsManager.cs.
      - [ ] Modifier src/GWGUI.Emulation/Interfaces/IEmulationVideoProcessingSettingsManager.cs pour remplacer la configuration vidéo commune dans une IEmulationConfiguration sans exposer ses types Amiga ou Atari à l’application.
      - [ ] Modifier src/GWGUI.Emulation.Amiga/Modules/AmigaEmulationModule.cs pour implémenter IEmulationVideoProcessingSettingsManager en remplaçant uniquement EmulationVideoProcessingConfiguration.
      - [ ] Modifier src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs pour implémenter le même contrat sans recopier les définitions d’effets.
      - [ ] Créer src/GWGUI.App/Controllers/Emulation/Options/EmulationVideoProcessingSettingsController.cs.
      - [ ] Modifier src/GWGUI.App/Controllers/Emulation/Options/EmulationVideoProcessingSettingsController.cs pour charger la configuration, valider les changements avec EmulationVideoConfigurationValidator, demander confirmation en cas d’incompatibilité et produire la configuration modifiée.
  - [ ] Créer le contrôle commun avant de l’insérer dans les onglets
    - [ ] Construire la liste, le panneau et les réglages permanents
      - [ ] Créer src/GWGUI.App/Views/Controls/Emulation/Options/EmulationVideoProcessingSettingsControl.cs.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationVideoProcessingSettingsControl.cs pour afficher la grande liste des fonctionnalités retenues et le panneau de la fonctionnalité actuellement choisie sans activer celle-ci par sa seule sélection.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationVideoProcessingSettingsControl.cs pour afficher dans le panneau l’activation, les présélections validées et seulement les paramètres de cette fonctionnalité.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationVideoProcessingSettingsControl.cs pour garder toujours visibles luminosité, contraste, gamma, saturation et netteté, initialisés à leur valeur neutre et réglables dans leurs bornes validées.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationVideoProcessingSettingsControl.cs pour utiliser les aides courtes et détaillées du système créé au point 4 sur les champs techniques qui ne sont pas compréhensibles par leur nom seul.
      - [ ] Modifier src/GWGUI.App/Controllers/Emulation/Options/EmulationVideoProcessingSettingsController.cs pour ne désactiver les fonctionnalités incompatibles qu’après une réponse Oui à la confirmation et conserver exactement la combinaison précédente après Non.
  - [ ] Séparer les réglages selon le code qu’ils commandent
    - [ ] Étendre les blocs de description avant de modifier leur disposition
      - [ ] Créer src/GWGUI.Emulation/Enums/EmulationSettingsProcessingScope.cs.
      - [ ] Modifier src/GWGUI.Emulation/Enums/EmulationSettingsProcessingScope.cs pour distinguer un réglage transmis à l’émulateur d’un traitement exécuté hors émulateur.
      - [ ] Modifier src/GWGUI.Emulation/Contracts/EmulationSettingsBlock.cs pour porter EmulationSettingsProcessingScope avec la portée émulateur comme valeur compatible avec les descriptions existantes.
      - [ ] Modifier src/GWGUI.Emulation.Amiga/Functions/AmigaSettingsDescriptionFunctions.cs pour marquer les blocs Vidéo et Audio existants d’après le code qu’ils commandent, sans déplacer ni renommer leurs champs.
      - [ ] Modifier src/GWGUI.Emulation.Atari/Functions/AtariSettingsDescriptionFunctions.cs pour effectuer le même classement sur chaque famille Atari sans créer de cadre vide.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour regrouper visuellement les blocs Vidéo et Audio par portée avec les titres validés, et n’afficher deux cadres que lorsque les deux portées contiennent réellement des réglages.
  - [ ] Insérer le contrôle uniquement dans Vidéo
    - [ ] Raccorder l’éditeur de machine au gestionnaire commun
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour créer EmulationVideoProcessingSettingsController uniquement si le module implémente IEmulationVideoProcessingSettingsManager.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour ajouter EmulationVideoProcessingSettingsControl dans le cadre hors émulateur de l’onglet Vidéo et ne l’ajouter ni dans Général, ni dans l’écran d’émulation, ni dans un autre onglet.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour faire passer chaque changement vidéo par le chemin d’enregistrement automatique créé au point 2 lorsque la configuration existe et ne rien écrire tant que la machine n’a pas été créée.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour reconstruire le contrôle avec la configuration de la machine nouvellement chargée sans conserver les effets de la machine précédente.

- [ ] Ressources et aides dans toutes les langues existantes
  - [ ] Créer les clés validées dans la ressource neutre avant leurs traductions
    - [ ] Ajouter tous les textes du point 7 à la base
      - [ ] Modifier src/GWGUI.App/Resources/00-Base/Emulation.resx pour ajouter les titres validés, les libellés des réglages généraux, les fonctionnalités et présélections retenues, la confirmation d’incompatibilité ainsi que les aides courtes et détaillées des champs concernés.
  - [ ] Traduire les mêmes clés sans ajouter de texte directement dans le code
    - [ ] Modifier chaque catalogue de langue existant
      - [ ] Modifier src/GWGUI.App/Resources/ar-SA/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/cs-CZ/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/da-DK/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/de-DE/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/el-GR/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/en-US/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/es-ES/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/fi-FI/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/fr-FR/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/he-IL/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/hu-HU/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/id-ID/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/it-IT/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/ja-JP/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/ko-KR/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/nb-NO/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/nl-NL/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/pl-PL/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/pt-BR/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/pt-PT/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/ro-RO/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/ru-RU/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/sv-SE/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/th-TH/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/tr-TR/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/uk-UA/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/vi-VN/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/zh-Hans/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/zh-Hant/Emulation.resx.
- [ ] Enregistrement immédiat et application à l’instance correspondante
  - [ ] Fournir la configuration enregistrée lors de l’ouverture d’une machine
    - [ ] Étendre les options avant le constructeur du présentateur
      - [ ] Modifier src/GWGUI.App/Contracts/Machine/MachineControllerOptions.cs pour transporter EmulationVideoProcessingConfiguration avec VideoRenderer.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationSectionMachineFunctions.cs pour fournir à chaque nouvelle instance les traitements enregistrés dans sa propre configuration.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs pour transmettre cette configuration à MachineVideoPresenter lors de sa création.
      - [ ] Modifier src/GWGUI.App/Presenters/Emulation/Machine/MachineVideoPresenter.cs pour appliquer la configuration à la surface avant de présenter sa première image.
  - [ ] Appliquer en direct uniquement les traitements hors émulateur
    - [ ] Raccorder l’événement de sauvegarde à la bonne machine ouverte
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationSectionConfigurationFunctions.cs pour transmettre la nouvelle EmulationVideoProcessingConfiguration uniquement au MachineController dont le ModuleId et l’Id correspondent à la configuration sauvegardée.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs pour ajouter ApplyVideoProcessingConfiguration et transmettre le changement au présentateur sans redémarrer, réinitialiser ou appeler l’émulateur.
      - [ ] Modifier src/GWGUI.App/Presenters/Emulation/Machine/MachineVideoPresenter.cs pour reconfigurer la surface active sur le thread d’interface et présenter les images suivantes avec le nouveau graphe.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationSectionConfigurationFunctions.cs pour laisser les autres modifications internes à l’émulateur suivre leur comportement actuel et ne pas leur appliquer le direct prévu uniquement pour les traitements externes.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationSectionConfigurationFunctions.cs pour ne faire aucune application en direct lorsqu’aucune instance correspondante n’est ouverte, la configuration sauvegardée étant alors utilisée au prochain démarrage.
  - [ ] Conserver les réglages pendant les changements de surface
    - [ ] Vérifier toutes les transitions déjà possibles
      - [ ] Modifier src/GWGUI.App/Presenters/Emulation/Machine/MachineVideoPresenter.cs pour réappliquer la dernière configuration après ApplyVideoRenderer, une recréation de taille ou un repli WPF, sans réinitialiser les valeurs choisies dans l’éditeur.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs pour ne jamais envoyer la configuration d’une autre machine lorsqu’un autre onglet devient actif.

- [ ] Tests fonctionnels, visuels, de licence et de performances
  - [ ] Verrouiller l’interface et la confirmation des incompatibilités
    - [ ] Créer le fichier de tests WPF avant ses scénarios
      - [ ] Créer tests/GWGUI.Tests/EmulationVideoProcessingSettingsTests.cs.
    - [ ] Ajouter les scénarios de l’éditeur commun
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoProcessingSettingsTests.cs pour vérifier que le contrôle apparaît seulement dans Vidéo, que les réglages généraux restent visibles et que la sélection d’une fonctionnalité affiche son panneau sans l’activer.
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoProcessingSettingsTests.cs pour vérifier les présélections, les paramètres, les valeurs neutres et la reconstruction complète après changement de machine.
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoProcessingSettingsTests.cs pour vérifier que Oui désactive exactement les fonctionnalités incompatibles annoncées et que Non ne modifie aucune activation.
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoProcessingSettingsTests.cs pour vérifier qu’une configuration existante est sauvegardée à chaque changement et qu’un brouillon non créé n’écrit aucun fichier.
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoProcessingSettingsTests.cs pour vérifier la séparation des blocs Vidéo et Audio selon EmulationSettingsProcessingScope sans afficher un cadre vide.
  - [ ] Verrouiller l’application en direct et l’isolation des instances
    - [ ] Créer le fichier de tests avant ses scénarios
      - [ ] Créer tests/GWGUI.Tests/EmulationVideoLiveUpdateTests.cs.
    - [ ] Ajouter les scénarios de machine ouverte et fermée
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoLiveUpdateTests.cs pour vérifier qu’un changement hors émulateur reconfigure l’instance correspondante sans recréer sa machine.
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoLiveUpdateTests.cs pour vérifier qu’aucune autre configuration ou instance ouverte ne reçoit le changement.
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoLiveUpdateTests.cs pour vérifier qu’une machine fermée utilise les valeurs sauvegardées à son prochain démarrage.
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoLiveUpdateTests.cs pour vérifier que les options internes à l’émulateur ne sont pas appliquées en direct par ce nouveau chemin.
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoLiveUpdateTests.cs pour vérifier la conservation de la configuration après changement de backend et repli vers WPF.
  - [ ] Vérifier chaque effet avec des images de référence approuvées
    - [ ] Créer le fichier de tests et le dossier avant les références
      - [ ] Créer tests/GWGUI.Tests/EmulationVideoRenderingTests.cs.
      - [ ] Créer le dossier tests/GWGUI.Tests/Fixtures/VideoFilters.
    - [ ] Ajouter les références seulement après validation visuelle
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md pour inscrire le chemin exact de chaque image de référence approuvée avant sa création dans tests/GWGUI.Tests/Fixtures/VideoFilters.
      - [ ] Créer chaque image de référence inscrite avec la mire, l’effet, la présélection, les paramètres, la taille d’entrée et la taille de sortie indiqués dans son nom ou son manifeste.
      - [ ] Modifier tests/GWGUI.Tests/GWGUI.Tests.csproj après la première image pour copier uniquement les références effectivement créées dans le répertoire de tests.
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoRenderingTests.cs pour comparer la sortie neutre pixel à pixel et les effets à leur référence avec la tolérance validée pour chaque backend, sans exiger arbitrairement des pixels identiques entre API graphiques différentes.
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoRenderingTests.cs pour vérifier les tailles variables, les rapports d’aspect, les changements de résolution, les combinaisons retenues et plusieurs images consécutives pour les effets utilisant l’historique.
  - [ ] Vérifier la traçabilité de chaque ressource tierce
    - [ ] Créer le test de correspondance des licences
      - [ ] Créer tests/GWGUI.Tests/EmulationVideoAssetLicenseTests.cs.
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoAssetLicenseTests.cs pour vérifier que chaque shader, include, préréglage et texture directement repris possède une entrée source-licence dans docs/EMULATION_VIDEO_FILTER_CATALOG.md et, lorsqu’elle est exigée, dans THIRD-PARTY-NOTICES.md.
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoAssetLicenseTests.cs pour échouer si un fichier embarqué n’est référencé par aucun effet de production ou si un effet référence une ressource absente.
  - [ ] Mesurer le coût réel avant d’accepter chaque présélection
    - [ ] Ajouter les mesures au banc d’essai existant
      - [ ] Modifier tests/GWGUI.Tests/EmulationVideoBackendCapabilityTests.cs pour mesurer le temps CPU, le temps de présentation, les allocations, les recréations de ressources et la mémoire intermédiaire de chaque présélection retenue aux résolutions validées.
      - [ ] Modifier docs/EMULATION_VIDEO_FILTER_CATALOG.md pour inscrire les résultats et les limites acceptées après essai, sans inventer un budget identique pour toutes les machines et toutes les cartes graphiques.
      - [ ] Modifier src/GWGUI.App/Rendering/Emulation/VideoFilters/EmulationVideoEffectCatalog.cs pour marquer indisponible sur un backend uniquement un effet dont les capacités ou limites validées ne sont réellement pas respectées.
  - [ ] Exécuter la validation complète sans corriger autre chose par préférence
    - [ ] Exécuter les tests ciblés puis toute la suite
      - [ ] Exécuter EmulationVideoProcessingConfigurationTests, EmulationVideoFilterGraphTests, EmulationVideoBackendCapabilityTests, EmulationVideoProcessingSettingsTests, EmulationVideoLiveUpdateTests, EmulationVideoRenderingTests et EmulationVideoAssetLicenseTests, puis corriger uniquement les erreurs introduites par le point 7.
      - [ ] Exécuter AmigaConfigurationStoreTests et AtariConfigurationStoreTests, puis corriger uniquement les régressions provoquées par la nouvelle configuration vidéo ou sa migration.
      - [ ] Exécuter la suite GWGUI.Tests et corriger uniquement les régressions causées par les fichiers modifiés dans cette checklist.
      - [ ] Vérifier manuellement sur une machine Amiga et une machine Atari les valeurs neutres, plusieurs présélections retenues, une combinaison compatible, le refus puis l’acceptation d’une incompatibilité, l’enregistrement immédiat et l’application en direct.
      - [ ] Vérifier manuellement Direct3D 11, Vulkan, OpenGL et WPF selon leurs capacités validées, puis confirmer que le rendu sans effet, le changement de moteur, le repli et la capture d’écran conservent leur comportement décidé.

## Checklist détaillée — Point 8 : habillages d’écran en plein écran

Cette fonctionnalité reste une évolution différée. Sa checklist peut être préparée, mais aucune image ni aucun code d’habillage ne doit être réalisé avant une décision explicite de commencer ce point.

- [ ] Catalogue validé des habillages réellement retenus
  - [ ] Créer le document de catalogue avant toute recherche d’image
    - [ ] Créer le fichier puis sa structure
      - [ ] Créer docs/EMULATION_SCREEN_SKIN_CATALOG.md.
      - [ ] Modifier docs/EMULATION_SCREEN_SKIN_CATALOG.md pour ajouter les sections Module, Machine, Variante, Type de matériel, Image, Source, Licence, Dimensions, Rectangle d’écran, Mode de cadrage, État et Décision, sans inscrire encore d’habillage retenu.
  - [ ] Définir le périmètre initial sans ajouter d’animations
    - [ ] Inscrire les règles validées dans le catalogue
      - [ ] Modifier docs/EMULATION_SCREEN_SKIN_CATALOG.md pour réserver les téléviseurs et écrans d’ordinateur aux ordinateurs et consoles de salon et réserver à chaque console portable son propre boîtier ou son propre contour d’écran.
      - [ ] Modifier docs/EMULATION_SCREEN_SKIN_CATALOG.md pour permettre plusieurs variantes d’une machine lorsqu’elle a réellement existé sous plusieurs modèles ou couleurs, notamment une variante matérielle ou colorée distincte par identifiant.
      - [ ] Modifier docs/EMULATION_SCREEN_SKIN_CATALOG.md pour limiter la première version à une image décorative et exclure les boutons pressés, voyants, animations et zones interactives jusqu’à une évolution explicitement validée.
      - [ ] Modifier docs/EMULATION_SCREEN_SKIN_CATALOG.md pour limiter la première version au plein écran et conserver le mode fenêtré comme évolution ultérieure non implémentée.
      - [ ] Modifier docs/EMULATION_SCREEN_SKIN_CATALOG.md pour préciser que le plein écran classique reste disponible et inchangé lorsque l’habillage est désactivé.
  - [ ] Définir le cadrage de chaque type d’habillage
    - [ ] Consigner les modes à comparer puis le choix retenu
      - [ ] Modifier docs/EMULATION_SCREEN_SKIN_CATALOG.md pour comparer l’affichage de l’image entière et le recadrage autorisé autour de l’écran sans en faire automatiquement deux options utilisateur.
      - [ ] Modifier docs/EMULATION_SCREEN_SKIN_CATALOG.md pour imposer que l’écran émulé reste entièrement visible, correctement proportionné et sans déformation, même lorsqu’une partie extérieure du boîtier est coupée.
      - [ ] Modifier docs/EMULATION_SCREEN_SKIN_CATALOG.md pour autoriser une console portable à n’afficher que le contour de son écran lorsque son boîtier complet ne peut pas être cadré correctement.
      - [ ] Modifier docs/EMULATION_SCREEN_SKIN_CATALOG.md pour définir, après comparaison visuelle, le mode de cadrage retenu séparément pour chaque variante au lieu d’appliquer le même choix à toutes les images.
  - [ ] Déterminer les comportements encore non validés avant l’interface
    - [ ] Écrire les décisions manquantes dans le document principal
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md pour inscrire si un changement d’habillage pendant qu’une instance est déjà en plein écran doit être appliqué immédiatement ou seulement lors de la prochaine entrée en plein écran.
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md pour inscrire l’affichage retenu dans l’onglet Vidéo lorsqu’aucun habillage n’existe pour la machine : contrôle masqué ou contrôle désactivé avec une information courte.
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md pour inscrire le repli retenu lorsqu’une variante enregistrée n’existe plus ou ne peut pas être chargée, sans choisir silencieusement ce comportement pendant le code.
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md pour confirmer que la capture d’écran reste limitée à l’image émulée ou, si un autre comportement est demandé, ajouter celui-ci comme évolution séparée avant de modifier MachineVideoPresenter.Snapshot.

- [ ] Sources, licences et préparation des images approuvées
  - [ ] Vérifier le droit d’utilisation avant de créer un fichier dans le projet
    - [ ] Documenter chaque source retenue dans le catalogue
      - [ ] Modifier docs/EMULATION_SCREEN_SKIN_CATALOG.md pour enregistrer l’URL directe, l’auteur, la licence ou l’autorisation et la date de consultation de chaque image candidate.
      - [ ] Modifier docs/EMULATION_SCREEN_SKIN_CATALOG.md pour refuser comme ressource de production une image simplement trouvée sur Internet lorsque son droit de modification et de redistribution n’est pas établi.
      - [ ] Modifier docs/EMULATION_SCREEN_SKIN_CATALOG.md pour distinguer les images réutilisables, les images servant seulement de référence pour créer une nouvelle ressource et les images refusées.
      - [ ] Créer THIRD-PARTY-NOTICES.md s’il n’existe pas encore et si la première ressource tierce retenue exige une notice dans le projet.
      - [ ] Modifier THIRD-PARTY-NOTICES.md pour ajouter les mentions obligatoires avant l’intégration de chaque image concernée.
  - [ ] Valider les chemins exacts avant la création des ressources
    - [ ] Ajouter au document une action séparée pour chaque fichier retenu
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md pour inscrire, avant sa création, le chemin exact de chaque image sous src/GWGUI.App/Assets/Emulation/ScreenSkins/{module}/{machine}, son identifiant de variante, sa source et sa licence.
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md pour ajouter après chaque action Créer une action Modifier distincte décrivant le détourage, la transparence, le cadrage et les corrections réellement nécessaires à cette image.
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md pour inscrire le chemin exact du manifeste associé à chaque image avant de créer ce manifeste.
  - [ ] Créer les dossiers uniquement pour les variantes approuvées
    - [ ] Préparer l’arborescence avant son premier fichier
      - [ ] Créer le dossier src/GWGUI.App/Assets/Emulation/ScreenSkins seulement après la validation du premier habillage.
      - [ ] Créer sous src/GWGUI.App/Assets/Emulation/ScreenSkins le dossier exact du module inscrit auparavant dans AMELIORATIONS_INTERFACE_EMULATION.md.
      - [ ] Créer sous le dossier du module le dossier exact de la machine inscrit auparavant dans AMELIORATIONS_INTERFACE_EMULATION.md avant de créer sa première variante.
  - [ ] Produire chaque image sans modifier les autres variantes
    - [ ] Exécuter les actions validées image par image
      - [ ] Exécuter pour chaque variante les actions Créer puis Modifier ajoutées auparavant dans AMELIORATIONS_INTERFACE_EMULATION.md, avec une vue adaptée au plein écran, un fond transparent si nécessaire et sans élément interactif ajouté.
      - [ ] Modifier docs/EMULATION_SCREEN_SKIN_CATALOG.md après chaque vérification visuelle pour enregistrer les dimensions finales de l’image, son rectangle d’écran normalisé et son mode de cadrage validé.
      - [ ] Modifier src/GWGUI.App/GWGUI.App.csproj après la création de la première variante pour embarquer uniquement les images et manifestes réellement retenus sous Assets/Emulation/ScreenSkins.

- [ ] Configuration persistante de l’habillage par machine
  - [ ] Créer le contrat commun avant de modifier Amiga et Atari
    - [ ] Créer puis remplir le fichier de configuration
      - [ ] Créer src/GWGUI.Emulation/Contracts/EmulationScreenSkinConfiguration.cs.
      - [ ] Modifier src/GWGUI.Emulation/Contracts/EmulationScreenSkinConfiguration.cs pour enregistrer uniquement l’activation et l’identifiant de variante, avec l’habillage désactivé par défaut.
  - [ ] Exposer la même configuration aux deux familles
    - [ ] Étendre les contrats sans créer de réglages propres à une marque
      - [ ] Modifier src/GWGUI.Emulation/Interfaces/IEmulationConfiguration.cs pour exposer EmulationScreenSkinConfiguration.
      - [ ] Modifier src/GWGUI.Emulation.Amiga/Contracts/AmigaMachineConfiguration.cs pour enregistrer EmulationScreenSkinConfiguration sans changer les autres réglages vidéo.
      - [ ] Modifier src/GWGUI.Emulation.Atari/Contracts/AtariMachineConfiguration.cs pour enregistrer la même configuration commune sans recopier le catalogue d’habillages.
  - [ ] Migrer les fichiers existants vers une valeur désactivée
    - [ ] Augmenter chaque schéma au moment réel de l’implémentation
      - [ ] Modifier src/GWGUI.Emulation.Amiga/Services/AmigaConfigurationStore.cs et src/GWGUI.Emulation.Amiga/Contracts/AmigaMachineConfiguration.cs pour accepter les schémas déjà pris en charge, enregistrer la prochaine version disponible et ajouter uniquement un habillage désactivé aux anciens fichiers.
      - [ ] Modifier src/GWGUI.Emulation.Atari/Constants/AtariConstants.cs pour augmenter CurrentConfigurationSchemaVersion d’une version après celle utilisée par le point 7.
      - [ ] Modifier src/GWGUI.Emulation.Atari/Contracts/AtariConfigurationDocument.cs et src/GWGUI.Emulation.Atari/Functions/AtariConfigurationStoreFunctions.cs pour sérialiser et restaurer EmulationScreenSkinConfiguration.
      - [ ] Modifier src/GWGUI.Emulation.Atari/Functions/AtariConfigurationMigrationFunctions.cs pour migrer explicitement le schéma précédent avec l’habillage désactivé sans modifier les autres données.
  - [ ] Verrouiller la configuration avant le catalogue d’images
    - [ ] Créer le fichier de tests avant ses scénarios
      - [ ] Créer tests/GWGUI.Tests/EmulationScreenSkinConfigurationTests.cs.
    - [ ] Ajouter les scénarios de valeur, migration et isolation
      - [ ] Modifier tests/GWGUI.Tests/EmulationScreenSkinConfigurationTests.cs pour vérifier la valeur désactivée, l’activation, l’identifiant de variante et l’indépendance de deux configurations de machines.
      - [ ] Modifier tests/GWGUI.Tests/AmigaConfigurationStoreTests.cs pour vérifier la migration du schéma précédent et le cycle chargement-enregistrement de l’habillage sans perte des autres données.
      - [ ] Modifier tests/GWGUI.Tests/AtariConfigurationStoreTests.cs pour vérifier la migration explicite équivalente côté Atari.

- [ ] Définitions et catalogue commun des variantes disponibles
  - [ ] Créer les contrats d’image avant de charger les manifestes
    - [ ] Créer puis remplir la définition normalisée
      - [ ] Créer src/GWGUI.App/Contracts/Emulation/ScreenSkins/EmulationScreenSkinDefinition.cs.
      - [ ] Modifier src/GWGUI.App/Contracts/Emulation/ScreenSkins/EmulationScreenSkinDefinition.cs pour décrire le module, la machine, la variante, la clé de libellé, l’image, le rectangle d’écran normalisé et uniquement le mode de cadrage validé dans le catalogue.
      - [ ] Créer src/GWGUI.App/Enums/EmulationScreenSkinLayoutMode.cs.
      - [ ] Modifier src/GWGUI.App/Enums/EmulationScreenSkinLayoutMode.cs pour déclarer seulement les modes de cadrage effectivement retenus après les comparaisons, sans ajouter d’options inutilisées.
  - [ ] Créer le chargeur avant d’enregistrer une variante
    - [ ] Créer puis remplir le catalogue partagé
      - [ ] Créer src/GWGUI.App/Services/Emulation/ScreenSkins/EmulationScreenSkinCatalog.cs.
      - [ ] Modifier src/GWGUI.App/Services/Emulation/ScreenSkins/EmulationScreenSkinCatalog.cs pour charger les manifestes embarqués, valider les coordonnées normalisées et indexer chaque variante par module, machine et identifiant.
      - [ ] Modifier src/GWGUI.App/Services/Emulation/ScreenSkins/EmulationScreenSkinCatalog.cs pour retourner uniquement les variantes de la machine demandée et ne jamais proposer un téléviseur, un écran ou une console portable appartenant à une autre machine.
      - [ ] Modifier src/GWGUI.App/Services/Emulation/ScreenSkins/EmulationScreenSkinCatalog.cs pour charger et mettre en cache l’image WPF sans conserver de verrou sur un fichier externe.
      - [ ] Modifier src/GWGUI.App/Services/Emulation/ScreenSkins/EmulationScreenSkinCatalog.cs pour appliquer exactement le repli validé lorsqu’une image, un manifeste ou une variante enregistrée est absent ou invalide.
  - [ ] Ajouter chaque manifeste seulement après son image
    - [ ] Exécuter les créations dans l’ordre inscrit dans le document
      - [ ] Créer chaque manifeste au chemin exact ajouté auparavant dans AMELIORATIONS_INTERFACE_EMULATION.md seulement après l’existence de son image approuvée.
      - [ ] Modifier chaque manifeste créé pour inscrire l’identifiant, la clé de libellé, le chemin de l’image, le rectangle d’écran normalisé et le mode de cadrage mesurés pour cette variante précise.
      - [ ] Modifier docs/EMULATION_SCREEN_SKIN_CATALOG.md pour marquer la variante disponible seulement après le chargement réussi de son image et de son manifeste par EmulationScreenSkinCatalog.
  - [ ] Vérifier le catalogue avant la composition plein écran
    - [ ] Créer le fichier de tests avant ses scénarios
      - [ ] Créer tests/GWGUI.Tests/EmulationScreenSkinCatalogTests.cs.
    - [ ] Ajouter les scénarios de correspondance et de ressources
      - [ ] Modifier tests/GWGUI.Tests/EmulationScreenSkinCatalogTests.cs pour vérifier que chaque manifeste possède une image, une clé unique, une machine existante et un rectangle d’écran entièrement compris entre 0 et 1.
      - [ ] Modifier tests/GWGUI.Tests/EmulationScreenSkinCatalogTests.cs pour vérifier qu’une variante n’est retournée que pour son module et sa machine et que les différentes couleurs ou modèles restent des choix distincts.
      - [ ] Modifier tests/GWGUI.Tests/EmulationScreenSkinCatalogTests.cs pour vérifier le comportement validé d’une image absente, d’un manifeste invalide et d’un identifiant enregistré devenu inconnu.
      - [ ] Modifier tests/GWGUI.Tests/EmulationScreenSkinCatalogTests.cs pour vérifier la transparence réellement requise, les dimensions et la correspondance entre le rectangle déclaré et l’ouverture approuvée de chaque image.
- [ ] Extraction du plein écran classique avant l’ajout des images
  - [ ] Verrouiller le comportement actuel avant son déplacement
    - [ ] Créer le fichier de tests du plein écran avant ses scénarios
      - [ ] Créer tests/GWGUI.Tests/EmulationFullscreenHostTests.cs.
      - [ ] Modifier tests/GWGUI.Tests/EmulationFullscreenHostTests.cs pour vérifier la fenêtre noire maximisée, le déplacement de MachineView.Screen, SetDisplayHost, le retour dans MachineView.DisplayHost, le focus et les appels BeginHostTransition puis CompleteHostTransition.
  - [ ] Créer l’hôte réutilisable sans modifier l’apparence classique
    - [ ] Créer puis remplir le contrôle de plein écran
      - [ ] Créer src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationFullscreenHost.cs.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationFullscreenHost.cs pour reproduire d’abord le Grid noir étiré actuel et exposer un ScreenSlot occupant toute sa surface lorsque aucun habillage n’est fourni.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs pour créer EmulationFullscreenHost à la place du Grid actuel, déplacer MachineView.Screen dans son ScreenSlot et fournir ce slot à MachineVideoPresenter.SetDisplayHost.
      - [ ] Modifier tests/GWGUI.Tests/EmulationFullscreenHostTests.cs pour comparer le mode sans habillage au comportement verrouillé avant de retirer le champ Grid devenu inutile.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs pour retirer uniquement l’ancien code de création du Grid noir après réussite des tests du nouvel hôte.

- [ ] Calcul commun du placement de l’habillage et de l’écran
  - [ ] Créer les contrats de géométrie avant le calcul
    - [ ] Créer puis remplir les fichiers de disposition
      - [ ] Créer src/GWGUI.App/Contracts/Emulation/ScreenSkins/EmulationScreenSkinLayout.cs.
      - [ ] Modifier src/GWGUI.App/Contracts/Emulation/ScreenSkins/EmulationScreenSkinLayout.cs pour retourner le rectangle final de l’image décorative et le rectangle final du ScreenSlot dans les coordonnées du plein écran.
      - [ ] Créer src/GWGUI.App/Functions/Rendering/Emulation/EmulationScreenSkinLayoutFunctions.cs.
      - [ ] Modifier src/GWGUI.App/Functions/Rendering/Emulation/EmulationScreenSkinLayoutFunctions.cs pour calculer les deux rectangles à partir de la fenêtre, des dimensions de l’image, du rectangle normalisé et du mode de cadrage validé.
  - [ ] Garantir l’intégrité de l’écran pour tous les formats
    - [ ] Ajouter les contraintes au calcul partagé
      - [ ] Modifier src/GWGUI.App/Functions/Rendering/Emulation/EmulationScreenSkinLayoutFunctions.cs pour conserver les proportions de l’image d’habillage sans l’étirer indépendamment sur les deux axes.
      - [ ] Modifier src/GWGUI.App/Functions/Rendering/Emulation/EmulationScreenSkinLayoutFunctions.cs pour maintenir le ScreenSlot entièrement dans la zone visible, même lorsqu’une partie extérieure de l’habillage est recadrée.
      - [ ] Modifier src/GWGUI.App/Functions/Rendering/Emulation/EmulationScreenSkinLayoutFunctions.cs pour laisser EmulationVideoLayoutFunctions.Fit conserver le rapport d’aspect réel de l’image émulée à l’intérieur du ScreenSlot.
      - [ ] Modifier src/GWGUI.App/Functions/Rendering/Emulation/EmulationScreenSkinLayoutFunctions.cs pour retourner une disposition invalide plutôt que des dimensions négatives, infinies ou un écran placé hors de la fenêtre lorsqu’un manifeste est incorrect.
  - [ ] Verrouiller les modes validés et les cas de recadrage
    - [ ] Créer le fichier de tests de géométrie avant ses scénarios
      - [ ] Créer tests/GWGUI.Tests/EmulationScreenSkinLayoutTests.cs.
      - [ ] Modifier tests/GWGUI.Tests/EmulationScreenSkinLayoutTests.cs pour vérifier chaque mode validé avec des fenêtres 4:3, 16:9, ultra-larges et verticales ainsi qu’avec plusieurs facteurs DPI.
      - [ ] Modifier tests/GWGUI.Tests/EmulationScreenSkinLayoutTests.cs pour vérifier une télévision ou un écran d’ordinateur entièrement visible, un boîtier portable partiellement coupé et un contour d’écran portable.
      - [ ] Modifier tests/GWGUI.Tests/EmulationScreenSkinLayoutTests.cs pour vérifier que le ScreenSlot reste toujours entier, que l’image décorative garde ses proportions et que l’image émulée n’est jamais déformée.
      - [ ] Modifier tests/GWGUI.Tests/EmulationScreenSkinLayoutTests.cs pour vérifier le refus des coordonnées invalides et l’application du repli décidé dans le catalogue.

- [ ] Composition décorative dans le plein écran uniquement
  - [ ] Ajouter l’image autour du ScreenSlot après le mode classique
    - [ ] Étendre l’hôte sans superposer de contrôle interactif
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationFullscreenHost.cs pour charger l’image fournie par EmulationScreenSkinCatalog, l’afficher dans son rectangle calculé et placer ScreenSlot dans l’ouverture déclarée.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationFullscreenHost.cs pour recalculer les rectangles après changement de taille, de DPI ou de variante sans retirer la surface vidéo de ScreenSlot.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationFullscreenHost.cs pour rendre l’image décorative non interactive et ne créer aucune zone de bouton, de voyant ou d’animation dans cette première version.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationFullscreenHost.cs pour afficher le fond noir classique autour des zones non couvertes par l’image.
  - [ ] Préserver la surface et les traitements vidéo existants
    - [ ] Raccorder le présentateur uniquement au nouveau slot
      - [ ] Modifier src/GWGUI.App/Presenters/Emulation/Machine/MachineVideoPresenter.cs pour ajuster MachineView.Screen dans EmulationFullscreenHost.ScreenSlot et continuer à présenter la même surface WPF, OpenGL, Direct3D 11 ou Vulkan.
      - [ ] Modifier src/GWGUI.App/Presenters/Emulation/Machine/MachineVideoPresenter.cs pour appliquer les filtres du point 7 uniquement à l’image émulée dans ScreenSlot et jamais à l’image décorative.
      - [ ] Modifier src/GWGUI.App/Presenters/Emulation/Machine/MachineVideoPresenter.cs pour conserver Snapshot limité au comportement validé avant le début du point 8.
      - [ ] Modifier tests/GWGUI.Tests/EmulationFullscreenHostTests.cs pour vérifier qu’un changement de backend ou de filtre ne recrée pas l’habillage, ne déplace pas ScreenSlot et n’applique aucun filtre à l’image décorative.
  - [ ] Préserver le focus et la capture de souris
    - [ ] Raccorder le fond décoratif au comportement validé au point 1
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationFullscreenHost.cs pour transmettre un clic sur l’habillage ou le fond à l’opération commune qui redonne le focus à l’émulation sans capturer la souris.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs pour conserver BeginHostTransition, CompleteHostTransition, le focus après ContentRendered et le retour du focus après la fermeture du plein écran.
      - [ ] Modifier tests/GWGUI.Tests/EmulationFullscreenHostTests.cs pour vérifier qu’un clic décoratif redonne le focus sans capture, qu’un clic dans ScreenSlot conserve le comportement de l’écran et que F12 ou la fermeture de la fenêtre libère toujours correctement la souris.
  - [ ] Laisser le mode fenêtré strictement inchangé
    - [ ] Maintenir l’habillage hors de MachineView.DisplayHost
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs pour ne créer EmulationFullscreenHost qu’à l’entrée en plein écran et replacer MachineView.Screen directement dans MachineView.DisplayHost à la sortie.
      - [ ] Modifier tests/GWGUI.Tests/EmulationFullscreenHostTests.cs pour vérifier que l’activation d’un habillage enregistré ne change ni la zone grise, ni la taille, ni la hiérarchie visuelle du mode fenêtré.
- [ ] Choix de l’habillage dans l’onglet Vidéo
  - [ ] Créer le gestionnaire commun avant l’interface
    - [ ] Créer puis remplir le contrat de remplacement
      - [ ] Créer src/GWGUI.Emulation/Interfaces/IEmulationScreenSkinSettingsManager.cs.
      - [ ] Modifier src/GWGUI.Emulation/Interfaces/IEmulationScreenSkinSettingsManager.cs pour remplacer EmulationScreenSkinConfiguration dans une IEmulationConfiguration sans exposer les contrats Amiga ou Atari à l’application.
      - [ ] Modifier src/GWGUI.Emulation.Amiga/Modules/AmigaEmulationModule.cs pour implémenter IEmulationScreenSkinSettingsManager en remplaçant uniquement la configuration d’habillage.
      - [ ] Modifier src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs pour implémenter le même contrat sans recopier les variantes disponibles.
  - [ ] Créer le contrôleur avant le contrôle WPF
    - [ ] Créer puis remplir le gestionnaire de choix
      - [ ] Créer src/GWGUI.App/Controllers/Emulation/Options/EmulationScreenSkinSettingsController.cs.
      - [ ] Modifier src/GWGUI.App/Controllers/Emulation/Options/EmulationScreenSkinSettingsController.cs pour demander au catalogue uniquement les variantes du module et de la machine affichés, charger le choix enregistré et produire une nouvelle EmulationScreenSkinConfiguration.
      - [ ] Modifier src/GWGUI.App/Controllers/Emulation/Options/EmulationScreenSkinSettingsController.cs pour appliquer exactement le comportement validé lorsqu’aucune variante n’existe ou lorsque le choix enregistré est introuvable.
  - [ ] Créer l’interface sans ajouter de fonctions non demandées
    - [ ] Créer puis remplir le contrôle commun
      - [ ] Créer src/GWGUI.App/Views/Controls/Emulation/Options/EmulationScreenSkinSettingsControl.cs.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationScreenSkinSettingsControl.cs pour afficher uniquement l’activation de l’habillage et la liste des variantes disponibles pour la machine.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationScreenSkinSettingsControl.cs pour conserver le choix désactivé comme plein écran classique et ne pas ajouter de prévisualisation, d’éditeur d’image, de bouton virtuel ou d’option fenêtrée non demandés.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationScreenSkinSettingsControl.cs pour utiliser le comportement validé dans AMELIORATIONS_INTERFACE_EMULATION.md lorsqu’aucun habillage n’est disponible.
  - [ ] Insérer le contrôle uniquement dans les réglages vidéo hors émulateur
    - [ ] Raccorder l’éditeur de machine au chemin automatique existant
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour créer le contrôle seulement dans l’onglet Vidéo et dans le cadre hors émulateur établi au point 7.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour reconstruire les variantes après un changement de marque ou de machine sans conserver le choix visuel de la machine précédente.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour enregistrer immédiatement l’activation ou la variante par le chemin créé au point 2 lorsqu’une configuration existe et ne rien écrire pour un brouillon non créé.

- [ ] Ressources multilingues des contrôles et variantes
  - [ ] Créer les clés neutres avant les traductions
    - [ ] Ajouter uniquement les textes effectivement utilisés
      - [ ] Modifier src/GWGUI.App/Resources/00-Base/Emulation.resx pour ajouter les libellés validés de l’activation, de la variante, de l’absence éventuelle d’habillage et chaque clé de variante présente dans un manifeste approuvé.
  - [ ] Traduire les mêmes clés dans toutes les langues existantes
    - [ ] Modifier chaque catalogue sans écrire de libellé dans le code ou le manifeste
      - [ ] Modifier src/GWGUI.App/Resources/ar-SA/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/cs-CZ/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/da-DK/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/de-DE/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/el-GR/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/en-US/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/es-ES/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/fi-FI/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/fr-FR/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/he-IL/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/hu-HU/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/id-ID/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/it-IT/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/ja-JP/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/ko-KR/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/nb-NO/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/nl-NL/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/pl-PL/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/pt-BR/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/pt-PT/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/ro-RO/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/ru-RU/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/sv-SE/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/th-TH/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/tr-TR/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/uk-UA/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/vi-VN/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/zh-Hans/Emulation.resx.
      - [ ] Modifier src/GWGUI.App/Resources/zh-Hant/Emulation.resx.

- [ ] Transmission de la configuration à la bonne instance
  - [ ] Fournir le contexte complet lors de l’ouverture
    - [ ] Étendre les options avant MachineController
      - [ ] Modifier src/GWGUI.App/Contracts/Machine/MachineControllerOptions.cs pour transporter le ModuleId, le MachineId et EmulationScreenSkinConfiguration avec les autres réglages de présentation.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationSectionMachineFunctions.cs pour fournir ces trois valeurs depuis la configuration réellement ouverte.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs pour résoudre la variante par EmulationScreenSkinCatalog à l’entrée en plein écran et utiliser le mode classique lorsque l’option est désactivée.
  - [ ] Appliquer une sauvegarde uniquement à l’instance correspondante
    - [ ] Raccorder l’événement sans appeler l’émulateur
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationSectionConfigurationFunctions.cs pour transmettre le nouveau choix uniquement au MachineController possédant le même ModuleId et le même Id de configuration.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs pour mémoriser le nouveau choix sans redémarrer, réinitialiser ou modifier la machine émulée.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs pour appliquer le choix immédiatement ou à la prochaine entrée en plein écran selon la décision inscrite avant l’implémentation.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs pour conserver le plein écran classique ou appliquer l’autre repli explicitement validé si la variante choisie ne peut pas être résolue.
      - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationSectionConfigurationFunctions.cs pour laisser une configuration sans instance ouverte utiliser simplement son choix enregistré au prochain démarrage.

- [ ] Tests de l’interface, du plein écran, des ressources et des non-régressions
  - [ ] Verrouiller le choix enregistré dans l’onglet Vidéo
    - [ ] Créer le fichier de tests WPF avant ses scénarios
      - [ ] Créer tests/GWGUI.Tests/EmulationScreenSkinSettingsTests.cs.
      - [ ] Modifier tests/GWGUI.Tests/EmulationScreenSkinSettingsTests.cs pour vérifier l’affichage uniquement dans Vidéo, l’activation, la désactivation, les variantes propres à la machine et le comportement validé lorsqu’aucune variante n’existe.
      - [ ] Modifier tests/GWGUI.Tests/EmulationScreenSkinSettingsTests.cs pour vérifier l’enregistrement immédiat d’une configuration existante, l’absence d’écriture d’un brouillon et la remise à zéro visuelle après changement de machine.
      - [ ] Modifier tests/GWGUI.Tests/EmulationScreenSkinSettingsTests.cs pour vérifier que chaque libellé provient des ressources et change avec la langue sans modifier l’identifiant enregistré.
  - [ ] Vérifier les images, manifestes et licences embarqués
    - [ ] Créer le fichier de contrôle des ressources
      - [ ] Créer tests/GWGUI.Tests/EmulationScreenSkinAssetTests.cs.
      - [ ] Modifier tests/GWGUI.Tests/EmulationScreenSkinAssetTests.cs pour vérifier que chaque image embarquée possède un manifeste, une entrée dans docs/EMULATION_SCREEN_SKIN_CATALOG.md et une source dont le statut autorise réellement la redistribution.
      - [ ] Modifier tests/GWGUI.Tests/EmulationScreenSkinAssetTests.cs pour vérifier la présence dans THIRD-PARTY-NOTICES.md lorsque la licence l’exige et refuser les fichiers non utilisés par le catalogue.
      - [ ] Modifier tests/GWGUI.Tests/EmulationScreenSkinAssetTests.cs pour vérifier les dimensions, le canal alpha lorsqu’il est requis et l’absence de manifeste pointant hors du dossier autorisé de sa machine.
  - [ ] Créer les références visuelles seulement après approbation des variantes
    - [ ] Préparer le fichier de tests et son dossier avant les images
      - [ ] Créer tests/GWGUI.Tests/EmulationScreenSkinRenderingTests.cs.
      - [ ] Créer le dossier tests/GWGUI.Tests/Fixtures/ScreenSkins.
      - [ ] Modifier AMELIORATIONS_INTERFACE_EMULATION.md pour inscrire le chemin exact de chaque capture de référence approuvée avant sa création dans tests/GWGUI.Tests/Fixtures/ScreenSkins.
      - [ ] Créer chaque référence inscrite avec la variante, la résolution de fenêtre, le facteur DPI et le rapport d’aspect vidéo indiqués dans son nom ou son manifeste de test.
      - [ ] Modifier tests/GWGUI.Tests/GWGUI.Tests.csproj après la première référence pour copier uniquement les images de test réellement créées.
    - [ ] Comparer la composition sans imposer une taille unique
      - [ ] Modifier tests/GWGUI.Tests/EmulationScreenSkinRenderingTests.cs pour comparer le mode classique et chaque variante approuvée aux références correspondant à plusieurs formats d’écran.
      - [ ] Modifier tests/GWGUI.Tests/EmulationScreenSkinRenderingTests.cs pour vérifier visuellement et géométriquement que la totalité de l’écran émulé reste dans l’ouverture, que le boîtier peut être recadré comme validé et qu’aucune déformation n’est introduite.
      - [ ] Modifier tests/GWGUI.Tests/EmulationScreenSkinRenderingTests.cs pour vérifier qu’aucun bouton, voyant, halo ou animation n’apparaît dans cette première version décorative.
  - [ ] Vérifier le cycle complet d’entrée et de sortie du plein écran
    - [ ] Compléter les scénarios de l’hôte partagé
      - [ ] Modifier tests/GWGUI.Tests/EmulationFullscreenHostTests.cs pour entrer et sortir plusieurs fois avec l’habillage désactivé, activé, changé et devenu indisponible selon le repli validé.
      - [ ] Modifier tests/GWGUI.Tests/EmulationFullscreenHostTests.cs pour vérifier la conservation du focus, de la libération de souris, de la machine en cours, du backend, des filtres et du rapport d’aspect après chaque transition.
      - [ ] Modifier tests/GWGUI.Tests/EmulationFullscreenHostTests.cs pour vérifier que deux instances ouvertes utilisent chacune uniquement l’habillage de leur propre configuration.
      - [ ] Modifier tests/GWGUI.Tests/EmulationFullscreenHostTests.cs pour vérifier qu’une fermeture directe de la fenêtre plein écran replace toujours MachineView.Screen dans son hôte fenêtré et libère les références à l’image décorative.
  - [ ] Mesurer le coût sans ajouter d’optimisation non justifiée
    - [ ] Ajouter les contrôles de chargement et de mémoire
      - [ ] Modifier tests/GWGUI.Tests/EmulationScreenSkinAssetTests.cs pour mesurer le décodage initial, vérifier le cache par variante et vérifier qu’un changement répété n’accumule ni BitmapImage ni gestionnaire d’événement inutilisé.
      - [ ] Modifier docs/EMULATION_SCREEN_SKIN_CATALOG.md pour inscrire les dimensions et le coût mémoire réels de chaque image approuvée et réduire une ressource seulement lorsqu’une mesure montre qu’elle est inutilement grande.
  - [ ] Exécuter la validation complète sans réaliser les évolutions futures
    - [ ] Exécuter les tests ciblés puis toute la suite
      - [ ] Exécuter EmulationScreenSkinConfigurationTests, EmulationScreenSkinCatalogTests, EmulationScreenSkinLayoutTests, EmulationScreenSkinSettingsTests, EmulationScreenSkinAssetTests, EmulationScreenSkinRenderingTests et EmulationFullscreenHostTests, puis corriger uniquement les erreurs introduites par le point 8.
      - [ ] Exécuter AmigaConfigurationStoreTests et AtariConfigurationStoreTests, puis corriger uniquement les régressions provoquées par la configuration d’habillage et sa migration.
      - [ ] Exécuter les tests du focus, de la souris, des surfaces vidéo et des filtres du point 7, puis corriger uniquement les régressions provoquées par le nouvel hôte plein écran.
      - [ ] Exécuter la suite GWGUI.Tests et corriger uniquement les régressions causées par les fichiers modifiés dans cette checklist.
      - [ ] Vérifier manuellement le plein écran classique, une télévision ou un écran d’ordinateur approuvé et une console portable approuvée sur plusieurs rapports d’écran, avec tous les backends disponibles.
      - [ ] Vérifier manuellement que le mode fenêtré n’affiche jamais l’habillage, que les clics décoratifs rendent le focus sans capturer la souris et que les boutons, LED et animations restent absents.
