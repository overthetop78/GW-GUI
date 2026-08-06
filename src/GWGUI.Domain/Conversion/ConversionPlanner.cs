using GWGUI.Domain.Formats;

namespace GWGUI.Domain.Conversion;

public sealed record ConversionSelection(string FormatId, IReadOnlySet<string> ExplicitExtensions);
public sealed record ConversionOutput(string FormatId, string Extension, string OutputPath, bool UsesImplicitExtension);

public static class ConversionCommandBuilder
{
    public static Commands.GwCommand Build(string executable, string source, ConversionOutput output, IReadOnlyList<Read.EnabledOption>? options = null, string? expertArguments = null)
    {
        if (options is not null) Commands.GwOptionValidator.Validate(options);
        var arguments = new List<string>();
        var formatArgument = GwFormatArgument.FromCatalogId(output.FormatId);
        if (formatArgument is not null) arguments.AddRange(["--format", formatArgument]);
        if (options is not null) foreach (var option in options) { arguments.Add(option.Argument); if (!string.IsNullOrWhiteSpace(option.Value)) arguments.Add(option.Value); }
        if (!string.IsNullOrWhiteSpace(expertArguments)) arguments.AddRange(Read.CommandLineTokenizer.Tokenize(expertArguments));
        arguments.Add(source); arguments.Add(output.OutputPath);
        return new Commands.GwCommand(executable, "convert", arguments);
    }
}

public sealed class ConversionPlanner(IImageFormatCatalog catalog)
{
    public IReadOnlyList<ConversionOutput> Plan(string sourcePath, string destinationFolder, string outputBaseName, IEnumerable<ConversionSelection> selections, bool addTags, string tagPattern = " [{FAMILY}-{FORMAT}]")
    {
        var sourceExtension = Path.GetExtension(sourcePath);
        var compatible = catalog.GetCompatibleOutputs(sourceExtension).ToDictionary(x => x.Id);
        var outputs = new List<ConversionOutput>();

        foreach (var selection in selections)
        {
            if (!compatible.TryGetValue(selection.FormatId, out var format))
                throw new InvalidOperationException($"Format '{selection.FormatId}' is incompatible with '{sourceExtension}'.");
            var extensions = selection.ExplicitExtensions.Count == 0
                ? format.Extensions.Where(x => x.IsDefault).Select(x => x.Extension)
                : selection.ExplicitExtensions;
            foreach (var extension in extensions)
            {
                var known = format.Extensions.FirstOrDefault(x => string.Equals(x.Extension, extension, StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidOperationException($"Extension '{extension}' is not valid for '{format.DisplayName}'.");
                var tag = addTags ? FormatTag(tagPattern, format, known.Extension, outputBaseName, DateTime.Now) : "";
                var baseName = addTags && tagPattern.Contains("{NAME}", StringComparison.OrdinalIgnoreCase) ? "" : outputBaseName;
                var outputPath = Path.Combine(destinationFolder, baseName + tag + known.Extension);
                outputs.Add(new ConversionOutput(format.Id, known.Extension, outputPath, selection.ExplicitExtensions.Count == 0));
            }
        }

        var duplicate = outputs.GroupBy(x => x.OutputPath, StringComparer.OrdinalIgnoreCase).FirstOrDefault(x => x.Count() > 1);
        if (duplicate is not null) throw new InvalidOperationException($"Several conversions would create '{duplicate.Key}'.");
        return outputs;
    }

    public static string FormatTag(string pattern, DiskFormat format, string extension, string sourceName, DateTime timestamp)
    {
        var supportedTokens = new[] { "{TAG}", "{NAME}", "{FAMILY}", "{FORMAT}", "{EXTENSION}", "{DATE:", "{TIME:" };
        if (string.IsNullOrWhiteSpace(pattern) || !supportedTokens.Any(token => pattern.Contains(token, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("The tag pattern must contain a supported variable.", nameof(pattern));
        var legacyTag = format.Tag ?? format.Id.ToUpperInvariant().Replace('.', '-');
        var separator = legacyTag.IndexOf('-');
        var family = separator < 0 ? legacyTag : legacyTag[..separator];
        var diskFormat = separator < 0 ? format.Id.Split('.').Last().ToUpperInvariant() : legacyTag[(separator + 1)..];
        return pattern
            .Replace("{TAG}", legacyTag, StringComparison.OrdinalIgnoreCase)
            .Replace("{FAMILY}", family, StringComparison.OrdinalIgnoreCase)
            .Replace("{FORMAT}", diskFormat, StringComparison.OrdinalIgnoreCase)
            .Replace("{EXTENSION}", extension.TrimStart('.').ToUpperInvariant(), StringComparison.OrdinalIgnoreCase)
            .Replace("{NAME}", sourceName, StringComparison.OrdinalIgnoreCase)
            .Replace("{DATE:YYYY-MM-DD}", timestamp.ToString("yyyy-MM-dd"), StringComparison.OrdinalIgnoreCase)
            .Replace("{DATE:YYYYMMDD}", timestamp.ToString("yyyyMMdd"), StringComparison.OrdinalIgnoreCase)
            .Replace("{DATE:DD-MM-YYYY}", timestamp.ToString("dd-MM-yyyy"), StringComparison.OrdinalIgnoreCase)
            .Replace("{TIME:HH-MM-SS}", timestamp.ToString("HH-mm-ss"), StringComparison.OrdinalIgnoreCase)
            .Replace("{TIME:HHMMSS}", timestamp.ToString("HHmmss"), StringComparison.OrdinalIgnoreCase)
            .Replace("{TIME:HH-MM}", timestamp.ToString("HH-mm"), StringComparison.OrdinalIgnoreCase);
    }
}
