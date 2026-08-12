namespace GWGUI.MediaEngine.Encoding;

/// <summary>Construit les erreurs communes produites pendant l'encodage d'une piste.</summary>
internal static class TrackEncodingExceptions
{
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
