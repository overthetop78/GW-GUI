# Inventaire des machines pour les bruitages mécaniques

Extraction statique des 24 modules présents le 26 octobre 2026. Cet inventaire conserve les identifiants et les déclarations du dépôt ; il ne prouve pas la présence d’enregistrements ni l’identité commerciale des lecteurs. Les mécanismes optionnels ne sont pas assimilés à des lecteurs intégrés.

## acorn

Modèles énumérés/référencés : `BbcMaster128`, `BbcMicroModelA`, `BbcMicroModelB`, `BbcMicroModelBPlus`, `BbcMicroModelBPlus128`.

Catalogues sources :

- `src/GWGUI.Emulation.Acorn/Common/Machines/BbcMicro/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Acorn/Common/Machines/Common/Dictionaries/HardwareModelCatalog.cs`
- `src/GWGUI.Emulation.Acorn/Common/Machines/Common/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Acorn/Common/Machines/Common/Enums/MachineEnums.cs`

Déclarations matérielles de stockage à relier aux références de lecteurs :

- `src/GWGUI.Emulation.Acorn/Common/Machines/BbcMicro/Dictionaries/ModelCatalog.cs:16` : `HardwareModelFunctions.Values(HardwareStorageCapability.Cassette),`
- `src/GWGUI.Emulation.Acorn/Common/Machines/BbcMicro/Dictionaries/ModelCatalog.cs:22` : `HardwareModelFunctions.Values(HardwareStorageCapability.Floppy, HardwareStorageCapability.Cassette),`
- `src/GWGUI.Emulation.Acorn/Common/Machines/BbcMicro/Dictionaries/ModelCatalog.cs:28` : `HardwareModelFunctions.Values(HardwareStorageCapability.Floppy, HardwareStorageCapability.Cassette),`
- `src/GWGUI.Emulation.Acorn/Common/Machines/BbcMicro/Dictionaries/ModelCatalog.cs:34` : `HardwareModelFunctions.Values(HardwareStorageCapability.Floppy, HardwareStorageCapability.Cassette),`
- `src/GWGUI.Emulation.Acorn/Common/Machines/BbcMicro/Dictionaries/ModelCatalog.cs:40` : `HardwareModelFunctions.Values(HardwareStorageCapability.Floppy, HardwareStorageCapability.Cassette, HardwareStorageCapability.Cartridge),`

Références acoustiques commerciales : à établir à partir des lecteurs intégrés et extensions déclarés ; ne pas attribuer un son PC générique à une machine différente.

## amstrad

Identifiants des définitions de modèles : `ModelConstants.Cpc464`, `ModelConstants.Cpc464Plus`, `ModelConstants.Cpc6128`, `ModelConstants.Cpc6128Plus`, `ModelConstants.Cpc664`, `ModelConstants.Gx4000`.

Catalogues sources :

- `src/GWGUI.Emulation.Amstrad/Common/Machines/Common/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Amstrad/Common/Machines/CpcClassic/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Amstrad/Common/Machines/CpcPlus/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Amstrad/Common/Machines/Gx4000/Dictionaries/ModelCatalog.cs`

Déclarations matérielles de stockage à relier aux références de lecteurs :

- `src/GWGUI.Emulation.Amstrad/Common/Machines/Common/Constants/StorageConstants.cs:24` : `internal const int MaximumFloppyDrives = 2;`
- `src/GWGUI.Emulation.Amstrad/Common/Machines/Common/Contracts/ModelContracts.cs:9` : `int BuiltInFloppyDriveCount,`
- `src/GWGUI.Emulation.Amstrad/Common/Machines/Common/Contracts/ModelContracts.cs:10` : `int MaximumFloppyDriveCount,`
- `src/GWGUI.Emulation.Amstrad/Common/Machines/Common/Contracts/ModelContracts.cs:11` : `bool HasBuiltInCassetteDrive,`
- `src/GWGUI.Emulation.Amstrad/Common/Machines/Common/Contracts/ModelContracts.cs:12` : `bool SupportsCassetteDrive,`

Références acoustiques commerciales : à établir à partir des lecteurs intégrés et extensions déclarés ; ne pas attribuer un son PC générique à une machine différente.

## apple

Modèles énumérés/référencés : `AppleII`, `AppleIIJPlus`, `AppleIIPlus`, `AppleIIe`, `AppleIIeEnhanced`, `Macintosh128K`, `Macintosh512K`, `MacintoshClassic`, `MacintoshII`, `MacintoshPlus`, `Pippin`.

Catalogues sources :

- `src/GWGUI.Emulation.Apple/Common/Machines/AppleII/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Apple/Common/Machines/Common/Dictionaries/HardwareModelCatalog.cs`
- `src/GWGUI.Emulation.Apple/Common/Machines/Common/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Apple/Common/Machines/Common/Enums/MachineEnums.cs`
- `src/GWGUI.Emulation.Apple/Common/Machines/Macintosh/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Apple/Common/Machines/Pippin/Dictionaries/ModelCatalog.cs`

Déclarations matérielles de stockage à relier aux références de lecteurs :

- `src/GWGUI.Emulation.Apple/Common/Machines/AppleII/Dictionaries/ModelCatalog.cs:24` : `Array.AsReadOnly(new[] { HardwareStorageCapability.Floppy, HardwareStorageCapability.HardDisk,`
- `src/GWGUI.Emulation.Apple/Common/Machines/AppleII/Dictionaries/ModelCatalog.cs:25` : `HardwareStorageCapability.Cassette }),`
- `src/GWGUI.Emulation.Apple/Common/Machines/Macintosh/Dictionaries/ModelCatalog.cs:28` : `Array.AsReadOnly(hardDisk ? new[] { HardwareStorageCapability.Floppy, HardwareStorageCapability.HardDisk } : new[] { HardwareStorageCapability.Floppy }),`
- `src/GWGUI.Emulation.Apple/Common/Machines/Pippin/Dictionaries/ModelCatalog.cs:15` : `[HardwareVideoCapability.Taos], [HardwareAudioCapability.Awacs], [HardwareStorageCapability.CompactDisc],`

Références acoustiques commerciales : à établir à partir des lecteurs intégrés et extensions déclarés ; ne pas attribuer un son PC générique à une machine différente.

## atari

