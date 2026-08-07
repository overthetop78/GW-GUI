namespace GWGUI.Scp.Decoding;

public sealed class FluxDecoderRegistry
{
    public IReadOnlyList<IFluxDecoder> Decoders { get; } = [new IsoMfmDecoder(), new IsoFmDecoder(), new AmigaMfmDecoder(), new AppleGcrDecoder(), new AppleMacGcrDecoder(), new CommodoreGcrDecoder(), new HpMmfmDecoder(), new DataGeneralFmDecoder(), new MicropolisMfmDecoder(), new MembrainMfmDecoder(), new Aed6200pMfmDecoder(), new QdMo5MfmDecoder(), new CenturionMfmDecoder(), new NorthstarMfmDecoder(), new HeathkitFmDecoder(), new MicralNFmDecoder(), new EmuFmDecoder(), new TycomFmDecoder(), new DecRx02Decoder(), new ArburgDecoder(), new Victor9kGcrDecoder(), new RawFluxDecoder()];
    public FluxDecodeResult DecodeAutomatic(ScpRevolution revolution) => Decoders.Select(x => x.Decode(revolution))
        .OrderByDescending(AutomaticScore)
        .ThenByDescending(result => result.Confidence)
        .ThenByDescending(result => result.Structures.Count)
        .First();
    public FluxDecodeResult Decode(string id, ScpRevolution revolution) => Decoders.First(x => x.Id == id).Decode(revolution);
    public (int RevolutionIndex, FluxDecodeResult Result)? DecodeBest(IReadOnlyList<ScpRevolution> revolutions, string? decoderId = null)
    {
        if (revolutions.Count == 0) return null;
        return revolutions.Select((revolution, index) => (RevolutionIndex: index, Result: decoderId is null ? DecodeAutomatic(revolution) : Decode(decoderId, revolution)))
            .OrderByDescending(candidate => candidate.Result.Confidence)
            .ThenByDescending(candidate => candidate.Result.Structures.Count)
            .First();
    }

    internal static double AutomaticScore(FluxDecodeResult result)
    {
        var sectors = result.Sectors ?? [];
        var valid = sectors.Count(sector => sector.IntegrityValid == true);
        var invalid = sectors.Count(sector => sector.IntegrityValid == false);
        if (valid > 0)
            return 4 + valid / (double)Math.Max(1, valid + invalid) + result.Confidence * .1;
        if (sectors.Count > 0 && invalid == 0)
            return 3 + result.Confidence;
        if (invalid > 0)
            return result.Confidence * .01;
        if (result.DecoderId == "raw")
            return 1 + result.Confidence;
        if (result.Structures.Count > 0)
            return 2 + result.Confidence;
        return result.Confidence;
    }
}
