namespace GWGUI.Emulation.HardDisks;

internal sealed class FileImageSetStorage : IImageSetStorage
{
    public IImageSetTransaction Begin(string targetDirectory)
    {
        if (Directory.Exists(targetDirectory) || File.Exists(targetDirectory))
            throw new IOException("The image set destination already exists.");
        var parent = Path.GetDirectoryName(targetDirectory)!;
        if (!Directory.Exists(parent)) throw new DirectoryNotFoundException("The image set parent directory does not exist.");
        var staging = Path.Combine(parent, ".gwgui-" + Guid.NewGuid().ToString("N") + ".creating");
        if (Directory.Exists(staging) || File.Exists(staging)) throw new IOException("The staging name already exists.");
        Directory.CreateDirectory(staging);
        return new Transaction(staging, targetDirectory);
    }

    private sealed class Transaction(string staging, string target) : IImageSetTransaction
    {
        private readonly List<string> ownedFiles = [];
        private readonly List<string> ownedDirectories = [];
        private bool committed;
        private bool disposed;

        public void WriteNew(string name, Stream source)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (committed) throw new InvalidOperationException("The image set is already published.");
            DiskImageSetPublication.ValidateMemberPath(name);
            RejectRedirectedStaging();
            var parts = name.Split('/');
            var parent = staging;
            foreach (var part in parts.SkipLast(1))
            {
                parent = Path.Combine(parent, part);
                if (!ownedDirectories.Contains(parent, StringComparer.OrdinalIgnoreCase))
                {
                    if (Directory.Exists(parent) || File.Exists(parent))
                        throw new IOException("An unexpected staging member occupies a required directory.");
                    Directory.CreateDirectory(parent);
                    ownedDirectories.Add(parent);
                }
                RejectRedirectedDirectory(parent);
            }
            var path = Path.Combine(parent, parts[^1]);
            using var destination = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            ownedFiles.Add(path);
            if (source.CanSeek) source.Position = 0;
            source.CopyTo(destination);
            destination.Flush(flushToDisk: true);
        }

        public void Commit()
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (committed) throw new InvalidOperationException("The image set is already published.");
            RejectRedirectedStaging();
            // Both paths share a parent, so publication is a same-volume directory rename.
            Directory.Move(staging, target);
            committed = true;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            if (committed || !Directory.Exists(staging)) return;
            RejectRedirectedStaging();
            List<Exception> errors = [];
            foreach (var path in ownedFiles)
            {
                try { File.Delete(path); }
                catch (Exception error) { errors.Add(error); }
            }
            foreach (var path in ownedDirectories.AsEnumerable().Reverse())
            {
                try { Directory.Delete(path, recursive: false); }
                catch (Exception error) { errors.Add(error); }
            }
            // Never recursively remove an unexpected file introduced into the staging directory.
            try { Directory.Delete(staging, recursive: false); }
            catch (Exception error) { errors.Add(error); }
            if (errors.Count != 0) throw new AggregateException("Some staging members could not be removed.", errors);
        }

        private void RejectRedirectedStaging()
        {
            RejectRedirectedDirectory(staging);
            foreach (var directory in ownedDirectories) RejectRedirectedDirectory(directory);
        }

        private static void RejectRedirectedDirectory(string path)
        {
            if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("The image set staging directory was redirected.");
        }
    }
}
