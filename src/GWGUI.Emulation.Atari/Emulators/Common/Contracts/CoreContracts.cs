namespace GWGUI.Emulation.Atari.Emulators.Common.Contracts;

internal sealed record HostError(string Type, string Message, ErrorCategory? Category, ErrorCode? Code, IReadOnlyDictionary<string, string> Context, bool IsLocalized);
