using System.Globalization;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;

namespace GWGUI.MediaEngine.Images.Formats.Optical.BinCue;

/// <summary>Writes the supported CUE commands as UTF-8 without changing their declared structure.</summary>
public sealed class CueSheetWriter
{
    public async Task WriteAsync(
        CueSheetDocument sheet,
        Stream destination,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sheet);
        ArgumentNullException.ThrowIfNull(destination);
        if (!destination.CanWrite) throw new ArgumentException("The destination stream is not writable.", nameof(destination));

        await using var writer = new StreamWriter(destination, CueSheetConstants.Utf8WithoutBom, leaveOpen: true);
        if (sheet.CatalogNumber is not null)
            await WriteLineAsync(writer, $"{CueSheetConstants.Catalog} {sheet.CatalogNumber}", cancellationToken).ConfigureAwait(false);
        await WriteMetadataAsync(writer, sheet.Metadata, string.Empty, cancellationToken).ConfigureAwait(false);

        foreach (var file in sheet.Files)
        {
            var type = file.Kind switch
            {
                CueFileKind.Binary => CueSheetConstants.BinaryFile,
                CueFileKind.WavePcm => CueSheetConstants.WaveFile,
                _ => throw new NotSupportedException($"Unsupported CUE file type '{file.Kind}'.")
            };
            await WriteLineAsync(
                writer,
                $"{CueSheetConstants.File} {Quote(file.DeclaredPath)} {type}",
                cancellationToken).ConfigureAwait(false);

            foreach (var track in file.Tracks)
            {
                var mode = FormatMode(track.Mode);
                await WriteLineAsync(
                    writer,
                    $"{CueSheetConstants.Indent}{CueSheetConstants.Track} {track.Number:D2} {mode}",
                    cancellationToken).ConfigureAwait(false);
                await WriteMetadataAsync(
                    writer,
                    track.Metadata,
                    CueSheetConstants.Indent + CueSheetConstants.Indent,
                    cancellationToken).ConfigureAwait(false);
                if (track.Flags.Count > 0)
                    await WriteLineAsync(
                        writer,
                        $"{CueSheetConstants.Indent}{CueSheetConstants.Indent}{CueSheetConstants.Flags} {string.Join(' ', track.Flags)}",
                        cancellationToken).ConfigureAwait(false);
                if (track.Isrc is not null)
                    await WriteLineAsync(
                        writer,
                        $"{CueSheetConstants.Indent}{CueSheetConstants.Indent}{CueSheetConstants.Isrc} {track.Isrc}",
                        cancellationToken).ConfigureAwait(false);
                if (track.PregapSectors > 0)
                    await WriteLineAsync(
                        writer,
                        $"{CueSheetConstants.Indent}{CueSheetConstants.Indent}{CueSheetConstants.Pregap} {FormatTime(track.PregapSectors)}",
                        cancellationToken).ConfigureAwait(false);
                foreach (var index in track.Indexes.OrderBy(item => item.Number))
                {
                    if (index.RelativeSector < 0)
                        throw new InvalidDataException("A CUE INDEX position cannot be negative in an output sheet.");
                    await WriteLineAsync(
                        writer,
                        $"{CueSheetConstants.Indent}{CueSheetConstants.Indent}{CueSheetConstants.Index} {index.Number:D2} {FormatTime(index.RelativeSector)}",
                        cancellationToken).ConfigureAwait(false);
                }
                if (track.PostgapSectors > 0)
                    await WriteLineAsync(
                        writer,
                        $"{CueSheetConstants.Indent}{CueSheetConstants.Indent}{CueSheetConstants.Postgap} {FormatTime(track.PostgapSectors)}",
                        cancellationToken).ConfigureAwait(false);
            }
        }

        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task WriteMetadataAsync(
        StreamWriter writer,
        IReadOnlyDictionary<string, string> metadata,
        string indent,
        CancellationToken cancellationToken)
    {
        foreach (var pair in metadata.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            var command = pair.Key.StartsWith(CueSheetConstants.Rem + ":", StringComparison.OrdinalIgnoreCase)
                ? $"{CueSheetConstants.Rem} {pair.Key[(CueSheetConstants.Rem.Length + 1)..]}"
                : pair.Key.ToUpperInvariant();
            if (command is not CueSheetConstants.Title
                and not CueSheetConstants.Performer
                and not CueSheetConstants.Songwriter
                && !command.StartsWith(CueSheetConstants.Rem + " ", StringComparison.Ordinal))
                throw new NotSupportedException($"Unsupported CUE metadata '{pair.Key}'.");
            await WriteLineAsync(writer, $"{indent}{command} {Quote(pair.Value)}", cancellationToken).ConfigureAwait(false);
        }
    }

    private static Task WriteLineAsync(StreamWriter writer, string value, CancellationToken cancellationToken) =>
        writer.WriteLineAsync(value.AsMemory(), cancellationToken);

    private static string FormatMode(OpticalTrackMode mode) => mode switch
    {
        OpticalTrackMode.Audio => CueSheetConstants.AudioTrack,
        OpticalTrackMode.Mode1Data2048 => CueSheetConstants.Mode1Data2048,
        OpticalTrackMode.Mode1Raw2352 => CueSheetConstants.Mode1Raw2352,
        OpticalTrackMode.Mode2Data2336 => CueSheetConstants.Mode2Data2336,
        OpticalTrackMode.Mode2Raw2352 => CueSheetConstants.Mode2Raw2352,
        _ => throw new NotSupportedException($"Unsupported CUE track mode '{mode}'.")
    };

    private static string FormatTime(long sectors)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(sectors);
        var minutes = sectors / (CueSheetConstants.SecondsPerMinute * CueSheetConstants.FramesPerSecond);
        var remainder = sectors % (CueSheetConstants.SecondsPerMinute * CueSheetConstants.FramesPerSecond);
        var seconds = remainder / CueSheetConstants.FramesPerSecond;
        var frames = remainder % CueSheetConstants.FramesPerSecond;
        return string.Create(CultureInfo.InvariantCulture, $"{minutes:D2}:{seconds:D2}:{frames:D2}");
    }

    private static string Quote(string value) => $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
