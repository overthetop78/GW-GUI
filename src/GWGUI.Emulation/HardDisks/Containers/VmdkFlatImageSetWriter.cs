using DiscUtils.Vmdk;

namespace GWGUI.Emulation.HardDisks.Containers;

/// <summary>One linear extent plus an external monolithicFlat descriptor.</summary>
public static class VmdkFlatImageSetWriter
{
    public static void Write(long capacity, string baseName, Action<string, Stream> emit,
        Action<Stream>? initialize = null, DiskAdapterType adapter = DiskAdapterType.LsiLogicScsi)
        => VmdkDescriptorImageSetWriter.Write(capacity, baseName, false, emit, initialize, adapter, capacity, false);
}
