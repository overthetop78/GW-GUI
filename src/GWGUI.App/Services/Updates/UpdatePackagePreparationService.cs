using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using GWGUI.Updates.Contracts;
using GWGUI.Updates.Services;

namespace GWGUI.App.Services.Updates;

internal sealed class UpdatePackagePreparationService
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();
    private readonly HttpClient _httpClient;
    private readonly Func<InstalledUpdateState> _installedState;
    private readonly string _applicationDirectory;
    private readonly UpdateArchiveValidator _archiveValidator;
    private readonly string _workingRootDirectory;

    internal UpdatePackagePreparationService(
        HttpClient? httpClient = null,
        Func<InstalledUpdateState>? installedState = null,
        string? applicationDirectory = null,
        UpdateArchiveValidator? archiveValidator = null,
        string? workingRootDirectory = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _installedState = installedState ?? ApplicationUpdateService.ReadInstalledState;
        _applicationDirectory = Path.GetFullPath(applicationDirectory ?? AppContext.BaseDirectory);
        _archiveValidator = archiveValidator ?? new UpdateArchiveValidator();
        _workingRootDirectory = Path.GetFullPath(workingRootDirectory
            ?? Path.Combine(Path.GetTempPath(), "GW GUI", "Updates"));
    }

    internal async Task<PreparedUpdateLaunch> PrepareAsync(UpdatePlan plan,
        IProgress<double>? progress = null, CancellationToken cancellationToken = default,
        IReadOnlyDictionary<string, string>? expectedModuleCatalogUrls = null)
    {
        if (!plan.IsCompatible || plan.Items.Count == 0)
            throw new InvalidOperationException("The selected update plan is empty or incompatible.");
        var installed = _installedState();
        ValidateFinalCompatibility(plan, installed);
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
                var validated = _archiveValidator.ExtractValidated(archive, extraction, item.ComponentId,
                    item.Kind, item.Release.Version);
                if (validated.ModuleManifest is not null)
                    ValidateModuleHostCompatibility(validated.ModuleManifest, installed.HostApiVersion);
                if (item.Kind == UpdateComponentKind.Module
                    && expectedModuleCatalogUrls?.TryGetValue(item.ComponentId, out var expectedCatalogUrl) == true
                    && (!Uri.TryCreate(validated.ModuleManifest?.UpdateCatalogUrl, UriKind.Absolute, out var declared)
                        || !Uri.TryCreate(expectedCatalogUrl, UriKind.Absolute, out var expected)
                        || declared != expected))
                    throw new InvalidDataException(
                        $"Module '{item.ComponentId}' declares a different update catalog URL.");
                var operation = item.Kind == UpdateComponentKind.Application
                    ? UpdateComponentOperation.UpdateApplication
                    : installed.Modules.Any(module => module.Id.Equals(item.ComponentId,
                        StringComparison.OrdinalIgnoreCase))
                        ? UpdateComponentOperation.UpdateModule
                        : UpdateComponentOperation.InstallModule;
                prepared.Add(new(item.ComponentId, item.Kind, operation, item.Release.Version,
                    validated.RootDirectory));
                progress?.Report((index + 1d) / plan.Items.Count);
            }

            return await CreateLaunchAsync(transactionId, workDirectory, prepared, cancellationToken)
                .ConfigureAwait(false);
        }
        catch
        {
            try { if (Directory.Exists(workDirectory)) Directory.Delete(workDirectory, recursive: true); }
            catch { }
            throw;
        }
    }

    internal async Task<PreparedModuleInstallation> PrepareLocalModuleAsync(string archivePath,
        CancellationToken cancellationToken = default)
    {
        var sourceArchive = Path.GetFullPath(archivePath);
        if (!File.Exists(sourceArchive)) throw new FileNotFoundException("Module archive was not found.", sourceArchive);
        VerifyInstallationIsWritable();
        var transactionId = Guid.NewGuid().ToString("N");
        var workDirectory = Path.Combine(_workingRootDirectory, transactionId);
        var localArchive = Path.Combine(workDirectory, "packages", "module.zip");
        Directory.CreateDirectory(Path.GetDirectoryName(localArchive)!);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            File.Copy(sourceArchive, localArchive, overwrite: false);
            cancellationToken.ThrowIfCancellationRequested();
            var validated = _archiveValidator.ExtractModuleValidated(localArchive,
                Path.Combine(workDirectory, "extracted"));
            var manifest = validated.ModuleManifest
                ?? throw new InvalidDataException("The module archive does not contain a manifest.");
            var installed = _installedState();
            if (installed.Modules.Any(module => module.Id.Equals(manifest.Id, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Module '{manifest.Id}' is already installed.");
            ValidateModuleHostCompatibility(manifest, installed.HostApiVersion);
            var component = new PreparedUpdateComponent(manifest.Id, UpdateComponentKind.Module,
                UpdateComponentOperation.InstallModule, manifest.ModuleVersion, validated.RootDirectory);
            var launch = await CreateLaunchAsync(transactionId, workDirectory, [component], cancellationToken)
                .ConfigureAwait(false);
            return new(manifest.Id, manifest.ModuleVersion, manifest.UpdateCatalogUrl, launch);
        }
        catch
        {
            try { if (Directory.Exists(workDirectory)) Directory.Delete(workDirectory, recursive: true); }
            catch { }
            throw;
        }
    }

    private async Task<PreparedUpdateLaunch> CreateLaunchAsync(string transactionId, string workDirectory,
        IReadOnlyList<PreparedUpdateComponent> prepared, CancellationToken cancellationToken)
    {
        var updaterSource = Path.Combine(_applicationDirectory, "Updater");
        var updaterDirectory = Path.Combine(workDirectory, "updater");
        CopyRegularDirectory(updaterSource, updaterDirectory);
        var updaterExecutable = Path.Combine(updaterDirectory, "gwgui.updater.exe");
        if (!File.Exists(updaterExecutable))
            throw new FileNotFoundException("The installed updater is missing.", updaterExecutable);

        var executionPlan = new UpdateExecutionPlan(2, transactionId, _applicationDirectory,
            Path.Combine(_applicationDirectory, "gwgui.exe"), RunningApplicationProcessIds(), 10, 30,
            Path.Combine(workDirectory, "startup.signal"), Path.Combine(workDirectory, "result.json"), prepared);
        var planPath = Path.Combine(workDirectory, "update-plan.json");
        var temporaryPlan = planPath + ".tmp";
        await File.WriteAllTextAsync(temporaryPlan, JsonSerializer.Serialize(executionPlan, JsonOptions),
            cancellationToken).ConfigureAwait(false);
        File.Move(temporaryPlan, planPath);
        return new(updaterExecutable, planPath, workDirectory, executionPlan.ResultPath);
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
        if (!Version.TryParse(apiValue, out var targetApi))
            throw new InvalidDataException("Invalid target host API.");
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

    private static void ValidateModuleHostCompatibility(
        GWGUI.Emulation.Contracts.EmulationModuleManifest manifest, string hostApiVersion)
    {
        if (!Version.TryParse(hostApiVersion, out var hostApi)
            || !Version.TryParse(manifest.HostApiMinimum, out var minimum)
            || !Version.TryParse(manifest.HostApiMaximum, out var maximum)
            || hostApi < minimum || hostApi > maximum)
            throw new InvalidDataException(
                $"Module '{manifest.Id}' is incompatible with host API {hostApiVersion}.");
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
        if (!Directory.Exists(source))
            throw new DirectoryNotFoundException($"Installed updater directory is missing: {source}");
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

internal sealed record PreparedModuleInstallation(
    string ModuleId,
    string ModuleVersion,
    string UpdateCatalogUrl,
    PreparedUpdateLaunch Launch);
