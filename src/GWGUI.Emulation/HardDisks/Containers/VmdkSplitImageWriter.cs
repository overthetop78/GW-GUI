using DiscUtils.Vmdk;

namespace GWGUI.Emulation.HardDisks.Containers;

/// <summary>Autonomous split VMDK set. Emits extents first and the descriptor last.</summary>
public static class VmdkSplitImageWriter
{
    public static void Write(long capacity, string baseName, bool sparse, Action<string, Stream> emit,
        Action<Stream>? initialize = null, DiskAdapterType adapter = DiskAdapterType.LsiLogicScsi,
        long extentBytes = 2047L << 20)
        => VmdkDescriptorImageSetWriter.Write(capacity, baseName, sparse, emit, initialize, adapter, extentBytes, true);
}
