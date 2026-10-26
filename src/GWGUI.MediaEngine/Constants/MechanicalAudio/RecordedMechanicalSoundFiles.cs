using System.Collections.Generic;

namespace GWGUI.MediaEngine.Constants;

internal static partial class RecordedMechanicalSoundFiles
{
    internal static IReadOnlyList<IReadOnlyList<IReadOnlyDictionary<string, string>>> Catalogs { get; } =
    [
        Floppy.Catalogs,
        HardDisk.Catalogs,
        Optical.Catalogs,
    ];

    internal static IReadOnlyDictionary<string, string> BySampleId { get; } = Aggregate();

    private static IReadOnlyDictionary<string, string> Aggregate()
    {
        var result = new Dictionary<string, string>();
        foreach (var device in Catalogs)
            foreach (var model in device)
                foreach (var sample in model)
                    result.Add(sample.Key, sample.Value);
        return result;
    }
}
