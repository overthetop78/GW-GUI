using System.Buffers.Binary;

using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaFileSystems.FileSystems.Atari.Dos;

/// <summary>Valide et lit les huit secteurs du répertoire Atari DOS.</summary>
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
            if (!IsPresent(flags) && !flags.HasFlag(AtariDosDirectoryFlags.Deleted)) return false;
            var nameIsBlank = true;
            for (var character = 0; character < AtariDosFileSystemLayout.NameLength + AtariDosFileSystemLayout.ExtensionLength; character++)
            {
                var value = data[offset + AtariDosFileSystemLayout.NameOffset + character];
                if (value is not (0 or AtariDosFileSystemLayout.NamePadding)) nameIsBlank = false;
                if (value is not (0 or AtariDosFileSystemLayout.NamePadding) && value is < AtariDosFileSystemLayout.MinimumNameCharacter or > AtariDosFileSystemLayout.MaximumNameCharacter) return false;
            }
            if (IsPresent(flags) && !flags.HasFlag(AtariDosDirectoryFlags.Deleted))
            {
                var sectorCount = data[offset + AtariDosFileSystemLayout.SectorCountOffset] | data[offset + AtariDosFileSystemLayout.SectorCountOffset + 1] << 8;
                var firstSector = data[offset + AtariDosFileSystemLayout.FirstSectorOffset] | data[offset + AtariDosFileSystemLayout.FirstSectorOffset + 1] << 8;
                if (nameIsBlank || (sectorCount == 0 && firstSector != 0 && !flags.HasFlag(AtariDosDirectoryFlags.OpenForOutput)) ||
                    (sectorCount != 0 && firstSector == 0)) return false;
            }
        }
        return true;
    }

    /// <summary>Lit les entrées actives et ajoute les avertissements des secteurs absents ou tronqués.</summary>
    public static IReadOnlyList<FileSystemEntry> Read(IMediaSectorImage image, ICollection<string> warnings)
    {
        if (!TryLocate(image, out var location)) return [];
        return Read(image, location, warnings);
    }

    /// <summary>Lit les entrées actives du catalogue indiqué.</summary>
    public static IReadOnlyList<FileSystemEntry> Read(
        IMediaSectorImage image,
        AtariDosDirectoryLocation location,
        ICollection<string> warnings)
    {
        var entries = new List<FileSystemEntry>();
        var usesExtendedLinks = AtariDosVtocReader.TrySector(image, AtariDosFileSystemLayout.VtocSector, out var vtoc)
            && vtoc.Length > 0
            && vtoc[0] >= 4;
        for (var entryNumber = 0; entryNumber < MaximumDirectoryEntries; entryNumber++)
        {
            var sectorNumber = location.FirstSector + entryNumber / location.EntriesPerSector;
            if (sectorNumber >= location.FirstSector + location.SectorCount) break;
            if (!AtariDosVtocReader.TrySector(image, sectorNumber, out var sector) || sector.Length < AtariDosFileSystemLayout.MinimumSectorSize)
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
            if (!IsPresent(flags) || flags.HasFlag(AtariDosDirectoryFlags.Deleted)) continue;
            var sectorCount = BinaryPrimitives.ReadUInt16LittleEndian(sector.AsSpan(offset + AtariDosFileSystemLayout.SectorCountOffset));
            var firstSector = BinaryPrimitives.ReadUInt16LittleEndian(sector.AsSpan(offset + AtariDosFileSystemLayout.FirstSectorOffset));
            var name = AtariDosNameCodec.Decode(sector.AsSpan(offset + AtariDosFileSystemLayout.NameOffset, AtariDosFileSystemLayout.NameLength + AtariDosFileSystemLayout.ExtensionLength));
            var fileNumber = entryNumber & 0x3f;
            var file = AtariDosFileReader.Read(image, firstSector, sectorCount, fileNumber, usesExtendedLinks, warnings, name);
            var metadataValid = file.IsValid && !flags.HasFlag(AtariDosDirectoryFlags.OpenForOutput);
            entries.Add(new(name, FileSystemEntryKind.File, file.Content.Count, null, string.Empty, (byte)flags, firstSector, metadataValid, [], file.Content));
        }
        return entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray();
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

    private static bool IsEndOfDirectory(AtariDosDirectoryFlags flags) => flags == AtariDosDirectoryFlags.None;

    private static bool IsPresent(AtariDosDirectoryFlags flags) =>
        flags.HasFlag(AtariDosDirectoryFlags.InUse) || flags.HasFlag(AtariDosDirectoryFlags.OpenForOutput);

    /// <summary>Indique si le catalogue contient au moins une entrée active avant sa fin ou sa première donnée invalide.</summary>
    public static bool ContainsRecordedEntry(IMediaSectorImage image)
    {
        for (var sectorNumber = AtariDosFileSystemLayout.FirstDirectorySector; sectorNumber <= AtariDosFileSystemLayout.LastDirectorySector; sectorNumber++)
        {
            if (!AtariDosVtocReader.TrySector(image, sectorNumber, out var sector) || sector.Length < AtariDosFileSystemLayout.MinimumSectorSize) return false;
            for (var slot = 0; slot < AtariDosFileSystemLayout.DirectoryEntriesPerSector; slot++)
            {
                var offset = slot * AtariDosFileSystemLayout.DirectoryEntrySize;
                var flags = (AtariDosDirectoryFlags)sector[offset];
                if (IsEndOfDirectory(flags)) return false;
                if (!LooksValidEntry(sector, offset, flags)) continue;
                if (IsPresent(flags) && !flags.HasFlag(AtariDosDirectoryFlags.Deleted)) return true;
            }
        }
        return false;
    }

    /// <summary>Indique si au moins une entrée active possède une chaîne de secteurs cohérente.</summary>
    public static bool ContainsReadableEntry(IMediaSectorImage image)
    {
        var usesExtendedLinks = AtariDosVtocReader.TrySector(image, AtariDosFileSystemLayout.VtocSector, out var vtoc)
            && vtoc.Length > 0
            && vtoc[0] >= 4;
        for (var sectorNumber = AtariDosFileSystemLayout.FirstDirectorySector; sectorNumber <= AtariDosFileSystemLayout.LastDirectorySector; sectorNumber++)
        {
            if (!AtariDosVtocReader.TrySector(image, sectorNumber, out var sector) || sector.Length < AtariDosFileSystemLayout.MinimumSectorSize)
                return false;

            for (var slot = 0; slot < AtariDosFileSystemLayout.DirectoryEntriesPerSector; slot++)
            {
                var offset = slot * AtariDosFileSystemLayout.DirectoryEntrySize;
                var flags = (AtariDosDirectoryFlags)sector[offset + AtariDosFileSystemLayout.FlagsOffset];
                if (IsEndOfDirectory(flags)) return false;
                if (!LooksValidEntry(sector, offset, flags) || !IsPresent(flags) || flags.HasFlag(AtariDosDirectoryFlags.Deleted))
                    continue;

                var sectorCount = BinaryPrimitives.ReadUInt16LittleEndian(sector.AsSpan(offset + AtariDosFileSystemLayout.SectorCountOffset));
                var firstSector = BinaryPrimitives.ReadUInt16LittleEndian(sector.AsSpan(offset + AtariDosFileSystemLayout.FirstSectorOffset));
                var name = AtariDosNameCodec.Decode(sector.AsSpan(offset + AtariDosFileSystemLayout.NameOffset, AtariDosFileSystemLayout.NameLength + AtariDosFileSystemLayout.ExtensionLength));
                var warnings = new List<string>();
                var file = AtariDosFileReader.Read(image, firstSector, sectorCount, FileNumber(sectorNumber, slot), usesExtendedLinks, warnings, name);
                if (file.IsValid && !flags.HasFlag(AtariDosDirectoryFlags.OpenForOutput))
                    return true;
            }
        }
        return false;
    }

    private static bool TryAnalyzeDirectory(
        IMediaSectorImage image,
        AtariDosDirectoryLocation location,
        bool relocated,
        out int recorded,
        out int readable)
    {
        recorded = 0;
        readable = 0;
        var ended = false;
        var usesExtendedLinks = AtariDosVtocReader.TrySector(image, AtariDosFileSystemLayout.VtocSector, out var vtoc)
            && vtoc.Length > 0
            && vtoc[0] >= AtariDosFileSystemLayout.MinimumExtendedVtocCode;
        for (var entryNumber = 0; entryNumber < MaximumDirectoryEntries; entryNumber++)
        {
            var sectorNumber = location.FirstSector + entryNumber / location.EntriesPerSector;
            if (sectorNumber >= location.FirstSector + location.SectorCount
                || !AtariDosVtocReader.TrySector(image, sectorNumber, out var sector))
                return false;
            var slot = entryNumber % location.EntriesPerSector;
            var offset = slot * AtariDosFileSystemLayout.DirectoryEntrySize;
            if (offset + AtariDosFileSystemLayout.DirectoryEntrySize > sector.Length) return false;
            var flags = (AtariDosDirectoryFlags)sector[offset + AtariDosFileSystemLayout.FlagsOffset];
            if (IsEndOfDirectory(flags))
            {
                ended = true;
                break;
            }
            if (!LooksValidEntry(sector, offset, flags)) return false;
            if (!IsPresent(flags) || flags.HasFlag(AtariDosDirectoryFlags.Deleted)) continue;
            recorded++;
            if (flags.HasFlag(AtariDosDirectoryFlags.OpenForOutput)) continue;
            var sectorCount = BinaryPrimitives.ReadUInt16LittleEndian(
                sector.AsSpan(offset + AtariDosFileSystemLayout.SectorCountOffset));
            var firstSector = BinaryPrimitives.ReadUInt16LittleEndian(
                sector.AsSpan(offset + AtariDosFileSystemLayout.FirstSectorOffset));
            var name = AtariDosNameCodec.Decode(sector.AsSpan(offset + AtariDosFileSystemLayout.NameOffset,
                AtariDosFileSystemLayout.NameLength + AtariDosFileSystemLayout.ExtensionLength));
            var warnings = new List<string>();
            var file = AtariDosFileReader.Read(image, firstSector, sectorCount,
                entryNumber & 0x3f, usesExtendedLinks, warnings, name);
            if (file.IsValid) readable++;
        }
        if (!ended && recorded < MaximumDirectoryEntries) return false;
        return relocated
            ? recorded >= 2 && readable >= 2 && readable * 2 >= recorded
            : recorded >= 1 && readable >= 1;
    }

    private static bool LooksValidEntry(IReadOnlyList<byte> data, int offset, AtariDosDirectoryFlags flags)
    {
        if (HasUnknownFlags(flags) || !IsPresent(flags) && !flags.HasFlag(AtariDosDirectoryFlags.Deleted)) return false;
        if (!IsPresent(flags) || flags.HasFlag(AtariDosDirectoryFlags.Deleted)) return true;
        var nameIsBlank = true;
        for (var character = 0; character < AtariDosFileSystemLayout.NameLength + AtariDosFileSystemLayout.ExtensionLength; character++)
        {
            var value = data[offset + AtariDosFileSystemLayout.NameOffset + character];
            if (value is not (0 or AtariDosFileSystemLayout.NamePadding)) nameIsBlank = false;
            if (value is not (0 or AtariDosFileSystemLayout.NamePadding) && value is < AtariDosFileSystemLayout.MinimumNameCharacter or > AtariDosFileSystemLayout.MaximumNameCharacter) return false;
        }
        var sectorCount = data[offset + AtariDosFileSystemLayout.SectorCountOffset] | data[offset + AtariDosFileSystemLayout.SectorCountOffset + 1] << 8;
        var firstSector = data[offset + AtariDosFileSystemLayout.FirstSectorOffset] | data[offset + AtariDosFileSystemLayout.FirstSectorOffset + 1] << 8;
        return !nameIsBlank && (sectorCount != 0 || firstSector == 0 || flags.HasFlag(AtariDosDirectoryFlags.OpenForOutput)) && (sectorCount == 0 || firstSector != 0);
    }

    private static bool HasUnknownFlags(AtariDosDirectoryFlags flags) =>
        (flags & ~(AtariDosDirectoryFlags.OpenForOutput | AtariDosDirectoryFlags.CreatedByDos2 | AtariDosDirectoryFlags.DoubleDensity | AtariDosDirectoryFlags.Locked | AtariDosDirectoryFlags.InUse | AtariDosDirectoryFlags.Deleted)) != 0;

    /// <summary>Calcule le numéro de fichier sur six bits depuis le secteur et le slot du répertoire.</summary>
    public static int FileNumber(int sectorNumber, int slot) => (slot + (sectorNumber - AtariDosFileSystemLayout.FirstDirectorySector) * AtariDosFileSystemLayout.DirectoryEntriesPerSector) & 0x3f;
}
