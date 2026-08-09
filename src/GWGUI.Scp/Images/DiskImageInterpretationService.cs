using GWGUI.Scp.FileSystems;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

internal sealed class DiskImageInterpretationService(FileSystemRegistry fileSystems)
{
    public SectorImage NormalizeRecognizedImage(SectorImage image, string readerId, FileSystemVolume volume)
    {
        if ((readerId.Equals("mac-hfs", StringComparison.OrdinalIgnoreCase) ||
             readerId.Equals("mac-mfs", StringComparison.OrdinalIgnoreCase)) &&
            image.BlockSize == 512 && image.BlockCount == 2880 &&
            !image.FormatId.Equals("mac.1440", StringComparison.OrdinalIgnoreCase))
            return Retag(image, "mac.1440");
        if (readerId.Equals("fat12", StringComparison.OrdinalIgnoreCase) &&
            TryCreateMsxInterpretation(image, out var msxInterpretation))
            return msxInterpretation;
        if (readerId.Equals("fat12", StringComparison.OrdinalIgnoreCase) &&
            image.FormatId.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase) &&
            TryReadFatGeometry(image, out var cylinders, out var heads, out var sectorsPerTrack, out var totalSectors) &&
            totalSectors < image.BlockCount)
        {
            var blocks = image.AvailableBlocks.Where(block => block.LogicalBlock < totalSectors).ToArray();
            return new($"atarist.{totalSectors / 2}", 512, cylinders, heads, sectorsPerTrack, blocks,
                capacity: totalSectors * 512L, logicalBlockCount: totalSectors);
        }
        if (readerId.Equals("fat12", StringComparison.OrdinalIgnoreCase) &&
            image.FormatId.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase) &&
            ContainsAtariStProgram(volume.Entries))
            return Retag(image, $"atarist.{image.Capacity / 1024}");
        return image;
    }

    public IEnumerable<SectorImage> AdditionalFileSystemInterpretations(SectorImage image)
    {
        var iso = image.FormatId.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase)
            || image.FormatId.StartsWith("atari.", StringComparison.OrdinalIgnoreCase)
            || image.FormatId.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase)
            || image.FormatId.StartsWith("acorn.dfs.", StringComparison.OrdinalIgnoreCase)
            || image.FormatId.Equals("imd", StringComparison.OrdinalIgnoreCase);
        if (!iso) yield break;
        if (image.BlockSize == 512)
        {
            if (TryCreateIbmInterpretation(image, out var ibm)) yield return ibm;
            if (TryCreateMsxInterpretation(image, out var msx)) yield return msx;
            foreach (var id in new[]
                     {
                         "ucsd.ibm.mfm", "commodore900.coherent", "epson.qx10.396",
                         "epson.qx10.399", "epson.qx10.logo"
                     })
                if (!id.Equals(image.FormatId, StringComparison.OrdinalIgnoreCase)) yield return Retag(image, id);
        }
        else if (image.BlockSize == 256)
        {
            foreach (var id in new[]
                     {
                         "acorn.dfs.ss", "acorn.dfs.ss80", "acorn.dfs.ds", "acorn.dfs.ds80", "epson.qx10.320"
                     })
                if (!id.Equals(image.FormatId, StringComparison.OrdinalIgnoreCase)) yield return Retag(image, id);
        }
        else if (image.BlockSize == 1024 &&
                 !image.FormatId.Equals("epson.qx10.400", StringComparison.OrdinalIgnoreCase))
            yield return Retag(image, "epson.qx10.400");
    }

    public ExploredDiskImage CreateDocument(
        string path,
        SectorImage image,
        IReadOnlyList<ExploredFileSystem> detected,
        IReadOnlyList<string>? detectedImageFormatIds = null)
    {
        if (detected.Count > 0)
            return new(path, image, detected[0].Volume, true, detected, detectedImageFormatIds);
        var physicalTracks = image.AvailableBlocks
            .GroupBy(block => (block.Address.Cylinder, block.Address.Head))
            .OrderBy(group => group.Key.Cylinder).ThenBy(group => group.Key.Head)
            .Select(group => new FileSystemEntry($"T{group.Key.Cylinder:D2} H{group.Key.Head}", FileSystemEntryKind.Directory,
                group.Sum(block => (long)block.Data.Count), null, string.Empty, 0, 0,
                group.All(block => block.IntegrityValid != false),
                group.OrderBy(block => block.Address.Number).Select(block => new FileSystemEntry(
                    $"S{block.Address.Number:D2}.bin", FileSystemEntryKind.File, block.Data.Count, null,
                    string.Empty, 0, block.LogicalBlock, block.IntegrityValid != false, [], block.Data.ToArray())).ToArray()))
            .ToArray();
        var physical = new FileSystemVolume(Path.GetFileNameWithoutExtension(path), image.FormatId,
            image.Capacity, 0, null, null, physicalTracks, []);
        return new(path, image, physical, false, [], detectedImageFormatIds);
    }

    public ExploredDiskImage Unknown(string path)
    {
        var capacity = new FileInfo(path).Length;
        var image = new SectorImage("unknown", 1, 1, 1, 1, [], capacity: capacity, logicalBlockCount: 1);
        return CreateDocument(path, image, []);
    }

    public static bool IsCredibleAlternative(FileSystemVolume volume) =>
        volume.Warnings.Count <= Math.Max(3, volume.Entries.Count);

    public static double DecodeScore(SectorImage image) =>
        image.AvailableBlocks.Count / (double)Math.Max(1, image.BlockCount);

    public static string FileSystemIdentity(FileSystemVolume volume)
    {
        static IEnumerable<string> Entries(IEnumerable<FileSystemEntry> entries, string prefix = "")
        {
            foreach (var entry in entries.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase))
            {
                var path = prefix + entry.Name;
                yield return $"{path}\0{entry.Kind}\0{entry.Size}";
                foreach (var child in Entries(entry.Children, path + "/")) yield return child;
            }
        }
        return $"{volume.Name}\0{string.Join('\u001f', Entries(volume.Entries))}";
    }

    public static SectorImage Retag(SectorImage image, string formatId) => new(formatId, image.BlockSize,
        image.Cylinders, image.Heads, image.SectorsPerTrack, image.AvailableBlocks,
        image.AvailableBlocks.Any(block => block.Data.Count != image.BlockSize), image.Capacity, image.BlockCount);

    private bool TryCreateIbmInterpretation(SectorImage image, out SectorImage interpretation)
    {
        interpretation = null!;
        if (image.FormatId.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase) ||
            !image.TryGetBlock(0, out var boot) || boot.Data.Count != 512)
            return false;
        var fatMedia = image.TryGetBlock(1, out var fat) && fat.Data.Count > 0 ? fat.Data[0] : (byte)0;
        if (!IbmPcImageReader.TryDetectFluxGeometry(boot.Data.ToArray(), fatMedia, out var geometry)) return false;
        var formatId = geometry.FormatId.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase) &&
            fileSystems.SupportedFormatIds.Contains(geometry.FormatId)
            ? geometry.FormatId : "ibm.scan";
        interpretation = Retag(image, formatId);
        return true;
    }

    private static bool TryCreateMsxInterpretation(SectorImage image, out SectorImage interpretation)
    {
        interpretation = null!;
        if (image.FormatId.StartsWith("msx.", StringComparison.OrdinalIgnoreCase) ||
            !image.TryGetBlock(0, out var boot) || boot.Data.Count != 512 ||
            !MsxImageReader.LooksLikeMsx(boot.Data.ToArray()))
            return false;
        var formatId = image.BlockCount switch
        {
            360 => "msx.1d",
            720 when boot.Data.Count > 21 && boot.Data[21] == 0xf8 => "msx.1dd",
            720 => "msx.2d",
            1440 => "msx.2dd",
            _ => string.Empty
        };
        if (formatId.Length == 0) return false;
        interpretation = Retag(image, formatId);
        return true;
    }

    private static bool ContainsAtariStProgram(IEnumerable<FileSystemEntry> entries)
    {
        foreach (var entry in entries)
        {
            if (entry.Kind == FileSystemEntryKind.File)
            {
                var extension = Path.GetExtension(entry.Name);
                if (extension.Equals(".ttp", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".tos", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".acc", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".gtp", StringComparison.OrdinalIgnoreCase) ||
                    entry.Content is { Count: >= 2 } && entry.Content[0] == 0x60 && entry.Content[1] == 0x1a)
                    return true;
            }
            if (ContainsAtariStProgram(entry.Children)) return true;
        }
        return false;
    }

    private static bool TryReadFatGeometry(SectorImage image, out int cylinders, out int heads,
        out int sectorsPerTrack, out int totalSectors)
    {
        cylinders = heads = sectorsPerTrack = totalSectors = 0;
        if (image.BlockSize != 512 || !image.TryGetBlock(0, out var boot) || boot.Data.Count < 36) return false;
        var bytes = boot.Data;
        var bytesPerSector = bytes[11] | bytes[12] << 8;
        totalSectors = bytes[19] | bytes[20] << 8;
        if (totalSectors == 0)
            totalSectors = bytes[32] | bytes[33] << 8 | bytes[34] << 16 | bytes[35] << 24;
        sectorsPerTrack = bytes[24] | bytes[25] << 8;
        heads = bytes[26] | bytes[27] << 8;
        if (bytesPerSector != 512 || totalSectors <= 0 || sectorsPerTrack <= 0 || heads <= 0 ||
            totalSectors > image.BlockCount || totalSectors % (sectorsPerTrack * heads) != 0)
            return false;
        cylinders = totalSectors / (sectorsPerTrack * heads);
        return cylinders > 0;
    }
}
