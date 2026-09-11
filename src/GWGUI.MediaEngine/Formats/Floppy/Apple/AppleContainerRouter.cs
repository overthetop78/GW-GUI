using GWGUI.MediaEngine.Constants;

using GWGUI.MediaEngine.Formats.Floppy.DiskCopy;

using GWGUI.MediaEngine.Formats.Floppy.Nib;

using GWGUI.MediaEngine.Formats.Floppy.Raw;

using GWGUI.MediaEngine.Formats.Floppy.TwoImg;

using GWGUI.MediaEngine.Formats.Floppy.Woz;

using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.Formats.Floppy.Apple;

/// <summary>Route un contenu Apple dÃ©jÃ  chargÃ© selon une signature certaine, un indice NIB, puis une reprÃ©sentation sectorielle brute.</summary>
internal static class AppleContainerRouter
{
    /// <summary>Valide et lit le contenu avec le Reader spÃ©cialisÃ© correspondant au premier critÃ¨re applicable.</summary>
    /// <param name="bytes">Contenu complet dÃ©jÃ  chargÃ©.</param>
    /// <param name="extension">Extension utilisÃ©e uniquement comme indice pour les formats sans signature.</param>
    /// <param name="requestedFormatId">Format explicitement demandÃ©, ou <see langword="null"/>.</param>
    /// <returns>Image sectorielle entiÃ¨rement validÃ©e.</returns>
    public static SectorImage Read(byte[] bytes, string extension, string? requestedFormatId)
    {
        if (bytes.AsSpan().StartsWith(TwoImgFormat.SignatureBytes)) return TwoImgReader.Read(bytes);
        if (DiskCopyReader.HasPrivateWord(bytes)) return DiskCopyReader.Read(bytes);
        if (bytes.AsSpan().StartsWith(WozFormat.Version1Signature) || bytes.AsSpan().StartsWith(WozFormat.Version2Signature)) return WozReader.Read(bytes);
        if (extension.Equals(DiskImageFileExtensions.Nib, StringComparison.OrdinalIgnoreCase)) return NibReader.Read(bytes);
        try
        {
            return AppleRawImageReader.Read(bytes, extension).Image;
        }
        catch (InvalidDataException)
        {
            throw AppleContainerExceptions.NoValidatedFormat(extension, requestedFormatId);
        }
    }
}
