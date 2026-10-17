using System.Buffers;
using GWGUI.Emulation.Services;

namespace GWGUI.Emulation.Coleco.Common.Services;

internal sealed class VideoBufferSet(ArrayPool<byte>? pool = null)
    : EmulationVideoBufferSet(VideoConstants.BufferCount, pool)
{
}
