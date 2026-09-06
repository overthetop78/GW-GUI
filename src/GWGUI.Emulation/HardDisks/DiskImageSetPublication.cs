namespace GWGUI.Emulation.HardDisks;

/// <summary>Publishes a complete image set by renaming a sibling staging directory without replacing an existing target.</summary>
public static class DiskImageSetPublication
{
    public static string Create(string directory, string entryPoint, Action<Action<string, Stream>> build)
        => Create(directory, entryPoint, build, new FileImageSetStorage());

    /// <summary>Publishes an image whose members may occupy relative subdirectories.</summary>
    public static string CreateTree(string directory, string entryPoint, Action<Action<string, Stream>> build)
        => Create(directory, entryPoint, build, new FileImageSetStorage(), true);

    internal static string Create(string directory, string entryPoint, Action<Action<string, Stream>> build,
        IImageSetStorage storage, bool allowSubdirectories = false)
    {
        ArgumentNullException.ThrowIfNull(build);
        ValidateMemberPath(entryPoint, allowSubdirectories);
        var target = Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory));
        if (string.Equals(target, Path.TrimEndingDirectorySeparator(Path.GetPathRoot(target)!), StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The image set must have its own directory.", nameof(directory));
        var transaction = storage.Begin(target);
        var members = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var active = true;
        Exception? emissionFailure = null;
        try
        {
            try
            {
                build((name, source) =>
                {
                    if (!active || emissionFailure is not null) throw new InvalidOperationException("Image set emission is closed.");
                    try
                    {
                        ValidateMemberPath(name, allowSubdirectories);
                        if (members.Any(existing => existing.StartsWith(name + "/", StringComparison.OrdinalIgnoreCase) ||
                            name.StartsWith(existing + "/", StringComparison.OrdinalIgnoreCase)))
                            throw new ArgumentException("An image set member conflicts with a directory.", nameof(name));
                        if (!members.Add(name)) throw new ArgumentException("Duplicate image set member.", nameof(name));
                        if (!source.CanRead) throw new ArgumentException("Image set members require a readable stream.");
                        transaction.WriteNew(name, source);
                    }
                    catch (Exception error) { emissionFailure = error; throw; }
                });
            }
            finally { active = false; }
            if (emissionFailure is not null) throw new IOException("An image set member failed to write.", emissionFailure);
            if (!members.Contains(entryPoint)) throw new InvalidDataException("The image set entry point was not produced.");
            transaction.Commit();
        }
        catch (Exception failure)
        {
            try { transaction.Dispose(); }
            catch (Exception cleanupFailure) { throw new AggregateException("Image set creation and cleanup failed.", failure, cleanupFailure); }
            throw;
        }
        transaction.Dispose();
        return Path.Combine(target, members.Single(name => name.Equals(entryPoint, StringComparison.OrdinalIgnoreCase)));
    }

    internal static void ValidateMemberName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.Length > 200 || name is "." or ".." || name[^1] is '.' or ' ' ||
            name.Any(c => c < 32 || "<>:\"/\\|?*".Contains(c)))
            throw new ArgumentException("An image set member must be a simple portable filename.", nameof(name));
        var stem = name.Split('.')[0].ToUpperInvariant();
        if (stem is "CON" or "PRN" or "AUX" or "NUL" ||
            stem.Length == 4 && (stem.StartsWith("COM", StringComparison.Ordinal) || stem.StartsWith("LPT", StringComparison.Ordinal)) &&
            (stem[3] is >= '1' and <= '9' or '¹' or '²' or '³'))
            throw new ArgumentException("A reserved device name cannot identify an image set member.", nameof(name));
    }

    internal static void ValidateMemberPath(string name, bool allowSubdirectories = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (!allowSubdirectories) { ValidateMemberName(name); return; }
        var parts = name.Split('/');
        if (name.Length > 400 || parts.Length > 8)
            throw new ArgumentException("The image member path exceeds the supported depth or length.", nameof(name));
        foreach (var part in parts) ValidateMemberName(part);
    }
}

internal interface IImageSetStorage
{
    IImageSetTransaction Begin(string targetDirectory);
}

internal interface IImageSetTransaction : IDisposable
{
    void WriteNew(string name, Stream source);
    void Commit();
}
