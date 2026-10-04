# Inventaire des visuels de contrôleurs

Source : ressources 00-Base des cinq modules au 4 octobre 2026. Chaque ligne demande une image de face transparente et des zones exactes pour les commandes exposées. Les périphériques sans commande mappable doivent rester identifiables sans halo fictif.

Chaîne existante : InputFunctions.Visuals.cs dans Atari/Amiga → EmulationControllerVisualIds → ControllerArtworkCatalog (PNG et zones normalisées) → ControllerVisualizer.Artwork (halo, survol, clic).

Référence du premier visuel NEC PI-PD001 : photo « [PC Engine Controller.jpg](https://commons.wikimedia.org/wiki/File:PC_Engine_Controller.jpg) » de SACHEN, licence [CC BY 3.0](https://creativecommons.org/licenses/by/3.0/). La copie de travail est `docs/tasks/emulation/pc-engine-controller-reference.jpg`. Le PNG `src/GWGUI.App/Assets/Controllers/nec-pc-engine-pad.png` est une retouche de cette photo : fond retiré et transparence ajoutée ; forme, commandes et inscriptions conservées.

Référence du TurboPad HES-PAD-01 : photo « [NEC-TurboGrafx-16-Controller-FL.jpg](https://commons.wikimedia.org/wiki/File:NEC-TurboGrafx-16-Controller-FL.jpg) » de Evan-Amos, [domaine public](https://creativecommons.org/publicdomain/mark/1.0/). La copie de travail est `docs/tasks/emulation/turbografx-turbopad-reference.jpg`. Le PNG `src/GWGUI.App/Assets/Controllers/nec-turbografx-turbopad.png` redresse la manette et retire le fond blanc ; commandes et interrupteurs restent visibles.

Les visuels PI-PD6 et PI-PD8 sont des recréations faites à partir de la forme du PI-PD001 ci-dessus et de descriptions photographiques des variantes CoreGrafx. Les couleurs, les deux interrupteurs Turbo et le marquage du modèle ont été adaptés ; la photo PI-PD001 de SACHEN reste créditée. Les images ont été inspectées de face, leurs coins sont transparents, et leurs profils utilisent huit zones correspondant aux commandes déjà exposées. L'essai interactif des halos et la compilation du deuxième lot restent à faire.

## Nec (53 clés)

| Clé | Nom invariant / libellé | Image de face et zones |
| --- | --- | --- |
| Emulation.Nec.Controller.PcEnginePad | NEC PC Engine Pad (PI-PD001) | `nec-pc-engine-pad.png` : RGBA vérifié, profil branché, huit zones (croix ×4, II, I, Select, Run). Build Debug app + huit modules réussi le 4 octobre 2026 ; essai interactif des halos encore à faire. |
| Emulation.Nec.Controller.PcEngineTurboPad | NEC Turbo Pad (PI-PD002) | À créer / vérifier |
| Emulation.Nec.Controller.CoreGrafxTurboPad | NEC Turbo Pad (PI-PD6) | `nec-coregrafx-turbopad.png` : RGBA vérifié, profil et huit zones branchés ; essai interactif restant. |
| Emulation.Nec.Controller.CoreGrafxIITurboPad | NEC Turbo Pad (PI-PD8) | `nec-coregrafx-ii-turbopad.png` : RGBA vérifié, profil et huit zones branchés ; essai interactif restant. |
| Emulation.Nec.Controller.DuoRTurboPad | NEC Turbo Pad (PCE-TP2) | À créer / vérifier |
| Emulation.Nec.Controller.PcEngineTurboPadII | NEC Turbo Pad II (PI-PD5) | À créer / vérifier |
| Emulation.Nec.Controller.PcEngineTurboStick | NEC Turbo Stick (PI-PD4) | À créer / vérifier |
| Emulation.Nec.Controller.AvenuePad3 | NEC Avenue Pad 3 (NAPD-1001) | À créer / vérifier |
| Emulation.Nec.Controller.AvenuePad6 | NEC Avenue Pad 6 (NAPD-1002) | À créer / vérifier |
| Emulation.Nec.Controller.ArcadePad6 | NEC Arcade Pad 6 (PCE-TP1) | À créer / vérifier |
| Emulation.Nec.Controller.PcEngineMouse | NEC PC Engine Mouse (PI-PD10) | À créer / vérifier |
| Emulation.Nec.Controller.CordlessPad | NEC Cordless Pad (PI-PD12) | À créer / vérifier |
| Emulation.Nec.Controller.CordlessMultiTapSet | NEC Cordless Multi Tap Set (PI-PD11) | À créer / vérifier |
| Emulation.Nec.Controller.PcEngineMultiTap | NEC Multi Tap (PI-PD003) | À créer / vérifier |
| Emulation.Nec.Controller.VirtualCushion | NEC Virtual Cushion (PI-AD20) | À créer / vérifier |
| Emulation.Nec.Controller.TurboGrafxTurboPad | NEC TurboPad (HES-PAD-01) | `nec-turbografx-turbopad.png` : RGBA vérifié, profil branché, huit zones (croix ×4, II, I, Select, Run). Interrupteurs Turbo visibles, sans commande distincte dans la liste actuelle. Build et essai interactif après ce lot encore à faire. |
| Emulation.Nec.Controller.TurboGrafxTurboStick | NEC TurboStick (HES-STK-01) | À créer / vérifier |
| Emulation.Nec.Controller.TurboGrafxTurboTap | NEC TurboTap (HES-TAP-01) | À créer / vérifier |
| Emulation.Nec.Controller.DuoPad | DuoPad | À créer / vérifier |
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
| Emulation.Nec.Controller.PcFxPad | NEC PC-FX FX-PAD | À créer / vérifier |
| Emulation.Nec.Controller.PcFxMouse | NEC PC-FX FX-MOU | À créer / vérifier |
| Emulation.Nec.Controller.PcFxBackupMemory | NEC PC-FX FX-BMP | À créer / vérifier |
| Emulation.Nec.Controller.PcFxScsiAdapter | NEC PC-FX FX-SCSI | À créer / vérifier |
| Emulation.Nec.Controller.Mouse | Mouse | À créer / vérifier |
| Emulation.Nec.Controller.TurboExpressControls | TurboExpress / PC Engine GT built-in controls | À créer / vérifier |
| Emulation.Nec.Controller.PcEngineLtControls | PC Engine LT built-in controls | À créer / vérifier |
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

| Clé | Nom invariant / libellé | Image de face et zones |
| --- | --- | --- |
| Emulation.Microsoft.Controller.Joystick | Xbox controller | À créer / vérifier |
| Emulation.Microsoft.Controller.Keyboard | Xbox keyboard | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxDuke | Xbox Controller (Duke) | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxControllerS | Xbox Controller S | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxDvdMoviePlaybackKit | Xbox DVD Movie Playback Kit | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxCommunicator | Xbox Communicator | À créer / vérifier |
| Emulation.Microsoft.Controller.Xbox360Controller | Xbox 360 Controller | À créer / vérifier |
| Emulation.Microsoft.Controller.Xbox360WirelessController | Xbox 360 Wireless Controller | À créer / vérifier |
| Emulation.Microsoft.Controller.Xbox360WirelessControllerTransformingDPad | Xbox 360 Wireless Controller with Transforming D-Pad | À créer / vérifier |
| Emulation.Microsoft.Controller.Xbox360WirelessRacingWheel | Xbox 360 Wireless Racing Wheel | À créer / vérifier |
| Emulation.Microsoft.Controller.Xbox360WirelessSpeedWheel | Xbox 360 Wireless Speed Wheel | À créer / vérifier |
| Emulation.Microsoft.Controller.Xbox360BigButtonPad | Xbox 360 Big Button Pad | À créer / vérifier |
| Emulation.Microsoft.Controller.Xbox360Chatpad | Xbox 360 Chatpad | À créer / vérifier |
| Emulation.Microsoft.Controller.Xbox360MediaRemote2005 | Xbox 360 Media Remote (2005) | À créer / vérifier |
| Emulation.Microsoft.Controller.Xbox360UniversalMediaRemote | Xbox 360 Universal Media Remote | À créer / vérifier |
| Emulation.Microsoft.Controller.Xbox360MediaRemote2011 | Xbox 360 Media Remote (2011) | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxLiveVision | Xbox Live Vision | À créer / vérifier |
| Emulation.Microsoft.Controller.KinectXbox360 | Kinect for Xbox 360 | À créer / vérifier |
| Emulation.Microsoft.Controller.Xbox360Headset | Xbox 360 Headset | À créer / vérifier |
| Emulation.Microsoft.Controller.Xbox360WirelessHeadset | Xbox 360 Wireless Headset | À créer / vérifier |
| Emulation.Microsoft.Controller.Xbox360WirelessHeadsetBluetooth | Xbox 360 Wireless Headset with Bluetooth | À créer / vérifier |
| Emulation.Microsoft.Controller.Xbox360WirelessMicrophone | Xbox 360 Wireless Microphone | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxOneWirelessController2013 | Xbox One Wireless Controller (2013) | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxOneWirelessController2015 | Xbox One Wireless Controller (2015) | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxOneWirelessController2016 | Xbox Wireless Controller (2016) | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxEliteWirelessController | Xbox Elite Wireless Controller | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxEliteWirelessControllerSeries2 | Xbox Elite Wireless Controller Series 2 | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxEliteWirelessControllerSeries2Core | Xbox Elite Wireless Controller Series 2 - Core | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxAdaptiveController | Xbox Adaptive Controller | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxChatpad | Xbox Chatpad | À créer / vérifier |
| Emulation.Microsoft.Controller.KinectXboxOne | Kinect for Xbox One | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxOneMediaRemote | Xbox One Media Remote | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxOneChatHeadset | Xbox One Chat Headset | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxOneStereoHeadset | Xbox One Stereo Headset | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxOneStereoHeadsetAdapter | Xbox One Stereo Headset Adapter | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxSeriesWirelessController | Xbox Wireless Controller (2020) | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxAdaptiveJoystick | Xbox Adaptive Joystick | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxStereoHeadset2021 | Xbox Stereo Headset (2021) | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxWirelessHeadset2021 | Xbox Wireless Headset (2021) | À créer / vérifier |
| Emulation.Microsoft.Controller.XboxWirelessHeadset2024 | Xbox Wireless Headset (2024) | À créer / vérifier |

## Sony (52 clés)

| Clé | Nom invariant / libellé | Image de face et zones |
| --- | --- | --- |
| Emulation.Sony.Controller.MSXJS55 | Sony JS-55 joystick | À créer / vérifier |
| Emulation.Sony.Controller.MSXJS75 | Sony JS-75 wireless joystick | À créer / vérifier |
| Emulation.Sony.Controller.MSXJS303T | Sony JS-303T joypad | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationController | PlayStation controller | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationAnalogJoystick | PlayStation Analog Joystick | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationDualAnalog | PlayStation Dual Analog controller | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationDualShock | PlayStation DUALSHOCK controller | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationMouse | PlayStation Mouse | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationMultitap | PlayStation Multitap | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation2DualShock2 | PlayStation 2 DUALSHOCK 2 controller | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation2Multitap | PlayStation 2 Multitap | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation2EyeToy | PlayStation 2 EyeToy camera | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation2SingStarMicrophones | PlayStation 2 SingStar microphones | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation2BuzzBuzzers | PlayStation 2 Buzz! Buzzers | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation3Sixaxis | PlayStation 3 SIXAXIS controller | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation3DualShock3 | PlayStation 3 DUALSHOCK 3 controller | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation3Eye | PlayStation Eye camera | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationMoveMotion | PlayStation Move motion controller | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationMoveNavigation | PlayStation Move navigation controller | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation4DualShock4 | PlayStation 4 DUALSHOCK 4 controller | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation4Camera | PlayStation Camera | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationVRAim | PlayStation VR Aim controller | À créer / vérifier |
| Emulation.Sony.Controller.PlayStationVR | PlayStation VR headset | À créer / vérifier |
| Emulation.Sony.Controller.PlayStation5DualSense | PlayStation 5 DualSense wireless controller | À créer / vérifier |
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
| Emulation.Nintendo.Controller.NesPad | Nintendo NES Controller (NES-004) | À créer / vérifier |
| Emulation.Nintendo.Controller.NesDogbonePad | Nintendo NES Controller (NES-039) | À créer / vérifier |
| Emulation.Nintendo.Controller.FamicomPad1 | Nintendo Famicom Controller I | À créer / vérifier |
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
| Emulation.Nintendo.Controller.SuperNesPad | Nintendo Super NES Controller (SNS-005) | À créer / vérifier |
| Emulation.Nintendo.Controller.SuperFamicomPad | Nintendo Super Famicom Controller (SHVC-005) | À créer / vérifier |
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
| Emulation.Nintendo.Controller.Nintendo64Pad | Nintendo 64 Controller (NUS-005) | À créer / vérifier |
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
| Emulation.Nintendo.Controller.GameBoy | Nintendo Game Boy (DMG-01) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoyPocket | Nintendo Game Boy pocket (MGB-001) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoyLight | Nintendo Game Boy Light (MGB-101) | À créer / vérifier |
| Emulation.Nintendo.Controller.GameBoyColor | Nintendo Game Boy Color (CGB-001) | À créer / vérifier |
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
| Emulation.Nintendo.Controller.GameBoyAdvance | Nintendo Game Boy Advance (AGB-001) | À créer / vérifier |
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

| Clé | Nom invariant / libellé | Image de face et zones |
| --- | --- | --- |
| Emulation.Sega.Controller.SegaControlStick | Control Stick | À créer / vérifier |
| Emulation.Sega.Controller.SegaLightPhaser | Light Phaser | À créer / vérifier |
| Emulation.Sega.Controller.SegaMegaMouse | Mega Mouse | À créer / vérifier |
| Emulation.Sega.Controller.SegaMenacer | Menacer | À créer / vérifier |
| Emulation.Sega.Controller.SegaSportsPad | Sports Pad | À créer / vérifier |
| Emulation.Sega.Controller.SegaPaddleControl | Paddle Control | À créer / vérifier |
| Emulation.Sega.Controller.SegaHandleController | Handle Controller | À créer / vérifier |
| Emulation.Sega.Controller.SegaArcadePowerStick | Arcade Power Stick | À créer / vérifier |
| Emulation.Sega.Controller.SegaXe1Ap | XE-1 AP | À créer / vérifier |
| Emulation.Sega.Controller.SegaActivator | Activator | À créer / vérifier |
| Emulation.Sega.Controller.SegaSaturnThreeDControlPad | Saturn 3D Control Pad | À créer / vérifier |
| Emulation.Sega.Controller.SegaSaturnVirtuaGun | Virtua Gun | À créer / vérifier |
| Emulation.Sega.Controller.SegaSaturnShuttleMouse | Shuttle Mouse | À créer / vérifier |
| Emulation.Sega.Controller.SegaSaturnMissionStick | Mission Stick | À créer / vérifier |
| Emulation.Sega.Controller.SegaSaturnArcadeRacer | Arcade Racer | À créer / vérifier |
| Emulation.Sega.Controller.SegaSaturnTwinStick | Twin Stick | À créer / vérifier |
| Emulation.Sega.Controller.SegaSaturnVirtuaStick | Virtua Stick | À créer / vérifier |
| Emulation.Sega.Controller.SegaSg1000Joystick | SG-1000 joystick | À créer / vérifier |
| Emulation.Sega.Controller.SegaSg1000IiJoypad | SG-1000 II gamepad | À créer / vérifier |
| Emulation.Sega.Controller.SegaSc3000Keyboard | SC-3000 keyboard | À créer / vérifier |
| Emulation.Sega.Controller.SegaMasterSystemController | Master System controller | À créer / vérifier |
| Emulation.Sega.Controller.SegaGameGearController | Game Gear built-in controls | À créer / vérifier |
| Emulation.Sega.Controller.SegaMegaDriveThreeButton | Mega Drive 3-button controller | À créer / vérifier |
| Emulation.Sega.Controller.SegaMegaDriveSixButton | Mega Drive 6-button controller | À créer / vérifier |
| Emulation.Sega.Controller.SegaArcadePowerStickSixButton | Arcade Power Stick 6-button controller | À créer / vérifier |
| Emulation.Sega.Controller.SegaSaturnController | Saturn controller | À créer / vérifier |
| Emulation.Sega.Controller.SegaDreamcastController | Dreamcast controller | À créer / vérifier |
| Emulation.Sega.Controller.SegaDreamcastMouse | Dreamcast mouse | À créer / vérifier |
| Emulation.Sega.Controller.SegaDreamcastKeyboard | Dreamcast keyboard | À créer / vérifier |
| Emulation.Sega.Controller.SegaDreamcastLightGun | Dreamcast light gun | À créer / vérifier |
| Emulation.Sega.Controller.SegaDreamcastFishingController | Dreamcast fishing controller | À créer / vérifier |
| Emulation.Sega.Controller.SegaDreamcastArcadeStick | Dreamcast arcade stick | À créer / vérifier |
| Emulation.Sega.Controller.SegaDreamcastTwinStick | Dreamcast twin stick | À créer / vérifier |
| Emulation.Sega.Controller.SegaDreamcastMaracas | Dreamcast maracas | À créer / vérifier |
