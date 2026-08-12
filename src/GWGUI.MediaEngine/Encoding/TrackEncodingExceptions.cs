namespace GWGUI.MediaEngine.Encoding;

/// <summary>Construit les erreurs communes produites pendant l'encodage d'une piste.</summary>
internal static class TrackEncodingExceptions
{
    /// <summary>Crée l'erreur signalant une valeur propre à un format située hors de sa plage.</summary>
    /// <param name="format">Nom du format.</param>
    /// <param name="field">Nom du champ concerné.</param>
    /// <param name="actual">Valeur observée.</param>
    /// <param name="maximum">Valeur maximale admise.</param>
    /// <returns>Erreur contenant le format, le champ, la valeur et sa limite.</returns>
    public static ArgumentOutOfRangeException FormatValueOutOfRange(string format, string field, int actual, int maximum) => new(field, actual, $"{format} {field} must be between 0 and {maximum}.");

    /// <summary>Crée l'erreur signalant un caractère invalide dans une chaîne binaire.</summary>
    /// <param name="value">Caractère observé.</param>
    /// <param name="index">Position du caractère.</param>
    /// <returns>Erreur contenant le caractère et sa position.</returns>
    public static ArgumentException InvalidBinaryCharacter(char value, int index) => new($"Binary text contains '{value}' at index {index}; only '0' and '1' are allowed.", "values");

    /// <summary>Crée l'erreur signalant une longueur de gap négative.</summary>
    /// <param name="count">Longueur observée, en cellules.</param>
    /// <returns>Erreur contenant la longueur invalide.</returns>
    public static ArgumentOutOfRangeException NegativeGapLength(int count) => new(nameof(count), count, "Gap length cannot be negative.");

    /// <summary>Crée l'erreur signalant le dépassement d'un intervalle de flux.</summary>
    /// <param name="cells">Nombre de cellules de l'intervalle.</param>
    /// <param name="cellTicks">Durée d'une cellule, en ticks.</param>
    /// <returns>Erreur contenant les deux facteurs du produit.</returns>
    public static OverflowException FluxIntervalOverflow(uint cells, uint cellTicks) => new($"A flux interval of {cells} cells at {cellTicks} ticks per cell exceeds UInt32.");

    /// <summary>Crée l'erreur signalant une taille sectorielle sans code ISO correspondant.</summary>
    /// <param name="sizeBytes">Taille sectorielle observée, en octets.</param>
    /// <returns>Erreur contenant la taille non prise en charge.</returns>
    public static ArgumentException UnsupportedSectorSize(int sizeBytes) => new($"Unsupported sector size: {sizeBytes} bytes.", nameof(sizeBytes));

    /// <summary>Crée l'erreur signalant une durée de cellule binaire nulle.</summary>
    /// <param name="cellTicks">Durée observée, en ticks.</param>
    /// <returns>Erreur contenant la durée invalide.</returns>
    public static ArgumentOutOfRangeException ZeroBitCell(uint cellTicks) => new(nameof(cellTicks), cellTicks, "Bit-cell duration must be greater than zero ticks.");

    /// <summary>Crée l'erreur signalant une durée de révolution nulle.</summary>
    /// <param name="indexTimeTicks">Durée observée, en ticks.</param>
    /// <returns>Erreur contenant la durée invalide.</returns>
    public static ArgumentOutOfRangeException ZeroIndexTime(uint indexTimeTicks) => new(nameof(indexTimeTicks), indexTimeTicks, "Index duration must be greater than zero ticks.");

    /// <summary>Crée l'erreur signalant un cylindre situé hors des limites communes.</summary>
    /// <param name="cylinder">Numéro de cylindre observé.</param>
    /// <returns>Erreur contenant la valeur et les limites admises.</returns>
    public static ArgumentOutOfRangeException InvalidCylinder(int cylinder) => new(nameof(cylinder), cylinder, $"Cylinder must be between {TrackEncodingLimits.MinimumCylinder} and {TrackEncodingLimits.MaximumCylinder}.");

    /// <summary>Crée l'erreur signalant une face située hors des limites communes.</summary>
    /// <param name="head">Numéro de face observé.</param>
    /// <returns>Erreur contenant la valeur et les limites admises.</returns>
    public static ArgumentOutOfRangeException InvalidHead(int head) => new(nameof(head), head, $"Head must be between {TrackEncodingLimits.MinimumHead} and {TrackEncodingLimits.MaximumHead}.");

    /// <summary>Crée l'erreur signalant qu'une piste ne contient aucun secteur.</summary>
    /// <param name="sectorCount">Nombre de secteurs observé.</param>
    /// <returns>Erreur contenant le nombre de secteurs reçu.</returns>
    public static ArgumentException MissingSectors(int sectorCount) => new($"At least {TrackEncodingLimits.MinimumSectorCount} sector is required; received {sectorCount}.", "request");

    /// <summary>Crée l'erreur signalant qu'un encodeur n'a produit aucune cellule binaire.</summary>
    /// <param name="encoderId">Identifiant technique de l'encodeur.</param>
    /// <param name="bitCount">Nombre de cellules produites.</param>
    /// <returns>Erreur contenant l'encodeur et le nombre de cellules observé.</returns>
    public static InvalidOperationException EmptyTrack(string encoderId, int bitCount) => new($"Encoder '{encoderId}' produced {bitCount} bit cells.");
}
