# Contrôleurs, associations et représentations

## Deux niveaux distincts

L’onglet général **Manettes** affiche les périphériques physiques détectés par GameInput. Les
paramètres d’une machine décrivent les ports et périphériques émulés. Les deux écrans réutilisent le
même système de profils d’image et de surimpression.

## Ports et associations

Chaque module annonce les ports de la machine, les types de périphériques compatibles et les
commandes disponibles. GW GUI construit un tableau d’associations pour le port sélectionné.
Une association peut provenir d’une manette, d’un joystick, du clavier, de la souris ou d’un autre
périphérique pris en charge, sans imposer une sélection physique globale préalable.

Les associations enregistrent leur source et l’identifiant stable GameInput. La lecture des anciens
identifiants `xinput:*` et de l’ancien champ `PhysicalDeviceId` est conservée comme compatibilité de
lecture ; elle ne réintroduit pas de lecteur XInput parallèle.

## Représentation du périphérique émulé

Le type émulé peut proposer plusieurs représentations matérielles compatibles. Le choix du visuel
est enregistré avec le port. Un profil décrit l’image et ses zones par rôles neutres ; le module
associe ces rôles à ses propres identifiants de commandes. Une zone n’est active que si la commande
existe réellement pour le type sélectionné.

Les profils actuellement raccordés couvrent notamment les joysticks QuickShot et apparentés, les
manettes CD32, Atari CX40, Atari 5200, Atari 7800 et Jaguar. Les modèles dont l’image exacte ou les
zones ne sont pas validées restent absents du sélecteur.

Les appuis reçus sont affichés simultanément sur l’image. Cliquer une zone sélectionne la ligne
correspondante et lance la même capture que le bouton **Assigner**. Les sticks, manches et gâchettes
analogiques utilisent une surimpression progressive commune.

## Travail différé

Les corrections visuelles et validations matérielles encore ouvertes sont suivies dans
[`../tasks/interface/emulation/controllers.md`](../tasks/interface/emulation/controllers.md). Les
images facultatives de périphériques supplémentaires possèdent un backlog séparé.
