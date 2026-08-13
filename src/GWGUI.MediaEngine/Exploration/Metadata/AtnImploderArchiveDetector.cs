using System.Buffers.Binary;

namespace GWGUI.MediaEngine.Exploration.Metadata;

/// <summary>Reconnaît une organisation sectorielle composée de blocs ATN!/File Imploder.</summary>
internal static class AtnImploderArchiveDetector
{
    private const int HeaderSize = 12;
    private const int MinimumMemberCount = 2;
    private const int MaximumExpansionRatio = 1024;
    private static ReadOnlySpan<byte> Signature => "ATN!"u8;

    /// <summary>Compte les blocs ATN! structurellement valides commençant sur une limite de secteur.</summary>
    public static bool TryDetect(ReadOnlySpan<byte> image, int sectorSize, out int memberCount)
    {
        memberCount = 0;
        if (sectorSize <= 0 || image.Length < sectorSize) return false;

        for (var offset = 0; offset <= image.Length - HeaderSize; offset += sectorSize)
        {
            if (!image.Slice(offset, Signature.Length).SequenceEqual(Signature)) continue;
            var expandedSize = BinaryPrimitives.ReadUInt32BigEndian(image[(offset + 4)..]);
            var compressedSize = BinaryPrimitives.ReadUInt32BigEndian(image[(offset + 8)..]);
            if (!IsValidMember(image.Length, offset, compressedSize, expandedSize)) continue;
            memberCount++;
        }
        return memberCount >= MinimumMemberCount;
    }

    private static bool IsValidMember(int imageLength, int offset, uint compressedSize, uint expandedSize)
    {
        if (compressedSize == 0 || expandedSize == 0) return false;
        if ((ulong)expandedSize > (ulong)compressedSize * MaximumExpansionRatio) return false;
        return (ulong)offset + HeaderSize + compressedSize <= (ulong)imageLength;
    }
}
