using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using GWGUI.App.Services.Emulation;
using GWGUI.Emulation.Constants;
using GWGUI.Updates.Contracts;
using GWGUI.Updates.Services;
using GWGUI.App.Contracts.Updates;

namespace GWGUI.App.Services.Updates;

internal sealed class ModuleInstallationService
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();
    private readonly HttpClient _httpClient;
    private readonly UpdatePackagePreparationService _packagePreparation;
    private readonly ModuleUpdateCatalogValidator _catalogValidator;
    private readonly Func<IReadOnlyList<LoadedEmulationModule>> _installedModules;
    private readonly Func<string> _hostApiVersion;
    private readonly Func<string, bool> _isPending;

    internal ModuleInstallationService(
        HttpClient? httpClient = null,
        UpdatePackagePreparationService? packagePreparation = null,
        ModuleUpdateCatalogValidator? catalogValidator = null,
        Func<IReadOnlyList<LoadedEmulationModule>>? installedModules = null,
        Func<string>? hostApiVersion = null,
        Func<string, bool>? isPending = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _packagePreparation = packagePreparation ?? new UpdatePackagePreparationService(_httpClient);
        _catalogValidator = catalogValidator ?? new ModuleUpdateCatalogValidator();
        _installedModules = installedModules ?? (() => EmulationModuleRegistry.Packages);
        _hostApiVersion = hostApiVersion ?? (() => EmulationHostApi.CurrentVersion.ToString(2));
        _isPending = isPending ?? (_ => false);
    }

    internal async Task<PendingModuleInstallation> PrepareFromFileAsync(string archivePath,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        var prepared = await _packagePreparation.PrepareLocalModuleAsync(archivePath, displayName, cancellationToken)
            .ConfigureAwait(false);
        EnsureNotInstalled(prepared.ModuleId);
        return prepared;
    }

    internal async Task<PendingModuleInstallation> PrepareFromCatalogAsync(string catalogUrl,
        string displayName,
        IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        var catalogUri = ValidateDirectCatalogUri(catalogUrl);
        using var response = await _httpClient.GetAsync(catalogUri, HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var catalog = await JsonSerializer.DeserializeAsync<UpdateCatalog>(content, JsonOptions, cancellationToken)
            .ConfigureAwait(false) ?? throw new InvalidDataException("The module update catalog is empty.");
        var component = _catalogValidator.Validate(catalog);
        EnsureNotInstalled(component.Id);

        var hostApi = Version.Parse(_hostApiVersion());
        var release = component.Releases
            .Where(candidate => Version.Parse(candidate.HostApiMinimum!) <= hostApi
                && Version.Parse(candidate.HostApiMaximum!) >= hostApi)
            .OrderByDescending(candidate => Version.Parse(candidate.Version))
            .FirstOrDefault()
            ?? throw new InvalidDataException(
                $"Module '{component.Id}' has no release compatible with host API {hostApi.ToString(2)}.");
        var plan = new UpdatePlan(UpdateSearchScope.Modules,
            [new UpdatePlanItem(component.Id, UpdateComponentKind.Module, "0.0.0", release)], true, []);
        return await _packagePreparation.PrepareRemoteModuleAsync(plan, displayName,
            catalogUri.AbsoluteUri, progress, cancellationToken).ConfigureAwait(false);
    }

    private void EnsureNotInstalled(string moduleId)
    {
        if (_isPending(moduleId))
            throw new InvalidOperationException($"Module '{moduleId}' is already downloaded.");
        if (_installedModules().Any(module =>
                module.Manifest.Id.Equals(moduleId, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException(
                $"Module '{moduleId}' is already installed. Use the module update action instead.");
    }

    private static Uri ValidateDirectCatalogUri(string value)
    {
        if (!Uri.TryCreate(value?.Trim(), UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            throw new ArgumentException("The module catalog address must be an absolute HTTPS URL.", nameof(value));
        if (!uri.AbsolutePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException(
                "Enter a direct machine-readable module catalog URL ending in .json, not a repository or web page.",
                nameof(value));
        return uri;
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }
}
