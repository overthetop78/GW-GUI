namespace GWGUI.Emulation.Contracts;

/// <summary>Modèle de lecteur et format de son image vierge lorsque sa création est prise en charge.</summary>
public sealed record FloppyDriveModelChoice(string Value, string DisplayResourceKey, string? InvariantDisplayValue = null, long BlankImageSize = 0)
{
    /// <summary>Formats vierges proposés pour ce modèle, indexés par leur extension ; le premier est sélectionné par défaut.</summary>
    /// <remarks>Chaque valeur est un identifiant du catalogue MediaEngine. L'absence de formats conserve
    /// la création brute déclarée par la taille et les options du périphérique.</remarks>
    public IReadOnlyDictionary<string, string>? BlankFormats { get; init; }
}
