namespace GWGUI.Emulation.HardDisks;

[Flags]
public enum DiskFormatOperations { None = 0, Create = 1, Read = 2, Modify = 4, Convert = 8 }

/// <summary>Describes one implemented profile. Extensions are suggestions, never proof of its contents.</summary>
public sealed class DiskFormatIdentity
{
    public string Family { get; }
    public string Variant { get; }
    public string? SignatureFamily { get; }
    public IReadOnlyList<string> Extensions { get; }

    public DiskFormatIdentity(string family, string variant, IEnumerable<string>? extensions = null,
        string? signatureFamily = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(family);
        ArgumentException.ThrowIfNullOrWhiteSpace(variant);
        var suffixes = (extensions ?? []).Select(value => value.ToLowerInvariant()).Distinct().ToArray();
        if (suffixes.Any(value => value.Length < 2 || value[0] != '.' ||
            value.Skip(1).Any(c => !char.IsAsciiLetterOrDigit(c) && c is not ('-' or '_'))))
            throw new ArgumentException("Expected filename extensions beginning with one dot.", nameof(extensions));
        Family = family;
        Variant = variant;
        SignatureFamily = signatureFamily;
        Extensions = Array.AsReadOnly(suffixes);
    }
}
