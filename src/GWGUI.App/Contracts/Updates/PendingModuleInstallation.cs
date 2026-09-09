using GWGUI.Updates.Contracts;

namespace GWGUI.App.Contracts.Updates;

internal sealed record PendingModuleInstallation(
    string ModuleId,
    string DisplayName,
    string ModuleVersion,
    string UpdateCatalogUrl,
    PreparedUpdateComponent Component,
    string WorkingDirectory);
