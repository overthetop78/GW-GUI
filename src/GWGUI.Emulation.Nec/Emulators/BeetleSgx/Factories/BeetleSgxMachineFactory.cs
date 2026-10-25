using GWGUI.Emulation.Nec.Emulators.Common.Interop.Factories;
using System.IO;
using GWGUI.Emulation.Nec.Common.Machines.SuperGrafx.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Functions;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Nec.Emulators.BeetleSgx.Constants;

namespace GWGUI.Emulation.Nec.Emulators.BeetleSgx.Factories;

internal sealed class BeetleSgxMachineFactory : CoreMachineFactory, IEmulatorMachineDefaults
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.BeetleSgx;
    public bool IsDefaultFor(string machineId) => Definition.MachineIds.Contains(machineId);

    public override EmulationEmulatorDefinition Definition { get; } = new(BeetleSgxConstants.Id, BeetleSgxConstants.DisplayName, BeetleSgxConstants.DescriptionResourceKey, new[] { SuperGrafxMachineConstants.Id }.ToHashSet(StringComparer.Ordinal));

    public override string EmulatorId => BeetleSgxConstants.Id;
    protected override CoreReleaseSettings ReleaseSettings => BeetleSgxConstants.ReleaseSettings;
    protected override MachineConfiguration Configure(MachineConfiguration configuration) => BeetlePceFastOptionFunctions.ToNative(configuration);
}
