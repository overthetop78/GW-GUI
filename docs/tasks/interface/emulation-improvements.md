# Améliorations souhaitées pour l’interface d’émulation

## But du document

Ce document reprend uniquement les demandes et les idées formulées à partir des six images de l’interface.

Il distingue les demandes validées des pistes encore à étudier. La fin du document contient l’ordre général retenu et les checklists techniques détaillées des points 1 à 8.

## 1. Écran d’émulation

### Focus de l’écran

Dans l’onglet d’émulation actif, le focus doit revenir à la fenêtre d’émulation après une action ponctuelle effectuée dans l’interface lorsque la machine est allumée ou vient d’être allumée. L’extinction ne rend pas le focus à la machine éteinte.

Cela concerne notamment :

- le chargement ou le changement d’une image de disquette ;
- l’allumage de la machine ;
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

- si la configuration de la machine n’existe pas encore, chaque changement effectué par l’utilisateur est conservé dans un brouillon en mémoire pendant toute l’exécution de l’application, sans écrire de fichier tant que l’utilisateur ne clique pas sur **Créer** ;
- chaque machine sans configuration possède son propre brouillon en mémoire et le conserve lorsque l’utilisateur affiche une autre machine ou ferme puis rouvre la fenêtre **Paramètres** ;
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

Lorsque l’utilisateur ouvre l’onglet d’une marque, l’application relit les configurations existantes. La présence du bouton **Créer** dépend donc de l’existence réelle de la configuration de la machine affichée.

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

La longueur affichée est limitée à 20 caractères, ellipse comprise. Si le libellé est trop long, il est tronqué avec une ellipse. La colonne **Destination** est placée après la colonne indiquant la compatibilité.

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

- Les groupes et les tâches sont toujours écrits et réalisés exactement dans leur ordre réel d’exécution.
- Une sous-tâche d’action est cochée uniquement après l’écriture, la création, la modification, la copie ou le déplacement demandé et sa vérification.
- Une tâche finalisée est cochée lorsque toutes ses sous-tâches sont cochées. Le même principe s’applique ensuite en remontant jusqu’au groupe général.
- Une lecture, une recherche ou une réflexion n’est jamais une tâche isolée : elle fait partie d’une action qui produit ou modifie dans la même sous-tâche un fichier identifié.
- Lorsqu’un fichier doit être créé, sa création précède toujours l’ajout de son contenu.
- Toute modification indique le fichier concerné avant de décrire les changements à y effectuer.
- Un déplacement de code commence par le déplacement ou la copie du code existant en conservant exactement son fonctionnement. La suppression de l’ancien emplacement intervient seulement après vérification du déplacement. Toute modification fonctionnelle éventuelle constitue une tâche ultérieure séparée.
- Aucun comportement n’est modifié, corrigé ou remplacé par préférence personnelle. Une correction non prévue n’est effectuée que si une erreur réelle est constatée.
- Ne jamais inventer et ne jamais extrapoler un comportement, une donnée, une dépendance, une solution ou une tâche.
- Ne jamais sauter une étape : chaque tâche et chaque sous-tâche est réalisée dans l’ordre écrit, uniquement lorsque toutes les étapes précédentes nécessaires sont réellement terminées, vérifiées et cochées.
- Ne passer à la tâche suivante qu’après avoir coché la tâche précédente réellement terminée. Si une tâche nécessaire a été oubliée pendant l’exécution, l’inscrire d’abord au bon endroit puis la réaliser avant de reprendre la suite.
- Lorsqu’une action potentiellement nécessaire n’est pas inscrite, lire d’abord les fichiers et le fonctionnement directement concernés afin de déterminer si elle est réellement indispensable et entièrement justifiée. Si elle l’est, ajouter la tâche correspondante au bon endroit dans l’ordre d’exécution, puis seulement effectuer cette action.
- Si cette vérification ne permet pas de trancher sans inventer, extrapoler ou choisir un comportement non validé, arrêter le travail et demander une décision avant toute modification.
- Lorsque plusieurs informations ou décisions sont nécessaires pour poursuivre, identifier toutes les questions réellement bloquantes et les poser ensemble afin de pouvoir compléter les tâches puis les exécuter sans interruptions évitables.
- Ne jamais casser le code : préserver le fonctionnement existant qui n’est pas explicitement concerné, vérifier chaque modification et corriger uniquement les régressions qu’elle provoque avant de poursuivre.
- Lorsqu’un changement nécessaire touche un système existant, l’améliorer sans le remplacer ni retirer son fonctionnement. Écrire auparavant toutes les tâches nécessaires après avoir relu les fichiers et le fonctionnement concernés ; tout remplacement explicitement nécessaire doit être décrit et validé avant son exécution.
- Toujours respecter l’ensemble des règles de rédaction, d’ordre, d’exécution, de vérification et de suivi des tâches, sans exception implicite.
- Avant toute modification, lire les fichiers directement concernés et uniquement les contrats, appels, dépendances, présentateurs ou contrôleurs pertinents pour la tâche, dans l’étendue nécessaire pour comprendre le fonctionnement réel et l’architecture utilisée, sans relire inutilement des fichiers inchangés déjà compris.
- Lire les tests existants lorsqu’une tâche demande de créer, modifier ou exécuter des tests, notamment pour vérifier qu’un fichier ou un scénario équivalent n’existe pas déjà ; ne pas parcourir des tests sans rapport avec l’action à réaliser.
- Ne jamais écrire, modifier ou extrapoler du code sans savoir comment la partie concernée de l’application fonctionne réellement. Si le fonctionnement ou l’architecture ne peut pas être établi avec certitude depuis le projet, arrêter le travail et demander une décision.
- Toujours respecter l’architecture existante du projet : placer les énumérations dans des fichiers d’énumérations sous le dossier Enums approprié, les constantes dans des fichiers de constantes sous le dossier Constants approprié et les fonctions dans des fichiers de fonctions sous le dossier Functions approprié.
- Lorsqu’une énumération, une constante ou une fonction peut être commune, l’écrire une seule fois pour l’usage commun et la placer dans la couche et le dossier communs correspondant à sa portée réelle, sans duplication locale.
- Ne laisser aucun nombre, texte ou autre valeur brute inexpliquée dans le code : toute valeur utilisée par le fonctionnement doit être portée par une constante nommée dans le fichier de constantes approprié.
- Ne laisser aucun texte visible directement dans le code. Tout texte affiché doit utiliser une ressource de localisation placée dans le fichier approprié, même lorsque sa valeur est identique dans toutes les langues ou qu’aucune variation de traduction n’est attendue.
- Lorsqu’un texte visible est ajouté ou modifié, créer ou modifier sa ressource dans la base appropriée puis dans tous les fichiers de langues pris en charge avant d’utiliser cette ressource dans l’interface.
- Les tests intermédiaires doivent être ciblés et rapides. Un petit test créé uniquement pour vérifier ponctuellement une action peut être retiré après cette vérification lorsqu’aucune tâche ne demande de le conserver.
- La création d’un test durable, plus large ou regroupant plusieurs vérifications doit toujours être prévue par une tâche écrite avant la création ou la modification de son fichier.

## Checklist détaillée — Point 1 : écran d’émulation

Cette checklist détaille uniquement le retour du focus du point 1. Dans l’ordre global, ce travail correspond au groupe 3. Les filtres vidéo et les habillages sont détaillés séparément dans les checklists des points 7 et 8 afin de ne pas dupliquer leurs tâches ici. Chaque dernière case constitue une modification atomique qui doit laisser le projet compilable, être vérifiée, puis être cochée avant la suivante.

