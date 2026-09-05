# Zones des profils de manettes

[Sommaire](../emulation-improvements.md) · [Règles communes](rules.md)

#### Zones du profil `quickshot`

Les coordonnées sont exprimées en pourcentage de l’image `quickshot.png`, mesurée sur 924 × 898 pixels. Les quatre zones directionnelles partagent l’emprise de la tête du joystick ; le rôle indique le secteur actif et permet au rendu commun de combiner les directions simultanées. La zone du bouton rouge est prioritaire au survol et au clic lorsqu’elle recouvre cette emprise.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 22,6 % | 0,0 % | 54,0 % | 52,6 % |
| `DirectionDown` | `JoystickDirection` | 22,6 % | 0,0 % | 54,0 % | 52,6 % |
| `DirectionLeft` | `JoystickDirection` | 22,6 % | 0,0 % | 54,0 % | 52,6 % |
| `DirectionRight` | `JoystickDirection` | 22,6 % | 0,0 % | 54,0 % | 52,6 % |
| `PrimaryAction` | `RoundedRectangle` | 43,8 % | 4,3 % | 13,4 % | 28,0 % |


#### Zones du profil `quickshot-deluxe`

Les coordonnées sont exprimées en pourcentage de l’image `quickshot-deluxe.png`, mesurée sur 820 × 832 pixels. Les quatre directions partagent l’emprise de la tête du joystick. Le bouton rouge central correspond à l’action principale ; les deux boutons bleus correspondent à l’action secondaire et au turbo.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 6,0 % | 0,0 % | 88,4 % | 49,2 % |
| `DirectionDown` | `JoystickDirection` | 6,0 % | 0,0 % | 88,4 % | 49,2 % |
| `DirectionLeft` | `JoystickDirection` | 6,0 % | 0,0 % | 88,4 % | 49,2 % |
| `DirectionRight` | `JoystickDirection` | 6,0 % | 0,0 % | 88,4 % | 49,2 % |
| `PrimaryAction` | `RoundedRectangle` | 37,4 % | 6,7 % | 24,4 % | 14,5 % |
| `SecondaryAction` | `RoundedRectangle` | 15,6 % | 7,5 % | 14,4 % | 11,7 % |
| `Turbo` | `RoundedRectangle` | 69,3 % | 7,6 % | 14,6 % | 11,5 % |


#### Zones du profil `quickshot-ii-turbo`

Les coordonnées sont exprimées en pourcentage de l’image `quickshot-ii-turbo.png`, mesurée sur 810 × 877 pixels. Le profil comporte la tête directionnelle centrale et son bouton rouge visible. Aucune zone `Turbo` séparée n’est ajoutée, car aucune commande de turbo distincte n’est visible sur cette image.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 33,3 % | 8,9 % | 33,4 % | 61,8 % |
| `DirectionDown` | `JoystickDirection` | 33,3 % | 8,9 % | 33,4 % | 61,8 % |
| `DirectionLeft` | `JoystickDirection` | 33,3 % | 8,9 % | 33,4 % | 61,8 % |
| `DirectionRight` | `JoystickDirection` | 33,3 % | 8,9 % | 33,4 % | 61,8 % |
| `PrimaryAction` | `RoundedRectangle` | 39,9 % | 15,6 % | 20,0 % | 41,4 % |


#### Zones du profil `competition-pro-5000`

Les coordonnées sont exprimées en pourcentage de l’image `competition-pro-5000.png`, mesurée sur 726 × 1045 pixels. La boule centrale porte les quatre directions. Le bouton rouge gauche correspond à l’action principale et le bouton rouge droit à l’action secondaire.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 22,5 % | 38,0 % | 54,7 % | 39,1 % |
| `DirectionDown` | `JoystickDirection` | 22,5 % | 38,0 % | 54,7 % | 39,1 % |
| `DirectionLeft` | `JoystickDirection` | 22,5 % | 38,0 % | 54,7 % | 39,1 % |
| `DirectionRight` | `JoystickDirection` | 22,5 % | 38,0 % | 54,7 % | 39,1 % |
| `PrimaryAction` | `Ellipse` | 5,8 % | 3,3 % | 30,9 % | 23,1 % |
| `SecondaryAction` | `Ellipse` | 63,9 % | 3,3 % | 31,0 % | 23,3 % |


