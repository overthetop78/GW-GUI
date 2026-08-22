using GWGUI.Domain.Commands.Options;
namespace GWGUI.Domain.Formats;

public static class BuiltInDiskDefinitions
{
    private static readonly HashSet<string> DefinedFormats = new(StringComparer.OrdinalIgnoreCase)
    {
        "atarist.810",
        "amstrad.cpc",
        "amstrad.pcw",
        "ucsd.ibm.mfm"
    };

    public static bool Supports(string? formatId) =>
        !string.IsNullOrWhiteSpace(formatId) && DefinedFormats.Contains(formatId);

    public static string FilePath =>
        Path.Combine(AppContext.BaseDirectory, "Assets", "DiskDefinitions", "built-in.cfg");

    public static void AddArgumentIfRequired(List<string> arguments, string? formatId, IReadOnlyList<EnabledOption>? options)
    {
        if (!Supports(formatId) || options?.Any(option => option.Argument.Equals("--diskdefs", StringComparison.OrdinalIgnoreCase)) == true)
            return;

        arguments.Add("--diskdefs");
        arguments.Add(FilePath);
    }
}
