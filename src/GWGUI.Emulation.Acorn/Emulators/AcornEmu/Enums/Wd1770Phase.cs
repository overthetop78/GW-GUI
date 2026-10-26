namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Enums;

/// <summary>Étapes internes du contrôleur Wd1770.</summary>
internal enum Wd1770Phase { Idle, Positioning, ReadSector, WriteSector, ReadAddress, ReadTrack, WriteTrack }
