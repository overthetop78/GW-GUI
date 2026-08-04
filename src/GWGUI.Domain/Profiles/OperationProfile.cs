namespace GWGUI.Domain.Profiles;

public enum OperationKind { Read, Write, Convert }

public sealed record OperationProfile(
    string Id,
    OperationKind Operation,
    string Name,
    IReadOnlyDictionary<string, string> Values,
    IReadOnlySet<string> EnabledOptions,
    bool IsSystem = false)
{
    public static OperationProfile Default(OperationKind operation) =>
        new($"default-{operation.ToString().ToLowerInvariant()}", operation, "Default", new Dictionary<string, string>(), new HashSet<string>(), true);
}
