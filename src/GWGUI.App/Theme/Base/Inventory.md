# Référence complète de présentation : Base

Base contient la présentation d’origine de l’application : fenêtres, dialogues, contrôles et interfaces de configuration des modules. Les namespaces sont conservés pour ne pas modifier les commandes, moteurs ou contrats.

## Fenêtres XAML

| Classe | Présentation Base |
|---|---|
| `GWGUI.App.Views.Dialogs.Conversion.ConversionConflictWindow` | `src\GWGUI.App\Theme\Base\Views\Dialogs\Conversion\ConversionConflictWindow.xaml` |
| `GWGUI.App.Views.Dialogs.Explorer.ExplorerIssuesWindow` | `src\GWGUI.App\Theme\Base\Views\Dialogs\Explorer\ExplorerIssuesWindow.xaml` |
| `GWGUI.App.Views.Dialogs.Hardware.HardwareUnavailableWindow` | `src\GWGUI.App\Theme\Base\Views\Dialogs\Hardware\HardwareUnavailableWindow.xaml` |
| `GWGUI.App.Views.Dialogs.Profiles.ProfileNameWindow` | `src\GWGUI.App\Theme\Base\Views\Dialogs\Profiles\ProfileNameWindow.xaml` |
| `GWGUI.App.Views.Dialogs.Read.ReadConflictWindow` | `src\GWGUI.App\Theme\Base\Views\Dialogs\Read\ReadConflictWindow.xaml` |
| `GWGUI.App.Views.Windows.About.AboutWindow` | `src\GWGUI.App\Theme\Base\Views\Windows\About\AboutWindow.xaml` |
| `GWGUI.App.Views.Windows.Conversion.FileMigrationWindow` | `src\GWGUI.App\Theme\Base\Views\Windows\Conversion\FileMigrationWindow.xaml` |
| `GWGUI.App.Views.Windows.EmulationModuleOptions.EmulationModuleOptionsWindow` | `src\GWGUI.App\Theme\Base\Views\Windows\EmulationModuleOptions\EmulationModuleOptionsWindow.xaml` |
| `GWGUI.App.Views.Windows.EmulationPreferences.EmulationPreferencesWindow` | `src\GWGUI.App\Theme\Base\Views\Windows\EmulationPreferences\EmulationPreferencesWindow.xaml` |
| `GWGUI.App.Views.Windows.Logs.LogHistoryWindow` | `src\GWGUI.App\Theme\Base\Views\Windows\Logs\LogHistoryWindow.xaml` |
| `GWGUI.App.Views.Windows.Preferences.PreferencesWindow` | `src\GWGUI.App\Theme\Base\Views\Windows\Preferences\PreferencesWindow.xaml` |
| `GWGUI.App.Views.Windows.Shell.MainWindow` | `src\GWGUI.App\Theme\Base\Views\Windows\Shell\MainWindow.xaml` |
| `GWGUI.App.Views.Windows.Tools.GwToolWindow` | `src\GWGUI.App\Theme\Base\Views\Windows\Tools\GwToolWindow.xaml` |
| `GWGUI.App.Views.Windows.Updates.UpdatesWindow` | `src\GWGUI.App\Theme\Base\Views\Windows\Updates\UpdatesWindow.xaml` |

## Fenêtres construites en C#

| Classe | Présentation Base |
|---|---|
| `AddStorageDeviceDialog` | `src\GWGUI.App\Theme\Base\Views\Dialogs\Emulation\Storage\AddStorageDeviceDialog.cs` |
| `CartridgeSlotConfigurationDialog` | `src\GWGUI.App\Theme\Base\Views\Dialogs\Emulation\Storage\CartridgeSlotConfigurationDialog.cs` |
| `CommonErrorDialog` | `src\GWGUI.App\Theme\Base\Views\Dialogs\Common\CommonErrorDialog.cs` |
| `CompactDiscDriveConfigurationDialog` | `src\GWGUI.App\Theme\Base\Views\Dialogs\Emulation\Storage\CompactDiscDriveConfigurationDialog.cs` |
| `CoreOptionsDialog` | `src\GWGUI.App\Theme\Base\Views\Dialogs\Emulation\CoreOptionsDialog.cs` |
| `FirmwareSelectionDialog` | `src\GWGUI.App\Theme\Base\Views\Dialogs\Emulation\FirmwareSelectionDialog.cs` |
| `FloppyDriveConfigurationDialog` | `src\GWGUI.App\Theme\Base\Views\Dialogs\Emulation\Storage\FloppyDriveConfigurationDialog.cs` |
| `HardDiskDriveConfigurationDialog` | `src\GWGUI.App\Theme\Base\Views\Dialogs\Emulation\Storage\HardDiskDriveConfigurationDialog.cs` |
| `ManagedStorageConfigurationDialog` | `src\GWGUI.App\Theme\Base\Views\Dialogs\Emulation\Storage\ManagedStorageConfigurationDialog.cs` |
| `MediaCreationDialog` | `src\GWGUI.App\Theme\Base\Views\Dialogs\Emulation\Storage\MediaCreationDialog.cs` |
| `MemoryCardDirectoryConfigurationDialog` | `src\GWGUI.App\Theme\Base\Views\Dialogs\Emulation\Storage\MemoryCardDirectoryConfigurationDialog.cs` |

