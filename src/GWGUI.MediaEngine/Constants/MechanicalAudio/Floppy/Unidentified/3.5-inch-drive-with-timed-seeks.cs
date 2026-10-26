using GWGUI.MediaEngine.Contracts;
using M = GWGUI.MediaEngine.Constants.MechanicalSoundConstants;
using R = GWGUI.MediaEngine.Constants.RecordedMechanicalSoundConstants;

namespace GWGUI.MediaEngine.Constants;

internal static partial class RecordedMechanicalSoundFiles
{
    internal static partial class Floppy
    {
        /// <summary>Dossier externe des sons de Unidentified 3.5-inch-drive-with-timed-seeks.</summary>
        private const string Unidentified35InchDriveWithTimedSeeksDirectory = "Floppy/Unidentified/3.5-inch-drive-with-timed-seeks/";

        /// <summary>Enregistrements Unidentified 3.5-inch-drive-with-timed-seeks.</summary>
        internal static RecordedMechanicalSoundProfile Unidentified35InchDriveWithTimedSeeks { get; } = new(
            Id: Unidentified35InchDriveWithTimedSeeksDirectory[..^1],
            Device: M.Floppy,
            DisplayName: "Unidentified 3.5-inch-drive-with-timed-seeks",
            Samples:
            [
                new(Unidentified35InchDriveWithTimedSeeksDirectory + "Motor-running-with-disk.flac", [new(M.Motor, Loop: true), new(M.Read, Loop: true), new(M.Write, Loop: true)]),
                new(Unidentified35InchDriveWithTimedSeeksDirectory + "Motor-running-without-disk.flac", [new(M.Motor, Loop: true), new(M.Read, Loop: true), new(M.Write, Loop: true)]),
                new(Unidentified35InchDriveWithTimedSeeksDirectory + "Motor-start-with-disk.flac", [new(R.Start)]),
                new(Unidentified35InchDriveWithTimedSeeksDirectory + "Motor-start-without-disk.flac", [new(R.Start)]),
                new(Unidentified35InchDriveWithTimedSeeksDirectory + "Motor-stop.flac", [new(M.Stop)]),
                new(Unidentified35InchDriveWithTimedSeeksDirectory + "Seek/Single-track-movement.flac", [new(M.Seek, Distance: 1)]),
                new(Unidentified35InchDriveWithTimedSeeksDirectory + "Seek/Timed-movements/12-ms.flac", [new(M.Seek)]),
                new(Unidentified35InchDriveWithTimedSeeksDirectory + "Seek/Timed-movements/2-ms.flac", [new(M.Seek)]),
                new(Unidentified35InchDriveWithTimedSeeksDirectory + "Seek/Timed-movements/20-ms.flac", [new(M.Seek)]),
                new(Unidentified35InchDriveWithTimedSeeksDirectory + "Seek/Timed-movements/6-ms.flac", [new(M.Seek)]),
            ]);
    }
}
