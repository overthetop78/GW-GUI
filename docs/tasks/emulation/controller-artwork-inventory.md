# Inventaire des visuels de contrôleurs

Source : ressources 00-Base des cinq modules au 4 octobre 2026. Chaque ligne demande une image de face transparente et des zones exactes pour les commandes exposées. Les périphériques sans commande mappable doivent rester identifiables sans halo fictif.

Chaîne existante : InputFunctions.Visuals.cs dans Atari/Amiga → EmulationControllerVisualIds → ControllerArtworkCatalog (PNG et zones normalisées) → ControllerVisualizer.Artwork (halo, survol, clic).

Référence du premier visuel NEC PI-PD001 : photo « [PC Engine Controller.jpg](https://commons.wikimedia.org/wiki/File:PC_Engine_Controller.jpg) » de SACHEN, licence [CC BY 3.0](https://creativecommons.org/licenses/by/3.0/). La copie de travail est `docs/tasks/emulation/pc-engine-controller-reference.jpg`. Le PNG `src/GWGUI.App/Assets/Controllers/nec-pc-engine-pad.png` est une retouche de cette photo : fond retiré et transparence ajoutée ; forme, commandes et inscriptions conservées.

Référence du TurboPad HES-PAD-01 : photo « [NEC-TurboGrafx-16-Controller-FL.jpg](https://commons.wikimedia.org/wiki/File:NEC-TurboGrafx-16-Controller-FL.jpg) » de Evan-Amos, [domaine public](https://creativecommons.org/publicdomain/mark/1.0/). La copie de travail est `docs/tasks/emulation/turbografx-turbopad-reference.jpg`. Le PNG `src/GWGUI.App/Assets/Controllers/nec-turbografx-turbopad.png` redresse la manette et retire le fond blanc ; commandes et interrupteurs restent visibles.

Les visuels PI-PD6 et PI-PD8 sont des recréations faites à partir de la forme du PI-PD001 ci-dessus et de descriptions photographiques des variantes CoreGrafx. Les couleurs, les deux interrupteurs Turbo et le marquage du modèle ont été adaptés ; la photo PI-PD001 de SACHEN reste créditée. Les images ont été inspectées de face, leurs coins sont transparents, et leurs profils utilisent huit zones correspondant aux commandes déjà exposées. L'essai interactif des halos et la compilation du deuxième lot restent à faire.

Les visuels PCE-TP2 et PI-PD5 sont des recréations de face dérivées de la forme du PI-PD001 de SACHEN ci-dessus ; leurs couleurs et marquages ont été adaptés. Le visuel PI-PD4 part de la photo « [NEC PC Engine Turbo Stick Controller.jpg](https://commons.wikimedia.org/wiki/File:NEC-PC-Engine-Turbo-Stick-Controller.jpg) » de Evan-Amos, domaine public ; copie de travail `docs/tasks/emulation/pc-engine-turbostick-reference.jpg`. Le détourage du PI-PD4 garde quelques pixels blancs hors de la coque : finition nécessaire avant validation du visuel. Les trois profils sont branchés avec croix/levier, II, I, Select et Run ; les deux boutons rouges du PI-PD4 partagent les halos des grands boutons correspondants. L'essai interactif reste à faire.

Le visuel PI-PD002 est recréé à partir du PI-PD001 de SACHEN et du [descriptif du Turbo Pad NEC PI-PD002](https://www2s.biglobe.ne.jp/tetuya/FXHP/pcengine/hard/turpad.html), qui confirme ses deux interrupteurs de tir rapide. Le PNG présente de face la croix, II, I, Select et Run sur fond transparent. Les interrupteurs physiques restent visibles mais n'ont pas de commande indépendante dans la liste de configuration.

Compilation du 4 octobre 2026 après raccordement de ces huit profils NEC : `scripts\local-building.cmd --building=debug --modules=A` terminé avec code 0 ; `build/Debug/GW GUI/gwgui.exe` et les DLL des huit modules sont présents. L'affichage interactif des visuels et des halos reste à vérifier.

Les trois visuels Avenue Pad 3, Avenue Pad 6 et Arcade Pad 6 sont des recréations à partir des PNG NEC transparents précédents. Apparence et disposition vérifiées contre les descriptions et photos de l'[Avenue Pad 3 NAPD-1001](https://www2s.biglobe.ne.jp/tetuya/FXHP/pcengine/hard/avepad3.html), de l'[Avenue Pad 6 NAPD-1002](https://www.genkivideogames.com/pc-engine-hardware/pc-engine-avenue-pad-6-unboxed/) et de l'[Arcade Pad 6 PCE-TP1](https://76retrogames.com/products/nec-pc-engine-controller-pce-tp1-pc-arcade-pad-6-jpn-ver). Le bouton III de l'Avenue Pad 3 est relié à Run, comme dans le mode physique correspondant, faute de commande III distincte dans la liste actuelle. Les six boutons des deux autres modèles ont chacun une commande distincte ; les interrupteurs physiques restent seulement visibles. Les trois PNG ont un canal alpha avec coin transparent ; le PCE-TP1 garde quelques bavures extérieures à nettoyer. Build de ce lot et essai interactif encore à faire.

La compilation Debug du 4 octobre 2026 après ce lot (`scripts\local-building.cmd --building=debug --modules=A`) a réussi avec code 0. `build/Debug/GW GUI/gwgui.exe` et les huit DLL de modules sont présents. Les essais interactifs dans la fenêtre de configuration et sur les halos ne sont pas encore réalisés.

TurboGrafx TurboStick HES-STK-01 : variante noire/orange créée à partir du visuel Turbo Stick NEC précédent ; l'[apparence de l'exemplaire nord-américain](https://consolevariations.com/collectibles/nec-turbografx-turbostick) sert de référence. DuoPad : variante TurboDuo créée à partir du TurboPad noir, comparée à [deux manettes TurboGrafx/TurboDuo](https://videogamecritic.com/t16info.htm). Cordless Pad PI-PD12 : recréation sans câble avec émetteur infrarouge et boutons violets, fondée sur l'[identification du modèle PI-PD12](https://www2s.biglobe.ne.jp/tetuya/FXHP/pcengine/hard/cordles.html) ; la forme et les inscriptions exactes du boîtier doivent encore être comparées à une photo nette. Les trois PNG sont de face, avec fond transparent et huit zones mappables chacun ; essai interactif et nouvelle compilation encore à faire.

Les cinq derniers profils de commandes NEC sont des illustrations recréées à partir des vues de la [PC Engine GT](https://www.ebay.com/itm/127201001762), de la [PC Engine LT](https://www.ebay.com/itm/303597622566), de la [FX-PAD](https://commons.wikimedia.org/wiki/File:NEC-PC-FX-Controller-Flat.jpg), de la [souris PI-PD10](https://okini.land/fr/5546-pc-engine-mouse-pi-pd10-loose-nec-home-electronics-4904323919338.html) et de la [souris FX-MOU](https://www.vgdb.com.br/pc-fx/jogos/pc-fx-mouse/). Ce ne sont pas des photographies des exemplaires cités. Les cinq coins PNG sont transparents. La GT/LT et la FX-PAD ont les boutons du module raccordés aux zones ; les souris sont prévues pour l'onglet Souris et ses actions gauche/droite. Les contours et inscriptions devront être comparés aux objets réels avant validation finale ; la FX-PAD présente encore quelques pixels de frange. L'essai interactif reste à faire.

Compilation du 4 octobre 2026 après les cinq profils NEC : `scripts\local-building.cmd --building=debug --modules=A` terminé avec code 0. `build/Debug/GW GUI/gwgui.exe` et les huit DLL sous `build/Debug/GW GUI/Modules/<module>/` sont présents. L'essai interactif des images, du placement des halos et de l'onglet Souris reste à faire.

## Nec (53 clés)

| Clé | Nom invariant / libellé | Image de face et zones |
| --- | --- | --- |
| Emulation.Nec.Controller.PcEnginePad | NEC PC Engine Pad (PI-PD001) | `nec-pc-engine-pad.png` : RGBA vérifié, profil branché, huit zones (croix ×4, II, I, Select, Run). Build Debug app + huit modules réussi le 4 octobre 2026 ; essai interactif des halos encore à faire. |
| Emulation.Nec.Controller.PcEngineTurboPad | NEC Turbo Pad (PI-PD002) | `nec-pc-engine-turbopad.png` : PNG RGBA, profil et huit zones branchés ; essai interactif des halos restant. |
| Emulation.Nec.Controller.CoreGrafxTurboPad | NEC Turbo Pad (PI-PD6) | `nec-coregrafx-turbopad.png` : RGBA vérifié, profil et huit zones branchés ; essai interactif restant. |
| Emulation.Nec.Controller.CoreGrafxIITurboPad | NEC Turbo Pad (PI-PD8) | `nec-coregrafx-ii-turbopad.png` : RGBA vérifié, profil et huit zones branchés ; essai interactif restant. |
| Emulation.Nec.Controller.DuoRTurboPad | NEC Turbo Pad (PCE-TP2) | `nec-duor-turbopad.png` : profil et huit zones branchés ; halos à essayer. |
| Emulation.Nec.Controller.PcEngineTurboPadII | NEC Turbo Pad II (PI-PD5) | `nec-pc-engine-turbopad-ii.png` : profil et huit zones branchés ; halos à essayer. |
| Emulation.Nec.Controller.PcEngineTurboStick | NEC Turbo Stick (PI-PD4) | `nec-pc-engine-turbostick.png` : profil branché, boutons principaux et rouges couplés ; alpha présent mais bavures à nettoyer, halos à essayer. |
| Emulation.Nec.Controller.AvenuePad3 | NEC Avenue Pad 3 (NAPD-1001) | `nec-avenue-pad-3.png` : transparent, profil neuf zones, III couplé à Run ; halos à essayer. |
| Emulation.Nec.Controller.AvenuePad6 | NEC Avenue Pad 6 (NAPD-1002) | `nec-avenue-pad-6.png` : transparent, profil douze zones ; halos à essayer. |
| Emulation.Nec.Controller.ArcadePad6 | NEC Arcade Pad 6 (PCE-TP1) | `nec-arcade-pad-6.png` : transparent, profil douze zones ; petites bavures à nettoyer, halos à essayer. |
| Emulation.Nec.Controller.PcEngineMouse | NEC PC Engine Mouse (PI-PD10) | `nec-pc-engine-mouse.png` : RGBA, profil de l'onglet Souris et deux zones ; essai interactif à faire. |
| Emulation.Nec.Controller.CordlessPad | NEC Cordless Pad (PI-PD12) | `nec-cordless-pad.png` : transparent, profil huit zones branché ; apparence exacte et halos à vérifier. |
| Emulation.Nec.Controller.CordlessMultiTapSet | NEC Cordless Multi Tap Set (PI-PD11) | À créer / vérifier |
| Emulation.Nec.Controller.PcEngineMultiTap | NEC Multi Tap (PI-PD003) | À créer / vérifier |
| Emulation.Nec.Controller.VirtualCushion | NEC Virtual Cushion (PI-AD20) | À créer / vérifier |
| Emulation.Nec.Controller.TurboGrafxTurboPad | NEC TurboPad (HES-PAD-01) | `nec-turbografx-turbopad.png` : RGBA vérifié, profil branché, huit zones (croix ×4, II, I, Select, Run). Interrupteurs Turbo visibles, sans commande distincte dans la liste actuelle. Build et essai interactif après ce lot encore à faire. |
| Emulation.Nec.Controller.TurboGrafxTurboStick | NEC TurboStick (HES-STK-01) | `nec-turbografx-turbostick.png` : transparent, profil huit zones branché ; halos à essayer. |
| Emulation.Nec.Controller.TurboGrafxTurboTap | NEC TurboTap (HES-TAP-01) | À créer / vérifier |
| Emulation.Nec.Controller.DuoPad | DuoPad | `nec-duopad.png` : transparent, profil huit zones branché ; halos à essayer. |
| Emulation.Nec.Controller.DuoTap | DuoTap | À créer / vérifier |
| Emulation.Nec.Controller.TurboGrafxDuoAdapter | TurboGrafx-16/Duo Adapter | À créer / vérifier |
| Emulation.Nec.Controller.PcEngineGtTvTuner | NEC PC Engine GT TV Tuner (PI-AD11) | À créer / vérifier |
| Emulation.Nec.Controller.PcEngineGtLinkCable | NEC PC Engine GT Link Cable | À créer / vérifier |
| Emulation.Nec.Controller.TurboVision | NEC TurboVision | À créer / vérifier |
| Emulation.Nec.Controller.TurboLink | NEC TurboLink | À créer / vérifier |
| Emulation.Nec.Controller.CdRom2 | NEC CD-ROM² (CDR-30) | À créer / vérifier |
| Emulation.Nec.Controller.CdRom2InterfaceUnit | NEC CD-ROM² Interface Unit (IFU-30) | À créer / vérifier |
| Emulation.Nec.Controller.SuperCdRom2 | NEC Super CD-ROM² | À créer / vérifier |
| Emulation.Nec.Controller.SuperGrafxRomRomAdapter | NEC SuperGrafx ROM² Adapter (RAU-30) | À créer / vérifier |
| Emulation.Nec.Controller.TurboGrafxCd | NEC TurboGrafx-CD | À créer / vérifier |
| Emulation.Nec.Controller.BackupBooster | NEC Backup Booster (PI-AD7) | À créer / vérifier |
| Emulation.Nec.Controller.BackupBoosterII | NEC Backup Booster II (PI-AD8) | À créer / vérifier |
| Emulation.Nec.Controller.TurboBooster | NEC TurboBooster | À créer / vérifier |
| Emulation.Nec.Controller.TurboBoosterPlus | NEC TurboBooster-Plus | À créer / vérifier |
| Emulation.Nec.Controller.SuperSystemCard | NEC Super System Card | À créer / vérifier |
| Emulation.Nec.Controller.ArcadeCardDuo | NEC Arcade Card Duo (PCE-AC1) | À créer / vérifier |
| Emulation.Nec.Controller.ArcadeCardPro | NEC Arcade Card Pro (PCE-AC2) | À créer / vérifier |
| Emulation.Nec.Controller.PcFxPad | NEC PC-FX FX-PAD | `nec-pc-fx-pad.png` : RGBA, profil et treize zones mappables ; frange et halos à vérifier. |
| Emulation.Nec.Controller.PcFxMouse | NEC PC-FX FX-MOU | `nec-pc-fx-mouse.png` : RGBA, profil de l'onglet Souris et deux zones ; essai interactif à faire. |
| Emulation.Nec.Controller.PcFxBackupMemory | NEC PC-FX FX-BMP | À créer / vérifier |
| Emulation.Nec.Controller.PcFxScsiAdapter | NEC PC-FX FX-SCSI | À créer / vérifier |
| Emulation.Nec.Controller.Mouse | Mouse | À créer / vérifier |
| Emulation.Nec.Controller.TurboExpressControls | TurboExpress / PC Engine GT built-in controls | `nec-turboexpress-controls.png` : RGBA, profil et huit zones ; essai interactif à faire. |
| Emulation.Nec.Controller.PcEngineLtControls | PC Engine LT built-in controls | `nec-pc-engine-lt-controls.png` : RGBA, profil et huit zones ; essai interactif à faire. |
| Emulation.Nec.Controller.Action.I | I | À créer / vérifier |
| Emulation.Nec.Controller.Action.II | II | À créer / vérifier |
| Emulation.Nec.Controller.Action.III | III | À créer / vérifier |
| Emulation.Nec.Controller.Action.IV | IV | À créer / vérifier |
| Emulation.Nec.Controller.Action.V | V | À créer / vérifier |
| Emulation.Nec.Controller.Action.VI | VI | À créer / vérifier |
| Emulation.Nec.Controller.Action.Select | Select | À créer / vérifier |
| Emulation.Nec.Controller.Action.Run | Run | À créer / vérifier |
| Emulation.Nec.Controller.Action.Mode | Mode | À créer / vérifier |

## Microsoft (40 clés)

Le module Microsoft décrit actuellement deux machines (`Xbox` et `Xbox 360`) et leurs ports ne proposent que `Joystick` ou `None` dans `ControllerCatalog`. Les 38 autres clés ci-dessous sont des noms de périphériques conservés dans les ressources, mais elles ne correspondent pas encore à des choix de contrôleur dans l'interface d'émulation. Quatre PNG Xbox déjà fournis par l'application ont été inspectés : `xbox-360-black.png`, `xbox-360-white.png`, `xbox-one.png` et `xbox-series.png` ; leurs coins sont transparents. Ils ne forment pas encore des profils sélectionnables et ne couvrent pas toutes les révisions. Les manettes, volants et télécommandes ont des commandes à identifier avant de placer des halos ; Kinect, caméras, casques, microphone et adaptateurs doivent être identifiables, sans inventer des boutons de jeu. Hypothèse provisoire en attendant la réponse de l'utilisateur : préparer également les images des périphériques non sélectionnables, sans simuler une prise en charge de leur entrée.

Les illustrations du Duke et du Controller S se fondent respectivement sur la [vue de face du Duke](https://www.copetti.org/writings/consoles/xbox/) et la [photographie du Controller S](https://www.bruktelektronikk.no/hjem/981-original-xbox-controller-s-kablet-brukt-testet-ok.html). Elles ne réutilisent pas ces photos. Les deux PNG ont un coin alpha 0 et chacun quatorze zones de commandes physiques (sticks ×2, croix ×4, six boutons, Back, Start). Aucune correspondance de commande du module n'a été inventée : leurs halos restent inactifs jusqu'à la définition d'un profil d'entrée Xbox complet ; le placement reste à vérifier visuellement.

Les quatre illustrations Xbox 360 suivantes s'appuient sur la [manette à croix transformable](https://www.powerupgaming.ca/products/copy-of-xbox-360-official-wireless-controller-w-upgraded-d-pad), le [volant sans fil classique](https://www.xboxgazette.com/test360_ac_volant.php), le [Speed Wheel](https://www.gamestop.com/gaming-accessories/controllers/xbox-360/products/microsoft-xbox-360-wireless-speed-wheel/10101911.html) et la [Big Button Pad](https://j2games.com/products/replacement-scene-it-lights-camera-action-big-button-controller-xbox-360). Les PNG ont leurs coins alpha 0 et les commandes de face ont des zones dans `ControllerArtworkCatalog`. Le volant classique conserve des bavures autour du socle et de l'intérieur de la jante ; tous les placements de zones demandent encore une inspection dans l'application. Ces visuels ne sont pas encore sélectionnables dans le module Microsoft.

Le kit DVD Xbox est représenté par une télécommande et son récepteur IR d'après [une photographie du kit](https://www.ebay.com/itm/355739493311). Les deux Chatpad sont des illustrations distinctes ; le [Chatpad Xbox One annoncé par Microsoft](https://news.xbox.com/en-us/2015/08/05/xbox-chatpad-pre-order/amp/) dispose notamment de touches programmables et de commandes audio. Les trois PNG ont un coin alpha 0. La télécommande a des zones de navigation et de lecture, les Chatpad une zone de clavier regroupée et le bouton Xbox. Ils ne sont pas proposés par le module Microsoft et les halos ne peuvent pas être actifs tant que celui-ci ne fournit pas de commandes correspondantes. Le visuel DVD présente un voile autour des objets à nettoyer avant validation.

Build Debug du 4 octobre 2026 après ces profils : `scripts\local-building.cmd --building=debug --modules=A` terminé avec code 0. `build/Debug/GW GUI/gwgui.exe` et les huit DLL de `Modules/<id>/gwgui.emulation.<id>.dll` ont été vérifiés présents. L'essai visuel interactif et les halos restent à valider.

| Clé | Nom invariant / libellé | Image de face et zones |
| --- | --- | --- |
| Emulation.Microsoft.Controller.Joystick | Xbox controller | À créer / vérifier |
| Emulation.Microsoft.Controller.Keyboard | Xbox keyboard | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxDuke | Xbox Controller (Duke) | `xbox-duke.png` : RGBA, quatorze zones physiques, profil non sélectionnable ; vérification de l'apparence et des zones à faire. |
| Emulation.Microsoft.Controller.XboxControllerS | Xbox Controller S | `xbox-controller-s.png` : RGBA, quatorze zones physiques, profil non sélectionnable ; vérification de l'apparence et des zones à faire. |
| Emulation.Microsoft.Controller.XboxDvdMoviePlaybackKit | Xbox DVD Movie Playback Kit | `xbox-dvd-movie-playback-kit.png` : RGBA, navigation et lecture zonées ; voile de fond à nettoyer, profil non sélectionnable. |
| Emulation.Microsoft.Controller.XboxCommunicator | Xbox Communicator | À créer / vérifier |
| Emulation.Microsoft.Controller.Xbox360Controller | Xbox 360 Controller | `xbox-360-black.png` et `xbox-360-white.png` existent, RGBA ; profil du module à définir avant de placer les halos. |
| Emulation.Microsoft.Controller.Xbox360WirelessController | Xbox 360 Wireless Controller | À créer / vérifier |
| Emulation.Microsoft.Controller.Xbox360WirelessControllerTransformingDPad | Xbox 360 Wireless Controller with Transforming D-Pad | `xbox-360-transforming-dpad.png` : RGBA, treize zones de commandes ; profil non sélectionnable. |
| Emulation.Microsoft.Controller.Xbox360WirelessRacingWheel | Xbox 360 Wireless Racing Wheel | `xbox-360-wireless-racing-wheel.png` : RGBA, direction, croix et boutons zonés ; franges à nettoyer, profil non sélectionnable. |
| Emulation.Microsoft.Controller.Xbox360WirelessSpeedWheel | Xbox 360 Wireless Speed Wheel | `xbox-360-wireless-speed-wheel.png` : RGBA, direction, croix et boutons zonés ; profil non sélectionnable. |
| Emulation.Microsoft.Controller.Xbox360BigButtonPad | Xbox 360 Big Button Pad | `xbox-360-big-button-pad.png` : RGBA, buzzer, quatre réponses, Back, Start, Guide zonés ; profil non sélectionnable. |
| Emulation.Microsoft.Controller.Xbox360Chatpad | Xbox 360 Chatpad | `xbox-360-chatpad.png` : RGBA, clavier regroupé et bouton Xbox zonés ; profil non sélectionnable. |
| Emulation.Microsoft.Controller.Xbox360MediaRemote2005 | Xbox 360 Media Remote (2005) | À créer / vérifier |
| Emulation.Microsoft.Controller.Xbox360UniversalMediaRemote | Xbox 360 Universal Media Remote | À créer / vérifier |
| Emulation.Microsoft.Controller.Xbox360MediaRemote2011 | Xbox 360 Media Remote (2011) | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxLiveVision | Xbox Live Vision | À créer / vérifier |
| Emulation.Microsoft.Controller.KinectXbox360 | Kinect for Xbox 360 | À créer / vérifier |
| Emulation.Microsoft.Controller.Xbox360Headset | Xbox 360 Headset | À créer / vérifier |
| Emulation.Microsoft.Controller.Xbox360WirelessHeadset | Xbox 360 Wireless Headset | À créer / vérifier |
| Emulation.Microsoft.Controller.Xbox360WirelessHeadsetBluetooth | Xbox 360 Wireless Headset with Bluetooth | À créer / vérifier |
| Emulation.Microsoft.Controller.Xbox360WirelessMicrophone | Xbox 360 Wireless Microphone | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxOneWirelessController2013 | Xbox One Wireless Controller (2013) | `xbox-one.png` existe, RGBA ; révision exacte et profil du module à vérifier. |
| Emulation.Microsoft.Controller.XboxOneWirelessController2015 | Xbox One Wireless Controller (2015) | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxOneWirelessController2016 | Xbox Wireless Controller (2016) | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxEliteWirelessController | Xbox Elite Wireless Controller | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxEliteWirelessControllerSeries2 | Xbox Elite Wireless Controller Series 2 | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxEliteWirelessControllerSeries2Core | Xbox Elite Wireless Controller Series 2 - Core | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxAdaptiveController | Xbox Adaptive Controller | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxChatpad | Xbox Chatpad | `xbox-chatpad.png` : RGBA, clavier regroupé et bouton Xbox zonés ; profil non sélectionnable. |
| Emulation.Microsoft.Controller.KinectXboxOne | Kinect for Xbox One | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxOneMediaRemote | Xbox One Media Remote | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxOneChatHeadset | Xbox One Chat Headset | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxOneStereoHeadset | Xbox One Stereo Headset | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxOneStereoHeadsetAdapter | Xbox One Stereo Headset Adapter | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxSeriesWirelessController | Xbox Wireless Controller (2020) | `xbox-series.png` existe, RGBA ; profil du module à définir avant de placer les halos. |
| Emulation.Microsoft.Controller.XboxAdaptiveJoystick | Xbox Adaptive Joystick | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxStereoHeadset2021 | Xbox Stereo Headset (2021) | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxWirelessHeadset2021 | Xbox Wireless Headset (2021) | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxWirelessHeadset2024 | Xbox Wireless Headset (2024) | À créer / vérifier |

## Sony (52 clés)

Les visuels déjà livrés avec l'application `playstation-1.png`, `playstation-2.png`, `playstation-4.png` et `playstation-5.png` servent de profils séparés pour la manette PlayStation standard, la DUALSHOCK 2, la DUALSHOCK 4 et la DualSense. Leurs zones physiques principales sont définies dans `ControllerArtworkCatalog` et leurs noms invariants dans les ressources communes de l'application. Pour PlayStation et PlayStation 2, la liste expose maintenant les 16 bits de joypad réellement lus par SwanStation et PCSX2 : directions, quatre symboles, Select/Start, L1/R1, L2/R2 et L3/R3. La sélection du profil correspondant est raccordée aux commandes de cette liste et aux halos. Les modèles PlayStation 4 et 5 disposent aussi de leur profil visuel, mais le module ne possède pas de cœur d'émulation pour eux ; leurs commandes restent les six entrées génériques. Le PNG DualSense existant garde des résidus blancs autour de la coque. Placement des zones et essai interactif encore à vérifier.

Le DUALSHOCK original possède désormais un second profil PlayStation sélectionnable : `sony-dualshock-1.png` (3060 × 2400, alpha 0 au coin), photographie détourée d'[Evan-Amos dans le domaine public](https://commons.wikimedia.org/wiki/File:PSX-DualShock.png). La croix, les quatre symboles, les deux sticks cliquables, Select, Start et les épaules L1/R1 visibles ont des zones associées aux commandes PlayStation existantes. Les boutons L2/R2 cachés par cette perspective et le bouton Analog ne reçoivent pas de halo. Le cœur utilise toujours le périphérique joypad actuellement configuré ; l'activation des axes analogiques en jeu et le placement interactif des zones restent à vérifier.

Build Debug du 4 octobre 2026 après le profil DUALSHOCK original : `scripts\local-building.cmd --building=debug --modules=A` terminé avec code 0 ; `build/Debug/GW GUI/gwgui.exe` et les huit DLL `Modules/<marque>/gwgui.emulation.<marque>.dll` vérifiés présents. Aucun essai interactif des halos n'a encore été effectué.

Build Debug du 4 octobre 2026 après ces quatre profils : `scripts\local-building.cmd --building=debug --modules=A` terminé avec code 0 ; `build/Debug/GW GUI/gwgui.exe` et les huit DLL sous `Modules/<id>/` sont présents.

Build Debug du 4 octobre 2026 après le raccordement des commandes PlayStation et PlayStation 2 : même script terminé avec code 0 ; l'exécutable et les huit DLL des modules sont présents. Cette vérification confirme la compilation, pas encore le comportement des halos en fenêtre.

| Clé | Nom invariant / libellé | Image de face et zones |
| --- | --- | --- |
| Emulation.Sony.Controller.MSXJS55 | Sony JS-55 joystick | À créer / vérifier |
| Emulation.Sony.Controller.MSXJS75 | Sony JS-75 wireless joystick | À créer / vérifier |
| Emulation.Sony.Controller.MSXJS303T | Sony JS-303T joypad | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationController | PlayStation controller | `playstation-1.png` réutilisé ; profil sélectionné pour PlayStation, 14 zones reliées à la liste (L3/R3 demandent une manette analogique). |
| Emulation.Sony.Controller.PlayStationAnalogJoystick | PlayStation Analog Joystick | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationDualAnalog | PlayStation Dual Analog controller | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationDualShock | PlayStation DUALSHOCK controller | `sony-dualshock-1.png` : profil PlayStation supplémentaire avec croix, symboles, deux sticks, Select/Start et L1/R1 ; axes analogiques et halos à vérifier. |
| Emulation.Sony.Controller.PlayStationMouse | PlayStation Mouse | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationMultitap | PlayStation Multitap | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation2DualShock2 | PlayStation 2 DUALSHOCK 2 controller | `playstation-2.png` réutilisé ; profil sélectionné pour PlayStation 2, 16 zones reliées à la liste. |
| Emulation.Sony.Controller.PlayStation2Multitap | PlayStation 2 Multitap | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation2EyeToy | PlayStation 2 EyeToy camera | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation2SingStarMicrophones | PlayStation 2 SingStar microphones | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation2BuzzBuzzers | PlayStation 2 Buzz! Buzzers | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation3Sixaxis | PlayStation 3 SIXAXIS controller | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation3DualShock3 | PlayStation 3 DUALSHOCK 3 controller | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation3Eye | PlayStation Eye camera | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationMoveMotion | PlayStation Move motion controller | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationMoveNavigation | PlayStation Move navigation controller | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation4DualShock4 | PlayStation 4 DUALSHOCK 4 controller | `playstation-4.png` réutilisé ; profil sélectionnable, cœur et commandes complètes absents. |
| Emulation.Sony.Controller.PlayStation4Camera | PlayStation Camera | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationVRAim | PlayStation VR Aim controller | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationVR | PlayStation VR headset | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation5DualSense | PlayStation 5 DualSense wireless controller | `playstation-5.png` réutilisé ; profil sélectionnable, cœur et commandes complètes absents, résidus blancs à nettoyer. |
| Emulation.Sony.Controller.PlayStation5DualSenseEdge | PlayStation 5 DualSense Edge wireless controller | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation5Access | PlayStation 5 Access controller | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationVR2Sense | PlayStation VR2 Sense controller | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationVR2 | PlayStation VR2 headset | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation5HDCamera | PlayStation 5 HD Camera | À créer / vérifier |
| Emulation.Sony.Controller.PSPIntegrated | PSP built-in controls | À créer / vérifier |
| Emulation.Sony.Controller.PSVitaIntegrated | PS Vita built-in controls | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationPortalIntegrated | PlayStation Portal built-in controls | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation2DVDRemote | PlayStation 2 DVD remote control | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation3BDRemote | PlayStation 3 BD remote control | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation3WirelessKeypad | PlayStation 3 wireless keypad | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation5MediaRemote | PlayStation 5 Media Remote | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationMoveShootingAttachment | PlayStation Move shooting attachment | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationMoveSharpShooter | PlayStation Move sharp shooter | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationMoveRacingWheel | PlayStation Move racing wheel | À créer / vérifier |
| Emulation.Sony.Controller.MSXJS33 | Sony JS-33 joypad | À créer / vérifier |
| Emulation.Sony.Controller.MSXJS70 | Sony JS-70 joystick | À créer / vérifier |
| Emulation.Sony.Controller.MSXJSC75 | Sony JS-C75 wireless joystick | À créer / vérifier |
| Emulation.Sony.Controller.MSXGB5 | Sony GB-5 trackball | À créer / vérifier |
| Emulation.Sony.Controller.MSXGB6 | Sony GB-6 trackball | À créer / vérifier |
| Emulation.Sony.Controller.MSXGB7 | Sony GB-7 trackball | À créer / vérifier |
| Emulation.Sony.Controller.MSXMOS1 | Sony MOS-1 mouse | À créer / vérifier |
| Emulation.Sony.Controller.MSXMOS2 | Sony MOS-2 mouse | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationKonamiJustifier | Konami Justifier / Hyper Blaster (PlayStation light gun) | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationNamcoGunCon | Namco GunCon / G-Con 45 (PlayStation light gun) | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation2NamcoGunCon2 | Namco GunCon 2 (PlayStation 2 light gun) | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation3NamcoGunCon3 | Namco GunCon 3 (PlayStation 3 light gun) | À créer / vérifier |
| Emulation.Sony.Controller.ZN1ArcadeLightGun | ZN-1 arcade light gun | À créer / vérifier |

## Nintendo (265 clés)

Premier lot fonctionnel : le NES-004, le Famicom Controller I et le Super Famicom SHVC-005 réutilisent des images de face déjà présentes dans l'application ; le Super NES SNS-005 a une variante violette créée à partir de `super-nintendo.png` avec fond transparent (coin alpha 0). L'[historique Nintendo de la Super Nintendo](https://www.nintendo.com/en-za/Hardware/Nintendo-History/Super-Nintendo/Super-Nintendo-627040.html) confirme les commandes supplémentaires X/Y et L/R. Les quatre profils disposent de zones pour croix, boutons et Select/Start, plus X/Y/L/R sur les deux profils 16 bits. La liste d'entrées du module est maintenant limitée aux boutons NES/Famicom pour NES et Famicom Disk System, et étendue aux boutons SNES pour Super Nintendo/Super Famicom. Les halos sont reliés aux identifiants de joypad lus par les cœurs ; essai visuel interactif encore à faire. Les 261 autres clés d'accessoires ne sont pas couvertes par ce lot.

Compilation Debug du 4 octobre 2026 après ce raccordement : `scripts\local-building.cmd --building=debug --modules=A` terminé avec code 0. `build/Debug/GW GUI/gwgui.exe` et les DLL des huit modules sont présents. L'essai interactif de sélection des profils et des halos reste à faire.

Deuxième lot : les commandes intégrées Game Boy DMG-01, Game Boy Color CGB-001 et Game Boy Advance AGB-001 disposent chacune d'un port et d'un profil de face transparent. Les vues Game Boy et Game Boy Color proviennent des photographies du domaine public d'[Evan-Amos (Game Boy)](https://commons.wikimedia.org/wiki/File:Game-Boy-Original.png) et d'[Evan-Amos (Game Boy Color)](https://commons.wikimedia.org/wiki/File:Nintendo_Game_Boy_Color.png) ; la seconde a été redressée, avec quelques pixels magenta résiduels sur le contour à nettoyer. La [vue Game Boy Advance](https://commons.wikimedia.org/wiki/File:GameBoyAdvance-transparent.png) est créditée à Zeartul (photo), Talgraf777 (fond blanc) et Paolos (fond transparent), sous [CC BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/) ; elle est reprise sans modification. Les halos couvrent la croix, B, A, Select et Start. L/R sont proposés dans la liste Game Boy Advance et reliés aux commandes RetroPad, mais ses gâchettes ne sont pas visibles sur cette vue de face : aucun halo n'est placé arbitrairement. Les essais interactifs restent à faire ; 258 autres clés Nintendo restent à couvrir.

Build Debug du 4 octobre 2026 après les trois profils Game Boy : `scripts\local-building.cmd --building=debug --modules=A` terminé avec code 0 ; `build/Debug/GW GUI/gwgui.exe` et les huit DLL sous `build/Debug/GW GUI/Modules/<marque>/` vérifiés présents. Les profils et halos n'ont pas encore été essayés dans l'interface.

Troisième lot : `nintendo-nes-dogbone.png` (1771 × 888, coin alpha 0) est un détourage de face produit à partir de la [photographie du NES-039 publiée dans le domaine public par Evan-Amos](https://commons.wikimedia.org/wiki/File:NES-Dogbone-Controller-Flat.jpg). Les quatre directions, B, A, Select et Start disposent de zones reliées aux commandes NES déjà prises en charge par le module. Le visuel est proposé sur NES et Famicom Disk System. Son contour comporte de légères franges et les halos restent à vérifier dans l'interface ; 257 autres clés Nintendo restent à couvrir.

Build Debug du 4 octobre 2026 après le NES-039 : `scripts\local-building.cmd --building=debug --modules=A` terminé avec code 0 ; l'exécutable `build/Debug/GW GUI/gwgui.exe` et les huit DLL de modules ont été vérifiés présents (9/9 artefacts). Les halos restent à essayer dans l'interface.

Quatrième lot : la vue de face transparente `nintendo-64.png` existait déjà dans l'application ; sa provenance n'est pas documentée dans le dépôt. Le profil NUS-005 utilise quatre ports. Les lignes d'affectation séparent la croix, le stick, A/B, les quatre C, L/R, Z et Start. Le [mappage source de Mupen64Plus-Next](https://github.com/libretro/mupen64plus-libretro-nx/blob/develop/custom/mupen64plus-core/plugin/emulate_game_controller_via_libretro.c) montre qu'en mode standard A/B correspondent aux bits RetroPad B/Y, Z à L2, L/R aux boutons correspondants, le stick à l'axe gauche et les C à l'axe droit. Le cœur interprète l'axe droit horizontal en sens inversé pour les C : la conversion des affectations en tient compte. Les halos couvrent les commandes visibles ; Z, derrière la manette, est affectable mais n'a pas de zone sur cette image. Essai interactif restant à faire.

Build Debug du 4 octobre 2026 après le profil Nintendo 64 : `scripts\local-building.cmd --building=debug --modules=A` terminé avec code 0 ; `build/Debug/GW GUI/gwgui.exe` et les huit DLL de modules ont été vérifiés présents (9/9 artefacts). `git diff --check` ne signale aucune erreur. Les commandes et halos n'ont pas encore été essayés dans l'interface.

Test ciblé Nintendo 64 du 4 octobre 2026 : `dotnet test tests\GWGUI.Tests\GWGUI.Tests.csproj --no-restore --filter FullyQualifiedName~Nintendo64ControllerTests --verbosity minimal` : 3 réussites, aucun échec. Il vérifie les quatre ports publiés, la présence des lignes et des identifiants de zones, les bits A/B/L/R/Z/Start/croix, les deux axes des boutons C et du stick, l'annulation de directions opposées et la conservation des axes physiques non réaffectés. Un avertissement NU1900 signale l'indisponibilité de l'index NuGet pour les données de vulnérabilité ; il n'a pas empêché la compilation ou les tests. Les halos rendus dans la fenêtre restent à contrôler visuellement.

| Clé | Nom invariant / libellé | Image de face et zones |
| --- | --- | --- |
| Emulation.Nintendo.Controller.GameWatchBall | Nintendo Game & Watch Ball (AC-01) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchFlagman | Nintendo Game & Watch Flagman (FL-02) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchVermin | Nintendo Game & Watch Vermin (MT-03) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchFireSilver | Nintendo Game & Watch Fire (RC-04) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchJudge | Nintendo Game & Watch Judge (IP-05) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchManholeGold | Nintendo Game & Watch Manhole (MH-06) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchHelmet | Nintendo Game & Watch Helmet (CN-07) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchLion | Nintendo Game & Watch Lion (LN-08) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchParachute | Nintendo Game & Watch Parachute (PR-21) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchOctopus | Nintendo Game & Watch Octopus (OC-22) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchPopeyeWide | Nintendo Game & Watch Popeye (PP-23) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchChef | Nintendo Game & Watch Chef (FP-24) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchMickeyMouseWide | Nintendo Game & Watch Mickey Mouse (MC-25) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchEgg | Nintendo Game & Watch Egg (EG-26) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchFireWide | Nintendo Game & Watch Fire (FR-27) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchTurtleBridge | Nintendo Game & Watch Turtle Bridge (TL-28) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchFireAttack | Nintendo Game & Watch Fire Attack (ID-29) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchSnoopyTennis | Nintendo Game & Watch Snoopy Tennis (SP-30) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchOilPanic | Nintendo Game & Watch Oil Panic (OP-51) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchDonkeyKong | Nintendo Game & Watch Donkey Kong (DK-52) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchMickeyDonald | Nintendo Game & Watch Mickey & Donald (DM-53) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchGreenHouse | Nintendo Game & Watch Green House (GH-54) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchDonkeyKongII | Nintendo Game & Watch Donkey Kong II (JR-55) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchMarioBros | Nintendo Game & Watch Mario Bros. (MW-56) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchRainShower | Nintendo Game & Watch Rain Shower (LP-57) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchLifeBoat | Nintendo Game & Watch Life Boat (TC-58) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchPinball | Nintendo Game & Watch Pinball (PB-59) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchBlackJack | Nintendo Game & Watch Black Jack (BJ-60) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchSquish | Nintendo Game & Watch Squish (MG-61) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchBombSweeper | Nintendo Game & Watch Bomb Sweeper (BD-62) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchSafebuster | Nintendo Game & Watch Safebuster (JB-63) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchGoldCliff | Nintendo Game & Watch Gold Cliff (MV-64) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchZelda | Nintendo Game & Watch Zelda (ZL-65) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchDonkeyKongJrWide | Nintendo Game & Watch Donkey Kong Jr. (DJ-101) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchMarioCementFactoryWide | Nintendo Game & Watch Mario's Cement Factory (ML-102) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchManholeNewWide | Nintendo Game & Watch Manhole (NH-103) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchTropicalFish | Nintendo Game & Watch Tropical Fish (TF-104) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchSuperMarioBrosWide | Nintendo Game & Watch Super Mario Bros. (YM-105) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchClimberWide | Nintendo Game & Watch Climber (DR-106) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchBalloonFightWide | Nintendo Game & Watch Balloon Fight (BF-107) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchMarioJuggler | Nintendo Game & Watch Mario the Juggler (MB-108) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchDonkeyKongJrTabletop | Nintendo Game & Watch Donkey Kong Jr. (CJ-71) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchMarioCementFactoryTabletop | Nintendo Game & Watch Mario's Cement Factory (CM-72) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchSnoopyTabletop | Nintendo Game & Watch Snoopy (SM-73) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchPopeyeTabletop | Nintendo Game & Watch Popeye (PG-74) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchSnoopyPanorama | Nintendo Game & Watch Snoopy (SM-91) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchPopeyePanorama | Nintendo Game & Watch Popeye (PG-92) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchDonkeyKongJrPanorama | Nintendo Game & Watch Donkey Kong Jr. (CJ-93) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchMarioBombsAway | Nintendo Game & Watch Mario's Bombs Away (TB-94) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchMickeyMousePanorama | Nintendo Game & Watch Mickey Mouse (DC-95) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchDonkeyKongCircus | Nintendo Game & Watch Donkey Kong Circus (MK-96) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchSpitballSparky | Nintendo Game & Watch Spitball Sparky (BU-201) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchCrabGrab | Nintendo Game & Watch Crab Grab (UD-202) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchBoxing | Nintendo Game & Watch Boxing / Punch-Out!! (BX-301) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchDonkeyKong3 | Nintendo Game & Watch Donkey Kong 3 (AK-302) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchDonkeyKongHockey | Nintendo Game & Watch Donkey Kong Hockey (HK-303) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchMicroVsPad1 | Nintendo Game & Watch Micro VS. System Controller 1 | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchMicroVsPad2 | Nintendo Game & Watch Micro VS. System Controller 2 | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchSuperMarioBrosCrystal | Nintendo Game & Watch Super Mario Bros. (YM-801) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchClimberCrystal | Nintendo Game & Watch Climber (DR-802) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchBalloonFightCrystal | Nintendo Game & Watch Balloon Fight (BF-803) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchSuperMarioBrosDiskun | Nintendo Game & Watch Super Mario Bros. (YM-901-S) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchBallClubNintendo | Nintendo Game & Watch Ball (Club Nintendo) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchSuperMarioBros2020 | Nintendo Game & Watch: Super Mario Bros. | À créer / vérifier |
| Emulation.Nintendo.Controller.GameWatchZelda2021 | Nintendo Game & Watch: The Legend of Zelda | À créer / vérifier |
| Emulation.Nintendo.Controller.NesPad | Nintendo NES Controller (NES-004) | `nintendo-entertainment-system.png` réutilisé, huit zones reliées aux commandes NES. |
| Emulation.Nintendo.Controller.NesDogbonePad | Nintendo NES Controller (NES-039) | `nintendo-nes-dogbone.png` : croix, B/A, Select et Start raccordés ; halos à vérifier. |
| Emulation.Nintendo.Controller.FamicomPad1 | Nintendo Famicom Controller I | `famicom-controller-i.png` réutilisé, huit zones reliées aux commandes NES/Famicom. |
| Emulation.Nintendo.Controller.FamicomPad2 | Nintendo Famicom Controller II | À créer / vérifier |
| Emulation.Nintendo.Controller.AvFamicomPad | Nintendo AV Famicom Controller (HVC-102) | À créer / vérifier |
| Emulation.Nintendo.Controller.NesAdvantage | Nintendo NES Advantage (NES-026) | À créer / vérifier |
| Emulation.Nintendo.Controller.NesMax | Nintendo NES Max (NES-027) | À créer / vérifier |
| Emulation.Nintendo.Controller.NesZapper | Nintendo NES Zapper (NES-005) | À créer / vérifier |
| Emulation.Nintendo.Controller.FamicomLightGun | Nintendo Famicom Light Gun (HVC-005) | À créer / vérifier |
| Emulation.Nintendo.Controller.NesPowerPad | Nintendo Power Pad / Family Fun Fitness Mat (NES-028) | À créer / vérifier |
| Emulation.Nintendo.Controller.NesRob | Nintendo R.O.B. (NES-012) | À créer / vérifier |
| Emulation.Nintendo.Controller.FamicomRobot | Nintendo Famicom Robot (HVC-012) | À créer / vérifier |
| Emulation.Nintendo.Controller.RobGyroSet | Nintendo R.O.B. Gyro Set | À créer / vérifier |
| Emulation.Nintendo.Controller.RobBlockSet | Nintendo R.O.B. Block Set | À créer / vérifier |
| Emulation.Nintendo.Controller.NesFourScore | Nintendo NES Four Score (NES-034) | À créer / vérifier |
| Emulation.Nintendo.Controller.NesSatellite | Nintendo NES Satellite (NES-032) | À créer / vérifier |
| Emulation.Nintendo.Controller.NesSatelliteReceiver | Nintendo NES Satellite Receiver (NES-033) | À créer / vérifier |
| Emulation.Nintendo.Controller.FamilyBasicKeyboard | Nintendo Family BASIC Keyboard (HVC-007) | À créer / vérifier |
| Emulation.Nintendo.Controller.FamilyBasicDataRecorder | Nintendo Family BASIC Data Recorder (HVC-008) | À créer / vérifier |
| Emulation.Nintendo.Controller.Famicom3dScope | Nintendo Famicom 3D System Scope (HVC-031) | À créer / vérifier |
| Emulation.Nintendo.Controller.Famicom3dAdapter | Nintendo Famicom 3D System Adapter (HVC-032) | À créer / vérifier |
| Emulation.Nintendo.Controller.FamicomNetworkSystem | Nintendo Famicom Network System (HVC-050) | À créer / vérifier |
| Emulation.Nintendo.Controller.FamicomNetworkPad | Nintendo Famicom Network Controller (HVC-051) | À créer / vérifier |
| Emulation.Nintendo.Controller.FamicomDiskDrive | Nintendo Famicom Disk System Disk Drive (HVC-022) | À créer / vérifier |
| Emulation.Nintendo.Controller.FamicomDiskRamAdapter | Nintendo Famicom Disk System RAM Adapter (HVC-023) | À créer / vérifier |
| Emulation.Nintendo.Controller.SuperNesPad | Nintendo Super NES Controller (SNS-005) | `super-nes-controller.png` créé, transparent, douze zones reliées aux commandes SNES. |
| Emulation.Nintendo.Controller.SuperFamicomPad | Nintendo Super Famicom Controller (SHVC-005) | `super-nintendo.png` réutilisé, douze zones reliées aux commandes SNES. |
| Emulation.Nintendo.Controller.SuperNesRedesignedPad | Nintendo Super NES Controller (SNS-102) | À créer / vérifier |
| Emulation.Nintendo.Controller.SuperNesMouse | Nintendo Super NES Mouse | À créer / vérifier |
| Emulation.Nintendo.Controller.SuperScope | Nintendo Super Scope / Nintendo Scope | À créer / vérifier |
| Emulation.Nintendo.Controller.SuperScopeReceiver | Nintendo Super Scope Receiver | À créer / vérifier |
| Emulation.Nintendo.Controller.SuperGameBoy | Nintendo Super Game Boy | À créer / vérifier |
| Emulation.Nintendo.Controller.SuperGameBoy2 | Nintendo Super Game Boy 2 | À créer / vérifier |
| Emulation.Nintendo.Controller.Satellaview | Nintendo Satellaview | À créer / vérifier |
| Emulation.Nintendo.Controller.SatellaviewMemoryPack | Nintendo Satellaview 8M Memory Pack | À créer / vérifier |
| Emulation.Nintendo.Controller.VirtualBoyPad | Nintendo Virtual Boy Controller | À créer / vérifier |
| Emulation.Nintendo.Controller.VirtualBoyBatteryBox | Nintendo Virtual Boy Battery Box | À créer / vérifier |
| Emulation.Nintendo.Controller.VirtualBoyAcAdapterTap | Nintendo Virtual Boy AC Adapter Tap | À créer / vérifier |
| Emulation.Nintendo.Controller.Nintendo64Pad | Nintendo 64 Controller (NUS-005) | `nintendo-64.png` existant : croix, stick, A/B, quatre boutons C, L/R et Start raccordés ; Z est derrière la manette et n'a donc pas de halo sur la vue de face. Provenance de l'image à documenter ; halos à essayer dans l'interface. |
| Emulation.Nintendo.Controller.Nintendo64ControllerPak | Nintendo 64 Controller Pak (NUS-004) | À créer / vérifier |
| Emulation.Nintendo.Controller.Nintendo64RumblePak | Nintendo 64 Rumble Pak (NUS-013) | À créer / vérifier |
| Emulation.Nintendo.Controller.Nintendo64TransferPak | Nintendo 64 Transfer Pak (NUS-019) | À créer / vérifier |
| Emulation.Nintendo.Controller.Nintendo64ExpansionPak | Nintendo 64 Expansion Pak (NUS-007) | À créer / vérifier |
| Emulation.Nintendo.Controller.Nintendo64JumperPak | Nintendo 64 Jumper Pak (NUS-008) | À créer / vérifier |
| Emulation.Nintendo.Controller.Nintendo64VoiceRecognitionUnit | Nintendo 64 Voice Recognition Unit (NUS-020) | À créer / vérifier |
| Emulation.Nintendo.Controller.Nintendo64Microphone | Nintendo 64 Microphone (NUS-021) | À créer / vérifier |
| Emulation.Nintendo.Controller.Nintendo64Mouse | Nintendo 64 Mouse (NUS-017) | À créer / vérifier |
| Emulation.Nintendo.Controller.Nintendo64DiskDrive | Nintendo 64DD (NUS-010) | À créer / vérifier |
| Emulation.Nintendo.Controller.Nintendo64CaptureCassette | Nintendo 64 Capture Cassette (NUS-028) | À créer / vérifier |
| Emulation.Nintendo.Controller.Nintendo64Modem | Nintendo 64 Modem (NUS-029) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoy | Nintendo Game Boy (DMG-01) | `nintendo-game-boy.png` : croix, B/A, Select et Start raccordés ; halos à vérifier. |
| Emulation.Nintendo.Controller.GameBoyPocket | Nintendo Game Boy pocket (MGB-001) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoyLight | Nintendo Game Boy Light (MGB-101) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoyColor | Nintendo Game Boy Color (CGB-001) | `nintendo-game-boy-color.png` : croix, B/A, Select et Start raccordés ; contour à nettoyer et halos à vérifier. |
| Emulation.Nintendo.Controller.GameBoyGameLinkCable | Nintendo Game Boy Game Link Cable (DMG-04) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoyFourPlayerAdapter | Nintendo Game Boy Four Player Adapter (DMG-07) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoyUniversalLinkAdapter | Nintendo Universal Game Link Adapter (DMG-14) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoyPocketLinkAdapter | Nintendo Game Boy pocket Game Link Adapter (MGB-004) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoyPocketLinkCable | Nintendo Game Boy pocket Game Link Cable (MGB-008) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoyUniversalLinkCable | Nintendo Universal Game Link Cable (MGB-010) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoyColorLinkCable | Nintendo Game Boy Color Game Link Cable (CGB-003) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoyCamera | Nintendo Game Boy Camera / Pocket Camera (MGB-006) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoyPrinter | Nintendo Game Boy Printer / Pocket Printer (MGB-007) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoyMobileAdapter | Nintendo Mobile Adapter GB | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoyAdvance | Nintendo Game Boy Advance (AGB-001) | `nintendo-game-boy-advance.png` : croix, B/A, Select et Start raccordés ; L/R présents dans la liste, cachés sur l'image de face. |
| Emulation.Nintendo.Controller.GameBoyAdvanceSp | Nintendo Game Boy Advance SP (AGS-001) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoyAdvanceSpBacklit | Nintendo Game Boy Advance SP (AGS-101) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoyMicro | Nintendo Game Boy micro (OXY-001) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoyAdvanceLinkCable | Nintendo Game Boy Advance Game Link Cable (AGB-005) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoyAdvanceWirelessAdapter | Nintendo Game Boy Advance Wireless Adapter (AGB-015) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoyMicroLinkCable | Nintendo Game Boy micro Game Link Cable (OXY-008) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoyMicroLinkAdapter | Nintendo Game Boy micro Game Link Adapter (OXY-009) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoyMicroWirelessAdapter | Nintendo Game Boy micro Wireless Adapter (OXY-004) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoyAdvanceEReader | Nintendo Card e-Reader (AGB-010) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoyAdvanceEReaderPlus | Nintendo e-Reader / Card e-Reader+ (AGB-014) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoyAdvanceSpHeadphoneAdapter | Nintendo Game Boy Advance SP Headphone Adapter (AGS-004) | À créer / vérifier |
| Emulation.Nintendo.Controller.PlayYan | Nintendo PLAY-YAN | À créer / vérifier |
| Emulation.Nintendo.Controller.PlayYanMicro | Nintendo PLAY-YAN micro | À créer / vérifier |
| Emulation.Nintendo.Controller.NintendoMp3Player | Nintendo MP3 Player | À créer / vérifier |
| Emulation.Nintendo.Controller.NintendoDs | Nintendo DS (NTR-001) | À créer / vérifier |
| Emulation.Nintendo.Controller.NintendoDsLite | Nintendo DS Lite (USG-001) | À créer / vérifier |
| Emulation.Nintendo.Controller.NintendoDsi | Nintendo DSi (TWL-001) | À créer / vérifier |
| Emulation.Nintendo.Controller.NintendoDsiXl | Nintendo DSi XL / DSi LL (UTL-001) | À créer / vérifier |
| Emulation.Nintendo.Controller.NintendoDsStylus | Nintendo DS Stylus (NTR-004) | À créer / vérifier |
| Emulation.Nintendo.Controller.NintendoDsLiteStylus | Nintendo DS Lite Stylus (USG-004) | À créer / vérifier |
| Emulation.Nintendo.Controller.NintendoDsiStylus | Nintendo DSi Stylus (TWL-004) | À créer / vérifier |
| Emulation.Nintendo.Controller.NintendoDsiXlStylus | Nintendo DSi XL Stylus (UTL-004) | À créer / vérifier |
| Emulation.Nintendo.Controller.NintendoDsTouchStrap | Nintendo DS Touch Strap (NTR-009) | À créer / vérifier |
| Emulation.Nintendo.Controller.NintendoDsRumblePak | Nintendo DS Rumble Pak (NTR-008) | À créer / vérifier |
| Emulation.Nintendo.Controller.NintendoDsLiteRumblePak | Nintendo DS Lite Rumble Pak (USG-006) | À créer / vérifier |
| Emulation.Nintendo.Controller.NintendoDsMemoryExpansion | Nintendo DS Memory Expansion Pak (NTR-011) | À créer / vérifier |
| Emulation.Nintendo.Controller.NintendoDsLiteMemoryExpansion | Nintendo DS Lite Memory Expansion Pak (USG-007) | À créer / vérifier |
| Emulation.Nintendo.Controller.NintendoDsSlideController | Nintendo DS Slide Controller (NTR-012) | À créer / vérifier |
| Emulation.Nintendo.Controller.NintendoDsFaceningScan | Nintendo DS Facening Scan (NTR-014) | À créer / vérifier |
| Emulation.Nintendo.Controller.NintendoDsTv | Nintendo DS TV (NTR-016) | À créer / vérifier |
| Emulation.Nintendo.Controller.NintendoDsTvAntenna | Nintendo DS TV External Antenna (NTR-025) | À créer / vérifier |
| Emulation.Nintendo.Controller.NintendoDsHeadset | Nintendo DS Headset (NTR-019) | À créer / vérifier |
| Emulation.Nintendo.Controller.NintendoDsBimojiFude | Nintendo DS Bimoji Fude (NTR-023) | À créer / vérifier |
| Emulation.Nintendo.Controller.NintendoDsStylusWithStrap | Nintendo DS Stylus with Strap (NTR-024) | À créer / vérifier |
| Emulation.Nintendo.Controller.NintendoWifiUsbConnector | Nintendo Wi-Fi USB Connector (NTR-010) | À créer / vérifier |
| Emulation.Nintendo.Controller.NintendoWifiNetworkAdapter | Nintendo Wi-Fi Network Adapter (WAP-001) | À créer / vérifier |
| Emulation.Nintendo.Controller.NintendoWirelessKeyboard | Nintendo Wireless Keyboard (NTR-034) | À créer / vérifier |
| Emulation.Nintendo.Controller.NintendoDsActivityMeter | Nintendo DS Activity Meter | À créer / vérifier |
| Emulation.Nintendo.Controller.Pokewalker | Nintendo Pokéwalker (NTR-032) | À créer / vérifier |
| Emulation.Nintendo.Controller.Nintendo3ds | Nintendo 3DS (CTR-001) | À créer / vérifier |
| Emulation.Nintendo.Controller.Nintendo3dsXl | Nintendo 3DS XL / 3DS LL (SPR-001) | À créer / vérifier |
| Emulation.Nintendo.Controller.Nintendo2ds | Nintendo 2DS (FTR-001) | À créer / vérifier |
| Emulation.Nintendo.Controller.NewNintendo3ds | New Nintendo 3DS (KTR-001) | À créer / vérifier |
| Emulation.Nintendo.Controller.NewNintendo3dsXl | New Nintendo 3DS XL / New 3DS LL (RED-001) | À créer / vérifier |
| Emulation.Nintendo.Controller.NewNintendo2dsXl | New Nintendo 2DS XL / New 2DS LL (JAN-001) | À créer / vérifier |
| Emulation.Nintendo.Controller.Nintendo3dsCirclePadPro | Nintendo 3DS Circle Pad Pro | À créer / vérifier |
| Emulation.Nintendo.Controller.Nintendo3dsCirclePadProXl | Nintendo 3DS Circle Pad Pro XL | À créer / vérifier |
| Emulation.Nintendo.Controller.Nintendo3dsNfcReaderWriter | Nintendo NFC Reader/Writer | À créer / vérifier |
| Emulation.Nintendo.Controller.Nintendo3dsStylus | Nintendo 3DS Stylus | À créer / vérifier |
| Emulation.Nintendo.Controller.Nintendo3dsXlStylus | Nintendo 3DS XL Stylus | À créer / vérifier |
| Emulation.Nintendo.Controller.Nintendo2dsStylus | Nintendo 2DS Stylus | À créer / vérifier |
| Emulation.Nintendo.Controller.NewNintendo3dsStylus | New Nintendo 3DS Stylus | À créer / vérifier |
| Emulation.Nintendo.Controller.NewNintendo3dsXlStylus | New Nintendo 3DS XL Stylus | À créer / vérifier |
| Emulation.Nintendo.Controller.NewNintendo2dsXlStylus | New Nintendo 2DS XL Stylus | À créer / vérifier |
| Emulation.Nintendo.Controller.GameCubePad | Nintendo GameCube Controller (DOL-003) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameCubeWaveBird | Nintendo GameCube WaveBird Wireless Controller (DOL-004) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameCubeWaveBirdReceiver | Nintendo GameCube WaveBird Receiver (DOL-005) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameCubeDkBongos | Nintendo GameCube DK Bongos (DOL-021) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameCubeMicrophone | Nintendo GameCube Microphone (DOL-022) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameCubeMicrophoneHolder | Nintendo GameCube Microphone Holder (DOL-025) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameCubeGbaCable | Nintendo GameCube Game Boy Advance Cable (DOL-011) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameCubeGameBoyPlayer | Nintendo GameCube Game Boy Player (DOL-017) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameCubeMemoryCard59 | Nintendo GameCube Memory Card 59 (DOL-008) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameCubeMemoryCard251 | Nintendo GameCube Memory Card 251 (DOL-014) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameCubeMemoryCard1019 | Nintendo GameCube Memory Card 1019 (DOL-020) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameCubeSdCardAdapter | Nintendo GameCube SD Card Adapter (DOL-019) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameCubeModemAdapter | Nintendo GameCube Modem Adapter (DOL-012) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameCubeBroadbandAdapter | Nintendo GameCube Broadband Adapter (DOL-015) | À créer / vérifier |
| Emulation.Nintendo.Controller.WiiRemote | Nintendo Wii Remote | À créer / vérifier |
| Emulation.Nintendo.Controller.WiiRemotePlus | Nintendo Wii Remote Plus | À créer / vérifier |
| Emulation.Nintendo.Controller.WiiNunchuk | Nintendo Wii Nunchuk | À créer / vérifier |
| Emulation.Nintendo.Controller.WiiClassicController | Nintendo Wii Classic Controller | À créer / vérifier |
| Emulation.Nintendo.Controller.WiiClassicControllerPro | Nintendo Wii Classic Controller Pro | À créer / vérifier |
| Emulation.Nintendo.Controller.WiiSuperFamicomClassicController | Nintendo Wii Super Famicom Classic Controller (Club Nintendo) | À créer / vérifier |
| Emulation.Nintendo.Controller.WiiMotionPlus | Nintendo Wii MotionPlus | À créer / vérifier |
| Emulation.Nintendo.Controller.WiiBalanceBoard | Nintendo Wii Balance Board | À créer / vérifier |
| Emulation.Nintendo.Controller.WiiZapper | Nintendo Wii Zapper | À créer / vérifier |
| Emulation.Nintendo.Controller.WiiWheel | Nintendo Wii Wheel | À créer / vérifier |
| Emulation.Nintendo.Controller.WiiSpeak | Nintendo Wii Speak | À créer / vérifier |
| Emulation.Nintendo.Controller.WiiSensorBar | Nintendo Wii Sensor Bar | À créer / vérifier |
| Emulation.Nintendo.Controller.WiiLanAdapter | Nintendo Wii LAN Adapter | À créer / vérifier |
| Emulation.Nintendo.Controller.WiiUGamePad | Nintendo Wii U GamePad | À créer / vérifier |
| Emulation.Nintendo.Controller.WiiUProController | Nintendo Wii U Pro Controller | À créer / vérifier |
| Emulation.Nintendo.Controller.WiiUMicrophone | Nintendo Wii U Microphone | À créer / vérifier |
| Emulation.Nintendo.Controller.WiiUGameCubeAdapter | Nintendo GameCube Controller Adapter for Wii U | À créer / vérifier |
| Emulation.Nintendo.Controller.GameCubeSmashBrosPad | Nintendo GameCube Controller Super Smash Bros. Edition | À créer / vérifier |
| Emulation.Nintendo.Controller.WiiUFitMeter | Nintendo Wii U Fit Meter | À créer / vérifier |
| Emulation.Nintendo.Controller.WiiUGamePadStylus | Nintendo Wii U GamePad Stylus | À créer / vérifier |
| Emulation.Nintendo.Controller.WiiUGamePadLargeStylus | Nintendo Wii U GamePad Stylus (Large) | À créer / vérifier |
| Emulation.Nintendo.Controller.Switch | Nintendo Switch | À créer / vérifier |
| Emulation.Nintendo.Controller.SwitchLite | Nintendo Switch Lite | À créer / vérifier |
| Emulation.Nintendo.Controller.SwitchOled | Nintendo Switch - OLED Model | À créer / vérifier |
| Emulation.Nintendo.Controller.SwitchJoyConLeft | Nintendo Switch Joy-Con (L) | À créer / vérifier |
| Emulation.Nintendo.Controller.SwitchJoyConRight | Nintendo Switch Joy-Con (R) | À créer / vérifier |
| Emulation.Nintendo.Controller.SwitchJoyConPair | Nintendo Switch Joy-Con (L/R) | À créer / vérifier |
| Emulation.Nintendo.Controller.SwitchProController | Nintendo Switch Pro Controller | À créer / vérifier |
| Emulation.Nintendo.Controller.SwitchJoyConGrip | Nintendo Switch Joy-Con Grip | À créer / vérifier |
| Emulation.Nintendo.Controller.SwitchJoyConChargingGrip | Nintendo Switch Joy-Con Charging Grip | À créer / vérifier |
| Emulation.Nintendo.Controller.SwitchJoyConStrap | Nintendo Switch Joy-Con Strap | À créer / vérifier |
| Emulation.Nintendo.Controller.SwitchJoyConWheel | Nintendo Switch Joy-Con Wheel | À créer / vérifier |
| Emulation.Nintendo.Controller.SwitchJoyConAaBatteryPack | Nintendo Switch Joy-Con AA Battery Pack | À créer / vérifier |
| Emulation.Nintendo.Controller.SwitchGameCubeAdapter | Nintendo GameCube Controller Adapter for Nintendo Switch | À créer / vérifier |
| Emulation.Nintendo.Controller.SwitchPokeBallPlus | Nintendo Poké Ball Plus | À créer / vérifier |
| Emulation.Nintendo.Controller.SwitchNesPad | Nintendo NES Controller (Nintendo Switch Online) | À créer / vérifier |
| Emulation.Nintendo.Controller.SwitchFamicomPad1 | Nintendo Famicom Controller I (Nintendo Switch Online) | À créer / vérifier |
| Emulation.Nintendo.Controller.SwitchFamicomPad2 | Nintendo Famicom Controller II (Nintendo Switch Online) | À créer / vérifier |
| Emulation.Nintendo.Controller.SwitchSuperNesPad | Nintendo Super NES Controller (Nintendo Switch Online) | À créer / vérifier |
| Emulation.Nintendo.Controller.SwitchSuperFamicomPad | Nintendo Super Famicom Controller (Nintendo Switch Online) | À créer / vérifier |
| Emulation.Nintendo.Controller.SwitchNintendo64Pad | Nintendo 64 Controller (Nintendo Switch Online) | À créer / vérifier |
| Emulation.Nintendo.Controller.SwitchUsbMicrophone | Nintendo USB Microphone | À créer / vérifier |
| Emulation.Nintendo.Controller.SwitchRingCon | Nintendo Ring-Con | À créer / vérifier |
| Emulation.Nintendo.Controller.SwitchLegStrap | Nintendo Leg Strap | À créer / vérifier |
| Emulation.Nintendo.Controller.SwitchJoyConChargingStand | Nintendo Switch Joy-Con Charging Stand (two-way) | À créer / vérifier |
| Emulation.Nintendo.Controller.LaboRcCar | Nintendo Labo Toy-Con RC Car | À créer / vérifier |
| Emulation.Nintendo.Controller.LaboFishingRod | Nintendo Labo Toy-Con Fishing Rod | À créer / vérifier |
| Emulation.Nintendo.Controller.LaboHouse | Nintendo Labo Toy-Con House | À créer / vérifier |
| Emulation.Nintendo.Controller.LaboMotorbike | Nintendo Labo Toy-Con Motorbike | À créer / vérifier |
| Emulation.Nintendo.Controller.LaboPiano | Nintendo Labo Toy-Con Piano | À créer / vérifier |
| Emulation.Nintendo.Controller.LaboRobot | Nintendo Labo Toy-Con Robot | À créer / vérifier |
| Emulation.Nintendo.Controller.LaboCar | Nintendo Labo Toy-Con Car | À créer / vérifier |
| Emulation.Nintendo.Controller.LaboPedal | Nintendo Labo Toy-Con Pedal | À créer / vérifier |
| Emulation.Nintendo.Controller.LaboPlane | Nintendo Labo Toy-Con Plane | À créer / vérifier |
| Emulation.Nintendo.Controller.LaboSubmarine | Nintendo Labo Toy-Con Submarine | À créer / vérifier |
| Emulation.Nintendo.Controller.LaboKey | Nintendo Labo Toy-Con Key | À créer / vérifier |
| Emulation.Nintendo.Controller.LaboVrGoggles | Nintendo Labo Toy-Con VR Goggles | À créer / vérifier |
| Emulation.Nintendo.Controller.LaboBlaster | Nintendo Labo Toy-Con Blaster | À créer / vérifier |
| Emulation.Nintendo.Controller.LaboCamera | Nintendo Labo Toy-Con Camera | À créer / vérifier |
| Emulation.Nintendo.Controller.LaboBird | Nintendo Labo Toy-Con Bird | À créer / vérifier |
| Emulation.Nintendo.Controller.LaboWindPedal | Nintendo Labo Toy-Con Wind Pedal | À créer / vérifier |
| Emulation.Nintendo.Controller.LaboElephant | Nintendo Labo Toy-Con Elephant | À créer / vérifier |
| Emulation.Nintendo.Controller.SwitchVirtualBoy | Nintendo Virtual Boy for Nintendo Switch 2 / Nintendo Switch | À créer / vérifier |
| Emulation.Nintendo.Controller.SwitchVirtualBoyCardboard | Nintendo Virtual Boy (Cardboard Model) | À créer / vérifier |
| Emulation.Nintendo.Controller.AmiiboFigure | Nintendo amiibo Figure | À créer / vérifier |
| Emulation.Nintendo.Controller.AmiiboCard | Nintendo amiibo Card | À créer / vérifier |
| Emulation.Nintendo.Controller.AmiiboYarnYoshi | Nintendo Yarn Yoshi amiibo | À créer / vérifier |

## Sega (34 clés)

Premier lot : les trois images de face `master-system.png`, `mega-drive-3.png` et `mega-drive-6.png` existaient déjà. Les zones ont été ajustées à leurs boutons visibles. Les commandes Master System 1/2 et Mega Drive A/B/C, X/Y/Z, Start et Mode utilisent les entrées RetroPad documentées par [Genesis Plus GX](https://docs.libretro.com/library/genesis_plus_gx/#joypad) ; le cœur reçoit désormais, lorsque disponible, le type de manette trois ou six boutons explicitement sélectionné. Les 31 autres clés Sega restent à traiter, ainsi que la vérification interactive des halos.

Deuxième lot : `dreamcast.png` représente déjà la manette de face sur fond transparent. La croix, A/B/X/Y, Start et les deux gâchettes disposent maintenant de zones distinctes, reliées aux identifiants RetroPad indiqués dans les [descripteurs du cœur Flycast](https://github.com/flyinghead/flycast/blob/master/shell/libretro/libretro.cpp). Le stick analogique reste transmis par les axes du contrôleur physique, mais la liste actuelle ne permet pas de lui attribuer une touche ou un halo directionnel. L'essai interactif des zones reste à faire. Trente autres clés Sega demeurent sans profil spécifique.

Build Debug du 4 octobre 2026 après le profil Dreamcast : `scripts\local-building.cmd --building=debug --modules=A` terminé avec code 0 ; `gwgui.exe` et les huit DLL de modules vérifiés présents sous `build/Debug/GW GUI`. L'essai interactif reste à faire.

Quatre tests Sega existants de `ConsoleFamilyModuleTests` ciblant la description des profils, leurs ressources, leur persistance et la sélection des périphériques Genesis Plus GX réussissent (0 échec, 4 succès). Ils ne contrôlent ni le placement visuel des halos ni toutes les nouvelles correspondances de boutons.

Troisième lot : l'image de face `saturn.png` (599 × 399, coin alpha 0) représente la manette Saturn standard. Ses six boutons, Start, L/R et la croix ont des zones distinctes reliées aux [descripteurs d'entrée du cœur Yabause](https://github.com/libretro/yabause/blob/master/yabause/src/libretro/libretro.c). Les zones de face Sega utilisent désormais des identifiants A/B/C/X/Y/Z explicites ; la manette Saturn 3D n'emprunte plus l'image du modèle standard, car son stick analogique et sa forme exigent un profil propre. Il reste 29 clés Sega sans profil dédié et tous les halos doivent encore être vérifiés dans l'interface.

Quatrième lot : le profil Saturn 3D utilise `saturn-3d-control-pad.png` (1172 × 1342, coin alpha 0), créé à partir de la [photographie CC0 d'Evan-Amos](https://commons.wikimedia.org/wiki/File:Sega-Saturn-3D-Controller.jpg) conservée dans `docs/tasks/emulation/saturn-3d-controller-reference.jpg`. La croix, les six boutons et Start ont des zones distinctes raccordées aux commandes numériques lues par [Yabause](https://github.com/libretro/yabause/blob/master/yabause/src/libretro/libretro.c). Le cœur reçoit maintenant `RETRO_DEVICE_ANALOG` pour ce modèle, selon ses descripteurs de périphériques et la [définition Libretro](https://github.com/libretro/libretro-common/blob/master/include/libretro.h) ; les axes du stick viennent du contrôleur physique. La liste ne permet pas encore l'affectation individuelle des axes ni un halo directionnel du stick ; les gâchettes ne sont pas localisables sur cette vue de face. Il reste 28 clés Sega sans profil dédié ; le placement et les halos attendent un essai interactif.

Cinquième lot : `game-gear-controls.png` (1532 × 1026, coin alpha 0) représente la portable de face, d'après la [photographie CC0 d'Evan-Amos](https://commons.wikimedia.org/wiki/File:Game-Gear-Handheld.jpg) conservée dans `docs/tasks/emulation/game-gear-reference.jpg`. La croix, les boutons 1/2 et Start disposent de zones et commandes distinctes. Le modèle présente un seul port de commandes intégrées. [Genesis Plus GX](https://github.com/ekeeke/Genesis-Plus-GX/blob/master/libretro/libretro.c) reçoit explicitement son périphérique deux boutons ; ses entrées RetroPad B, A et Start alimentent les commandes Game Gear. Il reste 27 clés Sega sans profil dédié ; les halos attendent un essai interactif.

Sixième lot : `sega-control-stick.png` (1476 × 1065, coin alpha 0) est une vue de dessus du Control Stick Master System fondée sur la [photographie de référence](https://www.videogameobsession.com/videogame/sms/hardware/). Son levier et ses deux boutons physiques disposent de zones distinctes ; les boutons 1 et 2 sont reliés aux entrées RetroPad B et A. Genesis Plus GX reçoit le périphérique `MS Joypad 2 Button` pour ce modèle. Le bouton « 1 START » du boîtier est le bouton 1, pas une troisième commande. La photo externe utilisée comme guide a été retirée du dépôt. Il reste 26 clés Sega sans profil dédié ; le placement des halos attend un essai interactif.

Septième lot : `sega-arcade-power-stick-3.png` (1586 × 992, coin alpha 0) et `sega-arcade-power-stick-6.png` (1536 × 1024, coin alpha 0) sont des vues de dessus transparentes guidées par des [photographies du modèle trois boutons](https://quedejapon.com/products/sega-megadrive-arcade-power-stick-controller-3-button-rapid-fire-japan) et du [modèle 6B](https://www.pricecharting.com/game/jp-sega-mega-drive/arcade-power-stick-6b). Chaque levier et les boutons A/B/C ont leurs zones ; le 6B ajoute X/Y/Z et Mode. Start a sa propre zone sur les deux. Les commandes suivent les entrées RetroPad des manettes Mega Drive trois et six boutons, et Genesis Plus GX reçoit le périphérique correspondant. Les interrupteurs Turbo restent visibles sans affectation indépendante, car ils modifient le comportement des boutons physiques. Les photographies externes temporaires ont été retirées. Il reste 24 clés Sega sans profil dédié ; le placement des halos attend un essai interactif.

Build Debug du 4 octobre 2026 après les deux Arcade Power Stick : `scripts\local-building.cmd --building=debug --modules=A` terminé avec code 0 ; `build/Debug/GW GUI/gwgui.exe` et les huit DLL sous `build/Debug/GW GUI/Modules/<marque>/` vérifiés présents. Les halos restent à essayer dans l'interface.

Build Debug du 4 octobre 2026 après le Control Stick : `scripts\local-building.cmd --building=debug --modules=A` terminé avec code 0 ; `build/Debug/GW GUI/gwgui.exe` et les huit DLL de `build/Debug/GW GUI/Modules/` vérifiés présents. Les halos restent à essayer dans l'interface.

Build Debug du 4 octobre 2026 après le profil Game Gear : `scripts\local-building.cmd --building=debug --modules=A` terminé avec code 0 ; `gwgui.exe` et les huit DLL sous `build/Debug/GW GUI/Modules/<module>/` sont présents. Le PNG est inclus dans les ressources WPF via `Assets\Controllers\*.png`. Aucun essai interactif des halos n'a encore été effectué.

Build Debug du 4 octobre 2026 après le profil Saturn 3D : `scripts\local-building.cmd --building=debug --modules=A` terminé avec code 0. `build/Debug/GW GUI/gwgui.exe` et les huit DLL sous `build/Debug/GW GUI/Modules/<module>/` sont présents. Les halos n'ont pas encore été essayés dans l'interface.

Build Debug du 4 octobre 2026 après le profil Saturn : `scripts\local-building.cmd --building=debug --modules=A` terminé avec code 0 ; l'exécutable `gwgui.exe` et les DLL des huit modules sont présents sous `build/Debug/GW GUI`. Aucun essai interactif des zones et halos n'a encore été réalisé.

Compilation Debug du 4 octobre 2026 après ce lot : `scripts\local-building.cmd --building=debug --modules=A` terminé avec code 0. `build/Debug/GW GUI/gwgui.exe` et les DLL des huit modules ont été vérifiés présents. Aucun essai interactif des halos n'a encore été effectué.

| Clé | Nom invariant / libellé | Image de face et zones |
| --- | --- | --- |
| Emulation.Sega.Controller.SegaControlStick | Control Stick | `sega-control-stick.png` : levier et boutons 1/2 raccordés au périphérique deux boutons ; placement interactif à vérifier. |
| Emulation.Sega.Controller.SegaLightPhaser | Light Phaser | À créer / vérifier |
| Emulation.Sega.Controller.SegaMegaMouse | Mega Mouse | À créer / vérifier |
| Emulation.Sega.Controller.SegaMenacer | Menacer | À créer / vérifier |
| Emulation.Sega.Controller.SegaSportsPad | Sports Pad | À créer / vérifier |
| Emulation.Sega.Controller.SegaPaddleControl | Paddle Control | À créer / vérifier |
| Emulation.Sega.Controller.SegaHandleController | Handle Controller | À créer / vérifier |
| Emulation.Sega.Controller.SegaArcadePowerStick | Arcade Power Stick | `sega-arcade-power-stick-3.png` : levier, A/B/C et Start raccordés au périphérique trois boutons ; halos à vérifier dans l'interface. |
| Emulation.Sega.Controller.SegaXe1Ap | XE-1 AP | À créer / vérifier |
| Emulation.Sega.Controller.SegaActivator | Activator | À créer / vérifier |
| Emulation.Sega.Controller.SegaSaturnThreeDControlPad | Saturn 3D Control Pad | `saturn-3d-control-pad.png` : croix, A/B/C, X/Y/Z et Start raccordés ; stick analogique visible mais axes sans liaison de halo ; placement interactif à vérifier. |
| Emulation.Sega.Controller.SegaSaturnVirtuaGun | Virtua Gun | À créer / vérifier |
| Emulation.Sega.Controller.SegaSaturnShuttleMouse | Shuttle Mouse | À créer / vérifier |
| Emulation.Sega.Controller.SegaSaturnMissionStick | Mission Stick | À créer / vérifier |
| Emulation.Sega.Controller.SegaSaturnArcadeRacer | Arcade Racer | À créer / vérifier |
| Emulation.Sega.Controller.SegaSaturnTwinStick | Twin Stick | À créer / vérifier |
| Emulation.Sega.Controller.SegaSaturnVirtuaStick | Virtua Stick | À créer / vérifier |
| Emulation.Sega.Controller.SegaSg1000Joystick | SG-1000 joystick | À créer / vérifier |
| Emulation.Sega.Controller.SegaSg1000IiJoypad | SG-1000 II gamepad | À créer / vérifier |
| Emulation.Sega.Controller.SegaSc3000Keyboard | SC-3000 keyboard | À créer / vérifier |
| Emulation.Sega.Controller.SegaMasterSystemController | Master System controller | `master-system.png` : croix, 1 et 2 raccordés au profil sélectionnable ; placement interactif à vérifier. |
| Emulation.Sega.Controller.SegaGameGearController | Game Gear built-in controls | `game-gear-controls.png` : croix, 1/2 et Start raccordés sur le port unique ; placement interactif à vérifier. |
| Emulation.Sega.Controller.SegaMegaDriveThreeButton | Mega Drive 3-button controller | `mega-drive-3.png` : croix, A/B/C et Start raccordés ; placement interactif à vérifier. |
| Emulation.Sega.Controller.SegaMegaDriveSixButton | Mega Drive 6-button controller | `mega-drive-6.png` : croix, A/B/C, X/Y/Z, Start et Mode raccordés ; placement interactif à vérifier. |
| Emulation.Sega.Controller.SegaArcadePowerStickSixButton | Arcade Power Stick 6-button controller | `sega-arcade-power-stick-6.png` : levier, A/B/C, X/Y/Z, Start et Mode raccordés au périphérique six boutons ; halos à vérifier dans l'interface. |
| Emulation.Sega.Controller.SegaSaturnController | Saturn controller | `saturn.png` : croix, A/B/C, X/Y/Z, Start et L/R raccordés aux boutons de Yabause ; halos à vérifier dans l'interface. |
| Emulation.Sega.Controller.SegaDreamcastController | Dreamcast controller | `dreamcast.png` : croix, A/B/X/Y, Start, L/R raccordés aux boutons réellement lus par Flycast ; axes du stick non configurables dans la liste ; halos à vérifier dans l'interface. |
| Emulation.Sega.Controller.SegaDreamcastMouse | Dreamcast mouse | À créer / vérifier |
| Emulation.Sega.Controller.SegaDreamcastKeyboard | Dreamcast keyboard | À créer / vérifier |
| Emulation.Sega.Controller.SegaDreamcastLightGun | Dreamcast light gun | À créer / vérifier |
| Emulation.Sega.Controller.SegaDreamcastFishingController | Dreamcast fishing controller | À créer / vérifier |
| Emulation.Sega.Controller.SegaDreamcastArcadeStick | Dreamcast arcade stick | À créer / vérifier |
| Emulation.Sega.Controller.SegaDreamcastTwinStick | Dreamcast twin stick | À créer / vérifier |
| Emulation.Sega.Controller.SegaDreamcastMaracas | Dreamcast maracas | À créer / vérifier |
