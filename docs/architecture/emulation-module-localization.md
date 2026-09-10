# Traductions des modules d'émulation

## État actuel

Le raccordement est réalisé dans le SDK, l'hôte et les modules Amiga et Atari. Ce document décrit
le mécanisme en production et conserve l'inventaire des clés pour vérifier leur propriétaire.

## Fonctionnement actuel

`src/GWGUI.App/Localization/Extensions/LocExtension.cs` construit un index des clés depuis
les catalogues neutres nommés dans `Constants/Localization/LocalizationCatalogNames.cs`.
Chaque catalogue utilise `ResourceManager` et les ressources de l'assembly App. Une clé absente
est présentée sous la forme `[clé]`. `Get` utilise la culture UI pour le texte et la culture
de formatage pour les arguments. `GetInvariant` utilise la ressource neutre.

`Localization/Sources/LocalizationSource.cs` annonce les changements via `Version` et
`PropertyChanged`. Les bindings de `LocExtension` s'y abonnent. `App.xaml.cs` applique les
cultures ; les vues construites en code doivent conserver leur parcours actuel de rafraîchissement.

`Resources/00-Base` contient aussi bien les valeurs invariantes que les textes anglais de repli.
Les ressources de langue complètent cette base. Ne pas confondre « présent dans la base » et
« ne nécessite aucune traduction ». Les noms de machines, unités, formats et abréviations
invariants restent uniquement dans la base appropriée.

## Répartition à préserver

- App conserve les commandes communes, la présentation hôte, les contrôles physiques communs,
  les formats communs et les textes sans propriétaire de module établi.
- Les clés `Emulation.Amiga.*`, `Emulation.Atari.*` et les identités des familles appartiennent
  aux modules correspondants, sous réserve du contrôle des consommateurs avant déplacement.
- Une clé sans préfixe de famille n'est pas attribuée automatiquement au seul module qui
  l'utilise aujourd'hui : elle peut représenter une capacité générique destinée à d'autres modules.
- Les clés de touches émises par les contrats d'entrée communs doivent rester utilisables pour
  l'affichage et la capture hôte, même lorsqu'un module n'est pas installé.
- Déplacer les textes existants sans les réécrire ni renommer les identifiants persistants.
  Le transfert ne doit pas changer le sens des textes actuellement affichés.

## Cultures distribuées

La liste provient de `src/GWGUI.App/Dictionaries/Localization/UiLanguageCatalog.cs` et des
répertoires de ressources. Le repli de langue déclaré par App est `en-US`.

ar-SA, cs-CZ, da-DK, de-DE, el-GR, en-US, es-ES, fi-FI, fr-FR, he-IL, hu-HU,
id-ID, it-IT, ja-JP, ko-KR, nb-NO, nl-NL, pl-PL, pt-BR, pt-PT, ro-RO, ru-RU,
sv-SE, th-TH, tr-TR, uk-UA, vi-VN, zh-Hans, zh-Hant.

Conserver les 29 cultures, plus `00-Base`. Argos est l'outil installé pour compléter uniquement
les traductions nouvelles ou manquantes ; le script existant est `scripts/translate-resx-argos.py`.

## Consommateurs à raccorder ou à préserver

La liste suivante relève les consommateurs de métadonnées de module ; les appels de textes
communs présents dans les mêmes fichiers restent à distinguer des clés issues du module.
Tous les chemins sont relatifs à `src/GWGUI.App/`.

| Fichier | Échanges concernés |
|---|---|
| `Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs` | Famille, machines, blocs, champs, explications et choix |
| `Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs` | Réglages et présentation des champs matériels |
| `Views/Controls/Emulation/Options/EmulationPreferencesSection.cs` | Familles présentes dans le tableau des configurations |
| `Views/Controls/Emulation/Options/EmulationPreferencesSectionConfigurationFunctions.cs` | Nom de machine et contexte d'édition |
| `Views/Windows/EmulationModuleOptions/EmulationModuleOptionsWindow.xaml.cs` | Titre de la fenêtre propre au module |
| `Views/Controls/Emulation/Machine/EmulationSectionLayoutFunctions.cs` | Libellés des familles |
| `Views/Controls/Emulation/Machine/EmulationSectionConfigurationFunctions.cs` | Catalogue et sélection des machines |
| `Presenters/Emulation/Configurations/EmulationConfigurationPresenter.cs` | Nom de machine dans le résumé |
| `Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs` | Nom et choix CPU du tableau de configurations |
| `Functions/Views/Emulation/Settings/EmulationSettingsValuePresentationFunctions.cs` | Valeur affichée d'un choix |
| `Controllers/Emulation/Input/EmulationInputSettingsController.cs` | Choix des périphériques et associations |
| `Views/Controls/Emulation/Input/InputBindingEditor.xaml.cs` | Noms et descriptions des associations |
| `Controllers/Emulation/Storage/EmulationStorageSettingsController.cs` | Descriptions et choix de lecteurs |
| `Views/Dialogs/Emulation/Storage/FloppyDriveConfigurationDialog.cs` | Choix et réglages de lecteur |
| `Controllers/Emulation/Firmware/EmulationFirmwareManagementController.cs` | Champ destinataire d'un firmware |
| `Services/Emulation/HardDiskDeletionService.cs` | Libellés des références à un disque |
| `Controllers/Emulation/Options/EmulationEmulatorManagementController.cs` | Textes hôte de gestion du cœur, à préserver |
| `Presenters/Common/ControlErrorPresenter.cs` | Traduction des messages communs d'émulation, à préserver avec leur contexte |

