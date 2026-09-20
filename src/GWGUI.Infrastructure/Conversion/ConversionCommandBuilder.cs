using GWGUI.Infrastructure.Commands;
using EnabledOption = global::GWGUI.MediaEngine.Commands.Options.EnabledOption;
using GwOptionValidator = global::GWGUI.Infrastructure.Commands.Options.GwOptionValidator;
using GWGUI.MediaEngine.Formats;
using GWGUI.MediaEngine.Conversion;
namespace GWGUI.Infrastructure.Conversion;

public static class ConversionCommandBuilder
{
    public static GwCommand Build(
        string executable,
        string source,
        ConversionOutput output,
        IReadOnlyList<EnabledOption>? options = null,
        string? expertArguments = null)
    {
        if (options is not null) GwOptionValidator.Validate(options);
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
        return new GwCommand(executable, "convert", arguments);
    }
}
