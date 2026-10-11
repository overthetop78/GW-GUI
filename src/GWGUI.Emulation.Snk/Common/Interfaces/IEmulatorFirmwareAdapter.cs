namespace GWGUI.Emulation.Snk.Common.Interfaces;

internal interface IEmulatorFirmwareAdapter
{
    IReadOnlyList<FirmwareCategory> FirmwareRoles(MachineModel model);
    bool IsFirmwareRequired(MachineModel model, FirmwareCategory category);
    ValueTask<EmulationFirmwareCandidate> InspectFirmwareAsync(string path, MachineModel model, CancellationToken cancellationToken);
}
