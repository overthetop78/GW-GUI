using System.IO;
using System.Buffers.Binary;

namespace GWGUI.Emulation.Sony.Common.Machines.PlayStation.Functions;

internal static class MemoryCardFunctions
{
    private const int FrameSize = 128;
    private const int FirstDirectoryFrame = 1;
    private const int LastDirectoryFrame = 15;
    private const int FirstBrokenSectorFrame = 16;
    private const int LastBrokenSectorFrame = 35;
    private const int FinalHeaderFrame = 63;
    private const int LinkOffset = 8;
    private const byte FreeBlock = 0xA0;
    private const ushort NoNextBlock = 0xFFFF;
    private const uint NoBrokenSector = 0xFFFFFFFF;
    private const byte HeaderM = (byte)'M';
    private const byte HeaderC = (byte)'C';

    internal static void InitializeBlank(Stream image)
    {
        var directory = new byte[(FinalHeaderFrame + 1) * FrameSize];
        directory[0] = HeaderM;
        directory[1] = HeaderC;
        directory[FrameSize - 1] = HeaderM ^ HeaderC;
        directory.AsSpan(0, FrameSize).CopyTo(directory.AsSpan(FinalHeaderFrame * FrameSize));
        for (var index = FirstDirectoryFrame; index <= LastBrokenSectorFrame; index++)
        {
            var frame = directory.AsSpan(index * FrameSize, FrameSize);
            if (index <= LastDirectoryFrame)
            {
                frame[0] = FreeBlock;
                BinaryPrimitives.WriteUInt16LittleEndian(frame[LinkOffset..], NoNextBlock);
            }
            else if (index >= FirstBrokenSectorFrame)
            {
                BinaryPrimitives.WriteUInt32LittleEndian(frame, NoBrokenSector);
                BinaryPrimitives.WriteUInt16LittleEndian(frame[LinkOffset..], NoNextBlock);
            }
            for (var offset = 0; offset < FrameSize - 1; offset++) frame[FrameSize - 1] ^= frame[offset];
        }
        image.Position = 0;
        image.Write(directory);
    }
}
