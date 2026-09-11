using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Contracts.Rendering.Sequential;
using GWGUI.App.Contracts.ViewModels.Visualization;
using GWGUI.App.Enums.Rendering.Sequential;
using GWGUI.App.Enums.ViewModels.Visualization;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Representations.Sequential;

namespace GWGUI.App.Presenters.Visualization;

public sealed class SequentialMediaInspectorPresenter(Func<string, object[], string> localize)
{
    public SequentialMediaRenderModel BuildRenderModel(MediaImageDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (document.Representation is not SequentialMediaImageRepresentation sequential)
            throw new ArgumentException("A sequential media representation is required.", nameof(document));
        var segments = sequential.Segments?.Select(segment => new SequentialMediaSegment(
            segment.Start.Ticks,
            segment.ChannelNumber ?? segment.TrackNumber ?? segment.FaceNumber ?? 0,
            segment.Start,
            segment.Duration,
            SequentialSegmentKind.Unknown,
            segment.FaceNumber,
            segment.TrackNumber,
            segment.ChannelNumber)).ToArray() ?? [];
        return new(sequential.LogicalLength, sequential.Duration, segments);
    }

    public MediaInspectorModel BuildInspectorModel(SequentialMediaRenderModel model, SequentialMediaSegment? segment)
    {
        ArgumentNullException.ThrowIfNull(model);
        var summaryEntries = new List<MediaInspectorEntry>();
        if (model.Duration is { } duration)
            summaryEntries.Add(new(Localize("Visual.DurationLabel"), duration.ToString("g")));
        if (model.LogicalLength is { } logicalLength)
            summaryEntries.Add(new(Localize("Visual.CapacityLabel"), logicalLength.ToString("N0"), Localize("Visual.BytesUnit")));
        summaryEntries.Add(new(Localize("Visual.SegmentCountLabel"), model.Segments.Count.ToString()));
        var sections = new List<MediaInspectorSection>
        {
            new(Localize("Visual.SummaryTab"), ControlVisualConstants.InformationGlyph, summaryEntries)
        };

        if (segment is not null)
        {
            var entries = new List<MediaInspectorEntry>
            {
                new(Localize("Visual.StartLabel"), segment.Start.ToString("g")),
                new(Localize("Visual.DurationLabel"), segment.Duration.ToString("g")),
                new(Localize("Visual.SegmentKindLabel"), Localize("Visual.SequentialSegmentKind." + segment.Kind))
            };
            if (segment.FaceNumber is { } face)
                entries.Add(new(Localize("Visual.SideLabel"), face.ToString()));
            if (segment.TrackNumber is { } track)
                entries.Add(new(Localize("Visual.TrackLabel"), track.ToString()));
            if (segment.ChannelNumber is { } channel)
                entries.Add(new(Localize("Visual.ChannelLabel"), channel.ToString()));
            if (!string.IsNullOrWhiteSpace(segment.RecognizedName))
                entries.Add(new(Localize("Visual.RecognizedNameLabel"), segment.RecognizedName));
            if (!string.IsNullOrWhiteSpace(segment.DecodeError))
                entries.Add(new(Localize("Visual.DecodeErrorLabel"), segment.DecodeError, null, MediaInspectorEntryLevel.Error));
            sections.Add(new(Localize("Visual.SegmentTitle"), ControlVisualConstants.InformationGlyph, entries));
        }

        return new(
            Localize("Visual.SequentialInspectorTitle"),
            segment is null ? null : $"{segment.Start:g} · {segment.Duration:g}",
            sections);
    }

    private string Localize(string key, params object[] arguments) => localize(key, arguments);
}
