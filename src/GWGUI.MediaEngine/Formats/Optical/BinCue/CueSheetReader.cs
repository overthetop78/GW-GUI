using System.Globalization;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;

namespace GWGUI.MediaEngine.Formats.Optical.BinCue;

/// <summary>Parses the supported CUE commands without resolving or reading referenced track files.</summary>
public sealed class CueSheetReader
{
    public async Task<CueSheetDocument> ReadAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var lines = await File.ReadAllLinesAsync(path, cancellationToken).ConfigureAwait(false);
        return Parse(lines);
    }

    public CueSheetDocument Parse(IEnumerable<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        var files = new List<FileBuilder>();
        var sheetMetadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        FileBuilder? currentFile = null;
        TrackBuilder? currentTrack = null;
        string? catalog = null;
        var lineNumber = 0;

        foreach (var sourceLine in lines)
        {
            lineNumber++;
            var line = sourceLine.Trim();
            if (line.Length == 0) continue;
            var separator = line.IndexOfAny([' ', '\t']);
            var command = (separator < 0 ? line : line[..separator]).ToUpperInvariant();
            var argument = separator < 0 ? string.Empty : line[(separator + 1)..].Trim();
            try
            {
                switch (command)
                {
                    case CueSheetConstants.File:
                        (var declaredPath, var kind) = ParseFile(argument);
                        currentFile = new FileBuilder(declaredPath, kind);
                        files.Add(currentFile);
                        currentTrack = null;
                        break;
                    case CueSheetConstants.Track:
                        if (currentFile is null) throw new InvalidDataException("TRACK appears before FILE.");
                        currentTrack = ParseTrack(argument);
                        currentFile.Tracks.Add(currentTrack);
                        break;
                    case CueSheetConstants.Index:
                        RequireTrack(currentTrack).Indexes.Add(ParseIndex(argument));
                        break;
                    case CueSheetConstants.Pregap:
                        RequireTrack(currentTrack).PregapSectors = ParseTime(argument);
                        break;
                    case CueSheetConstants.Postgap:
                        RequireTrack(currentTrack).PostgapSectors = ParseTime(argument);
                        break;
                    case CueSheetConstants.Flags:
                        RequireTrack(currentTrack).Flags.AddRange(SplitTokens(argument));
                        break;
                    case CueSheetConstants.Isrc:
                        RequireTrack(currentTrack).Isrc = RequireValue(argument, command);
                        break;
                    case CueSheetConstants.Catalog:
                        if (currentTrack is not null) throw new InvalidDataException("CATALOG must be declared before tracks.");
                        catalog = RequireValue(argument, command);
                        break;
                    case CueSheetConstants.Title:
                    case CueSheetConstants.Performer:
                    case CueSheetConstants.Songwriter:
                        AddMetadata(currentTrack?.Metadata ?? sheetMetadata, command, ParseText(argument));
                        break;
                    case CueSheetConstants.Rem:
                        ParseRemark(argument, currentTrack?.Metadata ?? sheetMetadata);
                        break;
                    default:
                        throw new NotSupportedException($"Unsupported CUE command '{command}'.");
                }
            }
            catch (Exception exception) when (exception is FormatException or InvalidDataException or NotSupportedException)
            {
                throw new InvalidDataException($"Invalid CUE line {lineNumber}: {sourceLine}", exception);
            }
        }

        if (files.Count == 0) throw new InvalidDataException("The CUE sheet does not contain a FILE declaration.");
        return new CueSheetDocument(files.Select(file => file.Build()).ToArray(), catalog, sheetMetadata);
    }

    private static (string Path, CueFileKind Kind) ParseFile(string argument)
    {
        var tokens = SplitTokens(argument);
        if (tokens.Count < 2) throw new InvalidDataException("FILE requires a path and a type.");
        var type = tokens[^1].ToUpperInvariant();
        var kind = type switch
        {
            CueSheetConstants.BinaryFile => CueFileKind.Binary,
            CueSheetConstants.WaveFile => CueFileKind.WavePcm,
            _ => throw new NotSupportedException($"Unsupported CUE file type '{type}'.")
        };
        var typeOffset = argument.LastIndexOf(tokens[^1], StringComparison.OrdinalIgnoreCase);
        var path = ParseText(argument[..typeOffset].Trim());
        return (RequireValue(path, CueSheetConstants.File), kind);
    }

    private static TrackBuilder ParseTrack(string argument)
    {
        var tokens = SplitTokens(argument);
        if (tokens.Count != 2
            || !int.TryParse(tokens[0], NumberStyles.None, CultureInfo.InvariantCulture, out var number)
            || number <= 0)
            throw new InvalidDataException("TRACK requires a positive number and a supported mode.");
        var mode = tokens[1].ToUpperInvariant() switch
        {
            CueSheetConstants.AudioTrack => OpticalTrackMode.Audio,
            CueSheetConstants.Mode1Data2048 => OpticalTrackMode.Mode1Data2048,
            CueSheetConstants.Mode1Raw2352 => OpticalTrackMode.Mode1Raw2352,
            CueSheetConstants.Mode2Data2336 => OpticalTrackMode.Mode2Data2336,
            CueSheetConstants.Mode2Raw2352 => OpticalTrackMode.Mode2Raw2352,
            _ => throw new NotSupportedException($"Unsupported CUE track mode '{tokens[1]}'.")
        };
        return new TrackBuilder(number, mode);
    }

    private static OpticalTrackIndex ParseIndex(string argument)
    {
        var tokens = SplitTokens(argument);
        if (tokens.Count != 2
            || !int.TryParse(tokens[0], NumberStyles.None, CultureInfo.InvariantCulture, out var number)
            || number < 0)
            throw new InvalidDataException("INDEX requires a non-negative number and MM:SS:FF position.");
        return new OpticalTrackIndex(number, ParseTime(tokens[1]));
    }

    private static long ParseTime(string value)
    {
        var parts = value.Split(':');
        if (parts.Length != 3
            || !long.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var minutes)
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var seconds)
            || !int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out var frames)
            || minutes < 0
            || seconds is < 0 or >= CueSheetConstants.SecondsPerMinute
            || frames is < 0 or >= CueSheetConstants.FramesPerSecond)
            throw new FormatException("A CUE position must use MM:SS:FF with 75 frames per second.");
        return checked((minutes * CueSheetConstants.SecondsPerMinute + seconds) * CueSheetConstants.FramesPerSecond + frames);
    }

    private static void ParseRemark(string argument, IDictionary<string, string> metadata)
    {
        var separator = argument.IndexOfAny([' ', '\t']);
        if (separator <= 0) throw new InvalidDataException("REM requires a name and value.");
        var name = argument[..separator].ToUpperInvariant();
        AddMetadata(metadata, $"{CueSheetConstants.Rem}:{name}", ParseText(argument[(separator + 1)..].Trim()));
    }

    private static void AddMetadata(IDictionary<string, string> metadata, string key, string value)
    {
        if (!metadata.TryAdd(key, value))
            throw new InvalidDataException($"Duplicate CUE metadata '{key}'.");
    }

    private static TrackBuilder RequireTrack(TrackBuilder? track) =>
        track ?? throw new InvalidDataException("The command requires a preceding TRACK declaration.");

    private static string RequireValue(string value, string command)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new InvalidDataException($"{command} requires a value.");
        return value;
    }

    private static string ParseText(string value)
    {
        value = value.Trim();
        if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
            return value[1..^1].Replace("\"\"", "\"", StringComparison.Ordinal);
        if (value.Contains('"')) throw new InvalidDataException("Unbalanced quotes in CUE text.");
        return value;
    }

    private static IReadOnlyList<string> SplitTokens(string value)
    {
        var tokens = new List<string>();
        var current = new System.Text.StringBuilder();
        var quoted = false;
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (character == '"')
            {
                if (quoted && index + 1 < value.Length && value[index + 1] == '"')
                {
                    current.Append('"');
                    index++;
                }
                else
                {
                    quoted = !quoted;
                }
                continue;
            }
            if (!quoted && char.IsWhiteSpace(character))
            {
                if (current.Length > 0)
                {
                    tokens.Add(current.ToString());
                    current.Clear();
                }
                continue;
            }
            current.Append(character);
        }
        if (quoted) throw new InvalidDataException("Unbalanced quotes in CUE command.");
        if (current.Length > 0) tokens.Add(current.ToString());
        return tokens;
    }

    private sealed class FileBuilder(string declaredPath, CueFileKind kind)
    {
        public string DeclaredPath { get; } = declaredPath;
        public CueFileKind Kind { get; } = kind;
        public List<TrackBuilder> Tracks { get; } = [];

        public CueFileDescriptor Build() =>
            new(DeclaredPath, Kind, Tracks.Select(track => track.Build()).ToArray());
    }

    private sealed class TrackBuilder(int number, OpticalTrackMode mode)
    {
        public int Number { get; } = number;
        public OpticalTrackMode Mode { get; } = mode;
        public List<OpticalTrackIndex> Indexes { get; } = [];
        public long PregapSectors { get; set; }
        public long PostgapSectors { get; set; }
        public List<string> Flags { get; } = [];
        public string? Isrc { get; set; }
        public Dictionary<string, string> Metadata { get; } = new(StringComparer.OrdinalIgnoreCase);

        public CueTrackDeclaration Build() =>
            new(Number, Mode, Indexes, PregapSectors, PostgapSectors, Flags, Isrc, Metadata);
    }
}
