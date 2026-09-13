using System.IO;
using System.Text.Json;
using GWGUI.Domain.Contracts;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Reconstruction.Scp;
using Xunit.Abstractions;

namespace GWGUI.Tests;

public sealed class MediaCorpusAuditTests(ITestOutputHelper output)
{
    private const string AuditVersion = "2026-09-12.13";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly AuditRoot[] DefaultRoots =
    [
        new(@"F:\Rétro", MediaKind.Floppy),
        new(@"F:\86Box\Isos", MediaKind.Optical),
        new(@"C:\Users\overt\Documents\GW GUI\Emulation\HDD", MediaKind.HardDisk),
        new(@"C:\Users\overt\86Box VMs", MediaKind.HardDisk)
    ];

    [Fact]
    public async Task EveryLocalMediaImageIsAuditedThroughTheCommonServices()
    {
        var repositoryRoot = FindRepositoryRoot();
        var engine = MediaEngineComposition.CreateDefault();
        var legacyDiskExplorer = DiskImageExplorer.CreateDefault();
        var fluxDecoders = new FluxDecoderRegistry();
        var roots = ResolveRoots();
        var extensionFilter = SplitEnvironment("GWGUI_MEDIA_AUDIT_EXTENSIONS")
            .Select(NormalizeExtension).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var pathFilter = Environment.GetEnvironmentVariable("GWGUI_MEDIA_AUDIT_PATH_CONTAINS");
        var limit = int.TryParse(Environment.GetEnvironmentVariable("GWGUI_MEDIA_AUDIT_LIMIT"), out var parsedLimit) && parsedLimit > 0
            ? parsedLimit : int.MaxValue;
        var supportedByKind = roots.Select(root => root.Kind).Distinct().ToDictionary(
            kind => kind,
            kind => PrimaryExtensions(engine, kind));
        var sources = roots.SelectMany(root => EnumerateSources(root, supportedByKind[root.Kind], extensionFilter))
            .Where(source => string.IsNullOrWhiteSpace(pathFilter) || source.FullPath.Contains(pathFilter, StringComparison.OrdinalIgnoreCase))
            .OrderBy(source => source.Root.Path, StringComparer.OrdinalIgnoreCase)
            .ThenBy(source => source.RelativePath, StringComparer.OrdinalIgnoreCase)
            .Take(limit)
            .ToArray();

        var validationDirectory = Path.Combine(repositoryRoot, "build", "validation");
        Directory.CreateDirectory(validationDirectory);
        var checkpointPath = Path.Combine(validationDirectory, "media-corpus-audit.checkpoint.jsonl");
        var reportPath = Path.Combine(validationDirectory, "media-corpus-audit.json");
        var completed = LoadCheckpoint(checkpointPath);
        await using var checkpoint = new StreamWriter(checkpointPath, append: true) { AutoFlush = true };
        var results = new List<MediaCorpusAuditEntry>(sources.Length);

        for (var index = 0; index < sources.Length; index++)
        {
            var source = sources[index];
            var fingerprint = Fingerprint(source.FullPath);
            if (completed.TryGetValue(source.FullPath, out var cached) && cached.AuditVersion == AuditVersion && cached.Fingerprint == fingerprint)
            {
                results.Add(cached);
                continue;
            }

            output.WriteLine($"[{index + 1}/{sources.Length}] {source.FullPath}");
            var result = await AuditAsync(engine, legacyDiskExplorer, fluxDecoders, source, fingerprint);
            results.Add(result);
            completed[source.FullPath] = result;
            await checkpoint.WriteLineAsync(JsonSerializer.Serialize(result));
        }

        var report = new MediaCorpusAuditReport(
            AuditVersion,
            roots,
            DateTimeOffset.UtcNow,
            sources.Length,
            results.Count(result => result.Status == "passed"),
            results.Count(result => result.Status == "unsupported"),
            results.Count(result => result.Status == "error"),
            results);
        await File.WriteAllTextAsync(reportPath, JsonSerializer.Serialize(report, JsonOptions));

        var failures = results.Where(result => result.Status != "passed")
            .Select(result => $"{result.FullPath}: {result.Error}").ToArray();
        Assert.True(failures.Length == 0,
            $"{failures.Length} image(s) failed the read-only media audit. Report: {reportPath}{Environment.NewLine}" +
            string.Join(Environment.NewLine, failures.Take(30)));
    }

