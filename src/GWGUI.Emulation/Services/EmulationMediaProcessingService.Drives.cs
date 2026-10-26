using GWGUI.MediaEngine.Interfaces;

namespace GWGUI.Emulation.Services;

internal sealed partial class EmulationMediaProcessingService
{
    /// <inheritdoc/>
    public IEmulationDriveMechanics CreateDriveMechanics(string device) => new DriveMechanics(Preparation.CreateDriveMechanics(device));

    /// <summary>Relaye le mécanisme commun ; le module possède son état et son horloge.</summary>
    private sealed class DriveMechanics(IMediaDriveMechanics mechanics) : IEmulationDriveMechanics
    {
        internal IMediaDriveMechanics Source => mechanics;
        public bool HasPendingAudio => mechanics.HasPendingAudio;
        public void Configure(string? profile, double volumePercent, bool enabled) => mechanics.Configure(profile, volumePercent, enabled);
        public void Insert() => mechanics.Insert();
        public void Eject() => mechanics.Eject();
        public void SetMotor(bool running) => mechanics.SetMotor(running);
        public void SetActivity(bool reading, bool writing) => mechanics.SetActivity(reading, writing);
        public void Seek(int cylinder, int head, int sector = -1, long logicalBlock = -1) => mechanics.Seek(new(cylinder, head, sector, logicalBlock));
        public short[] RenderAudio(int sampleRate, int sampleCount) => mechanics.RenderAudio(sampleRate, sampleCount);
        public void Reset() => mechanics.Reset();
    }
}
