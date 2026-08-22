# Migration des entrées d'émulation vers GameInput

## Statut

Cette tâche est volontairement différée. Elle ne doit pas être commencée pendant le rangement actuel des fichiers. Elle sera réalisée uniquement après instruction explicite.

## Décision

Remplacer les lectures physiques actuellement utilisées pour les entrées d'émulation par Microsoft GameInput. XInput 1.4 est la dernière version de XInput et aucune version liée à DirectX 11 ou DirectX 12 ne la remplace. GameInput est l'API actuelle destinée aux nouvelles applications.

GW GUI cible déjà `net10.0-windows10.0.19041.0`. GameInput fonctionne à partir de Windows 10 19H1. Cette migration ne retire donc pas une compatibilité Windows 7 existante : la version actuelle de GW GUI et .NET 10 ne prennent déjà pas Windows 7 en charge.

## Fonctionnement actuel à remplacer

- `src/GWGUI.App/Services/XInputControllerReader.cs` appelle d'abord `xinput1_4.dll`, puis utilise `xinput9_1_0.dll` comme repli.
- XInput limite la lecture à quatre manettes, identifiées par les index `0` à `3`.
- `Windows.Gaming.Input` est actuellement utilisé uniquement pour obtenir les noms affichés des manettes.
- Les boutons standards, les sticks et les deux gâchettes sont lus par XInput.
- Les gâchettes sont reçues séparément avec une valeur de `0` à `255`, puis normalisées par App.
- Le bouton Guide repose actuellement sur la valeur non officielle `0x0400`.
- Le bouton Share et les quatre palettes des manettes Xbox Elite ne sont pas correctement exposés par le lecteur actuel.

## Résultat attendu

- GameInput devient l'unique source de lecture des claviers, souris, manettes, volants, pédaliers, joysticks de simulation et autres contrôleurs physiques destinés à l'émulation.
- Un périphérique ne doit jamais être publié deux fois par deux API différentes.
- App continue de convertir les données physiques vers les contrats communs d'entrée, puis les regroupe dans `EmulationInputSnapshot` ; `EmulationControllerState` reste le contrat d'état d'un contrôleur.
- Les projets `GWGUI.Emulation.xxx` ne connaissent ni GameInput, ni XInput, ni une API physique Windows.
- Les boutons standards, le pavé directionnel, les clics de sticks, les axes, les gâchettes, Guide, Share et les quatre palettes Elite sont représentés séparément lorsqu'ils existent réellement sur le périphérique.
- Les périphériques et leurs capacités sont découverts depuis les informations fournies par GameInput ; aucune liste de capacités ne doit être supposée identique pour toutes les manettes.
- L'identité stable d'un périphérique ne doit pas dépendre uniquement de son ordre de connexion.
- Le nom affiché reste une donnée d'App et ne doit pas devenir un identifiant technique.
- La saisie de texte, la navigation, l'accessibilité et les interactions ordinaires des contrôles WPF restent entièrement gérées par WPF et par Windows ; elles ne font pas partie de cette migration.
- GameInput est utilisé uniquement pour transmettre les entrées physiques aux machines émulées, définir leurs associations et gérer les raccourcis spéciaux d'émulation concernés.

## Travail à réaliser

### 1. Dépendance et interopérabilité C#

- [ ] Sélectionner la version stable de `Microsoft.GameInput` disponible au moment de la réalisation.
- [ ] Ajouter le paquet officiel `Microsoft.GameInput` au projet qui assure la lecture physique des manettes.
- [ ] Vérifier le contenu réel du paquet, sa version d'API et ses conditions de redistribution avant d'écrire l'interopérabilité.
- [ ] Créer une couche d'interopérabilité C# dédiée, car le paquet officiel expose une API native C++ et ne fournit pas directement des types .NET prêts à utiliser.
- [ ] Isoler tous les pointeurs, interfaces COM, conversions natives et libérations de ressources dans cette couche.
- [ ] Garantir la libération déterministe des lectures, périphériques, callbacks et de l'instance GameInput.

### 2. Lecture commune des périphériques d'émulation

- [ ] Créer le lecteur GameInput dans le dossier correspondant à sa responsabilité après la fin du rangement structurel.
- [ ] Énumérer les manettes réellement connectées sans limite artificielle à quatre périphériques.
- [ ] Conserver pour chaque périphérique son identifiant stable, son nom affichable et ses capacités annoncées.
- [ ] Gérer proprement la connexion, la déconnexion et la reconnexion d'une manette.
- [ ] Associer chaque configuration de port au bon périphérique sans dépendre d'un index XInput `0` à `3`.
- [ ] Ne pas mélanger les claviers et souris physiques avec les manettes lorsque seules les manettes sont demandées.
- [ ] Lire séparément les claviers physiques utilisés par l'émulation sans remplacer la saisie de texte WPF.
- [ ] Lire les souris utilisées par l'émulation avec leurs cinq boutons standards, leurs mouvements et leurs molettes verticale et horizontale.
- [ ] Énumérer les volants, pédaliers, joysticks de vol, manettes des gaz, contrôleurs d'arcade et contrôleurs génériques réellement annoncés par GameInput.
- [ ] Représenter les axes, boutons et sélecteurs selon les capacités réelles de chaque périphérique sans imposer la disposition d'une manette Xbox.
- [ ] Prévoir une extension Raw HID séparée uniquement pour les commandes propriétaires que GameInput ne représente pas directement.

