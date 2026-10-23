using GWGUI.Emulation.Nec.Emulators.Quasi88.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nec.Emulators.Quasi88.Factories;

internal sealed class MachineFactory : CoreMachineFactory, IEmulatorMachineDefaults
{
    public bool IsDefaultFor(string machineId) => Definition.MachineIds.Contains(machineId);

    /// <inheritdoc/>
    public override IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration) => GWGUI.Emulation.Nec.Emulators.Quasi88.Constants.FirmwareConstants.Slots;
    protected override CoreReleaseSettings ReleaseSettings => CoreConstants.ReleaseSettings;
    public override string EmulatorId => CoreConstants.Id;
    public override EmulationEmulatorDefinition Definition { get; } = new(
        CoreConstants.Id, CoreConstants.DisplayName, CoreConstants.DescriptionResourceKey,
        new[]
        {
            GWGUI.Emulation.Nec.Common.Machines.Pc8001.Constants.MachineConstants.Id,
            GWGUI.Emulation.Nec.Common.Machines.Pc8801.Constants.MachineConstants.Id,
            GWGUI.Emulation.Nec.Common.Machines.Pc8801.Constants.MachineConstants.MkIIId,
            GWGUI.Emulation.Nec.Common.Machines.Pc8801.Constants.MachineConstants.MkIISrId,
        }.ToHashSet(StringComparer.Ordinal));
}
