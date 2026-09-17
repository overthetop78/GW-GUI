using GWGUI.App.Contracts.ViewModels.Visualization;
using GWGUI.App.Constants.Localization;
using System.IO;
using GWGUI.MediaEngine;

using GWGUI.MediaEngine.Formats.Floppy.Scp;

namespace GWGUI.App.Services.Visualization;

public sealed class ScpDocumentLoader(IScpReader reader, Func<string, object[], string> localize)
{
    public async Task<ScpDocumentModel> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        var image = await reader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        return CreateModel(path, image);
    }

    public ScpDocumentModel CreateModel(string path, ScpImage image)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(image);
        var checksum = Localize(image.ChecksumValid
            ? DiskImageResourceKeys.VisualChecksumValid
            : DiskImageResourceKeys.VisualChecksumInvalid);
        var summary = Localize(
            DiskImageResourceKeys.VisualSummary,
            image.Header.VersionText,
            image.Tracks.Count,
            image.Header.Revolutions,
            image.Header.ResolutionNanoseconds,
            checksum);
        return new(image, Path.GetFileName(path), summary, image.Tracks.Select(track => track.Head).ToHashSet());
    }

    private string Localize(string key, params object[] arguments) => localize(key, arguments);
}
