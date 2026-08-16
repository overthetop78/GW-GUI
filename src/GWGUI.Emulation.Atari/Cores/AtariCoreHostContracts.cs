namespace GWGUI.Emulation.Atari.Cores;

internal enum AtariHostCommand : byte
{
    Initialize = 1,
    RunFrame = 2,
    HardReset = 3,
    Stop = 4,
    InsertMedia = 5,
    EjectMedia = 6,
    SaveState = 7,
    LoadState = 8,
    SetOption = 9,
    SelectDisk = 10,
    Dispose = 11
}

internal enum AtariHostResponseStatus : byte
{
    Success = 1,
    Failure = 2
}

internal sealed record AtariHostError(
    string Type,
    string Message,
    AtariErrorKind? Kind,
    AtariErrorCode? Code,
    IReadOnlyDictionary<string, string> Context);
