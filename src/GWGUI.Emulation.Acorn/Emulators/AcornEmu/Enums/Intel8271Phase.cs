namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Enums;

/// <summary>Étapes internes du contrôleur Intel8271.</summary>
internal enum Intel8271Phase { Idle, Positioning, Read, Write, Verify, Id, Format }