    private static async Task<MediaCorpusAuditEntry> AuditAsync(
        MediaEngineComposition engine,
        DiskImageExplorer legacyDiskExplorer,
        FluxDecoderRegistry fluxDecoders,
        AuditSource source,
        string fingerprint)
    {
        try
        {
            var document = await engine.ReadingService.ReadAsync(new MediaSourceDescriptor(source.FullPath, SiblingFiles(source.FullPath)));
            if (document.MediaKind != source.Root.Kind)
                throw new InvalidDataException($"Expected {source.Root.Kind}, detected {document.MediaKind}.");
            var descriptor = engine.Visualization.Registry.CreateDescriptor(document);
            if (descriptor.RepresentationKind != document.Representation.RepresentationKind || descriptor.Elements.Count == 0)
                throw new InvalidDataException("The visualizer descriptor is empty or does not match the media representation.");
            var explored = await engine.Explorer.ExploreAsync(document);
            if (document.FormatId.Equals(GWGUI.MediaEngine.Constants.DiskImageFormatIds.ApricotPcXi315, StringComparison.OrdinalIgnoreCase) &&
                document.Representation is GWGUI.MediaEngine.Representations.Sectors.SectorMediaImageRepresentation apricotSectors)
            {
                var apricotFat = new GWGUI.MediaEngine.FileSystems.Fat12.Fat12FileSystemReader();
                var directVolume = apricotFat.Read(apricotSectors.Image);
                if (!apricotFat.CanRead(apricotSectors.Image))
                    throw new InvalidDataException($"Apricot FAT12 CanRead rejected a directly readable volume containing {directVolume.Entries.Count} entries.");
            }
            var fileSystems = explored.Volumes.Where(volume => volume.FileSystem is not null)
                .Select(volume => volume.FileSystem!.FileSystemId).Distinct(StringComparer.OrdinalIgnoreCase)
                .Order(StringComparer.OrdinalIgnoreCase).ToArray();
            var entries = explored.Volumes.Where(volume => volume.FileSystem is not null)
                .SelectMany(volume => FlattenEntries(volume.FileSystem!.Entries)).ToArray();
            IReadOnlyList<string> detectedFormats = [document.FormatId];
            IReadOnlyList<ScpTrackAudit> scpTracks = [];
            if (source.Root.Kind == MediaKind.Floppy)
            {
                var disk = await legacyDiskExplorer.ExploreAsync(source.FullPath);
                detectedFormats = disk.DetectedImageFormatIds.Count == 0 ? [disk.PrimaryFormatId] : disk.DetectedImageFormatIds;
                entries = entries.Concat(FlattenEntries(disk.Volume.Entries)).Distinct().ToArray();
                if (Path.GetExtension(source.FullPath).Equals(".scp", StringComparison.OrdinalIgnoreCase))
                    scpTracks = AuditScpTracks(disk.ScpImage ?? throw new InvalidDataException("The SCP exploration did not preserve its flux image."), fluxDecoders);
            }

            if (document.FormatId.Equals(GWGUI.MediaEngine.Constants.DiskImageFormatIds.ApricotPcXi315, StringComparison.OrdinalIgnoreCase))
            {
                if (descriptor.Elements.Count != 70) throw new InvalidDataException($"ApriDisk visualization exposes {descriptor.Elements.Count} tracks instead of 70.");
                if (!fileSystems.Contains(GWGUI.MediaEngine.FileSystems.Definitions.FileSystemIds.Fat12, StringComparer.OrdinalIgnoreCase)) throw new InvalidDataException("The Apricot FAT12 volume was not explored.");
                if (entries.Length == 0) throw new InvalidDataException("The Apricot FAT12 volume contains no visible file or directory.");
                if (detectedFormats.Any(format => format.Contains("apple", StringComparison.OrdinalIgnoreCase) || format.Contains("msx", StringComparison.OrdinalIgnoreCase) || format.Contains("amstrad", StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidDataException($"ApriDisk was also misclassified as {string.Join(", ", detectedFormats)}.");
            }

            return new(AuditVersion, source.Root.Path, source.Root.Kind.ToString(), source.RelativePath, source.FullPath,
                fingerprint, "passed", document.FormatId, document.Representation.RepresentationKind.ToString(),
                descriptor.Elements.Count, explored.Volumes.Count, detectedFormats, fileSystems, entries,
                scpTracks, document.Diagnostics.Concat(explored.Diagnostics).Distinct().ToArray(), null);
        }
        catch (NotSupportedException exception)
        {
            return Failure(source, fingerprint, "unsupported", exception);
        }
        catch (Exception exception)
        {
            return Failure(source, fingerprint, "error", exception);
        }
    }

    private static IReadOnlyList<ScpTrackAudit> AuditScpTracks(GWGUI.MediaEngine.Formats.Floppy.Scp.ScpImage image, FluxDecoderRegistry decoders) =>
        image.Tracks.OrderBy(track => track.TrackNumber).Select(track =>
        {
            var revolutions = ScpTrackDecodeWindowFactory.Create(track).Select(window =>
            {
                var decoded = decoders.DecodeAutomatic(window.Flux);
                return new ScpRevolutionAudit(window.Revolution, decoded.DecoderId, decoded.Confidence,
                    decoded.Sectors.Count, decoded.Sectors.Count(sector => sector.Data is not null && sector.IntegrityValid == true));
            }).ToArray();
            return new ScpTrackAudit(track.TrackNumber, track.Cylinder, track.Head, track.Revolutions.Count,
                revolutions.Select(item => item.DecoderId).Distinct(StringComparer.Ordinal).ToArray(), revolutions);
        }).ToArray();

    private static IEnumerable<MediaEntryAudit> FlattenEntries(IEnumerable<FileSystemEntry> entries, string prefix = "")
    {
        foreach (var entry in entries.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase))
        {
            var path = string.IsNullOrEmpty(prefix) ? entry.Name : $"{prefix}/{entry.Name}";
            yield return new(path, entry.Kind.ToString(), entry.Size);
            foreach (var child in FlattenEntries(entry.Children, path)) yield return child;
        }
    }

    private static IReadOnlySet<string> PrimaryExtensions(MediaEngineComposition engine, MediaKind kind)
    {
        var associated = engine.Recognition.Readers.SelectMany(reader => reader.AssociatedFileExtensions)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return engine.Recognition.Readers.Where(reader => reader.MediaKinds.Contains(kind))
            .SelectMany(reader => reader.Extensions).Where(extension => !associated.Contains(extension))
            .Select(NormalizeExtension).ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static IEnumerable<AuditSource> EnumerateSources(AuditRoot root, IReadOnlySet<string> supported, IReadOnlySet<string> filter)
    {
        if (!Directory.Exists(root.Path)) yield break;
        var extensions = filter.Count == 0 ? supported : filter;
        foreach (var path in Directory.EnumerateFiles(root.Path, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(root.Path, path);
            if (extensions.Contains(Path.GetExtension(path)) && !IsGeneratedFixture(relativePath))
                yield return new(root, relativePath, Path.GetFullPath(path));
        }
    }

    private static bool IsGeneratedFixture(string relativePath) =>
        relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Contains("_generated", StringComparer.OrdinalIgnoreCase);

    private static Dictionary<string, MediaCorpusAuditEntry> LoadCheckpoint(string path)
    {
        var entries = new Dictionary<string, MediaCorpusAuditEntry>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(path)) return entries;
        foreach (var line in File.ReadLines(path))
        {
            try
            {
                var entry = JsonSerializer.Deserialize<MediaCorpusAuditEntry>(line);
                if (entry is not null) entries[entry.FullPath] = entry;
            }
            catch (JsonException) { }
        }
        return entries;
    }

    private static MediaCorpusAuditEntry Failure(AuditSource source, string fingerprint, string status, Exception exception) =>
        new(AuditVersion, source.Root.Path, source.Root.Kind.ToString(), source.RelativePath, source.FullPath,
            fingerprint, status, null, null, 0, 0, [], [], [], [], [],
            $"{exception.GetType().Name}: {Flatten(exception)}");

    private static string Fingerprint(string path)
    {
        var file = new FileInfo(path);
        return $"{file.Length}:{file.LastWriteTimeUtc.Ticks}";
    }

    private static string Flatten(Exception exception) => exception is AggregateException aggregate
        ? string.Join(" | ", aggregate.Flatten().InnerExceptions.Select(item => item.Message)) : exception.Message;

    private static IReadOnlyList<string> SiblingFiles(string path) =>
        Directory.EnumerateFiles(Path.GetDirectoryName(path)!, "*", SearchOption.TopDirectoryOnly)
            .Where(candidate => !candidate.Equals(path, StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.OrdinalIgnoreCase).ToArray();

    private static IReadOnlyList<AuditRoot> ResolveRoots()
    {
        var configured = SplitEnvironment("GWGUI_MEDIA_AUDIT_ROOTS");
        if (configured.Count == 0) return DefaultRoots.Where(root => Directory.Exists(root.Path)).ToArray();
        return configured.Select(path =>
        {
            var fullPath = Path.GetFullPath(path);
            var known = DefaultRoots.FirstOrDefault(root => root.Path.Equals(fullPath, StringComparison.OrdinalIgnoreCase));
            return known ?? new(fullPath, MediaKind.Floppy);
        }).ToArray();
    }

    private static IReadOnlyList<string> SplitEnvironment(string name) =>
        (Environment.GetEnvironmentVariable(name) ?? string.Empty)
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string NormalizeExtension(string extension) => extension.StartsWith('.') ? extension : $".{extension}";

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "GWGUI.sln"))) return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("The GW GUI repository root could not be found.");
    }

