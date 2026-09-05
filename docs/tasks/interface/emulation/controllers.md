# Associations et visualisation des manettes

[Sommaire](../emulation-improvements.md) · [Règles communes](rules.md)

Compléments : [zones des profils](controller-profile-zones.md) · [visuels à créer](controller-artwork-backlog.md).

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

### Choix et enregistrement du visuel

Le type de périphérique émulé et son visuel sont deux choix distincts. Le type est la valeur fournie par la DLL d’émulation, par exemple `Joystick`, `Cd32Pad` ou `None`. Pour le port actuellement ouvert, l’utilisateur peut choisir un visuel parmi les modèles matériels déclarés compatibles avec le module, la machine et ce type.

Le changement de visuel ne modifie ni le type de périphérique émulé, ni ses associations. Le visuel choisi est enregistré avec la configuration du port de la machine par le même enregistrement automatique que les autres réglages. Tant qu’aucune configuration n’a encore été enregistrée, le choix reste porté par l’état d’édition courant.

Un même modèle matériel n’existe qu’une fois dans le catalogue et peut être proposé à plusieurs ordinateurs lorsque ce modèle a réellement existé pour eux. Une compatibilité technique seule ne suffit pas pour proposer le visuel d’une manette propre à une autre console ou famille de machines.

Les DLL d’émulation déclarent les VisualId compatibles avec chacun de leurs types et le VisualId utilisé par défaut. Elles peuvent déjà déclarer des VisualId dont l’image n’existe pas encore. L’application croise cette déclaration avec les profils réellement disponibles dans son catalogue et n’affiche dans le sélecteur que ceux dont l’image et les zones existent effectivement.

Lorsqu’une console ou un ordinateur possède un contrôleur de base propre à sa machine, ce modèle est le visuel par défaut. Les modules Amiga et Atari ne possédant pas un unique joystick de base commun à leurs machines, leur type `Joystick` utilise le QuickShot comme visuel par défaut. Les visuels Mega Drive peuvent être renseignés pour un futur module Mega Drive, mais ne sont pas proposés actuellement comme visuels d’une console absente.

Les noms de produits et de modèles, tels que `Competition Pro 5000`, ne sont pas traduits. Ils sont conservés dans les ressources générales `00-Base` et ne sont pas recopiés dans les fichiers propres aux langues.

### Périphériques à représenter

Pour commencer, il faut réaliser les images des périphériques basiques déjà reconnus par les émulateurs. La liste de ces périphériques existe déjà dans l’application et doit être utilisée directement.

Des représentations supplémentaires pourront être ajoutées plus tard.

#### Inventaire réel des périphériques émulés

Cet inventaire reprend uniquement les valeurs de `EmulationControllerChoice` effectivement produites par `AmigaInputSettingsFunctions` et `AtariInputSettingsFunctions`. Dans la colonne des commandes, chaque définition est écrite sous la forme `identifiant / clé de ressource / association par défaut / valeur invariante`. `—` signifie une chaîne vide ou une valeur nulle. Les DLL ne choisissent aucune touche ni aucun bouton physique par défaut : toutes les associations sont vides jusqu’à une affectation explicite de l’utilisateur.

