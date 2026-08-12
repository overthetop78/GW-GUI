using System.Buffers.Binary;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Conversion.Apple;
using GWGUI.MediaEngine.Geometries.Apple;

namespace GWGUI.MediaEngine.Images;

internal static class AppleRawImageReader
{
    public static SectorImage Read(byte[] data, string extension)
    {
        if (data.Length == 35 * 13 * 256)
            return AppleSectorImageFactory.CreateLinear(data, DiskImageFormatIds.AppleIIDos32, 256, 35, 1, 13);
        if (data.Length == 143_360)
            return ReadAppleTwo525(data, extension);
        if (data.Length is 409_600 or 819_200 or 1_474_560)
            return ReadApple35(data);
        throw new InvalidDataException("The Apple disk image has an unsupported size or signature.");
    }

    private static SectorImage ReadAppleTwo525(byte[] data, string extension)
    {
        if (extension.Equals(DiskImageFileExtensions.Po, StringComparison.OrdinalIgnoreCase) ||
            AppleDiskImageSignatures.LooksLikeProDos(data))
            return AppleSectorImageFactory.CreateLinear(data, DiskImageFormatIds.AppleIIProDos, 512, 35, 1, 8);
        if (AppleDiskImageSignatures.LooksLikeDos33(data))
            return AppleSectorImageFactory.CreateLinear(data, DiskImageFormatIds.AppleIIDos33, 256, 35, 1, 16);

        var prodosBlocks = AppleIISectorOrderConverter.DosToProDos(data);
        if (AppleDiskImageSignatures.LooksLikeSos(data))
            return AppleSectorImageFactory.CreateLinear(prodosBlocks, DiskImageFormatIds.AppleIIISos, 512, 35, 1, 8);
        if (AppleDiskImageSignatures.LooksLikeProDos(prodosBlocks))
            return AppleSectorImageFactory.CreateLinear(prodosBlocks, DiskImageFormatIds.AppleIIProDos, 512, 35, 1, 8);
        return AppleSectorImageFactory.CreateLinear(data, DiskImageFormatIds.AppleIIDos33, 256, 35, 1, 16);
    }

    private static SectorImage ReadApple35(byte[] data)
    {
        if (data.Length == 409_600 && AppleDiskImageSignatures.LooksLikeLisaOfficePayload(data))
            return AppleSectorImageFactory.CreateAppleMacZoned(data, DiskImageFormatIds.AppleLisaRaw, 1);
        if (AppleDiskImageSignatures.LooksLikeMac(data))
        {
            var signature = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(1024));
            var formatId = signature == 0xd2d7 ? DiskImageFormatIds.AppleMacMfs : DiskImageFormatIds.AppleMacHfs;
            return data.Length == 1_474_560
                ? AppleSectorImageFactory.CreateLinear(data, DiskImageFormatIds.Mac1440, MacintoshGcrGeometry.BlockSize, MacintoshGcrGeometry.CylinderCount, MacintoshGcrGeometry.DoubleSidedHeadCount, 18)
                : AppleSectorImageFactory.CreateAppleMacZoned(data, formatId, data.Length == 409_600 ? 1 : 2);
        }
        if (AppleDiskImageSignatures.LooksLikeProDos(data))
            return data.Length == 819_200
                ? AppleSectorImageFactory.CreateAppleMacZoned(data, DiskImageFormatIds.AppleIIProDos, 2)
                : AppleSectorImageFactory.CreateLinear(data, DiskImageFormatIds.AppleIIProDos, MacintoshGcrGeometry.BlockSize, MacintoshGcrGeometry.CylinderCount, MacintoshGcrGeometry.DoubleSidedHeadCount,
                    data.Length / 512 / 160);
        throw new InvalidDataException("The Apple disk image has an unsupported size or signature.");
    }
}