- [ ] Retour automatique du focus vers l’instance d’émulation ouverte
  - [x] Limiter la restitution du focus à l’instance affichée dans l’onglet actif
    - [x] Transporter la sélection réelle du TabControl jusqu’au contrôleur de machine
      - [x] Dans src/GWGUI.App/Contracts/Machine/MachineControllerOptions.cs, ajouter `Func<bool> IsActive` avant les paramètres facultatifs; dans `OpenMachineAsync` de src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationSectionMachineFunctions.cs, conserver la référence du `MachineController` créé et fournir une fonction `IsActive` qui compare cette référence à `_machines.SelectedContent`.
      - [x] Dans src/GWGUI.App/Controllers/Emulation/Machine/MachineInputController.cs, ajouter le champ `Func<bool> _isActive` et le paramètre de constructeur qui l’initialise; dans le constructeur de src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs, transmettre `options.IsActive` à ce nouveau paramètre.
    - [x] Centraliser la restitution vers la cible active et courante
      - [x] Dans src/GWGUI.App/Controllers/Emulation/Machine/MachineInputController.cs, ajouter RestoreFocus avec exactement deux chemins : retourner lorsque _powered est faux ou lorsque _isActive() est faux; sinon appeler RelativeMouseCapture.Focus(_inputView, _inputHandle). Ne faire aucun appel à Capture, ReleasePointer, SetInputView ou _view.Screen.Focus() dans cette méthode.
  - [x] Rendre le clic de la zone grise au contrôleur d’entrée
    - [x] Raccorder uniquement le fond extérieur à l’écran
      - [x] Dans src/GWGUI.App/Controllers/Emulation/Machine/MachineInputController.cs, abonner `MouseLeftButtonDown` de `_view.DisplayHost` dans le constructeur, créer `DisplayHostMouseLeftButtonDown` pour appeler `RestoreFocus` uniquement lorsque `args.OriginalSource` est exactement `_view.DisplayHost`, puis désabonner ce gestionnaire dans `Dispose`.
  - [x] Restituer le focus après les commandes de la barre d’outils
    - [x] Faire transporter l’opération commune par la barre sans casser sa construction
      - [x] Dans src/GWGUI.App/Views/Controls/Emulation/Machine/MachineCommandBar.cs, ajouter le champ `Action _restoreFocus` et le paramètre de constructeur qui l’initialise; dans le constructeur de src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs, créer `_input` avant `_commands` et fournir `_input.RestoreFocus` au nouveau paramètre.
      - [x] Dans src/GWGUI.App/Views/Controls/Emulation/Machine/MachineCommandBar.cs, rendre `RunAsync` non statique et ajouter un bloc `finally` qui appelle `_restoreFocus()` après le `try/catch` existant, sans modifier `Command` ni les actions qui lui sont fournies.
    - [x] Retirer les restitutions particulières remplacées par le chemin commun
      - [x] Dans `TogglePowerAsync` de src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs, supprimer uniquement `if (_session.IsPowered) _video.InputView.Focus()` et laisser inchangées les mises à jour de session, d’entrée, de commandes, de visibilité vidéo et de statut.
      - [x] Dans `ExecuteShortcutAsync` de src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs, ajouter un bloc `finally` qui appelle `_input.RestoreFocus()` après le `try/catch`, sans modifier le `switch`, les actions appelées ni la gestion actuelle des erreurs.
  - [x] Restituer le focus après les commandes des lecteurs
    - [x] Faire transporter l’opération commune jusqu’aux boutons de média sans dupliquer les erreurs
      - [x] Dans src/GWGUI.App/Views/Controls/Emulation/Machine/MachineView.cs, ajouter `Action restoreFocus` à `SetDevices`, `DeviceItem` et `RunAsync`, transmettre ce paramètre à chaque appel intermédiaire et l’appeler dans un `finally` de `RunAsync`; dans `RebuildMediaDevices` de src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs, fournir `_input.RestoreFocus` au nouvel argument de `_view.SetDevices`. Ne modifier ni `InsertMediaAsync`, ni `EjectMediaAsync`, ni le `catch` qui appelle `showError`.
  - [x] Conserver la séquence du plein écran avec la même opération de focus
    - [x] Utiliser la restitution commune après le déplacement de Screen
      - [x] Dans `CompleteHostTransition` de src/GWGUI.App/Controllers/Emulation/Machine/MachineInputController.cs, remplacer uniquement `RelativeMouseCapture.Focus(_inputView, _inputHandle)` par `RestoreFocus`, sans déplacer la lecture de `_restorePointerAfterHostTransition`, la remise à zéro de `_hostTransition` ni la restauration conditionnelle de `_pointerCapture`.
      - [x] Dans `EnterFullscreen` de src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs, supprimer uniquement `_video.InputView.Focus()` et conserver dans le même ordre `BeginHostTransition`, le déplacement de `Screen`, l’affichage et l’activation de la fenêtre, puis `CompleteHostTransition`.
      - [x] Dans `ExitFullscreen` de src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs, supprimer uniquement `_video.InputView.Focus()` et conserver dans le même ordre `BeginHostTransition`, le replacement de `Screen`, la fermeture de la fenêtre, puis `CompleteHostTransition`.
      - [x] Dans `FullscreenContentRendered` de src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs, remplacer uniquement `_video.InputView.Focus()` par `_input.RestoreFocus()` après `_video.FitScreen()`.
  - [ ] Verrouiller chaque comportement par des tests ciblés et rapides
    - [x] Préparer le fichier de tests unique du point 1
      - [x] Créer le fichier vide tests/GWGUI.Tests/MachineFocusTests.cs sans ajouter son contenu dans la même action.
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter uniquement les doubles minimaux de `IEmulatedMachine` et `IEmulationInput`, les créations de `MachineView` et les déclencheurs d’événements nécessaires aux scénarios suivants; vérifier que le projet de tests compile avant de cocher cette case.
    - [x] Vérifier la cible commune
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter le test d’une instance active et allumée qui appelle `RestoreFocus` puis vérifie le focus de la surface WPF courante et l’absence de capture; exécuter uniquement ce test avant de cocher la case.
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter le test qui remplace la surface par `SetInputView`, appelle `RestoreFocus` puis vérifie que la nouvelle surface, et non l’ancienne, reçoit le focus; exécuter uniquement ce test avant de cocher la case.
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter le test d’une instance éteinte qui appelle RestoreFocus puis vérifie que le focus existant ne change pas; exécuter uniquement ce test avant de cocher la case.
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter le test de deux contrôleurs dont les fonctions `IsActive` renvoient des valeurs opposées, puis vérifier que seul le contrôleur actif déplace le focus; exécuter uniquement ce test avant de cocher la case.
    - [x] Vérifier les deux zones de clic
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter le test qui déclenche `MouseLeftButtonDown` avec `DisplayHost` comme source d’origine puis vérifie le retour du focus sans capture; exécuter uniquement ce test avant de cocher la case.
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter le test qui déclenche le clic existant sur la surface avec la capture autorisée puis vérifie que le comportement de capture reste actif; exécuter uniquement ce test avant de cocher la case.
    - [x] Vérifier les commandes communes
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter le test de `MachineCommandBar` qui exécute une commande réussie puis une commande en erreur, et vérifie dans les deux cas un appel de restitution ainsi que la transmission de la seule erreur à `showError`; exécuter uniquement ce test avant de cocher la case.
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter le test des boutons de média de `MachineView` qui exécute une action terminée sans modification, représentant le retour Annuler, puis une action en erreur, et vérifie dans les deux cas un appel de restitution ainsi que la transmission de la seule erreur à `showError`; exécuter uniquement ce test avant de cocher la case.
    - [ ] Terminer la validation du point
      - [x] Exécuter tous les tests de tests/GWGUI.Tests/MachineFocusTests.cs et corriger uniquement les régressions produites par les fichiers du point 1 avant de cocher cette case.
      - [ ] Exécuter toute la suite tests/GWGUI.Tests/GWGUI.Tests.csproj et corriger uniquement les régressions produites par les fichiers du point 1 avant de cocher cette case.
## Checklist détaillée — Point 2 : identification et enregistrement de la machine modifiée

Cette checklist couvre uniquement les modifications faites par l’utilisateur. Une machine qui possède un fichier est enregistrée directement dans ce fichier. Une machine qui n’en possède pas conserve un brouillon distinct en mémoire pendant toute l’exécution de l’application, jusqu’à l’utilisation du bouton Créer. Les champs de saisie sont pris en compte à la perte du focus ; les sélecteurs, options, cases et actions spécialisées le sont immédiatement.

- [x] Conserver les brouillons des machines non créées pendant l’exécution de l’application
  - [x] Créer le stockage applicatif avant toute utilisation
    - [x] Créer le fichier vide src/GWGUI.App/Services/Emulation/EmulationConfigurationDraftStore.cs.
    - [x] Modifier src/GWGUI.App/Services/Emulation/EmulationConfigurationDraftStore.cs pour conserver en mémoire au plus un IEmulationConfiguration par identifiant de module et identifiant de machine.
    - [x] Modifier src/GWGUI.App/Services/Emulation/EmulationConfigurationDraftStore.cs pour permettre la lecture, le remplacement et le retrait du brouillon d’une machine sans écrire de fichier.

- [x] Traiter chaque modification faite par l’utilisateur
  - [x] Créer le traitement commun avant de raccorder les contrôles
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour ajouter un traitement commun qui capture dans _configuration les champs génériques, les entrées et le stockage actuellement affichés.
    - [x] Modifier ce traitement dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour remplacer le brouillon de la machine dans EmulationConfigurationDraftStore lorsqu’aucune configuration enregistrée correspondante n’existe.
    - [x] Modifier ce traitement dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler IEmulationModule.SaveConfigurationAsync et signaler ConfigurationSaved lorsque la configuration de la machine existe déjà.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour utiliser le sémaphore existant lors des écritures déclenchées par l’utilisateur et écrire la dernière modification reçue après une écriture déjà en cours.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour exécuter ce traitement par ExecuteAsync afin d’afficher une erreur lorsqu’une écriture échoue.
  - [x] Raccorder les sélecteurs, options et cases
    - [x] Modifier CreateSelection dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler le traitement commun immédiatement après un changement effectué par l’utilisateur.
    - [x] Modifier CreateToggle dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler le traitement commun immédiatement après un changement effectué par l’utilisateur.
    - [x] Modifier CreateSelection et CreateToggle dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour différer le raccordement de leurs gestionnaires utilisateur et y terminer CaptureEditorValues ainsi que la reconstruction demandée par RefreshSettingsOnChange avant l’appel au traitement commun.
    - [x] Modifier ApplySettingsRules dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs pour raccorder ensuite les gestionnaires utilisateur différés, terminer les règles existantes avant leur appel au traitement commun et ne pas traiter séparément les changements de contrôle produits par ces règles.
  - [x] Raccorder les champs de saisie
    - [x] Modifier CreateField dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler le traitement commun à la perte du focus des éditeurs Text, Number et Percentage.
    - [x] Modifier CreatePath dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler le traitement commun à la perte du focus du chemin saisi et immédiatement après la sélection réussie d’un fichier.
    - [x] Modifier CreateDirectoryPath dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler le traitement commun à la perte du focus du chemin saisi et immédiatement après la sélection réussie d’un dossier.
  - [x] Raccorder les actions spécialisées
    - [x] Ajouter ConfigurationChanged dans src/GWGUI.App/Controllers/Emulation/Firmware/EmulationFirmwareManagementController.cs et le déclencher immédiatement après qu’un utilisateur a appliqué un firmware compatible avec Utiliser.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler le traitement commun lorsque EmulationFirmwareManagementController signale ConfigurationChanged.
    - [x] Ajouter SettingsChanged dans src/GWGUI.App/Controllers/Emulation/Storage/EmulationStorageSettingsController.cs et le déclencher immédiatement après qu’un utilisateur a ajouté, supprimé ou configuré un lecteur ou son média.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler le traitement commun lorsque EmulationStorageSettingsController signale SettingsChanged.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour remplacer la sauvegarde particulière des entrées par le traitement commun lorsque EmulationInputSettingsController signale SettingsChanged.

- [x] Charger, créer et supprimer les configurations sans perdre les brouillons
  - [x] Charger la machine demandée depuis la bonne source
    - [x] Modifier ReloadAsync dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour afficher la configuration enregistrée de cette machine lorsqu’elle existe, sinon son brouillon applicatif lorsqu’il existe, sinon les valeurs de base créées par le module.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour ajouter ReloadWhenOpenedAsync qui exécute ReloadAsync par ExecuteAsync et conserve la présentation d’erreur existante.
    - [x] Modifier ModuleTabSelectionChanged dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour appeler ReloadWhenOpenedAsync chaque fois que l’utilisateur rouvre un onglet Amiga ou Atari dont la section existe déjà, en conservant le chargement initial par Loaded lors de sa première ouverture.
    - [x] Modifier MachineChanged dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour afficher la configuration enregistrée, le brouillon applicatif ou les valeurs de base de la machine choisie sans réutiliser les valeurs d’une autre machine.
  - [x] Disposer de Common.Create avant de modifier le bouton existant
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/00-Base/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/ar-SA/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/cs-CZ/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/da-DK/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/de-DE/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/el-GR/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/en-US/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/es-ES/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/fi-FI/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/fr-FR/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/he-IL/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/hu-HU/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/id-ID/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/it-IT/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/ja-JP/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/ko-KR/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/nb-NO/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/nl-NL/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/pl-PL/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/pt-BR/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/pt-PT/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/ro-RO/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/ru-RU/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/sv-SE/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/th-TH/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/tr-TR/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/uk-UA/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/vi-VN/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/zh-Hans/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/zh-Hant/Actions.resx.
  - [x] Utiliser le bouton existant uniquement lorsque la configuration n’existe pas
    - [x] Modifier BuildGeneralHeader dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour conserver le bouton local existant, lui affecter Common.Create et le masquer lorsque les configurations chargées contiennent la machine affichée.
    - [x] Modifier SaveAsync dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour retirer le brouillon correspondant uniquement après la réussite de SaveConfigurationAsync, signaler ConfigurationSaved et reconstruire l’éditeur afin de masquer Créer.
  - [x] Revenir à une machine non créée après sa suppression
    - [x] Modifier DeleteSelectedConfigurationAsync dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour demander à la section déjà créée du même module de recharger ses configurations après la suppression réussie.

