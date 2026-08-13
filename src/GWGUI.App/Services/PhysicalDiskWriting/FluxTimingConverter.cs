using System.IO;

namespace GWGUI.App.Services.PhysicalDiskWriting;

internal static class FluxTimingConverter
{
    private const ulong NanosecondsPerSecond = 1_000_000_000;

    public static uint[] ToDeviceTicks(
        IReadOnlyList<uint> source,
        uint sourceTickNanoseconds,
        uint deviceSampleFrequency)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (sourceTickNanoseconds == 0) throw new ArgumentOutOfRangeException(nameof(sourceTickNanoseconds));
        if (deviceSampleFrequency == 0) throw new ArgumentOutOfRangeException(nameof(deviceSampleFrequency));

        var result = new List<uint>(source.Count);
        ulong sourceTime = 0;
        ulong previousDeviceTime = 0;
        foreach (var interval in source)
        {
            sourceTime = checked(sourceTime + interval);
            var nanoseconds = checked(sourceTime * sourceTickNanoseconds);
            var numerator = checked(nanoseconds * deviceSampleFrequency + NanosecondsPerSecond / 2);
            var deviceTime = numerator / NanosecondsPerSecond;
            if (deviceTime <= previousDeviceTime) continue;
            result.Add(checked((uint)(deviceTime - previousDeviceTime)));
            previousDeviceTime = deviceTime;
        }

        if (result.Count == 0) throw new InvalidDataException("The track contains no writable flux transition.");
        return [.. result];
    }
}
