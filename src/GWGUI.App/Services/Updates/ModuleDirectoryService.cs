using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using GWGUI.App.Constants.Updates;
using GWGUI.App.Services.Emulation;
using GWGUI.Emulation.Constants;
using GWGUI.Updates.Contracts;
using GWGUI.Updates.Services;

namespace GWGUI.App.Services.Updates;

internal sealed class ModuleDirectoryService
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();
    private readonly HttpClient _httpClient;
    private readonly Uri _directoryUri;
    private readonly ModuleDirectoryCatalogValidator _directoryValidator;
    private readonly ModuleUpdateCatalogValidator _catalogValidator;
    private readonly Func<IReadOnlyDictionary<string, string>> _installedVersions;
    private readonly Func<string> _hostApiVersion;

    internal ModuleDirectoryService(
        HttpClient? httpClient = null,
        Uri? directoryUri = null,
        ModuleDirectoryCatalogValidator? directoryValidator = null,
        ModuleUpdateCatalogValidator? catalogValidator = null,
        Func<IReadOnlyDictionary<string, string>>? installedVersions = null,
        Func<string>? hostApiVersion = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _directoryUri = directoryUri ?? new Uri(UpdateEndpoints.ModuleDirectoryUrl);
        _directoryValidator = directoryValidator ?? new ModuleDirectoryCatalogValidator();
        _catalogValidator = catalogValidator ?? new ModuleUpdateCatalogValidator();
        _installedVersions = installedVersions ?? GetInstalledVersions;
        _hostApiVersion = hostApiVersion ?? (() => EmulationHostApi.CurrentVersion.ToString(2));
    }

    internal async Task<IReadOnlyList<AvailableModuleDirectoryItem>> SearchAsync(
        CancellationToken cancellationToken = default)
    {
        using var directoryResponse = await _httpClient.GetAsync(_directoryUri,
            HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        directoryResponse.EnsureSuccessStatusCode();
        await using var directoryContent = await directoryResponse.Content
            .ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var directory = await JsonSerializer.DeserializeAsync<ModuleDirectoryCatalog>(
            directoryContent, JsonOptions, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidDataException("The module directory is empty.");
        var entries = _directoryValidator.Validate(directory);
        var installed = _installedVersions();
        var hostApi = Version.Parse(_hostApiVersion());
        var result = new List<AvailableModuleDirectoryItem>();

        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var catalogResponse = await _httpClient.GetAsync(entry.CatalogUrl,
                HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            catalogResponse.EnsureSuccessStatusCode();
            await using var catalogContent = await catalogResponse.Content
                .ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var catalog = await JsonSerializer.DeserializeAsync<UpdateCatalog>(
                catalogContent, JsonOptions, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidDataException($"The update catalog for module '{entry.Id}' is empty.");
            var component = _catalogValidator.Validate(catalog, entry.Id);
            var release = component.Releases
                .Where(candidate => Version.Parse(candidate.HostApiMinimum!) <= hostApi
                    && Version.Parse(candidate.HostApiMaximum!) >= hostApi)
                .OrderByDescending(candidate => Version.Parse(candidate.Version))
                .FirstOrDefault();
            installed.TryGetValue(entry.Id, out var installedVersion);
            result.Add(new(entry.Id, entry.DisplayName, entry.CatalogUrl,
                release?.Version, installedVersion));
        }

        return result;
    }

    private static IReadOnlyDictionary<string, string> GetInstalledVersions() =>
        EmulationModuleRegistry.Packages.ToDictionary(
            module => module.Manifest.Id, module => module.Manifest.ModuleVersion,
            StringComparer.OrdinalIgnoreCase);

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }
}

internal sealed record AvailableModuleDirectoryItem(
    string Id,
    string DisplayName,
    string CatalogUrl,
    string? AvailableVersion,
    string? InstalledVersion)
{
    internal bool CanInstall => AvailableVersion is not null && InstalledVersion is null;
}
