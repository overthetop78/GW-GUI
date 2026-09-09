namespace GWGUI.Updates.Contracts;

public enum UpdateComponentKind
{
    Application,
    Module
}

public sealed record UpdateCatalog(
    int SchemaVersion,
    DateTimeOffset GeneratedAtUtc,
    IReadOnlyList<UpdateCatalogComponent> Components);

public sealed record UpdateCatalogComponent(
    string Id,
    UpdateComponentKind Kind,
    IReadOnlyList<UpdateCatalogRelease> Releases);

public sealed record UpdateCatalogRelease(
    string Version,
    string PackageUrl,
    string Sha256,
    string NotesUrl,
    string? HostApiVersion = null,
    string? HostApiMinimum = null,
    string? HostApiMaximum = null);
