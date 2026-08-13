using GWGUI.MediaEngine.Containers.Hfe;
using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.Conversion.Flux;

/// <summary>Vérifie qu'un SCP représente exactement les pistes uniformes de sa source HFE.</summary>
internal static class HfeScpParityValidator
{
    public static void Validate(HfeImage expected, ScpImage actual)
    {
        if (!actual.ChecksumValid || actual.Header.Revolutions != 1)
            throw new InvalidDataException("Le SCP issu du HFE est invalide.");
        foreach (var source in expected.Tracks)
        {
            var trackNumber = ScpFormatConstants.ToTrackNumber(source.Cylinder, source.Head);
            var target = actual.Tracks.SingleOrDefault(track => track.TrackNumber == trackNumber);
            if (target is null || target.Revolutions.Count != 1)
                throw new InvalidDataException("Une piste HFE manque dans le SCP produit.");
            var revolution = target.Revolutions[0];
            var bits = FluxBitCellConverter.ToBits(
                revolution.FluxIntervals,
                revolution.IndexTimeTicks,
                source.BitCellTicks);
            if (!source.Bits.SequenceEqual(bits))
                throw new InvalidDataException("Les cellules HFE ont changé dans le SCP produit.");
        }
    }
}
