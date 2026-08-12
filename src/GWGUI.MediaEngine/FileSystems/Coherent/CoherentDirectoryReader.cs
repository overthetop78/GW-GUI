using System.Buffers.Binary;

namespace GWGUI.MediaEngine.FileSystems.Coherent;

/// <summary>Parcourt les répertoires COHERENT et construit leurs entrées communes.</summary>
internal static class CoherentDirectoryReader
{
    /// <summary>Lit le répertoire racine en distinguant cycles et secondes références.</summary>
    public static IReadOnlyList<FileSystemEntry> ReadRoot(CoherentImageData image, int inodeZoneEnd, List<string> warnings) => Read(image, inodeZoneEnd, CoherentFileSystemLayout.RootInodeNumber, new HashSet<ushort>(), new HashSet<ushort>(), warnings, CoherentNameCodec.InodeDescription(CoherentFileSystemLayout.RootInodeNumber)).Entries;

    /// <summary>Lit récursivement un répertoire et retourne ses entrées ainsi que la validité de ses données.</summary>
    private static (IReadOnlyList<FileSystemEntry> Entries, bool IsValid) Read(CoherentImageData image, int inodeZoneEnd, ushort inodeNumber, HashSet<ushort> recursionStack, HashSet<ushort> visited, List<string> warnings, string displayName)
    {
        if (recursionStack.Contains(inodeNumber))
        {
            warnings.Add(CoherentWarnings.DirectoryCycle(displayName, inodeNumber));
            return ([], false);
        }
        if (!visited.Add(inodeNumber))
        {
            warnings.Add(CoherentWarnings.DirectoryRepeated(displayName, inodeNumber));
            return ([], false);
        }
        recursionStack.Add(inodeNumber);
        var inode = CoherentInodeReader.Read(image, inodeZoneEnd, inodeNumber);
        var directoryData = CoherentFileDataReader.Read(image, inode, warnings, displayName);
        var result = new List<FileSystemEntry>();
        var allValid = directoryData.IsValid;
        for (var offset = 0; offset + CoherentFileSystemLayout.DirectoryEntrySize <= directoryData.Content.Length; offset += CoherentFileSystemLayout.DirectoryEntrySize)
        {
            var childNumber = BinaryPrimitives.ReadUInt16LittleEndian(directoryData.Content.AsSpan(offset, CoherentFileSystemLayout.DirectoryInodeLength));
            if (childNumber == CoherentFileSystemLayout.NullInodeNumber) continue;
            var name = CoherentNameCodec.Decode(directoryData.Content.AsSpan(offset + CoherentFileSystemLayout.DirectoryInodeLength, CoherentFileSystemLayout.DirectoryNameLength));
            if (name.Length == 0 || name is CoherentFileSystemLayout.CurrentDirectoryName or CoherentFileSystemLayout.ParentDirectoryName) continue;
            try
            {
                var child = CoherentInodeReader.Read(image, inodeZoneEnd, childNumber);
                var kind = child.Mode.Type().ToCommonKind();
                CoherentFileData? content = null;
                IReadOnlyList<FileSystemEntry> children = [];
                var valid = directoryData.IsValid;
                if (kind == FileSystemEntryKind.Directory)
                {
                    var directory = Read(image, inodeZoneEnd, childNumber, recursionStack, visited, warnings, name);
                    children = directory.Entries;
                    valid &= directory.IsValid;
                }
                else
                {
                    content = CoherentFileDataReader.Read(image, child, warnings, name);
                    valid &= content.IsValid;
                }
                result.Add(new(name, kind, child.Size, CoherentFileSystemTime.Decode(child.Modified), CoherentNameCodec.InodeDescription(childNumber), (uint)(child.Mode & CoherentFileSystemLayout.ProtectionMask), childNumber, valid, children, content?.Content));
                allValid &= valid;
            }
            catch (InvalidDataException exception)
            {
                warnings.Add(CoherentWarnings.ChildInodeUnreadable(name, exception));
            }
        }
        recursionStack.Remove(inodeNumber);
        return (result.OrderByDescending(entry => entry.Kind == FileSystemEntryKind.Directory).ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(), allValid);
    }
}
