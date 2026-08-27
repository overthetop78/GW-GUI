# Référence locale GameInput pour les tests

Ce fichier conserve les informations techniques déjà vérifiées afin de ne pas refaire les mêmes recherches Internet. Les sections datées constituent un journal historique de validation : les fichiers de tests qu’elles citent ont depuis été retirés et ne représentent plus une commande à exécuter. Avant toute nouvelle recherche GameInput, vérifier ce document et l'en-tête installé :

`C:\Users\overt\.nuget\packages\microsoft.gameinput\3.5.268\native\include\GameInput.h`

## Sources Microsoft déjà consultées

- `GameInputDeviceInfo` : https://learn.microsoft.com/en-us/gaming/gdk/docs/reference/input/gameinput/structs/gameinputdeviceinfo
- `IGameInput` : https://learn.microsoft.com/en-us/gaming/gdk/docs/reference/input/gameinput/interfaces/igameinput/igameinput
- `IGameInput::FindDeviceFromId` : https://learn.microsoft.com/en-us/gaming/gdk/docs/reference/input/gameinput/interfaces/igameinput/methods/igameinput_finddevicefromid
- `GameInputRawDeviceReportInfo` : https://learn.microsoft.com/en-us/gaming/gdk/docs/reference/input/gameinput/structs/gameinputrawdevicereportinfo
- `GameInputGamepadInfo` : https://learn.microsoft.com/en-us/gaming/gdk/docs/reference/input/gameinput/structs/gameinputgamepadinfo
- `GameInputLabel` : https://learn.microsoft.com/en-us/gaming/gdk/docs/reference/input/gameinput/enums/gameinputlabel

## Fonctions de IGameInput déjà identifiées

- `GetCurrentTimestamp`, `GetCurrentReading`, `GetNextReading`, `GetPreviousReading`
- `RegisterReadingCallback`, `RegisterDeviceCallback`, `RegisterSystemButtonCallback`, `RegisterKeyboardLayoutCallback`
- `StopCallback`, `UnregisterCallback`
- `FindDeviceFromId`, `FindDeviceFromPlatformString`
- `CreateDispatcher`, `SetFocusPolicy`
- `CreateAggregateDevice`, `DisableAggregateDevice` en version 3

Méthodes anciennes à ne plus rechercher comme solution actuelle :

- `FindDeviceFromObject` : supprimée en version 1 et non implémentée en version 0.
- `FindDeviceFromPlatformHandle` : supprimée en version 1 et non implémentée en version 0.

### FindDeviceFromId

Accepte un `APP_LOCAL_DEVICE_ID` provenant de `deviceId` et retrouve l'objet `IGameInputDevice` correspondant. La documentation ne lui attribue aucune énumération d'enfants.

### FindDeviceFromPlatformString

Testée avec le chemin HID, l'identifiant d'instance HID, l'identifiant USB enfant, le récepteur USB et le nom affiché. Dans les états connecté et déconnecté, chaque appel a retourné `0x80070490` sans périphérique. Ces chaînes ne permettent donc pas de retrouver la manette sur cette machine.

## GameInputDeviceInfo

Champs fixes : `vendorId`, `productId`, `revisionNumber`, `usage`, `hardwareVersion`, `firmwareVersion`, `deviceId`, `deviceRootId`, `deviceFamily`, `supportedInput`, `supportedRumbleMotors`, `supportedSystemButtons`, `containerId`, `displayName`, `pnpPath`.

Pointeurs optionnels : `keyboardInfo`, `mouseInfo`, `sensorsInfo`, `controllerInfo`, `arcadeStickInfo`, `flightStickInfo`, `gamepadInfo`, `racingWheelInfo`.

Fin de structure :

- `forceFeedbackMotorCount` + `forceFeedbackMotorInfo`
- `inputReportCount` + `inputReportInfo`
- `outputReportCount` + `outputReportInfo`

La structure x64 complète fait **256 octets**. Le pointeur `outputReportInfo` commence à l'offset 248 ; une copie limitée à 248 octets le perdait entièrement. L'interop et les diagnostics copient maintenant les 256 octets.

### Identifiants

- `deviceId` : identifiant de 256 bits propre à l'application et au système ; utilisable avec `FindDeviceFromId`.
- `deviceRootId` : racine d'un périphérique composite. Microsoft indique que `deviceId == deviceRootId` lorsque l'entrée n'est pas l'une de plusieurs interfaces composites.
- `containerId` : GUID du conteneur Windows. Ne jamais fusionner des manettes uniquement sur ce champ.
- `pnpPath` : chemin officiel vers le périphérique sous-jacent utilisable avec les API de plateforme.
- `displayName` : nom convivial pouvant décrire le transport ; ce n'est pas une preuve de la nature de l'objet.

