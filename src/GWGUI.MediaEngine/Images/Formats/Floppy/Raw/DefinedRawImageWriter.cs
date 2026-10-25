using System.IO;
using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaEngine.Interfaces;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.Raw;

internal sealed class DefinedRawImageWriter(IAtomicImageFileWriter? fileWriter = null)
{
    private readonly IAtomicImageFileWriter files = fileWriter ?? new AtomicImageFileWriter();

    public Task WriteAsync(SectorImage image, string path, string formatId, CancellationToken cancellationToken)
    {
        var definition = DiskDefsFormatReader.RawDefinitions[formatId];
        var layout = definition.Layout();
        if (image.Cylinders != definition.Cylinders || image.Heads != definition.Heads || image.BlockCount != layout.Count)
            throw new InvalidDataException($"Image geometry does not match disk definition '{formatId}'.");
        return files.WriteAsync(path, async (output, token) =>
        {
            foreach (var sector in layout)
            {
                token.ThrowIfCancellationRequested();
                if (!image.TryGetBlock(sector.LogicalBlock, out var block) || block.Data.Count != sector.Size)
                    throw new InvalidDataException($"Missing or invalid sector {sector.Cylinder}:{sector.Head}:{sector.Number} for '{formatId}'.");
                await output.WriteAsync(block.Data.ToArray(), token).ConfigureAwait(false);
                if (sector.ImageSize > sector.Size)
                    await output.WriteAsync(new byte[sector.ImageSize - sector.Size], token).ConfigureAwait(false);
            }
        }, cancellationToken);
    }
}
