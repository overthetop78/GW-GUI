namespace GWGUI.Emulation.Contracts;

internal sealed record EmulationSavedStateFile<TModel, TCore>(
    EmulationSavedStateHeader<TModel, TCore> Header,
    byte[] State);
