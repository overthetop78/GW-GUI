using GWGUI.Scp.Containers.Apple.DiskCopy;
using GWGUI.Scp.Containers.Apple.TwoImg;
using GWGUI.Scp.Decoding;
using GWGUI.Scp.Recognition.Definitions;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

/// <summary>Routes Apple II, Macintosh and Lisa containers to their dedicated readers.</summary>
public sealed class AppleDiskImageReader : ISectorImageReader
{
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
        { DiskImageFileExtensions.D13, DiskImageFileExtensions.Do, DiskImageFileExtensions.Po,
            DiskImageFileExtensions.TwoMg, DiskImageFileExtensions.Image, DiskImageFileExtensions.Dc42,
            DiskImageFileExtensions.Nib, DiskImageFileExtensions.Woz, DiskImageFileExtensions.Dsk,
            DiskImageFileExtensions.Img };

    public bool CanRead(string path) => Extensions.Contains(Path.GetExtension(path));

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        var extension = Path.GetExtension(path);
        if (bytes.AsSpan().StartsWith("2IMG"u8)) return TwoImgReader.Read(bytes);
        if (extension.Equals(DiskImageFileExtensions.Image, StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(DiskImageFileExtensions.Dc42, StringComparison.OrdinalIgnoreCase))
            return DiskCopyReader.Read(bytes);
        if (extension.Equals(DiskImageFileExtensions.Nib, StringComparison.OrdinalIgnoreCase))
            return AppleNibbleImageDecoder.ReadNib(bytes);
        if (extension.Equals(DiskImageFileExtensions.Woz, StringComparison.OrdinalIgnoreCase))
            return AppleNibbleImageDecoder.ReadWoz(bytes);
        return AppleRawImageReader.Read(bytes, extension);
    }

    public static bool LooksLikeAppleImage(string path)
    {
        try
        {
            var extension = Path.GetExtension(path);
            if (extension.Equals(DiskImageFileExtensions.D13, StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(DiskImageFileExtensions.Do, StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(DiskImageFileExtensions.Po, StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(DiskImageFileExtensions.TwoMg, StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(DiskImageFileExtensions.Image, StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(DiskImageFileExtensions.Dc42, StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(DiskImageFileExtensions.Nib, StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(DiskImageFileExtensions.Woz, StringComparison.OrdinalIgnoreCase)) return true;
            if (extension.Equals(DiskImageFileExtensions.Img, StringComparison.OrdinalIgnoreCase))
            {
                var raw = File.ReadAllBytes(path);
                return AppleDiskImageSignatures.LooksLikeLisaOfficePayload(raw) ||
                       raw.Length is 409_600 or 819_200 or 1_474_560 &&
                       AppleDiskImageSignatures.LooksLikeMac(raw);
            }
            if (!extension.Equals(DiskImageFileExtensions.Dsk, StringComparison.OrdinalIgnoreCase)) return false;
            var bytes = File.ReadAllBytes(path);
            return bytes.Length == 143_360 ||
                   bytes.Length is 409_600 or 819_200 or 1_474_560 &&
                   AppleDiskImageSignatures.LooksLikeMac(bytes);
        }
        catch
        {
            return false;
        }
    }

    internal static int LisaFileWareSectors(int cylinder) => AppleDiskGeometry.LisaFileWareSectors(cylinder);
    internal static int AppleMacSectors(int cylinder) => AppleDiskGeometry.AppleMacSectors(cylinder);

    internal static SectorImage CreateAppleIIFromDecodedTracks(
        IEnumerable<(int Track, IReadOnlyList<DecodedSector> Sectors)> decodedTracks) =>
        AppleSectorImageFactory.CreateAppleIIFromDecodedTracks(decodedTracks);

    internal static SectorImage CreateRwts18FromDecodedTracks(
        IEnumerable<(int Track, IReadOnlyList<DecodedSector> Sectors)> decodedTracks) =>
        AppleSectorImageFactory.CreateRwts18FromDecodedTracks(decodedTracks);

    internal static bool LooksLikeLisaOfficePayload(ReadOnlySpan<byte> data) =>
        AppleDiskImageSignatures.LooksLikeLisaOfficePayload(data);
}
