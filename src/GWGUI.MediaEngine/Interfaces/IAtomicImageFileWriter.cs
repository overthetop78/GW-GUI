using System.IO;
using GWGUI.MediaEngine.Images.Writing;

namespace GWGUI.MediaEngine.Interfaces;

/// <summary>Publie une image seulement lorsque son écriture complète a réussi.</summary>
public interface IAtomicImageFileWriter
{
    /// <summary>Fournit un flux de sortie isolé et conserve la destination précédente en cas d'échec.</summary>
    Task WriteAsync(string path, Func<Stream, CancellationToken, Task> write, CancellationToken cancellationToken = default);
}

internal sealed class AtomicImageFileWriter : IAtomicImageFileWriter
{
    public async Task WriteAsync(string path, Func<Stream, CancellationToken, Task> write, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullPath = Path.GetFullPath(path);
        await AtomicFileWriter.WriteTemporaryAsync(fullPath, async temporaryPath =>
        {
            await using (var output = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous))
                await write(output, cancellationToken).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);
    }
}
