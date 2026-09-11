namespace GWGUI.MediaEngine.Writing;

/// <summary>Publishes a related set of output files as one recoverable operation.</summary>
internal static class AtomicMediaFileSetWriter
{
    public static async Task WriteAsync(
        IReadOnlyDictionary<string, Func<Stream, CancellationToken, Task>> files,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(files);
        if (files.Count == 0) throw new ArgumentException("At least one output file is required.", nameof(files));

        var outputs = files.Select(pair => CreateOutput(pair.Key, pair.Value)).ToArray();
        if (outputs.Select(output => output.Path).Distinct(StringComparer.OrdinalIgnoreCase).Count() != outputs.Length)
            throw new ArgumentException("Output file paths must be unique.", nameof(files));

        var publicationComplete = false;
        try
        {
            foreach (var output in outputs)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Directory.CreateDirectory(Path.GetDirectoryName(output.Path) ?? Directory.GetCurrentDirectory());
                await using var stream = new FileStream(
                    output.TemporaryPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    64 * 1_024,
                    FileOptions.Asynchronous);
                await output.Write(stream, cancellationToken).ConfigureAwait(false);
            }

            foreach (var output in outputs)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (File.Exists(output.Path)) File.Move(output.Path, output.BackupPath);
                File.Move(output.TemporaryPath, output.Path);
                output.Published = true;
            }
            publicationComplete = true;
        }
        catch
        {
            if (!publicationComplete)
            {
                foreach (var output in outputs.Reverse())
                {
                    if (output.Published && File.Exists(output.Path)) File.Delete(output.Path);
                    if (File.Exists(output.BackupPath)) File.Move(output.BackupPath, output.Path);
                }
            }
            throw;
        }
        finally
        {
            foreach (var output in outputs)
            {
                if (File.Exists(output.TemporaryPath)) File.Delete(output.TemporaryPath);
                if (publicationComplete && File.Exists(output.BackupPath)) File.Delete(output.BackupPath);
            }
        }
    }

    private static PendingOutput CreateOutput(
        string path,
        Func<Stream, CancellationToken, Task> write)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(write);
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory();
        var name = Path.GetFileName(fullPath);
        var transaction = Guid.NewGuid().ToString("N");
        return new PendingOutput(
            fullPath,
            Path.Combine(directory, $".{name}.{transaction}.tmp"),
            Path.Combine(directory, $".{name}.{transaction}.bak"),
            write);
    }

    private sealed class PendingOutput(
        string path,
        string temporaryPath,
        string backupPath,
        Func<Stream, CancellationToken, Task> write)
    {
        public string Path { get; } = path;
        public string TemporaryPath { get; } = temporaryPath;
        public string BackupPath { get; } = backupPath;
        public Func<Stream, CancellationToken, Task> Write { get; } = write;
        public bool Published { get; set; }
    }
}
