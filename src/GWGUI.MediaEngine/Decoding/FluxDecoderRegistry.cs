using System.Collections.Frozen;
using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Enregistre les décodeurs de flux disponibles et sélectionne leurs résultats.</summary>
public sealed class FluxDecoderRegistry
{
    private readonly System.Runtime.CompilerServices.ConditionalWeakTable<ScpRevolution,
        System.Collections.Concurrent.ConcurrentDictionary<string, Lazy<FluxDecodeResult>>> _cache = new();
    private readonly IReadOnlyDictionary<string, IFluxDecoder> decodersById;

    public FluxDecoderRegistry() : this(FluxDecoderCatalog.CreateDefault()) { }

    public FluxDecoderRegistry(IReadOnlyList<IFluxDecoder> decoders)
    {
        ArgumentNullException.ThrowIfNull(decoders);
        if (decoders.Count == 0) throw new ArgumentException("At least one flux decoder must be registered.", nameof(decoders));
        for (var index = 0; index < decoders.Count; index++)
        {
            if (decoders[index] is null) throw new ArgumentException($"The flux decoder at position {index} is null.", nameof(decoders));
            if (string.IsNullOrWhiteSpace(decoders[index].Id)) throw new ArgumentException($"The flux decoder at position {index} has an empty identifier.", nameof(decoders));
        }
        Decoders = Array.AsReadOnly(decoders.ToArray());
        var byId = new Dictionary<string, IFluxDecoder>(StringComparer.Ordinal);
        foreach (var decoder in Decoders) if (!byId.TryAdd(decoder.Id, decoder)) throw FluxDecoderRegistryExceptions.DuplicateIdentifier(decoder.Id);
        decodersById = byId.ToFrozenDictionary(StringComparer.Ordinal);
    }

    public IReadOnlyList<IFluxDecoder> Decoders { get; }
    /// <summary>Décode une révolution avec tous les décodeurs et retient le meilleur résultat.</summary>
    /// <param name="revolution">Révolution SCP à analyser.</param><returns>Résultat ayant obtenu le meilleur score automatique.</returns>
    public FluxDecodeResult DecodeAutomatic(ScpRevolution revolution) => Decoders.Select(x => Decode(x.Id, revolution))
        .OrderByDescending(FluxDecoderScoring.Calculate)
        .ThenByDescending(result => result.Confidence)
        .ThenByDescending(result => result.Structures.Count)
        .First();
    /// <summary>Décode une révolution avec le décodeur identifié et met le résultat en cache.</summary>
    /// <param name="id">Identifiant du décodeur.</param><param name="revolution">Révolution SCP à analyser.</param><returns>Résultat du décodeur demandé.</returns>
    /// <exception cref="KeyNotFoundException">Aucun décodeur ne possède l'identifiant demandé.</exception>
    public FluxDecodeResult Decode(string id, ScpRevolution revolution)
    {
        if (!decodersById.TryGetValue(id, out var decoder)) throw FluxDecoderRegistryExceptions.IdentifierNotFound(id);
        var results = GetOrCreateRevolutionCache(revolution);
        return results.GetOrAdd(decoder.Id, _ => CreateDeferredResult(decoder, revolution)).Value;
    }
    public FluxDecodeSelection? DecodeBest(IReadOnlyList<ScpRevolution> revolutions, string? decoderId = null)
    {
        if (revolutions.Count == 0) return null;
        var candidates = revolutions.Select((revolution, index) => new FluxDecodeSelection(index, decoderId is null ? DecodeAutomatic(revolution) : Decode(decoderId, revolution)));
        if (decoderId is null) return candidates.OrderByDescending(candidate => FluxDecoderScoring.Calculate(candidate.Result))
            .ThenByDescending(candidate => candidate.Result.Confidence)
            .ThenByDescending(candidate => candidate.Result.Structures.Count)
            .First();
        return candidates.OrderByDescending(candidate => FluxDecoderScoring.CalculateExplicit(candidate.Result)).First();
    }

    private System.Collections.Concurrent.ConcurrentDictionary<string, Lazy<FluxDecodeResult>> GetOrCreateRevolutionCache(ScpRevolution revolution) => _cache.GetValue(revolution, _ => new(StringComparer.Ordinal));
    private static Lazy<FluxDecodeResult> CreateDeferredResult(IFluxDecoder decoder, ScpRevolution revolution) => new(() => decoder.Decode(revolution), LazyThreadSafetyMode.ExecutionAndPublication);
}
