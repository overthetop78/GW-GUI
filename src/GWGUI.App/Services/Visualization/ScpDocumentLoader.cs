using GWGUI.App.Contracts.ViewModels.Visualization;
using System.IO;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.App.Services.Visualization;

public sealed class ScpDocumentLoader(IScpReader reader, Func<string, object[], string> localize)
{
    public async Task<ScpDocumentModel> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        var image = await reader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        var checksum = Localize(image.ChecksumValid ? "Visual.ChecksumValid" : "Visual.ChecksumInvalid");
        var summary = Localize("Visual.Summary", image.Header.VersionText, image.Tracks.Count, image.Header.Revolutions, image.Header.ResolutionNanoseconds, checksum);
        return new(image, Path.GetFileName(path), summary, image.Tracks.Select(track => track.Head).ToHashSet());
    }

    private string Localize(string key, params object[] arguments) => localize(key, arguments);
}
