using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using GWGUI.App.Constants.Updates;
using GWGUI.App.Services.Emulation;
using GWGUI.Emulation.Constants;
using GWGUI.Updates.Contracts;
using GWGUI.Updates.Services;

namespace GWGUI.App.Services.Updates;

internal sealed class ApplicationUpdateService
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();
    private readonly HttpClient _httpClient;
    private readonly Uri _catalogUri;
    private readonly Func<InstalledUpdateState> _installedState;
    private readonly UpdatePlanBuilder _planBuilder;
    private readonly UpdatePackagePreparationService _packagePreparation;

    internal ApplicationUpdateService(
        HttpClient? httpClient = null,
        Uri? catalogUri = null,
        Func<InstalledUpdateState>? installedState = null,
        UpdatePlanBuilder? planBuilder = null,
        string? applicationDirectory = null,
        UpdateArchiveValidator? archiveValidator = null,
        string? workingRootDirectory = null,
        UpdatePackagePreparationService? packagePreparation = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _catalogUri = catalogUri ?? new Uri(UpdateEndpoints.ApplicationCatalogUrl);
        _installedState = installedState ?? ReadInstalledState;
        _planBuilder = planBuilder ?? new UpdatePlanBuilder();
        _packagePreparation = packagePreparation ?? new UpdatePackagePreparationService(
            _httpClient, _installedState, applicationDirectory, archiveValidator, workingRootDirectory);
    }

    internal Task<PreparedUpdateLaunch> PrepareAsync(UpdatePlan plan,
        IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        if (plan.Scope != UpdateSearchScope.Application || plan.Items.Count != 1
            || plan.Items[0].Kind != UpdateComponentKind.Application)
            throw new InvalidOperationException("ApplicationUpdateService accepts only one GW GUI application update.");
        return _packagePreparation.PrepareAsync(plan, progress, cancellationToken);
    }

    internal async Task<UpdateSearchResult> SearchAsync(
        IReadOnlyDictionary<string, string>? selections = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(_catalogUri, HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var catalog = await JsonSerializer.DeserializeAsync<UpdateCatalog>(content, JsonOptions, cancellationToken)
            .ConfigureAwait(false) ?? throw new InvalidDataException("The update catalog is empty.");
        return _planBuilder.Search(catalog, _installedState(), UpdateSearchScope.Application, selections);
    }

    internal static InstalledUpdateState ReadInstalledState()
    {
        var installedModules = EmulationModuleRegistry.Packages.Select(package => new InstalledModuleVersion(
            package.Manifest.Id,
            package.Manifest.ModuleVersion,
            package.Manifest.HostApiMinimum,
            package.Manifest.HostApiMaximum));

        return new InstalledUpdateState(ReadApplicationVersion(), EmulationHostApi.CurrentVersion.ToString(2),
            installedModules.OrderBy(module => module.Id, StringComparer.OrdinalIgnoreCase).ToArray());
    }

    internal static string ReadApplicationVersion()
    {
        var assembly = typeof(App).Assembly;
        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        var value = informational?.Split('+')[0] ?? assembly.GetName().Version?.ToString(3);
        if (string.IsNullOrWhiteSpace(value) || !Version.TryParse(value, out var version))
            throw new InvalidDataException($"Invalid installed application version '{value}'.");
        return version.ToString(3);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }
}
