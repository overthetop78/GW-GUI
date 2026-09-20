using IMediaRandomAccessData = global::GWGUI.MediaFileSystems.Interfaces.IMediaRandomAccessData;

namespace GWGUI.MediaEngine.Reading.Sources;

/// <summary>Reads bounded ranges from a file without retaining the complete media image in memory.</summary>
public sealed class FileRandomAccessData : IMediaRandomAccessData
{
    public FileRandomAccessData(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Path = System.IO.Path.GetFullPath(path);
        Length = new FileInfo(Path).Length;
    }

    public string Path { get; }

    public long Length { get; }

    public async ValueTask ReadExactlyAsync(
        long offset,
        Memory<byte> destination,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        if (offset > Length || destination.Length > Length - offset)
            throw new ArgumentOutOfRangeException(nameof(destination), "The requested range exceeds the file data source.");
        if (destination.IsEmpty) return;

        await using var stream = new FileStream(
            Path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            64 * 1024,
            FileOptions.Asynchronous | FileOptions.RandomAccess);
        stream.Seek(offset, SeekOrigin.Begin);

        var totalRead = 0;
        while (totalRead < destination.Length)
        {
            var read = await stream.ReadAsync(destination[totalRead..], cancellationToken).ConfigureAwait(false);
            if (read == 0)
                throw new EndOfStreamException($"The media source ended at byte {offset + totalRead}.");
            totalRead += read;
        }
    }
}