#### Zones du profil `zipstik-super-pro`

Les coordonnées sont exprimées en pourcentage de l’image `zipstik-super-pro.png`, mesurée sur 700 × 947 pixels. La commande centrale porte les quatre directions. Le bouton jaune gauche correspond à l’action principale et le bouton jaune droit à l’action secondaire.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 22,1 % | 39,7 % | 56,1 % | 41,9 % |
| `DirectionDown` | `JoystickDirection` | 22,1 % | 39,7 % | 56,1 % | 41,9 % |
| `DirectionLeft` | `JoystickDirection` | 22,1 % | 39,7 % | 56,1 % | 41,9 % |
| `DirectionRight` | `JoystickDirection` | 22,1 % | 39,7 % | 56,1 % | 41,9 % |
| `PrimaryAction` | `RoundedRectangle` | 7,4 % | 5,1 % | 21,9 % | 16,8 % |
| `SecondaryAction` | `RoundedRectangle` | 71,0 % | 5,1 % | 21,7 % | 16,8 % |


#### Zones du profil `konix-speedking-left-hand`

Les coordonnées sont exprimées en pourcentage de l’image `konix-speedking-left-hand.png`, mesurée sur 554 × 1041 pixels. Seule la commande directionnelle visible du dessus possède des zones. Les gâchettes latérales non visibles ne reçoivent pas de fausse zone.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 30,7 % | 12,0 % | 41,0 % | 23,8 % |
| `DirectionDown` | `JoystickDirection` | 30,7 % | 12,0 % | 41,0 % | 23,8 % |
| `DirectionLeft` | `JoystickDirection` | 30,7 % | 12,0 % | 41,0 % | 23,8 % |
| `DirectionRight` | `JoystickDirection` | 30,7 % | 12,0 % | 41,0 % | 23,8 % |

#### Zones du profil `konix-speedking-right-hand`

Les coordonnées sont exprimées en pourcentage de l’image `konix-speedking-right-hand.png`, mesurée sur 584 × 1041 pixels. Seule la commande directionnelle visible du dessus possède des zones. Les gâchettes latérales non visibles ne reçoivent pas de fausse zone.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 27,1 % | 12,0 % | 44,0 % | 23,8 % |
| `DirectionDown` | `JoystickDirection` | 27,1 % | 12,0 % | 44,0 % | 23,8 % |
| `DirectionLeft` | `JoystickDirection` | 27,1 % | 12,0 % | 44,0 % | 23,8 % |
| `DirectionRight` | `JoystickDirection` | 27,1 % | 12,0 % | 44,0 % | 23,8 % |


#### Zones du profil `konix-speedking-analog`

Les coordonnées sont exprimées en pourcentage de l’image `konix-speedking-analog.png`, mesurée sur 1290 × 1219 pixels. La boule centrale porte les directions analogiques. Les boutons `A` et `B` correspondent aux actions principale et secondaire. Le réglage `ADJ CENTRE` et l’interrupteur `CENTRE RETURN` n’ont pas de zone, car ils ne correspondent à aucune `InputBindingDefinition` du choix.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 25,0 % | 17,1 % | 50,0 % | 53,4 % |
| `DirectionDown` | `JoystickDirection` | 25,0 % | 17,1 % | 50,0 % | 53,4 % |
| `DirectionLeft` | `JoystickDirection` | 25,0 % | 17,1 % | 50,0 % | 53,4 % |
| `DirectionRight` | `JoystickDirection` | 25,0 % | 17,1 % | 50,0 % | 53,4 % |
| `PrimaryAction` | `Ellipse` | 12,9 % | 74,0 % | 14,3 % | 15,0 % |
| `SecondaryAction` | `Ellipse` | 73,4 % | 74,0 % | 14,0 % | 15,0 % |