### 3. Correspondance des commandes

- [ ] Reprendre la correspondance actuelle des boutons standards sans changer les actions déjà enregistrées.
- [ ] Lire séparément `A`, `B`, `X`, `Y`, `LB`, `RB`, View/Back, Menu/Start, les quatre directions et les deux clics de sticks.
- [ ] Conserver séparément les axes X/Y de chaque stick.
- [ ] Conserver séparément les gâchettes gauche et droite ; ne pas les fusionner en un axe Z.
- [ ] Ajouter Guide comme bouton système pris en charge officiellement par GameInput.
- [ ] Ajouter Share comme bouton distinct de Guide.
- [ ] Ajouter les quatre palettes Elite comme quatre commandes distinctes lorsque la manette les expose.
- [ ] Ne pas inventer de palettes, de bouton Share ou d'autres commandes sur une manette qui ne les annonce pas.
- [ ] Conserver les zones mortes et normalisations dans des fonctions communes clairement séparées de la lecture native.

### 4. Compatibilité des configurations existantes

- [ ] Inventorier tous les identifiants de boutons et périphériques actuellement enregistrés avant de définir le nouveau format.
- [ ] Conserver les associations existantes lorsque leur commande possède un équivalent GameInput.
- [ ] Ajouter une migration explicite des identifiants `xinput:*` vers les identifiants GameInput lorsque la correspondance est certaine.
- [ ] Ne pas réaffecter silencieusement une configuration à une autre manette si l'identité du périphérique ne peut pas être confirmée.
- [ ] Prévoir un état clair pour une ancienne manette configurée mais absente.

### 5. Raccordement à App et aux émulations

- [ ] Remplacer tous les appels à `XInputControllerReader` par le lecteur GameInput.
- [ ] Conserver `EmulationControllerState` comme contrat commun transmis aux instances de machines.
- [ ] Adapter l'éditeur d'associations pour détecter toutes les commandes réellement exposées par la manette.
- [ ] Utiliser le même éditeur d'associations pour les entrées physiques compatibles : clavier d'émulation, souris, manette, volant, pédalier, joystick et contrôleurs génériques.
- [ ] Adapter la liste des périphériques dans les Options sans ajouter de présentation propre à une marque de manette.
- [ ] Vérifier les appels utilisés par les machines ouvertes, la capture d'une nouvelle association et la détection d'activité.
- [ ] Supprimer `XInputButtonConstants` et les déclarations `DllImport` XInput seulement après disparition de tous leurs consommateurs.
- [ ] Supprimer l'utilisation de `Windows.Gaming.Input` destinée uniquement à compenser l'absence de nom dans XInput si GameInput fournit directement les données nécessaires.
- [ ] Ne conserver aucun double chemin XInput/GameInput une fois la migration validée, sauf décision explicite motivée par un système réellement pris en charge.
- [ ] Ne modifier aucun comportement WPF de saisie, navigation, tabulation ou accessibilité dans cette tâche.

### 6. Installation et publication

- [ ] Intégrer le redistribuable GameInput officiel à l'installateur de GW GUI.
- [ ] Vérifier le mode de déploiement côte à côte disponible dans la version sélectionnée avant de choisir entre installation du runtime et déploiement local.
- [ ] Empêcher toute rétrogradation d'un runtime GameInput plus récent déjà installé.
- [ ] Produire une erreur traduite et exploitable si le runtime nécessaire ne peut pas être initialisé.
- [ ] Vérifier les publications installée et portable séparément.

### 7. Validation

- [ ] Tester une manette Xbox standard avec tous ses boutons, ses deux sticks et ses deux gâchettes.
- [ ] Tester simultanément les deux gâchettes afin de confirmer qu'elles restent indépendantes.
- [ ] Tester Guide et Share comme deux boutons différents.
- [ ] Tester une manette Xbox Elite et ses quatre palettes comme commandes distinctes.
- [ ] Tester une manette ne possédant ni Share ni palettes et vérifier qu'aucune commande inexistante n'apparaît.
- [ ] Tester plusieurs manettes simultanées, leur déconnexion, leur reconnexion et un changement d'ordre de connexion.
- [ ] Tester plus de quatre manettes si le matériel disponible le permet.
- [ ] Tester le clavier et la souris dans une machine émulée sans modifier leur fonctionnement dans les contrôles WPF.
- [ ] Tester au moins un périphérique à axes autre qu'une manette si le matériel est disponible : volant, pédalier ou joystick de simulation.
- [ ] Vérifier la conservation et la migration des associations enregistrées avant la modification.
- [ ] Tester les entrées dans l'éditeur d'associations et dans au moins une instance Amiga et une instance Atari en fonctionnement.
- [ ] Vérifier l'absence de double détection d'une même manette.
- [ ] Compiler en Debug et exécuter les tests concernés avant de considérer la migration terminée.

## Condition de fin

La migration est terminée uniquement lorsque XInput n'est plus utilisé, que le runtime GameInput est correctement livré, que les configurations existantes sont conservées ou migrées sans affectation silencieuse incorrecte, et que les commandes standards et modernes sont vérifiées sur du matériel réel.
