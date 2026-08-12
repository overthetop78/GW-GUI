using System.Buffers.Binary;
using GWGUI.MediaEngine.FileSystems.Acorn.FileCore;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Acorn.Adfs;

/// <summary>Lit et valide les répertoires ADFS et parcourt récursivement leurs entrées.</summary>
public static class AcornAdfsDirectoryReader
{
    /// <summary>Lit un répertoire et ses descendants.</summary>
    public static AcornAdfsDirectoryData Read(SectorImage image, int address, IFileCoreAddressResolver resolver, HashSet<int> visited, List<string> warnings, int depth)
    {
        if (depth > AcornAdfsLayout.MaximumDepth)
        {
            warnings.Add(AcornAdfsWarnings.DepthLimit(depth));
            return new(string.Empty, string.Empty, []);
        }
        if (!visited.Add(address))
        {
            warnings.Add(AcornAdfsWarnings.CyclicDirectory(address));
            return new(string.Empty, string.Empty, []);
        }
        if (!TryRead(image, address, resolver, out var directory)) throw AcornAdfsExceptions.InvalidDirectory(address);
        var entries = new List<FileSystemEntry>();
        for (var index = 0; index < AcornAdfsLayout.EntryCount; index++)
        {
            var offset = AcornAdfsLayout.EntriesOffset + index * AcornAdfsLayout.EntrySize;
            if (directory[offset] == AcornAdfsLayout.EndOfEntries) break;
            var name = AcornAdfsNameCodec.Decode(directory.AsSpan(offset + AcornAdfsLayout.EntryNameOffset, AcornAdfsLayout.EntryNameLength));
            if (name.Length == 0) continue;
            var load = BinaryPrimitives.ReadUInt32LittleEndian(directory.AsSpan(offset + AcornAdfsLayout.EntryLoadOffset, sizeof(uint)));
            var execute = BinaryPrimitives.ReadUInt32LittleEndian(directory.AsSpan(offset + AcornAdfsLayout.EntryExecuteOffset, sizeof(uint)));
            var length = BinaryPrimitives.ReadUInt32LittleEndian(directory.AsSpan(offset + AcornAdfsLayout.EntryLengthOffset, sizeof(uint)));
            var indirectAddress = LittleEndianUInt24.Read(directory, offset + AcornAdfsLayout.EntryIndirectAddressOffset);
            var attributes = directory[offset + AcornAdfsLayout.EntryAttributesOffset];
            var isDirectory = (attributes & AcornAdfsLayout.DirectoryAttribute) != 0;
            IReadOnlyList<FileSystemEntry> children = [];
            IReadOnlyList<byte>? content = null;
            var metadataValid = resolver.TryResolveByteOffset(indirectAddress, 0, out _);
            if (isDirectory && metadataValid)
            {
                try { children = Read(image, indirectAddress, resolver, visited, warnings, depth + 1).Children; }
                catch (InvalidDataException exception)
                {
                    warnings.Add(Definitions.FileSystemWarningMessages.EntryReadFailure(name, exception));
                    metadataValid = false;
                }
            }
            else if (!isDirectory) content = AcornAdfsFileReader.Read(image, indirectAddress, length, resolver, name, warnings, ref metadataValid);
            var type = Acorn.AcornFileSystemTime.HasTimestamp(load) ? (load >> BitPrimitives.BitsPerByte) & 0xfff : 0u;
            entries.Add(new(name, isDirectory ? FileSystemEntryKind.Directory : FileSystemEntryKind.File, isDirectory ? 0 : length, Acorn.AcornFileSystemTime.Decode(load, execute), Describe(load, execute, type), attributes, indirectAddress, metadataValid, children, content));
        }
        var title = AcornAdfsNameCodec.Decode(directory.AsSpan(AcornAdfsLayout.TitleOffset, AcornAdfsLayout.TitleLength));
        var directoryName = AcornAdfsNameCodec.Decode(directory.AsSpan(AcornAdfsLayout.DirectoryNameOffset, AcornAdfsLayout.DirectoryNameLength));
        return new(directoryName, title, entries.OrderBy(entry => entry.Kind != FileSystemEntryKind.Directory).ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>Tente de lire et de valider un répertoire complet.</summary>
    public static bool TryRead(SectorImage image, int address, IFileCoreAddressResolver resolver, out byte[] directory)
    {
        if (!TryReadBytes(image, address, resolver, AcornAdfsLayout.DirectorySize, out directory)) return false;
        var header = directory.AsSpan(AcornAdfsLayout.HeaderSignatureOffset, AcornAdfsLayout.SignatureLength);
        var footer = directory.AsSpan(AcornAdfsLayout.FooterSignatureOffset, AcornAdfsLayout.SignatureLength);
        return (header.SequenceEqual(AcornAdfsLayout.HugoSignature) || header.SequenceEqual(AcornAdfsLayout.NickSignature)) && footer.SequenceEqual(header) && directory[0] == directory[AcornAdfsLayout.TailSequenceOffset];
    }

    /// <summary>Tente de lire une plage d'octets à travers un résolveur FileCore.</summary>
    public static bool TryReadBytes(SectorImage image, int address, IFileCoreAddressResolver resolver, int length, out byte[] output)
    {
        output = new byte[length];
        var copied = 0;
        while (copied < length)
        {
            if (!resolver.TryResolveByteOffset(address, copied, out var byteOffset)) return false;
            var blockNumber = checked((int)(byteOffset / AcornAdfsLayout.BlockSize));
            var offsetInBlock = checked((int)(byteOffset % AcornAdfsLayout.BlockSize));
            if (!image.TryGetBlock(blockNumber, out var block) || block.Data.Count != AcornAdfsLayout.BlockSize) return false;
            var count = Math.Min(AcornAdfsLayout.BlockSize - offsetInBlock, length - copied);
            block.Data.ToArray().AsSpan(offsetInBlock, count).CopyTo(output.AsSpan(copied));
            copied += count;
        }
        return true;
    }

    /// <summary>Construit la description technique d'une entrée.</summary>
    public static string Describe(uint load, uint execute, uint type) => Acorn.AcornFileSystemTime.HasTimestamp(load) ? $"RISC OS file type &{type:X3}, load &{load:X8}, execute &{execute:X8}" : $"ADFS load &{load:X8}, execute &{execute:X8}";
}
