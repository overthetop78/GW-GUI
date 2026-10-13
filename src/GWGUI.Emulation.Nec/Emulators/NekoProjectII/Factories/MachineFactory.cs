using GWGUI.Emulation.Nec.Emulators.NekoProjectII.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nec.Emulators.NekoProjectII.Factories;

internal sealed class MachineFactory : CoreMachineFactory, IEmulatorFirmwareSettings, IEmulatorMachineDefaults
{
    public bool IsDefaultFor(string machineId) => Definition.MachineIds.Contains(machineId);

    public IReadOnlyList<FirmwareSlot> FirmwareSlots => GWGUI.Emulation.Nec.Emulators.NekoProjectII.Constants.FirmwareConstants.Slots;
    protected override CoreReleaseSettings ReleaseSettings => CoreConstants.ReleaseSettings;
    public override string EmulatorId => CoreConstants.Id;
    public override EmulationEmulatorDefinition Definition { get; } = new(
        CoreConstants.Id, CoreConstants.DisplayName, CoreConstants.DescriptionResourceKey,
        new[]
        {
            GWGUI.Emulation.Nec.Common.Machines.Pc9801.Constants.MachineConstants.Id,
            GWGUI.Emulation.Nec.Common.Machines.Pc9821.Constants.MachineConstants.Id,
        }.ToHashSet(StringComparer.Ordinal));
}