Modèles énumérés/référencés : `Atari130Xe`, `Atari2600`, `Atari400`, `Atari5200`, `Atari7800`, `Atari800`, `Atari800Xl`, `Falcon`, `Jaguar`, `JaguarCd`, `Lynx`, `MegaSt`, `MegaSte`, `St`, `Ste`, `Stf`, `Stfm`, `Tt`, `Xegs`, `XlXe`.

Catalogues sources :

- `src/GWGUI.Emulation.Atari/Common/Machines/Atari2600/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Atari/Common/Machines/Atari5200/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Atari/Common/Machines/Atari7800/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Atari/Common/Machines/Atari8Bit/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Atari/Common/Machines/AtariJaguar/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Atari/Common/Machines/AtariLynx/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Atari/Common/Machines/AtariST/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Atari/Common/Machines/Common/Dictionaries/HardwareModelCatalog.cs`
- `src/GWGUI.Emulation.Atari/Common/Machines/Common/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Atari/Common/Machines/Common/Enums/MachineEnums.cs`

Déclarations matérielles de stockage à relier aux références de lecteurs :

- `src/GWGUI.Emulation.Atari/Common/Machines/Atari2600/Dictionaries/ModelCatalog.cs:14` : `HardwareModelFunctions.Values(HardwareStorageCapability.Cartridge),`
- `src/GWGUI.Emulation.Atari/Common/Machines/Atari5200/Dictionaries/ModelCatalog.cs:14` : `HardwareModelFunctions.Values(HardwareStorageCapability.Cartridge),`
- `src/GWGUI.Emulation.Atari/Common/Machines/Atari7800/Dictionaries/ModelCatalog.cs:14` : `HardwareModelFunctions.Values(HardwareStorageCapability.Cartridge),`
- `src/GWGUI.Emulation.Atari/Common/Machines/Atari8Bit/Dictionaries/ModelCatalog.cs:10` : `private static readonly IReadOnlyList<HardwareStorageCapability> Storage = HardwareModelFunctions.Values(HardwareStorageCapability.Floppy, HardwareStorageCapability.Cassette, HardwareStorageCapability.Cartridge);`
- `src/GWGUI.Emulation.Atari/Common/Machines/AtariJaguar/Dictionaries/ModelCatalog.cs:16` : `HardwareModelFunctions.Values(HardwareStorageCapability.Cartridge),`
- `src/GWGUI.Emulation.Atari/Common/Machines/AtariJaguar/Dictionaries/ModelCatalog.cs:21` : `HardwareModelFunctions.Values(HardwareStorageCapability.Cartridge, HardwareStorageCapability.CompactDisc),`
- `src/GWGUI.Emulation.Atari/Common/Machines/AtariLynx/Dictionaries/ModelCatalog.cs:14` : `HardwareModelFunctions.Values(HardwareStorageCapability.Cartridge),`

Références acoustiques commerciales : à établir à partir des lecteurs intégrés et extensions déclarés ; ne pas attribuer un son PC générique à une machine différente.

## bandai

Modèles énumérés/référencés : `Playdia`, `SwanCrystal`, `TamagotchiP1`, `WonderSwan`, `WonderSwanColor`.

Catalogues sources :

- `src/GWGUI.Emulation.Bandai/Common/Machines/Common/Dictionaries/HardwareModelCatalog.cs`
- `src/GWGUI.Emulation.Bandai/Common/Machines/Common/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Bandai/Common/Machines/Common/Enums/MachineEnums.cs`

Déclarations matérielles de stockage à relier aux références de lecteurs :

- Aucun nom de mécanisme déduit automatiquement : consulter les catalogues ci-dessus et les fonctions de stockage de ce module.

Références acoustiques commerciales : à établir à partir des lecteurs intégrés et extensions déclarés ; ne pas attribuer un son PC générique à une machine différente.

## coleco

Modèles énumérés/référencés : `Adam`, `ColecoVision`.

Catalogues sources :

- `src/GWGUI.Emulation.Coleco/Common/Machines/Common/Dictionaries/HardwareModelCatalog.cs`
- `src/GWGUI.Emulation.Coleco/Common/Machines/Common/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Coleco/Common/Machines/Common/Enums/MachineEnums.cs`

Déclarations matérielles de stockage à relier aux références de lecteurs :

- Aucun nom de mécanisme déduit automatiquement : consulter les catalogues ci-dessus et les fonctions de stockage de ce module.

Références acoustiques commerciales : à établir à partir des lecteurs intégrés et extensions déclarés ; ne pas attribuer un son PC générique à une machine différente.

## commodore

Identifiants des définitions de modèles : `ModelConstants.C128`, `ModelConstants.C16`, `ModelConstants.C232`, `ModelConstants.C64`, `ModelConstants.C64Dtv`, `ModelConstants.C64SuperCpu`, `ModelConstants.CbmII510`, `ModelConstants.CbmII610`, `ModelConstants.CbmII620`, `ModelConstants.CbmII620Plus`, `ModelConstants.CbmII710`, `ModelConstants.CbmII720`, `ModelConstants.CbmII720Plus`, `ModelConstants.Pet2001`, `ModelConstants.Pet3008`, `ModelConstants.Pet3016`, `ModelConstants.Pet3032`, `ModelConstants.Pet3032B`, `ModelConstants.Pet4016`, `ModelConstants.Pet4032`, `ModelConstants.Pet4032B`, `ModelConstants.Pet8032`, `ModelConstants.Pet8096`, `ModelConstants.Pet8296`, `ModelConstants.Plus4`, `ModelConstants.SuperPet`, `ModelConstants.V364`, `ModelConstants.Vic20`, `ModelConstants.Vic21`.

Catalogues sources :

- `src/GWGUI.Emulation.Commodore/Common/Machines/AmigaCD32/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Commodore/Common/Machines/AmigaComputers/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Commodore/Common/Machines/C128/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Commodore/Common/Machines/C64/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Commodore/Common/Machines/CbmII/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Commodore/Common/Machines/CommodoreCDTV/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Common/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Pet/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Plus4/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Vic20/Dictionaries/ModelCatalog.cs`

Déclarations matérielles de stockage à relier aux références de lecteurs :

