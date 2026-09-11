using GWGUI.App.Contracts.ViewModels.Visualization;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Decoding;

using GWGUI.MediaEngine.Formats.Floppy.Scp;

namespace GWGUI.App.Presenters.Visualization;

public sealed class ScpInspectorPresenter(FluxDecoderRegistry decoders, Func<string, object[], string> localize)
{
    public MediaInspectorModel BuildCommonModel(ScpImage image, ScpTrack track, string? decoderId)
    {
        var model = BuildModel(image, track, decoderId);
        var sections = new List<MediaInspectorSection>
        {
            new(Localize("Visual.SummaryTab"), ControlVisualConstants.InformationGlyph,
            [
                new(Localize("Visual.SideLabel"), model.Head.ToString()),
                new(Localize("Visual.TrackLabel"), model.Cylinder.ToString()),
                new(Localize("Visual.ScpEntryLabel"), model.ScpEntry.ToString()),
                new(Localize("Visual.RevolutionsTitle"), model.RevolutionCount.ToString())
            ])
        };

        if (model.Revolutions.Count > 0)
        {
            sections.Add(new(Localize("Visual.RevolutionsTitle"), ControlVisualConstants.InformationGlyph,
                model.Revolutions.Select(revolution => new MediaInspectorEntry(
                    Localize("Visual.NumberPrefix") + revolution.Number,
                    $"{revolution.Transitions:N0}{Localize("Visual.ValueSeparator")}{revolution.DurationMilliseconds:F2} ms{Localize("Visual.ValueSeparator")}{revolution.Rpm:F2} RPM",
                    Localize("Visual.TransitionsUnit"))).ToArray()));
        }

        if (model.Decode is { } decode)
        {
            sections.Add(new(Localize("Visual.AnalysisTitle"), ControlVisualConstants.InformationGlyph,
            [
                new(Localize("Visual.FormatDetected"), decode.Decoder),
                new(Localize("Visual.ConfidenceLabel"), decode.Confidence.ToString("P0")),
                new(Localize("Visual.CellLabel"), decode.CellTicks.ToString("F1"), Localize("Visual.TicksUnit")),
                new(Localize("Visual.StructuresLabel"), decode.StructureCount.ToString())
            ]));
        }

        if (model.Structures.Count > 0)
            sections.Add(new(Localize("Visual.StructuresTitle"), ControlVisualConstants.InformationGlyph,
                model.Structures.Select(entry => new MediaInspectorEntry(entry.Name, entry.Detail)).ToArray()));

        if (model.Sectors.Count > 0)
            sections.Add(new(Localize("Visual.SectorsTitle"), ControlVisualConstants.InformationGlyph,
                model.Sectors.Select((sector, index) => new MediaInspectorEntry(Localize("Visual.NumberPrefix") + (index + 1), sector)).ToArray()));

        return new(
            Localize("Visual.Title"),
            Localize("Visual.TrackTooltip", model.Head, model.Cylinder, model.RevolutionCount),
            sections);
    }

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
