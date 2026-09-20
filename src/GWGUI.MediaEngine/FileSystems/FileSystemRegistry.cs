using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using GWGUI.MediaEngine.Images.Models.Sectors;
using FileSystemRegistryExceptions = GWGUI.MediaFileSystems.FileSystemRegistryExceptions;

namespace GWGUI.MediaEngine.FileSystems;

/// <summary>Parcours sectoriel utilisé pour inspecter les images SCP.</summary>
public sealed class FileSystemRegistry
{
    private readonly FrozenDictionary<string, IFileSystemReader> readersById;
    private readonly FrozenDictionary<string, IReadOnlyList<IFileSystemReader>> readersByFormatId;

    public FileSystemRegistry() : this(MediaFileSystemsReaderAdapter.CreateDefaultCatalog()) { }

    public FileSystemRegistry(IEnumerable<IFileSystemReader> readers)
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

    public FileSystemReadReport ReadAll(SectorImage image) => ReadCandidates(image, Readers);

    public FileSystemReadReport ReadCandidates(SectorImage image, string? readerOrFormatId)
    {
        if (readerOrFormatId is null) return ReadAll(image);
        if (readersById.TryGetValue(readerOrFormatId, out var reader)) return ReadCandidates(image, [reader]);
        return readersByFormatId.TryGetValue(readerOrFormatId, out var formatReaders)
            ? ReadCandidates(image, formatReaders)
            : new([], []);
    }

    public bool TryRead(SectorImage image, string? readerOrFormatId, [NotNullWhen(true)] out FileSystemMatch? match)
    {
        var report = ReadCandidates(image, readerOrFormatId);
        match = report.Matches.FirstOrDefault();
        return match is not null;
    }

    private static FileSystemReadReport ReadCandidates(SectorImage image, IEnumerable<IFileSystemReader> readers)
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
