namespace GWGUI.Emulation.Atari.Contracts;



internal sealed record AtariHostError(
    string Type,
    string Message,
    AtariErrorCategory? Category,
    AtariErrorCode? Code,
    IReadOnlyDictionary<string, string> Context);
