using GWGUI.Domain.Commands;
using GWGUI.Domain.Formats;

namespace GWGUI.Domain.Conversion;

public static class ConversionCommandBuilder
{
    public static Commands.GwCommand Build(
        string executable,
        string source,
        ConversionOutput output,
        IReadOnlyList<Read.EnabledOption>? options = null,
        string? expertArguments = null)
    {
        if (options is not null) Commands.GwOptionValidator.Validate(options);
        var arguments = new List<string>();
        BuiltInDiskDefinitions.AddArgumentIfRequired(arguments, output.FormatId, options);
        var formatArgument = GwFormatArgument.FromCatalogId(output.FormatId);
        if (formatArgument is not null) arguments.AddRange(["--format", formatArgument]);
        if (options is not null)
            foreach (var option in options)
            {
                arguments.Add(option.Argument);
                if (!string.IsNullOrWhiteSpace(option.Value)) arguments.Add(option.Value);
            }
        if (!string.IsNullOrWhiteSpace(expertArguments))
            arguments.AddRange(CommandLineTokenizer.Tokenize(expertArguments));
        arguments.Add(source);
        arguments.Add(output.OutputPath);
        return new Commands.GwCommand(executable, "convert", arguments);
    }
}
