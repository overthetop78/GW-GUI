using System.Buffers.Binary;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Amiga.FlatArchive;

/// <summary>Reconnaît et décode la table de ressources sans construire le volume final.</summary>
internal static class AmigaFlatResourceDirectoryReader
{
    public static bool TryRead(SectorImage image, out IReadOnlyList<AmigaFlatResourceDescriptor> descriptors)
    {
        descriptors = [];
        if (image.BlockSize < AmigaFlatResourceArchiveLayout.EntryLength ||
            !image.TryGetBlock(AmigaFlatResourceArchiveLayout.DirectoryStartBlock, out var firstBlock)) return false;
        if (!TryReadDescriptor(firstBlock.Data.ToArray(), 0, out var reserved) ||
            reserved.Name != AmigaFlatResourceArchiveLayout.ReservedName) return false;
        if (reserved.Length < (AmigaFlatResourceArchiveLayout.DirectoryStartBlock + 1L) * image.BlockSize || reserved.Length > image.Capacity) return false;

        var directoryLength = checked((int)(reserved.Length - (long)AmigaFlatResourceArchiveLayout.DirectoryStartBlock * image.BlockSize));
        if (directoryLength < AmigaFlatResourceArchiveLayout.EntryLength || directoryLength > image.Capacity) return false;
        var read = AmigaFlatResourceDataReader.Read(image, (long)AmigaFlatResourceArchiveLayout.DirectoryStartBlock * image.BlockSize, directoryLength);
        if (read.MissingBlocks.Count != 0 || read.InvalidBlocks.Count != 0) return false;

        var parsed = new List<AmigaFlatResourceDescriptor>();
        long payloadLength = 0;
        for (var offset = 0; offset + AmigaFlatResourceArchiveLayout.EntryLength <= read.Bytes.Length; offset += AmigaFlatResourceArchiveLayout.EntryLength)
        {
            if (read.Bytes[offset] == byte.MaxValue) break;
            if (!TryReadDescriptor(read.Bytes, offset, out var descriptor)) return false;
            parsed.Add(descriptor);
            payloadLength += descriptor.Length;
            if (payloadLength > image.Capacity) return false;
        }
        if (parsed.Count < 2 || parsed[0].Name != AmigaFlatResourceArchiveLayout.ReservedName || payloadLength > image.Capacity) return false;
        descriptors = parsed;
        return true;
    }

    private static bool TryReadDescriptor(ReadOnlySpan<byte> bytes, int offset, out AmigaFlatResourceDescriptor descriptor)
    {
        descriptor = default!;
        if (offset < 0 || offset + AmigaFlatResourceArchiveLayout.EntryLength > bytes.Length) return false;
        var nameBytes = bytes.Slice(offset, AmigaFlatResourceArchiveLayout.NameLength);
        var terminator = nameBytes.IndexOf((byte)0);
        if (terminator <= 0) return false;
        for (var index = 0; index < terminator; index++) if (nameBytes[index] is < 0x20 or > 0x7e) return false;
        for (var index = terminator + 1; index < nameBytes.Length; index++) if (nameBytes[index] is not (0 or byte.MaxValue)) return false;
        var length = BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(offset + AmigaFlatResourceArchiveLayout.SizeOffset, sizeof(uint)));
        descriptor = new(System.Text.Encoding.ASCII.GetString(nameBytes[..terminator]), length);
        return true;
    }
}
