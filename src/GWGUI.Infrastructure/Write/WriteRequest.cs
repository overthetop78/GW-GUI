using EnabledOption = global::GWGUI.MediaEngine.Contracts.Options.EnabledOption;
namespace GWGUI.Infrastructure.Write;

public sealed record WriteRequest(string GwExecutable, string SourcePath, string? FormatId, IReadOnlyList<EnabledOption> Options, bool DisableVerify = false, string? Device = null, string? Drive = null, string? ExpertArguments = null);