- [x] Distinguer visuellement les machines possédant une configuration
  - [x] Porter l’existence de la configuration dans chaque choix
    - [x] Modifier src/GWGUI.App/Contracts/Emulation/Machine/EmulationMachineChoice.cs pour ajouter l’état enregistré de Definition sans modifier DisplayName ni ToString.
    - [x] Modifier la création des choix dans le constructeur, ReloadAsync et RefreshLocalizedContent de src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour établir cet état uniquement depuis les configurations chargées du module.
  - [x] Construire la présentation commune du sélecteur
    - [x] Créer le fichier vide src/GWGUI.App/Constants/Controls/Visual/EmulationMachineChoiceVisualConstants.cs.
    - [x] Modifier src/GWGUI.App/Constants/Controls/Visual/EmulationMachineChoiceVisualConstants.cs pour définir les couleurs du fond gris clair et du texte vert forêt.
    - [x] Créer le fichier vide src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationMachineChoiceLayout.cs.
    - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationMachineChoiceLayout.cs pour créer le DataTemplate qui affiche normalement une machine non créée et applique le fond gris clair, le texte vert forêt et la graisse forte à une machine configurée.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour affecter ce DataTemplate à _machines dans la liste ouverte et dans la sélection fermée.

- [x] Afficher la marque et la machine modifiées dans le titre de Paramètres
  - [x] Ajouter le format localisé du titre avant son utilisation
    - [x] Modifier src/GWGUI.App/Resources/00-Base/Options.resx pour créer Options.EmulationMachineTitle avec les paramètres du titre Paramètres, de la marque et de la machine.
    - [x] Modifier src/GWGUI.App/Resources/ar-SA/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/cs-CZ/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/da-DK/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/de-DE/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/el-GR/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/en-US/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/es-ES/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/fi-FI/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/fr-FR/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/he-IL/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/hu-HU/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/id-ID/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/it-IT/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/ja-JP/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/ko-KR/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/nb-NO/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/nl-NL/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/pl-PL/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/pt-BR/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/pt-PT/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/ro-RO/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/ru-RU/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/sv-SE/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/th-TH/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/tr-TR/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/uk-UA/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/vi-VN/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/zh-Hans/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/zh-Hant/Options.resx pour ajouter Options.EmulationMachineTitle.
  - [x] Faire remonter la machine affichée jusqu’à Paramètres
    - [x] Créer le fichier vide src/GWGUI.App/Contracts/Emulation/Machine/EmulationMachineEditingContext.cs.
    - [x] Modifier src/GWGUI.App/Contracts/Emulation/Machine/EmulationMachineEditingContext.cs pour porter le nom localisé du module et le DisplayName de la machine affichée.
    - [x] Ajouter EditingContextChanged dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs et le déclencher après ReloadAsync, MachineChanged et RefreshLocalizedContent.
    - [x] Ajouter EditingContextChanged dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour transmettre le contexte de la section Amiga ou Atari active et l’absence de contexte dans Général, Raccourcis et Configuration.
  - [x] Modifier uniquement le titre de la fenêtre
    - [x] Modifier src/GWGUI.App/Views/Windows/Options/OptionsWindow.xaml et src/GWGUI.App/Views/Windows/Options/OptionsWindow.xaml.cs pour raccorder SelectionChanged de Navigation, écouter EditingContextChanged, afficher Options.Title seul hors de l’éditeur d’une machine et afficher Options.EmulationMachineTitle dans tous les sous-onglets de cette machine.
    - [x] Modifier RefreshLocalizedContent dans src/GWGUI.App/Views/Windows/Options/OptionsWindow.xaml.cs pour recalculer le titre après un changement de langue sans modifier le texte des onglets.

- [x] Corriger la présentation des machines possédant une configuration
  - [x] Modifier docs/tasks/interface/emulation-improvements.md pour inscrire, avant toute correction, la fermeture de l’instance ouverte, la correction des couleurs sur toute la ligne et toute la sélection fermée, la compilation, la vérification visuelle puis la fermeture.
  - [x] Fermer l’instance de GW GUI actuellement ouverte avant de modifier les fichiers utilisés par l’application.
  - [x] Modifier docs/tasks/interface/emulation-improvements.md pour inscrire le déplacement préalable de la palette Compatible vers les constantes visuelles communes et sa compilation avant sa réutilisation par le sélecteur de machines.
  - [x] Modifier src/GWGUI.App/Constants/Controls/Visual/ControlVisualConstants.cs et src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs pour déplacer les trois couleurs existantes de l’état Compatible vers les constantes visuelles communes, remplacer immédiatement leurs anciennes valeurs locales et conserver exactement le rendu du badge.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier uniquement le déplacement de la palette Compatible.
  - [x] Modifier src/GWGUI.App/Constants/Controls/Visual/EmulationMachineChoiceVisualConstants.cs pour remplacer le gris par le fond vert clair, le texte vert et la bordure verte de la palette Compatible commune.
  - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationMachineChoiceLayout.cs pour retirer le fond limité au texte, conserver le texte en gras pour une machine configurée et créer les styles qui appliquent le fond, le texte et la bordure à toute la ligne déroulée ainsi qu’à toute la sélection fermée.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appliquer au ComboBox des machines les deux styles créés, sans modifier sa sélection ni son fonctionnement.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par cette correction visuelle.
  - [x] Modifier docs/tasks/interface/emulation-improvements.md après la première vérification visuelle pour inscrire la correction de la liaison de l’état configuré au contexte de données réel de chaque ComboBoxItem avant de relancer l’application.
  - [x] Modifier CreateItemContainerStyle dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationMachineChoiceLayout.cs pour lier directement HasSavedConfiguration depuis EmulationMachineChoice au lieu de rechercher Content.HasSavedConfiguration sur cet objet.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par cette correction de liaison.
  - [x] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build et vérifier que chaque machine configurée colore toute sa ligne et toute la sélection fermée en vert clair, sans rectangle gris limité au texte, tandis qu’une machine non configurée conserve la présentation normale.
  - [x] Fermer l’instance de GW GUI utilisée pour cette vérification.

## Checklist détaillée — Point 3 : tableau des configurations

Dans l’ordre général, ce point constitue le groupe 2. Il utilise l’état fiable des configurations établi au point 2 et doit être terminé avant le retour automatique du focus du point 1. La présentation textuelle actuelle est remplacée par un tableau sans sélection de ligne. Seuls le filtre de marque, le crayon, le double-clic et la poubelle produisent une action.

- [x] Préparer les fonctions de présentation communes
  - [x] Créer le fichier commun avant d’y déplacer les fonctions
    - [x] Créer src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationSettingsValuePresentationFunctions.cs vide.
  - [x] Déplacer entièrement chaque fonction avant de passer à la suivante
    - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationSettingsValuePresentationFunctions.cs et src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs pour déplacer DisplayValue dans le fichier commun avec exactement les mêmes choix, valeurs de repli et règles de localisation, remplacer ses appels puis supprimer immédiatement sa définition privée d’origine.
    - [x] Modifier src/GWGUI.App/Contracts/Emulation/Machine/EmulationMachineEditingContext.cs pour remplacer le record qui hérite de EventArgs par une classe sealed conservant le même constructeur et les mêmes propriétés ModuleDisplayName et MachineDisplayName.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le déplacement complet de DisplayValue.
    - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationSettingsValuePresentationFunctions.cs et src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs pour déplacer DefaultNumericValue dans le fichier commun avec exactement les mêmes sources numériques et la même valeur de repli, remplacer son appel puis supprimer immédiatement sa définition privée d’origine.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le déplacement complet de DefaultNumericValue.
    - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationSettingsValuePresentationFunctions.cs et src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs pour déplacer FormatMemorySize dans le fichier commun avec exactement les mêmes seuils, formats et unités, remplacer son appel puis supprimer immédiatement sa définition privée d’origine.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le déplacement complet de FormatMemorySize.

- [x] Préparer les constantes et le style déjà utilisés
  - [x] Déplacer le glyphe du clavier vers sa portée commune
    - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationInputSettingsConstants.cs et src/GWGUI.App/Constants/Emulation/EmulationMachineTabConstants.cs pour déplacer la valeur U+E765 dans KeyboardIcon, remplacer immédiatement la valeur littérale de l’onglet Clavier puis ne conserver qu’une seule définition du glyphe.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le déplacement du glyphe Clavier.
  - [x] Ajouter le glyphe de modification manquant
    - [x] Modifier src/GWGUI.App/Constants/Controls/Visual/ControlVisualConstants.cs pour ajouter EditGlyph avec la valeur U+E70F déjà utilisée par l’action de modification, sans modifier DeleteGlyph.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier EditGlyph.
  - [x] Déplacer le style d’en-tête vers les ressources globales
    - [x] Modifier src/GWGUI.App/App.xaml et src/GWGUI.App/Views/Controls/Emulation/Input/InputBindingEditor.xaml pour déplacer TableHeaderText dans Application.Resources avec FontWeight à SemiBold, VerticalAlignment à Center et Margin à 14,0, laisser InputBindingEditor utiliser la ressource déplacée puis supprimer immédiatement sa déclaration locale.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le déplacement complet de TableHeaderText.

