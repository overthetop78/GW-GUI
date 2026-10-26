using GWGUI.MediaEngine.Contracts;
using M = GWGUI.MediaEngine.Constants.MechanicalSoundConstants;
using R = GWGUI.MediaEngine.Constants.RecordedMechanicalSoundConstants;

namespace GWGUI.MediaEngine.Constants;

internal static partial class RecordedMechanicalSoundFiles
{
    internal static partial class HardDisk
    {
        /// <summary>Dossier externe des sons de Seagate ST3120A.</summary>
        private const string SeagateSt3120aDirectory = "HardDisk/Seagate/ST3120A/";

        /// <summary>Enregistrements Seagate ST3120A.</summary>
        internal static RecordedMechanicalSoundProfile SeagateSt3120a { get; } = new(
            Id: M.HardDisk35,
            Device: M.HardDisk,
            DisplayName: "Seagate ST3120A",
            Samples:
            [
                new(SeagateSt3120aDirectory + "Motor-running.flac", [new(M.Motor, Loop: true), new(M.Read, Loop: true), new(M.Write, Loop: true)]),
                new(SeagateSt3120aDirectory + "Motor-start-with-initialization.flac", [new(R.Start)]),
                new(SeagateSt3120aDirectory + "Motor-start.flac", [new(R.Start)]),
                new(SeagateSt3120aDirectory + "Motor-stop.flac", [new(M.Stop)]),
                new(SeagateSt3120aDirectory + "Seek/Single-track-movement.flac", [new(M.Seek, Distance: 1)]),
            ]);
    }
}
