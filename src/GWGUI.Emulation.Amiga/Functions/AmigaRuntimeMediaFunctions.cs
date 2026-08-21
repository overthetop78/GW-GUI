using System.Security.Cryptography;
using System.Text;
using GWGUI.MediaEngine.Composition;

namespace GWGUI.Emulation.Amiga;

public static class AmigaRuntimeMediaFunctions
{
    public static async ValueTask<EmulationMedia> PrepareMediaAsync(EmulationMedia media,
        string conversionDirectory)
    {
        var path = await PreparePathAsync(media.Path, conversionDirectory).ConfigureAwait(false);
        return media with { Path = path };
    }

    public static async Task<AmigaMachineConfiguration> PrepareConfigurationAsync(
        AmigaMachineConfiguration configuration, string conversionDirectory)
    {
        var initial = string.IsNullOrWhiteSpace(configuration.InitialDiskPath)
            ? configuration.InitialDiskPath
            : await PreparePathAsync(configuration.InitialDiskPath, conversionDirectory).ConfigureAwait(false);
        if (configuration.Media is not { Count: > 0 })
            return configuration with { InitialDiskPath = initial };
        var media = new List<AmigaMediaConfiguration>(configuration.Media.Count);
        foreach (var item in configuration.Media)
            media.Add(item with
            {
                Path = await PreparePathAsync(item.Path, conversionDirectory).ConfigureAwait(false)
            });
        return configuration with { InitialDiskPath = initial, Media = media };
    }

    private static async Task<string> PreparePathAsync(string path, string conversionDirectory)
    {
        if (!Path.GetExtension(path).Equals(".scp", StringComparison.OrdinalIgnoreCase)) return path;
        var info = new FileInfo(path);
        var identity = $"{Path.GetFullPath(path)}|{info.Length}|{info.LastWriteTimeUtc.Ticks}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity)))[..16];
        Directory.CreateDirectory(conversionDirectory);
        var output = Path.Combine(conversionDirectory, $"{Path.GetFileNameWithoutExtension(path)}-{hash}.adf");
        if (File.Exists(output)) return output;
        var converter = MediaEngineFactory.CreateAmigaAdfConversionService();
        try { await converter.ConvertDetectedAsync(path, output).ConfigureAwait(false); }
        catch
        {
            if (File.Exists(output)) File.Delete(output);
            throw;
        }
        return output;
    }
}
