using System.Buffers.Binary;

using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaFileSystems.FileSystems.Atari.Dos;

/// <summary>Lit le répertoire Atari DOS avant de reconstruire et d'analyser globalement ses fichiers.</summary>
public static class AtariDosDirectoryReader
{
    private const int MaximumDirectoryEntries = 64;

    /// <summary>Indique si un secteur de répertoire contient uniquement des entrées plausibles.</summary>
    public static bool LooksValid(IReadOnlyList<byte> data)
    {
        if (data.Count < AtariDosFileSystemLayout.MinimumSectorSize) return false;
        for (var index = 0; index < AtariDosFileSystemLayout.DirectoryEntriesPerSector; index++)
        {
            var offset = index * AtariDosFileSystemLayout.DirectoryEntrySize;
            var flags = (AtariDosDirectoryFlags)data[offset];
            if (IsEndOfDirectory(flags)) return true;
            if (HasUnknownFlags(flags)) return false;
            if (!IsRecorded(flags) && !flags.HasFlag(AtariDosDirectoryFlags.Deleted)) return false;
            var nameIsBlank = true;
            for (var character = 0; character < AtariDosFileSystemLayout.NameLength + AtariDosFileSystemLayout.ExtensionLength; character++)
            {
                var value = data[offset + AtariDosFileSystemLayout.NameOffset + character];
                if (value is not (0 or AtariDosFileSystemLayout.NamePadding)) nameIsBlank = false;
                if (value is not (0 or AtariDosFileSystemLayout.NamePadding)
                    && value is < AtariDosFileSystemLayout.MinimumNameCharacter or > AtariDosFileSystemLayout.MaximumNameCharacter)
                    return false;
            }
            if (IsRecorded(flags) && !flags.HasFlag(AtariDosDirectoryFlags.Deleted))
            {
                var sectorCount = data[offset + AtariDosFileSystemLayout.SectorCountOffset]
                    | data[offset + AtariDosFileSystemLayout.SectorCountOffset + 1] << 8;
                var firstSector = data[offset + AtariDosFileSystemLayout.FirstSectorOffset]
                    | data[offset + AtariDosFileSystemLayout.FirstSectorOffset + 1] << 8;
                if (nameIsBlank
                    || sectorCount == 0 && firstSector != 0 && !flags.HasFlag(AtariDosDirectoryFlags.OpenForOutput)
                    || sectorCount != 0 && firstSector == 0)
                    return false;
            }
        }
        return true;
    }

    /// <summary>Lit les entrées actives du catalogue canonique ou déplacé.</summary>
    public static IReadOnlyList<FileSystemEntry> Read(IMediaSectorImage image, ICollection<string> warnings)
    {
        if (!TryLocate(image, out var location)) return [];
        return Read(image, location, warnings);
    }

