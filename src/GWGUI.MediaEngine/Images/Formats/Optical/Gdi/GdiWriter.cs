using System.Collections.Frozen;
using System.Globalization;
using System.IO;
using System.Text;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Optical;
using GWGUI.MediaEngine.Images.Writing;
using GWGUI.MediaEngine.Interfaces.Writing;

namespace GWGUI.MediaEngine.Images.Formats.Optical.Gdi;

/// <summary>Écrit un descripteur GDI et un fichier de données par piste optique.</summary>
public sealed class GdiWriter : IMediaImageWriter
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { GdiConstants.FormatId }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.OpticalTracks }.ToFrozenSet();

    public string Id => MediaImageWriterIds.OpticalGdi;
    public IReadOnlySet<string> FormatIds => SupportedFormatIds;
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentationKinds;
    public IReadOnlySet<string> ProducedFileExtensions => GdiConstants.Extensions;
    public bool ProducesMultipleFiles => true;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (!SupportedFormatIds.Contains(targetFormatId)
            || !GdiConstants.Extensions.Contains(NormalizeExtension(targetExtension))
            || document.MediaKind != MediaKind.Optical
            || document.Representation is not OpticalMediaImageRepresentation { Tracks.Count: > 0 } optical
            || optical.LayerCount is not null
            || optical.FaceCount is not null)
            return false;
        var numbers = new HashSet<int>();
        return optical.Tracks!.All(track =>
            numbers.Add(track.TrackNumber)
            && track.FirstSector >= 0
            && track.StoredPregapSectors == 0
            && !track.HasSubchannels
            && GetTrackSectorSize(track) is > 0
            && track.SectorCount > 0);
    }

    public async Task<IReadOnlyList<string>> WriteAsync(
        MediaImageDocument document,
        string outputPath,
        string targetFormatId,
        CancellationToken cancellationToken = default)
    {
        var descriptorPath = Path.GetFullPath(outputPath);
        if (!CanWrite(document, targetFormatId, Path.GetExtension(descriptorPath))
            || document.Representation is not OpticalMediaImageRepresentation optical)
            throw new InvalidDataException("The optical document cannot be written as a GDI track set.");

        var directory = Path.GetDirectoryName(descriptorPath) ?? Directory.GetCurrentDirectory();
        var baseName = Path.GetFileNameWithoutExtension(descriptorPath);
        var files = new Dictionary<string, Func<Stream, CancellationToken, Task>>(StringComparer.OrdinalIgnoreCase);
        var entries = new List<GdiOutputEntry>(optical.Tracks!.Count);
        foreach (var track in optical.Tracks.OrderBy(track => track.TrackNumber))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var name = $"{baseName}.track{track.TrackNumber:D2}{DiskImageFileExtensions.Bin}";
            var path = Path.Combine(directory, name);
            var sectorSize = GetTrackSectorSize(track);
            var control = track.IsAudio ? GdiConstants.AudioControl : GdiConstants.DataControl;
            if (track.FirstSector > long.MaxValue || track.FirstSector < 0)
                throw new InvalidDataException($"GDI track {track.TrackNumber} has an invalid LBA.");
            files.Add(path, (output, token) => CopyTrackAsync(track, output, sectorSize, token));
            entries.Add(new GdiOutputEntry(track.TrackNumber, track.FirstSector, control, sectorSize, name));
        }
        files.Add(descriptorPath, (output, token) => WriteDescriptorAsync(output, entries, token));
        await AtomicMediaFileSetWriter.WriteAsync(files, cancellationToken).ConfigureAwait(false);
        return files.Keys.ToArray();
    }

    private static async Task WriteDescriptorAsync(
        Stream output,
        IReadOnlyList<GdiOutputEntry> entries,
        CancellationToken cancellationToken)
    {
        await using var writer = new StreamWriter(output, new UTF8Encoding(false), 1024, leaveOpen: true);
        await writer.WriteLineAsync(entries.Count.ToString(CultureInfo.InvariantCulture).AsMemory(), cancellationToken).ConfigureAwait(false);
        foreach (var entry in entries)
        {
            var line = string.Join(
                ' ',
                entry.Number.ToString(CultureInfo.InvariantCulture),
                entry.Lba.ToString(CultureInfo.InvariantCulture),
                entry.Control.ToString(CultureInfo.InvariantCulture),
                entry.SectorSize.ToString(CultureInfo.InvariantCulture),
                Quote(entry.FileName),
                "0");
            await writer.WriteLineAsync(line.AsMemory(), cancellationToken).ConfigureAwait(false);
        }
        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task CopyTrackAsync(
        OpticalTrackDescriptor track,
        Stream output,
        int sectorSize,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[64 * DataSizeConstants.BytesPerKibibyte];
        var total = checked(track.SectorCount * sectorSize);
        long copied = 0;
        while (copied < total)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var count = (int)Math.Min(buffer.Length, total - copied);
            await track.DataSource.ReadExactlyAsync(
                checked(track.SourceOffset + copied),
                buffer.AsMemory(0, count),
                cancellationToken).ConfigureAwait(false);
            await output.WriteAsync(buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
            copied += count;
        }
    }

    private static int GetTrackSectorSize(OpticalTrackDescriptor track) => track.Mode switch
    {
        OpticalTrackMode.Audio when track.StoredSectorSize == GdiConstants.RawSectorSize => GdiConstants.RawSectorSize,
        OpticalTrackMode.Mode1Data2048 when track.StoredSectorSize == GdiConstants.DataSectorSize => GdiConstants.DataSectorSize,
        OpticalTrackMode.Mode1Raw2352 when track.StoredSectorSize == GdiConstants.RawSectorSize => GdiConstants.RawSectorSize,
        _ => 0
    };

    private static string Quote(string value) => $"\"{value.Replace("\"", "\\\"")}\"";

    private static string NormalizeExtension(string extension) =>
        extension.StartsWith(".", StringComparison.Ordinal)
            ? extension.ToLowerInvariant()
            : $".{extension.ToLowerInvariant()}";

    private sealed record GdiOutputEntry(int Number, long Lba, int Control, int SectorSize, string FileName);
}
