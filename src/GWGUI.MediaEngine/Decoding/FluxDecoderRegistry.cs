using System.Collections.Frozen;
using GWGUI.MediaEngine.Flux;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Enregistre les décodeurs de flux disponibles et sélectionne leurs résultats.</summary>
/// <remarks>Les identifiants sont comparés ordinalement. Le cache est attaché faiblement à chaque instance de révolution et indexé par l'identifiant canonique. Les tris sont stables : le premier élément du catalogue ou la première révolution départage une égalité complète.</remarks>
public sealed class FluxDecoderRegistry
{
    private readonly System.Runtime.CompilerServices.ConditionalWeakTable<FluxRevolution, System.Collections.Concurrent.ConcurrentDictionary<string, Lazy<FluxDecodeResult>>> _cache = new();
    private readonly IReadOnlyDictionary<string, IFluxDecoder> decodersById;
    private readonly IReadOnlyList<IFluxDecoder> automaticDecoders;

    /// <summary>Initialise le registre avec le catalogue de décodeurs par défaut.</summary>
    public FluxDecoderRegistry() : this(FluxDecoderCatalog.CreateDefault()) { }

    /// <summary>Initialise le registre avec une collection explicite de décodeurs.</summary>
    /// <param name="decoders">Décodeurs à copier et à indexer.</param>
    /// <exception cref="ArgumentNullException">La collection est nulle.</exception>
    /// <exception cref="ArgumentException">La collection est vide ou contient un décodeur nul ou un identifiant invalide.</exception>
    /// <exception cref="InvalidOperationException">Deux décodeurs possèdent le même identifiant ordinal.</exception>
    public FluxDecoderRegistry(IReadOnlyList<IFluxDecoder> decoders)
    {
        ArgumentNullException.ThrowIfNull(decoders);
        if (decoders.Count == 0) throw FluxDecoderRegistryExceptions.EmptyCollection(nameof(decoders));
        for (var index = 0; index < decoders.Count; index++)
        {
            var decoder = decoders[index];
            if (decoder is null) throw FluxDecoderRegistryExceptions.NullDecoder(index, nameof(decoders));
            if (string.IsNullOrWhiteSpace(decoder.Id)) throw FluxDecoderRegistryExceptions.InvalidIdentifier(index, nameof(decoders));
        }
        Decoders = Array.AsReadOnly(decoders.ToArray());
        automaticDecoders = Array.AsReadOnly(Decoders.Where(decoder => decoder is not AppleLisaFileWareGcrDecoder).ToArray());
        var byId = new Dictionary<string, IFluxDecoder>(StringComparer.Ordinal);
        foreach (var decoder in Decoders)
        {
            if (!byId.TryAdd(decoder.Id, decoder)) throw FluxDecoderRegistryExceptions.DuplicateIdentifier(decoder.Id);
        }
        decodersById = byId.ToFrozenDictionary(StringComparer.Ordinal);
    }

    /// <summary>Obtient la copie non modifiable des décodeurs dans l'ordre stable du catalogue.</summary>
    public IReadOnlyList<IFluxDecoder> Decoders { get; }
    /// <summary>Décode une révolution avec tous les décodeurs et retient le meilleur résultat.</summary>
    /// <param name="revolution">Révolution SCP à analyser.</param><returns>Résultat ayant obtenu le meilleur score automatique.</returns>
    public FluxDecodeResult DecodeAutomatic(FluxRevolution revolution)
    {
        var results = automaticDecoders.Select(decoder => Decode(decoder.Id, revolution));
        var ordered = results.OrderByDescending(FluxDecoderScoring.Calculate).ThenByDescending(result => result.Confidence).ThenByDescending(result => result.Structures.Count);
        return ordered.First();
    }
    /// <summary>Décode une révolution avec le décodeur identifié et met le résultat en cache.</summary>
    /// <param name="id">Identifiant du décodeur.</param><param name="revolution">Révolution SCP à analyser.</param><returns>Résultat du décodeur demandé.</returns>
    /// <exception cref="KeyNotFoundException">Aucun décodeur ne possède l'identifiant demandé.</exception>
    public FluxDecodeResult Decode(string id, FluxRevolution revolution)
    {
        if (!decodersById.TryGetValue(id, out var decoder)) throw FluxDecoderRegistryExceptions.IdentifierNotFound(id);
        var results = GetOrCreateRevolutionCache(revolution);
        var deferred = results.GetOrAdd(decoder.Id, _ => CreateDeferredResult(decoder, revolution));
        return deferred.Value;
    }
    /// <summary>Sélectionne le meilleur résultat parmi plusieurs révolutions.</summary>
    /// <param name="revolutions">Révolutions à comparer dans leur ordre d'origine.</param><param name="decoderId">Identifiant imposé, ou valeur nulle pour la sélection automatique.</param><returns>Sélection retenue, ou <see langword="null"/> si la collection est vide.</returns>
    public FluxDecodeSelection? DecodeBest(IReadOnlyList<FluxRevolution> revolutions, string? decoderId = null)
    {
        if (revolutions.Count == 0) return null;
        var candidates = revolutions.Select((revolution, index) => new FluxDecodeSelection(index, decoderId is null ? DecodeAutomatic(revolution) : Decode(decoderId, revolution)));
        if (decoderId is null)
        {
            var ordered = candidates.OrderByDescending(candidate => FluxDecoderScoring.Calculate(candidate.Result)).ThenByDescending(candidate => candidate.Result.Confidence).ThenByDescending(candidate => candidate.Result.Structures.Count);
            return ordered.First();
        }
        var explicitlyOrdered = candidates.OrderByDescending(candidate => FluxDecoderScoring.CalculateExplicit(candidate.Result));
        return explicitlyOrdered.First();
    }

    /// <summary>Obtient ou crée le cache faible associé à une révolution.</summary>
    /// <param name="revolution">Révolution utilisée comme clé faible.</param><returns>Cache ordinal des résultats différés.</returns>
    private System.Collections.Concurrent.ConcurrentDictionary<string, Lazy<FluxDecodeResult>> GetOrCreateRevolutionCache(FluxRevolution revolution) => _cache.GetValue(revolution, _ => new(StringComparer.Ordinal));
    /// <summary>Crée un résultat différé exécuté une seule fois en accès concurrent.</summary>
    /// <param name="decoder">Décodeur à exécuter.</param><param name="revolution">Révolution à décoder.</param><returns>Résultat différé synchronisé.</returns>
    private static Lazy<FluxDecodeResult> CreateDeferredResult(IFluxDecoder decoder, FluxRevolution revolution) => new(() => decoder.Decode(revolution), LazyThreadSafetyMode.ExecutionAndPublication);
}
