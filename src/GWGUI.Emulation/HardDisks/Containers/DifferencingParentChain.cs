using DiscUtils;
using DiscUtils.Streams;

namespace GWGUI.Emulation.HardDisks.Containers;

/// <summary>Resolves an explicitly supplied chain; no parent is opened by filename.</summary>
internal sealed class DifferencingParentChain<T> : IDisposable where T : VirtualDiskLayer
{
    private readonly List<DifferencingImageStreams.ReadOnlyParent> inputs = [];
    private readonly List<T> layers = [];
    private readonly List<SparseStream> contents = [];
    internal T ImmediateParent => layers[0];
    internal SparseStream Content => contents[^1];

    internal DifferencingParentChain(IReadOnlyList<Stream> images, Func<Stream, T> open,
        Func<T, Guid> uniqueId, Func<T, Guid> parentId, Action<long> validateCapacity)
    {
        if (images.Count is < 1 or > 64 || images.Distinct(ReferenceEqualityComparer.Instance).Count() != images.Count)
            throw new ArgumentException("A parent chain requires one to 64 distinct streams.", nameof(images));
        try
        {
            foreach (var image in images)
            {
                if (!image.CanRead || !image.CanSeek) throw new ArgumentException("Parent images must be readable and seekable.");
                var input = new DifferencingImageStreams.ReadOnlyParent(image); inputs.Add(input);
                var layer = open(input); layers.Add(layer); validateCapacity(layer.Capacity);
            }
            if (layers[^1].NeedsParent) throw new NotSupportedException("The chain must end in an autonomous base image.");
            for (var index = 0; index < layers.Count - 1; index++)
            {
                if (!layers[index].NeedsParent) throw new ArgumentException("The chain contains an unused ancestor.");
                if (parentId(layers[index]) != uniqueId(layers[index + 1]) || layers[index].Capacity != layers[index + 1].Capacity ||
                    layers[index].Geometry.BytesPerSector != layers[index + 1].Geometry.BytesPerSector)
                    throw new InvalidDataException("The ancestor identity or capacity does not match the child's parent reference.");
            }
            SparseStream? inherited = null;
            for (var index = layers.Count - 1; index >= 0; index--)
            {
                inherited = layers[index].OpenContent(inherited!, Ownership.None);
                contents.Add(inherited);
            }
        }
        catch { Dispose(); throw; }
    }

    public void Dispose()
    {
        for (var index = contents.Count - 1; index >= 0; index--) contents[index].Dispose();
        for (var index = layers.Count - 1; index >= 0; index--) layers[index].Dispose();
        for (var index = inputs.Count - 1; index >= 0; index--) inputs[index].Dispose();
        contents.Clear(); layers.Clear(); inputs.Clear();
    }
}
