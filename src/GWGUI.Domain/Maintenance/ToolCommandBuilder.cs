using System.Globalization;
using GWGUI.Domain.Commands;

namespace GWGUI.Domain.Maintenance;

public sealed record ToolCommandRequest(
    string Executable,
    string Verb,
    IReadOnlyDictionary<string, string> Values,
    IReadOnlySet<string> Enabled,
    string? Device = null,
    string? Drive = null);

public static class ToolCommandBuilder
{
    private static readonly HashSet<string> Supported = ["info", "bandwidth", "rpm", "seek", "pin", "reset", "delays", "update"];

    public static GwCommand Build(ToolCommandRequest request)
    {
        if (!Supported.Contains(request.Verb)) throw new ArgumentException("Unsupported tool command.", nameof(request.Verb));
        var args = request.Verb switch
        {
            "rpm" => ["--nr", Positive(request, "nr").ToString(CultureInfo.InvariantCulture)],
            "seek" => Seek(request),
            "pin" => Pin(request),
            "delays" => Delays(request),
            "update" when request.Enabled.Contains("bootloader") => ["--bootloader"],
            _ => []
        };
        if (!string.IsNullOrWhiteSpace(request.Device)) args.AddRange(["--device", request.Device]);
        if (!string.IsNullOrWhiteSpace(request.Drive) && request.Verb is "rpm" or "seek") args.AddRange(["--drive", request.Drive]);
        return new(request.Executable, request.Verb, args);
    }

    private static List<string> Seek(ToolCommandRequest request)
    {
        var args = new List<string> { NonNegative(request, "cylinder").ToString(CultureInfo.InvariantCulture) };
        if (request.Enabled.Contains("force")) args.Add("--force"); if (request.Enabled.Contains("motor-on")) args.Add("--motor-on"); return args;
    }

    private static List<string> Pin(ToolCommandRequest request)
    {
        var pin = NonNegative(request, "pin"); if (pin is not (8 or 26 or 28)) throw new ArgumentOutOfRangeException("pin", "Pin must be 8, 26 or 28.");
        var set = request.Enabled.Contains("set"); var args = new List<string> { set ? "set" : "get", pin.ToString(CultureInfo.InvariantCulture) };
        if (set) args.Add(request.Enabled.Contains("high") ? "H" : "L"); return args;
    }

    private static List<string> Delays(ToolCommandRequest request)
    {
        var args = new List<string>();
        foreach (var key in new[] { "select", "step", "settle", "motor", "watchdog", "pre-write", "post-write", "index-mask" })
            if (request.Enabled.Contains(key)) args.AddRange(["--" + key, NonNegative(request, key).ToString(CultureInfo.InvariantCulture)]);
        return args;
    }

    private static int Positive(ToolCommandRequest request, string key) { var value = Parse(request, key); if (value <= 0) throw new ArgumentOutOfRangeException(key, "Value must be greater than zero."); return value; }
    private static int NonNegative(ToolCommandRequest request, string key) { var value = Parse(request, key); if (value < 0) throw new ArgumentOutOfRangeException(key, "Value must not be negative."); return value; }
    private static int Parse(ToolCommandRequest request, string key) => request.Values.TryGetValue(key, out var text) && int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : throw new ArgumentException("Value must be an integer.", key);
}
