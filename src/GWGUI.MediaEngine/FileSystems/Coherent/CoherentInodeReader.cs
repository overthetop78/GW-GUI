using System.Buffers.Binary;

namespace GWGUI.MediaEngine.FileSystems.Coherent;

/// <summary>Lit les inodes COHERENT dans les limites de leur zone déclarée.</summary>
internal static class CoherentInodeReader
{
    /// <summary>Lit un inode et copie ses treize pointeurs.</summary>
    public static CoherentInode Read(CoherentImageData image, int inodeZoneEnd, ushort number)
    {
        if (number == 0) throw CoherentExceptions.NullInode();
        var offset = checked(CoherentFileSystemLayout.BlockSize * 2 + (number - 1) * CoherentFileSystemLayout.InodeSize);
        var inodeZoneByteEnd = checked(inodeZoneEnd * CoherentFileSystemLayout.BlockSize);
        if (offset < 0 || offset > inodeZoneByteEnd - CoherentFileSystemLayout.InodeSize || !image.IsRangePresent(offset, CoherentFileSystemLayout.InodeSize)) throw CoherentExceptions.InodeOutsideImage(number, inodeZoneByteEnd);
        var value = image.Bytes.AsSpan(offset, CoherentFileSystemLayout.InodeSize);
        var pointers = new int[CoherentFileSystemLayout.InodePointerCount];
        for (var index = 0; index < pointers.Length; index++) pointers[index] = CoherentBlockPointer.Read(value.Slice(CoherentFileSystemLayout.InodePointersOffset + index * CoherentFileSystemLayout.InodePointerSize, CoherentFileSystemLayout.InodePointerSize));
        return new(BinaryPrimitives.ReadUInt16LittleEndian(value[CoherentFileSystemLayout.InodeModeOffset..]), CoherentFormat.ReadCanonicalUInt32(value.Slice(CoherentFileSystemLayout.InodeSizeOffset, CoherentFormat.UInt32Length)), pointers, CoherentFormat.ReadCanonicalUInt32(value.Slice(CoherentFileSystemLayout.InodeModifiedOffset, CoherentFormat.UInt32Length)));
    }
}
