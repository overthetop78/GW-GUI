namespace GWGUI.Emulation.Contracts;

internal sealed record HostError(string Type, string Message, EmulationMessageCategory? Category, EmulationMessageCode? Code);