- [x] Ajouter les textes visibles du nouveau tableau
  - [x] Créer les ressources de base
    - [x] Modifier src/GWGUI.App/Resources/00-Base/Emulation.resx pour créer Emulation.Configuration.Brand, Emulation.Configuration.Machine, Emulation.Configuration.TotalRam, Emulation.Configuration.Readers, Emulation.Configuration.Peripherals, Emulation.Configuration.Actions et Emulation.Configuration.DeleteConfirm ; DeleteConfirm reçoit uniquement la marque et la machine.
  - [x] Ajouter les sept ressources dans toutes les langues prises en charge
    - [x] Modifier src/GWGUI.App/Resources/ar-SA/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/cs-CZ/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/da-DK/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/de-DE/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/el-GR/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/en-US/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/es-ES/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/fi-FI/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/fr-FR/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/he-IL/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/hu-HU/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/id-ID/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/it-IT/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/ja-JP/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/ko-KR/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/nb-NO/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/nl-NL/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/pl-PL/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/pt-BR/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/pt-PT/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/ro-RO/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/ru-RU/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/sv-SE/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/th-TH/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/tr-TR/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/uk-UA/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/vi-VN/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/zh-Hans/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/zh-Hant/Emulation.resx.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier toutes les ressources ajoutées.

- [x] Créer les données structurées des lignes
  - [x] Créer le contrat avant son contenu
    - [x] Créer src/GWGUI.App/Contracts/Emulation/Configurations/EmulationConfigurationTableRow.cs vide.
  - [x] Définir uniquement les données nécessaires aux colonnes et aux actions
    - [x] Modifier src/GWGUI.App/Contracts/Emulation/Configurations/EmulationConfigurationTableRow.cs pour porter IEmulationModule, IEmulationConfiguration, le nom localisé de la machine, le CPU, la RAM totale, la liste des glyphes de lecteurs et la liste des glyphes de périphériques.
  - [x] Créer le présentateur avant son contenu
    - [x] Créer src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs vide.
  - [x] Produire Machine, CPU et RAM totale depuis les données structurées
    - [x] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour créer une ligne depuis IEmulationModule et IEmulationConfiguration, retrouver la machine dans IEmulationModule.Machines et localiser sa DisplayResourceKey sans analyser EmulationConfigurationSummary ni le DisplayName textuel existant.
    - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationHardwareSettingsConstants.cs et src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs pour déplacer la clé existante Emulation.Cpu.Model dans CpuModelResourceKey, remplacer immédiatement son utilisation actuelle puis ne conserver qu’une seule définition de cette valeur.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le déplacement de CpuModelResourceKey.
    - [x] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour obtenir le CPU depuis le champ visible Emulation.Cpu.Model de l’onglet CPU renvoyé par IEmulationModule.Describe et le formater avec EmulationSettingsValuePresentationFunctions.DisplayValue.
    - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationMemorySettingsConstants.cs pour ajouter ValueUnitSeparator avec un espace unique destiné à séparer la valeur de RAM de son unité dans le tableau.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier ValueUnitSeparator.
    - [x] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour additionner les valeurs numériques des champs visibles de l’onglet RAM avec EmulationSettingsValuePresentationFunctions.DefaultNumericValue, formater le total avec FormatMemorySize et laisser CPU ou RAM vide uniquement en l’absence réelle de donnée correspondante.
  - [x] Produire exactement une icône par lecteur configuré
    - [x] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour appeler IEmulationStorageSettingsManager.DescribeStorageSettings lorsque le module fournit ce service, parcourir ConfiguredSlots, retrouver chaque périphérique par EmulationMediaSlot dans AvailableDevices et produire un glyphe par occurrence avec FloppyGlyph, HardDiskGlyph, CompactDiscGlyph, CassetteGlyph ou CartridgeGlyph selon EmulationMediaType.
  - [x] Produire exactement une icône par périphérique configuré
    - [x] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour appeler IEmulationInputSettingsManager.DescribeInputSettings lorsque le module fournit ce service, ajouter KeyboardIcon lorsque Keyboard existe et MouseIcon lorsque Mouse existe.
    - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationInputSettingsConstants.cs pour ajouter NoneControllerResourceKey avec Emulation.Controller.None, KeyboardControllerId avec Keyboard et MouseControllerId avec Mouse afin d’identifier les choix de port sans valeur brute.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier les identifiants de contrôleur.
    - [x] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour résoudre chaque SelectedControllerId dans ControllerChoices, ignorer le choix dont la ressource est Emulation.Controller.None et ajouter pour chaque autre port le glyphe clavier, souris ou manette correspondant au choix.
  - [x] Classer et limiter les lignes
    - [x] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour classer les lignes par nom de machine avec StringComparer.CurrentCulture et ne produire aucun identifiant technique affichable, ROM, moteur vidéo, format vidéo, état audio ou action de lancement.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le contrat et le présentateur.

- [x] Créer le tableau sans mécanisme de sélection
  - [x] Créer le contrôle avant son contenu
    - [x] Créer src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs vide.
  - [x] Construire les six colonnes et les lignes
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour recevoir des EmulationConfigurationTableRow, utiliser un ItemsControl dans un ScrollViewer et ne créer ni SelectedItem, ni SelectedIndex, ni état visuel de sélection.
    - [x] Créer src/GWGUI.App/Constants/Emulation/EmulationConfigurationTableConstants.cs vide.
    - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationConfigurationTableConstants.cs pour définir dans l’ordre les six clés d’en-tête, TableHeaderTextStyleResource, CellMargin, HeaderSeparatorThickness et RowSeparatorThickness avec les valeurs déjà utilisées par les tableaux existants.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour créer dans l’ordre Machine, CPU, RAM totale, Lecteurs, Périphériques et Actions en utilisant les ressources Emulation.Configuration correspondantes, Emulation.Tab.Cpu, TableHeaderText, CardBrush et BorderBrush.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour afficher Machine, CPU et RAM totale, puis chaque glyphe de lecteur et de périphérique séparément sans nombre, texte permanent, infobulle de port ni information supplémentaire.
  - [x] Ajouter uniquement les trois interactions autorisées
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour ajouter EditRequested et DeleteRequested en transmettant directement la EmulationConfigurationTableRow concernée.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour créer le bouton crayon avec EditGlyph et le bouton poubelle avec DeleteGlyph dans la colonne Actions.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour envoyer le bouton crayon et le double-clic de ligne vers le même chemin interne qui déclenche EditRequested.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour traiter l’action du bouton poubelle avant DeleteRequested afin qu’un double-clic sur ce bouton ne remonte jamais vers EditRequested.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour ne déclencher aucune action lors d’un simple clic ailleurs dans une ligne.
  - [x] Actualiser les textes du contrôle
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour ajouter RefreshLocalizedContent et y reconstruire les six en-têtes avec les ressources de la langue active.
    - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationConfigurationTableConstants.cs pour définir CellMargin avec les quatre côtés attendus par Thickness.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le contrôle avant son raccordement.

- [x] Préparer l’ouverture complète d’une configuration
  - [x] Déplacer entièrement la création différée d’une section de marque
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour déplacer le bloc de création de EmulationModuleSettingsSection depuis ModuleTabSelectionChanged vers GetOrCreateModuleSection, conserver les abonnements ConfigurationSaved et EditingContextChanged, l’ajout dans _moduleSections et l’affectation à TabItem.Content, remplacer immédiatement l’ancien bloc par l’appel à la méthode puis supprimer le bloc d’origine.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier que l’ouverture manuelle des onglets de marque utilise GetOrCreateModuleSection et conserve ReloadWhenOpenedAsync.
  - [x] Ajouter l’ouverture explicite de la configuration choisie
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour ajouter EditConfigurationAsync recevant IEmulationConfiguration, recharger _saved, retenir exactement la configuration transmise, sélectionner sa machine, fixer _selectedTab à EmulationMachineTab.General, reconstruire tous les sous-onglets et actualiser l’état de l’émulateur installé sans lancer la machine.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier l’entrée explicite de l’éditeur.
  - [x] Ajouter la remise à zéro après une suppression
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour ajouter ReloadAfterConfigurationDeletedAsync recevant l’identifiant et la machine supprimés, retirer le brouillon de cette machine uniquement si l’identifiant supprimé est actuellement chargé, puis réutiliser ReloadAsync afin de recharger _saved, reconstruire les valeurs de base de cette machine et faire réapparaître Créer.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour typer configurationId en Guid comme IEmulationConfiguration.Id.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier la remise à zéro ciblée.

- [x] Préparer le nouveau contenu avant de remplacer l’ancienne liste
  - [x] Ajouter les champs du filtre et du tableau
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour ajouter le TextBlock du libellé Marque, le ComboBox de marque, la collection de EmulationModuleListItem des marques configurées, la liste complète de EmulationConfigurationTableRow et EmulationConfigurationTable sans supprimer encore _configurations, _configurationList ni _removeConfiguration.
  - [x] Alimenter le filtre et le tableau pendant le chargement existant
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour construire les nouvelles lignes avec EmulationConfigurationTablePresenter après chaque chargement tout en continuant provisoirement d’alimenter _configurations.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour reconstruire la liste des marques avec uniquement les modules possédant au moins une configuration, conserver la marque choisie si elle existe encore et laisser le ComboBox sans sélection si elle a disparu ou si aucune configuration n’existe.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour afficher uniquement les lignes de la marque choisie et laisser le tableau vide sans marque choisie.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour raccorder SelectionChanged du ComboBox à ce filtrage sans créer de sélection dans le tableau.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier les données du nouveau contenu.
  - [x] Raccorder l’ouverture de ligne
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour ajouter EditConfigurationAsync recevant EmulationConfigurationTableRow, obtenir la section de row.Module par GetOrCreateModuleSection, appeler son EditConfigurationAsync avec row.Configuration puis sélectionner le TabItem correspondant une fois la configuration chargée.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour raccorder EditRequested du tableau à cette méthode unique afin que le crayon et le double-clic ne puissent pas diverger.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le raccordement de l’ouverture.
  - [x] Raccorder la suppression de ligne
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour ajouter DeleteConfigurationAsync recevant EmulationConfigurationTableRow et ouvrir une MessageBox Oui/Non utilisant Common.Delete comme titre et Emulation.Configuration.DeleteConfirm comme message avec uniquement la marque localisée et la machine localisée.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour ne rien supprimer ni recharger lorsque la réponse n’est pas Oui, appeler row.Module.DeleteConfigurationAsync avec row.Configuration.Id uniquement après Oui et présenter toute erreur avec ControlErrorPresenter sans retirer la ligne lorsque la suppression échoue.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour recharger après une suppression réussie, conserver la marque tant qu’elle possède une ligne, laisser le ComboBox et le tableau sans sélection après sa dernière ligne sans choisir une autre marque, puis appeler ReloadAfterConfigurationDeletedAsync sur la section correspondante lorsqu’elle existe déjà.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour raccorder DeleteRequested à DeleteConfigurationAsync sans utiliser SelectedItem.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour importer System.Windows utilisé par la MessageBox de confirmation.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le raccordement de la suppression.
  - [x] Raccorder l’actualisation localisée
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour importer EmulationConfigurationTablePresenter utilisé par RefreshLocalizedContent.
    - [x] Modifier RefreshLocalizedContent dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour actualiser le libellé Marque, appeler EmulationConfigurationTable.RefreshLocalizedContent et reconstruire les marques et les lignes localisées en conservant la marque choisie si elle existe encore sans en sélectionner une autre.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier l’actualisation localisée du nouveau contenu.

