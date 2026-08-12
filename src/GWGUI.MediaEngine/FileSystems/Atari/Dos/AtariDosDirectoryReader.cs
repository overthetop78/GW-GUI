using System.Buffers.Binary;
using GWGUI.MediaEngine.SectorImages;

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
            if (flags == AtariDosDirectoryFlags.None) continue;
            var nameIsBlank = true;
            for (var character = 0; character < AtariDosFileSystemLayout.NameLength + AtariDosFileSystemLayout.ExtensionLength; character++)
            {
                var value = data[offset + AtariDosFileSystemLayout.NameOffset + character];
                if (value is not (0 or AtariDosFileSystemLayout.NamePadding)) nameIsBlank = false;
                if (value is not (0 or AtariDosFileSystemLayout.NamePadding) && value is < AtariDosFileSystemLayout.MinimumNameCharacter or > AtariDosFileSystemLayout.MaximumNameCharacter) return false;
            }
            if (flags.HasFlag(AtariDosDirectoryFlags.InUse) && !flags.HasFlag(AtariDosDirectoryFlags.Deleted))
            {
                var sectorCount = data[offset + AtariDosFileSystemLayout.SectorCountOffset] | data[offset + AtariDosFileSystemLayout.SectorCountOffset + 1] << 8;
                var firstSector = data[offset + AtariDosFileSystemLayout.FirstSectorOffset] | data[offset + AtariDosFileSystemLayout.FirstSectorOffset + 1] << 8;
                if (nameIsBlank || sectorCount == 0 || firstSector == 0) return false;
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
                if (!flags.HasFlag(AtariDosDirectoryFlags.InUse) || flags.HasFlag(AtariDosDirectoryFlags.Deleted)) continue;
                var sectorCount = BinaryPrimitives.ReadUInt16LittleEndian(sector.AsSpan(offset + AtariDosFileSystemLayout.SectorCountOffset));
                var firstSector = BinaryPrimitives.ReadUInt16LittleEndian(sector.AsSpan(offset + AtariDosFileSystemLayout.FirstSectorOffset));
                var name = AtariDosNameCodec.Decode(sector.AsSpan(offset + AtariDosFileSystemLayout.NameOffset, AtariDosFileSystemLayout.NameLength + AtariDosFileSystemLayout.ExtensionLength));
                var fileNumber = FileNumber(sectorNumber, slot);
                var file = AtariDosFileReader.Read(image, firstSector, sectorCount, fileNumber, warnings, name);
                entries.Add(new(name, FileSystemEntryKind.File, file.Content.Count, null, string.Empty, (byte)flags, firstSector, file.IsValid, [], file.Content));
            }
        }
        return entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    /// <summary>Calcule le numéro de fichier sur six bits depuis le secteur et le slot du répertoire.</summary>
    public static int FileNumber(int sectorNumber, int slot) => (slot + (sectorNumber - AtariDosFileSystemLayout.FirstDirectorySector) * AtariDosFileSystemLayout.DirectoryEntriesPerSector) & 0x3f;
}
