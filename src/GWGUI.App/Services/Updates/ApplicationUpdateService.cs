using System.Reflection;
using System.Net.Http;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
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
    private readonly string _applicationDirectory;
    private readonly UpdateArchiveValidator _archiveValidator;
    private readonly string _workingRootDirectory;

    internal ApplicationUpdateService(
        HttpClient? httpClient = null,
        Uri? catalogUri = null,
        Func<InstalledUpdateState>? installedState = null,
        UpdatePlanBuilder? planBuilder = null,
        string? applicationDirectory = null,
        UpdateArchiveValidator? archiveValidator = null,
        string? workingRootDirectory = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _catalogUri = catalogUri ?? new Uri(UpdateEndpoints.CatalogUrl);
        _installedState = installedState ?? ReadInstalledState;
        _planBuilder = planBuilder ?? new UpdatePlanBuilder();
        _applicationDirectory = Path.GetFullPath(applicationDirectory ?? AppContext.BaseDirectory);
        _archiveValidator = archiveValidator ?? new UpdateArchiveValidator();
        _workingRootDirectory = Path.GetFullPath(workingRootDirectory
            ?? Path.Combine(Path.GetTempPath(), "GW GUI", "Updates"));
    }

    internal async Task<PreparedUpdateLaunch> PrepareAsync(UpdatePlan plan,
        IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        if (!plan.IsCompatible || plan.Items.Count == 0)
            throw new InvalidOperationException("The selected update plan is empty or incompatible.");
        ValidateFinalCompatibility(plan, _installedState());
        VerifyInstallationIsWritable();

        var transactionId = Guid.NewGuid().ToString("N");
        var workDirectory = Path.Combine(_workingRootDirectory, transactionId);
        var packagesDirectory = Path.Combine(workDirectory, "packages");
        var extractedDirectory = Path.Combine(workDirectory, "extracted");
        Directory.CreateDirectory(packagesDirectory);
        try
        {
            var prepared = new List<PreparedUpdateComponent>();
            for (var index = 0; index < plan.Items.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var item = plan.Items[index];
                var archive = Path.Combine(packagesDirectory, $"{item.ComponentId}-{item.Release.Version}.zip");
                await DownloadVerifiedAsync(item.Release, archive, cancellationToken).ConfigureAwait(false);
                var extraction = Path.Combine(extractedDirectory, item.ComponentId);
                var root = _archiveValidator.ExtractValidated(archive, extraction, item.ComponentId,
                    item.Kind, item.Release.Version);
                prepared.Add(new(item.ComponentId, item.Kind, item.Release.Version, root));
                progress?.Report((index + 1d) / plan.Items.Count);
            }

            var updaterSource = Path.Combine(_applicationDirectory, "Updater");
            var updaterDirectory = Path.Combine(workDirectory, "updater");
            CopyRegularDirectory(updaterSource, updaterDirectory);
            var updaterExecutable = Path.Combine(updaterDirectory, "gwgui.updater.exe");
            if (!File.Exists(updaterExecutable)) throw new FileNotFoundException("The installed updater is missing.", updaterExecutable);

            var executionPlan = new UpdateExecutionPlan(1, transactionId, _applicationDirectory,
                Path.Combine(_applicationDirectory, "gwgui.exe"), RunningApplicationProcessIds(), 10, 30,
                Path.Combine(workDirectory, "startup.signal"), Path.Combine(workDirectory, "result.json"), prepared);
            var planPath = Path.Combine(workDirectory, "update-plan.json");
            var temporaryPlan = planPath + ".tmp";
            await File.WriteAllTextAsync(temporaryPlan, JsonSerializer.Serialize(executionPlan, JsonOptions),
                cancellationToken).ConfigureAwait(false);
            File.Move(temporaryPlan, planPath);
            return new(updaterExecutable, planPath, workDirectory, executionPlan.ResultPath);
        }
        catch
        {
            try { if (Directory.Exists(workDirectory)) Directory.Delete(workDirectory, recursive: true); }
            catch { }
            throw;
        }
    }

    internal async Task<UpdateSearchResult> SearchAsync(
        UpdateSearchScope scope,
        IReadOnlyDictionary<string, string>? selections = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(_catalogUri, HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var catalog = await JsonSerializer.DeserializeAsync<UpdateCatalog>(content, JsonOptions, cancellationToken)
            .ConfigureAwait(false) ?? throw new InvalidDataException("The update catalog is empty.");
        return _planBuilder.Search(catalog, _installedState(), scope, selections);
    }

    private static InstalledUpdateState ReadInstalledState()
    {
        var loadedIds = EmulationModuleRegistry.Modules.Select(module => module.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var installedModules = new List<InstalledModuleVersion>();
        var modulesDirectory = Path.Combine(AppContext.BaseDirectory, "Modules");
        if (Directory.Exists(modulesDirectory))
        {
            foreach (var directory in Directory.EnumerateDirectories(modulesDirectory))
            {
                try
                {
                    var manifest = EmulationModuleManifestReader.Read(directory);
                    if (loadedIds.Contains(manifest.Id))
                        installedModules.Add(new(manifest.Id, manifest.ModuleVersion,
                            manifest.HostApiMinimum, manifest.HostApiMaximum));
                }
                catch (Exception) { }
            }
        }

        return new InstalledUpdateState(ReadApplicationVersion(), EmulationHostApi.CurrentVersion.ToString(2),
            installedModules.OrderBy(module => module.Id, StringComparer.OrdinalIgnoreCase).ToArray());
    }

    private async Task DownloadVerifiedAsync(UpdateCatalogRelease release, string destination,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(release.PackageUrl, HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        await using var input = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None,
            81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[81920];
        while (true)
        {
            var count = await input.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            if (count == 0) break;
            await output.WriteAsync(buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
            hash.AppendData(buffer, 0, count);
        }
        await output.FlushAsync(cancellationToken).ConfigureAwait(false);
        var actual = Convert.ToHexString(hash.GetHashAndReset());
        if (!actual.Equals(release.Sha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"SHA-256 mismatch for component package '{release.PackageUrl}'.");
    }

    private static void ValidateFinalCompatibility(UpdatePlan plan, InstalledUpdateState installed)
    {
        var application = plan.Items.FirstOrDefault(item => item.Kind == UpdateComponentKind.Application);
        var apiValue = application?.Release.HostApiVersion ?? installed.HostApiVersion;
        if (!Version.TryParse(apiValue, out var targetApi)) throw new InvalidDataException("Invalid target host API.");
        foreach (var module in installed.Modules)
        {
            var update = plan.Items.FirstOrDefault(item => item.Kind == UpdateComponentKind.Module
                && item.ComponentId.Equals(module.Id, StringComparison.OrdinalIgnoreCase));
            var minimumValue = update?.Release.HostApiMinimum ?? module.HostApiMinimum;
            var maximumValue = update?.Release.HostApiMaximum ?? module.HostApiMaximum;
            if (!Version.TryParse(minimumValue, out var minimum) || !Version.TryParse(maximumValue, out var maximum)
                || targetApi < minimum || targetApi > maximum)
                throw new InvalidDataException($"Module '{module.Id}' is incompatible with target host API {apiValue}.");
        }
    }

    private void VerifyInstallationIsWritable()
    {
        var probe = Path.Combine(_applicationDirectory, $".gwgui-update-{Guid.NewGuid():N}.tmp");
        try { using (File.Create(probe)) { } }
        finally { if (File.Exists(probe)) File.Delete(probe); }
    }

    private IReadOnlyList<int> RunningApplicationProcessIds()
    {
        var executable = Path.Combine(_applicationDirectory, "gwgui.exe");
        var processIds = new HashSet<int> { Environment.ProcessId };
        foreach (var process in Process.GetProcessesByName("gwgui"))
        {
            using (process)
            {
                try
                {
                    if (string.Equals(process.MainModule?.FileName, executable, StringComparison.OrdinalIgnoreCase))
                        processIds.Add(process.Id);
                }
                catch { }
            }
        }
        return processIds.Order().ToArray();
    }

    private static void CopyRegularDirectory(string source, string destination)
    {
        if (!Directory.Exists(source)) throw new DirectoryNotFoundException($"Installed updater directory is missing: {source}");
        foreach (var directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories).Prepend(source))
        {
            if ((File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException($"Updater directory cannot contain a reparse point: {directory}");
            var relative = Path.GetRelativePath(source, directory);
            Directory.CreateDirectory(relative == "." ? destination : Path.Combine(destination, relative));
        }
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            if ((File.GetAttributes(file) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException($"Updater directory cannot contain a reparse point: {file}");
            var target = Path.Combine(destination, Path.GetRelativePath(source, file));
            File.Copy(file, target, overwrite: false);
        }
    }

    private static string ReadApplicationVersion()
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

internal sealed record PreparedUpdateLaunch(
    string UpdaterExecutable,
    string PlanPath,
    string WorkingDirectory,
    string ResultPath);
