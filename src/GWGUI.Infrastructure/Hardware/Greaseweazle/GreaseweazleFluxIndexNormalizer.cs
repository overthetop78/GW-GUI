namespace GWGUI.Infrastructure.Hardware.Greaseweazle;

public static class GreaseweazleFluxIndexNormalizer
{
    public static GreaseweazleRotationLayout FromPhysicalIndexes(
        GreaseweazleFluxCapture capture,
        int revolutions)
    {
        ArgumentNullException.ThrowIfNull(capture);
        if (revolutions <= 0) throw new ArgumentOutOfRangeException(nameof(revolutions));
        if (capture.IndexIntervals.Count < revolutions + 1)
            throw new InvalidDataException($"The capture contains {capture.IndexIntervals.Count} index marks; {revolutions + 1} are required.");
        return new(capture.IndexIntervals[0], capture.IndexIntervals.Skip(1).Take(revolutions).ToArray());
    }

    public static GreaseweazleRotationLayout FromFakeIndex(
        GreaseweazleFluxCapture capture,
        int revolutions,
        uint ticksPerRevolution)
    {
        ArgumentNullException.ThrowIfNull(capture);
        if (revolutions <= 0) throw new ArgumentOutOfRangeException(nameof(revolutions));
        if (ticksPerRevolution == 0) throw new ArgumentOutOfRangeException(nameof(ticksPerRevolution));
        var preIndexTicks = checked((uint)Math.Max(1, Math.Round(capture.SampleFrequency * GreaseweazleProtocol.FakeIndexLeadSeconds)));
        return new(preIndexTicks, Enumerable.Repeat(ticksPerRevolution, revolutions).ToArray());
    }

    public static GreaseweazleRotationLayout FromHardSectorIndexes(
        GreaseweazleFluxCapture capture,
        int revolutions)
    {
        ArgumentNullException.ThrowIfNull(capture);
        if (revolutions <= 0) throw new ArgumentOutOfRangeException(nameof(revolutions));
        if (capture.IndexIntervals.Count < 5)
            throw new InvalidDataException("At least five index marks are required to identify hard sectors.");

        var initialIndexTicks = capture.IndexIntervals[0];
        var intervals = capture.IndexIntervals.Skip(1).ToArray();
        var thresholdSamples = new[] { intervals[0], intervals[2] };
        Array.Sort(thresholdSamples);
        var threshold = thresholdSamples[1] * 3d / 4d;
        var rotationTicks = new List<uint>();
        var sectorCounts = new List<int>();
        ulong elapsed = 0;
        ulong shortTicks = 0;
        var shortCount = 0;
        var sectors = 0;

        foreach (var interval in intervals)
        {
            var isShort = interval < threshold;
            if (isShort)
            {
                shortTicks += interval;
                shortCount++;
            }
            if (shortCount != 0 && (shortCount > 1 || !isShort))
            {
                elapsed += shortTicks;
                sectors++;
                rotationTicks.Add(checked((uint)elapsed));
                sectorCounts.Add(sectors);
                elapsed = 0;
                shortTicks = 0;
                shortCount = 0;
                sectors = 0;
                if (rotationTicks.Count == revolutions) break;
            }
            if (!isShort)
            {
                elapsed += interval;
                sectors++;
            }
        }

        if (rotationTicks.Count < revolutions)
            throw new InvalidDataException($"Only {rotationTicks.Count} complete hard-sector revolutions were identified; {revolutions} are required.");
        if (sectorCounts.Count > 1 && sectorCounts.Skip(1).Any(count => count != sectorCounts[0]))
            throw new InvalidDataException("The number of hard sectors is inconsistent between revolutions.");
        return new(initialIndexTicks, rotationTicks, sectorCounts);
    }
}
