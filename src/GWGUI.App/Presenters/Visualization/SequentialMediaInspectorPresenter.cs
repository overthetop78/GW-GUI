using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Contracts.Rendering.Sequential;
using GWGUI.App.Contracts.ViewModels.Visualization;
using GWGUI.App.Enums.Rendering.Sequential;
using GWGUI.App.Enums.ViewModels.Visualization;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Images.Models.Sequential;
using RenderSegment = GWGUI.App.Contracts.Rendering.Sequential.SequentialMediaSegment;

namespace GWGUI.App.Presenters.Visualization;

public sealed class SequentialMediaInspectorPresenter(Func<string, object[], string> localize)
{
    public SequentialMediaRenderModel BuildRenderModel(MediaImageDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (document.Representation is not SequentialMediaImageRepresentation sequential)
            throw new ArgumentException("A sequential media representation is required.", nameof(document));
        var segments = sequential.Segments?.Select(segment => new RenderSegment(
            segment.Position,
            segment.ChannelNumber ?? segment.TrackNumber ?? segment.FaceNumber ?? 0,
            segment.Start ?? TimeSpan.Zero,
            segment.Duration ?? TimeSpan.Zero,
            MapKind(segment.Kind),
            segment.FaceNumber,
            segment.TrackNumber,
            segment.ChannelNumber,
            StoredLength: segment.Length,
            Metadata: segment.Metadata)).ToArray() ?? [];
        return new(sequential.LogicalLength, sequential.Duration, segments, FormatId: document.FormatId, Metadata: document.Metadata);
    }

    public MediaInspectorModel BuildInspectorModel(SequentialMediaRenderModel model, RenderSegment? segment)
    {
        ArgumentNullException.ThrowIfNull(model);
        var summaryEntries = new List<MediaInspectorEntry>();
        if (model.Duration is { } duration)
            summaryEntries.Add(new(Localize("Visual.DurationLabel"), duration.ToString("g")));
        if (model.LogicalLength is { } logicalLength)
            summaryEntries.Add(new(Localize("Visual.CapacityLabel"), logicalLength.ToString("N0"), Localize("Visual.BytesUnit")));
        summaryEntries.Add(new(Localize("Visual.SegmentCountLabel"), model.Segments.Count.ToString()));
        if (model.FormatId?.Equals(TapeImageFormatIds.AtariCas, StringComparison.OrdinalIgnoreCase) == true)
        {
            AddMetadata(summaryEntries, model.Metadata, "internalName", "Explorer.InternalName");
            AddMetadata(summaryEntries, model.Metadata, "baudRates", "Explorer.BaudRates", "baud");
            AddChunkSummary(summaryEntries, model.Metadata);
            AddMetadata(summaryEntries, model.Metadata, "dataChunkCount", "Explorer.DataBlocks");
            AddMetadata(summaryEntries, model.Metadata, "fskChunkCount", "Explorer.FskBlocks");
        }
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
            if (segment.StoredLength is { } storedLength)
                entries.Add(new(Localize("Visual.SizeLabel"), storedLength.ToString("N0"), Localize("Visual.BytesUnit")));
            AddMetadata(entries, segment.Metadata, "chunkId", "Explorer.ChunkType");
            AddMetadata(entries, segment.Metadata, "baudRate", "Explorer.BaudRate", "baud");
            AddMetadata(entries, segment.Metadata, "auxiliary", "Explorer.DelayBeforeBlock", "ms");
            if (!string.IsNullOrWhiteSpace(segment.RecognizedName))
                entries.Add(new(Localize("Visual.RecognizedNameLabel"), segment.RecognizedName));
            if (!string.IsNullOrWhiteSpace(segment.DecodeError))
                entries.Add(new(Localize("Visual.DecodeErrorLabel"), segment.DecodeError, null, MediaInspectorEntryLevel.Error));
            sections.Add(new(Localize("Visual.SegmentTitle"), ControlVisualConstants.InformationGlyph, entries));
        }

        return new(
            Localize(model.FormatId?.Equals(TapeImageFormatIds.AtariCas, StringComparison.OrdinalIgnoreCase) == true
                ? "Visual.CassetteInspectorTitle"
                : "Visual.SequentialInspectorTitle"),
            segment is null ? null : $"{segment.Start:g} · {segment.Duration:g}",
            sections);
    }

    private string Localize(string key, params object[] arguments) => localize(key, arguments);

    private void AddMetadata(
        ICollection<MediaInspectorEntry> entries,
        IReadOnlyDictionary<string, string>? metadata,
        string metadataKey,
        string labelKey,
        string? unit = null)
    {
        if (metadata?.TryGetValue(metadataKey, out var value) == true && !string.IsNullOrWhiteSpace(value))
            entries.Add(new(Localize(labelKey), value, unit));
    }

    private void AddChunkSummary(
        ICollection<MediaInspectorEntry> entries,
        IReadOnlyDictionary<string, string>? metadata)
    {
        if (metadata is null) return;
        metadata.TryGetValue("chunkCount", out var count);
        metadata.TryGetValue("chunkTypes", out var types);
        if (!string.IsNullOrWhiteSpace(count) || !string.IsNullOrWhiteSpace(types))
            entries.Add(new(Localize("Explorer.Chunks"), string.IsNullOrWhiteSpace(types)
                ? count!
                : string.IsNullOrWhiteSpace(count) ? types : $"{types} ({count})"));
    }

    private static SequentialSegmentKind MapKind(GWGUI.MediaEngine.Enums.SequentialSegmentKind kind) => kind switch
    {
        GWGUI.MediaEngine.Enums.SequentialSegmentKind.Samples or
        GWGUI.MediaEngine.Enums.SequentialSegmentKind.Pulse or
        GWGUI.MediaEngine.Enums.SequentialSegmentKind.Carrier => SequentialSegmentKind.Signal,
        GWGUI.MediaEngine.Enums.SequentialSegmentKind.Silence => SequentialSegmentKind.Silence,
        GWGUI.MediaEngine.Enums.SequentialSegmentKind.DataBlock or
        GWGUI.MediaEngine.Enums.SequentialSegmentKind.Record => SequentialSegmentKind.DecodedBlock,
        _ => SequentialSegmentKind.Unknown
    };
}
