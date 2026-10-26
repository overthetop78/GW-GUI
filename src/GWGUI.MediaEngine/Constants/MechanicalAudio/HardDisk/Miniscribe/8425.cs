using GWGUI.MediaEngine.Contracts;
using M = GWGUI.MediaEngine.Constants.MechanicalSoundConstants;
using R = GWGUI.MediaEngine.Constants.RecordedMechanicalSoundConstants;

namespace GWGUI.MediaEngine.Constants;

internal static partial class RecordedMechanicalSoundFiles
{
    internal static partial class HardDisk
    {
        /// <summary>Dossier externe des sons de Miniscribe 8425.</summary>
        private const string Miniscribe8425Directory = "HardDisk/Miniscribe/8425/";

        /// <summary>Enregistrements Miniscribe 8425.</summary>
        internal static RecordedMechanicalSoundProfile Miniscribe8425 { get; } = new(
            Id: M.Winchester,
            Device: M.HardDisk,
            DisplayName: "Miniscribe 8425",
            Samples:
            [
                new(Miniscribe8425Directory + "Motor-running.flac", [new(M.Motor, Loop: true), new(M.Read, Loop: true), new(M.Write, Loop: true)]),
                new(Miniscribe8425Directory + "Motor-start.flac", [new(R.Start)]),
                new(Miniscribe8425Directory + "Motor-stop.flac", [new(M.Stop)]),
                new(Miniscribe8425Directory + "Seek/Single-track-movement.flac", [new(M.Seek, Distance: 1)]),
            ]);
    }
}
