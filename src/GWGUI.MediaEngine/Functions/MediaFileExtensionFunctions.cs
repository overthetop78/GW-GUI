namespace GWGUI.MediaEngine.Functions;

/// <summary>Provides shared operations for media file extensions.</summary>
public static class MediaFileExtensionFunctions
{
    public static string Normalize(string extension)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(extension);
        return NormalizeCore(extension);
    }

    internal static string NormalizeCore(string extension)
        => extension.StartsWith('.') ? extension.ToLowerInvariant() : "." + extension.ToLowerInvariant();
}
