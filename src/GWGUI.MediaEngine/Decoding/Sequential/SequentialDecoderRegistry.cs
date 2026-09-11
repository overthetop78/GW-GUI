using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Interfaces.Decoding;

namespace GWGUI.MediaEngine.Decoding.Sequential;

/// <summary>Selects compatible sequential decoders without containing any machine decoding algorithm.</summary>
public sealed class SequentialDecoderRegistry
{
    private readonly IReadOnlyList<ISequentialMediaDecoder> decoders;

    public SequentialDecoderRegistry(IReadOnlyList<ISequentialMediaDecoder> decoders)
    {
        ArgumentNullException.ThrowIfNull(decoders);
        if (decoders.Any(decoder => decoder is null))
            throw new ArgumentException("A sequential media decoder cannot be null.", nameof(decoders));
        this.decoders = decoders.ToArray();
    }

    public IReadOnlyList<ISequentialMediaDecoder> FindCompatible(
        MediaImageDocument document,
        string? machineId = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        return decoders.Where(decoder => decoder.CanDecode(document, machineId)).ToArray();
    }

    public ISequentialMediaDecoder GetRequired(MediaImageDocument document, string? machineId = null)
    {
        var compatible = FindCompatible(document, machineId);
        return compatible.Count switch
        {
            1 => compatible[0],
            0 => throw new NotSupportedException(
                $"No sequential decoder accepts format '{document.FormatId}' for machine '{machineId ?? "unspecified"}'."),
            _ => throw new InvalidOperationException(
                $"Multiple sequential decoders accept format '{document.FormatId}' for machine '{machineId ?? "unspecified"}'; an explicit machine is required.")
        };
    }

    public Task<SequentialDecodeResult> DecodeAsync(
        MediaImageDocument document,
        string? machineId = null,
        CancellationToken cancellationToken = default) =>
        GetRequired(document, machineId).DecodeAsync(document, machineId, cancellationToken);
}
