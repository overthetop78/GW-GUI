using System.Buffers.Binary;
using GWGUI.MediaEngine.Containers.Apple.Raw;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Apple.TwoImg;

/// <summary>Enveloppe une image sectorielle Apple validée dans un conteneur 2IMG version 1.</summary>
public sealed class TwoImgWriter
{
    /// <summary>Construit l'en-tête et la charge utile puis remplace atomiquement le fichier de destination.</summary>
    public async Task WriteAsync(SectorImage image, string path, string targetFormatId, CancellationToken cancellationToken = default)
    {
        var imageFormat = ImageFormat(targetFormatId);
        var payload = AppleRawImageWriter.BuildTwoImgPayload(image, targetFormatId);
        var container = new byte[checked(TwoImgLayout.MinimumHeaderSize + payload.Length)];
        TwoImgFormat.SignatureBytes.CopyTo(container.AsSpan(TwoImgLayout.SignatureOffset, TwoImgLayout.SignatureLength));
        TwoImgFormat.CreatorBytes.CopyTo(container.AsSpan(TwoImgLayout.CreatorOffset, TwoImgLayout.CreatorLength));
        BinaryPrimitives.WriteUInt16LittleEndian(container.AsSpan(TwoImgLayout.HeaderSizeOffset), TwoImgLayout.MinimumHeaderSize);
        BinaryPrimitives.WriteUInt16LittleEndian(container.AsSpan(TwoImgLayout.VersionOffset), TwoImgFormat.SupportedVersion);
        BinaryPrimitives.WriteUInt32LittleEndian(container.AsSpan(TwoImgLayout.ImageFormatOffset), (uint)imageFormat);
        BinaryPrimitives.WriteUInt32LittleEndian(container.AsSpan(TwoImgLayout.FlagsOffset), 0);
        BinaryPrimitives.WriteUInt32LittleEndian(container.AsSpan(TwoImgLayout.BlockCountOffset), imageFormat == TwoImgImageFormat.ProDos ? checked((uint)(payload.Length / 512)) : 0);
        BinaryPrimitives.WriteUInt32LittleEndian(container.AsSpan(TwoImgLayout.DataOffsetOffset), TwoImgLayout.MinimumHeaderSize);
        BinaryPrimitives.WriteUInt32LittleEndian(container.AsSpan(TwoImgLayout.DataLengthOffset), checked((uint)payload.Length));
        payload.CopyTo(container, TwoImgLayout.MinimumHeaderSize);
        await AppleRawImageWriter.WriteAtomicallyAsync(path, container, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Détermine le type de charge utile depuis l'identifiant de format explicite.</summary>
    private static TwoImgImageFormat ImageFormat(string targetFormatId)
    {
        if (targetFormatId.Equals(DiskImageFormatIds.AppleIIAppleDos113, StringComparison.OrdinalIgnoreCase) || targetFormatId.Equals(DiskImageFormatIds.AppleIIAppleDos140, StringComparison.OrdinalIgnoreCase)) return TwoImgImageFormat.Dos;
        if (AppleRawImageWriter.IsProDos140(targetFormatId) || targetFormatId.Equals(DiskImageFormatIds.AppleIIProDos800, StringComparison.OrdinalIgnoreCase)) return TwoImgImageFormat.ProDos;
        throw AppleRawImageWriterExceptions.UnsupportedTarget(targetFormatId, DiskImageFileExtensions.TwoMg);
    }
}
