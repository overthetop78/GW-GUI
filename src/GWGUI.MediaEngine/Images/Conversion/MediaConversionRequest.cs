using System.Collections.ObjectModel;
using GWGUI.MediaEngine.Contracts;

namespace GWGUI.MediaEngine.Images.Conversion;

/// <summary>Describes a conversion from an already read media document to one output target.</summary>
public sealed class MediaConversionRequest
{
    public MediaConversionRequest(
        MediaImageDocument source,
        string outputPath,
        string targetFormatId,
        IReadOnlyDictionary<string, string>? options = null,
        Action<MediaExplorationProgress>? progress = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetFormatId);

        Source = source;
        OutputPath = outputPath;
        TargetFormatId = targetFormatId;
        Progress = progress;
        Options = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(options ?? new Dictionary<string, string>(), StringComparer.Ordinal));
    }

    public MediaImageDocument Source { get; }

    public string OutputPath { get; }

    public string TargetFormatId { get; }

    public Action<MediaExplorationProgress>? Progress { get; }

    public IReadOnlyDictionary<string, string> Options { get; }
}
