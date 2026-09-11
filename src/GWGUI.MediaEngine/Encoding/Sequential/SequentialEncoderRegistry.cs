using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Interfaces.Encoding;
using GWGUI.MediaEngine.Representations.Sequential;

namespace GWGUI.MediaEngine.Encoding.Sequential;

/// <summary>Selects compatible sequential encoders without containing any format encoding algorithm.</summary>
public sealed class SequentialEncoderRegistry
{
    private readonly IReadOnlyList<ISequentialMediaEncoder> encoders;

    public SequentialEncoderRegistry(IReadOnlyList<ISequentialMediaEncoder> encoders)
    {
        ArgumentNullException.ThrowIfNull(encoders);
        if (encoders.Any(encoder => encoder is null))
            throw new ArgumentException("A sequential media encoder cannot be null.", nameof(encoders));
        this.encoders = encoders.ToArray();
    }

    public IReadOnlyList<ISequentialMediaEncoder> FindCompatible(SequentialEncodeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return encoders.Where(encoder => encoder.CanEncode(request)).ToArray();
    }

    public ISequentialMediaEncoder GetRequired(SequentialEncodeRequest request)
    {
        var compatible = FindCompatible(request);
        return compatible.Count switch
        {
            1 => compatible[0],
            0 => throw new NotSupportedException(
                $"No sequential encoder accepts format '{request.TargetFormatId}' for machine '{request.MachineId}'."),
            _ => throw new InvalidOperationException(
                $"Multiple sequential encoders accept format '{request.TargetFormatId}' for machine '{request.MachineId}'.")
        };
    }

    public Task<SequentialMediaImageRepresentation> EncodeAsync(
        SequentialEncodeRequest request,
        CancellationToken cancellationToken = default) =>
        GetRequired(request).EncodeAsync(request, cancellationToken);
}