## Résolution mise en place

La recherche reçoit explicitement le module concerné. Elle interroge ses ressources dans
la culture UI demandée puis ses cultures parentes, la langue de repli `en-US` et sa base
neutre `00-Base`. Elle n'interroge jamais le catalogue d'une autre famille. Si la clé reste
absente du module, la recherche App existante prend le relais. Une valeur vide trouvée est
une valeur présente, pas une clé manquante. Les appels sans contexte de module conservent
leur fonctionnement App actuel.

La recherche invariante interroge uniquement la base du module puis celle d'App. Les formats,
unités, noms et abréviations invariants communs restent dans la base App ; ceux appartenant
à une famille résident dans sa base. Les clés dont le nom semble générique ne sont déplacées
que lorsque leur propriété est établie par l'inventaire de leurs usages.

Le contrat commun `IEmulationModuleLocalization` reçoit une clé et une `CultureInfo`, retourne un indicateur de présence
et la valeur trouvée. Les ressources de toutes les cultures sont embarquées dans l'assembly
principal du module, avec des noms distincts, sans assembly satellite à installer. Les sources
restent des RESX maintenables avec Argos. Un lecteur commun dans `GWGUI.Emulation` réalise
la recherche ; les modules n'accèdent ni à `LocExtension` ni aux types WPF.

`LocExtension` applique ensuite le formatage existant avec `LocalizationSource.Culture`.
Ses bindings contextualisés observent la même propriété `Version` que les bindings actuels.
Pour les vues construites en code, conserver leur reconstruction lors du changement de langue
et transmettre le même contexte lors de cette reconstruction. Aucun catalogue global ne doit
être remplacé par la dernière famille chargée ; le registre expose seulement les modules déjà
initialisés et leur capacité de traduction, sans être appelé depuis l'index statique d'App.

Le transfert des clés ne doit pas supprimer les textes communs employés par d'autres fonctions
(notamment formats de disques et touches physiques). Le test de priorité utilise une concurrence
temporaire module/App ; il ne justifie pas de conserver des copies permanentes des textes transférés.

## Consommateurs communs vérifiés

Les appels CPU de `EmulationModuleHardwareSettingsSection.cs` transmettent désormais le module
à `DisplayValue`. Les contrôles eux-mêmes passent déjà par `CreateControlField` contextualisé.
Les titres et totaux génériques du matériel restent hôte. `EmulationEmulatorManagementController.cs`
présente les opérations communes de recherche/installation ; ses clés ne sont pas transférées.
`ControlErrorPresenter.cs` traduit les codes de `EmulationMessage` avec les constantes de l'hôte,
sans recevoir de clé propre à une famille ; ce fonctionnement et les messages originaux des cœurs
restent inchangés. Les touches physiques et les noms des manettes physiques restent communs.

Les associations propres à la machine passent par le contexte transmis à `InputBindingEditor.SetRows`.
Les raccourcis globaux utilisent toujours l'appel sans module. Les dialogues de lecteurs reçoivent
le contexte pour les modèles décrits par le module ; leurs commandes communes restent hôte.
Les parcours `RefreshLocalizedContent` des options et de la fenêtre principale sont conservés.

## Inventaire des clés

Les tables ci-dessous recensent les clés neutres retrouvées littéralement dans les sources
Amiga/Atari, ainsi que les clés préfixées par famille. « Référence » décrit un constat de code,
non une décision de transfert. Les clés construites dynamiquement doivent être couvertes par
leurs catalogues et leurs consommateurs avant migration. Les textes neutres sont conservés
pour rendre les attributions vérifiables sans modifier le logiciel.

