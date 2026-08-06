using System.Text.RegularExpressions;

namespace GWGUI.Domain.Commands;

public enum GwTrackState { Success, Retry, Failed }

public sealed record GwTrackProgress(int Cylinder, int Head, int CompletedTracks, int? TotalTracks, int CompletedOnHead, int? TotalOnHead, bool Head0Expected, bool Head1Expected, GwTrackState State, IReadOnlyList<int> Cylinders, int? NextCylinder, int? NextHead)
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
    private IReadOnlyList<int> _cylinders = [];
    private IReadOnlyList<int> _heads = [];

    public GwTrackProgress? Accept(string line)
    {
        var header = HeaderRegex().Match(line);
        if (header.Success)
        {
            _cylinders = ParseSet(header.Groups[1].Value).Order().ToArray();
            var cylinders = _cylinders.Count;
            var headValues = ParseSet(header.Groups[2].Value);
            _heads = headValues.Order().ToArray();
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
        var state = TrackState(line);
        if (state is GwTrackState.Success or GwTrackState.Failed) _completed.Add((cylinder, head));
        var next = _cylinders.SelectMany(c => _heads.Select(h => (Cylinder: c, Head: h))).FirstOrDefault(item => !_completed.Contains(item));
        var hasNext = _cylinders.Count > 0 && _heads.Count > 0 && !_completed.Contains(next);
        return new(cylinder, head, _completed.Count, _total, _completed.Count(item => item.Head == head), _totalPerHead, _head0Expected, _head1Expected, state, _cylinders, hasNext ? next.Cylinder : null, hasNext ? next.Head : null);
    }

    public void Reset() { _completed.Clear(); _total = null; _totalPerHead = null; _head0Expected = false; _head1Expected = false; _cylinders = []; _heads = []; }

    private static GwTrackState TrackState(string line)
    {
        if (line.Contains("Retry", StringComparison.OrdinalIgnoreCase)) return GwTrackState.Retry;
        if (line.Contains("fail", StringComparison.OrdinalIgnoreCase) || line.Contains("error", StringComparison.OrdinalIgnoreCase)) return GwTrackState.Failed;
        return GwTrackState.Success;
    }

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

    [GeneratedRegex(@"^(?:Reading|Writing|Converting|Erasing)\s+c=([0-9,\-/]+):h=([0-9,\-/]+)", RegexOptions.IgnoreCase)]
    private static partial Regex HeaderRegex();
    [GeneratedRegex(@"^T(\d+)\.([01])(?::|\s|$)", RegexOptions.IgnoreCase)]
    private static partial Regex TrackRegex();
    [GeneratedRegex(@"^(\d+)(?:-(\d+)(?:/(\d+))?)?$")]
    private static partial Regex RangeRegex();
}
