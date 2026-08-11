using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;

namespace GWGUI.App.ViewModels;

public sealed class ScpInspectorPresenter(FluxDecoderRegistry decoders, Func<string, object[], string> localize)
{
    public ScpInspectorModel BuildModel(ScpImage image, ScpTrack track, string? decoderId)
    {
        var best = decoders.DecodeBest(track.Revolutions.Select(revolution => revolution.Flux).ToArray(), decoderId);
        var decoded = best?.Result;
        var revolutions = track.Revolutions.Select((revolution, index) => new ScpRevolutionInfo(
            index + 1, revolution.FluxIntervals.Count,
            revolution.DurationMilliseconds(image.Header.ResolutionNanoseconds),
            revolution.Rpm(image.Header.ResolutionNanoseconds))).ToArray();
        var structures = decoded?.Structures.Take(30).Select(structure =>
            new ScpInspectorEntry(Localize("Visual.StructureKind." + structure.Kind), Localize("Visual.BitOffset", structure.BitOffset))).ToArray() ?? [];
        var sectors = decoded?.Sectors is { } decodedSectors ? decodedSectors.Take(30).Select(sector =>
            Localize("Visual.SectorDetail", sector.Cylinder, sector.Head, sector.Number, sector.SizeBytes, Localize("Visual.Integrity." + sector.IntegrityKind),
                Localize(sector.IntegrityValid is null ? "Visual.IntegrityUnavailable" : sector.IntegrityValid.Value ? "Visual.CrcValid" : "Visual.CrcInvalid"))).ToArray() : [];
        return new(track.Head, track.Cylinder, track.TrackNumber, revolutions,
            decoded is null ? null : new ScpDecodeInfo(Localize("Visual.DecoderName." + decoded.DecoderId), decoded.Confidence, decoded.EstimatedBitCellTicks, decoded.Structures.Count, best!.RevolutionIndex + 1),
            structures, sectors);
    }

    public string Build(ScpImage image, ScpTrack track, string? decoderId)
    {
        var best = decoders.DecodeBest(track.Revolutions.Select(revolution => revolution.Flux).ToArray(), decoderId);
        var decoded = best?.Result;
        var revolutions = string.Join(Environment.NewLine, track.Revolutions.Select((revolution, index) =>
            Localize("Visual.Revolution", index + 1, revolution.FluxIntervals.Count, revolution.DurationMilliseconds(image.Header.ResolutionNanoseconds), revolution.Rpm(image.Header.ResolutionNanoseconds))));
        var details = decoded is null ? "" : string.Join(Environment.NewLine, decoded.Structures.Take(30).Select(structure =>
            $"• {Localize("Visual.StructureKind." + structure.Kind)} · {Localize("Visual.BitOffset", structure.BitOffset)}"));
        var sectors = decoded?.Sectors is not { Count: > 0 } ? "" : string.Join(Environment.NewLine, decoded.Sectors.Take(30).Select(sector =>
            Localize("Visual.SectorDetail", sector.Cylinder, sector.Head, sector.Number, sector.SizeBytes, Localize("Visual.Integrity." + sector.IntegrityKind),
                Localize(sector.IntegrityValid is null ? "Visual.IntegrityUnavailable" : sector.IntegrityValid.Value ? "Visual.CrcValid" : "Visual.CrcInvalid"))));
        var analysis = decoded is null ? "" : "\n\n" + Localize("Visual.Analysis", Localize("Visual.DecoderName." + decoded.DecoderId), decoded.Confidence, decoded.EstimatedBitCellTicks, decoded.Structures.Count)
            + $"\n{Localize("Visual.AnalysedRevolution", best!.RevolutionIndex + 1)}"
            + (details.Length > 0 ? $"\n\n{details}" : "") + (sectors.Length > 0 ? $"\n\n{sectors}" : "");
        return Localize("Visual.Track", track.Head, track.Cylinder, track.TrackNumber) + $"\n\n{revolutions}{analysis}";
    }

    private string Localize(string key, params object[] arguments) => localize(key, arguments);
}

public sealed record ScpInspectorModel(int Head, int Cylinder, int ScpEntry, IReadOnlyList<ScpRevolutionInfo> Revolutions, ScpDecodeInfo? Decode, IReadOnlyList<ScpInspectorEntry> Structures, IReadOnlyList<string> Sectors)
{
    public int RevolutionCount => Revolutions.Count;
    public int SectorCount => Sectors.Count;
    public int TotalTransitions => Revolutions.Sum(item => item.Transitions);
    public double AverageRpm => Revolutions.Count == 0 ? 0 : Revolutions.Average(item => item.Rpm);
    public double AverageDurationMilliseconds => Revolutions.Count == 0 ? 0 : Revolutions.Average(item => item.DurationMilliseconds);
}
public sealed record ScpRevolutionInfo(int Number, int Transitions, double DurationMilliseconds, double Rpm);
public sealed record ScpDecodeInfo(string Decoder, double Confidence, double CellTicks, int StructureCount, int Revolution);
public sealed record ScpInspectorEntry(string Name, string Detail);
