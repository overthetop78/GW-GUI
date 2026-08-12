namespace GWGUI.MediaEngine.Encoding;

/// <summary>Répertorie les encodeurs de pistes disponibles et les sélectionne par identifiant technique.</summary>
public sealed class FluxEncoderRegistry
{
    private readonly IReadOnlyDictionary<string, ITrackEncoder> _encodersById;

    /// <summary>Initialise le registre avec les encodeurs fournis par MediaEngine.</summary>
    /// <exception cref="ArgumentException">Deux encodeurs possèdent le même identifiant technique.</exception>
    public FluxEncoderRegistry()
    {
        ITrackEncoder[] encoders =
        [
            new IsoMfmTrackEncoder(), new IsoFmTrackEncoder(), new AmigaMfmTrackEncoder(),
            new AppleIIGcrTrackEncoder(), new AppleRwts18TrackEncoder(), new AppleMacGcrTrackEncoder(), new AppleLisaFileWareGcrTrackEncoder(), new CommodoreGcrTrackEncoder(),
            new HpMmfmTrackEncoder(), new DataGeneralFmTrackEncoder(), new MicropolisMfmTrackEncoder(),
            new MembrainMfmTrackEncoder(), new Aed6200pMfmTrackEncoder(), new QdMo5MfmTrackEncoder(),
            new CenturionMfmTrackEncoder(), new NorthstarMfmTrackEncoder(), new HeathkitFmTrackEncoder(),
            new MicralNFmTrackEncoder(), new EmuFmTrackEncoder(), new TycomFmTrackEncoder(),
            new DecRx02TrackEncoder(), new ArburgTrackEncoder(), new Victor9kGcrTrackEncoder(),
            new Commodore900GcrTrackEncoder()
        ];
        var encodersById = new Dictionary<string, ITrackEncoder>(StringComparer.Ordinal);
        foreach (var encoder in encoders)
        {
            if (!encodersById.TryAdd(encoder.Id, encoder)) throw FluxEncoderRegistryExceptions.DuplicateEncoder(encoder.Id);
        }
        Encoders = Array.AsReadOnly(encoders);
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
