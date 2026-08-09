using GWGUI.Domain.Read;

namespace GWGUI.Domain.Write;

public enum FormatConfidence { Certain, Inferred, Ambiguous, Manual }

public sealed record WriteRequest(string GwExecutable, string SourcePath, string? FormatId, IReadOnlyList<EnabledOption> Options, bool DisableVerify = false, string? Device = null, string? Drive = null, string? ExpertArguments = null);
