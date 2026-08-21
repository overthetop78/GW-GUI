namespace GWGUI.Emulation;

public sealed record EmulationMessage(
    EmulationMessageCategory Category,
    EmulationMessageCode MessageCode,
    EmulationMessageSeverity Severity,
    EmulationMessageTarget Target,
    IEmulationMessageContext? Context = null,
    string? OriginalText = null);
