using MediaPhysicalWriteProgress = global::GWGUI.MediaEngine.Contracts.MediaPhysicalWriteProgress;
using MediaPhysicalWriteResult = global::GWGUI.MediaEngine.Contracts.MediaPhysicalWriteResult;
using MediaWritePlan = global::GWGUI.MediaEngine.Contracts.MediaWritePlan;
using GWGUI.MediaEngine.Enums;

namespace GWGUI.MediaEngine.Interfaces;

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
