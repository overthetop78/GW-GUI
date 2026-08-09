using GWGUI.Scp.Decoding;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

/// <summary>Routes Apple II, Macintosh and Lisa containers to their dedicated readers.</summary>
public sealed class AppleDiskImageReader : ISectorImageReader
{
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
        { ".d13", ".do", ".po", ".2mg", ".image", ".dc42", ".nib", ".woz", ".dsk", ".img" };

    public bool CanRead(string path) => Extensions.Contains(Path.GetExtension(path));

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        var extension = Path.GetExtension(path);
        if (bytes.AsSpan().StartsWith("2IMG"u8)) return AppleContainerImageReader.ReadTwoImg(bytes);
        if (extension.Equals(".image", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".dc42", StringComparison.OrdinalIgnoreCase))
            return AppleContainerImageReader.ReadDiskCopy(bytes);
        if (extension.Equals(".nib", StringComparison.OrdinalIgnoreCase))
            return AppleNibbleImageDecoder.ReadNib(bytes);
        if (extension.Equals(".woz", StringComparison.OrdinalIgnoreCase))
            return AppleNibbleImageDecoder.ReadWoz(bytes);
        return AppleRawImageReader.Read(bytes, extension);
    }

    public static bool LooksLikeAppleImage(string path)
    {
        try
        {
            var extension = Path.GetExtension(path);
            if (extension.Equals(".d13", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".do", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".po", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".2mg", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".image", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".dc42", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".nib", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".woz", StringComparison.OrdinalIgnoreCase)) return true;
            if (extension.Equals(".img", StringComparison.OrdinalIgnoreCase))
            {
                var raw = File.ReadAllBytes(path);
                return AppleDiskImageSignatures.LooksLikeLisaOfficePayload(raw) ||
                       raw.Length is 409_600 or 819_200 or 1_474_560 &&
                       AppleDiskImageSignatures.LooksLikeMac(raw);
            }
            if (!extension.Equals(".dsk", StringComparison.OrdinalIgnoreCase)) return false;
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
