using System.IO;
using GWGUI.Updates.Contracts;

namespace GWGUI.Updates.Services;

public sealed class UpdatePlanBuilder
{
    public const int SupportedSchemaVersion = 1;
    public const string ApplicationId = "gwgui";

    public UpdateSearchResult Search(
        UpdateCatalog catalog,
        InstalledUpdateState installed,
        UpdateSearchScope scope,
        IReadOnlyDictionary<string, string>? selections = null)
    {
        Validate(catalog);
        var installedApplicationVersion = ParseVersion(installed.ApplicationVersion, 3, "installed application version");
        var installedHostApi = ParseVersion(installed.HostApiVersion, 2, "installed host API version");
        var installedModules = installed.Modules.ToDictionary(module => module.Id, StringComparer.OrdinalIgnoreCase);
        foreach (var module in installedModules.Values)
        {
            ValidateId(module.Id, "installed module id");
            _ = ParseVersion(module.Version, 3, $"installed module '{module.Id}' version");
            var minimum = ParseVersion(module.HostApiMinimum, 2, $"installed module '{module.Id}' minimum API");
            var maximum = ParseVersion(module.HostApiMaximum, 2, $"installed module '{module.Id}' maximum API");
            if (minimum > maximum) throw new InvalidDataException($"Installed module '{module.Id}' has reversed API bounds.");
        }

        var componentsById = catalog.Components.ToDictionary(component => component.Id, StringComparer.OrdinalIgnoreCase);
        var moduleCatalogs = componentsById.Values
            .Where(component => component.Kind == UpdateComponentKind.Module)
            .ToDictionary(component => component.Id, StringComparer.OrdinalIgnoreCase);
        var issues = new List<string>();
        var rows = new List<AvailableComponentUpdate>();
        UpdateCatalogRelease? selectedApplication = null;
        var targetHostApi = installedHostApi;

        if (scope is UpdateSearchScope.Application or UpdateSearchScope.All)
        {
            if (componentsById.TryGetValue(ApplicationId, out var application))
            {
                var newer = NewerReleases(application, installedApplicationVersion);
                var feasible = newer.Where(release => ApplicationReleaseIsFeasible(
                    release, installedModules, moduleCatalogs, scope == UpdateSearchScope.All)).ToArray();
                selectedApplication = Select(application.Id, feasible, selections, issues);
                if (selectedApplication is not null)
                    targetHostApi = ParseVersion(selectedApplication.HostApiVersion, 2,
                        $"application {selectedApplication.Version} host API");
                rows.Add(new(application.Id, application.Kind, installed.ApplicationVersion,
                    newer.Count == 0 ? UpdateAvailability.UpToDate
                    : feasible.Length == 0 ? UpdateAvailability.Incompatible : UpdateAvailability.Available,
                    feasible, selectedApplication?.Version));
            }
            else issues.Add("The update catalog does not contain the application component.");
        }

        var moduleSelections = new Dictionary<string, UpdateCatalogRelease>(StringComparer.OrdinalIgnoreCase);
        if (scope is UpdateSearchScope.Modules or UpdateSearchScope.All)
        {
            foreach (var installedModule in installed.Modules.OrderBy(module => module.Id, StringComparer.OrdinalIgnoreCase))
            {
                if (!moduleCatalogs.TryGetValue(installedModule.Id, out var component))
                {
                    issues.Add($"The update catalog does not contain installed module '{installedModule.Id}'.");
                    continue;
                }
                var installedVersion = ParseVersion(installedModule.Version, 3, $"installed module '{installedModule.Id}' version");
                var newer = NewerReleases(component, installedVersion);
                var compatible = newer.Where(release => AcceptsHostApi(release, targetHostApi)).ToArray();
                var selected = Select(component.Id, compatible, selections, issues);
                var availability = newer.Count == 0 ? UpdateAvailability.UpToDate
                    : compatible.Length > 0 ? UpdateAvailability.Available
                    : newer.Any(release => ParseVersion(release.HostApiMinimum, 2,
                        $"module '{component.Id}' minimum API") > targetHostApi)
                        ? UpdateAvailability.ApplicationUpdateRequired : UpdateAvailability.Incompatible;
                var requiredApi = availability == UpdateAvailability.ApplicationUpdateRequired
                    ? newer.Select(release => ParseVersion(release.HostApiMinimum, 2,
                            $"module '{component.Id}' minimum API"))
                        .Min()?.ToString(2)
                    : null;
                rows.Add(new(component.Id, component.Kind, installedModule.Version, availability,
                    compatible.Length > 0 ? compatible : newer, selected?.Version, requiredApi));
                if (selected is not null) moduleSelections[component.Id] = selected;
            }
        }

        var items = new List<UpdatePlanItem>();
        if (selectedApplication is not null)
            items.Add(new(ApplicationId, UpdateComponentKind.Application, installed.ApplicationVersion, selectedApplication));
        foreach (var pair in moduleSelections.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
            items.Add(new(pair.Key, UpdateComponentKind.Module, installedModules[pair.Key].Version, pair.Value));

        foreach (var installedModule in installed.Modules)
        {
            var release = moduleSelections.GetValueOrDefault(installedModule.Id);
            var minimum = ParseVersion(release?.HostApiMinimum ?? installedModule.HostApiMinimum, 2,
                $"module '{installedModule.Id}' minimum API");
            var maximum = ParseVersion(release?.HostApiMaximum ?? installedModule.HostApiMaximum, 2,
                $"module '{installedModule.Id}' maximum API");
            if (targetHostApi < minimum || targetHostApi > maximum)
                issues.Add($"Module '{installedModule.Id}' does not support target host API {targetHostApi.ToString(2)}.");
        }

        var plan = new UpdatePlan(scope, items, issues.Count == 0, issues);
        return new(scope, rows, plan);
    }

    private static IReadOnlyList<UpdateCatalogRelease> NewerReleases(
        UpdateCatalogComponent component, Version installedVersion) => component.Releases
        .Where(release => ParseVersion(release.Version, 3,
            $"component '{component.Id}' release version") > installedVersion)
        .OrderByDescending(release => ParseVersion(release.Version, 3,
            $"component '{component.Id}' release version"))
        .ToArray();

    private static UpdateCatalogRelease? Select(
        string componentId,
        IReadOnlyList<UpdateCatalogRelease> releases,
        IReadOnlyDictionary<string, string>? selections,
        ICollection<string> issues)
    {
        if (releases.Count == 0) return null;
        if (selections is null || !selections.TryGetValue(componentId, out var requested)) return releases[0];
        var selected = releases.FirstOrDefault(release => string.Equals(release.Version, requested, StringComparison.Ordinal));
        if (selected is null) issues.Add($"Selected version '{requested}' is unavailable for component '{componentId}'.");
        return selected;
    }

    private static bool ApplicationReleaseIsFeasible(
        UpdateCatalogRelease release,
        IReadOnlyDictionary<string, InstalledModuleVersion> installedModules,
        IReadOnlyDictionary<string, UpdateCatalogComponent> moduleCatalogs,
        bool mayUpdateModules)
    {
        var hostApi = ParseVersion(release.HostApiVersion, 2, $"application {release.Version} host API");
        foreach (var module in installedModules.Values)
        {
            if (AcceptsHostApi(module.HostApiMinimum, module.HostApiMaximum, hostApi)) continue;
            if (!mayUpdateModules || !moduleCatalogs.TryGetValue(module.Id, out var component)) return false;
            var installedVersion = ParseVersion(module.Version, 3, $"installed module '{module.Id}' version");
            if (!component.Releases.Any(candidate =>
                    ParseVersion(candidate.Version, 3, $"module '{module.Id}' release version") > installedVersion
                    && AcceptsHostApi(candidate, hostApi))) return false;
        }
        return true;
    }

    private static bool AcceptsHostApi(UpdateCatalogRelease release, Version hostApi) =>
        AcceptsHostApi(release.HostApiMinimum, release.HostApiMaximum, hostApi);

    private static bool AcceptsHostApi(string? minimumValue, string? maximumValue, Version hostApi)
    {
        var minimum = ParseVersion(minimumValue, 2, "module minimum API");
        var maximum = ParseVersion(maximumValue, 2, "module maximum API");
        return hostApi >= minimum && hostApi <= maximum;
    }

    private static void Validate(UpdateCatalog catalog)
    {
        if (catalog.SchemaVersion != SupportedSchemaVersion)
            throw new InvalidDataException($"Unsupported update catalog schema '{catalog.SchemaVersion}'.");
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var component in catalog.Components)
        {
            ValidateId(component.Id, "component id");
            if (!ids.Add(component.Id)) throw new InvalidDataException($"Duplicate update component '{component.Id}'.");
            if (component.Kind == UpdateComponentKind.Application
                && !string.Equals(component.Id, ApplicationId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("The application component id must be 'gwgui'.");
            var versions = new HashSet<string>(StringComparer.Ordinal);
            foreach (var release in component.Releases)
            {
                _ = ParseVersion(release.Version, 3, $"component '{component.Id}' release version");
                if (!versions.Add(release.Version))
                    throw new InvalidDataException($"Duplicate version '{release.Version}' for component '{component.Id}'.");
                ValidateUri(release.PackageUrl, "packageUrl");
                ValidateUri(release.NotesUrl, "notesUrl");
                if (release.Sha256.Length != 64 || !release.Sha256.All(char.IsAsciiHexDigit))
                    throw new InvalidDataException($"Invalid SHA-256 for component '{component.Id}' version {release.Version}.");
                if (component.Kind == UpdateComponentKind.Application)
                    _ = ParseVersion(release.HostApiVersion, 2, $"application {release.Version} host API");
                else
                {
                    var minimum = ParseVersion(release.HostApiMinimum, 2, $"module '{component.Id}' minimum API");
                    var maximum = ParseVersion(release.HostApiMaximum, 2, $"module '{component.Id}' maximum API");
                    if (minimum > maximum)
                        throw new InvalidDataException($"Module '{component.Id}' version {release.Version} has reversed API bounds.");
                }
            }
        }
    }

    private static void ValidateId(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value) || value != value.Trim()
            || !value.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_'))
            throw new InvalidDataException($"Invalid {field} '{value}'.");
    }

    private static void ValidateUri(string value, string field)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            throw new InvalidDataException($"Update catalog field '{field}' must be an absolute HTTPS URL.");
    }

    private static Version ParseVersion(string? value, int components, string field)
    {
        if (string.IsNullOrEmpty(value) || value.Split('.').Length != components
            || value.Split('.').Any(part => part.Length == 0 || !part.All(char.IsAsciiDigit))
            || !Version.TryParse(value, out var version))
            throw new InvalidDataException($"Invalid {field} '{value}'.");
        return version;
    }
}
