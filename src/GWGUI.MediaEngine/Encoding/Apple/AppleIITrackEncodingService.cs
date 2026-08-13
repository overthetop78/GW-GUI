using GWGUI.MediaEngine.Conversion.Apple;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Encoding.Apple;

/// <summary>Encode les images sectorielles Apple II standard en pistes GCR indépendantes du conteneur.</summary>
public sealed class AppleIITrackEncodingService
{
    private readonly FluxEncoderRegistry _encoders;

    /// <summary>Crée le service avec le registre d'encodeurs fourni ou le catalogue par défaut.</summary>
    public AppleIITrackEncodingService(FluxEncoderRegistry? encoders = null) => _encoders = encoders ?? new FluxEncoderRegistry();

    /// <summary>Indique si le format utilise le GCR Apple II 5,25 pouces standard.</summary>
    public static bool Supports(string formatId) => formatId.Equals(DiskImageFormatIds.AppleIIDos32, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.AppleIIDos33, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.AppleIIGcr, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.AppleIIAppleDos113, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.AppleIIAppleDos140, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.AppleIIProDos140, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.AppleIIProDos, StringComparison.OrdinalIgnoreCase);

    /// <summary>Encode toutes les pistes standard et valide chaque secteur attendu.</summary>
    public IReadOnlyList<IReadOnlyList<bool>> Encode(SectorImage image, int maximumBits, CancellationToken cancellationToken = default)
    {
        if (!Supports(image.FormatId)) throw AppleIITrackEncodingExceptions.UnsupportedSource(image.FormatId);
        var sectorsPerTrack = ResolveSectorsPerTrack(image);
        var imageSectorsPerTrack = image.BlockSize == AppleIIGeometry.ProDosBlockSize ? AppleIIGeometry.ProDosBlocksPerTrack : sectorsPerTrack;
        if (image.Heads != 1 || image.Cylinders <= 0 || image.SectorsPerTrack != imageSectorsPerTrack) throw AppleIITrackEncodingExceptions.InvalidGeometry(image.Cylinders, image.Heads, image.SectorsPerTrack);
        var tracks = new List<IReadOnlyList<bool>>(image.Cylinders);
        for (var cylinder = 0; cylinder < image.Cylinders; cylinder++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sectors = image.BlockSize == AppleIIGeometry.ProDosBlockSize ? CreateProDosSectors(image, cylinder) : CreateDosSectors(image, cylinder, sectorsPerTrack);
            var attributes = new Dictionary<string, int> { [TrackEncodingAttributeKeys.SectorsPerTrack] = sectorsPerTrack };
            var encoded = _encoders.Encode(FluxCodecIds.AppleIIGcr, new TrackEncodeRequest(cylinder, AppleIIGcrFormat.LogicalHead, sectors, attributes));
            if (encoded.Bits.Count > maximumBits) throw AppleIITrackEncodingExceptions.TrackTooLong(cylinder, encoded.Bits.Count, maximumBits);
            tracks.Add(encoded.Bits);
        }
        return tracks;
    }

    /// <summary>Résout les treize ou seize secteurs physiques attendus sur chaque piste.</summary>
    private static int ResolveSectorsPerTrack(SectorImage image)
    {
        if (image.FormatId.Equals(DiskImageFormatIds.AppleIIDos32, StringComparison.OrdinalIgnoreCase) || image.FormatId.Equals(DiskImageFormatIds.AppleIIAppleDos113, StringComparison.OrdinalIgnoreCase)) return AppleIIGeometry.Dos32SectorsPerTrack;
        return AppleIIGeometry.SectorsPerTrack;
    }

    /// <summary>Rassemble les secteurs DOS en utilisant leur véritable adresse physique.</summary>
    private static IReadOnlyList<TrackSector> CreateDosSectors(SectorImage image, int cylinder, int sectorsPerTrack)
    {
        var blocks = image.AvailableBlocks.Where(block => block.Address.Cylinder == cylinder && block.Address.Head == AppleIIGcrFormat.LogicalHead).ToDictionary(block => block.Address.Number);
        var sectors = new List<TrackSector>(sectorsPerTrack);
        for (var sector = 0; sector < sectorsPerTrack; sector++)
        {
            if (!blocks.TryGetValue(sector, out var block) || block.Data.Count != AppleIIGeometry.SectorSize) throw AppleIITrackEncodingExceptions.InvalidSector(cylinder, sector, block?.Data.Count ?? 0, AppleIIGeometry.SectorSize);
            sectors.Add(new TrackSector(sector, block.Data));
        }
        return sectors;
    }

    /// <summary>Découpe les huit blocs ProDOS en seize secteurs physiques de 256 octets.</summary>
    private static IReadOnlyList<TrackSector> CreateProDosSectors(SectorImage image, int cylinder)
    {
        var sectors = new TrackSector[AppleIIGeometry.SectorsPerTrack];
        for (var blockNumber = 0; blockNumber < AppleIIGeometry.ProDosBlocksPerTrack; blockNumber++)
        {
            var logical = cylinder * AppleIIGeometry.ProDosBlocksPerTrack + blockNumber;
            if (!image.TryGetBlock(logical, out var block) || block.Data.Count != AppleIIGeometry.ProDosBlockSize) throw AppleIITrackEncodingExceptions.InvalidSector(cylinder, blockNumber, block?.Data.Count ?? 0, AppleIIGeometry.ProDosBlockSize);
            var first = AppleIISectorOrderConverter.ProDosToPhysicalSector(blockNumber * AppleIIGeometry.SectorsPerProDosBlock);
            var second = AppleIISectorOrderConverter.ProDosToPhysicalSector(blockNumber * AppleIIGeometry.SectorsPerProDosBlock + 1);
            sectors[first] = new TrackSector(first, block.Data.Take(AppleIIGeometry.SectorSize).ToArray());
            sectors[second] = new TrackSector(second, block.Data.Skip(AppleIIGeometry.SectorSize).Take(AppleIIGeometry.SectorSize).ToArray());
        }
        return sectors;
    }
}