- `src/GWGUI.Emulation.Commodore/Common/Machines/AmigaCD32/Dictionaries/ModelCatalog.cs:14` : `HasBuiltInFloppyDrive: false, HasKeyboard: false);`
- `src/GWGUI.Emulation.Commodore/Common/Machines/AmigaComputers/Contracts/ModelContracts.cs:15` : `bool SupportsHardDrives = true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/AmigaComputers/Contracts/ModelContracts.cs:20` : `bool HasBuiltInFloppyDrive = true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/C128/Dictionaries/ModelCatalog.cs:13` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/C64/Dictionaries/ModelCatalog.cs:13` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/C64/Dictionaries/ModelCatalog.cs:21` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/C64/Dictionaries/ModelCatalog.cs:29` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/CbmII/Dictionaries/ModelCatalog.cs:13` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/CbmII/Dictionaries/ModelCatalog.cs:21` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/CbmII/Dictionaries/ModelCatalog.cs:29` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/CbmII/Dictionaries/ModelCatalog.cs:37` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/CbmII/Dictionaries/ModelCatalog.cs:45` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/CbmII/Dictionaries/ModelCatalog.cs:53` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/CbmII/Dictionaries/ModelCatalog.cs:61` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/CommodoreCDTV/Dictionaries/ModelCatalog.cs:14` : `HasBuiltInFloppyDrive: false);`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Common/Contracts/ModelContracts.cs:14` : `bool HasBuiltInFloppyDrive,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Common/Contracts/ModelContracts.cs:20` : `public int MaximumFloppyDrives => (int)FloppyDriveCapacity;`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Common/Contracts/ModelContracts.cs:21` : `public int MaximumHardDrives => (int)HardDriveCapacity;`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Common/Contracts/ModelContracts.cs:22` : `public bool SupportsHardDrives => HardDriveCapacity != DriveCapacity.None;`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Pet/Dictionaries/ModelCatalog.cs:13` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Pet/Dictionaries/ModelCatalog.cs:21` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Pet/Dictionaries/ModelCatalog.cs:29` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Pet/Dictionaries/ModelCatalog.cs:37` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Pet/Dictionaries/ModelCatalog.cs:45` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Pet/Dictionaries/ModelCatalog.cs:53` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Pet/Dictionaries/ModelCatalog.cs:61` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Pet/Dictionaries/ModelCatalog.cs:69` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Pet/Dictionaries/ModelCatalog.cs:77` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Pet/Dictionaries/ModelCatalog.cs:85` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Pet/Dictionaries/ModelCatalog.cs:93` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Pet/Dictionaries/ModelCatalog.cs:101` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Plus4/Dictionaries/ModelCatalog.cs:13` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Plus4/Dictionaries/ModelCatalog.cs:21` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Plus4/Dictionaries/ModelCatalog.cs:29` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Plus4/Dictionaries/ModelCatalog.cs:37` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Vic20/Dictionaries/ModelCatalog.cs:13` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`
- `src/GWGUI.Emulation.Commodore/Common/Machines/Vic20/Dictionaries/ModelCatalog.cs:21` : `HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,`

Références acoustiques commerciales : à établir à partir des lecteurs intégrés et extensions déclarés ; ne pas attribuer un son PC générique à une machine différente.

## epoch

Modèles énumérés/référencés : `CassetteVision`, `CassetteVisionJr`, `SuperCassetteVision`, `SuperCassetteVisionLady`, `SuperCassetteVisionYeno`.

Catalogues sources :

- `src/GWGUI.Emulation.Epoch/Common/Machines/Common/Dictionaries/HardwareModelCatalog.cs`
- `src/GWGUI.Emulation.Epoch/Common/Machines/Common/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Epoch/Common/Machines/Common/Enums/MachineEnums.cs`

Déclarations matérielles de stockage à relier aux références de lecteurs :

- Aucun nom de mécanisme déduit automatiquement : consulter les catalogues ci-dessus et les fonctions de stockage de ce module.

Références acoustiques commerciales : à établir à partir des lecteurs intégrés et extensions déclarés ; ne pas attribuer un son PC générique à une machine différente.

## example

Modèles énumérés/référencés : `ExampleMachineA`, `ExampleMachineB`, `disquette`.

Catalogues sources :

- `src/GWGUI.Emulation.Example/Common/Machines/Common/Dictionaries/HardwareModelCatalog.cs`
- `src/GWGUI.Emulation.Example/Common/Machines/Common/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Example/Common/Machines/Common/Enums/MachineEnums.cs`
- `src/GWGUI.Emulation.Example/Common/Machines/ExampleMachineA/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Example/Common/Machines/ExampleMachineB/Dictionaries/ModelCatalog.cs`

Déclarations matérielles de stockage à relier aux références de lecteurs :

- `src/GWGUI.Emulation.Example/Common/Machines/ExampleMachineA/Dictionaries/ModelCatalog.cs:15` : `[HardwareStorageCapability.Floppy, HardwareStorageCapability.Cassette, HardwareStorageCapability.HardDisk],`
- `src/GWGUI.Emulation.Example/Common/Machines/ExampleMachineB/Dictionaries/ModelCatalog.cs:15` : `[HardwareStorageCapability.Cartridge, HardwareStorageCapability.MemoryCard],`

Références acoustiques commerciales : à établir à partir des lecteurs intégrés et extensions déclarés ; ne pas attribuer un son PC générique à une machine différente.

## fairchild

Modèles énumérés/référencés : `ChannelF`, `ChannelFSystemII`.

Catalogues sources :

- `src/GWGUI.Emulation.Fairchild/Common/Machines/Common/Dictionaries/HardwareModelCatalog.cs`
- `src/GWGUI.Emulation.Fairchild/Common/Machines/Common/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Fairchild/Common/Machines/Common/Enums/MachineEnums.cs`

Déclarations matérielles de stockage à relier aux références de lecteurs :

- Aucun nom de mécanisme déduit automatiquement : consulter les catalogues ci-dessus et les fonctions de stockage de ce module.

Références acoustiques commerciales : à établir à partir des lecteurs intégrés et extensions déclarés ; ne pas attribuer un son PC générique à une machine différente.

## mattel

Modèles énumérés/référencés : `Intellivision`.

Catalogues sources :

- `src/GWGUI.Emulation.Mattel/Common/Machines/Common/Dictionaries/HardwareModelCatalog.cs`
- `src/GWGUI.Emulation.Mattel/Common/Machines/Common/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Mattel/Common/Machines/Common/Enums/MachineEnums.cs`

Déclarations matérielles de stockage à relier aux références de lecteurs :

