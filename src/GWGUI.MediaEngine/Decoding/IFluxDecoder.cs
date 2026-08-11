using GWGUI.MediaEngine.Flux;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Définit un décodeur capable d'interpréter une révolution de flux SCP.</summary>
public interface IFluxDecoder
{
    string Id { get; }
    string DisplayName { get; }
    FluxDecodeResult Decode(FluxRevolution revolution);
}