## Familles

- `-1` : virtuel
- `0` : inconnu
- `1` : Xbox One, pilotes Xbox GIP ou XInputHID
- `2` : Xbox 360, pilote XUSB22
- `3` : HID générique
- `4` : i8042
- `5` : agrégé

## Rapports bruts

`GameInputRawDeviceReportInfo` contient `kind` (entrée/sortie), `id` (rapport ou message GIP) et `size` (octets).

## Résultats matériels conservés

### Xbox Series sans fil allumée

- GameInput : `Xbox Wireless Adapter for Windows #2`
- VID:PID : `045E:0B12`
- famille `1`, usage `0001:0005`
- 6 axes, 18 boutons, 0 commutateur
- `controllerInfo` et `gamepadInfo` présents
- 1 rapport d'entrée

Manette éteinte et récepteur toujours branché : l'entrée `045E:0B12` disparaît entièrement de GameInput.

### Turtle Beach filaire

- GameInput : `Périphérique de jeu Xbox`
- VID:PID : `10F5:7122`
- Windows : `Xbox Rematch Core Wired Controller- Black`
- famille `1`
- capacités : `RawDeviceReport, ControllerAxis, ControllerButton, Gamepad, UiNavigation`
- 6 axes, 18 boutons, 0 commutateur
- `controllerInfo` et `gamepadInfo` présents
- 1 rapport d'entrée

### Mega Drive USB

- GameInput : `Contrôleur de jeu IHM`
- VID:PID : `0810:E501`
- famille `3`, usage `0001:0004`
- `controllerInfo` et `gamepadInfo` absents
- 2 rapports d'entrée

## Règle avant toute recherche Internet

1. Lire ce fichier.
2. Lire le `GameInput.h` installé.
3. Examiner l’interopérabilité et les services actuels dans `src/GWGUI.App/Services/Input/GameInput`.
4. Chercher sur Internet seulement si l’information manque dans ces trois sources.
5. Ajouter immédiatement ici toute nouvelle information vérifiée.


## Collecte exhaustive en cours — 23 août 2026

L’inventaire brut complet des interfaces du redistribuable 3.5.268 est archivé dans `docs/gameinput-captures/2026-08-23-gameinput-3.5.268-interface-inventory.md`. Toutes les fonctions susceptibles de retourner une information doivent être testées et leurs sorties conservées avant interprétation. Une seconde capture identique devra être réalisée avec la Xbox Series éteinte, récepteur toujours branché, afin de comparer connecté/déconnecté.

Attention : le `GameInput.h` du Windows SDK 10.0.26100.0 est ancien. La référence correcte pour le runtime utilisé est `C:\Users\overt\.nuget\packages\microsoft.gameinput\3.5.268\native\include\GameInput.h`.


## Session matérielle Xbox Series connectée

La capture complète et la procédure de comparaison déconnectée sont enregistrées dans `docs/gameinput-captures/2026-08-23-xbox-series-connected-summary.md`. Consulter ce fichier et les relevés bruts associés avant tout nouveau test.


## Comparaison Xbox Series déconnectée

Le relevé déconnecté et la comparaison complète sont enregistrés dans `docs/gameinput-captures/2026-08-23-xbox-series-disconnected-summary.md`. Résultat central : le récepteur `045E:02E6` reste présent, mais sa propriété `Children` disparaît ; les nœuds enfant `045E:0B12` passent à `Present=false`/problème 45 et l’entrée GameInput disparaît.


## Couche d'inspection et interface Manettes — 23 août 2026

- L'énumération applicative conserve un descripteur complet par objet GameInput ; aucun regroupement par `containerId`.
- Le nom présenté suit la chaîne d'identité Windows reliée au `pnpPath` GameInput, sans supposer un modèle d'après le nom du récepteur.
- Les capacités standard, contrôles étiquetés, rapports bruts, moteurs, haptique et états vivants sont exposés séparément.
- Le drapeau runtime `0x01000000` est nommé `UiNavigation` afin d'éviter l'affichage numérique `17039367`.
- L'onglet principal Options `Manettes` permet de choisir le périphérique et le modèle visuel, puis affiche et anime ses entrées.
- Les modèles visuels connus sont suggérés par identité exacte ; les périphériques inconnus restent sélectionnables et modifiables manuellement.
- La capture enrichie déconnectée est `docs/gameinput-captures/2026-08-23-xbox-series-disconnected-enriched.trx`.


