using System.Security.Cryptography;
using System.Text;
using GWGUI.MediaEngine.Composition;

namespace GWGUI.Emulation.Amiga.Functions;

public static class AmigaRuntimeMediaFunctions
{
    public static ValueTask<EmulationMedia> PrepareMediaAsync(EmulationMedia media,
        string conversionDirectory) => ValueTask.FromResult(media);

    public static Task<AmigaMachineConfiguration> PrepareConfigurationAsync(
        AmigaMachineConfiguration configuration, string conversionDirectory) =>
        Task.FromResult(configuration);

    internal static async Task<string> ConvertScpPathAsync(string path, string conversionDirectory)
    {
        if (!Path.GetExtension(path).Equals(AmigaRuntimeMediaFunctionsConstants.Scp, StringComparison.OrdinalIgnoreCase)) return path;
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
