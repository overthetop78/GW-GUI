namespace GWGUI.Emulation.Contracts;

internal sealed record EmulationSavedStateHeader<TModel, TCore>(
    int FormatVersion,
    TCore Core,
    string CoreName,
    string CoreVersion,
    string CoreSha256,
    TModel Model,
    string ConfigurationSha256,
    string ContentSha256,
    string StateSha256);