## Stabilisation COM et validation Debug — 23 août 2026

- Les exceptions `InvalidCastException` consignées entre 06:31 et 06:46 provenaient toutes de `testhost.exe`, pas de `gwgui.exe`.
- Cause corrigée : l'objet COM GameInput était créé sur un thread puis interrogé directement depuis le thread STA de WPF. Toutes les opérations publiques passent désormais par un worker MTA dédié.
- Les lectures ne récupèrent plus `IGameInputDevice` par conversion d'un RCW partagé et ne libèrent plus ce RCW depuis un callback brut.
- Régression matérielle ajoutée : lecture détaillée de chaque contrôleur depuis un thread WPF.
- 49 tests GameInput/localisation non interactifs passent ensemble, puis le build officiel `scripts/build.ps1 -Configuration Debug` passe.
- Validation dans `build/Debug/GW GUI/gwgui.exe` : ouverture de l'onglet Manettes, lecture continue et nouvelle détection sans nouvelle entrée dans `errors-20260823.log` ; le processus reste réactif.

Captures supplémentaires de la session :

- `docs/gameinput-captures/2026-08-23-xbox-series-live-enriched.trx`
- `docs/gameinput-captures/2026-08-23-xbox-series-live-enriched-2.trx`
- `docs/gameinput-captures/2026-08-23-xbox-series-live-enriched-3.trx`
- `docs/gameinput-captures/2026-08-23-xbox-series-live-activity.trx`

Ces trois relevés enrichis instantanés ne contiennent que `usb gamepad` et `Xbox Rematch Core Wired Controller- Black`. Pendant l'écoute interactive de 15 secondes, aucune variation d'entrée n'a été reçue. Cela décrit seulement l'état matériel observé à ces instants ; la capture reconnectée antérieure prouve séparément que l'entrée Xbox Series `045E:0B12` est bien énumérée lorsqu'elle est connectée.

## Validation réelle du 23 août 2026 — crash WPF et états matériels

- Capture brute avec les contrôleurs allumés : `docs/captures/gameinput-20260823-controller-on-raw.txt`.
- Lecture matérielle détaillée et dix cycles de réinitialisation/lecture : `docs/captures/gameinput-20260823-live-stress.txt`.
- Les périphériques encore connectés pendant la lecture détaillée étaient `usb gamepad` (0810:E501) et `Xbox Rematch Core Wired Controller- Black` (10F5:7122). La Xbox Series sans fil 045E:0B12 s'était déjà éteinte et n'était plus énumérée par GameInput.
- Le scénario réel Options > Manettes > Détecter > sélectionner la Turtle Beach a révélé quatre exceptions WPF, conservées dans `docs/captures/gwgui-errors-after-live-ui-20260823.txt` : la colonne `Active` utilisait la liaison implicite TwoWay sur une propriété en lecture seule.
- Correction : la liaison `Active` est explicitement `Mode=OneWay`. Après reconstruction par `scripts/build.ps1 -Configuration Debug`, redétection, changement de périphérique et quinze secondes de lecture continue, `errors-20260823.log` est resté à 92 964 octets : zéro nouvelle erreur.
- Le lot GameInput/visualisation/localisation passe à 58/58 dans l'environnement Windows normal. Le test matériel détaillé confirme au repos 6 axes à 0,000 et 18 boutons à 0 pour la Turtle Beach.
- Captures visuelles : `build/Debug/options-manettes-after-binding-fix.png`, `build/Debug/options-manettes-live-values.png` et `build/Debug/options-manettes-current-idle.png`.
- Le faux état bleu au repos est corrigé : les boutons Gamepad absents ne sont plus interprétés comme pressés et les axes bruts absents sont centrés à 0,5. Le test `MissingRawControlsAreReleasedAndCentered` et la capture `build/Debug/options-manettes-empty-raw-fixed.png` couvrent cette régression.


## Capture reconnectée et décodage HID brut — 23 août 2026

