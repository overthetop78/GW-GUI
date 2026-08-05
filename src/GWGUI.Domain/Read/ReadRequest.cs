using GWGUI.Domain.Commands;

namespace GWGUI.Domain.Read;

public enum ReadResultKind { RawScp, KnownFormat }

public sealed record EnabledOption(string Argument, string? Value = null);

public sealed record ReadRequest(
    string GwExecutable,
    string DestinationPath,
    ReadResultKind ResultKind,
    string? FormatId,
    IReadOnlyList<EnabledOption> Options,
    string? Device = null,
    string? Drive = null,
    string? ExpertArguments = null);

public static class ReadCommandBuilder
{
    public static GwCommand Build(ReadRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DestinationPath)) throw new ArgumentException("A destination is required.");
        GwOptionValidator.Validate(request.Options);
        var arguments = new List<string>();
        Add(arguments, "--device", request.Device);
        Add(arguments, "--drive", request.Drive);
        if (!string.IsNullOrWhiteSpace(request.FormatId)) Add(arguments, "--format", request.FormatId);
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

public static class CommandLineTokenizer
{
    public static IReadOnlyList<string> Tokenize(string value)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        var quoted = false;
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (character == '"') { quoted = !quoted; continue; }
            if (char.IsWhiteSpace(character) && !quoted)
            {
                if (current.Length > 0) { result.Add(current.ToString()); current.Clear(); }
                continue;
            }
            current.Append(character);
        }
        if (quoted) throw new ArgumentException("An expert argument contains an unclosed quote.");
        if (current.Length > 0) result.Add(current.ToString());
        return result;
    }
}
