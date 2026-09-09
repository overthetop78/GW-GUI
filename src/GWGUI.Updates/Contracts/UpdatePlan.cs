namespace GWGUI.Updates.Contracts;

public enum UpdateSearchScope
{
    Application,
    Modules
}

public enum UpdateAvailability
{
    UpToDate,
    Available,
    ApplicationUpdateRequired,
    Incompatible
}

public sealed record InstalledUpdateState(
    string ApplicationVersion,
    string HostApiVersion,
    IReadOnlyList<InstalledModuleVersion> Modules);

public sealed record InstalledModuleVersion(
    string Id,
    string Version,
    string HostApiMinimum,
    string HostApiMaximum);

public sealed record AvailableComponentUpdate(
    string ComponentId,
    UpdateComponentKind Kind,
    string InstalledVersion,
    UpdateAvailability Availability,
    IReadOnlyList<UpdateCatalogRelease> Releases,
    string? SelectedVersion,
    string? RequiredHostApiVersion = null);

public sealed record UpdatePlanItem(
    string ComponentId,
    UpdateComponentKind Kind,
    string InstalledVersion,
    UpdateCatalogRelease Release);

public sealed record UpdatePlan(
    UpdateSearchScope Scope,
    IReadOnlyList<UpdatePlanItem> Items,
    bool IsCompatible,
    IReadOnlyList<string> Issues);

public sealed record UpdateSearchResult(
    UpdateSearchScope Scope,
    IReadOnlyList<AvailableComponentUpdate> Components,
    UpdatePlan Plan);