- Capture GameInput brute conservée dans docs/captures/gameinput-controller-on-20260823.txt et sa source TRX. L’objet reçu porte le nom GameInput brut Périphérique de jeu Xbox, VID:PID d’interface 045E:02FF, usage 0001:0005, avec controllerInfo et gamepadInfo présents, 6 axes et 18 boutons.
- Une lecture ultérieure de toute la chaîne PnP a établi que 045E:02FF est l’interface XInput enfant de la Turtle Beach : HID\VID_045E&PID_02FF&IG_00 remonte à USB\VID_10F5&PID_7122, dont le nom de bus est Xbox Rematch Core Wired Controller- Black. Cette capture ne constitue donc pas une reconnexion de la Xbox Series sans fil.
- La manette USB `0810:E501` est bien détectée par GameInput comme `RawDeviceReport`, mais GameInput 3.5.268 laisse `controllerInfo` et `gamepadInfo` à null. Son chemin `pnpPath` GameInput est donc ouvert uniquement pour récupérer les données préparées HID ; la détection et les octets d’entrée restent ceux de GameInput.
- Le parseur repose sur les structures Windows `HIDP_CAPS`, `HIDP_BUTTON_CAPS` et `HIDP_VALUE_CAPS`, puis sur `HidP_GetUsages` et `HidP_GetUsageValue`. Références officielles : https://learn.microsoft.com/windows-hardware/drivers/ddi/hidpi/ns-hidpi-_hidp_caps, https://learn.microsoft.com/windows-hardware/drivers/ddi/hidpi/ns-hidpi-_hidp_button_caps, https://learn.microsoft.com/windows-hardware/drivers/ddi/hidpi/ns-hidpi-_hidp_value_caps.
- Résultat matériel réel après branchement du parseur : `usb gamepad | 0810:E501 | RawDeviceReport | controls=13`, soit 11 boutons et 2 axes centrés à 0,5 au repos, au lieu de 0 contrôle.
- Les tailles natives sont vérifiées par test : 64 octets pour `HIDP_CAPS`, 72 pour `HIDP_BUTTON_CAPS`, 72 pour `HIDP_VALUE_CAPS`. Les conversions signées, normalisations et chapeaux 0/1-based sont également testées.
- Une écoute interactive de 15 secondes est conservée dans `docs/captures/gameinput-usb-live-20260823.*`. Aucun changement physique n’a été capturé pendant cette fenêtre ; elle ne prouve donc ni une panne ni une réussite du signal actif.
- Le journal `errors-20260823.log` n’a reçu aucune nouvelle entrée de `gwgui.exe` après les erreurs WPF de 08:01 déjà corrigées. Les entrées suivantes à 08:09 proviennent du faux périphérique volontairement défaillant d’un test.

- Capture visuelle du build Debug final : `docs/captures/manettes-hid-final-build.png`. L’écran réel affiche `2 axes · 11 boutons · 0 commutateurs` pour `usb gamepad` (0810:E501).
- Après ouverture de Manettes et redétection, `gwgui.exe` reste réactif. Le journal reste stable à 99 757 octets sans lancer de tests ; les nouvelles entrées de 09:04 à 09:13 sont toutes produites par `testhost.exe`, pas par l’application.
- Validation ciblée finale : 74/74 tests GameInput, HID, interface Manettes, visualisation et localisation passent ; le build officiel `scripts/build.ps1 -Configuration Debug` passe.


## Reconnexion courte et lecture des plantages — 23 août 2026

- La capture brute avait d’abord été attribuée à tort à la Xbox sans fil. La lecture PnP complète réalisée ensuite prouve que l’objet 045E:02FF appartient à la Turtle Beach 10F5:7122. La Xbox Series sans fil observée dans les captures connectée/déconnectée précédentes est l’objet GameInput 045E:0B12.
- La lecture détaillée conservée dans `docs/captures/gameinput-controller-reconnected-detailed-20260823.txt` contient encore `usb gamepad` (0810:E501, 11 boutons et 2 axes décodés) et `Xbox Rematch Core Wired Controller- Black` (10F5:7122, 6 axes et 18 boutons).
- Dix réinitialisations et lectures successives sont conservées dans `docs/captures/gameinput-refresh-stability-20260823.txt`; elles terminent sans crash et conservent les deux périphériques présents.
- Au moment du contrôle, `gwgui.exe` PID 28480 est encore `Running`. Le journal d'événements Windows ne contient aucun événement récent 1000 (`Application Error`) ni 1026 (`.NET Runtime`).
- Dans `errors-20260823.log`, les seules erreurs dont `Application` vaut `gwgui` restent les quatre liaisons WPF `Active` de 08:01 déjà corrigées par `Mode=OneWay`. Les entrées plus récentes, jusqu'à 09:29, ont toutes `Application: testhost` et proviennent des scénarios de test volontairement défaillants.


## Correction ABI COM GameInput et validation réelle — 23 août 2026