- Aucun nom de mécanisme déduit automatiquement : consulter les catalogues ci-dessus et les fonctions de stockage de ce module.

Références acoustiques commerciales : à établir à partir des lecteurs intégrés et extensions déclarés ; ne pas attribuer un son PC générique à une machine différente.

## microsoft

Identifiants des définitions de modèles : `ModelConstants.Xbox`, `ModelConstants.Xbox360`.

Catalogues sources :

- `src/GWGUI.Emulation.Microsoft/Common/Machines/Common/Dictionaries/ModelCatalog.cs`

Déclarations matérielles de stockage à relier aux références de lecteurs :

- `src/GWGUI.Emulation.Microsoft/Common/Machines/Common/Contracts/ModelContracts.cs:9` : `int BuiltInFloppyDriveCount,`
- `src/GWGUI.Emulation.Microsoft/Common/Machines/Common/Contracts/ModelContracts.cs:10` : `int MaximumFloppyDriveCount,`
- `src/GWGUI.Emulation.Microsoft/Common/Machines/Common/Contracts/ModelContracts.cs:11` : `bool HasBuiltInCassetteDrive,`
- `src/GWGUI.Emulation.Microsoft/Common/Machines/Common/Contracts/ModelContracts.cs:12` : `bool SupportsCassetteDrive,`
- `src/GWGUI.Emulation.Microsoft/Common/Machines/Common/Contracts/ModelContracts.cs:17` : `bool HasBuiltInCompactDiscDrive = false,`
- `src/GWGUI.Emulation.Microsoft/Common/Machines/Common/Contracts/ModelContracts.cs:18` : `bool SupportsCompactDiscDrive = false,`
- `src/GWGUI.Emulation.Microsoft/Common/Machines/Common/Dictionaries/ModelCatalog.cs:9` : `ControllerPortCount: 4, HasBuiltInCompactDiscDrive: true,`
- `src/GWGUI.Emulation.Microsoft/Common/Machines/Common/Dictionaries/ModelCatalog.cs:10` : `SupportsCompactDiscDrive: true, CpuModels: [ModelConstants.CpuPentiumIii],`
- `src/GWGUI.Emulation.Microsoft/Common/Machines/Common/Dictionaries/ModelCatalog.cs:15` : `ControllerPortCount: 4, HasBuiltInCompactDiscDrive: true,`
- `src/GWGUI.Emulation.Microsoft/Common/Machines/Common/Dictionaries/ModelCatalog.cs:16` : `SupportsCompactDiscDrive: false, CpuModels: [ModelConstants.CpuXenon],`

Références acoustiques commerciales : à établir à partir des lecteurs intégrés et extensions déclarés ; ne pas attribuer un son PC générique à une machine différente.

## nec

Catalogues sources :

- `src/GWGUI.Emulation.Nec/Common/Machines/Common/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Nec/Common/Machines/Pc8801/Dictionaries/ModelCatalog.cs`

Déclarations matérielles de stockage à relier aux références de lecteurs :

