namespace GWGUI.Domain.Hardware;

public sealed record GwDeviceInfo(
    string? HostToolsVersion,
    string? Port,
    string? Model,
    string? Mcu,
    string? FirmwareVersion,
    string? SerialNumber,
    string? UsbSpeed,
    bool HasNetworkWarning,
    string RawOutput);

public static class GwInfoParser
{
    public static GwDeviceInfo Parse(string output)
    {
        string? ValueAfter(params string[] labels)
        {
            foreach (var rawLine in output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
            {
                var line = rawLine.Trim();
                foreach (var label in labels)
                    if (line.StartsWith(label, StringComparison.OrdinalIgnoreCase))
                        return line[(line.IndexOf(':') + 1)..].Trim();
            }
            return null;
        }

        var port = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .FirstOrDefault(x => x.StartsWith("COM", StringComparison.OrdinalIgnoreCase));

        return new GwDeviceInfo(
            ValueAfter("Host Tools"),
            port,
            ValueAfter("Model"),
            ValueAfter("MCU"),
            ValueAfter("Firmware"),
            ValueAfter("Serial"),
            ValueAfter("USB"),
            output.Contains("github", StringComparison.OrdinalIgnoreCase) &&
            (output.Contains("error", StringComparison.OrdinalIgnoreCase) || output.Contains("failed", StringComparison.OrdinalIgnoreCase)),
            output);
    }
}
