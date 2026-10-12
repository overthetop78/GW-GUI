namespace GWGUI.Emulation.Apple.Common.Interfaces;

internal interface IEmulatorFirmwareAdapter
{
    void PrepareFirmware(MachineConfiguration configuration, string systemDirectory);
    ValueTask<EmulationFirmwareCandidate> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken);
}
