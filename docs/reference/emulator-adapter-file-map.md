# Carte des fichiers communs des modules d'émulation

La chaîne de connexion est unique :

`GWGUI.App` → interfaces publiques de `GWGUI.Emulation` → module familial
`GWGUI.Emulation.<Famille>` → `Common/Interfaces/IEmulatorAdapter.cs` → adaptateur concret sous
`Emulators/<nom>/`.

`GWGUI.App` ne référence aucun module familial. `GWGUI.Emulation` ne connaît aucun émulateur.
Chaque module familial se branche sur les interfaces publiques de `GWGUI.Emulation`, puis expose à
ses adaptateurs le même contrat interne `IEmulatorAdapter`. Les fichiers Atari et Amiga de ce
contrat ont exactement les mêmes membres et le même ordre ; seuls leur espace de noms et les types
familiaux résolus localement diffèrent. Les contextes `EmulatorCreationContext` et
`EmulatorManagementContext` ont eux aussi exactement les mêmes membres et le même ordre.

Une DLL, une option native, un protocole, un message propre au cœur ou une règle d'installation ne
doit jamais être ajouté à ces interfaces communes. L'adaptateur concret effectue la traduction vers
son émulateur dans `Emulators/<nom>/`. Le futur module Amstrad doit reprendre ces trois prises sans
ajouter de membre tant qu'un besoin commun aux trois familles n'a pas été établi.

## Structure canonique générale

Les rôles équivalents emploient les mêmes noms dans chaque projet `GWGUI.Emulation.Xxxx`.
Un fichier supplémentaire est autorisé uniquement lorsqu'il décrit une machine, un format ou une
capacité sans équivalent dans l'autre famille.

| Catégorie | Fichiers généraux |
|---|---|
| `Constants` | `AudioConstants.cs`, `ConfigurationConstants.cs`, `ControllerConstants.cs`, `CoreConstants.cs`, `CoreManagementConstants.cs`, `EmulationModuleConstants.cs`, `FirmwareConstants.cs`, `InputConstants.cs`, `MachineConstants.cs`, `MediaConstants.cs`, `ModelConstants.cs`, `RuntimeConstants.cs`, `SettingsConstants.cs`, `SettingsTextConstants.cs`, `SettingsChoiceConstants.cs`, `StateConstants.cs`, `StorageConstants.cs`, `VideoConstants.cs` |
| `Contracts` | `AudioContracts.cs`, `ConfigurationContracts.cs`, `ControllerContracts.cs`, `CoreContracts.cs`, `EmulatorContracts.cs`, `FirmwareContracts.cs`, `InputContracts.cs`, `MachineContracts.cs`, `MediaContracts.cs`, `ModelContracts.cs`, `RuntimeContracts.cs`, `SettingsContracts.cs`, `StateContracts.cs`, `StorageContracts.cs`, `VideoContracts.cs` |
| `Dictionaries` | `ControllerCatalog.cs`, `EmulatorCatalog.cs`, `FirmwareCatalog.cs`, `MachineCatalog.cs`, `ModelCatalog.cs` |
| `Enums` | `ControllerEnums.cs`, `CoreEnums.cs`, `FirmwareEnums.cs`, `InputEnums.cs`, `MachineEnums.cs`, `MediaEnums.cs`, `RuntimeEnums.cs`, `SettingsEnums.cs`, `StateEnums.cs`, `StorageEnums.cs`, `VideoEnums.cs` |
| `Functions` | fichiers `AudioFunctions`, `ConfigurationFunctions`, `ControllerFunctions`, `CoreFunctions`, `FirmwareFunctions`, `InputFunctions`, `MachineFunctions`, `MediaFunctions`, `ModelFunctions`, `RuntimeFunctions`, `SettingsFunctions`, `StateFunctions`, `StorageFunctions` et `VideoFunctions`, avec des fichiers partiels lorsque les responsabilités le justifient |
| `Interfaces` | `IEmulatorAdapter.cs`, `IEmulatorCore.cs`, puis une interface séparée uniquement pour une capacité réellement optionnelle |
| `Services` | `ConfigurationStore.cs`, `Engine.cs`, `Machine.cs`, puis un service séparé lorsqu'il possède son propre état, sa propre ressource ou son propre cycle de vie |

## Règles de rangement

- Un fichier général ne porte pas le nom Atari, Amiga, Amstrad ou celui d'un émulateur.
- Chaque adaptateur possède dans son dossier son identifiant, son nom natif, sa clé de description,
  sa DLL, sa source, sa révision et les autres données d'installation propres au cœur. Le catalogue
  de `Common` découvre ces métadonnées par `IEmulatorAdapter` et ne les recopie pas en dur.
- Les constantes et petits types d'un même domaine sont regroupés selon leur responsabilité.
- Les classes à état ou possédant des ressources restent séparées et sont découpées en fichiers partiels si nécessaire.
- Les éléments propres à une famille matérielle sont placés sous `Common/Machines/<famille>/`.
- L’infrastructure utilisée par toutes les familles du module est rangée sous
  `Common/Machines/Common/<catégorie>/` ; aucune catégorie `Constants`, `Contracts`,
  `Dictionaries`, `Enums`, `Exceptions` ou `Functions` ne reste directement sous `Machines`.

## Familles matérielles

- Atari sépare `Atari8Bit`, `Atari2600`, `Atari5200`, `Atari7800`, `AtariLynx`, `AtariJaguar`
  et `AtariST` sous `Common/Machines/`. Les six familles non-ST construisent le même contrat
  `Common/Machines/Common/Contracts/HardwareModelContracts.cs`, puis
  `Common/Machines/Common/Dictionaries/HardwareModelCatalog.cs` les agrège sans recopier leurs données.
- Amiga utilise `Common/Machines/AmigaComputers/` pour les ordinateurs A500 à A4000,
  `Common/Machines/AmigaCDTV/` pour le CDTV et `Common/Machines/AmigaCD32/` pour le CD32.
- Les contrats réellement identiques entre plusieurs familles restent dans le `Common` général ; les
  constantes et catalogues propres à une famille restent dans son dossier.