#### Zones du profil `suncom-tac-2`

Les coordonnées sont exprimées en pourcentage de l’image `suncom-tac-2.png`, mesurée sur 1290 × 1219 pixels. La boule centrale porte les quatre directions. Le bouton rouge gauche correspond à l’action principale et le bouton rouge droit à l’action secondaire.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 37,2 % | 32,2 % | 25,0 % | 26,0 % |
| `DirectionDown` | `JoystickDirection` | 37,2 % | 32,2 % | 25,0 % | 26,0 % |
| `DirectionLeft` | `JoystickDirection` | 37,2 % | 32,2 % | 25,0 % | 26,0 % |
| `DirectionRight` | `JoystickDirection` | 37,2 % | 32,2 % | 25,0 % | 26,0 % |
| `PrimaryAction` | `Ellipse` | 13,6 % | 67,4 % | 16,4 % | 18,3 % |
| `SecondaryAction` | `Ellipse` | 69,4 % | 67,4 % | 16,8 % | 18,3 % |


#### Zones du profil `powerplay-cruiser`

Les coordonnées sont exprimées en pourcentage de l’image `powerplay-cruiser.png`, mesurée sur 1199 × 1312 pixels. La commande centrale porte les quatre directions. Le bouton jaune gauche correspond à l’action principale et le bouton jaune droit à l’action secondaire.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 29,8 % | 10,6 % | 41,4 % | 39,3 % |
| `DirectionDown` | `JoystickDirection` | 29,8 % | 10,6 % | 41,4 % | 39,3 % |
| `DirectionLeft` | `JoystickDirection` | 29,8 % | 10,6 % | 41,4 % | 39,3 % |
| `DirectionRight` | `JoystickDirection` | 29,8 % | 10,6 % | 41,4 % | 39,3 % |
| `PrimaryAction` | `Ellipse` | 14,8 % | 64,9 % | 19,2 % | 17,8 % |
| `SecondaryAction` | `Ellipse` | 67,8 % | 65,0 % | 18,9 % | 17,8 % |


#### Zones du profil `suzo-the-arcade-turbo`

Les coordonnées sont exprimées en pourcentage de l’image `suzo-the-arcade-turbo.png`, mesurée sur 1254 × 1254 pixels. La commande noire centrale porte les quatre directions, son bouton rouge correspond à l’action principale et la commande rouge séparée en bas correspond au turbo.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 34,4 % | 20,4 % | 30,8 % | 32,5 % |
| `DirectionDown` | `JoystickDirection` | 34,4 % | 20,4 % | 30,8 % | 32,5 % |
| `DirectionLeft` | `JoystickDirection` | 34,4 % | 20,4 % | 30,8 % | 32,5 % |
| `DirectionRight` | `JoystickDirection` | 34,4 % | 20,4 % | 30,8 % | 32,5 % |
| `PrimaryAction` | `Ellipse` | 43,1 % | 30,0 % | 13,2 % | 13,4 % |
| `Turbo` | `RoundedRectangle` | 39,9 % | 81,2 % | 20,0 % | 9,3 % |


#### Zones du profil `commodore-cd32`

