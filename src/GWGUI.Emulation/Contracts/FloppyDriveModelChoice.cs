namespace GWGUI.Emulation.Contracts;

/// <summary>Modèle de lecteur et format de son image vierge lorsque sa création est prise en charge.</summary>
public sealed record FloppyDriveModelChoice(string Value, string DisplayResourceKey, string? InvariantDisplayValue = null, long BlankImageSize = 0)
{
    /// <summary>Géométrie physique destinée à la présentation localisée ; zéro conserve le libellé du module.</summary>
    public decimal DiameterInches { get; init; }
    public int Tracks { get; init; }
    public int Sides { get; init; }
    public bool DoubleDensity { get; init; }
    /// <summary>Formats et capacités compatibles, y compris plusieurs tailles pour une même extension.</summary>
    public IReadOnlyList<FloppyCreationFormatChoice> CreationFormats { get; init; } = [];
    /// <summary>Extensions issues du profil physique du service média, indépendamment de la ROM de la machine.</summary>
    public IReadOnlyList<string> AcceptedExtensions { get; init; } = [];
    /// <summary>Formats vierges proposés pour ce modèle, indexés par leur extension ; le premier est sélectionné par défaut.</summary>
    /// <remarks>Chaque valeur est un identifiant du catalogue MediaEngine. L'absence de formats conserve
    /// la création brute déclarée par la taille et les options du périphérique.</remarks>
    public IReadOnlyDictionary<string, string>? BlankFormats { get; init; }
}
