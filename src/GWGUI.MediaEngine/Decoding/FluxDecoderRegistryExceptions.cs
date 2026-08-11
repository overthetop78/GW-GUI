namespace GWGUI.MediaEngine.Decoding;

internal static class FluxDecoderRegistryExceptions
{
    public static InvalidOperationException DuplicateIdentifier(string decoderId) => new($"A flux decoder with identifier '{decoderId}' is already registered.");
    public static KeyNotFoundException IdentifierNotFound(string decoderId) => new($"No flux decoder is registered with identifier '{decoderId}'.");
}
