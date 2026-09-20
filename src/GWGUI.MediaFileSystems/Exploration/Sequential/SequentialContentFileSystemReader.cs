using System.Globalization;
using MediaVolumeDescriptor = global::GWGUI.MediaFileSystems.Contracts.MediaVolumeDescriptor;
using IMediaImageDocument = global::GWGUI.MediaFileSystems.Interfaces.IMediaImageDocument;
using GWGUI.MediaFileSystems.Definitions;

namespace GWGUI.MediaFileSystems.Exploration.Sequential;

/// <summary>Construit les fichiers nommés d'une bande à partir de blocs déjà décodés.</summary>
public sealed class SequentialContentFileSystemReader
{
    private const string FileNameKey = "fileName";
    private const string BlockNumberKey = "blockNumber";

    public string Id => FileSystemIds.SequentialContent;

    public FileSystemVolume Read(
        IMediaImageDocument document,
        MediaVolumeDescriptor volume,
        IMediaSequentialContent content,
        string volumeName)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(volume);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(volumeName);

        var warnings = new List<string>(content.Diagnostics);
        var entries = new List<FileSystemEntry>();
        var current = new List<MediaSequentialDecodedBlock>();
        string? currentName = null;
        int? previousNumber = null;
        var anonymousCount = 0;

        void Flush()
        {
            if (current.Count == 0 || currentName is null) return;
            var data = new List<byte>();
            foreach (var block in current) data.AddRange(block.Data.ToArray());
            var integrity = current.Any(block => block.IntegrityValid == false)
                ? false
                : current.All(block => block.IntegrityValid == true) ? true : (bool?)null;
            var first = current[0];
            first.Metadata.TryGetValue("fileType", out var nativeTypeId);
            entries.Add(new FileSystemEntry(
                currentName,
                FileSystemEntryKind.File,
                data.Count,
                null,
                string.Empty,
                0,
                first.Position > int.MaxValue ? int.MaxValue : checked((int)first.Position),
                true,
                [],
                data,
                nativeTypeId: nativeTypeId,
                dataValid: integrity,
                diagnostics: integrity == false ? ["The decoded file has an invalid block."] : [],
                metadata: first.Metadata));
            current.Clear();
            currentName = null;
            previousNumber = null;
        }

        foreach (var block in content.Blocks)
        {
            if (!block.Metadata.TryGetValue(FileNameKey, out var storedName) || string.IsNullOrWhiteSpace(storedName))
            {
                Flush();
                anonymousCount++;
                continue;
            }

            var name = storedName.Trim();
            int? number = block.Metadata.TryGetValue(BlockNumberKey, out var rawNumber)
                && int.TryParse(rawNumber, NumberStyles.None, CultureInfo.InvariantCulture, out var parsedNumber)
                    ? parsedNumber : null;
            if (current.Count > 0 && (!string.Equals(currentName, name, StringComparison.Ordinal)
                || number is null || previousNumber is null || number != previousNumber + 1))
                Flush();

            currentName = name;
            current.Add(block);
            previousNumber = number;
        }
        Flush();

        if (anonymousCount > 0)
            warnings.Add($"{anonymousCount} decoded blocks have no stored file name.");
        if (content.Blocks.Any(block => block.IntegrityValid == false))
            warnings.Add("One or more decoded blocks failed their integrity check.");
        if (entries.Count == 0)
            warnings.Add("No named file is available for exploration.");

        return new FileSystemVolume(
            volumeName,
            Id,
            volume.Length,
            0,
            null,
            null,
            entries,
            warnings.Distinct(StringComparer.Ordinal),
            freeSpaceKnown: false,
            attributes: content.DecoderId is null ? [document.FormatId] : [document.FormatId, content.DecoderId]);
    }
}
