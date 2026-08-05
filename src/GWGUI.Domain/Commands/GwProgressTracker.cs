using System.Text.RegularExpressions;

namespace GWGUI.Domain.Commands;

public sealed record GwTrackProgress(int Cylinder, int Head, int CompletedTracks, int? TotalTracks)
{
    public double? Fraction => TotalTracks > 0 ? Math.Clamp((double)CompletedTracks / TotalTracks.Value, 0, 1) : null;
}

public sealed partial class GwProgressTracker
{
    private readonly HashSet<(int Cylinder, int Head)> _completed = [];
    private int? _total;

    public GwTrackProgress? Accept(string line)
    {
        var header = HeaderRegex().Match(line);
        if (header.Success)
        {
            var cylinders = CountSet(header.Groups[1].Value);
            var heads = CountSet(header.Groups[2].Value);
            if (cylinders > 0 && heads > 0) _total = cylinders * heads;
        }

        var track = TrackRegex().Match(line);
        if (!track.Success) return null;
        var cylinder = int.Parse(track.Groups[1].Value);
        var head = int.Parse(track.Groups[2].Value);
        _completed.Add((cylinder, head));
        return new(cylinder, head, _completed.Count, _total);
    }

    public void Reset() { _completed.Clear(); _total = null; }

    private static int CountSet(string specification)
    {
        var values = new HashSet<int>();
        foreach (var item in specification.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var match = RangeRegex().Match(item);
            if (!match.Success) return 0;
            var start = int.Parse(match.Groups[1].Value);
            var end = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : start;
            var step = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 1;
            if (step <= 0 || end < start) return 0;
            for (var value = start; value <= end; value += step) values.Add(value);
        }
        return values.Count;
    }

    [GeneratedRegex(@"^(?:Reading|Writing)\s+c=([0-9,\-/]+):h=([0-9,\-/]+)", RegexOptions.IgnoreCase)]
    private static partial Regex HeaderRegex();
    [GeneratedRegex(@"^T(\d+)\.([01])(?::|\s|$)", RegexOptions.IgnoreCase)]
    private static partial Regex TrackRegex();
    [GeneratedRegex(@"^(\d+)(?:-(\d+)(?:/(\d+))?)?$")]
    private static partial Regex RangeRegex();
}
