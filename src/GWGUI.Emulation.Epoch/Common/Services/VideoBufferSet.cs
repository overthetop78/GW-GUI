using System.Buffers;
using GWGUI.Emulation.Services;

namespace GWGUI.Emulation.Epoch.Common.Services;

internal sealed class VideoBufferSet(ArrayPool<byte>? pool = null)
    : EmulationVideoBufferSet(VideoConstants.BufferCount, pool)
{
}