## Contrôles et composants

- 64 fichiers XAML de présentation.
- 377 fichiers C# de présentation et de coordination des vues.
- Les sept fonctions restent Lecture, Écriture, Conversion, Visualisation, Explorateur, Outils et Émulation.
- `Views/Controls/Emulation/Options/` : générateur commun de réglages des modules (matériel, mémoire, ROM, vidéo, audio, stockage, entrées et réseau).
- `Views/Dialogs/Emulation/` : sélection de firmware et configurations/ créations des supports.
- `Views/Controls/Emulation/Machine/` : sélection, onglets de machines, commandes et écran.
- `Views/Styles/` : styles de référence. Les traductions restent dans Resources.

## Modules existants et interfaces communes

| Module | Version | DLL déclarée | Source |
|---|---|---|---|
| `acorn` | `1.3.0` | `gwgui.emulation.acorn.dll` | `src/GWGUI.Emulation.Acorn/module.json` |
| `amstrad` | `1.3.0` | `gwgui.emulation.amstrad.dll` | `src/GWGUI.Emulation.Amstrad/module.json` |
| `apple` | `1.3.0` | `gwgui.emulation.apple.dll` | `src/GWGUI.Emulation.Apple/module.json` |
| `atari` | `1.3.0` | `gwgui.emulation.atari.dll` | `src/GWGUI.Emulation.Atari/module.json` |
| `bandai` | `1.3.0` | `gwgui.emulation.bandai.dll` | `src/GWGUI.Emulation.Bandai/module.json` |
| `coleco` | `1.3.0` | `gwgui.emulation.coleco.dll` | `src/GWGUI.Emulation.Coleco/module.json` |
| `commodore` | `1.3.0` | `gwgui.emulation.commodore.dll` | `src/GWGUI.Emulation.Commodore/module.json` |
| `epoch` | `1.3.0` | `gwgui.emulation.epoch.dll` | `src/GWGUI.Emulation.Epoch/module.json` |
| `example` | `1.3.0` | `gwgui.emulation.example.dll` | `src/GWGUI.Emulation.Example/module.json` |
| `fairchild` | `1.3.0` | `gwgui.emulation.fairchild.dll` | `src/GWGUI.Emulation.Fairchild/module.json` |
| `mattel` | `1.3.0` | `gwgui.emulation.mattel.dll` | `src/GWGUI.Emulation.Mattel/module.json` |
| `microsoft` | `1.3.0` | `gwgui.emulation.microsoft.dll` | `src/GWGUI.Emulation.Microsoft/module.json` |
| `nec` | `1.3.0` | `gwgui.emulation.nec.dll` | `src/GWGUI.Emulation.Nec/module.json` |
| `nintendo` | `1.3.0` | `gwgui.emulation.nintendo.dll` | `src/GWGUI.Emulation.Nintendo/module.json` |
| `panasonic` | `1.3.0` | `gwgui.emulation.panasonic.dll` | `src/GWGUI.Emulation.Panasonic/module.json` |
| `philips` | `1.3.0` | `gwgui.emulation.philips.dll` | `src/GWGUI.Emulation.Philips/module.json` |
| `sega` | `1.3.0` | `gwgui.emulation.sega.dll` | `src/GWGUI.Emulation.Sega/module.json` |
| `sharp` | `1.3.0` | `gwgui.emulation.sharp.dll` | `src/GWGUI.Emulation.Sharp/module.json` |
| `sinclair` | `1.3.0` | `gwgui.emulation.sinclair.dll` | `src/GWGUI.Emulation.Sinclair/module.json` |
| `snk` | `1.3.0` | `gwgui.emulation.snk.dll` | `src/GWGUI.Emulation.Snk/module.json` |
| `sony` | `1.3.0` | `gwgui.emulation.sony.dll` | `src/GWGUI.Emulation.Sony/module.json` |
| `thomson` | `1.3.0` | `gwgui.emulation.thomson.dll` | `src/GWGUI.Emulation.Thomson/module.json` |
| `watara` | `1.3.0` | `gwgui.emulation.watara.dll` | `src/GWGUI.Emulation.Watara/module.json` |
| `wenquxing` | `1.3.0` | `gwgui.emulation.wenquxing.dll` | `src/GWGUI.Emulation.Wenquxing/module.json` |

Les 24 modules exposent leurs descriptions à l’interface générique ci-dessus. Leurs moteurs et descriptions restent dans leurs projets ; les présentations de ces descriptions sont rassemblées dans Base.

## Audit des émulateurs

Recherche effectuée dans les sources des 24 modules : créations Window/Button/Border/TextBlock, imports WPF/WinForms, pinceaux, couleurs d’interface, fenêtres natives et paramètres de thème.

