namespace GWGUI.MediaEngine.Decoding;

/// <summary>Construit les erreurs liées à l'enregistrement et à la recherche des décodeurs de flux.</summary>
internal static class FluxDecoderRegistryExceptions
{
    public static ArgumentException EmptyCollection(string parameterName) => new("At least one flux decoder must be registered.", parameterName);
    public static ArgumentException NullDecoder(int position, string parameterName) => new($"The flux decoder at position {position} is null.", parameterName);
    public static ArgumentException InvalidIdentifier(int position, string parameterName) => new($"The flux decoder at position {position} has an empty identifier.", parameterName);
    public static InvalidOperationException DuplicateIdentifier(string decoderId) => new($"A flux decoder with identifier '{decoderId}' is already registered.");
    public static KeyNotFoundException IdentifierNotFound(string decoderId) => new($"No flux decoder is registered with identifier '{decoderId}'.");
}
