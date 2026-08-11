using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Définit un décodeur capable d'interpréter une révolution de flux SCP.</summary>
public interface IFluxDecoder
{
    string Id { get; }
    string DisplayName { get; }
    FluxDecodeResult Decode(ScpRevolution revolution);
}
