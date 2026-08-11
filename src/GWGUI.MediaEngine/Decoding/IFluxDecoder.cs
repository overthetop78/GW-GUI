using GWGUI.MediaEngine.Flux;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Définit un décodeur capable d'interpréter une révolution de flux SCP.</summary>
/// <remarks>L'identifiant est une chaîne technique extensible qui doit être unique dans un registre. Le nom affiché est purement descriptif et ne doit jamais servir à identifier un codec.</remarks>
public interface IFluxDecoder
{
    string Id { get; }
    string DisplayName { get; }
    FluxDecodeResult Decode(FluxRevolution revolution);
}
