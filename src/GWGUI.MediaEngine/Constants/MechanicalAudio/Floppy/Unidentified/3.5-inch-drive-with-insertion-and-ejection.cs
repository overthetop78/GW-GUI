using GWGUI.MediaEngine.Contracts;
using M = GWGUI.MediaEngine.Constants.MechanicalSoundConstants;
using R = GWGUI.MediaEngine.Constants.RecordedMechanicalSoundConstants;

namespace GWGUI.MediaEngine.Constants;

internal static partial class RecordedMechanicalSoundFiles
{
    internal static partial class Floppy
    {
        /// <summary>Dossier externe des sons de Unidentified 3.5-inch-drive-with-insertion-and-ejection.</summary>
        private const string Unidentified35InchDriveWithInsertionAndEjectionDirectory = "Floppy/Unidentified/3.5-inch-drive-with-insertion-and-ejection/";

        /// <summary>Enregistrements Unidentified 3.5-inch-drive-with-insertion-and-ejection.</summary>
        internal static RecordedMechanicalSoundProfile Unidentified35InchDriveWithInsertionAndEjection { get; } = new(
            Id: Unidentified35InchDriveWithInsertionAndEjectionDirectory[..^1],
            Device: M.Floppy,
            DisplayName: "Unidentified 3.5-inch-drive-with-insertion-and-ejection",
            Samples:
            [
                new(Unidentified35InchDriveWithInsertionAndEjectionDirectory + "Disk-eject.flac", [new(M.Eject)]),
                new(Unidentified35InchDriveWithInsertionAndEjectionDirectory + "Disk-insert.flac", [new(M.Insert)]),
                new(Unidentified35InchDriveWithInsertionAndEjectionDirectory + "Head-engage.flac", [new(M.HeadEngage)]),
                new(Unidentified35InchDriveWithInsertionAndEjectionDirectory + "Motor-running.flac", [new(M.Motor, Loop: true), new(M.Read, Loop: true), new(M.Write, Loop: true)]),
                new(Unidentified35InchDriveWithInsertionAndEjectionDirectory + "Motor-start.flac", [new(R.Start)]),
                new(Unidentified35InchDriveWithInsertionAndEjectionDirectory + "Motor-stop.flac", [new(M.Stop)]),
                new(Unidentified35InchDriveWithInsertionAndEjectionDirectory + "Power-on-with-disk.flac", [new(M.Boot)]),
                new(Unidentified35InchDriveWithInsertionAndEjectionDirectory + "Power-on.flac", [new(M.Boot)]),
                new(Unidentified35InchDriveWithInsertionAndEjectionDirectory + "Seek/Single-track-movement.flac", [new(M.Seek, Distance: 1)]),
                new(Unidentified35InchDriveWithInsertionAndEjectionDirectory + "Seek/Toward-center/Movement.flac", [new(M.Seek, Direction: 1)]),
                new(Unidentified35InchDriveWithInsertionAndEjectionDirectory + "Seek/Toward-edge/Movement.flac", [new(M.Seek, Direction: -1)]),
            ]);
    }
}
