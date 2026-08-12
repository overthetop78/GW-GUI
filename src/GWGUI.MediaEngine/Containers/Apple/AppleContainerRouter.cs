using GWGUI.MediaEngine.Containers.Apple.DiskCopy;
using GWGUI.MediaEngine.Containers.Apple.Nib;
using GWGUI.MediaEngine.Containers.Apple.TwoImg;
using GWGUI.MediaEngine.Containers.Apple.Woz;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Apple;

/// <summary>Route un contenu Apple déjà chargé selon une signature certaine, un indice NIB, puis une représentation sectorielle brute.</summary>
internal static class AppleContainerRouter
{
    /// <summary>Valide et lit le contenu avec le Reader spécialisé correspondant au premier critère applicable.</summary>
    /// <param name="bytes">Contenu complet déjà chargé.</param>
    /// <param name="extension">Extension utilisée uniquement comme indice pour les formats sans signature.</param>
    /// <param name="requestedFormatId">Format explicitement demandé, ou <see langword="null"/>.</param>
    /// <returns>Image sectorielle entièrement validée.</returns>
    public static SectorImage Read(byte[] bytes, string extension, string? requestedFormatId)
    {
        if (bytes.AsSpan().StartsWith(TwoImgFormat.SignatureBytes)) return TwoImgReader.Read(bytes);
        if (DiskCopyReader.HasPrivateWord(bytes)) return DiskCopyReader.Read(bytes);
        if (bytes.AsSpan().StartsWith(WozFormat.Version1Signature) || bytes.AsSpan().StartsWith(WozFormat.Version2Signature)) return WozReader.Read(bytes);
        if (extension.Equals(DiskImageFileExtensions.Nib, StringComparison.OrdinalIgnoreCase)) return NibReader.Read(bytes);
        try
        {
            return AppleRawImageReader.Read(bytes, extension);
        }
        catch (InvalidDataException)
        {
            throw AppleContainerExceptions.NoValidatedFormat(extension, requestedFormatId);
        }
    }
}
