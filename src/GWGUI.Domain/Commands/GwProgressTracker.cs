using System.Text.RegularExpressions;

namespace GWGUI.Domain.Commands;

public sealed record GwTrackProgress(int Cylinder, int Head, int CompletedTracks, int? TotalTracks, int CompletedOnHead, int? TotalOnHead, bool Head0Expected, bool Head1Expected)
{
    public double? Fraction => TotalTracks > 0 ? Math.Clamp((double)CompletedTracks / TotalTracks.Value, 0, 1) : null;
    public double? HeadFraction => TotalOnHead > 0 ? Math.Clamp((double)CompletedOnHead / TotalOnHead.Value, 0, 1) : null;
}

public sealed partial class GwProgressTracker
{
    private readonly HashSet<(int Cylinder, int Head)> _completed = [];
    private int? _total;
    private int? _totalPerHead;
    private bool _head0Expected;
    private bool _head1Expected;

    public GwTrackProgress? Accept(string line)
    {
        var header = HeaderRegex().Match(line);
        if (header.Success)
        {
            var cylinders = CountSet(header.Groups[1].Value);
            var headValues = ParseSet(header.Groups[2].Value);
            var heads = headValues.Count;
            if (cylinders > 0 && heads > 0) _total = cylinders * heads;
            if (cylinders > 0) _totalPerHead = cylinders;
            _head0Expected = headValues.Contains(0);
            _head1Expected = headValues.Contains(1);
        }

        var track = TrackRegex().Match(line);
        if (!track.Success) return null;
        var cylinder = int.Parse(track.Groups[1].Value);
        var head = int.Parse(track.Groups[2].Value);
        _completed.Add((cylinder, head));
        return new(cylinder, head, _completed.Count, _total, _completed.Count(item => item.Head == head), _totalPerHead, _head0Expected, _head1Expected);
    }

    public void Reset() { _completed.Clear(); _total = null; _totalPerHead = null; _head0Expected = false; _head1Expected = false; }

    private static int CountSet(string specification)
        => ParseSet(specification).Count;

    private static HashSet<int> ParseSet(string specification)
    {
        var values = new HashSet<int>();
        foreach (var item in specification.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var match = RangeRegex().Match(item);
            if (!match.Success) return [];
            var start = int.Parse(match.Groups[1].Value);
            var end = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : start;
            var step = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 1;
            if (step <= 0 || end < start) return [];
            for (var value = start; value <= end; value += step) values.Add(value);
        }
        return values;
    }

    [GeneratedRegex(@"^(?:Reading|Writing)\s+c=([0-9,\-/]+):h=([0-9,\-/]+)", RegexOptions.IgnoreCase)]
    private static partial Regex HeaderRegex();
    [GeneratedRegex(@"^T(\d+)\.([01])(?::|\s|$)", RegexOptions.IgnoreCase)]
    private static partial Regex TrackRegex();
    [GeneratedRegex(@"^(\d+)(?:-(\d+)(?:/(\d+))?)?$")]
    private static partial Regex RangeRegex();
}