| Module | `EmulationControllerChoice.Id` | Réalisation | Machines et ports concernés | `InputBindingDefinition` produites |
| --- | --- | --- | --- | --- |
| Amiga | `Joystick` | Maintenant — image présente | Tous les modèles Amiga, ports standards ; tous les modèles avec adaptateur parallèle activé, ports parallèles | `Up / Emulation.Controller.Action.Up / — / —` ; `Down / Emulation.Controller.Action.Down / — / —` ; `Left / Emulation.Controller.Action.Left / — / —` ; `Right / Emulation.Controller.Action.Right / — / —` ; `B / Emulation.Controller.Action.Fire1 / — / —` ; `A / Emulation.Controller.Action.Fire2 / — / —` ; `L2 / Emulation.Controller.Action.TurboFire / — / —` |
| Amiga | `AnalogJoystick` | Maintenant — image présente | Tous les modèles Amiga, ports standards | Mêmes définitions que `Joystick` |
| Amiga | `Cd32Pad` | Maintenant — image présente | Amiga CD32, ports standards | `Up / Emulation.Controller.Action.Up / — / —` ; `Down / Emulation.Controller.Action.Down / — / —` ; `Left / Emulation.Controller.Action.Left / — / —` ; `Right / Emulation.Controller.Action.Right / — / —` ; `B / Emulation.Amiga.Controller.Cd32.Red / — / —` ; `A / Emulation.Amiga.Controller.Cd32.Blue / — / —` ; `Y / Emulation.Amiga.Controller.Cd32.Green / — / —` ; `X / Emulation.Amiga.Controller.Cd32.Yellow / — / —` ; `L / Emulation.Amiga.Controller.Cd32.Rewind / — / —` ; `R / Emulation.Amiga.Controller.Cd32.FastForward / — / —` ; `Start / Emulation.Amiga.Controller.Cd32.PlayPause / — / —` ; `L2 / Emulation.Controller.Action.TurboFire / — / —` |
| Amiga | `None` | Sans représentation | Tous les modèles Amiga, ports standards et ports parallèles | Aucune définition |
| Atari | `Joystick` | Maintenant — image présente | Atari ST, STF, STFM, Mega ST, STE, Mega STE, TT et Falcon | `Up / Emulation.Controller.Action.Up / — / —` ; `Down / Emulation.Controller.Action.Down / — / —` ; `Left / Emulation.Controller.Action.Left / — / —` ; `Right / Emulation.Controller.Action.Right / — / —` ; `Fire1 / Emulation.Controller.Action.Fire1 / — / —` ; `Turbo / Emulation.Controller.Action.TurboFire / — / —` |
| Atari | `Joystick` | Maintenant — image présente | Atari 400, 800, 800XL, 130XE, XEGS, XL/XE et 2600 | Définitions `Up`, `Down`, `Left`, `Right` et `Fire1` de la ligne précédente, avec les mêmes associations par défaut |
| Atari | `AnalogJoystick` | Maintenant — image présente | Atari 5200 | `Up`, `Down`, `Left`, `Right`, `Fire1` et `Fire2` avec les clés `Emulation.Controller.Action.{identifiant}` et les associations par défaut `DPadUp`, `DPadDown`, `DPadLeft`, `DPadRight`, `ButtonA` et `ButtonB` ; `Start / Emulation.Controller.Action.Start / — / Start` ; `Pause / Emulation.Controller.Action.Pause / — / Pause` ; `Reset / Emulation.Controller.Action.Reset / — / Reset` ; `Key0` à `Key9`, `Star` et `Hash / Emulation.Controller.Action.{identifiant} / — / {identifiant}` |
| Atari | `Paddle` | Ajout ultérieur — image manquante | Atari 400, 800, 800XL, 130XE, XEGS, XL/XE et 2600 | `Fire1 / Emulation.Controller.Action.Fire1 / — / —` |
| Atari | `DrivingController` | Ajout ultérieur — image manquante | Atari 2600 | `Fire1 / Emulation.Controller.Action.Fire1 / — / —` |
| Atari | `BoosterGrip` | Ajout ultérieur — image manquante | Atari 2600 | `Up`, `Down`, `Left`, `Right`, `Fire1` et `Fire2` avec les clés et associations par défaut déjà détaillées ; `Turbo / Emulation.Controller.Action.TurboFire / — / —` |
| Atari | `GenesisController` | Sans représentation actuelle — le visuel Mega Drive reste réservé à un futur module Mega Drive | Atari 2600 | `Up`, `Down`, `Left`, `Right` et `Fire1` avec les clés et associations par défaut déjà détaillées |
| Atari | `Joy2BPlus` | Ajout ultérieur — image manquante | Atari 2600 | `Up`, `Down`, `Left`, `Right`, `Fire1` et `Fire2` avec les clés et associations par défaut déjà détaillées |
| Atari | `ProLineController` | Maintenant — image présente | Atari 7800 | `Up`, `Down`, `Left`, `Right`, `Fire1` et `Fire2` avec les clés et associations par défaut déjà détaillées |
| Atari | `LightGun` | Ajout ultérieur — image manquante | Atari 7800 | `Fire1 / Emulation.Controller.Action.Fire1 / — / —` |
| Atari | `EnhancedController` | Ajout ultérieur — image manquante | Atari Lynx | `Up`, `Down`, `Left`, `Right`, `Fire1` et `Fire2` avec les clés et associations par défaut déjà détaillées ; `Option1 / Emulation.Controller.Action.Option1 / — / Option 1` ; `Option2 / Emulation.Controller.Action.Option2 / — / Option 2` ; `Pause / Emulation.Controller.Action.Pause / — / Pause` |
| Atari | `EnhancedController` | Maintenant — image présente | Atari Jaguar et Jaguar CD | `Up`, `Down`, `Left` et `Right` avec les clés et associations par défaut déjà détaillées ; `A / Emulation.Controller.Action.A / — / A` ; `B / Emulation.Controller.Action.B / — / B` ; `C / Emulation.Controller.Action.C / — / C` ; `Option / Emulation.Controller.Action.Option / — / Option` ; `Pause / Emulation.Controller.Action.Pause / — / Pause` ; `Key0` à `Key9`, `Star` et `Hash / Emulation.Controller.Action.{identifiant} / — / {identifiant}` |
| Atari | `None` | Sans représentation | Tous les modèles Atari | Aucune définition |

#### Visuels déclarés par les DLL

Les listes suivantes sont déclarées par les DLL d’émulation. L’application n’affiche dans le sélecteur que les VisualId possédant effectivement un profil dans son catalogue.