| Clé | Catalogue App | Référence | Texte de base |
|---|---|---|---|
| `Emulation.Amiga.Chipset.Name` | Emulation.resx | Préfixe de famille | Chipset |
| `Emulation.Amiga.Chipset.NameCompatibility` | Emulation.resx | Préfixe de famille | Chipset compatibility |
| `Emulation.Amiga.Controller.Cd32` | Emulation.resx | Amiga | CD32 controller |
| `Emulation.Amiga.Controller.Cd32.Blue` | Emulation.resx | Amiga | CD32 blue button |
| `Emulation.Amiga.Controller.Cd32.FastForward` | Emulation.resx | Amiga | CD32 fast forward |
| `Emulation.Amiga.Controller.Cd32.Green` | Emulation.resx | Amiga | CD32 green button |
| `Emulation.Amiga.Controller.Cd32.PlayPause` | Emulation.resx | Amiga | CD32 play / pause |
| `Emulation.Amiga.Controller.Cd32.Red` | Emulation.resx | Amiga | CD32 red button |
| `Emulation.Amiga.Controller.Cd32.Rewind` | Emulation.resx | Amiga | CD32 rewind |
| `Emulation.Amiga.Controller.Cd32.Yellow` | Emulation.resx | Amiga | CD32 yellow button |
| `Emulation.Amiga.Controller.Joystick` | Emulation.resx | Amiga | Amiga joystick |
| `Emulation.Amiga.Controller.ParallelAdapter` | Emulation.resx | Amiga | Parallel four-player joystick adapter |
| `Emulation.Amiga.Model.A1000` | Emulation.resx | Préfixe de famille | Amiga 1000 |
| `Emulation.Amiga.Model.A1200` | Emulation.resx | Préfixe de famille | Amiga 1200 |
| `Emulation.Amiga.Model.A2000` | Emulation.resx | Préfixe de famille | Amiga 2000 |
| `Emulation.Amiga.Model.A3000` | Emulation.resx | Préfixe de famille | Amiga 3000 |
| `Emulation.Amiga.Model.A4000` | Emulation.resx | Préfixe de famille | Amiga 4000 |
| `Emulation.Amiga.Model.A500` | Emulation.resx | Préfixe de famille | Amiga 500 |
| `Emulation.Amiga.Model.A500PLUS` | Emulation.resx | Préfixe de famille | Amiga 500 Plus |
| `Emulation.Amiga.Model.A600` | Emulation.resx | Préfixe de famille | Amiga 600 |
| `Emulation.Amiga.Model.CD32` | Emulation.resx | Préfixe de famille | Amiga CD32 |
| `Emulation.Amiga.Model.CDTV` | Emulation.resx | Préfixe de famille | Commodore CDTV |
| `Emulation.Amiga.Storage.Floppy.Dd` | Emulation.resx | Préfixe de famille | Amiga 3.5-inch DD · 880 KiB |
| `Emulation.Amiga.Storage.Floppy.Hd` | Emulation.resx | Préfixe de famille | Amiga 3.5-inch HD · 1.76 MiB |
| `Emulation.Amiga.Storage.MediaFilter` | Emulation.resx | Préfixe de famille | Amiga media&#124;*.adf;*.adz;*.dms;*.fdi;*.ipf;*.raw;*.hdf;*.hdz;*.lha;*.slave;*.info;*.cue;*.ccd;*.chd;*.nrg;*.mds;*.iso;*.uae;*.m3u;*.zip;*.7z&#124;All files&#124;*.* |
| `Emulation.Atari.Audio.PokeyStereo` | Emulation.resx | Atari | Stereo POKEY |
| `Emulation.Atari.Controller.AnalogSensitivity` | Emulation.resx | Atari | Analog sensitivity |
| `Emulation.Atari.Controller.Atari5200` | Emulation.resx | Atari | Atari 5200 Controller |
| `Emulation.Atari.Controller.Autofire` | Emulation.resx | Atari | Autofire |
| `Emulation.Atari.Controller.AutofireAlways` | Emulation.resx | Atari | Always |
| `Emulation.Atari.Controller.AutofireButton` | Emulation.resx | Atari | While fire is held |
| `Emulation.Atari.Controller.BoosterGrip` | Emulation.resx | Atari | Atari Booster Grip |
| `Emulation.Atari.Controller.Compatibility` | Emulation.resx | Atari | Controller compatibility |
| `Emulation.Atari.Controller.DigitalSensitivity` | Emulation.resx | Atari | Digital sensitivity |
| `Emulation.Atari.Controller.Driving` | Emulation.resx | Atari | Atari Driving Controller |
| `Emulation.Atari.Controller.DualStick` | Emulation.resx | Atari | Dual stick |
| `Emulation.Atari.Controller.Genesis` | Emulation.resx | Atari | Sega Genesis Controller |
| `Emulation.Atari.Controller.Jaguar` | Emulation.resx | Atari | Atari Jaguar Controller |
| `Emulation.Atari.Controller.Joy2BPlus` | Emulation.resx | Atari | Atari Joy 2B+ Controller |
| `Emulation.Atari.Controller.Joystick` | Emulation.resx | Atari | Atari joystick |
| `Emulation.Atari.Controller.Lynx` | Emulation.resx | Atari | Atari Lynx |
| `Emulation.Atari.Controller.NumericKeypad` | Emulation.resx | Atari | Numeric keypad |
| `Emulation.Atari.Controller.PaddleControllers` | Emulation.resx | Atari | Atari Paddle Controllers |
| `Emulation.Atari.Controller.PaddleSpeed` | Emulation.resx | Atari | Paddle movement speed |
| `Emulation.Atari.Controller.ProLine` | Emulation.resx | Atari | Atari Pro-Line Joystick |
| `Emulation.Atari.Controller.SwapPorts` | Emulation.resx | Atari | Swap ports |
| `Emulation.Atari.Controller.Xg1LightGun` | Emulation.resx | Atari | Atari XG-1 Light Gun |
| `Emulation.Atari.Error.ActiveConfiguration` | Emulation.resx | Préfixe de famille | The active Atari configuration cannot be modified or deleted. |
| `Emulation.Atari.Error.ContentNotFound` | Emulation.resx | Préfixe de famille | The configured Atari media was not found. |
| `Emulation.Atari.Error.ContentUnsupported` | Emulation.resx | Préfixe de famille | The configured media is not supported by this Atari machine. |
| `Emulation.Atari.Error.CoreNotFound` | Emulation.resx | Préfixe de famille | The required Atari core was not found. |
| `Emulation.Atari.Error.CoreNotInstalled` | Emulation.resx | Préfixe de famille | The Atari core '{0}' is not installed. |
| `Emulation.Atari.Error.CoreRejected` | Emulation.resx | Préfixe de famille | The selected Atari core could not be loaded. |
| `Emulation.Atari.Error.Details` | Emulation.resx | Préfixe de famille | Details: {0} |
| `Emulation.Atari.Error.FirmwareFileMissing` | Emulation.resx | Préfixe de famille | Configured Atari firmware '{0}' was not found: {1} |
| `Emulation.Atari.Error.FirmwareInvalid` | Emulation.resx | Préfixe de famille | An Atari firmware is invalid or incompatible. |
| `Emulation.Atari.Error.FirmwareMissing` | Emulation.resx | Préfixe de famille | A required Atari firmware is missing. |
| `Emulation.Atari.Error.HostExecutableMissing` | Emulation.resx | Préfixe de famille | The Atari host executable path is unavailable. |
| `Emulation.Atari.Error.HostProtocolFailure` | Emulation.resx | Préfixe de famille | Communication with the Atari host failed. |
| `Emulation.Atari.Error.MediaFileMissing` | Emulation.resx | Préfixe de famille | Configured Atari media was not found: {0} |
| `Emulation.Atari.Error.OptionInvalid` | Emulation.resx | Préfixe de famille | An Atari option has an invalid value. |
| `Emulation.Atari.Error.RequiredFirmwareMissing` | Emulation.resx | Préfixe de famille | Required Atari firmware '{0}' is not configured for {1}. |
| `Emulation.Atari.Error.StateIncompatible` | Emulation.resx | Préfixe de famille | The Atari saved state is incompatible with the current machine. |
| `Emulation.Atari.Error.StateInvalid` | Emulation.resx | Préfixe de famille | The Atari saved state is invalid. |
| `Emulation.Atari.Error.Unexpected` | Emulation.resx | Préfixe de famille | An unexpected Atari emulation error occurred. |
| `Emulation.Atari.FastBoot` | Emulation.resx | Atari | Fast boot |
| `Emulation.Atari.Firmware.BasicEnabled` | Emulation.resx | Préfixe de famille | Internal BASIC |
| `Emulation.Atari.Firmware.BasicVersion` | Emulation.resx | Préfixe de famille | BASIC revision |
| `Emulation.Atari.Firmware.Multilingual` | Emulation.resx | Atari | Multilingual |
| `Emulation.Atari.Firmware.OsRevision` | Emulation.resx | Préfixe de famille | Operating system revision |
| `Emulation.Atari.Memory.Axlon` | Emulation.resx | Atari | Axlon RAM expansion |
| `Emulation.Atari.Memory.AxlonShadow` | Emulation.resx | Atari | Axlon $0F bank shadow |
| `Emulation.Atari.Memory.MapRam` | Emulation.resx | Atari | MapRAM |
| `Emulation.Atari.Memory.Mosaic` | Emulation.resx | Atari | Mosaic RAM expansion |
| `Emulation.Atari.Model.130Xe` | Emulation.resx | Atari | Atari 130XE |
| `Emulation.Atari.Model.2600` | Emulation.resx | Atari | Atari 2600 |
| `Emulation.Atari.Model.400` | Emulation.resx | Atari | Atari 400 |
| `Emulation.Atari.Model.5200` | Emulation.resx | Atari | Atari 5200 |
| `Emulation.Atari.Model.7800` | Emulation.resx | Atari | Atari 7800 |
| `Emulation.Atari.Model.800` | Emulation.resx | Atari | Atari 800 |
| `Emulation.Atari.Model.800Xl` | Emulation.resx | Atari | Atari 800XL |
| `Emulation.Atari.Model.Falcon` | Emulation.resx | Atari | Atari Falcon |
| `Emulation.Atari.Model.Jaguar` | Emulation.resx | Atari | Atari Jaguar |
| `Emulation.Atari.Model.JaguarCd` | Emulation.resx | Atari | Atari Jaguar CD |
| `Emulation.Atari.Model.Lynx` | Emulation.resx | Atari | Atari Lynx |
| `Emulation.Atari.Model.MegaSt` | Emulation.resx | Atari | Atari Mega ST |
| `Emulation.Atari.Model.MegaSte` | Emulation.resx | Atari | Atari Mega STE |
| `Emulation.Atari.Model.St` | Emulation.resx | Atari | Atari ST |
| `Emulation.Atari.Model.Ste` | Emulation.resx | Atari | Atari STE |
| `Emulation.Atari.Model.Stf` | Emulation.resx | Atari | Atari STF |
| `Emulation.Atari.Model.Stfm` | Emulation.resx | Atari | Atari STFM |
| `Emulation.Atari.Model.Tt` | Emulation.resx | Atari | Atari TT |
| `Emulation.Atari.Model.Xegs` | Emulation.resx | Atari | Atari XEGS |
| `Emulation.Atari.Model.XlXe` | Emulation.resx | Atari | Atari XL/XE |
| `Emulation.Atari.Storage.CassetteBoot` | Emulation.resx | Atari | Boot from cassette |
| `Emulation.Atari.Storage.MediaFilter` | Emulation.resx | Préfixe de famille | Atari media&#124;*.st;*.msa;*.stx;*.dim;*.ipf;*.vhd;*.ide;*.gem;*.atr;*.xfd;*.atx;*.cas;*.car;*.rom;*.bin;*.a26;*.a78;*.lnx;*.j64;*.jag;*.cue;*.ccd;*.chd;*.iso;*.m3u;*.zip;*.7z&#124;All files&#124;*.* |
| `Emulation.Atari.Storage.PrinterDevice` | Emulation.resx | Atari | P: printer device |
| `Emulation.Atari.Storage.RealTimeClock` | Emulation.resx | Atari | R-Time 8 real-time clock |
| `Emulation.Atari.Storage.SectorOsd` | Emulation.resx | Atari | Show sector/block counter |
| `Emulation.Atari.Storage.SerialDevice` | Emulation.resx | Atari | R: serial device |
| `Emulation.Atari.Storage.SioAcceleration` | Emulation.resx | Atari | SIO acceleration |
| `Emulation.Atari.Storage.SpeedOsd` | Emulation.resx | Atari | Show emulation speed on screen |
| `Emulation.Atari.Unavailable.ForcedByModel` | Emulation.resx | Atari | This value is determined by the selected model. |
| `Emulation.Atari.Unavailable.JaguarStandardNoCd` | Emulation.resx | Atari | Select the Jaguar CD model to use CD media. |
| `Emulation.Atari.Unavailable.NoAlternateMemory` | Emulation.resx | Atari | No alternate memory is available for this model. |
| `Emulation.Atari.Unavailable.NoFirmware` | Emulation.resx | Atari | This model has no configurable firmware. |
| `Emulation.Atari.Unavailable.NoFpu` | Emulation.resx | Atari | No compatible FPU is available for this model. |
| `Emulation.Atari.Unavailable.NoKeyboard` | Emulation.resx | Atari | This model has no keyboard. |
| `Emulation.Atari.Unavailable.NoMouse` | Emulation.resx | Atari | This model has no mouse. |
| `Emulation.Atari.Unavailable.NoStorage` | Emulation.resx | Atari | This model has no storage media. |
| `Emulation.Atari.Video.Artifacting` | Emulation.resx | Atari | Hi-res artifacting |
| `Emulation.Atari.Video.Artifacting.BlueBrown1` | Emulation.resx | Atari | Blue/brown 1 |
| `Emulation.Atari.Video.Artifacting.BlueBrown2` | Emulation.resx | Atari | Blue/brown 2 |
| `Emulation.Atari.Video.Brightness` | Emulation.resx | Atari | Brightness |
| `Emulation.Atari.Video.ColorDelay` | Emulation.resx | Atari | GTIA color delay |
| `Emulation.Atari.Video.Contrast` | Emulation.resx | Atari | Contrast |
| `Emulation.Atari.Video.ExternalPalette` | Emulation.resx | Atari | External palette |
| `Emulation.Atari.Video.Hue` | Emulation.resx | Atari | Hue |
| `Emulation.Atari.Video.Region` | Emulation.resx | Atari | Region |
| `Emulation.Atari.Video.RegionFree` | Emulation.resx | Atari | Region-free |
| `Emulation.Atari.Video.Saturation` | Emulation.resx | Atari | Saturation |
| `Emulation.Audio` | Options.resx | Amiga, Atari | Audio |
| `Emulation.Audio.Cd.Volume` | Emulation.resx | Amiga | CD audio volume |
| `Emulation.Audio.DefaultOutput` | Emulation.resx | Amiga, Atari | Windows default output |
| `Emulation.Audio.Device` | Emulation.resx | Amiga | Device |
| `Emulation.Audio.Enabled` | Emulation.resx | Amiga, Atari | Enable audio |
| `Emulation.Audio.Filter` | Emulation.resx | Amiga | Amiga audio filter |
| `Emulation.Audio.Filter.Emulated` | Emulation.resx | Amiga | Emulated |
| `Emulation.Audio.FilterType` | Emulation.resx | Amiga | Filter type |
| `Emulation.Audio.Floppy.Enabled` | Emulation.resx | Atari | Floppy-drive sound |
| `Emulation.Audio.Floppy.MuteEmpty` | Emulation.resx | Amiga | Mute empty drives |
| `Emulation.Audio.Floppy.Sound` | Emulation.resx | Amiga, Atari | Floppy-drive sound volume |
| `Emulation.Audio.Floppy.SoundType` | Emulation.resx | Amiga | Floppy-drive sound type |
| `Emulation.Audio.Interpolation` | Emulation.resx | Amiga | Interpolation |
| `Emulation.Audio.Interpolation.Anti` | Emulation.resx | Amiga | Anti interpolation |
| `Emulation.Audio.Latency` | Emulation.resx | Atari | Latency (ms) |
| `Emulation.Audio.LatencyLabel` | Emulation.resx | Amiga | Latency |
| `Emulation.Audio.Output` | Emulation.resx | Atari | Audio output |
| `Emulation.Audio.PolarizedFilter` | Emulation.resx | Atari | Polarized audio filter |
| `Emulation.Audio.StereoSeparation` | Emulation.resx | Amiga | Stereo separation (%) |
| `Emulation.Controller.Action.Down` | Emulation.resx | Amiga | Down |
| `Emulation.Controller.Action.Fire1` | Emulation.resx | Amiga | Fire button 1 |
| `Emulation.Controller.Action.Fire2` | Emulation.resx | Amiga | Fire button 2 |
| `Emulation.Controller.Action.Left` | Emulation.resx | Amiga | Left |
| `Emulation.Controller.Action.Right` | Emulation.resx | Amiga | Right |
| `Emulation.Controller.Action.TurboFire` | Emulation.resx | Amiga, Atari | Turbo fire |
| `Emulation.Controller.Action.Up` | Emulation.resx | Amiga | Up |
| `Emulation.Controller.AnalogJoystick` | Emulation.resx | Amiga, Atari | Analog joystick |
| `Emulation.Controller.Automatic` | Emulation.resx | Amiga, Atari | Automatic |
| `Emulation.Controller.None` | Emulation.resx | Amiga, Atari | None |
| `Emulation.Controller.Stick.Both` | Emulation.resx | Amiga | Both sticks |
| `Emulation.Controller.Stick.Left` | Emulation.resx | Amiga | Left stick |
| `Emulation.Controller.Stick.Right` | Emulation.resx | Amiga | Right stick |
| `Emulation.Controller.Tab` | Emulation.resx | Atari | Controllers |
| `Emulation.Controller.Turbo.Pulse` | Emulation.resx | Amiga | Turbo pulse |
| `Emulation.Cpu.Compatibility.Compatible` | Emulation.resx | Amiga, Atari | Compatible |
| `Emulation.Cpu.Compatibility.Exact` | Emulation.resx | Amiga, Atari | Cycle exact |
| `Emulation.Cpu.Compatibility.Memory` | Emulation.resx | Amiga | Memory compatible |
| `Emulation.Cpu.Compatibility.Normal` | Emulation.resx | Amiga | Normal |
| `Emulation.Cpu.Model` | Emulation.resx | Amiga, Atari | CPU model |
| `Emulation.Cpu.Precision` | Emulation.resx | Amiga, Atari | Precision |
| `Emulation.Cpu.Processor` | Emulation.resx | Amiga, Atari | Processor |
| `Emulation.Cpu.Speed` | Emulation.resx | Amiga, Atari | CPU speed |
| `Emulation.Cpu.SpeedOriginal` | Emulation.resx | Amiga, Atari | Original speed |
| `Emulation.Family.Amiga` | Emulation.resx | Amiga | Amiga |
| `Emulation.Family.Atari` | Emulation.resx | Atari | Atari |
| `Emulation.Firmware.Rom.Basic` | Emulation.resx | Atari | BASIC ROM |
| `Emulation.Firmware.Rom.Extended` | Emulation.resx | Amiga | Extended ROM |
| `Emulation.Firmware.Rom.Key` | Emulation.resx | Amiga | ROM key |
| `Emulation.Firmware.Rom.Kickstart` | Emulation.resx | Amiga | Kickstart |
| `Emulation.Firmware.Rom.System` | Emulation.resx | Amiga, Atari | System ROM |
| `Emulation.Firmware.Rom.Xegs` | Emulation.resx | Atari | XEGS ROM |
| `Emulation.Folder.Default` | Emulation.resx | Atari | Default folders |
| `Emulation.Fpu.Model` | Emulation.resx | Amiga, Atari | FPU |
| `Emulation.Key.AtariBreak` | Emulation.resx | Atari | Break |
| `Emulation.Key.AtariHelp` | Emulation.resx | Atari | Help |
| `Emulation.Key.AtariUndo` | Emulation.resx | Atari | Undo |
| `Emulation.Key.Help` | Emulation.resx | Amiga | Help |
| `Emulation.Key.LeftAmiga` | Emulation.resx | Amiga | Left Amiga |
| `Emulation.Key.RightAmiga` | Emulation.resx | Amiga | Right Amiga |
| `Emulation.Memory.Extensions` | Emulation.resx | Amiga, Atari | Memory extensions |
| `Emulation.Memory.Fast` | Emulation.resx | Amiga | Fast RAM |
| `Emulation.Memory.Main` | Emulation.resx | Amiga, Atari | Main memory |
| `Emulation.Memory.None` | Emulation.resx | Amiga, Atari | None |
| `Emulation.Memory.Slow` | Emulation.resx | Amiga | Slow RAM |
| `Emulation.Memory.Z3` | Emulation.resx | Amiga | Zorro III RAM |
| `Emulation.Mouse.Analog` | Emulation.resx | Amiga | Analog sticks controlling the mouse |
| `Emulation.Mouse.AnalogDeadzone` | Emulation.resx | Amiga | Analog-stick dead zone |
| `Emulation.Mouse.AnalogSpeed` | Emulation.resx | Amiga | Analog-stick mouse speed |
| `Emulation.Mouse.Button.Left` | Emulation.resx | Amiga, Atari | Left mouse button |
| `Emulation.Mouse.Button.Middle` | Emulation.resx | Amiga | Middle mouse button |
| `Emulation.Mouse.Button.Right` | Emulation.resx | Amiga, Atari | Right mouse button |
| `Emulation.Mouse.Speed` | Emulation.resx | Amiga, Atari | Mouse speed |
| `Emulation.State.Immediate` | Emulation.resx | Amiga | Immediate |
| `Emulation.State.ImmediateBlits` | Emulation.resx | Amiga | Blitter operation |
| `Emulation.State.Locked` | Emulation.resx | Amiga | Locked |
| `Emulation.State.Waiting` | Emulation.resx | Amiga | Waiting |
| `Emulation.Storage.ActivityOsd` | Emulation.resx | Atari | Show drive activity on the emulator screen |
| `Emulation.Storage.Device.List` | Emulation.resx | Atari | Storage devices |
| `Emulation.Storage.HardDisk.List` | Emulation.resx | Atari | Hard disks |
| `Emulation.Tab.Mouse` | Emulation.resx | Amiga, Atari | Mouse |
| `Emulation.Value.Default` | Emulation.resx | Atari | Default |
| `Emulation.Value.Disabled` | Emulation.resx | Amiga, Atari | Disabled |
| `Emulation.Value.Enabled` | Emulation.resx | Amiga, Atari | Enabled |
| `Emulation.Value.Enhanced` | Emulation.resx | Amiga | Enhanced |
| `Emulation.Value.Gray` | Emulation.resx | Atari | Gray |
| `Emulation.Value.Internal` | Emulation.resx | Amiga | Internal |
| `Emulation.Value.Large` | Emulation.resx | Amiga | Large |
| `Emulation.Value.Loud` | Emulation.resx | Amiga | Loud |
| `Emulation.Value.Maximum` | Emulation.resx | Amiga | Maximum |
| `Emulation.Value.Medium` | Emulation.resx | Amiga | Medium |
| `Emulation.Value.Minimum` | Emulation.resx | Amiga | Minimum |
| `Emulation.Value.None` | Emulation.resx | Atari | None |
| `Emulation.Value.Small` | Emulation.resx | Amiga | Small |
| `Emulation.Value.Standard` | Emulation.resx | Amiga | Standard |
| `Emulation.Value.VeryLarge` | Emulation.resx | Amiga | Very large |
| `Emulation.Value.VerySmall` | Emulation.resx | Amiga | Very small |
| `Emulation.Video.AspectRatio` | Emulation.resx | Amiga, Atari | Aspect ratio |
| `Emulation.Video.Collision.Full` | Emulation.resx | Amiga | Full |
| `Emulation.Video.Collision.Level` | Emulation.resx | Amiga | Collision detection |
| `Emulation.Video.Collision.Playfields` | Emulation.resx | Amiga | Playfields |
| `Emulation.Video.Collision.Sprites` | Emulation.resx | Amiga | Sprites |
| `Emulation.Video.Colors` | Emulation.resx | Amiga | Color depth |
| `Emulation.Video.Crop` | Emulation.resx | Amiga, Atari | Crop borders |
| `Emulation.Video.FlickerFixer` | Emulation.resx | Amiga | Flicker fixer |
| `Emulation.Video.FrameSkip` | Emulation.resx | Amiga, Atari | Frame skip |
| `Emulation.Video.Gamma` | Emulation.resx | Amiga, Atari | Gamma |
| `Emulation.Video.HzChange` | Emulation.resx | Amiga | Refresh-rate changes |
| `Emulation.Video.LineMode` | Emulation.resx | Amiga | Line mode |
| `Emulation.Video.LineMode.Double` | Emulation.resx | Amiga | Double scan |
| `Emulation.Video.LineMode.Single` | Emulation.resx | Amiga | Single scan |
| `Emulation.Video.Resolution` | Emulation.resx | Amiga, Atari | Resolution |
| `Emulation.Video.Resolution.AutoLow` | Emulation.resx | Amiga | Automatic low resolution |
| `Emulation.Video.Resolution.AutoSuperHigh` | Emulation.resx | Amiga | Automatic super-high resolution |
| `Emulation.Video.Resolution.High` | Emulation.resx | Amiga | High resolution |
| `Emulation.Video.Resolution.Low` | Emulation.resx | Amiga | Low resolution |
| `Emulation.Video.Resolution.SuperHigh` | Emulation.resx | Amiga | Super-high resolution |
| `Emulation.Video.Settings.Display` | Emulation.resx | Amiga, Atari | Display |
| `Emulation.Video.Standard` | Emulation.resx | Amiga, Atari | Video standard |
| `Explorer.Volume` | Explorer.resx | Atari | Volume |
| `Format.atari.130` | Formats.resx | Atari | Atari 8-bit — 130 KiB |
| `Format.atari.180` | Formats.resx | Atari | Atari 8-bit — 180 KiB |
| `Format.atari.90` | Formats.resx | Atari | Atari 8-bit — 90 KiB |
| `Format.atarist.1440` | Formats.resx | Atari | Atari ST — 1.44 MiB |
| `Format.atarist.720` | Formats.resx | Atari | Atari ST — 720 KiB |
| `HostTools.None` | HostTools.resx | Amiga | No installation detected. |
| `Visual.Automatic` | Visualizer.resx | Amiga, Atari | Automatic |
