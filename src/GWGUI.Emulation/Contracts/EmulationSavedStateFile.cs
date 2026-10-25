namespace GWGUI.Emulation.Contracts;

/// <summary>État décodé et son enveloppe ; le tableau State appartient au résultat, sans ressource native associée.</summary>
public sealed record EmulationSavedStateFile<TModel, TCore>(
    EmulationSavedStateHeader<TModel, TCore> Header,
    byte[] State);