| Module et choix | VisualId compatibles déclarés | VisualId par défaut |
| --- | --- | --- |
| Amiga `Joystick` | `quickshot`, `quickshot-deluxe`, `quickshot-ii-turbo`, `competition-pro-5000`, `zipstik-super-pro`, `konix-speedking-left-hand`, `konix-speedking-right-hand`, `suncom-tac-2`, `powerplay-cruiser`, `suzo-the-arcade-turbo`, `advanced-gravis-gamepad` | `quickshot` |
| Amiga `AnalogJoystick` | `konix-speedking-analog` | `konix-speedking-analog` |
| Amiga `Cd32Pad` | `commodore-cd32`, `competition-pro-cd32` | `commodore-cd32` |
| Amiga `None` | — | — |
| Atari 2600 `Joystick` | `atari-cx40` | `atari-cx40` |
| Autres ordinateurs Atari `Joystick` | `quickshot`, `quickshot-deluxe`, `quickshot-ii-turbo`, `competition-pro-5000`, `zipstik-super-pro`, `konix-speedking-left-hand`, `konix-speedking-right-hand`, `suncom-tac-2`, `powerplay-cruiser`, `suzo-the-arcade-turbo`, `advanced-gravis-gamepad`, `atari-cx40` | `quickshot` |
| Atari 5200 `AnalogJoystick` | `atari-5200-controller` | `atari-5200-controller` |
| Atari `Paddle` | `atari-paddle` | `atari-paddle` |
| Atari 2600 `DrivingController` | `atari-2600-driving-controller` | `atari-2600-driving-controller` |
| Atari 2600 `BoosterGrip` | `atari-booster-grip` | `atari-booster-grip` |
| Atari 2600 `GenesisController` | — | — |
| Atari 2600 `Joy2BPlus` | `atari-joy2b-plus` | `atari-joy2b-plus` |
| Atari 7800 `ProLineController` | `atari-7800-control-pad-europe`, `atari-7800-pro-line-cx24` | `atari-7800-control-pad-europe` |
| Atari 7800 `LightGun` | `atari-xg-1-light-gun` | `atari-xg-1-light-gun` |
| Atari Lynx `EnhancedController` | `atari-lynx`, `atari-lynx-ii` | `atari-lynx` |
| Atari Jaguar et Jaguar CD `EnhancedController` | `atari-jaguar-controller`, `atari-jaguar-pro-controller` | `atari-jaguar-controller` |
| Atari `None` | — | — |

`mega-drive-3` reste enregistré dans le catalogue général pour un futur module Mega Drive, mais aucune DLL de console Mega Drive actuellement disponible ne peut encore le déclarer comme visuel de port.

#### Profils dont l’image existe déjà dans l’application

| VisualId | Modèle matériel | Image |
| --- | --- | --- |
| `quickshot` | QuickShot | `quickshot.png` |
| `quickshot-deluxe` | QuickShot Deluxe | `quickshot-deluxe.png` |
| `quickshot-ii-turbo` | QuickShot II Turbo | `quickshot-ii-turbo.png` |
| `competition-pro-5000` | Competition Pro 5000 | `competition-pro-5000.png` |
| `zipstik-super-pro` | Zipstik Super Pro | `zipstik-super-pro.png` |
| `konix-speedking-left-hand` | Konix Speedking, modèle pour gaucher | `konix-speedking-left-hand.png` |
| `konix-speedking-right-hand` | Konix Speedking, modèle pour droitier | `konix-speedking-right-hand.png` |
| `konix-speedking-analog` | Konix Speedking analogique | `konix-speedking-analog.png` |
| `suncom-tac-2` | Suncom TAC-2 | `suncom-tac-2.png` |
| `powerplay-cruiser` | Powerplay Cruiser | `powerplay-cruiser.png` |
| `suzo-the-arcade-turbo` | Suzo The Arcade Turbo | `suzo-the-arcade-turbo.png` |

| `commodore-cd32` | Manette Commodore CD32 | `commodore-cd32.png` |
| `competition-pro-cd32` | Competition Pro CD32 | `competition-pro-cd32.png` |
| `atari-cx40` | Atari CX40 | `atari-cx40.png` |
| `atari-5200-controller` | Contrôleur Atari 5200 | `atari-5200-controller.png` |
| `atari-7800-pro-line-cx24` | Atari 7800 Pro-Line CX24 | `atari-7800-pro-line-cx24.png` |
| `atari-7800-control-pad-europe` | Atari 7800 Control Pad européen | `atari-7800-control-pad-europe.png` |
| `atari-jaguar-controller` | Manette Atari Jaguar | `atari-jaguar-controller.png` |
| `atari-jaguar-pro-controller` | Manette Atari Jaguar Pro | `atari-jaguar-pro-controller.png` |


Le fichier `advanced-gravis-gamepad.png` présent dans le dossier ne reproduit pas le modèle matériel exact : sa commande directionnelle n’a pas la croix violette de l’Advanced Gravis GamePad. Aucun profil ni aucune zone ne lui sont associés. Le VisualId peut rester déclaré par les DLL pour une utilisation future, mais l’application l’exclut du sélecteur tant qu’une image conforme et ses zones n’ont pas été validées.

#### Correspondance des rôles visuels avec les commandes des DLL

Les noms de la colonne **Rôle visuel** sont les valeurs typées communes utilisées par les profils d’image. La colonne **Identifiant de commande DLL** reprend exclusivement un identifiant présent dans les `InputBindingDefinition` de la ligne concernée. Une ligne absente signifie que la zone correspondante du profil reste inactive pour ce choix.

