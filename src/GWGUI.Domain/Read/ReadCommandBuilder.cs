using GWGUI.Domain.Commands;
using GWGUI.Domain.Commands.Options;
using GWGUI.Domain.Formats;
namespace GWGUI.Domain.Read;

public static class ReadCommandBuilder
{
    public static GwCommand Build(ReadRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DestinationPath)) throw new ArgumentException("A destination is required.");
        GwOptionValidator.Validate(request.Options);
        var arguments = new List<string>();
        BuiltInDiskDefinitions.AddArgumentIfRequired(arguments, request.FormatId, request.Options);
        Add(arguments, "--device", request.Device);
        Add(arguments, "--drive", request.Drive);
        if (request.ResultKind == ReadResultKind.KnownFormat)
        {
            if (string.IsNullOrWhiteSpace(request.FormatId)) throw new ArgumentException("A known disk format is required.");
            Add(arguments, "--format", request.FormatId);
        }
        foreach (var option in request.Options)
        {
            arguments.Add(option.Argument);
            if (!string.IsNullOrWhiteSpace(option.Value)) arguments.Add(option.Value);
        }
        if (!string.IsNullOrWhiteSpace(request.ExpertArguments))
            arguments.AddRange(CommandLineTokenizer.Tokenize(request.ExpertArguments));
        arguments.Add(request.DestinationPath);
        return new GwCommand(request.GwExecutable, "read", arguments);
    }

    private static void Add(List<string> arguments, string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        arguments.Add(name);
        arguments.Add(value);
    }
}
