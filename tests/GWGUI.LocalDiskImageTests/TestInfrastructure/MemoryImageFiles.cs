using GWGUI.MediaEngine.Interfaces;

namespace GWGUI.Tests.Application.TestInfrastructure;

internal sealed class MemoryImageFiles : IAtomicImageFileWriter
{
    public Dictionary<string, byte[]> Files { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> Calls { get; } = [];
    public bool FailCommit { get; set; }

    public async Task WriteAsync(
        string path,
        Func<Stream, CancellationToken, Task> write,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Calls.Add(path);
        using var output = new MemoryStream();
        await write(output, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        if (FailCommit)
        {
            throw new IOException("synthetic storage failure");
        }

        Files[path] = output.ToArray();
    }
}
