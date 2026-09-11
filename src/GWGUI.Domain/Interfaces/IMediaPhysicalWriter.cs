using GWGUI.Domain.Contracts;
using GWGUI.Domain.Enums;

namespace GWGUI.Domain.Interfaces;

/// <summary>Writes a neutral media plan to a physical device without exposing its implementation type.</summary>
public interface IMediaPhysicalWriter
{
    string Id { get; }

    IReadOnlySet<MediaKind> SupportedMediaKinds { get; }

    bool CanWrite(MediaWritePlan plan, string deviceId);

    Task<MediaPhysicalWriteResult> WriteAsync(
        MediaWritePlan plan,
        string deviceId,
        IReadOnlyDictionary<string, string> options,
        IProgress<MediaPhysicalWriteProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