- `src/GWGUI.Emulation.Nec/Common/Machines/Common/Contracts/ModelContracts.cs:9` : `int BuiltInFloppyDriveCount,`
- `src/GWGUI.Emulation.Nec/Common/Machines/Common/Contracts/ModelContracts.cs:10` : `int MaximumFloppyDriveCount,`
- `src/GWGUI.Emulation.Nec/Common/Machines/Common/Contracts/ModelContracts.cs:11` : `bool HasBuiltInCassetteDrive,`
- `src/GWGUI.Emulation.Nec/Common/Machines/Common/Contracts/ModelContracts.cs:12` : `bool SupportsCassetteDrive,`
- `src/GWGUI.Emulation.Nec/Common/Machines/Common/Contracts/ModelContracts.cs:19` : `bool HasBuiltInCdDrive,`
- `src/GWGUI.Emulation.Nec/Common/Machines/Common/Contracts/ModelContracts.cs:20` : `bool SupportsCdDrive,`
- `src/GWGUI.Emulation.Nec/Common/Machines/Common/Contracts/ModelContracts.cs:21` : `bool SupportsHardDrives = false);`
- `src/GWGUI.Emulation.Nec/Common/Machines/CoreGrafx/Constants/CoreGrafxMachineConstants.cs:12` : `internal const int BuiltInFloppyDriveCount = 0;`
- `src/GWGUI.Emulation.Nec/Common/Machines/CoreGrafx/Constants/CoreGrafxMachineConstants.cs:13` : `internal const int MaximumFloppyDriveCount = 0;`
- `src/GWGUI.Emulation.Nec/Common/Machines/CoreGrafx/Constants/CoreGrafxMachineConstants.cs:14` : `internal const bool HasBuiltInCassetteDrive = false;`
- `src/GWGUI.Emulation.Nec/Common/Machines/CoreGrafx/Constants/CoreGrafxMachineConstants.cs:15` : `internal const bool SupportsCassetteDrive = false;`
- `src/GWGUI.Emulation.Nec/Common/Machines/CoreGrafx/Constants/CoreGrafxMachineConstants.cs:22` : `internal const bool HasBuiltInCdDrive = false;`
- `src/GWGUI.Emulation.Nec/Common/Machines/CoreGrafx/Constants/CoreGrafxMachineConstants.cs:23` : `internal const bool SupportsCdDrive = true;`
- `src/GWGUI.Emulation.Nec/Common/Machines/LaserActive/Constants/LaserActiveMachineConstants.cs:12` : `internal const int BuiltInFloppyDriveCount = 0;`
- `src/GWGUI.Emulation.Nec/Common/Machines/LaserActive/Constants/LaserActiveMachineConstants.cs:13` : `internal const int MaximumFloppyDriveCount = 0;`
- `src/GWGUI.Emulation.Nec/Common/Machines/LaserActive/Constants/LaserActiveMachineConstants.cs:14` : `internal const bool HasBuiltInCassetteDrive = false;`
- `src/GWGUI.Emulation.Nec/Common/Machines/LaserActive/Constants/LaserActiveMachineConstants.cs:15` : `internal const bool SupportsCassetteDrive = false;`
- `src/GWGUI.Emulation.Nec/Common/Machines/LaserActive/Constants/LaserActiveMachineConstants.cs:22` : `internal const bool HasBuiltInCdDrive = true;`
- `src/GWGUI.Emulation.Nec/Common/Machines/LaserActive/Constants/LaserActiveMachineConstants.cs:23` : `internal const bool SupportsCdDrive = true;`
- `src/GWGUI.Emulation.Nec/Common/Machines/Pc8001/Constants/MachineConstants.cs:16` : `HasBuiltInCassetteDrive: false, SupportsCassetteDrive: false,`
- `src/GWGUI.Emulation.Nec/Common/Machines/Pc8001/Constants/MachineConstants.cs:21` : `HasBuiltInCdDrive: false, SupportsCdDrive: false);`
- `src/GWGUI.Emulation.Nec/Common/Machines/Pc8801/Constants/MachineConstants.cs:20` : `HasBuiltInCassetteDrive: false, SupportsCassetteDrive: false,`
- `src/GWGUI.Emulation.Nec/Common/Machines/Pc8801/Constants/MachineConstants.cs:25` : `HasBuiltInCdDrive: false, SupportsCdDrive: false);`
- `src/GWGUI.Emulation.Nec/Common/Machines/Pc9801/Constants/MachineConstants.cs:16` : `HasBuiltInCassetteDrive: false, SupportsCassetteDrive: false,`
- `src/GWGUI.Emulation.Nec/Common/Machines/Pc9801/Constants/MachineConstants.cs:21` : `HasBuiltInCdDrive: false, SupportsCdDrive: false, SupportsHardDrives: true);`
- `src/GWGUI.Emulation.Nec/Common/Machines/Pc9821/Constants/MachineConstants.cs:16` : `HasBuiltInCassetteDrive: false, SupportsCassetteDrive: false,`
- `src/GWGUI.Emulation.Nec/Common/Machines/Pc9821/Constants/MachineConstants.cs:21` : `HasBuiltInCdDrive: false, SupportsCdDrive: false, SupportsHardDrives: true);`
- `src/GWGUI.Emulation.Nec/Common/Machines/PcEngine/Constants/PcEngineMachineConstants.cs:12` : `internal const int BuiltInFloppyDriveCount = 0;`
- `src/GWGUI.Emulation.Nec/Common/Machines/PcEngine/Constants/PcEngineMachineConstants.cs:13` : `internal const int MaximumFloppyDriveCount = 0;`
- `src/GWGUI.Emulation.Nec/Common/Machines/PcEngine/Constants/PcEngineMachineConstants.cs:14` : `internal const bool HasBuiltInCassetteDrive = false;`
- `src/GWGUI.Emulation.Nec/Common/Machines/PcEngine/Constants/PcEngineMachineConstants.cs:15` : `internal const bool SupportsCassetteDrive = false;`
- `src/GWGUI.Emulation.Nec/Common/Machines/PcEngine/Constants/PcEngineMachineConstants.cs:22` : `internal const bool HasBuiltInCdDrive = false;`
- `src/GWGUI.Emulation.Nec/Common/Machines/PcEngine/Constants/PcEngineMachineConstants.cs:23` : `internal const bool SupportsCdDrive = true;`
- `src/GWGUI.Emulation.Nec/Common/Machines/PcEngineDuo/Constants/PcEngineDuoMachineConstants.cs:12` : `internal const int BuiltInFloppyDriveCount = 0;`
- `src/GWGUI.Emulation.Nec/Common/Machines/PcEngineDuo/Constants/PcEngineDuoMachineConstants.cs:13` : `internal const int MaximumFloppyDriveCount = 0;`
- `src/GWGUI.Emulation.Nec/Common/Machines/PcEngineDuo/Constants/PcEngineDuoMachineConstants.cs:14` : `internal const bool HasBuiltInCassetteDrive = false;`
- `src/GWGUI.Emulation.Nec/Common/Machines/PcEngineDuo/Constants/PcEngineDuoMachineConstants.cs:15` : `internal const bool SupportsCassetteDrive = false;`
- `src/GWGUI.Emulation.Nec/Common/Machines/PcEngineDuo/Constants/PcEngineDuoMachineConstants.cs:22` : `internal const bool HasBuiltInCdDrive = true;`
- `src/GWGUI.Emulation.Nec/Common/Machines/PcEngineDuo/Constants/PcEngineDuoMachineConstants.cs:23` : `internal const bool SupportsCdDrive = true;`
- `src/GWGUI.Emulation.Nec/Common/Machines/PcEngineLt/Constants/PcEngineLtMachineConstants.cs:12` : `internal const int BuiltInFloppyDriveCount = 0;`
- `src/GWGUI.Emulation.Nec/Common/Machines/PcEngineLt/Constants/PcEngineLtMachineConstants.cs:13` : `internal const int MaximumFloppyDriveCount = 0;`
- `src/GWGUI.Emulation.Nec/Common/Machines/PcEngineLt/Constants/PcEngineLtMachineConstants.cs:14` : `internal const bool HasBuiltInCassetteDrive = false;`
- `src/GWGUI.Emulation.Nec/Common/Machines/PcEngineLt/Constants/PcEngineLtMachineConstants.cs:15` : `internal const bool SupportsCassetteDrive = false;`
- `src/GWGUI.Emulation.Nec/Common/Machines/PcEngineLt/Constants/PcEngineLtMachineConstants.cs:22` : `internal const bool HasBuiltInCdDrive = false;`
- `src/GWGUI.Emulation.Nec/Common/Machines/PcEngineLt/Constants/PcEngineLtMachineConstants.cs:23` : `internal const bool SupportsCdDrive = true;`
- `src/GWGUI.Emulation.Nec/Common/Machines/PcFx/Constants/PcFxMachineConstants.cs:12` : `internal const int BuiltInFloppyDriveCount = 0;`
- `src/GWGUI.Emulation.Nec/Common/Machines/PcFx/Constants/PcFxMachineConstants.cs:13` : `internal const int MaximumFloppyDriveCount = 0;`
- `src/GWGUI.Emulation.Nec/Common/Machines/PcFx/Constants/PcFxMachineConstants.cs:14` : `internal const bool HasBuiltInCassetteDrive = false;`
- `src/GWGUI.Emulation.Nec/Common/Machines/PcFx/Constants/PcFxMachineConstants.cs:15` : `internal const bool SupportsCassetteDrive = false;`
- `src/GWGUI.Emulation.Nec/Common/Machines/PcFx/Constants/PcFxMachineConstants.cs:22` : `internal const bool HasBuiltInCdDrive = true;`
- `src/GWGUI.Emulation.Nec/Common/Machines/PcFx/Constants/PcFxMachineConstants.cs:23` : `internal const bool SupportsCdDrive = true;`
- `src/GWGUI.Emulation.Nec/Common/Machines/SuperGrafx/Constants/SuperGrafxMachineConstants.cs:12` : `internal const int BuiltInFloppyDriveCount = 0;`
- `src/GWGUI.Emulation.Nec/Common/Machines/SuperGrafx/Constants/SuperGrafxMachineConstants.cs:13` : `internal const int MaximumFloppyDriveCount = 0;`
- `src/GWGUI.Emulation.Nec/Common/Machines/SuperGrafx/Constants/SuperGrafxMachineConstants.cs:14` : `internal const bool HasBuiltInCassetteDrive = false;`
- `src/GWGUI.Emulation.Nec/Common/Machines/SuperGrafx/Constants/SuperGrafxMachineConstants.cs:15` : `internal const bool SupportsCassetteDrive = false;`
- `src/GWGUI.Emulation.Nec/Common/Machines/SuperGrafx/Constants/SuperGrafxMachineConstants.cs:22` : `internal const bool HasBuiltInCdDrive = false;`
- `src/GWGUI.Emulation.Nec/Common/Machines/SuperGrafx/Constants/SuperGrafxMachineConstants.cs:23` : `internal const bool SupportsCdDrive = true;`
- `src/GWGUI.Emulation.Nec/Common/Machines/TurboExpress/Constants/TurboExpressMachineConstants.cs:12` : `internal const int BuiltInFloppyDriveCount = 0;`
- `src/GWGUI.Emulation.Nec/Common/Machines/TurboExpress/Constants/TurboExpressMachineConstants.cs:13` : `internal const int MaximumFloppyDriveCount = 0;`
- `src/GWGUI.Emulation.Nec/Common/Machines/TurboExpress/Constants/TurboExpressMachineConstants.cs:14` : `internal const bool HasBuiltInCassetteDrive = false;`
- `src/GWGUI.Emulation.Nec/Common/Machines/TurboExpress/Constants/TurboExpressMachineConstants.cs:15` : `internal const bool SupportsCassetteDrive = false;`
- `src/GWGUI.Emulation.Nec/Common/Machines/TurboExpress/Constants/TurboExpressMachineConstants.cs:22` : `internal const bool HasBuiltInCdDrive = false;`
- `src/GWGUI.Emulation.Nec/Common/Machines/TurboExpress/Constants/TurboExpressMachineConstants.cs:23` : `internal const bool SupportsCdDrive = false;`

