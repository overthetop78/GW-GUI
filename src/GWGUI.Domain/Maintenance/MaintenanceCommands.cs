using GWGUI.Domain.Commands;
using GWGUI.Domain.Read;

namespace GWGUI.Domain.Maintenance;

public sealed record EraseRequest(string Executable, IReadOnlyList<EnabledOption> Options, string? Device = null, string? Drive = null, string? ExpertArguments = null);
public sealed record CleanRequest(string Executable, int? Cylinders = null, int? Passes = null, int? LingerMilliseconds = null, string? Device = null, string? Drive = null, string? ExpertArguments = null);

public static class MaintenanceCommandBuilder
{
    public static GwCommand Erase(EraseRequest request)
    {
        var args = Common(request.Device, request.Drive);
        foreach (var option in request.Options) { args.Add(option.Argument); if (!string.IsNullOrWhiteSpace(option.Value)) args.Add(option.Value); }
        AddExpert(args, request.ExpertArguments);
        return new GwCommand(request.Executable, "erase", args);
    }

    public static GwCommand Clean(CleanRequest request)
    {
        var args = Common(request.Device, request.Drive);
        Add(args, "--cylinders", request.Cylinders); Add(args, "--passes", request.Passes); Add(args, "--linger", request.LingerMilliseconds);
        AddExpert(args, request.ExpertArguments);
        return new GwCommand(request.Executable, "clean", args);
    }

    private static List<string> Common(string? device, string? drive) { var args = new List<string>(); if (!string.IsNullOrWhiteSpace(device)) { args.Add("--device"); args.Add(device); } if (!string.IsNullOrWhiteSpace(drive)) { args.Add("--drive"); args.Add(drive); } return args; }
    private static void Add(List<string> args, string name, int? value) { if (value is not null) { args.Add(name); args.Add(value.Value.ToString()); } }
    private static void AddExpert(List<string> args, string? value) { if (!string.IsNullOrWhiteSpace(value)) args.AddRange(CommandLineTokenizer.Tokenize(value)); }
}
