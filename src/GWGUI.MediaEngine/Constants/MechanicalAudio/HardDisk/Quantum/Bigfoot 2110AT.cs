using GWGUI.MediaEngine.Contracts;
using M = GWGUI.MediaEngine.Constants.MechanicalSoundConstants;
using R = GWGUI.MediaEngine.Constants.RecordedMechanicalSoundConstants;

namespace GWGUI.MediaEngine.Constants;

internal static partial class RecordedMechanicalSoundFiles
{
    internal static partial class HardDisk
    {
        /// <summary>Dossier externe des sons de Quantum Bigfoot 2110AT.</summary>
        private const string QuantumBigfoot2110atDirectory = "HardDisk/Quantum/Bigfoot 2110AT/";

        /// <summary>Enregistrements Quantum Bigfoot 2110AT.</summary>
        internal static RecordedMechanicalSoundProfile QuantumBigfoot2110at { get; } = new(
            Id: QuantumBigfoot2110atDirectory[..^1],
            Device: M.HardDisk,
            DisplayName: "Quantum Bigfoot 2110AT",
            Samples:
            [
                new(QuantumBigfoot2110atDirectory + "Motor-running.flac", [new(M.Motor, Loop: true), new(M.Read, Loop: true), new(M.Write, Loop: true)]),
                new(QuantumBigfoot2110atDirectory + "Motor-start.flac", [new(R.Start)]),
                new(QuantumBigfoot2110atDirectory + "Motor-stop.flac", [new(M.Stop)]),
                new(QuantumBigfoot2110atDirectory + "Seek/Single-track-movement.flac", [new(M.Seek, Distance: 1)]),
            ]);
    }
}