Références acoustiques commerciales : à établir à partir des lecteurs intégrés et extensions déclarés ; ne pas attribuer un son PC générique à une machine différente.

## nintendo

Identifiants des définitions de modèles : `ModelConstants.FamicomDisk`, `ModelConstants.GameBoy`, `ModelConstants.GameBoyAdvance`, `ModelConstants.GameBoyColor`, `ModelConstants.GameCube`, `ModelConstants.GameWatch`, `ModelConstants.Nes`, `ModelConstants.Nintendo3Ds`, `ModelConstants.Nintendo64`, `ModelConstants.NintendoDs`, `ModelConstants.NintendoDsi`, `ModelConstants.PokemonMini`, `ModelConstants.Snes`, `ModelConstants.Switch`, `ModelConstants.VirtualBoy`, `ModelConstants.Wii`, `ModelConstants.WiiU`.

Catalogues sources :

- `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Dictionaries/ModelCatalog.cs`

Déclarations matérielles de stockage à relier aux références de lecteurs :

- `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Contracts/ModelContracts.cs:9` : `int BuiltInFloppyDriveCount,`
- `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Contracts/ModelContracts.cs:10` : `int MaximumFloppyDriveCount,`
- `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Contracts/ModelContracts.cs:11` : `bool HasBuiltInCassetteDrive,`
- `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Contracts/ModelContracts.cs:12` : `bool SupportsCassetteDrive,`
- `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Contracts/ModelContracts.cs:17` : `bool HasBuiltInCompactDiscDrive = false,`
- `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Contracts/ModelContracts.cs:18` : `bool SupportsCompactDiscDrive = false,`
- `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Dictionaries/ModelCatalog.cs:45` : `HasBuiltInCompactDiscDrive: true, SupportsCompactDiscDrive: true,`
- `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Dictionaries/ModelCatalog.cs:48` : `HasBuiltInCompactDiscDrive: true, SupportsCompactDiscDrive: true,`
- `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Dictionaries/ModelCatalog.cs:51` : `HasBuiltInCompactDiscDrive: true, SupportsCompactDiscDrive: true,`

Références acoustiques commerciales : à établir à partir des lecteurs intégrés et extensions déclarés ; ne pas attribuer un son PC générique à une machine différente.

## panasonic

Modèles énumérés/référencés : `ThreeDO`.

Catalogues sources :

- `src/GWGUI.Emulation.Panasonic/Common/Machines/Common/Dictionaries/HardwareModelCatalog.cs`
- `src/GWGUI.Emulation.Panasonic/Common/Machines/Common/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Panasonic/Common/Machines/Common/Enums/MachineEnums.cs`
- `src/GWGUI.Emulation.Panasonic/Common/Machines/ThreeDO/Dictionaries/HardwareModelCatalog.cs`

Déclarations matérielles de stockage à relier aux références de lecteurs :

- Aucun nom de mécanisme déduit automatiquement : consulter les catalogues ci-dessus et les fonctions de stockage de ce module.

Références acoustiques commerciales : à établir à partir des lecteurs intégrés et extensions déclarés ; ne pas attribuer un son PC générique à une machine différente.

## philips

Modèles énumérés/référencés : `JopacJo7400`, `Odyssey2`, `P2000T`, `PhilipsCdi`, `VideopacG7000`, `VideopacG7400`.

Catalogues sources :

- `src/GWGUI.Emulation.Philips/Common/Machines/Common/Dictionaries/HardwareModelCatalog.cs`
- `src/GWGUI.Emulation.Philips/Common/Machines/Common/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Philips/Common/Machines/Common/Enums/MachineEnums.cs`

Déclarations matérielles de stockage à relier aux références de lecteurs :

