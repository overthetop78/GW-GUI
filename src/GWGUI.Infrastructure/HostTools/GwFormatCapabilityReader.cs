using GWGUI.Domain.Formats;
using GWGUI.Domain.Formats.Parsing;
using System.Diagnostics;

namespace GWGUI.Infrastructure.HostTools;

public sealed class GwFormatCapabilityReader : IGwFormatCapabilityReader
{
    public async Task<GwFormatCapabilities> ReadAsync(string executablePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath)) return GwFormatCapabilities.Unknown;
        using var process = new Process { StartInfo = new ProcessStartInfo
        {
            FileName = executablePath, UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true
        }};
        process.StartInfo.ArgumentList.Add("read");
        process.StartInfo.ArgumentList.Add("--help");
        try
        {
            if (!process.Start()) return GwFormatCapabilities.Unknown;
            var output = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var error = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            return GwFormatCapabilitiesParser.ParseReadHelp((await output) + Environment.NewLine + (await error));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return GwFormatCapabilities.Unknown;
        }
    }
}