- Aucun constructeur d’interface WPF/WinForms ni pinceau d’habillage n’a été trouvé dans ces modules. Il n’y a donc pas de fausse extraction à effectuer depuis leurs moteurs.
- Les identifiants de couleurs trouvés dans les périphériques Sony désignent des boutons physiques (guitares/Buzz), pas des couleurs de thème.
- `src/GWGUI.Emulation/Services/EmulationOpenGlHardwareRenderContext.cs` crée une fenêtre native de contexte graphique : elle ne constitue pas une interface à thématiser.
- Les palettes de pixels, effets vidéo de la machine, logos et couleurs des périphériques ne doivent pas être remplacés par les pinceaux d’habillage.
- L’injection des styles s’effectue dans les vues App de Base puis dans les présentations des thèmes, via les ressources et les contrôles communs ; elle ne modifie pas le rendu matériel.

## Fenêtres ponctuelles et sélecteurs système

| Source Base | Ligne initiale | Création |
|---|---|
| `src\GWGUI.App\Theme\Base\Views\Controls\Emulation\Machine\MachineController.cs` | 247 | `_fullscreenWindow = new Window` |
| `src\GWGUI.App\Theme\Base\Views\Controls\Emulation\Machine\MachineController.cs` | 334 | `var dialog = new OpenFileDialog` |
| `src\GWGUI.App\Theme\Base\Views\Controls\Emulation\Machine\MachineController.cs` | 397 | `var dialog = new SaveFileDialog` |
| `src\GWGUI.App\Theme\Base\Views\Controls\Emulation\Options\EmulationPreferencesSectionSettingsFunctions.cs` | 13 | `var dialog = new OpenFolderDialog { InitialDirectory = target.Text };` |
| `src\GWGUI.App\Theme\Base\Views\Controls\Emulation\Options\ModuleSettings\EmulationModuleSettingsSection.Fields.cs` | 125 | `var dialog = new OpenFileDialog` |
| `src\GWGUI.App\Theme\Base\Views\Controls\Emulation\Options\ModuleSettings\EmulationModuleSettingsSection.Fields.cs` | 165 | `var dialog = new OpenFolderDialog { InitialDirectory = path.Text };` |
| `src\GWGUI.App\Theme\Base\Views\Controls\Emulation\Storage\FloppyCreationSettingsView.cs` | 69 | `var dialog = new SaveFileDialog` |
| `src\GWGUI.App\Theme\Base\Views\Dialogs\Emulation\Storage\FloppyDriveConfigurationDialog.cs` | 143 | `var dialog = new SaveFileDialog` |
| `src\GWGUI.App\Theme\Base\Views\Dialogs\Emulation\Storage\HardDiskDriveConfigurationDialog.cs` | 263 | `var dialog = new OpenFileDialog` |
| `src\GWGUI.App\Theme\Base\Views\Dialogs\Emulation\Storage\MemoryCardDirectoryConfigurationDialog.cs` | 30 | `var browse = new OpenFolderDialog { InitialDirectory = _path.Text, Title = Title };` |
| `src\GWGUI.App\Theme\Base\Views\Windows\Logs\LogHistoryWindow.xaml.cs` | 39 | `var dialog = new SaveFileDialog { Filter = LocExtension.Get("Logs.ExportFilter"), FileName = Path.ChangeExtension(file.Name, ".txt"), DefaultExt = ".txt" };` |

Le plein écran de la machine et le dialogue d’erreur utilisent intentionnellement un cadre propre. Les sélecteurs de fichiers/dossiers sont des dialogues système. Leur fonction et leur propriétaire doivent être conservés.

## Périmètre actuel

Base est le seul thème conservé. Dock, Flow, Desktop, Index et Instrument, leurs gabarits et leur code de réorganisation ont été retirés à la demande de l'utilisateur. Les prochains thèmes seront réalisés et validés séparément.

L'audit des 24 modules couvre les imports WPF/WinForms, les constructions de fenêtres/contrôles et les pinceaux/couleurs de surface. Aucun constructeur d'interface ou pinceau d'habillage WPF/WinForms n'a été trouvé dans les moteurs des modules. Leurs descriptions de réglages sont affichées par les vues communes déplacées sous Base. Les noms InstrumentColors/BuzzColors de Sony décrivent les boutons du matériel, et non les couleurs du thème.

Le contexte OpenGL du SDK utilise une fenêtre native invisible pour le rendu matériel ; ce n'est pas une fenêtre d'interface à habiller. Les palettes vidéo, écrans d'émulation et représentations des périphériques conservent leurs couleurs matérielles.

Les ressources de Base fournissent désormais les couleurs des surfaces, du texte, des états et des icônes de l'interface. Elles sont chargées suivant le style clair/sombre. Aucune couleur de framebuffer n'a été modifiée.

Les constructeurs C# de `Functions/Views`, `Factories/Views` et des raccourcis d’émulation sont également rassemblés dans Base. `Base/Tokens.xaml` fournit les dimensions et couleurs de référence.
