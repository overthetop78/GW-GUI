using System.Buffers.Binary;

using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.FileSystems.Atari.Dos;

/// <summary>Valide et lit les huit secteurs du répertoire Atari DOS.</summary>
public static class AtariDosDirectoryReader
{
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
    public static IReadOnlyList<FileSystemEntry> Read(SectorImage image, ICollection<string> warnings)
    {
        var entries = new List<FileSystemEntry>();
        for (var sectorNumber = AtariDosFileSystemLayout.FirstDirectorySector; sectorNumber <= AtariDosFileSystemLayout.LastDirectorySector; sectorNumber++)
        {
            if (!AtariDosVtocReader.TrySector(image, sectorNumber, out var sector) || sector.Length < AtariDosFileSystemLayout.MinimumSectorSize)
            {
                warnings.Add(AtariDosFileSystemExceptions.MissingDirectorySector(sectorNumber));
                continue;
            }
            for (var slot = 0; slot < AtariDosFileSystemLayout.DirectoryEntriesPerSector; slot++)
            {
                var offset = slot * AtariDosFileSystemLayout.DirectoryEntrySize;
                var flags = (AtariDosDirectoryFlags)sector[offset + AtariDosFileSystemLayout.FlagsOffset];
                if (IsEndOfDirectory(flags)) return entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray();
                if (!LooksValidEntry(sector, offset, flags)) return entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray();
                if (!IsPresent(flags) || flags.HasFlag(AtariDosDirectoryFlags.Deleted)) continue;
                var sectorCount = BinaryPrimitives.ReadUInt16LittleEndian(sector.AsSpan(offset + AtariDosFileSystemLayout.SectorCountOffset));
                var firstSector = BinaryPrimitives.ReadUInt16LittleEndian(sector.AsSpan(offset + AtariDosFileSystemLayout.FirstSectorOffset));
                var name = AtariDosNameCodec.Decode(sector.AsSpan(offset + AtariDosFileSystemLayout.NameOffset, AtariDosFileSystemLayout.NameLength + AtariDosFileSystemLayout.ExtensionLength));
                var fileNumber = FileNumber(sectorNumber, slot);
                var file = AtariDosFileReader.Read(image, firstSector, sectorCount, fileNumber, warnings, name);
                var metadataValid = file.IsValid && !flags.HasFlag(AtariDosDirectoryFlags.OpenForOutput);
                entries.Add(new(name, FileSystemEntryKind.File, file.Content.Count, null, string.Empty, (byte)flags, firstSector, metadataValid, [], file.Content));
            }
        }
        return entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static bool IsEndOfDirectory(AtariDosDirectoryFlags flags) => flags == AtariDosDirectoryFlags.None;

    private static bool IsPresent(AtariDosDirectoryFlags flags) =>
        flags.HasFlag(AtariDosDirectoryFlags.InUse) || flags.HasFlag(AtariDosDirectoryFlags.OpenForOutput);

    /// <summary>Indique si le catalogue contient au moins une entrée active avant sa fin ou sa première donnée invalide.</summary>
    public static bool ContainsRecordedEntry(SectorImage image)
    {
        for (var sectorNumber = AtariDosFileSystemLayout.FirstDirectorySector; sectorNumber <= AtariDosFileSystemLayout.LastDirectorySector; sectorNumber++)
        {
            if (!AtariDosVtocReader.TrySector(image, sectorNumber, out var sector) || sector.Length < AtariDosFileSystemLayout.MinimumSectorSize) return false;
            for (var slot = 0; slot < AtariDosFileSystemLayout.DirectoryEntriesPerSector; slot++)
            {
                var offset = slot * AtariDosFileSystemLayout.DirectoryEntrySize;
                var flags = (AtariDosDirectoryFlags)sector[offset];
                if (IsEndOfDirectory(flags) || !LooksValidEntry(sector, offset, flags)) return false;
                if (IsPresent(flags) || flags.HasFlag(AtariDosDirectoryFlags.Deleted)) return true;
            }
        }
        return false;
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
        (flags & ~(AtariDosDirectoryFlags.OpenForOutput | AtariDosDirectoryFlags.CreatedByDos2 | AtariDosDirectoryFlags.Locked | AtariDosDirectoryFlags.InUse | AtariDosDirectoryFlags.Deleted)) != 0;

    /// <summary>Calcule le numéro de fichier sur six bits depuis le secteur et le slot du répertoire.</summary>
    public static int FileNumber(int sectorNumber, int slot) => (slot + (sectorNumber - AtariDosFileSystemLayout.FirstDirectorySector) * AtariDosFileSystemLayout.DirectoryEntriesPerSector) & 0x3f;
}