- [x] Remplacer définitivement l’ancienne liste par le nouveau contenu
  - [x] Effectuer le remplacement complet dans une seule tâche
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionLayoutFunctions.cs, src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs et src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour ajouter le libellé Marque, son ComboBox et EmulationConfigurationTable dans BuildConfigurationsTab, puis supprimer immédiatement l’ancienne ListBox, le bouton Supprimer global, RemoveConfiguration, DeleteSelectedConfigurationAsync, l’alimentation de _configurations, les champs _configurations, _configurationList et _removeConfiguration, leur gestionnaire SelectionChanged, l’ancienne actualisation de Common.Delete et les directives using devenues inutiles.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le remplacement complet et la suppression de tout le fonctionnement fondé sur une ligne sélectionnée.

- [x] Espacer uniformément les icônes des lecteurs et des périphériques
  - [x] Modifier docs/tasks/interface/emulation-improvements.md pour inscrire, avant toute correction, l’ajout d’un espacement commun de 8 pixels, son application aux deux colonnes, la compilation, la vérification visuelle puis la fermeture.
  - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationConfigurationTableConstants.cs pour ajouter un espacement horizontal de 8 pixels entre deux icônes d’une même cellule.
  - [x] Modifier GlyphCell dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour appliquer cet espacement entre les icônes sans marge extérieure supplémentaire, afin que Lecteurs et Périphériques utilisent exactement la même règle.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par cet espacement.
  - [x] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build et vérifier dans le tableau Configuration que toutes les icônes multiples de Lecteurs et de Périphériques possèdent le même espacement, sans modifier le nombre ni l’ordre des icônes.
  - [x] Fermer l’instance de GW GUI utilisée pour cette vérification.

## Checklist détaillée — Point 4 : destination des ROM détectées

Cette checklist réalise la demande fonctionnelle décrite dans la section 5. Elle conserve la liste et le bouton Utiliser existants. La destination affichée provient du même identifiant de champ que celui consommé par Utiliser ; l’application ne maintient aucune seconde correspondance.

- [x] Inscrire les deux décisions d’affichage encore manquantes avant de modifier le code
  - [x] Modifier docs/tasks/interface/emulation-improvements.md, dans la section 5, pour remplacer le nombre maximal de caractères restant à fixer par la valeur validée et préciser si l’ellipse est comprise dans cette limite.
  - [x] Modifier docs/tasks/interface/emulation-improvements.md, dans la section 5, pour inscrire la position validée de Destination par rapport au nom de la ROM et à Compatibilité.

- [x] Faire porter la destination par le résultat commun du scan
  - [x] Modifier src/GWGUI.Emulation/Contracts/EmulationFirmwareCandidate.cs pour ajouter l’identifiant optionnel du champ de destination à la ROM détectée.
  - [x] Modifier ScanFirmwareAsync dans src/GWGUI.Emulation.Amiga/Modules/AmigaEmulationModule.cs pour renseigner cet identifiant depuis le type déjà obtenu par AmigaFirmwareCatalog, avec KickstartPath, ExtendedRomPath ou RomKeyPath, et le laisser vide lorsqu’aucun de ces champs ne correspond.
  - [x] Modifier UseFirmware dans src/GWGUI.Emulation.Amiga/Modules/AmigaEmulationModule.cs pour utiliser l’identifiant porté par EmulationFirmwareCandidate afin de choisir le champ à remplacer et supprimer la seconde inspection actuellement réalisée uniquement pour retrouver cette destination.
  - [x] Modifier ScanFirmwareAsync dans src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs pour renseigner SystemFirmware lorsque la ROM détectée possède une destination pour la machine affichée et laisser l’identifiant vide sinon.
  - [x] Modifier UseFirmware dans src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs pour vérifier et consommer ce même identifiant avant d’appliquer la sélection Atari existante, sans ajouter une autre table de routage.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs provoquées par l’ajout de la destination au contrat commun.

- [x] Transmettre le module nécessaire à la résolution du libellé
  - [x] Modifier le constructeur de src/GWGUI.App/Controllers/Emulation/Firmware/EmulationFirmwareManagementController.cs pour recevoir le IEmulationModule déjà détenu par EmulationModuleSettingsSection.
  - [x] Modifier docs/tasks/interface/emulation-improvements.md pour placer le raccordement de EmulationModuleSettingsSection avant l’utilisation du nouveau constructeur et réunir dans une seule tâche la résolution et la transmission du libellé après l’extension de FirmwareRow.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour transmettre _module à EmulationFirmwareManagementController sans changer les raccordements de ConfigurationChanged.
  - [x] Modifier docs/tasks/interface/emulation-improvements.md pour renommer ce groupe selon les actions qu’il contient maintenant, avant de le cocher.

- [x] Ajouter la cellule informative à la ligne existante
  - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationFirmwareSettingsConstants.cs pour ajouter la limite de caractères validée et uniquement les dimensions nécessaires à la colonne validée.
  - [x] Modifier FirmwareRow dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs pour recevoir le libellé de destination, le limiter avec une ellipse selon la décision inscrite et l’afficher comme texte simple à la position validée, dans la présentation de la compatibilité existante.
  - [x] Modifier RefreshAsync dans src/GWGUI.App/Controllers/Emulation/Firmware/EmulationFirmwareManagementController.cs pour rechercher DestinationFieldId dans les champs retournés par IEmulationModule.Describe pour la machine et la configuration affichées, localiser directement LabelResourceKey, transmettre ce texte à FirmwareRow et transmettre un texte vide si l’identifiant est absent ou introuvable, sans modifier le nom, la version, la compatibilité, le chemin ou l’ordre des ROM.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par la nouvelle cellule.

- [x] Corriger les écarts constatés dans l’affichage réel avant de reprendre la vérification
  - [x] Modifier docs/tasks/interface/emulation-improvements.md pour inscrire, avant toute correction, la correction du libellé Kickstart, de la présentation et de la largeur de Destination, la compilation puis la reprise de la vérification dans une nouvelle exécution.
  - [x] Modifier src/GWGUI.App/Resources/00-Base/Emulation.resx et chaque src/GWGUI.App/Resources/<langue>/Emulation.resx pour ajouter une clé de ressource Kickstart dont la valeur visible reste Kickstart dans toutes les langues prises en charge.
  - [x] Modifier src/GWGUI.Emulation.Amiga/Constants/AmigaSettingsDescriptionFunctionsConstants.cs et src/GWGUI.Emulation.Amiga/Functions/AmigaSettingsDescriptionFunctions.cs pour remplacer le texte brut Kickstart utilisé comme LabelResourceKey par la nouvelle clé de ressource, afin que le champ existant et Destination affichent tous deux Kickstart sans crochets.
  - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationFirmwareSettingsConstants.cs pour remplacer la largeur de Destination copiée depuis Compatibilité par uniquement l’identifiant de groupe nécessaire à une largeur partagée calculée depuis le contenu.
  - [x] Modifier docs/tasks/interface/emulation-improvements.md pour inscrire la copie préalable de la construction du badge Compatibilité dans une fonction commune et sa compilation avant le remplacement de l’ancien bloc.
  - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs pour copier la construction actuelle du Border de Compatibilité dans une fonction FirmwareBadge recevant le texte et les couleurs, sans retirer ni remplacer le bloc existant.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier uniquement l’ajout de FirmwareBadge avant son utilisation.
  - [x] Modifier FirmwareSettingsPage et FirmwareRow dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs pour partager automatiquement la largeur de Destination entre les lignes, rendre au nom de ROM l’espace restant, remplacer le bloc Compatibilité par FirmwareBadge puis afficher Destination avec la même fonction et les couleurs de compatibilité de la ligne.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par ces corrections d’affichage.

- [x] Corriger la largeur des deux badges après le constat visuel
  - [x] Modifier docs/tasks/interface/emulation-improvements.md pour inscrire la fermeture de l’instance affichée, la largeur identique de Compatibilité et Destination, leur alignement à droite avec un petit espacement, l’espace restant réservé au nom, la compilation et la nouvelle vérification visuelle.
  - [x] Fermer l’instance de GW GUI utilisée pour constater cette disposition.
  - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationFirmwareSettingsConstants.cs pour remplacer les largeurs distinctes de Compatibilité et Destination par un seul groupe de largeur partagée entre les deux badges et ajouter uniquement l’espacement validé entre eux.
  - [x] Modifier FirmwareRow et FirmwareBadge dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs pour laisser la colonne du nom prendre tout l’espace restant, placer à droite deux colonnes automatiques dans le même groupe de largeur, étirer et centrer chaque badge dans sa colonne et conserver le petit espacement entre eux.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par cette disposition.
  - [x] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build et ouvrir Options > Émulation > Amiga > ROM.
  - [x] Capturer uniquement la fenêtre Options et vérifier que le nom de ROM utilise l’espace restant tandis que Compatibilité et Destination ont exactement la même largeur, restent à droite et sont séparées par un petit espace.
  - [x] Fermer l’instance de GW GUI utilisée pour cette vérification visuelle.
- [x] Restaurer le libellé Atari et supprimer le redimensionnement de la fenêtre Options
  - [x] Modifier docs/tasks/interface/emulation-improvements.md pour inscrire la fermeture de l’instance affichée, la restauration immédiate du libellé Atari, la désactivation du redimensionnement, la compilation et la vérification visuelle.
  - [x] Fermer l’instance de GW GUI affichée pendant ce constat.
  - [x] Restaurer dans src/GWGUI.App/Resources/fr-FR/Emulation.resx la valeur exacte ROM système pour Emulation.Firmware.Rom.System.
  - [x] Modifier src/GWGUI.App/Views/Windows/Options/OptionsWindow.xaml pour remplacer ResizeMode=CanResizeWithGrip par ResizeMode=NoResize sans modifier Width, Height, MinWidth ni MinHeight.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par ces deux corrections.
  - [x] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build, ouvrir Options > Émulation > Atari > ROM et vérifier que le titre et le champ affichent de nouveau ROM système et que la fenêtre ne possède plus de poignée ni de commande de redimensionnement.
  - [x] Fermer l’instance de GW GUI utilisée pour cette vérification.
