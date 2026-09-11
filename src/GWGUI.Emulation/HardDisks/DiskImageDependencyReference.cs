namespace GWGUI.Emulation.HardDisks;

/// <summary>Identifies one required image by a resolved path, an image UUID, or a SHA-1 digest.</summary>
public sealed record DiskImageDependencyReference
{
    private DiskImageDependencyReference(string? path, Guid? uuid, string? sha1)
    {
        Path = path;
        Uuid = uuid;
        Sha1 = sha1;
    }

    public string? Path { get; }

    public Guid? Uuid { get; }

    public string? Sha1 { get; }

    public static DiskImageDependencyReference FromPath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return new(path, null, null);
    }

    public static DiskImageDependencyReference FromUuid(Guid uuid)
    {
        if (uuid == Guid.Empty) throw new ArgumentException("An empty UUID cannot identify a parent image.", nameof(uuid));
        return new(null, uuid, null);
    }

    public static DiskImageDependencyReference FromSha1(ReadOnlySpan<byte> sha1)
    {
        if (sha1.Length != 20 || sha1.ContainsAnyExcept((byte)0) == false)
            throw new ArgumentException("A parent SHA-1 must contain exactly 20 non-zero digest bytes.", nameof(sha1));
        return new(null, null, Convert.ToHexString(sha1));
    }
}
