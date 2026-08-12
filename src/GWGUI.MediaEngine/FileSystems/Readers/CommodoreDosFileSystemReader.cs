using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Commodore;
using GWGUI.MediaEngine.Geometries.Commodore;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Readers;

/// <summary>Lit les volumes Commodore DOS contenus dans les images D64, D71 et D81.</summary>
public sealed class CommodoreDosFileSystemReader : IFileSystemReader
{
    /// <inheritdoc />
    public string Id => Definitions.FileSystemIds.CommodoreDos;

    /// <inheritdoc />
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { DiskImageFormatIds.Commodore1541, DiskImageFormatIds.Commodore1571, DiskImageFormatIds.Commodore1581 };

    /// <inheritdoc />
    public bool CanRead(SectorImage image)
    {
        var layout = CommodoreDosLayout.Resolve(image.FormatId);
        return layout is not null && image.BlockSize == CommodoreDosLayout.SectorSize && TryGetSector(image, layout.HeaderTrack, layout.HeaderSector, out var header) && header.Length == CommodoreDosLayout.SectorSize && header[CommodoreDosLayout.DirectoryEntriesOffset] is 0x41 or 0x44 && HasPlausibleDirectory(image, header, layout);
    }

    /// <inheritdoc />
    public FileSystemVolume Read(SectorImage image)
    {
        var layout = CommodoreDosLayout.Resolve(image.FormatId) ?? throw CommodoreDosExceptions.UnsupportedLayout(image.FormatId);
        if (!TryGetSector(image, layout.HeaderTrack, layout.HeaderSector, out var header)) throw CommodoreDosExceptions.MissingHeader(layout.HeaderTrack, layout.HeaderSector);
        if (!CanRead(image)) throw CommodoreDosExceptions.UnsupportedLayout(image.FormatId);
        var name = PetsciiDecoder.Decode(header.AsSpan(layout.VolumeNameOffset, CommodoreDosLayout.NameLength));
        var warnings = new List<string>();
        var directoryTrack = header[CommodoreDosLayout.NextTrackOffset];
        var directorySector = header[CommodoreDosLayout.NextSectorOffset];
        if (directoryTrack == 0)
        {
            directoryTrack = (byte)layout.DirectoryTrack;
            directorySector = (byte)layout.DirectorySector;
        }
        var entries = ReadDirectory(image, directoryTrack, directorySector, warnings);
        var freeBlocks = ReadFreeBlocks(image, layout);
        return new(name, Definitions.FileSystemIds.CommodoreDos, image.Capacity, Math.Max(0, freeBlocks) * CommodoreDosLayout.SectorSize, null, null, entries, warnings);
    }

