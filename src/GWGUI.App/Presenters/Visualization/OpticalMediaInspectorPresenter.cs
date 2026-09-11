using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Contracts.Rendering.Optical;
using GWGUI.App.Contracts.ViewModels.Visualization;
using GWGUI.App.Enums.Rendering.Optical;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Representations.Optical;

namespace GWGUI.App.Presenters.Visualization;

public sealed class OpticalMediaInspectorPresenter(Func<string, object[], string> localize)
{
    public OpticalMediaRenderModel BuildRenderModel(MediaImageDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (document.Representation is not OpticalMediaImageRepresentation optical)
            throw new ArgumentException("An optical media representation is required.", nameof(document));
        var tracks = optical.Tracks?.Select(track => new OpticalMediaTrack(
            track.SessionNumber,
            track.TrackNumber,
            track.FirstSector,
            track.SectorCount,
            OpticalTrackKind.Unknown)).ToArray() ?? [];
        return new(
            optical.LogicalLength,
            optical.FaceCount,
            optical.LayerCount,
            tracks,
            optical.AssociatedFiles ?? []);
    }

    public MediaInspectorModel BuildInspectorModel(MediaImageDocument document, OpticalMediaRenderModel model, OpticalMediaTrack? track)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(model);
        var summaryEntries = new List<MediaInspectorEntry>();
        if (model.LogicalLength is { } logicalLength)
            summaryEntries.Add(new(Localize("Visual.CapacityLabel"), logicalLength.ToString("N0"), Localize("Visual.SectorsUnit")));
        if (model.FaceCount is { } faceCount)
            summaryEntries.Add(new(Localize("Visual.FaceCountLabel"), faceCount.ToString()));
        if (model.LayerCount is { } layerCount)
            summaryEntries.Add(new(Localize("Visual.LayerCountLabel"), layerCount.ToString()));
        summaryEntries.Add(new(Localize("Visual.TrackCountLabel"), model.Tracks.Count.ToString()));

        var sections = new List<MediaInspectorSection>
        {
            new(Localize("Visual.SummaryTab"), ControlVisualConstants.InformationGlyph, summaryEntries)
        };
        if (track is not null)
        {
            var entries = new List<MediaInspectorEntry>
            {
                new(Localize("Visual.SessionLabel"), track.SessionNumber.ToString()),
                new(Localize("Visual.TrackLabel"), track.TrackNumber.ToString()),
                new(Localize("Visual.StartLabel"), track.FirstSector.ToString("N0"), "LBA"),
                new(Localize("Visual.LengthLabel"), track.SectorCount.ToString("N0"), Localize("Visual.SectorsUnit")),
                new(Localize("Visual.TrackKindLabel"), Localize("Visual.OpticalTrackKind." + track.Kind))
            };
            if (track.FaceNumber is { } face)
                entries.Add(new(Localize("Visual.SideLabel"), face.ToString()));
            if (track.LayerNumber is { } layer)
                entries.Add(new(Localize("Visual.LayerLabel"), layer.ToString()));
            sections.Add(new(Localize("Visual.OpticalTrackTitle"), ControlVisualConstants.InformationGlyph, entries));

            var volume = document.Volumes.FirstOrDefault(item =>
                item.Start <= track.FirstSector && item.Start + item.Length >= track.FirstSector + track.SectorCount);
            if (volume is not null)
            {
                var volumeEntries = new List<MediaInspectorEntry>
                {
                    new(Localize("Visual.VolumeOriginLabel"), volume.Origin)
                };
                if (!string.IsNullOrWhiteSpace(volume.FileSystemId))
                    volumeEntries.Add(new(Localize("Visual.FileSystemLabel"), volume.FileSystemId));
                sections.Add(new(Localize("Visual.VolumeTitle"), ControlVisualConstants.InformationGlyph, volumeEntries));
            }
        }

        if (model.AssociatedFiles.Count > 0)
            sections.Add(new(Localize("Visual.AssociatedFilesTitle"), ControlVisualConstants.InformationGlyph,
                model.AssociatedFiles.Select((path, index) => new MediaInspectorEntry((index + 1).ToString(), path)).ToArray()));

        return new(
            Localize("Visual.OpticalInspectorTitle"),
            track is null ? null : $"{Localize("Visual.SessionLabel")} {track.SessionNumber} · {Localize("Visual.TrackLabel")} {track.TrackNumber}",
            sections);
    }

    private string Localize(string key, params object[] arguments) => localize(key, arguments);
}