    /// <summary>Parse tout le catalogue, lit les étendues déclarées, puis analyse leurs allocations communes.</summary>
    public static IReadOnlyList<FileSystemEntry> Read(
        IMediaSectorImage image,
        AtariDosDirectoryLocation location,
        ICollection<string> warnings)
    {
        var directoryEntries = ReadDirectoryEntries(image, location, warnings);
        var files = ReadFiles(image, directoryEntries, UsesExtendedLinks(image), warnings);
        var analyzedFiles = AtariDosAllocationAnalyzer.Analyze(directoryEntries, files, warnings);
        return directoryEntries
            .Select(entry => CreateFileSystemEntry(entry, analyzedFiles[entry.EntryNumber]))
            .OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>Retrouve le catalogue normal ou, s'il est illisible, un catalogue déplacé validé par ses fichiers.</summary>
    public static bool TryLocate(IMediaSectorImage image, out AtariDosDirectoryLocation location)
    {
        location = new AtariDosDirectoryLocation(
            AtariDosFileSystemLayout.FirstDirectorySector,
            AtariDosFileSystemLayout.DirectorySectorCount,
            AtariDosFileSystemLayout.DirectoryEntriesPerSector);
        if (TryAnalyzeDirectory(image, location, false, out _, out _)) return true;

        AtariDosDirectoryLocation? best = null;
        var bestReadable = 0;
        var bestRecorded = 0;
        for (var firstSector = 1; firstSector <= image.BlockCount; firstSector++)
        {
            if (firstSector is >= AtariDosFileSystemLayout.VtocSector and <= AtariDosFileSystemLayout.LastDirectorySector)
                continue;
            if (!AtariDosVtocReader.TrySector(image, firstSector, out var first)
                || first.Length < AtariDosFileSystemLayout.MinimumSectorSize)
                continue;
            var entriesPerSector = first.Length / AtariDosFileSystemLayout.DirectoryEntrySize;
            if (entriesPerSector < AtariDosFileSystemLayout.DirectoryEntriesPerSector) continue;
            var sectorCount = (MaximumDirectoryEntries + entriesPerSector - 1) / entriesPerSector;
            var candidate = new AtariDosDirectoryLocation(firstSector, sectorCount, entriesPerSector);
            if (!TryAnalyzeDirectory(image, candidate, true, out var recorded, out var readable)) continue;
            if (readable < bestReadable || readable == bestReadable && recorded <= bestRecorded) continue;
            best = candidate;
            bestReadable = readable;
            bestRecorded = recorded;
        }
        if (best is null) return false;
        location = best;
        return true;
    }

    /// <summary>Indique si le catalogue canonique contient au moins une entrée active avant sa fin.</summary>
    public static bool ContainsRecordedEntry(IMediaSectorImage image)
    {
        var location = new AtariDosDirectoryLocation(
            AtariDosFileSystemLayout.FirstDirectorySector,
            AtariDosFileSystemLayout.DirectorySectorCount,
            AtariDosFileSystemLayout.DirectoryEntriesPerSector);
        return TryReadDirectoryEntries(image, location, false, out var entries, out _)
            && entries.Count != 0;
    }

    /// <summary>Indique si au moins une entrée active possède une étendue déclarée entièrement lisible.</summary>
    public static bool ContainsReadableEntry(IMediaSectorImage image)
    {
        var location = new AtariDosDirectoryLocation(
            AtariDosFileSystemLayout.FirstDirectorySector,
            AtariDosFileSystemLayout.DirectorySectorCount,
            AtariDosFileSystemLayout.DirectoryEntriesPerSector);
        return TryAnalyzeDirectory(image, location, false, out _, out var readable)
            && readable != 0;
    }

    private static IReadOnlyList<AtariDosDirectoryEntry> ReadDirectoryEntries(
        IMediaSectorImage image,
        AtariDosDirectoryLocation location,
        ICollection<string> warnings)
    {
        var entries = new List<AtariDosDirectoryEntry>();
        for (var entryNumber = 0; entryNumber < MaximumDirectoryEntries; entryNumber++)
        {
            var sectorNumber = location.FirstSector + entryNumber / location.EntriesPerSector;
            if (sectorNumber >= location.FirstSector + location.SectorCount) break;
            if (!AtariDosVtocReader.TrySector(image, sectorNumber, out var sector)
                || sector.Length < AtariDosFileSystemLayout.MinimumSectorSize)
            {
                warnings.Add(AtariDosFileSystemExceptions.MissingDirectorySector(sectorNumber));
                continue;
            }
            var slot = entryNumber % location.EntriesPerSector;
            var offset = slot * AtariDosFileSystemLayout.DirectoryEntrySize;
            var flags = (AtariDosDirectoryFlags)sector[offset + AtariDosFileSystemLayout.FlagsOffset];
            if (IsEndOfDirectory(flags)) break;
            if (!LooksValidEntry(sector, offset, flags))
            {
                warnings.Add(AtariDosFileSystemExceptions.InvalidDirectoryEntry(sectorNumber, slot));
                continue;
            }
            if (!IsRecorded(flags) || flags.HasFlag(AtariDosDirectoryFlags.Deleted)) continue;
            var name = ReadName(sector, offset);
            if (!IsFinalized(flags))
            {
                warnings.Add(AtariDosWarnings.OpenForOutputEntryIgnored(name, sectorNumber, slot));
                continue;
            }
            entries.Add(ReadEntry(entryNumber, sectorNumber, slot, sector, offset, flags, name));
        }
        return entries;
    }

    private static IReadOnlyDictionary<int, AtariDosFileData> ReadFiles(
        IMediaSectorImage image,
        IReadOnlyList<AtariDosDirectoryEntry> entries,
        bool usesExtendedLinks,
        ICollection<string> warnings)
        => entries.ToDictionary(
            entry => entry.EntryNumber,
            entry => AtariDosFileReader.Read(
                image,
                entry.FirstSector,
                entry.DeclaredSectorCount,
                usesExtendedLinks,
                warnings,
                entry.Name));

    private static FileSystemEntry CreateFileSystemEntry(AtariDosDirectoryEntry entry, AtariDosFileData file) =>
        new(
            entry.Name,
            FileSystemEntryKind.File,
            file.Content.Count,
            null,
            string.Empty,
            (byte)entry.Flags,
            entry.FirstSector,
            metadataValid: true,
            [],
            file.Content,
            attributes: file.Attributes,
            dataValid: file.IsValid,
            diagnostics: file.Diagnostics);

    private static bool TryAnalyzeDirectory(
        IMediaSectorImage image,
        AtariDosDirectoryLocation location,
        bool relocated,
        out int recorded,
        out int readable)
    {
        recorded = 0;
        readable = 0;
        if (!TryReadDirectoryEntries(image, location, relocated, out var entries, out var ended)) return false;
        recorded = entries.Count;
        if (!ended && recorded < MaximumDirectoryEntries) return false;
        var warnings = new List<string>();
        var files = ReadFiles(image, entries, UsesExtendedLinks(image), warnings);
        var analyzed = AtariDosAllocationAnalyzer.Analyze(entries, files, warnings);
        readable = analyzed.Values.Count(file => file.IsValid);
        return relocated
            ? recorded >= 2 && readable >= 2 && readable * 2 >= recorded
            : recorded >= 1 && readable >= 1;
    }

    private static bool TryReadDirectoryEntries(
        IMediaSectorImage image,
        AtariDosDirectoryLocation location,
        bool requireValidEntries,
        out IReadOnlyList<AtariDosDirectoryEntry> entries,
        out bool ended)
    {
        var result = new List<AtariDosDirectoryEntry>();
        ended = false;
        for (var entryNumber = 0; entryNumber < MaximumDirectoryEntries; entryNumber++)
        {
            var sectorNumber = location.FirstSector + entryNumber / location.EntriesPerSector;
            if (sectorNumber >= location.FirstSector + location.SectorCount) break;
            if (!AtariDosVtocReader.TrySector(image, sectorNumber, out var sector))
            {
                entries = [];
                return false;
            }
            var slot = entryNumber % location.EntriesPerSector;
            var offset = slot * AtariDosFileSystemLayout.DirectoryEntrySize;
            if (offset + AtariDosFileSystemLayout.DirectoryEntrySize > sector.Length)
            {
                entries = [];
                return false;
            }
            var flags = (AtariDosDirectoryFlags)sector[offset + AtariDosFileSystemLayout.FlagsOffset];
            if (IsEndOfDirectory(flags))
            {
                ended = true;
                break;
            }
            if (!LooksValidEntry(sector, offset, flags))
            {
                if (requireValidEntries)
                {
                    entries = [];
                    return false;
                }
                continue;
            }
            if (!IsRecorded(flags) || flags.HasFlag(AtariDosDirectoryFlags.Deleted) || !IsFinalized(flags)) continue;
            var name = ReadName(sector, offset);
            result.Add(ReadEntry(entryNumber, sectorNumber, slot, sector, offset, flags, name));
        }
        entries = result;
        return true;
    }

    private static AtariDosDirectoryEntry ReadEntry(
        int entryNumber,
        int directorySector,
        int directorySlot,
        IReadOnlyList<byte> sector,
        int offset,
        AtariDosDirectoryFlags flags,
        string name)
        => new(
            entryNumber & 0x3f,
            directorySector,
            directorySlot,
            name,
            flags,
            sector[offset + AtariDosFileSystemLayout.SectorCountOffset]
                | sector[offset + AtariDosFileSystemLayout.SectorCountOffset + 1] << 8,
            sector[offset + AtariDosFileSystemLayout.FirstSectorOffset]
                | sector[offset + AtariDosFileSystemLayout.FirstSectorOffset + 1] << 8);

    private static string ReadName(IReadOnlyList<byte> sector, int offset) =>
        AtariDosNameCodec.Decode(sector
            .Skip(offset + AtariDosFileSystemLayout.NameOffset)
            .Take(AtariDosFileSystemLayout.NameLength + AtariDosFileSystemLayout.ExtensionLength)
            .ToArray());

    private static bool UsesExtendedLinks(IMediaSectorImage image) =>
        AtariDosVtocReader.TrySector(image, AtariDosFileSystemLayout.VtocSector, out var vtoc)
        && vtoc.Length > 0
        && vtoc[0] >= AtariDosFileSystemLayout.MinimumExtendedVtocCode;

    private static bool IsEndOfDirectory(AtariDosDirectoryFlags flags) => flags == AtariDosDirectoryFlags.None;

    private static bool IsRecorded(AtariDosDirectoryFlags flags) =>
        flags.HasFlag(AtariDosDirectoryFlags.InUse) || flags.HasFlag(AtariDosDirectoryFlags.OpenForOutput);

    private static bool IsFinalized(AtariDosDirectoryFlags flags) =>
        IsRecorded(flags)
        && !flags.HasFlag(AtariDosDirectoryFlags.Deleted)
        && !flags.HasFlag(AtariDosDirectoryFlags.OpenForOutput);

    private static bool LooksValidEntry(IReadOnlyList<byte> data, int offset, AtariDosDirectoryFlags flags)
    {
        if (HasUnknownFlags(flags) || !IsRecorded(flags) && !flags.HasFlag(AtariDosDirectoryFlags.Deleted)) return false;
        if (!IsRecorded(flags) || flags.HasFlag(AtariDosDirectoryFlags.Deleted)) return true;
        var nameIsBlank = true;
        for (var character = 0; character < AtariDosFileSystemLayout.NameLength + AtariDosFileSystemLayout.ExtensionLength; character++)
        {
            var value = data[offset + AtariDosFileSystemLayout.NameOffset + character];
            if (value is not (0 or AtariDosFileSystemLayout.NamePadding)) nameIsBlank = false;
            if (value is not (0 or AtariDosFileSystemLayout.NamePadding)
                && value is < AtariDosFileSystemLayout.MinimumNameCharacter or > AtariDosFileSystemLayout.MaximumNameCharacter)
                return false;
        }
        var sectorCount = data[offset + AtariDosFileSystemLayout.SectorCountOffset]
            | data[offset + AtariDosFileSystemLayout.SectorCountOffset + 1] << 8;
        var firstSector = data[offset + AtariDosFileSystemLayout.FirstSectorOffset]
            | data[offset + AtariDosFileSystemLayout.FirstSectorOffset + 1] << 8;
        return !nameIsBlank
            && (sectorCount != 0 || firstSector == 0 || flags.HasFlag(AtariDosDirectoryFlags.OpenForOutput))
            && (sectorCount == 0 || firstSector != 0);
    }

    private static bool HasUnknownFlags(AtariDosDirectoryFlags flags) =>
        (flags & ~(AtariDosDirectoryFlags.OpenForOutput
            | AtariDosDirectoryFlags.CreatedByDos2
            | AtariDosDirectoryFlags.DoubleDensity
            | AtariDosDirectoryFlags.Locked
            | AtariDosDirectoryFlags.InUse
            | AtariDosDirectoryFlags.Deleted)) != 0;
}
