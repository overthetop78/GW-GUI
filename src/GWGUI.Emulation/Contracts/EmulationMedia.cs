namespace GWGUI.Emulation.Contracts;

public sealed record EmulationMedia(string Path, EmulationMediaSlot Slot, EmulationMediaType Type, bool IsReadOnly, bool IsInserted)
{
    /// <summary>Indications explicites de lecture du média, notamment le profil d'un signal RAW sans en-tête.</summary>
    public IReadOnlyDictionary<string, string>? Parameters { get; init; }
}