Les coordonnées sont exprimées en pourcentage de l’image `commodore-cd32.png`, mesurée sur 1534 × 603 pixels. Le disque gauche porte les quatre directions. Les actions principale à quaternaire suivent les boutons rouge, bleu, vert et jaune. Les commandes supérieures correspondent au rembobinage et à l’avance rapide ; le bouton noir central correspond à lecture-pause. Aucune zone `Turbo` distincte n’est ajoutée.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `DirectionalPad` | 5,9 % | 24,0 % | 13,7 % | 34,0 % |
| `DirectionDown` | `DirectionalPad` | 5,9 % | 24,0 % | 13,7 % | 34,0 % |
| `DirectionLeft` | `DirectionalPad` | 5,9 % | 24,0 % | 13,7 % | 34,0 % |
| `DirectionRight` | `DirectionalPad` | 5,9 % | 24,0 % | 13,7 % | 34,0 % |
| `PrimaryAction` | `Ellipse` | 80,2 % | 47,8 % | 6,6 % | 16,6 % |
| `SecondaryAction` | `Ellipse` | 88,8 % | 44,6 % | 6,7 % | 16,7 % |
| `TertiaryAction` | `Ellipse` | 78,7 % | 26,5 % | 6,5 % | 16,4 % |
| `QuaternaryAction` | `Ellipse` | 87,2 % | 23,4 % | 6,6 % | 16,4 % |
| `LeftShoulder` | `RoundedRectangle` | 10,9 % | 0,0 % | 13,0 % | 2,7 % |
| `RightShoulder` | `RoundedRectangle` | 76,1 % | 0,0 % | 11,0 % | 2,7 % |
| `Start` | `RoundedRectangle` | 59,0 % | 67,5 % | 9,8 % | 7,1 % |


#### Zones du profil `competition-pro-cd32`

Les coordonnées sont exprimées en pourcentage de l’image `competition-pro-cd32.png`, mesurée sur 1568 × 807 pixels. Le disque gauche porte les directions. Les quatre boutons gris portant les symboles rouge, bleu, vert et jaune correspondent aux actions principale à quaternaire. Les palettes supérieures correspondent aux épaules gauche et droite. Les deux boutons argentés de lecture-pause portent tous deux le rôle `Start`. Le sélecteur supérieur situé sous `OFF / TURBO / AUTO` porte le rôle `Turbo`. Les autres curseurs de réglage n’ont pas de zone.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `DirectionalPad` | 7,3 % | 25,8 % | 25,1 % | 52,4 % |
| `DirectionDown` | `DirectionalPad` | 7,3 % | 25,8 % | 25,1 % | 52,4 % |
| `DirectionLeft` | `DirectionalPad` | 7,3 % | 25,8 % | 25,1 % | 52,4 % |
| `DirectionRight` | `DirectionalPad` | 7,3 % | 25,8 % | 25,1 % | 52,4 % |
| `PrimaryAction` | `Ellipse` | 74,2 % | 63,4 % | 7,7 % | 15,0 % |
| `SecondaryAction` | `Ellipse` | 84,5 % | 53,7 % | 7,7 % | 14,7 % |
| `TertiaryAction` | `Ellipse` | 69,5 % | 43,0 % | 7,4 % | 15,2 % |
| `QuaternaryAction` | `Ellipse` | 80,0 % | 33,6 % | 7,7 % | 14,5 % |
| `LeftShoulder` | `RoundedRectangle` | 6,5 % | 4,5 % | 19,0 % | 22,5 % |
| `RightShoulder` | `RoundedRectangle` | 74,5 % | 4,5 % | 19,0 % | 22,5 % |
| `Start` | `RoundedRectangle` | 39,2 % | 63,1 % | 6,1 % | 8,1 % |
| `Start` | `RoundedRectangle` | 48,5 % | 63,1 % | 6,1 % | 8,1 % |
| `Turbo` | `RoundedRectangle` | 55,2 % | 23,0 % | 6,6 % | 5,3 % |


#### Zones du profil `atari-cx40`

Les coordonnées sont exprimées en pourcentage de l’image `atari-cx40.png`, mesurée sur 1254 × 1254 pixels. La commande centrale porte les quatre directions et l’unique bouton rouge correspond à l’action principale.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 26,6 % | 28,1 % | 46,3 % | 44,6 % |
| `DirectionDown` | `JoystickDirection` | 26,6 % | 28,1 % | 46,3 % | 44,6 % |
| `DirectionLeft` | `JoystickDirection` | 26,6 % | 28,1 % | 46,3 % | 44,6 % |
| `DirectionRight` | `JoystickDirection` | 26,6 % | 28,1 % | 46,3 % | 44,6 % |
| `PrimaryAction` | `Ellipse` | 15,8 % | 14,4 % | 14,5 % | 14,4 % |


