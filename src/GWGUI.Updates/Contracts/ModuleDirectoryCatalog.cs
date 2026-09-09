namespace GWGUI.Updates.Contracts;

public sealed record ModuleDirectoryCatalog(
    int SchemaVersion,
    DateTimeOffset GeneratedAtUtc,
    IReadOnlyList<ModuleDirectoryEntry> Modules);

public sealed record ModuleDirectoryEntry(
    string Id,
    string DisplayName,
    string CatalogUrl);