    private sealed record AuditRoot(string Path, MediaKind Kind);
    private sealed record AuditSource(AuditRoot Root, string RelativePath, string FullPath);
    private sealed record MediaEntryAudit(string Path, string Kind, long Size);
    private sealed record ScpRevolutionAudit(int Revolution, string DecoderId, double Confidence, int SectorCount, int ValidSectorCount);
    private sealed record ScpTrackAudit(int TrackNumber, int Cylinder, int Head, int RevolutionCount, IReadOnlyList<string> DecoderIds, IReadOnlyList<ScpRevolutionAudit> Revolutions);
    private sealed record MediaCorpusAuditReport(string AuditVersion, IReadOnlyList<AuditRoot> Roots, DateTimeOffset GeneratedAtUtc, int ImageCount, int Passed, int Unsupported, int Errors, IReadOnlyList<MediaCorpusAuditEntry> Images);
    private sealed record MediaCorpusAuditEntry(string AuditVersion, string Root, string ExpectedMediaKind, string RelativePath, string FullPath, string Fingerprint, string Status, string? ContainerFormatId, string? RepresentationKind, int VisualizationElementCount, int VolumeCount, IReadOnlyList<string> DetectedFormatIds, IReadOnlyList<string> FileSystemIds, IReadOnlyList<MediaEntryAudit> Entries, IReadOnlyList<ScpTrackAudit> ScpTracks, IReadOnlyList<string> Diagnostics, string? Error);
}
