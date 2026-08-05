using GWGUI.Scp;
using GWGUI.Scp.Decoding;

namespace GWGUI.App.ViewModels;

public sealed class ScpInspectorPresenter(FluxDecoderRegistry decoders, Func<string, object[], string> localize)
{
    public string Build(ScpImage image, ScpTrack track, string? decoderId)
    {
        var best = decoders.DecodeBest(track.Revolutions, decoderId);
        var decoded = best?.Result;
        var revolutions = string.Join(Environment.NewLine, track.Revolutions.Select((revolution, index) =>
            Localize("Visual.Revolution", index + 1, revolution.FluxIntervals.Count, revolution.DurationMilliseconds(image.Header.ResolutionNanoseconds), revolution.Rpm(image.Header.ResolutionNanoseconds))));
        var details = decoded is null ? "" : string.Join(Environment.NewLine, decoded.Structures.Take(30).Select(structure =>
            $"• {Localize("Visual.StructureKind." + structure.Kind)} · {Localize("Visual.BitOffset", structure.BitOffset)}"));
        var sectors = decoded?.Sectors is not { Count: > 0 } ? "" : string.Join(Environment.NewLine, decoded.Sectors.Take(30).Select(sector =>
            Localize("Visual.SectorDetail", sector.Cylinder, sector.Head, sector.Number, sector.SizeBytes, Localize("Visual.Integrity." + sector.IntegrityKind),
                Localize(sector.IntegrityValid is null ? "Visual.IntegrityUnavailable" : sector.IntegrityValid.Value ? "Visual.CrcValid" : "Visual.CrcInvalid"))));
        var analysis = decoded is null ? "" : "\n\n" + Localize("Visual.Analysis", Localize("Visual.DecoderName." + decoded.DecoderId), decoded.Confidence, decoded.EstimatedBitCellTicks, decoded.Structures.Count)
            + $"\n{Localize("Visual.AnalysedRevolution", best!.Value.RevolutionIndex + 1)}"
            + (details.Length > 0 ? $"\n\n{details}" : "") + (sectors.Length > 0 ? $"\n\n{sectors}" : "");
        return Localize("Visual.Track", track.Head, track.Cylinder, track.TrackNumber) + $"\n\n{revolutions}{analysis}";
    }

    private string Localize(string key, params object[] arguments) => localize(key, arguments);
}
