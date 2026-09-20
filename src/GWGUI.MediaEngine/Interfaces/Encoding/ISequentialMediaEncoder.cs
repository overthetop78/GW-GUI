using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Images.Models.Sequential;

namespace GWGUI.MediaEngine.Interfaces.Encoding;

/// <summary>Encodes validated protocol blocks into a compatible sequential representation.</summary>
public interface ISequentialMediaEncoder
{
    string Id { get; }
    IReadOnlySet<string> FormatIds { get; }
    IReadOnlySet<string> MachineIds { get; }

    bool CanEncode(SequentialEncodeRequest request);

    Task<SequentialMediaImageRepresentation> EncodeAsync(
        SequentialEncodeRequest request,
        CancellationToken cancellationToken = default);
}
