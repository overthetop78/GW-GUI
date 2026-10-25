using System.IO;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Interfaces;

namespace GWGUI.Emulation.Services;

/// <summary>Relie les contrats média des modules à MediaEngine, sans exposer ses types dans le SDK public.</summary>
internal sealed class EmulationMediaProcessingService : IEmulationMediaProcessing
{
    private static IMediaRuntimePreparation Preparation => MediaEngineComposition.RuntimePreparation;
    /// <inheritdoc/>
    public IReadOnlyDictionary<string, string> FileExtensions => Preparation.FileExtensions;
    /// <inheritdoc/>
    public int RawOpticalSectorSize => Preparation.RawOpticalSectorSize;
    /// <inheritdoc/>
    public int AudioSectorSize => Preparation.AudioSectorSize;
    /// <inheritdoc/>
    public int OpticalSubchannelSize => Preparation.OpticalSubchannelSize;
    /// <inheritdoc/>
    public Task ConvertScpAsync(string sourcePath, string outputPath, string outputExtension, CancellationToken cancellationToken = default) => Preparation.ConvertScpAsync(sourcePath, outputPath, outputExtension, cancellationToken);
    /// <inheritdoc/>
    public IEmulationCueSheet ReadCueSheet(IEnumerable<string> lines) => new CueSheet(Preparation.ReadCueSheet(lines));
    /// <inheritdoc/>
    public bool IsSingleBinaryRawAudioCue(IEmulationCueSheet sheet, int maximumTracks) => Preparation.IsSingleBinaryRawAudioCue(RequireDocument(sheet), maximumTracks);
    /// <inheritdoc/>
    public IEmulationCueSheet WithSingleCueFile(IEmulationCueSheet sheet, string declaredPath) => new CueSheet(Preparation.WithSingleCueFile(RequireDocument(sheet), declaredPath));
    /// <inheritdoc/>
    public Task WriteCueSheetAsync(IEmulationCueSheet sheet, Stream destination, CancellationToken cancellationToken = default) => Preparation.WriteCueSheetAsync(RequireDocument(sheet), destination, cancellationToken);
    /// <inheritdoc/>
    public Task PrepareCueWaveFilesAsync(string sourceCue, string stagedCue, Func<string, string> stageFile, Func<Exception> unsupported, int copyBufferBytes, CancellationToken cancellationToken = default) => Preparation.PrepareCueWaveFilesAsync(sourceCue, stagedCue, stageFile, unsupported, copyBufferBytes, cancellationToken);

    private static IMediaCueSheet RequireDocument(IEmulationCueSheet sheet)
    {
        ArgumentNullException.ThrowIfNull(sheet);
        return sheet is CueSheet parsed ? parsed.Document : throw new ArgumentException("The CUE document must come from this SDK service.", nameof(sheet));
    }

    private sealed class CueSheet(IMediaCueSheet document) : IEmulationCueSheet
    {
        internal IMediaCueSheet Document { get; } = document;
        public IReadOnlyList<string> DeclaredFiles => Document.DeclaredFiles;
    }
}