- [x] Restaurer l’identification TOS dans le nom des ROM Atari reconnues
  - [x] Modifier docs/tasks/interface/emulation-improvements.md pour inscrire le préfixe TOS devant la version d’une ROM TOS reconnue, la conservation du nom complet pour une ROM non reconnue, la compilation et la vérification visuelle dans l’application.
  - [x] Modifier ScanFirmwareAsync dans src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs pour afficher TOS suivi de la version lorsqu’une ROM TOS est reconnue et conserver Path.GetFileName(scanned.Path) lorsqu’elle n’est pas reconnue.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par cette modification.
  - [x] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build, ouvrir Options > Émulation > Atari > ROM et vérifier que les quatre ROM reconnues affichent TOS devant leur version au lieu du seul numéro.
  - [x] Fermer l’instance de GW GUI utilisée pour cette vérification.
- [x] Vérifier le fonctionnement demandé avant de terminer le point
  - [x] Modifier docs/tasks/interface/emulation-improvements.md avant la nouvelle exécution pour séparer chaque cas vérifié, inscrire les fichiers et le libellé temporaires nécessaires aux données absentes, puis inscrire leur suppression ou restauration et la compilation finale.
  - [x] Modifier docs/tasks/interface/emulation-improvements.md pour remplacer les fausses données de vérification prévues par les vraies données retrouvées : C:/Users/overt/Downloads/Recalbox_10.0.8_BIOS_Pack/rom.key et les quatre ROM TOS déjà présentes dans %APPDATA%/GW GUI/Emulation/Machines/Atari/Firmware/ST.
  - [x] Copier temporairement la vraie clé C:/Users/overt/Downloads/Recalbox_10.0.8_BIOS_Pack/rom.key vers %APPDATA%/GW GUI/Emulation/Machines/Amiga/Firmware/rom.key, uniquement si la cible n’existe pas, afin que le scan Amiga puisse la détecter sans modifier le fichier source.
  - [x] Modifier temporairement uniquement la valeur de Emulation.Firmware.Rom.System dans src/GWGUI.App/Resources/fr-FR/Emulation.resx de ROM système vers ROM système particulièrement longue pour vérifier la limite de 20 caractères et l’ellipse, sans modifier sa clé.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore afin d’intégrer uniquement le libellé temporaire nécessaire à cette vérification.
  - [x] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build et ouvrir Options > Émulation.
  - [x] Vérifier dans Amiga > ROM qu’une Kickstart affiche Kickstart après Compatibilité, sans crochets, et que Utiliser renseigne le champ Kickstart sur une machine sans configuration enregistrée.
  - [x] Vérifier dans une machine Amiga CDTV ou CD32 qu’une ROM étendue réelle affiche ROM étendue après Compatibilité et correspond au champ ROM étendue.
  - [x] Vérifier dans une machine Amiga sans configuration enregistrée que rom.key affiche Clé ROM après Compatibilité et que Utiliser renseigne le champ Clé ROM.
  - [x] Vérifier dans une machine Atari ST sans configuration enregistrée qu’une des quatre vraies ROM TOS compatibles affiche le libellé système tronqué à 20 caractères avec une ellipse après Compatibilité et que Utiliser renseigne le champ système.
  - [x] Vérifier dans la même machine Atari ST que toute vraie ROM TOS incompatible laisse Destination vide et Utiliser désactivé lorsqu’elle est sélectionnée ; si les quatre ROM possèdent une destination pour le modèle affiché, constater explicitement que ce cas ne peut pas être vérifié avec les données réelles au lieu de fabriquer une ROM.
  - [x] Dans la même exécution, vérifier que les badges Destination utilisent la même présentation et les mêmes couleurs que Compatibilité, que le nom et la version des ROM disposent de l’espace restant, et que la sélection et le bouton Utiliser conservent leur comportement.
  - [x] Fermer l’instance de GW GUI utilisée pour cette vérification.
  - [x] Supprimer uniquement la copie temporaire %APPDATA%/GW GUI/Emulation/Machines/Amiga/Firmware/rom.key créée pour cette vérification, sans modifier la vraie clé source ni les ROM Atari existantes.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier l’état final après suppression de tous les artefacts temporaires.

## Checklist détaillée — Point 5 : aides contextuelles sur les champs

Cette checklist réalise la demande fonctionnelle décrite dans la section 4. Les aides concernent uniquement les champs explicitement validés dans les éditeurs Amiga et Atari. ExplanationResourceKey devient la clé de l’aide courte ; une seconde clé distincte transporte l’aide concise au clic.

- [ ] Fixer le périmètre et le contenu avant de créer l’interface
  - [ ] Modifier docs/tasks/interface/emulation-improvements.md, dans la section 4, pour ajouter un tableau des champs visibles provenant de src/GWGUI.Emulation.Amiga/Functions/AmigaSettingsDescriptionFunctions.cs, src/GWGUI.Emulation.Atari/Functions/AtariSettingsDescriptionFunctions.cs et des champs fixes construits par l’application, en excluant les boutons et titres.
  - [ ] Modifier le tableau de la section 4 dans docs/tasks/interface/emulation-improvements.md pour marquer uniquement les champs dont le libellé ne suffit pas, après validation de leur présence ou de leur absence d’aide ; ne pas prévoir d’aide pour le sélecteur de périphérique physique dont la suppression est demandée au point 6.
  - [ ] Modifier le tableau de la section 4 dans docs/tasks/interface/emulation-improvements.md pour inscrire, pour chaque champ retenu, la clé d’aide courte, son texte d’une ligne, la clé d’aide concise et son texte expliquant uniquement le rôle, les choix et leurs différences utiles.
  - [ ] Modifier la section 4 dans docs/tasks/interface/emulation-improvements.md pour inscrire la présentation validée du post-it, notamment ses dimensions maximales, son placement et ses couleurs, afin qu’aucune valeur visuelle ne soit choisie pendant l’implémentation.

- [ ] Étendre les contrats communs avant de modifier les mises en page
  - [ ] Modifier src/GWGUI.Emulation/Contracts/EmulationSettingsField.cs pour conserver ExplanationResourceKey comme clé optionnelle de l’aide courte et ajouter DetailedExplanationResourceKey comme clé optionnelle de l’aide concise.
  - [ ] Modifier src/GWGUI.App/Contracts/Emulation/Settings/EmulationSettingsControlField.cs pour transporter le libellé, le contrôle, l’aide courte localisée et l’aide concise localisée, tout en autorisant l’absence des deux aides.
  - [ ] Modifier src/GWGUI.App/Contracts/Views/Emulation/Settings/EmulationCpuSettingsContent.cs pour transporter des EmulationSettingsControlField pour les champs CPU actuellement séparés, sans intégrer le résumé du processeur à un champ d’aide.
  - [ ] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs provoquées par l’extension de ces contrats.

- [ ] Créer le libellé réutilisable avant de remplacer les libellés actuels
  - [ ] Créer le fichier vide src/GWGUI.App/Constants/Emulation/EmulationSettingsFieldHelpConstants.cs.
  - [ ] Modifier src/GWGUI.App/Constants/Emulation/EmulationSettingsFieldHelpConstants.cs pour définir uniquement les dimensions, espacements et couleurs validés du post-it.
  - [ ] Créer le fichier vide src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs.
  - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour reproduire le TextBlock actuel lorsque les deux aides sont absentes et ne créer aucune icône dans ce cas.
  - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour afficher immédiatement après le libellé une icône permanente utilisant ControlVisualConstants.InformationGlyph lorsque les deux aides sont présentes, avec uniquement sa taille visible comme zone cliquable.
  - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour affecter l’aide courte à une infobulle sans retour à la ligne ni défilement, visible seulement pendant le survol.
  - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour ouvrir au clic un Popup de type post-it contenant le libellé et l’aide concise, selon les valeurs validées, et activer le défilement uniquement lorsque le contenu dépasse ses dimensions maximales.
  - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour fermer ce Popup sur toute touche ou sur le clic suivant, sans le fermer pendant le clic d’ouverture, puis détacher tous ses gestionnaires lors de la fermeture et de Unloaded.
  - [ ] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par ce contrôle.

- [ ] Faire passer les champs décrits par les modules par un seul chemin
  - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour ajouter CreateControlField, qui crée le contrôle existant, localise LabelResourceKey et les deux clés d’aide lorsqu’elles existent, puis retourne EmulationSettingsControlField.
  - [ ] Modifier AddBlocks dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour utiliser CreateControlField sans modifier l’ordre, les colonnes, la visibilité ou les contrôles des blocs.
  - [ ] Modifier BuildCpuSettingsTab et BuildMemorySettingsTab dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs pour utiliser CreateControlField sans modifier les choix, les règles, les résumés ni le calcul de RAM totale.
  - [ ] Modifier BuildInputSettingsTab dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleInputSettingsSection.cs pour utiliser CreateControlField sans modifier les associations ni leur enregistrement.

- [ ] Remplacer les libellés des mises en page par le contrôle commun
  - [ ] Modifier CompactForm dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationSettingsLayout.cs pour recevoir des EmulationSettingsControlField et construire leurs libellés avec EmulationSettingsFieldLabel, puis conserver une surcharge sans aide pour les appelants hors des éditeurs de machine.
  - [ ] Modifier SettingsFields et SettingsFieldGrid dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationHardwareSettingsLayout.cs pour recevoir des EmulationSettingsControlField, utiliser EmulationSettingsFieldLabel et lier sa visibilité à celle du contrôle correspondant.
  - [ ] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationCpuSettingsLayout.cs pour consommer les EmulationSettingsControlField de EmulationCpuSettingsContent sans modifier les cartes Processeur, Compatibilité et Accélération.
  - [ ] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationMemorySettingsLayout.cs pour transmettre les EmulationSettingsControlField sans perdre les aides ni modifier les cadres de mémoire.
  - [ ] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationInputSettingsLayout.cs pour transmettre les EmulationSettingsControlField de la souris et des options analogiques sans modifier les tableaux d’associations.
  - [ ] Modifier docs/tasks/interface/emulation-improvements.md après validation du tableau pour ajouter à cet emplacement une sous-tâche distincte, nommant son fichier, pour chaque champ fixe approuvé qui ne passe pas encore par EmulationSettingsControlField ; n’effectuer aucune modification de ce champ avant l’ajout de sa sous-tâche.
  - [ ] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs provoquées par le remplacement des libellés.