| Module, machines et choix | Rôle visuel | Identifiant de commande DLL |
| --- | --- | --- |
| Amiga, tous les modèles, `Joystick` | `DirectionUp`, `DirectionDown`, `DirectionLeft`, `DirectionRight` | `Up`, `Down`, `Left`, `Right` |
| Amiga, tous les modèles, `Joystick` | `PrimaryAction`, `SecondaryAction`, `Turbo` | `B`, `A`, `L2` |
| Amiga, tous les modèles, `AnalogJoystick` | `DirectionUp`, `DirectionDown`, `DirectionLeft`, `DirectionRight` | `Up`, `Down`, `Left`, `Right` |
| Amiga, tous les modèles, `AnalogJoystick` | `PrimaryAction`, `SecondaryAction`, `Turbo` | `B`, `A`, `L2` |
| Amiga CD32, `Cd32Pad` | `DirectionUp`, `DirectionDown`, `DirectionLeft`, `DirectionRight` | `Up`, `Down`, `Left`, `Right` |
| Amiga CD32, `Cd32Pad` | `PrimaryAction`, `SecondaryAction`, `TertiaryAction`, `QuaternaryAction` | `B`, `A`, `Y`, `X` |
| Amiga CD32, `Cd32Pad` | `LeftShoulder`, `RightShoulder`, `Start`, `Turbo` | `L`, `R`, `Start`, `L2` |
| Atari ST/STF/STFM/Mega ST/STE/Mega STE/TT/Falcon, `Joystick` | `DirectionUp`, `DirectionDown`, `DirectionLeft`, `DirectionRight` | `Up`, `Down`, `Left`, `Right` |
| Atari ST/STF/STFM/Mega ST/STE/Mega STE/TT/Falcon, `Joystick` | `PrimaryAction`, `Turbo` | `Fire1`, `Turbo` |
| Atari 400/800/800XL/130XE/XEGS/XL-XE/2600, `Joystick` | `DirectionUp`, `DirectionDown`, `DirectionLeft`, `DirectionRight`, `PrimaryAction` | `Up`, `Down`, `Left`, `Right`, `Fire1` |
| Atari 5200, `AnalogJoystick` | `DirectionUp`, `DirectionDown`, `DirectionLeft`, `DirectionRight` | `Up`, `Down`, `Left`, `Right` |
| Atari 5200, `AnalogJoystick` | `PrimaryAction`, `SecondaryAction`, `Start`, `Pause`, `Reset` | `Fire1`, `Fire2`, `Start`, `Pause`, `Reset` |
| Atari 5200, `AnalogJoystick` | `Key0` à `Key9`, `KeyStar`, `KeyHash` | `Key0` à `Key9`, `Star`, `Hash` |
| Atari 400/800/800XL/130XE/XEGS/XL-XE/2600, `Paddle` | `PrimaryAction` | `Fire1` |
| Atari 2600, `DrivingController` | `PrimaryAction` | `Fire1` |
| Atari 2600, `BoosterGrip` | `DirectionUp`, `DirectionDown`, `DirectionLeft`, `DirectionRight` | `Up`, `Down`, `Left`, `Right` |
| Atari 2600, `BoosterGrip` | `PrimaryAction`, `SecondaryAction`, `Turbo` | `Fire1`, `Fire2`, `Turbo` |
| Atari 2600, `GenesisController` | `DirectionUp`, `DirectionDown`, `DirectionLeft`, `DirectionRight`, `PrimaryAction` | `Up`, `Down`, `Left`, `Right`, `Fire1` |
| Atari 2600, `Joy2BPlus` | `DirectionUp`, `DirectionDown`, `DirectionLeft`, `DirectionRight` | `Up`, `Down`, `Left`, `Right` |
| Atari 2600, `Joy2BPlus` | `PrimaryAction`, `SecondaryAction` | `Fire1`, `Fire2` |
| Atari 7800, `ProLineController` | `DirectionUp`, `DirectionDown`, `DirectionLeft`, `DirectionRight` | `Up`, `Down`, `Left`, `Right` |
| Atari 7800, `ProLineController` | `PrimaryAction`, `SecondaryAction` | `Fire1`, `Fire2` |
| Atari 7800, `LightGun` | `PrimaryAction` | `Fire1` |
| Atari Lynx, `EnhancedController` | `DirectionUp`, `DirectionDown`, `DirectionLeft`, `DirectionRight` | `Up`, `Down`, `Left`, `Right` |
| Atari Lynx, `EnhancedController` | `PrimaryAction`, `SecondaryAction`, `Option1`, `Option2`, `Pause` | `Fire1`, `Fire2`, `Option1`, `Option2`, `Pause` |
| Atari Jaguar/Jaguar CD, `EnhancedController` | `DirectionUp`, `DirectionDown`, `DirectionLeft`, `DirectionRight` | `Up`, `Down`, `Left`, `Right` |
| Atari Jaguar/Jaguar CD, `EnhancedController` | `PrimaryAction`, `SecondaryAction`, `TertiaryAction`, `Option`, `Pause` | `A`, `B`, `C`, `Option`, `Pause` |
| Atari Jaguar/Jaguar CD, `EnhancedController` | `Key0` à `Key9`, `KeyStar`, `KeyHash` | `Key0` à `Key9`, `Star`, `Hash` |
| Amiga ou Atari, `None` | — | — |

### Réutilisation du système existant

Le système de représentation déjà utilisé dans l’onglet général **Manettes** doit être repris et adapté. Il ne faut pas en créer une copie indépendante pour les périphériques émulés.

Chaque image possède sa propre définition des positions, dimensions et formes de ses zones, puisque les commandes ne se trouvent pas au même endroit d’un périphérique à l’autre. Ces coordonnées propres à l’image sont exprimées en pourcentage par rapport à celle-ci afin de rester correctement alignées lorsque l’image est redimensionnée dans son bloc.

Pour un port donné, seules les zones correspondant aux `InputBindingDefinition` produites par la DLL pour le type de périphérique émulé sont actives, survolables et cliquables. Les commandes supplémentaires éventuellement visibles sur l’image ne créent aucune commande que l’émulateur ne gère pas.