#### Zones du profil `atari-5200-controller`

Les coordonnées sont exprimées en pourcentage de l’image `atari-5200-controller.png`, mesurée sur 858 × 1832 pixels. Le joystick central porte les directions analogiques. Les boutons latéraux supérieurs gauche et droit portent tous deux l’action principale ; les boutons latéraux inférieurs portent tous deux l’action secondaire. Les trois boutons système et les douze touches du clavier reprennent exactement les rôles déclarés pour le 5200.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 23,9 % | 17,1 % | 51,9 % | 24,0 % |
| `DirectionDown` | `JoystickDirection` | 23,9 % | 17,1 % | 51,9 % | 24,0 % |
| `DirectionLeft` | `JoystickDirection` | 23,9 % | 17,1 % | 51,9 % | 24,0 % |
| `DirectionRight` | `JoystickDirection` | 23,9 % | 17,1 % | 51,9 % | 24,0 % |
| `PrimaryAction` | `RoundedRectangle` | 13,4 % | 10,6 % | 3,6 % | 7,9 % |
| `PrimaryAction` | `RoundedRectangle` | 82,3 % | 10,6 % | 3,3 % | 7,9 % |
| `SecondaryAction` | `RoundedRectangle` | 13,4 % | 19,2 % | 3,6 % | 7,5 % |
| `SecondaryAction` | `RoundedRectangle` | 82,3 % | 19,2 % | 3,3 % | 7,5 % |
| `Start` | `RoundedRectangle` | 25,1 % | 8,4 % | 13,5 % | 4,6 % |
| `Pause` | `RoundedRectangle` | 43,5 % | 8,4 % | 13,6 % | 4,6 % |
| `Reset` | `RoundedRectangle` | 61,5 % | 8,4 % | 13,6 % | 4,6 % |
| `Key1` | `RoundedRectangle` | 27,9 % | 57,3 % | 13,0 % | 5,4 % |
| `Key2` | `RoundedRectangle` | 43,0 % | 57,3 % | 13,2 % | 5,4 % |
| `Key3` | `RoundedRectangle` | 60,6 % | 57,3 % | 13,2 % | 5,4 % |
| `Key4` | `RoundedRectangle` | 27,9 % | 63,3 % | 13,0 % | 5,4 % |
| `Key5` | `RoundedRectangle` | 43,0 % | 63,3 % | 13,2 % | 5,4 % |
| `Key6` | `RoundedRectangle` | 60,6 % | 63,3 % | 13,2 % | 5,4 % |
| `Key7` | `RoundedRectangle` | 27,9 % | 70,8 % | 13,0 % | 5,4 % |
| `Key8` | `RoundedRectangle` | 43,0 % | 70,8 % | 13,2 % | 5,4 % |
| `Key9` | `RoundedRectangle` | 60,6 % | 70,8 % | 13,2 % | 5,4 % |
| `KeyStar` | `RoundedRectangle` | 27,9 % | 78,3 % | 13,0 % | 5,4 % |
| `Key0` | `RoundedRectangle` | 43,0 % | 78,3 % | 13,2 % | 5,4 % |
| `KeyHash` | `RoundedRectangle` | 60,6 % | 78,3 % | 13,2 % | 5,4 % |


#### Zones du profil `atari-7800-pro-line-cx24`

Les coordonnées sont exprimées en pourcentage de l’image `atari-7800-pro-line-cx24.png`, mesurée sur 1023 × 1537 pixels. La commande centrale porte les quatre directions. Le bouton latéral rouge gauche correspond à l’action principale et le bouton latéral rouge droit à l’action secondaire.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 36,8 % | 27,4 % | 25,7 % | 18,4 % |
| `DirectionDown` | `JoystickDirection` | 36,8 % | 27,4 % | 25,7 % | 18,4 % |
| `DirectionLeft` | `JoystickDirection` | 36,8 % | 27,4 % | 25,7 % | 18,4 % |
| `DirectionRight` | `JoystickDirection` | 36,8 % | 27,4 % | 25,7 % | 18,4 % |
| `PrimaryAction` | `RoundedRectangle` | 26,2 % | 11,9 % | 6,8 % | 16,9 % |
| `SecondaryAction` | `RoundedRectangle` | 66,2 % | 11,7 % | 6,5 % | 17,0 % |


