namespace GWGUI.MediaEngine.Encoding;

/// <summary>Fournit les validations et la construction du résultat communes aux encodeurs de pistes.</summary>
public abstract class TrackEncoderBase : ITrackEncoder
{
    /// <summary>Obtient l'identifiant technique stable de l'encodeur.</summary>
    public abstract string Id { get; }
    /// <summary>Obtient le nom de l'encodeur destiné à l'affichage.</summary>
    public abstract string DisplayName { get; }

    /// <summary>Valide puis encode une piste logique complète.</summary>
    /// <param name="request">Description de la piste et de ses secteurs.</param>
    /// <returns>Piste encodée et révolution de flux correspondante.</returns>
    /// <exception cref="ArgumentNullException">La requête est nulle.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Le cylindre ou la face se situe hors des limites communes.</exception>
    /// <exception cref="ArgumentException">La piste ne contient aucun secteur.</exception>
    /// <exception cref="InvalidOperationException">L'encodeur n'a produit aucune cellule binaire.</exception>
    public EncodedTrack Encode(TrackEncodeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Cylinder is < TrackEncodingLimits.MinimumCylinder or > TrackEncodingLimits.MaximumCylinder) throw TrackEncodingExceptions.InvalidCylinder(request.Cylinder);
        if (request.Head is < TrackEncodingLimits.MinimumHead or > TrackEncodingLimits.MaximumHead) throw TrackEncodingExceptions.InvalidHead(request.Head);
        if (request.Sectors.Count == 0) throw TrackEncodingExceptions.MissingSectors(request.Sectors.Count);
        var bits = EncodeBits(request);
        if (bits.Count == 0) throw TrackEncodingExceptions.EmptyTrack(Id, bits.Count);
        return new(Id, bits, TrackEncoding.ToRevolution(bits, request.BitCellTicks, request.IndexTimeTicks));
    }

    /// <summary>Produit les cellules binaires propres au format de l'encodeur.</summary>
    /// <param name="request">Requête déjà validée par les règles communes.</param>
    /// <returns>Cellules binaires dans leur ordre d'émission.</returns>
    protected abstract IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request);

    /// <summary>Lit un attribut de piste ou retourne sa valeur de repli.</summary>
    /// <param name="request">Requête portant les attributs.</param>
    /// <param name="key">Clé technique définie par le format.</param>
    /// <param name="fallback">Valeur retournée lorsque la clé est absente.</param>
    /// <returns>Valeur de l'attribut ou valeur de repli.</returns>
    protected static int Attribute(TrackEncodeRequest request, string key, int fallback) => request.Attributes is not null && request.Attributes.TryGetValue(key, out var value) ? value : fallback;

    /// <summary>Lit un attribut de secteur ou retourne sa valeur de repli.</summary>
    /// <param name="sector">Secteur portant les attributs.</param>
    /// <param name="key">Clé technique définie par le format.</param>
    /// <param name="fallback">Valeur retournée lorsque la clé est absente.</param>
    /// <returns>Valeur de l'attribut ou valeur de repli.</returns>
    protected static int Attribute(TrackSector sector, string key, int fallback) => sector.Attributes is not null && sector.Attributes.TryGetValue(key, out var value) ? value : fallback;
}
