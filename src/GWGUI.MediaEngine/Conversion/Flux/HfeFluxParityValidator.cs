using GWGUI.MediaEngine.Containers.Hfe;

namespace GWGUI.MediaEngine.Conversion.Flux;

/// <summary>Vérifie la parité structurelle et temporelle de deux conteneurs HFE.</summary>
internal static class HfeFluxParityValidator
{
    public static void Validate(HfeImage expected, HfeImage actual)
    {
        if (expected.Revision != actual.Revision ||
            expected.Cylinders != actual.Cylinders ||
            expected.Heads != actual.Heads ||
            expected.Encoding != actual.Encoding ||
            expected.BitRate != actual.BitRate ||
            expected.Tracks.Count != actual.Tracks.Count)
            throw new InvalidDataException("La structure HFE a changé pendant la conversion.");
        foreach (var source in expected.Tracks)
        {
            var target = actual.Tracks.SingleOrDefault(track =>
                track.Cylinder == source.Cylinder && track.Head == source.Head);
            if (target is null ||
                source.BitCellTicks != target.BitCellTicks ||
                !source.Bits.SequenceEqual(target.Bits))
                throw new InvalidDataException("Une piste ou un timing HFE a changé pendant la conversion.");
        }
    }
}
