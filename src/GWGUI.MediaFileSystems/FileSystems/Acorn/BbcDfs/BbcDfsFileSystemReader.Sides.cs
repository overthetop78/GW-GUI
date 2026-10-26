using System.IO;
using GWGUI.MediaFileSystems.Constants;
using GWGUI.MediaFileSystems.Contracts;
using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaFileSystems.FileSystems.Acorn.BbcDfs;

/// <summary>Projette les pistes entrelacées des deux faces DSD sur les catalogues DFS indépendants.</summary>
public sealed partial class BbcDfsFileSystemReader
{
    private static bool IsDoubleSided(IMediaSectorImage image) => image.Heads == 2 && image.FormatId is MediaImageFormatIds.AcornDfsDoubleSided or MediaImageFormatIds.AcornDfsDoubleSided80;

    private FileSystemVolume ReadSides(IMediaSectorImage image)
    {
        var entries = new List<FileSystemEntry>(); var warnings = new List<string>();
        long capacity = 0, free = 0;
        for (var head = 0; head < image.Heads; head++)
        {
            var name = (head * 2).ToString(System.Globalization.CultureInfo.InvariantCulture);
            try
            {
                var volume = Read(new SideImage(image, head));
                entries.Add(new(name, FileSystemEntryKind.Directory, 0, null, volume.Name, 0, head, true, volume.Entries));
                capacity += volume.Capacity; free += volume.FreeBytes;
                warnings.AddRange(volume.Warnings.Select(warning => name + ": " + warning));
            }
            catch (InvalidDataException error)
            {
                warnings.Add(name + ": " + error.Message);
                entries.Add(new(name, FileSystemEntryKind.Directory, 0, null, error.Message, 0, head, false, []));
                capacity += (long)image.Cylinders * image.SectorsPerTrack * image.BlockSize;
            }
        }
        return new(string.Empty, Definitions.FileSystemIds.AcornDfs, capacity, free, null, null, entries, warnings, freeSpaceKnown: warnings.Count == 0);
    }

    /// <summary>Vue empruntée sans copie des données ; le numéro DFS est relatif à sa face.</summary>
    private sealed class SideImage(IMediaSectorImage image, int head) : IMediaSectorImage
    {
        public string FormatId => image.Cylinders > 40 ? MediaImageFormatIds.AcornDfsSingleSided80 : MediaImageFormatIds.AcornDfsSingleSided;
        public int BlockSize => image.BlockSize;
        public int Cylinders => image.Cylinders;
        public int Heads => 1;
        public int SectorsPerTrack => image.SectorsPerTrack;
        public int BlockCount => Cylinders * SectorsPerTrack;
        public long Capacity => (long)BlockCount * BlockSize;
        public IReadOnlyCollection<IMediaSectorBlock> AvailableBlocks => Enumerable.Range(0, BlockCount).Select(index => TryGetBlock(index, out var block) ? block : null).OfType<IMediaSectorBlock>().ToArray();
        public bool TryGetBlock(int logicalBlock, out IMediaSectorBlock block)
        {
            if (logicalBlock >= 0 && logicalBlock < BlockCount && image.TryGetBlock((logicalBlock / SectorsPerTrack * image.Heads + head) * SectorsPerTrack + logicalBlock % SectorsPerTrack, out var original))
            { block = new SideBlock(logicalBlock, original); return true; }
            block = null!; return false;
        }
    }

    private sealed class SideBlock(int index, IMediaSectorBlock original) : IMediaSectorBlock
    {
        public int LogicalBlock => index;
        public int Cylinder => original.Cylinder;
        public int Head => 0;
        public int PhysicalSectorNumber => original.PhysicalSectorNumber;
        public IReadOnlyList<byte> Data => original.Data;
        public bool? IntegrityValid => original.IntegrityValid;
        public IReadOnlyList<byte>? Tag => original.Tag;
    }
}
