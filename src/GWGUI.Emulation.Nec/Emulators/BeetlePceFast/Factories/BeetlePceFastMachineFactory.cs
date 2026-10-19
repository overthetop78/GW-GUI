using GWGUI.Emulation.Nec.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Functions;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Services;
using System.IO;
using GWGUI.Emulation.Nec.Common.Machines.PcEngine.Constants;
using GWGUI.Emulation.Nec.Common.Machines.CoreGrafx.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcEngineDuo.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcEngineLt.Constants;
using GWGUI.Emulation.Nec.Common.Machines.TurboExpress.Constants;

namespace GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Factories;

internal sealed class BeetlePceFastMachineFactory : CoreMachineFactory, IEmulatorMachineDefaults
{
    public bool IsDefaultFor(string machineId) => Definition.MachineIds.Contains(machineId);

    public override EmulationEmulatorDefinition Definition { get; } = new(
        BeetlePceFastConstants.Id, BeetlePceFastConstants.DisplayName,
        BeetlePceFastConstants.DescriptionResourceKey,
        new[] { PcEngineMachineConstants.Id, CoreGrafxMachineConstants.Id,
            PcEngineDuoMachineConstants.Id, PcEngineLtMachineConstants.Id,
            TurboExpressMachineConstants.Id,
            GWGUI.Emulation.Nec.Common.Machines.PcEngineCd.Constants.MachineConstants.Id,
            GWGUI.Emulation.Nec.Common.Machines.PcEngineSuperCd.Constants.MachineConstants.Id ,
            GWGUI.Emulation.Nec.Common.Machines.PcEngineArcadeCard.Constants.MachineConstants.Id }
            .ToHashSet(StringComparer.Ordinal));

    public override string EmulatorId => BeetlePceFastConstants.Id;
    protected override CoreReleaseSettings ReleaseSettings => ReleaseConstants.Settings;
    protected override MachineConfiguration Configure(MachineConfiguration configuration) => BeetlePceFastOptionFunctions.ToNative(configuration);

    public override ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context.HttpClient);
        return base.FindInstalledCorePathAsync(context, cancellationToken);
    }
}