- Aucun nom de mécanisme déduit automatiquement : consulter les catalogues ci-dessus et les fonctions de stockage de ce module.

Références acoustiques commerciales : à établir à partir des lecteurs intégrés et extensions déclarés ; ne pas attribuer un son PC générique à une machine différente.

## sega

Identifiants des définitions de modèles : `ModelConstants.Atomiswave`, `ModelConstants.Dreamcast`, `ModelConstants.DreamcastVmu`, `ModelConstants.GameGear`, `ModelConstants.MarkIII`, `ModelConstants.MasterSystem`, `ModelConstants.MegaCd`, `ModelConstants.MegaDrive`, `ModelConstants.Model3`, `ModelConstants.Naomi`, `ModelConstants.Naomi2`, `ModelConstants.Pico`, `ModelConstants.Saturn`, `ModelConstants.Sc3000`, `ModelConstants.Sf7000`, `ModelConstants.Sg1000`, `ModelConstants.StV`, `ModelConstants.SystemSp`, `ModelConstants.ThirtyTwoX`.

Catalogues sources :

- `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/ModelCatalog.cs`

Déclarations matérielles de stockage à relier aux références de lecteurs :

- `src/GWGUI.Emulation.Sega/Common/Machines/Common/Contracts/ModelContracts.cs:9` : `int BuiltInFloppyDriveCount,`
- `src/GWGUI.Emulation.Sega/Common/Machines/Common/Contracts/ModelContracts.cs:10` : `int MaximumFloppyDriveCount,`
- `src/GWGUI.Emulation.Sega/Common/Machines/Common/Contracts/ModelContracts.cs:11` : `bool HasBuiltInCassetteDrive,`
- `src/GWGUI.Emulation.Sega/Common/Machines/Common/Contracts/ModelContracts.cs:12` : `bool SupportsCassetteDrive,`
- `src/GWGUI.Emulation.Sega/Common/Machines/Common/Contracts/ModelContracts.cs:17` : `bool HasBuiltInCompactDiscDrive = false,`
- `src/GWGUI.Emulation.Sega/Common/Machines/Common/Contracts/ModelContracts.cs:18` : `bool SupportsCompactDiscDrive = false,`
- `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/ModelCatalog.cs:36` : `HasBuiltInCompactDiscDrive: true, SupportsCompactDiscDrive: true,`
- `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/ModelCatalog.cs:40` : `HasBuiltInCompactDiscDrive: true, SupportsCompactDiscDrive: true,`
- `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/ModelCatalog.cs:44` : `false, ModelConstants.NoDrive, ModelConstants.NoDrive, false, false, true, true, SupportsCompactDiscDrive: true,`
- `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/ModelCatalog.cs:48` : `false, ModelConstants.NoDrive, ModelConstants.NoDrive, false, false, true, true, SupportsCompactDiscDrive: true,`
- `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/ModelCatalog.cs:62` : `false, false, true, true, HasBuiltInCompactDiscDrive: true, SupportsCompactDiscDrive: true,`

Références acoustiques commerciales : à établir à partir des lecteurs intégrés et extensions déclarés ; ne pas attribuer un son PC générique à une machine différente.

## sharp

Modèles énumérés/référencés : `X1`, `X1Turbo`, `X1TurboZ`, `X68000`, `X68000Xvi`, `X68030`.

Catalogues sources :

- `src/GWGUI.Emulation.Sharp/Common/Machines/Common/Dictionaries/HardwareModelCatalog.cs`
- `src/GWGUI.Emulation.Sharp/Common/Machines/Common/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Sharp/Common/Machines/Common/Enums/MachineEnums.cs`

Déclarations matérielles de stockage à relier aux références de lecteurs :

- Aucun nom de mécanisme déduit automatiquement : consulter les catalogues ci-dessus et les fonctions de stockage de ce module.

Références acoustiques commerciales : à établir à partir des lecteurs intégrés et extensions déclarés ; ne pas attribuer un son PC générique à une machine différente.

## sinclair

Modèles énumérés/référencés : `Pentagon1024K`, `Pentagon128K`, `Pentagon512K`, `Scorpion256K`, `Spectrum128K`, `Spectrum16K`, `Spectrum48K`, `Spectrum48KNtsc`, `SpectrumPlus2`, `SpectrumPlus2A`, `SpectrumPlus3`, `SpectrumPlus3E`, `SpectrumSe`, `TimexTc2048`, `TimexTc2068`, `TimexTs2068`, `Zx80`, `Zx81`.

Catalogues sources :

- `src/GWGUI.Emulation.Sinclair/Common/Machines/Common/Dictionaries/HardwareModelCatalog.cs`
- `src/GWGUI.Emulation.Sinclair/Common/Machines/Common/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Sinclair/Common/Machines/Common/Enums/MachineEnums.cs`

Déclarations matérielles de stockage à relier aux références de lecteurs :

- Aucun nom de mécanisme déduit automatiquement : consulter les catalogues ci-dessus et les fonctions de stockage de ce module.

Références acoustiques commerciales : à établir à partir des lecteurs intégrés et extensions déclarés ; ne pas attribuer un son PC générique à une machine différente.

## snk

Modèles énumérés/référencés : `NeoGeoAes`, `NeoGeoCd`, `NeoGeoCdz`, `NeoGeoMvs`, `NeoGeoPocket`, `NeoGeoPocketColor`.

Catalogues sources :

- `src/GWGUI.Emulation.Snk/Common/Machines/Common/Dictionaries/HardwareModelCatalog.cs`
- `src/GWGUI.Emulation.Snk/Common/Machines/Common/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Snk/Common/Machines/Common/Enums/MachineEnums.cs`

Déclarations matérielles de stockage à relier aux références de lecteurs :

- Aucun nom de mécanisme déduit automatiquement : consulter les catalogues ci-dessus et les fonctions de stockage de ce module.

Références acoustiques commerciales : à établir à partir des lecteurs intégrés et extensions déclarés ; ne pas attribuer un son PC générique à une machine différente.

## sony

Identifiants des définitions de modèles : `ModelConstants.PlayStation`, `ModelConstants.PlayStation2`, `ModelConstants.PlayStation3`, `ModelConstants.PlayStation4`, `ModelConstants.PlayStation5`, `ModelConstants.PsVita`, `ModelConstants.Psp`.

Catalogues sources :

