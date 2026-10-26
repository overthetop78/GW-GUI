using GWGUI.MediaEngine.Contracts;
using M = GWGUI.MediaEngine.Constants.MechanicalSoundConstants;
using R = GWGUI.MediaEngine.Constants.RecordedMechanicalSoundConstants;

namespace GWGUI.MediaEngine.Constants;

internal static partial class RecordedMechanicalSoundFiles
{
    internal static partial class HardDisk
    {
        /// <summary>Dossier externe des sons de IBM H3171.</summary>
        private const string IbmH3171Directory = "HardDisk/IBM/H3171/";

        /// <summary>Enregistrements IBM H3171.</summary>
        internal static RecordedMechanicalSoundProfile IbmH3171 { get; } = new(
            Id: IbmH3171Directory[..^1],
            Device: M.HardDisk,
            DisplayName: "IBM H3171",
            Samples:
            [
                new(IbmH3171Directory + "Motor-running.flac", [new(M.Motor, Loop: true), new(M.Read, Loop: true), new(M.Write, Loop: true)]),
                new(IbmH3171Directory + "Motor-start.flac", [new(R.Start)]),
                new(IbmH3171Directory + "Motor-stop.flac", [new(M.Stop)]),
                new(IbmH3171Directory + "Seek/Single-track-movement.flac", [new(M.Seek, Distance: 1)]),
            ]);
    }
}
