using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Definitions;

namespace GWGUI.MediaEngine.Encoding.Rare;

/// <summary>Associe chaque format sectoriel rare à son encodeur et à sa cadence de reconstruction nominale.</summary>
internal static class RareTrackEncodingProfiles
{
    private static readonly IReadOnlyDictionary<string, RareTrackEncodingProfile> Profiles =
        new Dictionary<string, RareTrackEncodingProfile>(StringComparer.OrdinalIgnoreCase)
        {
            [DiskImageFormatIds.HpMmfm] = new(FluxCodecIds.HpMmfm, TrackEncodingTimings.HighDensityMfmBitCellTicks, TrackEncodingTimings.Rpm300IndexTimeTicks),
            [DiskImageFormatIds.DataGeneralFm] = new(FluxCodecIds.DataGeneralFm, TrackEncodingTimings.DoubleDensityMfmBitCellTicks, TrackEncodingTimings.Rpm300IndexTimeTicks),
            [DiskImageFormatIds.MicropolisMfm] = new(FluxCodecIds.MicropolisMfm, TrackEncodingTimings.DoubleDensityMfmBitCellTicks, TrackEncodingTimings.Rpm300IndexTimeTicks),
            [DiskImageFormatIds.MembrainMfm] = Default(FluxCodecIds.MembrainMfm),
            [DiskImageFormatIds.Aed6200pMfm] = new(FluxCodecIds.Aed6200pMfm, TrackEncodingTimings.HighDensityMfmBitCellTicks, TrackEncodingTimings.Rpm360IndexTimeTicks),
            [DiskImageFormatIds.QdMo5Mfm] = Default(FluxCodecIds.QdMo5Mfm),
            [DiskImageFormatIds.CenturionMfm] = Default(FluxCodecIds.CenturionMfm),
            [DiskImageFormatIds.NorthstarMfm] = Default(FluxCodecIds.NorthstarMfm),
            [DiskImageFormatIds.HeathkitFm] = Default(FluxCodecIds.HeathkitFm),
            [DiskImageFormatIds.MicralNFm] = Default(FluxCodecIds.MicralNFm),
            [DiskImageFormatIds.EmuFm] = Default(FluxCodecIds.EmuFm),
            [DiskImageFormatIds.TycomFm] = Default(FluxCodecIds.TycomFm),
            [DiskImageFormatIds.Arburg] = Default(FluxCodecIds.Arburg),
            [DiskImageFormatIds.Victor9kGcr] = Default(FluxCodecIds.Victor9kGcr)
        };

    public static bool TryResolve(string formatId, out RareTrackEncodingProfile profile) => Profiles.TryGetValue(formatId, out profile!);

    private static RareTrackEncodingProfile Default(string encoderId) =>
        new(encoderId, TrackEncodingDefaults.BitCellTicks, TrackEncodingDefaults.IndexTimeTicks);
}

/// <summary>Décrit le codec et les durées nominales d'un format rare.</summary>
internal sealed record RareTrackEncodingProfile(string EncoderId, uint BitCellTicks, uint IndexTimeTicks);
