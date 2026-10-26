using GWGUI.MediaEngine.Contracts;
using M = GWGUI.MediaEngine.Constants.MechanicalSoundConstants;
using R = GWGUI.MediaEngine.Constants.RecordedMechanicalSoundConstants;

namespace GWGUI.MediaEngine.Constants;

internal static partial class RecordedMechanicalSoundFiles
{
    internal static partial class Floppy
    {
        /// <summary>Dossier externe des sons de Unidentified 5.25-inch-drive-with-timed-seeks.</summary>
        private const string Unidentified525InchDriveWithTimedSeeksDirectory = "Floppy/Unidentified/5.25-inch-drive-with-timed-seeks/";

        /// <summary>Enregistrements Unidentified 5.25-inch-drive-with-timed-seeks.</summary>
        internal static RecordedMechanicalSoundProfile Unidentified525InchDriveWithTimedSeeks { get; } = new(
            Id: Unidentified525InchDriveWithTimedSeeksDirectory[..^1],
            Device: M.Floppy,
            DisplayName: "Unidentified 5.25-inch-drive-with-timed-seeks",
            Samples:
            [
                new(Unidentified525InchDriveWithTimedSeeksDirectory + "Motor-running-with-disk.flac", [new(M.Motor, Loop: true), new(M.Read, Loop: true), new(M.Write, Loop: true)]),
                new(Unidentified525InchDriveWithTimedSeeksDirectory + "Motor-running-without-disk.flac", [new(M.Motor, Loop: true), new(M.Read, Loop: true), new(M.Write, Loop: true)]),
                new(Unidentified525InchDriveWithTimedSeeksDirectory + "Motor-start-with-disk.flac", [new(R.Start)]),
                new(Unidentified525InchDriveWithTimedSeeksDirectory + "Motor-start-without-disk.flac", [new(R.Start)]),
                new(Unidentified525InchDriveWithTimedSeeksDirectory + "Motor-stop.flac", [new(M.Stop)]),
                new(Unidentified525InchDriveWithTimedSeeksDirectory + "Seek/Single-track-movement.flac", [new(M.Seek, Distance: 1)]),
                new(Unidentified525InchDriveWithTimedSeeksDirectory + "Seek/Timed-movements/12-ms.flac", [new(M.Seek)]),
                new(Unidentified525InchDriveWithTimedSeeksDirectory + "Seek/Timed-movements/2-ms.flac", [new(M.Seek)]),
                new(Unidentified525InchDriveWithTimedSeeksDirectory + "Seek/Timed-movements/20-ms.flac", [new(M.Seek)]),
                new(Unidentified525InchDriveWithTimedSeeksDirectory + "Seek/Timed-movements/6-ms.flac", [new(M.Seek)]),
            ]);
    }
}