- Comparaison faite contre le header installé `Microsoft.GameInput 3.5.268\native\include\GameInput.h` : les méthodes qui retournent directement `void` ou `bool` doivent conserver leur signature native.
- Toutes les méthodes des interfaces `ComImport` GameInput portent désormais `PreserveSig`. `IGameInputReading.GetRawReport` retourne désormais le booléen officiel au lieu d'être déclaré `void`.
- Un test de réflexion `GameInputInteropSignatureTests` verrouille toutes les interfaces COM et le type de retour de `GetRawReport`.
- Preuve matérielle avant/après sur la Turtle Beach 10F5:7122 : avant la correction, le périphérique annonçait `Gamepad` mais l'état standard restait `null`; après la correction, `GetGamepadState` renvoie bien `GAMEPAD buttons=None` et les six valeurs analogiques au repos.
- Capture : `docs/captures/gameinput-detailed-after-preservesig-20260823.txt`.
- Le build officiel `scripts/build.ps1 -Configuration Debug` passe. Dans le vrai `build/Debug/GW GUI/gwgui.exe`, Options > Manettes affiche `usb gamepad`, le visuel Mega Drive, 2 axes, 11 boutons et les deux rapports d'entrée. Capture : `docs/captures/gameinput-preservesig-ui-20260823.png`.
- Après un clic réel sur Détecter, le processus PID 38168 reste répondant et `errors-20260823.log` reste exactement à 101349 octets.
- La suite GameInput/HID/visualisation hors écoute interactive passe 38/38. Une écoute annoncée de 15 secondes n'a observé aucun mouvement ; sa sortie est conservée dans `docs/captures/gameinput-live-signal-after-preservesig-20260823.txt` et ne constitue pas une validation physique réussie.


## Audit final de l'interface et nouvelle lecture matérielle — 23 août 2026

