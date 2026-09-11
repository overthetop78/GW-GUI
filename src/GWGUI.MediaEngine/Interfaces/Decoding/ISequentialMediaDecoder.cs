using GWGUI.MediaEngine.Contracts;

namespace GWGUI.MediaEngine.Interfaces.Decoding;

/// <summary>Decodes machine protocol blocks from a compatible sequential media document.</summary>
public interface ISequentialMediaDecoder
{
    string Id { get; }
    IReadOnlySet<string> FormatIds { get; }
    IReadOnlySet<string> MachineIds { get; }

    bool CanDecode(MediaImageDocument document, string? machineId = null);

    Task<SequentialDecodeResult> DecodeAsync(
        MediaImageDocument document,
        string? machineId = null,
        CancellationToken cancellationToken = default);
}
