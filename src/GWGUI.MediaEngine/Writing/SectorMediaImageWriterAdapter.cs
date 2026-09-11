using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.Writing;

/// <summary>Extracts a sector image for an existing writer using the common adapter behavior.</summary>
internal sealed class SectorMediaImageWriterAdapter : MediaImageWriterAdapter<SectorImage>
{
    public SectorMediaImageWriterAdapter(
        string id,
        IEnumerable<string> formatIds,
        IEnumerable<string> producedFileExtensions,
        Func<SectorImage, MediaImageDocument, string, string, bool> canWrite,
        Func<SectorImage, MediaImageDocument, string, string, CancellationToken, Task<IReadOnlyList<string>>> write,
        bool producesMultipleFiles = false)
        : base(
            id,
            MediaRepresentationKind.Sectors,
            formatIds,
            producedFileExtensions,
            canWrite,
            write,
            producesMultipleFiles)
    {
    }

    protected override bool TryGetImage(MediaImageDocument document, out SectorImage image)
    {
        if (document.Representation is SectorMediaImageRepresentation sectors)
        {
            image = sectors.Image;
            return true;
        }

        image = null!;
        return false;
    }
}
