using DiscUtils.Streams;
using DiscUtils.Vhd;

namespace GWGUI.Emulation.HardDisks.Containers;

/// <summary>Creates a VHD child without modifying its explicitly supplied parent chain.</summary>
public static class VhdDifferencingImageWriter
{
    public static void Write(Stream destination, Stream parentImage, string parentAbsolutePath, string parentRelativePath,
        DateTime parentModificationTimeUtc, Action<Stream>? initialize = null)
        => WriteChain(destination, [parentImage], parentAbsolutePath, parentRelativePath, parentModificationTimeUtc, initialize);

    /// <summary>Parents are ordered from immediate parent to autonomous base; all must have matching identities and capacity.</summary>
    public static void WriteChain(Stream destination, IReadOnlyList<Stream> parentImages, string parentAbsolutePath, string parentRelativePath,
        DateTime parentModificationTimeUtc, Action<Stream>? initialize = null)
    {
        ArgumentNullException.ThrowIfNull(parentImages);
        var images = parentImages.ToArray();
        foreach (var image in images) DifferencingImageStreams.Validate(destination, image, parentAbsolutePath, parentRelativePath, parentModificationTimeUtc);
        using var chain = new DifferencingParentChain<DiskImageFile>(images, stream => new(stream, Ownership.None),
            layer => layer.UniqueId, layer => layer.ParentUniqueId, VhdImageWriter.Validate);
        var parent = chain.ImmediateParent;
        using var staged = new SparseMemoryStream();
        VhdDifferencingHeaderWriter.Write(staged, parent, parentAbsolutePath, parentRelativePath, parentModificationTimeUtc);
        using (var child = new DiskImageFile(staged, Ownership.None))
        using (var content = child.OpenContent(chain.Content, Ownership.None))
        {
            initialize?.Invoke(content);
            if (content.Length != parent.Capacity) throw new InvalidOperationException("The child capacity was changed.");
            content.Flush();
        }
        DifferencingImageStreams.Publish(staged, destination);
    }
}
