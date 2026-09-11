using System.Globalization;
using System.IO;
using GWGUI.App.Contracts.Services.PhysicalDiskWriting;
using GWGUI.App.Enums.Services.PhysicalDiskWriting;
using GWGUI.Domain.Constants;
using GWGUI.Domain.Contracts;
using GWGUI.Infrastructure.Constants;
using GWGUI.Infrastructure.Hardware.Media;

namespace GWGUI.App.Services.PhysicalDiskWriting;

public sealed class PhysicalDiskWriteService(MediaPhysicalWriterRegistry writers)
{
    public async Task<PhysicalDiskWriteResult> WriteAsync(
        MediaWritePlan plan,
        PhysicalDiskWriteOptions options,
        IProgress<PhysicalTrackWriteProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.PortName) || options.Verify)
        {
            var failure = new PhysicalTrackWriteFailure(
                null,
                null,
                PhysicalDiskWriteFailureCategory.Validation);
            return new PhysicalDiskWriteResult(0, plan.WriteOrder.Count, false, [failure]);
        }
        var writeProgress = new Progress<MediaPhysicalWriteProgress>(value =>
            ReportProgress(plan, value, progress));
        var result = await writers.WriteAsync(
            plan,
            options.PortName,
            CreateOptions(options),
            writeProgress,
            cancellationToken).ConfigureAwait(false);
        var failures = result.Diagnostics.Select(MapFailure).ToArray();
        return new PhysicalDiskWriteResult(
            result.CompletedUnits,
            result.TotalUnits,
            result.Cancelled,
            failures);
    }

    private static IReadOnlyDictionary<string, string> CreateOptions(PhysicalDiskWriteOptions options) =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [GreaseweazleMediaOptionKeys.BusType] = options.BusType.ToString(),
            [GreaseweazleMediaOptionKeys.DriveUnit] = options.DriveUnit.ToString(CultureInfo.InvariantCulture),
            [GreaseweazleMediaOptionKeys.CueAtIndex] = options.CueAtIndex.ToString(CultureInfo.InvariantCulture),
            [GreaseweazleMediaOptionKeys.TerminateAtIndex] = options.TerminateAtIndex.ToString(CultureInfo.InvariantCulture),
            [GreaseweazleMediaOptionKeys.HardSectorTicks] = options.HardSectorTicks.ToString(CultureInfo.InvariantCulture)
        };

    private static void ReportProgress(
        MediaWritePlan plan,
        MediaPhysicalWriteProgress value,
        IProgress<PhysicalTrackWriteProgress>? progress)
    {
        var unit = plan.DataUnits.FirstOrDefault(item => item.Position == value.Position);
        var cylinder = unit is null ? 0 : ReadPosition(unit.Metadata, MediaPhysicalMetadataKeys.Cylinder);
        var head = unit is null ? 0 : ReadPosition(unit.Metadata, MediaPhysicalMetadataKeys.Head);
        progress?.Report(new PhysicalTrackWriteProgress(
            value.CompletedUnits,
            value.TotalUnits,
            cylinder,
            head,
            value.Verifying));
    }

    private static int ReadPosition(IReadOnlyDictionary<string, string> metadata, string key) =>
        metadata.TryGetValue(key, out var value)
        && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0;

    private static PhysicalTrackWriteFailure MapFailure(string diagnostic)
    {
        var category = diagnostic.Contains("WriteProtected", StringComparison.OrdinalIgnoreCase)
            || diagnostic.Contains("write protected", StringComparison.OrdinalIgnoreCase)
                ? PhysicalDiskWriteFailureCategory.WriteProtected
                : PhysicalDiskWriteFailureCategory.Device;
        return new PhysicalTrackWriteFailure(
            null,
            null,
            category,
            new IOException(diagnostic));
    }
}
