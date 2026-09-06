namespace GWGUI.Emulation.HardDisks;

/// <summary>A short-lived dependency snapshot. Rebuild it before confirming a destructive operation.</summary>
public sealed class DiskImageDependencyIndex(Func<string, Stream> openRead, Func<string, string>? resolveIdentity = null)
{
    private readonly Dictionary<string, IReadOnlyList<string>> dependencies = new(DiskImageDependencyReader.PathComparer);
    private readonly Dictionary<string, string> identities = new(DiskImageDependencyReader.PathComparer);

    private string Identity(string path)
    {
        if (!identities.TryGetValue(path, out var identity))
        {
            identity = resolveIdentity is null ? path : Path.GetFullPath(resolveIdentity(path));
            identities.Add(path, identity);
        }
        return identity;
    }

    public bool Uses(string rootImage, string candidate)
    {
        var candidatePath = Path.GetFullPath(candidate);
        var rootPath = Path.GetFullPath(rootImage);
        candidatePath = DiskImageDependencyReader.Resolve(candidatePath, candidatePath);
        rootPath = DiskImageDependencyReader.Resolve(rootPath, rootPath);
        var target = Identity(candidatePath);
        var active = new HashSet<string>(DiskImageDependencyReader.PathComparer);
        var visited = new HashSet<string>(DiskImageDependencyReader.PathComparer);
        bool Visit(string path, int depth)
        {
            var identity = Identity(path);
            if (DiskImageDependencyReader.PathComparer.Equals(identity, target)) return true;
            if (active.Contains(identity)) throw new InvalidDataException("The disk image dependency graph contains a cycle.");
            if (!visited.Add(path)) return false;
            if (depth > 64 || visited.Count > 4096) throw new InvalidDataException("The disk image dependency graph exceeds the supported bounds.");
            if (!dependencies.TryGetValue(path, out var children))
            {
                using var stream = openRead(path);
                children = DiskImageDependencyReader.Read(stream, path);
                dependencies.Add(path, children);
            }
            if (children.Contains(target, DiskImageDependencyReader.PathComparer)) return true;
            active.Add(identity);
            try { return children.Any(child => Visit(child, depth + 1)); }
            finally { active.Remove(identity); }
        }
        return Visit(rootPath, 0);
    }
}
