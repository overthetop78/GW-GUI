using GWGUI.MediaEngine.Contracts;
using M = GWGUI.MediaEngine.Constants.MechanicalSoundConstants;
using R = GWGUI.MediaEngine.Constants.RecordedMechanicalSoundConstants;

namespace GWGUI.MediaEngine.Constants;

internal static partial class RecordedMechanicalSoundFiles
{
    internal static partial class Floppy
    {
        /// <summary>Dossier externe des sons de Unidentified 5.25-inch-drive-with-insertion-and-ejection.</summary>
        private const string Unidentified525InchDriveWithInsertionAndEjectionDirectory = "Floppy/Unidentified/5.25-inch-drive-with-insertion-and-ejection/";

        /// <summary>Enregistrements Unidentified 5.25-inch-drive-with-insertion-and-ejection.</summary>
        internal static RecordedMechanicalSoundProfile Unidentified525InchDriveWithInsertionAndEjection { get; } = new(
            Id: Unidentified525InchDriveWithInsertionAndEjectionDirectory[..^1],
            Device: M.Floppy,
            DisplayName: "Unidentified 5.25-inch-drive-with-insertion-and-ejection",
            Samples:
            [
                new(Unidentified525InchDriveWithInsertionAndEjectionDirectory + "Disk-eject.flac", [new(M.Eject)]),
                new(Unidentified525InchDriveWithInsertionAndEjectionDirectory + "Disk-insert.flac", [new(M.Insert)]),
                new(Unidentified525InchDriveWithInsertionAndEjectionDirectory + "Motor-running.flac", [new(M.Motor, Loop: true), new(M.Read, Loop: true), new(M.Write, Loop: true)]),
                new(Unidentified525InchDriveWithInsertionAndEjectionDirectory + "Motor-start.flac", [new(R.Start)]),
                new(Unidentified525InchDriveWithInsertionAndEjectionDirectory + "Motor-stop.flac", [new(M.Stop)]),
                new(Unidentified525InchDriveWithInsertionAndEjectionDirectory + "Power-on.flac", [new(M.Boot)]),
                new(Unidentified525InchDriveWithInsertionAndEjectionDirectory + "Seek/Single-track-movement.flac", [new(M.Seek, Distance: 1)]),
                new(Unidentified525InchDriveWithInsertionAndEjectionDirectory + "Seek/Toward-center/Movement.flac", [new(M.Seek, Direction: 1)]),
                new(Unidentified525InchDriveWithInsertionAndEjectionDirectory + "Seek/Toward-edge/Movement.flac", [new(M.Seek, Direction: -1)]),
            ]);
    }
}