#### Zones du profil `atari-7800-control-pad-europe`

Les coordonnées sont exprimées en pourcentage de l’image `atari-7800-control-pad-europe.png`, mesurée sur 1518 × 1036 pixels. La croix gauche porte les quatre directions. Le bouton rouge `1` correspond à l’action principale et le bouton rouge `2` à l’action secondaire.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `DirectionalPad` | 14,8 % | 17,0 % | 19,4 % | 29,9 % |
| `DirectionDown` | `DirectionalPad` | 14,8 % | 17,0 % | 19,4 % | 29,9 % |
| `DirectionLeft` | `DirectionalPad` | 14,8 % | 17,0 % | 19,4 % | 29,9 % |
| `DirectionRight` | `DirectionalPad` | 14,8 % | 17,0 % | 19,4 % | 29,9 % |
| `PrimaryAction` | `Ellipse` | 46,4 % | 51,4 % | 10,3 % | 16,0 % |
| `SecondaryAction` | `Ellipse` | 64,8 % | 51,4 % | 10,3 % | 16,0 % |


#### Zones du profil `atari-jaguar-controller`

Les coordonnées sont exprimées en pourcentage de l’image `atari-jaguar-controller.png`, mesurée sur 1402 × 1122 pixels. La croix gauche porte les directions. Les boutons `A`, `B`, `C`, `Pause`, `Option` et les douze touches du clavier correspondent directement aux rôles déclarés pour la Jaguar.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `DirectionalPad` | 17,5 % | 16,9 % | 19,3 % | 23,4 % |
| `DirectionDown` | `DirectionalPad` | 17,5 % | 16,9 % | 19,3 % | 23,4 % |
| `DirectionLeft` | `DirectionalPad` | 17,5 % | 16,9 % | 19,3 % | 23,4 % |
| `DirectionRight` | `DirectionalPad` | 17,5 % | 16,9 % | 19,3 % | 23,4 % |
| `PrimaryAction` | `RoundedRectangle` | 71,8 % | 14,6 % | 10,7 % | 10,9 % |
| `SecondaryAction` | `RoundedRectangle` | 65,6 % | 23,5 % | 10,5 % | 10,6 % |
| `TertiaryAction` | `RoundedRectangle` | 59,2 % | 32,7 % | 11,1 % | 10,4 % |
| `Pause` | `RoundedRectangle` | 42,4 % | 32,6 % | 5,1 % | 7,4 % |
| `Option` | `RoundedRectangle` | 49,0 % | 32,6 % | 4,8 % | 7,4 % |
| `Key1` | `RoundedRectangle` | 35,3 % | 55,1 % | 7,8 % | 4,0 % |
| `Key2` | `RoundedRectangle` | 46,2 % | 55,1 % | 7,6 % | 4,0 % |
| `Key3` | `RoundedRectangle` | 56,8 % | 55,1 % | 7,7 % | 4,0 % |
| `Key4` | `RoundedRectangle` | 35,3 % | 63,6 % | 7,8 % | 4,0 % |
| `Key5` | `RoundedRectangle` | 46,2 % | 63,6 % | 7,6 % | 4,0 % |
| `Key6` | `RoundedRectangle` | 56,8 % | 63,6 % | 7,7 % | 4,0 % |
| `Key7` | `RoundedRectangle` | 35,3 % | 72,3 % | 7,8 % | 4,0 % |
| `Key8` | `RoundedRectangle` | 46,2 % | 72,3 % | 7,6 % | 4,0 % |
| `Key9` | `RoundedRectangle` | 56,8 % | 72,3 % | 7,7 % | 4,0 % |
| `KeyStar` | `RoundedRectangle` | 35,3 % | 80,7 % | 7,8 % | 4,1 % |
| `Key0` | `RoundedRectangle` | 46,2 % | 80,7 % | 7,6 % | 4,1 % |
| `KeyHash` | `RoundedRectangle` | 56,8 % | 80,7 % | 7,7 % | 4,1 % |


