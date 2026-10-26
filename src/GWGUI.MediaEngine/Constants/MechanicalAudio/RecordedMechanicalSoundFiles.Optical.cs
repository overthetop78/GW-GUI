using System.Collections.Generic;

namespace GWGUI.MediaEngine.Constants;

internal static partial class RecordedMechanicalSoundFiles
{
    internal static class Optical
    {
        // Unidentified CD-ROM-tray-drive
        internal static IReadOnlyDictionary<string, string> UnidentifiedCdRomTrayDrive { get; } = new Dictionary<string, string>
        {
            ["Optical/Unidentified/CD-ROM-tray-drive/Motor-running"] = "Optical/Unidentified/CD-ROM-tray-drive/Motor-running.flac",
            ["Optical/Unidentified/CD-ROM-tray-drive/Motor-start"] = "Optical/Unidentified/CD-ROM-tray-drive/Motor-start.flac",
            ["Optical/Unidentified/CD-ROM-tray-drive/Motor-stop"] = "Optical/Unidentified/CD-ROM-tray-drive/Motor-stop.flac",
            ["Optical/Unidentified/CD-ROM-tray-drive/Seek/Single-track-movement"] = "Optical/Unidentified/CD-ROM-tray-drive/Seek/Single-track-movement.flac",
            ["Optical/Unidentified/CD-ROM-tray-drive/Seek/Toward-center/Movement"] = "Optical/Unidentified/CD-ROM-tray-drive/Seek/Toward-center/Movement.flac",
            ["Optical/Unidentified/CD-ROM-tray-drive/Seek/Toward-edge/Movement"] = "Optical/Unidentified/CD-ROM-tray-drive/Seek/Toward-edge/Movement.flac",
            ["Optical/Unidentified/CD-ROM-tray-drive/Tray-close"] = "Optical/Unidentified/CD-ROM-tray-drive/Tray-close.flac",
            ["Optical/Unidentified/CD-ROM-tray-drive/Tray-open"] = "Optical/Unidentified/CD-ROM-tray-drive/Tray-open.flac",
        };

        internal static IReadOnlyList<IReadOnlyDictionary<string, string>> Catalogs { get; } =
        [
            UnidentifiedCdRomTrayDrive,
        ];
    }
}
