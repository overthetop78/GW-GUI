using System.Buffers.Binary;
using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Images.Formats.Floppy.Raw;
using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaEngine.Images.Reading.Reconstruction;
using GWGUI.MediaEngine.Interfaces;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.Apridisk;

public sealed class ApridiskWriter(IAtomicImageFileWriter? fileWriter = null)
{
    private readonly IAtomicImageFileWriter files = fileWriter ?? new AtomicImageFileWriter();
    private static readonly RegularSectorGeometry Geometry = new(DiskImageFormatIds.ApricotPcXi315,
        ApridiskReader.BlockSize, ApridiskReader.Cylinders, ApridiskReader.Heads, ApridiskReader.SectorsPerTrack);

    public async Task WriteAsync(SectorImage image, string path, CancellationToken cancellationToken = default)
    {
        LinearSectorImageWriter.ValidateGeometry(image, Geometry);
        await files.WriteAsync(path, async (output, token) =>
        {
            var header = new byte[ApridiskReader.HeaderSize];
            ApridiskReader.Signature.CopyTo(header, 0);
            await output.WriteAsync(header, token).ConfigureAwait(false);
            var record = new byte[ApridiskReader.RecordHeaderSize];
            BinaryPrimitives.WriteUInt32LittleEndian(record, ApridiskReader.SectorRecord);
            BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(4), ApridiskReader.Uncompressed);
            BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(6), ApridiskReader.RecordHeaderSize);
            BinaryPrimitives.WriteUInt32LittleEndian(record.AsSpan(8), ApridiskReader.BlockSize);
            foreach (var block in LinearSectorImageWriter.ReadBlocks(image, Geometry, token))
            {
                var track = block.LogicalBlock / Geometry.SectorsPerTrack;
                record[12] = checked((byte)(track % Geometry.Heads));
                record[13] = checked((byte)(block.LogicalBlock % Geometry.SectorsPerTrack + 1));
                BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(14), checked((ushort)(track / Geometry.Heads)));
                await output.WriteAsync(record, token).ConfigureAwait(false);
                await output.WriteAsync(block.Data.ToArray(), token).ConfigureAwait(false);
            }
        }, cancellationToken).ConfigureAwait(false);
    }
}