- `src/GWGUI.Emulation.Sony/Common/Machines/Common/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Sony/Common/Machines/PocketStation/Dictionaries/ModelCatalog.cs`

Déclarations matérielles de stockage à relier aux références de lecteurs :

- `src/GWGUI.Emulation.Sony/Common/Machines/Common/Contracts/ModelContracts.cs:9` : `int BuiltInFloppyDriveCount,`
- `src/GWGUI.Emulation.Sony/Common/Machines/Common/Contracts/ModelContracts.cs:10` : `int MaximumFloppyDriveCount,`
- `src/GWGUI.Emulation.Sony/Common/Machines/Common/Contracts/ModelContracts.cs:11` : `bool HasBuiltInCassetteDrive,`
- `src/GWGUI.Emulation.Sony/Common/Machines/Common/Contracts/ModelContracts.cs:12` : `bool SupportsCassetteDrive,`
- `src/GWGUI.Emulation.Sony/Common/Machines/Common/Contracts/ModelContracts.cs:17` : `bool HasBuiltInCompactDiscDrive = false,`
- `src/GWGUI.Emulation.Sony/Common/Machines/Common/Contracts/ModelContracts.cs:18` : `bool SupportsCompactDiscDrive = false,`
- `src/GWGUI.Emulation.Sony/Common/Machines/Common/Dictionaries/ModelCatalog.cs:9` : `(int)RamCapacity._2MB, false, 0, 0, false, false, false, false, HasBuiltInCompactDiscDrive: true,`
- `src/GWGUI.Emulation.Sony/Common/Machines/Common/Dictionaries/ModelCatalog.cs:10` : `SupportsCompactDiscDrive: true, CpuModels: [ModelConstants.CpuR3000A],`
- `src/GWGUI.Emulation.Sony/Common/Machines/Common/Dictionaries/ModelCatalog.cs:14` : `(int)RamCapacity._32MB, false, 0, 0, false, false, false, false, HasBuiltInCompactDiscDrive: true,`
- `src/GWGUI.Emulation.Sony/Common/Machines/Common/Dictionaries/ModelCatalog.cs:15` : `SupportsCompactDiscDrive: true, CpuModels: [ModelConstants.CpuR5900],`
- `src/GWGUI.Emulation.Sony/Common/Machines/Common/Dictionaries/ModelCatalog.cs:20` : `HasBuiltInCompactDiscDrive: true, SupportsCompactDiscDrive: true,`
- `src/GWGUI.Emulation.Sony/Common/Machines/Common/Dictionaries/ModelCatalog.cs:30` : `HasBuiltInCompactDiscDrive: true, SupportsCompactDiscDrive: true,`
- `src/GWGUI.Emulation.Sony/Common/Machines/Common/Dictionaries/ModelCatalog.cs:35` : `HasBuiltInCompactDiscDrive: true, SupportsCompactDiscDrive: true,`
- `src/GWGUI.Emulation.Sony/Common/Machines/Common/Dictionaries/ModelCatalog.cs:40` : `HasBuiltInCompactDiscDrive: true, SupportsCompactDiscDrive: true,`
- `src/GWGUI.Emulation.Sony/Common/Machines/PocketStation/Dictionaries/ModelCatalog.cs:12` : `HasBuiltInCassetteDrive: false, SupportsCassetteDrive: false,`

Références acoustiques commerciales : à établir à partir des lecteurs intégrés et extensions déclarés ; ne pas attribuer un son PC générique à une machine différente.

## thomson

Modèles énumérés/référencés : `Mo5`, `Mo6`, `To7`, `To770`, `To8`, `To8D`, `To9`, `To9Plus`.

Catalogues sources :

- `src/GWGUI.Emulation.Thomson/Common/Machines/Common/Dictionaries/HardwareModelCatalog.cs`
- `src/GWGUI.Emulation.Thomson/Common/Machines/Common/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Thomson/Common/Machines/Common/Enums/MachineEnums.cs`
- `src/GWGUI.Emulation.Thomson/Common/Machines/Mo/Dictionaries/HardwareModelCatalog.cs`
- `src/GWGUI.Emulation.Thomson/Common/Machines/To/Dictionaries/HardwareModelCatalog.cs`

Déclarations matérielles de stockage à relier aux références de lecteurs :

- Aucun nom de mécanisme déduit automatiquement : consulter les catalogues ci-dessus et les fonctions de stockage de ce module.

Références acoustiques commerciales : à établir à partir des lecteurs intégrés et extensions déclarés ; ne pas attribuer un son PC générique à une machine différente.

## watara

Modèles énumérés/référencés : `Supervision`.

Catalogues sources :

- `src/GWGUI.Emulation.Watara/Common/Machines/Common/Dictionaries/HardwareModelCatalog.cs`
- `src/GWGUI.Emulation.Watara/Common/Machines/Common/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Watara/Common/Machines/Common/Enums/MachineEnums.cs`
- `src/GWGUI.Emulation.Watara/Common/Machines/Supervision/Dictionaries/HardwareModelCatalog.cs`

Déclarations matérielles de stockage à relier aux références de lecteurs :

- Aucun nom de mécanisme déduit automatiquement : consulter les catalogues ci-dessus et les fonctions de stockage de ce module.

Références acoustiques commerciales : à établir à partir des lecteurs intégrés et extensions déclarés ; ne pas attribuer un son PC générique à une machine différente.

## wenquxing

Modèles énumérés/référencés : `CC800`, `NC1020`, `NC2000`, `NC3000`, `PC1000`.

Catalogues sources :

- `src/GWGUI.Emulation.Wenquxing/Common/Machines/Common/Dictionaries/HardwareModelCatalog.cs`
- `src/GWGUI.Emulation.Wenquxing/Common/Machines/Common/Dictionaries/ModelCatalog.cs`
- `src/GWGUI.Emulation.Wenquxing/Common/Machines/Common/Enums/MachineEnums.cs`
- `src/GWGUI.Emulation.Wenquxing/Common/Machines/ElectronicDictionaries/Dictionaries/HardwareModelCatalog.cs`

Déclarations matérielles de stockage à relier aux références de lecteurs :

- Aucun nom de mécanisme déduit automatiquement : consulter les catalogues ci-dessus et les fonctions de stockage de ce module.

Références acoustiques commerciales : à établir à partir des lecteurs intégrés et extensions déclarés ; ne pas attribuer un son PC générique à une machine différente.

