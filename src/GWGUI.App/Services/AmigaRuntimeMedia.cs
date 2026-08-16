using System.IO;
using System.Security.Cryptography;
using System.Text;
using GWGUI.Emulation.Amiga;
using GWGUI.MediaEngine.Composition;

namespace GWGUI.App.Services;

internal static class AmigaRuntimeMedia
{
    internal static async Task<string> PrepareAsync(string path)
    {
        if (!Path.GetExtension(path).Equals(".scp", StringComparison.OrdinalIgnoreCase)) return path;
        var info = new FileInfo(path);
        var identity = $"{Path.GetFullPath(path)}|{info.Length}|{info.LastWriteTimeUtc.Ticks}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity)))[..16];
        var folder = Path.Combine(Path.GetTempPath(), "GW GUI", "Emulation", "Amiga", "Converted");
        Directory.CreateDirectory(folder);
        var output = Path.Combine(folder, $"{Path.GetFileNameWithoutExtension(path)}-{hash}.adf");
        if (File.Exists(output)) return output;
        var converter = MediaEngineFactory.CreateAmigaAdfConversionService();
        try
        {
            await converter.ConvertDetectedAsync(path, output);
        }
        catch
        {
            if (File.Exists(output)) File.Delete(output);
            throw;
        }
        return output;
    }

    internal static async Task<AmigaMachineConfiguration> PrepareConfigurationAsync(
        AmigaMachineConfiguration configuration)
    {
        var initial = string.IsNullOrWhiteSpace(configuration.InitialDiskPath)
            ? configuration.InitialDiskPath : await PrepareAsync(configuration.InitialDiskPath);
        if (configuration.Media is not { Count: > 0 }) return configuration with { InitialDiskPath = initial };
        var media = new List<AmigaMediaConfiguration>(configuration.Media.Count);
        foreach (var item in configuration.Media)
            media.Add(item with { Path = await PrepareAsync(item.Path) });
        return configuration with { InitialDiskPath = initial, Media = media };
    }
}
