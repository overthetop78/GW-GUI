namespace GWGUI.Emulation;

public interface IEmulationFirmwareManager
{
    string GetFirmwareDirectory(string machineId);
    ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareAsync(string machineId,
        IEmulationConfiguration configuration, CancellationToken cancellationToken = default);
    IEmulationConfiguration UseFirmware(IEmulationConfiguration configuration,
        EmulationFirmwareCandidate firmware);
}
