using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Containers;

/// <summary>Présélectionne les conteneurs et images brutes pris en charge par le lecteur Apple.</summary>
/// <param name="reader">Lecteur public des images disque Apple.</param>
internal sealed class AppleContainerPolicy(AppleDiskImageReader reader) : IDiskImageContainerPolicy
{
    private static readonly HashSet<string> AppleExtensions = new(StringComparer.OrdinalIgnoreCase)
        { DiskImageFileExtensions.Do, DiskImageFileExtensions.Po, DiskImageFileExtensions.TwoMg,
            DiskImageFileExtensions.Image, DiskImageFileExtensions.D13, DiskImageFileExtensions.Dc42,
            DiskImageFileExtensions.Nib, DiskImageFileExtensions.Woz };

    /// <summary>Évalue les signatures, les indices d’extension et le format Apple éventuellement demandé.</summary>
    /// <param name="context">Contexte du fichier à examiner.</param>
    /// <param name="cancellationToken">Jeton d’annulation de l’opération.</param>
    /// <returns><see langword="true"/> lorsque le fichier est un candidat Apple.</returns>
    public ValueTask<bool> CanReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken)
    {
        if (AppleExtensions.Contains(context.Extension)) return ValueTask.FromResult(true);
        if (context.Extension.Equals(DiskImageFileExtensions.Dsk, StringComparison.OrdinalIgnoreCase))
            return ValueTask.FromResult(context.RequestedFormatId?.StartsWith(DiskImageFormatIds.AppleIIPrefix, StringComparison.OrdinalIgnoreCase) == true ||
                                        context.RequestedFormatId?.StartsWith(DiskImageFormatIds.AppleIIIPrefix, StringComparison.OrdinalIgnoreCase) == true ||
                                        context.RequestedFormatId?.StartsWith(DiskImageFormatIds.AppleLisaPrefix, StringComparison.OrdinalIgnoreCase) == true ||
                                        context.RequestedFormatId?.StartsWith(DiskImageFormatIds.AppleMacPrefix, StringComparison.OrdinalIgnoreCase) == true ||
                                        AppleDiskImageReader.LooksLikeAppleImage(context.Path));
        if (context.Extension.Equals(DiskImageFileExtensions.Img, StringComparison.OrdinalIgnoreCase))
            return ValueTask.FromResult(context.RequestedFormatId?.StartsWith(DiskImageFormatIds.MacPrefix, StringComparison.OrdinalIgnoreCase) == true ||
                                        AppleDiskImageReader.LooksLikeAppleImage(context.Path));
        return ValueTask.FromResult(false);
    }

    /// <summary>Transmet le fichier candidat au lecteur Apple afin qu’il valide et reconstruise son contenu.</summary>
    /// <param name="context">Contexte du fichier à lire.</param>
    /// <param name="cancellationToken">Jeton d’annulation de l’opération.</param>
    /// <returns>Image sectorielle Apple reconstruite.</returns>
    public Task<SectorImage> ReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken) =>
        reader.ReadAsync(context.Path, cancellationToken);
}
