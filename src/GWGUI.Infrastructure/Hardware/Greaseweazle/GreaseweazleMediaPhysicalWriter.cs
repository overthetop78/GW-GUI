using System.Collections.Frozen;
using System.Globalization;
using MediaPhysicalEncodingIds = global::GWGUI.MediaEngine.Constants.MediaPhysicalEncodingIds;
using MediaPhysicalMetadataKeys = global::GWGUI.MediaEngine.Constants.MediaPhysicalMetadataKeys;
using MediaPhysicalDataUnit = global::GWGUI.MediaEngine.Contracts.MediaPhysicalDataUnit;
using MediaPhysicalWriteProgress = global::GWGUI.MediaEngine.Contracts.MediaPhysicalWriteProgress;
using MediaPhysicalWriteResult = global::GWGUI.MediaEngine.Contracts.MediaPhysicalWriteResult;
using MediaWritePlan = global::GWGUI.MediaEngine.Contracts.MediaWritePlan;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Functions;
using IMediaPhysicalWriter = global::GWGUI.MediaEngine.Interfaces.IMediaPhysicalWriter;
using GWGUI.Infrastructure.Constants;

namespace GWGUI.Infrastructure.Hardware.Greaseweazle;

/// <summary>Writes neutral floppy flux plans through a Greaseweazle device.</summary>
public sealed class GreaseweazleMediaPhysicalWriter(
    Func<IGreaseweazleWriteDevice> deviceFactory) : IMediaPhysicalWriter
{
    private static readonly IReadOnlySet<MediaKind> MediaKinds =
        new[] { MediaKind.Floppy }.ToFrozenSet();

    public string Id => GreaseweazleProtocol.MediaProviderId;

    public IReadOnlySet<MediaKind> SupportedMediaKinds => MediaKinds;

    public bool CanWrite(MediaWritePlan plan, string deviceId)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return plan.MediaKind == MediaKind.Floppy
            && plan.RepresentationKind == MediaRepresentationKind.Flux
            && !string.IsNullOrWhiteSpace(deviceId)
            && plan.DataUnits.Count > 0
            && plan.DataUnits.All(unit =>
                unit.Metadata.TryGetValue(MediaPhysicalMetadataKeys.Encoding, out var encoding)
                && encoding.Equals(MediaPhysicalEncodingIds.FluxTrackV1, StringComparison.Ordinal));
    }

    public async Task<MediaPhysicalWriteResult> WriteAsync(
        MediaWritePlan plan,
        string deviceId,
        IReadOnlyDictionary<string, string> options,
        IProgress<MediaPhysicalWriteProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!CanWrite(plan, deviceId))
            throw new NotSupportedException("Greaseweazle requires a neutral floppy flux write plan.");
        ArgumentNullException.ThrowIfNull(options);
        var busType = ReadEnum<GreaseweazleBusType>(options, GreaseweazleMediaOptionKeys.BusType);
        var driveUnit = checked((byte)ReadInt(options, GreaseweazleMediaOptionKeys.DriveUnit));
        var cueAtIndex = ReadBool(options, GreaseweazleMediaOptionKeys.CueAtIndex, true);
        var terminateAtIndex = ReadBool(options, GreaseweazleMediaOptionKeys.TerminateAtIndex, true);
        var hardSectorTicks = checked((uint)ReadLong(options, GreaseweazleMediaOptionKeys.HardSectorTicks, 0));
        var completed = 0;
        var diagnostics = new List<string>();

        await using var device = deviceFactory();
        try
        {
            var firmware = await device.OpenAsync(deviceId, cancellationToken).ConfigureAwait(false);
            await device.SetBusTypeAsync(busType, cancellationToken).ConfigureAwait(false);
            await device.SelectDriveAsync(driveUnit, cancellationToken).ConfigureAwait(false);
            await device.SetMotorAsync(true, cancellationToken).ConfigureAwait(false);
            foreach (var unitIndex in plan.WriteOrder)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var unit = plan.DataUnits[unitIndex];
                var cylinder = ReadMetadataInt(unit, MediaPhysicalMetadataKeys.Cylinder);
                var head = ReadMetadataInt(unit, MediaPhysicalMetadataKeys.Head);
                var track = MediaFluxTrackDataFunctions.Deserialize(unit.Data.Span);
                if (track.Revolutions.Count != 1)
                    throw new InvalidDataException("A physical write unit must contain exactly one selected flux revolution.");
                await device.SeekAsync(checked((short)cylinder), checked((byte)head), cancellationToken).ConfigureAwait(false);
                var intervals = ConvertNanosecondsToDeviceTicks(
                    track.Revolutions[0].FluxIntervalsNanoseconds,
                    firmware.SampleFrequency);
                await device.WriteFluxAsync(
                    intervals,
                    cueAtIndex,
                    terminateAtIndex,
                    hardSectorTicks,
                    cancellationToken).ConfigureAwait(false);
                completed++;
                progress?.Report(new MediaPhysicalWriteProgress(completed, plan.WriteOrder.Count, unit.Position, false));
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new MediaPhysicalWriteResult(completed, plan.WriteOrder.Count, true, diagnostics);
        }
        catch (Exception exception)
        {
            diagnostics.Add($"{exception.GetType().Name}: {exception.Message}");
        }
        finally
        {
            try
            {
                await device.CloseAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                diagnostics.Add($"{exception.GetType().Name}: {exception.Message}");
            }
        }
        return new MediaPhysicalWriteResult(completed, plan.WriteOrder.Count, false, diagnostics);
    }

    private static uint[] ConvertNanosecondsToDeviceTicks(
        IReadOnlyList<uint> intervalsNanoseconds,
        uint sampleFrequency)
    {
        var result = new uint[intervalsNanoseconds.Count];
        ulong sourceElapsed = 0;
        ulong targetElapsed = 0;
        for (var index = 0; index < intervalsNanoseconds.Count; index++)
        {
            sourceElapsed += intervalsNanoseconds[index];
            var convertedElapsed = (ulong)Math.Round(sourceElapsed * sampleFrequency / 1_000_000_000d);
            if (convertedElapsed <= targetElapsed)
                throw new InvalidDataException("A neutral flux interval is shorter than one Greaseweazle sample tick.");
            result[index] = checked((uint)(convertedElapsed - targetElapsed));
            targetElapsed = convertedElapsed;
        }
        return result;
    }

    private static int ReadMetadataInt(MediaPhysicalDataUnit unit, string key) =>
        unit.Metadata.TryGetValue(key, out var value)
        && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : throw new InvalidDataException($"Physical write metadata '{key}' is missing or invalid.");

    private static string ReadRequired(IReadOnlyDictionary<string, string> options, string key) =>
        options.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ArgumentException($"Required Greaseweazle option '{key}' is missing.", nameof(options));

    private static int ReadInt(IReadOnlyDictionary<string, string> options, string key) =>
        int.TryParse(ReadRequired(options, key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : throw new ArgumentException($"Invalid integer value for option '{key}'.", nameof(options));

    private static long ReadLong(IReadOnlyDictionary<string, string> options, string key, long defaultValue) =>
        !options.TryGetValue(key, out var value) ? defaultValue
        : long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed
        : throw new ArgumentException($"Invalid integer value for option '{key}'.", nameof(options));

    private static bool ReadBool(IReadOnlyDictionary<string, string> options, string key, bool defaultValue) =>
        !options.TryGetValue(key, out var value) ? defaultValue
        : bool.TryParse(value, out var parsed) ? parsed
        : throw new ArgumentException($"Invalid Boolean value for option '{key}'.", nameof(options));

    private static T ReadEnum<T>(IReadOnlyDictionary<string, string> options, string key) where T : struct, Enum =>
        Enum.TryParse<T>(ReadRequired(options, key), true, out var parsed)
            ? parsed
            : throw new ArgumentException($"Invalid {typeof(T).Name} value for option '{key}'.", nameof(options));
}