    /// <summary>Lit les entrées de la chaîne de répertoire.</summary>
    private static IReadOnlyList<FileSystemEntry> ReadDirectory(SectorImage image, int firstTrack, int firstSector, List<string> warnings)
    {
        var entries = new List<FileSystemEntry>();
        var visited = new HashSet<(int Track, int Sector)>();
        var track = firstTrack;
        var sector = firstSector;
        while (track != 0)
        {
            if (!visited.Add((track, sector))) { warnings.Add(CommodoreDosExceptions.CyclicDirectory(track, sector)); break; }
            if (!TryGetSector(image, track, sector, out var data)) { warnings.Add(CommodoreDosExceptions.MissingDirectorySector(track, sector)); break; }
            for (var slot = 0; slot < CommodoreDosLayout.DirectoryEntryCount; slot++)
            {
                var offset = CommodoreDosLayout.DirectoryEntriesOffset + slot * CommodoreDosLayout.DirectoryEntrySize;
                var rawType = (CommodoreDosFileType)data[offset + CommodoreDosLayout.FileTypeOffset];
                if (((byte)rawType & 0x0f) == 0) continue;
                var name = PetsciiDecoder.Decode(data.AsSpan(offset + CommodoreDosLayout.FileNameOffset, CommodoreDosLayout.NameLength));
                if (name.Length == 0) continue;
                var firstDataTrack = data[offset + CommodoreDosLayout.FirstDataTrackOffset];
                var firstDataSector = data[offset + CommodoreDosLayout.FirstDataSectorOffset];
                var declaredBlocks = data[offset + CommodoreDosLayout.DeclaredBlockCountOffset] | data[offset + CommodoreDosLayout.DeclaredBlockCountOffset + 1] << BitPrimitives.BitsPerByte;
                IReadOnlyList<byte> content = [];
                try { content = ReadFile(image, firstDataTrack, firstDataSector, warnings, name); }
                catch (InvalidDataException exception) { warnings.Add(Definitions.FileSystemWarningMessages.EntryReadFailure(name, exception)); }
                var comment = CommodoreDosFileTypeNames.GetComment(rawType);
                entries.Add(new(name, FileSystemEntryKind.File, content.Count == 0 ? declaredBlocks * CommodoreDosLayout.DataBytesPerSector : content.Count, null, comment, (uint)(byte)rawType, TryToLogicalBlock(image, firstDataTrack, firstDataSector), true, [], content));
            }
            track = data[CommodoreDosLayout.NextTrackOffset];
            sector = data[CommodoreDosLayout.NextSectorOffset];
        }
        return entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    /// <summary>Reconstruit le contenu d'un fichier à partir de sa chaîne de secteurs.</summary>
    private static IReadOnlyList<byte> ReadFile(SectorImage image, int firstTrack, int firstSector, List<string> warnings, string name)
    {
        if (firstTrack == 0) return [];
        using var stream = new MemoryStream();
        var visited = new HashSet<(int Track, int Sector)>();
        var track = firstTrack;
        var sector = firstSector;
        while (track != 0)
        {
            if (!visited.Add((track, sector))) throw CommodoreDosExceptions.CyclicChain(name, track, sector);
            if (!TryGetSector(image, track, sector, out var data)) throw CommodoreDosExceptions.MissingDataSector(name, track, sector);
            var nextTrack = data[CommodoreDosLayout.NextTrackOffset];
            var nextSector = data[CommodoreDosLayout.NextSectorOffset];
            var used = nextTrack == 0 ? Math.Clamp(nextSector - 1, 0, CommodoreDosLayout.DataBytesPerSector) : CommodoreDosLayout.DataBytesPerSector;
            stream.Write(data, CommodoreDosLayout.DirectoryEntriesOffset, used);
            track = nextTrack;
            sector = nextSector;
            if (stream.Length > image.Capacity) { warnings.Add(CommodoreDosExceptions.ChainExceedsCapacity(name)); break; }
        }
        return stream.ToArray();
    }

    /// <summary>Compte les blocs libres déclarés dans le BAM.</summary>
    private static int ReadFreeBlocks(SectorImage image, CommodoreDosLayout layout)
    {
        if (ReferenceEquals(layout, CommodoreDosLayout.D64D71))
        {
            var tracksPerSide = image.Cylinders;
            var total = 0;
            if (TryGetSector(image, layout.HeaderTrack, layout.BamSectors[0], out var bam))
                for (var track = 1; track <= Math.Min(layout.BamEntryCount, tracksPerSide); track++) total += bam[layout.BamEntriesOffset + (track - 1) * layout.BamEntrySize];
            if (image.Heads > DiskGeometryConstants.SingleSidedHeadCount && TryGetSector(image, layout.HeaderTrack + tracksPerSide, layout.BamSectors[0], out var secondBam))
                for (var track = 1; track <= Math.Min(layout.BamEntryCount, tracksPerSide); track++) total += secondBam[layout.BamEntriesOffset + (track - 1) * layout.BamEntrySize];
            return total;
        }
        var free = 0;
        foreach (var bamSector in layout.BamSectors)
        {
            if (!TryGetSector(image, layout.HeaderTrack, bamSector, out var bam)) continue;
            for (var entry = 0; entry < layout.BamEntryCount; entry++)
            {
                var offset = layout.BamEntriesOffset + entry * layout.BamEntrySize;
                if (offset < bam.Length) free += bam[offset];
            }
        }
        return free;
    }

    /// <summary>Tente de lire un secteur Commodore à partir de son adresse piste/secteur.</summary>
    internal static bool TryGetSector(SectorImage image, int track, int sector, out byte[] data)
    {
        try
        {
            var logical = ToLogicalBlock(image, track, sector);
            if (image.TryGetBlock(logical, out var block) && block.Data.Count == CommodoreDosLayout.SectorSize) { data = block.Data.ToArray(); return true; }
        }
        catch (ArgumentOutOfRangeException) { }
        data = [];
        return false;
    }

    /// <summary>Convertit une adresse Commodore en numéro de bloc logique.</summary>
    internal static int ToLogicalBlock(SectorImage image, int track, int sector)
    {
        if (image.FormatId == DiskImageFormatIds.Commodore1581) return Commodore1581Geometry.ToLogicalBlock(track, sector);
        var tracksPerSide = image.Cylinders;
        var side = track > tracksPerSide ? 1 : 0;
        var sideTrack = side == 0 ? track : track - tracksPerSide;
        return image.Heads == Commodore1571Geometry.SideCount ? Commodore1571Geometry.ToLogicalBlock(sideTrack, sector, tracksPerSide, side) : Commodore1541Geometry.ToSideLogicalBlock(sideTrack, sector, tracksPerSide);
    }

    /// <summary>Tente de convertir une adresse Commodore en numéro de bloc logique.</summary>
    private static int TryToLogicalBlock(SectorImage image, int track, int sector)
    {
        try { return ToLogicalBlock(image, track, sector); }
        catch (ArgumentOutOfRangeException) { return -1; }
    }

    /// <summary>Vérifie que la chaîne de répertoire contient uniquement des entrées plausibles.</summary>
    private static bool HasPlausibleDirectory(SectorImage image, byte[] header, CommodoreDosLayout layout)
    {
        var track = header[CommodoreDosLayout.NextTrackOffset] == 0 ? layout.DirectoryTrack : header[CommodoreDosLayout.NextTrackOffset];
        var sector = header[CommodoreDosLayout.NextTrackOffset] == 0 ? layout.DirectorySector : header[CommodoreDosLayout.NextSectorOffset];
        var visited = new HashSet<(int Track, int Sector)>();
        var valid = 0;
        var invalid = 0;
        while (track != 0 && visited.Count < CommodoreDosLayout.MaximumDirectoryChainLength && visited.Add((track, sector)))
        {
            if (!TryGetSector(image, track, sector, out var data)) return false;
            for (var slot = 0; slot < CommodoreDosLayout.DirectoryEntryCount; slot++)
            {
                var offset = CommodoreDosLayout.DirectoryEntriesOffset + slot * CommodoreDosLayout.DirectoryEntrySize;
                var rawType = data[offset + CommodoreDosLayout.FileTypeOffset];
                if ((rawType & 0x0f) == 0) continue;
                var type = rawType & 0x07;
                var name = PetsciiDecoder.Decode(data.AsSpan(offset + CommodoreDosLayout.FileNameOffset, CommodoreDosLayout.NameLength));
                var dataTrack = data[offset + CommodoreDosLayout.FirstDataTrackOffset];
                var dataSector = data[offset + CommodoreDosLayout.FirstDataSectorOffset];
                var plausible = type is >= 1 and <= 5 && name.Length > 0 && !name.Contains('\ufffd') && (dataTrack == 0 || TryToLogicalBlock(image, dataTrack, dataSector) >= 0);
                if (plausible) valid++; else invalid++;
            }
            track = data[CommodoreDosLayout.NextTrackOffset];
            sector = data[CommodoreDosLayout.NextSectorOffset];
        }
        return invalid == 0 && (valid > 0 || visited.Count == 1);
    }
}
