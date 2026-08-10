using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images;

public sealed record ExploredFileSystem(string FormatId, string ReaderId, FileSystemVolume Volume);

public sealed record ExploredDiskImage(
    string SourcePath,
    SectorImage Image,
    FileSystemVolume Volume,
    bool FileSystemRecognized = true,
    IReadOnlyList<ExploredFileSystem>? DetectedFileSystems = null,
    IReadOnlyList<string>? DetectedImageFormatIds = null)
{
    public DiskImageMetadata Metadata
    {
        get
        {
            var recognized = DetectedFileSystems?.Select(item => item.FormatId).ToArray() ?? [];
            return DiskImageMetadata.From(Image, recognized.Length > 0 ? recognized : [Image.FormatId]);
        }
    }
}