- [ ] Ajouter les paires de textes validées dans toutes les ressources avant de les utiliser
  - [ ] Modifier src/GWGUI.App/Resources/00-Base/Emulation.resx pour ajouter exactement les clés et textes validés dans le tableau de la section 4.
      - [ ] Modifier src/GWGUI.App/Resources/ar-SA/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/cs-CZ/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/da-DK/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/de-DE/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/el-GR/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/en-US/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/es-ES/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/fi-FI/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/fr-FR/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/he-IL/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/hu-HU/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/id-ID/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/it-IT/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/ja-JP/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/ko-KR/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/nb-NO/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/nl-NL/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/pl-PL/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/pt-BR/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/pt-PT/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/ro-RO/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/ru-RU/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/sv-SE/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/th-TH/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/tr-TR/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/uk-UA/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/vi-VN/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/zh-Hans/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [ ] Modifier src/GWGUI.App/Resources/zh-Hant/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
  - [ ] Modifier src/GWGUI.Emulation.Amiga/Functions/AmigaSettingsDescriptionFunctions.cs pour affecter les deux clés uniquement aux champs Amiga approuvés dans le tableau.
  - [ ] Modifier src/GWGUI.Emulation.Atari/Functions/AtariSettingsDescriptionFunctions.cs pour affecter les deux clés uniquement aux champs Atari approuvés dans le tableau, sans réutiliser les explications de compatibilité propres à Atari.
  - [ ] Réaliser dans l’ordre chaque sous-tâche de champ fixe ajoutée à docs/tasks/interface/emulation-improvements.md afin de transporter exactement les deux clés approuvées, sans étendre l’aide à un autre élément.

- [ ] Vérifier les ressources et le comportement avant de terminer le point
  - [ ] Exécuter un contrôle de parité des clés d’aide entre src/GWGUI.App/Resources/00-Base/Emulation.resx et les 29 fichiers de langue, puis corriger uniquement les clés absentes ou supplémentaires créées par ce point.
  - [ ] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par les ressources et les clés d’aide.
  - [ ] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build et vérifier chaque champ approuvé dans les onglets Amiga et Atari : icône toujours visible, aide courte d’une ligne au survol et post-it au clic.
  - [ ] Dans la même exécution, vérifier qu’une touche et le clic suivant ferment le post-it, que le défilement n’apparaît qu’en cas de dépassement et qu’aucune icône n’est présente sur un bouton ou un titre.
  - [ ] Dans la même exécution, vérifier au minimum le français, l’anglais et une langue de droite à gauche, puis vérifier que le changement de langue actualise le libellé, l’infobulle et le post-it.
  - [ ] Fermer l’instance de GW GUI utilisée pour cette vérification.

## Checklist détaillée — Point 6 : associations et visualisation des manettes et joysticks

Cette checklist adapte le ControllerVisualizer déjà utilisé dans l’onglet général Manettes. Elle ne crée aucun second visualiseur. Les identifiants des périphériques émulés et de leurs commandes restent ceux fournis par AmigaInputSettingsFunctions et AtariInputSettingsFunctions.

- [ ] Inscrire les décisions et l’inventaire nécessaires avant de créer des images ou des zones
  - [ ] Modifier docs/tasks/interface/emulation-improvements.md, dans la section 6, pour ajouter un tableau de toutes les valeurs EmulationControllerChoice réellement produites par src/GWGUI.Emulation.Amiga/Functions/AmigaInputSettingsFunctions.cs et src/GWGUI.Emulation.Atari/Functions/AtariInputSettingsFunctions.cs, avec les machines concernées et leurs InputBindingDefinition.
  - [ ] Modifier le tableau de la section 6 dans docs/tasks/interface/emulation-improvements.md après validation pour identifier les périphériques basiques à réaliser maintenant et laisser les autres comme ajouts ultérieurs, sans inventer de périphérique absent des deux listes.
  - [ ] Modifier ce tableau pour inscrire, pour chaque périphérique basique validé, le nom exact de l’image à placer dans src/GWGUI.App/Assets/Controllers, sa source ou son mode de création, son droit de redistribution et les zones associées aux identifiants de commandes existants.
  - [ ] Modifier la section 6 dans docs/tasks/interface/emulation-improvements.md pour inscrire le seuil analogique par défaut validé pour le visualiseur général et décider explicitement si le visualiseur d’un port réutilise DeadZonePercent de ce port.
  - [ ] Modifier la section 6 dans docs/tasks/interface/emulation-improvements.md pour inscrire les dimensions minimale et maximale validées du bloc visuel à droite du tableau.

- [ ] Séparer l’état visuel des données GameInput sans changer le visualiseur général
  - [ ] Créer le fichier vide src/GWGUI.App/Contracts/Input/ControllerVisualState.cs.
  - [ ] Modifier src/GWGUI.App/Contracts/Input/ControllerVisualState.cs pour transporter simultanément les valeurs numériques et les états actifs nécessaires aux zones, sans contenir de contrôle WPF.
  - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualInput.cs pour convertir GameInputLiveState vers ControllerVisualState et lire ensuite uniquement cet état commun.
  - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualizer.cs pour conserver les propriétés publiques Model et State de l’onglet général, convertir State par ControllerVisualInput et permettre à l’éditeur d’émulation de fournir directement un ControllerVisualState.
  - [ ] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par la séparation de l’état visuel.
  - [ ] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build et vérifier que l’onglet général Manettes conserve ses modèles et ses appuis.
  - [ ] Fermer l’instance de GW GUI utilisée pour cette vérification.

- [ ] Décrire les images et zones en pourcentage dans le visualiseur existant
  - [ ] Créer le fichier vide src/GWGUI.App/Enums/Input/ControllerVisualZoneShape.cs.
  - [ ] Modifier src/GWGUI.App/Enums/Input/ControllerVisualZoneShape.cs pour déclarer uniquement les formes effectivement validées dans le tableau de la section 6.
  - [ ] Créer le fichier vide src/GWGUI.App/Contracts/Input/ControllerVisualZone.cs.
  - [ ] Modifier src/GWGUI.App/Contracts/Input/ControllerVisualZone.cs pour porter l’identifiant de commande, la forme et les coordonnées en pourcentage propres à l’image.
  - [ ] Créer le fichier vide src/GWGUI.App/Contracts/Input/ControllerArtworkProfile.cs.
  - [ ] Modifier src/GWGUI.App/Contracts/Input/ControllerArtworkProfile.cs pour porter l’image et la liste de ControllerVisualZone sans dupliquer le rendu.
  - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkCatalog.cs pour résoudre un profil de périphérique émulé depuis l’identifiant du module, de la machine et du EmulationControllerChoice, tout en conservant le catalogue des ControllerVisualModel actuels.
  - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualizer.cs pour afficher un ControllerArtworkProfile avec le même calcul de redimensionnement que les images existantes et exposer le survol et le clic de ses zones.
  - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualizer.Artwork.cs pour dessiner les halos des profils avec les fonctions communes déjà utilisées par les modèles généraux et aligner les zones depuis leurs pourcentages.
  - [ ] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par les profils et zones.

- [ ] Ajouter une image réaliste validée pour chaque périphérique basique
  - [ ] Ajouter dans cette checklist, avant toute création, une sous-tâche Créer distincte donnant le chemin exact de chaque image validée dans le tableau de la section 6.
  - [ ] Réaliser ensuite chaque sous-tâche ajoutée dans l’ordre pour créer uniquement l’image correspondante, vue de face et avec fond transparent, puis vérifier sa correspondance avec le périphérique avant de cocher sa création.
  - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkCatalog.cs après la création de chaque image pour ajouter uniquement son profil validé et ses zones, puis vérifier l’alignement de chaque zone à plusieurs tailles.
  - [ ] Vérifier que src/GWGUI.App/GWGUI.App.csproj continue d’embarquer toutes les images ajoutées par son motif Assets\Controllers\*.png sans ajouter une seconde règle de ressources.

- [ ] Retirer le choix global du périphérique physique sans perdre les configurations existantes
  - [ ] Modifier src/GWGUI.Emulation/Functions/EmulationInputMappingFunctions.cs pour exposer la valeur d’une source de manette identifiée et faire conserver à IsControllerSourcePressed ses résultats actuels en utilisant cette valeur.
  - [ ] Modifier src/GWGUI.Emulation.Atari/Functions/AtariInputSnapshotFunctions.cs pour résoudre, pour chaque association, l’identifiant de périphérique inclus dans sa source et conserver DeviceId enregistré comme repli pour les anciennes associations.
  - [ ] Modifier src/GWGUI.Emulation.Atari/Functions/AtariInputSnapshotFunctions.cs pour accepter les sources clavier et souris déjà représentées dans EmulationInputSnapshot, comme le chemin Amiga, sans modifier les commandes cibles.
  - [ ] Modifier src/GWGUI.Emulation.Amiga/Functions/AmigaInputSnapshotFunctions.cs uniquement pour faire passer ses sources de manette par la valeur commune ajoutée, en conservant la résolution par association, les sources clavier et souris et le repli DeviceId existants.
  - [ ] Modifier src/GWGUI.Emulation.Amiga/Functions/AmigaInputSettingsFunctions.cs pour autoriser la souris parmi les sources capturables des ports Amiga, sans modifier les types de périphériques émulés.
  - [ ] Modifier src/GWGUI.Emulation.Atari/Functions/AtariInputSettingsFunctions.cs pour autoriser la souris parmi les sources capturables des ports Atari, sans modifier les types de périphériques émulés.
  - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/EmulationControllerPortEditor.cs pour supprimer le ComboBox Device et son choix automatique après capture, tout en conservant la valeur PhysicalDeviceId déjà enregistrée comme donnée de compatibilité non modifiable.
  - [ ] Modifier src/GWGUI.App/Contracts/Emulation/Controllers/EmulationControllerPortSettings.cs pour retirer le contrôle Device et conserver uniquement les éléments encore affichés.
  - [ ] Modifier src/GWGUI.App/Controllers/Emulation/Input/EmulationInputSettingsController.cs pour ne plus remplir ni enregistrer un sélecteur physique, préserver PhysicalDeviceId d’une configuration existante et laisser chaque nouvelle association conserver sa propre source.
  - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationControllerSettingsSection.cs pour supprimer la détection et la sélection globales devenues inutilisées.
  - [ ] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationControllerSettingsLayout.cs pour retirer le champ Périphérique du port et conserver le choix du type de périphérique émulé.
  - [ ] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs provoquées par ce retrait et l’élargissement des sources.

