using GWGUI.MediaEngine.Contracts;
using M = GWGUI.MediaEngine.Constants.MechanicalSoundConstants;
using R = GWGUI.MediaEngine.Constants.RecordedMechanicalSoundConstants;

namespace GWGUI.MediaEngine.Constants;

internal static partial class RecordedMechanicalSoundFiles
{
    internal static partial class HardDisk
    {
        /// <summary>Dossier externe des sons de Unidentified Hard-disk-drive.</summary>
        private const string UnidentifiedHardDiskDriveDirectory = "HardDisk/Unidentified/Hard-disk-drive/";

        /// <summary>Enregistrements Unidentified Hard-disk-drive.</summary>
        internal static RecordedMechanicalSoundProfile UnidentifiedHardDiskDrive { get; } = new(
            Id: UnidentifiedHardDiskDriveDirectory[..^1],
            Device: M.HardDisk,
            DisplayName: "Unidentified Hard-disk-drive",
            Samples:
            [
                new(UnidentifiedHardDiskDriveDirectory + "Motor-running.flac", [new(M.Motor, Loop: true), new(M.Read, Loop: true), new(M.Write, Loop: true)]),
                new(UnidentifiedHardDiskDriveDirectory + "Motor-start.flac", [new(R.Start)]),
                new(UnidentifiedHardDiskDriveDirectory + "Motor-stop.flac", [new(M.Stop)]),
                new(UnidentifiedHardDiskDriveDirectory + "Seek/Head-movement.flac", [new(M.Seek)]),
                new(UnidentifiedHardDiskDriveDirectory + "Seek/Long-head-movement.flac", [new(M.Seek)]),
            ]);
    }
}
