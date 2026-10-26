namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Enums;

/// <summary>Étapes internes du contrôleur AcornScsi.</summary>
internal enum AcornScsiPhase { Free, Selection, Command, Executing, Read, Write, Formatting, Status, Message }