- [ ] Placer le visualiseur à droite du tableau du port actif
  - [ ] Modifier src/GWGUI.App/Constants/Emulation/EmulationControllerSettingsConstants.cs pour ajouter uniquement les dimensions validées du bloc visuel et la largeur nécessaire à l’icône de la colonne État.
  - [ ] Modifier src/GWGUI.App/Contracts/Emulation/Controllers/EmulationControllerPortSettings.cs pour transporter le ControllerVisualizer du port avec son type et son InputBindingEditor.
  - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/EmulationControllerPortEditor.cs pour créer un seul ControllerVisualizer par port et lui affecter le profil correspondant au type émulé sélectionné.
  - [ ] Modifier UpdateControllerBindings dans src/GWGUI.App/Controllers/Emulation/Input/EmulationInputSettingsController.cs pour changer ensemble les lignes et le profil lorsqu’un type de périphérique émulé est choisi.
  - [ ] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationControllerSettingsLayout.cs pour placer le tableau à gauche et le visualiseur du même port à droite, conserver ce visualiseur hors du défilement vertical du tableau et ne réduire que l’image lorsque la largeur disponible diminue.
  - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/InputBindingEditor.xaml pour réduire la colonne État à son icône, retirer uniquement StateText de la ligne et conserver les boutons Assigner et Supprimer.
  - [ ] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par cette disposition.

- [ ] Relier les associations et la représentation sans créer un second chemin de capture
  - [ ] Créer le fichier vide src/GWGUI.App/Controllers/Emulation/Input/EmulationBindingVisualizationController.cs.
  - [ ] Modifier src/GWGUI.App/Controllers/Emulation/Input/EmulationBindingVisualizationController.cs pour lire les associations courantes de InputBindingEditor, les états clavier, souris et GameInput disponibles et produire un ControllerVisualState contenant tous les appuis simultanés.
  - [ ] Modifier src/GWGUI.App/Controllers/Emulation/Input/EmulationBindingVisualizationController.cs pour appliquer le seuil ou DeadZonePercent validé avant de transmettre une valeur analogique et revenir à l’état neutre sous ce seuil.
  - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/InputBindingEditor.xaml.cs pour exposer une opération commune qui sélectionne une ligne par son identifiant et démarre sa capture.
  - [ ] Modifier AssignClicked dans src/GWGUI.App/Views/Controls/Emulation/Input/InputBindingEditorCaptureFunctions.cs pour appeler cette opération commune sans changer les sources ni le délai de capture.
  - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/EmulationControllerPortEditor.cs pour raccorder le clic d’une zone du ControllerVisualizer à la même opération commune et ne créer ni double-clic ni bouton supplémentaire.
  - [ ] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/EmulationControllerPortEditor.cs pour démarrer et arrêter EmulationBindingVisualizationController avec le chargement et le déchargement du port, sans laisser de temporisateur ou de gestionnaire attaché.
  - [ ] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par la visualisation en direct et le clic des zones.

- [ ] Refaire la surimpression analogique dans le système commun
  - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualizer.Artwork.cs pour remplacer le trait terminé par un point des sticks par un rond partant du centre et se déplaçant selon la direction et l’inclinaison.
  - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualizer.Artwork.cs pour faire partir le halo des joysticks à manche du centre et l’allonger selon leur direction et leur valeur.
  - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualizer.Artwork.cs pour faire partir le halo des gâchettes du centre et l’allonger vers le bas selon leur pression.
  - [ ] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par les trois rendus analogiques.
  - [ ] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build et vérifier ces trois rendus avec plusieurs périphériques physiques ; ne cocher cette tâche que lorsque la forme est validée.
  - [ ] Modifier docs/tasks/interface/emulation-improvements.md, dans la section 6, pour inscrire la forme précise validée pendant cette vérification.
  - [ ] Fermer l’instance de GW GUI utilisée pour cette vérification.

- [ ] Vérifier tout le point dans l’application
  - [ ] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build et vérifier successivement chaque périphérique basique validé dans chaque port Amiga et Atari où il est proposé.
  - [ ] Dans la même exécution, vérifier que le changement d’onglet de port affiche un seul tableau avec son seul visuel, que le visuel reste fixe pendant le défilement et que le tableau ne rétrécit pas lorsque la fenêtre se resserre.
  - [ ] Dans la même exécution, vérifier simultanément des associations provenant de plusieurs manettes, du clavier, de la souris et d’un périphérique déconnecté, sans sélection préalable d’un périphérique physique.
  - [ ] Dans la même exécution, vérifier qu’un clic sur chaque zone sélectionne la bonne ligne et démarre la même capture que Assigner, puis vérifier que la modification d’association ne laisse aucun halo permanent.
  - [ ] Dans la même exécution, vérifier une configuration ancienne contenant PhysicalDeviceId afin de confirmer que son repli continue à fonctionner alors que le champ n’est plus affiché.
  - [ ] Dans la même exécution, revenir à l’onglet général Manettes et vérifier que le visualiseur existant utilise toujours ses modèles et bénéficie du nouveau rendu analogique commun.
  - [ ] Fermer l’instance de GW GUI utilisée pour cette vérification.

## Checklist détaillée — Point 7 : recherche et architecture des filtres vidéo

Ce point produit la recherche et les décisions d’architecture demandées. Il ne crée aucun filtre, shader, réglage de configuration ou contrôle d’interface avant validation du catalogue et de l’architecture.

- [ ] Créer le document de recherche avant d’y inscrire des résultats
  - [ ] Créer le fichier vide docs/reference/emulation-video-filters.md.
  - [ ] Modifier docs/reference/emulation-video-filters.md pour décrire le périmètre, distinguer les filtres réalisés par GW GUI des options de signal fournies par les émulateurs et reprendre les questions encore ouvertes de la section Filtres vidéo.

- [ ] Établir le catalogue depuis les sources de référence
  - [ ] Modifier docs/reference/emulation-video-filters.md à partir de la documentation officielle Libretro et des catalogues officiels slang-shaders et common-shaders pour recenser les familles de filtres, notamment CRT, scanlines, LCD et moiré horizontal ou vertical, avec un lien vers chaque source consultée.
  - [ ] Modifier docs/reference/emulation-video-filters.md pour inscrire, pour chaque filtre ou famille, son effet, ses réglages utiles, ses dépendances éventuelles, ses combinaisons connues et son statut de licence ; ne recopier aucun code de shader dont la licence n’autorise pas clairement l’usage retenu.
  - [ ] Modifier docs/reference/emulation-video-filters.md après examen des moteurs Amiga et Atari utilisés par le projet pour séparer leurs options RGB, composite, S-Video, RF, PAL, NTSC ou équivalentes des effets propres à GW GUI.
  - [ ] Modifier docs/reference/emulation-video-filters.md pour inclure les filtres utiles aux futures machines sans limiter le catalogue aux capacités Amiga et Atari actuelles.

- [ ] Comparer le catalogue aux quatre surfaces de rendu actuelles
  - [ ] Modifier docs/reference/emulation-video-filters.md après examen de src/GWGUI.App/Interfaces/Rendering/Emulation/IEmulationVideoSurface.cs, src/GWGUI.App/Presenters/Emulation/Machine/MachineVideoPresenter.cs, src/GWGUI.App/Rendering/Emulation/Surfaces/WpfVideoSurface.cs, src/GWGUI.App/Rendering/Emulation/Surfaces/OpenGlVideoSurface.cs et src/GWGUI.App/Rendering/Emulation/Surfaces/VeldridVideoSurface.cs pour décrire où les pixels sont disponibles et où un traitement commun peut être appliqué.
  - [ ] Modifier docs/reference/emulation-video-filters.md pour comparer WPF, OpenGL, Direct3D 11 et Vulkan pour chaque famille de filtre, indiquer ce qui peut partager une définition et ce qui exige une implémentation de backend, sans choisir silencieusement de supprimer un renderer.
  - [ ] Modifier docs/reference/emulation-video-filters.md pour décrire l’effet du traitement envisagé sur Snapshot, le rapport d’aspect, le redimensionnement, le repli actuel vers WPF et l’application immédiate à une instance ouverte.

- [ ] Faire valider les groupes, compatibilités et réglages avant l’architecture définitive
  - [ ] Modifier docs/reference/emulation-video-filters.md pour proposer, à partir du catalogue établi, les groupes logiques, les combinaisons compatibles et les incompatibilités nécessitant la confirmation décrite dans la demande.
  - [ ] Modifier docs/reference/emulation-video-filters.md pour proposer les présélections et les réglages propres à chaque fonctionnalité, sans dupliquer luminosité, contraste, gamma, saturation et netteté.
  - [ ] Modifier docs/tasks/interface/emulation-improvements.md après validation pour inscrire la plage et la valeur neutre du gamma ainsi que la liste validée des groupes, compatibilités, présélections et réglages.

- [ ] Inscrire l’architecture validée sans commencer son implémentation
  - [ ] Modifier docs/architecture/emulation.md pour décrire la séparation validée entre configuration commune, catalogue de filtres, chaîne de traitement et implémentations propres aux backends.
  - [ ] Modifier docs/architecture/emulation.md pour décrire l’enregistrement par configuration de machine, l’application immédiate à la seule instance correspondante et l’utilisation au prochain démarrage lorsqu’aucune instance n’est ouverte.
  - [ ] Modifier docs/architecture/emulation.md pour décrire l’emplacement unique des contrôles dans l’onglet Vidéo, la séparation visuelle avec les options internes de l’émulateur et le maintien permanent des cinq réglages généraux.
  - [ ] Modifier docs/tasks/interface/emulation-improvements.md pour ajouter, seulement après validation de cette architecture, une future checklist d’implémentation donnant les fichiers et actions réellement retenus ; ne créer ni contrat, ni shader, ni contrôle avant cette validation.

## Checklist détaillée — Point 8 : habillages d’écran en plein écran

La section Idée future : habillages d’écran indique explicitement que cette fonction ne doit pas être réalisée maintenant. Aucune tâche de code, d’image, de configuration, de test ou de traduction n’est donc autorisée dans l’état actuel du document.

- [ ] Autoriser explicitement le démarrage de cette idée future avant toute autre action
  - [ ] Modifier docs/tasks/interface/emulation-improvements.md, dans la section Idée future : habillages d’écran, uniquement après une décision explicite de réalisation, pour inscrire que le point 8 peut commencer et conserver la date de cette décision.

- [ ] Compléter les décisions encore ouvertes avant d’écrire une checklist d’implémentation
  - [ ] Modifier docs/tasks/interface/emulation-improvements.md après l’autorisation pour inscrire les décisions validées concernant le mode fenêtré, les variantes initiales, les images à produire ou rechercher, leur redistribution, le recadrage autorisé et le comportement lorsqu’un habillage manque.
  - [ ] Modifier docs/tasks/interface/emulation-improvements.md après ces décisions pour remplacer le présent bloc par une checklist d’implémentation fondée sur les fichiers alors réellement présents, sans anticiper maintenant une architecture, des actifs ou des comportements non validés.
