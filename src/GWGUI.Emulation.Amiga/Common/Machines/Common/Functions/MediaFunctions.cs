using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation;
using GWGUI.MediaEngine;
using System.Security.Cryptography;
using System.Text;

namespace GWGUI.Emulation.Amiga.Common.Machines.Common.Functions;

internal static class EmulationMediaActivityFunctions
{
    internal static IReadOnlyDictionary<EmulationMediaSlot, bool> FromLedStates(
        IReadOnlyDictionary<int, bool> ledStates)
    {
        var activity = new Dictionary<EmulationMediaSlot, bool>();
        for (var index = 0; index < 4; index++)
            activity[new EmulationMediaSlot(EmulationMediaCategory.FloppyDrive, index)] =
                ledStates.GetValueOrDefault(3 + index);

        activity[EmulationMediaSlot.HardDisk0] = ledStates.GetValueOrDefault(7);
        activity[EmulationMediaSlot.Cd0] = ledStates.GetValueOrDefault(8);
        return activity;
    }
}

internal static class EmulationMediaConversionFunctions
{
    internal static IReadOnlyList<EmulationMedia> ToCommon(
        IReadOnlyList<MediaConfiguration> media)
    {
        var indexes = new Dictionary<EmulationMediaCategory, int>();
        var result = new List<EmulationMedia>();
        foreach (var item in media)
        {
            if (!TryCategory(item.Category, out var category)) continue;
            var index = indexes.GetValueOrDefault(category);
            indexes[category] = index + 1;
            result.Add(new EmulationMedia(Path.GetFullPath(item.Path),
                new EmulationMediaSlot(category, index), ToType(item.Category), item.IsReadOnly, true));
        }
        return result;
    }

    private static bool TryCategory(MediaCategory media, out EmulationMediaCategory category)
    {
        category = media switch
        {
            MediaCategory.Floppy => EmulationMediaCategory.FloppyDrive,
            MediaCategory.HardDrive => EmulationMediaCategory.HardDisk,
            MediaCategory.CompactDisc => EmulationMediaCategory.CompactDiscDrive,
            _ => default
        };
        return media is MediaCategory.Floppy or MediaCategory.HardDrive or MediaCategory.CompactDisc;
    }

    private static EmulationMediaType ToType(MediaCategory media) => media switch
    {
        MediaCategory.Floppy => EmulationMediaType.Floppy,
        MediaCategory.HardDrive => EmulationMediaType.HardDisk,
        MediaCategory.CompactDisc => EmulationMediaType.CompactDisc,
        _ => throw new ArgumentOutOfRangeException(nameof(media), media, null)
    };
}

internal static class HardDiskFormats
{
    private static readonly DiskFormatRegistry Formats = DiskFormatRegistry.CreateDefault();

    // Raw single-volume hardfiles use the UAE virtual controller. Stay below
    // the classic filesystem's 2 GiB signed-offset boundary. This is a safe
    // supported profile, not a claim that all Amiga controllers stop at 2 GiB.
    internal static readonly IReadOnlyList<HardDiskImageFormat> All = Describe(
        [new("amiga-hdf", ".hdf", "UAE", 2L * 1024 * 1024 * 1024 - 512, 40L * 1024 * 1024,
            [HardDiskPreparation.Blank, HardDiskPreparation.AmigaOfs, HardDiskPreparation.AmigaFfs,
             HardDiskPreparation.AmigaRdbOfs, HardDiskPreparation.AmigaRdbFfs]),
         new("amiga-hdz", ".hdz", "UAE", 2L * 1024 * 1024 * 1024 - 512, 40L * 1024 * 1024,
             [HardDiskPreparation.Blank, HardDiskPreparation.AmigaOfs, HardDiskPreparation.AmigaFfs],
             GWGUI.Emulation.HardDisks.Containers.DiskContainerKind.Gzip)]);

    private static IReadOnlyList<HardDiskImageFormat> Describe(IEnumerable<HardDiskImageFormat> formats) =>
        formats.Select(Formats.Describe).ToArray();
}

public static class RuntimeMediaFunctions
{
    private static readonly MediaEngineComposition MediaEngine = MediaEngineComposition.CreateDefault();

    public static ValueTask<EmulationMedia> PrepareMediaAsync(EmulationMedia media,
        string conversionDirectory) => ValueTask.FromResult(media);

    public static Task<MachineConfiguration> PrepareConfigurationAsync(
        MachineConfiguration configuration, string conversionDirectory) =>
        Task.FromResult(configuration);

    internal static async Task<string> ConvertScpPathAsync(string path, string conversionDirectory)
    {
        if (!Path.GetExtension(path).Equals(RuntimeMediaFunctionsConstants.Scp, StringComparison.OrdinalIgnoreCase)) return path;
        var info = new FileInfo(path);
        var identity = $"{Path.GetFullPath(path)}|{info.Length}|{info.LastWriteTimeUtc.Ticks}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity)))[..16];
        Directory.CreateDirectory(conversionDirectory);
        var output = Path.Combine(conversionDirectory, $"{Path.GetFileNameWithoutExtension(path)}-{hash}.adf");
        if (File.Exists(output)) return output;
        var converter = MediaEngine.Conversion.AmigaAdfRuntimeConverter;
        try { await converter.ConvertDetectedAsync(path, output).ConfigureAwait(false); }
        catch
        {
            if (File.Exists(output)) File.Delete(output);
            throw;
        }
        return output;
    }
}
