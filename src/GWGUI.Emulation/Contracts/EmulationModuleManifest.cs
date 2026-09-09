namespace GWGUI.Emulation.Contracts;

public sealed record EmulationModuleManifest(
    int SchemaVersion,
    string Id,
    string EntryAssembly,
    string ModuleVersion,
    string HostApiMinimum,
    string HostApiMaximum,
    string UpdateCatalogUrl);
