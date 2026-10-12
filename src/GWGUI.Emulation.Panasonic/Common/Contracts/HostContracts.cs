namespace GWGUI.Emulation.Panasonic.Common.Contracts;

internal sealed record HostError(string Type, string Message, EmulationMessageCategory? Category, EmulationMessageCode? Code);
