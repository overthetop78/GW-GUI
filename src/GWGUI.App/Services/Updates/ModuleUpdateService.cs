using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using GWGUI.App.Services.Emulation;
using GWGUI.Emulation.Constants;
using GWGUI.Updates.Contracts;
using GWGUI.Updates.Services;

namespace GWGUI.App.Services.Updates;

internal sealed class ModuleUpdateService
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();
    private readonly HttpClient _httpClient;
    private readonly Func<IReadOnlyList<LoadedEmulationModule>> _installedModules;
    private readonly ModuleUpdateCatalogValidator _catalogValidator;
    private readonly UpdatePlanBuilder _planBuilder;
    private readonly string _modulesDirectory;

    internal ModuleUpdateService(
        HttpClient? httpClient = null,
        Func<IReadOnlyList<LoadedEmulationModule>>? installedModules = null,
        ModuleUpdateCatalogValidator? catalogValidator = null,
        UpdatePlanBuilder? planBuilder = null,
        string? modulesDirectory = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _installedModules = installedModules ?? (() => EmulationModuleRegistry.Packages);
        _catalogValidator = catalogValidator ?? new ModuleUpdateCatalogValidator();
        _planBuilder = planBuilder ?? new UpdatePlanBuilder();
        _modulesDirectory = Path.GetFullPath(modulesDirectory ?? Path.Combine(AppContext.BaseDirectory, "Modules"));
    }

    internal async Task<UpdateSearchResult> SearchAsync(
        IReadOnlyDictionary<string, string>? selections = null,
        CancellationToken cancellationToken = default)
    {
        var legacyModules = FindModulesWithoutUpdateSource();
        if (legacyModules.Count > 0)
            throw new InvalidDataException(
                $"Installed module(s) {string.Join(", ", legacyModules)} have no update source. " +
                "Reinstall each module from a ZIP archive or a direct module catalog URL.");
        var packages = _installedModules();
        var installedState = new InstalledUpdateState(
            ReadApplicationVersion(),
            EmulationHostApi.CurrentVersion.ToString(2),
            packages.Select(ToInstalledVersion).ToArray());
        var rows = new List<AvailableComponentUpdate>();
        var items = new List<UpdatePlanItem>();
        var issues = new List<string>();

        foreach (var package in packages.OrderBy(package => package.Manifest.Id, StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var response = await _httpClient.GetAsync(package.Manifest.UpdateCatalogUrl,
                HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var catalog = await JsonSerializer.DeserializeAsync<UpdateCatalog>(content, JsonOptions, cancellationToken)
                .ConfigureAwait(false) ?? throw new InvalidDataException(
                    $"The update catalog for module '{package.Manifest.Id}' is empty.");
            _catalogValidator.Validate(catalog, package.Manifest.Id);
            var result = _planBuilder.Search(catalog, installedState, UpdateSearchScope.Modules, selections);
            rows.AddRange(result.Components);
            items.AddRange(result.Plan.Items);
            issues.AddRange(result.Plan.Issues);
        }

        var plan = new UpdatePlan(UpdateSearchScope.Modules, items, issues.Count == 0, issues);
        return new UpdateSearchResult(UpdateSearchScope.Modules, rows, plan);
    }

    private IReadOnlyList<string> FindModulesWithoutUpdateSource()
    {
        if (!Directory.Exists(_modulesDirectory)) return [];
        var legacy = new List<string>();
        foreach (var directory in Directory.EnumerateDirectories(_modulesDirectory))
        {
            var manifestPath = Path.Combine(directory, EmulationHostApi.ManifestFileName);
            if (!File.Exists(manifestPath)) continue;
            try
            {
                using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
                var root = document.RootElement;
                if (root.TryGetProperty("updateCatalogUrl", out var source)
                    && !string.IsNullOrWhiteSpace(source.GetString())) continue;
                var id = root.TryGetProperty("id", out var idProperty) && !string.IsNullOrWhiteSpace(idProperty.GetString())
                    ? idProperty.GetString()! : Path.GetFileName(directory);
                legacy.Add(id);
            }
            catch (JsonException)
            {
                // Invalid manifests are already reported by module discovery and are not classified as legacy.
            }
        }
        return legacy.Order(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static InstalledModuleVersion ToInstalledVersion(LoadedEmulationModule package) => new(
        package.Manifest.Id,
        package.Manifest.ModuleVersion,
        package.Manifest.HostApiMinimum,
        package.Manifest.HostApiMaximum);

    private static string ReadApplicationVersion()
    {
        var version = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0, 0);
        return $"{version.Major}.{version.Minor}.{Math.Max(version.Build, 0)}";
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }
}