- Tous les enums déclarés localement ont été comparés numériquement au header installé Microsoft.GameInput 3.5.268 ; l'audit permanent est GameInputEnumLayoutTests et sa sortie est conservée dans docs/captures/gameinput-enum-audit-3.5.268-20260823.json. Le seul drapeau supplémentaire est UiNavigation=0x01000000, réellement observé au runtime.
- Les 125 valeurs GameInputLabel possèdent désormais un libellé explicite. Les valeurs inconnues restent numériques (#N) au lieu d'être transformées en texte anglais.
- Les dessins modernes, rétro, volant, manche de vol et borne d'arcade réagissent maintenant à leurs épaules, directions, boutons, axes, pédales, rapport engagé, lacet et chapeau directionnel correspondants. Les sélecteurs couvrent tous les modèles demandés et leurs libellés restent localisés lors d'un changement de langue.
- Les fonctionnalités affichent aussi le statut GameInput, le nombre et le type de moteurs de vibration, le nombre de moteurs à retour de force, leur alimentation, les axes et effets pris en charge.
- Le build officiel Debug suivant ces changements passe. Le vrai gwgui.exe PID 38368 ouvre Options > Manettes, accepte une nouvelle détection et reste répondant.
- errors-20260823.log contient 72 blocs : 68 appartiennent à testhost.exe ; les quatre seuls blocs de gwgui.exe datent tous de 08:01 et correspondent à l'ancienne liaison WPF Active déjà corrigée par Mode=OneWay. Aucun événement Windows récent 1000 ou 1026 ne concerne GW GUI.
- Nouvelle lecture matérielle : dix actualisations consécutives passent avec usb gamepad et la Turtle Beach. L'écoute de 15 secondes n'a reçu aucun mouvement et ne vaut donc pas validation du signal. La Xbox sans fil 045E:0B12 n'était pas exposée par GameInput pendant cette lecture.
- La chaîne PnP complète de la Turtle Beach confirme : interface GameInput HID\VID_045E&PID_02FF&IG_00, parent matériel USB\VID_10F5&PID_7122, nom Xbox Rematch Core Wired Controller- Black.


## Validation réelle des modèles et correction des axes — 23 août 2026

- Le sélecteur du vrai build Debug a été manipulé dans Options > Manettes avec le périphérique USB sélectionné. Les modèles Stick arcade, Joystick de vol et Volant ont chacun été rendus sans fermeture ni blocage du processus.
- Captures : docs/captures/manettes-arcade-real-debug-20260823.png, docs/captures/manettes-flight-real-debug-20260823.png et docs/captures/manettes-wheel-real-debug-20260823.png.
- La sélection réelle de la Turtle Beach au repos a révélé un défaut : ses axes ControllerAxis centrés à 0,000 étaient traités comme des axes HID centrés à 0,500, ce qui colorait à tort Haut et Gauche.
- Correction : les axes GameInput ControllerAxis restent signés et centrés sur zéro ; seuls les axes du chemin HID RawDeviceReport sont convertis de [0,1] vers [-1,1]. Lorsque Gamepad est disponible, ses bits D-pad sont autoritaires et aucun axe générique ne les remplace.
- Régression ajoutée : NeutralStandardGamepadDoesNotActivateDpadFromCenteredControllerAxes. Les 10 tests ControllerVisualizationTests passent.
- Le test d’intégration de OptionsControllersSection injecte maintenant un bouton A, quatre axes et deux gâchettes, vérifie le dessin pressé, puis relâché, en plus du tableau et des valeurs analogiques. Les 5 tests GameInputControllersSectionBehaviorTests passent.
- Les erreurs volontaires de ce test utilisent désormais un logger injecté. Elles restent vérifiées mais n’écrivent plus dans le journal utilisateur de GW GUI.
- Preuve : une nouvelle exécution des 5 tests a laissé errors-20260823.log strictement inchangé à 106125 octets.
- Le build officiel scripts/build.ps1 -Configuration Debug passe après ces changements.
- Validation finale du binaire build/Debug/GW GUI/gwgui.exe, PID 36244 : Options > Manettes, Turtle Beach sélectionnée, 6 axes, 18 boutons, 4 moteurs de vibration, D-pad neutre, processus répondant.
- Capture finale : docs/captures/manettes-final-debug-turtlebeach-20260823.png.
- Après cette validation réelle, le journal reste strictement inchangé à 106125 octets et conserve seulement les quatre anciennes erreurs gwgui de 08:01 déjà corrigées.
- Le bouton Tester les vibrations a été actionné dans le vrai build Debug sur la Turtle Beach. GameInput a accepté la commande, l’interface a affiché Test terminé après 500 ms, le processus est resté répondant et le journal est resté inchangé à 106125 octets. Capture : docs/captures/manettes-rumble-completed-real-debug-20260823.png.



## Validation multilingue réelle — 23 août 2026

- Le vrai build Debug a été basculé de français vers japonais, puis de japonais vers italien, sans redémarrer Options.
- En japonais, l’onglet Manettes remplace immédiatement le bouton de détection, les étiquettes des sélecteurs, le compteur, les capacités, les valeurs analogiques, l’identité et le bouton de vibration. Capture : docs/captures/manettes-japanese-real-debug-20260823.png.
- Après japonais vers italien, aucun libellé japonais ne reste dans Manettes. Capture : docs/captures/manettes-italian-after-japanese-real-debug-20260823.png.
- La page autrefois fautive Émulation > Atari > CPU a également été vérifiée après cette bascule : onglets, cartes, listes et valeurs sont tous italiens. Capture : docs/captures/atari-cpu-italian-after-japanese-real-debug-20260823.png.
- Le nom brut GameInput Périphérique de jeu Xbox reste affiché dans l’identité matérielle parce qu’il provient directement de Windows ; ce n’est pas un libellé d’interface ni un résidu du dictionnaire précédent.
- La langue utilisateur a ensuite été restaurée en français. Le processus PID 36244 reste répondant et errors-20260823.log reste inchangé à 106125 octets.


## Correction du crash natif de détection — 23 août 2026

- Rectification de la validation précédente : le processus PID 36244 a ensuite quitté brutalement à 12:55:19 lors d'une nouvelle détection. Le journal géré de GW GUI n'a rien reçu parce que l'arrêt était natif.
- Preuve Windows WER : événement BEX64, module fautif `C:\Windows\system32\GameInputRedist.dll` 3.5.268.0, exception `0xC0000409`, donnée `7`, offset `0x240AD`, identifiant `ba07d824-e623-4f16-831e-5d1bc5df7bdb`.
- Cause dans GW GUI : le bouton Détecter détruisait puis recréait toute l'instance GameInput. Le callback de périphérique enregistrait et désinscrivait également des callbacks de lecture avant d'être revenu dans GameInput. Or GameInput impose de ne libérer les ressources d'un callback qu'après une désinscription réussie et déclenche un arrêt fatal si une désinscription intervient dans un callback.
- Correction : le callback natif se limite désormais à mettre la notification en file ; toutes les inspections, inscriptions, désinscriptions et libérations COM sont effectuées ensuite sur le worker MTA. L'énumération initiale vide complètement cette file avant de rendre son premier résultat.
- Le bouton Détecter ne reconstruit plus une instance GameInput saine : le callback de périphérique maintient déjà la liste en temps réel. Une reconstruction n'est tentée qu'après un véritable échec d'initialisation.
- Le retour booléen de `UnregisterCallback` est maintenant contrôlé. Aucun contexte GCHandle, pointeur COM, mapper ou décodeur HID n'est libéré si GameInput n'a pas confirmé la désinscription.
- Test matériel après correction : dix actualisations et lectures détaillées successives ont conservé simultanément `usb gamepad`, `Xbox Rematch Core Wired Controller- Black` et `Xbox Series X Controller`, sans fermeture.
- La toute première lecture renvoie maintenant les trois périphériques. La Xbox Series est résolue comme `045E:0B12`, famille `XboxOne`, 6 axes, 18 boutons, avec toutes les correspondances GameInput. Capture permanente : `docs/captures/gameinput-callback-lifecycle-fixed-20260823.trx`.
- Une surveillance réelle de 30 secondes démarrée à 13:04:53 a conservé les trois périphériques sans crash ; aucune connexion ou déconnexion ne s'est produite durant ce créneau, donc elle ne valide pas encore le callback de disparition.
- Exigence visuelle rectifiée : les silhouettes abstraites actuelles ne constituent pas la validation finale. Chaque modèle doit avoir une représentation réellement reconnaissable et fidèle à sa forme et à la disposition de ses commandes.


### Refonte visuelle et hiérarchie de l'onglet Manettes — 23 août 2026

- La vue principale affiche uniquement le périphérique, la détection, son nom résolu, la représentation interactive et les valeurs analogiques utiles.
- La table exhaustive des contrôles, les capacités et l'identité technique sont conservées dans un panneau replié.
- Un périphérique reconnu précisément impose automatiquement son modèle visuel ; aucun sélecteur manuel n'est affiché.
- Un périphérique non typé affiche un choix **Auto** puis les modèles manuels.
- SEGA Mega Drive est séparée en deux modèles : 3 boutons et 6 boutons.
- Les silhouettes Xbox, PlayStation, Master System, SEGA Mega Drive, Saturn et Dreamcast ont été redessinées avec des formes, couleurs, commandes et matériaux propres à chaque matériel.
- Planche de contrôle : docs/captures/controller-models-reference-20260823.png.
- Capture de la vraie section WPF avec une Xbox Series injectée : docs/captures/options-controllers-xbox-series-reference-20260823.png.
- Dernière lecture matérielle : la Xbox Series 045E:0B12 était déconnectée ; seules la manette USB 0810:E501 et la Turtle Beach 10F5:7122 étaient présentes. La validation physique du nom n'a donc pas été simulée.


## Audit final de l’onglet Manettes — 23 août 2026

- La vue principale ne conserve que le sélecteur de périphérique, la détection, le nom résolu, le visuel interactif, les valeurs analogiques et le test de vibration. Les contrôles bruts, capacités et identifiants restent disponibles dans le panneau **Fonctionnalités**, replié par défaut.
- Les périphériques reconnus exactement ne montrent aucun sélecteur de forme. Les périphériques inconnus montrent **Auto** et les formes manuelles disponibles.
- Le catalogue distingue **SEGA Mega Drive 3 boutons** et **SEGA Mega Drive 6 boutons**. Le périphérique réel `0810:E501` est reconnu exactement comme `usb gamepad` et utilise automatiquement `MegaDrive6`.
- Le périphérique réel `10F5:7122` est résolu comme `Xbox Rematch Core Wired Controller- Black`, utilise automatiquement le modèle distinct `XboxRematchCore` et n’affiche aucun sélecteur manuel.
- Les visuels Xbox Series/Xbox One/Turtle Beach, PlayStation 4/5, Master System, Mega Drive 3/6, PlayStation 1/2, Saturn et Dreamcast ont des silhouettes et commandes propres. Volant, joystick de vol et stick arcade restent les représentations génériques de leur catégorie quand aucun modèle exact n’est connu.
- Tous les bits de boutons standard GameInput des gamepads, volants, joysticks de vol et sticks arcade sont injectés un par un dans les tests ; chaque signal modifie le visuel concerné. Les axes, gâchettes, pédales, volant, rapport engagé et chapeau directionnel sont également injectés individuellement.
- Le test de vibration vérifie la séquence complète : activation des seuls moteurs annoncés par le périphérique, attente de 500 ms, puis arrêt explicite des quatre canaux.
- Les 66 tests non interactifs GameInput/onglet/visuels passent. Les 40 tests de localisation passent pour les 29 langues, y compris le remplacement immédiat des textes et les nouvelles valeurs de modèles.
- Le build officiel `scripts/build.ps1 -Configuration Debug` passe. Le binaire `build/Debug/GW GUI/gwgui.exe` a été lancé, était répondant, puis a été fermé avec `Remaining=0`. Aucun nouvel événement Windows `gwgui`, `GW GUI` ou `GameInputRedist.dll` n’a été trouvé.
- Relevé matériel après rallumage annoncé de la Xbox Series : trois exécutions séparées, dix actualisations sur douze secondes et une capture brute `RegisterDeviceCallback` n’ont exposé que `usb gamepad` et `Xbox Rematch Core Wired Controller- Black`. Aucune entrée `045E:0B12` n’était fournie par GameInput.
- Une écoute d’activité de quinze secondes n’a reçu aucun changement. Elle a échoué avec la liste GameInput exacte `usb gamepad (0810:E501)`, `Xbox Rematch Core Wired Controller- Black (10F5:7122)`. Cette absence est conservée comme constat matériel ; elle n’est pas attribuée au filtre de GW GUI puisque la capture brute GameInput ne contenait pas non plus la Xbox Series.
- Au même instant, Windows PnP marquait pourtant `HID\VID_045E&PID_0B12&IG_00`, son interface USB et le récepteur `USB\VID_045E&PID_02E6` comme présents et sans problème. Les résolutions directes GameInput du chemin HID, des deux identifiants PnP, du récepteur et du nom `Xbox Wireless Adapter for Windows #2` ont toutes renvoyé `0x80070490` (introuvable). Le désaccord actuel est donc Windows PnP présent / GameInput absent, pas une suppression par GW GUI.

## Correction de l’énumération multi-manettes GameInput — 23 août 2026

- Steam reçoit tous les boutons de la Xbox Series ; aucune API employée par Steam n’est déduite de ce seul fait.
- Windows PnP confirme `045E:0B12` présent et sans erreur, tandis que l’ancien callback combiné de GW GUI ne le conservait pas.
- Le diagnostic charge séparément `C:\Windows\System32\GameInputRedist.dll` et le runtime déclaré par `HKLM\SOFTWARE\WOW6432Node\Microsoft\GameInput\RedistDir`. Les fichiers ont la même version, la même taille et le même SHA-256, mais leur chemin de chargement change les périphériques Xbox exposés dans ce processus.
- Avec le runtime System32, les filtres `Gamepad`, `Controller` et `RawDeviceReport` n’ont renvoyé que la Turtle Beach, puis la Turtle Beach et la SEGA pour le filtre brut.
- Avec le runtime enregistré dans `C:\Program Files\Microsoft GameInput\x64`, `Gamepad` et `Controller` ont renvoyé la Xbox Series et la Turtle Beach ; `RawDeviceReport` a renvoyé Xbox Series, Turtle Beach et SEGA USB.
- Un gros masque unique mélangeant brut, contrôleurs, clavier et souris peut omettre l’une des manettes Xbox. GW GUI utilise désormais quatre callbacks indépendants : rapport brut, contrôleurs, clavier et souris. Les résultats sont fusionnés par le `GameInputDeviceId` propre à chaque périphérique, jamais par récepteur ni `ContainerId`.
- Le bouton Détecter réénumère uniquement les deux familles utiles aux contrôleurs — rapport brut et contrôleurs standard — afin de ne pas attendre inutilement les callbacks clavier et souris.
- Le chargeur choisit d’abord un runtime enregistré ou app-local. Entre ceux-ci, la version la plus récente gagne et le runtime enregistré gagne une égalité. Le runtime System32 n’est utilisé qu’en secours, même si sa version affichée est supérieure.
- Preuve après correction : le callback brut de GW GUI a chargé `C:\Program Files\Microsoft GameInput\x64\GameInputRedist.dll` puis reçu `045E:0B12` sous les filtres `RawDeviceReport` et contrôleurs, avec état `Connected`, six axes, dix-huit boutons et le chemin HID attendu.
- Le test matériel visible a ensuite détecté simultanément `0810:E501`, `10F5:7122` et `045E:0B12`, puis reçu de la Xbox Series les axes 0, 2 et 3 ainsi que les boutons `XboxA` et `XboxB`.
- Le diagnostic interactif ouvre une fenêtre avec compte à rebours, active l’entrée en arrière-plan uniquement pendant ses quinze secondes, puis restaure la politique GameInput par défaut et ferme la fenêtre.
- Les essais effectués après l’extinction automatique de la manette ne sont pas utilisés pour invalider les relevés faits lorsqu’elle était connectée.
