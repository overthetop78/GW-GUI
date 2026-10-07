using GWGUI.Emulation.Nec.Emulators.BeetlePce.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nec.Emulators.BeetlePce.Factories;

internal sealed class MachineFactory : CoreMachineFactory
{
    protected override CoreReleaseSettings ReleaseSettings => CoreConstants.ReleaseSettings;
    public override string EmulatorId => CoreConstants.Id;
    public override EmulationEmulatorDefinition Definition { get; } = new(
        CoreConstants.Id, CoreConstants.DisplayName, CoreConstants.DescriptionResourceKey,
        new[]
        {
            GWGUI.Emulation.Nec.Common.Machines.PcEngine.Constants.PcEngineMachineConstants.Id,
            GWGUI.Emulation.Nec.Common.Machines.CoreGrafx.Constants.CoreGrafxMachineConstants.Id,
            GWGUI.Emulation.Nec.Common.Machines.PcEngineDuo.Constants.PcEngineDuoMachineConstants.Id,
            GWGUI.Emulation.Nec.Common.Machines.PcEngineLt.Constants.PcEngineLtMachineConstants.Id,
            GWGUI.Emulation.Nec.Common.Machines.TurboExpress.Constants.TurboExpressMachineConstants.Id,
            GWGUI.Emulation.Nec.Common.Machines.SuperGrafx.Constants.SuperGrafxMachineConstants.Id,
            GWGUI.Emulation.Nec.Common.Machines.PcEngineCd.Constants.MachineConstants.Id,
            GWGUI.Emulation.Nec.Common.Machines.PcEngineSuperCd.Constants.MachineConstants.Id,
            GWGUI.Emulation.Nec.Common.Machines.PcEngineArcadeCard.Constants.MachineConstants.Id,
        }.ToHashSet(StringComparer.Ordinal));
}