Un profil d’image ne contient pas directement les identifiants de commandes propres à Amiga, Atari ou à un autre module. Il décrit ses zones avec des rôles visuels neutres et typés. Chaque DLL associe elle-même ces rôles aux identifiants exacts de ses `InputBindingDefinition` pour chaque `EmulationControllerChoice`. L’application active une zone uniquement lorsque cette association existe et que l’identifiant associé fait partie des définitions du choix courant. Ainsi, un même profil QuickShot reste unique tout en utilisant `B` sur Amiga et `Fire1` sur Atari pour son bouton principal, sans chaîne de commande propre à un module dans le catalogue de l’application.

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

## Checklist détaillée — Point 6 : associations et visualisation des manettes et joysticks

Cette checklist adapte le ControllerVisualizer déjà utilisé dans l’onglet général Manettes. Elle ne crée aucun second visualiseur. Les identifiants des périphériques émulés et de leurs commandes restent ceux fournis par AmigaInputSettingsFunctions et AtariInputSettingsFunctions.

- [x] Inscrire les décisions et l’inventaire nécessaires avant de créer des images ou des zones
  - [x] Modifier docs/tasks/interface/emulation/controllers.md, dans la section 6, pour ajouter un tableau de toutes les valeurs EmulationControllerChoice réellement produites par src/GWGUI.Emulation.Amiga/Functions/AmigaInputSettingsFunctions.cs et src/GWGUI.Emulation.Atari/Functions/AtariInputSettingsFunctions.cs, avec les machines concernées et leurs InputBindingDefinition.
  - [x] Modifier le tableau de la section 6 dans docs/tasks/interface/emulation/controllers.md après validation pour identifier les périphériques basiques à réaliser maintenant et laisser les autres comme ajouts ultérieurs, sans inventer de périphérique absent des deux listes.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation/controllers.md pour inscrire, pour chaque périphérique réellement produit par une DLL, les VisualId compatibles, le VisualId par défaut et, lorsqu’il existe déjà, le nom exact de l’image présente dans src/GWGUI.App/Assets/Controllers avec son modèle matériel.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation/controllers.md pour préciser que les profils portent des rôles visuels neutres et typés, puis que chaque DLL associe ces rôles uniquement aux identifiants de commandes de ses propres InputBindingDefinition.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation/controllers.md pour inscrire séparément, pour chaque EmulationControllerChoice réellement produit, la correspondance entre ses rôles visuels et les identifiants exacts de commandes produits par sa DLL.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation/controllers.md pour inscrire séparément les zones du profil `quickshot` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation/controllers.md pour inscrire séparément les zones du profil `quickshot-deluxe` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation/controllers.md pour inscrire séparément les zones du profil `quickshot-ii-turbo` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation/controllers.md pour inscrire séparément les zones du profil `competition-pro-5000` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation/controllers.md pour inscrire séparément les zones du profil `zipstik-super-pro` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation/controllers.md pour inscrire séparément les zones des profils `konix-speedking-left-hand` et `konix-speedking-right-hand` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation/controllers.md pour inscrire séparément les zones du profil `konix-speedking-analog` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation/controllers.md pour inscrire séparément les zones du profil `suncom-tac-2` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation/controllers.md pour inscrire séparément les zones du profil `powerplay-cruiser` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation/controllers.md pour inscrire séparément les zones du profil `suzo-the-arcade-turbo` en pourcentage et avec leur rôle visuel typé.
  - [x] Inspecter src/GWGUI.App/Assets/Controllers/advanced-gravis-gamepad.png, inscrire dans la section 6 son exclusion des profils disponibles tant que le modèle exact n’est pas remplacé et validé, et ne créer aucune zone pour l’image non conforme.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation/controllers.md pour inscrire séparément les zones du profil `commodore-cd32` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation/controllers.md pour inscrire séparément les zones du profil `competition-pro-cd32` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation/controllers.md pour inscrire séparément les zones du profil `atari-cx40` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation/controllers.md pour inscrire séparément les zones du profil `atari-5200-controller` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation/controllers.md pour inscrire séparément les zones du profil `atari-7800-pro-line-cx24` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation/controllers.md pour inscrire séparément les zones du profil `atari-7800-control-pad-europe` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation/controllers.md pour inscrire séparément les zones du profil `atari-jaguar-controller` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation/controllers.md pour inscrire séparément les zones du profil `atari-jaguar-pro-controller` en pourcentage et avec leur rôle visuel typé.

- [x] Séparer l’état visuel des données GameInput sans changer le visualiseur général
  - [x] Créer le fichier vide src/GWGUI.App/Enums/Input/ControllerVisualControl.cs.
  - [x] Modifier src/GWGUI.App/Enums/Input/ControllerVisualControl.cs pour déclarer uniquement les contrôles généraux effectivement consommés par le ControllerVisualizer existant, sans nom écrit en chaîne brute.
  - [x] Créer le fichier vide src/GWGUI.App/Contracts/Input/ControllerVisualState.cs.
  - [x] Modifier src/GWGUI.App/Contracts/Input/ControllerVisualState.cs pour transporter par des propriétés typées les valeurs numériques et les états actifs du visualiseur général, ainsi que les valeurs des commandes émulées indexées uniquement par les identifiants fournis par les profils et les InputBindingDefinition, sans contrôle WPF ni nom d’axe écrit en chaîne brute.
  - [x] Modifier src/GWGUI.App/Contracts/Input/ControllerVisualState.cs pour distinguer les états standard des états résolus par libellé, mémoriser la présence des états Gamepad, volant, vol et arcade, et transporter la première direction de commutateur afin de préserver les priorités et replis actuels.
  - [x] Compléter src/GWGUI.App/Enums/Input/ControllerVisualControl.cs et src/GWGUI.App/Contracts/Input/ControllerVisualState.cs avec les commandes C/Z et un ensemble de directions du premier commutateur afin de conserver les diagonales sans dépendre des enums GameInput.
  - [x] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualInput.cs pour convertir GameInputLiveState vers ControllerVisualState, préserver exactement les priorités et replis actuels entre états standard et contrôles bruts, puis lire uniquement les propriétés typées de cet état commun.
  - [x] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualizer.cs pour conserver sans changement les propriétés Model et State de l’onglet général, convertir State par ControllerVisualInput et permettre à l’éditeur d’émulation de fournir directement un ControllerVisualState sans remplacer le chemin existant.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par la séparation de l’état visuel.
