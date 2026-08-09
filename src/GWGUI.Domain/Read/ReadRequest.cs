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
