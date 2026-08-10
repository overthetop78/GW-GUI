using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.Decoding;

public interface IFluxDecoder
{
    string Id { get; }
    string DisplayName { get; }
    FluxDecodeResult Decode(ScpRevolution revolution);
}
