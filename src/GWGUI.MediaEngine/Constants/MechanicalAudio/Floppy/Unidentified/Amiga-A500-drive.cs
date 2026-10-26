using GWGUI.MediaEngine.Contracts;
using M = GWGUI.MediaEngine.Constants.MechanicalSoundConstants;
using R = GWGUI.MediaEngine.Constants.RecordedMechanicalSoundConstants;

namespace GWGUI.MediaEngine.Constants;

internal static partial class RecordedMechanicalSoundFiles
{
    internal static partial class Floppy
    {
        /// <summary>Dossier externe des sons de Unidentified Amiga-A500-drive.</summary>
        private const string UnidentifiedAmigaA500DriveDirectory = "Floppy/Unidentified/Amiga-A500-drive/";

        /// <summary>Enregistrements Unidentified Amiga-A500-drive.</summary>
        internal static RecordedMechanicalSoundProfile UnidentifiedAmigaA500Drive { get; } = new(
            Id: UnidentifiedAmigaA500DriveDirectory[..^1],
            Device: M.Floppy,
            DisplayName: "Unidentified Amiga-A500-drive",
            Samples:
            [
                new(UnidentifiedAmigaA500DriveDirectory + "Head-engage.flac", [new(M.HeadEngage)]),
                new(UnidentifiedAmigaA500DriveDirectory + "Motor-running-without-disk.flac", [new(M.Motor, Loop: true), new(M.Read, Loop: true), new(M.Write, Loop: true)]),
                new(UnidentifiedAmigaA500DriveDirectory + "Motor-running.flac", [new(M.Motor, Loop: true), new(M.Read, Loop: true), new(M.Write, Loop: true)]),
                new(UnidentifiedAmigaA500DriveDirectory + "Power-on.flac", [new(R.Start)]),
                new(UnidentifiedAmigaA500DriveDirectory + "Seek/Head-click.flac", [new(M.Seek, Distance: 1)]),
            ]);
    }
}
