using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using GWGUI.MediaFileSystems.Interfaces;
using GWGUI.MediaFileSystems.Exploration.Interpretation;

namespace GWGUI.MediaFileSystems.Exploration;

/// <summary>Interroge les lecteurs sectoriels dans l’ordre du catalogue.</summary>
public sealed class SectorFileSystemRegistry
{
    private readonly FrozenDictionary<string, IFileSystemReader> readersById;
    private readonly FrozenDictionary<string, IReadOnlyList<IFileSystemReader>> readersByFormatId;

    public SectorFileSystemRegistry() : this(FileSystemReaderCatalog.CreateDefault()) { }

    public SectorFileSystemRegistry(IEnumerable<IFileSystemReader> readers)
    {
        ArgumentNullException.ThrowIfNull(readers);
        var copied = readers.ToArray();
        for (var index = 0; index < copied.Length; index++)
        {
            if (copied[index] is null) throw FileSystemRegistryExceptions.NullReader(index);
            if (string.IsNullOrWhiteSpace(copied[index].Id)) throw FileSystemRegistryExceptions.EmptyReaderId(index);
        }
        var duplicate = copied.GroupBy(reader => reader.Id, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Skip(1).Any());
        if (duplicate is not null) throw FileSystemRegistryExceptions.DuplicateReaderId(duplicate.Key);
        Readers = Array.AsReadOnly(copied);
        readersById = copied.ToFrozenDictionary(reader => reader.Id, StringComparer.OrdinalIgnoreCase);
        readersByFormatId = copied
            .SelectMany(reader => reader.CatalogFormatIds.Select(formatId => (formatId, reader)))
            .GroupBy(item => item.formatId, StringComparer.OrdinalIgnoreCase)
            .ToFrozenDictionary(
                group => group.Key,
                group => (IReadOnlyList<IFileSystemReader>)Array.AsReadOnly(group.Select(item => item.reader).ToArray()),
                StringComparer.OrdinalIgnoreCase);
        SupportedFormatIds = readersByFormatId.Keys.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<IFileSystemReader> Readers { get; }
    public IReadOnlySet<string> SupportedFormatIds { get; }

    public FileSystemReadReport ReadAll(IMediaSectorImage image) => ReadCandidates(image, Readers);

    public IReadOnlyList<SectorFileSystemCandidate> ReadDistinctCandidates(IEnumerable<IMediaSectorImage> images)
    {
        ArgumentNullException.ThrowIfNull(images);
        var identities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var candidates = new List<SectorFileSystemCandidate>();
        var primary = true;
        foreach (var image in images)
        {
            ArgumentNullException.ThrowIfNull(image);
            var matches = readersByFormatId.TryGetValue(image.FormatId, out var formatReaders)
                ? ReadCandidates(image, formatReaders).Matches
                : ReadAll(image).Matches;
            foreach (var match in primary ? matches : matches.Take(1))
            {
                if (!identities.Add(FileSystemInterpretationIdentity.Create(image.FormatId, match.Volume))) continue;
                candidates.Add(new(image, match));
            }
            primary = false;
        }
        return candidates;
    }

    public FileSystemReadReport ReadCandidates(IMediaSectorImage image, string? readerOrFormatId)
    {
        if (readerOrFormatId is null) return ReadAll(image);
        if (readersById.TryGetValue(readerOrFormatId, out var reader)) return ReadCandidates(image, [reader]);
        return readersByFormatId.TryGetValue(readerOrFormatId, out var formatReaders)
            ? ReadCandidates(image, formatReaders)
            : new([], []);
    }

    public bool TryRead(IMediaSectorImage image, string? readerOrFormatId, [NotNullWhen(true)] out FileSystemMatch? match)
    {
        var report = ReadCandidates(image, readerOrFormatId);
        match = report.Matches.FirstOrDefault();
        return match is not null;
    }

    private static FileSystemReadReport ReadCandidates(IMediaSectorImage image, IEnumerable<IFileSystemReader> readers)
    {
        var matches = new List<FileSystemMatch>();
        var failures = new List<FileSystemReadFailure>();
        foreach (var reader in readers)
        {
            if (!reader.CanRead(image)) continue;
            try
            {
                matches.Add(new(reader.Id, reader.Read(image)));
            }
            catch (InvalidDataException exception)
            {
                failures.Add(new(reader.Id, exception));
            }
        }
        return new(matches, failures);
    }
}
