using System.Collections.Frozen;
using System.Globalization;
using MediaPhysicalEncodingIds = global::GWGUI.MediaEngine.Constants.MediaPhysicalEncodingIds;
using MediaPhysicalMetadataKeys = global::GWGUI.MediaEngine.Constants.MediaPhysicalMetadataKeys;
using MediaAcquisitionProgress = global::GWGUI.MediaEngine.Contracts.MediaAcquisitionProgress;
using MediaAcquisitionResult = global::GWGUI.MediaEngine.Contracts.MediaAcquisitionResult;
using MediaFluxRevolutionData = global::GWGUI.MediaEngine.Contracts.MediaFluxRevolutionData;
using MediaFluxTrackData = global::GWGUI.MediaEngine.Contracts.MediaFluxTrackData;
using MediaPhysicalDataUnit = global::GWGUI.MediaEngine.Contracts.MediaPhysicalDataUnit;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Functions;
using IMediaAcquisitionProvider = global::GWGUI.MediaEngine.Interfaces.IMediaAcquisitionProvider;
using GWGUI.Infrastructure.Constants;

namespace GWGUI.Infrastructure.Hardware.Greaseweazle;

/// <summary>Acquires Greaseweazle flux as neutral, versioned physical track units.</summary>
public sealed class GreaseweazleMediaAcquisitionProvider(
    Func<IGreaseweazleReadDevice> deviceFactory) : IMediaAcquisitionProvider
{
    private static readonly IReadOnlySet<MediaKind> MediaKinds =
        new[] { MediaKind.Floppy }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> RepresentationKinds =
        new[] { MediaRepresentationKind.Flux }.ToFrozenSet();

    public string Id => GreaseweazleProtocol.MediaProviderId;

    public IReadOnlySet<MediaKind> SupportedMediaKinds => MediaKinds;

    public IReadOnlySet<MediaRepresentationKind> OutputRepresentationKinds => RepresentationKinds;

    public bool CanAcquire(MediaKind mediaKind, string deviceId) =>
        mediaKind == MediaKind.Floppy && !string.IsNullOrWhiteSpace(deviceId);

    public async Task<MediaAcquisitionResult> AcquireAsync(
        MediaKind mediaKind,
        string deviceId,
        IReadOnlyDictionary<string, string> options,
        IProgress<MediaAcquisitionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!CanAcquire(mediaKind, deviceId))
            throw new NotSupportedException($"Greaseweazle cannot acquire media family '{mediaKind}' from device '{deviceId}'.");
        ArgumentNullException.ThrowIfNull(options);
        var busType = ReadEnum<GreaseweazleBusType>(options, GreaseweazleMediaOptionKeys.BusType);
        var driveUnit = checked((byte)ReadInt(options, GreaseweazleMediaOptionKeys.DriveUnit));
        var tracks = ParseTracks(ReadRequired(options, GreaseweazleMediaOptionKeys.Tracks));
        var revolutions = ReadInt(options, GreaseweazleMediaOptionKeys.Revolutions);
        var overflowRetries = ReadInt(options, GreaseweazleMediaOptionKeys.FluxOverflowRetries);
        var seekRetries = ReadInt(options, GreaseweazleMediaOptionKeys.SeekRetries);
        var hardSectors = ReadBool(options, GreaseweazleMediaOptionKeys.HardSectors, false);
        var fakeIndex = ReadOptionalMilliseconds(options, GreaseweazleMediaOptionKeys.FakeIndexMilliseconds);
        var motorDelay = ReadOptionalMilliseconds(options, GreaseweazleMediaOptionKeys.MotorSpinUpMilliseconds);
        var settleDelay = ReadOptionalMilliseconds(options, GreaseweazleMediaOptionKeys.TrackSettleMilliseconds);
        Validate(tracks, revolutions, overflowRetries, seekRetries, fakeIndex, hardSectors);

        var units = new List<MediaPhysicalDataUnit>(tracks.Length);
        await using var device = deviceFactory();
        try
        {
            var firmware = await device.OpenAsync(deviceId, cancellationToken).ConfigureAwait(false);
            await device.SetBusTypeAsync(busType, cancellationToken).ConfigureAwait(false);
            await device.SelectDriveAsync(driveUnit, cancellationToken).ConfigureAwait(false);
            await device.SetMotorAsync(true, cancellationToken).ConfigureAwait(false);
            if (motorDelay is { } spinUp) await Task.Delay(spinUp, cancellationToken).ConfigureAwait(false);

            for (var index = 0; index < tracks.Length; index++)
            {
                var track = tracks[index];
                var acquired = await AcquireTrackAsync(
                    device,
                    track,
                    revolutions,
                    overflowRetries,
                    seekRetries,
                    fakeIndex,
                    hardSectors,
                    settleDelay,
                    firmware.SampleFrequency,
                    progress,
                    index,
                    tracks.Length,
                    cancellationToken).ConfigureAwait(false);
                units.Add(acquired.Unit);
                progress?.Report(new MediaAcquisitionProgress(
                    index + 1,
                    tracks.Length,
                    acquired.Attempt,
                    acquired.Unit,
                    acquired.Unit.Metadata));
            }
        }
        finally
        {
            await device.CloseAsync(CancellationToken.None).ConfigureAwait(false);
        }

        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [MediaPhysicalMetadataKeys.DeviceId] = deviceId,
            [MediaPhysicalMetadataKeys.BusType] = busType.ToString(),
            [MediaPhysicalMetadataKeys.DriveUnit] = driveUnit.ToString(CultureInfo.InvariantCulture),
            [MediaPhysicalMetadataKeys.RevolutionCount] = revolutions.ToString(CultureInfo.InvariantCulture)
        };
        if (options.TryGetValue(GreaseweazleMediaOptionKeys.DiskType, out var diskType))
            metadata[MediaPhysicalMetadataKeys.DiskType] = diskType;
        return new MediaAcquisitionResult(MediaKind.Floppy, MediaRepresentationKind.Flux, units, metadata, []);
    }

    private static async Task<(MediaPhysicalDataUnit Unit, int Attempt)> AcquireTrackAsync(
        IGreaseweazleReadDevice device,
        (int Cylinder, int Head, int DriveCylinder, int DriveHead) track,
        int revolutions,
        int overflowRetries,
        int seekRetries,
        TimeSpan? fakeIndex,
        bool hardSectors,
        TimeSpan? settleDelay,
        uint sampleFrequency,
        IProgress<MediaAcquisitionProgress>? progress,
        int completed,
        int total,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await device.SeekAsync(checked((short)track.DriveCylinder), checked((byte)track.DriveHead), cancellationToken).ConfigureAwait(false);
                if (settleDelay is { } settle) await Task.Delay(settle, cancellationToken).ConfigureAwait(false);
                progress?.Report(new MediaAcquisitionProgress(
                    completed,
                    total,
                    attempt,
                    metadata: PositionMetadata(track.Cylinder, track.Head)));
                var capture = await ReadTrackAsync(
                    device,
                    revolutions,
                    overflowRetries,
                    fakeIndex,
                    hardSectors,
                    sampleFrequency,
                    cancellationToken).ConfigureAwait(false);
                var layout = CreateLayout(capture, revolutions, fakeIndex, hardSectors);
                var unit = CreateUnit(track.Cylinder, track.Head, capture, layout, revolutions, completed);
                return (unit, attempt);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch when (attempt <= seekRetries)
            {
                await device.SeekAsync(0, 0, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static async ValueTask<GreaseweazleFluxCapture> ReadTrackAsync(
        IGreaseweazleReadDevice device,
        int revolutions,
        int overflowRetries,
        TimeSpan? fakeIndex,
        bool hardSectors,
        uint sampleFrequency,
        CancellationToken cancellationToken)
    {
        if (fakeIndex is { } indexPeriod)
        {
            var revolutionTicks = DurationToTicks(indexPeriod, sampleFrequency);
            var leadTicks = checked((uint)Math.Max(1, Math.Round(sampleFrequency * GreaseweazleProtocol.FakeIndexLeadSeconds)));
            var captureTicks = checked(revolutionTicks * (uint)revolutions + leadTicks * 2);
            return await device.ReadFluxAsync(0, captureTicks, overflowRetries, cancellationToken).ConfigureAwait(false);
        }
        if (hardSectors)
        {
            var captureTicks = DurationToTicks(TimeSpan.FromSeconds(GreaseweazleProtocol.HardSectorCaptureSeconds), sampleFrequency);
            return await device.ReadFluxAsync(0, captureTicks, overflowRetries, cancellationToken).ConfigureAwait(false);
        }
        return await device.ReadFluxAsync(revolutions, retries: overflowRetries, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private static GreaseweazleRotationLayout CreateLayout(
        GreaseweazleFluxCapture capture,
        int revolutions,
        TimeSpan? fakeIndex,
        bool hardSectors) => fakeIndex is { } indexPeriod
        ? GreaseweazleFluxIndexNormalizer.FromFakeIndex(capture, revolutions, DurationToTicks(indexPeriod, capture.SampleFrequency))
        : hardSectors
            ? GreaseweazleFluxIndexNormalizer.FromHardSectorIndexes(capture, revolutions)
            : GreaseweazleFluxIndexNormalizer.FromPhysicalIndexes(capture, revolutions);

    private static MediaPhysicalDataUnit CreateUnit(
        int cylinder,
        int head,
        GreaseweazleFluxCapture capture,
        GreaseweazleRotationLayout layout,
        int revolutionCount,
        int position)
    {
        if (layout.RevolutionTicks.Count < revolutionCount)
            throw new InvalidDataException("The normalized capture does not contain every requested revolution.");
        var endpoints = new List<ulong>(capture.FluxIntervals.Count);
        ulong elapsed = 0;
        foreach (var interval in capture.FluxIntervals)
        {
            elapsed += interval;
            endpoints.Add(elapsed);
        }

        var revolutions = new List<MediaFluxRevolutionData>(revolutionCount);
        ulong start = layout.InitialIndexTicks;
        var endpointIndex = 0;
        while (endpointIndex < endpoints.Count && endpoints[endpointIndex] <= start) endpointIndex++;
        for (var revolution = 0; revolution < revolutionCount; revolution++)
        {
            var duration = layout.RevolutionTicks[revolution];
            var end = start + duration;
            var intervals = new List<uint>();
            var previous = start;
            while (endpointIndex < endpoints.Count && endpoints[endpointIndex] <= end)
            {
                intervals.Add(ConvertTicksToNanoseconds(endpoints[endpointIndex] - previous, capture.SampleFrequency));
                previous = endpoints[endpointIndex++];
            }
            revolutions.Add(new MediaFluxRevolutionData(
                ConvertTicksToNanoseconds(duration, capture.SampleFrequency),
                intervals));
            start = end;
        }

        var track = new MediaFluxTrackData(revolutions);
        return new MediaPhysicalDataUnit(
            position,
            MediaFluxTrackDataFunctions.Serialize(track),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [MediaPhysicalMetadataKeys.Encoding] = MediaPhysicalEncodingIds.FluxTrackV1,
                [MediaPhysicalMetadataKeys.Cylinder] = cylinder.ToString(CultureInfo.InvariantCulture),
                [MediaPhysicalMetadataKeys.Head] = head.ToString(CultureInfo.InvariantCulture),
                [MediaPhysicalMetadataKeys.RevolutionCount] = revolutionCount.ToString(CultureInfo.InvariantCulture),
                [MediaPhysicalMetadataKeys.TickNanoseconds] = "1"
            });
    }

    private static IReadOnlyDictionary<string, string> PositionMetadata(int cylinder, int head) =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [MediaPhysicalMetadataKeys.Cylinder] = cylinder.ToString(CultureInfo.InvariantCulture),
            [MediaPhysicalMetadataKeys.Head] = head.ToString(CultureInfo.InvariantCulture)
        };

    private static uint ConvertTicksToNanoseconds(ulong ticks, uint sampleFrequency) =>
        checked((uint)Math.Max(1, Math.Round(ticks * 1_000_000_000d / sampleFrequency)));

    private static uint DurationToTicks(TimeSpan duration, uint sampleFrequency)
    {
        if (duration <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(duration));
        return checked((uint)Math.Round(duration.TotalSeconds * sampleFrequency));
    }

    private static (int Cylinder, int Head, int DriveCylinder, int DriveHead)[] ParseTracks(string value)
    {
        var tracks = value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(item => item.Split(':', StringSplitOptions.TrimEntries))
            .Select(parts => parts.Length == 4
                ? (ParseInt(parts[0]), ParseInt(parts[1]), ParseInt(parts[2]), ParseInt(parts[3]))
                : throw new ArgumentException($"Invalid Greaseweazle track selection '{string.Join(':', parts)}'."))
            .ToArray();
        if (tracks.Length == 0) throw new ArgumentException("At least one Greaseweazle track must be selected.", nameof(value));
        return tracks;
    }

    private static void Validate(
        IReadOnlyList<(int Cylinder, int Head, int DriveCylinder, int DriveHead)> tracks,
        int revolutions,
        int overflowRetries,
        int seekRetries,
        TimeSpan? fakeIndex,
        bool hardSectors)
    {
        if (revolutions is <= 0 or > byte.MaxValue) throw new ArgumentOutOfRangeException(nameof(revolutions));
        if (overflowRetries < 0) throw new ArgumentOutOfRangeException(nameof(overflowRetries));
        if (seekRetries < 0) throw new ArgumentOutOfRangeException(nameof(seekRetries));
        if (fakeIndex is not null && hardSectors)
            throw new ArgumentException("Fake index and hard-sector acquisition are mutually exclusive.");
        if (tracks.Any(track => track.Cylinder is < 0 or > 83 || track.Head is < 0 or > 1
                || track.DriveCylinder is < 0 or > short.MaxValue || track.DriveHead is < 0 or > byte.MaxValue))
            throw new ArgumentOutOfRangeException(nameof(tracks), "A selected track is outside the supported range.");
        if (tracks.GroupBy(track => (track.Cylinder, track.Head)).Any(group => group.Count() > 1))
            throw new ArgumentException("A logical track is selected more than once.", nameof(tracks));
    }

    private static string ReadRequired(IReadOnlyDictionary<string, string> options, string key) =>
        options.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ArgumentException($"Required Greaseweazle option '{key}' is missing.", nameof(options));

    private static int ReadInt(IReadOnlyDictionary<string, string> options, string key) =>
        ParseInt(ReadRequired(options, key));

    private static int ParseInt(string value) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : throw new ArgumentException($"Invalid invariant integer '{value}'.");

    private static bool ReadBool(IReadOnlyDictionary<string, string> options, string key, bool defaultValue) =>
        !options.TryGetValue(key, out var value) ? defaultValue
        : bool.TryParse(value, out var parsed) ? parsed
        : throw new ArgumentException($"Invalid Boolean value '{value}' for option '{key}'.", nameof(options));

    private static TimeSpan? ReadOptionalMilliseconds(IReadOnlyDictionary<string, string> options, string key)
    {
        if (!options.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value)) return null;
        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var milliseconds) || milliseconds <= 0)
            throw new ArgumentException($"Invalid positive duration '{value}' for option '{key}'.", nameof(options));
        return TimeSpan.FromMilliseconds(milliseconds);
    }

    private static T ReadEnum<T>(IReadOnlyDictionary<string, string> options, string key) where T : struct, Enum =>
        Enum.TryParse<T>(ReadRequired(options, key), true, out var parsed)
            ? parsed
            : throw new ArgumentException($"Invalid {typeof(T).Name} value for option '{key}'.", nameof(options));
}
