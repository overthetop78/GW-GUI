using System.Collections.Frozen;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Encoding.Apple;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Containers.Apple.Nib;
using GWGUI.MediaEngine.Containers.Apple.Woz;

namespace GWGUI.MediaEngine.Containers.Apple;

/// <summary>Encode puis écrit une image Apple II représentable dans un conteneur NIB ou WOZ1.</summary>
public sealed class AppleDiskImageWriter
{
    private static readonly IReadOnlyDictionary<string, (int MaximumBits, Func<IReadOnlyList<IReadOnlyList<bool>>, string, CancellationToken, Task> Write)> Outputs = new Dictionary<string, (int, Func<IReadOnlyList<IReadOnlyList<bool>>, string, CancellationToken, Task>)>(StringComparer.OrdinalIgnoreCase)
    {
        [DiskImageFileExtensions.Nib] = (NibLayout.MaximumTrackBitCount, NibWriter.WriteAsync),
        [DiskImageFileExtensions.Woz] = (WozWriter.MaximumTrackBitCount, WozWriter.WriteAsync)
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    private readonly AppleRwts18TrackEncodingService _encoder;
    private readonly AppleIITrackEncodingService _standardEncoder;
    private readonly Func<string, byte[], CancellationToken, Task>? _writeBytes;

    /// <summary>Crée la façade avec le service d'encodage fourni ou le service par défaut.</summary>
    /// <param name="encoder">Service d'encodage RWTS18 optionnel.</param>
    /// <param name="standardEncoder">Service d'encodage Apple II GCR standard optionnel.</param>
    public AppleDiskImageWriter(AppleRwts18TrackEncodingService? encoder = null, AppleIITrackEncodingService? standardEncoder = null,
        Func<string, byte[], CancellationToken, Task>? writeBytes = null)
    {
        _encoder = encoder ?? new();
        _standardEncoder = standardEncoder ?? new();
        _writeBytes = writeBytes;
    }

    /// <summary>Indique si l'extension correspond à un conteneur actuellement écrit.</summary>
    public static bool SupportsExtension(string extension) => Outputs.ContainsKey(extension);

    /// <summary>Choisit le Writer depuis l'extension, encode les pistes puis écrit le fichier.</summary>
    /// <param name="image">Image Apple II source.</param>
    /// <param name="path">Chemin NIB ou WOZ de destination.</param>
    /// <param name="cancellationToken">Jeton d'annulation propagé à l'encodage et à l'écriture.</param>
    public Task WriteAsync(SectorImage image, string path, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(path);
        if (!Outputs.TryGetValue(extension, out var output)) throw AppleDiskImageWriterExceptions.UnsupportedExtension(extension);
        var tracks = image.FormatId.Equals(DiskImageFormatIds.AppleIIRwts18, StringComparison.OrdinalIgnoreCase) ? _encoder.Encode(image, output.MaximumBits, cancellationToken) : _standardEncoder.Encode(image, output.MaximumBits, cancellationToken);
        if (_writeBytes is not null)
            return extension.Equals(DiskImageFileExtensions.Nib, StringComparison.OrdinalIgnoreCase)
                ? NibWriter.WriteAsync(tracks, path, cancellationToken, _writeBytes)
                : WozWriter.WriteAsync(tracks, path, cancellationToken, _writeBytes);
        return output.Write(tracks, path, cancellationToken);
    }
}
