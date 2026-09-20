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
        IReadOnlyDictionary<string, string>? options = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetFormatId);

        Source = source;
        OutputPath = outputPath;
        TargetFormatId = targetFormatId;
        Options = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(options ?? new Dictionary<string, string>(), StringComparer.Ordinal));
    }

    public MediaImageDocument Source { get; }

    public string OutputPath { get; }

    public string TargetFormatId { get; }

    public IReadOnlyDictionary<string, string> Options { get; }
}
