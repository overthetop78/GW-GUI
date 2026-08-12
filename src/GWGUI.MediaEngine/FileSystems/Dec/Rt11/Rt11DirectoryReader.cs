using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Dec.Rt11;

/// <summary>Parcourt les segments de répertoire RT-11 et reconstruit leurs entrées.</summary>
public static class Rt11DirectoryReader
{
    /// <summary>Lit tous les segments accessibles depuis le premier bloc.</summary>
    public static Rt11DirectoryResult Read(SectorImage image, int directoryBlock)
    {
        var entries = new List<FileSystemEntry>();
        var warnings = new List<string>();
        var freeBlocks = 0L;
        var seenSegments = new HashSet<int>();
        var segment = 1;
        var valid = true;
        while (segment != 0)
        {
            if (segment is < 1 or > Rt11FileSystemLayout.MaximumSegmentCount) { warnings.Add(Rt11FileSystemExceptions.InvalidSegment(segment, directoryBlock, "numéro hors plage")); valid = false; break; }
            if (!seenSegments.Add(segment)) { warnings.Add(Rt11FileSystemExceptions.InvalidSegment(segment, directoryBlock, "cycle")); valid = false; break; }
            var firstBlockLong = (long)directoryBlock + (segment - 1L) * Rt11FileSystemLayout.SegmentBlockCount;
            if (firstBlockLong < 0 || firstBlockLong + 1 >= image.BlockCount) { warnings.Add(Rt11FileSystemExceptions.InvalidSegment(segment, checked((int)Math.Clamp(firstBlockLong, int.MinValue, int.MaxValue)), "bloc hors image")); valid = false; break; }
            var firstBlock = (int)firstBlockLong;
            var pair = Rt11BlockPairReader.Read(image, firstBlock);
            if (!pair.IsValid) { warnings.Add(Rt11FileSystemExceptions.InvalidSegment(segment, firstBlock, "paire absente ou tronquée")); valid = false; break; }
            var bytes = pair.Bytes.ToArray();
            var entrySize = Rt11FileSystemLayout.MinimumEntrySize + Rt11Primitives.ReadUInt16(bytes, Rt11FileSystemLayout.ExtraBytesOffset);
            if (entrySize is < Rt11FileSystemLayout.MinimumEntrySize or > Rt11FileSystemLayout.MaximumEntrySize) { warnings.Add(Rt11FileSystemExceptions.InvalidEntrySize(segment, entrySize)); valid = false; break; }
            var dataBlock = (int)Rt11Primitives.ReadUInt16(bytes, Rt11FileSystemLayout.DataBlockOffset);
            ReadEntries(image, bytes, entrySize, ref dataBlock, entries, warnings, ref freeBlocks, ref valid);
            segment = Rt11Primitives.ReadUInt16(bytes, Rt11FileSystemLayout.NextSegmentOffset);
        }
        return new(entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(), valid ? freeBlocks : 0, valid, warnings);
    }

    /// <summary>Lit les entrées complètes d'un segment et avance toujours selon leur longueur déclarée.</summary>
    private static void ReadEntries(SectorImage image, ReadOnlySpan<byte> bytes, int entrySize, ref int dataBlock, ICollection<FileSystemEntry> entries, ICollection<string> warnings, ref long freeBlocks, ref bool valid)
    {
        for (var offset = Rt11FileSystemLayout.EntriesOffset; offset + entrySize <= bytes.Length; offset += entrySize)
        {
            var status = (Rt11DirectoryEntryStatus)Rt11Primitives.ReadUInt16(bytes, offset + Rt11FileSystemLayout.StatusOffset);
            if (status.HasFlag(Rt11DirectoryEntryStatus.EndOfSegment)) break;
            var blockLength = Rt11Primitives.ReadUInt16(bytes, offset + Rt11FileSystemLayout.BlockLengthOffset);
            if (status.HasFlag(Rt11DirectoryEntryStatus.Empty)) { freeBlocks += blockLength; dataBlock = checked(dataBlock + blockLength); continue; }
            if ((status & (Rt11DirectoryEntryStatus.Permanent | Rt11DirectoryEntryStatus.Tentative)) == 0) { dataBlock = checked(dataBlock + blockLength); continue; }
            var name = (Rt11Radix50.Decode(Rt11Primitives.ReadUInt16(bytes, offset + Rt11FileSystemLayout.NameOffset)) + Rt11Radix50.Decode(Rt11Primitives.ReadUInt16(bytes, offset + Rt11FileSystemLayout.NameOffset + sizeof(ushort)))).TrimEnd();
            var extension = Rt11Radix50.Decode(Rt11Primitives.ReadUInt16(bytes, offset + Rt11FileSystemLayout.ExtensionOffset)).TrimEnd();
            if (extension.Length != 0) name += "." + extension;
            if (string.IsNullOrWhiteSpace(name)) { warnings.Add(Rt11FileSystemExceptions.EmptyName(dataBlock, offset)); dataBlock = checked(dataBlock + blockLength); continue; }
            var content = Rt11FileContentReader.Read(image, dataBlock, blockLength);
            if (!content.IsValid) { warnings.Add(Rt11FileSystemExceptions.MissingContent(name, content.MissingBlocks)); valid = false; }
            entries.Add(new(name, FileSystemEntryKind.File, blockLength * (long)Rt11FileSystemLayout.BlockSize, Rt11Date.Decode(Rt11Primitives.ReadUInt16(bytes, offset + Rt11FileSystemLayout.DateOffset)), Rt11FileSystemLayout.FileDescription(status), status.HasFlag(Rt11DirectoryEntryStatus.Protected) ? Rt11FileSystemLayout.ProtectedAttribute : Rt11FileSystemLayout.UnprotectedAttribute, dataBlock, content.IsValid, [], content.Content));
            dataBlock = checked(dataBlock + blockLength);
        }
    }
}

/// <summary>Résultat de lecture du répertoire RT-11.</summary>
/// <param name="Entries">Entrées reconnues et triées.</param>
/// <param name="FreeBlocks">Blocs libres lorsque la chaîne est valide.</param>
/// <param name="IsValid">Validité de toute la chaîne.</param>
/// <param name="Warnings">Diagnostics produits pendant la lecture.</param>
public sealed record Rt11DirectoryResult(IReadOnlyList<FileSystemEntry> Entries, long FreeBlocks, bool IsValid, IReadOnlyList<string> Warnings);
