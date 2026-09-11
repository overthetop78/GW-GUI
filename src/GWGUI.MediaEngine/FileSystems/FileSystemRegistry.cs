using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Interfaces.Exploration;

using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.FileSystems;

/// <summary>Indexe les lecteurs et exécute tous les candidats pertinents dans leur ordre public.</summary>
public sealed class FileSystemRegistry
{
    private readonly FrozenDictionary<string, IFileSystemReader> readersById;
    private readonly FrozenDictionary<string, IReadOnlyList<IFileSystemReader>> readersByFormatId;
    private readonly FrozenDictionary<MediaRepresentationKind, IReadOnlyList<IMediaFileSystemReader>> readersByRepresentation;

    /// <summary>Crée le registre à partir du catalogue par défaut.</summary>
    public FileSystemRegistry() : this(FileSystemReaderCatalog.CreateDefault(), []) { }

    /// <summary>Crée le registre à partir d'une collection ordonnée de lecteurs.</summary>
    public FileSystemRegistry(IEnumerable<IFileSystemReader> readers)
        : this(readers, [])
    {
    }

    /// <summary>Creates a registry from existing sector readers and representation-independent media readers.</summary>
    public FileSystemRegistry(
        IEnumerable<IFileSystemReader> readers,
        IEnumerable<IMediaFileSystemReader> mediaReaders)
    {
        ArgumentNullException.ThrowIfNull(readers);
        ArgumentNullException.ThrowIfNull(mediaReaders);
        var copied = readers.ToArray();
        for (var index = 0; index < copied.Length; index++)
        {
            if (copied[index] is null) throw FileSystemRegistryExceptions.NullReader(index);
            if (string.IsNullOrWhiteSpace(copied[index].Id)) throw FileSystemRegistryExceptions.EmptyReaderId(index);
        }
        var additional = mediaReaders.ToArray();
        for (var index = 0; index < additional.Length; index++)
        {
            if (additional[index] is null) throw FileSystemRegistryExceptions.NullReader(copied.Length + index);
            if (string.IsNullOrWhiteSpace(additional[index].Id)) throw FileSystemRegistryExceptions.EmptyReaderId(copied.Length + index);
        }
        var allMediaReaders = copied.Cast<IMediaFileSystemReader>().Concat(additional).ToArray();
        var duplicate = allMediaReaders.GroupBy(reader => reader.Id, StringComparer.OrdinalIgnoreCase).FirstOrDefault(group => group.Skip(1).Any());
        if (duplicate is not null) throw FileSystemRegistryExceptions.DuplicateReaderId(duplicate.Key);
        Readers = Array.AsReadOnly(copied);
        MediaReaders = Array.AsReadOnly(allMediaReaders);
        readersById = copied.ToFrozenDictionary(reader => reader.Id, StringComparer.OrdinalIgnoreCase);
        readersByFormatId = copied.SelectMany(reader => reader.CatalogFormatIds.Select(formatId => (formatId, reader))).GroupBy(item => item.formatId, StringComparer.OrdinalIgnoreCase).ToFrozenDictionary(group => group.Key, group => (IReadOnlyList<IFileSystemReader>)Array.AsReadOnly(group.Select(item => item.reader).ToArray()), StringComparer.OrdinalIgnoreCase);
        readersByRepresentation = allMediaReaders
            .SelectMany(reader => reader.RepresentationKinds.Select(kind => (kind, reader)))
            .GroupBy(item => item.kind)
            .ToFrozenDictionary(
                group => group.Key,
                group => (IReadOnlyList<IMediaFileSystemReader>)Array.AsReadOnly(group.Select(item => item.reader).ToArray()));
        SupportedFormatIds = readersByFormatId.Keys.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Lecteurs copiés et exposés dans l'ordre du catalogue.</summary>
    public IReadOnlyList<IFileSystemReader> Readers { get; }
    /// <summary>All readers available to representation-independent media exploration.</summary>
    public IReadOnlyList<IMediaFileSystemReader> MediaReaders { get; }
    /// <summary>Ensemble immuable des formats de catalogue déclarés.</summary>
    public IReadOnlySet<string> SupportedFormatIds { get; }

    /// <summary>Explores a media volume with readers compatible with its actual representation.</summary>
    public ExploredMediaVolume Explore(MediaImageDocument document, MediaVolumeDescriptor volume)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(volume);
        if (!readersByRepresentation.TryGetValue(document.Representation.RepresentationKind, out var candidates))
            return new(volume, null, null, []);

        var ordered = string.IsNullOrWhiteSpace(volume.FileSystemId)
            ? candidates
            : candidates.OrderByDescending(reader => reader.Id.Equals(volume.FileSystemId, StringComparison.OrdinalIgnoreCase)).ToArray();
        var diagnostics = new List<string>();
        foreach (var reader in ordered)
        {
            if (!reader.CanRead(document, volume)) continue;
            try
            {
                return new(volume, reader.Id, reader.Read(document, volume), diagnostics);
            }
            catch (InvalidDataException exception)
            {
                diagnostics.Add($"{reader.Id}: {exception.Message}");
            }
        }

        return new(volume, null, null, diagnostics);
    }

    /// <summary>Lit tous les lecteurs qui reconnaissent l'image.</summary>
    public FileSystemReadReport ReadAll(SectorImage image) => ReadCandidates(image, Readers);

    /// <summary>Lit les candidats associés à un identifiant de lecteur ou de format ; tous les lecteurs sont considérés lorsque l'identifiant est absent.</summary>
    public FileSystemReadReport ReadCandidates(SectorImage image, string? readerOrFormatId)
    {
        if (readerOrFormatId is null) return ReadAll(image);
        if (readersById.TryGetValue(readerOrFormatId, out var reader)) return ReadCandidates(image, [reader]);
        return readersByFormatId.TryGetValue(readerOrFormatId, out var formatReaders) ? ReadCandidates(image, formatReaders) : new([], []);
    }

    /// <summary>Tente chaque candidat jusqu'à la première lecture réussie.</summary>
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
