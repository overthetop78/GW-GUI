using System.IO;
using GWGUI.Infrastructure.Hardware.Greaseweazle;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding;

namespace GWGUI.App.Services.PhysicalDiskWriting;

public sealed class PhysicalDiskWriteService(
    IGreaseweazleWriteDevice device,
    IPhysicalTrackVerifier? verifier = null)
{
    public Task<PhysicalDiskWriteResult> WriteAsync(
        ScpImage image,
        PhysicalDiskWriteOptions options,
        IProgress<PhysicalTrackWriteProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(options);
        var tracks = CreateScpTracks(image, options.ScpRevolution);
        return WriteTracksAsync(tracks, options, progress, cancellationToken);
    }

    public Task<PhysicalDiskWriteResult> WriteAsync(
        IReadOnlyList<EncodedDiskTrack> encodedTracks,
        PhysicalDiskWriteOptions options,
        IProgress<PhysicalTrackWriteProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(encodedTracks);
        ArgumentNullException.ThrowIfNull(options);
        var tracks = encodedTracks.Select(track => new PhysicalDiskWriteTrack(
            track.Cylinder,
            track.Head,
            track.Track.Revolution.FluxIntervals,
            EncodedTrackTiming.TickNanoseconds,
            track)).ToArray();
        return WriteTracksAsync(tracks, options, progress, cancellationToken);
    }

    private async Task<PhysicalDiskWriteResult> WriteTracksAsync(
        IReadOnlyList<PhysicalDiskWriteTrack> tracks,
        PhysicalDiskWriteOptions options,
        IProgress<PhysicalTrackWriteProgress>? progress,
        CancellationToken cancellationToken)
    {
        var orderedTracks = tracks.OrderBy(track => track.Cylinder).ThenBy(track => track.Head).ToArray();
        var validationFailure = Validate(orderedTracks, options);
        if (validationFailure is not null) return new(0, orderedTracks.Length, false, [validationFailure]);

        var written = 0;
        var failures = new List<PhysicalTrackWriteFailure>();
        try
        {
            var firmware = await device.OpenAsync(options.PortName, cancellationToken);
            await device.SetBusTypeAsync(options.BusType, cancellationToken);
            await device.SelectDriveAsync(options.DriveUnit, cancellationToken);
            await device.SetMotorAsync(true, cancellationToken);

            foreach (var track in orderedTracks)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await device.SeekAsync(checked((short)track.Cylinder), checked((byte)track.Head), cancellationToken);
                    var intervals = PrepareIntervals(track, options, firmware.SampleFrequency);
                    await device.WriteFluxAsync(
                        intervals,
                        options.CueAtIndex,
                        options.TerminateAtIndex,
                        options.HardSectorTicks,
                        cancellationToken);
                    written++;
                    progress?.Report(new(written, orderedTracks.Length, track.Cylinder, track.Head, false));

                    if (!options.Verify) continue;
                    progress?.Report(new(written, orderedTracks.Length, track.Cylinder, track.Head, true));
                    if (!await verifier!.VerifyAsync(track.Cylinder, track.Head, intervals, cancellationToken))
                    {
                        failures.Add(new(track.Cylinder, track.Head, PhysicalDiskWriteFailureKind.Verification,
                            "The written track did not match the expected flux."));
                        break;
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    failures.Add(MapFailure(track, exception));
                    break;
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new(written, orderedTracks.Length, true, failures.AsReadOnly());
        }
        catch (Exception exception)
        {
            failures.Add(MapFailure(null, exception));
        }
        finally
        {
            try
            {
                await device.CloseAsync(CancellationToken.None);
            }
            catch (Exception exception)
            {
                failures.Add(new(null, null, PhysicalDiskWriteFailureKind.Device,
                    "The Greaseweazle device could not be closed safely.", exception));
            }
        }

        return new(written, orderedTracks.Length, false, failures.AsReadOnly());
    }

    private static IReadOnlyList<PhysicalDiskWriteTrack> CreateScpTracks(ScpImage image, int revolution)
    {
        if (revolution < 0) throw new ArgumentOutOfRangeException(nameof(revolution));
        var tickNanoseconds = checked((uint)image.Header.ResolutionNanoseconds);
        return image.Tracks.Select(track =>
        {
            if (revolution >= track.Revolutions.Count)
                throw new InvalidDataException($"Track {track.Cylinder}.{track.Head} does not contain revolution {revolution}.");
            return new PhysicalDiskWriteTrack(
                track.Cylinder,
                track.Head,
                track.Revolutions[revolution].FluxIntervals,
                tickNanoseconds);
        }).ToArray();
    }

    private PhysicalTrackWriteFailure? Validate(
        IReadOnlyList<PhysicalDiskWriteTrack> tracks,
        PhysicalDiskWriteOptions options)
    {
        if (tracks.Count == 0)
            return new(null, null, PhysicalDiskWriteFailureKind.Validation, "The image contains no writable track.");
        if (string.IsNullOrWhiteSpace(options.PortName))
            return new(null, null, PhysicalDiskWriteFailureKind.Validation, "A serial port is required.");
        if (options.Verify && verifier is null)
            return new(null, null, PhysicalDiskWriteFailureKind.Validation, "Verification is unavailable.");
        if (options.Precompensation is { Count: > 0 } && tracks.Any(track => track.EncodedTrack is null))
            return new(null, null, PhysicalDiskWriteFailureKind.Validation,
                "Precompensation requires tracks produced by an internal track encoder.");
        if (tracks.Any(track => track.Cylinder is < 0 or > short.MaxValue || track.Head is < 0 or > byte.MaxValue))
            return new(null, null, PhysicalDiskWriteFailureKind.Validation, "A track address is outside the controller range.");
        return null;
    }

    private static uint[] PrepareIntervals(
        PhysicalDiskWriteTrack track,
        PhysicalDiskWriteOptions options,
        uint sampleFrequency)
    {
        var precompensation = ResolvePrecompensation(options.Precompensation, track.Cylinder);
        var source = precompensation > 0
            ? EncodedTrackPrecompensator.Apply(track.EncodedTrack!, options.PrecompensationEncoding, precompensation)
            : track.FluxIntervals;
        return FluxTimingConverter.ToDeviceTicks(source, track.SourceTickNanoseconds, sampleFrequency);
    }

    private static double ResolvePrecompensation(
        IReadOnlyList<PhysicalWritePrecompensationStep>? steps,
        int cylinder) =>
        steps?.Where(step => step.FromCylinder <= cylinder)
            .OrderBy(step => step.FromCylinder)
            .LastOrDefault()?.Nanoseconds ?? 0;

    private static PhysicalTrackWriteFailure MapFailure(
        PhysicalDiskWriteTrack? track,
        Exception exception)
    {
        var kind = exception switch
        {
            GreaseweazleProtocolException
            {
                Acknowledgement: GreaseweazleAcknowledgement.WriteProtected
            } => PhysicalDiskWriteFailureKind.WriteProtected,
            GreaseweazleProtocolException => PhysicalDiskWriteFailureKind.Device,
            ArgumentException or InvalidDataException or OverflowException => PhysicalDiskWriteFailureKind.Validation,
            _ => PhysicalDiskWriteFailureKind.Unexpected
        };
        return new(track?.Cylinder, track?.Head, kind, exception.Message, exception);
    }
}