- [x] Décrire les images et zones en pourcentage dans le visualiseur existant
  - [x] Créer le fichier vide src/GWGUI.Emulation/Enums/EmulationControllerVisualControl.cs.
  - [x] Modifier src/GWGUI.Emulation/Enums/EmulationControllerVisualControl.cs pour déclarer uniquement les rôles visuels neutres utilisés par les profils validés, sans identifiant de module ni texte affiché.
  - [x] Créer le fichier vide src/GWGUI.Emulation/Constants/EmulationControllerVisualIds.cs.
  - [x] Modifier src/GWGUI.Emulation/Constants/EmulationControllerVisualIds.cs pour centraliser les VisualId neutres des modèles matériels, y compris ceux préparés pour de futurs modules, sans nom de module ni texte affiché.
  - [x] Modifier src/GWGUI.Emulation/Contracts/EmulationControllerChoice.cs pour transporter la liste des VisualId compatibles déclarée par la DLL, son VisualId par défaut et la correspondance typée entre rôles visuels et identifiants de ses InputBindingDefinition, sans dépendre de WPF ni de l’existence d’une image dans l’application.
  - [x] Créer le fichier vide src/GWGUI.Emulation/Constants/EmulationControllerCommandIds.cs.
  - [x] Modifier src/GWGUI.Emulation/Constants/EmulationControllerCommandIds.cs pour centraliser uniquement les identifiants de commandes communs réellement utilisés par les profils, sans texte visible ni identifiant de module.
  - [x] Modifier src/GWGUI.Emulation.Amiga/Constants/AmigaInputSettingsFunctionsConstants.cs, src/GWGUI.Emulation.Atari/Constants/AtariInputSettingsFunctionsConstants.cs et src/GWGUI.Emulation.Atari/Constants/AtariControllerConstants.cs pour réutiliser les constantes communes correspondant exactement à leurs valeurs actuelles, sans modifier les InputBindingDefinition produites.
  - [x] Modifier src/GWGUI.Emulation.Amiga/Functions/AmigaInputSettingsFunctions.cs et src/GWGUI.Emulation.Atari/Functions/AtariInputSettingsFunctions.cs pour déclarer les VisualId compatibles et la correspondance entre rôles visuels et commandes de chaque EmulationControllerChoice réellement produit, utiliser QuickShot par défaut pour leurs types Joystick et ne pas déclarer un visuel propre à une console absente.
  - [x] Créer le fichier vide src/GWGUI.App/Enums/Input/ControllerVisualZoneShape.cs.
  - [x] Modifier src/GWGUI.App/Enums/Input/ControllerVisualZoneShape.cs pour déclarer uniquement les formes effectivement validées dans le tableau de la section 6.
  - [x] Créer le fichier vide src/GWGUI.App/Contracts/Input/ControllerVisualZone.cs.
  - [x] Modifier src/GWGUI.App/Contracts/Input/ControllerVisualZone.cs pour porter un EmulationControllerVisualControl neutre, la forme et les coordonnées en pourcentage propres à l’image, sans identifiant de commande propre à un module.
  - [x] Créer le fichier vide src/GWGUI.App/Contracts/Input/ControllerArtworkProfile.cs.
  - [x] Modifier src/GWGUI.App/Contracts/Input/ControllerArtworkProfile.cs pour porter l’image et la liste de ControllerVisualZone sans dupliquer le rendu.
  - [x] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkCatalog.cs pour conserver le catalogue des ControllerVisualModel actuels, exposer les profils réellement disponibles par VisualId et retourner uniquement l’intersection entre ce catalogue et les VisualId compatibles déclarés par la DLL.
  - [x] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualizer.cs pour afficher un ControllerArtworkProfile avec le même calcul de redimensionnement que les images existantes et exposer le survol et le clic de ses zones.
  - [x] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualizer.Artwork.cs pour dessiner les halos des profils avec les fonctions communes déjà utilisées par les modèles généraux et aligner les zones depuis leurs pourcentages.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par les profils et zones.

