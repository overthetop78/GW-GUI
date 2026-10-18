namespace GWGUI.Emulation.Contracts;

internal sealed record HostError<TCategory, TCode>(
    string Type,
    string Message,
    TCategory? Category,
    TCode? Code,
    IReadOnlyDictionary<string, string> Context,
    bool IsLocalized)
    where TCategory : struct
    where TCode : struct;
