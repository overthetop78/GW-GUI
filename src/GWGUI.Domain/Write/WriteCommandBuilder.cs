using GWGUI.Domain.Commands;
using GWGUI.Domain.Commands.Options;
using GWGUI.Domain.Formats;
namespace GWGUI.Domain.Write;

public static class WriteCommandBuilder
{
    public static GwCommand Build(WriteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SourcePath)) throw new ArgumentException("A source image is required.");
        GwOptionValidator.Validate(request.Options);
        var arguments = new List<string>();
        BuiltInDiskDefinitions.AddArgumentIfRequired(arguments, request.FormatId, request.Options);
        Add(arguments, "--device", request.Device);
        Add(arguments, "--drive", request.Drive);
        Add(arguments, "--format", GwFormatArgument.FromCatalogId(request.FormatId));
        if (request.DisableVerify) arguments.Add("--no-verify");
        foreach (var option in request.Options)
        {
            arguments.Add(option.Argument);
            if (!string.IsNullOrWhiteSpace(option.Value)) arguments.Add(option.Value);
        }
        if (!string.IsNullOrWhiteSpace(request.ExpertArguments))
            arguments.AddRange(CommandLineTokenizer.Tokenize(request.ExpertArguments));
        arguments.Add(request.SourcePath);
        return new GwCommand(request.GwExecutable, "write", arguments);
    }

    private static void Add(List<string> values, string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        values.Add(name);
        values.Add(value);
    }
}
