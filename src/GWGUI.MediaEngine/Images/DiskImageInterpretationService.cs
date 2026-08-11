using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Images.Interpretations;
using GWGUI.MediaEngine.SectorImages;


namespace GWGUI.MediaEngine.Images;

internal sealed class DiskImageInterpretationService(FileSystemRegistry fileSystems)
{
    private readonly RecognizedImageNormalizerRegistry normalizers = new();
    private readonly AdditionalImageInterpretationRegistry additionalInterpretations = new(fileSystems);

    public SectorImage NormalizeRecognizedImage(SectorImage image, string readerId, FileSystemVolume volume) =>
        normalizers.Normalize(image, readerId, volume);

    public IEnumerable<SectorImage> AdditionalFileSystemInterpretations(SectorImage image) =>
        additionalInterpretations.Create(image);

    public ExploredDiskImage CreateDocument(string path, SectorImage image, IReadOnlyList<ExploredFileSystem> detected, IReadOnlyList<string>? detectedImageFormatIds = null)
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
        var image = new SectorImage(DiskImageFormatIds.Unknown, 1, 1, 1, 1, [], capacity: capacity, logicalBlockCount: 1);
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

    public static string InterpretationIdentity(ExploredFileSystem interpretation) =>
        $"{FormatFamily(interpretation.FormatId)}\0{FileSystemIdentity(interpretation.Volume)}";

    private static string FormatFamily(string formatId)
    {
        var separator = formatId.IndexOf('.');
        return separator < 0 ? formatId : formatId[..separator];
    }

}
