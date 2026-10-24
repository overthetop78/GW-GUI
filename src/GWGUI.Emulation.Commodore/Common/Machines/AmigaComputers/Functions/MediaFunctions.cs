using CommonCoreHostFunctions = GWGUI.Emulation.Commodore.Common.Functions.CoreHostFunctions;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using GWGUI.Emulation.HardDisks;

namespace GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Functions;

internal static class AmigaHardDiskFormats
{
    private static readonly DiskFormatRegistry Formats = DiskFormatRegistry.CreateDefault();

    // Raw single-volume hardfiles use the UAE virtual controller. Stay below
    // the classic filesystem's 2 GiB signed-offset boundary. This is a safe
    // supported profile, not a claim that all Amiga controllers stop at 2 GiB.
    internal static readonly IReadOnlyList<HardDiskImageFormat> All = Describe(
        [new(RuntimeMediaConstants.HardfileFormatId, StorageSettingsFunctionsConstants.Hdf,
            RuntimeMediaConstants.VirtualHardDiskController, RuntimeMediaConstants.MaximumHardfileSizeBytes,
            RuntimeMediaConstants.DefaultHardfileSizeBytes,
            [HardDiskPreparation.Blank, HardDiskPreparation.AmigaOfs, HardDiskPreparation.AmigaFfs,
             HardDiskPreparation.AmigaRdbOfs, HardDiskPreparation.AmigaRdbFfs]),
         new(RuntimeMediaConstants.CompressedHardfileFormatId, StorageSettingsFunctionsConstants.Hdz,
             RuntimeMediaConstants.VirtualHardDiskController, RuntimeMediaConstants.MaximumHardfileSizeBytes,
             RuntimeMediaConstants.DefaultHardfileSizeBytes,
             [HardDiskPreparation.Blank, HardDiskPreparation.AmigaOfs, HardDiskPreparation.AmigaFfs],
             GWGUI.Emulation.HardDisks.Containers.DiskContainerKind.Gzip)]);

    private static IReadOnlyList<HardDiskImageFormat> Describe(IEnumerable<HardDiskImageFormat> formats) => formats.Select(Formats.Describe).ToArray();
}

internal static class AmigaRuntimeMediaFunctions
{

    internal static async ValueTask<EmulationMedia> PrepareMediaAsync(EmulationMedia media, string conversionDirectory) => media.Type == EmulationMediaType.Floppy ? media with { Path = await ConvertScpPathAsync(media.Path, conversionDirectory).ConfigureAwait(false) } : media;

    internal static async Task<MachineConfiguration> PrepareConfigurationAsync(MachineConfiguration configuration, string conversionDirectory)
    {
        var media = new List<MediaConfiguration>();
        foreach (var item in ExternalCore.ResolveConfiguredMedia(configuration))
            media.Add(item.Category == MediaCategory.Floppy ? item with { Path = await ConvertScpPathAsync(item.Path, conversionDirectory).ConfigureAwait(false) } : item);
        return configuration with { Media = media };
    }

    internal static async Task<string> ConvertScpPathAsync(string path, string conversionDirectory)
    {
        if (!Path.GetExtension(path).Equals(StorageSettingsFunctionsConstants.Scp, StringComparison.OrdinalIgnoreCase)) return path;
        var info = new FileInfo(path);
        var identity = string.Join(RuntimeMediaConstants.ConversionIdentitySeparator, Path.GetFullPath(path), info.Length, info.LastWriteTimeUtc.Ticks);
        var hash = CommonCoreHostFunctions.StateFiles.HashBytes(Encoding.UTF8.GetBytes(identity))[..RuntimeMediaConstants.ConversionCacheHashLength];
        Directory.CreateDirectory(conversionDirectory);
        var output = Path.Combine(conversionDirectory, string.Concat(Path.GetFileNameWithoutExtension(path), RuntimeMediaConstants.ConversionFileNameSeparator, hash, StorageSettingsFunctionsConstants.Adf));
        if (File.Exists(output)) return output;

        try { await CommonCoreHostFunctions.MediaProcessing.ConvertScpAsync(path, output, StorageSettingsFunctionsConstants.Adf).ConfigureAwait(false); }
        catch
        {
            if (File.Exists(output)) File.Delete(output);
            throw;
        }
        return output;
    }
}
