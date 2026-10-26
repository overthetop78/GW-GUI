using GWGUI.MediaEngine.Contracts;
using M = GWGUI.MediaEngine.Constants.MechanicalSoundConstants;
using R = GWGUI.MediaEngine.Constants.RecordedMechanicalSoundConstants;

namespace GWGUI.MediaEngine.Constants;

internal static partial class RecordedMechanicalSoundFiles
{
    internal static partial class Floppy
    {
        /// <summary>Dossier externe des sons de Unidentified BBC-Micro-5.25-inch-drive.</summary>
        private const string UnidentifiedBbcMicro525InchDriveDirectory = "Floppy/Unidentified/BBC-Micro-5.25-inch-drive/";

        /// <summary>Enregistrements Unidentified BBC-Micro-5.25-inch-drive.</summary>
        internal static RecordedMechanicalSoundProfile UnidentifiedBbcMicro525InchDrive { get; } = new(
            Id: UnidentifiedBbcMicro525InchDriveDirectory[..^1],
            Device: M.Floppy,
            DisplayName: "Unidentified BBC-Micro-5.25-inch-drive",
            Samples:
            [
                new(UnidentifiedBbcMicro525InchDriveDirectory + "Motor-running.flac", [new(M.Motor, Loop: true), new(M.Read, Loop: true), new(M.Write, Loop: true)]),
                new(UnidentifiedBbcMicro525InchDriveDirectory + "Motor-start.flac", [new(R.Start)]),
                new(UnidentifiedBbcMicro525InchDriveDirectory + "Motor-stop.flac", [new(M.Stop)]),
                new(UnidentifiedBbcMicro525InchDriveDirectory + "Seek/Recorded-patterns/Pattern-3.flac", [new(M.Seek)]),
                new(UnidentifiedBbcMicro525InchDriveDirectory + "Seek/Single-track-movement.flac", [new(M.Seek, Distance: 1)]),
            ]);
    }
}
