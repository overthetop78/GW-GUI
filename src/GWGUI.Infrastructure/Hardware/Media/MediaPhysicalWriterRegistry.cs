using GWGUI.Domain.Contracts;
using GWGUI.Domain.Interfaces;

namespace GWGUI.Infrastructure.Hardware.Media;

/// <summary>Selects a physical media writer by write plan and device identifier.</summary>
public sealed class MediaPhysicalWriterRegistry
{
    private readonly IReadOnlyList<IMediaPhysicalWriter> writers;

    public MediaPhysicalWriterRegistry(IEnumerable<IMediaPhysicalWriter> writers)
    {
        ArgumentNullException.ThrowIfNull(writers);
        var registered = writers.ToArray();
        if (registered.Any(writer => writer is null))
            throw new ArgumentException("A physical writer cannot be null.", nameof(writers));
        if (registered.Any(writer => string.IsNullOrWhiteSpace(writer.Id)))
            throw new ArgumentException("A physical writer identifier cannot be empty.", nameof(writers));
        var duplicate = registered.GroupBy(writer => writer.Id, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Skip(1).Any());
        if (duplicate is not null)
            throw new ArgumentException($"Physical writer identifier '{duplicate.Key}' is registered more than once.", nameof(writers));
        this.writers = Array.AsReadOnly(registered);
    }

    public IReadOnlyList<IMediaPhysicalWriter> Writers => writers;

    public IMediaPhysicalWriter GetRequired(MediaWritePlan plan, string deviceId)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        var compatible = writers.Where(writer =>
            writer.SupportedMediaKinds.Contains(plan.MediaKind)
            && writer.CanWrite(plan, deviceId)).ToArray();
        return compatible.Length switch
        {
            1 => compatible[0],
            0 => throw new NotSupportedException(
                $"No physical writer accepts device '{deviceId}' for media family '{plan.MediaKind}'."),
            _ => throw new InvalidOperationException(
                $"Multiple physical writers accept device '{deviceId}' for media family '{plan.MediaKind}'.")
        };
    }

    public Task<MediaPhysicalWriteResult> WriteAsync(
        MediaWritePlan plan,
        string deviceId,
        IReadOnlyDictionary<string, string> options,
        IProgress<MediaPhysicalWriteProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        return GetRequired(plan, deviceId).WriteAsync(plan, deviceId, options, progress, cancellationToken);
    }
}