#### Zones du profil `atari-jaguar-pro-controller`

Les coordonnées sont exprimées en pourcentage de l’image `atari-jaguar-pro-controller.png`, mesurée sur 1337 × 1176 pixels. Seules les commandes produites par la DLL Jaguar actuelle possèdent une zone : directions, `A`, `B`, `C`, `Pause`, `Option` et clavier. Les commandes `X`, `Y`, `Z`, `L` et `R` visibles sur ce modèle Pro restent sans zone.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `DirectionalPad` | 22,0 % | 25,4 % | 14,8 % | 17,3 % |
| `DirectionDown` | `DirectionalPad` | 22,0 % | 25,4 % | 14,8 % | 17,3 % |
| `DirectionLeft` | `DirectionalPad` | 22,0 % | 25,4 % | 14,8 % | 17,3 % |
| `DirectionRight` | `DirectionalPad` | 22,0 % | 25,4 % | 14,8 % | 17,3 % |
| `PrimaryAction` | `Ellipse` | 73,2 % | 26,0 % | 6,7 % | 7,7 % |
| `SecondaryAction` | `Ellipse` | 66,7 % | 31,7 % | 6,4 % | 7,1 % |
| `TertiaryAction` | `Ellipse` | 61,3 % | 38,2 % | 6,2 % | 7,1 % |
| `Pause` | `RoundedRectangle` | 42,7 % | 36,2 % | 4,3 % | 4,9 % |
| `Option` | `RoundedRectangle` | 48,2 % | 36,2 % | 4,5 % | 4,9 % |
| `Key1` | `RoundedRectangle` | 36,6 % | 53,7 % | 5,8 % | 2,5 % |
| `Key2` | `RoundedRectangle` | 46,5 % | 53,7 % | 5,6 % | 2,5 % |
| `Key3` | `RoundedRectangle` | 56,1 % | 53,7 % | 5,8 % | 2,5 % |
| `Key4` | `RoundedRectangle` | 36,6 % | 60,5 % | 5,8 % | 2,5 % |
| `Key5` | `RoundedRectangle` | 46,5 % | 60,5 % | 5,6 % | 2,5 % |
| `Key6` | `RoundedRectangle` | 56,1 % | 60,5 % | 5,8 % | 2,5 % |
| `Key7` | `RoundedRectangle` | 36,6 % | 67,4 % | 5,8 % | 2,6 % |
| `Key8` | `RoundedRectangle` | 46,5 % | 67,4 % | 5,6 % | 2,6 % |
| `Key9` | `RoundedRectangle` | 56,1 % | 67,4 % | 5,8 % | 2,6 % |
| `KeyStar` | `RoundedRectangle` | 36,6 % | 74,4 % | 5,8 % | 2,5 % |
| `Key0` | `RoundedRectangle` | 46,5 % | 74,4 % | 5,6 % | 2,5 % |
| `KeyHash` | `RoundedRectangle` | 56,1 % | 74,4 % | 5,8 % | 2,5 % |

Chaque représentation doit être :

- réaliste ;
- vue du dessus dans son sens normal d’utilisation ;
- correctement réalisée, et non remplacée par un dessin générique de mauvaise qualité ;
- fournie avec un fond transparent ;
- accompagnée de zones de surimpression correctement placées sur ses directions, boutons et autres commandes ;
- accompagnée, au passage de la souris sur une zone cliquable, d’un petit halo ou d’un changement de couleur du halo permettant de voir immédiatement quelle commande peut être assignée.
