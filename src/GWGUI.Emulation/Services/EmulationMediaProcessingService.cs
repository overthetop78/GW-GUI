using System.IO;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Interfaces;

namespace GWGUI.Emulation.Services;

/// <summary>Relie les contrats média des modules à MediaEngine, sans exposer ses types dans le SDK public.</summary>
internal sealed partial class EmulationMediaProcessingService : IEmulationMediaProcessing
{
    public IReadOnlyList<FloppyDriveModelChoice> FloppyDriveModels(string family, bool doubleDensity) => Preparation.FloppyDriveModels(family, doubleDensity).Select(model => new FloppyDriveModelChoice(model.Id, string.Empty, model.DisplayName, model.Capacity) { BlankFormats = model.BlankFormats, AcceptedExtensions = model.AcceptedExtensions }).ToArray();
    public bool SupportsFloppyImage(string model, IEmulationDiskImage image) => Preparation.SupportsFloppyImage(model, image.Cylinders, image.Heads, image.SectorSize, image.SectorsPerTrack, image.SectorCount);
    /// <inheritdoc/>
    public IReadOnlyList<string> CassetteRecordingExtensions => Preparation.CassetteRecordingExtensions;
    /// <inheritdoc/>
    public IReadOnlyList<string> MechanicalSoundProfiles(string device) => Preparation.MechanicalSoundProfiles(device);
    public string MechanicalSoundProfileDisplayName(string profile) => Preparation.MechanicalSoundProfileDisplayName(profile);
    public short[] RenderMechanicalSound(string device, string operation, int sampleRate, int sampleCount, long offset, string? profile = null, double volumePercent = GWGUI.Emulation.Constants.EmulationMechanicalSoundConstants.DefaultVolumePercent) => Preparation.RenderMechanicalSound(device, operation, sampleRate, sampleCount, offset, profile, volumePercent);
    public double MechanicalSoundDurationSeconds(string device, string profile, string operation) => Preparation.MechanicalSoundDurationSeconds(device, profile, operation);
    public short[] ProtectMechanicalAudioMix(IReadOnlyList<double> samples) => Preparation.ProtectMechanicalAudioMix(samples);
    /// <inheritdoc/>
    public Task WriteCassetteAsync(string path, EmulationCassetteSignal signal, CancellationToken cancellationToken = default, IReadOnlyDictionary<string, string>? parameters = null) => Preparation.WriteCassetteAsync(path, new GWGUI.MediaEngine.Contracts.MediaCassetteSignal(signal.Samples, signal.SampleRate), cancellationToken, parameters);
    /// <inheritdoc/>
    public async Task<EmulationCassetteSignal> ReadCassetteAsync(string path, CancellationToken cancellationToken = default, string? machineId = null, IReadOnlyDictionary<string, string>? parameters = null)
    {
        var signal = await Preparation.ReadCassetteAsync(path, cancellationToken, machineId, parameters).ConfigureAwait(false);
        return new EmulationCassetteSignal(signal.Samples, signal.SampleRate, signal.BlockPositions, signal.Blocks?.Select(block => new EmulationCassetteBlock(block.SamplePosition, block.Data, block.IntegrityValid, block.Metadata)).ToArray());
    }
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
