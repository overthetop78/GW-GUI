using System.IO;
using GWGUI.MediaFileSystems.Contracts;
using GWGUI.MediaFileSystems.Interfaces;
using GWGUI.MediaFileSystems.Interfaces.Exploration;

namespace GWGUI.MediaFileSystems.FileSystems.Acorn.Adfs;

/// <summary>Adapte les volumes adressés en octets au lecteur ADFS existant, sans charger un HDD en entier.</summary>
public sealed partial class AcornAdfsFileSystemReader
{
    bool IMediaFileSystemReader.CanRead(IMediaImageDocument document, MediaVolumeDescriptor volume)
    {
        var image = SectorView(document, volume);
        return image is not null && CanRead(image);
    }

    FileSystemVolume IMediaFileSystemReader.Read(IMediaImageDocument document, MediaVolumeDescriptor volume) => Read(SectorView(document, volume) ?? throw new NotSupportedException("ADFS requires an addressable sector or block volume."));

    private static IMediaSectorImage? SectorView(IMediaImageDocument document, MediaVolumeDescriptor volume)
    {
        if (document.Representation is IMediaSectorRepresentation sectors && volume.Start == 0 && volume.Length == sectors.Image.Capacity) return sectors.Image;
        if (document.Representation is not IMediaBlockRepresentation blocks || volume.Start < 0 || volume.Length < 1792 || volume.Length % 256 != 0 || volume.Start > blocks.Capacity || volume.Length > blocks.Capacity - volume.Start || volume.Length / 256 > int.MaxValue) return null;
        return new BlockSectorView(blocks, document.FormatId, volume.Start, volume.Length);
    }

    /// <summary>Vue empruntée, lecture d'un seul secteur à la demande ; aucun fichier ni flux possédé.</summary>
    private sealed class BlockSectorView(IMediaBlockRepresentation blocks, string format, long start, long length) : IMediaSectorImage
    {
        public string FormatId => format;
        public int BlockSize => AcornAdfsLayout.FileCoreUnitSize;
        public int Cylinders => 0;
        public int Heads => 0;
        public int SectorsPerTrack => 0;
        public int BlockCount => checked((int)(length / BlockSize));
        public long Capacity => length;
        public IReadOnlyCollection<IMediaSectorBlock> AvailableBlocks => [];
        public bool TryGetBlock(int logicalBlock, out IMediaSectorBlock block)
        {
            block = null!;
            if (logicalBlock < 0 || logicalBlock >= BlockCount) return false;
            var data = new byte[BlockSize];
            try { blocks.ReadExactlyAsync(start + (long)logicalBlock * BlockSize, data).AsTask().GetAwaiter().GetResult(); }
            catch (Exception exception) when (exception is IOException or InvalidDataException) { return false; }
            block = new BlockSector(logicalBlock, data);
            return true;
        }
    }

    private sealed record BlockSector(int LogicalBlock, IReadOnlyList<byte> Data) : IMediaSectorBlock
    {
        public int Cylinder => 0;
        public int Head => 0;
        public int PhysicalSectorNumber => LogicalBlock;
        public bool? IntegrityValid => null;
        public IReadOnlyList<byte>? Tag => null;
    }
}
