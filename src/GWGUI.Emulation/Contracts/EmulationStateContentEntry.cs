namespace GWGUI.Emulation.Contracts;

/// <summary>Identité d’un média dans une empreinte d’état : slot, catégorie et empreinte du contenu.</summary>
public sealed record EmulationStateContentEntry(string Slot, string Category, string Hash);
