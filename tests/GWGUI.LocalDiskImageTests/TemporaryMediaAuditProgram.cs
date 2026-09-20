using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using GWGUI.App.Functions.Explorer;
using GWGUI.App.ViewModels.Explorer;
using MediaSourceDescriptor = global::GWGUI.MediaEngine.Contracts.MediaSourceDescriptor;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Formats;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Contracts.Explorer;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Models.Blocks;
using GWGUI.MediaEngine.Images.Models.Flux;
using GWGUI.MediaEngine.Images.Models.Optical;
using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaEngine.Images.Models.Sequential;
using GWGUI.MediaAudit.TestInfrastructure;

namespace GWGUI.MediaAudit;

internal static partial class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task<int> Main(string[] args)
    {
        try
        {
            var engine = MediaEngineComposition.CreateDefault();
            if (args.Contains("--list-extensions", StringComparer.OrdinalIgnoreCase))
            {
                var extensions = engine.Recognition.Readers.SelectMany(reader => reader.Extensions)
                    .Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase).ToArray();
                Console.WriteLine(JsonSerializer.Serialize(extensions, JsonOptions));
                return 0;
            }

            var listRoot = Argument(args, "--list-images");
            if (!string.IsNullOrWhiteSpace(listRoot))
            {
                Console.WriteLine(JsonSerializer.Serialize(ListMediaImages(engine, Path.GetFullPath(listRoot)), JsonOptions));
                return 0;
            }

            var imagePath = Argument(args, "--image") ?? throw new ArgumentException("--image is required.");
            var outputDirectory = Argument(args, "--output") ?? throw new ArgumentException("--output is required.");
            Directory.CreateDirectory(outputDirectory);
            var report = await AuditAsync(engine, Path.GetFullPath(imagePath), outputDirectory);
            await WriteJsonAsync(Path.Combine(outputDirectory, "report.json"), report);
            TemporaryMediaSignatureCatalog.Record(report, outputDirectory, Environment.CurrentDirectory);
            if (report.Validation.Passed) return 0;
            throw new MediaAuditValidationException(imagePath, report.Validation.Errors);
        }
        catch (Exception exception)
        {
            var outputDirectory = Argument(args, "--output");
            if (!string.IsNullOrWhiteSpace(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
                await WriteJsonAsync(Path.Combine(outputDirectory, "failure.json"), new
                {
                    failedAt = DateTimeOffset.UtcNow,
                    image = Argument(args, "--image"),
                    exception = exception.GetType().FullName,
                    exception.Message,
                    exception.StackTrace,
                    innerException = exception.InnerException?.ToString()
                });
            }
            Console.Error.WriteLine(exception.Message);
            return 2;
        }
        finally
        {
            WpfResourceCleanup.Release();
        }
    }

    private static async Task<MediaAuditReport> AuditAsync(MediaEngineComposition engine, string path, string outputDirectory)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("Media image not found.", path);
        var file = new FileInfo(path);
        var sha256 = await HashFileAsync(path);
        var sourceEdges = await ReadContentEdgesAsync(path);
        var source = new MediaSourceDescriptor(path, []);
        var context = new MediaRecognitionContext(source);
        var recognized = await engine.Recognition.Registry.RecognizeAsync(context);
        var document = recognized.Document;
        var associatedFiles = new List<AssociatedSourceFileAudit>();
        foreach (var associatedPath in document.Source.AssociatedPaths
                     .Where(File.Exists)
                     .Where(candidate => !Path.GetFullPath(candidate).Equals(path, StringComparison.OrdinalIgnoreCase))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var associated = new FileInfo(associatedPath);
            var associatedHash = await HashFileAsync(associated.FullName);
            associatedFiles.Add(new(associated.FullName, associated.Extension.ToLowerInvariant(), associated.Length, associatedHash));
        }
        var explored = await engine.Explorer.ExploreAsync(document);
        TemporaryNamedMediaFormatIdentification.ThrowIfIdentificationIsRequired(document, explored);
        var visualization = engine.Visualization.Registry.CreateDescriptor(explored.Document);
        var catalogFormat = new BuiltInImageFormatCatalog().Formats.FirstOrDefault(format =>
            format.Id.Equals(document.FormatId, StringComparison.OrdinalIgnoreCase));
        var destinations = engine.ConversionService.GetAvailableDestinations(document)
            .Select(destination => new ConversionAudit(destination.FormatId, destination.Extension, destination.WriterId, destination.ProducesMultipleFiles))
            .ToArray();

        var writePlan = false;
        string? writeDiagnostic = null;
        if (document.MediaKind == MediaKind.Floppy)
        {
            try
            {
                var plan = await engine.FloppyMediaWritePlanningService.CreatePlanAsync(path, document.FormatId);
                writePlan = plan.DataUnits.Count > 0;
            }
            catch (Exception error) when (error is InvalidDataException or NotSupportedException or ArgumentException)
            {
                writeDiagnostic = error.Message;
            }
        }

        var volumes = explored.Volumes.Select(volume => CreateVolume(document, volume)).ToArray();
        var logicalFiles = volumes.SelectMany(volume => Flatten(volume.Entries)).Count(entry => entry.Kind == FileSystemEntryKind.File.ToString());
        var validation = Validate(document, recognized, explored, visualization.Elements.Count, volumes, logicalFiles);
        var declaredCapacity = volumes.Select(volume => volume.Capacity).Where(value => value.HasValue).Sum(value => value!.Value);
        var freeBytes = volumes.Where(volume => volume.FreeSpaceKnown == true).Select(volume => volume.FreeBytes).Where(value => value.HasValue).Sum(value => value!.Value);
        long? usedBytes = declaredCapacity > 0 ? Math.Max(0, declaredCapacity - freeBytes) : null;

        return new MediaAuditReport(
            3,
            DateTimeOffset.UtcNow,
            new SourceAudit(
                path, file.Name, file.Extension.ToLowerInvariant(), file.Length, file.CreationTimeUtc, file.LastWriteTimeUtc,
                sha256, sourceEdges.Start, sourceEdges.End, ExpectedFormatHint(path), associatedFiles),
            new RecognitionAudit(
                true, recognized.Reader.GetType().FullName ?? recognized.Reader.GetType().Name, document.FormatId,
                document.MediaKind.ToString(), document.Representation.RepresentationKind.ToString(), document.Metadata,
                document.Diagnostics, recognized.CandidateFailures.ToDictionary(pair => pair.Key.GetType().FullName ?? pair.Key.GetType().Name, pair => pair.Value.Message)),
            new MediaAudit(document.Representation.LogicalLength, document.Representation.SupportsRandomAccess,
                document.Representation.SupportsSequentialAccess, declaredCapacity > 0 ? declaredCapacity : document.Representation.LogicalLength,
                usedBytes, declaredCapacity > 0 ? freeBytes : null),
            volumes,
            new VisualizationAudit(true, visualization.RepresentationKind.ToString(), visualization.ProgressUnit.ToString(),
                visualization.Direction.ToString(), visualization.Surfaces, visualization.Layers, visualization.Elements.Count),
            new SupportAudit(
                volumes.Any(volume => volume.FileSystemId is not null), true, catalogFormat?.SupportsPhysicalRead,
                writePlan, writeDiagnostic, catalogFormat?.Family, catalogFormat?.FormFactor.ToString(), catalogFormat?.Density.ToString(),
                catalogFormat?.Extensions.Select(extension => extension.Extension).ToArray() ?? recognized.Reader.Extensions.Order().ToArray(), destinations),
            CreateRepresentation(document),
            new ValidationAudit(validation.Errors.Count == 0, validation.Errors, validation.Warnings));
    }

    private static VolumeAudit CreateVolume(MediaImageDocument document, ExploredMediaVolume volume)
    {
        var fileSystem = volume.FileSystem;
        var family = ExplorerFileIconClassifier.FamilyFor(document.FormatId, fileSystem?.FileSystemId);
        return new VolumeAudit(
            volume.Descriptor.Start, volume.Descriptor.Length, volume.Descriptor.Origin, volume.Descriptor.PartitionScheme,
            volume.Descriptor.PartitionNumber, volume.Descriptor.SessionNumber, volume.Descriptor.TrackNumber,
            volume.Descriptor.PartitionType, volume.Descriptor.PartitionId, fileSystem?.Name ?? volume.Descriptor.Name,
            volume.ReaderId, fileSystem?.FileSystemId, fileSystem?.Capacity, fileSystem?.FreeBytes, fileSystem?.FreeSpaceKnown,
            fileSystem?.Bootable, fileSystem?.Created, fileSystem?.Modified, fileSystem?.Attributes ?? [],
            volume.Diagnostics, fileSystem?.Warnings ?? [], fileSystem is null ? [] : CreateEntries(fileSystem.Entries, family));
    }

    private static IReadOnlyList<FileEntryAudit> CreateEntries(
        IEnumerable<FileSystemEntry> entries,
        GWGUI.App.Enums.Explorer.ExplorerFileSystemFamily family)
    {
        var result = new List<FileEntryAudit>();
        foreach (var entry in entries)
            result.Add(CreateEntry(entry, family));
        return result;
    }

    private static FileEntryAudit CreateEntry(
        FileSystemEntry entry,
        GWGUI.App.Enums.Explorer.ExplorerFileSystemFamily family)
    {
        var item = new ExplorerContentItem(entry, family);
        var content = entry.Content?.ToArray();
        var children = CreateEntries(entry.Children, family);
        var contentHex = content is { Length: > 0 }
            && item.Definition.ContentFormat == GWGUI.App.Enums.Explorer.ExplorerContentFormat.Unknown
                ? Convert.ToHexString(content)
                : null;
        return new FileEntryAudit(
            entry.Name, item.Name, entry.Kind.ToString(), item.TypeText, item.Definition.Category.ToString(),
            item.Definition.ContentFormat.ToString(), item.Definition.TextEncoding.ToString(), item.Definition.ExecutionKind.ToString(),
            item.Definition.PreviewKind.ToString(), entry.Size, entry.OccupiedSize,
            entry.Created, entry.Modified, entry.Accessed, entry.Comment, entry.RawAttributes, entry.StorageReference,
            entry.MetadataValid, entry.DataValid, entry.SyntheticName, entry.NativeTypeId, entry.LinkTarget,
            entry.Attributes, entry.Diagnostics, entry.Metadata,
            content is null ? null : Convert.ToHexString(SHA256.HashData(content)),
            ContentEdgeHex(content, fromEnd: false),
            ContentEdgeHex(content, fromEnd: true),
            contentHex,
            content is not null,
            children);
    }

    private static string? ContentEdgeHex(byte[]? content, bool fromEnd)
    {
        const int edgeLength = 32;
        if (content is not { Length: > 0 }) return null;
        var length = Math.Min(edgeLength, content.Length);
        var offset = fromEnd ? content.Length - length : 0;
        return Convert.ToHexString(content.AsSpan(offset, length));
    }

    private static async Task<(string? Start, string? End)> ReadContentEdgesAsync(string path)
    {
        const int edgeLength = 32;
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, edgeLength, FileOptions.Asynchronous | FileOptions.SequentialScan);
        if (stream.Length == 0) return (null, null);
        var length = (int)Math.Min(edgeLength, stream.Length);
        var start = new byte[length];
        await stream.ReadExactlyAsync(start);
        stream.Seek(-length, SeekOrigin.End);
        var end = new byte[length];
        await stream.ReadExactlyAsync(end);
        return (Convert.ToHexString(start), Convert.ToHexString(end));
    }

    private static object CreateRepresentation(MediaImageDocument document) => document.Representation switch
    {
        SectorMediaImageRepresentation sectors => new
        {
            kind = "sectors", sectors.Image.FormatId, sectors.Image.BlockSize, sectors.Image.BlockCount,
            sectors.Image.Capacity, sectors.Image.Cylinders, sectors.Image.Heads, sectors.Image.SectorsPerTrack,
            sectors.Image.AllowsVariableBlockSize, sectors.Image.MissingBlocks,
            blocks = sectors.Image.AvailableBlocks.OrderBy(block => block.LogicalBlock).Select(block => new
            {
                block.LogicalBlock, block.Address.Cylinder, block.Address.Head, block.Address.Number,
                block.IntegrityValid, block.Revolution, block.FormatCode, block.DiagnosticCode,
                dataSha256 = Convert.ToHexString(SHA256.HashData(block.Data.ToArray())),
                tagSha256 = block.Tag is null ? null : Convert.ToHexString(SHA256.HashData(block.Tag.ToArray()))
            }).ToArray()
        },
        FluxMediaImageRepresentation flux => new
        {
            kind = "flux", flux.Image.WriteProtected, flux.RevolutionCount, flux.TransitionCount, flux.Surfaces,
            tracks = flux.Tracks.Select(track => new
            {
                track.Cylinder, track.Head,
                bits = track.Bits is null ? null : string.Concat(track.Bits.Select(bit => bit ? '1' : '0')),
                track.Timing, track.Structures, track.Features,
                revolutions = track.Revolutions.Select(revolution => new
                {
                    revolution.ResolutionNanoseconds, revolution.Flux.IndexTimeTicks, revolution.Flux.FluxIntervals
                }).ToArray()
            }).ToArray()
        },
        SequentialMediaImageRepresentation sequential => new
        {
            kind = "sequential", sequential.LogicalLength, sequential.Duration, sequential.Faces,
            sequential.Tracks, sequential.Channels, sequential.Segments
        },
        BlockMediaImageRepresentation blocks => new
        {
            kind = "blocks", blocks.LogicalLength, blocks.Capacity, blocks.LogicalBlockSize,
            blocks.LogicalBlockCount, blocks.Geometry,
            ranges = blocks.Ranges.Select(range => new { range.Address, range.Length, kind = range.Kind.ToString(), range.SourceOffset }).ToArray()
        },
        OpticalMediaImageRepresentation optical => new
        {
            kind = "optical", optical.LogicalLength, optical.Sessions, optical.LayerCount, optical.FaceCount, optical.AssociatedFiles,
            tracks = optical.Tracks?.Select(track => new
            {
                track.SessionNumber, track.TrackNumber, mode = track.Mode.ToString(), track.FirstSector, track.SectorCount,
                track.StoredSectorSize, track.UserDataOffset, track.UserDataLength, track.SourceOffset,
                track.Indexes, track.PregapSectors, track.PostgapSectors, track.Flags, track.CatalogNumber, track.Isrc,
                track.HasSubchannels, track.SubchannelSourceOffset, track.SubchannelBytesPerSector,
                track.StoredPregapSectors, track.StoredPregapSourceOffset, track.SubchannelStride
            }).ToArray()
        },
        _ => new { kind = document.Representation.RepresentationKind.ToString(), document.Representation.LogicalLength }
    };

    private static (List<string> Errors, List<string> Warnings) Validate(
        MediaImageDocument document,
        MediaRecognitionResult recognized,
        ExploredMediaImage explored,
        int visualizationElementCount,
        IReadOnlyList<VolumeAudit> volumes,
        int logicalFileCount)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        if (!recognized.Reader.SupportsFormatId(document.FormatId)) errors.Add("The selected reader does not declare the recognized format.");
        if (!recognized.Reader.Extensions.Contains(Path.GetExtension(document.Source.PrimaryPath), StringComparer.OrdinalIgnoreCase))
            warnings.Add("The selected reader does not declare the source extension; the media content was recognized independently of its file name.");
        if (visualizationElementCount == 0) errors.Add("The visualizer produced no media element.");
        if (explored.Volumes.Count == 0) errors.Add("The explorer produced no volume.");
        if (logicalFileCount == 0) errors.Add("No logical file was extracted from the media.");
        foreach (var entry in volumes.SelectMany(volume => Flatten(volume.Entries)))
        {
            if (!entry.MetadataValid) warnings.Add($"Invalid metadata recorded for '{entry.DisplayName}'.");
            if (entry.DataValid == false) warnings.Add($"Invalid data recorded for '{entry.DisplayName}'.");
            if (entry.Kind == FileSystemEntryKind.File.ToString() && !entry.ContentExtracted)
                errors.Add($"No content was extracted for '{entry.DisplayName}'.");
        }
        return (errors.Distinct(StringComparer.Ordinal).ToList(), warnings.Distinct(StringComparer.Ordinal).ToList());
    }

    private static IEnumerable<FileEntryAudit> Flatten(IEnumerable<FileEntryAudit> entries)
    {
        foreach (var entry in entries)
        {
            yield return entry;
            foreach (var child in Flatten(entry.Children)) yield return child;
        }
    }

    private static async Task<string> HashFileAsync(string sourcePath)
    {
        await using var source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, FileOptions.SequentialScan);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[1024 * 1024];
        int read;
        while ((read = await source.ReadAsync(buffer)) > 0)
            hash.AppendData(buffer, 0, read);
        return Convert.ToHexString(hash.GetHashAndReset());
    }

    private static async Task WriteJsonAsync(string path, object value)
        => await File.WriteAllTextAsync(path, JsonSerializer.Serialize(value, JsonOptions), new UTF8Encoding(false));

    private static IReadOnlyList<string> ListMediaImages(MediaEngineComposition engine, string root)
    {
        if (!Directory.Exists(root)) throw new DirectoryNotFoundException($"Media root not found: {root}");
        var extensions = engine.Recognition.Readers.SelectMany(reader => reader.Extensions)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var paths = Directory.EnumerateFiles(root, "*", new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = false,
                ReturnSpecialDirectories = false
            })
            .Where(path => extensions.Contains(Path.GetExtension(path)))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var associatedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in paths)
        {
            var extension = Path.GetExtension(path);
            if (extension.Equals(".cue", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var line in File.ReadLines(path))
                {
                    var match = CueFileReference().Match(line);
                    if (match.Success)
                        associatedPaths.Add(Path.GetFullPath(match.Groups[1].Value, Path.GetDirectoryName(path)!));
                }
            }
            else if (extension.Equals(".ccd", StringComparison.OrdinalIgnoreCase))
            {
                associatedPaths.Add(Path.ChangeExtension(path, ".img"));
                associatedPaths.Add(Path.ChangeExtension(path, ".sub"));
            }
            else if (extension.Equals(".mds", StringComparison.OrdinalIgnoreCase))
            {
                associatedPaths.Add(Path.ChangeExtension(path, ".mdf"));
            }
        }

        return paths
            .Where(path => !associatedPaths.Contains(path))
            .Where(path => !IsNonMediaBinaryCollection(path))
            .ToArray();
    }

    private static bool IsNonMediaBinaryCollection(string path) =>
        Path.GetExtension(path).Equals(".bin", StringComparison.OrdinalIgnoreCase)
        && BracketValue().Matches(path).Any(match =>
            match.Groups[1].Value.Equals("BIN", StringComparison.OrdinalIgnoreCase));

    private static string? Argument(IReadOnlyList<string> args, string name)
    {
        for (var index = 0; index < args.Count - 1; index++)
            if (args[index].Equals(name, StringComparison.OrdinalIgnoreCase)) return args[index + 1];
        return null;
    }

    private static string? ExpectedFormatHint(string path)
    {
        var matches = BracketValue().Matches(path);
        return matches.Count == 0 ? null : string.Join(" / ", matches.Select(match => match.Groups[1].Value));
    }

    [GeneratedRegex(@"\[([^\]]+)\]")]
    private static partial Regex BracketValue();

    [GeneratedRegex("^\\s*FILE\\s+\"([^\"]+)\"", RegexOptions.IgnoreCase)]
    private static partial Regex CueFileReference();
}

internal sealed class MediaAuditValidationException(string path, IReadOnlyList<string> errors)
    : Exception($"Media validation failed for '{path}': {string.Join(" | ", errors)}");