- [x] Enregistrer le choix du visuel de chaque port sans modifier le périphérique émulé
  - [x] Modifier src/GWGUI.Emulation/Contracts/EmulationControllerPort.cs pour transporter un VisualId facultatif après les données existantes, sans modifier leur ordre ni leur valeur.
  - [x] Modifier src/GWGUI.Emulation.Amiga/Contracts/AmigaControllerBinding.cs et src/GWGUI.Emulation.Atari/Contracts/AtariControllerBinding.cs pour enregistrer un VisualId facultatif à la fin de chaque contrat afin que les anciennes configurations restent lisibles.
  - [x] Modifier src/GWGUI.Emulation.Amiga/Functions/AmigaInputSettingsFunctions.cs et src/GWGUI.Emulation.Atari/Functions/AtariInputSettingsFunctions.cs pour transporter VisualId entre la configuration du module et EmulationControllerPort sans modifier le type, DeviceId, les associations ni DeadZonePercent.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/EmulationControllerPortEditor.cs pour créer le sélecteur de visuel du port et conserver séparément le type émulé, le VisualId sélectionné et les associations.
  - [x] Modifier src/GWGUI.App/Contracts/Emulation/Controllers/EmulationControllerPortSettings.cs pour transporter le sélecteur de visuel avec les contrôles du port.
  - [x] Modifier src/GWGUI.App/Controllers/Emulation/Input/EmulationInputSettingsController.cs pour remplir le sélecteur avec l’intersection des VisualId compatibles déclarés par la DLL et des profils présents dans ControllerArtworkCatalog, restaurer le VisualId enregistré ou le défaut déclaré par la DLL, conserver le choix dans l’état d’édition courant et le transmettre à Apply sans modifier les associations.
  - [x] Modifier src/GWGUI.App/Resources/00-Base/Emulation.resx pour ajouter les noms invariants des modèles matériels disponibles, sans les ajouter aux fichiers de langues.
  - [x] Modifier src/GWGUI.App/Resources/00-Base/Emulation.resx et tous les fichiers src/GWGUI.App/Resources/*/Emulation.resx pris en charge pour ajouter uniquement le libellé traduisible du sélecteur de visuel.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par le transport et l’enregistrement du VisualId.

- [x] Retirer le choix global du périphérique physique sans perdre les configurations existantes
  - [x] Modifier src/GWGUI.Emulation/Functions/EmulationInputMappingFunctions.cs pour exposer la valeur d’une source de manette identifiée et faire conserver à IsControllerSourcePressed ses résultats actuels en utilisant cette valeur.
  - [x] Modifier src/GWGUI.Emulation.Atari/Functions/AtariInputSnapshotFunctions.cs pour résoudre, pour chaque association, l’identifiant de périphérique inclus dans sa source et conserver DeviceId enregistré comme repli pour les anciennes associations.
  - [x] Modifier src/GWGUI.Emulation.Atari/Functions/AtariInputSnapshotFunctions.cs pour accepter les sources clavier et souris déjà représentées dans EmulationInputSnapshot, comme le chemin Amiga, sans modifier les commandes cibles.
  - [x] Modifier src/GWGUI.Emulation.Amiga/Functions/AmigaInputSnapshotFunctions.cs uniquement pour faire passer ses sources de manette par la valeur commune ajoutée, en conservant la résolution par association, les sources clavier et souris et le repli DeviceId existants.
  - [x] Modifier src/GWGUI.Emulation.Amiga/Functions/AmigaInputSettingsFunctions.cs pour autoriser la souris parmi les sources capturables des ports Amiga, sans modifier les types de périphériques émulés.
  - [x] Modifier src/GWGUI.Emulation.Atari/Functions/AtariInputSettingsFunctions.cs pour autoriser la souris parmi les sources capturables des ports Atari, sans modifier les types de périphériques émulés.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/EmulationControllerPortEditor.cs pour supprimer le ComboBox Device et son choix automatique après capture, tout en conservant la valeur PhysicalDeviceId déjà enregistrée comme donnée de compatibilité non modifiable.
  - [x] Modifier src/GWGUI.App/Contracts/Emulation/Controllers/EmulationControllerPortSettings.cs pour retirer le contrôle Device et conserver uniquement les éléments encore affichés.
  - [x] Modifier src/GWGUI.App/Controllers/Emulation/Input/EmulationInputSettingsController.cs pour ne plus remplir ni enregistrer un sélecteur physique, préserver PhysicalDeviceId d’une configuration existante et laisser chaque nouvelle association conserver sa propre source.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationControllerSettingsSection.cs pour supprimer la détection et la sélection globales devenues inutilisées.
  - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationControllerSettingsLayout.cs pour retirer le champ Périphérique du port et conserver le choix du type de périphérique émulé.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs provoquées par ce retrait et l’élargissement des sources.

- [x] Placer le visualiseur à droite du tableau du port actif
  - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationControllerSettingsConstants.cs pour ajouter uniquement la largeur nécessaire à l’icône de la colonne État, sans créer de dimensions propres à une copie du visualiseur.
  - [x] Modifier src/GWGUI.App/Contracts/Emulation/Controllers/EmulationControllerPortSettings.cs pour transporter le ControllerVisualizer du port avec son type et son InputBindingEditor.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/EmulationControllerPortEditor.cs pour créer un seul ControllerVisualizer par port et lui affecter le profil correspondant au type émulé sélectionné.
  - [x] Modifier src/GWGUI.App/Controllers/Emulation/Input/EmulationInputSettingsController.cs pour conserver ModuleId et MachineId de la configuration courante et les transmettre à chaque EmulationControllerPortEditor sans les déduire d’un libellé affiché.
  - [x] Modifier UpdateControllerBindings dans src/GWGUI.App/Controllers/Emulation/Input/EmulationInputSettingsController.cs pour changer ensemble les lignes et le profil lorsqu’un type de périphérique émulé est choisi.
  - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationControllerSettingsLayout.cs pour réutiliser le ControllerVisualizer commun à droite du tableau du même port, le conserver hors du défilement vertical et le contraindre à l’espace restant afin qu’il ne dépasse pas, sans réduire le tableau ni créer un second bloc visuel.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/InputBindingEditor.xaml pour réduire la colonne État à son icône, retirer uniquement StateText de la ligne et conserver les boutons Assigner et Supprimer.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par cette disposition.

- [x] Relier les associations et la représentation sans créer un second chemin de capture
  - [x] Créer le fichier vide src/GWGUI.App/Controllers/Emulation/Input/EmulationBindingVisualizationController.cs.
  - [x] Modifier src/GWGUI.App/Controllers/Emulation/Input/EmulationBindingVisualizationController.cs pour lire les associations courantes de InputBindingEditor, les états clavier, souris et GameInput disponibles et produire un ControllerVisualState contenant tous les appuis simultanés.
  - [x] Reporter l’application d’un seuil ou de DeadZonePercent dans EmulationBindingVisualizationController tant que le choix entre réglage général, émulateur ou machine n’est pas validé ; transmettre entre-temps les valeurs analogiques brutes sans inventer de règle.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/InputBindingEditor.xaml.cs pour exposer une opération commune qui sélectionne une ligne par son identifiant et démarre sa capture.
  - [x] Modifier AssignClicked dans src/GWGUI.App/Views/Controls/Emulation/Input/InputBindingEditorCaptureFunctions.cs pour appeler cette opération commune sans changer les sources ni le délai de capture.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/EmulationControllerPortEditor.cs pour raccorder le clic d’une zone du ControllerVisualizer à la même opération commune et ne créer ni double-clic ni bouton supplémentaire.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/EmulationControllerPortEditor.cs pour démarrer et arrêter EmulationBindingVisualizationController avec le chargement et le déchargement du port, sans laisser de temporisateur ou de gestionnaire attaché.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par la visualisation en direct et le clic des zones.

- [ ] Corriger les régressions constatées pendant la validation du point 6
  - [x] Sérialiser les sauvegardes automatiques Amiga et Atari afin d’empêcher toute collision sur leur fichier temporaire lors d’un changement de type, notamment vers Aucune.
  - [ ] Supprimer toute association physique générique fournie par les DLL et ne pas recycler les associations de l’ancien type lors d’un changement de type.
  - [x] Limiter l’Atari 2600 au visuel CX40 et utiliser le Control Pad européen comme visuel par défaut de l’Atari 7800.
  - [ ] Conserver le tableau et le visualiseur à dimensions fixes, faire défiler uniquement les lignes du tableau et garder le visualiseur visible à droite.
  - [ ] Rendre les listes de types et de visuels défilantes, puis élargir la colonne État sans couper son icône.
  - [ ] Agrandir l’image dans son espace fixe sans déformer son rapport d’aspect.
  - [ ] Refaire les surimpressions communes sans point blanc ni barre de manche et compléter les zones de boutons des deux Konix Speedking.
  - [ ] Construire avec scripts/build.ps1 -Configuration Debug puis relancer le binaire Debug pour vérifier les huit défauts signalés.

- [ ] Refaire la surimpression analogique dans le système commun
  - [x] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualizer.Artwork.cs pour remplacer le trait terminé par un point des sticks par un rond partant du centre et se déplaçant selon la direction et l’inclinaison.
  - [x] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualizer.Artwork.cs pour faire partir le halo des joysticks à manche du centre et l’allonger selon leur direction et leur valeur.
  - [x] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualizer.Artwork.cs pour faire partir le halo des gâchettes du centre et l’allonger vers le bas selon leur pression.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par les trois rendus analogiques.
  - [ ] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build et vérifier ces trois rendus avec plusieurs périphériques physiques ; ne cocher cette tâche que lorsque la forme est validée.
  - [ ] Modifier docs/tasks/interface/emulation/controllers.md, dans la section 6, pour inscrire la forme précise validée pendant cette vérification.
  - [ ] Fermer l’instance de GW GUI utilisée pour cette vérification.

- [ ] Vérifier tout le point dans l’application
  - [ ] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build et vérifier successivement chaque périphérique basique validé dans chaque port Amiga et Atari où il est proposé.
  - [ ] Dans la même exécution, vérifier que le changement d’onglet de port affiche un seul tableau avec son seul visuel, que le visuel reste fixe pendant le défilement et que le tableau ne rétrécit pas lorsque la fenêtre se resserre.
  - [ ] Dans la même exécution, vérifier simultanément des associations provenant de plusieurs manettes, du clavier, de la souris et d’un périphérique déconnecté, sans sélection préalable d’un périphérique physique.
  - [ ] Dans la même exécution, vérifier qu’un clic sur chaque zone sélectionne la bonne ligne et démarre la même capture que Assigner, puis vérifier que la modification d’association ne laisse aucun halo permanent.
  - [ ] Dans la même exécution, vérifier une configuration ancienne contenant PhysicalDeviceId afin de confirmer que son repli continue à fonctionner alors que le champ n’est plus affiché.
  - [ ] Dans la même exécution, revenir à l’onglet général Manettes et vérifier que le visualiseur existant utilise toujours ses modèles et bénéficie du nouveau rendu analogique commun.
  - [ ] Fermer l’instance de GW GUI utilisée pour cette vérification.
