using System.Collections.Generic;

namespace GWGUI.MediaEngine.Constants;

internal static partial class RecordedMechanicalSoundFiles
{
    internal static class HardDisk
    {
        // IBM H3171
        internal static IReadOnlyDictionary<string, string> IbmH3171 { get; } = new Dictionary<string, string>
        {
            ["HardDisk/IBM/H3171/Motor-running"] = "HardDisk/IBM/H3171/Motor-running.flac",
            ["HardDisk/IBM/H3171/Motor-start"] = "HardDisk/IBM/H3171/Motor-start.flac",
            ["HardDisk/IBM/H3171/Motor-stop"] = "HardDisk/IBM/H3171/Motor-stop.flac",
            ["HardDisk/IBM/H3171/Seek/Single-track-movement"] = "HardDisk/IBM/H3171/Seek/Single-track-movement.flac",
        };

        // Miniscribe 8425
        internal static IReadOnlyDictionary<string, string> Miniscribe8425 { get; } = new Dictionary<string, string>
        {
            ["HardDisk/Miniscribe/8425/Motor-running"] = "HardDisk/Miniscribe/8425/Motor-running.flac",
            ["HardDisk/Miniscribe/8425/Motor-start"] = "HardDisk/Miniscribe/8425/Motor-start.flac",
            ["HardDisk/Miniscribe/8425/Motor-stop"] = "HardDisk/Miniscribe/8425/Motor-stop.flac",
            ["HardDisk/Miniscribe/8425/Seek/Single-track-movement"] = "HardDisk/Miniscribe/8425/Seek/Single-track-movement.flac",
        };

        // Quantum Bigfoot 2110AT
        internal static IReadOnlyDictionary<string, string> QuantumBigfoot2110at { get; } = new Dictionary<string, string>
        {
            ["HardDisk/Quantum/Bigfoot 2110AT/Motor-running"] = "HardDisk/Quantum/Bigfoot 2110AT/Motor-running.flac",
            ["HardDisk/Quantum/Bigfoot 2110AT/Motor-start"] = "HardDisk/Quantum/Bigfoot 2110AT/Motor-start.flac",
            ["HardDisk/Quantum/Bigfoot 2110AT/Motor-stop"] = "HardDisk/Quantum/Bigfoot 2110AT/Motor-stop.flac",
            ["HardDisk/Quantum/Bigfoot 2110AT/Seek/Single-track-movement"] = "HardDisk/Quantum/Bigfoot 2110AT/Seek/Single-track-movement.flac",
        };

        // Seagate ST3120A
        internal static IReadOnlyDictionary<string, string> SeagateSt3120a { get; } = new Dictionary<string, string>
        {
            ["HardDisk/Seagate/ST3120A/Motor-running"] = "HardDisk/Seagate/ST3120A/Motor-running.flac",
            ["HardDisk/Seagate/ST3120A/Motor-start-with-initialization"] = "HardDisk/Seagate/ST3120A/Motor-start-with-initialization.flac",
            ["HardDisk/Seagate/ST3120A/Motor-start"] = "HardDisk/Seagate/ST3120A/Motor-start.flac",
            ["HardDisk/Seagate/ST3120A/Motor-stop"] = "HardDisk/Seagate/ST3120A/Motor-stop.flac",
            ["HardDisk/Seagate/ST3120A/Seek/Single-track-movement"] = "HardDisk/Seagate/ST3120A/Seek/Single-track-movement.flac",
        };

        // Unidentified Hard-disk-drive
        internal static IReadOnlyDictionary<string, string> UnidentifiedHardDiskDrive { get; } = new Dictionary<string, string>
        {
            ["HardDisk/Unidentified/Hard-disk-drive/Motor-running"] = "HardDisk/Unidentified/Hard-disk-drive/Motor-running.flac",
            ["HardDisk/Unidentified/Hard-disk-drive/Motor-start"] = "HardDisk/Unidentified/Hard-disk-drive/Motor-start.flac",
            ["HardDisk/Unidentified/Hard-disk-drive/Motor-stop"] = "HardDisk/Unidentified/Hard-disk-drive/Motor-stop.flac",
            ["HardDisk/Unidentified/Hard-disk-drive/Seek/Head-movement"] = "HardDisk/Unidentified/Hard-disk-drive/Seek/Head-movement.flac",
            ["HardDisk/Unidentified/Hard-disk-drive/Seek/Long-head-movement"] = "HardDisk/Unidentified/Hard-disk-drive/Seek/Long-head-movement.flac",
        };

        internal static IReadOnlyList<IReadOnlyDictionary<string, string>> Catalogs { get; } =
        [
            IbmH3171,
            Miniscribe8425,
            QuantumBigfoot2110at,
            SeagateSt3120a,
            UnidentifiedHardDiskDrive,
        ];
    }
}
