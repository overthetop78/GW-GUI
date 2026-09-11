using System.IO;
using System.Text.Json;
using GWGUI.Domain.Contracts;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Composition;

namespace GWGUI.Tests;

public sealed class MediaCorpusAuditTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    [Fact]
    public async Task EveryLocalMediaImageIsAuditedThroughTheCommonServices()
    {
        var repositoryRoot = FindRepositoryRoot();
        var corpusRoot = ResolveCorpusRoot(repositoryRoot);
        var validatedRoot = Path.Combine(corpusRoot, "validated_images");
        var engine = MediaEngineComposition.CreateDefault();
        var supportedExtensions = engine.Recognition.Readers
            .SelectMany(reader => reader.Extensions)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var imagePaths = Directory.EnumerateFiles(corpusRoot, "*", SearchOption.AllDirectories)
            .Where(path => supportedExtensions.Contains(Path.GetExtension(path)))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var results = new List<MediaCorpusAuditEntry>(imagePaths.Length);

        foreach (var imagePath in imagePaths)
        {
            var relativePath = Path.GetRelativePath(corpusRoot, imagePath);
            var isValidated = IsBelow(imagePath, validatedRoot);
            try
            {
                var document = await engine.ReadingService.ReadAsync(
                    new MediaSourceDescriptor(imagePath, SiblingFiles(imagePath)));
                var descriptor = engine.Visualization.Registry.CreateDescriptor(document);
                if (descriptor.RepresentationKind != document.Representation.RepresentationKind)
                    throw new InvalidDataException("The visualization descriptor does not match the media representation.");
                if (descriptor.Elements.Count == 0)
                    throw new InvalidDataException("The visualization descriptor contains no element.");

                var explored = await engine.Explorer.ExploreAsync(document);
                if (explored.Volumes.Count == 0)
                    throw new InvalidDataException("The media explorer returned no volume.");
                var fileSystems = explored.Volumes
                    .Where(volume => volume.FileSystem is not null)
                    .Select(volume => volume.FileSystem!.FileSystemId)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Order(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                var expectedRepresentation = ExpectedFloppyRepresentation(imagePath);
                var expectationError = isValidated &&
                    (document.MediaKind != MediaKind.Floppy ||
                     document.Representation.RepresentationKind != expectedRepresentation)
                    ? $"Expected a {expectedRepresentation} floppy image."
                    : null;

                results.Add(new MediaCorpusAuditEntry(
                    relativePath,
                    isValidated,
                    expectationError is null ? "passed" : "error",
                    document.FormatId,
                    document.MediaKind.ToString(),
                    document.Representation.RepresentationKind.ToString(),
                    descriptor.Elements.Count,
                    explored.Volumes.Count,
                    fileSystems,
                    document.Diagnostics.Concat(explored.Diagnostics).ToArray(),
                    expectationError));
            }
            catch (NotSupportedException exception)
            {
                results.Add(Failure(relativePath, isValidated, "unsupported", exception));
            }
            catch (Exception exception)
            {
                results.Add(Failure(relativePath, isValidated, "error", exception));
            }
        }

        var report = new MediaCorpusAuditReport(
            corpusRoot,
            DateTimeOffset.UtcNow,
            imagePaths.Length,
            results.Count(result => result.Status == "passed"),
            results.Count(result => result.Status == "unsupported"),
            results.Count(result => result.Status == "error"),
            results);
        var reportPath = Path.Combine(repositoryRoot, "build", "validation", "media-corpus-audit.json");
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
        await File.WriteAllTextAsync(reportPath, JsonSerializer.Serialize(report, JsonOptions));

        var validatedFailures = results
            .Where(result => result.IsValidated && result.Status != "passed")
            .Select(result => $"{result.RelativePath}: {result.Error}")
            .ToArray();
        Assert.True(validatedFailures.Length == 0,
            $"{validatedFailures.Length} previously validated image(s) failed the common media audit.{Environment.NewLine}" +
            string.Join(Environment.NewLine, validatedFailures.Take(20)));
    }

    private static MediaCorpusAuditEntry Failure(
        string relativePath,
        bool isValidated,
        string status,
        Exception exception) =>
        new(relativePath, isValidated, status, null, null, null, 0, 0, [], [],
            $"{exception.GetType().Name}: {Flatten(exception)}");

    private static string Flatten(Exception exception) => exception is AggregateException aggregate
        ? string.Join(" | ", aggregate.Flatten().InnerExceptions.Select(item => item.Message))
        : exception.Message;

    private static MediaRepresentationKind ExpectedFloppyRepresentation(string path) =>
        Path.GetExtension(path).Equals(".scp", StringComparison.OrdinalIgnoreCase) ||
        Path.GetExtension(path).Equals(".86f", StringComparison.OrdinalIgnoreCase)
            ? MediaRepresentationKind.Flux
            : MediaRepresentationKind.Sectors;

    private static IReadOnlyList<string> SiblingFiles(string path) =>
        Directory.EnumerateFiles(Path.GetDirectoryName(path)!, "*", SearchOption.TopDirectoryOnly)
            .Where(candidate => !candidate.Equals(path, StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static bool IsBelow(string path, string directory)
    {
        var relative = Path.GetRelativePath(directory, path);
        return !relative.Equals("..", StringComparison.Ordinal) &&
               !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
               !Path.IsPathRooted(relative);
    }

    private static string ResolveCorpusRoot(string repositoryRoot)
    {
        var configured = Environment.GetEnvironmentVariable("GWGUI_IMAGE_TEST_ROOT");
        var candidates = new[]
        {
            configured,
            @"F:\Rétro\image_test",
            Path.Combine(repositoryRoot, "image_test")
        };
        var root = candidates.FirstOrDefault(candidate =>
            !string.IsNullOrWhiteSpace(candidate) && Directory.Exists(candidate));
        return root is null
            ? throw new DirectoryNotFoundException(
                "Set GWGUI_IMAGE_TEST_ROOT to the local image_test corpus directory.")
            : Path.GetFullPath(root);
    }

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

    private sealed record MediaCorpusAuditReport(
        string CorpusRoot,
        DateTimeOffset GeneratedAtUtc,
        int ImageCount,
        int Passed,
        int Unsupported,
        int Errors,
        IReadOnlyList<MediaCorpusAuditEntry> Images);

    private sealed record MediaCorpusAuditEntry(
        string RelativePath,
        bool IsValidated,
        string Status,
        string? FormatId,
        string? MediaKind,
        string? RepresentationKind,
        int VisualizationElementCount,
        int VolumeCount,
        IReadOnlyList<string> FileSystemIds,
        IReadOnlyList<string> Diagnostics,
        string? Error);
}
