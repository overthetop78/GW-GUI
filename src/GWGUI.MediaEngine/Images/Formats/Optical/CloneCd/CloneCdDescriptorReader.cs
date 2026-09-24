using System.Globalization;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;

namespace GWGUI.MediaEngine.Images.Formats.Optical.CloneCd;

/// <summary>Parses the documented CloneCD INI sections used to describe sessions, TOC entries, and tracks.</summary>
public sealed class CloneCdDescriptorReader
{
    public async Task<CloneCdDescriptor> ReadAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var lines = await File.ReadAllLinesAsync(path, cancellationToken).ConfigureAwait(false);
        return Parse(lines);
    }

    public CloneCdDescriptor Parse(IEnumerable<string> lines)
    {
        var sections = ParseSections(lines);
        var header = RequireSection(sections, CloneCdConstants.HeaderSection);
        var version = ReadInteger(header, CloneCdConstants.VersionKey);
        if (!CloneCdFormat.IsSupportedVersion(version))
            throw new NotSupportedException($"Unsupported CloneCD descriptor version {version}.");

        var disc = RequireSection(sections, CloneCdConstants.DiscSection);
        var sessionCount = ReadInteger(disc, CloneCdConstants.SessionsKey);
        var tocEntryCount = ReadInteger(disc, CloneCdConstants.TocEntriesKey);
        if (sessionCount <= 0 || tocEntryCount <= 0)
            throw new InvalidDataException("CloneCD session and TOC entry counts must be positive.");

        var sessionSections = sections
            .Where(pair => pair.Key.StartsWith(CloneCdConstants.SessionSectionPrefix, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(
                pair => ParseSectionNumber(pair.Key, CloneCdConstants.SessionSectionPrefix),
                pair => (IReadOnlyDictionary<string, int>)pair.Value.ToDictionary(
                    property => property.Key,
                    property => ParseInteger(property.Value),
                    StringComparer.OrdinalIgnoreCase));
        if (sessionSections.Count != sessionCount)
            throw new InvalidDataException("The number of CloneCD session sections does not match the Disc declaration.");

        var entries = sections
            .Where(pair => pair.Key.StartsWith(CloneCdConstants.EntrySectionPrefix, StringComparison.OrdinalIgnoreCase))
            .Select(pair => ReadTocEntry(
                ParseSectionNumber(pair.Key, CloneCdConstants.EntrySectionPrefix),
                pair.Value))
            .OrderBy(entry => entry.EntryNumber)
            .ToArray();
        if (entries.Length != tocEntryCount)
            throw new InvalidDataException("The number of CloneCD TOC entry sections does not match the Disc declaration.");

        var tracks = sections
            .Where(pair => pair.Key.StartsWith(CloneCdConstants.TrackSectionPrefix, StringComparison.OrdinalIgnoreCase))
            .Select(pair => ReadTrack(
                ParseSectionNumber(pair.Key, CloneCdConstants.TrackSectionPrefix),
                pair.Value))
            .OrderBy(track => track.Number)
            .ToArray();
        var catalog = disc.TryGetValue(CueSheetConstants.Catalog, out var catalogValue)
            ? catalogValue
            : null;
        return new CloneCdDescriptor(version, sessionCount, catalog, entries, tracks, sessionSections);
    }

    private static Dictionary<string, Dictionary<string, string>> ParseSections(IEnumerable<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        var sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string>? current = null;
        var lineNumber = 0;
        foreach (var sourceLine in lines)
        {
            lineNumber++;
            var line = sourceLine.Trim();
            if (line.Length == 0 || line[0] is ';' or '#') continue;
            if (line[0] == '[' && line[^1] == ']')
            {
                var name = line[1..^1].Trim();
                if (name.Length == 0 || !sections.TryAdd(name, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)))
                    throw new InvalidDataException($"Invalid or duplicate CCD section at line {lineNumber}.");
                current = sections[name];
                continue;
            }
            if (current is null) throw new InvalidDataException($"CCD property before first section at line {lineNumber}.");
            var separator = line.IndexOf('=');
            if (separator <= 0) throw new InvalidDataException($"Invalid CCD property at line {lineNumber}.");
            var key = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim();
            if (!current.TryAdd(key, value))
                throw new InvalidDataException($"Duplicate CCD property '{key}' at line {lineNumber}.");
        }
        return sections;
    }

    private static CloneCdTocEntry ReadTocEntry(int entryNumber, IReadOnlyDictionary<string, string> values) =>
        new(
            entryNumber,
            ReadInteger(values, CloneCdConstants.SessionKey),
            ReadInteger(values, CloneCdConstants.PointKey),
            ReadInteger(values, CloneCdConstants.AdrKey),
            ReadInteger(values, CloneCdConstants.ControlKey),
            ReadInteger(values, CloneCdConstants.TrackNumberKey),
            ReadInteger(values, CloneCdConstants.AbsoluteMinuteKey),
            ReadInteger(values, CloneCdConstants.AbsoluteSecondKey),
            ReadInteger(values, CloneCdConstants.AbsoluteFrameKey),
            ReadInteger(values, CloneCdConstants.AbsoluteLbaKey),
            ReadInteger(values, CloneCdConstants.ZeroKey),
            ReadInteger(values, CloneCdConstants.PointMinuteKey),
            ReadInteger(values, CloneCdConstants.PointSecondKey),
            ReadInteger(values, CloneCdConstants.PointFrameKey),
            ReadInteger(values, CloneCdConstants.AbsoluteSectorKey));

    private static CueTrackDeclaration ReadTrack(int trackNumber, IReadOnlyDictionary<string, string> values)
    {
        var modeValue = ReadInteger(values, CloneCdConstants.ModeKey);
        var mode = modeValue switch
        {
            0 => OpticalTrackMode.Audio,
            1 => OpticalTrackMode.Mode1Raw2352,
            2 => OpticalTrackMode.Mode2Raw2352,
            _ => throw new NotSupportedException($"Unsupported CloneCD track mode {modeValue}.")
        };
        var indexes = values
            .Where(pair => pair.Key.StartsWith(CloneCdConstants.IndexKeyPrefix, StringComparison.OrdinalIgnoreCase))
            .Select(pair => new OpticalTrackIndex(
                ParseSectionNumber(pair.Key, CloneCdConstants.IndexKeyPrefix),
                ParseInteger(pair.Value)))
            .OrderBy(index => index.Number)
            .ToArray();
        var flags = values.TryGetValue(CloneCdConstants.FlagsKey, out var flagsValue)
            ? flagsValue.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : [];
        values.TryGetValue(CloneCdConstants.IsrcKey, out var isrc);
        return new CueTrackDeclaration(trackNumber, mode, indexes, flags: flags, isrc: isrc);
    }

    private static Dictionary<string, string> RequireSection(
        IReadOnlyDictionary<string, Dictionary<string, string>> sections,
        string name) =>
        sections.TryGetValue(name, out var section)
            ? section
            : throw new InvalidDataException($"Required CCD section [{name}] is missing.");

    private static int ReadInteger(IReadOnlyDictionary<string, string> values, string key) =>
        values.TryGetValue(key, out var value)
            ? ParseInteger(value)
            : throw new InvalidDataException($"Required CCD property '{key}' is missing.");

    private static int ParseInteger(string value)
    {
        var sign = 1;
        var text = value.Trim();
        if (text.StartsWith('-'))
        {
            sign = -1;
            text = text[1..];
        }
        var hex = text.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
        if (hex) text = text[2..];
        if (!int.TryParse(
                text,
                hex ? NumberStyles.AllowHexSpecifier : NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var number))
            throw new InvalidDataException($"Invalid CCD integer '{value}'.");
        return checked(number * sign);
    }

    private static int ParseSectionNumber(string sectionName, string prefix)
    {
        var value = sectionName[prefix.Length..].Trim();
        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var number) || number < 0)
            throw new InvalidDataException($"Invalid indexed CCD section or property '{sectionName}'.");
        return number;
    }
}
