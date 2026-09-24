using System.IO;
using GWGUI.MediaAnalysis.Enums;
using GWGUI.MediaAnalysis.Functions;
using GWGUI.MediaFileSystems.Contracts;

namespace GWGUI.MediaFileSystems.Exploration;

/// <summary>Complète les entrées déjà extraites avec les identifiants fournis par MediaAnalysis.</summary>
public static class FileSystemEntryAnalyzer
{
    private static readonly MediaContentClassifier Classifier = new();

    public static IReadOnlyList<FileSystemEntry> AnalyzeEntries(string formatId, FileSystemVolume volume)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(formatId);
        ArgumentNullException.ThrowIfNull(volume);
        var family = MediaFileSystemFamilyResolver.Resolve(formatId, volume.FileSystemId);
        return volume.Entries.Select(entry => AnalyzeEntry(entry, family)).ToArray();
    }

    private static FileSystemEntry AnalyzeEntry(FileSystemEntry entry, MediaFileSystemFamily family)
    {
        var kind = entry.Kind switch
        {
            FileSystemEntryKind.Directory => MediaEntryKind.Directory,
            FileSystemEntryKind.File => MediaEntryKind.File,
            FileSystemEntryKind.Link => MediaEntryKind.Link,
            _ => MediaEntryKind.Unknown
        };
        var type = Classifier.Classify(Path.GetExtension(entry.Name), kind, entry.NativeTypeId,
            entry.Comment, entry.DataValid, entry.Content, entry.Metadata, family)
            ?? throw new InvalidOperationException("The media content classifier did not return an entry type.");
        return entry with
        {
            Children = Array.AsReadOnly(entry.Children.Select(child => AnalyzeEntry(child, family)).ToArray()),
            Analysis = new FileSystemEntryAnalysis(
                type.Category.ToString(), type.IconId, type.TypeResourceKey, type.Extension,
                type.ExecutionKind.ToString(), type.ContentFormat.ToString(),
                type.TextEncoding.ToString(), type.PreviewKind.ToString())
        };
    }
}
