
using GWGUI.MediaEngine.Images.Models.Flux;

namespace GWGUI.MediaEngine.Images.Writing.Encoding;

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
    /// <remarks>Cette méthode applique uniquement les validations communes ; chaque encodeur conserve les contraintes propres à son format.</remarks>
    public EncodedTrack Encode(TrackEncodeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Cylinder is < TrackEncodingLimits.MinimumCylinder or > TrackEncodingLimits.MaximumCylinder) throw TrackEncodingExceptions.InvalidCylinder(request.Cylinder);
        if (request.Head is < TrackEncodingLimits.MinimumHead or > TrackEncodingLimits.MaximumHead) throw TrackEncodingExceptions.InvalidHead(request.Head);
        if (request.Sectors.Count < TrackEncodingLimits.MinimumSectorCount) throw TrackEncodingExceptions.MissingSectors(request.Sectors.Count);
        if (request.BitCellTicks == 0) throw TrackEncodingExceptions.ZeroBitCell(request.BitCellTicks);
        if (request.IndexTimeTicks == 0) throw TrackEncodingExceptions.ZeroIndexTime(request.IndexTimeTicks);
        var bits = EncodeBits(request);
        if (bits.Count == 0) throw TrackEncodingExceptions.EmptyTrack(Id, bits.Count);
        return new(Id, bits, FluxRevolutionFactory.Create(bits, request.BitCellTicks, request.IndexTimeTicks));
    }

    /// <summary>Produit les cellules binaires propres au format de l'encodeur.</summary>
    /// <param name="request">Requête déjà validée par les règles communes.</param>
    /// <returns>Cellules binaires dans leur ordre d'émission.</returns>
    protected abstract IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request);

    /// <summary>Parcourt les secteurs ISO en conservant les opérations propres au format FM ou MFM.</summary>
    /// <param name="request">Piste déjà validée par les règles communes.</param>
    /// <param name="validateAddress">Validation des champs d'adresse du format.</param>
    /// <param name="resolveSizeCode">Calcul et validation de la taille sectorielle.</param>
    /// <param name="writeAddress">Écriture de l'adresse et de son CRC.</param>
    /// <param name="writeData">Écriture des données, de leur CRC et du gap.</param>
    /// <returns>Cellules binaires dans l'ordre des secteurs fournis.</returns>
    protected static IReadOnlyList<bool> EncodeIsoBits(TrackEncodeRequest request,
        Action<string, int> validateAddress, Func<TrackSector, byte> resolveSizeCode,
        Action<List<bool>, TrackEncodeRequest, int, byte> writeAddress, Action<List<bool>, TrackSector> writeData)
    {
        var bits = TrackBitEncoding.Bits();
        validateAddress(nameof(request.Cylinder), request.Cylinder);
        validateAddress(nameof(request.Head), request.Head);
        foreach (var sector in request.Sectors)
        {
            validateAddress(nameof(sector.Number), sector.Number);
            var sizeCode = resolveSizeCode(sector);
            writeAddress(bits, request, sector.Number, sizeCode);
            writeData(bits, sector);
        }
        return bits;
    }

    /// <summary>Lit un attribut de piste ou retourne sa valeur de repli.</summary>
    /// <param name="request">Requête portant les attributs.</param>
    /// <param name="key">Clé technique définie par le format.</param>
    /// <param name="fallback">Valeur retournée lorsque la clé est absente.</param>
    /// <returns>Valeur de l'attribut ou valeur de repli.</returns>
    protected static int Attribute(TrackEncodeRequest request, string key, int fallback) => Attribute(request.Attributes, key, fallback);

    /// <summary>Lit un attribut de secteur ou retourne sa valeur de repli.</summary>
    /// <param name="sector">Secteur portant les attributs.</param>
    /// <param name="key">Clé technique définie par le format.</param>
    /// <param name="fallback">Valeur retournée lorsque la clé est absente.</param>
    /// <returns>Valeur de l'attribut ou valeur de repli.</returns>
    protected static int Attribute(TrackSector sector, string key, int fallback) => Attribute(sector.Attributes, key, fallback);

    /// <summary>Lit une valeur dans un dictionnaire d'attributs ou retourne sa valeur de repli.</summary>
    /// <param name="attributes">Dictionnaire optionnel portant les attributs.</param>
    /// <param name="key">Clé technique définie par le format.</param>
    /// <param name="fallback">Valeur retournée lorsque le dictionnaire ou la clé est absent.</param>
    /// <returns>Valeur de l'attribut ou valeur de repli.</returns>
    private static int Attribute(IReadOnlyDictionary<string, int>? attributes, string key, int fallback) => attributes is not null && attributes.TryGetValue(key, out var value) ? value : fallback;
}
