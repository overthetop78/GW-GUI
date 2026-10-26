using GWGUI.MediaEngine.Contracts;
using M = GWGUI.MediaEngine.Constants.MechanicalSoundConstants;
using R = GWGUI.MediaEngine.Constants.RecordedMechanicalSoundConstants;

namespace GWGUI.MediaEngine.Constants;

internal static partial class RecordedMechanicalSoundFiles
{
    internal static partial class Optical
    {
        /// <summary>Dossier externe des sons de Unidentified CD-ROM-tray-drive.</summary>
        private const string UnidentifiedCdRomTrayDriveDirectory = "Optical/Unidentified/CD-ROM-tray-drive/";

        /// <summary>Enregistrements Unidentified CD-ROM-tray-drive.</summary>
        internal static RecordedMechanicalSoundProfile UnidentifiedCdRomTrayDrive { get; } = new(
            Id: M.OpticalTray,
            Device: M.Optical,
            DisplayName: "Unidentified CD-ROM-tray-drive",
            Samples:
            [
                new(UnidentifiedCdRomTrayDriveDirectory + "Motor-running.flac", [new(M.Motor, Loop: true), new(M.Read, Loop: true), new(M.Write, Loop: true)]),
                new(UnidentifiedCdRomTrayDriveDirectory + "Motor-start.flac", [new(R.Start)]),
                new(UnidentifiedCdRomTrayDriveDirectory + "Motor-stop.flac", [new(M.Stop)]),
                new(UnidentifiedCdRomTrayDriveDirectory + "Seek/Single-track-movement.flac", [new(M.Seek, Distance: 1)]),
                new(UnidentifiedCdRomTrayDriveDirectory + "Seek/Toward-center/Movement.flac", [new(M.Seek, Direction: 1)]),
                new(UnidentifiedCdRomTrayDriveDirectory + "Seek/Toward-edge/Movement.flac", [new(M.Seek, Direction: -1)]),
                new(UnidentifiedCdRomTrayDriveDirectory + "Tray-close.flac", [new(M.Insert)]),
                new(UnidentifiedCdRomTrayDriveDirectory + "Tray-open.flac", [new(M.Eject)]),
            ]);
    }
}
