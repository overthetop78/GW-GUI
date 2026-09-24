using EnabledOption = global::GWGUI.MediaEngine.Contracts.Options.EnabledOption;
namespace GWGUI.Infrastructure.Read;

public enum ReadResultKind { RawScp, KnownFormat }

public sealed record ReadRequest(
    string GwExecutable,
    string DestinationPath,
    ReadResultKind ResultKind,
    string? FormatId,
    IReadOnlyList<EnabledOption> Options,
    string? Device = null,
    string? Drive = null,
    string? ExpertArguments = null);
