namespace GWGUI.Emulation.Philips.Common.Interfaces;

internal interface IEmulatorFirmwareSettings
{
    ValueTask<EmulationFirmwareCandidate> ScanFirmwareAsync(string path, MachineModel model, CancellationToken cancellationToken);
}
