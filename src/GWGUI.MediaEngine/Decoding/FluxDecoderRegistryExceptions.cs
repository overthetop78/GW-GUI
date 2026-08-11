namespace GWGUI.MediaEngine.Decoding;

/// <summary>Construit les erreurs liées à l'enregistrement et à la recherche des décodeurs de flux.</summary>
internal static class FluxDecoderRegistryExceptions
{
    /// <summary>Crée l'erreur signalant une collection vide.</summary>
    public static ArgumentException EmptyCollection(string parameterName) => new("At least one flux decoder must be registered.", parameterName);
    /// <summary>Crée l'erreur signalant un décodeur nul à la position indiquée.</summary>
    public static ArgumentException NullDecoder(int position, string parameterName) => new($"The flux decoder at position {position} is null.", parameterName);
    /// <summary>Crée l'erreur signalant un identifiant invalide à la position indiquée.</summary>
    public static ArgumentException InvalidIdentifier(int position, string parameterName) => new($"The flux decoder at position {position} has an empty identifier.", parameterName);
    /// <summary>Crée l'erreur signalant un identifiant dupliqué.</summary>
    public static InvalidOperationException DuplicateIdentifier(string decoderId) => new($"A flux decoder with identifier '{decoderId}' is already registered.");
    /// <summary>Crée l'erreur signalant un identifiant absent.</summary>
    public static KeyNotFoundException IdentifierNotFound(string decoderId) => new($"No flux decoder is registered with identifier '{decoderId}'.");
}
