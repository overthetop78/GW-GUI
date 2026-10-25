# Affectations groupées des sticks

## Fonctionnement existant

Le contrat commun `InputBindingDefinition.OppositeDirectionId` décrit déjà les paires de directions. L’éditeur commun sait capturer un axe physique, compléter sa direction opposée et conserver la pression analogique avec les sources `Controller:<DeviceId>:LeftStickUp` ou `AxisNPositive/Negative`. Chaque module conserve ses identifiants et sa conversion native. Aucun second format de configuration n’est nécessaire.

Les définitions explicites des sticks Sony (PS1/PS2/PSP/PS3), Nintendo 64/GameCube et des axes Sega utilisent ces paires. Acorn, Apple, Atari et Commodore les appliquent selon le périphérique analogique sélectionné. Les autres familles (Coleco, Epoch, Fairchild, Mattel, Panasonic, Philips, Sharp, Sinclair, Thomson, Watara) déclarent aussi leurs paires directionnelles ou axes spécialisés. L’éditeur devra uniquement proposer les groupes présents dans les définitions reçues pour le périphérique réellement sélectionné, pas ajouter un périphérique ni annoncer une capacité native.

## Présentation retenue

Un raccourci d’affectation au-dessus de la liste permet de choisir une manette et son stick physique pour les quatre directions liées. Les paires isolées conservent un choix d’axe physique. Les appareils non standard utilisent leurs vrais indices d’axes déclarés par GameInput, sans supposer que deux axes consécutifs forment un stick. Les associations clavier et les affectations directionnelles détaillées restent disponibles. Le choix groupé écrit les mêmes associations directionnelles déjà enregistrées et interprétées par les modules ; la relecture du choix se déduit des valeurs enregistrées, sans préférence parallèle.

## Réalisation et vérification

L’éditeur commun propose maintenant le choix direct d’un stick physique pour les quatre directions dont les identifiants et les relations opposées décrivent un groupe complet. Les paires isolées proposent les axes physiques réels. Pour les appareils GameInput standard, les sticks gauche/droit sont proposés ; pour les appareils à axes bruts, toutes les combinaisons distinctes des indices réellement déclarés sont proposées. Aucun couple d’axes consécutifs n’est supposé.

Les directions détaillées restent dans la liste : on peut garder ou remplacer chaque association clavier. Une sélection groupée modifie seulement son groupe et enregistre les mêmes sources qu’avant. Les choix se synchronisent avec les associations existantes ; les associations d’un appareil déconnecté sont conservées. « Aucun » efface seulement le groupe choisi. Les groupes disparaissent lorsqu’on sélectionne un périphérique qui ne déclare plus ces directions.

L’éditeur reste commun à toutes les familles : l’audit des conversions `MapControllerAxis` dans les modules trouve Atari, Commodore, Nintendo, Sega et Sony ; toutes disposent déjà des définitions opposées correspondantes. Les autres modules utilisant des paires directionnelles bénéficient du même raccourci pour piloter ces commandes ; une commande numérique reste numérique, aucune prise en charge analogique native n’est inventée. Aucun catalogue de capacités ni format de sauvegarde parallèle n’a été ajouté.

Traductions des huit libellés : Argos dans les 30 catalogues App, relecture française, unicité des clés et conservation des paramètres {0}/{1} vérifiées. Analyse syntaxique Roslyn de 40 fichiers C# modifiés/nouveaux et analyse XML du XAML : sans erreur. `git diff --check` : sans erreur.

Scénarios ajoutés dans les tests sources existants : conservation du clavier, affectation complète du stick droit d’un autre périphérique, conservation d’une amplitude intermédiaire via le convertisseur commun, relecture et appareil hors ligne, axes bruts non consécutifs 2/7, effacement du groupe et disparition des sélecteurs après retrait des définitions. Ces tests n’ont pas été compilés ni exécutés. Aucun build n’a été lancé. Le seul script temporaire de traduction a été supprimé après usage.


## Simplification des manettes reconnues

Lorsqu’un appareil fournit une vue gamepad standard, seuls ses sticks gauche/droit (ou leurs axes X/Y pour une paire isolée) sont proposés. Les combinaisons d’axes bruts ne doublonnent plus ces choix. Elles restent proposées pour les périphériques sans vue gamepad standard, à partir de leurs véritables indices. Le scénario source des manettes standard inclut désormais des axes bruts pour vérifier leur exclusion ; celui des axes non consécutifs 2/7 reste conservé. Vérification syntaxique effectuée, sans nouveau build.
