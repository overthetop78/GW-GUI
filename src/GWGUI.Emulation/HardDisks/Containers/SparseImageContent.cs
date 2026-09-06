using DiscUtils.Streams;

namespace GWGUI.Emulation.HardDisks.Containers;

internal static class SparseImageContent
{
    internal static void CopyAllocated(SparseMemoryStream source, Stream destination)
    {
        var buffer = new byte[64 * 1024];
        foreach (var extent in source.Extents)
        {
            source.Position = destination.Position = extent.Start;
            var remaining = Math.Min(extent.Length, source.Length - extent.Start);
            while (remaining > 0)
            {
                var count = (int)Math.Min(buffer.Length, remaining);
                source.ReadExactly(buffer.AsSpan(0, count));
                destination.Write(buffer.AsSpan(0, count));
                remaining -= count;
            }
        }
    }

    internal static SparseMemoryStream Create(long capacity, Action<Stream>? initialize)
    {
        var content = new SparseMemoryStream();
        try
        {
            content.SetLength(capacity); initialize?.Invoke(content);
            if (content.Length != capacity) throw new InvalidOperationException("The initializer changed the logical capacity.");
            return content;
        }
        catch { content.Dispose(); throw; }
    }
    internal static SortedSet<long> AllocatedUnits(SparseMemoryStream content, int unitBytes)
    {
        var units = new SortedSet<long>();
        foreach (var extent in content.Extents)
            for (var unit = extent.Start / unitBytes; unit < (Math.Min(content.Length, extent.Start + extent.Length) + unitBytes - 1) / unitBytes; unit++)
                units.Add(unit);
        return units;
    }
    internal static void CopyUnit(Stream content, Stream destination, long logicalOffset, long physicalOffset, byte[] buffer)
    {
        Array.Clear(buffer); content.Position = logicalOffset;
        content.ReadExactly(buffer.AsSpan(0, (int)Math.Min(buffer.Length, content.Length - logicalOffset)));
        destination.Position = physicalOffset; destination.Write(buffer);
    }
}
