using GWGUI.MediaEngine.Containers.Scp;

using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

public abstract class SignatureMfmDecoder : IFluxDecoder
{
    public abstract string Id { get; }
    public abstract string DisplayName { get; }
    protected abstract IReadOnlyList<(byte[] Pattern, FluxStructureKind Kind, string Description)> Signatures { get; }
    protected virtual double ExpectedStructures => 10;
    protected virtual bool IsFm => false;
    protected virtual bool IsNrzi => false;

    public virtual FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = IsNrzi ? FluxTransitionDecoder.DecodeNrzi(revolution.FluxIntervals) : IsFm ? FluxTransitionDecoder.DecodeAdaptiveFm(revolution.FluxIntervals) : FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals); var structures = new List<FluxStructure>();
        for (var offset = 0; offset < stream.Bits.Length; offset++)
        {
            foreach (var signature in Signatures)
            {
                if (!FluxBitReader.MatchBytes(stream, offset, signature.Pattern)) continue;
                structures.Add(new(signature.Kind, offset, signature.Pattern.Length * BitPrimitives.BitsPerByte, signature.Description));
                offset += signature.Pattern.Length * BitPrimitives.BitsPerByte - 1; break;
            }
        }
        return new(Id, DisplayName, Math.Min(1, structures.Count / ExpectedStructures), stream.BitCellTicks, structures, []);
    }

}
