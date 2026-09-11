using System.Collections.ObjectModel;
using GWGUI.MediaEngine.Enums;

namespace GWGUI.MediaEngine.Contracts;

/// <summary>Contains one track declaration parsed from a CUE sheet before source offsets are resolved.</summary>
public sealed class CueTrackDeclaration
{
    public CueTrackDeclaration(
        int number,
        OpticalTrackMode mode,
        IReadOnlyList<OpticalTrackIndex> indexes,
        long pregapSectors = 0,
        long postgapSectors = 0,
        IReadOnlyList<string>? flags = null,
        string? isrc = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(number);
        ArgumentNullException.ThrowIfNull(indexes);
        if (indexes.Count == 0) throw new ArgumentException("A CUE track requires at least one index.", nameof(indexes));
        if (indexes.Select(index => index.Number).Distinct().Count() != indexes.Count)
            throw new ArgumentException("CUE index numbers must be unique within a track.", nameof(indexes));
        ArgumentOutOfRangeException.ThrowIfNegative(pregapSectors);
        ArgumentOutOfRangeException.ThrowIfNegative(postgapSectors);

        Number = number;
        Mode = mode;
        Indexes = new ReadOnlyCollection<OpticalTrackIndex>(indexes.OrderBy(index => index.Number).ToArray());
        PregapSectors = pregapSectors;
        PostgapSectors = postgapSectors;
        Flags = new ReadOnlyCollection<string>((flags ?? []).ToArray());
        Isrc = isrc;
        Metadata = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(metadata ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase));
    }

    public int Number { get; }
    public OpticalTrackMode Mode { get; }
    public IReadOnlyList<OpticalTrackIndex> Indexes { get; }
    public long PregapSectors { get; }
    public long PostgapSectors { get; }
    public IReadOnlyList<string> Flags { get; }
    public string? Isrc { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
