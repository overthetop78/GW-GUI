namespace GWGUI.MediaEngine.Encoding;

/// <summary>Répertorie les encodeurs de pistes disponibles et les sélectionne par identifiant technique.</summary>
public sealed class FluxEncoderRegistry
{
    private readonly IReadOnlyDictionary<string, ITrackEncoder> _encodersById;

    /// <summary>Initialise le registre avec le catalogue fourni par MediaEngine.</summary>
    public FluxEncoderRegistry() : this(FluxEncoderCatalog.CreateDefault()) { }

    /// <summary>Initialise le registre avec une collection explicite d'encodeurs.</summary>
    /// <param name="encoders">Encodeurs à copier et à indexer dans l'ordre reçu.</param>
    /// <exception cref="ArgumentNullException">La collection est nulle.</exception>
    /// <exception cref="ArgumentException">Un encodeur est nul, son identifiant est vide ou un identifiant est dupliqué.</exception>
    public FluxEncoderRegistry(IEnumerable<ITrackEncoder> encoders)
    {
        ArgumentNullException.ThrowIfNull(encoders);
        var encoderArray = encoders.ToArray();
        var encodersById = new Dictionary<string, ITrackEncoder>(StringComparer.Ordinal);
        for (var index = 0; index < encoderArray.Length; index++)
        {
            var encoder = encoderArray[index];
            if (encoder is null) throw FluxEncoderRegistryExceptions.NullEncoder(index);
            if (string.IsNullOrWhiteSpace(encoder.Id)) throw FluxEncoderRegistryExceptions.EmptyEncoderId(index);
            if (!encodersById.TryAdd(encoder.Id, encoder)) throw FluxEncoderRegistryExceptions.DuplicateEncoder(encoder.Id);
        }
        Encoders = Array.AsReadOnly(encoderArray);
        _encodersById = encodersById;
    }

    /// <summary>Obtient les encodeurs enregistrés dans leur ordre de déclaration.</summary>
    public IReadOnlyList<ITrackEncoder> Encoders { get; }

    /// <summary>Obtient l'encodeur portant l'identifiant demandé.</summary>
    /// <param name="id">Identifiant technique exact de l'encodeur.</param>
    /// <returns>Encodeur associé à l'identifiant.</returns>
    /// <exception cref="KeyNotFoundException">Aucun encodeur ne porte cet identifiant.</exception>
    public ITrackEncoder Get(string id) => _encodersById.TryGetValue(id, out var encoder) ? encoder : throw FluxEncoderRegistryExceptions.EncoderNotFound(id);

    /// <summary>Encode une piste avec l'encodeur portant l'identifiant demandé.</summary>
    /// <param name="id">Identifiant technique exact de l'encodeur.</param>
    /// <param name="request">Piste logique à encoder.</param>
    /// <returns>Piste encodée et sa révolution de flux.</returns>
    /// <exception cref="KeyNotFoundException">Aucun encodeur ne porte cet identifiant.</exception>
    public EncodedTrack Encode(string id, TrackEncodeRequest request) => Get(id).Encode(request);
}
