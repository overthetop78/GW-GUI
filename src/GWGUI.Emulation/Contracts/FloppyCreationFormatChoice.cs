namespace GWGUI.Emulation.Contracts;

/// <summary>Choix de création décrit par le service média, sans exposer sa géométrie interne.</summary>
public sealed record FloppyCreationFormatChoice(string FormatId, string Extension, string DisplayName, long Capacity)
{
    /// <summary>Géométrie de l'image vierge pour sa présentation localisée.</summary>
    public int Tracks { get; init; }
    public int Sides { get; init; }
    public bool DoubleDensity { get; init; }
}
