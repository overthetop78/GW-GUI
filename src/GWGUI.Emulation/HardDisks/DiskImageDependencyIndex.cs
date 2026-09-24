namespace GWGUI.Emulation.HardDisks;

/// <summary>A short-lived dependency snapshot. Rebuild it before confirming a destructive operation.</summary>
public sealed class DiskImageDependencyIndex(
    Func<string, Stream> openRead,
    Func<string, string>? resolveIdentity = null,
    Func<string, DiskImageDependencyReference, string?>? resolveReference = null)
{
    private readonly Dictionary<string, IReadOnlyList<DiskImageDependencyReference>> dependencies =
        new(DiskImageDependencyReader.PathComparer);
    private readonly Dictionary<string, IReadOnlyList<DiskImageDependencyReference>> imageIdentifiers =
        new(DiskImageDependencyReader.PathComparer);
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

    private IReadOnlyList<DiskImageDependencyReference> Identifiers(string path)
    {
        if (!imageIdentifiers.TryGetValue(path, out var identifiers))
        {
            using var stream = openRead(path);
            identifiers = DiskImageDependencyReader.ReadIdentities(stream);
            imageIdentifiers.Add(path, identifiers);
        }
        return identifiers;
    }

    public bool Uses(string rootImage, string candidate)
    {
        var candidatePath = Normalize(candidate);
        var rootPath = Normalize(rootImage);
        var target = Identity(candidatePath);
        IReadOnlyList<DiskImageDependencyReference>? targetIdentifiers = null;
        var active = new HashSet<string>(DiskImageDependencyReader.PathComparer);
        var visited = new HashSet<string>(DiskImageDependencyReader.PathComparer);

        bool MatchesTarget(DiskImageDependencyReference reference)
        {
            if (reference.Path is { } path)
                return DiskImageDependencyReader.PathComparer.Equals(Identity(path), target);
            targetIdentifiers ??= Identifiers(candidatePath);
            return targetIdentifiers.Any(identifier => SameIdentifier(reference, identifier));
        }

        string ResolveReference(string ownerPath, DiskImageDependencyReference reference)
        {
            if (reference.Path is { } path) return path;
            var resolved = resolveReference?.Invoke(ownerPath, reference);
            if (string.IsNullOrWhiteSpace(resolved))
                throw new InvalidDataException("An identifier-based disk image dependency is absent from the image inventory.");
            var resolvedPath = Normalize(resolved);
            if (!Identifiers(resolvedPath).Any(identifier => SameIdentifier(reference, identifier)))
                throw new InvalidDataException("The resolved disk image does not match the dependency identifier.");
            return resolvedPath;
        }

        bool Visit(string path, int depth)
        {
            var identity = Identity(path);
            if (DiskImageDependencyReader.PathComparer.Equals(identity, target)) return true;
            if (!active.Add(identity)) throw new InvalidDataException("The disk image dependency graph contains a cycle.");
            if (!visited.Add(identity))
            {
                active.Remove(identity);
                return false;
            }
            if (depth > 64 || visited.Count > 4096)
                throw new InvalidDataException("The disk image dependency graph exceeds the supported bounds.");
            try
            {
                if (!dependencies.TryGetValue(path, out var children))
                {
                    using var stream = openRead(path);
                    children = DiskImageDependencyReader.ReadReferences(stream, path);
                    dependencies.Add(path, children);
                }
                foreach (var child in children)
                {
                    if (MatchesTarget(child)) return true;
                    if (Visit(ResolveReference(path, child), depth + 1)) return true;
                }
                return false;
            }
            finally
            {
                active.Remove(identity);
            }
        }

        return Visit(rootPath, 0);
    }

    private static string Normalize(string path)
    {
        var fullPath = Path.GetFullPath(path);
        return DiskImageDependencyReader.Resolve(fullPath, fullPath);
    }

    private static bool SameIdentifier(
        DiskImageDependencyReference first,
        DiskImageDependencyReference second) =>
        first.Uuid is { } uuid && second.Uuid == uuid ||
        first.Sha1 is { } sha1 && string.Equals(second.Sha1, sha1, StringComparison.OrdinalIgnoreCase);
}
